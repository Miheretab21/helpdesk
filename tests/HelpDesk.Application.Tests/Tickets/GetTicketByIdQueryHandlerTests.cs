using HelpDesk.Application.Tests.Fakes;
using HelpDesk.Application.Tickets.Queries.GetTicketById;
using HelpDesk.Domain.Common;
using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;

namespace HelpDesk.Application.Tests.Tickets;

public class GetTicketByIdQueryHandlerTests
{
    private static Ticket CreateTicket(Guid createdById) =>
        new("Test ticket", "desc", TicketPriority.Medium, Guid.NewGuid(), createdById);

    [Fact]
    public async Task Admin_CanRetrieveAnyTicket()
    {
        // arrange
        var adminId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var ticket = CreateTicket(employeeId);

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new GetTicketByIdQueryHandler(repo);

        // act
        var result = await handler.HandleAsync(
            new GetTicketByIdQuery(ticket.Id, adminId, UserRole.Admin));

        // assert
        Assert.Equal(ticket.Id, result.Id);
        Assert.Equal("Test ticket", result.Title);
        Assert.Equal(TicketStatus.New, result.Status);
    }

    [Fact]
    public async Task Employee_CannotRetrieveTicketTheyDidNotCreate()
    {
        // arrange
        var employeeId = Guid.NewGuid();
        var anotherEmployeeId = Guid.NewGuid();
        var ticket = CreateTicket(anotherEmployeeId);

        var repo = new FakeTicketRepository(new[] { ticket });
        var handler = new GetTicketByIdQueryHandler(repo);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new GetTicketByIdQuery(ticket.Id, employeeId, UserRole.Employee)));

        // The message says "not found" on purpose — we do not reveal that the ticket exists
        // but is not visible to this user.
        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task GetById_NonExistentTicket_ShouldThrow()
    {
        // arrange
        var repo = new FakeTicketRepository();
        var handler = new GetTicketByIdQueryHandler(repo);

        // act & assert
        var ex = await Assert.ThrowsAsync<DomainException>(() =>
            handler.HandleAsync(new GetTicketByIdQuery(
                Guid.NewGuid(), Guid.NewGuid(), UserRole.Admin)));

        Assert.Contains("not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }
}