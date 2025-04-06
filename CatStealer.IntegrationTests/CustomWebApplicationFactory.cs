using CatStealer.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace CatStealer.IntegrationTests;

public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("ConnectionStrings:DefaultConnection", 
            "Server=127.0.0.1,1433;Initial Catalog=cat.stealer-int;User Id=sa;Password=Password#23;MultipleActiveResultSets=False;Encrypt=False;TrustServerCertificate=True;Connection Timeout=30;");
        builder.ConfigureServices(services =>
        {
            // Mock the Cat API service
            services.AddScoped<ICatApiService, MockCatApiService>();
        });
    }
}