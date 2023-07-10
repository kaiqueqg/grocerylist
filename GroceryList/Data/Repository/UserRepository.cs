using GroceryList.Model;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GroceryList.Data.Repository
{
	public class UserRepository
	{
		SqlServerContext _sqlServerContext;

		public UserRepository(SqlServerContext sqlServerContext)
		{
			_sqlServerContext = sqlServerContext;
		}
		public UserModel? GetUser(string username, string password)
		{
			if(username == "test" && password == "test")
			{
				return new UserModel() 
				{ 
					id = 0,
					UserName = username,
					Email = "test@gmail.com",
					Role = "admin"
				};
			}
			else return null;
		}
	}
}
