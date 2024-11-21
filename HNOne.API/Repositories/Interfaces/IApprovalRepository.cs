using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IApprovalRepository
    {
        Task<IEnumerable<ApprovalModel>> GetApproval(RequestModel request);
        Task<ResponseModel> AddApproval(Approvals entity);
        Task<ResponseModel> UpdateApproval(string actionType, IEnumerable<Approvals> lstEntity);
        Task<ResponseModel> GetFnDocumentHistory(RequestModel request);
    }
}
