using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CatStealer.Data;

public class TagEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
        
    [Required]
    [MaxLength(50)]
    public string Name { get; set; }
        
    public DateTime Created { get; set; } = DateTime.UtcNow;
        
    public virtual ICollection<CatTag> CatTags { get; set; } = new List<CatTag>();
}