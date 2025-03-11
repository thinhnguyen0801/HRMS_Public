namespace HNOne.Common
{
    public class ProcessConstants
    {
        public const string GET_MENU = "get-menu";
        public const string GET_BRANCH = "get-branch";
        public const string POST_BRANCH = "post-branch";
        public const string PUT_BRANCH = "put-branch";
        public const string GET_USER= "get-user";
        public const string POST_USER= "post-user";
        public const string PUT_USER= "put-user";
        public const string DELETE_USER= "delete-user";

        public const string GET_DEPARTMENT= "get-department";
        public const string POST_DEPARTMENT= "post-department";
        public const string PUT_DEPARTMENT= "put-department";

        public const string GET_POSITION = "get-position";
        public const string POST_POSITION = "post-position";
        public const string PUT_POSITION = "put-position";

        public const string GET_TITLE = "get-title";
        public const string POST_TITLE = "post-title";
        public const string PUT_TITLE = "put-title";
        public const string GET_EMPLOYEE = "get-employee";
        public const string POST_EMPLOYEE = "post-employee";
        public const string PUT_EMPLOYEE = "put-employee";
        public const string PUT_EMPLOYEE_INFO = "put-employee-info";
        public const string GET_ENUM = "get-enum";
        public const string POST_ENUM_CATA = "post-enum-cata";
        public const string PUT_ENUM_CATA = "put-enum-cata";

        public const string GET_CONTRACTTYPE = "get-contracttype";
        public const string POST_CONTRACTTYPE = "post-contracttype";
        public const string PUT_CONTRACTTYPE = "put-contracttype";

        public const string GET_REASONCATEGORIE = "get-reasoncategorie";
        public const string POST_REASONCATEGORIE = "post-reasoncategorie";
        public const string PUT_REASONCATEGORIE = "put-reasoncategorie";

        public const string GET_SALARY_CATEGORY = "get-salary-category";
        public const string POST_SALARY_CATEGORY = "post-salary-category";
        public const string PUT_SALARY_CATEGORY = "put-salary-category";

        public const string GET_SALARY_CONFIG = "get-salary-config";
        public const string POST_SALARY_CONFIG = "post-salary-config";
        public const string PUT_SALARY_CONFIG = "put-salary-config";

        public const string GET_SALARY_PARAMETER= "get-salary-parameter";
        public const string POST_SALARY_PARAMETER = "post-salary-parameter";
        public const string PUT_SALARY_PARAMETER = "put-salary-parameter";

        public const string GET_DOCUMENT_NO = "get-document-no";

        public const string GET_CONTRACT = "get-contract";
        public const string POST_CONTRACT = "post-contract";
        public const string PUT_CONTRACT = "put-contract";
        public const string EXPORT_CONTRACT_WORD = "export-contract-word";

        public const string GET_PER_GROUP = "get-per-group";
        public const string POST_PER_GROUP = "post-per-group";
        public const string PUT_PER_GROUP = "put-per-group";
        public const string DELETE_PER_GROUP = "delete-per_group";

        public const string GET_FAMILYRELATIONSHIP = "get-familyrelationship";
        public const string POST_FAMILYRELATIONSHIP = "post-familyrelationship";
        public const string PUT_FAMILYRELATIONSHIP = "put-familyrelationship";

        public const string GET_LOCATION = "get-location";

        public const string GET_INSURANCE = "get-insurance";
        public const string POST_INSURANCE = "post-insurance";
        public const string PUT_INSURANCE = "put-insurance";

        public const string GET_COMBO_MASTER_DATA = "get-combo-master-data";

        public const string GET_FUN_ENUM = "get-fun-enum";

        public const string GET_CONTRACT_APPENDIX = "get-contract-appendix";
        public const string POST_CONTRACT_APPENDIX = "post-contract-appendix";
        public const string PUT_CONTRACT_APPENDIX = "put-contract-appendix";

        public const string GET_EDUCATION = "get-education";
        public const string POST_EDUCATION = "post-education";
        public const string PUT_EDUCATION = "put-education";

        public const string GET_PER_GROUP_ACCESS_CONTROL = "get-per-group-access-control";
        public const string GET_DATA_PER_GROUP = "get-data-per-group";
        public const string POST_PER_GROUP_ACCESS_CONTROL = "post-per-group-access-control";

        public const string GET_LEAVE_CONFIG = "get-leave-config";
        public const string POST_LEAVE_CONFIG = "post-leave-config";
        public const string PUT_LEAVE_CONFIG = "put-leave-config";

        public const string GET_APPROVAL = "get-approval";
        public const string POST_APPROVAL = "post-approval";
        public const string PUT_APPROVAL = "put-approval";
        public const string GET_DOCUMENT_HISTORY = "get-document-history";
        public const string PUT_CANCEL_DOCUMENT = "put-cancel-document";

        public const string GET_WORKFORCE_MASTER_DATA = "get-workforce-master-data";

        public const string GET_LEAVE_REQUEST = "get-leave-request";
        public const string POST_LEAVE_REQUEST = "post-leave-request";
        public const string PUT_LEAVE_REQUEST = "put-leave-request";

        public const string GET_LEAVE_WORKING_HOUR = "get-leave-working-hour";
        public const string POST_LEAVE_WORKING_HOUR = "post-leave-working-hour";
        public const string PUT_LEAVE_WORKING_HOUR = "put-leave-working-hour";

        public const string GET_HOILDAY_CATAGORY = "get-hoilday-catagory";
        public const string POST_HOILDAY_CATAGORY = "post-hoilday-catagory";
        public const string PUT_HOILDAY_CATAGORY = "put-hoilday-catagory";

        public const string GET_SHIFT_CHANGE_REQUEST = "get-shift-change-request";
        public const string POST_SHIFT_CHANGE_REQUEST = "post-shift-change-request";
        public const string PUT_SHIFT_CHANGE_REQUEST = "put-shift-change-request";

        public const string GET_OVERTIME_REQUEST = "get-overtime-request";
        public const string POST_OVERTIME_REQUEST = "post-overtime-request";
        public const string PUT_OVERTIME_REQUEST = "put-overtime-request";

        public const string GET_WORK_CONFIG = "get-work-config";
        public const string POST_WORK_CONFIG = "post-work-config"; // phát sinh dữ liệu công chi tiết
        public const string PUT_WORK_CONFIG = "put-work-config"; // cập nhật thông tin cấu hình
        public const string GET_ARRANGE_SHIFT = "get-arrange-shift"; // phân ca làm việc
        public const string GET_WORK_SHEDULE = "get-work-shedule"; // lịch làm việc
        public const string GET_ANNUAL_LEAVE_INFO = "get-annual-leave-info"; // lịch làm việc
        public const string POST_TIME_SHEET = "post-time-sheet"; // phát sinh công làm việc
        public const string POST_SHIFT_ASSIGNMENT = "post-shift-assignment"; // lưu thông tin sắp ca làm việc
        public const string GET_CHECK_IN_OUT = "get-check-in-out"; // lịch làm việc
        public const string GET_WORK_CALCULATE = "get-work-calculate"; // lấy dữ liệu tính công tháng
        public const string POST_ATTENDENCE_SUMMARY = "post-attendance-summary"; // lấy dữ liệu tính công tháng
        public const string GET_WORKING_DAY_MISSING_HOURS = "get-working-day-missing-hours"; // lấy dữ liệu tính công tháng
        public const string PUT_UNLOCK_ATTENDENCE_SUMMARY = "put-unlock-attendance-summary"; // lấy dữ liệu tính công tháng

        public const string DELETE_DYNAMIC = "delete-dynamic"; // xóa động

        public const string GET_TRAINING = "get-training";
        public const string POST_TRAINING = "post-training";
        public const string PUT_TRAINING = "put-training";
        public const string PUT_TRAINING_EVALUATE = "put-training-evaluate";

        public const string GET_TAXT_RATE = "get-taxt-rate";
        public const string POST_TAXT_RATE = "post-taxt-rate";
        public const string PUT_TAXT_RATE = "put-taxt-rate";

        public const string GET_DEDUCTION_CONFIG = "get-deduction-config";
        public const string POST_DEDUCTION_CONFIG = "post-deduction-config";
        public const string PUT_DEDUCTION_CONFIG = "put-deduction-config";

        public const string GET_MONTHLY_SALARY = "get-monthly-salary"; // lấy dữ liệu tính công tháng
        public const string POST_PAYROLL_SALARY = "post-payroll-salary"; // lưu dữ liệu lương tháng
        public const string PUT_UNLOCK_PAYROLL_SALARY = "put-unlock-payroll-salary"; // lưu dữ liệu lương tháng

        public const string GET_CONFIRM_WORKING_HOUR_REQUEST = "get-confirm-working-hour-request";
        public const string POST_CONFIRM_WORKING_HOUR_REQUEST = "post-confirm-working-hour-request";
        public const string PUT_CONFIRM_WORKING_HOUR_REQUEST = "put-confirm-working-hour-request";

        public const string GET_ATTENDANCE_SUMMARY = "get-attendance-summary";
        public const string GET_PAYROLL_SUMMARY = "get-payroll-summary";
        public const string GET_SALARY_MASTER_DATA = "get-salary-master-data";
        public const string GET_SALARY_EXPENSE_ACCOUNTING = "get-salary-expense-accounting";
        public const string POST_SALARY_EXPENSE_ACCOUNTING = "post-salary-expense-accounting";
        public const string PUT_SALARY_EXPENSE_ACCOUNTING = "put-salary-expense-accounting";
        public const string POST_IMPORT_DATA = "post-import-data";
        public const string POST_CHECK_IN_OUT = "post-check-in-out";
        public const string POST_NOTIFICATION_STATUS_READ = "post-notification-status-read";

        public const string GET_WORKING_BRANCH = "get-working-branch";
        public const string POST_WORKING_BRANCH = "post-working-branch";
        public const string PUT_WORKING_BRANCH = "put-working-branch";

        public const string GET_RPT_PAYROLL_SUMMARY = "get-rpt-payroll-summary";

        #region Loại dưới store
        public const string GET_COMBO_TYPE_CONTRACT_BY_EMPLOYEEID = "CONTRACT_BY_EMPLOYEEID";
        public const string GET_COMBO_TYPE_SALARY_ADJUSTMENT_BY_CONTRACT = "SALARY_ADJUSTMENT_BY_CONTRACT";
        public const string GET_COMBO_TYPE_NOTIFICATION_BY_EMPLOYEE_MAIN = "NOTIFICATION_BY_EMPLOYEE_MAIN";
        public const string GET_COMBO_TYPE_NOTIFICATION_BY_EMPLOYEE = "NOTIFICATION_BY_EMPLOYEE";
        public const string GET_MENU_TYPE_AUTHENTICATION = "AUTHENTICATION";
        public const string GET_MENU_TYPE_CHECK_PERMISSION = "CHECK_PERMISSION";
        public const string GET_MENU_TYPE_MENU = "MENU";
        public const string GET_COMBO_LIST_OF_VACATION_DAY = "LR_LIST_OF_VACATION_DAYS";
        public const string GET_COMBO_LIST_OF_SHIFT_CHANGE_DAY = "SC_LIST_OF_SHIFT_CHANGE_DAY";
        public const string GET_COMBO_LIST_OVERTIME_REQUEST_DAY = "OTR_LIST_OVERTIME_REQUEST_DAY";
        public const string GET_COMBO_LIST_SHIFT_PREIOD = "SA_LIST_SHIFT_PREIOD"; // danh sách kỳ công năm
        public const string GET_ITEM_DETAIL = "ITEM_DETAIL"; //
        public const string GET_ITEM_HEADER = "ITEM_HEADER"; //
        public const string GET_COMBO_LIST_SALARY_PREIOD = "SA_LIST_SALARY_PREIOD"; // danh sách kỳ lương năm
        public const string GET_COMBO_LIST_ACCOUNTED_SALARY_TYPE = "SA_LIST_ACCOUNTED_SALARY_TYPE"; // danh sách các khoản lương cần hạch toán
        public const string GET_COMBO_LIST_ACCOUNTING = "SA_LIST_ACCOUNTING"; // danh sách các TÀI KHOẢN
        #endregion
    }
}
