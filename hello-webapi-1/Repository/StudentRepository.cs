using System;
using Microsoft.EntityFrameworkCore;
using hello_webapi.Models;

namespace hello_webapi.Repository
{
    public class StudentRepository:DbContext
    {
        public DbSet<Student> Students{get;set;}

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=MyDatabase;Trusted_Connection=True;");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Student>().ToTable("students");
            modelBuilder.Entity<Student>().Property(p=>p.Id).HasColumnName("id");
            base.OnModelCreating(modelBuilder);
        }
    }
}