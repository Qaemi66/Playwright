using Microsoft.Playwright;
using WebApi.Tests.E2E.BaseClasses;

namespace WebApi.Tests.E2E.ControllerTests;

[CollectionDefinition("TestController", DisableParallelization = false)]
public class ProductsControllerTests(CustomWebApplicationFactory<Startup> factory) : BaseTestController(factory)
{
	[Fact]
	public async Task GetProductById2_ShouldShowCorrectInfo()
	{
		await Factory.ResetDatabaseAsync();

		await page.GotoAsync($"{Factory.BaseUrl}/swagger/index.html");

		await page.GetByRole(AriaRole.Button, new() { Name = "GET /Products", Exact = true }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Try it out" }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Execute" }).ClickAsync();

		await Expect(page.GetByText("[ { \"id\": 1, \"name\": \"Test")).ToBeVisibleAsync();

		await page.GetByRole(AriaRole.Button, new() { Name = "GET /Products/{id}", Exact = true }).ClickAsync();
		await page.GetByRole(AriaRole.Button, new() { Name = "Try it out" }).ClickAsync();
		await page.GetByRole(AriaRole.Textbox, new() { Name = "id" }).FillAsync("1");
		await page.Locator("#operations-Products-get_Products__id_").GetByRole(AriaRole.Button, new() { Name = "Execute" }).ClickAsync();

		await Expect(page.GetByText("{ \"id\": 1, \"name\": \"Test Product\", \"quantity\": 11 }", new() { Exact = true })).ToBeVisibleAsync();
	}
}