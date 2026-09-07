using NUnit.Framework;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Helper to consistently enrich error messages with the current NUnit test context (test class and, if
/// known, test method).
/// </summary>
public static class TestContextDescription
{
    /// <summary>
    /// Returns a description of the current test context for error messages, e.g.
    /// "Testklasse 'Foo', Testmethode 'Bar'". The test method is omitted if it is not known in the current
    /// context (e.g. inside OneTimeSetUp).
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
