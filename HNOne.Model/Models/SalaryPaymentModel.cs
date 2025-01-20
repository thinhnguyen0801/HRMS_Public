using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Models
{
    public class SalaryPaymentModel : AuditableModel
    {
        public int id { get; set; }
        public string? voucherNo { get; set; } // số chứng từ 
        public string? statusCode { get; set; }
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public string? paymentRequestTypeCode { get; set; } // loại chi lương
        public string? paymentRequestTypeName { get; set; } // loại chi lương
        public string? paymentTypeCode { get; set; } // loại chi lương
        public string? paymentTypeName { get; set; } // loại chi lương
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public DateTime? dateOfSigning { get; set; } // ngày kí
        public int branchId { get; set; }
        public string? branchName { get; set; }
        public string? salaryPreiod { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public DateTime? docDate { get; set; }
        public DateTime? dueDate { get; set; }
        public string? remark { get; set; } // ghi chú
        public string? link { get; set; }
        public string? jsonDetail { get; set; }

        #region

        #endregion
    }

    public class SalaryPayment1Model
    {
        public int id { get; set; }
        public int salaryPaymentId { get; set; }
        public int employeeId { get; set; }
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public int departmentId { get; set; } // phòng ban
        public string? departmentCode { get; set; }
        public string? departmentName { get; set; }
        public string? accountNumber { get; set; }
        public string? bankName { get; set; }
        public string? lineTotal { get; set; }
        public decimal amountPaid { get; set; }
        public decimal remainingAmount { get; set; }
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }
    }
}
