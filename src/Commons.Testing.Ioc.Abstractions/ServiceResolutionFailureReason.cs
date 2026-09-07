namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Distinguishes the two failure cases reported in <see cref="ServiceResolutionException"/> during service
/// resolution by <see cref="IocTestBase.GetService{T}"/>.
/// </summary>
public enum ServiceResolutionFailureReason
{
    /// <summary>
    /// No registration exists for the requested service type.
    /// </summary>
    MissingRegistration,

    /// <summary>
    /// A registration exists for the requested service type, but its resolution failed, e.g. due to an
    /// exception in the constructor of a service or one of its dependencies, or due to a captive dependency
    /// uncovered by <c>validateScopes</c>.
    /// </summary>
    ResolutionFailed,
}
