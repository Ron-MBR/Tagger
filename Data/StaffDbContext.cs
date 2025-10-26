using Microsoft.EntityFrameworkCore;
using Tagger.Models;
using Tagger.Data.ModelsConfiguration;

namespace Tagger.Data
{
    public class StaffDbContext : DbContext
    {
        public DbSet<Staff> Staffs {get; set;}
        
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite("Data Source=Staffs.db");
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Staff>();
            modelBuilder.ApplyConfiguration(new StaffConfiguration());
        }
    }
}