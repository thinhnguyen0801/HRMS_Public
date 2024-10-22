using HNOne.Model;
using HNOne.Model.Entities;

namespace HNOne.API.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<IEnumerable<Menus>> GetMenu();
        Task<IEnumerable<Branchs>> GetBranch();
        Task<ResponseModel> UpdateBranch(string actionType, Branchs branch);
        Task<ResponseModel> UpdateDepartment(string actionType, Departments entity);
        Task<ResponseModel> UpdatePosition(string actionType, Positions entity);
        Task<ResponseModel> UpdateTitle(string actionType, Titles entity);

    }
}
