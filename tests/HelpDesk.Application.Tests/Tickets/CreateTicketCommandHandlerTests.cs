using HelpDesk.Application.Tests.Fakes;
using HelpDesk.Application.Tickets.Commands.CreateTicket;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tests.Tickets;

public class CreateTicketCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_ShouldCreateAndPersistTicket()
    {
        // arrange
        var repo = new FakeTicketRepository();
        var handler = new CreateTicketCommandHandler(repo);

        var command = new CreateTicketCommand(
            Title: "Cannot access email",
            Description: "Getting an invalid login error.",
            Priority: TicketPriority.High,
            CategoryId: Guid.NewGuid(),
            CreatedById: Guid.NewGuid());

        // act
        var ticketId = await handler.HandleAsync(command);

        // assert
        var stored = await repo.GetByIdAsync(ticketId);
        Assert.NotNull(stored);
        Assert.Equal(TicketStatus.New, stored!.Status);
        Assert.Equal("Cannot access email", stored.Title);
        Assert.Null(stored.AssignedToId);
    }

    [Fact]
    public async Task Handle_WithEmptyTitle_ShouldThrowDomainException()
    {
        // arrange
        var repo = new FakeTicketRepository();
        var handler = new CreateTicketCommandHandler(repo);

        var command = new CreateTicketCommand(
            Title: "",
            Description: "Valid description",
            Priority: TicketPriority.Low,
            CategoryId: Guid.NewGuid(),
            CreatedById: Guid.NewGuid());

        // act + assert
        await Assert.ThrowsAsync<DomainException>(() => handler.HandleAsync(command));

        // and: nothing should have been saved
        var all = await repo.ListAsync(null, null);
        Assert.Empty(all);
    }
}