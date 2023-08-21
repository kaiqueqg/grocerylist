using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GroceryList.Model.MongoDB
{
  public class MongoDBUserModel
  {
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; } = null!;

    [BsonElement("Username")]
    public string Username { get; set; } = null!;

    [BsonElement("Password")]
    public string Password { get; set; } = null!;

    [BsonElement("UserPrefs")]
    public MongoDBUserPrefsModel? UserPrefs { get; set; }
  }
}
