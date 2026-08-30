using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// Regressionstest: BaseTearDown muss den ServiceProvider ueber DisposeAsync() freigeben, nicht ueber das
// synchrone Dispose(), da Letzteres bei Services, die ausschliesslich IAsyncDisposable implementieren, eine
// Exception wirft.

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
