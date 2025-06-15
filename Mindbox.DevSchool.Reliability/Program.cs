using EasyCaching.Advanced.Template;
using EasyCaching.Core;
using EasyCaching.Core.Serialization;
using EasyCaching.InMemory;

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

		builder.Services.AddSingleton<IEasyCachingSerializer, JsonCachingSerializer>();
		builder.Services.AddEasyCaching((Action<EasyCachingOptions>) (options =>
		{
			options.UseInMemory("InMemory");
		}));
		builder.Services.AddCache<WeatherForecast>("InMemory", TimeSpan.FromHours(1));

		var app = builder.Build();

		app.MapControllers();

		app.Run();
	}
}