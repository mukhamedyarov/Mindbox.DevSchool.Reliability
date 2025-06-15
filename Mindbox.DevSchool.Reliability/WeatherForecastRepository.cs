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

	private static readonly SemaphoreSlim Semaphore = new(4, 4);

	private static volatile bool _brokenForever;

	private static volatile int _brokenRequests;

	private static DateTime _brokenTill = DateTime.MinValue;

	private static volatile int _requestsInProgress;

	private readonly ILogger<WeatherForecastRepository> _logger;

	public WeatherForecastRepository(ILogger<WeatherForecastRepository> logger)
	{
		_logger = logger;
	}

	public async Task<WeatherForecast> GetByIAsync(Guid id, CancellationToken? cancellationToken = null)
	{
		if (_brokenForever)
			throw new BrokenForeverException();

		if (_brokenTill >= DateTime.UtcNow)
		{
			var brokenCount = Interlocked.Increment(ref _brokenRequests);
			
			_logger.LogInformation($"Broken count: {brokenCount}");
			
			if (brokenCount > 100)
			{
				Interlocked.Exchange(ref _brokenForever, true);
				
				_logger.LogInformation($"Broken forever!");
				
				throw new BrokenForeverException();
			}

			throw new BrokenTemporaryException();
		}
		
		Interlocked.Exchange(ref _brokenRequests, 0);

		try
		{
			var requestsInProgress = Interlocked.Increment(ref _requestsInProgress);

			_logger.LogInformation($"Requests in progress: {requestsInProgress}");

			if (requestsInProgress > 200)
			{
				_brokenTill = DateTime.UtcNow.AddSeconds(30);
				_logger.LogInformation($"Broken till: {_brokenTill}");
				return await GetByIAsync(id, cancellationToken);
			}

			await Semaphore.WaitAsync(cancellationToken ?? CancellationToken.None);

			await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken ?? CancellationToken.None);

			var chance = Random.Shared.Next(100);
			if (chance <= 20)
				throw new TransientException();

			Semaphore.Release();

			return Forecasts.Single(x => x.Id == id);
		}
		catch
		{
			Semaphore.Release();
			throw;
		}
		finally
		{
			Interlocked.Decrement(ref _requestsInProgress);
		}
	}

	public WeatherForecast GetById(Guid id)
	{
		if (_brokenForever)
			throw new BrokenForeverException();

		Interlocked.Increment(ref _requestsInProgress);

		if (_requestsInProgress > 100)
			Interlocked.Exchange(ref _brokenForever, true);

		Semaphore.Wait();

		try
		{

			Task.Delay(TimeSpan.FromSeconds(2)).GetAwaiter().GetResult();
			return Forecasts.Single(x => x.Id == id);
		}
		finally
		{
			Interlocked.Decrement(ref _requestsInProgress);
			Semaphore.Release();
		}
	}
}