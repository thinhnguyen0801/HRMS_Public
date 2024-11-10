using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using System.Data;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IMasterDataRepository
    {
        Task<IEnumerable<MenuModel>> GetMenu(RequestModel request);
        Task<IEnumerable<Branchs>> GetBranch();
        Task<IEnumerable<DepartmentModel>> GetDepartment(RequestModel request);
        Task<IEnumerable<TitleModel>> GetTitle(RequestModel request);
        Task<IEnumerable<PositionModel>> GetPosition(RequestModel request);
        Task<ResponseModel> AddBranch(Branchs entity);
        Task<ResponseModel> UpdateBranch(Branchs entity);
        Task<ResponseModel> AddDepartment(Departments entity);
        Task<ResponseModel> UpdateDepartment(Departments entity);
        Task<ResponseModel> AddPosition(Positions entity);
        Task<ResponseModel> UpdatePosition(Positions entity);
        Task<ResponseModel> AddTitle(Titles entity);
        Task<ResponseModel> UpdateTitle(Titles entity);
        Task<IEnumerable<EnumCatagories>> GetEnum(RequestModel request);
        Task<IEnumerable<ContractTypes>> GetContractType(RequestModel request);
        Task<ResponseModel> AddContractType(ContractTypes entity);
        Task<ResponseModel> UpdateContractType(ContractTypes entity);
        Task<IEnumerable<ReasonCategories>> GetReasonCategorie(RequestModel request);
        Task<ResponseModel> AddReasonCategorie(ReasonCategories entity);
        Task<ResponseModel> UpdateReasonCategorie(ReasonCategories entity);
        Task<IEnumerable<SalaryCategories>> GetSalaryCatagory(RequestModel request);
        Task<ResponseModel> AddSalaryCategory(SalaryCategories entity);
        Task<ResponseModel> UpdateSalaryCategory(SalaryCategories entity);
        Task<IEnumerable<SalaryConfigurationModel>> GetSalarySalaryConfig();
        Task<ResponseModel> UpdateSalaryConfig(SalaryConfigurations entity);
        Task<ResponseModel> AddSalaryConfig(SalaryConfigurations entity);
        Task<string?> GetDocumentNo(string? type, string? opt = "", string? opt1 = "", string? opt2 = "");
        Task<IEnumerable<ComboboxModel?>> GetLocationData(string? type, string? opt = "", string? opt1 = "", string? opt2 = "");
        Task<IEnumerable<dynamic>?> GetMasterData(RequestModel request);
    }
}
