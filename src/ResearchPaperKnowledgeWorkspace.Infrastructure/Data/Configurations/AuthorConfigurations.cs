using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public sealed class AuthorConfiguration : EntityBaseConfiguration<Author>
{
    public override void Configure(EntityTypeBuilder<Author> builder)
    {
        base.Configure(builder);

        builder.ToTable("Authors");

        builder.Property(author => author.FullName)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(author => author.GivenName)
            .HasMaxLength(300);

        builder.Property(author => author.FamilyName)
            .HasMaxLength(300);

        builder.Property(author => author.SortName)
            .HasMaxLength(500);

        builder.Property(author => author.Orcid)
            .HasMaxLength(100);

        builder.Property(author => author.Affiliation)
            .HasMaxLength(1000);

        builder.Property(author => author.Biography)
            .HasColumnType("TEXT");

        builder.HasIndex(author => author.FullName);

        builder.HasIndex(author => author.SortName);

        builder.HasIndex(author => author.Orcid);
    }
}

public sealed class PaperAuthorConfiguration
    : EntityBaseConfiguration<PaperAuthor>
{
    public override void Configure(EntityTypeBuilder<PaperAuthor> builder)
    {
        base.Configure(builder);

        builder.ToTable(
            "PaperAuthors",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_PaperAuthors_AuthorOrder",
                    "\"AuthorOrder\" >= 1");
            });

        builder.Property(paperAuthor => paperAuthor.ContributionRole)
            .HasMaxLength(300);

        builder.HasIndex(
                paperAuthor => new
                {
                    paperAuthor.PaperId,
                    paperAuthor.AuthorId
                })
            .IsUnique();

        builder.HasIndex(
            paperAuthor => new
            {
                paperAuthor.PaperId,
                paperAuthor.AuthorOrder
            });

        builder.HasOne(paperAuthor => paperAuthor.Paper)
            .WithMany(paper => paper.PaperAuthors)
            .HasForeignKey(paperAuthor => paperAuthor.PaperId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(paperAuthor => paperAuthor.Author)
            .WithMany(author => author.PaperAuthors)
            .HasForeignKey(paperAuthor => paperAuthor.AuthorId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}