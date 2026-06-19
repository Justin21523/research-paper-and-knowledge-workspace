using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public sealed class AttachmentConfiguration
    : EntityBaseConfiguration<Attachment>
{
    public override void Configure(
        EntityTypeBuilder<Attachment> builder)
    {
        base.Configure(builder);

        builder.Property(attachment => attachment.OriginalFileName)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(attachment => attachment.StoredFileName)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(attachment => attachment.FilePath)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(attachment => attachment.MimeType)
            .HasMaxLength(300);

        builder.Property(attachment => attachment.FileExtension)
            .HasMaxLength(50);

        builder.Property(attachment => attachment.Sha256Hash)
            .HasMaxLength(64);
        
        builder.Property(attachment => attachment.ExtractedText)
            .HasColumnType("TEXT");

        builder.ToTable(
            "Attachments",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_Attachments_FileSizeBytes",
                    "\"FileSizeBytes\" >= 0");

                tableBuilder.HasCheckConstraint(
                    "CK_Attachments_PageCount",
                    "\"PageCount\" IS NULL OR \"PageCount\" >= 0");
            });
        
        builder.HasIndex(attachment => attachment.PaperId);

        builder.HasIndex(attachment => attachment.Sha256Hash)
            .IsUnique();

        builder.HasIndex(attachment => attachment.AttachmentType);

        builder.HasIndex(attachment => attachment.IsPrimary);

        builder.HasOne(attachment => attachment.Paper)
            .WithMany(paper => paper.Attachments)
            .HasForeignKey(attachment => attachment.PaperId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}