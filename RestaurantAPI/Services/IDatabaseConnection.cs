namespace RestaurantAPI.Services.Interfaces;

public interface IDatabaseConnection
{
  Task OpenConnection();
  Task CloseConnection();
  Task<ICollection<T>?> GetList<T>(string storedProcedure, Dictionary<string, object> parameters);
  Task<T?> GetOne<T>(string storedProcedure, Dictionary<string, object> parameters) where T : class;
}