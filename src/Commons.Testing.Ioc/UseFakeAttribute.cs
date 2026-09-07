using Commons.Testing.Ioc.Abstractions;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Commons.Testing.Ioc;

/// <summary>
/// Method-level service override for plain mode: replaces all registrations of the given service type with
/// the given implementation type for the test marked with this attribute. Only supported in plain mode
/// (<see cref="IocIntegrationTestBase"/>); in web host mode, using it results in a configuration error
/// during test setup.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class UseFakeAttribute : ServiceOverrideAttribute
{
    private readonly Type _implementationType;
    private readonly ServiceLifetime? _lifetime;

    /// <summary>
    /// Creates a new method-level override. The lifetime is taken from the production registration being
    /// replaced (or is <see cref="ServiceLifetime.Singleton"/> if no registration exists for
    /// <paramref name="serviceType"/>).
    /// </summary>
    /// <param name="serviceType">The service type to replace.</param>
    /// <param name="implementationType">The implementation type of the fake.</param>
    public UseFakeAttribute(Type serviceType, Type implementationType)
        : base(serviceType)
    {
        _implementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
    }

    /// <summary>
    /// Creates a new method-level override with an explicitly specified lifetime.
    /// </summary>
    /// <param name="serviceType">The service type to replace.</param>
    /// <param name="implementationType">The implementation type of the fake.</param>
    /// <param name="lifetime">The lifetime with which the fake is registered.</param>
    public UseFakeAttribute(Type serviceType, Type implementationType, ServiceLifetime lifetime)
        : base(serviceType)
    {
        _implementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        _lifetime = lifetime;
    }

    /// <inheritdoc />
    public override void ApplyOverride(IServiceCollection services, IConfiguration configuration)
    {
        // Without an explicitly specified lifetime, the lifetime of the registration being replaced is used,
        // instead of unconditionally forcing Singleton: a fake that defaults to Singleton for a service that
        // is actually registered as Scoped/Transient could otherwise trigger a captive dependency exception
        // via validateScopes, which has nothing to do with a mistake by the test author.
        var lifetime = _lifetime
            ?? services.LastOrDefault(descriptor => descriptor.ServiceType == ServiceType)?.Lifetime
            ?? ServiceLifetime.Singleton;

        // services.Replace(...) only removes the first matching registration, not all of them. For a
        // deterministic, complete replacement (even with multiple prior registrations of the same type, e.g.
        // visible via IEnumerable<T>), all registrations are therefore removed first.
        services.RemoveAll(ServiceType);
        services.Add(ServiceDescriptor.Describe(ServiceType, _implementationType, lifetime));
    }
}
