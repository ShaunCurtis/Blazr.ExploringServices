using Microsoft.Extensions.DependencyInjection;

namespace Blazr.ExploringServices;

public class BadService
{
    private readonly IServiceProvider _serviceProvider;

    public BadService(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public void DoSomething()
    {
        var service = _serviceProvider.GetRequiredService<DisposableScopedService>();
        // Do something with service
    }
}

public class GoodService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly DisposableScopedService? _scopedService;

    public GoodService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _scopedService = _serviceProvider.GetService<DisposableScopedService>();

        if (_scopedService is not null)
        {
            // Do some service registration
        }
    }

    public void DoSomething()
    {
        if (_scopedService is not null)
        {
            // Do something with service
        }
    }
}

public class MyService
{
    private readonly IServiceProvider _serviceProvider;

    public MyService(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public void DoSomething()
    {
        var transient = ActivatorUtilities.CreateInstance<DisposableTransientService>(_serviceProvider);

        if (transient is not null)
        {
            // Do something with service
        }
        transient?.Dispose();
    }
}

public class MyNonService
{
    private readonly DisposableScopedService _service;
    private Guid _id;

    public MyNonService(DisposableScopedService service, Guid id)
    {
        _service = service;
        _id = id;
    }
}

public class MyAService
{
    private readonly IServiceProvider _serviceProvider;

    public MyAService(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public void DoSomething(Guid id)
    {
        var myObject = ActivatorUtilities.CreateInstance<MyNonService>(_serviceProvider, new[] { id });

        // Do something with myObject
    }
}
