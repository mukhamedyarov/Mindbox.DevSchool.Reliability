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
	
	private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(4, 4); 

	public async Task<WeatherForecast> GetByIAsync(Guid id)
	{
		await _semaphore.WaitAsync();

		try
		{
			await Task.Delay(TimeSpan.FromSeconds(2));
			return Forecasts.Single(x => x.Id == id);
		}
		finally
		{
			_semaphore.Release();
		}
	}

	public async Task<WeatherForecast> GetByIAsync(Guid id, CancellationToken token)
	{
		await _semaphore.WaitAsync(token);

		try
		{
			await Task.Delay(TimeSpan.FromSeconds(2), token);
			return Forecasts.Single(x => x.Id == id);
		}
		finally
		{
			_semaphore.Release();
		}

	}

	public WeatherForecast GetById(Guid id)
	{
		_semaphore.Wait();

		try
		{

			Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
			return Forecasts.Single(x => x.Id == id);
		}
		finally
		{
			_semaphore.Release();
		}
	}
}