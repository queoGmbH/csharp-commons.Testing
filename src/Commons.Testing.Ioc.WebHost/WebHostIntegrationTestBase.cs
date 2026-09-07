using Commons.Testing.Ioc.Abstractions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Commons.Testing.Ioc.WebHost;

/// <summary>
/// Base class for integration tests with a real ASP.NET Core host via <see cref="WebApplicationFactory{TEntryPoint}"/>.
/// The host is built once per test class from <typeparamref name="TEntryPoint"/> (<c>OneTimeSetUp</c> /
/// <c>OneTimeTearDown</c>); for each test, only a new <see cref="AsyncServiceScope"/> is created
/// (<c>SetUp</c> / <c>TearDown</c>). The production service registration of <typeparamref name="TEntryPoint"/>
/// is not invoked again manually; test-side adjustments (<c>ConfigureOverrides</c>) are instead applied via
/// <c>ConfigureTestServices</c>, after the production host bootstrap has already run.
/// </summary>
/// <remarks>
/// <typeparamref name="TEntryPoint"/> must be visible to the test assembly (e.g. via
/// <c>public partial class Program</c> or <c>[assembly: InternalsVisibleTo(...)]</c> in the application
/// under test). Method-level service overrides (<see cref="ServiceOverrideAttribute"/>) are not supported in
/// this mode.
/// </remarks>
/// <remarks>
/// The base class holds no shared mutable static state and is therefore compatible with parallel NUnit
/// execution at the fixture level. Thread safety of the production registration logic of
/// <typeparamref name="TEntryPoint"/> and its production singletons is the responsibility of the
/// application.
/// </remarks>
public abstract class WebHostIntegrationTestBase<TEntryPoint> : IocTestBase
    where TEntryPoint : class
{
    private WebApplicationFactory<TEntryPoint>? _factory;
    private AsyncServiceScope? _scope;

    /// <summary>
    /// Builds the ASP.NET Core host once for the entire test class.
    /// </summary>
    [OneTimeSetUp]
    public void BaseOneTimeSetUp()
    {
        try
        {
            var testConfiguration = BuildTestConfiguration();

            _factory = new WebApplicationFactory<TEntryPoint>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddConfiguration(testConfiguration);
                });

                builder.ConfigureTestServices(services =>
                {
                    ConfigureOverrides(services, testConfiguration);
                });
            });

            // Touches the host so that a faulty bootstrap becomes visible here already, rather than only on
            // the first test access to services.
            _ = _factory.Services;
        }
        catch (Exception ex)
        {
            throw new IntegrationTestSetupException(
                DiFailureDiagnostics.DescribeSetupFailure(
                    $"Der Aufbau des Test-Hosts fuer '{typeof(TEntryPoint).FullName}' ist fehlgeschlagen. " +
                    $"{TestContextDescription.Current()}.", ex),
                ex);
        }
    }

    /// <summary>
    /// Creates a new <see cref="AsyncServiceScope"/> before each test. Aborts if the test method requests a
    /// method-level service override.
    /// </summary>
    [SetUp]
    public void BaseSetUp()
    {
        MethodLevelOverrideGuard.EnsureNoMethodLevelOverride(ServiceOverrideAttributes.CurrentTestMethod());

        _scope = _factory!.Services.CreateAsyncScope();
        Services = _scope.Value.ServiceProvider;
    }

    /// <summary>
    /// Releases the scope of the current test.
    /// </summary>
    [TearDown]
    public async Task BaseTearDown()
    {
        if (_scope is not null)
        {
            await _scope.Value.DisposeAsync();
            _scope = null;
        }

        ClearServices();
    }

    /// <summary>
    /// Releases the test host after the test class has completed.
    /// </summary>
    [OneTimeTearDown]
    public async Task BaseOneTimeTearDown()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }
}
