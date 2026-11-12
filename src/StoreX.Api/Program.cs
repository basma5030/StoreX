
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using StoreX.Api.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Register application services
builder.Services.AddOpenApi();

// Register StoreX database context
var connectionString = builder.Configuration
    .GetConnectionString("StoreX")
    ?? throw new InvalidOperationException(
        "Connection string 'StoreX' was not found.");

builder.Services.AddDbContext<StoreXDbContext>(options =>
    options.UseSqlServer(connectionString));

var app = builder.Build();

// Configure HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.Run();
