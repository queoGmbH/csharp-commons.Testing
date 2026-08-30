namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Wird geworfen, wenn die Konfiguration von Service-Overrides ungueltig ist, z. B. weil fuer denselben
/// Service-Typ mehrere methodenspezifische Overrides angegeben wurden oder weil ein methodenspezifisches
/// Override in einem Modus verwendet wird, der dies nicht unterstuetzt (Web-Host-Modus).
/// </summary>
public sealed class InvalidOverrideConfigurationException : IntegrationTestException
{
    /// <inheritdoc />
    public InvalidOverrideConfigurationException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public InvalidOverrideConfigurationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
