namespace HNOne.Web.Commons
{
    public enum EnumType
    {
        Add,
        Update,
        Delete,
        AllowEdit,
    }

    public enum EnumCatagory
    {
        TrangThaiNhanVien,
        TinhTrangHonNhan,
        CapDoNhanVien,
        DanhMucThueTNCN,
        CachTinhLuongPhuCap,
        TrangThaiHopDong,
        County,
        Province,
        District,
        Ward,
        QuanHeGiaDinh,
        LoaiBaoHiem,
        LoaiNhanVien,
        XepLoaiDaoTao,
        LoaiLyDo,
        LoaiDangKyXinNghiTrongGio,
        CaLamViec,
        LoaiNgayNghi,
        QuyDinhSoGioTangCa,
        CapNhatThongTinNhanVien,
        HinhThucDaoTao,
        TrichNop,
        QuyCachDanhMa,
        LoaiNghiPhep,
        TrangThaiPhatSinhCong,
        LoaiChiLuong,
        PhuongThucThanhToan,
        DanhMucImport,
    }

    public enum EnumObjType
    {
        Contracts, // hợp đồng
        ContractAppendices, // Phụ lục hợp đồng
        LeaveRequests, // đề nghị nghỉ phép
        LeaveWorkingHours, // xin nghỉ trong giờ
        ShiftChanges, // xin nghỉ trong giờ
        OvertimeRequests, // đề nghị tăng ca
        Trainings,
        ConfirmWorkingDays, // xin nghỉ trong giờ
        SalaryExpenseAccountings, // hạch toán chi phí lương

        // danh mục bảng
        Branchs,
        ContractTypes,
        Departments,
        Positions,
        Titles,
        HolidayCatagories,
        ReasonCategories,
        SalaryCategories,
        SalaryConfigurations,
        Users,
        LeaveConfigs,
        Employees,
        EnumCatagories,
        TaxRates,
        DeductionConfigs,
        PermissionGroups
    }
}
