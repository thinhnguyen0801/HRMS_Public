using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Services.Interfaces
{
    public interface IPersonnelService
    {
        Task<List<EmployeeModel>?> GetEmployeeAsync(RequestModel request);
        Task<int> UpdateEmployeeAsync(string processKey, int userId, string token, string json, bool isCreateAccount = false);
        Task<int> UpdateContractAsync(string processKey, int userId, string token, int branchId, string json, string jsonDetail, bool isShowToast = true);
        Task<List<ContractModel>?> GetContractAsync(RequestModel request, bool isShowToast = false);
        Task<List<InsuranceModel>?> GetInsuranceAsync(RequestModel request, bool isShowToast = false);
        Task<bool> UpdateInsuranceAsync(string processKey, int userId, string token, string json);
        Task<bool> UpdateFamilyRelationshipAsync(string processKey, int userId, string token, string json);
        Task<List<FamilyRelationshipModel>?> GetFamilyRelationshipAsync(RequestModel request, bool isShowToast = false);
        Task<List<ContractAppendixModel>?> GetContractAppendixAsync(RequestModel request, bool isShowToast = false);
        Task<List<LevelOfEducationModel>?> GetEducationAsync(RequestModel request, bool isShowToast = false);
        Task<ResponseModel?> CheckDataAsync(RequestModel request);
    }
}
