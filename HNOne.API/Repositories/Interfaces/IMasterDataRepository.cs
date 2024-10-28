using HNOne.Model;
using HNOne.Model.Entities;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IMasterDataRepository
    {
        Task<IEnumerable<Menus>> GetMenu();
        Task<IEnumerable<Branchs>> GetBranch();
        Task<IEnumerable<Departments>> GetDepartment(RequestModel request);
        Task<IEnumerable<Titles>> GetTitle(RequestModel request);
        Task<IEnumerable<Positions>> GetPosition(RequestModel request);
        Task<ResponseModel> AddBranch(Branchs entity);
        Task<ResponseModel> UpdateBranch(Branchs entity);
        Task<ResponseModel> AddDepartment(Departments entity);
        Task<ResponseModel> UpdateDepartment(Departments entity);
        Task<ResponseModel> AddPosition(Positions entity);
        Task<ResponseModel> UpdatePosition(Positions entity);
        Task<ResponseModel> AddTitle(Titles entity);
        Task<ResponseModel> UpdateTitle(Titles entity);
        Task<IEnumerable<EnumCatagories>> GetEnum(string enumType);
        Task<IEnumerable<ContractTypes>> GetContractType(RequestModel request);
        Task<ResponseModel> AddContractType(ContractTypes entity);
        Task<ResponseModel> UpdateContractType(ContractTypes entity);
        Task<IEnumerable<ReasonCategories>> GetReasonCategorie(RequestModel request);
        Task<ResponseModel> AddReasonCategorie(ReasonCategories entity);
        Task<ResponseModel> UpdateReasonCategorie(ReasonCategories entity);
    }
}
