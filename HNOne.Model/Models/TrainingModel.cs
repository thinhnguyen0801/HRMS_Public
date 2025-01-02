namespace HNOne.Model.Models
{
    /// <summary>
    /// khóa đào tạo
    /// </summary>
    public class TrainingModel : AuditableModel
    {
        public int id { get; set; }
        public string? voucherNo { get; set; } // số chứng từ
        public string? trainingCourseName { get; set; } // tên khóa đào tạo
        public string? typeOfTraning { get; set; } // loại đào tạo (Nội bộ/ ngoài)
        public string? traningFormatCode { get; set; } // hình thức đào tạo (Bắt buộc,...)
        public string? traningFormatName { get; set; }
        public string? statusCode { get; set; } // tình trạng
        public string? statusName { get; set; } // trạng thái lấy từ enum
        public string? address { get; set; } // địa điểm
        public DateTime? fromDate { get; set; } // Ngày bắt đầu
        public DateTime? toDate { get; set; } // Ngày kết thúc 
        public string? content { get; set; } // Nội dung đào tạo
        public string? objectives { get; set; } // Mục tiêu đào tạo
        public string? noteForAll { get; set; } // chi tiết khóa học
        public string? remark { get; set; } // ghi chú
        public int employeeSignatureId { get; set; } // nhân viên kí
        public string? employeeSignatureCode { get; set; }
        public string? employeeSignatureName { get; set; }
        public int branchId { get; set; }
        public DateTime? dateOfSigning { get; set; } // Ngày kí
        public string? link { get; set; }
        public string? jsonDetail { get; set; } // danh sách chi tiết
    }

    /// <summary>
    /// danh sách nhân viên trong khóa đạo tạo
    /// </summary>
    public class Training1Model
    {
        public int id { get; set; }
        public int trainId { get; set; } // id 
        public int employeeId { get; set; } // id nhân viên
        public string? employeeCode { get; set; }
        public string? employeeName { get; set; }
        public bool isAbsent { get; set; } // vắng mặt ?
        public string? noteForAll { get; set; } // đánh giá
        public string? remark { get; set; } // ghi chú
        public DateTime? dateTracking { get; set; }
        public int? userSign { get; set; }
    }
}
