using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    [ConnectionString("Nsap4ServeDbContext")]
    public class Nsap4ServeDbContext : DbContext
    {
        public Nsap4ServeDbContext(DbContextOptions<Nsap4ServeDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
        public virtual DbSet<Solution> Solutions { get; set; }
        public virtual DbSet<ProblemType> Problemtypes { get; set; }
        public virtual DbSet<AttendanceClock> Attendanceclocks { get; set; }
        public virtual DbSet<AttendanceClockPicture> Attendanceclockpictures { get; set; }
        public virtual DbSet<CompletionReport> Completionreports { get; set; }
        public virtual DbSet<CompletionReportPicture> Completionreportpictures { get; set; }
        public virtual DbSet<ServiceOrder> Serviceorders { get; set; }
        public virtual DbSet<ServiceOrderMessage> Serviceordermessages { get; set; }
        public virtual DbSet<ServiceOrderMessagePicture> Serviceordermessagepictures { get; set; }
        public virtual DbSet<ServiceOrderMessageUser> Serviceordermessageusers { get; set; }
        public virtual DbSet<ServiceOrderPicture> Serviceorderpictures { get; set; }
        public virtual DbSet<ServiceWorkOrder> Serviceworkorders { get; set; }
        public virtual DbSet<ServiceOrderLog> Serviceorderlogs { get; set; }

        public virtual DbSet<ServiceOrderSerial> Serviceorderserials { get; set; }
        public virtual DbSet<AppServiceOrderLog> Appserviceorderlogs { get; set; }
        public virtual DbSet<ServiceEvaluate> Serviceevaluates { get; set; }
        public virtual DbSet<RealTimeLocation> Realtimelocations { get; set; }

        public virtual DbSet<SeviceTechnicianApplyOrder> Sevicetechnicianapplyorders { get; set; }
    }
}
