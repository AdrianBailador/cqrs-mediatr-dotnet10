# CQRS with MediatR on .NET 10

Companion repo for [MediatR Needs a License Key Now. Most CQRS Guides Don't Mention That.](https://adrianbailador.github.io/blog/) — a runnable CQRS setup on .NET 10 and MediatR 14, built to show what actually changed since MediatR went dual-licensed in 2025: the license key registration, cancellation-token propagation through pipeline behaviors, and a source-generated alternative that sidesteps the licensing question entirely.

## What's here

- **`CqrsMediatRDotNet10.Api`** — a minimal API with a `CreateProductCommand` and a `GetProductByIdQuery`, each going through MediatR's pipeline:
  - `ValidationBehavior` — runs FluentValidation validators before the handler, short-circuits with a `ValidationException` that an `IExceptionHandler` turns into a 400 `ValidationProblemDetails`.
  - `LoggingBehavior` — times every request and logs it, valid or not.
  - `ProductCreatedEvent` / `ProductCreatedEventHandler` — a domain notification published after a successful create, handled separately from the write itself.
- **`MediatorAlternative.Demo`** — the same `CreateProductCommand` shape, rewritten against [martinothamar/Mediator](https://github.com/martinothamar/Mediator) instead of MediatR. Source-generated, MIT-licensed, no license key, no reflection-based assembly scanning. Proves the handler code barely changes if you don't want the licensing conversation at all.
- **`CqrsMediatRDotNet10.Tests`** — 8 tests against the EF Core InMemory provider, not a mocked `DbContext` (mocking `SaveChangesAsync`/`DbSet<T>` behavior is fragile and tells you less than it looks like it does). Includes a regression test for a real bug this repo hit while building it: see below.

## The bug this repo caught

`RequestHandlerDelegate<TResponse>`'s `CancellationToken` parameter has a default value. `await next()` compiles — and silently stops forwarding the caller's actual token to the rest of the pipeline. `ValidationBehaviorTests.Handle_ForwardsTheCallersCancellationToken_ToNext` pins the fix (`next(cancellationToken)`, explicit, every time).

## MediatR's license key

MediatR 13+ is dual-licensed (RPL-1.5 / commercial) through Lucky Penny Software. The Community edition is free under $5M gross annual revenue, but it still wants a license key for usage auditing — register one at [mediatr.io](https://mediatr.io). Running this repo without one works fine; MediatR just logs a reminder. Set yours via `MEDIATR_LICENSE_KEY`/`LUCKYPENNY_LICENSE_KEY` environment variables, or `MediatR:LicenseKey` in configuration — never commit it to `appsettings.json`.

## Running it

```bash
dotnet test                                               # 8 tests: validation, persistence, events, cancellation propagation

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
  CreateProductCommandHandlerTests.cs    persistence + event publishing, EF Core InMemory
  GetProductByIdQueryHandlerTests.cs
  ValidationBehaviorTests.cs             validation short-circuit + cancellation-token propagation
```

## License

This repo's code is MIT — use it however's useful. MediatR itself is licensed separately by Lucky Penny Software; see [mediatr.io](https://mediatr.io) for its terms.
