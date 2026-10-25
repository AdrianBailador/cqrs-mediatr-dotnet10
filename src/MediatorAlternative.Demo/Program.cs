// Same CreateProduct flow as the API, rewritten against martinothamar/Mediator instead of
// MediatR. The point isn't that this is better — it's that the handler code barely changes.
// See "Comparativa con Mediator" in the companion article for the full write-up.

using Mediator;
using MediatorAlternative.Demo;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddMediator();

await using var provider = services.BuildServiceProvider();
var sender = provider.GetRequiredService<ISender>();

var id = await sender.Send(new CreateProductCommand("Keyboard", 49.99m));
Console.WriteLine($"Created product {id}");

namespace MediatorAlternative.Demo
{
    public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<int>;

    public sealed class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, int>
    {
        private static int _nextId = 1;

        public ValueTask<int> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // No database here — this demo exists to compare the mediator plumbing, not to
            // duplicate the API's persistence layer.
            var id = _nextId++;
            return new ValueTask<int>(id);
        }
    }
}
