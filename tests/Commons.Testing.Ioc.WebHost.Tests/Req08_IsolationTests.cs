using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-08: Lifecycle and isolation in web host mode - the host is built once per test class, but each test
// gets a new AsyncServiceScope, so that Scoped services are isolated between tests. Both tests increment
// independently of one another and check for exactly 1: if the scope were shared between tests, the second
// test executed would see an already increased value, regardless of the actual execution order.

[TestFixture]
public class Req08_IsolationTests : WebHostIntegrationTestBase<Program>
{
    protected override void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<CounterService>();
    }

    [Test]
    public void First_Test_Sees_A_Fresh_Scoped_Counter()
    {
        var counter = GetService<CounterService>();
        counter.Increment();
        Assert.That(counter.Value, Is.EqualTo(1));
    }

    [Test]
    public void Second_Test_Sees_A_Fresh_Scoped_Counter()
    {
        var counter = GetService<CounterService>();
        counter.Increment();
        Assert.That(counter.Value, Is.EqualTo(1));
    }
}
