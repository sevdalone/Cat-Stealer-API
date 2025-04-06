using CatStealer.Controllers;
using CatStealer.Models;
using CatStealer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CatStealer.Tests;

public class JobsControllerTests
{
    private readonly Mock<IBackgroundJobService> _mockBackgroundJobService;
    private readonly JobsController _controller;

    public JobsControllerTests()
    {
        _mockBackgroundJobService = new Mock<IBackgroundJobService>();
        var mockLogger = new Mock<ILogger<JobsController>>();
        _controller = new JobsController(_mockBackgroundJobService.Object, mockLogger.Object);
    }

    [Fact]
    public async Task GetJobStatus_WithValidId_ReturnsJobResponse()
    {
        // Arrange
        _mockBackgroundJobService
            .Setup(s => s.CheckJobStatus("job123"))
            .ReturnsAsync(new JobResponse { JobId = "job123", Status = "Processing" });

        // Act
        var result = await _controller.GetJobStatus("job123");

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var jobResponse = Assert.IsType<JobResponse>(okResult.Value);
            
        Assert.Equal("job123", jobResponse.JobId);
        Assert.Equal("Processing", jobResponse.Status);
    }

    [Fact]
    public async Task GetJobStatus_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockBackgroundJobService
            .Setup(s => s.CheckJobStatus("nonexistent"))
            .ReturnsAsync(new JobResponse { JobId = "nonexistent", Status = "Not found" });

        // Act
        var result = await _controller.GetJobStatus("nonexistent");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
    }
}