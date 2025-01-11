using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;
using Microsoft.JSInterop;

namespace HNOne.Web.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<List<MenuModel>?> GetMenuAsync(RequestModel request);
        Task<List<BranchModel>?> GetBranchAsync(int userId, string token = "");
        Task<List<DepartmentModel>?> GetDepartmentAsync(int userId, string token, int branchId, string opt = "", string opt2 = "", bool isShowToast = false);
        Task<List<TitleModel>?> GetTitleAsync(int userId, string token, int branchId, bool isShowToast = false);
        Task<List<PositionModel>?> GetPositionAsync(int userId, string token, int branchId, bool isShowToast = false);
        Task<List<ContractTypeModel>?> GetContractTypeAsync(int userId, string token);
        Task<List<ReasonCategorieModel>?> GetReasonCategoryAsync(int userId, string token, string reasonType = "", string opt = "", bool isShowToast = false);
        Task<bool> UpdateBranchAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdateDepartmentAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdateTitleAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdatePositionAsync(string processKey, int userId, string token, string json);
        Task<List<EnumCatagoryModel>?> GetEnumAsync(int userId, string token, string? enumType, bool isShowToast = false);
        Task<bool> UpdateContractTypeAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdateReasonCategorieAsync(string processKey, int userId, string token, string json);
        Task<List<SalaryCategoryModel>?> GetSalaryCatagoryAsync(int userId, string token, string condition = "", bool isShowToast = false);
        Task<bool> UpdateSalaryCategoryAsync(string processKey, int userId, string token, string json);
        Task<List<SalaryConfigurationModel>?> GetSalaryConfigAsync(int userId, string token, int branchId, bool isShowToast = false);
        Task<bool> UpdateSalaryConfigAsync(string processKey, int userId, string token, string json);
        Task<string?> GetDocumentNo(int userId, string token, int branchId, string? type, string? opt = "", string? opt1 = "", string? opt2 = "");
        Task<List<ComboboxModel>?> GetLocationAsync(int userId, string token, string? type, string? opt = "", string? opt1 = "", string? opt2 = "");
        Task<List<T>?> GetMasterDataAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
        Task<List<FileUploadModel>?> UploadImagesAsync(List<FileUploadModel> listImages, string subFolder);
        Task<List<EnumCatagoryModel>?> GetFunEnumAsync(int userId, string token, string enumType
            , string opt = "", string opt1 = "", bool isShowToast = false);
        Task<string> DeleteDynnamicAsync(int userId, string token, int branchId
            , string tableName, string pKey, string fKey, string documentId, string reasonDelete);
        Task<DotNetStreamReference?> PrintDocumentAsync(int userId, string token, int branchId, int documentId, string processKey, string fileName, int documentId2 = -1);
        Task<List<SalaryParameterModel>?> GetSalaryParameterAsync(int userId, string token, int branchId, string condition = "", bool isShowToast = false);
        Task<List<T>?> GetMasterAsync<T>(RequestModel request, bool isShowToast = false) where T : class;
        Task<bool> UpdateMasterAsync(RequestModel request);
    }
}
