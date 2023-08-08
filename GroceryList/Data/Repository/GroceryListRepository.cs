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
    public async Task<GroceryListModel?> SyncGroceryList(GroceryListModel model)
    {
      try {
        //^ Deleting categories
        if(model.deletedCategories != null) 
        {
          foreach(CategoryModel c in model.deletedCategories)
          {
            try
            {
              await DeleteCategory(c);
            }
            catch{}
          }
        }
        //^ Deleting items
        if(model.deletedItems != null)
        {
          foreach(ItemModel i in model.deletedItems)
          {
            try
            {
              await DeleteItem(i);
            }
            catch { }
          }
        }

        //^ CATEGORY
        GroceryListModel groceryList = await GetGroceryList();

        List<CategoryModel>? dbCategoryList = groceryList.categories;
        if(dbCategoryList != null) //^ there's any data on db?
        {
          foreach(CategoryModel c in model.categories)
          {
            CategoryModel? dbCategory = null;

            //^ equal id exist?
            dbCategory = dbCategoryList.Find(x => x.id == c.id);
            if(dbCategory == null)
            {
              string oldId = c.id;
              string newId = "";
              //^ equal text exist?
              dbCategory = dbCategoryList.Find(x => x.text == c.text);

              //^ this category is unique, insert it
              if(dbCategory == null)
              {
                c.id = "";
                CategoryModel? insertedCategory = await PutCategory(c);

                if(insertedCategory != null) c.id = insertedCategory.id;
              }
              else //^ there's equal text category, get id, update it
              {
                c.id = dbCategory.id;
                await PatchCategory(c);
              }

              //^ update all items id that have the old id
              List<ItemModel>? items = model.items.Where(x => x.myCategory == oldId).ToList();
              if(items != null)
              {
                foreach(ItemModel i in items) i.myCategory = c.id;
              }
            }
            else //^ id already exist in db, just update it
            {
              await PatchCategory(c);
            }
          }

          //^ ITEM
          foreach(ItemModel i in model.items)
          {
            //^ item is already in db?
            ItemModel? dbItem = groceryList.items.Find(x => x.id == i.id);

            if(dbItem == null)
            {
              //^ equal text exist in the same category?
              dbItem = groceryList.items.Find(x => x.text == i.text && x.myCategory == i.myCategory);

              if(dbItem == null) //^ this item is unique, insert it
              {
                i.id = "";
                ItemModel? newItem = await PutItem(i);
                if(newItem != null) i.id = newItem.id;
              }
              else //^ there's equal text item, get id, update it
              {
                i.id = dbItem.id;
                await PatchItem(i);
              }
            }
            else //^ id already exist in db, just update it
            {
              await PatchItem(i);
            }
          }

          return await GetGroceryList();
        }
        else
        {
          return groceryList;
        }
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
