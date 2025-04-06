using System.Net;
using System.Text.Json;
using CatStealer.Models;
using CatStealer.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;
using Xunit;

namespace CatStealer.Tests;

public class CatApiServiceTests
{
    private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
    private readonly CatApiService _service;

    public CatApiServiceTests()
    {
        _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
        var httpClient = new HttpClient(_mockHttpMessageHandler.Object);
        httpClient.BaseAddress = new Uri("https://example.com/");
        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(httpClient);
        
        var mockConfig = new Mock<IConfiguration>();
        mockConfig.Setup(c => c["CatApi:ApiKey"]).Returns("test-api-key");

        _service = new CatApiService(httpClientFactory.Object);
    }

    [Fact]
    public async Task GetCatsWithBreeds_ReturnsCatList()
    {
        // Arrange
        var mockResponse = new List<CatApiResponse>
        {
            new()
            {
                Id = "cat1",
                Url = "https://example.com/cat1.jpg",
                Width = 500,
                Height = 400,
                Breeds =
                [
                    new CatBreed
                    {
                        Id = "breed1",
                        Name = "Persian",
                        Temperament = "Playful, Friendly"
                    }
                ]
            }
        };

        var json = JsonSerializer.Serialize(mockResponse);
            
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(json)
            });

        // Act
        var result = await _service.GetCatsWithBreeds();

        // Assert
        Assert.Single(result);
        Assert.Equal("cat1", result[0].Id);
        Assert.Equal(500, result[0].Width);
        Assert.Equal(400, result[0].Height);
        Assert.Equal("Persian", result[0].Breeds[0].Name);
        Assert.Equal("Playful, Friendly", result[0].Breeds[0].Temperament);
    }

    [Fact]
    public async Task DownloadImage_ReturnsImageBytes()
    {
        // Arrange
        var imageBytes = new byte[] { 1, 2, 3 };
            
        _mockHttpMessageHandler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new ByteArrayContent(imageBytes)
            });

        // Act
        var result = await _service.DownloadImage("https://example.com/cat.jpg");

        // Assert
        Assert.Equal(imageBytes, result);
    }
}