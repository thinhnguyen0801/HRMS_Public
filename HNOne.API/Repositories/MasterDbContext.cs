using HNOne.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace HNOne.API.Repositories
{
    public partial class MasterDbContext : DbContext
    {
        public DbSet<Menus> Menus { get; set; }
        public DbSet<Branchs> Branchs { get; set; }
        public DbSet<Departments> Departments { get; set; }
        public DbSet<Positions> Positions { get; set; }
        public DbSet<Titles> Titles { get; set; }
        public DbSet<Employees> Employees { get; set; }
        public DbSet<ContractTypes> ContractTypes { get; set; }
        public DbSet<ReasonCategories> ReasonCategories { get; set; }
        public DbSet<EnumCatagories> EnumCatagories { get; set; }
        public DbSet<Users> Users { get; set; }
        public DbSet<FamilyRelationships> FamilyRelationships { get; set; }
        public DbSet<SalaryCategories> SalaryCategories { get; set; }
        public DbSet<SalaryConfigurations> SalaryConfigurations { get; set; }
        public DbSet<Contracts> Contracts { get; set; }
        public DbSet<SalaryAdjustments> SalaryAdjustments { get; set; }
        public DbSet<Countries> Countries { get; set; }
        public DbSet<Provinces> Provinces { get; set; }
        public DbSet<Districts> Districts { get; set; }
        public DbSet<Wards> Wards { get; set; }
        public DbSet<WorkHistories> WorkHistories { get; set; }
        public DbSet<Insurances> Insurances { get; set; }
        public DbSet<PermissionGroups> PermissionGroups { get; set; }
        public DbSet<EventConfigurations> EventConfigurations { get; set; }
        public DbSet<ContractAppendices> ContractAppendices { get; set; }
        public DbSet<LevelOfEducations> LevelOfEducations { get; set; }
        public DbSet<GroupAccessControls> GroupAccessControls { get; set; }
        public DbSet<LeaveConfigs> LeaveConfigs { get; set; }
        public DbSet<Approvals> Approvals { get; set; }



        public MasterDbContext(DbContextOptions<MasterDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Branchs>().HasIndex(m => m.BranchCode).IsUnique();
            modelBuilder.Entity<Departments>().HasIndex(m => m.Code).IsUnique();
            modelBuilder.Entity<Positions>().HasIndex(m => m.Code).IsUnique();
            modelBuilder.Entity<Titles>().HasIndex(m => m.Code).IsUnique();
            modelBuilder.Entity<Employees>().HasIndex(m => m.Code).IsUnique();
            modelBuilder.Entity<ContractTypes>().HasIndex(m => new { m.Code, m.BranchId }).IsUnique();
            modelBuilder.Entity<Users>().HasIndex(m => m.UserName).IsUnique();
            modelBuilder.Entity<SalaryCategories>().HasIndex(m => m.Code).IsUnique();
            modelBuilder.Entity<PermissionGroups>().HasIndex(m => m.Code).IsUnique();
            modelBuilder.Entity<SalaryConfigurations>().HasIndex(m => new { m.SalaryCategoryId, m.BranchId }).IsUnique();
            modelBuilder.Entity<EventConfigurations>().HasIndex(m => m.ActionKey).IsUnique();
        }


        //Add-Migration NewMigration -Project HNOne.API
        //Remove-Migration
        //update-database
    }
}
