using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-09: Automatic disposal in web host mode - the test scope must be released via
// AsyncServiceScope.DisposeAsync(), not via the synchronous IServiceScope.Dispose(), since the latter
// throws an exception for services that implement only IAsyncDisposable.

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
