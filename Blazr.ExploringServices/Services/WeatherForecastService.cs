namespace Blazr.ExploringServices;

public class WeatherForecastService : IDisposable
{
    private IEnumerable<WeatherForecast>? _weatherForecasts;
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public async Task<IEnumerable<WeatherForecast>> GetForecastAsync(DateOnly startDate)
    {
        // Fake async behaviour
        await Task.Yield();

        if (_weatherForecasts is null)
            _weatherForecasts = Enumerable.Range(1, 50).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToList();

        return _weatherForecasts;
    }

    public void Dispose()
    {
        Console.WriteLine("Weather Forecast Service Disposed");
    }
}

public class WeatherForecast
{
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }
}
