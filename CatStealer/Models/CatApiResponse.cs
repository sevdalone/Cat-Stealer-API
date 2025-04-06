namespace CatStealer.Models;

public class CatApiResponse
{
    public string Id { get; set; }
    public string Url { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<CatBreed> Breeds { get; set; } = new();
}