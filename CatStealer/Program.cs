using CatStealer.Data;
using CatStealer.Services;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("CatStealer.Tests")]
[assembly:InternalsVisibleTo("CatStealer.IntegrationTests")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add DbContext
builder.Services.AddDbContext<CatDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add HttpClient
builder.Services.AddHttpClient<ICatApiService, CatApiService>("CatApi", client =>
{
    client.BaseAddress = new Uri("https://api.thecatapi.com/v1/");
    client.DefaultRequestHeaders.Add("x-api-key", builder.Configuration["CatApiKey"]);
});

// Add Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
builder.Services.AddHangfireServer();

// Register services
builder.Services.AddScoped<ICatApiService, CatApiService>();
builder.Services.AddScoped<IBackgroundJobService, BackgroundJobService>();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Cat Stealer API",
        Version = "v1",
        Description = "An API to 'steal' and manage cat images from the Cat API"
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

// Configure Hangfire dashboard
app.UseHangfireDashboard();

app.MapControllers();

app.Run();

public partial class Program { }