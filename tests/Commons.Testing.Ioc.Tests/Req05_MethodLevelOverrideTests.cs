using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commons.Testing.Ioc.Tests;

// REQ-05: Method-specific service overrides in plain mode.

[TestFixture]
public class Req05_MethodLevelOverride : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) => services.AddSingleton<IGreeter, Greeter>();

    [Test]
    [UseFake(typeof(IGreeter), typeof(FakeGreeter))]
    public void Method_With_UseFake_Resolves_The_Fake()
    {
        Assert.That(GetService<IGreeter>(), Is.InstanceOf<FakeGreeter>());
    }

    [Test]
    public void Method_Without_UseFake_Resolves_The_Production_Service()
    {
        // Proves that a method-specific override only applies to the test that requests it.
        Assert.That(GetService<IGreeter>(), Is.InstanceOf<Greeter>());
    }
}

[TestFixture]
public class Req05_MethodLevelOverride_On_Top_Of_ClassWideOverride : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) => services.AddSingleton<IGreeter, Greeter>();

    protected override void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
    {
        services.RemoveAll<IGreeter>();
        services.AddSingleton<IGreeter, FakeGreeter>();
    }

    [Test]
    [UseFake(typeof(IGreeter), typeof(SecondFakeGreeter))]
    public void MethodLevelOverride_Wins_Over_ClassWideOverride()
    {
        // Order per requirement: production bootstrap -> class-wide overrides -> method-specific
        // overrides. The method-specific override must therefore win last.
        Assert.That(GetService<IGreeter>(), Is.InstanceOf<SecondFakeGreeter>());
    }
}

[TestFixture]
public class Req05_DuplicateMethodLevelOverride
{
    private class DuplicateOverrideFixture
    {
        [UseFake(typeof(IGreeter), typeof(FakeGreeter))]
        [UseFake(typeof(IGreeter), typeof(SecondFakeGreeter))]
        public void MethodWithConflictingOverrides()
        {
        }
    }

    [Test]
    public void Duplicate_Overrides_For_Same_ServiceType_Throw_Before_Test_Execution()
    {
        var methodInfo = typeof(DuplicateOverrideFixture).GetMethod(nameof(DuplicateOverrideFixture.MethodWithConflictingOverrides))!;
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        var exception = Assert.Throws<InvalidOverrideConfigurationException>(new Action(
            () => IocIntegrationTestBase.ApplyMethodLevelOverrides(services, configuration, methodInfo)));

        Assert.That(exception!.Message, Does.Contain(nameof(IGreeter)));
    }
}
