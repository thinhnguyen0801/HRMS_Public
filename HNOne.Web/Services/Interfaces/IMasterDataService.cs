using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<List<Menus>?> GetMenuAsync(RequestModel request);
        Task<List<BranchModel>?> GetBranchAsync(int userId, string token = "");
        Task<List<DepartmentModel>?> GetDepartmentAsync(int userId, string token = "");
        Task<List<TitleModel>?> GetTitleAsync(int userId, string token = "");
        Task<List<PositionModel>?> GetPositionAsync(int userId, string token = "");
        Task<List<ContractTypeModel>?> GetContractTypeAsync(int userId, string token = "");
        Task<List<ReasonCategorieModel>?> GetReasonCategorieAsync(int userId, string token = "");
        Task<bool> UpdateBranchAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdateDepartmentAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdateTitleAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdatePositionAsync(string processKey, int userId, string token, string json);
        Task<List<EnumCatagoryModel>?> GetEnumAsync(int userId, string token, string? enumType, bool isShowToast = false);
        Task<bool> UpdateContractTypeAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdateReasonCategorieAsync(string processKey, int userId, string token, string json);
    }
}
