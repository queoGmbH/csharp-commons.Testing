namespace Commons.Testing.Ioc.WebHost.Tests;

public class CounterService
{
    public int Value { get; private set; }

    public void Increment() => Value++;
}

public class AsyncDisposableOnlyService : IAsyncDisposable
{
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
