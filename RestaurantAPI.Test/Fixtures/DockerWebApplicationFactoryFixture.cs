using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RestaurantAPI.Services;
using RestaurantAPI.Services.Interfaces;
using Testcontainers.MsSql;

namespace RestaurantAPI.Test.Fixtures;
public class DockerWebApplicationFactoryFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;

    public DockerWebApplicationFactoryFixture()
    {
        _dbContainer = new MsSqlBuilder()
          .WithResourceMapping(new FileInfo("../../../../.devcontainer/mssql/"), "/scripts")
          .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var connectionString = _dbContainer.GetConnectionString();
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IDatabaseConnection));
            services.AddScoped<IDatabaseConnection>(options => new DatabaseConnection(connectionString));
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        using (var scope = Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var cntx = scopedServices.GetRequiredService<IDatabaseConnection>();
            await _dbContainer.ExecAsync(["/opt/mssql-tools/bin/sqlcmd", "-i", "/scripts/setup.sql"]);
            await _dbContainer.ExecAsync(["/opt/mssql-tools/bin/sqlcmd", "-i", "/scripts/test.sql"]);
        }
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}
