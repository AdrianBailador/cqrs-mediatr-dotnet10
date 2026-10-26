using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CqrsMediatRDotNet10.Tests;

/// <summary>
/// Proves, against the real MediatR pipeline (not a hand-built delegate), what happens when a
/// behavior deliberately passes CancellationToken.None downstream instead of forwarding the
/// token it received. RequestHandlerWrapperImpl substitutes the original Send() token back in
/// whenever it sees `default` at any layer — see the "One more thing about the library" section
/// of the companion article for the source line this pins.
/// </summary>
public class CancellationTokenPipelineTests
{
    [Fact]
    public async Task Send_IgnoresADeliberateCancellationTokenNone_AndUsesTheOriginalToken()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssemblyContaining<CancellationTokenPipelineTests>();
            cfg.AddOpenBehavior(typeof(ForceNoneBehavior<,>));
        });

        await using var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        using var cts = new CancellationTokenSource();

        var received = await sender.Send(new Ping(), cts.Token);

        Assert.Equal(cts.Token, received);
        Assert.NotEqual(CancellationToken.None, received);
    }

    public sealed record Ping : IRequest<CancellationToken>;

    public sealed class PingHandler : IRequestHandler<Ping, CancellationToken>
    {
        public Task<CancellationToken> Handle(Ping request, CancellationToken cancellationToken)
            => Task.FromResult(cancellationToken);
    }

    /// <summary>A behavior that deliberately tries to suppress cancellation downstream.</summary>
    public sealed class ForceNoneBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        public Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
            => next(CancellationToken.None);
    }
}
