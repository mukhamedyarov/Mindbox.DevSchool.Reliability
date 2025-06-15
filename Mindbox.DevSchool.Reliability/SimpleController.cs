using EasyCaching.Abstractions;

using Microsoft.AspNetCore.Mvc;

using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;
using Polly.Wrap;

namespace Mindbox.DevSchool.Reliability;

[ApiController]
public class SimpleController : ControllerBase
{
	private static readonly AsyncRetryPolicy RetryPolicy = Policy.Handle<TransientException>()
		.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), 3, fastFirst: true));
	private static readonly AsyncPolicyWrap RetryAndCircuitBreakerPolicy = Policy.WrapAsync(RetryPolicy,
		Policy.Handle<BrokenTemporaryException>()
			.CircuitBreakerAsync(3, TimeSpan.FromSeconds(35)));

	private readonly ICache<WeatherForecast> _forecastCache;


	private readonly WeatherForecastRepository _forecastRepository;

	public SimpleController(WeatherForecastRepository forecastRepository, ICache<WeatherForecast> forecastCache)
	{
		_forecastRepository = forecastRepository;
		_forecastCache = forecastCache;
	}

	[HttpGet("weatherForecast/{id:guid}")]
	public WeatherForecast? GetById(Guid id) => _forecastRepository.GetById(id);

	[HttpGet("async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdAsync(Guid id) => await _forecastRepository.GetByIAsync(id);

	[HttpGet("ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdAsync(Guid id, CancellationToken token) =>
		await _forecastRepository.GetByIAsync(id, token);

	[HttpGet("cache/cb/retry/timeout/ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdCacheAsync(Guid id, CancellationToken token)
	{
		return await _forecastCache.GetOrAddAsync(id.ToString("D"), async () =>
		{
			return await RetryAndCircuitBreakerPolicy.ExecuteAsync(async () =>
			{
				var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
				cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

				return await _forecastRepository.GetByIAsync(id, cancellationTokenSource.Token);
			});
		});
	}

	[HttpGet("cb/retry/timeout/ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdCbAsync(Guid id, CancellationToken token)
	{
		return await RetryAndCircuitBreakerPolicy.ExecuteAsync(async () =>
		{
			var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
			cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

			return await _forecastRepository.GetByIAsync(id, cancellationTokenSource.Token);
		});
	}

	[HttpGet("retry/timeout/ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdRetryAsync(Guid id, CancellationToken token)
	{
		return await RetryPolicy.ExecuteAsync(async () =>
		{
			var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
			cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

			return await _forecastRepository.GetByIAsync(id, cancellationTokenSource.Token);
		});
	}

	[HttpGet("timeout/ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdTimeoutAsync(Guid id, CancellationToken token)
	{
		var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
		cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

		return await _forecastRepository.GetByIAsync(id, cancellationTokenSource.Token);
	}
}