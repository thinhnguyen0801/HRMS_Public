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
        public DbSet<HolidayCatagories> HolidayCatagories { get; set; }
        public DbSet<LeaveRequests> LeaveRequests { get; set; }
        public DbSet<LeaveRequest1s> LeaveRequest1s { get; set; }
        public DbSet<LeaveWorkingHours> LeaveWorkingHours { get; set; }
        public DbSet<LeaveWorkingHour1s> LeaveWorkingHour1s { get; set; }
        public DbSet<Notifications> Notifications { get; set; }
        public DbSet<Timesheets> Timesheets { get; set; }
        public DbSet<ShiftChanges> ShiftChanges { get; set; }
        public DbSet<ShiftChange1s> ShiftChange1s { get; set; }
        public DbSet<OvertimeRequests> OvertimeRequests { get; set; }
        public DbSet<OvertimeRequest1s> OvertimeRequest1s { get; set; }
        public DbSet<WorkConfigs> WorkConfigs { get; set; }
        public DbSet<ShiftAssignments> ShiftAssignments { get; set; }
        public DbSet<CheckInOuts> CheckInOuts { get; set; }
        public DbSet<AttendanceSummarys> AttendanceSummarys { get; set; }
        public DbSet<Trainings> Trainings { get; set; }
        public DbSet<Training1s> Training1s { get; set; }
        public DbSet<SalaryParameters> SalaryParameters { get; set; }
        public DbSet<TaxRates> TaxRates { get; set; }
        public DbSet<DeductionConfigs> DeductionConfigs { get; set; }
        public DbSet<Payrolls> Payrolls { get; set; }
        public DbSet<ConfirmWorkingDays> ConfirmWorkingDays { get; set; }
        public DbSet<ConfirmWorkingDay1s> ConfirmWorkingDay1s { get; set; }

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
            modelBuilder.Entity<LeaveRequests>().HasIndex(m => m.VoucherNo).IsUnique();
            modelBuilder.Entity<LeaveWorkingHours>().HasIndex(m => m.VoucherNo).IsUnique();
            modelBuilder.Entity<OvertimeRequests>().HasIndex(m => m.VoucherNo).IsUnique();
            modelBuilder.Entity<Trainings>().HasIndex(m => m.VoucherNo).IsUnique();
            modelBuilder.Entity<Timesheets>().HasIndex(m => new { m.EmployeeId, m.BranchId, m.WorkingDate }).IsUnique();
            modelBuilder.Entity<ShiftAssignments>().HasIndex(m => new { m.EmployeeId, m.BranchId, m.Month, m.Year }).IsUnique();
            modelBuilder.Entity<AttendanceSummarys>().HasIndex(m => new { m.EmployeeId, m.BranchId, m.Month, m.Year }).IsUnique();
            modelBuilder.Entity<Payrolls>().HasIndex(m => new { m.EmployeeId, m.BranchId, m.Month, m.Year }).IsUnique();
            modelBuilder.Entity<ConfirmWorkingDays>().HasIndex(m => m.VoucherNo).IsUnique();
        }


        //Add-Migration NewMigration -Project HNOne.API
        //Remove-Migration
        //update-database
    }
}
