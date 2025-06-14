namespace Mindbox.DevSchool.Reliability;

public class WeatherForecastRepository
{
	private static readonly IReadOnlyCollection<WeatherForecast> Forecasts =
	[
		new()
		{
			Id = Guid.Parse("c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"),
			Date = new DateOnly(2021, 07, 01),
			TemperatureC = 27,
			Summary = null
		}
	];

	public async Task<WeatherForecast> GetByIAsync(Guid id)
	{
		await Task.Delay(TimeSpan.FromSeconds(2));
		return Forecasts.Single(x => x.Id == id);
	}

	public WeatherForecast GetById(Guid id)
	{
		Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
		return Forecasts.Single(x => x.Id == id);
	}
}