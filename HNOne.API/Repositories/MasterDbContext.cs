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
            modelBuilder.Entity<ContractTypes>().HasIndex(m => m.Code).IsUnique(true);
            modelBuilder.Entity<Users>().HasIndex(m => m.UserName).IsUnique(true);
        }


        //Add-Migration NewMigration -Project HNOne.API
        //Remove-Migration
        //update-database
    }
}
