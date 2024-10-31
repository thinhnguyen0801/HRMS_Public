using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Services.Interfaces
{
    public interface IPersonnelService
    {
        Task<IEnumerable<EmployeeModel>> GetEmployee(RequestModel request);
        Task<ResponseModel> UpdateEmployee(string actionType, Employees entity);
        Task<ResponseModel> UpdateContract(string actionType, Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
        Task<IEnumerable<ContractModel>> GetContract(RequestModel request);
    }
}
