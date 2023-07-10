using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GroceryList.Model
{
	public class UserModel
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public int? id { get; set; }

    [Required(ErrorMessage = "User is required!")]
		public string UserName { get; set; }
		public string Password { get; set; }
		public string? Email { get; set; }
		public string? Role { get; set; }
	}
}
