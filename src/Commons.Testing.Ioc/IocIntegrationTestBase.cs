using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Commons.Testing.Ioc;

/// <summary>
/// Basisklasse fuer Integrationstests ohne ASP.NET-Core-Host. Baut vor jedem Test einen vollstaendig neuen
/// <see cref="IServiceProvider"/> auf: produktiver Bootstrap (<see cref="RegisterApplicationServices"/>),
/// dann klassenweite Overrides (<c>ConfigureOverrides</c>), dann methodenspezifische Overrides
/// (<see cref="ServiceOverrideAttribute"/>, z. B. <see cref="UseFakeAttribute"/>).
/// </summary>
public abstract class IocIntegrationTestBase : IocTestBase
{
    private ServiceProvider? _provider;

    /// <summary>
    /// Registrierungsfunktion, die den produktionsnahen Servicegraphen der Anwendung aufbaut. Muss von der
    /// abgeleiteten Testklasse bereitgestellt werden und darf zur Laufzeit nicht <see langword="null"/>
    /// liefern.
    /// </summary>
    protected abstract Action<IServiceCollection, IConfiguration> RegisterApplicationServices { get; }

    /// <summary>
    /// Baut vor jedem Test einen neuen <see cref="IServiceProvider"/> auf.
    /// </summary>
    [SetUp]
    public void BaseSetUp()
    {
        var registerApplicationServices = RegisterApplicationServices ?? throw new IntegrationTestSetupException(
            $"Die Registrierungsfunktion 'RegisterApplicationServices' liefert 'null'. Sie muss den " +
            $"produktiven Servicegraphen registrieren. {TestContextDescription.Current()}.");

        IConfiguration configuration;
        var services = new ServiceCollection();

        try
        {
            configuration = BuildTestConfiguration();
            registerApplicationServices(services, configuration);
            ConfigureOverrides(services, configuration);
            ApplyMethodLevelOverrides(services, configuration, ServiceOverrideAttributes.CurrentTestMethod());
        }
        catch (IntegrationTestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new IntegrationTestSetupException(
                $"Der Testaufbau des Servicegraphen ist fehlgeschlagen. {TestContextDescription.Current()}.", ex);
        }

        try
        {
            _provider = services.BuildServiceProvider(validateScopes: true);
        }
        catch (Exception ex)
        {
            throw new IntegrationTestSetupException(
                $"Der Testaufbau des Servicegraphen ist fehlgeschlagen. {TestContextDescription.Current()}.", ex);
        }

        Services = _provider;
    }

    /// <summary>
    /// Gibt den <see cref="IServiceProvider"/> des Tests frei. Verwendet <c>DisposeAsync()</c>, da
    /// <c>Dispose()</c> fuer Services, die ausschliesslich <see cref="IAsyncDisposable"/> implementieren,
    /// eine Exception wirft (siehe REQ-09).
    /// </summary>
    [TearDown]
    public async Task BaseTearDown()
    {
        if (_provider is not null)
        {
            await _provider.DisposeAsync();
            _provider = null;
        }

        ClearServices();
    }

    /// <summary>
    /// Wertet die <see cref="ServiceOverrideAttribute"/>-Attribute der aktuellen Testmethode aus und wendet
    /// sie an. Nimmt den <see cref="System.Reflection.MethodInfo"/> explizit als Parameter entgegen (statt ihn
    /// selbst ueber NUnit's <see cref="NUnit.Framework.TestContext"/> zu ermitteln), damit die
    /// Konfigurationsfehler-Erkennung (doppelte Overrides fuer denselben Service-Typ) unabhaengig von einer
    /// laufenden NUnit-Testausfuehrung testbar ist.
    /// </summary>
    internal static void ApplyMethodLevelOverrides(
        IServiceCollection services, IConfiguration configuration, System.Reflection.MethodInfo? methodInfo)
    {
        var overrideAttributes = ServiceOverrideAttributes.ReadFrom(methodInfo);

        var duplicateServiceTypes = overrideAttributes
            .GroupBy(attribute => attribute.ServiceType)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateServiceTypes.Count > 0)
        {
            var serviceTypeNames = string.Join(", ", duplicateServiceTypes.Select(type => type.FullName));
            throw new InvalidOverrideConfigurationException(
                $"Fuer den/die Service-Typ(en) '{serviceTypeNames}' wurden mehrere methodenspezifische " +
                $"Overrides angegeben. Pro Service-Typ ist nur ein methodenspezifisches Override je " +
                $"Testmethode zulaessig. {TestContextDescription.Current()}.");
        }

        foreach (var overrideAttribute in overrideAttributes)
        {
            overrideAttribute.ApplyOverride(services, configuration);
        }
    }
}
