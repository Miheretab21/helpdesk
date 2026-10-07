using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Tests.Fakes;
using HelpDesk.Application.Tickets.Commands.AssignTicket;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tests.Tickets;

public class AssignTicketCommandHandlerTests
{
    private static Ticket CreateUnassignedTicket() =>
        new("Test ticket", "desc", TicketPriority.Medium, Guid.NewGuid(), Guid.NewGuid());

    [Fact]
    public async Task Admin_CanAssignUnassignedTicketToAnyAgent()
    {
        // arrange
        var adminId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var ticket = CreateUnassignedTicket();

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new AssignTicketCommandHandler(repo);

        // act
        await handler.HandleAsync(new AssignTicketCommand(
            TicketId: ticket.Id,
            AssignToUserId: agentId,
            RequestingUserId: adminId,
            RequestingUserRole: UserRole.Admin));

        // assert
        var stored = await repo.GetByIdAsync(ticket.Id);
        Assert.NotNull(stored);
        Assert.Equal(agentId, stored!.AssignedToId);
        Assert.Equal(TicketStatus.Assigned, stored.Status);
    }

    [Fact]
    public async Task Agent_CanClaimUnassignedTicketForSelf()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var ticket = CreateUnassignedTicket();

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new AssignTicketCommandHandler(repo);

        // act
        await handler.HandleAsync(new AssignTicketCommand(
            TicketId: ticket.Id,
            AssignToUserId: agentId,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent));

        // assert
        var stored = await repo.GetByIdAsync(ticket.Id);
        Assert.Equal(agentId, stored!.AssignedToId);
    }

    [Fact]
    public async Task Agent_TryingToClaimAlreadyAssignedTicket_ShouldThrowDomainException()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var otherAgentId = Guid.NewGuid();
        var ticket = CreateUnassignedTicket();
        ticket.AssignTo(otherAgentId);

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new AssignTicketCommandHandler(repo);

        var command = new AssignTicketCommand(
            TicketId: ticket.Id,
            AssignToUserId: agentId,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("already assigned", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Agent_TryingToAssignToDifferentAgent_ShouldThrowDomainException()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var anotherAgentId = Guid.NewGuid();
        var ticket = CreateUnassignedTicket();

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new AssignTicketCommandHandler(repo);

        var command = new AssignTicketCommand(
            TicketId: ticket.Id,
            AssignToUserId: anotherAgentId,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("themselves", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Employee_TryingToAssignTicket_ShouldThrowDomainException()
    {
        // arrange
        var employeeId = Guid.NewGuid();
        var agentId = Guid.NewGuid();
        var ticket = CreateUnassignedTicket();

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new AssignTicketCommandHandler(repo);

        var command = new AssignTicketCommand(
            TicketId: ticket.Id,
            AssignToUserId: agentId,
            RequestingUserId: employeeId,
            RequestingUserRole: UserRole.Employee);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("Only admins and agents", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Assigning_NonExistentTicket_ShouldThrowDomainExceptionWithNotFound()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var nonExistentId = Guid.NewGuid();

        var repo = new FakeTicketRepository();
        var handler = new AssignTicketCommandHandler(repo);

        var command = new AssignTicketCommand(
            TicketId: nonExistentId,
            AssignToUserId: agentId,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task When_AgentAssignmentFails_TicketStateShouldBeUnchanged()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var otherAgentId = Guid.NewGuid();
        var ticket = CreateUnassignedTicket();

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new AssignTicketCommandHandler(repo);

        var command = new AssignTicketCommand(
            TicketId: ticket.Id,
            AssignToUserId: otherAgentId,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("themselves", ex.Message, StringComparison.OrdinalIgnoreCase);

        var stored = await repo.GetByIdAsync(ticket.Id);
        Assert.NotNull(stored);
        Assert.Null(stored!.AssignedToId);
        Assert.Equal(TicketStatus.New, stored.Status);
    }
}