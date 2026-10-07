using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320);

        // Unique index — no two users can share an email.
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role).HasConversion<int>().IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();

        // IsAgent and IsAdmin are computed — do not map them.
        builder.Ignore(u => u.IsAgent);
        builder.Ignore(u => u.IsAdmin);
    }
}