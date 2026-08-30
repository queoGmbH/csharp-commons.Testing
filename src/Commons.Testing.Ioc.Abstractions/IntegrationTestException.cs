namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Gemeinsame Basisklasse aller dedizierten Exceptions dieser Bibliothek. Fehlermeldungen sind gemaess
/// den Rahmenbedingungen der Bibliothek konsistent auf Deutsch formuliert und werden mit Testkontext
/// (mindestens Testklasse, wo bekannt auch Testmethode) angereichert.
/// </summary>
public abstract class IntegrationTestException : Exception
{
    /// <summary>
    /// Erstellt eine neue Instanz mit einer Fehlermeldung.
    /// </summary>
    protected IntegrationTestException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Erstellt eine neue Instanz mit einer Fehlermeldung und der urspruenglichen Exception als
    /// <see cref="Exception.InnerException"/>.
    /// </summary>
    protected IntegrationTestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
