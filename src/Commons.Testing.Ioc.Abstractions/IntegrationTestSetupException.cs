namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Thrown when test setup (container or host bootstrap) fails, e.g. because a required registration
/// function is missing or unexpectedly returns <see langword="null"/>, or because the production bootstrap
/// throws an exception.
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
