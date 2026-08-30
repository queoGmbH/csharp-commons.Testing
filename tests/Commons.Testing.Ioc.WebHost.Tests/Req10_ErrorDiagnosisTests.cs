using Commons.Testing.Ioc.Abstractions;
using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-10: Fehlerdiagnose beim Testaufbau im Web-Host-Modus - der Aufbaufehler wird mit Testkontext
// angereichert, die urspruengliche Exception bleibt als InnerException erhalten, und der ausloesende
// Service-Typ wird benannt, wenn er sich aus der zugrunde liegenden Exception extrahieren laesst.

[TestFixture]
public class Req10_ErrorDiagnosisTests
{
    private class ExtractableFailureFixture : WebHostIntegrationTestBase<Program>
    {
        protected override void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
        {
            throw new InvalidOperationException(
                "Unable to resolve service for type 'Commons.Testing.Ioc.WebHost.Tests.SampleApp.IGreeter' " +
                "while attempting to activate 'Foo'.");
        }
    }

    [Test]
    public void OneTimeSetUp_Failure_Names_The_Triggering_Service_Type_When_Extractable_From_The_Underlying_Exception()
    {
        var fixture = new ExtractableFailureFixture();

        var exception = Assert.Throws<IntegrationTestSetupException>(new Action(fixture.BaseOneTimeSetUp));

        Assert.That(exception!.Message, Does.Contain("IGreeter"));
        Assert.That(exception.Message, Does.Contain("ermittelbar"));
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>());
    }
}
