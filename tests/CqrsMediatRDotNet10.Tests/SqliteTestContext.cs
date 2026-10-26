using CqrsMediatRDotNet10.Api.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CqrsMediatRDotNet10.Tests;

/// <summary>
/// A real SQLite database, in memory, for the lifetime of one test — not EF Core's InMemory
/// provider, which skips relational behavior (constraints, real LINQ-to-SQL translation)
/// entirely. The connection has to stay open for as long as the context does; SQLite drops
/// an in-memory database the moment its one connection closes.
/// </summary>
internal sealed class SqliteTestContext : IAsyncDisposable
{
    private readonly SqliteConnection _connection;

    public ApplicationDbContext Context { get; }

    private SqliteTestContext(SqliteConnection connection, ApplicationDbContext context)
    {
        _connection = connection;
        Context = context;
    }

    public static async Task<SqliteTestContext> CreateAsync()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new ApplicationDbContext(options);
        await context.Database.EnsureCreatedAsync();

        return new SqliteTestContext(connection, context);
    }

    public async ValueTask DisposeAsync()
    {
        await Context.DisposeAsync();
        await _connection.DisposeAsync();
    }
}
