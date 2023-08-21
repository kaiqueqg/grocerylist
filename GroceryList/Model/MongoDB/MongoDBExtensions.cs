using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace GroceryList.Model.MongoDB
{
	public static class MongoDBExtensions
	{
		public static MongoDBItemModel FromModel(this ItemModel i) 
		{ 
			return new MongoDBItemModel() 
			{ 
				Id = i.id, 
				Text = i.text, 
				Quantity = i.quantity,
				QuantityUnit = i.quantityUnit,
				GoodPrice	= i.goodPrice,
				IsChecked = i.isChecked, 
				MyCategory = i.myCategory
			}; 
		}
		public static ItemModel ToModel(this MongoDBItemModel i)
		{
			return new ItemModel()
			{
				id = i.Id,
				text = i.Text,
				quantity = i.Quantity,
				quantityUnit = i.QuantityUnit,
				goodPrice = i.GoodPrice,
				isChecked = i.IsChecked,
				myCategory = i.MyCategory
			};
		}

		public static MongoDBCategoryModel FromModel(this CategoryModel c) 
		{ 
			return new MongoDBCategoryModel() 
			{ 
				Id = c.id, 
				Text = c.text, 
				IsOpen = c.isOpen
			}; 
		}
		public static CategoryModel ToModel(this MongoDBCategoryModel c)
		{
			return new CategoryModel()
			{
				id = c.Id,
				text = c.Text,
				isOpen = c.IsOpen
			};
		}

		public static List<MongoDBCategoryModel> FromModelList(this List<CategoryModel> list)
		{
			List<MongoDBCategoryModel> rtnList = new List<MongoDBCategoryModel>();
			foreach(CategoryModel i in list)
			{
				rtnList.Add(i.FromModel());
			}

			return rtnList;
		}
		public static List<MongoDBItemModel> FromModelList(this List<ItemModel> list)
		{
			List<MongoDBItemModel> rtnList = new List<MongoDBItemModel>();
			foreach(ItemModel i in list)
			{
				rtnList.Add(i.FromModel());
			}

			return rtnList;
		}

		public static List<CategoryModel> ToModelList(this List<MongoDBCategoryModel> list)
		{
			List<CategoryModel> catList = new List<CategoryModel>();

			foreach(MongoDBCategoryModel c in list)
			{
				catList.Add(c.ToModel());
			}

			return catList;
		}
		public static List<ItemModel> ToModelList(this List<MongoDBItemModel> list)
		{
			List<ItemModel> rtnList = new List<ItemModel>();

			foreach(MongoDBItemModel i in list)
			{
				rtnList.Add(i.ToModel());
			}

			return rtnList;
		}

		public static UserModel ToModel(this MongoDBUserModel u)
		{
			return new UserModel()
			{
				Id = u.Id,
				UserName = u.Username,
				Password = u.Password,
				UserPrefs = u.UserPrefs.ToModel(),
			};
		}
		public static MongoDBUserModel FromModel(this UserModel u)
		{
			return new MongoDBUserModel()
			{
				Id = u.Id,
				Username = u.UserName,
				Password = u.Password,
				UserPrefs = u.UserPrefs == null ? null : u.UserPrefs.FromModel(),
			};
    }

    public static UserPrefsModel ToModel(this MongoDBUserPrefsModel p)
    {
      return new UserPrefsModel()
      {
        HideQuantity = p.HideQuantity,
				ShouldCreateNewItemWhenCreateNewCategory = p.ShouldCreateNewItemWhenCreateNewCategory,
      };
    }
    public static MongoDBUserPrefsModel FromModel(this UserPrefsModel p)
    {
      return new MongoDBUserPrefsModel()
      {
        HideQuantity = p.HideQuantity,
        ShouldCreateNewItemWhenCreateNewCategory = p.ShouldCreateNewItemWhenCreateNewCategory,
      };
    }
  }
}
