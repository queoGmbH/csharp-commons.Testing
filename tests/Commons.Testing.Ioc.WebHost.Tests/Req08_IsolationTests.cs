using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-08: Lifecycle und Isolation im Web-Host-Modus - der Host wird einmal pro Testklasse aufgebaut, aber
// jeder Test erhaelt einen neuen AsyncServiceScope, sodass Scoped-Services zwischen Tests isoliert sind.
// Beide Tests inkrementieren unabhaengig voneinander und pruefen auf genau 1: Wuerde der Scope zwischen
// Tests geteilt, saehe der zweite ausgefuehrte Test einen bereits erhoehten Wert, unabhaengig von der
// tatsaechlichen Ausfuehrungsreihenfolge.

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
