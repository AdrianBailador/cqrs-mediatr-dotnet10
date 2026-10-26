using CqrsMediatRDotNet10.Api.Domain;
using CqrsMediatRDotNet10.Api.Products.Queries;

namespace CqrsMediatRDotNet10.Tests;

public class GetProductByIdQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsTheMatchingProduct()
    {
        await using var db = await SqliteTestContext.CreateAsync();
        var context = db.Context;
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
        await using var db = await SqliteTestContext.CreateAsync();
        var handler = new GetProductByIdQueryHandler(db.Context);

        var result = await handler.Handle(new GetProductByIdQuery(999), CancellationToken.None);

        Assert.Null(result);
    }
}
