using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Model
{
  public class CategoryModel
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public string? id { get; set; }
    public string text { get; set; }
    public bool? isOpen { get; set; }
  }
}
