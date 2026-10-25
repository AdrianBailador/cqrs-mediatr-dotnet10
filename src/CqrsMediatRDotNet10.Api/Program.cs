using CqrsMediatRDotNet10.Api.Behaviors;
using CqrsMediatRDotNet10.Api.Data;
using CqrsMediatRDotNet10.Api.Domain;
using CqrsMediatRDotNet10.Api.Infrastructure;
using CqrsMediatRDotNet10.Api.Products.Commands;
using CqrsMediatRDotNet10.Api.Products.Queries;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("CqrsDemo"));

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblyContaining<Program>();
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));

    // Community edition is free under $5M annual revenue, but MediatR 13+ still wants a
    // license key for usage auditing. Register at https://mediatr.io, then set it via
    // MEDIATR_LICENSE_KEY (or LUCKYPENNY_LICENSE_KEY) rather than hardcoding it here —
    // see the "MediatR's License, In Practice" section of the companion article.
    var licenseKey = builder.Configuration["MediatR:LicenseKey"];
    if (!string.IsNullOrWhiteSpace(licenseKey))
    {
        cfg.LicenseKey = licenseKey;
    }
});

builder.Services.AddValidatorsFromAssemblyContaining<CreateProductCommandValidator>();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.MapPost("/products", async (CreateProductCommand command, ISender sender, CancellationToken ct) =>
{
    var id = await sender.Send(command, ct);
    return Results.Created($"/products/{id}", new { id });
});

app.MapGet("/products/{id:int}", async (int id, ISender sender, CancellationToken ct) =>
{
    var product = await sender.Send(new GetProductByIdQuery(id), ct);
    return product is Product found ? Results.Ok(found) : Results.NotFound();
});

app.Run();

public partial class Program;
