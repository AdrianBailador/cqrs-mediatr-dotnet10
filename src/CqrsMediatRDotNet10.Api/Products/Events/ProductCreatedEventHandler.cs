using MediatR;

namespace CqrsMediatRDotNet10.Api.Products.Events;

public sealed class ProductCreatedEventHandler(ILogger<ProductCreatedEventHandler> logger)
    : INotificationHandler<ProductCreatedEvent>
{
    public Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Product {ProductId} created", notification.ProductId);
        return Task.CompletedTask;
    }
}
