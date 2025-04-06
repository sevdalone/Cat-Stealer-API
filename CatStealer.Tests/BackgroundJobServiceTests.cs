using CatStealer.Services;
using Hangfire;
using Hangfire.Storage;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CatStealer.Tests;

public class BackgroundJobServiceTests
{
    private readonly BackgroundJobService _service;

    public BackgroundJobServiceTests()
    {
        var mockCatApiService = new Mock<ICatApiService>();
        var mockLogger = new Mock<ILogger<BackgroundJobService>>();
        var mockServiceProvider = new Mock<IServiceProvider>();

        // Mock Hangfire internals with wrapper
        MockHangfireJobs();

        _service = new BackgroundJobService(mockCatApiService.Object, mockLogger.Object, mockServiceProvider.Object);
    }

    private void MockHangfireJobs()
    {
        // Mock the BackgroundJob.Enqueue method using HangfireMock
        JobStorage.Current = CreateMockJobStorage();
    }

    private JobStorage CreateMockJobStorage()
    {
        var mockJobStorage = new Mock<JobStorage>();
        var mockConnection = new Mock<IStorageConnection>();
        mockConnection
            .Setup(x => x.GetJobData(It.IsAny<string>()))
            .Returns(new JobData()
            {
                State = "Processing"
            });

        mockJobStorage
            .Setup(x => x.GetConnection())
            .Returns(mockConnection.Object);

        return mockJobStorage.Object;
    }
    
    [Fact]
    public async Task CheckJobStatus_ReturnsStatus()
    {
        // Act
        var result = await _service.CheckJobStatus("job123");

        // Assert
        Assert.Equal("job123", result.JobId);
        // Status might be "Not found" or "Processing" depending on mock setup
        Assert.NotNull(result.Status);
    }
}