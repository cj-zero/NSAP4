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
using OpenAuth.App.Request;

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
        public TableData ForScreeningViewInfo(SalesOrderMaterialReq req, int SboId)//, int? SalesOrderId, string ItemCode, string CardCode)
        {
            #region 注释
            var result = new TableData();
            var modeldata = (from n in UnitWork.Find<ManageScreening>(null)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()), c => c.DocEntry == req.SalesOrderId)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.MaterialCode), c => c.ItemCode.Contains(req.MaterialCode))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.CustomerCode), c => c.CardCode.Contains(req.CustomerCode))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.custom_req), c => c.custom_req.Contains(req.custom_req))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), c => c.ItemName.Contains(req.ItemName))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.VersionNo), c => c.VersionNo.Contains(req.VersionNo))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.SalesMan), c => c.SlpName.Contains(req.SalesMan))
                             select n).ToList();
            var manageData = GetProgressAll().AsEnumerable(); // new DataTable().AsEnumerable();


            var querydata = from n in modeldata
                            join m in manageData
                            on new { DocEntry = "SE-" + n.DocEntry, itemCode = n.ItemCode }
                            equals new { DocEntry = m.Field<string>("DocEntry"), itemCode = m.Field<string>("itemCode") } into temp
                            from t in temp.DefaultIfEmpty()
                            select new
                            {
                                id = n.Id,
                                DocEntry = n.DocEntry,
                                CardCode = n.CardCode,
                                CardName = n.CardName,
                                ItemCode = n.ItemCode,
                                ItemDesc = n.ItemDesc,
                                SlpName = n.SlpName,
                                ContractReviewCode = n.ContractReviewCode,
                                custom_req = n.custom_req,
                                ItemTypeName = n.ItemTypeName,
                                ItemName = n.ItemName,
                                SubmitTime = n.SubmitTime,
                                VersionNo = n.VersionNo,
                                FileUrl = n.FileUrl,
                                ProjectNo = t == null ? "" : t.Field<string>("_System_objNBS"),
                                ProduceNo = ""//t == null ? "" : t.Field<string>("_System_objNBS")
                            };
            //先把数据加载到内存
            if (!string.IsNullOrWhiteSpace(req.ProjectNo))
            {
                querydata = querydata.Where(t => t.ProjectNo == req.ProjectNo);
            }
            if (!string.IsNullOrWhiteSpace(req.ProduceNo))
            {
                querydata = querydata.Where(t => t.ProduceNo == req.ProduceNo);
            }
            if (!string.IsNullOrWhiteSpace(req.IsPro))
            {
                if (req.IsPro == "Y")
                {
                    querydata = querydata.Where(t => t.ProjectNo != null && t.ProjectNo != "");
                }
                else
                {
                    querydata = querydata.Where(t => t.ProjectNo == null || t.ProjectNo == "");
                }
            }
            if (!string.IsNullOrWhiteSpace(req.IsDraw))
            {
                if (req.IsDraw == "Y")
                {
                    querydata = querydata.Where(t => t.FileUrl != null && t.FileUrl != "");
                }
                else
                {
                    querydata = querydata.Where(t => t.FileUrl == null || t.FileUrl == "");
                }
            }
            if (!string.IsNullOrWhiteSpace(req.ItemTypeName))
            {
                querydata = querydata.Where(t => t.ItemTypeName == req.ItemTypeName);
            }
            if (!string.IsNullOrWhiteSpace(req.IsVersionNo))
            {
                if (req.IsVersionNo == "Y")
                {
                    querydata = querydata.Where(t => t.VersionNo != null && t.VersionNo != "");
                }
                else
                {
                    querydata = querydata.Where(t => t.VersionNo == null || t.VersionNo == "");
                }

            }
            if (req.sortorder == "ASC")
            {
                querydata = querydata.OrderBy(q => q.SubmitTime);
            }
            else
            {
                querydata = querydata.OrderByDescending(q => q.SubmitTime);
            }
            var data = querydata.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();

            result.Data = data;
            result.Count = querydata.Count();

            return result;
            #endregion

        }


        public async Task<Infrastructure.Response> AddDrawingFiles(List<int> ids, string url, string VersionNo)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            for (int i = 0; i < ids.Count; i++)
            {
                int v = ids[i];
                ManageScreening info = UnitWork.Find<ManageScreening>(q => q.Id == v).FirstOrDefault();

                info.FileUrl = url;
                info.VersionNo = VersionNo;
                await UnitWork.UpdateAsync<ManageScreening>(info);
            }

            await UnitWork.SaveAsync();
            response.Message = "操作成功";
            return response;
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
            DateTime date = DateTime.Now;
            foreach (var item in ItemCodeList)
            {
                var dataitem = await (from n in UnitWork.Find<sale_rdr1>(null)
                               .Where(s => s.sbo_id == SboId && s.DocEntry == DocEntry && s.ItemCode.Equals(item))
                                      join m in UnitWork.Find<sale_ordr>(null)
                                      on n.DocEntry equals m.DocEntry
                                      join s in UnitWork.Find<crm_oslp>(null)
                                      on m.SlpCode equals s.SlpCode
                                      select new
                                      {
                                          DocEntry = n.DocEntry,
                                          CardCode = m.CardCode,
                                          CardName = m.CardName,
                                          ItemCode = n.ItemCode,
                                          ItemDesc = n.Dscription,
                                          SlpCode = m.SlpCode,
                                          ContractReviewCode = n.ContractReviewCode,
                                          SlpName = s.SlpName
                                      }).FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(dataitem.ContractReviewCode))
                {
                    int ContractReviewCode = Convert.ToInt32(dataitem.ContractReviewCode);
                    List<string> typeList = new List<string> { "套线", "铝条&#92;铜条", "钣金" };
                    var data = (from n in UnitWork.Find<sale_contract_review>(q => q.sbo_id == 1)
                                join m in UnitWork.Find<sale_contract_review_detail>(q => q.sbo_id == 1)
                                on n.contract_id equals m.contract_id into temp
                                from t in temp
                                join s in UnitWork.Find<store_itemtype>(q => q.is_default == true && typeList.Contains(q.ItemTypeName))
                                 on t.ItemTypeID equals s.ItemTypeId into temp1
                                from t1 in temp1
                                where n.contract_id == ContractReviewCode
                                select new
                                {
                                    contract_id = n.contract_id,
                                    custom_req = n.custom_req,
                                    ItemTypeName = t1 == null ? "" : t1.ItemTypeName,
                                    ItemName = t == null ? "" : t.ItemCode
                                }).ToList();
                    foreach (var item1 in data)
                    {
                        ManageScreening manageScreening = new ManageScreening();
                        manageScreening.DocEntry = dataitem.DocEntry.ToString();
                        manageScreening.CardCode = dataitem.CardCode;
                        manageScreening.CardName = dataitem.CardName;
                        manageScreening.ItemCode = dataitem.ItemCode;
                        manageScreening.ItemDesc = dataitem.ItemDesc;
                        manageScreening.SlpCode = dataitem.SlpCode.ToInt();
                        manageScreening.SlpName = dataitem.SlpName;
                        manageScreening.SubmitTime = date;
                        manageScreening.ContractReviewCode = dataitem.ContractReviewCode.ToInt();
                        manageScreening.custom_req = item1.custom_req;
                        manageScreening.ItemTypeName = item1.ItemTypeName;
                        manageScreening.ItemName = item1.ItemName;
                        manageScreening.CreateUser = loginContext.User.User_Id.Value;
                        manageScreening.CreateDate = DateTime.Now;
                        await UnitWork.AddAsync<ManageScreening, int>(manageScreening);
                    }
                }
                else
                {
                    ManageScreening manageScreening = new ManageScreening();
                    manageScreening.DocEntry = dataitem.DocEntry.ToString();
                    manageScreening.CardCode = dataitem.CardCode;
                    manageScreening.CardName = dataitem.CardName;
                    manageScreening.ItemCode = dataitem.ItemCode;
                    manageScreening.ItemDesc = dataitem.ItemDesc;
                    manageScreening.SlpCode = dataitem.SlpCode.ToInt();
                    manageScreening.SlpName = dataitem.SlpName;
                    manageScreening.SubmitTime = date;
                    manageScreening.ContractReviewCode = dataitem.ContractReviewCode.ToInt();
                    manageScreening.custom_req = "";
                    manageScreening.ItemTypeName = "";
                    manageScreening.ItemName = "";
                    manageScreening.CreateUser = loginContext.User.User_Id.Value;
                    manageScreening.CreateDate = DateTime.Now;
                    await UnitWork.AddAsync<ManageScreening, int>(manageScreening);
                }
            }
            await UnitWork.SaveAsync();
            return result;
        }


        //public Infrastructure.Response PostDataToManager(int SboID, List<MaterialDes> list)// int SboId, int? SalesOrderId, string ItemCode, string CardCode, int Type)
        //{
        //    var result = new Infrastructure.Response();
        //    int OrderId = list[0].SalesOrderId.Value;

        //    #region 分配项目经理
        //    List<string> liujing = "A605,A608,A313,A302,CE-8,CE-7,CTE-8".Split(',').ToList();//刘静101
        //    List<string> wulei = "CT-4,CT-8,CE-4,CTE-4,CA,CJE,CJ,CGE,CGE,BT-4/8,BTE-4/8,BE-4/8,BA-4/8".Split(',').ToList();//武蕾103
        //    List<string> zhengjing = "CE-6,M202,M203,MGDW,MIGW,MGW,MGDW,MHW,MIHW".Split(',').ToList();//郑静102
        //    List<string> daijiawei = "MCD,MFF,MFYHS,MWL,MJZJ,MFXJ,MXFC,MFHL,MET,MRBH,MRH,MRF,MFB,MFYB,MRZH,MFYZ,MYSFR,MFSF,MYSHC,MYHF,MRSH,MRHF,MXFS,MZZ,MDCIR,MJR,MJF,MTP,MJY,MJJ,MCH,MCJ,MZJ".Split(',').ToList();//戴嘉魏292

        //    List<string> wenxiang = "MGDW,MIGW,MGW,MGDW,MHW,MIHW".Split(',').ToList();//温箱2
        //    List<string> changgui = "A605,A608,A313,A302,CT-4,CT-8,CE-4,CE-8,CE-7,CTE-4,CTE-8,CE-6,CA,CJE,CJ,CGE,BT-4/8,BTE-4/8,BE-4/8,BA-4/8".Split(',').ToList();//常规1
        //    #endregion
        //    for (int i = 0; i < list.Count; i++)
        //    {
        //        int manager = 104;
        //        int projectType = 1;
        //        string ItemCode = list[i].MaterialCode;
        //        int ReviewCode = list[i].ReviewCode.Value;

        //        foreach (var liujingitem in liujing)
        //        {
        //            if (ItemCode.IndexOf(liujingitem) > -1)
        //            {
        //                manager = 101;
        //                break;
        //            }
        //        }
        //        foreach (var wuleiitem in wulei)
        //        {
        //            if (ItemCode.IndexOf(wuleiitem) > -1)
        //            {
        //                manager = 103;
        //                break;
        //            }
        //        }
        //        foreach (var zhengjingitem in zhengjing)
        //        {
        //            if (ItemCode.IndexOf(zhengjingitem) > -1)
        //            {
        //                manager = 102;
        //                break;
        //            }
        //        }
        //        foreach (var daijiaweiitem in daijiawei)
        //        {
        //            if (ItemCode.IndexOf(daijiaweiitem) > -1)
        //            {
        //                manager = 292;
        //                projectType = 3;
        //                break;
        //            }
        //        }
        //        foreach (var wenxiangitem in wenxiang)
        //        {
        //            if (ItemCode.IndexOf(wenxiangitem) > -1)
        //            {
        //                projectType = 2;
        //                break;
        //            }
        //        }
        //        foreach (var changguiitem in changgui)
        //        {
        //            if (ItemCode.IndexOf(changguiitem) > -1)
        //            {
        //                projectType = 1;
        //                break;
        //            }
        //        }
        //        string getDataSql = "select * from nsap_bone.sale_contract_review_detail t where t.contract_id=" + ReviewCode + " and ItemTypeID in (14,15,43)";
        //        DataTable dt = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, getDataSql.ToString(), CommandType.Text, null);
        //        string banjin = "";
        //        string tongtiao = "";
        //        string taoxian = "";
        //        if (dt.Rows.Count > 0)
        //        {
        //            for (int j = 0; j < dt.Rows.Count; j++)
        //            {
        //                if (j == 0) banjin = dt.Rows[j]["ItemCode"].ToString();
        //                if (j == 1) tongtiao = dt.Rows[j]["ItemCode"].ToString();
        //                if (j == 2) taoxian = dt.Rows[j]["ItemCode"].ToString();
        //            }
        //        }
        //        //string banjinCode = UnitWork.Find<sale_contract_review_detail>

        //        string str = "exec add_projectFromERP @projectType=" + projectType + ",@customerCode=" + list[i].CustomerCode + ",@saleCode = " + OrderId + ",@banjinCode='" + banjin + "',@taoxianCode='" + taoxian + "',@tongtiaoCode = '" + tongtiao + "',@salesman='" + list[i].SalesMan + "',@projectManager=" + manager + ",@itemCode='" + ItemCode + "',@modelType=11,@peopleStr='' ";
        //        try
        //        {
        //            DataTable returnDt = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, str, CommandType.Text, null);
        //            string RecordGuid = returnDt.Rows[0]["RecordGuid"].ToString();
        //            var sale_rdr1 = UnitWork.Find<sale_rdr1>(s => s.sbo_id == SboID && s.DocEntry == OrderId && s.ItemCode.Equals(ItemCode)).FirstOrDefault();
        //            sale_rdr1.IsSync = 1;
        //            sale_rdr1.RecordGuid = RecordGuid;
        //            sale_rdr1.SubmitTime = DateTime.Now;
        //            UnitWork.Update<sale_rdr1>(sale_rdr1);
        //            UnitWork.SaveAsync();
        //        }
        //        catch (Exception)
        //        {
        //            result.Code = 500;
        //            result.Message = "内部出现错误";
        //            throw;
        //        }
        //    }
        //    return result;
        //}

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
            //var salesOrderIds = await UnitWork.Find<sale_rdr1>(q => !string.IsNullOrWhiteSpace(q.RecordGuid.ToString()) && q.Advance != 100).Select(q => q.RecordGuid).ToListAsync();

            //if (salesOrderIds.Count() > 0)
            //{
            //    for (int i = 0; i < salesOrderIds.Count; i++)
            //    {
            //        string id = salesOrderIds[i];
            //        sale_rdr1 rdr = UnitWork.Find<sale_rdr1>(s => s.RecordGuid == id).FirstOrDefault();
            //        string str = "select top 1 _System_Progress from OBJ162 where RecordGuid = '" + id + "'";
            //        DataTable returnDt = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, str, CommandType.Text, null);
            //        if (returnDt.Rows.Count > 0)
            //        {
            //            int progress = returnDt.Rows[0]["_System_Progress"].ToInt();
            //            rdr.Advance = progress;
            //            UnitWork.Update<sale_rdr1>(rdr);
            //        }
            //    }
            //}
            //await UnitWork.SaveAsync();
        }




        #region 获取进度
        public DataTable GetProgressAll()
        {
            string strSql = string.Format(@"select  _System_objNBS,RecordGuid, b.DocEntry, progress,itemCode from
                                            (select _System_objNBS, RecordGuid, progress, itemCode, DocEntry = cast('<v>' + replace(DocEntry, '/', '</v><v>') + '</v>' as xml) from
                                            (select * from(select _System_objNBS, RecordGuid, fld005508 DocEntry, max(_System_Progress) progress, fld005506 itemCode
                                            from OBJ162 group by RecordGuid, fld005508, _System_objNBS, fld005506) a) t
                                            ) as a outer apply(select DocEntry = T.C.value('.', 'varchar(20)') from a.DocEntry.nodes('v') as T(C)) as b");
            return UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null);
        }


        #endregion

        public async Task<TableData> GetMaterialRecord(MaterialRecordReq req)
        {

            string where1 = "";
            string where2 = " where 1=1 ";

            if (!string.IsNullOrEmpty(req.MaterialCode))
            {
                where1 += $" and  t1.MaterialCode = '{req.MaterialCode}' ";
                where2 += $" and  t1.MaterialCode = '{req.MaterialCode}' ";
            }
            if (!string.IsNullOrEmpty(req.CreateUser))
            {
                where1 += $" and  t3.CreateUser = '{req.CreateUser}' ";
                where2 += $" and  t3.CreateUserName  = '{req.CreateUser}' ";
            }
            if (!string.IsNullOrEmpty(req.DocEntry))
            {
                where1 += $" and  t2.id = '{req.DocEntry}' ";
                where2 += $" and  t2.id  = '{req.DocEntry}' ";
            }
            if (!string.IsNullOrEmpty(req.CustomerId))
            {
                where1 += $" and  t4.CustomerId = '{req.CustomerId}' ";
                where2 += $" and  t4.CustomerId = '{req.CustomerId}' ";
            }
            if (!string.IsNullOrEmpty(req.WhsCode))
            {
                where1 += $" and  t1.WhsCode = '{req.WhsCode}' ";
                where2 += $" and ( t1.GoodWhsCode = '{req.WhsCode}' or t1.SecondWhsCode = '{req.WhsCode}' ) ";
            }
            if (req.StartTime != null)
            {
                where1 += $" and  t3.CreateTime >= '{req.StartTime}' ";
                where2 += $" and  t3.createDate  >= '{req.StartTime}' ";
            }
            if (req.EndTime != null)
            {
                where1 += $" and  t3.CreateTime < '{req.EndTime}' ";
                where2 += $" and  t3.createDate < '{req.EndTime}' ";
            }
            string strSql = @$"select * from (
select t2.id as 'Id', t1.MaterialCode as 'MaterialCode',t1.MaterialDescription as 'MaterialDescription', t3.Quantity as 'Quantity' ,t3.CreateUser as 'Name',
t3.CreateTime as 'CreateTime',t1.WhsCode as 'WhsCode',t4.CustomerName as 'CustomerName' ,t4.CustomerId as 'CustomerId',t2.Remark  as 'Remark','1' as 'OrderType' 
from quotationmergematerial t1
inner join quotation t2 on t2.id = t1.QuotationId
inner join logisticsrecord t3 on t1.id =t3.QuotationMaterialId
inner join erp4_serve.serviceorder t4 on t2.ServiceOrderId = t4.id
where t2.Status =2 and t2.QuotationStatus =11 {where1}
UNION All
select t2.id as 'Id',t1.MaterialCode as 'MaterialCode',t1.MaterialDescription as 'MaterialDescription', '1' as 'Quantity',t3.CreateUserName as 'Name',
t3.createDate as 'CreateTime',CASE IsGood WHEN IsGood =1 THEN GoodWhsCode ELSE SecondWhsCode END  as 'WhsCode',t4.CustomerName as 'CustomerName',
t4.CustomerId as 'CustomerId',t1.ReceivingRemark as 'Remark' ,'2' as 'OrderType'
from returnnotematerial t1
inner join ReturnNote t2 on t2.id = t1.ReturnNoteId
inner join (select * from  erp4.flowinstanceoperationhistory 
where Content='仓库入库'  )t3 on t2.FlowInstanceId =t3.instanceid
inner join erp4_serve.serviceorder t4 on t2.ServiceOrderId = t4.id {where2}
)t ORDER BY t.CreateTime desc LIMIT {(req.pageIndex - 1) * req.pageSize},{req.pageSize}";


            string strSql2 = @$"select count(*) as 'num' from (
