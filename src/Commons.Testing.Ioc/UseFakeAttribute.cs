using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commons.Testing.Ioc;

/// <summary>
/// Methodenspezifisches Service-Override fuer den Plain-Modus: Ersetzt beim mit diesem Attribut markierten
/// Test alle Registrierungen des angegebenen Service-Typs durch den angegebenen Implementierungstyp.
/// Nur im Plain-Modus (<see cref="IocIntegrationTestBase"/>) unterstuetzt; im Web-Host-Modus fuehrt die
/// Verwendung zu einem Konfigurationsfehler beim Testaufbau.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class UseFakeAttribute : ServiceOverrideAttribute
{
    private readonly Type _implementationType;
    private readonly ServiceLifetime? _lifetime;

    /// <summary>
    /// Erstellt ein neues methodenspezifisches Override. Die Lifetime wird von der zu ersetzenden
    /// Produktivregistrierung uebernommen (oder ist <see cref="ServiceLifetime.Singleton"/>, falls keine
    /// Registrierung fuer <paramref name="serviceType"/> existiert).
    /// </summary>
    /// <param name="serviceType">Der zu ersetzende Service-Typ.</param>
    /// <param name="implementationType">Der Implementierungstyp des Fakes.</param>
    public UseFakeAttribute(Type serviceType, Type implementationType)
        : base(serviceType)
    {
        _implementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
    }

    /// <summary>
    /// Erstellt ein neues methodenspezifisches Override mit explizit angegebener Lifetime.
    /// </summary>
    /// <param name="serviceType">Der zu ersetzende Service-Typ.</param>
    /// <param name="implementationType">Der Implementierungstyp des Fakes.</param>
    /// <param name="lifetime">Die Lifetime, mit der der Fake registriert wird.</param>
    public UseFakeAttribute(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        : base(serviceType)
    {
        _implementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        _lifetime = lifetime;
    }

    /// <inheritdoc />
    public override void ApplyOverride(IServiceCollection services, IConfiguration configuration)
    {
        // Ohne explizit angegebene Lifetime wird die Lifetime der zu ersetzenden Registrierung uebernommen,
        // statt pauschal Singleton zu erzwingen: Ein per Default auf Singleton hochgestufter Fake fuer einen
        // eigentlich Scoped/Transient registrierten Service kann sonst mit validateScopes (NFR-05) eine
        // Captive-Dependency-Exception ausloesen, die nichts mit einem Fehler der Testautorin/des Testautors
        // zu tun hat.
        var lifetime = _lifetime
            ?? services.LastOrDefault(descriptor => descriptor.ServiceType == ServiceType)?.Lifetime
            ?? ServiceLifetime.Singleton;

        // services.Replace(...) entfernt nur die erste passende Registrierung, nicht alle. Fuer eine
        // deterministische, vollstaendige Ersetzung (auch bei mehreren Vorregistrierungen desselben Typs,
        // z. B. sichtbar ueber IEnumerable<T>) werden daher zunaechst alle Registrierungen entfernt.
        services.RemoveAll(ServiceType);
        services.Add(ServiceDescriptor.Describe(ServiceType, _implementationType, lifetime));
    }
}
