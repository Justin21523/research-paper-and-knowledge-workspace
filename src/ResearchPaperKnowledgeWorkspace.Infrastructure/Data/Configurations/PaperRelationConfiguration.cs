using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public sealed class PaperRelationConfiguration
    : EntityBaseConfiguration<PaperRelation>
{
    public override void Configure(
        EntityTypeBuilder<PaperRelation> builder)
    {
        base.Configure(builder);

        builder.ToTable(
            "PaperRelations",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PaperRelations_Confidence",
                    "\"Confidence\" BETWEEN 0.0 AND 1.0");

                tableBuilder.HasCheckConstraint(
                    "CK_PaperRelations_DifferentPapers",
                    "\"SourcePaperId\" <> \"TargetPaperId\"");
            });

        builder.Property(relation => relation.Description)
            .HasMaxLength(2000);

        builder.HasIndex(
                relation => new
                {
                    relation.SourcePaperId,
                    relation.TargetPaperId,
                    relation.RelationType
                })
            .IsUnique();

        builder.HasIndex(relation => relation.SourcePaperId);

        builder.HasIndex(relation => relation.TargetPaperId);

        builder.HasIndex(relation => relation.RelationType);

        builder.HasOne(relation => relation.SourcePaper)
            .WithMany(paper => paper.OutgoingRelations)
            .HasForeignKey(relation => relation.SourcePaperId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(relation => relation.TargetPaper)
            .WithMany(paper => paper.IncomingRelations)
            .HasForeignKey(relation => relation.TargetPaperId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}