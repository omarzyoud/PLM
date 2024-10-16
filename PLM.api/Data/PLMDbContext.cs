using PLM.api.Models.Domain;
using System.Collections.Generic;
using System.Reflection.Emit;
using System;
using Microsoft.EntityFrameworkCore;

namespace PLM.api.Data
{
    public class PLMDbContext : DbContext
    {
        public PLMDbContext(DbContextOptions<PLMDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Media> Media { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Relationships
            modelBuilder.Entity<Media>()
                .HasOne(m => m.Uploader)
                .WithMany(u => u.UploadedMedia)
                .HasForeignKey(m => m.UploaderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Media>()
                .HasOne(m => m.ApprovedBy)
                .WithMany(u => u.ApprovedMedia)
                .HasForeignKey(m => m.ApprovedById)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Media>()
       .Property(m => m.Status)
       .HasConversion<string>();
            modelBuilder.Entity<User>()
       .Property(u => u.Type)
       .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}
