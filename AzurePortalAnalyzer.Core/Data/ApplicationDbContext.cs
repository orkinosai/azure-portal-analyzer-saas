using AzurePortalAnalyzer.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AzurePortalAnalyzer.Core.Data;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<AzureAnalysisRequest> AnalysisRequests => Set<AzureAnalysisRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AzureAnalysisRequest>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserEmail).IsRequired().HasMaxLength(255);
            entity.Property(e => e.SubscriptionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ResourceGroup).HasMaxLength(255);
            entity.Property(e => e.DocumentUrl).HasMaxLength(2048);
            entity.Property(e => e.ErrorMessage).HasMaxLength(4000);
            
            entity.Property(e => e.ResourceTypes)
                .HasConversion(
                    v => string.Join(',', v),
                    v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
        });
    }
}