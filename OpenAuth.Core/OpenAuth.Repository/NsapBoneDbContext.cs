using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;
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
        public static readonly LoggerFactory loggerFactory = new LoggerFactory(new[] { new DebugLoggerProvider() });
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseLoggerFactory(loggerFactory);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //当主键为联合主键时，需要把这里的内容拷贝到对应的位置
            modelBuilder.Entity<sale_transport>().HasKey(s => new { s.Base_DocEntry, s.Base_DocType, s.Buy_DocEntry, s.SboId });
            modelBuilder.Entity<buy_opor>().HasKey(b => new { b.sbo_id, b.DocEntry });
            modelBuilder.Entity<sale_dln1>().HasKey(d => new { d.sbo_id, d.DocEntry, d.LineNum });
            modelBuilder.Entity<v_storeitemstock>().HasKey(o => o.ItemCode);
            modelBuilder.Entity<sale_rdr1>().HasKey(o => new { o.sbo_id, o.DocEntry, o.LineNum });
            modelBuilder.Entity<sale_ordr>().HasKey(o => new { o.sbo_id, o.DocEntry });
            modelBuilder.Entity<store_oitm>().HasKey(o => new { o.sbo_id, o.ItemCode });
            modelBuilder.Entity<store_oitw>().HasKey(o => new { o.sbo_id, o.ItemCode, o.WhsCode });
            modelBuilder.Entity<store_oitl>().HasKey(o => new { o.sbo_id, o.LogEntry });
            modelBuilder.Entity<store_itl1>().HasKey(o => new { o.sbo_id, o.LogEntry, o.SysNumber, o.ItemCode });
            modelBuilder.Entity<store_osrn>().HasKey(o => new { o.sbo_id, o.AbsEntry });
            modelBuilder.Entity<sale_odln>().HasKey(o => new { o.sbo_id, o.DocEntry });
            modelBuilder.Entity<crm_oslp>().HasKey(o => new { o.sbo_id, o.SlpCode });
            modelBuilder.Entity<crm_ocrd>().HasKey(o => new { o.sbo_id, o.CardCode });
            modelBuilder.Entity<crm_ocry>().HasKey(o => new { o.Code });
            modelBuilder.Entity<crm_ocst>().HasKey(o => new { o.Code, o.Country });
            modelBuilder.Entity<sale_orin>().HasKey(o => new { o.sbo_id, o.DocEntry });
            modelBuilder.Entity<sale_rin1>().HasKey(o => new { o.sbo_id, o.DocEntry, o.LineNum });
            modelBuilder.Entity<sale_oinv>().HasKey(o => new { o.sbo_id, o.DocEntry });
            modelBuilder.Entity<sale_inv1>().HasKey(o => new { o.sbo_id, o.DocEntry, o.LineNum });
            modelBuilder.Entity<sale_ordr_plugin>().HasKey(o => new { o.sbo_id, o.DocEntry });
            modelBuilder.Entity<store_osrn_alreadyexists>().HasKey(o => new { o.ItemCode, o.SysNumber });
            modelBuilder.Entity<store_drawing_job>().HasKey(o => new { o.Id });
            modelBuilder.Entity<crm_ocrg>().HasKey(o => new { o.sbo_id,o.GroupCode });
            modelBuilder.Entity<crm_oidc>().HasKey(o => new { o.sbo_id,o.Code });
            modelBuilder.Entity<crm_ohem>().HasKey(o => new { o.empID,o.sbo_id });
            modelBuilder.Entity<crm_ocry>().HasKey(o => new { o.Code});
            modelBuilder.Entity<crm_ocst>().HasKey(o => new { o.Code,o.Country });
            modelBuilder.Entity<product_owor>().HasKey(o => new { o.sbo_id, o.DocEntry,o.DocNum }); 
            modelBuilder.Entity<crm_crd1>().HasKey(o => new { o.sbo_id, o.Address,o.CardCode,o.AdresType });
            modelBuilder.Entity<crm_ocpr>().HasKey(o => new { o.sbo_id, o.CardCode, o.CntctCode });
            modelBuilder.Entity<product_wor1>().HasKey(o => new { o.sbo_id, o.DocEntry, o.LineNum });
            modelBuilder.Entity<product_oign>().HasKey(o => new { o.sbo_id, o.DocEntry });
            modelBuilder.Entity<product_ign1>().HasKey(o => new { o.sbo_id, o.DocEntry, o.LineNum });
            modelBuilder.Entity<product_owor_wor1>().HasKey(o => new { o.DocEntry });
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
        public virtual DbSet<sale_orin> sale_orins { get; set; }
        public virtual DbSet<sale_rin1> sale_rin1s { get; set; }
        public virtual DbSet<sale_oinv> sale_oinvs { get; set; }
        public virtual DbSet<sale_inv1> sale_inv1s { get; set; }
        public virtual DbSet<crm_ocrg> crm_ocrgs { get; set; }
        public virtual DbSet<crm_oidc> crm_oidc { get; set; }
        public virtual DbSet<crm_ohem> crm_ohem { get; set; }
        public virtual DbSet<crm_ocpr> crm_ocpr { get; set; }
        public virtual DbSet<service_oins> service_oins { get; set; }
        public virtual DbSet<crm_crd1> crm_crd1 { get; set; }
        
        public virtual DbSet<product_owor> product_owors { get; set; }
        
        public virtual DbSet<product_wor1> product_wor1s { get; set; }
        /// <summary>
        /// 生产订单关联查询，非数据库表
        /// </summary>
        public virtual DbSet<product_owor_wor1> product_owor_wor1s { get; set; }
        public virtual DbSet<product_oign> product_oigns { get; set; }
        public virtual DbSet<product_ign1> product_ign1s { get; set; }


        //非数据库表格
        public virtual DbSet<v_storeitemstock> v_storeitemstocks { get; set; }
        public virtual DbSet<sale_ordr_plugin> SaleOrdrPlugins { get; set; }


        public virtual DbSet<store_drawing_job> store_drawing_job { get; set; }
        public virtual DbSet<store_osrn_alreadyexists> store_osrn_alreadyexists { get; set; }

    }
}
