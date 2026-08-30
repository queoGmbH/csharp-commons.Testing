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

public interface IUnregisteredService
{
}

public interface IThrowingService
{
}

public class ThrowingService : IThrowingService
{
    public ThrowingService()
    {
        throw new InvalidOperationException("Konstruktor schlaegt absichtlich fehl.");
    }
}

public interface IScopedGreeter : IGreeter
{
}

public class ScopedGreeter : IScopedGreeter
{
    public string Greet() => "Scoped";
}

public class CounterService
{
    public int Value { get; private set; }

    public void Increment() => Value++;
}
