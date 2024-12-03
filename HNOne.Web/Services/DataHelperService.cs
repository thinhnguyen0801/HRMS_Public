namespace HNOne.Web.Services
{
    public class DataHelperService
    {
        public Dictionary<string, object> ListUris { get; set; } = new Dictionary<string, object>();
        public DataHelperService()
        {
            ListUris.Add("LeaveRequests", "de-nghi-nghi-phep?key=");
            ListUris.Add("LeaveWorkingHours", "xin-nghi-trong-gio?key=");
            ListUris.Add("Contracts", "chi-tiet-hop-dong?key=");
            ListUris.Add("ContractAppendices", "chi-tiet-phu-luc-hop-dong?key=");
            ListUris.Add("ShiftChanges", "dang-ky-doi-ca?key=");
            ListUris.Add("OvertimeRequests", "de-nghi-lam-them?key=");
        }
    }
}
