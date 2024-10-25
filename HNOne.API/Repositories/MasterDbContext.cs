using HNOne.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace HNOne.API.Repositories
{
    public class MasterDbContext : DbContext
    {
        public DbSet<Menus> Menus { get; set; }
        public DbSet<Branchs> Branchs { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Positions> Positions { get; set; }
        public DbSet<Titles> Titles { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public MasterDbContext(DbContextOptions<MasterDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Branchs>().HasIndex(m => m.BranchCode).IsUnique(true);
            modelBuilder.Entity<Departments>().HasIndex(m => m.Code).IsUnique(true);
            modelBuilder.Entity<Positions>().HasIndex(m => m.Code).IsUnique(true);
            modelBuilder.Entity<Titles>().HasIndex(m => m.Code).IsUnique(true);
            modelBuilder.Entity<Employees>().HasIndex(m => m.Code).IsUnique(true);
        }

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



        //Add-Migration NewMigration -Project HNOne.API
        //Remove-Migration
        //update-database
    }
}
