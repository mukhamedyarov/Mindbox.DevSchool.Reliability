using Microsoft.AspNetCore.Mvc;

using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Retry;

namespace Mindbox.DevSchool.Reliability;

[ApiController]
public class SimpleController : ControllerBase
{
	private readonly WeatherForecastRepository _forecastRepository;
	private readonly AsyncRetryPolicy _retryPolicy;

	public SimpleController(WeatherForecastRepository forecastRepository)
	{
		_forecastRepository = forecastRepository;
		_retryPolicy = Policy.Handle<TransientException>()
			.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromMilliseconds(100), 3, fastFirst: true));
	}

	[HttpGet("weatherForecast/{id:guid}")]
	public WeatherForecast? GetById(Guid id) => _forecastRepository.GetById(id);

	[HttpGet("async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdAsync(Guid id) => await _forecastRepository.GetByIAsync(id);

	[HttpGet("ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdAsync(Guid id, CancellationToken token) =>
		await _forecastRepository.GetByIAsync(id, token);

	[HttpGet("timeout/ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdTimeoutAsync(Guid id, CancellationToken token)
	{
		var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
		cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

		return await _forecastRepository.GetByIAsync(id, cancellationTokenSource.Token);
	}
	
	[HttpGet("retry/timeout/ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdRetryAsync(Guid id, CancellationToken token)
	{
		return await _retryPolicy.ExecuteAsync(async () =>
		{
			var cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
			cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(3));

			return await _forecastRepository.GetByIAsync(id, cancellationTokenSource.Token);
		});
	}
}