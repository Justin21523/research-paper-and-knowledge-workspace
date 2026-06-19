using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public sealed class TagConfiguration : EntityBaseConfiguration<Tag>
{
    public override void Configure(EntityTypeBuilder<Tag> builder)
    {
        base.Configure(builder);

        builder.ToTable("Tags");

        builder.Property(tag => tag.Name)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(tag => tag.NormalizedName)
            .HasMaxLength(300);

        builder.Property(tag => tag.Description)
            .HasMaxLength(2000);

        builder.Property(tag => tag.ColorHex)
            .HasMaxLength(20);

        builder.HasIndex(tag => tag.Name);

        builder.HasIndex(tag => tag.NormalizedName)
            .IsUnique();

        builder.HasIndex(tag => tag.ParentTagId);

        builder.HasOne(tag => tag.ParentTag)
            .WithMany(tag => tag.ChildTags)
            .HasForeignKey(tag => tag.ParentTagId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class PaperTagConfiguration
    : EntityBaseConfiguration<PaperTag>
{
    public override void Configure(EntityTypeBuilder<PaperTag> builder)
    {
        base.Configure(builder);

        builder.ToTable(
            "PaperTags",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PaperTags_Confidence",
                    "\"Confidence\" BETWEEN 0.0 AND 1.0");
            });

        builder.HasIndex(
                paperTag => new
                {
                    paperTag.PaperId,
                    paperTag.TagId
                })
            .IsUnique();

        builder.HasIndex(paperTag => paperTag.AssignmentSource);

        builder.HasOne(paperTag => paperTag.Paper)
            .WithMany(paper => paper.PaperTags)
            .HasForeignKey(paperTag => paperTag.PaperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(paperTag => paperTag.Tag)
            .WithMany(tag => tag.PaperTags)
            .HasForeignKey(paperTag => paperTag.TagId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}