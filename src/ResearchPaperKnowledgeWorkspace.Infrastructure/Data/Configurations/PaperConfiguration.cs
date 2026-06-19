using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public sealed class PaperConfiguration : EntityBaseConfiguration<Paper>
{
    public override void Configure(EntityTypeBuilder<Paper> builder)
    {
        base.Configure(builder);

        builder.ToTable(
            "Papers",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Papers_Rating",
                    "\"Rating\" BETWEEN 0 AND 5");

                tableBuilder.HasCheckConstraint(
                    "CK_Papers_Priority",
                    "\"Priority\" BETWEEN 0 AND 5");

                tableBuilder.HasCheckConstraint(
                    "CK_Papers_PublicationYear",
                    "\"PublicationYear\" IS NULL OR " +
                    "(\"PublicationYear\" BETWEEN 1000 AND 9999)");
            });

        builder.Property(paper => paper.Title)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(paper => paper.Subtitle)
            .HasMaxLength(1000);

        builder.Property(paper => paper.AbstractText)
            .HasColumnType("TEXT");

        builder.Property(paper => paper.JournalTitle)
            .HasMaxLength(500);

        builder.Property(paper => paper.ConferenceName)
            .HasMaxLength(500);

        builder.Property(paper => paper.Publisher)
            .HasMaxLength(500);

        builder.Property(paper => paper.Volume)
            .HasMaxLength(100);

        builder.Property(paper => paper.Issue)
            .HasMaxLength(100);

        builder.Property(paper => paper.PageRange)
            .HasMaxLength(100);

        builder.Property(paper => paper.Doi)
            .HasMaxLength(500);

        builder.Property(paper => paper.Isbn)
            .HasMaxLength(100);

        builder.Property(paper => paper.Issn)
            .HasMaxLength(100);

        builder.Property(paper => paper.Url)
            .HasMaxLength(4000);

        builder.Property(paper => paper.LanguageCode)
            .HasMaxLength(20);

        builder.Property(paper => paper.CitationKey)
            .HasMaxLength(300);

        builder.HasIndex(paper => paper.Title);

        builder.HasIndex(paper => paper.Doi);

        builder.HasIndex(paper => paper.CitationKey);

        builder.HasIndex(paper => paper.PublicationYear);

        builder.HasIndex(paper => paper.ReadingStatus);

        builder.HasIndex(paper => paper.IsFavorite);

        builder.HasIndex(paper => paper.IsArchived);
    }
}