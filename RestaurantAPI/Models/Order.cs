namespace RestaurantAPI.Models;

public class Order
{
  public Guid Id { get; set; }
  public string CustomerName { get; set; }
  public string CustomerAddress { get; set; }
  public string CustomerPhoneNumber { get; set; }
  public List<OrderProduct> OrderProducts { get; set; } = [];
  public decimal TotalPrice { get; set; }
  public DateTime OrderDate { get; set; }
  public string Status { get; set; }

  public Order(string customerName, string customerAddress, string customerPhoneNumber, List<OrderProduct> orderProducts)
  {
    Id = Guid.NewGuid();
    CustomerName = customerName;
    CustomerAddress = customerAddress;
    CustomerPhoneNumber = customerPhoneNumber;
    OrderProducts = orderProducts;
    TotalPrice = orderProducts.Sum(p => p.Price);
    OrderDate = DateTime.Now;
    Status = "Pending";
  }
}