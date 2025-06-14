namespace Mindbox.DevSchool.Reliability;

public class Program
{
	public static void Main(string[] args)
	{
		// ThreadPool.SetMinThreads(workerThreads: 2, completionPortThreads: 100);
		// ThreadPool.SetMaxThreads(workerThreads: 2, completionPortThreads: 200);
		
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers();

		builder.Services.AddSingleton<WeatherForecastRepository>();

		var app = builder.Build();

		app.MapControllers();

		app.Run();
	}
}