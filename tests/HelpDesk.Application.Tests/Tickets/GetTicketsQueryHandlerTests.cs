using HelpDesk.Application.Tests.Fakes;
using HelpDesk.Application.Tickets.Queries.GetTickets;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tests.Tickets;

public class GetTicketsQueryHandlerTests
{
    [Fact]
    public async Task Agent_ShouldSeeUnassignedPlusOwnAssigned_ButNotOthers()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var otherAgentId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var unassigned = new Ticket("Unassigned", "desc", TicketPriority.Low, categoryId, employeeId);

        var mine = new Ticket("Mine", "desc", TicketPriority.Medium, categoryId, employeeId);
        mine.AssignTo(agentId);

        var someoneElses = new Ticket("Not mine", "desc", TicketPriority.High, categoryId, employeeId);
        someoneElses.AssignTo(otherAgentId);

        var repo = new FakeTicketRepository(new[] { unassigned, mine, someoneElses });
        var handler = new GetTicketsQueryHandler(repo);

        // act
        var result = await handler.HandleAsync(new GetTicketsQuery(agentId, UserRole.Agent));

        // assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Unassigned");
        Assert.Contains(result, t => t.Title == "Mine");
        Assert.DoesNotContain(result, t => t.Title == "Not mine");
    }
    [Fact]
    public async Task Admin_ShouldSeeAllTicketsRegardlessOfAssignmentOrCreator()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var ticket1 = new Ticket("Ticket One", "desc", TicketPriority.Low, categoryId, Guid.NewGuid());
        var ticket2 = new Ticket("Ticket Two", "desc", TicketPriority.High, categoryId, Guid.NewGuid());
        ticket2.AssignTo(Guid.NewGuid());

        var repo = new FakeTicketRepository(new[] { ticket1, ticket2 });
        var handler = new GetTicketsQueryHandler(repo);

        // Act
        var result = await handler.HandleAsync(new GetTicketsQuery(adminId, UserRole.Admin));

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Ticket One");
        Assert.Contains(result, t => t.Title == "Ticket Two");
    }

    [Fact]
    public async Task Employee_ShouldOnlySeeTicketsTheyCreated()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var myTicket = new Ticket("My Ticket", "desc", TicketPriority.Medium, categoryId, employeeId);
        var someoneElsesTicket = new Ticket("Someone Else's Ticket", "desc", TicketPriority.Low, categoryId, otherEmployeeId);

        var repo = new FakeTicketRepository(new[] { myTicket, someoneElsesTicket });
        var handler = new GetTicketsQueryHandler(repo);

        // Act
        var result = await handler.HandleAsync(new GetTicketsQuery(employeeId, UserRole.Employee));

        // Assert
        Assert.Single(result);
        Assert.Contains(result, t => t.Title == "My Ticket");
        Assert.DoesNotContain(result, t => t.Title == "Someone Else's Ticket");
    }

    [Fact]
    public async Task Query_WithStatusFilter_ShouldReturnOnlyMatchingStatus()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();

        var newTicket = new Ticket("New Ticket", "desc", TicketPriority.Low, categoryId, Guid.NewGuid());
        
        var assignedTicket = new Ticket("Assigned Ticket", "desc", TicketPriority.Medium, categoryId, Guid.NewGuid());
        assignedTicket.AssignTo(Guid.NewGuid()); // Moves status to Assigned

        var repo = new FakeTicketRepository(new[] { newTicket, assignedTicket });
        var handler = new GetTicketsQueryHandler(repo);

        // Act - Request only Assigned status
        var result = await handler.HandleAsync(new GetTicketsQuery(adminId, UserRole.Admin, StatusFilter: TicketStatus.Assigned));

        // Assert
        Assert.Single(result);
        Assert.Contains(result, t => t.Title == "Assigned Ticket");
        Assert.DoesNotContain(result, t => t.Title == "New Ticket");
    }
    [Fact]
public async Task Agent_WithStatusFilter_ShouldStillNotSeeOtherAgentsTicketsWithSameStatus()
{
    // Arrange
    var agentId = Guid.NewGuid();
    var otherAgentId = Guid.NewGuid();
    var categoryId = Guid.NewGuid();

    // Both tickets are Assigned — the agent should see theirs, not the other agent's.
    var mine = new Ticket("My Assigned", "desc", TicketPriority.Low, categoryId, Guid.NewGuid());
    mine.AssignTo(agentId);

    var theirs = new Ticket("Other Agent's Assigned", "desc", TicketPriority.Low, categoryId, Guid.NewGuid());
    theirs.AssignTo(otherAgentId);

    var repo = new FakeTicketRepository(new[] { mine, theirs });
    var handler = new GetTicketsQueryHandler(repo);

    // Act — agent asks for Assigned tickets
    var result = await handler.HandleAsync(
        new GetTicketsQuery(agentId, UserRole.Agent, StatusFilter: TicketStatus.Assigned));

    // Assert
    Assert.Single(result);
    Assert.Equal("My Assigned", result[0].Title);
}
}