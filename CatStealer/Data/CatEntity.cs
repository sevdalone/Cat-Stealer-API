using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatStealer.Data;

public class CatEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
        
    [Required]
    public string CatId { get; set; }
        
    public int Width { get; set; }
        
    public int Height { get; set; }
        
    [Required]
    public byte[] Image { get; set; }
        
    public DateTime Created { get; set; } = DateTime.UtcNow;
        
    public virtual ICollection<CatTag> CatTags { get; set; } = new List<CatTag>();
}