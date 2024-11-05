using Azure;
using HNOne.API.Services.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HNOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly ILogger<MasterDataController> _logger;
        private readonly IMasterDataService _masterDataService;
        public MasterDataController(IMasterDataService masterDataService, ILogger<MasterDataController> logger)
        {
            _masterDataService = masterDataService;
            _logger = logger;
        }

        /// <summary>
        /// lấy dữ liệu cho danh sách không cần token
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("get-data-without-token")]
        public async Task<IActionResult> GetDataWithoutToken([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                switch (processKey)
                {
                    case ProcessConstants.GET_BRANCH:
                        response.data = await _masterDataService.GetBranch();
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }
                if (!(response.data is IEnumerable<object> dataList) || dataList.IsNullOrEmpty())
                {
                    response.status = StatusCodes.Status204NoContent;
                    response.message = "Không tìm thấy dữ liệu!!!";
                }
            }
            catch (Exception ex)
            {
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
            }
            return Ok(response);
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
                    case ProcessConstants.GET_MENU:
                        response.data = await _masterDataService.GetMenu(request);
                        break;
                    case ProcessConstants.GET_DEPARTMENT:
                        response.data = await _masterDataService.GetDepartment(request);
                        break;
                    case ProcessConstants.GET_TITLE:
                        response.data = await _masterDataService.GetTitle(request);
                        break;
                    case ProcessConstants.GET_POSITION:
                        response.data = await _masterDataService.GetPosition(request);
                        break;
                    case ProcessConstants.GET_CONTRACTTYPE:
                        response.data = await _masterDataService.GetContractType(request);
                        break;
                    case ProcessConstants.GET_REASONCATEGORIE:
                        response.data = await _masterDataService.GetReasonCategorie(request);
                        break;
                    case ProcessConstants.GET_ENUM:
                        response.data = await _masterDataService.GetEnum($"{request.opt}");
                        break;
                    case ProcessConstants.GET_SALARY_CATEGORY:
                        response.data = await _masterDataService.GetSalaryCatagory(request);
                        break;
                    case ProcessConstants.GET_SALARY_CONFIG:
                        response.data = await _masterDataService.GetSalaryConfig();
                        break;
                    case ProcessConstants.GET_DOCUMENT_NO:
                        response.data = await _masterDataService.GetDocumentNo(request.type, request.opt, request.opt1, request.opt2);
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return Ok(response);
                }
                if((response.data is IEnumerable<object> dataList) && dataList.IsNullOrEmpty())
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

        /// <summary>
        /// Post dữ liệu
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Authorize] // sau có token mở ra
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
                    case ProcessConstants.PUT_BRANCH:
                    case ProcessConstants.POST_BRANCH:
                        var branch = JsonConvert.DeserializeObject<Branchs>($"{request.json}");
                        response = await _masterDataService.UpdateBranch(processKey, branch!);
                        break;
                    case ProcessConstants.PUT_DEPARTMENT:
                    case ProcessConstants.POST_DEPARTMENT:
                        var department = JsonConvert.DeserializeObject<Departments>($"{request.json}");
                        response = await _masterDataService.UpdateDepartment(processKey, department!);
                        break;
                    case ProcessConstants.PUT_POSITION:
                    case ProcessConstants.POST_POSITION:
                        var position = JsonConvert.DeserializeObject<Positions>($"{request.json}");
                        response = await _masterDataService.UpdatePosition(processKey, position!);
                        break;
                    case ProcessConstants.PUT_TITLE:
                    case ProcessConstants.POST_TITLE:
                        var title = JsonConvert.DeserializeObject<Titles>($"{request.json}");
                        response = await _masterDataService.UpdateTitle(processKey, title!);
                        break;
                    case ProcessConstants.PUT_CONTRACTTYPE:
                    case ProcessConstants.POST_CONTRACTTYPE:
                        var contractType = JsonConvert.DeserializeObject<ContractTypes>($"{request.json}");
                        response = await _masterDataService.UpdateContractType(processKey, contractType!);
                        break;
                    case ProcessConstants.PUT_REASONCATEGORIE:
                    case ProcessConstants.POST_REASONCATEGORIE:
                        var reasonCategorie = JsonConvert.DeserializeObject<ReasonCategories>($"{request.json}");
                        response = await _masterDataService.UpdateReasonCategorie(processKey, reasonCategorie!);
                        break;
                    case ProcessConstants.PUT_SALARY_CATEGORY:
                    case ProcessConstants.POST_SALARY_CATEGORY:
                        var salaryCatagory = JsonConvert.DeserializeObject<SalaryCategories>($"{request.json}");
                        response = await _masterDataService.UpdateSalaryCategory(processKey, salaryCatagory!);
                        break;
                    case ProcessConstants.PUT_SALARY_CONFIG:
                    case ProcessConstants.POST_SALARY_CONFIG:
                        var salaryConfig = JsonConvert.DeserializeObject<SalaryConfigurations>($"{request.json}");
                        response = await _masterDataService.UpdateSalaryConfig(processKey, salaryConfig!);
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
        
        
        #region Private Function
        #endregion
    }
}
