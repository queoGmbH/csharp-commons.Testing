using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// REQ-10: Error diagnosis during test setup - the test context is always included, the original exception
// is preserved as InnerException, and the triggering service type of a generic container setup failure is
// named if it can be extracted from the underlying exception (best-effort rule "if determinable").

[TestFixture]
public class Req10_ErrorDiagnosisTests
{
    private class ExtractableFailureFixture : IocIntegrationTestBase
    {
        protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
            (_, _) => throw new InvalidOperationException(
                "Unable to resolve service for type 'Commons.Testing.Ioc.Tests.IGreeter' while attempting to activate 'Foo'.");
    }

    private class UnextractableFailureFixture : IocIntegrationTestBase
    {
        protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
            (_, _) => throw new InvalidOperationException("Boom, ganz ohne Typnamen.");
    }

    [Test]
    public void SetUp_Failure_Names_The_Triggering_Service_Type_When_Extractable_From_The_Underlying_Exception()
    {
        var fixture = new ExtractableFailureFixture();

        var exception = Assert.Throws<IntegrationTestSetupException>(new Action(fixture.BaseSetUp));

        Assert.That(exception!.Message, Does.Contain("IGreeter"));
        Assert.That(exception.Message, Does.Contain("ermittelbar"));
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>());
    }

    [Test]
    public void SetUp_Failure_Still_Contains_TestContext_And_InnerException_When_No_Type_Is_Extractable()
    {
        var fixture = new UnextractableFailureFixture();

        var exception = Assert.Throws<IntegrationTestSetupException>(new Action(fixture.BaseSetUp));

        Assert.That(exception!.Message, Does.Not.Contain("ermittelbar"));
        Assert.That(exception.Message, Does.Contain("Testklasse"));
        Assert.That(exception.InnerException, Is.InstanceOf<InvalidOperationException>());
    }
}
