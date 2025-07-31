using HNOne.Web.Commons;

namespace HNOne.Web.Services
{
    public class DataHelperService
    {
        public Dictionary<string, object> ListUris { get; set; } = new Dictionary<string, object>();
        public DataHelperService()
        {
            ListUris.Add(nameof(EnumObjType.LeaveRequests), "de-nghi-nghi-phep?key=");
            ListUris.Add(nameof(EnumObjType.LeaveWorkingHours), "xin-nghi-trong-gio?key=");
            ListUris.Add(nameof(EnumObjType.Contracts), "chi-tiet-hop-dong?key=");
            ListUris.Add(nameof(EnumObjType.ContractAppendices), "chi-tiet-phu-luc-hop-dong?key=");
            ListUris.Add(nameof(EnumObjType.ShiftChanges), "dang-ky-doi-ca?key=");
            ListUris.Add(nameof(EnumObjType.OvertimeRequests), "de-nghi-lam-them?key=");
            ListUris.Add(nameof(EnumObjType.Trainings), "dao-tao?key=");
            ListUris.Add(nameof(EnumObjType.ConfirmWorkingDays), "xac-nhan-gio-cong?key=");
            ListUris.Add(nameof(EnumObjType.SalaryExpenseAccountings), "hach-toan-chi-phi-luong?key=");
            ListUris.Add(nameof(EnumObjType.AdjustedAnnualLeaveRequests), "dieu-chinh-phep-nam?key=");
            ListUris.Add(nameof(EnumObjType.DecisionDocuments), "chung-tu-quyet-dinh?key=");
            ListUris.Add(nameof(EnumObjType.RewardAllowanceRequests), "khen-thuong-phu-cap?key=");
        }
    }
}
