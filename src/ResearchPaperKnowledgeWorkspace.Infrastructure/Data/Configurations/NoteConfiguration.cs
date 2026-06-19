using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public sealed class NoteConfiguration : EntityBaseConfiguration<Note>
{
    public override void Configure(EntityTypeBuilder<Note> builder)
    {
        base.Configure(builder);

        builder.ToTable("Notes");

        builder.Property(note => note.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(note => note.ContentMarkdown)
            .IsRequired()
            .HasColumnType("TEXT");

        builder.HasIndex(note => note.PaperId);

        builder.HasIndex(note => note.NoteType);

        builder.HasIndex(note => note.IsPinned);

        builder.HasIndex(note => note.LastEditedAtUtc);

        builder.HasOne(note => note.Paper)
            .WithMany(paper => paper.Notes)
            .HasForeignKey(note => note.PaperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}