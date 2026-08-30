using NUnit.Framework;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Hilfsfunktion, um Fehlermeldungen konsistent mit dem aktuellen NUnit-Testkontext (Testklasse und, sofern
/// bekannt, Testmethode) anzureichern.
/// </summary>
public static class TestContextDescription
{
    /// <summary>
    /// Liefert eine Beschreibung des aktuellen Testkontexts fuer Fehlermeldungen, z. B.
    /// "Testklasse 'Foo', Testmethode 'Bar'". Die Testmethode wird ausgelassen, wenn sie im aktuellen
    /// Kontext nicht bekannt ist (z. B. innerhalb von OneTimeSetUp).
    /// </summary>
    public static string Current()
    {
        var test = TestContext.CurrentContext.Test;
        var className = string.IsNullOrEmpty(test.ClassName) ? "(unbekannt)" : test.ClassName;

        return test.MethodName is null || test.IsSuite
            ? $"Testklasse '{className}'"
            : $"Testklasse '{className}', Testmethode '{test.MethodName}'";
    }
}
