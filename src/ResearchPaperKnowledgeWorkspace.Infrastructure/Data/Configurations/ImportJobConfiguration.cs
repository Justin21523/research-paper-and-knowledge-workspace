using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Entities;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public sealed class ImportJobConfiguration
    : EntityBaseConfiguration<ImportJob>
{
    public override void Configure(
        EntityTypeBuilder<ImportJob> builder)
    {
        base.Configure(builder);

        builder.ToTable("ImportJobs");

        builder.Property(job => job.OriginalFilePath)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(job => job.OriginalFileName)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(job => job.Sha256Hash)
            .HasMaxLength(64);

        builder.Property(job => job.DetectedTitle)
            .HasMaxLength(1000);

        builder.Property(job => job.DetectedAuthorText)
            .HasMaxLength(2000);

        builder.Property(job => job.ErrorMessage)
            .HasColumnType("TEXT");

        builder.HasIndex(job => job.Status);

        builder.HasIndex(job => job.Sha256Hash);

        builder.HasIndex(job => job.CreatedAtUtc);

        builder.HasIndex(job => job.PaperId);

        builder.HasIndex(job => job.AttachmentId);

        builder.HasOne<Paper>()
            .WithMany()
            .HasForeignKey(job => job.PaperId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne<Attachment>()
            .WithMany()
            .HasForeignKey(job => job.AttachmentId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}