using NotificationForwarder.Api.Endpoints;
using NotificationForwarder.Application.Contracts;
using NotificationForwarder.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddTransient<NotificationProcessor>();
builder.Services.AddTransient<IDiscordNotifier, DiscordNotifier>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapNotificationEndpoints();
app.MapGet("/health", () => Results.Ok("Healthy"));

app.Run();

