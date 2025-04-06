using System.Net;
using System.Net.Http.Json;
using CatStealer.Data;
using CatStealer.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace CatStealer.IntegrationTests;

public class CatsApiIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly CustomWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CatsApiIntegrationTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        ResetDatabase();
    }

    [Fact]
    public async Task GetCats_ReturnsSuccessAndCorrectContentType()
    {
        // Act
        var response = await _client.GetAsync("/api/cats");

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json; charset=utf-8",
            response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task GetCats_ReturnsExpectedCats()
    {
        // Act
        var response = await _client.GetAsync("/api/cats");
        var content = await response.Content.ReadFromJsonAsync<PagedResponse<CatDto>>();

        // Assert
        Assert.NotNull(content);
        Assert.Equal(2, content.Items.Count);
        Assert.Equal(1, content.PageNumber);
        Assert.Equal(10, content.PageSize);
    }

    [Fact]
    public async Task GetCats_WithPaging_ReturnsCorrectPage()
    {
        // Act
        var response = await _client.GetAsync("/api/cats?page=1&pageSize=1");
        var content = await response.Content.ReadFromJsonAsync<PagedResponse<CatDto>>();

        // Assert
        Assert.NotNull(content);
        Assert.Single(content.Items);
        Assert.Equal(1, content.PageNumber);
        Assert.Equal(1, content.PageSize);
        Assert.Equal(2, content.TotalCount);
        Assert.Equal(2, content.TotalPages);
    }

    [Fact]
    public async Task GetCats_WithTag_ReturnsFilteredResults()
    {
        // Act
        var response = await _client.GetAsync("/api/cats?tag=Playful");
        var content = await response.Content.ReadFromJsonAsync<PagedResponse<CatDto>>();

        // Assert
        Assert.NotNull(content);
        Assert.Single(content.Items);
        Assert.Contains("Playful", content.Items[0].Tags);
    }

    [Fact]
    public async Task GetCat_WithValidId_ReturnsCat()
    {
        // Act
        var response = await _client.GetAsync("/api/cats/1");
        var content = await response.Content.ReadFromJsonAsync<CatDto>();

        // Assert
        Assert.NotNull(content);
        Assert.Equal(1, content.Id);
        Assert.Equal("cat1", content.CatId);
        Assert.Equal(500, content.Width);
        Assert.Equal(400, content.Height);
        Assert.Equal(2, content.Tags.Count);
    }

    [Fact]
    public async Task GetCat_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync("/api/cats/999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task FetchCats_ReturnsJobResponse()
    {
        // Act
        var response = await _client.PostAsync("/api/cats/fetch", null);
        var content = await response.Content.ReadFromJsonAsync<JobResponse>();

        // Assert
        Assert.NotNull(content);
        Assert.NotNull(content.JobId);
        Assert.Equal("Queued", content.Status);
    }


    private void ResetDatabase()
    {
        // Create a scope to get scoped services
        using (var scope = _factory.Services.CreateScope())
        {
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<CatDbContext>();
            // Ensure the database is dropped/created
            db.Database.EnsureDeleted();
            db.Database.EnsureCreated();
            SeedDatabase(db);
        }
    }

    private void SeedDatabase(CatDbContext dbContext)
    {
        // Create test tags
        var playfulTag = new TagEntity { Name = "Playful", Created = DateTime.UtcNow };
        var friendlyTag = new TagEntity { Name = "Friendly", Created = DateTime.UtcNow };
        dbContext.Tags.AddRange(playfulTag, friendlyTag);
        dbContext.SaveChanges();

        // Create test cats
        var cats = new List<CatEntity>
        {
            new()
            {
                CatId = "cat1",
                Width = 500,
                Height = 400,
                Image = [1, 2, 3],
                Created = DateTime.UtcNow
            },
            new()
            {
                CatId = "cat2",
                Width = 600,
                Height = 500,
                Image = [4, 5, 6],
                Created = DateTime.UtcNow
            }
        };
        dbContext.Cats.AddRange(cats);
        dbContext.SaveChanges();

        // Create cat-tag relationships
        var catTags = new List<CatTag>
        {
            new() { CatId = 1, TagId = 1 },
            new() { CatId = 1, TagId = 2 },
            new() { CatId = 2, TagId = 2 }
        };
        dbContext.Set<CatTag>().AddRange(catTags);
        dbContext.SaveChanges();
    }
}