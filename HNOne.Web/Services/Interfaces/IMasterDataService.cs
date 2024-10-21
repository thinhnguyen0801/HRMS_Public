using HNOne.Model.Entities;
using HNOne.Model;

namespace HNOne.Web.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<List<Menus>?> GetMenuAsync(RequestModel request);
        Task<List<Branchs>?> GetBranchAsync(RequestModel request);
    }
}
