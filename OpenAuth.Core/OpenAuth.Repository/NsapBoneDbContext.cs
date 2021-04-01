using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.NsapBone;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.Repository
{
    [ConnectionString("NsapBoneDbContext")]
    public class NsapBoneDbContext : DbContext
    {
        public NsapBoneDbContext(DbContextOptions<NsapBoneDbContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
            modelBuilder.Entity<sale_transport>().HasKey(s => new { s.Base_DocEntry, s.Base_DocType,s.Buy_DocEntry,s.SboId });
            modelBuilder.Entity<buy_opor>().HasKey(b => new { b.sbo_id,b.DocEntry });
            modelBuilder.Entity<sale_dln1>().HasKey(d => new { d.sbo_id,d.DocEntry,d.LineNum });
            modelBuilder.Entity<v_storeitemstock>().HasKey(o => o.ItemCode);
        }
        public virtual DbSet<sale_transport> sale_transports { get; set; }

        public virtual DbSet<buy_opor> buy_opors { get; set; }

        public virtual DbSet<sale_dln1> sale_dln1s { get; set; }

        //非数据库表格
        public virtual DbSet<v_storeitemstock>  v_storeitemstocks { get; set; }
    }
}
