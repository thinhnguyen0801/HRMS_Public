using HNOne.Model;
using HNOne.Model.Entities;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IPersonnelRepository
    {
        Task<IEnumerable<EmployeeModel>> GetEmployee();
        Task<ResponseModel> AddEmployee(Employees entity);
    }
}
