using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// REQ-07: Accessing services in the test - GetService<T>() must report missing registration and failed
// resolution as distinguishable via ServiceResolutionException.Reason.

[TestFixture]
public class Req07_ServiceResolutionTests : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) =>
        {
            services.AddSingleton<IGreeter, Greeter>();
            // Deliberately not Scoped: a Scoped service would already be rejected by validateScopes
            // (NFR-05) before the constructor even runs (see the Req_For_Scoped_Service test below), and
            // would therefore not cover the constructor failure case being tested here.
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
        // Plain mode does not create its own scope (see REQ-08); resolving a Scoped service directly via
        // the root services fails under validateScopes (NFR-05). This is the captive dependency case
        // explicitly named in REQ-07 and must be reported as ResolutionFailed.
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
