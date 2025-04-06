using System.Text.Json;
using CatStealer.Models;

namespace CatStealer.Services;

public interface ICatApiService
{
    Task<List<CatApiResponse>> GetCatsWithBreeds(int limit = 25);
    Task<byte[]> DownloadImage(string url);
}

public class CatApiService : ICatApiService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CatApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<List<CatApiResponse>> GetCatsWithBreeds(int limit = 25)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("CatApi");
        var response = await httpClient.GetAsync($"images/search?limit={limit}&has_breeds=1");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var cats = JsonSerializer.Deserialize<List<CatApiResponse>>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        return cats;
    }

    public async Task<byte[]> DownloadImage(string url)
    {
        HttpClient httpClient = _httpClientFactory.CreateClient("CatApi");
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsByteArrayAsync();
    }
}