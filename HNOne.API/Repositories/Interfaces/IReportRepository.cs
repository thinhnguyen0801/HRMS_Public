using HNOne.Model;

namespace HNOne.API.Repositories.Interfaces
{
    public interface IReportRepository
    {
        Task<IEnumerable<dynamic>> GetRptPayrollSummary(RequestModel request);
    }
}
