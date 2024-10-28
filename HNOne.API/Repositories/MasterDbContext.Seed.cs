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
            };
            modelBuilder.Entity<EnumCatagories>().HasData(lstData);
        }
    }
}
