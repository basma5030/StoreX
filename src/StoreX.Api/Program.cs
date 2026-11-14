using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scalar.AspNetCore;

using StoreX.Api.Infrastructure;
using StoreX.Api.Infrastructure.Persistence;
using StoreX.Api.Infrastructure.Persistence.Auditing;
using StoreX.Api.Infrastructure.Seeding;
using StoreX.Api.Modules.Organization.Application;
using StoreX.Api.Modules.Organization.Contracts;
using StoreX.Api.Modules.Organization.Endpoints;
using StoreX.Api.Modules.Organization.Seeding;
using StoreX.Api.Shared;
using StoreX.Api.Infrastructure.Exceptions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentUser, HttpCurrentUser>();

builder.Services.AddSingleton<IClock, SystemClock>();

// Register Seed Infrastructure
builder.Services.AddScoped<StoreXSeederRunner>();

builder.Services.AddStoreXSeeder<OrganizationSeeder>();
builder.Services.AddScoped<BranchService>();
builder.Services.AddScoped<IBranchService>(
    serviceProvider => serviceProvider.GetRequiredService<BranchService>());
builder.Services.AddScoped<IBranchLookup>(
    serviceProvider => serviceProvider.GetRequiredService<BranchService>());

// Register application services
builder.Services.AddOpenApi();

// Register Audit Infrastructure
builder.Services.AddScoped<AuditSaveChangesInterceptor>();

// Register StoreX database context
var connectionString = builder.Configuration
    .GetConnectionString("StoreX")
    ?? throw new InvalidOperationException(
        "Connection string 'StoreX' was not found.");

builder.Services.AddDbContext<StoreXDbContext>(
    (serviceProvider, options) =>
    {
        options.UseSqlServer(connectionString);

        options.AddInterceptors(
            serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
    });

// Register Health Checks
builder.Services
    .AddHealthChecks()
    .AddDbContextCheck<StoreXDbContext>("database");

var app = builder.Build();
app.UseExceptionHandler();

// Initialize database before accepting requests.
await using (var scope = app.Services.CreateAsyncScope())
{
    var dbContext = scope.ServiceProvider
        .GetRequiredService<StoreXDbContext>();

    if (app.Environment.IsEnvironment("Testing"))
    {
        await dbContext.Database.EnsureCreatedAsync();
    }
    else
    {
        await dbContext.Database.MigrateAsync();
    }
    var seederRunner = scope.ServiceProvider
        .GetRequiredService<StoreXSeederRunner>();

    await seederRunner.SeedAllAsync();
}

app.Logger.LogInformation(
    "StoreX database initialization completed successfully.");

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Health Check Endpoint
app.MapHealthChecks("/health");
app.MapBranchEndpoints();

app.Run();

public partial class Program { }
