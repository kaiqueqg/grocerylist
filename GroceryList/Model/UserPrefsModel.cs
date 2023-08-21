using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GroceryList.Model
{
  public class UserPrefsModel
  {
    public bool ShouldCreateNewItemWhenCreateNewCategory { get; set; }
    public bool HideQuantity { get; set; }
  }
}
