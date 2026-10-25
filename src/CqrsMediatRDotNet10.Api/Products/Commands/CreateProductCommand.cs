using MediatR;

namespace CqrsMediatRDotNet10.Api.Products.Commands;

public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<int>;
