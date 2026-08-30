namespace Commons.Testing.Ioc.WebHost.Tests.SampleApp;

public interface IGreeter
{
    string Greet();
}

public class Greeter : IGreeter
{
    public string Greet() => "Hallo";
}

public class FakeGreeter : IGreeter
{
    public string Greet() => "Fake";
}
