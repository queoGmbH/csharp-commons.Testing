using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Basisklasse fuer Attribute, mit denen einzelne Testmethoden zusaetzliche, methodenspezifische
/// Service-Overrides anfordern koennen. Wird ausschliesslich im Plain-Modus (<c>IocIntegrationTestBase</c>)
/// ausgewertet; im Web-Host-Modus fuehrt die Verwendung eines solchen Attributs zu einem Konfigurationsfehler
/// beim Testaufbau.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class ServiceOverrideAttribute : Attribute
{
    /// <summary>
    /// Erstellt eine neue Instanz fuer den angegebenen Service-Typ.
    /// </summary>
    protected ServiceOverrideAttribute(Type serviceType)
    {
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
    }

    /// <summary>
    /// Der Service-Typ, fuer den dieses Attribut ein Override anfordert. Fuer denselben Service-Typ darf
    /// pro Testmethode nur ein methodenspezifisches Override angegeben werden.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Wendet das Override auf den Servicegraphen der aktuellen Testmethode an. Wird nach dem produktiven
    /// Bootstrap und nach den klassenweiten Overrides (<c>ConfigureOverrides</c>) ausgefuehrt.
    /// </summary>
    public abstract void ApplyOverride(IServiceCollection services, IConfiguration configuration);
}
