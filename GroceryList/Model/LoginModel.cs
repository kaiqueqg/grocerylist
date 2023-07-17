namespace GroceryList.Model
{
	public class LoginModel
	{
		public UserModel? user { get; set; }
		public string token { get; set; }
    public string errorMessage { get; set; }
	}
}
