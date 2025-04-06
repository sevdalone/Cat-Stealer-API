namespace CatStealer.Models;

public class CatDto
{
    public int Id { get; set; }
    public string CatId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public string ImageUrl { get; set; } // URL to fetch the image from our API
    public DateTime Created { get; set; }
    public List<string> Tags { get; set; } = new();
}