using System.Net;
using System.Net.Sockets;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Playwright;
using Microsoft.Playwright.Xunit;
using Respawn;
using Testcontainers.MsSql;
using WebApi.Data;

namespace WebApi.Tests.E2E.BaseClasses;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>, IAsyncLifetime
	where TStartup : class
{
	protected IBrowserContext BrowserContext { get; set; } = null!;
	public string BaseUrl => $"http://localhost:{_port}";
	public IPage Page { get; private set; } = null!;
	//public TestConfigureServices? CustomConfigureServices { private get; init; }

	private IBrowser _browser = null!;
	private int _port;
	private MsSqlContainer? _container;
	private string _connectionString = null!;
	private Respawner _respawner = null!;

	public async Task InitializeAsync()
	{
		_port = GetAvailablePort();
		await StartDatabaseAsync();
		await InitRespawnerAsync();
		await InitPlaywrightAsync();
		await StartServerAsync();
	}

	public new async Task DisposeAsync()
	{
		if (_host is not null)
			await _host.StopAsync();
		await BrowserContext.DisposeAsync();
		await _browser.DisposeAsync();
		if (_container is not null)
			await _container.DisposeAsync();
	}

	protected override void ConfigureWebHost(IWebHostBuilder builder)
	{
		builder
			.UseKestrel()
			.UseUrls(BaseUrl)
			.UseEnvironment("test")
			.UseContentRoot(Directory.GetCurrentDirectory())
			.ConfigureAppConfiguration((context, config) =>
			{
				var env = context.HostingEnvironment;
				config.SetBasePath(env.ContentRootPath)
					.AddJsonFile("appSettings.test.json", optional: true)
					.AddEnvironmentVariables();
			});

		base.ConfigureWebHost(builder);
	}

	public async Task ResetDatabaseAsync() => await _respawner.ResetAsync(_connectionString);

	private async Task StartDatabaseAsync()
	{
		_container = new MsSqlBuilder()
			.WithImage("mcr.microsoft.com/mssql/server:2019-latest")
			.WithAutoRemove(true)
			.WithCleanUp(true)
			.Build();

		await _container.StartAsync();

		_connectionString = _container.GetConnectionString().Replace("master", "TestDb");

		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlServer(_connectionString)
			.Options;

		await using var context = new AppDbContext(options);
		await context.Database.EnsureCreatedAsync();
	}

	private async Task InitRespawnerAsync()
	{
		_respawner = await Respawner.CreateAsync(_connectionString,
			new RespawnerOptions
			{
				DbAdapter = DbAdapter.SqlServer
			});
	}

	private async Task InitPlaywrightAsync()
	{
		var playwright = await Playwright.CreateAsync();
		_browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
		{
			Headless = false
		});
		BrowserContext = await _browser.NewContextAsync();
		Page = await BrowserContext.NewPageAsync();
	}

	private IHost? _host;
	
	private async Task StartServerAsync()
	{
		_host = Host.CreateDefaultBuilder()
			.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<TStartup>();
				webBuilder.UseUrls(BaseUrl);
				webBuilder.UseEnvironment("test");
			})
			.Build();

		await _host.StartAsync();
	}

	private static int GetAvailablePort()
	{
		using var listener = new TcpListener(IPAddress.Loopback, 0);
		listener.Start();
		var port = ((IPEndPoint) listener.LocalEndpoint).Port;
		listener.Stop();
		return port;
	}
}
