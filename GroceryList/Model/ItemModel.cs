using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GroceryList.Model
{
    public class ItemModel
    {
      [Key]
      [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
      public string? id { get; set; }
      public string text { get; set; }
      public int? quantity { get; set; }
      public string? quantityUnit { get; set; }
      public string? goodPrice { get; set; }
      public bool? isChecked { get; set; }

      [Required(ErrorMessage = "Item required a category id!")]
      public string myCategory { get; set; }
    }
}
