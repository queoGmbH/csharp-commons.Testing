# Commons.Testing.Ioc

Eine .NET-8-Bibliothek fuer NUnit-basierte Integrationstests auf Basis von
`Microsoft.Extensions.DependencyInjection`. Sie erlaubt es, in Tests denselben, produktionsnahen
Servicegraphen der Anwendung aufzubauen wie zur Laufzeit und dabei gezielt einzelne Abhaengigkeiten
(z. B. externe Dienste) durch Test-Doubles zu ersetzen.

## 1. Ueberblick

### Was die Bibliothek macht

- Baut fuer Testklassen einen echten `IServiceProvider` bzw. einen echten ASP.NET-Core-Host auf, statt
  eigene Mocking-Mechanismen bereitzustellen.
- Bietet in beiden Betriebsarten dieselbe Zugriffs-API (`Services`, `GetService<T>()`) sowie dieselben
  Erweiterungspunkte fuer Testkonfiguration (`BuildTestConfiguration()`) und Service-Overrides
  (`ConfigureOverrides(...)`).
- Erzeugt konsistente, deutschsprachige Fehlermeldungen, die immer mit Testklasse und (wo bekannt)
  Testmethode angereichert sind, sowie dedizierte Exception-Typen fuer Testaufbau-, Override- und
  Aufloesungsfehler.
- Ist mit `BuildServiceProvider(validateScopes: true)` kompatibel und haelt keinen gemeinsamen
  veraenderbaren statischen Zustand, sodass parallele NUnit-Ausfuehrung auf Fixture-Ebene moeglich ist.

Nicht Teil des Scopes sind ein eigener Mocking-Mechanismus, Logging-Assertion-Hilfen, automatische
Thread-Sicherheit der produktiven Anwendung sowie generische Testdaten- oder Fixture-Verwaltung.

### Wie die Bibliothek aufgebaut ist

Die Bibliothek besteht aus drei NuGet-Paketen; welche(s) Paket(e) ein Testprojekt benoetigt, richtet
sich danach, ob die getestete Anwendung ueber ASP.NET Core gehostet wird oder nicht:

| Paket | Enthaelt | Abhaengigkeiten |
|---|---|---|
| `Commons.Testing.Ioc.Abstractions` | Gemeinsame, ASP.NET-Core-freie Basis: `IocTestBase` (`Services`, `GetService<T>()`, `BuildTestConfiguration()`, `ConfigureOverrides(...)`), `ServiceOverrideAttribute`, dedizierte Exceptions | `Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Configuration(.Json)` |
| `Commons.Testing.Ioc` | Plain-Modus: `IocIntegrationTestBase`, `UseFakeAttribute` | `Microsoft.Extensions.DependencyInjection`, Abstractions |
| `Commons.Testing.Ioc.WebHost` | Web-Host-Modus: `WebHostIntegrationTestBase<TEntryPoint>` | `Microsoft.AspNetCore.Mvc.Testing`, Abstractions |

`Commons.Testing.Ioc` hat dadurch keine harte Abhaengigkeit auf ASP.NET Core; wer nur den Plain-Modus
braucht, referenziert nur dieses Paket (das Abstractions-Paket wird automatisch mitgezogen).

Beide Modi unterscheiden sich in der Lebensdauer des Servicegraphen:

- **Plain-Modus** (`IocIntegrationTestBase`): Vor **jedem** Test wird ein komplett neuer
  `ServiceProvider` aufgebaut und danach wieder freigegeben. Reihenfolge: produktiver Bootstrap ->
  klassenweite Overrides -> methodenspezifische Overrides.
- **Web-Host-Modus** (`WebHostIntegrationTestBase<TEntryPoint>`): Der ASP.NET-Core-Host wird einmal pro
  **Testklasse** aufgebaut (`OneTimeSetUp`/`OneTimeTearDown`); pro Test wird nur ein neuer, isolierter
  Scope erzeugt. Dadurch bleiben Singletons testklassenweit bestehen, was den Modus performanter, aber
  weniger isoliert macht. Methodenspezifische Overrides werden hier absichtlich **nicht** unterstuetzt
  (siehe Abschnitt 2.6).

## 2. Verwendung

Die folgenden Beispiele richten sich an Entwickler:innen, die eigene NUnit-Testklassen auf Basis
dieser Bibliothek schreiben.

### 2.1 Installation

```
dotnet add package Commons.Testing.Ioc          # Plain-Modus
dotnet add package Commons.Testing.Ioc.WebHost  # Web-Host-Modus (zusaetzlich, falls benoetigt)
```

