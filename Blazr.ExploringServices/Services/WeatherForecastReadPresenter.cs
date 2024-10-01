using Microsoft.AspNetCore.Components.QuickGrid;
using Microsoft.Extensions.DependencyInjection;

namespace Blazr.ExploringServices;

public class WeatherForecastReadPresenter
{
    private readonly IServiceProvider _serviceProvider;
    private readonly WeatherForecastService _weatherForecastService;

    public WeatherForecastReadPresenter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _weatherForecastService = _serviceProvider.GetRequiredService<WeatherForecastService>();
    }

    public async ValueTask<GridItemsProviderResult<WeatherForecast>> GetItemsAsync(GridItemsProviderRequest<WeatherForecast> request)
    {
        var items = await _weatherForecastService.GetForecastAsync();
        var count = items.Count();
        var pagedItems = items.Skip(request.StartIndex).Take(request.Count ?? 15).ToList();
        return new GridItemsProviderResult<WeatherForecast>() { Items=pagedItems, TotalItemCount=count };
    }
}


