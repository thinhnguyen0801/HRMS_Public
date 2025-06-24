using HNOne.Web.Components.Controls;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using HNOne.Web.Services.Interfaces;
using HNOne.Web.Commons;
using DevExpress.Blazor;
using HNOne.Model.Models;
using HNOne.Model;
using HNOne.Web.Models;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using HNOne.Common;
using HNOne.Web.Services;
using Microsoft.AspNetCore.Components.Forms;
using System.Data;
using Microsoft.AspNetCore.Hosting;
using System.ComponentModel;
using OfficeOpenXml;
using DevExpress.XtraExport.Implementation;
using Blazored.Toast.Services;
using DevExpress.XtraPrinting.Native;

namespace HNOne.Web.Controllers
{
    public class ImportController : DocumentControllerBase
    {
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IWebHostEnvironment _webHostEnvironment { get; init; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public int ActiveTabIndex { get; set; } = 0;
        public SearchModel ImportUpdate { get; set; } = new SearchModel();
        public DataTable dtData { get; set; } = new DataTable();
        public DataTable dtLogError { get; set; } = new DataTable();
        public List<EnumCatagoryModel>? ListCboCatagory { get; set; } // cbo ds loại danh mục import
        public List<string>? ListFileTemp { get; set; } // ds hình tạm -> khi chọn lưu vào đây'
        public string filePath { get; set; } = "";
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    string errMessage = await CheckMenuPermissionAsync("import-data");
                    if (errMessage == "401") return; // kiểm quyền menu page danh sách
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Hệ thống"),
                        new BreadcrumbModel("Import dữ liệu", isActive: true)
                    };
                    ListCboCatagory = await _masterDataService.GetFunEnumAsync(UserId, Token, nameof(EnumCatagory.DanhMucImport)); // ds trạng thái
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "OnAfterRenderAsync");
                    ShowError(ex.Message);
                }
                finally
                {
                    await ShowLoading(false);
                    await InvokeAsync(StateHasChanged);
                }
            }    
                
        }

        #region Protected
        protected async void OnLoadFileHandler(InputFileChangeEventArgs args)
        {
            try
            {
                dtData = new DataTable();
                dtLogError = new DataTable();
                if (args.FileCount <= 0) return;
                await ShowLoading();
                ListFileTemp ??= new List<string>();
                var rootFolder = Path.Combine(_webHostEnvironment!.WebRootPath, "Upload", "Temps");
                //tạo thư mục
                if (!Directory.Exists(rootFolder)) Directory.CreateDirectory(rootFolder);
                var file = args.GetMultipleFiles().First();
                string fileNameNew = $"{Guid.NewGuid()}---{file.Name}";
                filePath = Path.Combine(rootFolder, fileNameNew);
                await using FileStream fs = new(filePath, FileMode.Create);
                await file.OpenReadStream(long.MaxValue).CopyToAsync(fs);
                await fs.FlushAsync();
                await fs.DisposeAsync();
                ListFileTemp.Add(filePath);
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "OnLoadFileHandler");
                _toastService!.ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(75);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task RefreshHandler()
        {
            try
            {
                var itemSelect = ListCboCatagory?.FirstOrDefault(m => m.code == ImportUpdate.code);
                if (itemSelect == null)
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại danh mục"));
                    return;
                }
                if (string.IsNullOrEmpty(filePath))
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Đường dẫn file"));
                    return;
                }
                dtData = new DataTable();
                dtLogError = new DataTable();
                await ShowLoading();
                FileInfo fileInfo = new FileInfo(filePath);
                //ExcelPackage
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;
                using (ExcelPackage excelPackage = new ExcelPackage(fileInfo))
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets[0];
                    if (worksheet == null) return;
                    int exCols = worksheet.Dimension.End.Column; //tổng số cột trên file excel
                    int exRows = worksheet.Dimension.End.Row; //tổng số dòng trên file excel
                    if (exRows < 1)
                    {
                        ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                        return;
                    }
                    DataTable excelasTable = new DataTable();
                    // Thêm danh sách các cột
                    for (int col = 1; col <= exCols; col++)
                    {
                        if (worksheet.Cells[1, col]?.Value + "" == "" || worksheet.Cells[2, col]?.Value + "" == "")
                        {
                            ShowWarning("File excel có định dạng không hợp lệ. Bạn vui lòng tải lại file !");
                            return;
                        }
                        DataColumn dtColumn = new DataColumn();
                        dtColumn.ColumnName = $"{worksheet.Cells[1, col].Value}";
                        dtColumn.Caption = $"{worksheet.Cells[2, col].Value}";
                        dtColumn.AllowDBNull = true;
                        dtColumn.DataType = typeof(string);
                        excelasTable.Columns.Add(dtColumn);
                    }
                    var startRow = 3; // bỏ hai cái header -> bắt đầu đọc dòng thứ 3
                    for (int rowNum = startRow; rowNum <= exRows; rowNum++)
                    {
                        var wsRow = worksheet.Cells[rowNum, 1, rowNum, excelasTable.Columns.Count];
                        DataRow row = excelasTable.Rows.Add();
                        foreach (var cell in wsRow)
                        {
                            row[cell.Start.Column - 1] = cell.Text;
                        }
                    }
                    dtData = excelasTable;
                }
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ReLoadDataHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        protected async Task SaveDataHandler()
        {
            try
            {
                var itemSelect = ListCboCatagory?.FirstOrDefault(m => m.code == ImportUpdate.code);
                if (itemSelect == null)
                {
                    ShowWarning(string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Loại danh mục"));
                    return;
                }
                if(dtData == null || dtData.Rows.Count < 1)
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }    
                bool isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, string.Format(MessageConstants.MESSAGE_CONFIRM_ADD_FORMAT, itemSelect.name));
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.branchId = BranchId;
                request.token = Token;
                request.userId = UserId;
                request.process = ProcessConstants.POST_IMPORT_DATA;
                request.opt = ImportUpdate.code;
                request.json = JsonConvert.SerializeObject(dtData);
                var strResult = await _masterDataService.ImportDataAsync(request);
                if (strResult == "-1") return;
                if (strResult == StatusCodes.Status200OK.ToString())
                {
                    dtData = new DataTable();
                    dtLogError = new DataTable();
                    ImportUpdate.code = string.Empty;
                    filePath = string.Empty;
                    return;
                }
                dtLogError = JsonConvert.DeserializeObject<DataTable>(strResult) ?? new DataTable();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SaveDataHandler");
            }
            finally
            {
                // xóa hình ảnh trong folder tạm
                if (ListFileTemp != null && ListFileTemp.Any())
                {
                    foreach (var file in ListFileTemp)
                    {
                        if (File.Exists($"{file}")) File.Delete($"{file}");
                    }
                }
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }
        #endregion
    }
}
