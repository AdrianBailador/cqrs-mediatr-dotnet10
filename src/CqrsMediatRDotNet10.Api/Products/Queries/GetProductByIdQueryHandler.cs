using CqrsMediatRDotNet10.Api.Data;
using CqrsMediatRDotNet10.Api.Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CqrsMediatRDotNet10.Api.Products.Queries;

public sealed class GetProductByIdQueryHandler(ApplicationDbContext context)
    : IRequestHandler<GetProductByIdQuery, Product?>
{
    public Task<Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        return context.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
    }
}
