using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// ASP.NET Core-free base for both operating modes of the integration test infrastructure (plain mode and
/// web host mode). Defines the shared access API (<see cref="Services"/>, <see cref="GetService{T}"/>) as
/// well as the shared extension points <see cref="BuildTestConfiguration"/> and
/// <see cref="ConfigureOverrides"/>, which are executed by both modes in the same order relative to the
/// production bootstrap.
/// </summary>
public abstract class IocTestBase
{
    private IServiceProvider? _services;

    /// <summary>
    /// The <see cref="IServiceProvider"/> of the current test. Only available after successful setup.
    /// </summary>
    protected IServiceProvider Services
    {
        get => _services ?? throw new InvalidOperationException(
            $"Services ist erst nach erfolgreichem Testaufbau verfuegbar. {TestContextDescription.Current()}.");
        set => _services = value;
    }

    /// <summary>
    /// Resolves a service from <see cref="Services"/>. Uses <see cref="ServiceResolutionException.Reason"/>
    /// to distinguish whether no registration exists for <typeparamref name="T"/>
    /// (<see cref="ServiceResolutionFailureReason.MissingRegistration"/>) or whether an existing registration
    /// could not be resolved (<see cref="ServiceResolutionFailureReason.ResolutionFailed"/>), e.g. due to an
    /// exception in the constructor of a service or one of its dependencies, or due to a captive dependency
    /// uncovered by <c>validateScopes</c>.
    /// </summary>
    protected T GetService<T>() where T : notnull
    {
        var services = Services;

        T? service;

        try
        {
            service = services.GetService<T>();
        }
        catch (Exception ex)
        {
            throw new ServiceResolutionException(
                ServiceResolutionFailureReason.ResolutionFailed,
                typeof(T),
                $"Die Aufloesung des Service-Typs '{typeof(T).FullName}' ist fehlgeschlagen. " +
                $"{TestContextDescription.Current()}.",
                ex);
        }

        return service ?? throw new ServiceResolutionException(
            ServiceResolutionFailureReason.MissingRegistration,
            typeof(T),
            $"Fuer den Service-Typ '{typeof(T).FullName}' existiert keine Registrierung. " +
            $"{TestContextDescription.Current()}.");
    }

    /// <summary>
    /// Makes <see cref="Services"/> unavailable again after teardown, so that an accidental access after the
    /// provider/scope has been disposed triggers the clear error message from <see cref="Services"/> instead
    /// of an <see cref="ObjectDisposedException"/> from the already-disposed provider.
    /// </summary>
    protected void ClearServices() => _services = null;

    /// <summary>
    /// Builds the <see cref="IConfiguration"/> of the test context. Without an override, a minimal
    /// configuration is created that optionally takes the file <c>appsettings.integrationtests.json</c> into
    /// account, if present. Derived test classes can override this hook to combine in-memory values and/or
    /// their own configuration files.
    /// </summary>
    protected virtual IConfiguration BuildTestConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.integrationtests.json", optional: true, reloadOnChange: false)
            .Build();
    }

    /// <summary>
    /// Class-wide hook for service overrides. Executed after the production bootstrap and is a no-op by
    /// default. An override for a service type should replace all previous registrations of that type, e.g.
    /// using <c>services.RemoveAll(serviceType)</c> followed by <c>services.Add(...)</c> (not
    /// <c>services.Replace(...)</c>, since that only removes the first matching registration). This also
    /// removes all entries that would be visible via <c>IEnumerable&lt;T&gt;</c>, so that
    /// <see cref="GetService{T}"/> and resolution of <c>IEnumerable&lt;T&gt;</c> afterwards consistently see
    /// only the override registration.
    /// </summary>
    protected virtual void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
    {
    }
}
