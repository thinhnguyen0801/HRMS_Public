using HNOne.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace HNOne.API.Repositories
{
    public partial class MasterDbContext
    {
        private void seedMenus(ModelBuilder modelBuilder)
        {
            //modelBuilder.Entity<Menus>().HasData(
            //    new Menus() { MenuID = "000-001", MenuName = "Trang chủ"},
            //    new Menus() { MenuID = "000-001", MenuName = "Trang chủ"},
            //    new Menus() { MenuID = "000-001", MenuName = "Trang chủ"},
            //    new Menus() { MenuID = "000-001", MenuName = "Trang chủ"},
            //    new Menus() { MenuID = "000-001", MenuName = "Trang chủ"}
            //    );
        }

        /// <summary>
        /// khởi tạo dữ liệu danh sách Enum
        /// </summary>
        /// <param name="modelBuilder"></param>
        private void seedEnums(ModelBuilder modelBuilder)
        {
            List<EnumCatagories> lstData = new List<EnumCatagories>()
            {
                // danh sách trạng thái nhân viên
                new EnumCatagories() {Id = Guid.Parse("B46365A8-9FBC-4446-B39F-18680774598F"), EnumType = "TrangThaiNhanVien", Code = "NV", Name = "Nghỉ việc", DateTracking = DateTime.Now, RowOrder = 5},
                new EnumCatagories() {Id = Guid.Parse("5430EA50-B7DC-4F74-AEE8-40BDDADC1E7D"), EnumType = "TrangThaiNhanVien", Code = "CT", Name = "Chính thức", DateTracking = DateTime.Now, RowOrder = 3},
                new EnumCatagories() {Id = Guid.Parse("27038D5B-BC06-4930-A416-4FE689D4F211"), EnumType = "TrangThaiNhanVien", Code = "TV", Name = "Thử việc", DateTracking = DateTime.Now, RowOrder = 2},
                new EnumCatagories() {Id = Guid.Parse("74D9D617-0C08-41A9-9DF7-6727D7CCC295"), EnumType = "TrangThaiNhanVien", Code = "NVTV", Name = "Thời vụ", DateTracking = DateTime.Now, RowOrder = 4},
                new EnumCatagories() {Id = Guid.Parse("EE9D83A0-3A33-411A-BF9F-9A1E40882764"), EnumType = "TrangThaiNhanVien", Code = "HV", Name = "Học việc", DateTracking = DateTime.Now, RowOrder = 1},
                new EnumCatagories() {Id = Guid.Parse("CD5B8BA3-F7B7-486F-8264-ECCC44A368EC"), EnumType = "TrangThaiNhanVien", Code = "TC", Name = "Tất cả", DateTracking = DateTime.Now, RowOrder = 0},

                // tình trạng hôn nhân
                new EnumCatagories() {Id = Guid.Parse("f3c05ddc-3e2a-498e-bbe9-cb9286003034"), EnumType = "TinhTrangHonNhan", Code = "DT", Name = "Độc thân", DateTracking = DateTime.Now, RowOrder = 1},
                new EnumCatagories() {Id = Guid.Parse("c864a687-4769-48ba-99bd-8dac4956bfeb"), EnumType = "TinhTrangHonNhan", Code = "GD", Name = "Lập gia đình", DateTracking = DateTime.Now, RowOrder = 2},
                new EnumCatagories() {Id = Guid.Parse("f38544fb-1227-421f-8e67-5c69e20d8c87"), EnumType = "TinhTrangHonNhan", Code = "LD", Name = "Ly dị", DateTracking = DateTime.Now, RowOrder = 3},

                // Cấp độ nhân viên
                new EnumCatagories() {Id = Guid.Parse("a71c1547-2365-4d93-a799-46a7ad2bbb5d"), EnumType = "CapDoNhanVien", Code = "NV", Name = "Nhân viên", DateTracking = DateTime.Now, RowOrder = 1},
                new EnumCatagories() {Id = Guid.Parse("95d4b868-9fa8-45b8-a4d6-d7863c6cf0b0"), EnumType = "CapDoNhanVien", Code = "QLCT", Name = "Quản lý cấp trung", DateTracking = DateTime.Now, RowOrder = 2},
                new EnumCatagories() {Id = Guid.Parse("1b21ae4b-9eb1-48c6-98c3-8593a5bd9ec6"), EnumType = "CapDoNhanVien", Code = "QL", Name = "Quản lý", DateTracking = DateTime.Now, RowOrder = 3},
                new EnumCatagories() {Id = Guid.Parse("82e38ad5-9feb-4345-a42f-514c651197ef"), EnumType = "CapDoNhanVien", Code = "CC", Name = "Cấp cao", DateTracking = DateTime.Now, RowOrder = 4},

                // Danh mục thuế TNCN
                new EnumCatagories() {Id = Guid.Parse("cc0c722d-210f-4a63-85ba-68fd6130883a"), EnumType = "DanhMucThueTNCN", Code = "0", Name = "Không tính", DateTracking = DateTime.Now, RowOrder = 1},
                new EnumCatagories() {Id = Guid.Parse("6c367ce5-d19f-402e-b375-35aad3ebfdf5"), EnumType = "DanhMucThueTNCN", Code = "1", Name = "Tính lũy tiến", DateTracking = DateTime.Now, RowOrder = 2},
                new EnumCatagories() {Id = Guid.Parse("9a258f1b-4c21-4f72-9558-4fff7161bdf7"), EnumType = "DanhMucThueTNCN", Code = "2", Name = "Tính 10%", DateTracking = DateTime.Now, RowOrder = 3},
                new EnumCatagories() {Id = Guid.Parse("145d39d8-2358-4b8e-b73b-2f70cd4eeefd"), EnumType = "DanhMucThueTNCN", Code = "3", Name = "Tính lũy tiến không giảm trừ", DateTracking = DateTime.Now, RowOrder = 4},
            };
            modelBuilder.Entity<EnumCatagories>().HasData(lstData);
        }
    }
}
