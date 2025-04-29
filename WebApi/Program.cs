namespace WebApi;

public static class Program
{
	static void Main(string[]? args)
	{
		HostBuilder hostBuilder = new();

		hostBuilder.UseContentRoot(Directory.GetCurrentDirectory());
		hostBuilder.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); })
			.ConfigureHostConfiguration(config =>
			{
				config.AddEnvironmentVariables("DOTNET_");
				if (args != null)
				{
					config.AddCommandLine(args);
				}
			});
		hostBuilder.ConfigureAppConfiguration((hostingContext, config) =>
				{
					IHostEnvironment env = hostingContext.HostingEnvironment;

					bool reloadOnChange =
						hostingContext.Configuration.GetValue("hostBuilder:reloadConfigOnChange", true);
					config.AddJsonFile("appsettings.json", true, reloadOnChange)
						.AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, reloadOnChange);

					//config.AddUserSecrets<TAppSecrets>();

					config.AddEnvironmentVariables();
				}
			)
			.UseDefaultServiceProvider((context, options) =>
			{
				bool isDevelopment = context.HostingEnvironment.IsDevelopment();
				bool isStaging = context.HostingEnvironment.IsStaging();
				options.ValidateScopes = isDevelopment || isStaging;
				options.ValidateOnBuild = isDevelopment || isStaging;
			});
		//.UseMetrics();

		hostBuilder.Build().Run();
	}
}
