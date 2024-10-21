using HNOne.Model.Entities;
using Microsoft.EntityFrameworkCore;

namespace HNOne.API.Repositories
{
    public class MasterDbContext : DbContext
    {
        public DbSet<Menus> Menus { get; set; }
        public DbSet<Branchs> Branchs { get; set; }
        public MasterDbContext(DbContextOptions<MasterDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Branchs>().HasIndex(m => m.BranchCode).IsUnique(true);
        }



        //Add-Migration NewMigration -Project HNOne.API
        //Remove-Migration
        //update-database
    }
}
