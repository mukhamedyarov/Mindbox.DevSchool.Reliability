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
		},
		new()
		{
			Id = Guid.Parse("80ff7a2f-f64a-4079-b8e1-86dd661f1ec2"),
			Date = new DateOnly(2021, 07, 02),
			TemperatureC = 28,
			Summary = null
		},
		new()
		{
			Id = Guid.Parse("72bb2fe6-1753-4afa-bf66-342f9c5fb903"),
			Date = new DateOnly(2021, 07, 03),
			TemperatureC = 29,
			Summary = null
		},
		new()
		{
			Id = Guid.Parse("0dc178a2-6d2c-430d-8f4f-9d80811d5331"),
			Date = new DateOnly(2021, 07, 04),
			TemperatureC = 30,
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

	public async Task<IReadOnlyCollection<WeatherForecast>> GetByIdsAsync(IEnumerable<Guid> ids)
	{
		await Task.Delay(TimeSpan.FromSeconds(2));
		return ids.Chunk(100).SelectMany(chunk => Forecasts.Where(x => chunk.Contains(x.Id))).ToArray();
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
}