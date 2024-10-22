using HNOne.Model;
using HNOne.Model.Entities;

namespace HNOne.API.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<IEnumerable<Menus>> GetMenu();
        Task<IEnumerable<Branchs>> GetBranch();
        Task<ResponseModel> UpdateBranch(string actionType, Branchs branch);
    }
}
