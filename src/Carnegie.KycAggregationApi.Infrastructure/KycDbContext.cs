using Carnegie.KycAggregationApi.Domain;
using Microsoft.EntityFrameworkCore;

namespace Carnegie.KycAggregationApi.Infrastructure;

public class KycDbContext : DbContext
{
    public KycDbContext(DbContextOptions<KycDbContext> options) : base(options) { }

    public DbSet<AggregatedKycData> KycData => Set<AggregatedKycData>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AggregatedKycData>(entity =>
        {
            entity.HasKey(e => e.Ssn);
            entity.Property(e => e.Ssn).IsRequired();
            entity.Property(e => e.FirstName).IsRequired();
            entity.Property(e => e.LastName).IsRequired();
            entity.Property(e => e.Address).IsRequired();
        });
    }
}
