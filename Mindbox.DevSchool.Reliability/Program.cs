namespace Mindbox.DevSchool.Reliability;

public class Program
{
	public static void Main(string[] args)
	{
		ThreadPool.SetMinThreads(2, 100);
		ThreadPool.SetMaxThreads(2, 200);

		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers();

		builder.Services.AddSingleton<WeatherForecastClient>();

		var app = builder.Build();

		app.MapControllers();

		app.Run();
	}
}