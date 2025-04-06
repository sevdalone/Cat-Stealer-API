using CatStealer.Controllers;
using CatStealer.Data;
using CatStealer.Models;
using CatStealer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CatStealer.Tests;

public class CatsControllerTests
{
    private readonly Mock<IBackgroundJobService> _mockBackgroundJobService;
    private readonly CatDbContext _dbContext;
    private readonly CatsController _controller;

    public CatsControllerTests()
    {
        // Setup mock services
        _mockBackgroundJobService = new Mock<IBackgroundJobService>();
        var mockLogger = new Mock<ILogger<CatsController>>();

        // Setup in-memory database
        var options = new DbContextOptionsBuilder<CatDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new CatDbContext(options);

        // Seed database with test data
        SeedDatabase();

        // Setup URL helper for controller
        var mockUrlHelper = new Mock<IUrlHelper>();
        mockUrlHelper
            .Setup(x => x.Action(It.IsAny<UrlActionContext>()))
            .Returns("https://localhost:5001/api/cats/1/image");

        // Create controller
        _controller = new CatsController(_dbContext, _mockBackgroundJobService.Object, mockLogger.Object);
        _controller.Url = mockUrlHelper.Object;

        // Setup controller context
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Scheme = "https";
        httpContext.Request.Host = new HostString("localhost:5001");
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    private void SeedDatabase()
    {
        // Create test tags
        var playfulTag = new TagEntity { Id = 1, Name = "Playful", Created = DateTime.UtcNow };
        var friendlyTag = new TagEntity { Id = 2, Name = "Friendly", Created = DateTime.UtcNow };
        _dbContext.Tags.AddRange(playfulTag, friendlyTag);
        _dbContext.SaveChanges();

        // Create test cats
        var cats = new List<CatEntity>
        {
            new()
            {
                Id = 1,
                CatId = "cat1",
                Width = 500,
                Height = 400,
                Image = [1, 2, 3],
                Created = DateTime.UtcNow
            },
            new()
            {
                Id = 2,
                CatId = "cat2",
                Width = 600,
                Height = 500,
                Image = [4, 5, 6],
                Created = DateTime.UtcNow
            }
        };
        _dbContext.Cats.AddRange(cats);
        _dbContext.SaveChanges();

        // Create cat-tag relationships
        var catTags = new List<CatTag>
        {
            new() { CatId = 1, TagId = 1 },
            new() { CatId = 1, TagId = 2 },
            new() { CatId = 2, TagId = 2 }
        };
        _dbContext.Set<CatTag>().AddRange(catTags);
        _dbContext.SaveChanges();
    }

    [Fact]
    public async Task GetCats_ReturnsPagedResponse()
    {
        // Act
        var result = await _controller.GetCats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedResponse = Assert.IsType<PagedResponse<CatDto>>(okResult.Value);
            
        Assert.Equal(2, pagedResponse.Items.Count);
        Assert.Equal(1, pagedResponse.PageNumber);
        Assert.Equal(10, pagedResponse.PageSize);
        Assert.Equal(2, pagedResponse.TotalCount);
        Assert.Equal(1, pagedResponse.TotalPages);
    }

    [Fact]
    public async Task GetCats_WithTag_ReturnsFilteredResults()
    {
        // Act
        var result = await _controller.GetCats(tag: "Playful");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var pagedResponse = Assert.IsType<PagedResponse<CatDto>>(okResult.Value);
            
        Assert.Single(pagedResponse.Items);
        Assert.Equal("cat1", pagedResponse.Items[0].CatId);
        Assert.Contains("Playful", pagedResponse.Items[0].Tags);
    }

    [Fact]
    public async Task GetCat_WithValidId_ReturnsCat()
    {
        // Act
        var result = await _controller.GetCat(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var catDto = Assert.IsType<CatDto>(okResult.Value);
            
        Assert.Equal(1, catDto.Id);
        Assert.Equal("cat1", catDto.CatId);
        Assert.Equal(500, catDto.Width);
        Assert.Equal(400, catDto.Height);
        Assert.Equal(2, catDto.Tags.Count);
        Assert.Contains("Playful", catDto.Tags);
        Assert.Contains("Friendly", catDto.Tags);
    }

    [Fact]
    public async Task GetCat_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetCat(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }

    [Fact]
    public async Task GetCatImage_WithValidId_ReturnsFileResult()
    {
        // Act
        var result = await _controller.GetCatImage(1);

        // Assert
        var fileResult = Assert.IsType<FileContentResult>(result);
        Assert.Equal("image/jpeg", fileResult.ContentType);
        Assert.Equal(new byte[] { 1, 2, 3 }, fileResult.FileContents);
    }

    [Fact]
    public async Task GetCatImage_WithInvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetCatImage(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public void FetchCats_ReturnsJobResponse()
    {
        // Arrange
        _mockBackgroundJobService
            .Setup(s => s.QueueCatsFetch())
            .Returns("job123");

        // Act
        var result = _controller.FetchCats();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var jobResponse = Assert.IsType<JobResponse>(okResult.Value);
            
        Assert.Equal("job123", jobResponse.JobId);
        Assert.Equal("Queued", jobResponse.Status);
    }
}