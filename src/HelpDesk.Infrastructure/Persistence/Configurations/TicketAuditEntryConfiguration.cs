using HelpDesk.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HelpDesk.Infrastructure.Persistence.Configurations;

public sealed class TicketAuditEntryConfiguration : IEntityTypeConfiguration<TicketAuditEntry>
{
    public void Configure(EntityTypeBuilder<TicketAuditEntry> builder)
    {
        builder.ToTable("TicketAuditEntries"); builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Details).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TicketId, x.OccurredAt });
        builder.HasOne<Ticket>().WithMany().HasForeignKey(x => x.TicketId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<User>().WithMany().HasForeignKey(x => x.ActorId).OnDelete(DeleteBehavior.SetNull);
    }
}
