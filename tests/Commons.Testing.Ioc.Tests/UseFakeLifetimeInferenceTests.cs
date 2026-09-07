using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// Regression test: UseFakeAttribute without an explicit lifetime adopts the lifetime of the replaced
// registration, instead of unconditionally forcing Singleton. Otherwise, a fake for a service actually
// registered as Scoped/Transient could trigger a captive dependency exception under validateScopes
// (NFR-05) as soon as the fake itself depends on a Scoped service - an error that would have nothing to do
// with the test author.

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
