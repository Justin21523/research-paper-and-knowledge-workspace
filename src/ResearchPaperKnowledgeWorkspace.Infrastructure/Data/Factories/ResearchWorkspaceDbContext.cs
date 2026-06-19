using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using ResearchPaperKnowledgeWorkspace.Core.Common;
using ResearchPaperKnowledgeWorkspace.Core.Entities;
using ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Converters;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data;

public sealed class ResearchWorkspaceDbContext : DbContext
{
    public ResearchWorkspaceDbContext(
        DbContextOptions<ResearchWorkspaceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Paper> Papers => Set<Paper>();

    public DbSet<Author> Authors => Set<Author>();

    public DbSet<PaperAuthor> PaperAuthors => Set<PaperAuthor>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<PaperTag> PaperTags => Set<PaperTag>();

    public DbSet<ResearchProject> ResearchProjects =>
        Set<ResearchProject>();

    public DbSet<ProjectPaper> ProjectPapers =>
        Set<ProjectPaper>();

    public DbSet<Note> Notes => Set<Note>();

    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<ImportJob> ImportJobs =>
        Set<ImportJob>();
        
    public DbSet<PaperRelation> PaperRelations =>
        Set<PaperRelation>();

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<DateTimeOffset>()
            .HaveConversion<DateTimeOffsetToUnixMillisecondsConverter>()
            .HaveColumnType("INTEGER");
    }

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ResearchWorkspaceDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();

        return base.SaveChanges();
    }

    public override int SaveChanges(
        bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInformation();

        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();

        return base.SaveChangesAsync(
            acceptAllChangesOnSuccess,
            cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var now = DateTimeOffset.UtcNow;

        foreach (var entry in ChangeTracker.Entries<EntityBase>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity.Id == Guid.Empty)
                    {
                        entry.Entity.Id = Guid.NewGuid();
                    }

                    entry.Entity.CreatedAtUtc = now;
                    entry.Entity.UpdatedAtUtc = now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = now;

                    entry.Property(entity => entity.CreatedAtUtc)
                        .IsModified = false;
                    break;
            }
        }
    }
}