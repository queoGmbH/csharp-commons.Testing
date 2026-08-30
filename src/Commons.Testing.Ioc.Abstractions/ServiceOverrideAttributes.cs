using System.Reflection;

using NUnit.Framework;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Liest <see cref="ServiceOverrideAttribute"/>-Attribute der aktuellen Testmethode aus. Wird sowohl vom
/// Plain-Modus (Anwendung der Overrides) als auch vom Web-Host-Modus (Ablehnung der Overrides, siehe REQ-06)
/// verwendet, damit beide Modi dieselbe Erkennungslogik teilen.
/// </summary>
public static class ServiceOverrideAttributes
{
    /// <summary>
    /// Liefert den <see cref="MethodInfo"/> der aktuell von NUnit ausgefuehrten Testmethode, sofern bekannt.
    /// </summary>
    /// <remarks>
    /// <see cref="TestContext.TestAdapter.MethodInfo"/> liefert in aktuellen NUnit-Versionen keinen
    /// vollwertigen <see cref="MethodInfo"/> mit Zugriff auf Custom Attributes; das veraltete
    /// <see cref="TestContext.TestAdapter.Method"/> ist dafuer weiterhin der einzige Weg.
    /// </remarks>
    public static MethodInfo? CurrentTestMethod()
    {
#pragma warning disable CS0618
        return TestContext.CurrentContext.Test.Method?.MethodInfo;
#pragma warning restore CS0618
    }

    /// <summary>
    /// Liest alle <see cref="ServiceOverrideAttribute"/>-Attribute der angegebenen Testmethode aus.
    /// </summary>
    public static IReadOnlyList<ServiceOverrideAttribute> ReadFrom(MethodInfo? methodInfo)
    {
        if (methodInfo is null)
        {
            return Array.Empty<ServiceOverrideAttribute>();
        }

        return methodInfo.GetCustomAttributes<ServiceOverrideAttribute>(inherit: true).ToList();
    }
}
