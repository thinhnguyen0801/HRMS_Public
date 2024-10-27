using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Services.Interfaces
{
    public interface IPersonnelService
    {
        Task<List<EmployeeModel>?> GetEmployeeAsync(RequestModel request);
        Task<bool> UpdateEmployeeAsync(string processKey, int userId, string token, string json);
    }
}
