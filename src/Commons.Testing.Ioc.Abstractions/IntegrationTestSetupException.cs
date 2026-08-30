namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Wird geworfen, wenn der Testaufbau (Container- oder Hostaufbau) fehlschlaegt, z. B. weil eine
/// erforderliche Registrierungsfunktion fehlt oder unerwartet <see langword="null"/> liefert, oder weil
/// der produktive Bootstrap eine Exception wirft.
/// </summary>
public sealed class IntegrationTestSetupException : IntegrationTestException
{
    /// <inheritdoc />
    public IntegrationTestSetupException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public IntegrationTestSetupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
