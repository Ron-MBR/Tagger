using Microsoft.EntityFrameworkCore;
using Tagger.Models;
using Tagger.Data.ModelsConfiguration;

namespace Tagger.Data
{
    public class TagDbContext : DbContext
    {
        public DbSet<Tag> Tags { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite("Data Source=tagtable.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tag>();
            modelBuilder.ApplyConfiguration(new TagConfiguration());
        }

    }
}