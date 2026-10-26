# CQRS with MediatR on .NET 10

Companion repo for [CQRS with MediatR, Revisited for .NET 10](https://adrianbailador.github.io/blog/75-cqrs-mediatr-dotnet10-rebuilt/) — a runnable CQRS setup on .NET 10 and MediatR 14, rebuilt from scratch instead of patched in place: the license key registration MediatR now needs, pipeline behaviors and exception handling done properly, SQLite-in-memory tests instead of a mocked `DbContext`, and a source-generated alternative worth knowing about.

## What's here

- **`CqrsMediatRDotNet10.Api`** — a minimal API with a `CreateProductCommand` and a `GetProductByIdQuery`, each going through MediatR's pipeline:
  - `ValidationBehavior` — runs FluentValidation validators before the handler, short-circuits with a `ValidationException` that an `IExceptionHandler` turns into a 400 `ValidationProblemDetails`.
  - `LoggingBehavior` — times every request and logs it, valid or not.
  - `ProductCreatedEvent` / `ProductCreatedEventHandler` — a domain notification published after a successful create, handled separately from the write itself.
- **`MediatorAlternative.Demo`** — the same `CreateProductCommand` *shape*, rewritten against [martinothamar/Mediator](https://github.com/martinothamar/Mediator) instead of MediatR. Source-generated, MIT-licensed, no license key, no reflection-based assembly scanning. It's deliberately thin — a static counter instead of persistence, no notification published — to compare the mediator plumbing, not to claim functional parity with the API's `CreateProductCommandHandler`.
- **`CqrsMediatRDotNet10.Tests`** — 9 tests against a real SQLite database kept in memory, not a mocked `DbContext` and not EF Core's InMemory provider (mocking `SaveChangesAsync`/`DbSet<T>` is fragile, and InMemory skips real relational behavior — constraints, actual query translation — that SQLite doesn't).

## A detail about `next()`, not a bug

`RequestHandlerDelegate<TResponse>`'s `CancellationToken` parameter has a default value, so `await next()` compiles without it. That's not a footgun: MediatR's own pipeline wrapper (`RequestHandlerWrapperImpl` in its source) substitutes the original request token back in whenever it sees `default` at any layer, so the caller's real token always reaches the handler either way — even if a behavior passes `CancellationToken.None` on purpose. This repo still calls `next(cancellationToken)` explicitly throughout — it reads clearer, and it's the only way to intentionally pass a *different* token downstream without that fallback overriding it.

Two tests pin this against the real pipeline, not a hand-built delegate: `ValidationBehaviorTests.Handle_ForwardsTheCallersCancellationToken_ToNext` (this repo's own behavior forwards the token it's given) and `CancellationTokenPipelineTests.Send_IgnoresADeliberateCancellationTokenNone_AndUsesTheOriginalToken` (a behavior that deliberately passes `CancellationToken.None` still can't stop the original `Send()` token from reaching the handler, end to end through `AddMediatR` and a real `ISender`).

## MediatR's license key

MediatR 13+ is dual-licensed (RPL-1.5 / commercial) through Lucky Penny Software. The Community edition is free under $5M gross annual revenue, but it still wants a license key for usage auditing — register one at [mediatr.io](https://mediatr.io). Running this repo without one works fine; MediatR just logs a reminder. Set yours via `MEDIATR_LICENSE_KEY`/`LUCKYPENNY_LICENSE_KEY` environment variables, or `MediatR:LicenseKey` in configuration — never commit it to `appsettings.json`.

## Running it

```bash
dotnet test                                               # 9 tests: validation, persistence, events, cancellation propagation

dotnet run --project src/CqrsMediatRDotNet10.Api         # API on the port printed to the console
dotnet run --project src/MediatorAlternative.Demo        # source-generated alternative, no web host needed
```

```bash
curl -X POST http://localhost:<port>/products -H "Content-Type: application/json" -d '{"name":"Keyboard","price":49.99}'
curl http://localhost:<port>/products/1

# Invalid input short-circuits in the pipeline before the handler ever runs:
curl -X POST http://localhost:<port>/products -H "Content-Type: application/json" -d '{"name":"","price":-5}'
```

## Project layout

```
src/CqrsMediatRDotNet10.Api/
  Domain/Product.cs
  Data/ApplicationDbContext.cs
  Products/Commands/           CreateProductCommand, handler, FluentValidation validator
  Products/Queries/            GetProductByIdQuery, handler
  Products/Events/             ProductCreatedEvent, handler
  Behaviors/                   ValidationBehavior, LoggingBehavior
  Infrastructure/              ValidationExceptionHandler (FluentValidation -> 400 ProblemDetails)
  Program.cs                   minimal API endpoints, MediatR + license key registration

src/MediatorAlternative.Demo/  same CreateProduct flow on martinothamar/Mediator

tests/CqrsMediatRDotNet10.Tests/
  SqliteTestContext.cs                   real SQLite database, in memory, per test
  CreateProductCommandHandlerTests.cs    persistence + event publishing
  GetProductByIdQueryHandlerTests.cs
  ValidationBehaviorTests.cs             validation short-circuit + explicit token forwarding
  CancellationTokenPipelineTests.cs      proves the None-substitution fallback against the real MediatR pipeline
```

## License

This repo's code is MIT — use it however's useful. MediatR itself is licensed separately by Lucky Penny Software; see [mediatr.io](https://mediatr.io) for its terms.
