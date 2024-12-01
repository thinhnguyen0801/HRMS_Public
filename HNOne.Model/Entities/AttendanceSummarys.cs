using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HNOne.Model.Entities
{
    /// <summary>
    /// Dữ liệu công của từng nhân viên trong tháng
    /// </summary>
    [Table("AttendanceSummarys")]
    public sealed class AttendanceSummarys : Auditable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // tự tăng
        public long Id { get; set; }
        public int EmployeeId { get; set; } // id nhân viên
        [MaxLength(50)]
        [Required]
        public string? EmployeeCode { get; set; } // Mã nhân viên
        public int BranchId { get; set; } // id chi nhánh
        public int DepartmentId { get; set; }
        public int PositionId { get; set; }
        public int TitleId { get; set; }
        
        [MaxLength(50)]
        [Required]
        public string? ShiftCode { get; set; } // ca làm việc lấy từ bảng enum
        public int Month { get; set; } // tháng công
        public int Year { get; set; } // năm công
        public double totalWorkingHoursActual { get; set; } // tổng số giờ làm việc thực tế
        public double TNC { get; set; } // tổng ngày công
        public double CDM { get; set; } // công định mức của tháng
        public double CTT { get; set; } // công thực tế
        public double NPN { get; set; } // nghỉ phép năm
        public double NCD { get; set; } // nghỉ chết độ
        public double NPKL { get; set; } // nghỉ phép không lương
        public double NB { get; set; } // nghỉ bù
        public double NKP { get; set; } // nghỉ không phép
        public double CTPC { get; set; } // số công tính phụ cấp
        public double TGDLTVS { get; set; } // thời gian đi trễ về sớm
        public double SLDLTVS { get; set; } // số lần đi trễ về sớm
        public double SGT { get; set; } // số giờ thiếu
        public double SGTC { get; set; } // số giờ trừ công
        public double GCTC { get; set; } // giờ công của 1 ngày
        public double TGTC { get; set; } // số giờ thiếu
        public bool IsLocked { get; set; } // chốt chưa, chốt những ai
        [MaxLength(50)]
        public string? N01 { get; set; }
        [MaxLength(50)]
        public string? N02 { get; set; }
        [MaxLength(50)]
        public string? N03 { get; set; }
        [MaxLength(50)]
        public string? N04 { get; set; }
        [MaxLength(50)]
        public string? N05 { get; set; }
        [MaxLength(50)]
        public string? N06 { get; set; }
        [MaxLength(50)]
        public string? N07 { get; set; }
        [MaxLength(50)]
        public string? N08 { get; set; }
        [MaxLength(50)]
        public string? N09 { get; set; }
        [MaxLength(50)]
        public string? N10 { get; set; }
        [MaxLength(50)]
        public string? N11 { get; set; }
        [MaxLength(50)]
        public string? N12 { get; set; }
        [MaxLength(50)]
        public string? N13 { get; set; }
        [MaxLength(50)]
        public string? N14 { get; set; }
        [MaxLength(50)]
        public string? N15 { get; set; }
        [MaxLength(50)]
        public string? N16 { get; set; }
        [MaxLength(50)]
        public string? N17 { get; set; }
        [MaxLength(50)]
        public string? N18 { get; set; }
        [MaxLength(50)]
        public string? N19 { get; set; }
        [MaxLength(50)]
        public string? N20 { get; set; }
        [MaxLength(50)]
        public string? N21 { get; set; }
        [MaxLength(50)]
        public string? N22 { get; set; }
        [MaxLength(50)]
        public string? N23 { get; set; }
        [MaxLength(50)]
        public string? N24 { get; set; }
        [MaxLength(50)]
        public string? N25 { get; set; }
        [MaxLength(50)]
        public string? N26 { get; set; }
        [MaxLength(50)]
        public string? N27 { get; set; }
        [MaxLength(50)]
        public string? N28 { get; set; }
        [MaxLength(50)]
        public string? N29 { get; set; }
        [MaxLength(50)]
        public string? N30 { get; set; }
        [MaxLength(50)]
        public string? N31 { get; set; }
    }
}