### 2.2 Plain-Modus: Grundgeruest

Von `IocIntegrationTestBase` ableiten und `RegisterApplicationServices` mit der produktionsnahen
Registrierungslogik der Anwendung befuellen. Diese Funktion darf nicht `null` liefern; ein Verstoss
bricht den Testaufbau mit einer `IntegrationTestSetupException` ab.

```csharp
using Commons.Testing.Ioc;

public class OrderServiceTests : IocIntegrationTestBase
{
    protected override Action<IServiceCollection, IConfiguration> RegisterApplicationServices =>
        (services, configuration) =>
        {
            services.AddSingleton<IPaymentGateway, HttpPaymentGateway>();
            services.AddScoped<IOrderRepository, SqlOrderRepository>();
            services.AddScoped<IOrderService, OrderService>();
        };

    [Test]
    public void CreateOrder_Persists_The_Order()
    {
        var orderService = GetService<IOrderService>();

        var order = orderService.CreateOrder(new CreateOrderRequest(...));

        Assert.That(order.Id, Is.Not.EqualTo(Guid.Empty));
    }
}
```

Da vor jedem Test ein vollstaendig neuer Container aufgebaut wird, ist kein Test in der Lage, Zustand
in den naechsten Test zu uebertragen - unabhaengig von der gewaehlten Lifetime der Services.

### 2.3 Web-Host-Modus: Grundgeruest

Von `WebHostIntegrationTestBase<TEntryPoint>` ableiten, wobei `TEntryPoint` der `Program`-Typ der zu
testenden ASP.NET-Core-Anwendung ist. Die produktive Registrierung wird automatisch ueber den echten
Host-Bootstrap ausgefuehrt, nicht ein zweites Mal manuell.

```csharp
using Commons.Testing.Ioc.WebHost;

public class OrderApiTests : WebHostIntegrationTestBase<Program>
{
    [Test]
    public void GetService_Resolves_The_Production_Registration_From_The_Real_Host()
    {
        var orderService = GetService<IOrderService>();

        Assert.That(orderService, Is.Not.Null);
    }
}
```

`TEntryPoint` (in der Regel `Program`) muss fuer das Testprojekt sichtbar sein. Bei Minimal-APIs reicht
dafuer meist eine Zeile am Ende der `Program.cs` der getesteten Anwendung:

```csharp
public partial class Program
{
}
```

### 2.4 Test-Konfiguration ueberschreiben (`BuildTestConfiguration`)

Ohne Override wird eine minimale Konfiguration erzeugt, die optional eine Datei
`appsettings.integrationtests.json` beruecksichtigt (sofern vorhanden). Um zusaetzliche Werte
einzubringen, `BuildTestConfiguration()` ueberschreiben - im Web-Host-Modus fliesst dieselbe
Konfiguration automatisch auch in den Test-Host ein:

```csharp
protected override IConfiguration BuildTestConfiguration()
{
    return new ConfigurationBuilder()
        .AddJsonFile("appsettings.integrationtests.json", optional: true)
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PaymentGateway:BaseUrl"] = "https://payment.invalid",
        })
        .Build();
}
```

### 2.5 Klassenweite Service-Overrides (`ConfigureOverrides`)

Der Hook `ConfigureOverrides(...)` steht in **beiden** Modi zur Verfuegung und wird nach dem
produktiven Bootstrap ausgefuehrt. Ein Override fuer einen Service-Typ soll alle bisherigen
Registrierungen dieses Typs ersetzen - dafuer `RemoveAll` gefolgt von `Add`/`AddSingleton` verwenden,
**nicht** `Replace(...)` (dieses entfernt nur die erste passende Registrierung):

```csharp
protected override void ConfigureOverrides(IServiceCollection services, IConfiguration configuration)
{
    services.RemoveAll<IPaymentGateway>();
    services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
}
```

Fuer Web-Host-Tests mit voneinander abweichenden Overrides werden separate Testklassen (oder ein
eigener Test-Host) verwendet - der Host samt seiner Singletons wird ja nur einmal pro Testklasse
aufgebaut.

### 2.6 Methodenspezifische Overrides mit `[UseFake]` (nur Plain-Modus)

Im Plain-Modus koennen einzelne Testmethoden zusaetzlich zu klassenweiten Overrides einen eigenen,
nur fuer diesen Test geltenden Fake anfordern:

