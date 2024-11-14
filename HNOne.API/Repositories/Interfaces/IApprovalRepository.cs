using HNOne.Model.Entities;
using HNOne.Model;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IApprovalRepository
    {
        Task<ResponseModel> AddApproval(Approvals entity);
        Task<ResponseModel> UpdateApproval(string actionType, Approvals entity);
    }
}
