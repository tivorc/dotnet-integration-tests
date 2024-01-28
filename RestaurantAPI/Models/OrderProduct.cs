namespace RestaurantAPI.Models;

public class OrderProduct
{
    public Guid Id { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Product Product { get; set; }

    public OrderProduct(Guid id, decimal price, int quantity, Product product)
    {
        Id = id;
        Price = price;
        Quantity = quantity;
        Product = product;
    }
}