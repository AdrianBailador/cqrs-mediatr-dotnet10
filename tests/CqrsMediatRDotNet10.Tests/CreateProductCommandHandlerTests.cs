using CqrsMediatRDotNet10.Api.Data;
using CqrsMediatRDotNet10.Api.Products.Commands;
using CqrsMediatRDotNet10.Api.Products.Events;
using Microsoft.EntityFrameworkCore;

namespace CqrsMediatRDotNet10.Tests;

public class CreateProductCommandHandlerTests
{
    private static ApplicationDbContext NewContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    [Fact]
    public async Task Handle_PersistsProduct_AndReturnsItsId()
    {
        await using var context = NewContext();
        var publisher = new RecordingPublisher();
        var handler = new CreateProductCommandHandler(context, publisher);

        var id = await handler.Handle(new CreateProductCommand("Keyboard", 49.99m), CancellationToken.None);

        var stored = await context.Products.FindAsync(id);
        Assert.NotNull(stored);
        Assert.Equal("Keyboard", stored!.Name);
        Assert.Equal(49.99m, stored.Price);
    }

    [Fact]
    public async Task Handle_PublishesProductCreatedEvent_WithTheNewId()
    {
        await using var context = NewContext();
        var publisher = new RecordingPublisher();
        var handler = new CreateProductCommandHandler(context, publisher);

        var id = await handler.Handle(new CreateProductCommand("Monitor", 199.00m), CancellationToken.None);

        var published = Assert.Single(publisher.Published);
        var productCreated = Assert.IsType<ProductCreatedEvent>(published);
        Assert.Equal(id, productCreated.ProductId);
    }
}
