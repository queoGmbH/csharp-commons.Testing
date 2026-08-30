namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Wird von <see cref="IocTestBase.GetService{T}"/> geworfen, wenn ein Service nicht aufgeloest werden kann.
/// <see cref="Reason"/> unterscheidet, ob fuer den angefragten Service-Typ keine Registrierung existiert
/// (<see cref="ServiceResolutionFailureReason.MissingRegistration"/>) oder ob die Aufloesung einer
/// vorhandenen Registrierung fehlgeschlagen ist (<see cref="ServiceResolutionFailureReason.ResolutionFailed"/>),
/// siehe REQ-07.
/// </summary>
public sealed class ServiceResolutionException : IntegrationTestException
{
    /// <summary>
    /// Erstellt eine neue Instanz fuer den Fall <see cref="ServiceResolutionFailureReason.MissingRegistration"/>.
    /// </summary>
    public ServiceResolutionException(ServiceResolutionFailureReason reason, Type serviceType, string message)
        : base(message)
    {
        Reason = reason;
        ServiceType = serviceType;
    }

    /// <summary>
    /// Erstellt eine neue Instanz fuer den Fall <see cref="ServiceResolutionFailureReason.ResolutionFailed"/>,
    /// mit der urspruenglichen Exception als <see cref="Exception.InnerException"/>.
    /// </summary>
    public ServiceResolutionException(ServiceResolutionFailureReason reason, Type serviceType, string message, Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
        ServiceType = serviceType;
    }

    /// <summary>
    /// Der Grund, warum die Serviceauflosung fehlgeschlagen ist.
    /// </summary>
    public ServiceResolutionFailureReason Reason { get; }

    /// <summary>
    /// Der Service-Typ, dessen Aufloesung fehlgeschlagen ist.
    /// </summary>
    public Type ServiceType { get; }
}
