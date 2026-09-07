using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// Regression test: BaseTearDown must release the ServiceProvider via DisposeAsync(), not via the
// synchronous Dispose(), since the latter throws an exception for services that implement only
// IAsyncDisposable.

[TestFixture]
public class TearDownAsyncDisposalTests : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) => services.AddSingleton<IGreeter, AsyncDisposableOnlyGreeter>();

    [Test]
    public void Test_With_AsyncDisposable_Only_Service_Completes_Without_TearDown_Failure()
    {
        Assert.That(GetService<IGreeter>().Greet(), Is.EqualTo("AsyncDisposable"));
    }
}
