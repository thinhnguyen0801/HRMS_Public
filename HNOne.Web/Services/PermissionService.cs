using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Models;
using HNOne.Web.Commons;

namespace HNOne.Web.Services
{
    public interface IPermissionService
    {
        Task<List<string>> GetEventKeyAsync(int userId, string token, string menuId);
    }
    public class PermissionService : ApiServiceBase, IPermissionService
    {
        public PermissionService(IHttpClientFactory factory, ILogger<PermissionService> logger) : base(factory, logger) { }

        /// <summary>
        /// lấy danh sách event theo User với menu
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="token"></param>
        /// <param name="menuId"></param>
        /// <returns></returns>
        public async Task<List<string>> GetEventKeyAsync(int userId, string token, string menuId)
        {
            List<string> lstKeyAction = new List<string>();
            try
            {
                RequestModel request = new RequestModel();
                request.token = token;
                request.userId = userId;
                request.type = ProcessConstants.GET_MENU_TYPE_CHECK_PERMISSION;
                request.process = ProcessConstants.GET_MENU;
                request.opt = menuId;
                HttpResponseMessage httpResponse = await PostAsync(EnpointConstants.MASTERDATA_GET_DATA, request);
                var checkContent = ValidateJsonContent(httpResponse.Content);
                if (checkContent) 
                {
                    var response = await httpResponse.Content.ReadFromJsonAsync<ResCliModel<MenuModel>>();
                    if (response == null || response.status != StatusCodes.Status200OK) return lstKeyAction;
                    response.data ??= new List<MenuModel>();
                    lstKeyAction = response.data.Select(m=>$"{m.actionKey}").ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEventKeyAsync");
            }
            return lstKeyAction;
        }
    }
}
