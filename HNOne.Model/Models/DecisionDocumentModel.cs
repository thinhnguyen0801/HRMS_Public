
namespace HNOne.Model.Models
{
    /// <summary>
    /// Model dành cho chứng từ quyết định
    /// </summary>
    public class DecisionDocumentModel : AuditableModel
    {
        public int id { get; set; }
        public string? voucherNo { get; set; } // số chứng từ
        public string? decisionTypeCode { get; set; }
        public string? decisionTypeName { get; set; }
        public string? statusCode { get; set; } // trạng thái lấy từ enum
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public int branchId { get; set; }
        public string? branchCode { get; set; } //  chi nhánh
        public string? branchName { get; set; } //  chi nhánh
        public string? remark { get; set; } // lý do
        public DateTime? effectiveDate { get; set; } // ngày hiệu lực
        public string? noteForAll { get; set; }
        public string? link { get; set; }
        #region Thông tin hiện tại
        public int branchIdCur { get; set; } // ID chi nhánh
        public string? branchCodeCur { get; set; } //  chi nhánh
        public string? branchNameCur { get; set; } //  chi nhánh
        public int departmentIdCur { get; set; } // phòng ban
        public string? departmentCodeCur { get; set; }
        public string? departmentNameCur { get; set; }
        public int positionIdCur { get; set; } // chức vụ
        public string? positionCodeCur { get; set; }
        public string? positionNameCur { get; set; }
        public int titleIdCur { get; set; } // chức danh
        public string? titleCodeCur { get; set; }
        public string? titleNameCur { get; set; }
        public int subDepartmentIdCur { get; set; } // bộ phận
        public string? subDepartmentCodeCur { get; set; }
        public string? subDepartmentNameCur { get; set; }
        public int workingBranchIdCur { get; set; } // ID chi nhánh làm việc
        public string? workingBranchCodeCur { get; set; } // chi nhánh làm việc
        public string? workingBranchNameCur { get; set; } // chi nhánh làm việc
        #endregion

        #region Thông tin mới
        public int branchIdNew { get; set; } // ID chi nhánh
        public string? branchCodeNew { get; set; } //  chi nhánh
        public string? branchNameNew { get; set; } //  chi nhánh
        public int departmentIdNew { get; set; } // phòng ban
        public string? departmentCodeNew { get; set; }
        public string? departmentNameNew { get; set; }
        public int positionIdNew { get; set; } // chức vụ
        public string? positionCodeNew { get; set; }
        public string? positionNameNew { get; set; }
        public int titleIdNew { get; set; } // chức danh
        public string? titleCodeNew { get; set; }
        public string? titleNameNew { get; set; }
        public int subDepartmentIdNew { get; set; } // bộ phận
        public string? subDepartmentCodeNew { get; set; }
        public string? subDepartmentNameNew { get; set; }
        public int workingBranchIdNew { get; set; } // ID chi nhánh làm việc
        public string? workingBranchCodeNew { get; set; } // chi nhánh làm việc
        public string? workingBranchNameNew { get; set; } // chi nhánh làm việc
        #endregion

        public string? employeeNamePrint { get; set; }
        public string? noteForAllCurPrint { get; set; }
        public string? noteForAllNewPrint { get; set; }
    }
}
