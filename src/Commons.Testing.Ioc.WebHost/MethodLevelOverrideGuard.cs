using System.Reflection;

using Commons.Testing.Ioc.Abstractions;

namespace Commons.Testing.Ioc.WebHost;

/// <summary>
/// Prueft, dass eine Testmethode im Web-Host-Modus kein methodenspezifisches Service-Override
/// (<see cref="ServiceOverrideAttribute"/>) anfordert. Als eigenstaendige, von
/// <see cref="NUnit.Framework.TestContext"/> entkoppelte Klasse implementiert, damit REQ-06 unabhaengig von
/// einer laufenden NUnit-Testausfuehrung testbar ist.
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