```csharp
[Test]
[UseFake(typeof(IPaymentGateway), typeof(FailingPaymentGateway))]
public void CreateOrder_When_Payment_Fails_Does_Not_Persist_The_Order()
{
    var orderService = GetService<IOrderService>();

    Assert.ThrowsAsync<PaymentFailedException>(() => orderService.CreateOrderAsync(...));
}
```

Ohne explizite Angabe uebernimmt der Fake die Lifetime der ersetzten Produktivregistrierung (oder
`Singleton`, falls keine existiert); eine abweichende Lifetime kann optional angegeben werden:

```csharp
[UseFake(typeof(IPaymentGateway), typeof(FakePaymentGateway), ServiceLifetime.Scoped)]
```

Zwei `[UseFake]`-Attribute fuer denselben Service-Typ auf derselben Testmethode sind ein
Konfigurationsfehler und werfen vor Testausfuehrung eine `InvalidOverrideConfigurationException`.

**Im Web-Host-Modus wird `[UseFake]` nicht unterstuetzt**, da der Host testklassenweit aufgebaut wird
und ein methodenspezifischer Fake nur durch einen Host-Neuaufbau pro Test korrekt isolierbar waere.
Ein entsprechender Versuch bricht den Testaufbau mit einer `InvalidOverrideConfigurationException` ab,
die auf eine eigene Testklasse oder einen separaten Test-Host verweist.

### 2.7 Fehlerbehandlung

Alle von der Bibliothek geworfenen Exceptions leiten von `IntegrationTestException` ab und enthalten
Testklasse (und, wo bekannt, Testmethode) in der Fehlermeldung:

| Exception | Wird geworfen, wenn ... |
|---|---|
| `IntegrationTestSetupException` | der Testaufbau fehlschlaegt (Container-/Host-Aufbau, fehlende/fehlerhafte `RegisterApplicationServices`). Die urspruengliche Exception bleibt als `InnerException` erhalten; ist der ausloesende Service-Typ aus der zugrunde liegenden Exception ermittelbar, wird er zusaetzlich genannt. |
| `InvalidOverrideConfigurationException` | eine Override-Konfiguration ungueltig ist (doppeltes `[UseFake]` fuer denselben Service-Typ, `[UseFake]` im Web-Host-Modus). |
| `ServiceResolutionException` | `GetService<T>()` fehlschlaegt. `Reason` unterscheidet `MissingRegistration` (kein Service dieses Typs registriert) von `ResolutionFailed` (Registrierung vorhanden, aber Konstruktor wirft oder `validateScopes` deckt eine Captive Dependency auf); die urspruengliche Exception bleibt im letzteren Fall als `InnerException` erhalten. |

```csharp
var exception = Assert.Throws<ServiceResolutionException>(new Action(() => GetService<IPaymentGateway>()));

if (exception.Reason == ServiceResolutionFailureReason.MissingRegistration)
{
    // IPaymentGateway wurde in RegisterApplicationServices/ConfigureOverrides nicht registriert.
}
```

> Hinweis: In NUnit 4.6.1 ist die `Assert.Throws<T>(...)`-Ueberladung fuer einen einfachen Lambda-Ausdruck
> mehrdeutig; ein explizites `new Action(...)` wie oben loest das auf.

### 2.8 Parallele Testausfuehrung

Die Bibliothek haelt keinen gemeinsamen veraenderbaren statischen Zustand und ist daher mit paralleler
NUnit-Ausfuehrung auf Fixture-Ebene kompatibel (`[assembly: Parallelizable(ParallelScope.Fixtures)]`).
Die Thread-Sicherheit der eigenen, in `RegisterApplicationServices` registrierten produktiven
Registrierungslogik und produktiven Singletons liegt in der Verantwortung der jeweiligen Anwendung.

### 2.9 Weitere Hinweise

- `Services`/`GetService<T>()` stehen erst nach erfolgreichem Testaufbau zur Verfuegung; ein Zugriff
  davor oder nach dem Teardown wirft eine klare `InvalidOperationException`.
- Die Bibliothek ist standardmaessig mit `BuildServiceProvider(validateScopes: true)` kompatibel -
  Captive Dependencies (z. B. ein Singleton, der von einem Scoped-Service abhaengt) werden dadurch wie
  in der produktiven Anwendung aufgedeckt.
- Mehrfachregistrierungen desselben Service-Typs gelten im Override-Fall nicht als Sonderfall mit
  impliziter Merge-Logik: Overrides ersetzen deterministisch alle bisherigen Registrierungen.
