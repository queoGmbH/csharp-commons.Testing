using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-06: No method-specific override in web host mode.

[TestFixture]
public class Req06_MethodLevelOverrideRejected
{
    private sealed class NoOpOverrideAttribute : ServiceOverrideAttribute
    {
        public NoOpOverrideAttribute(Type serviceType)
            : base(serviceType)
        {
        }

        public override void ApplyOverride(IServiceCollection services, IConfiguration configuration)
        {
        }
    }

    private class FixtureWithMethodLevelOverride
    {
        [NoOpOverrideAttribute(typeof(object))]
        public void MethodWithOverride()
        {
        }

        public void MethodWithoutOverride()
        {
        }
    }

    [Test]
    public void EnsureNoMethodLevelOverride_Throws_When_Test_Method_Carries_A_ServiceOverrideAttribute()
    {
        var methodInfo = typeof(FixtureWithMethodLevelOverride)
            .GetMethod(nameof(FixtureWithMethodLevelOverride.MethodWithOverride))!;

        var exception = Assert.Throws<InvalidOverrideConfigurationException>(new Action(
            () => MethodLevelOverrideGuard.EnsureNoMethodLevelOverride(methodInfo)));

        Assert.That(exception!.Message, Does.Contain("Web-Host-Modus"));
    }

    [Test]
    public void EnsureNoMethodLevelOverride_Does_Not_Throw_When_Test_Method_Has_No_Override()
    {
        var methodInfo = typeof(FixtureWithMethodLevelOverride)
            .GetMethod(nameof(FixtureWithMethodLevelOverride.MethodWithoutOverride))!;

        Assert.DoesNotThrow(new Action(() => MethodLevelOverrideGuard.EnsureNoMethodLevelOverride(methodInfo)));
    }
}
