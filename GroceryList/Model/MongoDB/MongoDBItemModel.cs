using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace GroceryList.Model.MongoDB
{
	public class MongoDBItemModel
	{
		[BsonId]
		[BsonRepresentation(BsonType.ObjectId)]
		public string? Id { get; set; } = null!;

		[BsonElement("Text")]
		public string Text { get; set; } = null!;

		[BsonElement("IsChecked")]
		public bool? IsChecked { get; set; }

    [BsonElement("Quantity")]
    public int? Quantity { get; set; }

    [BsonElement("QuantityUnit")]
    public string? QuantityUnit { get; set; }

    [BsonElement("GoodPrice")]
    public string? GoodPrice { get; set; }

    [BsonElement("MyCategory")]
		public string MyCategory { get; set; } = null!;
	}
}
