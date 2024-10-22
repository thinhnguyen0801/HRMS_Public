using HNOne.Model;
using HNOne.Model.Entities;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IMasterDataRepository
    {
        Task<IEnumerable<Menus>> GetMenu();
        Task<IEnumerable<Branchs>> GetBranch();
        Task<ResponseModel> AddBranch(Branchs entity);
        Task<ResponseModel> UpdateBranch(Branchs entity);
        Task<ResponseModel> AddDepartment(Departments entity);
        Task<ResponseModel> UpdateDepartment(Departments entity);
        Task<ResponseModel> AddPosition(Positions entity);
        Task<ResponseModel> UpdatePosition(Positions entity);
        Task<ResponseModel> AddTitle(Titles entity);
        Task<ResponseModel> UpdateTitle(Titles entity);
    }
}
