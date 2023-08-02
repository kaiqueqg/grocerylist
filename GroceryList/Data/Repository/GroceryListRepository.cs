using Amazon.Runtime.Internal.Util;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using DocumentFormat.OpenXml.Packaging;
using GroceryList.Data.Caching;
using GroceryList.Data.Services;
using GroceryList.Model;
using GroceryList.Model.MongoDB;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Identity.Client;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace GroceryList.Data.Repository
{
  public class GroceryListRepository
  {
    string path = Directory.GetCurrentDirectory() + "/data.json";
    SqlServerContext _sqlServerContext;
    readonly ICachingService _cache;
    MongoDbService _mongoDbService;
    ILogger<GroceryListRepository> _logger;

    

    public GroceryListRepository(
            SqlServerContext sqlServerContext,
            ICachingService cache,
            MongoDbService mongoDbService,
            ILogger<GroceryListRepository> logger)
    {
      _sqlServerContext = sqlServerContext;
      _cache = cache;
      _mongoDbService = mongoDbService;
      _logger = logger;
    }

    #region Repository:UNITY
    public async Task<GroceryListModel?> SaveGroceryList(GroceryListModel model)
    {
      try {
        //^ CATEGORY
        List<CategoryModel>? dbCategoryList = await GetCategoryList();
        foreach(CategoryModel c in model.categories)
        {
          if(dbCategoryList != null)
          {
            CategoryModel? existingCategory = dbCategoryList.Where(x => x.text == c.text).FirstOrDefault();
            if(existingCategory != null)
            {
              c.id = existingCategory.id;
            }
          }

          if(c.id == "")
            await _mongoDbService.InsertOneCategoryAsync(c.FromModel());
          else
            await _mongoDbService.ReplaceOneCategoryAsync(c.FromModel());
        }

        //^ ITEM
        dbCategoryList = await GetCategoryList();

        foreach(ItemModel i in model.items)
        {
          if(dbCategoryList != null)
          {
            //^ first, there's a category for this item?
            CategoryModel? existingCategory = dbCategoryList.Where(x => x.id == i.myCategory).FirstOrDefault();

            //^ The myCategory id doesn't return anyone, but it could be because is the text of a category, not the category.id...
            if(existingCategory == null) 
              existingCategory = dbCategoryList.Where(x => x.text == i.myCategory).FirstOrDefault();
            
            //^ So, did we found a category for this item
            if(existingCategory != null) 
            {
              //^ update current category id from db
              i.myCategory = existingCategory.id; 
              List<ItemModel>? itemsInCategory = await GetItemListInCategory(existingCategory.id);

              //^ new item from app
              if(i.id == "") 
              {
                //^ Make sure that there isn't a item with the same text...If there's is, update it.
                ItemModel? existingSameNameItem = itemsInCategory.Where(x=>x.text == i.text).FirstOrDefault();
  
                if(existingSameNameItem == null)
                {
                  await PutItemByMongoDb(i);
                }
                else
                {
                  i.id = existingCategory.id;
                  await PatchItemByMongoDb(i);
                }
              }
              //^ existing item in db
              else
              {
                await PatchItemByMongoDb(i);
              }
            }
            else{} //! warm someone somehow about an item without category
          }
        }
        

        if(dbCategoryList != null)
        {
          List<ItemModel> dbItemsList = new List<ItemModel>();
          foreach(CategoryModel c in dbCategoryList)
          {
            List<ItemModel> possibleItems = await GetItemListInCategory(c.id);
            if(possibleItems != null) dbItemsList.AddRange(possibleItems);
          }

          return new GroceryListModel() { categories = dbCategoryList, items = dbItemsList };
        }
        else return null;
      } catch { return null; }
    }
    public async Task<GroceryListModel> GetGroceryList()
    {
      GroceryListModel model = new GroceryListModel();

      model.categories = await GetCategoryListByMongoDb();
      model.items = new List<ItemModel>();

      foreach(CategoryModel c in model.categories)
      {
        List<ItemModel> items = await GetItemListInCategory(c.id);
        if(items != null) model.items.AddRange(items);
      }

      return model;
    }
    #endregion

    #region Repository:Category
    public async Task<CategoryModel?> GetCategory(string id)
    {
      return await GetCategoryByMongoDb(id);
    }
    public async Task<List<CategoryModel>?> GetCategoryList()
    {
      try
      {
        List<CategoryModel>? rtnList;

        //GET CACHE
        rtnList = await GetCategoryListByRedis();
        if (rtnList != null) { return rtnList; }

        //MONGO
        rtnList = await GetCategoryListByMongoDb();

        //SET CACHE
        SetCategoryListByRedis(rtnList);
        return rtnList;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
        return null;
      }
    }
    public async Task<CategoryModel?> DoesCategoryAlreadyExist(CategoryModel c)
    {
      List<CategoryModel>? categories;

      _cache.Wait();

      try
      {
        categories = await GetCategoryListByMongoDb();
        foreach (CategoryModel category in categories)
        {
          if(category.text == c.text && category.id != c.id)
            return category;
        }

        return null;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
        return null;
      }

      finally { _cache.Release(); }
    }
    public async Task<CategoryModel?> PutCategory(CategoryModel c)
    {
      try
      {
        CategoryModel rtnCategory = await PutCategoryByMongoDb(c);
        CategoryWasChanged(c);
        return rtnCategory;
      }
      catch (Exception ex)
      { 
        _logger.LogError(ex.Message); 
        return null;
      }
    }
    public async Task<CategoryModel?> PatchCategory(CategoryModel c)
    {
      try
      {
        CategoryModel? rtnCategory = await PatchCategoryByMongoDb(c);
        if(rtnCategory != null)
          CategoryWasChanged(c);

        return rtnCategory;
      }
      catch (Exception ex) 
      { 
        _logger.LogError(ex.Message); 
        return null;
      }
    }
    public async Task<bool> DeleteCategory(CategoryModel c)
    {
      try
      {
        CategoryModel? rtnCategory = await DeleteCategoryByMongoDb(c);
        CategoryWasChanged(c);
        return true;
      }
      catch (Exception ex) 
      { 
        _logger.LogError(ex.Message);
        return false;
      }
    }

    // public void ChangeDisplayAllCategories(bool value)
    // {
    //   ChangeDisplayAllCategoriesByMongoDb(value);
    // }
    #endregion

    #region Repository:Item
    public async Task<ItemModel?> GetItem(string id)
    {
      return await GetItemByMongoDb(id);
    }
    public async Task<List<ItemModel>?> GetItemListInCategory(string categoryId)
    {
      try
      {
        List<ItemModel>? rtnList = null;

        //GET CACHE
        //rtnList = await GetItemListInCategoryByRedis(categoryId);
        if (rtnList != null) { return rtnList; }

        //MONGO
        rtnList = await GetItemListInCategoryByMongoDb(categoryId);

        //SET CACHE
        SetItemListInCategoryByRedis(categoryId, rtnList);
        
        return rtnList;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
        return null;
      }
    }
    public async Task<ItemModel?> PutItem(ItemModel i)
    {
      try
      {
        ItemModel? rtnItem = await PutItemByMongoDb(i);
        ItemWasChanged(i);

        return rtnItem;
      }
      catch (Exception ex) 
      { 
        _logger.LogError(ex.Message); 
        return null;
      }
    }
    public async Task<ItemModel?> DoesItemWithSameNameAlreadyExist(ItemModel item)
    {
      List<ItemModel>? rtnList;

      try
      {
        rtnList = await GetItemListInCategoryByMongoDb(item.myCategory);
        foreach (ItemModel i in rtnList)
        {
          if(i.text == item.text && i.id != item.id)//! same item
            return i;
        }

        return null;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
        return null;
      }
    }
    public async Task<ItemModel?> PatchItem(ItemModel i)
    {
      try
      {
        ItemModel? rtnItem = await PatchItemByMongoDb(i);

        if(rtnItem != null)
          ItemWasChanged(i);

        return rtnItem;
      }
      catch (Exception ex) 
      { 
        _logger.LogError(ex.Message);
        return null;
        }

    }
    public async Task<bool> DeleteItem(ItemModel i)
    {
      try 
      {
        await DeleteItemByMongoDb(i);
        ItemWasChanged(i);
        return true;
      } 
      catch (Exception ex) 
      {
        _logger.LogError(ex.Message);
        return false;
      }
    }
    #endregion

    #region Redis:Category
    private void CategoryWasChanged(CategoryModel category)
    {
      _logger.LogDebug("[REDIS] CategoryWasChanged");
      _cache.DeleteAsync("categorylist");
    }
    private async Task<List<CategoryModel>?> GetCategoryListByRedis()
    {
      try
      {
        _cache.Wait();

        string? cacheList = await _cache.GetAsync("categorylist");
        if (cacheList != null)
        {
          _logger.LogDebug("[REDIS] GetCategoryListByRedis");
          return JsonConvert.DeserializeObject<List<CategoryModel>>(cacheList);
        }
  
        return null;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
        return null;
      }
      finally
      {
        _cache.Release();
      }
    }
    private async void SetCategoryListByRedis(List<CategoryModel> list)
    {
      try
      {
        _logger.LogDebug("[REDIS] SetCategoryListByRedis");
        await _cache.SetAsync("categorylist", JsonConvert.SerializeObject(list));
      } 
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
      }
    }
    #endregion

    #region Redis:Item
    private void ItemWasChanged(ItemModel item)
    {
      _logger.LogDebug("[REDIS] ItemWasChanged");
      _cache.DeleteAsync(item.myCategory + "itemlist:");
    }
    private async Task<List<ItemModel>?> GetItemListInCategoryByRedis(string categoryId)
    {
      return null;
      try
      {
        _cache.Wait();

        string? cacheList = await _cache.GetAsync(categoryId + ":itemlist");
        if (cacheList != null)
        {
          _logger.LogDebug("[REDIS] GetItemListInCategoryByRedis");
          return JsonConvert.DeserializeObject<List<ItemModel>>(cacheList);
        }
  
        return null;
      }
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
        return null;
      }
      finally
      {
        _cache.Release();
      }
    }
    private async void SetItemListInCategoryByRedis(string categoryId, List<ItemModel> list)
    {
      return;
      try
      {
        _logger.LogDebug("[REDIS] SetItemListInCategoryByRedis");
        await _cache.SetAsync(categoryId + ":itemlist", JsonConvert.SerializeObject(list));
      } 
      catch (Exception ex)
      {
        _logger.LogError(ex.Message);
      }
    }
    #endregion

    #region MongoDb:Category
    private async Task<CategoryModel?> GetCategoryByMongoDb(string categoryId)
    {
      MongoDBCategoryModel? c = (await _mongoDbService.GetCategoryAsync(Builders<MongoDBCategoryModel>.Filter.Eq("Id", categoryId))).FirstOrDefault();
      if(c != null) return c.ToModel();
      
      return null;
    }
    private async Task<List<CategoryModel>> GetCategoryListByMongoDb()
    {
      return (await _mongoDbService.GetCategoryAsync(Builders<MongoDBCategoryModel>.Filter.Empty)).ToModelList();
    }
    private async Task<CategoryModel> PutCategoryByMongoDb(CategoryModel c)
    {
      return (await _mongoDbService.InsertOneCategoryAsync(c.FromModel())).ToModel(); ;
    }
    private async Task<CategoryModel?> PatchCategoryByMongoDb(CategoryModel c)
    {
      if(c.id != null) {
        CategoryModel? existingCategory = await GetCategoryByMongoDb(c.id);
  
        if (existingCategory != null)
        {
          ReplaceOneResult result = await _mongoDbService.ReplaceOneCategoryAsync(c.FromModel());
  
          return result.ModifiedCount > 0? c : null;
        }
      }

      return null;
    }
    private async Task<CategoryModel?> DeleteCategoryByMongoDb(CategoryModel c)
    {
      List<MongoDBItemModel> items = await _mongoDbService.GetItemAsync(Builders<MongoDBItemModel>.Filter.Eq("MyCategory", c.id));

      foreach (MongoDBItemModel item in items)
      {
        await _mongoDbService.DeleteOneItemAsync(item);
      }

      await _mongoDbService.DeleteOneCategoryAsync(c.id);
      return c;

      return null;
    }

    // private async void ChangeDisplayAllCategoriesByMongoDb(bool value)
    // {
    //   List<CategoryModel> list = await GetCategoryList();

    //   foreach (CategoryModel c in list)
    //   {
    //     c.isOpen = value;
    //   }

    //   await _mongoDbService.ReplaceManyCategoriesAsync(list.FromModelList());
    // }

    #endregion

    #region MongoDb:Item
    private async Task<ItemModel?> GetItemByMongoDb(string itemId){
      MongoDBItemModel? item = (await _mongoDbService.GetItemAsync(Builders<MongoDBItemModel>.Filter.Eq("Id", itemId))).FirstOrDefault();

      if(item != null) return item.ToModel();
      
      return null;
    }
    private async Task<List<ItemModel>> GetItemListInCategoryByMongoDb(string categoryId)
    {
      return (await _mongoDbService.GetItemAsync(Builders<MongoDBItemModel>.Filter.Eq("MyCategory", categoryId))).ToModelList();
    }
    private async Task<ItemModel?> PutItemByMongoDb(ItemModel i)
    {
      return (await _mongoDbService.InsertOneItemAsync(i.FromModel())).ToModel();
    }
    private async Task<ItemModel?> PatchItemByMongoDb(ItemModel i)
    {
      if(i.id != null){
        ItemModel? item = await GetItemByMongoDb(i.id);

        if(item != null){

          ReplaceOneResult? result = await _mongoDbService.ReplaceOneItemAsync(i.FromModel());

          return result.ModifiedCount > 0? i : null;
        }
      }

      return null;
    }
    private async Task DeleteItemByMongoDb(ItemModel item)
    {
      await _mongoDbService.DeleteOneItemAsync(item.FromModel());
    }
    #endregion
  }
}
