using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.Web.Services.Interfaces
{
    public interface IMasterDataService
    {
        Task<List<Menus>?> GetMenuAsync(RequestModel request);
        Task<List<BranchModel>?> GetBranchAsync(int userId, string token = "");
        Task<bool> UpdateBranchAsync(string processKey, int userId, string token, string json);
    }
}
