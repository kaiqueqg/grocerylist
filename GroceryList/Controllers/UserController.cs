using GroceryList.Data.UnitOfWork;
using GroceryList.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace GroceryList.Controllers
{
	[ApiController]
	[Route("api/")]
	public class UserController : ControllerBase
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IConfiguration _config;

		public UserController(IUnitOfWork unitOfWork, IConfiguration config)
		{
			_unitOfWork = unitOfWork;
			_config = config;
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("Login")]
		public IActionResult Login(UserModel userModel)
		{
			userModel = _unitOfWork.UserRepository().GetUser(userModel.UserName, userModel.Password);

			if(userModel == null)
			{
				return Unauthorized(userModel);
			}
			else
			{
				LoginModel loginModel = new LoginModel();
				string token = GenerateJSONWebToken(userModel);
				loginModel.user = userModel;
				loginModel.token = token;
				return Ok(loginModel);
			}
		}

		private string GenerateJSONWebToken(UserModel userModel)
		{
			var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Authentication:Jwt:Key"]));
			var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
			var claims = new List<Claim>()
			{
				new Claim(ClaimTypes.Name, userModel.UserName),
				new Claim(ClaimTypes.Email, userModel.Email),
				new Claim(ClaimTypes.Role, userModel.Role)
			};

			var token = new JwtSecurityToken(
			  issuer: _config["Authentication:Jwt:Issuer"],
			  claims: claims,
			  expires: DateTime.Now.AddDays(30),
			  signingCredentials: credentials); ;

			return new JwtSecurityTokenHandler().WriteToken(token);
		}
	}
}
