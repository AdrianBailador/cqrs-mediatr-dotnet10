using CqrsMediatRDotNet10.Api.Behaviors;
using CqrsMediatRDotNet10.Api.Products.Commands;
using FluentValidation;

namespace CqrsMediatRDotNet10.Tests;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_ThrowsValidationException_WhenRequestIsInvalid()
    {
        var behavior = new ValidationBehavior<CreateProductCommand, int>(
            [new CreateProductCommandValidator()]);

        var command = new CreateProductCommand(Name: "", Price: -5);

        await Assert.ThrowsAsync<ValidationException>(() =>
            behavior.Handle(command, _ => Task.FromResult(1), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_CallsNext_WhenRequestIsValid()
    {
        var behavior = new ValidationBehavior<CreateProductCommand, int>(
            [new CreateProductCommandValidator()]);

        var command = new CreateProductCommand(Name: "Webcam", Price: 39.99m);
        var result = await behavior.Handle(command, _ => Task.FromResult(42), CancellationToken.None);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task Handle_CallsNext_WhenNoValidatorsAreRegistered()
    {
        var behavior = new ValidationBehavior<CreateProductCommand, int>([]);

        var command = new CreateProductCommand(Name: "", Price: -5);
        var result = await behavior.Handle(command, _ => Task.FromResult(7), CancellationToken.None);

        Assert.Equal(7, result);
    }

    [Fact]
    public async Task Handle_ForwardsTheCallersCancellationToken_ToNext()
    {
        // RequestHandlerDelegate<T>'s CancellationToken parameter has a default value, so
        // `next()` compiles without it — MediatR's own pipeline substitutes the real token
        // back in when that happens, so this isn't guarding against data loss. It pins the
        // behavior this codebase actually relies on: next(cancellationToken), explicit.
        var behavior = new ValidationBehavior<CreateProductCommand, int>(
            [new CreateProductCommandValidator()]);
        using var cts = new CancellationTokenSource();

        var command = new CreateProductCommand(Name: "Webcam", Price: 39.99m);
        CancellationToken? received = null;

        await behavior.Handle(command, ct =>
        {
            received = ct;
            return Task.FromResult(1);
        }, cts.Token);

        Assert.Equal(cts.Token, received);
    }
}
