using System.Reflection;

using NUnit.Framework;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Reads <see cref="ServiceOverrideAttribute"/> attributes of the current test method. Used both by plain
/// mode (applying the overrides) and web host mode (rejecting the overrides), so that both modes share the
/// same detection logic.
/// </summary>
public static class ServiceOverrideAttributes
{
    /// <summary>
    /// Returns the <see cref="MethodInfo"/> of the test method currently being executed by NUnit, if known.
    /// </summary>
    /// <remarks>
    /// <see cref="TestContext.TestAdapter.MethodInfo"/> does not return a full-fledged <see cref="MethodInfo"/>
    /// with access to custom attributes in current NUnit versions; the obsolete
    /// <see cref="TestContext.TestAdapter.Method"/> remains the only way to obtain one.
    /// </remarks>
    public static MethodInfo? CurrentTestMethod()
    {
#pragma warning disable CS0618
        return TestContext.CurrentContext.Test.Method?.MethodInfo;
#pragma warning restore CS0618
    }

    /// <summary>
    /// Reads all <see cref="ServiceOverrideAttribute"/> attributes of the given test method.
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
