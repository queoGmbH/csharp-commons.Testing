using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Commons.Testing.Ioc;

/// <summary>
/// Base class for integration tests without an ASP.NET Core host. Builds a completely new
/// <see cref="IServiceProvider"/> before each test: production bootstrap
/// (<see cref="RegisterApplicationServices"/>), then class-wide overrides (<c>ConfigureOverrides</c>), then
/// method-level overrides (<see cref="ServiceOverrideAttribute"/>, e.g. <see cref="UseFakeAttribute"/>).
/// </summary>
/// <remarks>
/// The base class holds no shared mutable static state and is therefore compatible with parallel NUnit
/// execution at the fixture level. Thread safety of the production registration logic registered by
/// <see cref="RegisterApplicationServices"/>, and of production singletons, is the responsibility of the
/// application.
/// </remarks>
public abstract class IocIntegrationTestBase : IocTestBase
{
    private ServiceProvider? _provider;

    /// <summary>
    /// Registration function that builds the application's production-like service graph. Must be provided
    /// by the derived test class and must not return <see langword="null"/> at runtime.
    /// </summary>
    protected abstract Action<IServiceCollection, IConfiguration> RegisterApplicationServices { get; }

    /// <summary>
    /// Builds a new <see cref="IServiceProvider"/> before each test.
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
                DiFailureDiagnostics.DescribeSetupFailure(
                    $"Der Testaufbau des Servicegraphen ist fehlgeschlagen. {TestContextDescription.Current()}.", ex),
                ex);
        }

        try
        {
            _provider = services.BuildServiceProvider(validateScopes: true);
        }
        catch (Exception ex)
        {
            throw new IntegrationTestSetupException(
                DiFailureDiagnostics.DescribeSetupFailure(
                    $"Der Testaufbau des Servicegraphen ist fehlgeschlagen. {TestContextDescription.Current()}.", ex),
                ex);
        }

        Services = _provider;
    }

    /// <summary>
    /// Releases the test's <see cref="IServiceProvider"/>. Uses <c>DisposeAsync()</c>, since <c>Dispose()</c>
    /// throws an exception for services that implement only <see cref="IAsyncDisposable"/>.
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
    /// Reads the <see cref="ServiceOverrideAttribute"/> attributes of the current test method and applies
    /// them. Takes the <see cref="System.Reflection.MethodInfo"/> explicitly as a parameter (instead of
    /// determining it itself via NUnit's <see cref="NUnit.Framework.TestContext"/>), so that the
    /// configuration error detection (duplicate overrides for the same service type) can be tested
    /// independently of a running NUnit test execution.
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
