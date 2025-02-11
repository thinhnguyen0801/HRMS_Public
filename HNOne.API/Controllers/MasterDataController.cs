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
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Routing.Template;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using HNOne.API.Constants;
using System.Data;

namespace HNOne.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly ILogger<MasterDataController> _logger;
        private readonly IMasterDataService _masterDataService;
        private readonly IApprovalRepository _approvalRepository;
        private readonly IPersonnelService _personnelService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public MasterDataController(IMasterDataService masterDataService
            , ILogger<MasterDataController> logger, IWebHostEnvironment webHostEnvironment
            , IApprovalRepository approvalRepository, IPersonnelService personnelService)
        {
            _masterDataService = masterDataService;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
            _approvalRepository = approvalRepository;
            _personnelService = personnelService;
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
                        response.data = await _masterDataService.GetBranch(request);
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
                        response.data = await _masterDataService.GetReasonCategory(request);
                        break;
                    case ProcessConstants.GET_ENUM:
                        response.data = await _masterDataService.GetEnum(request);
                        break;
                    case ProcessConstants.GET_SALARY_CATEGORY:
                        response.data = await _masterDataService.GetSalaryCatagory(request);
                        break;
                    case ProcessConstants.GET_SALARY_PARAMETER:
                        response.data = await _masterDataService.GetSalaryParameter(request);
                        break;
                    case ProcessConstants.GET_SALARY_CONFIG:
                        response.data = await _masterDataService.GetSalaryConfig(request);
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
                    case ProcessConstants.GET_TAXT_RATE:
                        response.data = await _masterDataService.GetTaxRate(request);
                        break;
                    case ProcessConstants.GET_DEDUCTION_CONFIG:
                        response.data = await _masterDataService.GetDeductionConfig(request);
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
                        var approval = JsonConvert.DeserializeObject<ApprovalModel>($"{request.json}")!;
                        response = await _approvalRepository.AddApproval(approval);
                        break;
                    case ProcessConstants.PUT_APPROVAL:
                        // duyệt/từ chối
                        var approvalList = JsonConvert.DeserializeObject<List<ApprovalModel>>($"{request.json}");
                        response = await _approvalRepository.UpdateApproval($"{request.type}", approvalList!);
                        break;
                    case ProcessConstants.PUT_CANCEL_DOCUMENT:
                        // hủy chứng từ
                        var cancelDocList = JsonConvert.DeserializeObject<List<ApprovalModel>>($"{request.json}");
                        response = await _approvalRepository.CancelDocument(cancelDocList!);
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
                    case ProcessConstants.PUT_SALARY_PARAMETER:
                    case ProcessConstants.POST_SALARY_PARAMETER:
                        var salaryPara = JsonConvert.DeserializeObject<SalaryParameters>($"{request.json}");
                        response = await _masterDataService.UpdateSalaryParameter(processKey, salaryPara!);
                        break;
                    case ProcessConstants.PUT_ENUM_CATA:
                    case ProcessConstants.POST_ENUM_CATA:
                        var enumCata = JsonConvert.DeserializeObject<EnumCatagories>($"{request.json}");
                        response = await _masterDataService.UpdateEnumCatagory(processKey, enumCata!);
                        break;
                    case ProcessConstants.PUT_TAXT_RATE:
                    case ProcessConstants.POST_TAXT_RATE:
                        var taxRate = JsonConvert.DeserializeObject<TaxRates>($"{request.json}");
                        response = await _masterDataService.UpdateTaxRate(processKey, taxRate!);
                        break;
                    case ProcessConstants.PUT_DEDUCTION_CONFIG:
                    case ProcessConstants.POST_DEDUCTION_CONFIG:
                        var deductionConfig = JsonConvert.DeserializeObject<DeductionConfigs>($"{request.json}");
                        response = await _masterDataService.UpdateDeductionConfig(processKey, deductionConfig!);
                        break;
                    case ProcessConstants.DELETE_DYNAMIC:
                        response = await _masterDataService.DeleteDynamic(request);
                        break;
                    case ProcessConstants.POST_IMPORT_DATA:
                        response = await _masterDataService.ImportData(request.branchId, request.userId, processKey: $"{request.opt}", $"{request.json}");
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

        [HttpPost]
        [Route("export-data")]
        public async Task<IActionResult> ExportData([FromBody] RequestModel request)
        {
            ResponseModel response = new ResponseModel();
            string outputPath = string.Empty;
            try
            {
                IEnumerable<object> dataList;
                string? processKey = request.process?.Trim();
                // đi lấy dữ liệu
                switch (processKey)
                {
                    case ProcessConstants.GET_CONTRACT:
                        dataList = await _personnelService.GetContract(request);
                        break;
                    case ProcessConstants.GET_CONTRACT_APPENDIX:
                        dataList = await _personnelService.GetContractAppendix(request);
                        break;
                    default:
                        response.status = StatusCodes.Status404NotFound;
                        response.message = $"Process Key {processKey} was not provider!!!";
                        return BadRequest(response);
                }
                if (dataList.IsNullOrEmpty())
                {
                    response.status = StatusCodes.Status204NoContent;
                    response.message = "Không tìm thấy dữ liệu!!!";
                    return BadRequest(response);
                }
                string templatePath = $"{this._webHostEnvironment.WebRootPath}\\Templates\\{request.opt}";
                if (!System.IO.File.Exists(templatePath))
                {
                    response.status = StatusCodes.Status404NotFound;
                    response.message = MessageConstants.MESSAGE_FILE_NOT_FOUNT;
                    return BadRequest(response);
                }
                string path = $"{this._webHostEnvironment.WebRootPath}\\Exports";
                outputPath = $"{path}\\{Guid.NewGuid().ToString().Replace("-", "")}-{request.opt}";
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                // copy nội dung sang file mới
                System.IO.File.Copy(templatePath, outputPath, true);
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(outputPath, true))
                {
                    // Lấy nội dung văn bản & thay thế nội dung placeholder
                    var body = wordDoc.MainDocumentPart!.Document.Body;
                    if (processKey == ProcessConstants.GET_CONTRACT) fillContractToFile(body!, dataList);
                    if (processKey == ProcessConstants.GET_CONTRACT_APPENDIX) fillContractAppendixToFile(body!, dataList);
                    wordDoc.MainDocumentPart.Document.Save();
                }
                var fileBytes = await System.IO.File.ReadAllBytesAsync(outputPath);
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"{request.opt}");    
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                response.status = StatusCodes.Status400BadRequest;
                response.message = ex.Message;
                return BadRequest(response);
            }
            finally
            {
                if (!string.IsNullOrEmpty(outputPath)
                    && System.IO.File.Exists(outputPath))
                {
                    // loại bỏ file đính kèm
                    System.IO.File.Delete(outputPath);
                }
            }
            
        }
        #region Private Function
        /// <summary>
        /// fill dữ liệu Hợp đồng
        /// </summary>
        /// <param name="body"></param>
        /// <param name="dataList"></param>
        private void fillContractToFile(Body body, IEnumerable<object> dataList)
        {
            ContractModel contract = dataList.Cast<ContractModel>().First();
            List<SalaryConfigurationModel> lstSalaryConfig = JsonConvert.DeserializeObject<List<SalaryConfigurationModel>>(contract.jsonDetail!)!; // nếu không có cho rớt catch
            Table? table = body.Elements<Table>().FirstOrDefault();
            if(table != null)
            {
                // Tìm một dòng mẫu từ bảng để lấy định dạng
                var sampleRow = table.Descendants<TableRow>().Skip(1).FirstOrDefault(); // Bỏ qua dòng tiêu đề
                var sampleRunProperties = sampleRow!.Descendants<RunProperties>().FirstOrDefault();
                sampleRow.Remove();
                decimal totalSalary = 0;
                foreach (var item in lstSalaryConfig)
                {
                    if (!item.isPrintContract) continue;
                    totalSalary += item.amount;
                    TableRow dataRow = new TableRow();
                    // Tạo ô cho "Nội dung"
                    if (!string.IsNullOrEmpty(item.SalaryCalculateMethodName)) item.salaryCategoryName = $"{item.salaryCategoryName} ({item.SalaryCalculateMethodName})";
                    var cell1 = new TableCell(new Paragraph(createFormattedRun($"{item.salaryCategoryName}", sampleRunProperties)));
                    var cell2 = new TableCell(new Paragraph(createFormattedRun($"{item.amount.ToString(GlobalConstants.FORMAT_CURRENCY)}", sampleRunProperties)));
                    dataRow.Append(cell1,cell2);
                    table.Append(dataRow);
                }
                TableRow dataRowLast = new TableRow();
                dataRowLast.Append(
                    new TableCell(new Paragraph(createFormattedRun($"Tổng cộng", sampleRunProperties, true))),
                    new TableCell(new Paragraph(createFormattedRun($"{totalSalary.ToString(GlobalConstants.FORMAT_CURRENCY)}", sampleRunProperties, true)))
                );
                table.Append(dataRowLast);
            }    
            foreach (var text in body.Descendants<Text>())
            { 
                if (text.Text.Contains("ContractCode")) text.Text = text.Text.Replace("ContractCode", contract.contractCode);
                if (text.Text.Contains("##FullName##")) text.Text = text.Text.Replace("##FullName##", contract.employeeName);
                if (text.Text.Contains("##BirthDate##")) text.Text = text.Text.Replace("##BirthDate##", contract.dateOfBirth?.ToString(GlobalConstants.FORMAT_DATE));
                if (text.Text.Contains("##IdentifyNumber##")) text.Text = text.Text.Replace("##IdentifyNumber##", contract.cIC);      
                if (text.Text.Contains("##IdentifyNumberIssuedDate##")) text.Text = text.Text.Replace("##IdentifyNumberIssuedDate##", contract.issuanceDateCIC?.ToString(GlobalConstants.FORMAT_DATE));      
                if (text.Text.Contains("##IdentifyNumberIssuedPlace##")) text.Text = text.Text.Replace("##IdentifyNumberIssuedPlace##", contract.placeOfIssuanceCIC);
                if (text.Text.Contains("##PermanentAddress##")) text.Text = text.Text.Replace("##PermanentAddress##", contract.placeOfResidence);
                if (text.Text.Contains("##PermanentAddress##")) text.Text = text.Text.Replace("##PermanentAddress##", contract.placeOfResidence);
                if (text.Text.Contains("##ContractType##")) text.Text = text.Text.Replace("##ContractType##", contract.contractTypeName);
                if (text.Text.Contains("##ContractPeriod##")) text.Text = text.Text.Replace("##ContractPeriod##", $"{contract.numberOfMonths} tháng");
                if (text.Text.Contains("##StartDate##")) text.Text = text.Text.Replace("##StartDate##", contract.startDate?.ToString(GlobalConstants.FORMAT_DATE));
                if (text.Text.Contains("##EndDate##")) text.Text = text.Text.Replace("##EndDate##", contract.endDate?.ToString(GlobalConstants.FORMAT_DATE));
                if (text.Text.Contains("##JobPositionName##")) text.Text = text.Text.Replace("##JobPositionName##", contract.titleName);
                if (text.Text.Contains("##OrganizationUnitName##")) text.Text = text.Text.Replace("##OrganizationUnitName##", contract.branchName);
                if (text.Text.Contains("##SALARY_DECIDES##")) text.Text = text.Text.Replace("##SALARY_DECIDES##", (lstSalaryConfig.FirstOrDefault(m => m.isPrintSalary)?.amount ?? 0).ToString(GlobalConstants.FORMAT_CURRENCY));
            }
        }

        /// <summary>
        /// fill dữ liệu phụ lục hợp đồng
        /// </summary>
        /// <param name="body"></param>
        /// <param name="dataList"></param>
        private void fillContractAppendixToFile(Body body, IEnumerable<object> dataList)
        {
            ContractAppendixModel contract = dataList.Cast<ContractAppendixModel>().First();
            List<SalaryConfigurationModel> lstSalaryConfig = JsonConvert.DeserializeObject<List<SalaryConfigurationModel>>(contract.jsonDetail!)!; // nếu không có cho rớt catch
            Table? table = body.Elements<Table>().FirstOrDefault();
            if (table != null)
            {
                // Tìm một dòng mẫu từ bảng để lấy định dạng
                var sampleRow = table.Descendants<TableRow>().Skip(1).FirstOrDefault(); // Bỏ qua dòng tiêu đề
                var sampleRunProperties = sampleRow!.Descendants<RunProperties>().FirstOrDefault();
                sampleRow.Remove();
                decimal totalSalary = 0;
                foreach (var item in lstSalaryConfig)
                {
                    if (!item.isPrintContract) continue;
                    totalSalary += item.amount;
                    TableRow dataRow = new TableRow();
                    // Tạo ô cho "Nội dung"
                    if (!string.IsNullOrEmpty(item.SalaryCalculateMethodName)) item.salaryCategoryName = $"{item.salaryCategoryName} ({item.SalaryCalculateMethodName})";
                    var cell1 = new TableCell(new Paragraph(createFormattedRun($"{item.salaryCategoryName}", sampleRunProperties)));
                    var cell2 = new TableCell(new Paragraph(createFormattedRun($"{item.amount.ToString(GlobalConstants.FORMAT_CURRENCY)}", sampleRunProperties)));
                    dataRow.Append(cell1, cell2);
                    table.Append(dataRow);
                }
                TableRow dataRowLast = new TableRow();
                dataRowLast.Append(
                    new TableCell(new Paragraph(createFormattedRun($"Tổng cộng", sampleRunProperties, true))),
                    new TableCell(new Paragraph(createFormattedRun($"{totalSalary.ToString(GlobalConstants.FORMAT_CURRENCY)}", sampleRunProperties, true)))
                );
                table.Append(dataRowLast);
            }
            foreach (var text in body.Descendants<Text>())
            {
                if (text.Text.Contains("ContractApendixCode")) text.Text = text.Text.Replace("ContractApendixCode", contract.contractAppendixCode);
                if (text.Text.Contains("##FullName##")) text.Text = text.Text.Replace("##FullName##", contract.employeeName);
                if (text.Text.Contains("##BirthDate##")) text.Text = text.Text.Replace("##BirthDate##", contract.dateOfBirth?.ToString(GlobalConstants.FORMAT_DATE));
                if (text.Text.Contains("##IdentifyNumber##")) text.Text = text.Text.Replace("##IdentifyNumber##", contract.cIC);
                if (text.Text.Contains("##IdentifyNumberIssuedDate##")) text.Text = text.Text.Replace("##IdentifyNumberIssuedDate##", contract.issuanceDateCIC?.ToString(GlobalConstants.FORMAT_DATE));
                if (text.Text.Contains("##IdentifyNumberIssuedPlace##")) text.Text = text.Text.Replace("##IdentifyNumberIssuedPlace##", contract.placeOfIssuanceCIC);
                if (text.Text.Contains("##PermanentAddress##")) text.Text = text.Text.Replace("##PermanentAddress##", contract.placeOfResidence);
                if (text.Text.Contains("##PermanentAddress##")) text.Text = text.Text.Replace("##PermanentAddress##", contract.placeOfResidence);
                if (text.Text.Contains("##ContractCode##")) text.Text = text.Text.Replace("##ContractCode##", $"{contract.contractCode}");
                if (text.Text.Contains("##StartDate##")) text.Text = text.Text.Replace("##StartDate##", contract.dateOfSigning?.ToString(GlobalConstants.FORMAT_DATE));
                if (text.Text.Contains("##SALARY_DECIDES##")) text.Text = text.Text.Replace("##SALARY_DECIDES##", (lstSalaryConfig.FirstOrDefault(m => m.isPrintSalary)?.amount ?? 0).ToString(GlobalConstants.FORMAT_CURRENCY));
            }
        }

        /// <summary>
        /// Hàm hỗ trợ: Sao chép định dạng từ RunProperties
        /// </summary>
        /// <param name="text"></param>
        /// <param name="sampleRunProperties"></param>
        /// <returns></returns>
        private Run createFormattedRun(string text, RunProperties? sampleRunProperties, bool isFontWeightBold = false)
        {
            // Tạo một đối tượng Run mới
            var run = new Run(new Text(text));

            // Sao chép định dạng từ sampleRunProperties nếu có
            if (sampleRunProperties != null)
            {
                var newRunProperties = (RunProperties)sampleRunProperties.CloneNode(true);
                run.PrependChild(newRunProperties);
            }
            if (isFontWeightBold)
            {
                var boldElement = new Bold();
                run.RunProperties = run.RunProperties ?? new RunProperties();
                run.RunProperties.Append(boldElement);  // Thêm in đậm vào RunProperties
            }
            return run;
        }
        #endregion
    }
}
