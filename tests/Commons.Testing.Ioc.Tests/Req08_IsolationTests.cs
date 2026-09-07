using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// REQ-08: Lifecycle and isolation in plain mode - a completely new ServiceProvider per test, so that no
// state is carried over between tests. Both tests increment independently of one another and check for
// exactly 1: if state were shared across tests, the second test executed would see an already increased
// value, regardless of the actual execution order.

[TestFixture]
public class Req08_IsolationTests : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) => services.AddSingleton<CounterService>();

    [Test]
    public void First_Test_Sees_A_Fresh_Counter()
    {
        var counter = GetService<CounterService>();
        counter.Increment();
        Assert.That(counter.Value, Is.EqualTo(1));
    }

    [Test]
    public void Second_Test_Sees_A_Fresh_Counter()
    {
        var counter = GetService<CounterService>();
        counter.Increment();
        Assert.That(counter.Value, Is.EqualTo(1));
    }
}
