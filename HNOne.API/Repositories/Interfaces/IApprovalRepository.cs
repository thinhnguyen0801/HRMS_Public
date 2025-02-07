using HNOne.Model.Entities;
using HNOne.Model;
using HNOne.Model.Models;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IApprovalRepository
    {
        Task<IEnumerable<ApprovalModel>> GetApproval(RequestModel request);
        Task<ResponseModel> AddApproval(ApprovalModel entity);
        Task<ResponseModel> UpdateApproval(string actionType, IEnumerable<ApprovalModel> lstEntity);
        Task<ResponseModel> GetFnDocumentHistory(RequestModel request);
        Task<ResponseModel> CancelDocument(IEnumerable<ApprovalModel> lstEntity);
    }
}
