using HNOne.Model.Entities;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IMasterDataRepository
    {
        Task<IEnumerable<Menus>> GetMenu();
        Task<IEnumerable<Branchs>> GetBranch();
    }
}
