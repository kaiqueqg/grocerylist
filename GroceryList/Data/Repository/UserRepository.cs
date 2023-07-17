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
		public LoginModel? GetUser(string username, string password)
		{
      if(username != "test") return new LoginModel() { user = null, token = "", errorMessage = "Wrong username!" };
      if(password != "test") return new LoginModel() { user = null, token = "", errorMessage = "Wrong password!" };
			
      return new LoginModel() { 
        user = new UserModel() 
        { 
          id = 0,
          UserName = username,
          Password = "",
          Email = "test@gmail.com",
          Role = "admin"
        }
      };
		}
	}
}
