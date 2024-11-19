using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Services.Interfaces
{
    public interface IPersonnelService
    {
        Task<IEnumerable<EmployeeModel>> GetEmployee(RequestModel request);
        Task<ResponseModel> UpdateEmployee(string actionType, Employees entity, bool isCreateAccount = false);
        Task<ResponseModel> UpdateContract(string actionType, Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
        Task<IEnumerable<ContractModel>> GetContract(RequestModel request);
        Task<IEnumerable<FamilyRelationshipModel>> GetFamilyRelationship(int employeeId);
        Task<ResponseModel> UpdateFamilyRelationship(string actionType, FamilyRelationships entity);
        Task<IEnumerable<InsuranceModel>> GetInsurance(int employeeId);
        Task<ResponseModel> UpdateInsurance(string actionType, Insurances entity);
        Task<ResponseModel> UpdateContractAppendix(string actionType, ContractAppendices entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
        Task<IEnumerable<ContractAppendixModel>> GetContractAppendix(RequestModel request);
        Task<ResponseModel> UpdateEducation(string actionType, LevelOfEducations entity);
        Task<IEnumerable<LevelOfEducationModel>> GetEducation(int employeeId);
        Task<ResponseModel> CheckExistsData(RequestModel request);
    }
}
