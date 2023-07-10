using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GroceryList.Model.MongoDB
{
	public class MongoDBCategoryModel
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; } = null!;

		[BsonElement("Text")]
		public string Text { get; set; } = null!;

		[BsonElement("IsOpen")]
		public bool? IsOpen { get; set; }
	}
}
