using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GroceryList.Model.MongoDB
{
  public class MongoDBUserPrefsModel
  {
    [BsonElement("ShouldCreateNewItemWhenCreateNewCategory")]
    public bool ShouldCreateNewItemWhenCreateNewCategory { get; set; }
    [BsonElement("HideQuantity")]
    public bool HideQuantity { get; set; }
  }
}
