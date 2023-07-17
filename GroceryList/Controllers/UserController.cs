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
    private readonly ILogger _logger;

		public UserController(IUnitOfWork unitOfWork, IConfiguration config, ILogger<UserController> logger)
		{
			_unitOfWork = unitOfWork;
			_config = config;
      _logger = logger;
		}

		[HttpPost]
		[AllowAnonymous]
		[Route("Login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status401Unauthorized)]
		public IActionResult Login(UserModel userModel)
		{
      _logger.LogTrace("Login");

			try
			{
				LoginModel login = _unitOfWork.UserRepository().GetUser(userModel.UserName, userModel.Password);

        if(login.user == null)
        {
          return Unauthorized(login.errorMessage);
        }
        else
        {
          LoginModel loginModel = new LoginModel();
          string token = GenerateJSONWebToken(login.user);
          loginModel.user = userModel;
          loginModel.token = token;
          return Ok(loginModel);
        }
			}
			catch(Exception ex)
			{
				_logger.LogError(ex.Message);
				return StatusCode(500, ex.Message);
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
