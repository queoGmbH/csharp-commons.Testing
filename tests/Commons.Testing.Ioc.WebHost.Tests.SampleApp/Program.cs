using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGreeter, Greeter>();

var app = builder.Build();

app.MapGet("/greet", (IGreeter greeter) => greeter.Greet());

app.Run();

/// <summary>
/// Minimaler Beispiel-Host, der in den Tests als <c>TEntryPoint</c> fuer
/// <c>WebHostIntegrationTestBase&lt;TEntryPoint&gt;</c> dient.
/// </summary>
public partial class Program
{
}
