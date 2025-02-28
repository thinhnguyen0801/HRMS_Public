using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;
using System.Data;

namespace HNOne.API.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<IEnumerable<MenuModel>> GetMenu(RequestModel request);
        Task<IEnumerable<Branchs>> GetBranch(RequestModel request);
        Task<IEnumerable<DepartmentModel>> GetDepartment(RequestModel request);
        Task<IEnumerable<TitleModel>> GetTitle(RequestModel request);
        Task<IEnumerable<PositionModel>> GetPosition(RequestModel request);
        Task<IEnumerable<ContractTypeModel>> GetContractType(RequestModel request);
        Task<IEnumerable<ReasonCategorieModel>> GetReasonCategory(RequestModel request);
        Task<ResponseModel> UpdateBranch(string actionType, Branchs branch);
        Task<ResponseModel> UpdateDepartment(string actionType, Departments entity);
        Task<ResponseModel> UpdatePosition(string actionType, Positions entity);
        Task<ResponseModel> UpdateTitle(string actionType, Titles entity);
        Task<IEnumerable<EnumCatagories>> GetEnum(RequestModel request);
        Task<ResponseModel> UpdateContractType(string actionType, ContractTypes entity);
        Task<ResponseModel> UpdateReasonCategorie(string actionType, ReasonCategories entity);
        Task<IEnumerable<SalaryCategories>> GetSalaryCatagory(RequestModel request);
        Task<ResponseModel> UpdateSalaryCategory(string actionType, SalaryCategories entity);
        Task<IEnumerable<SalaryConfigurationModel>> GetSalaryConfig(RequestModel request);
        Task<ResponseModel> UpdateSalaryConfig(string actionType, SalaryConfigurations entity);
        Task<IEnumerable<SalaryParameterModel>> GetSalaryParameter(RequestModel request);
        Task<ResponseModel> UpdateSalaryParameter(string actionType, SalaryParameters entity);
        Task<string?> GetDocumentNo(string? type, int branchId, string? opt = "", string? opt1 = "", string? opt2 = "");
        Task<IEnumerable<ComboboxModel?>> GetLocationData(string? type, string? opt = "", string? opt1 = "", string? opt2 = "");
        Task<IEnumerable<dynamic>?> GetMasterData(RequestModel request);
        Task<IEnumerable<EnumCatagoryModel>> GetFnEnum(RequestModel request);
        Task<ResponseModel> UpdateEnumCatagory(string actionType, EnumCatagories entity);
        Task<ResponseModel> DeleteDynamic(RequestModel request);
        Task<IEnumerable<TaxRateModel>> GetTaxRate(RequestModel request);
        Task<ResponseModel> UpdateTaxRate(string actionType, TaxRates entity);
        Task<ResponseModel> UpdateDeductionConfig(string actionType, DeductionConfigs entity);
        Task<IEnumerable<DeductionConfigModel>> GetDeductionConfig(RequestModel request);
        Task<ResponseModel> ImportData(int branchId, int userId, string processKey, string jData);
        Task<ResponseModel> UpdateStatusNotification(RequestModel request);
        Task<ResponseModel> UpdateWorkingBranch(string actionType, WorkingBranchs entity);
        Task<IEnumerable<WorkingBranchModel>> GetWorkingBranch(RequestModel request);
    }
}
