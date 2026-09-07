using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Commons.Testing.Ioc.Abstractions;

/// <summary>
/// Base class for attributes that allow individual test methods to request additional, method-level
/// service overrides. Evaluated exclusively in plain mode (<c>IocIntegrationTestBase</c>); in web host mode,
/// using such an attribute results in a configuration error during test setup.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public abstract class ServiceOverrideAttribute : Attribute
{
    /// <summary>
    /// Creates a new instance for the given service type.
    /// </summary>
    protected ServiceOverrideAttribute(Type serviceType)
    {
        ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
    }

    /// <summary>
    /// The service type for which this attribute requests an override. Only one method-level override may
    /// be specified per test method for the same service type.
    /// </summary>
    public Type ServiceType { get; }

    /// <summary>
    /// Applies the override to the service graph of the current test method. Executed after the production
    /// bootstrap and after the class-wide overrides (<c>ConfigureOverrides</c>).
    /// </summary>
    public abstract void ApplyOverride(IServiceCollection services, IConfiguration configuration);
}
