using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// REQ-07: Zugriff auf Services im Test - GetService<T>() muss fehlende Registrierung und fehlgeschlagene
// Aufloesung ueber ServiceResolutionException.Reason unterscheidbar melden.

[TestFixture]
public class Req07_ServiceResolutionTests : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) =>
        {
            services.AddSingleton<IGreeter, Greeter>();
            // Bewusst nicht Scoped: Ein Scoped-Service wuerde bereits durch validateScopes (NFR-05)
            // abgelehnt, bevor der Konstruktor ueberhaupt laeuft (siehe Req_For_Scoped_Service-Test unten),
            // und wuerde damit nicht den hier zu pruefenden Konstruktor-Fehlerfall abdecken.
            services.AddSingleton<IThrowingService, ThrowingService>();
            services.AddScoped<IScopedGreeter, ScopedGreeter>();
        };

    [Test]
    public void GetService_Without_Registration_Throws_With_MissingRegistration_Reason()
    {
        var exception = Assert.Throws<ServiceResolutionException>(new Action(
            () => GetService<IUnregisteredService>()));

        Assert.That(exception!.Reason, Is.EqualTo(ServiceResolutionFailureReason.MissingRegistration));
        Assert.That(exception.ServiceType, Is.EqualTo(typeof(IUnregisteredService)));
        Assert.That(exception.Message, Does.Contain(nameof(IUnregisteredService)));
        Assert.That(exception.InnerException, Is.Null);
    }

    [Test]
    public void GetService_With_Throwing_Constructor_Throws_With_ResolutionFailed_Reason()
    {
        var exception = Assert.Throws<ServiceResolutionException>(new Action(
            () => GetService<IThrowingService>()));

        Assert.That(exception!.Reason, Is.EqualTo(ServiceResolutionFailureReason.ResolutionFailed));
        Assert.That(exception.ServiceType, Is.EqualTo(typeof(IThrowingService)));
        Assert.That(exception.Message, Does.Contain(nameof(IThrowingService)));
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>());
        Assert.That(exception.InnerException!.Message, Does.Contain("Konstruktor schlaegt absichtlich fehl"));
    }

    [Test]
    public void GetService_For_Scoped_Service_From_Root_Provider_Throws_With_ResolutionFailed_Reason()
    {
        // Der Plain-Modus erzeugt keinen eigenen Scope (siehe REQ-08); die Aufloesung eines Scoped-Service
        // direkt ueber die Root-Services schlaegt unter validateScopes (NFR-05) fehl. Dies ist der in REQ-07
        // ausdruecklich genannte Captive-Dependency-Fall und muss als ResolutionFailed gemeldet werden.
        var exception = Assert.Throws<ServiceResolutionException>(new Action(
            () => GetService<IScopedGreeter>()));

        Assert.That(exception!.Reason, Is.EqualTo(ServiceResolutionFailureReason.ResolutionFailed));
        Assert.That(exception.ServiceType, Is.EqualTo(typeof(IScopedGreeter)));
        Assert.That(exception.InnerException, Is.Not.Null);
    }

    [Test]
    public void GetService_With_Valid_Registration_Resolves_Normally()
    {
        Assert.That(GetService<IGreeter>().Greet(), Is.EqualTo("Hallo"));
    }
}
