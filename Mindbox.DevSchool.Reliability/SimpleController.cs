using Microsoft.AspNetCore.Mvc;

namespace Mindbox.DevSchool.Reliability;

[ApiController]
public class SimpleController : ControllerBase
{
	private readonly SimpleDbContext _dbContext;

	public SimpleController(SimpleDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	[HttpGet("weatherForecast/{id:guid}")]
	public WeatherForecast? GetById(Guid id)
	{
		return _dbContext.WeatherForecasts.Find(id);
	}
}