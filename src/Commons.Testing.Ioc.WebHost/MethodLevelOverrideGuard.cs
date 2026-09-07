using System.Reflection;

using Commons.Testing.Ioc.Abstractions;

namespace Commons.Testing.Ioc.WebHost;

/// <summary>
/// Ensures that a test method in web host mode does not request a method-level service override
/// (<see cref="ServiceOverrideAttribute"/>). Implemented as a standalone class, decoupled from
/// <see cref="NUnit.Framework.TestContext"/>, so that this rule can be tested independently of a running
/// NUnit test execution.
/// </summary>
internal static class MethodLevelOverrideGuard
{
    public static void EnsureNoMethodLevelOverride(MethodInfo? methodInfo)
    {
        var overrideAttribute = ServiceOverrideAttributes.ReadFrom(methodInfo).FirstOrDefault();

        if (overrideAttribute is null)
        {
            return;
        }

        throw new InvalidOverrideConfigurationException(
            $"Methodenspezifische Service-Overrides ('{overrideAttribute.GetType().Name}') werden im " +
            $"Web-Host-Modus nicht unterstuetzt, da der Host testklassenweit aufgebaut wird. Verwenden " +
            $"Sie stattdessen einen klassenweiten Override (ConfigureOverrides) mit einer eigenen " +
            $"Testklasse oder einen separaten Test-Host fuer diesen Fall. {TestContextDescription.Current()}.");
    }
}
