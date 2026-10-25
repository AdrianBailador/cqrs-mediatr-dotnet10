using MediatR;

namespace CqrsMediatRDotNet10.Api.Products.Events;

public sealed record ProductCreatedEvent(int ProductId) : INotification;
