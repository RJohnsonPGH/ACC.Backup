using Microsoft.EntityFrameworkCore;

namespace ACC.Backup.Core.Data;

public sealed class RepositoryDbContext(DbContextOptions<RepositoryDbContext> options) : DbContext(options)
{
	public DbSet<RepositoryItem> Items { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		modelBuilder.Entity<RepositoryItem>(entity =>
		{
			entity.HasKey(e => e.Id);
			entity.Property(e => e.Id)
				.IsRequired();
			entity.Property(e => e.LatestVersion)
				.IsRequired();
		});
	}
}
