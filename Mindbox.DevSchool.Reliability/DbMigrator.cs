using Microsoft.EntityFrameworkCore;

namespace Mindbox.DevSchool.Reliability;

public class DbMigrator : IHostedService
{

	private readonly SimpleDbContext _dbContext;

	public DbMigrator(SimpleDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
		await _dbContext.Database.EnsureCreatedAsync(cancellationToken);
		await _dbContext.Database.MigrateAsync(cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}