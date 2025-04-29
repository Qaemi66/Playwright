using Microsoft.EntityFrameworkCore;
using WebApi.Data;

namespace WebApi;

public class Startup(IConfiguration configuration)
{
	public void ConfigureServices(IServiceCollection services)
	{
		// Add services to the container.
		services.AddControllers();
		services.AddEndpointsApiExplorer();
		services.AddSwaggerGen();

		// Configure DbContext
		services.AddDbContext<AppDbContext>(options =>
			options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		// Configure the HTTP request pipeline.
		if (env.IsDevelopment())
		{
			app.UseSwagger();
			app.UseSwaggerUI();
		}

		app.UseHttpsRedirection();
		
		app.UseRouting();
		
		// Add health check endpoint
		app.UseEndpoints(endpoints =>
		{
			endpoints.Map("/health",
				() => Results.Ok(new
				{
					status = "healthy"
				}));

			endpoints.MapControllers();
		});
	}
}
