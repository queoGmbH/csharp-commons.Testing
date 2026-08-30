using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// REQ-01: Bootstrap im Plain-Modus.

[TestFixture]
public class Req01_ValidBootstrap : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, _) => services.AddSingleton<IGreeter, Greeter>();

    [Test]
    public void GetService_Resolves_The_Production_Registration()
    {
        Assert.That(GetService<IGreeter>().Greet(), Is.EqualTo("Hallo"));
    }
}

[TestFixture]
public class Req01_NullBootstrap
{
    private class NullBootstrapFixture : IocIntegrationTestBase
    {
        protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices => null!;
    }

    [Test]
    public void SetUp_Throws_IntegrationTestSetupException_When_RegisterApplicationServices_Is_Null()
    {
        var fixture = new NullBootstrapFixture();

        var exception = Assert.Throws<IntegrationTestSetupException>(new Action(fixture.BaseSetUp));

        Assert.That(exception!.Message, Does.Contain("RegisterApplicationServices"));
    }
}

[TestFixture]
public class Req01_FailingBootstrap
{
    private class FailingBootstrapFixture : IocIntegrationTestBase
    {
        protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
            (_, _) => throw new InvalidOperationException("Boom");
    }

    [Test]
    public void SetUp_Wraps_Bootstrap_Exception_As_IntegrationTestSetupException()
    {
        var fixture = new FailingBootstrapFixture();

        var exception = Assert.Throws<IntegrationTestSetupException>(new Action(fixture.BaseSetUp));

        Assert.That(exception!.InnerException, Is.InstanceOf<InvalidOperationException>());
    }
}
