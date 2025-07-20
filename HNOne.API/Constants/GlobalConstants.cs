namespace HNOne.API.Constants
{
    public class GlobalConstants
    {
        public const int COMMAND_TIMEOUT = 500;

        #region
        public const string TABLE_BRANCH = "Branchs";
        public const string TABLE_DEPARTMENT = "Departments";
        public const string TABLE_POSITION = "Positions";
        public const string TABLE_TITLE = "Titles";
        public const string TABLE_EMPLOYEE = "Employees";
        public const string TABLE_CONTRACTTYPE = "ContractTypes";
        public const string TABLE_REASON_CATEGORY = "ReasonCategories";
        public const string TABLE_USER = "Users";
        public const string TABLE_PERMISSION_GROUP = "PermissionGroups";
        public const string TABLE_CONTRACT = "Contracts"; // hợp đồng
        public const string TABLE_CONTRACT_APPENDIX = "ContractAppendices"; // phụ lục hợp đồng
        public const string TABLE_LEAVE_REQUEST = "LeaveRequests";
        public const string TABLE_CONFIRM_WORKING_DAY = "ConfirmWorkingDays";
        public const string TABLE_LEAVE_WORKING_HOURS = "LeaveWorkingHours";
        public const string TABLE_SHIFT_CHANGE_REQUEST = "ShiftChanges"; // đăng kí đổi ca
        public const string TABLE_OVERTIME_REQUEST = "OvertimeRequests"; // đề nghị tăng ca
        public const string TABLE_TRAINING = "Trainings"; // đề nghị tăng ca
        public const string TABLE_SALARY_EXPENSE_ACCOUNTING = "SalaryExpenseAccountings"; // đề nghị tăng ca
        public const string TABLE_SUB_DEPARTMENT = "SubDepartments"; // bảng bộ phận
        public const string TABLE_ADJUSTED_ANNUAL_LEAVE_REQUEST = "AdjustedAnnualLeaveRequests";
        #endregion
        public const string FORMAT_DATE = "dd/MM/yyyy";
        public const string FORMAT_CURRENCY = "###,###,###,##0.##";//
    }
}
