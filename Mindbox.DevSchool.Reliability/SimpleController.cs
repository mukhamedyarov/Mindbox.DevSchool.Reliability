using Microsoft.AspNetCore.Mvc;

namespace Mindbox.DevSchool.Reliability;

[ApiController]
public class SimpleController : ControllerBase
{
	private readonly WeatherForecastClient _forecastClient;

	public SimpleController(WeatherForecastClient forecastClient)
	{
		_forecastClient = forecastClient;
	}

	[HttpGet("weatherForecast/{id:guid}")]
	public WeatherForecast GetById(Guid id) => _forecastClient.GetByIAsync(id).GetAwaiter().GetResult();
}