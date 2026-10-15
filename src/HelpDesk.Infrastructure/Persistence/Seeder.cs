using HelpDesk.Application.Abstractions;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Persistence;

public static class Seeder
{
    public static async Task SeedAsync(AppDbContext db, IPasswordHasher hasher)
    {
        if (await db.Users.AnyAsync()) return;

        var password = hasher.Hash("password123");

        var alice = new User("alice@example.com", "Alice Employee", password, UserRole.Employee);
        var bob   = new User("bob@example.com",   "Bob Agent",      password, UserRole.Agent);
        var carol = new User("carol@example.com", "Carol Admin",    password, UserRole.Admin);

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
        var ticket4 = new Ticket("Monitor flickering", "Screen flickers intermittently.",
            TicketPriority.Medium, hardware.Id, alice.Id);
        var ticket5 = new Ticket("Printer offline", "Cannot connect to network printer.",
            TicketPriority.High, network.Id, alice.Id);
        var ticket6 = new Ticket("Software license expired", "Need to renew license.",
            TicketPriority.Low, software.Id, alice.Id);
        var ticket7 = new Ticket("Email sync issues", "Outlook not syncing emails.",
            TicketPriority.High, software.Id, alice.Id);

        // New status (unassigned)
        // ticket1 stays as New

        // Assigned status (assigned to bob)
        ticket2.AssignTo(bob.Id);
        ticket3.AssignTo(bob.Id);

        // InProgress status
        ticket4.AssignTo(bob.Id);
        ticket4.GetType().GetProperty("Status")?.SetValue(ticket4, TicketStatus.InProgress);

        // Resolved status
        ticket5.AssignTo(bob.Id);
        ticket5.GetType().GetProperty("Status")?.SetValue(ticket5, TicketStatus.InProgress);
        ticket5.GetType().GetProperty("Status")?.SetValue(ticket5, TicketStatus.Resolved);

        // Closed status
        ticket6.AssignTo(bob.Id);
        ticket6.GetType().GetProperty("Status")?.SetValue(ticket6, TicketStatus.InProgress);
        ticket6.GetType().GetProperty("Status")?.SetValue(ticket6, TicketStatus.Resolved);
        ticket6.GetType().GetProperty("Status")?.SetValue(ticket6, TicketStatus.Closed);

        // Cancelled status
        ticket7.GetType().GetProperty("Status")?.SetValue(ticket7, TicketStatus.Cancelled);

        db.Tickets.AddRange(ticket1, ticket2, ticket3, ticket4, ticket5, ticket6, ticket7);
        await db.SaveChangesAsync();
    }
}