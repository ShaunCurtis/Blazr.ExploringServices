namespace Blazr.ExploringServices;

public class DisposableSingletonService : IDisposable
{
    public readonly Guid ServiceId = Guid.NewGuid();
    public DisposableSingletonService()
        => Console.WriteLine($"Disposable Singleton Service Created => {ServiceId.ToString().Substring(0,4)}.");

    public void Dispose()
        => Console.WriteLine($"Disposable Singleton Service Disposed => {ServiceId.ToString().Substring(0,4)}.");
}

public class DisposableScopedService : IDisposable
{
    public readonly Guid ServiceId = Guid.NewGuid();
    public DisposableScopedService()
        => Console.WriteLine($"Disposable Scoped Service Created => {ServiceId.ToString().Substring(0, 4)}.");
    public void Dispose()
        => Console.WriteLine($"Disposable Scoped Service Disposed => {ServiceId.ToString().Substring(0, 4)}.");
}

public class TransientService { }

public class DisposableTransientService : IDisposable
{
    public DisposableTransientService()
        => Console.WriteLine($"Disposable Transient Service Created => {ServiceId.ToString().Substring(0, 4)}.");
    public readonly Guid ServiceId = Guid.NewGuid();
    public void Dispose()
        => Console.WriteLine($"Disposable Transient Service Disposed => {ServiceId.ToString().Substring(0, 4)}.");
}
