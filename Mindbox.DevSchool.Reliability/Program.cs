using Microsoft.EntityFrameworkCore;

namespace Mindbox.DevSchool.Reliability;

public class Program
{
	public static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddControllers();

		builder.Services.AddDbContext<SimpleDbContext>(option =>
		{
			option.UseNpgsql(builder.Configuration["ConnectionStrings:DefaultConnection"]);
		});

		var app = builder.Build();

		app.Run();
	}
}