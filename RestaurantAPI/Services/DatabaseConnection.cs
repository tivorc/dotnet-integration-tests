using System.Data;
using System.Text;
using System.Text.Json;
using Microsoft.Data.SqlClient;
using RestaurantAPI.Services.Interfaces;

namespace RestaurantAPI.Services;

  public class DatabaseConnection : IDatabaseConnection
  {
    private readonly SqlConnection connection;
    public DatabaseConnection(string connectionString)
    {
      connection = new SqlConnection(connectionString);
    }

    public async Task OpenConnection()
    {
      if (connection.State != ConnectionState.Open)
      {
        await connection.OpenAsync();
      }
    }

    public async Task CloseConnection()
    {
      if (connection.State == ConnectionState.Open)
      {
        await connection.CloseAsync();
      }
    }

    public async Task<ICollection<T>?> GetList<T>(string storedProcedure, Dictionary<string, object> parameters)
    {
      try
      {
        await OpenConnection();
        var result = new List<T>();
        using var command = connection.CreateCommand();

        command.CommandText = storedProcedure;
        command.CommandType = CommandType.StoredProcedure;
        AddParameters(command, parameters);

        using SqlDataReader reader = await command.ExecuteReaderAsync();
        var response = new StringBuilder();

        while (reader.Read())
        {
          response.Append(reader.GetString(0));
        }

        var str = response.ToString();
        return str == "" ? new List<T>() : JsonSerializer.Deserialize<List<T>>(str);
      }
      catch (Exception)
      {
        throw;
      }
      finally
      {
        await CloseConnection();
      }
    }

    public async Task<T?> GetOne<T>(string storedProcedure, Dictionary<string, object> parameters) where T : class
    {
      try
      {
        await OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = storedProcedure;
        command.CommandType = CommandType.StoredProcedure;
        AddParameters(command, parameters);

        SqlDataReader reader = await command.ExecuteReaderAsync();
        var response = new StringBuilder();

        while (reader.Read())
        {
          response.Append(reader.GetString(0));
        }

        var str = response.ToString();
        return str == "" ? null : JsonSerializer.Deserialize<T>(str);
      }
      catch (Exception)
      {
        throw;
      }
      finally
      {
        await CloseConnection();
      }
    }

    public async Task<bool> Confirm(string storedProcedure, Dictionary<string, object> parameters)
    {
      try
      {
        await OpenConnection();
        using var command = connection.CreateCommand();

        command.CommandText = storedProcedure;
        command.CommandType = CommandType.StoredProcedure;
        AddParameters(command, parameters);

        int reader = await command.ExecuteNonQueryAsync();

        return reader > 0;
      }
      catch (Exception)
      {
        throw;
      }
      finally
      {
        await CloseConnection();
      }
    }

    private void AddParameters(SqlCommand command, Dictionary<string, object> parameters)
    {
      if (parameters != null)
      {
        foreach (KeyValuePair<string, object> parametro in parameters)
        {
          SqlParameter sqlPar = new SqlParameter("@" + parametro.Key, parametro.Value)
          {
            Direction = ParameterDirection.Input
          };
          command.Parameters.Add(sqlPar);
        }
      }
    }
  }