namespace CqrsMediatRDotNet10.Api.Domain;

public sealed class Product
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public required decimal Price { get; set; }
}
