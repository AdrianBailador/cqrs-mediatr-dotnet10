using CqrsMediatRDotNet10.Api.Data;
using CqrsMediatRDotNet10.Api.Domain;
using CqrsMediatRDotNet10.Api.Products.Events;
using MediatR;

namespace CqrsMediatRDotNet10.Api.Products.Commands;

public sealed class CreateProductCommandHandler(ApplicationDbContext context, IPublisher publisher)
    : IRequestHandler<CreateProductCommand, int>
{
    public async Task<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        var product = new Product { Name = request.Name, Price = request.Price };

        context.Products.Add(product);
        await context.SaveChangesAsync(cancellationToken);

        await publisher.Publish(new ProductCreatedEvent(product.Id), cancellationToken);

        return product.Id;
    }
}
