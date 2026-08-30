using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// ASP.NET-Core-freie Basis fuer beide Betriebsarten der Integrationstest-Infrastruktur (Plain-Modus und
/// Web-Host-Modus). Definiert die gemeinsame Zugriffs-API (<see cref="Services"/>, <see cref="GetService{T}"/>)
/// sowie die gemeinsamen Erweiterungspunkte <see cref="BuildTestConfiguration"/> und
/// <see cref="ConfigureOverrides"/>, die von beiden Modi in derselben Reihenfolge relativ zum produktiven
/// Bootstrap ausgefuehrt werden.
/// </summary>
public abstract class IocTestBase
{
    private IServiceProvider? _services;

    /// <summary>
    /// Der <see cref="IServiceProvider"/> des aktuellen Tests. Steht erst nach erfolgreichem Setup zur
    /// Verfuegung.
    /// </summary>
    protected IServiceProvider Services
    {
        get => _services ?? throw new InvalidOperationException(
            $"Services ist erst nach erfolgreichem Testaufbau verfuegbar. {TestContextDescription.Current()}.");
        set => _services = value;
    }

    /// <summary>
    /// Loest einen Service aus <see cref="Services"/> auf. Unterscheidet ueber
    /// <see cref="ServiceResolutionException.Reason"/>, ob keine Registrierung fuer <typeparamref name="T"/>
    /// existiert (<see cref="ServiceResolutionFailureReason.MissingRegistration"/>) oder ob eine vorhandene
    /// Registrierung nicht aufgeloest werden konnte (<see cref="ServiceResolutionFailureReason.ResolutionFailed"/>),
    /// z. B. durch eine Exception im Konstruktor eines Service oder einer seiner Abhaengigkeiten, oder durch
    /// eine von <c>validateScopes</c> aufgedeckte Captive Dependency (siehe REQ-07).
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
    /// Macht <see cref="Services"/> nach dem Teardown wieder unverfuegbar, damit ein versehentlicher Zugriff
    /// nach Freigabe des Providers/Scopes die klare Fehlermeldung von <see cref="Services"/> ausloest statt
    /// einer <see cref="ObjectDisposedException"/> des bereits freigegebenen Providers.
    /// </summary>
    protected void ClearServices() => _services = null;

    /// <summary>
    /// Baut die <see cref="IConfiguration"/> des Testkontexts auf. Ohne Override wird eine minimale
    /// Konfiguration erzeugt, die optional die Datei <c>appsettings.integrationtests.json</c> beruecksichtigt,
    /// sofern diese vorhanden ist. Abgeleitete Testklassen koennen diesen Hook ueberschreiben, um In-Memory-
    /// Werte und/oder eigene Konfigurationsdateien zu kombinieren.
    /// </summary>
    protected virtual IConfiguration BuildTestConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.integrationtests.json", optional: true, reloadOnChange: false)
            .Build();
    }

    /// <summary>
    /// Klassenweiter Hook fuer Service-Overrides. Wird nach dem produktiven Bootstrap ausgefuehrt und ist
    /// standardmaessig ein No-op. Ein Override fuer einen Service-Typ soll alle bisherigen Registrierungen
    /// dieses Typs ersetzen, z. B. mittels <c>services.RemoveAll(serviceType)</c> gefolgt von
    /// <c>services.Add(...)</c> (nicht <c>services.Replace(...)</c>, da dieses nur die erste passende
    /// Registrierung entfernt). So werden auch alle Eintraege entfernt, die ueber
    /// <c>IEnumerable&lt;T&gt;</c> sichtbar waeren, sodass <see cref="GetService{T}"/> und die Aufloesung von
    /// <c>IEnumerable&lt;T&gt;</c> danach konsistent nur die Override-Registrierung sehen.
    /// </summary>
    protected virtual void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
    {
    }
}
