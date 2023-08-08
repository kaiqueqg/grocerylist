using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GroceryList.Model
{
  public class GroceryListModel
  {
    public List<CategoryModel> categories { get; set; }
    public List<ItemModel> items { get; set; }

    public List<CategoryModel>? deletedCategories { get; set; }
    public List<ItemModel>? deletedItems { get; set; }
  }
}
