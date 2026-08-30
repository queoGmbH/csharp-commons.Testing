using Commons.Testing.Ioc.Abstractions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using NUnit.Framework;

namespace Commons.Testing.Ioc.WebHost;

/// <summary>
/// Basisklasse fuer Integrationstests mit echtem ASP.NET-Core-Host ueber <see cref="WebApplicationFactory{TEntryPoint}"/>.
/// Der Host wird einmal pro Testklasse aus <typeparamref name="TEntryPoint"/> aufgebaut (<c>OneTimeSetUp</c>
/// / <c>OneTimeTearDown</c>); pro Test wird lediglich ein neuer <see cref="AsyncServiceScope"/> erzeugt
/// (<c>SetUp</c> / <c>TearDown</c>). Die produktive Service-Registrierung von <typeparamref name="TEntryPoint"/>
/// wird dabei nicht erneut manuell aufgerufen; testseitige Anpassungen (<c>ConfigureOverrides</c>) werden
/// stattdessen ueber <c>ConfigureTestServices</c> angewendet, nachdem der produktive Host-Bootstrap bereits
/// gelaufen ist.
/// </summary>
/// <remarks>
/// <typeparamref name="TEntryPoint"/> muss fuer das Testassembly sichtbar sein (z. B. ueber
/// <c>public partial class Program</c> oder <c>[assembly: InternalsVisibleTo(...)]</c> in der getesteten
/// Anwendung). Methodenspezifische Service-Overrides (<see cref="ServiceOverrideAttribute"/>) werden in
/// diesem Modus nicht unterstuetzt, siehe REQ-06.
/// </remarks>
public abstract class WebHostIntegrationTestBase<TEntryPoint> : IocTestBase
    where TEntryPoint : class
{
    private WebApplicationFactory<TEntryPoint>? _factory;
    private AsyncServiceScope? _scope;

    /// <summary>
    /// Baut den ASP.NET-Core-Host einmal fuer die gesamte Testklasse auf.
    /// </summary>
    [OneTimeSetUp]
    public void BaseOneTimeSetUp()
    {
        try
        {
            var testConfiguration = BuildTestConfiguration();

            _factory = new WebApplicationFactory<TEntryPoint>().WithWebHostBuilder(builder =>
            {
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddConfiguration(testConfiguration);
                });

                builder.ConfigureTestServices(services =>
                {
                    ConfigureOverrides(services, testConfiguration);
                });
            });

            // Beruehrt den Host, damit ein fehlerhafter Bootstrap bereits hier und nicht erst beim ersten
            // Testzugriff auf Services sichtbar wird.
            _ = _factory.Services;
        }
        catch (Exception ex)
        {
            throw new IntegrationTestSetupException(
                $"Der Aufbau des Test-Hosts fuer '{typeof(TEntryPoint).FullName}' ist fehlgeschlagen. " +
                $"{TestContextDescription.Current()}.", ex);
        }
    }

    /// <summary>
    /// Erzeugt vor jedem Test einen neuen <see cref="AsyncServiceScope"/>. Bricht ab, wenn die Testmethode
    /// ein methodenspezifisches Service-Override anfordert (siehe REQ-06).
    /// </summary>
    [SetUp]
    public void BaseSetUp()
    {
        MethodLevelOverrideGuard.EnsureNoMethodLevelOverride(ServiceOverrideAttributes.CurrentTestMethod());

        _scope = _factory!.Services.CreateAsyncScope();
        Services = _scope.Value.ServiceProvider;
    }

    /// <summary>
    /// Gibt den Scope des aktuellen Tests frei.
    /// </summary>
    [TearDown]
    public async Task BaseTearDown()
    {
        if (_scope is not null)
        {
            await _scope.Value.DisposeAsync();
            _scope = null;
        }

        ClearServices();
    }

    /// <summary>
    /// Gibt den Test-Host nach Abschluss der Testklasse frei.
    /// </summary>
    [OneTimeTearDown]
    public async Task BaseOneTimeTearDown()
    {
        if (_factory is not null)
        {
            await _factory.DisposeAsync();
        }
    }
}
