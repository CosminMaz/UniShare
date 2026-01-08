using FluentValidation;
using Microsoft.EntityFrameworkCore;
using UniShare.Infrastructure.Features.Reviews.CreateReview;
using UniShare.Infrastructure.Features.Users.Login;
using UniShare.Infrastructure.Features.Users.Register;
using UniShare.Infrastructure.Features.Items.Delete;
using UniShare.Infrastructure.Features.Bookings;
using UniShare.Infrastructure.Features.Bookings.CreateBooking;
using UniShare.Infrastructure.Features.Bookings.ApproveBooking;
using UniShare.Infrastructure.Features.Bookings.CompleteBooking;
using UniShare.Infrastructure.Persistence;
using UniShare.Infrastructure.Validators;
using UniShare.Api;
using UniShare.Infrastructure.Features.Users.GetAll;
using UniShare.Common;
using UniShare.Infrastructure.Features.Items.CreateItem;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON serializer to handle enum strings
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null;
    options.SerializerOptions.WriteIndented = true;
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});

// Database: prefer configured connection, but fall back to InMemory for local/dev/testing so Swagger works out-of-the-box
if (builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<UniShareContext>(options =>
        options.UseInMemoryDatabase("UserApiTestsDb"));
}
else
{
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
            options.UseNpgsql("UniShareDb")
        );
    }
}

builder.Services.AddHealthChecks();

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<RegisterUserHandler>();
builder.Services.AddScoped<GetAllUsersHandler>(); // Register GetAllUsersHandler
builder.Services.AddScoped<IValidator<RegisterUserRequest>, CreateUserValidator>();
builder.Services.AddScoped<CreateItemHandler>();
builder.Services.AddScoped<DeleteItemHandler>();
builder.Services.AddScoped<CreateReviewHandler>();
builder.Services.AddScoped<LoginHandler>();
builder.Services.AddScoped<IValidator<CreateItemRequest>, CreateItemValidator>();
builder.Services.AddScoped<IValidator<CreateReviewRequest>, CreateReviewValidator>();
builder.Services.AddScoped<IValidator<LoginRequest>, LoginRequestValidator>();
builder.Services.AddScoped<CreateBookingHandler>();
builder.Services.AddScoped<ApproveBookingHandler>();
builder.Services.AddScoped<CompleteBookingHandler>();
builder.Services.AddScoped<IValidator<CreateBookingRequest>, CreateBookingValidator>();
builder.Services.AddScoped<IValidator<ApproveBookingRequest>, ApproveBookingValidator>();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());
builder.Services.AddHostedService<BookingBackgroundService>();


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
if (!app.Environment.IsEnvironment("Testing"))
{
    using (var scope = app.Services.CreateScope())
    {
        var ctx = scope.ServiceProvider.GetRequiredService<UniShareContext>();
        ctx.Database.Migrate();
    }
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

// Enable CORS for the frontend (applies to all endpoints)
app.UseCors("AllowFrontend");

// Enable authentication middleware
app.UseAuthenticationMiddleware();

app.UseHttpsRedirection();

app.MapItemEndpoints();
app.MapUserEndpoints();
app.MapBookingEndpoints();
app.MapReviewEndpoints();

app.Run();

public partial class Program { }