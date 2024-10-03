﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnDemandTutor.Contract.Repositories.Entity;
using OnDemandTutor.Repositories.Entity;

namespace OnDemandTutor.Repositories.Context
{
    public class DatabaseContext : IdentityDbContext<Accounts, ApplicationRole, Guid, ApplicationUserClaims, ApplicationUserRoles, ApplicationUserLogins, ApplicationRoleClaims, ApplicationUserTokens>
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        // DbSets cho các thực thể  
        public virtual DbSet<Accounts> ApplicationUsers => Set<Accounts>();
        public virtual DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
        public virtual DbSet<ApplicationUserClaims> ApplicationUserClaims => Set<ApplicationUserClaims>();
        public virtual DbSet<ApplicationUserRoles> ApplicationUserRoles => Set<ApplicationUserRoles>();
        public virtual DbSet<ApplicationUserLogins> ApplicationUserLogins => Set<ApplicationUserLogins>();
        public virtual DbSet<ApplicationRoleClaims> ApplicationRoleClaims => Set<ApplicationRoleClaims>();
        public virtual DbSet<ApplicationUserTokens> ApplicationUserTokens => Set<ApplicationUserTokens>();
        public virtual DbSet<UserInfo> UserInfos => Set<UserInfo>();
        public virtual DbSet<Class> Classes { get; set; }
        public virtual DbSet<Complaint> Complaints { get; set; }
        public virtual DbSet<Feedback> Feedbacks { get; set; }
        public virtual DbSet<Schedule> Schedules { get; set; }
        public virtual DbSet<Slot> Slots { get; set; }
        public virtual DbSet<Subject> Subjects { get; set; }
        public virtual DbSet<TutorSubject> TutorSubjects { get; set; }
        public virtual DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình mối quan hệ cho TutorSubject  
            modelBuilder.Entity<TutorSubject>()
                .HasKey(ts => new { ts.TutorId, ts.UserId }); // Khóa chính cho TutorSubject  

            modelBuilder.Entity<TutorSubject>()
                .HasOne(ts => ts.User) // Mối quan hệ với Accounts  
                .WithMany(a => a.TutorSubjects)
                .HasForeignKey(ts => ts.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TutorSubject>()
                .HasOne(ts => ts.Subject) // Mối quan hệ với Subject  
                .WithMany(s => s.TutorSubjects)
                .HasForeignKey(ts => ts.SubjectId) // Khóa ngoại  
                .OnDelete(DeleteBehavior.Cascade);

            // Cấu hình mối quan hệ cho Schedule  
            modelBuilder.Entity<Schedule>()
                .HasKey(s => new { s.StudentId, s.SlotId }); // Khóa chính cho Schedule  

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Student) // Mối quan hệ với Accounts  
                .WithMany(a => a.Schedules)
                .HasForeignKey(s => s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Schedule>()
                .HasOne(s => s.Slot) // Mối quan hệ với Slot  
                .WithMany(s => s.Schedules)
                .HasForeignKey(s => s.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            // Cấu hình mối quan hệ cho Complaint  
            modelBuilder.Entity<Complaint>()
                .HasKey(c => new { c.StudentId, c.TutorId }); // Khóa chính cho Complaint  

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Accounts) // Mối quan hệ với Accounts  
                .WithMany(a => a.Complaints)
                .HasForeignKey(c => c.StudentId);

            modelBuilder.Entity<Complaint>()
                .HasOne(c => c.Accounts) // Mối quan hệ với Tutor  
                .WithMany(a => a.Complaints)
                .HasForeignKey(c => c.TutorId);

            // Cấu hình mối quan hệ cho Feedback  
            modelBuilder.Entity<Feedback>()
                .HasKey(f => new { f.StudentId, f.TutorId }); // Khóa chính cho Feedback  

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Slot) // Mối quan hệ với Slot  
                .WithMany(s => s.Feedbacks)
                .HasForeignKey(f => f.SlotId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Accounts) // Mối quan hệ với Accounts  
                .WithMany(a => a.Feedbacks)
                .HasForeignKey(f => f.StudentId);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Accounts) // Mối quan hệ với Tutor  
                .WithMany(a => a.Feedbacks)
                .HasForeignKey(f => f.TutorId);

            // Cấu hình mối quan hệ cho Slot  
            modelBuilder.Entity<Slot>()
                .HasOne(s => s.Class) // Mối quan hệ với Class  
                .WithMany(c => c.Slots)
                .HasForeignKey(s => s.ClassId);
        }
    }
}