using Microsoft.Playwright;

namespace WebApi.Tests.E2E.BaseClasses;

[CollectionDefinition("TestController", DisableParallelization = false)]
public class BaseTestController : IClassFixture<CustomWebApplicationFactory<Startup>>
{
	protected readonly CustomWebApplicationFactory<Startup> Factory;
	protected readonly IPage page;

	public BaseTestController(CustomWebApplicationFactory<Startup> factory)
	{
		Factory = factory;
		page = Factory.Page;
	}

	protected ILocatorAssertions Expect(ILocator locator) => Assertions.Expect(locator);

	protected IPageAssertions Expect(IPage page) => Assertions.Expect(page);

	protected IAPIResponseAssertions Expect(IAPIResponse response) => Assertions.Expect(response);
}
