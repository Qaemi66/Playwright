using Respawn;
using WebApi.Data;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;
using Microsoft.Extensions.Logging;

namespace WebApi.Tests.E2E;

public class SqlContainerFixture : IAsyncLifetime
{
    private readonly ILogger<SqlContainerFixture> _logger;
    public MsSqlContainer Container { get; private set; }
    public Respawner Respawner { get; private set; }
    public string ConnectionString { get; private set; }

    public SqlContainerFixture()
    {
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<SqlContainerFixture>();
    }

    public async Task InitializeAsync()
    {
        try
        {
            _logger.LogInformation("Starting SQL Server container...");
            
            // Create and start the container with more specific configuration
            Container = new MsSqlBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2019-latest")
                //.WithPassword("p@ssword")
                .WithAutoRemove(true)
                .WithCleanUp(true)
                // .WithStartupCallback(async (container, ct) =>
                // {
                //     // Wait for SQL Server to be ready
                //     await Task.Delay(TimeSpan.FromSeconds(10), ct);
                // })
                .Build();

            await Container.StartAsync();
            _logger.LogInformation("SQL Server container started successfully");

            // Get the connection string after container is started
            ConnectionString = Container.GetConnectionString().Replace("master", "TestDb");
            _logger.LogInformation("Connection string obtained: {ConnectionString}", ConnectionString);

            // Create database and tables
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlServer(ConnectionString)
                .Options;

            await using var context = new AppDbContext(options);
            await context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Database and tables created successfully");

            // Setup Respawn
            Respawner = await Respawner.CreateAsync(ConnectionString, new RespawnerOptions
            {
                DbAdapter = DbAdapter.SqlServer,
                SchemasToInclude = new[] { "dbo" }
            });
            _logger.LogInformation("Respawner configured successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing SQL Server container");
            if (Container != null)
            {
                await Container.DisposeAsync();
            }
            throw;
        }
    }

    public async Task ResetDbAsync()
    {
        try
        {
            _logger.LogInformation("Resetting database...");
            await Respawner.ResetAsync(ConnectionString);
            _logger.LogInformation("Database reset completed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting database");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        try
        {
            _logger.LogInformation("Disposing SQL Server container...");
            if (Container != null)
            {
                await Container.DisposeAsync();
            }
            _logger.LogInformation("SQL Server container disposed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing SQL Server container");
            throw;
        }
    }
} 