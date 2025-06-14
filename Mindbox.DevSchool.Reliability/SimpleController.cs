using Microsoft.AspNetCore.Mvc;

namespace Mindbox.DevSchool.Reliability;

[ApiController]
public class SimpleController : ControllerBase
{
	private readonly WeatherForecastRepository _forecastRepository;

	public SimpleController(WeatherForecastRepository forecastRepository)
	{
		_forecastRepository = forecastRepository;
	}

	[HttpGet("weatherForecast/{id:guid}")]
	public WeatherForecast? GetById(Guid id) => _forecastRepository.GetById(id);

	[HttpGet("async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdAsync(Guid id) => await _forecastRepository.GetByIAsync(id);

	[HttpGet("ct/async/weatherForecast/{id:guid}")]
	public async Task<WeatherForecast?> GetByIdAsync(Guid id, CancellationToken token) =>
		await _forecastRepository.GetByIAsync(id, token);
}