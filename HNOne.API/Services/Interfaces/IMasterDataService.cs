using HNOne.Model;
using HNOne.Model.Entities;

namespace HNOne.API.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<IEnumerable<Menus>> GetMenu();
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
        Task<ResponseModel> UpdateContractType(string actionType, ContractTypes entity);
        Task<ResponseModel> UpdateReasonCategorie(string actionType, ReasonCategories entity);

    }
}
