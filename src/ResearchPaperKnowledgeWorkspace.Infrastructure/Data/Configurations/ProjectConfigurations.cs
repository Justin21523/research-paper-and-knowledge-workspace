using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public sealed class ResearchProjectConfiguration
    : EntityBaseConfiguration<ResearchProject>
{
    public override void Configure(
        EntityTypeBuilder<ResearchProject> builder)
    {
        base.Configure(builder);

        builder.ToTable("ResearchProjects");

        builder.Property(project => project.Name)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(project => project.Description)
            .HasColumnType("TEXT");

        builder.Property(project => project.ColorHex)
            .HasMaxLength(20);

        builder.HasIndex(project => project.Name);

        builder.HasIndex(project => project.Status);

        builder.HasIndex(project => project.IsArchived);
    }
}

public sealed class ProjectPaperConfiguration
    : EntityBaseConfiguration<ProjectPaper>
{
    public override void Configure(
        EntityTypeBuilder<ProjectPaper> builder)
    {
        base.Configure(builder);

        builder.ToTable(
            "ProjectPapers",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_ProjectPapers_SortOrder",
                    "\"SortOrder\" >= 0");
            });

        builder.Property(projectPaper => projectPaper.ProjectSpecificNote)
            .HasColumnType("TEXT");

        builder.HasIndex(
                projectPaper => new
                {
                    projectPaper.ResearchProjectId,
                    projectPaper.PaperId
                })
            .IsUnique();

        builder.HasIndex(
            projectPaper => new
            {
                projectPaper.ResearchProjectId,
                projectPaper.SortOrder
            });

        builder.HasOne(projectPaper => projectPaper.ResearchProject)
            .WithMany(project => project.ProjectPapers)
            .HasForeignKey(projectPaper => projectPaper.ResearchProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(projectPaper => projectPaper.Paper)
            .WithMany(paper => paper.ProjectPapers)
            .HasForeignKey(projectPaper => projectPaper.PaperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}