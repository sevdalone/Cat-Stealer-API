using CatStealer.Models;
using CatStealer.Services;

namespace CatStealer.IntegrationTests;

public class MockCatApiService : ICatApiService
{
    public Task<List<CatApiResponse>> GetCatsWithBreeds(int limit = 25)
    {
        var cats = new List<CatApiResponse>
        {
            new()
            {
                Id = "cat3",
                Url = "https://example.com/cat3.jpg",
                Width = 700,
                Height = 600,
                Breeds =
                [
                    new()
                    {
                        Id = "breed1",
                        Name = "Persian",
                        Temperament = "Playful, Curious"
                    }
                ]
            }
        };

        return Task.FromResult(cats);
    }

    public Task<byte[]> DownloadImage(string url)
    {
        return Task.FromResult(new byte[] { 7, 8, 9 });
    }
}