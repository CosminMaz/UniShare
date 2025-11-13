using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Items;
using UniShare.Infrastructure.Features.Users;
using UniShare.Infrastructure.Persistence;
using UniShare.Infrastructure.Validators;

var builder = WebApplication.CreateBuilder(args);

// Database: prefer configured connection, but fall back to InMemory for local/dev/testing so Swagger works out-of-the-box
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (!string.IsNullOrWhiteSpace(connectionString))
{
    builder.Services.AddDbContext<UniShareContext>(options =>
        options.UseNpgsql(connectionString)
    );
}
else
{
    builder.Services.AddDbContext<UniShareContext>(options =>
        options.UseInMemoryDatabase("UniShareDb")
    );
}

builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddScoped<CreateUserHandler>();
builder.Services.AddScoped<GetAllUsersHandler>(); // Register GetAllUsersHandler
builder.Services.AddScoped<IValidator<CreateUserRequest>, CreateUserValidator>();
builder.Services.AddScoped<CreateItemHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<IValidator<CreateItemRequest>, CreateItemValidator>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// CORS: allow the frontend dev server (Vite) during development
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "AllowFrontend",
        policy =>
        {
            policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply any pending migrations at startup (creates tables if needed via migrations)
using (var scope = app.Services.CreateScope())
{
    var ctx = scope.ServiceProvider.GetRequiredService<UniShareContext>();
    ctx.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Enable CORS for the frontend (applies to all endpoints)
app.UseCors("AllowFrontend");

app.UseHttpsRedirection();

// Health check
app.MapHealthChecks("/health");

app.MapPost("/users", async (CreateUserRequest request, [FromServices] CreateUserHandler handler) =>
{
    // The handler returns the correct IResult (Results.Created)
    return await handler.Handle(request);
})
.WithName("CreateUser");

app.MapGet("/users", async ([FromServices] GetAllUsersHandler handler) =>
{
    return await handler.Handle();
})
.WithName("GetAllUsers");

app.MapPost("/items", async (CreateItemRequest request, [FromServices] CreateItemHandler handler) =>
{
    return await handler.Handle(request);
})
.WithName("CreateItem");

app.MapGet("/items", async (IMediator mediator) =>
{
    return await mediator.Send(new GetAllItems.Query());
})
.WithName("GetAllItems");

app.MapPost("/api/auth/login", async (LoginRequest request, [FromServices] LoginHandler handler) =>
{
    return await handler.Handle(request);
})
.WithName("Login");

app.Run();
