using Commons.Testing.Ioc.Abstractions;
using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-10: Error diagnosis during test setup in web host mode - the setup failure is enriched with test
// context, the original exception is preserved as InnerException, and the triggering service type is
// named if it can be extracted from the underlying exception.

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
