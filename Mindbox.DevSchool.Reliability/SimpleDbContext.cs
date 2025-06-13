using Microsoft.EntityFrameworkCore;

namespace Mindbox.DevSchool.Reliability;

public class SimpleDbContext : DbContext
{
	public DbSet<WeatherForecast> WeatherForecasts => Set<WeatherForecast>();

	public SimpleDbContext(DbContextOptions<SimpleDbContext> options) : base(options)
	{
		base.Database.EnsureCreated();
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<WeatherForecast>()
			.HasKey(x => x.Id);

		modelBuilder.Entity<WeatherForecast>()
			.HasData(new WeatherForecast
			{
				Id = Guid.Parse("c0f4ac08-eafc-4fdb-91f8-fb39dda1d216"),
				Date = new DateOnly(2021, 07, 01),
				TemperatureC = 27,
				Summary = null
			});
	}
}