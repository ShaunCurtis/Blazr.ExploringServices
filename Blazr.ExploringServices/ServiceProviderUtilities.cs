namespace Blazr.ExploringServices;

public record ServiceProviderUtilities
{
    private readonly IServiceProvider _serviceProvider;

    private ServiceProviderUtilities(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    private static ServiceProviderUtilities? instance;

    public static ServiceProviderUtilities GetInstance()
    {
        ArgumentNullException.ThrowIfNull(instance);
        return instance;
    }

    public static void SetInstance(IServiceProvider serviceProvider)
    {
        if (instance != null)
            throw new Exception("An instance already exists");

        instance = new ServiceProviderUtilities(serviceProvider);
    }
}
