using DocumentFormat.OpenXml.Office2010.Excel;
using GroceryList.Model;
using GroceryList.Model.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Runtime.CompilerServices;

namespace GroceryList.Data.Services
{
	public class MongoDbService
	{
		MongoClient? _mongoClient;
		IMongoDatabase _groceryListDatabase;
		private readonly IMongoCollection<MongoDBCategoryModel> _categoryCollection;
		private readonly IMongoCollection<MongoDBItemModel> _itemCollection;
		private readonly string _databaseName;
		private readonly string _categoryCollectionName;
		private readonly string _itemCollectionName;
    ILogger<MongoDbService> _logger;

		public MongoDbService(IConfiguration config, ILogger<MongoDbService> logger)
		{
			_databaseName = "GroceryListDatabase";
			_categoryCollectionName = "CategoryCollection";
			_itemCollectionName = "ItemCollection";
      _logger = logger;

			string connectionString = (string)Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");

      if(connectionString == "") 
      {
        _logger.LogError("Connection string empty!");
        _logger.LogError("MONGO_CONNECTION_STRING enviroment variable necessary!");
      }

			_mongoClient = new MongoClient(connectionString);

			_groceryListDatabase = _mongoClient.GetDatabase(_databaseName);
			_categoryCollection = _groceryListDatabase.GetCollection<MongoDBCategoryModel>(_categoryCollectionName);
			_itemCollection = _groceryListDatabase.GetCollection<MongoDBItemModel>(_itemCollectionName);
		}

#region Category

		public async Task<List<MongoDBCategoryModel>> GetCategoryAsync(FilterDefinition<MongoDBCategoryModel> f)
		{
			return await _categoryCollection.Find(f, new FindOptions(){MaxTime = TimeSpan.FromMilliseconds(300)}).ToListAsync();
		}
		public async Task<MongoDBCategoryModel?> GetCategoryAsync(string id)
		{
			return await _categoryCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
		}
		public async Task<MongoDBCategoryModel> InsertOneCategoryAsync(MongoDBCategoryModel o)
		{
			await _categoryCollection.InsertOneAsync(o);
			return o;
		}
		public async Task<ReplaceOneResult> ReplaceOneCategoryAsync(MongoDBCategoryModel c)
		{
			return await _categoryCollection.ReplaceOneAsync(x => x.Id == c.Id, c);
		}
		public async Task ReplaceManyCategoriesAsync(List<MongoDBCategoryModel> cs)
		{
			foreach(MongoDBCategoryModel c in cs)
			{
				await ReplaceOneCategoryAsync(c);
			}
		}
		public async Task DeleteOneCategoryAsync(string id)
		{
			await _categoryCollection.DeleteOneAsync(x => x.Id == id);
		}
		public async Task InsertManyCategoriesAsync(List<MongoDBCategoryModel> list)
		{
			await _categoryCollection.InsertManyAsync(list);
		}
#endregion

#region Item

		public async Task<List<MongoDBItemModel>> GetItemAsync(FilterDefinition<MongoDBItemModel> f)
		{
			return await _itemCollection.Find(f).ToListAsync();
		}
		public async Task<MongoDBItemModel?> GetItemAsync(string id)
		{
			return await _itemCollection.Find(x => x.Id == id).FirstOrDefaultAsync();
		}
		public async Task<MongoDBItemModel> InsertOneItemAsync(MongoDBItemModel i)
		{
			await _itemCollection.InsertOneAsync(i);
			return i;
		}
		public async Task InsertManyItemsAsync(List<MongoDBItemModel> list)
		{
			await _itemCollection.InsertManyAsync(list);
		}
		public async Task<ReplaceOneResult> ReplaceOneItemAsync(MongoDBItemModel i)
		{
			return await _itemCollection.ReplaceOneAsync(x => x.Id == i.Id, i);
		}
		public async Task<DeleteResult> DeleteOneItemAsync(MongoDBItemModel i)
		{
			return await _itemCollection.DeleteOneAsync(x => x.Id == i.Id);
		}
		public void DropCategoryCollection() { _groceryListDatabase.DropCollection(_categoryCollectionName); }
		public void DropItemCollection() { _groceryListDatabase.DropCollection(_itemCollectionName); }
#endregion
	}
}