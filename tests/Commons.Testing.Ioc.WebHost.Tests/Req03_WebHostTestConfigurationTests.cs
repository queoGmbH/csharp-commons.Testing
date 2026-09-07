using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.WebHost.Tests;

// REQ-03: Providing the test configuration in web host mode.

[TestFixture]
public class Req03_WebHostTestConfiguration : WebHostIntegrationTestBase<Program>
{
    private string? _configurationSeenByConfigureOverrides;

    protected override IConfiguration BuildTestConfiguration()
    {
        var configFilePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");
        File.WriteAllText(configFilePath, /*lang=json,strict*/ "{ \"Greeting\": { \"Message\": \"Custom\" } }");

        try
        {
            return new ConfigurationBuilder()
                .AddJsonFile(configFilePath, optional: false)
                .Build();
        }
        finally
        {
            File.Delete(configFilePath);
        }
    }

    protected override void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
    {
        _configurationSeenByConfigureOverrides = configuration["Greeting:Message"];
    }

    [Test]
    public void TestConfiguration_Is_Merged_Into_The_Host_Configuration()
    {
        var hostConfiguration = GetService<IConfiguration>();

        Assert.That(hostConfiguration["Greeting:Message"], Is.EqualTo("Custom"));
    }

    [Test]
    public void ConfigureOverrides_Received_The_Same_TestConfiguration_As_The_Host()
    {
        Assert.That(_configurationSeenByConfigureOverrides, Is.EqualTo("Custom"));
    }
}
