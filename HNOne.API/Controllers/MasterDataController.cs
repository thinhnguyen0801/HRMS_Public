using Azure;
using HNOne.API.Repositories.Interfaces;
using HNOne.API.Services.Interfaces;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace HNOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly ILogger<MasterDataController> _logger;
        private readonly IMasterDataService _masterDataService;
        private readonly IApprovalRepository _approvalRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MasterDataController(IMasterDataService masterDataService
            , ILogger<MasterDataController> logger, IWebHostEnvironment webHostEnvironment
            , IApprovalRepository approvalRepository)
        {
            _masterDataService = masterDataService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _approvalRepository = approvalRepository;
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
                        response.data = await _masterDataService.GetEnum(request);
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
                    case ProcessConstants.GET_LOCATION:
                        response.data = await _masterDataService.GetLocationData(request.type, request.opt, request.opt1, request.opt2);
                        break;
                    case ProcessConstants.GET_COMBO_MASTER_DATA:
                        response.data = await _masterDataService.GetMasterData(request);
                        break;
                    case ProcessConstants.GET_FUN_ENUM:
                        response.data = await _masterDataService.GetFnEnum(request);
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

        [HttpPost]
        [Route("approval")]
        public async Task<IActionResult> Approval([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            try
            {
                string? processKey = request.process?.Trim();
                switch (processKey)
                {
                    case ProcessConstants.GET_APPROVAL:
                        response.data = await _approvalRepository.GetApproval(request);
                        if ((response.data is IEnumerable<object> dataList) && dataList.IsNullOrEmpty())
                        {
                            response.status = StatusCodes.Status204NoContent;
                            response.message = "Không tìm thấy dữ liệu!!!";
                        }
                        break;
                    case ProcessConstants.POST_APPROVAL:
                        // gửi phê duyệt
                        var approval = JsonConvert.DeserializeObject<Approvals>($"{request.json}")!;
                        response = await _approvalRepository.AddApproval(approval);
                        break;
                    case ProcessConstants.PUT_APPROVAL:
                        // duyệt/từ chối
                        var approvalList = JsonConvert.DeserializeObject<List<Approvals>>($"{request.json}");
                        response = await _approvalRepository.UpdateApproval($"{request.type}", approvalList!);
                        break;
                    case ProcessConstants.GET_DOCUMENT_HISTORY:
                        response = await _approvalRepository.GetFnDocumentHistory(request);
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
                    case ProcessConstants.PUT_ENUM_CATA:
                    case ProcessConstants.POST_ENUM_CATA:
                        var enumCata = JsonConvert.DeserializeObject<EnumCatagories>($"{request.json}");
                        response = await _masterDataService.UpdateEnumCatagory(processKey, enumCata!);
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


        [HttpPost]
        [Route("upload-images")]
        public async Task<IActionResult> UploadImages([FromForm] List<IFormFile> files, string subFolder)
        {
            try
            {
                if (files == null || !files.Any())
                {
                    return StatusCode(StatusCodes.Status400BadRequest, new
                    {
                        StatusCode = StatusCodes.Status400BadRequest,
                        Message = "Không có dữ liệu file đính kèm"
                    });
                }
                var result = new List<FileUploadModel>();
                string fileName = string.Empty;
                string path = $"{this._webHostEnvironment.WebRootPath}\\{subFolder}";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);

                foreach (var file in files)
                {
                    fileName = file.FileName; // trên kia mã hóa
                    string fullPath = Path.Combine(path, fileName);
                    using (var image = Image.Load(file.OpenReadStream()))
                    {
                        image.Mutate(m => m.Resize(400, 400));
                        await image.SaveAsync(fullPath);
                    }
                    result.Add(new FileUploadModel() { fileName = fileName, filePath = fullPath });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadImages");
                return StatusCode(StatusCodes.Status400BadRequest, new
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    ex.Message
                });

            }
        }
        #region Private Function
        #endregion
    }
}
