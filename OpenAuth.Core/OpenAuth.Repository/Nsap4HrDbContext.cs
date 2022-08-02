using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    /// <summary>
    /// 
    /// </summary>
    [ConnectionString("Nsap4HrDbContext")]
    public partial class Nsap4HrDbContext : DbContext
    {
        public Nsap4HrDbContext(DbContextOptions<Nsap4HrDbContext> options): base(options)
        { }

        public static readonly LoggerFactory loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
            modelBuilder.Entity<MaterialRange>().HasKey(s => s.ItemCode);
        }

        public virtual DbSet<classroom_teacher_apply_log> Classroom_Apply_Teacher_Logs { get; set; }
        public virtual DbSet<classroom_course_package> Classroom_Course_Packages { get; set; }
        public virtual DbSet<classroom_course_package_user> Classroom_Course_Package_Users { get; set; }
        public virtual DbSet<classroom_course> Classroom_Courses { get; set; }
        public virtual DbSet<classroom_teacher_course> Classroom_Teacher_Courses { get; set; }
        public virtual DbSet<classroom_course_package_map> Classroom_Course_Package_Maps { get; set; }
        public virtual DbSet<classroom_course_video> Classroom_Course_Videos { get; set; }
        public virtual DbSet<classroom_course_video_subject> Classroom_Course_Video_Subjects { get; set; }
        public virtual DbSet<classroom_video_play_log> Classroom_Video_Play_Logs { get; set; }
        public virtual DbSet<classroom_course_exam> Classroom_Course_Exams { get; set; }
    }
}
