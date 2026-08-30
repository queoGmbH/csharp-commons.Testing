using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Tests;

// REQ-03: Bereitstellung der Test-Konfiguration.

[TestFixture]
public class Req03_DefaultTestConfiguration : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, configuration) => services.AddSingleton(configuration);

    [Test]
    public void Default_BuildTestConfiguration_Does_Not_Throw_And_Is_Minimal()
    {
        var configuration = GetService<IConfiguration>();

        Assert.That(configuration["Some:Unconfigured:Key"], Is.Null);
    }
}

[TestFixture]
public class Req03_CustomTestConfiguration : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, configuration) => services.AddSingleton(configuration);

    protected override IConfiguration BuildTestConfiguration()
    {
        // Kombiniert eine (hier vorhandene) Konfigurationsdatei mit der optionalen Standarddatei, die in
        // diesem Test nicht existiert.
        var configFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(configFilePath, /*lang=json,strict*/ "{ \"Greeting\": { \"Message\": \"Custom\" } }");

        try
        {
            return new ConfigurationBuilder()
                .AddJsonFile("appsettings.integrationtests.json", optional: true)
                .AddJsonFile(configFilePath, optional: false)
                .Build();
        }
        finally
        {
            File.Delete(configFilePath);
        }
    }

    [Test]
    public void Overridden_BuildTestConfiguration_Is_Used_For_Bootstrap()
    {
        var configuration = GetService<IConfiguration>();

        Assert.That(configuration["Greeting:Message"], Is.EqualTo("Custom"));
    }
}
