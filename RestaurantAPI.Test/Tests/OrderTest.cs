using System.Net.Http.Json;
using FluentAssertions;
using RestaurantAPI.Models;
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

        // Act
        await _client.PostAsync(HttpHelper.Urls.AddOrder, HttpHelper.GetJsonHttpContent(order));
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllOrders);
        var result = await response.Content.ReadFromJsonAsync<List<Order>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task Test2()
    {
        // Arrange

        // Act
        var response = await _client.GetAsync(HttpHelper.Urls.GetAllOrders);
        var result = await response.Content.ReadFromJsonAsync<List<Order>>();

        // Assert
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        result.Should().HaveCount(0);
    }
}