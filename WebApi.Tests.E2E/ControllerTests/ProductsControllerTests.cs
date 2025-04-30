using Microsoft.Playwright;
using WebApi.Tests.E2E.BaseClasses;

namespace WebApi.Tests.E2E.ControllerTests;

public class ProductsControllerTests : CustomWebApplicationFactory<Startup>
{
	
	[Fact]
	public async Task IncreaseProductQuantity_ShouldUpdateDatabase()
	{
		await ResetDatabaseAsync();

		var page = await BrowserContext.NewPageAsync();
		await page.GotoAsync("http://localhost:5000/swagger/index.html");
		await page.GetByRole(AriaRole.Button, new() { Name = "GET /Products", Exact = true }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Try it out" }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Execute" }).ClickAsync();
		await Expect(page.GetByText("[ { \"id\": 1, \"name\": \"Test")).ToBeVisibleAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "GET /Products/{id}", Exact = true }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Try it out" }).ClickAsync();
		await page.GetByRole(AriaRole.Textbox, new() { Name = "id" }).ClickAsync();
		await page.GetByRole(AriaRole.Textbox, new() { Name = "id" }).FillAsync("1");
		await page.Locator("#operations-Products-get_Products__id_").GetByRole(AriaRole.Button, new() { Name = "Execute" }).ClickAsync();
		await Expect(page.GetByText("{ \"id\": 1, \"name\": \"Test Product\", \"quantity\": 11 }", new() { Exact = true })).ToBeVisibleAsync();
	}
	
	[Fact]
	public async Task IncreaseProductQuantity_ShouldUpdateDatabase2()
	{
		await ResetDatabaseAsync();
	
		var page = await BrowserContext.NewPageAsync();
		await page.GotoAsync("http://localhost:5000/swagger/index.html");
		await page.GetByRole(AriaRole.Button, new() { Name = "GET /Products", Exact = true }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Try it out" }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Execute" }).ClickAsync();
		await Expect(page.GetByText("[ { \"id\": 1, \"name\": \"Test")).ToBeVisibleAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "GET /Products/{id}", Exact = true }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Try it out" }).ClickAsync();
		await page.GetByRole(AriaRole.Textbox, new() { Name = "id" }).ClickAsync();
		await page.GetByRole(AriaRole.Textbox, new() { Name = "id" }).FillAsync("1");
		await page.Locator("#operations-Products-get_Products__id_").GetByRole(AriaRole.Button, new() { Name = "Execute" }).ClickAsync();
		await Expect(page.GetByText("{ \"id\": 1, \"name\": \"Test Product\", \"quantity\": 11 }", new() { Exact = true })).ToBeVisibleAsync();
	}
} 