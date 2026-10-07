using HelpDesk.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace HelpDesk.Infrastructure.Tests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=helpdesk_test;Username=helpdesk;Password=helpdesk";

    public AppDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    public async Task InitializeAsync()
    {
        await using var db = CreateContext();
        await db.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await using var db = CreateContext();
        // Drop all data after the test run, so re-runs start clean.
        await db.Database.ExecuteSqlRawAsync(
            "TRUNCATE TABLE \"Comments\", \"Tickets\", \"Users\", \"Categories\" RESTART IDENTITY CASCADE;");
    }
}