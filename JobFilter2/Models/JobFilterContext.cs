using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

#nullable disable

namespace JobFilter2.Models
{
    public partial class JobFilterContext : DbContext
    {
        public JobFilterContext()
        {
        }

        public JobFilterContext(DbContextOptions<JobFilterContext> options)
            : base(options)
        {
        }

        public virtual DbSet<BlockCompany> BlockCompanies { get; set; }
        public virtual DbSet<BlockJobItem> BlockJobItems { get; set; }
        public virtual DbSet<CrawlSetting> CrawlSettings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Name=ConnectionStrings:DefaultConnection");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasAnnotation("Relational:Collation", "Chinese_Taiwan_Stroke_CI_AS");

            modelBuilder.Entity<BlockCompany>(entity =>
            {
                entity.ToTable("BlockCompany");

                entity.Property(e => e.BlockReason)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.CompanyName)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            modelBuilder.Entity<BlockJobItem>(entity =>
            {
                entity.ToTable("BlockJobItem");

                entity.Property(e => e.BlockReason)
                    .IsRequired()
                    .HasMaxLength(20)
                    .IsUnicode(false);

                entity.Property(e => e.JobCode)
                    .IsRequired()
                    .HasMaxLength(20);
            });

            modelBuilder.Entity<CrawlSetting>(entity =>
            {
                entity.ToTable("CrawlSetting");

                entity.Property(e => e.Seniority)
                    .IsRequired()
                    .HasMaxLength(10);

                entity.Property(e => e.TargetUrl)
                    .IsRequired()
                    .HasMaxLength(400);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
