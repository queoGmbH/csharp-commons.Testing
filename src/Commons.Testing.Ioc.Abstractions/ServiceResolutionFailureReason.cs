namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Unterscheidet die beiden in <see cref="ServiceResolutionException"/> gemeldeten Fehlerfaelle bei der
/// Serviceauflosung durch <see cref="IocTestBase.GetService{T}"/> (siehe REQ-07).
/// </summary>
public enum ServiceResolutionFailureReason
{
    /// <summary>
    /// Fuer den angefragten Service-Typ existiert keine Registrierung.
    /// </summary>
    MissingRegistration,

    /// <summary>
    /// Fuer den angefragten Service-Typ existiert eine Registrierung, ihre Aufloesung ist jedoch
    /// fehlgeschlagen, z. B. durch eine Exception im Konstruktor eines Service oder einer seiner
    /// Abhaengigkeiten, oder durch eine von <c>validateScopes</c> aufgedeckte Captive Dependency.
    /// </summary>
    ResolutionFailed,
}
