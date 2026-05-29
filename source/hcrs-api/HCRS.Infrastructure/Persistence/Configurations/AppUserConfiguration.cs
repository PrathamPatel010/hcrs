using HCRS.Domain.Entities;
using HCRS.Infrastructure.Extension;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HCRS.Infrastructure.Persistence.Configurations;

public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
{
    public void Configure(EntityTypeBuilder<AppUser> builder)
    {
        builder.ConfigureBaseFields();

        builder.ToTable("Users");

        builder.Property(builder => builder.UserName)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(builder => builder.UserEmail)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(builder => builder.UserDisplayName)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(builder => builder.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);
        builder.Property(builder => builder.RoleId)
            .IsRequired();

        builder.HasIndex(u=> u.UserName)
            .IsUnique();
        builder.HasIndex(u=> u.UserEmail)
            .IsUnique();
        builder.HasOne(u=>u.Role)
            .WithMany(r=>r.Users)
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
