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
