using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IPersonnelRepository
    {
        Task<IEnumerable<EmployeeModel>> GetEmployee(RequestModel request);
        Task<ResponseModel> AddEmployee(Employees entity);
        Task<ResponseModel> UpdateEmployee(Employees entity);
        Task<ResponseModel> AddContract(Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
        Task<ResponseModel> UpdateContract(Contracts entity, IEnumerable<SalaryAdjustments>? lstSalaryConfig);
    }
}
