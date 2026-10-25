using CqrsMediatRDotNet10.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CqrsMediatRDotNet10.Api.Data;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
}
