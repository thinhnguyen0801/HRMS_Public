using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;

namespace HNOne.API.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<IEnumerable<MenuModel>> GetMenu(RequestModel request);
        Task<IEnumerable<Branchs>> GetBranch();
        Task<IEnumerable<Departments>> GetDepartment(RequestModel request);
        Task<IEnumerable<Titles>> GetTitle(RequestModel request);
        Task<IEnumerable<Positions>> GetPosition(RequestModel request);
        Task<IEnumerable<ContractTypes>> GetContractType(RequestModel request);
        Task<IEnumerable<ReasonCategories>> GetReasonCategorie(RequestModel request);
        Task<ResponseModel> UpdateBranch(string actionType, Branchs branch);
        Task<ResponseModel> UpdateDepartment(string actionType, Departments entity);
        Task<ResponseModel> UpdatePosition(string actionType, Positions entity);
        Task<ResponseModel> UpdateTitle(string actionType, Titles entity);
        Task<IEnumerable<EnumCatagories>> GetEnum(string enumType);
        Task<ResponseModel> UpdateContractType(string actionType, ContractTypes entity);
        Task<ResponseModel> UpdateReasonCategorie(string actionType, ReasonCategories entity);
        Task<IEnumerable<SalaryCategories>> GetSalaryCatagory(RequestModel request);
        Task<ResponseModel> UpdateSalaryCategory(string actionType, SalaryCategories entity);
        Task<IEnumerable<SalaryConfigurationModel>> GetSalaryConfig();
        Task<ResponseModel> UpdateSalaryConfig(string actionType, SalaryConfigurations entity);
        Task<string?> GetDocumentNo(string? type, string? opt = "", string? opt1 = "", string? opt2 = "");
    }
}
