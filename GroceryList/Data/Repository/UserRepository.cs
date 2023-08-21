using DocumentFormat.OpenXml.Spreadsheet;
using GroceryList.Data.Services;
using GroceryList.Model;
using GroceryList.Model.MongoDB;
using MongoDB.Bson;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GroceryList.Data.Repository
{
	public class UserRepository
	{
    MongoDbService _mongoDbService;
    ILogger<UserRepository> _logger;

    public UserRepository(MongoDbService mongoDbService, ILogger<UserRepository> logger)
		{
      _mongoDbService = mongoDbService;
      _logger = logger;
		}

    private UserModel GetTempUser()
    {
      return new UserModel()
      {
        Id = ObjectId.GenerateNewId().ToString(),
        UserName = "test",
        Password = "test",
        UserPrefs = new UserPrefsModel()
        {
          HideQuantity = false,
          ShouldCreateNewItemWhenCreateNewCategory = false,
        }
      };
    }

		public async Task<LoginModel> UserLogin(string username, string password)
		{
      if(username != "test") return new LoginModel() { user = null, token = "", errorMessage = "Wrong username!" };
      if(password != "test") return new LoginModel() { user = null, token = "", errorMessage = "Wrong password!" };

      UserModel? user = (await _mongoDbService.GetUserAsync(Builders<MongoDBUserModel>.Filter.Eq("Username", username)))?.ToModel();
      if(user == null)
      {
        UserModel? newUser = (await _mongoDbService.InsertOneUserAsync(GetTempUser().FromModel()))?.ToModel();
        return new LoginModel(){ user = newUser };
      }
      else{
        return new LoginModel(){ user = user };
      }
		}

    public async Task<UserModel?> GetUserById(string userId)
    {
      return (await _mongoDbService.GetUserAsync(Builders<MongoDBUserModel>.Filter.Eq("Id", userId)))?.ToModel();
    }

    public async Task<UserPrefsModel?> GetUserPrefs(string userId)
    {
      MongoDBUserModel? u = (await _mongoDbService.GetUserAsync(Builders<MongoDBUserModel>.Filter.Eq("Id", userId)));

      return u?.UserPrefs?.ToModel();
    }

    public async Task<UserPrefsModel?> PatchUserPrefs(string userId, UserPrefsModel userPrefs)
    {
      UserModel? user = await GetUserById(userId);

      if(user != null)
      {
        user.UserPrefs = userPrefs;
        ReplaceOneResult result = await _mongoDbService.ReplaceOneUserAsync(user.FromModel());

        return result.ModifiedCount > 0? userPrefs : null;
      }

      return null;
    }
  }
}
