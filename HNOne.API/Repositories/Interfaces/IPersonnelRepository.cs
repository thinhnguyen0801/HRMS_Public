using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IPersonnelRepository
    {
        Task<IEnumerable<EmployeeModel>> GetEmployee(RequestModel request);
        Task<ResponseModel> AddEmployee(Employees entity, bool isCreateAccount = false);
        Task<ResponseModel> UpdateEmployee(Employees entity, bool isCreateAccount = false);
        Task<ResponseModel> AddContract(Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
        Task<ResponseModel> UpdateContract(Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
        Task<IEnumerable<ContractModel>> GetContract(RequestModel request);
        Task<ResponseModel> AddFamilyRelationship(FamilyRelationships entity);
        Task<ResponseModel> UpdateFamilyRelationship(FamilyRelationships entity);
        Task<IEnumerable<FamilyRelationshipModel>> GetFamilyRelationship(int employeeId);
        Task<IEnumerable<InsuranceModel>> GetInsurance(int employeeId);
        Task<ResponseModel> UpdateInsurance(string actionType, Insurances entity);
        Task<ResponseModel> AddContractAppendix(ContractAppendices entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
        Task<ResponseModel> UpdateContractAppendix(ContractAppendices entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
        Task<IEnumerable<ContractAppendixModel>> GetContractAppendix(RequestModel request);
        Task<ResponseModel> UpdateEducation(string actionType, LevelOfEducations entity);
        Task<IEnumerable<LevelOfEducationModel>> GetEducation(int employeeId);
        Task<ResponseModel> CheckExistsData(RequestModel request);
    }
}