select t2.id as 'Id', t1.MaterialCode as 'MaterialCode',t1.MaterialDescription as 'MaterialDescription', t3.Quantity as 'Quantity' ,t3.CreateUser as 'Name',
t3.CreateTime as 'CreateTime',t1.WhsCode as 'WhsCode',t4.CustomerName as 'CustomerName' ,t4.CustomerId as 'CustomerId',t2.Remark  as 'Remark','1' as 'OrderType' 
from quotationmergematerial t1
inner join quotation t2 on t2.id = t1.QuotationId
inner join logisticsrecord t3 on t1.id =t3.QuotationMaterialId
inner join erp4_serve.serviceorder t4 on t2.ServiceOrderId = t4.id
where t2.Status =2 and t2.QuotationStatus =11 {where1}
UNION All
select t2.id as 'Id',t1.MaterialCode as 'MaterialCode',t1.MaterialDescription as 'MaterialDescription', '1' as 'Quantity',t3.CreateUserName as 'Name',
t3.createDate as 'CreateTime',CASE IsGood WHEN IsGood =1 THEN GoodWhsCode ELSE SecondWhsCode END  as 'WhsCode',t4.CustomerName as 'CustomerName',
t4.CustomerId as 'CustomerId',t1.ReceivingRemark as 'Remark' ,'2' as 'OrderType'
from returnnotematerial t1
inner join ReturnNote t2 on t2.id = t1.ReturnNoteId
inner join (select * from  erp4.flowinstanceoperationhistory 
where Content='仓库入库'  )t3 on t2.FlowInstanceId =t3.instanceid
inner join erp4_serve.serviceorder t4 on t2.ServiceOrderId = t4.id {where2}
)t ";
            var data = UnitWork.ExcuteSqlTable(ContextType.Nsap4MaterialDbContextType, strSql, CommandType.Text, null);

            var count = UnitWork.ExcuteSqlTable(ContextType.Nsap4MaterialDbContextType, strSql2, CommandType.Text, null);
            return new TableData
            {
                Data = data,
                Count = count.Rows[0][0].ToInt(),
            };
        }
    }
}
