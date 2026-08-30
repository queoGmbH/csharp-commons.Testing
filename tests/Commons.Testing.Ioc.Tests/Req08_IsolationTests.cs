using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// REQ-08: Lifecycle und Isolation im Plain-Modus - pro Test ein vollstaendig neuer ServiceProvider, sodass
// kein Zustand zwischen Tests uebertragen wird. Beide Tests inkrementieren unabhaengig voneinander und
// pruefen auf genau 1: Wuerde Zustand ueber Tests hinweg geteilt, saehe der zweite ausgefuehrte Test einen
// bereits erhoehten Wert, unabhaengig von der tatsaechlichen Ausfuehrungsreihenfolge.

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
