using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RestaurantAPI.Models;
using RestaurantAPI.Services.Interfaces;
using RestaurantAPI.Test.Fixtures;
using RestaurantAPI.Test.Helpers;

namespace RestaurantAPI.Test;

public class OrderTest: IClassFixture<DockerWebApplicationFactoryFixture>
{
    private readonly DockerWebApplicationFactoryFixture _factory;
    private readonly HttpClient _client;

    public OrderTest(DockerWebApplicationFactoryFixture factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Test1()
    {
        // Arrange
        var order = new Order(
            "John Doe",
            "123 Main St",
            "555-555-5555",
            [
                new OrderProduct(Guid.NewGuid(), 12.5m, 1, new Product(new Guid("b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1a"), "Test Product", 12.5m)),
                new OrderProduct(Guid.NewGuid(), 12.5m, 1, new Product(new Guid("b3c7f7a0-0f1a-4f1a-9f1a-0f1a0f1a0f1b"), "Test Product", 12.5m))
            ]
        );
        using var scope = _factory.Services.CreateScope();
        var dbConnection = scope.ServiceProvider.GetRequiredService<IDatabaseConnection>();

        // Act
        await _client.PostAsync(HttpHelper.Urls.AddOrder, HttpHelper.GetJsonHttpContent(order));
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllOrders);
        var result = await response.Content.ReadFromJsonAsync<List<Order>>();
        var orderProducts = await dbConnection.GetList<OrderProduct>("test_validate_order_products", new Dictionary<string, object> { });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().HaveCount(1);
        orderProducts.Should().HaveCount(0);
    }

    [Fact]
    public async Task Test2()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var dbConnection = scope.ServiceProvider.GetRequiredService<IDatabaseConnection>();

        // Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllOrders);
        var result = await response.Content.ReadFromJsonAsync<List<Order>>();
        var orderProducts = await dbConnection.GetList<OrderProduct>("test_validate_order_products", new Dictionary<string, object> { });

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().HaveCount(0);
        orderProducts.Should().HaveCount(0);
    }
}