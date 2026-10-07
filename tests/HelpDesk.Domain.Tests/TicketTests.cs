using HelpDesk.Domain.Common;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Domain.Tests;

public class TicketTests
{
    // A small helper so each test can start from a valid ticket without repeating setup.
    private static Ticket CreateValidTicket() =>
        new(
            title: "Printer is not working",
            description: "Nothing prints when I press print.",
            priority: TicketPriority.Medium,
            categoryId: Guid.NewGuid(),
            createdById: Guid.NewGuid()
        );

    [Fact]
    public void NewTicket_ShouldStartInNewStatus_AndBeUnassigned()
    {
        var ticket = CreateValidTicket();

        Assert.Equal(TicketStatus.New, ticket.Status);
        Assert.Null(ticket.AssignedToId);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void CreatingTicket_WithInvalidTitle_ShouldThrow(string? badTitle)
    {
        var ex = Assert.Throws<DomainException>(() =>
            new Ticket(
                title: badTitle!,
                description: "Valid description",
                priority: TicketPriority.Low,
                categoryId: Guid.NewGuid(),
                createdById: Guid.NewGuid()
            ));

        Assert.Contains("Title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void AssignTo_OnNewTicket_ShouldMoveToAssigned()
    {
        var ticket = CreateValidTicket();
        var agentId = Guid.NewGuid();

        ticket.AssignTo(agentId);

        Assert.Equal(TicketStatus.Assigned, ticket.Status);
        Assert.Equal(agentId, ticket.AssignedToId);
    }

    [Fact]
    public void AssignTo_WhenAlreadyAssigned_ShouldThrow()
    {
        var ticket = CreateValidTicket();
        ticket.AssignTo(Guid.NewGuid());

        Assert.Throws<DomainException>(() => ticket.AssignTo(Guid.NewGuid()));
    }

    [Fact]
    public void StartWork_OnUnassignedTicket_ShouldThrow()
    {
        var ticket = CreateValidTicket();

        Assert.Throws<DomainException>(() => ticket.StartWork(Guid.NewGuid()));
    }

    [Fact]
    public void StartWork_ByDifferentAgent_ShouldThrow()
    {
        var ticket = CreateValidTicket();
        var assignedAgent = Guid.NewGuid();
        ticket.AssignTo(assignedAgent);

        Assert.Throws<DomainException>(() => ticket.StartWork(Guid.NewGuid()));
    }

    [Fact]
    public void FullHappyPath_ShouldEndInClosed()
    {
        var ticket = CreateValidTicket();
        var agentId = Guid.NewGuid();

        ticket.AssignTo(agentId);
        ticket.StartWork(agentId);
        ticket.Resolve(agentId);
        ticket.Close(agentId);

        Assert.Equal(TicketStatus.Closed, ticket.Status);
    }

    [Fact]
    public void Close_BeforeResolve_ShouldThrow()
    {
        var ticket = CreateValidTicket();
        var agentId = Guid.NewGuid();
        ticket.AssignTo(agentId);
        ticket.StartWork(agentId);

        Assert.Throws<DomainException>(() => ticket.Close(agentId));
    }

    [Fact]
    public void AssignTo_WithEmptyGuid_ShouldThrow()
    {
        var ticket = CreateValidTicket();

        Assert.Throws<DomainException>(() => ticket.AssignTo(Guid.Empty));
    }
    [Fact]
    public void Resolve_WhenAlreadyResolved_ShouldThrow()
    {
        var ticket = CreateValidTicket();
        var agentId = Guid.NewGuid();
        
        ticket.AssignTo(agentId);
        ticket.StartWork(agentId);
        ticket.Resolve(agentId);

        // Attempting to resolve again should fail
        Assert.Throws<DomainException>(() => ticket.Resolve(agentId));
    }
    [Fact]
    public void CreatingTicket_WithTooLongDescription_ShouldThrow()
    {
        var overlyLongDescription = new string('A', 4001);

        var ex = Assert.Throws<DomainException>(() =>
            new Ticket(
                title: "Valid Title",
                description: overlyLongDescription,
                priority: TicketPriority.Medium,
                categoryId: Guid.NewGuid(),
                createdById: Guid.NewGuid()
            ));

        Assert.Contains("Description", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}