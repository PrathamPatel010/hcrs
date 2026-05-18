using HCRS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HCRS.Infrastructure.Extension
{
    public static class EntityConfigurationExtension
    {
        public static void ConfigureBaseFields<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : BaseEntity
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.CreatedBy).IsRequired();
            builder.Property(x => x.IsDeleted).HasDefaultValue(false).IsRequired();
        }
    }
}
