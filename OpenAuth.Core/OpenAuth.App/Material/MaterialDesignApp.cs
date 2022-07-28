extern alias MySqlConnectorAlias;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using System.Data;
using OpenAuth.Repository;
using System.Data.SqlClient;

namespace OpenAuth.App.Material
{
    public class MaterialDesignApp : OnlyUnitWorkBaeApp
    {
        public MaterialDesignApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }


        public DataTable GetSboNamePwd(int SboId)
        {
            string strSql = string.Format("SELECT sql_db,sql_name,sql_pswd,sap_name,sap_pswd,sql_conn,is_open FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", SboId);
            return UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, null);
        }

        /// <summary>
        /// 查看视图【设计项目筛选】
        /// </summary>
        /// <returns></returns>
        public TableData ForScreeningViewInfo(SalesOrderMaterialReq req, int SboId, int type)//, int? SalesOrderId, string ItemCode, string CardCode)
        {
            #region 注释
            var result = new TableData();

            var query = from n in UnitWork.Find<sale_rdr1>(null)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), c => c.DocEntry == req.SalesOrderId)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.MaterialCode), c => c.ItemCode == req.MaterialCode)
                         .WhereIf(type == 0, c => c.ReviewSubmitTime != null && c.IsSync != 1)
                         .WhereIf(type == 1, c => c.IsSync == 1)
                         .Where(c => c.sbo_id == SboId)
                        join m in UnitWork.Find<sale_ordr>(null)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.CustomerCode), c => c.CardCode == req.CustomerCode)
                        on n.DocEntry equals m.DocEntry
                        select new
                        {
                            m.DocNum,
                            m.CardCode,
                            m.CardName,
                            n.ItemCode,
                            n.Dscription,
                            m.SlpCode,
                            n.ReviewSubmitTime,
                            n.ContractReviewCode,
                            n.RecordGuid,
                            n.Advance
                        };
            //先把数据加载到内存
            var data = query.OrderBy(q => q.CardCode).Skip((req.page - 1) * req.limit).Take(req.limit).ToList();

            result.Data = data;
            result.Count = query.Count();

            return result;
            #endregion

        }

        /// <summary>
        /// 将评审单号回写到物料表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> BindReviewCode(int SboId, int DocEntry, string ItemCode, string ReviewCode)
        {
            var result = new Infrastructure.Response();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var item = await UnitWork.Find<sale_rdr1>(s => s.sbo_id == SboId && s.DocEntry == DocEntry && s.ItemCode.Equals(ItemCode)).FirstOrDefaultAsync();
            item.ContractReviewCode = ReviewCode;
            //await UnitWork.UpdateAsync<sale_rdr1>(s => s.sbo_id.Equals(SboId) && s.DocEntry.Equals(DocEntry) && s.ItemCode.Equals(ItemCode), s => new sale_rdr1
            //{
            //    ContractReviewCode = ReviewCode
            //});
            await UnitWork.UpdateAsync<sale_rdr1>(item);
            await UnitWork.SaveAsync();
            return result;
        }
        /// <summary>
        /// 提交物料设计到筛选列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> SubmitItemCodeList(int SboId, int DocEntry, List<string> ItemCodeList)
        {
            var result = new Infrastructure.Response();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            foreach (var item in ItemCodeList)
            {
                DateTime date = DateTime.Now;
                var dataitem = await UnitWork.Find<sale_rdr1>(s => s.sbo_id == SboId && s.DocEntry == DocEntry && s.ItemCode.Equals(item)).FirstOrDefaultAsync();
                dataitem.ReviewSubmitTime = date;
                await UnitWork.UpdateAsync<sale_rdr1>(dataitem);
            }
            await UnitWork.SaveAsync();
            return result;
        }


        public Infrastructure.Response PostDataToManager(int SboID, List<MaterialDes> list)// int SboId, int? SalesOrderId, string ItemCode, string CardCode, int Type)
        {
            var result = new Infrastructure.Response();
            int OrderId = list[0].SalesOrderId.Value;

            #region 分配项目经理
            List<string> liujing = "A605,A608,A313,A302,CE-8,CE-7,CTE-8".Split(',').ToList();//刘静101
            List<string> wulei = "CT-4,CT-8,CE-4,CTE-4,CA,CJE,CJ,CGE,CGE,BT-4/8,BTE-4/8,BE-4/8,BA-4/8".Split(',').ToList();//武蕾103
            List<string> zhengjing = "CE-6,M202,M203,MGDW,MIGW,MGW,MGDW,MHW,MIHW".Split(',').ToList();//郑静102
            List<string> daijiawei = "MCD,MFF,MFYHS,MWL,MJZJ,MFXJ,MXFC,MFHL,MET,MRBH,MRH,MRF,MFB,MFYB,MRZH,MFYZ,MYSFR,MFSF,MYSHC,MYHF,MRSH,MRHF,MXFS,MZZ,MDCIR,MJR,MJF,MTP,MJY,MJJ,MCH,MCJ,MZJ".Split(',').ToList();//戴嘉魏292

            List<string> wenxiang = "MGDW,MIGW,MGW,MGDW,MHW,MIHW".Split(',').ToList();//温箱2
            List<string> changgui = "A605,A608,A313,A302,CT-4,CT-8,CE-4,CE-8,CE-7,CTE-4,CTE-8,CE-6,CA,CJE,CJ,CGE,BT-4/8,BTE-4/8,BE-4/8,BA-4/8".Split(',').ToList();//常规1
            #endregion
            for (int i = 0; i < list.Count; i++)
            {
                int manager = 104;
                int projectType = 1;
                string ItemCode = list[i].MaterialCode;
                int ReviewCode = list[i].ReviewCode.Value;

                foreach (var liujingitem in liujing)
                {
                    if (ItemCode.IndexOf(liujingitem) > -1)
                    {
                        manager = 101;
                        break;
                    }
                }
                foreach (var wuleiitem in wulei)
                {
                    if (ItemCode.IndexOf(wuleiitem) > -1)
                    {
                        manager = 103;
                        break;
                    }
                }
                foreach (var zhengjingitem in zhengjing)
                {
                    if (ItemCode.IndexOf(zhengjingitem) > -1)
                    {
                        manager = 102;
                        break;
                    }
                }
                foreach (var daijiaweiitem in daijiawei)
                {
                    if (ItemCode.IndexOf(daijiaweiitem) > -1)
                    {
                        manager = 292;
                        projectType = 3;
                        break;
                    }
                }
                foreach (var wenxiangitem in wenxiang)
                {
                    if (ItemCode.IndexOf(wenxiangitem) > -1)
                    {
                        projectType = 2;
                        break;
                    }
                }
                foreach (var changguiitem in changgui)
                {
                    if (ItemCode.IndexOf(changguiitem) > -1)
                    {
                        projectType = 1;
                        break;
                    }
                }
                string getDataSql = "select * from nsap_bone.sale_contract_review_detail t where t.contract_id=" + ReviewCode + " and ItemTypeID in (14,15,43)";
                DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, getDataSql.ToString(), CommandType.Text, null);
                string banjin = "";
                string tongtiao = "";
                string taoxian = "";
                if (dt.Rows.Count > 0)
                {
                    for (int j = 0; j < dt.Rows.Count; j++)
                    {
                        if (j == 0) banjin = dt.Rows[j]["ItemCode"].ToString();
                        if (j == 1) tongtiao = dt.Rows[j]["ItemCode"].ToString();
                        if (j == 2) taoxian = dt.Rows[j]["ItemCode"].ToString();
                    }
                }
                //string banjinCode = UnitWork.Find<sale_contract_review_detail>

                string str = "exec add_projectFromERP @projectType=" + projectType + ",@customerCode=" + list[i].CustomerCode + ",@saleCode = " + OrderId + ",@banjinCode='" + banjin + "',@taoxianCode='" + taoxian + "',@tongtiaoCode = '" + tongtiao + "',@salesman='" + list[i].SalesMan + "',@projectManager=" + manager + ",@itemCode='" + ItemCode + "',@modelType=11,@peopleStr='' ";
                try
                {
                    DataTable returnDt = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, str, CommandType.Text, null);
                    string RecordGuid = returnDt.Rows[0]["RecordGuid"].ToString();
                    var sale_rdr1 = UnitWork.Find<sale_rdr1>(s => s.sbo_id == SboID && s.DocEntry == OrderId && s.ItemCode.Equals(ItemCode)).FirstOrDefault();
                    sale_rdr1.IsSync = 1;
                    sale_rdr1.RecordGuid = RecordGuid;
                    sale_rdr1.SubmitTime = DateTime.Now;
                    UnitWork.Update<sale_rdr1>(sale_rdr1);
                    UnitWork.SaveAsync();
                }
                catch (Exception)
                {
                    result.Code = 500;
                    result.Message = "内部出现错误";
                    throw;
                }
            }
            return result;
        }

        public class MaterialDes
        {
            /// <summary>
            /// 销售单号
            /// </summary>
            public int? SalesOrderId { get; set; }

            /// <summary>
            /// 客户
            /// </summary>
            public string CustomerCode { get; set; }

            /// <summary>
            /// 物料编码
            /// </summary>
            public string MaterialCode { get; set; }
            /// <summary>
            /// 业务员
            /// </summary>

            public string SalesMan { get; set; }

            public int? ReviewCode
            {
                get
                {
                    return 0;
                }
                set { ReviewCode = value; }
            }

        }

        /// <summary>
        /// 同步物料设计进度
        /// </summary>
        /// <returns></returns>
        public async Task SyncMaterialDesignAdvance()
        {
            var salesOrderIds = await UnitWork.Find<sale_rdr1>(q => !string.IsNullOrWhiteSpace(q.RecordGuid.ToString()) && q.Advance != 100).Select(q => q.RecordGuid).ToListAsync();

            if (salesOrderIds.Count() > 0)
            {
                for (int i = 0; i < salesOrderIds.Count; i++)
                {
                    string id = salesOrderIds[i];
                    sale_rdr1 rdr = UnitWork.Find<sale_rdr1>(s => s.RecordGuid == id).FirstOrDefault();
                    string str = "select top 1 _System_Progress from OBJ162 where RecordGuid = '" + id + "'";
                    DataTable returnDt = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, str, CommandType.Text, null);
                    if (returnDt.Rows.Count > 0)
                    {
                        int progress = returnDt.Rows[0]["_System_Progress"].ToInt();
                        rdr.Advance = progress;
                        UnitWork.Update<sale_rdr1>(rdr);
                    }
                }
            }
            await UnitWork.SaveAsync();
        }




        #region 获取进度
        public DataTable GetProgressAll()
        {
            string strSql = string.Format("select * from ( select RecordGuid, fld005508 DocEntry, max(_System_Progress) progress,fld005506 itemCode from OBJ162 group by RecordGuid, fld005508,_System_objNBS,fld005506) a");
            return UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null);
        }


        #endregion
    }
}
