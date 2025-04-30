using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Respawn;
using Testcontainers.MsSql;
using WebApi.Data;

namespace WebApi.Tests.E2E.BaseClasses;

public delegate void TestConfigureServices(IServiceCollection services);

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>, IAsyncLifetime
	where TStartup : class
{
	protected IBrowserContext BrowserContext { get; private set; } = null!;
	public TestConfigureServices? CustomConfigureServices { private get; init; }
	private IBrowser browser = null!;
	private MsSqlContainer? container;
	private string connectionString = null!;
	private Respawner respawner = null!;

	public async Task InitializeAsync()
	{
		await InitializeDbConnectionAsync();
		await InitializeRespawner();
		await InitializePlaywrightAsync();
	}

	private async Task InitializePlaywrightAsync()
	{
		PageTest tester = new PageTest();
		await tester.InitializeAsync();

		browser = await tester.Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = true,
		});

		BrowserContext = await browser.NewContextAsync();
	}

	public new async Task DisposeAsync()
	{
		await BrowserContext!.DisposeAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder
			.UseContentRoot(Directory.GetCurrentDirectory())
			.ConfigureAppConfiguration((webHostBuilder, configEnvironment) =>
			{
				builder.UseEnvironment("test");
				IWebHostEnvironment environment = webHostBuilder.HostingEnvironment;

				configEnvironment.SetBasePath(environment.ContentRootPath);
				configEnvironment.AddJsonFile("appSettings.test.json", true, true);
				configEnvironment.AddEnvironmentVariables();
			});
		builder.ConfigureServices((_, services) => CustomConfigureServices?.Invoke(services));
		base.ConfigureWebHost(builder);
	}


	protected async Task ResetDatabaseAsync() => await respawner.ResetAsync(connectionString);

	private async Task InitializeDbConnectionAsync()
	{
		// Create and start the container with more specific configuration
		container = new MsSqlBuilder()
			.WithImage("mcr.microsoft.com/mssql/server:2019-latest")
			.WithAutoRemove(true)
			.WithCleanUp(true)
			.Build();

		await container.StartAsync();

		// Get the connection string after container is started
		connectionString = container.GetConnectionString().Replace("master", "TestDb");

		// Create database and tables
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlServer(connectionString)
			.Options;

		await using var context = new AppDbContext(options);
		await context.Database.EnsureCreatedAsync();
	}

	private async Task InitializeRespawner()
	{
		respawner = await Respawner.CreateAsync(connectionString,
			new RespawnerOptions
			{
				DbAdapter = DbAdapter.SqlServer
			});
	}

	protected ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);

	protected IPageAssertions Expect(IPage page) => Assertions.Expect(page);

	protected IAPIResponseAssertions Expect(IAPIResponse response) => Assertions.Expect(response);
}
