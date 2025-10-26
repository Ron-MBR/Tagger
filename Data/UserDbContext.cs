using Microsoft.EntityFrameworkCore;
using Tagger.Models;
using Tagger.Data.ModelsConfiguration;

namespace Tagger.Data
{
    public class UserDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
    
        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite("Data Source=Users.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>();
            modelBuilder.ApplyConfiguration(new UserConfiguration());
        }


    }
}