using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AzurePortalAnalyzer.Api.Models;
using System.Text.Json;

namespace AzurePortalAnalyzer.Api.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<PortalAnalysis> PortalAnalyses { get; set; }
    public DbSet<PortalComponent> PortalComponents { get; set; }
    public DbSet<RequirementDocument> RequirementDocuments { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configure PortalComponent properties as JSON
        builder.Entity<PortalComponent>()
            .Property(e => e.Properties)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>()
            );

        // Configure PortalComponent requirements as JSON
        builder.Entity<PortalComponent>()
            .Property(e => e.Requirements)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions?)null) ?? new List<string>()
            );

        // Configure relationships
        builder.Entity<PortalComponent>()
            .HasOne(pc => pc.PortalAnalysis)
            .WithMany(pa => pa.Components)
            .HasForeignKey(pc => pc.PortalAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<RequirementDocument>()
            .HasOne(rd => rd.PortalAnalysis)
            .WithMany(pa => pa.Documents)
            .HasForeignKey(rd => rd.PortalAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}