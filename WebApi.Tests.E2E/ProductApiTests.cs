using FluentAssertions;
using Microsoft.Playwright;
using WebApi.Data;
using Microsoft.EntityFrameworkCore;
using WebApi.Models;

namespace WebApi.Tests.E2E;

public class ProductApiTests(SqlContainerFixture fixture) : IClassFixture<SqlContainerFixture>, IAsyncLifetime
{
	private WebApiFactory _factory;
	private HttpClient _client;
	private const string BaseUrl = "http://localhost:5000";

	public async Task InitializeAsync()
	{
		_factory = new WebApiFactory(fixture.ConnectionString);
		_client = _factory.CreateClient();
		
		// Wait for the server to be ready
		var maxAttempts = 10;
		var attempt = 0;
		while (attempt < maxAttempts)
		{
			try
			{
				var response = await _client.GetAsync("/health");
				if (response.IsSuccessStatusCode)
				{
					Console.WriteLine("Server is ready!");
					return;
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Attempt {attempt + 1}: {ex.Message}");
			}
			
			await Task.Delay(1000); // Wait 1 second before retrying
			attempt++;
		}
		
		throw new Exception("Server failed to start within the expected time");
	}

	public async Task DisposeAsync()
	{
		_client.Dispose();
		await _factory.DisposeAsync();
	}
	
	[Fact]
	public async Task IncreaseProductQuantity_ShouldUpdateDatabase()
	{
		// Arrange
		var options = new DbContextOptionsBuilder<AppDbContext>()
			.UseSqlServer(fixture.ConnectionString)
			.Options;

		// Create initial product
		await using var context = new AppDbContext(options);

		context.Products.Add(new Product
		{
			Name = "Test Product",
			Quantity = 1
		});
		await context.SaveChangesAsync();

		// Act
		using var playwright = await Playwright.CreateAsync();
		var request = await playwright.APIRequest.NewContextAsync(new APIRequestNewContextOptions
		{
			BaseURL = BaseUrl,
			IgnoreHTTPSErrors = true
		});

		var response = await request.PostAsync("/products/1/increase");
		response.Status.Should().Be(200);

		// Assert
		var product = await context.Products.FirstAsync();
		product.Quantity.Should().Be(2);

		// Reset database for next test
		await fixture.ResetDbAsync();
	}
} 