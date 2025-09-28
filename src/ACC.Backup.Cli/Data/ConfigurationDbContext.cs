using Microsoft.EntityFrameworkCore;

namespace ACC.Backup.Cli.Data;

/// <summary>
/// The database context for storing configuration data such as credentials and jobs.
/// </summary>
/// <param name="options"></param>
public sealed class ConfigurationDbContext(DbContextOptions<ConfigurationDbContext> options) : DbContext(options)
{
	public DbSet<Credential> Credentials { get; set; }
	public DbSet<Job> Jobs { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<Credential>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Id)
				.IsRequired();
			entity.Property(e => e.Secret)
				.IsRequired();
		});

		modelBuilder.Entity<Job>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Id)
				.IsRequired();
			entity.Property(e => e.IncludeIds)
				.IsRequired();
			entity.Property(e => e.ExcludeIds)
				.IsRequired();
		});
	}
}
