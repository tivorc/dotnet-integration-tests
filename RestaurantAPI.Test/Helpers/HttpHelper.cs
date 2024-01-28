using System.Text;
using System.Text.Json;

namespace RestaurantAPI.Test.Helpers;
internal class HttpHelper
{
    public static StringContent GetJsonHttpContent(object items)
    {
        return new StringContent(JsonSerializer.Serialize(items), Encoding.UTF8, "application/json");
    }

    internal static class Urls
    {
        public readonly static string GetAllOrders = "/api/orders";
        public readonly static string GetOrder = "/api/order";
        public readonly static string AddOrder = "/api/order";
    }
}
