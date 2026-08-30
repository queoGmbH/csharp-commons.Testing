using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-02: Bootstrap im Web-Host-Modus.

[TestFixture]
public class Req02_HostBootstrap : WebHostIntegrationTestBase<Program>
{
    [Test]
    public void GetService_Resolves_The_Production_Registration_From_The_Real_Host()
    {
        Assert.That(GetService<IGreeter>().Greet(), Is.EqualTo("Hallo"));
    }
}
