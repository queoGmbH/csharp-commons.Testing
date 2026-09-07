namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Common base class for all dedicated exceptions in this library. Error messages are consistently
/// formulated in German, by convention of this library, and are enriched with test context (at minimum the
/// test class, and the test method where known).
/// </summary>
public abstract class IntegrationTestException : Exception
{
    /// <summary>
    /// Creates a new instance with an error message.
    /// </summary>
    protected IntegrationTestException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates a new instance with an error message and the original exception as
    /// <see cref="Exception.InnerException"/>.
    /// </summary>
    protected IntegrationTestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
