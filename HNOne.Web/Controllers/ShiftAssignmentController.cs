using DevExpress.Blazor;
using HNOne.Common;
using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using HNOne.Web.Commons;
using HNOne.Web.Components.Controls;
using HNOne.Web.Models;
using HNOne.Web.Services.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace HNOne.Web.Controllers
{
    public class ShiftAssignmentController : DocumentControllerBase
    {
        [Inject] IWorkforceService _workforceService { get; init; }
        [Inject] IMasterDataService _masterDataService { get; init; }
        [Inject] IJSRuntime _jsRuntime { get; set; }
        public W1Confirm confirm { get; set; }
        #region Properties
        public SearchModel SearchUpdate { get; set; } = new SearchModel();
        public List<ShiftAssignmentModel>? ListShiftAssignment { get; set; }
        public IGrid? GridShiftAssignment { get; set; }


        public List<ComboboxModel>? ListCboBranch { get; set; } // cbo ds chi nhánh
        public List<ComboboxModel>? ListCboStatus { get; set; } // cbo ds tình trạng
        public IEnumerable<ComboboxModel>? ListCboStatusSelected { get; set; }
        public List<EnumCatagoryModel>? ListCboShift { get; set; } // cbo ds ca làm việc
        public List<ComboboxModel>? ListCboShiftPreiod { get; set; } // kì sắp ca làm việc

        public List<DepartmentModel>? ListDepartmentSearch { get; set; } // danh sách phòng ban
        public IGrid? GridDepartmentSearch { get; set; }
        public IReadOnlyList<object>? SelectedDataDepartments { get; set; }

        private List<ComboboxModel>? lstDayOffInMonth { get; set; } // danh sách ngày nghỉ
        public int MaxDaysInMonth { get; set; } = 30; // max số ngày trong tháng
        public bool IsShowFilter { get; set; } = true; // mở rộng vùng tìm kiếm
        #endregion

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            if (firstRender)
            {
                try
                {
                    await ShowLoading();
                    ListBreadcrumbs = new List<BreadcrumbModel>()
                    {
                        new BreadcrumbModel("Công - Phép"),
                        new BreadcrumbModel("Chứng từ đề nghị"),
                        new BreadcrumbModel("Phân công ca làm việc", isActive: true),
                    };
                    await NotifyBreadcrumb.InvokeAsync(ListBreadcrumbs);

                    //
                    await buildComboAsync();
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

        #region Private Functions
        /// <summary>
        /// lấy dữ liệu cho combobox
        /// </summary>
        /// <returns></returns>
        private async Task buildComboAsync()
        {
            try
            {
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.GET_WORKFORCE_MASTER_DATA;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.type = ProcessConstants.GET_COMBO_LIST_SHIFT_PREIOD;
                
                var getTask1 = _masterDataService.GetDepartmentAsync(UserId, Token, BranchId, opt: CommonConstants.ENUM_ACTIVE); // ds phòng ban
                var getTask2 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.CaLamViec)); // ds trạng thái
                var getTask3 = _workforceService.GetMasterDataAsync<ComboboxModel>(request, isShowToast: true);
                var getTask4 = _masterDataService.GetBranchAsync(UserId, Token, BranchId, $"{BranchIds}");
                var getTask5 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiNhanVien)); // ds trạng thái
                var getTask6 = _masterDataService.GetEnumAsync(UserId, Token, nameof(EnumCatagory.TrangThaiPhatSinhCong)); // ds trạng thái cho phép phát sinh công
                await Task.WhenAll(
                    getTask1,
                    getTask2,
                    getTask3,
                    getTask4,
                    getTask5
                );
                ListDepartmentSearch = await getTask1;
                ListCboShift = await getTask2;
                ListCboShiftPreiod = await getTask3;
                ListCboBranch = (await getTask4)?.Select(m => new ComboboxModel() { id = m.branchId, name = m.branchName })?.ToList();
                ListCboStatus = (await getTask5)?.Where(m => m.rowOrder != 0).Select(m => new ComboboxModel() { code = m.code, name = m.name })?.ToList();
                SearchUpdate.shiftPreiodId = DateTime.Now.ToString("yyyy-MM");
                SearchUpdate.branchId = BranchId;
                MaxDaysInMonth = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);
                // gán dữ liệu mặc định
                string[]? statusIds = $"{(await getTask6)?.FirstOrDefault()?.value}".Split(",");
                if (!statusIds.IsNullOrEmpty()
                    && !ListCboStatus.IsNullOrEmpty())
                {
                    ListCboStatusSelected = ListCboStatus!.Where(m => statusIds.Contains(m.code));
                }
            }
            catch (Exception) { throw; }
        }

        private void validateForSave(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(SearchUpdate.shiftPreiodId))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Kỳ công");
                fieldName = "pShiftPreiodId";
                return;
            }
        }

        /// <summary>
        /// kiểm tra dữ liệu kỳ công
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="fieldName"></param>
        private void validateForGenerateTimeSheets(ref string errorMessage, ref string fieldName)
        {
            if (string.IsNullOrEmpty(SearchUpdate.shiftPreiodId))
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_COMBOBOX_REQUIRE, "Kỳ công");
                fieldName = "pShiftPreiodId";
                return;
            }  
            if(ListDepartmentSearch.IsNullOrEmpty())
            {
                errorMessage = string.Format(MessageConstants.MESSAGE_NOT_FOUNT_FORMAT, "Phòng ban");
                fieldName = "gridDepartment";
            }    
        }

        /// <summary>
        /// lấy dữ liệu bảng công
        /// </summary>
        /// <returns></returns>
        private async Task getTimeSheet()
        {
            ListShiftAssignment = new List<ShiftAssignmentModel>();
            var arrShiftPreiod = SearchUpdate.shiftPreiodId!.Split('-');
            int year = int.Parse(arrShiftPreiod[0]);
            int month = int.Parse(arrShiftPreiod[1]);
            MaxDaysInMonth = DateTime.DaysInMonth(year, month);
            RequestModel request = new RequestModel();
            request.process = ProcessConstants.GET_ARRANGE_SHIFT;
            request.userId = UserId;
            request.branchId = BranchId;
            request.token = Token;
            request.departmentIds = SelectedDataDepartments.IsNullOrEmpty() ? "" : string.Join(",", SelectedDataDepartments!.Cast<DepartmentModel>().Select(m => m.id));
            request.year = year;
            request.month = month;
            var response = await _workforceService.GetMasterDataAsync<ShiftAssignmentModel>(request, isShowToast: true);
            if(!response.IsNullOrEmpty())
            {
                ListShiftAssignment = response;
                string? jsonDetail =  response!.FirstOrDefault(m => !string.IsNullOrEmpty(m.jsonDetail))?.jsonDetail;
                if(!string.IsNullOrEmpty(jsonDetail)) lstDayOffInMonth = JsonConvert.DeserializeObject<List<ComboboxModel>>(jsonDetail);
            }    
        }
        #endregion

        #region Protected Functions
        protected async Task RefreshDataHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                validateForSave(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                await ShowLoading();
                await getTimeSheet();
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "RefreshDataHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// điều chỉnh dữ liệu trong lưới -> lưu lại
        /// </summary>
        /// <param name="e"></param>
        protected void GridShiftAssignmentEditSavingHandler(GridEditModelSavingEventArgs e)
        {
            try
            {
                var itemEdit = (ShiftAssignmentModel)e.EditModel;
                var itemFind = ListShiftAssignment?.FirstOrDefault(m => m.employeeId == itemEdit.employeeId && m.employeeCode == itemEdit.employeeCode);
                if (itemFind == null) return;
                itemFind.n01 = itemEdit.n01;
                itemFind.n02 = itemEdit.n02;
                itemFind.n03 = itemEdit.n03;
                itemFind.n04 = itemEdit.n04;
                itemFind.n05 = itemEdit.n05;
                itemFind.n06 = itemEdit.n06;
                itemFind.n07 = itemEdit.n07;
                itemFind.n08 = itemEdit.n08;
                itemFind.n09 = itemEdit.n09;
                itemFind.n10 = itemEdit.n10;
                itemFind.n11 = itemEdit.n11;
                itemFind.n12 = itemEdit.n12;
                itemFind.n13 = itemEdit.n13;
                itemFind.n14 = itemEdit.n14;
                itemFind.n15 = itemEdit.n15;
                itemFind.n16 = itemEdit.n16;
                itemFind.n17 = itemEdit.n17;
                itemFind.n18 = itemEdit.n18;
                itemFind.n19 = itemEdit.n19;
                itemFind.n20 = itemEdit.n20;
                itemFind.n21 = itemEdit.n21;
                itemFind.n22 = itemEdit.n22;
                itemFind.n23 = itemEdit.n23;
                itemFind.n24 = itemEdit.n24;
                itemFind.n25 = itemEdit.n25;
                itemFind.n26 = itemEdit.n26;
                itemFind.n27 = itemEdit.n27;
                itemFind.n28 = itemEdit.n28;
                itemFind.n29 = itemEdit.n29;
                itemFind.n30 = itemEdit.n30;
                itemFind.n31 = itemEdit.n31;
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "GridShiftAssignmentEditSavingHandler");
            }
        }

        /// <summary>
        /// lưu lại thông tin ca làm việc của nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task SaveDataHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                validateForSave(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                if(ListShiftAssignment.IsNullOrEmpty())
                {
                    ShowWarning("Không tìm thấy dữ liệu công làm việc!!!");
                    return;
                }    
                bool isConfirm = true;
                await Task.Yield();
                errorMessage = string.Format(MessageConstants.MESSAGE_CONFIRM_UPDATE_FORMAT, "Phân công ca làm việc");
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                var arrShiftPreiod = SearchUpdate.shiftPreiodId!.Split('-');
                ListShiftAssignment.Update(m=>
                {
                    m.userSign = UserId;
                    m.userSign2 = UserId;
                });
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_SHIFT_ASSIGNMENT;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.json = JsonConvert.SerializeObject(ListShiftAssignment);
                //request.opt = pDepartmentId.ToString();
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if (isConfirm)
                {
                    await getTimeSheet();
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "SaveDataHandler");
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// phát sinh phát sinh công làm việc của nhân viên
        /// </summary>
        /// <returns></returns>
        protected async Task GenerateWorkHandler()
        {
            try
            {
                string errorMessage = string.Empty;
                string fieldName = string.Empty; // trả ra trường nào cần validate
                validateForGenerateTimeSheets(ref errorMessage, ref fieldName);
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    ShowWarning(errorMessage);
                    await _jsRuntime.InvokeVoidAsync("focusInput", fieldName);
                    return;
                }
                bool isConfirm = true;
                string? nameShiftPreiodId = ListCboShiftPreiod!.FirstOrDefault(m => m.code == SearchUpdate.shiftPreiodId)?.name;
                string departmentName = SelectedDataDepartments.IsNullOrEmpty() ? string.Join("<br /> - ", ListDepartmentSearch!.Select(m => m.name))
                    : string.Join("<br /> - ", SelectedDataDepartments!.Cast<DepartmentModel>().Select(m => m.name));
                errorMessage = errorMessage = $"Bạn có chắc muốn phát sinh dữ liệu công của kỳ [{nameShiftPreiodId}] <br /> Cho tất cả nhân viên trong phòng ban: <br />- {departmentName}";
                await Task.Yield();
                isConfirm = await confirm.SetConfirm(MessageConstants.MESSAGE_TITLE, errorMessage);
                if (!isConfirm) return;
                await ShowLoading();
                RequestModel request = new RequestModel();
                request.process = ProcessConstants.POST_TIME_SHEET;
                request.userId = UserId;
                request.branchId = BranchId;
                request.token = Token;
                request.fromDate = DateTime.Parse(nameShiftPreiodId +"-01");
                request.departmentIds = SelectedDataDepartments.IsNullOrEmpty() ? "" : string.Join(",", SelectedDataDepartments!.Cast<DepartmentModel>().Select(m => m.id));
                isConfirm = await _workforceService.UpdateMasterDataAsync(request);
                if (isConfirm) await getTimeSheet();
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "GenerateWorkHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await Task.Delay(50);
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Kết xuất dữ liệu sang file excel
        /// xlsx
        /// </summary>
        /// <returns></returns>
        protected async Task ExportExcelHandler()
        {
            try
            {
                if (GridShiftAssignment == null || ListShiftAssignment.IsNullOrEmpty())
                {
                    ShowWarning(MessageConstants.MESSAGE_NOT_FOUNT);
                    return;
                }
                await ShowLoading();
                await GridShiftAssignment!.ExportToXlsxAsync("Phan-cong-ca-lam-viec", new GridXlExportOptions()
                {
                    ExportTotalSummaries = false,
                    ExportGroupSummaries = false
                });
            }
            catch (Exception ex)
            {
                _logger!.LogError(ex, "ExportExcelHandler");
                ShowError(ex.Message);
            }
            finally
            {
                await ShowLoading(false);
                await InvokeAsync(StateHasChanged);
            }
        }

        /// <summary>
        /// Mở rộng & thu gọn vùng tìm kiếm
        /// </summary>
        protected void ShowFilterHandler() => IsShowFilter = !IsShowFilter;

        /// <summary>
        /// render ra màu cho dòng nào là chủ nhật
        /// </summary>
        /// <param name="arg"></param>
        protected void GridCustomizeElement(GridCustomizeElementEventArgs arg)
        {
            try
            {
                if (arg.ElementType == GridElementType.DataCell)
                {
                    var item = lstDayOffInMonth?.FirstOrDefault(m => $"{m.code}" == arg.Column.Name);
                    if(item != null)
                    {
                        arg.Style = $"background-color: {item.value}";
                    }    
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
                _logger.LogError(ex, "GridCustomizeElement");
            }
        }
        #endregion
    }
}
