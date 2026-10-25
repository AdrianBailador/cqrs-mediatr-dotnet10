using CqrsMediatRDotNet10.Api.Data;
using CqrsMediatRDotNet10.Api.Domain;
using CqrsMediatRDotNet10.Api.Products.Queries;
using Microsoft.EntityFrameworkCore;

namespace CqrsMediatRDotNet10.Tests;

public class GetProductByIdQueryHandlerTests
{
    private static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_ReturnsTheMatchingProduct()
    {
        await using var context = NewContext();
        context.Products.Add(new Product { Id = 1, Name = "Mouse", Price = 25.00m });
        await context.SaveChangesAsync();

        var handler = new GetProductByIdQueryHandler(context);
        var result = await handler.Handle(new GetProductByIdQuery(1), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Mouse", result!.Name);
    }

    [Fact]
    public async Task Handle_ReturnsNull_WhenProductDoesNotExist()
    {
        await using var context = NewContext();
        var handler = new GetProductByIdQueryHandler(context);

        var result = await handler.Handle(new GetProductByIdQuery(999), CancellationToken.None);

        Assert.Null(result);
    }
}
