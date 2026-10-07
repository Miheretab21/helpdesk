using HelpDesk.Application.Abstractions;
using HelpDesk.Application.Tests.Fakes;
using HelpDesk.Application.Tickets.Commands.ChangeTicketStatus;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tests.Tickets;

public class ChangeTicketStatusCommandHandlerTests
{
    private static Ticket CreateTicketAssignedTo(Guid agentId)
    {
        var ticket = new Ticket("T", "d", TicketPriority.Medium, Guid.NewGuid(), Guid.NewGuid());
        ticket.AssignTo(agentId);
        return ticket;
    }

    [Fact]
    public async Task Agent_ResolvesTheirOwnAssignedTicket_HappyPath()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var ticket = CreateTicketAssignedTo(agentId);
        ticket.StartWork(agentId); // Move to InProgress so it can be resolved

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new ChangeTicketStatusCommandHandler(repo);

        var command = new ChangeTicketStatusCommand(
            TicketId: ticket.Id,
            TargetStatus: TicketStatus.Resolved,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent);

        // act
        await handler.HandleAsync(command);

        // assert
        var stored = await repo.GetByIdAsync(ticket.Id);
        Assert.NotNull(stored);
        Assert.Equal(TicketStatus.Resolved, stored!.Status);
    }

    [Fact]
    public async Task Agent_TriesToStartWorkOnSomeoneElsesTicket_ShouldThrow()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var otherAgentId = Guid.NewGuid();
        var ticket = CreateTicketAssignedTo(otherAgentId);

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new ChangeTicketStatusCommandHandler(repo);

        var command = new ChangeTicketStatusCommand(
            TicketId: ticket.Id,
            TargetStatus: TicketStatus.InProgress,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("only the assigned agent", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Employee_TriesToChangeAnyStatus_ShouldThrow()
    {
        // arrange
        var employeeId = Guid.NewGuid();
        var ticket = CreateTicketAssignedTo(Guid.NewGuid());

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new ChangeTicketStatusCommandHandler(repo);

        var command = new ChangeTicketStatusCommand(
            TicketId: ticket.Id,
            TargetStatus: TicketStatus.InProgress,
            RequestingUserId: employeeId,
            RequestingUserRole: UserRole.Employee);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("Only agents and admins", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Requesting_TicketStatusNew_ShouldThrow()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var ticket = CreateTicketAssignedTo(agentId);

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new ChangeTicketStatusCommandHandler(repo);

        var command = new ChangeTicketStatusCommand(
            TicketId: ticket.Id,
            TargetStatus: TicketStatus.New,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("Cannot transition", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Resolving_AssignedTicket_ShouldThrow()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var ticket = CreateTicketAssignedTo(agentId); // Status is Assigned, not InProgress

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new ChangeTicketStatusCommandHandler(repo);

        var command = new ChangeTicketStatusCommand(
            TicketId: ticket.Id,
            TargetStatus: TicketStatus.Resolved,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));
        Assert.Contains("in-progress", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task FullWorkflow_AssignToClose_ShouldEndInClosedStatus()
    {
        // arrange
        var agentId = Guid.NewGuid();
        var ticket = CreateTicketAssignedTo(agentId);

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new ChangeTicketStatusCommandHandler(repo);

        // act - Step 1: Start Work (InProgress)
        await handler.HandleAsync(new ChangeTicketStatusCommand(
            TicketId: ticket.Id,
            TargetStatus: TicketStatus.InProgress,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent));

        // act - Step 2: Resolve
        await handler.HandleAsync(new ChangeTicketStatusCommand(
            TicketId: ticket.Id,
            TargetStatus: TicketStatus.Resolved,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent));

        // act - Step 3: Close
        await handler.HandleAsync(new ChangeTicketStatusCommand(
            TicketId: ticket.Id,
            TargetStatus: TicketStatus.Closed,
            RequestingUserId: agentId,
            RequestingUserRole: UserRole.Agent));

        // assert
        var stored = await repo.GetByIdAsync(ticket.Id);
        Assert.NotNull(stored);
        Assert.Equal(TicketStatus.Closed, stored!.Status);
    }
}