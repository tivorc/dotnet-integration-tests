using System.Text.Json;
using RestaurantAPI.Models;
using RestaurantAPI.Services;
using RestaurantAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new ArgumentNullException("Connection string is null or empty");
}

builder.Services.AddScoped<IDatabaseConnection>(s => new DatabaseConnection(connectionString));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/orders", async (IDatabaseConnection db) =>
{
    var orders = await db.GetList<Order>("dbo.get_orders", new Dictionary<string, object> { });
    return Results.Ok(orders);
})
.WithName("GetOrders")
.WithOpenApi();

app.MapGet("/api/order/{orderId}", async (IDatabaseConnection db, Guid orderId) =>
{
    var order = await db.GetOne<Order>("dbo.get_order_by_id", new Dictionary<string, object> { { "order_id", orderId } });
    return Results.Ok(order);
})
.WithName("GetOrderById")
.WithOpenApi();

app.MapPost("/api/order", async (IDatabaseConnection db, Order order) =>
{
    var serializedOrder = JsonSerializer.Serialize(order, new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });
    var orderId = await db.GetOne<Order>("dbo.insert_order", new Dictionary<string, object> { { "order", serializedOrder } });
    return Results.Ok(orderId);
})
.WithName("InsertOrder")
.WithOpenApi();

app.Run();

public partial class Program {}