namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Thrown by <see cref="IocTestBase.GetService{T}"/> when a service cannot be resolved. <see cref="Reason"/>
/// distinguishes whether no registration exists for the requested service type
/// (<see cref="ServiceResolutionFailureReason.MissingRegistration"/>) or whether resolution of an existing
/// registration failed (<see cref="ServiceResolutionFailureReason.ResolutionFailed"/>).
/// </summary>
public sealed class ServiceResolutionException : IntegrationTestException
{
    /// <summary>
    /// Creates a new instance for the <see cref="ServiceResolutionFailureReason.MissingRegistration"/> case.
    /// </summary>
    public ServiceResolutionException(ServiceResolutionFailureReason reason, Type serviceType, string message)
        : base(message)
    {
        Reason = reason;
        ServiceType = serviceType;
    }

    /// <summary>
    /// Creates a new instance for the <see cref="ServiceResolutionFailureReason.ResolutionFailed"/> case,
    /// with the original exception as <see cref="Exception.InnerException"/>.
    /// </summary>
    public ServiceResolutionException(ServiceResolutionFailureReason reason, Type serviceType, string message, Exception innerException)
        : base(message, innerException)
    {
        Reason = reason;
        ServiceType = serviceType;
    }

    /// <summary>
    /// The reason why service resolution failed.
    /// </summary>
    public ServiceResolutionFailureReason Reason { get; }

    /// <summary>
    /// The service type whose resolution failed.
    /// </summary>
    public Type ServiceType { get; }
}
