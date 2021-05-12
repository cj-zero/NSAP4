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
            modelBuilder.Entity<sale_rdr1>().HasKey(o =>new { o.sbo_id,o.DocEntry,o.LineNum });
            modelBuilder.Entity<sale_ordr>().HasKey(o => new { o.sbo_id, o.DocEntry});
            modelBuilder.Entity<store_oitm>().HasKey(o => new { o.sbo_id, o.ItemCode });
            modelBuilder.Entity<store_oitw>().HasKey(o => new { o.sbo_id, o.ItemCode,o.WhsCode });
            modelBuilder.Entity<store_oitl>().HasKey(o => new { o.sbo_id, o.LogEntry });
            modelBuilder.Entity<store_itl1>().HasKey(o => new { o.sbo_id, o.LogEntry,o.SysNumber,o.ItemCode });
            modelBuilder.Entity<store_osrn>().HasKey(o => new { o.sbo_id, o.AbsEntry });
            modelBuilder.Entity<sale_odln>().HasKey(o => new { o.sbo_id, o.DocEntry });
            modelBuilder.Entity<crm_oslp>().HasKey(o => new { o.sbo_id, o.SlpCode });
            modelBuilder.Entity<crm_ocrd>().HasKey(o => new { o.sbo_id, o.CardCode });
            modelBuilder.Entity<crm_ocry>().HasKey(o => new { o.Code });
            modelBuilder.Entity<crm_ocst>().HasKey(o => new { o.Code, o.Country });
        }
        public virtual DbSet<sale_transport> sale_transports { get; set; }

        public virtual DbSet<buy_opor> buy_opors { get; set; }

        public virtual DbSet<sale_dln1> sale_dln1s { get; set; }

        public virtual DbSet<sale_ordr> sale_ordrs { get; set; }

        public virtual DbSet<sale_rdr1> sale_rdr1s { get; set; }

        public virtual DbSet<sale_odln> sale_odln { get; set; }
        
        public virtual DbSet<store_oitm> store_oitms { get; set; }

        public virtual DbSet<store_oitw> store_oitws { get; set; }
        public virtual DbSet<store_itl1> store_itl1 { get; set; }
        public virtual DbSet<store_oitl> store_oitl { get; set; }
        public virtual DbSet<store_osrn> store_osrn { get; set; }
        public virtual DbSet<crm_oslp> crm_oslp { get; set; }
        public virtual DbSet<crm_ocrd> crm_ocrd { get; set; }
        public virtual DbSet<crm_ocry> crm_ocry { get; set; }
        public virtual DbSet<crm_ocst> crm_ocst { get; set; }

        //非数据库表格
        public virtual DbSet<v_storeitemstock>  v_storeitemstocks { get; set; }
    }
}
