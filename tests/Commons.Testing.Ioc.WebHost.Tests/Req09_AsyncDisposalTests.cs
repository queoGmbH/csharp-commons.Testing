using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-09: Automatisches Disposal im Web-Host-Modus - der Test-Scope muss ueber AsyncServiceScope.DisposeAsync()
// freigegeben werden, nicht ueber das synchrone IServiceScope.Dispose(), da Letzteres bei Services, die
// ausschliesslich IAsyncDisposable implementieren, eine Exception wirft.

[TestFixture]
public class Req09_AsyncDisposalTests : WebHostIntegrationTestBase<Program>
{
    protected override void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AsyncDisposableOnlyService>();
    }

    [Test]
    public void Test_With_AsyncDisposable_Only_Scoped_Service_Completes_Without_TearDown_Failure()
    {
        Assert.That(GetService<AsyncDisposableOnlyService>(), Is.Not.Null);
    }
}
