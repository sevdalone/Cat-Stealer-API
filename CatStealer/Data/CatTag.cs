namespace CatStealer.Data;

public class CatTag
{
    public int CatId { get; set; }
    public CatEntity Cat { get; set; }
        
    public int TagId { get; set; }
    public TagEntity Tag { get; set; }
}