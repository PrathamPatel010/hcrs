using HCRS.Domain.Entities;
using HCRS.Infrastructure.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HCRS.Infrastructure.Persistence.Configurations;

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("Roles");

        builder.ConfigureBaseFields();
        builder.Property(x=>x.Name).HasMaxLength(255).IsRequired();
        builder.HasIndex(x=>x.Name).IsUnique();
    }
}
