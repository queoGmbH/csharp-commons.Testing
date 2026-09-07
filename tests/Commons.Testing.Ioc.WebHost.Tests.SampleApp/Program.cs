using Commons.Testing.Ioc.WebHost.Tests.SampleApp;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IGreeter, Greeter>();

var app = builder.Build();

app.MapGet("/greet", (IGreeter greeter) => greeter.Greet());

app.Run();

/// <summary>
/// Minimal example host that serves in the tests as <c>TEntryPoint</c> for
/// <c>WebHostIntegrationTestBase&lt;TEntryPoint&gt;</c>.
/// </summary>
public partial class Program
{
}
