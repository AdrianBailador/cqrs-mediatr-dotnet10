using CqrsMediatRDotNet10.Api.Domain;
using MediatR;

namespace CqrsMediatRDotNet10.Api.Products.Queries;

public sealed record GetProductByIdQuery(int Id) : IRequest<Product?>;
