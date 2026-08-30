using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// Regressionstest: UseFakeAttribute ohne explizite Lifetime uebernimmt die Lifetime der ersetzten
// Registrierung, statt pauschal Singleton zu erzwingen. Andernfalls koennte ein Fake fuer einen eigentlich
// Scoped/Transient registrierten Service mit validateScopes (NFR-05) eine Captive-Dependency-Exception
// ausloesen, sobald der Fake selbst von einem Scoped-Service abhaengt - ein Fehler, der nichts mit der
// Testautorin/dem Testautor zu tun haette.

[TestFixture]
public class UseFakeLifetimeInferenceTests
{
    [Test]
    public void ApplyOverride_Without_Explicit_Lifetime_Infers_Lifetime_From_Existing_Registration()
    {
        var services = new ServiceCollection();
        services.AddScoped<IGreeter, Greeter>();

        new UseFakeAttribute(typeof(IGreeter), typeof(FakeGreeter))
            .ApplyOverride(services, new ConfigurationBuilder().Build());

        var descriptor = services.Single(d => d.ServiceType == typeof(IGreeter));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Scoped));
        Assert.That(descriptor.ImplementationType, Is.EqualTo(typeof(FakeGreeter)));
    }

    [Test]
    public void ApplyOverride_Without_Existing_Registration_Defaults_To_Singleton()
    {
        var services = new ServiceCollection();

        new UseFakeAttribute(typeof(IGreeter), typeof(FakeGreeter))
            .ApplyOverride(services, new ConfigurationBuilder().Build());

        var descriptor = services.Single(d => d.ServiceType == typeof(IGreeter));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Singleton));
    }

    [Test]
    public void ApplyOverride_With_Explicit_Lifetime_Uses_That_Lifetime_Regardless_Of_The_Existing_Registration()
    {
        var services = new ServiceCollection();
        services.AddSingleton<IGreeter, Greeter>();

        new UseFakeAttribute(typeof(IGreeter), typeof(FakeGreeter), ServiceLifetime.Transient)
            .ApplyOverride(services, new ConfigurationBuilder().Build());

        var descriptor = services.Single(d => d.ServiceType == typeof(IGreeter));
        Assert.That(descriptor.Lifetime, Is.EqualTo(ServiceLifetime.Transient));
    }
}
