using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ResearchPaperKnowledgeWorkspace.Core.Common;

namespace ResearchPaperKnowledgeWorkspace.Infrastructure.Data.Configurations;

public abstract class EntityBaseConfiguration<TEntity>
    : IEntityTypeConfiguration<TEntity>
    where TEntity : EntityBase
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.Id)
            .ValueGeneratedNever();

        builder.Property(entity => entity.CreatedAtUtc)
            .IsRequired();

        builder.Property(entity => entity.UpdatedAtUtc)
            .IsRequired();
    }
}