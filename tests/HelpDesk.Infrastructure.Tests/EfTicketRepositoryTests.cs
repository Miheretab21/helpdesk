using HelpDesk.Domain.Entities;
using HelpDesk.Domain.Enums;
using HelpDesk.Infrastructure.Persistence.Repositories;

namespace HelpDesk.Infrastructure.Tests;

public class EfTicketRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _fixture;

    public EfTicketRepositoryTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task AddAsync_ThenGetById_ShouldRoundTripTicket()
    {
        // arrange
        await using var db = _fixture.CreateContext();

        // We need a real Category and User, because of the FK constraints.
        var category = new Category("Hardware");
        var employee = new User(
            email: "alice@example.com",
            fullName: "Alice",
            passwordHash: "not-a-real-hash",
            role: UserRole.Employee);

        db.Categories.Add(category);
        db.Users.Add(employee);
        await db.SaveChangesAsync();

        var repo = new EfTicketRepository(db);

        var ticket = new Ticket(
            title: "Printer broken",
            description: "It jams every time.",
            priority: TicketPriority.High,
            categoryId: category.Id,
            createdById: employee.Id);

        // act
        await repo.AddAsync(ticket);
        var reloaded = await repo.GetByIdAsync(ticket.Id);

        // assert
        Assert.NotNull(reloaded);
        Assert.Equal(ticket.Id, reloaded!.Id);
        Assert.Equal("Printer broken", reloaded.Title);
        Assert.Equal(TicketStatus.New, reloaded.Status);
        Assert.Null(reloaded.AssignedToId);
        Assert.Equal(category.Id, reloaded.CategoryId);
        Assert.Equal(employee.Id, reloaded.CreatedById);
    }
}