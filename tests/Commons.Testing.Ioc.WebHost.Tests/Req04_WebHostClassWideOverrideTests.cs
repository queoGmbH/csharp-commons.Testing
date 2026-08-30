using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-04: Klassenweite Service-Overrides im Web-Host-Modus.

[TestFixture]
public class Req04_WebHostClassWideOverride : WebHostIntegrationTestBase<Program>
{
    protected override void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
    {
        services.RemoveAll<IGreeter>();
        services.AddSingleton<IGreeter, FakeGreeter>();
    }

    [Test]
    public void GetService_Returns_The_ClassWide_Override_Instead_Of_The_Production_Service()
    {
        Assert.That(GetService<IGreeter>(), Is.InstanceOf<FakeGreeter>());
    }
}
