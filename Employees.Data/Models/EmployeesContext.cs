using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Logging;

namespace Employees.Data.Models
{
    public partial class EmployeesContext : DbContext
    {
        public EmployeesContext()
        {
        }

        public EmployeesContext(DbContextOptions<EmployeesContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Company> Companies { get; set; } = null!;
        public virtual DbSet<Employee> Employees { get; set; } = null!;
        public virtual DbSet<EmployeeQualificationRef> EmployeeQualificationRefs { get; set; } = null!;
        public virtual DbSet<Profession> Professions { get; set; } = null!;
        public virtual DbSet<Qualification> Qualifications { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=Employees;Trusted_Connection=True;")
                    .LogTo(Console.Write, new[] { DbLoggerCategory.Database.Command.Name, DbLoggerCategory.Database.Transaction.Name }, LogLevel.Debug)
                    .EnableSensitiveDataLogging();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Company>(entity =>
            {
                entity.ToTable("Company");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            modelBuilder.Entity<Employee>(entity =>
            {
                entity.ToTable("Employee");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(e => e.Company)
                    .WithMany(c => c.Employees)
                    .HasForeignKey(e => e.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull)
                    .HasConstraintName("FK_Employee_Company");

                entity.HasOne(e => e.Profession)
                    .WithMany(p => p.Employees)
                    .HasForeignKey(e => e.ProfessionId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Employee_Profession");
            });

            modelBuilder.Entity<EmployeeQualificationRef>(entity =>
            {
                entity.ToTable("EmployeeQualificationRef");

                entity.HasOne(d => d.Employee)
                    .WithMany(p => p.EmployeeQualificationRefs)
                    .HasForeignKey(d => d.EmployeeId)
                    .HasConstraintName("FK_EmployeeQualificationRef_Employee");

                entity.HasOne(d => d.Qualification)
                    .WithMany(p => p.EmployeeQualificationRefs)
                    .HasForeignKey(d => d.QualificationId)
                    .HasConstraintName("FK_EmployeeQualificationRef_Qualification");
            });

            modelBuilder.Entity<Profession>(entity =>
            {
                entity.ToTable("Profession");

                entity.Property(e => e.Name).HasMaxLength(50);

                entity.HasOne(d => d.ParentProfession)
                    .WithMany(p => p.InverseParentProfession)
                    .HasForeignKey(d => d.ParentProfessionId)
                    .HasConstraintName("FK_Profession_Profession");
            });

            modelBuilder.Entity<Qualification>(entity =>
            {
                entity.ToTable("Qualification");

                entity.Property(e => e.Name).HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
