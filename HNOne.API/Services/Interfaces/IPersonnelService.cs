using HNOne.Model.Entities;
using HNOne.Model;

namespace HNOne.API.Services.Interfaces
{
    public interface IPersonnelService
    {
        Task<ResponseModel> UpdateEmployee(string actionType, Employees entity);
    }
}
