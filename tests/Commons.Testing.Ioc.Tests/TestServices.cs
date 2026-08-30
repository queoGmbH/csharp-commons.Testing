namespace Commons.Testing.Ioc.Tests;

public interface IGreeter
{
    string Greet();
}

public class Greeter : IGreeter
{
    public string Greet() => "Hallo";
}

public class SecondGreeter : IGreeter
{
    public string Greet() => "Hallo (zweite Registrierung)";
}

public class FakeGreeter : IGreeter
{
    public string Greet() => "Fake";
}

public class SecondFakeGreeter : IGreeter
{
    public string Greet() => "Fake (zweite Registrierung)";
}

public class AsyncDisposableOnlyGreeter : IGreeter, IAsyncDisposable
{
    public string Greet() => "AsyncDisposable";

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
