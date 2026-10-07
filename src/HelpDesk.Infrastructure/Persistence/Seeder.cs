using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return; // already seeded

        var alice = new User("alice@example.com", "Alice Employee", "hash", UserRole.Employee);
        var bob   = new User("bob@example.com",   "Bob Agent",      "hash", UserRole.Agent);
        var carol = new User("carol@example.com", "Carol Admin",    "hash", UserRole.Admin);

        var hardware = new Category("Hardware");
        var software = new Category("Software");
        var network  = new Category("Network");

        db.Users.AddRange(alice, bob, carol);
        db.Categories.AddRange(hardware, software, network);
        await db.SaveChangesAsync();

        var ticket1 = new Ticket("Laptop won't boot", "Presses power, nothing happens.",
            TicketPriority.High, hardware.Id, alice.Id);
        var ticket2 = new Ticket("Excel crashes on save", "Large file, crashes every time.",
            TicketPriority.Medium, software.Id, alice.Id);
        var ticket3 = new Ticket("Wi-Fi drops every hour", "Connection resets.",
            TicketPriority.Low, network.Id, alice.Id);
        ticket2.AssignTo(bob.Id);
        ticket3.AssignTo(bob.Id);

        db.Tickets.AddRange(ticket1, ticket2, ticket3);
        await db.SaveChangesAsync();
    }
}