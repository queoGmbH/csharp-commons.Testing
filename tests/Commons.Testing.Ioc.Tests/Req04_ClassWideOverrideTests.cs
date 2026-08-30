using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commons.Testing.Ioc.Tests;

// REQ-04: Klassenweite Service-Overrides.

[TestFixture]
public class Req04_ClassWideOverride : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) =>
        {
            services.AddSingleton<IGreeter, Greeter>();
            services.AddSingleton<IGreeter, SecondGreeter>();
        };

    protected override void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
    {
        services.RemoveAll<IGreeter>();
        services.AddSingleton<IGreeter, FakeGreeter>();
    }

    [Test]
    public void GetService_Returns_The_ClassWide_Override()
    {
        Assert.That(GetService<IGreeter>(), Is.InstanceOf<FakeGreeter>());
    }

    [Test]
    public void Override_Deterministically_Replaces_All_Previous_Registrations_For_IEnumerable()
    {
        var allGreeters = Services.GetServices<IGreeter>().ToList();

        Assert.That(allGreeters, Has.Count.EqualTo(1));
        Assert.That(allGreeters[0], Is.InstanceOf<FakeGreeter>());
    }
}

[TestFixture]
public class Req04_NoOverrideConfigured : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) => services.AddSingleton<IGreeter, Greeter>();

    [Test]
    public void Without_ConfigureOverrides_Production_Service_Is_Used()
    {
        Assert.That(GetService<IGreeter>(), Is.InstanceOf<Greeter>());
    }
}
