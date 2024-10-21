using HNOne.API.Configurations;
using HNOne.API.Services.Interfaces;
using HNOne.Model;
using HNOne.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HNOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private ILogger<UserController> _logger { get; set; }
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public UserController(ILogger<UserController> logger, IConfiguration configuration, IUserService userService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = ""
                    });
                    
                }
                var result = await _userService.LoginAsync(request);
                if(result == null)
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Message = ""
                    });
                }
                if (result.status == StatusCodes.Status404NotFound)
                {
                    return StatusCode(StatusCodes.Status404NotFound, new
                    {
                        Status = StatusCodes.Status404NotFound,
                        result.message
                    });
                }

                // generate token
                string accessToken = generateAccessToken(result.data!);
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    Status = StatusCodes.Status400BadRequest,
                    ex.Message
                });
            }
        }


        #region Private Function
        private string generateAccessToken(UserModel pUser)
        {
            var claims = new[]
            {
                new Claim(nameof(pUser.userId), $"{pUser.userId}"),
                new Claim(nameof(pUser.userName), $"{pUser.userName}")
            };

            var jwtConfiguration = new JwtConfiguration();
            _configuration.GetSection(nameof(JwtConfiguration)).Bind(jwtConfiguration);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.JwtSecurityKey)); // key mã hóa
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256); // loại mã hóa (Header)
            //var expiry = DateTime.Now.AddMinutes(Convert.ToInt32(jwtConfiguration.JwtExpiryInDays)); // hết hạn token
            var expiry = DateTime.Now.AddSeconds(15); // hết hạn token
            var token = new JwtSecurityToken(jwtConfiguration.JwtIssuer
                , jwtConfiguration.JwtAudience
                , claims
                , expires: expiry
                , signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        #endregion

    }
}
