using HNOne.API.Configurations;
using HNOne.API.Services.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using Microsoft.AspNetCore.Mvc;
using IdentityModelTokens = Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        private JwtConfiguration jwtConfiguration;

        public UserController(ILogger<UserController> logger, IConfiguration configuration, IUserService userService)
        {
            _logger = logger;
            _configuration = configuration;
            _userService = userService;
            jwtConfiguration = new JwtConfiguration();
            _configuration.GetSection(nameof(JwtConfiguration)).Bind(jwtConfiguration);
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestModel request)
        {
            ResponseModel<UserModel> response = new ResponseModel<UserModel>();
            try
            {
                if (!ModelState.IsValid)
                {
                    response.status = StatusCodes.Status400BadRequest;
                    response.message = "Không tìm thấy dữ liệu!!!";
                    return Ok(response);
                }
                response = await _userService.Login(request);
                if(response.status == StatusCodes.Status200OK)
                {
                    var user = new UserModel();
                    user.branchId = response.data!.branchId;
                    user.branchCode = response.data!.branchCode;
                    user.branchName = response.data!.branchName;
                    user.branchIds = response.data!.branchIds;
                    user.userId = response.data!.userId;
                    user.userName = response.data!.userName;
                    user.employeeName = response.data!.employeeName;
                    user.employeeCode = response.data!.employeeCode;
                    user.employeeId = response.data!.employeeId;
                    user.isAdmin = response.data!.isAdmin;
                    user.departmentId = response.data!.departmentId;
                    user.departmentIds = response.data!.departmentIds;
                    // generate token
                    string accessToken = generateAccessToken(user);
                    user.token = accessToken;
                    // generate refresh token
                    string refreshToken = generateRefreshToken();
                    user.refreshToken = refreshToken;
                    await _userService.UpdateRefreshToken(user.userId, refreshToken, jwtConfiguration.JwtRefreshTokenExpiryInDays);
                    response.data = user;
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }


        #region Private Function
        /// <summary>
        /// tạo access token
        /// </summary>
        /// <param name="pUser"></param>
        /// <returns></returns>
        private string generateAccessToken(UserModel pUser)
        {
            var claims = new[]
            {
                new Claim(nameof(pUser.userId), $"{pUser.userId}"),
                new Claim(nameof(pUser.userName), $"{pUser.userName}"),
                new Claim(nameof(pUser.branchId), $"{pUser.branchId}"),
            };

            var key = new IdentityModelTokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtConfiguration.JwtSecurityKey)); // key mã hóa
            var creds = new IdentityModelTokens.SigningCredentials(key, IdentityModelTokens.SecurityAlgorithms.HmacSha256); // loại mã hóa (Header)
            //var expiry = DateTime.Now.AddMinutes(Convert.ToInt32(jwtConfiguration.JwtExpiryInDays)); // hết hạn token
            var expiry = DateTime.Now.AddSeconds(15); // hết hạn token
            var token = new JwtSecurityToken(jwtConfiguration.JwtIssuer
                , jwtConfiguration.JwtAudience
                , claims
                , expires: expiry
                , signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// tạo refresh token
        /// </summary>
        /// <returns></returns>
        private string generateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }

        /// <summary>
        /// lấy dữ liệu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Authorize] // sau có token mở ra
        [HttpPost]
        [Route("get-data")]
        public async Task<IActionResult> GetData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                switch (processKey)
                {
                    case ProcessConstants.GET_USER:
                        response.data = await _userService.GetUser(request);
                        break;
                    case ProcessConstants.GET_PER_GROUP:
                        response.data = await _userService.GetPermissionGroup(request);
                        break;
                    case ProcessConstants.GET_PER_GROUP_ACCESS_CONTROL:
                        response.data = await _userService.GetPermissionByGroupId(request.documentId);
                        break;

                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }
                if ((response.data is IEnumerable<object> dataList) && dataList.IsNullOrEmpty())
                {
                    response.status = StatusCodes.Status204NoContent;
                    response.message = "Không tìm thấy dữ liệu!!!";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }

        [HttpPost]
        [Route("post-data")]
        public async Task<IActionResult> PostData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                switch (processKey)
                {
                    case ProcessConstants.POST_USER:
                    case ProcessConstants.PUT_USER:
                        var user = JsonConvert.DeserializeObject<Users>($"{request.json}");
                        response = await _userService.UpdateUser(processKey, user!);
                        break;
                    case ProcessConstants.POST_PER_GROUP:
                    case ProcessConstants.PUT_PER_GROUP:
                        var perGroup = JsonConvert.DeserializeObject<PermissionGroups>($"{request.json}");
                        response = await _userService.UpdatePermissionGroup(processKey, perGroup!);
                        break;
                    case ProcessConstants.POST_PER_GROUP_ACCESS_CONTROL:
                        var perGroupAccessControl = JsonConvert.DeserializeObject<List<GroupAccessControls>>($"{request.json}");
                        response = await _userService.UpdateGroupAccessControl(request.documentId, perGroupAccessControl!);
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
        }
        #endregion

    }
}
