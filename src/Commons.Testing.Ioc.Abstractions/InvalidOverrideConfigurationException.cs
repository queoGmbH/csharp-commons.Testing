namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Thrown when the configuration of service overrides is invalid, e.g. because multiple method-level
/// overrides were specified for the same service type, or because a method-level override is used in a
/// mode that does not support it (web host mode).
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
