﻿extern alias MySqlConnectorAlias;
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
using Newtonsoft.Json;
using OpenAuth.Repository.Domain.Material;
using static OpenAuth.App.Material.MaterialDesignApp;
using Org.BouncyCastle.Ocsp;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using OpenAuth.App.Order;
using Magicodes.ExporterAndImporter.Core.Extension;
using NPOI.OpenXmlFormats.Dml;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Logging;
using OpenAuth.App.Client;
using OpenAuth.App.ClientRelation.Response;
using NPOI.SS.Formula.Functions;
using System.Windows.Forms;
using OpenAuth.Repository.Domain.View;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository.Extensions;
using Z.BulkOperations;
using OpenAuth.App.Material.Response;
using Microsoft.EntityFrameworkCore.Internal;

namespace OpenAuth.App.Material
{
    
    public class MaterialDesignApp : OnlyUnitWorkBaeApp
    {
        private ILogger<MaterialDesignApp> _logger;
        public MaterialDesignApp(IUnitWork unitWork, ILogger<MaterialDesignApp> logger, IAuth auth) : base(unitWork, auth)
        {
            _logger = logger;
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
            DateTime nowTime = DateTime.Now;
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            string sql = string.Format(@" select  *   from   (select TO_DAYS(NOW())-TO_DAYS(n.SubmitTime) as SubmitDay,IFNULL(TO_DAYS(NOW())-TO_DAYS(n.UrlUpdate),0)  as UrlDay, n.Id,n.DocEntry,n.U_ZS,n.CardCode,n.CardName,n.ItemCode,n.ItemDesc,n.SlpName,n.ContractReviewCode,n.custom_req,n.ItemTypeName,n.ItemName,n.SubmitTime, n.VersionNo,n.FileUrl,
                                         n.DemoUpdate, n.UrlUpdate, n.Quantity, n.IsDemo, m.Id SubmitNo, s.DocEntry ProductNo,row_number() OVER(PARTITION BY n.itemCode,n.CardCode,n.SlpName, n.itemTypeName, n.Quantity ,n.DocEntry) AS rn
                                         from erp4_serve.manage_screening n
                                         left
                                         join erp4_serve.manage_screening_history m on n.DocEntry = m.DocEntry and n.U_ZS = m.U_ZS and n.ItemCode = m.ItemCode and n.Quantity = m.Quantity
                                         left join nsap_bone.product_owor s on n.DocEntry = s.OriginNum and n.ItemCode = s.ItemCode )  as n  where rn = 1 ");
            if (!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()))
            {
                sql += " and n.DocEntry =" + req.SalesOrderId;
            }
            if (!string.IsNullOrWhiteSpace(req.MaterialCode))
            {
                sql += " and n.ItemCode like '%" + req.MaterialCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.CustomerCode))
            {
                sql += " and n.CardCode like '%" + req.CustomerCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.custom_req))
            {
                sql += " and n.custom_req like '%" + req.custom_req + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.ItemName))
            {
                sql += " and n.ItemName like '%" + req.ItemName + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.VersionNo))
            {
                sql += " and n.VersionNo like '%" + req.VersionNo + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.SalesMan))
            {
                sql += " and n.SlpName like '%" + req.SalesMan + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.SubmitNo))
            {
                sql += " and n.SubmitNo = " + req.SubmitNo;
            }

            var modeldata = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql, CommandType.Text, null).AsEnumerable();
            var manageData = GetProgressAll().AsEnumerable(); // new DataTable().AsEnumerable();


            var querydata = from n in modeldata
                             join m in manageData
                            on new { DocEntry = "SE-" + n.Field<string>("DocEntry"), itemCode = n.Field<string>("ItemCode") }
                            equals new { DocEntry = m.Field<string>("DocEntry"), itemCode = m.Field<string>("itemCode") !=null ? m.Field<string>("itemCode").Trim() : m.Field<string>("itemCode") } into temp
                            from t in temp.DefaultIfEmpty()
                            select new MaterialRsp
                            {
                                id = n.Field<int>("Id"),
                                DocEntry = n.Field<string>("DocEntry"),
                                u_zs = n.Field<string>("U_ZS"),
                                CardCode = n.Field<string>("CardCode"),
                                CardName = n.Field<string>("CardName"),
                                ItemCode = n.Field<string>("ItemCode"),
                                ItemDesc = n.Field<string>("ItemDesc"),
                                SlpName = n.Field<string>("SlpName"),
                                ContractReviewCode = n.Field<int>("ContractReviewCode"),
                                custom_req = n.Field<string>("custom_req"),
                                ItemTypeName = n.Field<string>("ItemTypeName"),
                                ItemName = n.Field<string>("ItemName"),
                                SubmitTime = n.Field<DateTime>("SubmitTime"),
                                VersionNo = n.Field<string>("VersionNo"),
                                FileUrl = n.Field<string>("FileUrl"),
                                DemoUpdate = n.Field<DateTime?>("DemoUpdate"),
                                UrlUpdate = n.Field<DateTime?>("UrlUpdate"),
                                Quantity = n.Field<decimal?>("Quantity"),
                                IsDemo = n.Field<string>("IsDemo"),
                                type = (n.Field<Int32?>("SubmitDay") < 2) ? "-1"
                                : ((n.Field<Int32?>("SubmitDay") >= 2 && t == null) ? "0"
                                : ((n.Field<Int32?>("SubmitDay") >= 9 && string.IsNullOrEmpty(n.Field<string>("FileUrl"))) ? "1"
                                : ((n.Field<Int32?>("UrlDay") >= 10 && n.Field<string>("IsDemo") != "批量") ? "2"
                                : "3"))),
                                SubmitNo = n.Field<Int64?>("SubmitNo"),
                                ProjectNo = t == null ? "" : t.Field<string>("_System_objNBS"),
                                //ProCreatedDate = t == null ? "" : t.Field<string>("CreatedDate"),
                                ProduceNo = n.Field<int?>("ProductNo"),
                                Process = t == null ? 0 : t.Field<double?>("progress")
                            };
            //先把数据加载到内存
            if (!string.IsNullOrWhiteSpace(req.ProjectNo))
            {
                querydata = querydata.Where(t => t.ProjectNo == req.ProjectNo);
            }
            if (!string.IsNullOrWhiteSpace(req.ProduceNo))
            {
                querydata = querydata.Where(t => t.ProduceNo == Convert.ToInt32(req.ProduceNo));
            }
            if (!string.IsNullOrWhiteSpace(req.IsDemo))
            {
                if (req.IsDemo == "未设置")
                {
                    querydata = querydata.Where(t => t.IsDemo == null || t.IsDemo == "");
                }
                else
                {
                    querydata = querydata.Where(t => t.IsDemo == req.IsDemo);
                }
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
            if (!string.IsNullOrWhiteSpace(req.TimeRemind))
            {
                querydata = querydata.Where(t => t.type == req.TimeRemind);
            }
            if (req.sortorder == "ASC")
            {
                querydata = querydata.OrderBy(q => q.SubmitTime);
            }
            else
            {
                querydata = querydata.OrderByDescending(q => q.SubmitTime);
            }

            var datar = querydata.GroupBy(o => o.id).Select(o => o.FirstOrDefault());
            var data = datar.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            //20221226 replace the itemCode according to the request
            var MDetailList = new List<MCDetail>();
            foreach (var item in data)
            {
                if ( loginContext.Orgs.Exists(a => a.Name == "PMC"))
                {
                    item.CardName = "*";
                }
                if (item.ItemCode.StartsWith("M") && !MDetailList.Exists(a=>a.ItemCode == item.ItemCode))
                {
                    string sql2 = string.Format(@" SELECT  ItemCode, ItemTypeID , Contract_id    from sale_contract_review_detail  where contract_id =  {0} ", item.ContractReviewCode);
                   var  itemMList =  UnitWork.ExcuteSql<MCDetail>(ContextType.NsapBoneDbContextType, sql2, CommandType.Text, null);
                    MDetailList.AddRange(itemMList);
                }
            }
        
            //79 套线     80 钣金     81 机加
            foreach (var mitem in data)
            {
                if (mitem.ItemCode.Contains("M") && MDetailList.Exists(a => a.Contract_id == mitem.ContractReviewCode))
                {
                    //var mtypeid = MDetailList.Where(a => a.Contract_id == mitem.ContractReviewCode).FirstOrDefault().ItemTypeID;
                    //var mdetail = MDetailList.Where(a => a.Contract_id == mitem.ContractReviewCode).FirstOrDefault();
                    if (mitem.ItemTypeName == "套线")
                    {
                        // 79
                        mitem.ItemName = MDetailList.Where(a => a.Contract_id == mitem.ContractReviewCode && a.ItemTypeID == 79).FirstOrDefault().ItemCode;
                        //mitem.ItemTypeName = "套线";
                    }
                    if (mitem.ItemTypeName == "钣金")
                    {
                        //80
                        mitem.ItemName = MDetailList.Where(a => a.Contract_id == mitem.ContractReviewCode && a.ItemTypeID == 80).FirstOrDefault().ItemCode;
                    }
                    if (mitem.ItemTypeName != "钣金"  && mitem.ItemTypeName != "套线")
                    {
                        //81
                        mitem.ItemName = MDetailList.Where(a => a.Contract_id == mitem.ContractReviewCode && a.ItemTypeID == 81).FirstOrDefault().ItemCode;
                        mitem.ItemTypeName = "机加";
                    }
                    
                }
            }

            result.Data = data;
            result.Count = datar.Count();

            return result;
            #endregion

        }


        public TableData ScreeningCoupleView(SalesOrderMaterialReq req, int SboId)//, int? SalesOrderId, string ItemCode, string CardCode)
        {
            #region 注释
            DateTime nowTime = DateTime.Now;
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }


            var productionDetail = GetDataF(req);

            string sql = string.Format(@" select  *   from   (select TO_DAYS(NOW())-TO_DAYS(n.SubmitTime) as SubmitDay,IFNULL(TO_DAYS(NOW())-TO_DAYS(n.UrlUpdate),0)  as UrlDay, n.Id,n.DocEntry,n.U_ZS,n.CardCode,n.CardName,n.ItemCode,n.ItemDesc,n.SlpName,n.ContractReviewCode,n.custom_req,n.ItemTypeName,n.ItemName,n.SubmitTime, n.VersionNo,n.FileUrl,
                                         n.DemoUpdate, n.UrlUpdate, n.Quantity, n.IsDemo, m.Id SubmitNo, s.DocEntry ProductNo,row_number() OVER(PARTITION BY n.itemCode,n.CardCode,n.SlpName, n.itemTypeName, n.Quantity ,n.DocEntry) AS rn
                                         from erp4_serve.manage_screening n
                                         left
                                         join erp4_serve.manage_screening_history m on n.DocEntry = m.DocEntry and n.U_ZS = m.U_ZS and n.ItemCode = m.ItemCode and n.Quantity = m.Quantity
                                         left join nsap_bone.product_owor s on n.DocEntry = s.OriginNum and n.ItemCode = s.ItemCode )  as n  where rn = 1 ");
            #region filter query
            if (!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()))
            {
                sql += " and n.DocEntry =" + req.SalesOrderId;
            }
            if (!string.IsNullOrWhiteSpace(req.MaterialCode))
            {
                sql += " and n.ItemCode like '%" + req.MaterialCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.CustomerCode))
            {
                sql += " and n.CardCode like '%" + req.CustomerCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.custom_req))
            {
                sql += " and n.custom_req like '%" + req.custom_req + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.ItemName))
            {
                sql += " and n.ItemName like '%" + req.ItemName + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.VersionNo))
            {
                sql += " and n.VersionNo like '%" + req.VersionNo + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.SalesMan))
            {
                sql += " and n.SlpName like '%" + req.SalesMan + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.SubmitNo))
            {
                sql += " and n.SubmitNo = " + req.SubmitNo;
            }
            #endregion
            var modeldata = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql, CommandType.Text, null).AsEnumerable();
            var manageData = GetProgressAll().AsEnumerable(); // new DataTable().AsEnumerable();


            var querydata = from n in modeldata
                            join m in manageData
                            on new { DocEntry = "SE-" + n.Field<string>("DocEntry"), itemCode = n.Field<string>("ItemCode") }
                            equals new { DocEntry = m.Field<string>("DocEntry"), itemCode = m.Field<string>("itemCode") != null ? m.Field<string>("itemCode").Trim() : m.Field<string>("itemCode") } into temp
                            from t in temp.DefaultIfEmpty()
                            select new EchoView
                            {
                                //id = n.Field<int>("Id"),
                                docEntry =  n.Field<string>("DocEntry"),
                                U_ZS = n.Field<string>("U_ZS"),
                                CardCode = n.Field<string>("CardCode"),
                                CardName = n.Field<string>("CardName"),
                                itemCode = n.Field<string>("ItemCode"),
                                itemDesc = n.Field<string>("ItemDesc"),
                                slpName = n.Field<string>("SlpName"),
                                contractReviewCode = n.Field<int>("ContractReviewCode").ToString(),
                                custom_req = n.Field<string>("custom_req"),
                                itemTypeName = n.Field<string>("ItemTypeName"),
                                itemName = n.Field<string>("ItemName"),
                                submitTime = n.Field<DateTime>("SubmitTime"),
                                versionNo = n.Field<string>("VersionNo"),
                                fileUrl = n.Field<string>("FileUrl"),
                                demoUpdate = n.Field<DateTime?>("DemoUpdate").ToString(),
                                urlUpdate = n.Field<DateTime?>("UrlUpdate").ToString(),
                                quantity = n.Field<decimal?>("Quantity"),
                                isDemo = n.Field<string>("IsDemo"),
                                type = (n.Field<Int32?>("SubmitDay") < 2) ? "-1"
                                : ((n.Field<Int32?>("SubmitDay") >= 2 && t == null) ? "0"
                                : ((n.Field<Int32?>("SubmitDay") >= 9 && string.IsNullOrEmpty(n.Field<string>("FileUrl"))) ? "1"
                                : ((n.Field<Int32?>("UrlDay") >= 10 && n.Field<string>("IsDemo") != "批量") ? "2"
                                : "3"))),
                                submitNo = n.Field<Int64?>("SubmitNo").ToString(),
                                projectNo = t == null ? "" : t.Field<string>("_System_objNBS"),
                                //ProCreatedDate = t == null ? "" : t.Field<string>("CreatedDate"),
                                produceNo = n.Field<int?>("ProductNo").ToInt(),
                                process = t == null ? 0 : t.Field<double?>("progress").ToDouble(),
                                rn = 1,
                                origin = 0
                            };
            //先把数据加载到内存
            if (!string.IsNullOrWhiteSpace(req.ProjectNo))
            {
                querydata = querydata.Where(t => t.projectNo == req.ProjectNo);
            }
            if (!string.IsNullOrWhiteSpace(req.ProduceNo))
            {
                querydata = querydata.Where(t => t.produceNo == Convert.ToInt32(req.ProduceNo));
            }
            if (!string.IsNullOrWhiteSpace(req.IsDemo))
            {
                if (req.IsDemo == "未设置")
                {
                    querydata = querydata.Where(t => t.isDemo == null || t.isDemo == "");
                }
                else
                {
                    querydata = querydata.Where(t => t.isDemo == req.IsDemo);
                }
            }
            if (!string.IsNullOrWhiteSpace(req.IsPro))
            {
                if (req.IsPro == "Y")
                {
                    querydata = querydata.Where(t => t.projectNo != null && t.projectNo != "");
                }
                else
                {
                    querydata = querydata.Where(t => t.projectNo == null || t.projectNo == "");
                }
            }
            if (!string.IsNullOrWhiteSpace(req.IsDraw))
            {
                if (req.IsDraw == "Y")
                {
                    querydata = querydata.Where(t => t.fileUrl != null && t.fileUrl != "");
                }
                else
                {
                    querydata = querydata.Where(t => t.fileUrl == null || t.fileUrl == "");
                }
            }
            if (!string.IsNullOrWhiteSpace(req.ItemTypeName))
            {
                querydata = querydata.Where(t => t.itemTypeName == req.ItemTypeName);
            }
            if (!string.IsNullOrWhiteSpace(req.IsVersionNo))
            {
                if (req.IsVersionNo == "Y")
                {
                    querydata = querydata.Where(t => t.versionNo != null && t.versionNo != "");
                }
                else
                {
                    querydata = querydata.Where(t => t.versionNo == null || t.versionNo == "");
                }
            }
            if (!string.IsNullOrWhiteSpace(req.TimeRemind))
            {
                querydata = querydata.Where(t => t.type == req.TimeRemind);
            }
            if (req.sortorder == "ASC")
            {
                querydata = querydata.OrderBy(q => q.submitTime);
            }
            else
            {
                querydata = querydata.OrderByDescending(q => q.submitTime);
            }

            productionDetail.echoes.AddRange(querydata.ToList());
            var data = productionDetail.echoes.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
      


            //var data = querydata.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            //20221226 replace the itemCode according to the request
            var MDetailList = new List<MCDetail>();
            var produceNoList = data.Select(a => a.produceNo).ToList();
            foreach (var item in data)
            {
                if (item.origin ==1)
                {
                   
                    var info = UnitWork.Find<MPrOrDetail>(q => produceNoList.Contains(Convert.ToInt32(q.ProduceNo))).ToList();
                        var specificDataEcho = info.Where(q => q.CardCode == item.CardCode && q.ItemCode == item.itemCode && q.SlpName == item.slpName && q.ItemName == item.itemName && q.ProduceNo == item.produceNo.ToString()).FirstOrDefault();
                        if (specificDataEcho != null)
                        {
                            item.fileUrl = specificDataEcho.FileUrl;
                            item.versionNo = specificDataEcho.VersionNo;
                            item.urlUpdate = specificDataEcho.UrlUpdate.ToString();
                        }

                }
                if (loginContext.Orgs.Exists(a => a.Name == "PMC"))
                {
                    item.CardName = "*";
                }
                if (item.itemCode.StartsWith("M") && !MDetailList.Exists(a => a.ItemCode == item.itemCode))
                {
                    string sql2 = string.Format(@" SELECT  ItemCode, ItemTypeID , Contract_id    from sale_contract_review_detail  where contract_id =  {0} ", item.contractReviewCode);
                    var itemMList = UnitWork.ExcuteSql<MCDetail>(ContextType.NsapBoneDbContextType, sql2, CommandType.Text, null);
                    MDetailList.AddRange(itemMList);
                }
            }

            //79 套线     80 钣金     81 机加
            foreach (var mitem in data)
            {
                if (mitem.itemCode.Contains("M") && MDetailList.Exists(a => a.Contract_id.ToString() == mitem.contractReviewCode))
                {
                    //var mtypeid = MDetailList.Where(a => a.Contract_id == mitem.ContractReviewCode).FirstOrDefault().ItemTypeID;
                    //var mdetail = MDetailList.Where(a => a.Contract_id == mitem.ContractReviewCode).FirstOrDefault();
                    if (mitem.itemTypeName == "套线")
                    {
                        // 79
                        mitem.itemName = MDetailList.Where(a => a.Contract_id.ToString() == mitem.contractReviewCode && a.ItemTypeID == 79).FirstOrDefault().ItemCode;
                        //mitem.ItemTypeName = "套线";
                    }
                    if (mitem.itemTypeName == "钣金")
                    {
                        //80
                        mitem.itemName = MDetailList.Where(a => a.Contract_id.ToString() == mitem.contractReviewCode && a.ItemTypeID == 80).FirstOrDefault().ItemCode;
                    }
                    if (mitem.itemTypeName != "钣金" && mitem.itemTypeName != "套线")
                    {
                        //81
                        mitem.itemName = MDetailList.Where(a => a.Contract_id.ToString() == mitem.contractReviewCode && a.ItemTypeID == 81).FirstOrDefault().ItemCode;
                        mitem.itemTypeName = "机加";
                    }

                }
            }


            result.Data = data;
            result.Count = querydata.Count() + productionDetail.count;

            //result.Data = data;
            //result.Count = querydata.Count();

            return result;
            #endregion

        }


        public CoupleEcho GetDataF(SalesOrderMaterialReq req)//, int? SalesOrderId, string ItemCode, string CardCode)
        {
            #region 注释
            DateTime nowTime = DateTime.Now;
            var result = new CoupleEcho();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }


            string sql = string.Format(@" select  *   from   (
select '' as submitNo , CONVERT(p.OriginNum,char) as docEntry, p.CardCode ,p.U_ZS,c.CardName,p.itemCode,p.txtitemName as itemDesc ,p.PlannedQty as quantity,p.U_XT_CZ as slpName , p.CreateDate as submitTime,'' as contractReviewCode ,'' as  custom_req, '' as itemTypeName,s.ItemCode as itemName,'' as versionNo,'' as  projectNo, 0 as process, '' as fileUrl, '' as urlUpdate, '' as isDemo, '' as demoUpdate, p.DocEntry as produceNo, '' as type, 1 as origin ,row_number() OVER(PARTITION BY p.DocEntry,s.ItemCode) AS rn   from nsap_bone.product_owor AS p
left join nsap_bone.crm_ocrd as c on c.CardCode = p.CardCode
left join  nsap_bone.product_wor1 as  s on s.DocEntry = p.DocEntry
 where p.CmpltQty = 0   AND  p.ItemCode REGEXP 'CT-4|CT-5|CT-8|CE-4|CTH-8|CT-9|CE-4|CE-5|CE-6|CE-8|CE-7|CTE-4|CTE-8|CE-6|CE-5|CJE|CJ|CGE|CGE|CA|BT-4/8|BTE-4/8|BE-4/8|BA-4/8|MB|MGDW|MIGW|MGW|MGDW|MHW|MIHW|MCD|MFF|MFYHS|MWL|MJZJ|MFXJ|MXFC|MFHL|MET|MRBH|MRH|MRF|MFB|MFYB|MRZH|MFYZ|MYSFR|MFSF|MYSHC|MYHF|MRSH|MRHF|MXFS|MZZ|MDCIR|MJR|MJF|MTP|MJY|MJJ|MCH|MCJ|MOCV|MRGV|MRP|MXC|MS|MB' and s.ItemCode REGEXP 'A302|A303|A310|A312|A313|A314|A315|A316|A317|A318|A319|A320|A321|A322|A326|A604|A606|A608|A609|A612|A613|M203|M202|A603|A402|A414|A416|A417|A418|A406|A405|A407|A408|A420' ) as n where rn = 1  and n.submitTime >'2022-11-01'  ");
            if (!string.IsNullOrWhiteSpace(req.SalesOrderId.ToString()))
            {
                sql += " and n.DocEntry =" + req.SalesOrderId;
            }
            if (!string.IsNullOrWhiteSpace(req.MaterialCode))
            {
                sql += " and n.ItemCode like '%" + req.MaterialCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.CustomerCode))
            {
                sql += " and n.CardCode like '%" + req.CustomerCode + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.custom_req))
            {
                sql += " and n.custom_req like '%" + req.custom_req + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.ItemName))
            {
                sql += " and n.ItemName like '%" + req.ItemName + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.VersionNo))
            {
                sql += " and n.VersionNo like '%" + req.VersionNo + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.SalesMan))
            {
                sql += " and n.SlpName like '%" + req.SalesMan + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.SubmitNo))
            {
                sql += " and n.SubmitNo = " + req.SubmitNo;
            }
            if (!string.IsNullOrWhiteSpace(req.ProduceNo))
            {
                sql += " and n.produceNo = " + req.ProduceNo;
            }
            if (req.sortorder == "ASC")
            {
                sql += " ORDER BY submitTime ASC ";
            }
            else
            {
                sql += " ORDER BY submitTime DESC ";
            }
            var countsql = sql;
            //sql += " limit " + (req.page - 1) * req.limit + ", " + req.limit;
            var echodata = UnitWork.ExcuteSql<EchoView>(ContextType.NsapBoneDbContextType, sql, CommandType.Text, null).ToList();
            result.echoes.AddRange(echodata);

            //result.Count = modeldata.Count();
            //result.Data = modeldata.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            var countquery = "select count(1) count  from ( " + countsql + " ) s";
            var countCardList = UnitWork.ExcuteSql<CardCountDto>(ContextType.NsapBoneDbContextType, countquery.ToString(), CommandType.Text, null);
            result.count = countCardList.FirstOrDefault().count;
            return result;
            #endregion

        }



        public async Task<Infrastructure.Response> AddDrawingFiles(UpdateManageScreen manageScreen)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            int id = manageScreen.id;
            ManageScreening info = UnitWork.Find<ManageScreening>(q => q.Id == id).FirstOrDefault();
            if (info != null)
            {
                info.FileUrl = manageScreen.url;
                info.VersionNo = manageScreen.VersionNo;
                info.UrlUpdate = manageScreen.UrlUpdate;
                info.IsDemo = manageScreen.IsDemo;
                info.DemoUpdate = manageScreen.DemoUpdate;
            }
            await UnitWork.UpdateAsync<ManageScreening>(info);
            await UnitWork.SaveAsync();
            response.Message = "操作成功";
            return response;
        }


        public async Task<Infrastructure.Response> AddPDrawingFiles(UpdatePManageScreen manageScreen)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            MPrOrDetail info = UnitWork.Find<MPrOrDetail>(q => q.DocEntry == manageScreen.DocEntry && q.CardCode == manageScreen.CardCode && q.ItemCode == manageScreen.ItemCode  && q.SlpName == manageScreen.SlpName && q.ItemName == manageScreen.ItemName && q.ProduceNo == manageScreen.ProduceNo).FirstOrDefault();
            if (info != null)
            {
                info.FileUrl = manageScreen.FileUrl;
                info.VersionNo = manageScreen.VersionNo;
                info.UrlUpdate = DateTime.Now;
                info.IsDemo = manageScreen.IsDemo;
                await UnitWork.UpdateAsync<MPrOrDetail>(info);
            }
            else
            {
                await UnitWork.AddAsync<MPrOrDetail, int>(new MPrOrDetail
                {
                    DocEntry = manageScreen.DocEntry,
                    CardCode = manageScreen.CardCode,
                    CardName = manageScreen.CardName,
                    ItemCode = manageScreen.ItemCode,
                    SlpName = manageScreen.SlpName,
                    ItemName = manageScreen.ItemName,
                    Quantity = manageScreen.Quantity,
                    VersionNo = manageScreen.VersionNo,
                    FileUrl = manageScreen.FileUrl,
                    IsDemo = manageScreen.IsDemo,
                    ProduceNo = manageScreen.ProduceNo,
                    UrlUpdate = DateTime.Now,
                    CreateDate = DateTime.Now,
                    IsDelete = 0

                }) ;
            }
            
            await UnitWork.SaveAsync();
            response.Message = "操作成功";
            return response;
        }


        public async Task<Infrastructure.Response> ItemCodeSync(List<int> ids)
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

                string ItemTypeName = info.ItemTypeName;
                int contract_id = info.ContractReviewCode.Value;
                store_itemtype storeitemtype = UnitWork.Find<store_itemtype>(q => q.is_default == true && q.ItemTypeName == ItemTypeName).FirstOrDefault();
                if (storeitemtype != null)
                {
                    int typeid = storeitemtype.ItemTypeId;
                    sale_contract_review_detail contract_review_detail = UnitWork.Find<sale_contract_review_detail>(q => q.contract_id == contract_id && q.ItemTypeID == typeid).FirstOrDefault();
                    if (contract_review_detail != null)
                    {
                        info.ItemName = contract_review_detail.ItemCode;
                        await UnitWork.UpdateAsync<ManageScreening>(info);
                    }
                }
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

        public async Task<bool> AutoReviewCode(int SboId, int DocEntry, string ItemCode, string ReviewCode)
        {
            var result = true;

            var item = await UnitWork.Find<sale_rdr1>(s => s.sbo_id == SboId && s.DocEntry == DocEntry && s.ItemCode.Equals(ItemCode)).FirstOrDefaultAsync();
            item.ContractReviewCode = ReviewCode;
            await UnitWork.UpdateAsync<sale_rdr1>(item);
            await UnitWork.SaveAsync();
            return result;
        }



        /// <summary>
        /// 提交物料设计到筛选列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> SubmitItemCodeList(int SboId, int DocEntry, List<SubmitItemCode> ItemCodeList)
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
                var dataitem = await (from n in UnitWork.Find<sale_rdr1>(s => s.sbo_id == SboId && s.DocEntry == DocEntry && s.ItemCode == item.ItemCode && s.U_ZS == item.U_ZS && s.Quantity == item.Quantity)
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
                                          U_ZS = n.U_ZS,
                                          Quantity = n.Quantity,
                                          SlpName = s.SlpName
                                      }).FirstOrDefaultAsync();

                ManageScreeningHistory history = new ManageScreeningHistory();
                history.DocEntry = DocEntry.ToString();
                history.U_ZS = dataitem.U_ZS;
                history.ItemCode = dataitem.ItemCode;
                history.ItemDesc = dataitem.ItemDesc;
                history.Quantity = dataitem.Quantity.ToDecimal();
                history.ContractReviewCode = dataitem.ContractReviewCode;
                history.SlpCode = dataitem.SlpCode.ToInt();
                history.SubmitTime = date;
                history.CreateUser = loginContext.User.Name;
                await UnitWork.AddAsync<ManageScreeningHistory, int>(history);

                if (!string.IsNullOrEmpty(dataitem.ContractReviewCode))
                {
                    int ContractReviewCode = Convert.ToInt32(dataitem.ContractReviewCode);
                    List<string> typeList = new List<string> { "套线", "铝条&#92;铜条", "钣金", "机加" };
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
                                    custom_req = n.custom_req,
                                    ItemTypeName = t1 == null ? "" : t1.ItemTypeName,
                                    ItemName = t == null ? "" : t.ItemCode
                                }).ToList();
                    if (data == null || data.Count == 0)
                    {
                        string custom_req = UnitWork.Find<sale_contract_review>(q => q.sbo_id == 1 && q.contract_id == ContractReviewCode).Select(q=>q.custom_req).FirstOrDefault();
                        data = (from s in UnitWork.Find<store_itemtype>(q => q.is_default == true && typeList.Contains(q.ItemTypeName))
                                select new
                                {
                                    custom_req = custom_req,
                                    ItemTypeName = s.ItemTypeName,
                                    ItemName = ""
                                }).ToList();
                    }
                    foreach (var item1 in data)
                    {
                        ManageScreening manageScreening = new ManageScreening();
                        manageScreening.DocEntry = dataitem.DocEntry.ToString();
                        manageScreening.CardCode = dataitem.CardCode;
                        manageScreening.CardName = dataitem.CardName;
                        manageScreening.ItemCode = dataitem.ItemCode;
                        manageScreening.ItemDesc = dataitem.ItemDesc;
                        manageScreening.SlpCode = dataitem.SlpCode.ToInt();
                        manageScreening.U_ZS = dataitem.U_ZS;
                        manageScreening.Quantity = dataitem.Quantity.ToDecimal();
                        manageScreening.SlpName = dataitem.SlpName;
                        manageScreening.SubmitTime = date;
                        manageScreening.ContractReviewCode = dataitem.ContractReviewCode.ToInt();
                        manageScreening.custom_req = item1.custom_req;
                        manageScreening.ItemTypeName = item1.ItemTypeName;
                        manageScreening.ItemName = item1.ItemName;
                        manageScreening.CreateUser = loginContext.User.Name;
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
                    manageScreening.U_ZS = dataitem.U_ZS;
                    manageScreening.Quantity = dataitem.Quantity.ToDecimal();
                    manageScreening.CreateUser = loginContext.User.Name;
                    manageScreening.CreateDate = DateTime.Now;
                    await UnitWork.AddAsync<ManageScreening, int>(manageScreening);
                }
            }
            await UnitWork.SaveAsync();
            return result;
        }



        public async Task<Infrastructure.Response> AutoSubmitList(int SboId, int DocEntry, List<AutoSubmitItemCode> ItemCodeList)
        {
            var result = new Infrastructure.Response();
            DateTime date = DateTime.Now;
            foreach (var item in ItemCodeList)
            {
                var dataitem = await (from n in UnitWork.Find<sale_rdr1>(s => s.sbo_id == SboId && s.DocEntry == DocEntry && s.ItemCode == item.ItemCode && s.U_ZS == item.U_ZS && s.Quantity == item.Quantity && s.LineNum == item.LineNum)
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
                                          U_ZS = n.U_ZS,
                                          Quantity = n.Quantity,
                                          SlpName = s.SlpName
                                      }).FirstOrDefaultAsync();

                ManageScreeningHistory history = new ManageScreeningHistory();
                history.DocEntry = DocEntry.ToString();
                history.U_ZS = dataitem.U_ZS;
                history.ItemCode = dataitem.ItemCode;
                history.ItemDesc = dataitem.ItemDesc;
                history.Quantity = dataitem.Quantity.ToDecimal();
                history.ContractReviewCode = dataitem.ContractReviewCode;
                history.SlpCode = dataitem.SlpCode.ToInt();
                history.SubmitTime = date;
                history.CreateUser = "定时任务";
                await UnitWork.AddAsync<ManageScreeningHistory, int>(history);

                if (!string.IsNullOrEmpty(dataitem.ContractReviewCode))
                {
                    int ContractReviewCode = Convert.ToInt32(dataitem.ContractReviewCode);
                    List<string> typeList = new List<string> { "套线", "铝条&#92;铜条", "钣金", "机加" };
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
                                    custom_req = n.custom_req,
                                    ItemTypeName = t1 == null ? "" : t1.ItemTypeName,
                                    ItemName = t == null ? "" : t.ItemCode
                                }).ToList();
                    if (data == null || data.Count == 0)
                    {
                        string custom_req = UnitWork.Find<sale_contract_review>(q => q.sbo_id == 1 && q.contract_id == ContractReviewCode).Select(q => q.custom_req).FirstOrDefault();
                        data = (from s in UnitWork.Find<store_itemtype>(q => q.is_default == true && typeList.Contains(q.ItemTypeName))
                                select new
                                {
                                    custom_req = custom_req,
                                    ItemTypeName = s.ItemTypeName,
                                    ItemName = ""
                                }).ToList();
                    }
                    foreach (var item1 in data)
                    {
                        ManageScreening manageScreening = new ManageScreening();
                        manageScreening.DocEntry = dataitem.DocEntry.ToString();
                        manageScreening.CardCode = dataitem.CardCode;
                        manageScreening.CardName = dataitem.CardName;
                        manageScreening.ItemCode = dataitem.ItemCode;
                        manageScreening.ItemDesc = dataitem.ItemDesc;
                        manageScreening.SlpCode = dataitem.SlpCode.ToInt();
                        manageScreening.U_ZS = dataitem.U_ZS;
                        manageScreening.Quantity = dataitem.Quantity.ToDecimal();
                        manageScreening.SlpName = dataitem.SlpName;
                        manageScreening.SubmitTime = date;
                        manageScreening.ContractReviewCode = dataitem.ContractReviewCode.ToInt();
                        manageScreening.custom_req = item1.custom_req;
                        manageScreening.ItemTypeName = item1.ItemTypeName;
                        manageScreening.ItemName = item1.ItemName;
                        manageScreening.CreateUser = "定时任务";
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
                    manageScreening.U_ZS = dataitem.U_ZS;
                    manageScreening.Quantity = dataitem.Quantity.ToDecimal();
                    manageScreening.CreateUser = "定时任务";
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

        /// <summary>
        /// 自动提交工程设计
        /// </summary>
        /// <returns></returns>
        public async Task<bool> MByJob()
        {
            _logger.LogInformation("运行自动提交工程设计定时任务");
            var result = true;
            var saleOrders =  UnitWork.Find<sale_ordr>(c => c.CreateDate >= DateTime.Now.AddMinutes(-2)).ToList();
            //var saleOrders = UnitWork.Find<sale_ordr>(c => c.CreateDate >= DateTime.Now.AddHours(-120)).ToList();
            if (saleOrders.Count == 0)
            {
                return false;
            }
            else
            {
                var saleOrdersIds = saleOrders.Select(a => a.DocEntry).ToList();
                var querySaleOrder = String.Join(",", saleOrdersIds);
                var MItemList = GetMItemList(querySaleOrder);
                foreach (var mitem in MItemList)
                {
                    var specificSaleOrder = saleOrders.Where(c => c.DocEntry == mitem.DocEntry).FirstOrDefault();
                    var specificSaleOrderItems = UnitWork.Find<sale_rdr1>(c => c.DocEntry == mitem.DocEntry).ToList();
                    List<AutoSubmitItemCode> ItemCodeList = new List<AutoSubmitItemCode>();
                    if (mitem.U_RelDoc == "--")
                    {
                        // no need 
                        
                        if (haveContact(specificSaleOrder.DocEntry.ToString()))
                        {
                            ItemCodeList.Add(new AutoSubmitItemCode
                            { 
                                U_ZS =mitem.U_ZS,
                                ItemCode = mitem.ItemCode,
                                Quantity = mitem.Quantity,
                                LineNum = mitem.LineNum
                            });
                            _logger.LogInformation("自动提交工程设计,无需合约评审,参数为 DocEntry: " + mitem.DocEntry + " 提交参数： "+JsonConvert.SerializeObject(ItemCodeList));
                            await AutoSubmitList(1, mitem.DocEntry, ItemCodeList);
                        }
                        else
                        {
                            continue;
                        }

                    }
                    else
                    {
                        //need
                        var BindContractList = GetContractReviewList(mitem.ItemCode, specificSaleOrder.CardCode);
                        foreach (var citem in BindContractList)
                        {
                            if (mitem.ItemCode == citem.ItemCode && specificSaleOrder.SlpCode == citem.SlpCode && specificSaleOrder.CardCode == citem.CardCode )
                            {
                                if (haveContact(specificSaleOrder.DocEntry.ToString()))
                                {
                                    //bind contract review code
                                    var bindFlag = await AutoReviewCode(1, mitem.DocEntry, mitem.ItemCode, citem.Contract_Id.ToString());
                                    if (bindFlag)
                                    {
                                        ItemCodeList.Add(new AutoSubmitItemCode
                                        {
                                            U_ZS = mitem.U_ZS,
                                            ItemCode = mitem.ItemCode,
                                            Quantity = mitem.Quantity,
                                            LineNum = mitem.LineNum
                                        });
                                        _logger.LogInformation("自动提交工程设计,需合约评审,合约评审绑定号为：" + citem.Contract_Id.ToString() + " ,参数为 DocEntry: " + mitem.DocEntry + " 提交参数： " + JsonConvert.SerializeObject(ItemCodeList));
                                        await AutoSubmitList(1, mitem.DocEntry, ItemCodeList);
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }

                    }
                }

            }

            return result;
        }


        public List<AutoContractReview> GetContractReviewList(string DocNum, string CardCode)/*, string ations = "", string billPageurl = ""*/
        {
            TableData tableData = new TableData();

            StringBuilder stringBuilder = new StringBuilder();
            string strSql = string.Format("select a.contract_id contract_Id,a.CardCode, a.SlpCode, a.ItemCode, a.CardName, a.apply_dt Apply_dt,a.upd_dt ");
            strSql += string.Format(" FROM nsap_bone.sale_contract_review a");
            strSql += string.Format(" WHERE a.sbo_id = {0} AND a.ItemCode='" + DocNum.Replace("'", "\\'") + "' AND a.CardCode = '" + CardCode + "' AND a.apply_dt > DATE_SUB(CURDATE(), INTERVAL 6 MONTH)", 1);
            DataTable dts = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            tableData.Data = dts.Tolist<AutoContractReview>();
            return tableData.Data;
        }








        public bool haveContact(string DocNum)/*, string ations = "", string billPageurl = ""*/
        {
            var result = new TableData();
            bool flag = false;
            //合同类型为”商务合同“且点击【上传合同】按钮弹出的弹窗中选择”销售文件（我司为供货商）“，且合同状态为”结束“
            var info = from n in UnitWork.Find<ContractApply>(q => q.ContractType == "1" && q.ContractStatus == "-1")
                       join m in UnitWork.Find<ContractFileType>(q => q.FileType == "6")
                       on n.Id equals m.ContractApplyId into temp
                       from t in temp
                       where n.SaleNo == DocNum
                       select n;
            //合同类型为”工程设计申请“且合同状态为”结束“
            var info1 = from n in UnitWork.Find<ContractApply>(q => q.ContractType == "3" && q.ContractStatus == "-1")
                        where n.SaleNo == DocNum
                        select n;
            if ((info != null && info.Count() > 0) || (info1 != null && info1.Count() > 0)) flag = true;
            return flag;
        }

        public List<OrderItemInfo> GetMItemList(string DocEntry)/*, string ations = "", string billPageurl = ""*/
        {
            string tablename = "sale_rdr1";
            TableData tableData = new TableData();
            bool ViewSales = true;
            //int SboId = _serviceBaseApp.GetUserNaspSboID(userId);

            StringBuilder stringBuilder = new StringBuilder();
            string strSql = string.Format(" SELECT  d.ItemCode,d.DocEntry,Dscription,d.Quantity,d.LineNum ," +
                         "IF(" + ViewSales + ",Price,0)Price," +
                         "IF(" + ViewSales + ",LineTotal,0) LineTotal," +
                         "IF(" + ViewSales + ", StockPrice, 0) StockPrice,");
            strSql += string.Format("d.WhsCode,w.OnHand,");
            strSql += string.Format("d.U_ZS");
            strSql += ",(CASE WHEN d.ItemCode REGEXP 'A605|A608|A313|A302|BT-4/8|BTE-4/8|BE-4/8|BA-4/8|M202|M203|A405|CT-4XXX-5V12A|CA|MB|MJR|MJF|MTP|MJY|MCJ|MZJ|MDCJ|CT-5|CTH-8|A604|A316' THEN '--'  WHEN (LOCATE('mA', d.ItemCode)  !=0  || REGEXP_LIKE(d.ItemCode, 'V([0-9]|[0-9].[0-9]|1[0-2])A') =  1) &&  LOCATE('CT-4', d.ItemCode)  !=0   THEN '--'    ELSE d.ContractReviewCode END) U_RelDoc";
            strSql += string.Format(" FROM {0}." + tablename + " d", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_oitw w ON d.ItemCode=w.ItemCode AND d.WhsCode=w.WhsCode AND d.sbo_id=w.sbo_id", "nsap_bone");
            strSql += string.Format(" left join manage_screening m on d.DocEntry = m.DocEntry and d.ItemCode = m.ItemCode and d.U_ZS = m.U_ZS and d.Quantity = m.Quantity", "erp4_serve");
            strSql += string.Format(" WHERE m.DocEntry is null and d.DocEntry in ( " + DocEntry + "  ) AND d.sbo_id={0}", 1);
            strSql += string.Format(" and d.ItemCode REGEXP 'A605|A608|A313|A302|CT-4|CT-8|CE-4|CE-8|CE-7|CTE-4|CTE-8|CE-6|CA|CJE|CJ|CGE|CGE|BT-4/8|BTE-4/8|BE-4/8|BA-4/8|M202|M203|MGDW|MIGW|MGW|MGDW|MHW|MIHW|MCD|MFF|MFYHS|MWL|MJZJ|MFXJ|MXFC|MFHL|MET|MRBH|MRH|MRF|MFB|MFYB|MRZH|MFYZ|MYSFR|MFSF|MYSHC|MYHF|MRSH|MRHF|MXFS|MZZ|MDCIR|MJR|MJF|MTP|MJY|MJJ|MCH|MCJ|MOCV|MRGV|MJ|MRP|MXC|MS|MB|MZJ|BT' ");
            tableData.Count = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, strSql.ToString(), CommandType.Text, null).Rows.Count;

            strSql += string.Format(" order by d.ItemCode ");
            DataTable dts = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, strSql.ToString(), CommandType.Text, null);

            tableData.Data = dts.Tolist<OrderItemInfo>();
            return tableData.Data;
        }



        #region 获取进度
        public DataTable GetProgressAll()
        {
            string strSql = string.Format(@" select  _System_objNBS, b.DocEntry, progress,itemCode from
                                            (select _System_objNBS,CreatedDate, RecordGuid, progress, itemCode, DocEntry = cast('<v>' + replace(DocEntry, '/', '</v><v>') + '</v>' as xml) from
                                            (select * from(select _System_objNBS,CreatedDate, RecordGuid, fld005508 DocEntry, max(_System_Progress) progress, fld005506 itemCode
                                            from OBJ162 group by RecordGuid, fld005508, _System_objNBS, fld005506,CreatedDate) a) t
                                            ) as a outer apply(select DocEntry = T.C.value('.', 'varchar(20)') from a.DocEntry.nodes('v') as T(C)) as b");
            strSql += string.Format(@"  union all     select  _System_objNBS, b.DocEntry, progress,itemCode from
                                            (select _System_objNBS,CreatedDate, RecordGuid, progress, itemCode, DocEntry = cast('<v>' + replace(DocEntry, '/', '</v><v>') + '</v>' as xml) from
                                            (select * from(select _System_objNBS,CreatedDate, RecordGuid, fld017268 DocEntry, max(_System_Progress) progress, fld005787 itemCode
                                            from OBJ170 group by RecordGuid, fld005787, _System_objNBS, fld017268,CreatedDate) a) t
                                            ) as a outer apply(select DocEntry = T.C.value('.', 'varchar(20)') from a.DocEntry.nodes('v') as T(C)) as b");

            strSql += string.Format(@"  union all     select  _System_objNBS, b.DocEntry, progress,itemCode from
                                            (select _System_objNBS,CreatedDate, RecordGuid, progress, itemCode, DocEntry = cast('<v>' + replace(DocEntry, '/', '</v><v>') + '</v>' as xml) from
                                            (select * from(select _System_objNBS,CreatedDate, RecordGuid, fld005878 DocEntry, max(_System_Progress) progress, fld005879 itemCode
                                            from OBJ163 group by RecordGuid, fld005878, _System_objNBS, fld005879,CreatedDate) a) t
                                            ) as a outer apply(select DocEntry = T.C.value('.', 'varchar(20)') from a.DocEntry.nodes('v') as T(C)) as b");

            strSql += string.Format(@"  union all     select  _System_objNBS, b.DocEntry, progress,itemCode from
                                            (select _System_objNBS,CreatedDate, RecordGuid, progress, itemCode, DocEntry = cast('<v>' + replace(DocEntry, '/', '</v><v>') + '</v>' as xml) from
                                            (select * from(select _System_objNBS,CreatedDate, RecordGuid, fld005717 DocEntry, max(_System_Progress) progress, fld005719 itemCode
                                            from OBJ169 group by RecordGuid, fld005717, _System_objNBS, fld005719,CreatedDate) a) t
                                            ) as a outer apply(select DocEntry = T.C.value('.', 'varchar(20)') from a.DocEntry.nodes('v') as T(C)) as b");
            var dt = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null);

            return dt;
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



        public string AddBOMExcel(DataTable dt, string filename)
        {
            this._logger.LogError("bom导入excel开始：", Array.Empty<object>());
            if (dt.Rows.Count < 1 || dt.Columns.Count != 5)
            {
                return null;
            }
            this._logger.LogError("bom导入excel（dt.Rows.Count）：" + dt.Rows.Count.ToString(), Array.Empty<object>());
            string text = string.Format("select excel_id from store_oitt_excel where excel_nm= '" + filename + "' order by crt_dt desc", Array.Empty<object>());
            DataTable dataTable = this.UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, text.ToString(), CommandType.Text, null);
            int num = 1;
            if (dataTable != null && dataTable.Rows.Count > 0)
            {
                num = dataTable.Rows.Count + 1;
            }
            string text2 = string.Format("INSERT INTO {0}.store_oitt_excel (excel_nm,ItemCode,sbo_id,U_BBH,crt_dt,opt_id,is_import) ", "nsap_bone");
            text2 += string.Format("VALUES ('{0}','{1}',{2},'{3}','{4}','{5}',{6});SELECT LAST_INSERT_ID() NewIdentity;", new object[]
            {
        filename,
        StringExtension.FilterESC(filename),
        1,
        "V1." + num.ToString().PadLeft(2, '0'),
        DateTime.Now.ToString(),
        0,
        0
            });
            if (this.UnitWork.ExecuteSql(text2.ToString(), ContextType.NsapBoneDbContextType) > 0)
            {
                dataTable = this.UnitWork.ExcuteSqlTable(ContextType.NsapBoneDbContextType, text.ToString(), CommandType.Text, null);
                string arg = "";
                if (dataTable != null && dataTable.Rows.Count > 0)
                {
                    arg = dataTable.Rows[0]["excel_id"].ToString();
                }
                int num2 = 0;
                StringBuilder stringBuilder = new StringBuilder();
                List<MySqlParameter> list = new List<MySqlParameter>();
                stringBuilder.AppendFormat("INSERT INTO {0}.store_oitt_excel_detail (excel_id,excel_line,crt_dt,MaterialNo,PCBFootprint,ItemValue,Quantity,Reference,ItemCode) VALUES ", "nsap_bone");
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    string text3 = dt.Rows[i][0].ToString().Trim();
                    string text4 = "";
                    if (!string.IsNullOrEmpty(text3))
                    {
                        text4 = this.GetItemCodeByPCBA(text3, 1.ToString());
                    }
                    num2++;
                    stringBuilder.AppendFormat("({1},?excel_line{0},?crt_dt{0},?MaterialNo{0},?PCBFootprint{0},?ItemValue{0},?Quantity{0},?Reference{0},?ItemCode{0}),", num2, arg);
                    list.Add(new MySqlParameter(string.Format("?excel_line{0}", num2), num2 - 1));
                    list.Add(new MySqlParameter(string.Format("?crt_dt{0}", num2), DateTime.Now.ToString()));
                    list.Add(new MySqlParameter(string.Format("?MaterialNo{0}", num2), text3));
                    list.Add(new MySqlParameter(string.Format("?PCBFootprint{0}", num2), dt.Rows[i][2].ToString()));
                    list.Add(new MySqlParameter(string.Format("?ItemValue{0}", num2), dt.Rows[i][1].ToString()));
                    list.Add(new MySqlParameter(string.Format("?Quantity{0}", num2), float.Parse((dt.Rows[i][4].ToString() == "") ? "0" : dt.Rows[i][4].ToString())));
                    list.Add(new MySqlParameter(string.Format("?Reference{0}", num2), dt.Rows[i][3].ToString().Trim()));
                    list.Add(new MySqlParameter(string.Format("?ItemCode{0}", num2), text4));
                }
                string text5 = string.Format("{0};", stringBuilder.ToString().TrimEnd(','));
                try
                {
                    this.UnitWork.ExecuteNonQuery(ContextType.NsapBoneDbContextType, CommandType.Text, text5, new object[]
                    {
                list
                    });
                }
                catch (Exception ex)
                {
                    this._logger.LogError("bom导入excel（ex.Message）：" + ex.Message, Array.Empty<object>());
                    throw;
                }
            }
            return "1";
        }


        public string GetItemCodeByPCBA(string pcbaCode, string sboId)
        {
            string text = string.Format("SELECT ItemCode FROM {0}.store_oitm WHERE U_PCBA_item_ID='{2}' AND sbo_id = {1} limit 1", "nsap_bone", sboId, StringExtension.FilterSQL(pcbaCode));
            object obj = this.UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, text, CommandType.Text, null);
            if (obj != null)
            {
                return obj.ToString();
            }
            return "";
        }





        #region 工程部考勤
        /// <summary>
        /// 查看视图【统计分析】
        /// </summary>
        /// <returns></returns>
        public TableData TaskView(TaskViewReq req, int SboId)//, int? SalesOrderId, string ItemCode, string CardCode)
        {
            #region 注释
            DateTime nowTime = DateTime.Now;
            var result = new TableData();

            var userInfo = _auth.GetCurrentUser();
            if (userInfo == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //判断是否是工程部主管  工程部HR
            bool isSupervisor = userInfo.Roles.Any(r => r.Name == "工程部主管" || r.Name == "工程部HR");
            //get all submit to statistics data 
            List<string> NumberList = new List<string>();
            List<TaskView> statisticView = new List<TaskView>();
            if (!string.IsNullOrEmpty(req.Month))
            {
                statisticView.AddRange(UnitWork.Find<TaskView>(q => q.Month == req.Month && q.IsDelete ==0).ToList());
                NumberList.AddRange(statisticView.Select(q => q.Number).ToList());
                
            }
            else
            {
                statisticView.AddRange(UnitWork.Find<TaskView>(q =>  q.IsDelete == 0).ToList());
                NumberList.AddRange(statisticView.Select(q => q.Number).ToList());
            }

            string sql = string.Format(@"select Owner,Number,objnbs,StageName,fld005506,complete,
                            fld006314,isFinished , duedate ,DueDays ,AssignedBy ,AssignedTo,CreatedBy ,CreatedDate,
                            Owner,AssignDate,startDate,DATEADD(dd,-DueDays,duedate) Completedate,'' as Month
                            from TaskView5
                            where 1=1  ");
            string sqlcount = string.Format(@"select count(1) count
                            from ( select  *  from TaskView5
                            where 1=1  ");
            string sqlwhere = "";
            if (!string.IsNullOrWhiteSpace(req.Owner))
            {
                sqlwhere += " and Owner like N'%" + req.Owner + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.AssignedTo))
            {
                sqlwhere += " and AssignedTo like N'%" + req.AssignedTo + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.Number))
            {
                sqlwhere += " and Number like N'%" + req.Number + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.objnbs))
            {
                sqlwhere += " and objnbs like N'%" + req.objnbs + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.StageName))
            {
                sqlwhere += " and StageName like N'%" + req.StageName + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.fld005506))
            {
                sqlwhere += " and fld005506 like N'%" + req.fld005506 + "%'";
            }
            if (!string.IsNullOrWhiteSpace(req.fld006314))
            {
                sqlwhere += " and fld006314 = N'" + req.fld006314+"'";
            }
            if (!string.IsNullOrWhiteSpace(req.complete))
            {
                sqlwhere += " and complete = " + req.complete;
            }
            if (req.isFinished == 0 || req.isFinished == 1)
            {
                sqlwhere += " and isFinished = " + req.isFinished;
            }
            if (req.AssignDateStart != null)
            {
                sqlwhere += " and AssignDate >= '" + req.AssignDateStart + "'";
            }
            if (req.AssignDateEnd != null)
            {
                sqlwhere += " and AssignDate <= '" + req.AssignDateEnd + "'";
            }
            if (req.duedateStart != null)
            {
                sqlwhere += " and DATEADD(dd,-DueDays,duedate)  >= '" + req.duedateStart + "'";
            }
            if (req.duedateEnd != null)
            {
                sqlwhere += " and DATEADD(dd,-DueDays,duedate) <= '" + req.duedateEnd + "'";
            }
            if (!string.IsNullOrEmpty(req.DutyFlag))
            {
                if (req.DutyFlag=="1")
                {
                    string str = String.Join(",", NumberList.Select(x => $"'{x}'"));
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        sqlwhere += string.Format(@"and Number in (" + str + ")");
                    }
                }
                if (req.DutyFlag == "0")
                {
                    string str = String.Join(",", NumberList.Select(x => $"'{x}'"));
                    if (!string.IsNullOrWhiteSpace(str))
                    {
                        sqlwhere += string.Format(@"and Number not  in (" + str + ")");
                    }
                }
            }

            if (!isSupervisor)
            {
                //get  related account
                StringBuilder strSql = new StringBuilder();
                strSql.AppendFormat("select * from manageaccountbind u  where u.LName =  \"{0}\"  and  u.IsDelete = 0 ", userInfo.User.Name);
                var erp4Bind = UnitWork.ExcuteSql<ManageAccountBind>(ContextType.DefaultContextType, strSql.ToString(), CommandType.Text, null).FirstOrDefault();
                if (erp4Bind != null)
                {
                    //sql = sql.Where(q => q.Owner == erp4Bind.MName);
                    sqlwhere += string.Format(@"and Owner = " + erp4Bind.MName);
                }

            }
            sql += sqlwhere;
            sqlcount += sqlwhere;
            sqlcount += " ) T ";
            sql += "  ORDER BY CreatedDate DESC  OFFSET   " + ((req.page - 1) * req.limit).ToString() + "   ROWS  FETCH NEXT  " + req.limit.ToString() + "  ROWS ONLY    ";

            var modeldata = UnitWork.ExcuteSql<statisticsTable>(ContextType.ManagerDbContext, sql, CommandType.Text, null).ToList();
            var countList = UnitWork.ExcuteSql<CardCountDto>(ContextType.ManagerDbContext, sqlcount.ToString(), CommandType.Text, null);

            var alterLevelData = modeldata.Where(a => a.objnbs.Contains("PRJA") || a.objnbs.Contains("PRJW")).ToList();
            string strSql2 = string.Format("	SELECT * from ( select b._System_objNBS as objnbs , b.fld005787 as itemcode ,b.fld017268 as num , b.RecordGuid,b.fld017270 as levelm , b.deleted from (select * from OBJ170 where idRecord in (select max(idRecord) from OBJ170 group by _System_objNBS )) b union all select d._System_objNBS as objnbs ,d.fld005719 as itemcode ,d.fld005717 as num ,d.RecordGuid,d.fld017273 as levelm ,d.deleted from (select * from OBJ169 where idRecord in (select max(idRecord) from OBJ169 group by _System_objNBS)) d) as t WHERE deleted =0");
            var LevelList = UnitWork.ExcuteSql<LevelMDetails>(ContextType.ManagerDbContext, strSql2, CommandType.Text, null);
            foreach (var item in modeldata)
            {
                //20230101 change manage default level
                if (LevelList.Exists(a=>a.objnbs == item.objnbs))
                {
                    var specificLevelDetail = LevelList.Where(a => a.objnbs == item.objnbs).FirstOrDefault();
                    item.fld006314 = specificLevelDetail.levelm;
                }
 
                var specSta = statisticView.Where(u => u.Number == item.Number && u.IsDelete != 1).FirstOrDefault();
                if (specSta!=null)
                {
                    item.Month = statisticView.Where(u => u.Number == item.Number).FirstOrDefault().Month;
                }
                
            }

            #region rejected code 
            //var taskView = UnitWork.Find<TaskView>(null);
            //if (!string.IsNullOrWhiteSpace(req.Month))
            //{
            //    taskView.Where(q => q.Month == req.Month);
            //}

            //var manageData = taskView.ToDataTable().AsEnumerable(); // new DataTable().AsEnumerable();


            //var querydata = from n in modeldata
            //                 join m in statisticView
            //                 on new { Number = n.Field<string>("Number") }
            //                equals new { Number = m.Number } into temp
            //                from t in temp.DefaultIfEmpty()
            //                select new
            //                {
            //                    Owner = n.Field<string>("Owner"),
            //                    Number = n.Field<string>("Number"),
            //                    objnbs = n.Field<string>("objnbs"),
            //                    StageName = n.Field<string>("StageName"),
            //                    fld005506 = n.Field<string>("fld005506"),
            //                    complete = n.Field<int?>("complete"),
            //                    isFinished = n.Field<bool?>("isFinished"),
            //                    fld006314 = n.Field<string>("fld006314"),
            //                    duedate = n.Field<DateTime?>("duedate"),
            //                    DueDays = n.Field<int?>("DueDays"),
            //                    AssignedBy = n.Field<string>("AssignedBy"),
            //                    AssignedTo = n.Field<string>("AssignedTo"),
            //                    CreatedBy = n.Field<string>("CreatedBy"),
            //                    CreatedDate = n.Field<DateTime?>("CreatedDate"),
            //                    AssignDate = n.Field<DateTime?>("AssignDate"),
            //                    startDate = n.Field<DateTime?>("startDate"),
            //                    Completedate = n.Field<DateTime?>("Completedate"),
            //                    Month = t == null ? "" : t.Month
            //                };
            //if (req.Status != null)
            //{
            //    if (req.Status == "Y")
            //    {
            //        querydata = querydata.Where(q => q.Month != "");
            //    }
            //    else
            //    {
            //        querydata = querydata.Where(q => q.Month == "");
            //    }
            //}
            //if (req.Month != null)
            //{
            //    querydata = querydata.Where(q => q.Month == req.Month);
            //}
            #endregion 


           // var data = querydata.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();

            result.Data = modeldata.ToList();
            result.Count = countList.FirstOrDefault().count;

            return result;
            #endregion

        }

        /// <summary>
        /// 提交到月度统计
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<Infrastructure.Response> SubmitMonth(submitMonth req)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            List<TaskView> list = new List<TaskView>();
            for (int i = 0; i < req.Number.Count; i++)
            {
                string v = req.Number[i];
                TaskView info = UnitWork.Find<TaskView>(q => q.Number == v && q.Month == req.Month).FirstOrDefault();
                if (info == null)
                {
                    TaskView item = new TaskView();
                    item.Number = v;
                    item.Month = req.Month;
                    item.CreateUser = loginContext.User.Name;
                    item.CreateDate = DateTime.Now;
                    item.UpdateUser = loginContext.User.Name;
                    item.UpdateDate = DateTime.Now;
                    list.Add(item);
                }
            }
            UnitWork.BatchAdd<TaskView, int>(list.ToArray());
            await UnitWork.SaveAsync();
            response.Message = "操作成功";
            return response;
        }


        public async Task<Infrastructure.Response> WithDarwSubmit(withdarwSubmitReq req)
        {
            var response = new Infrastructure.Response();
            response.Message = "";
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var rejectedList = UnitWork.Find<TaskView>(q => req.Number.Contains(q.Number)).ToList();
            foreach (var rejecteditem in rejectedList)
            {
                rejecteditem.IsDelete = 1;
            }
            UnitWork.BatchUpdate<TaskView>(rejectedList.ToArray());
            await UnitWork.SaveAsync();
            response.Message = "操作成功";
            return response;
        }


        public List<DataTable> DataView(string date)
        {
            List<DataTable> list = new List<DataTable>();
            string start = Convert.ToDateTime(date + "-01").ToString("yyyy-MM-dd");
            string end = Convert.ToDateTime(date + "-01").AddMonths(1).ToString("yyyy-MM-dd");
            //数据概览
            string strSql = string.Format(@" SELECT
                            convert(varchar(100),A.DT,23) '日期',
                            ISNULL(T.NUM,'0') '分配数',
                            ISNULL(T1.NUM,'0') '完成数'
                            FROM 
                            (select DATEADD(D,NUMBER,'" + start + "') DT FROM master..spt_values WHERE type='P' AND DATEADD(D,NUMBER, '" + start + "') < '" + end + "') A LEFT JOIN");
            strSql += string.Format(@"(SELECT AssignTime AS date,count(Number) AS num 
                            from  TaskView5
                            where  AssignTime >= '" + start + "' AND AssignTime < '" + end + "' group by AssignTime ) T ON T.date = A.DT LEFT JOIN");
            strSql += string.Format(@"  (SELECT CompleteTime date,count(Number) AS num 
                            from  TaskView5
                            where isFinished = 1 and  CompleteTime >= '" + start + "' AND CompleteTime < '" + end + "' group by CompleteTime) T1 ON T1.date = A.DT");
            list.Add(UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null));
            //难度
            strSql = string.Format(@" select  name,
                            count(1) as value
                            from (select fld006314 name from TaskView5 where StartDate >= '" + start + "' AND StartDate < '" + end + "') t group by name");
            list.Add(UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null));
            //是否延期
            strSql = string.Format(@"
                            select  name,
                            count(1) as value
                            from (select (case when DueDays>=0 then N'按时完成' else N'延期完成' end ) name from TaskView5 where CompleteTime >= '" + start + "' AND CompleteTime < '" + end + "') t group by name");
            list.Add(UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null));
            //进度
            strSql = string.Format(@"
                            select  name,
                            count(1) as value
                            from (select Status name from TaskView5 where CompleteTime >= '" + start + "' AND CompleteTime < '" + end + "') t group by name");
            list.Add(UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null));


            //评分明细表
            var timeList = date.Split('.');
            string timeStr = timeList[0] + "年" + timeList[1] + "月";
            var NumberList = UnitWork.Find<TaskView>(q => q.Month == timeStr).Select(q => q.Number).ToList();
            string str = String.Join(",", NumberList.Select(x => $"'{x}'"));
            strSql = string.Format(@"select
                                     name
                                     ,count(case when t.fld006314 = N'一般' then 1 else null end)*0.5 as LowDifficulty
                                      ,count(case when t.fld006314 = N'中等' then 1 else null end) as MediumDifficulty
                                      ,count(case when t.fld006314 = N'高级' then 1 else null end)*2 as HighDifficulty
                                      ,count(case when t.fld006314 = N'特级' then 1 else null end)*3 as SuperDifficulty
                                      ,count(case when t.DueDays >= 0 then 1 else null end) as OnTime
                                      ,count(case when t.DueDays < 0 then 1 else null end) as Delayed
                                     from(select AssignedTo as name, fld006314, DueDays  from TaskView5 where 1 = 1 ");
            if (!string.IsNullOrWhiteSpace(str))
            {
                strSql += string.Format(@"and Number in (" + str + ")");
            }
            strSql += string.Format(@") t group by name");
            //str is null
            DataTable dt = new DataTable();
            var obj = UnitWork.ExcuteSql<ScoringDetail>(ContextType.ManagerDbContext, strSql, CommandType.Text, null);
            var personalNames = obj.Select(u => u.name).ToList();
            StringBuilder strSqlbind = new StringBuilder();
            strSqlbind.AppendFormat("select * from manageaccountbind u  where LOCATE(u.MName , \"{0}\")  > 0  and u.DutyFlag = 1 and  Level is not null ", JsonConvert.SerializeObject(personalNames).Replace(@"""", ""));
            var erp4BindList = UnitWork.ExcuteSql<ManageAccountBind>(ContextType.DefaultContextType, strSqlbind.ToString(), CommandType.Text, null).ToList();
            var legitPersonalNameList = erp4BindList.Select(u => u.MName).ToList();
            obj = obj.Where(u => legitPersonalNameList.Contains(u.name)).ToList();
            if (!string.IsNullOrWhiteSpace(str))
            {
                dt = (from n in erp4BindList
                      join m in obj on n.MName equals m.name
                      select new
                      {
                          n.Level,
                          name = n.LName,
                          m.LowDifficulty,
                          m.MediumDifficulty,
                          m.HighDifficulty,
                          m.SuperDifficulty,
                          m.OnTime,
                          m.Delayed
                      }).ToDataTable();
            }
            list.Add(dt);

            return list;
        }

        public List<DataTable> DataViewOwner(string date)
        {
            List<DataTable> list = new List<DataTable>();
            string start = Convert.ToDateTime(date + "-01").ToString("yyyy-MM-dd");
            string end = Convert.ToDateTime(date + "-01").AddMonths(1).ToString("yyyy-MM-dd");

            //当前月份是否归档
            var adata = UnitWork.FindSingle<RateDetail>(a => a.Time == date);
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //任务件数指标
            DataTable dt = new DataTable();
            dt.Columns.Add("qualified", Type.GetType("System.Int32"));//合格
            dt.Columns.Add("excellent", Type.GetType("System.Int32"));//优秀
            dt.Columns.Add("archive", Type.GetType("System.Int32"));//是否存档
            dt.Rows.Add( dt.NewRow());
            dt.Rows[0]["archive"] = adata!=null?1:0;
            var ManageAccountBind = UnitWork.Find<ManageAccountBind>(q => q.LName == loginContext.User.Name && q.DutyFlag == 1 && q.Level != null).FirstOrDefault();
            if (ManageAccountBind != null)
            {
                if (ManageAccountBind.Level == "1")
                {
                    dt.Rows[0]["qualified"] = 20;
                    dt.Rows[0]["excellent"] = 30;
                }
                else if (ManageAccountBind.Level == "2")
                {
                    dt.Rows[0]["qualified"] = 15;
                    dt.Rows[0]["excellent"] = 25;
                }
                else
                {
                    dt.Rows[0]["qualified"] = 10;
                    dt.Rows[0]["excellent"] = 15;
                }
            }
            list.Add(dt);
            if (ManageAccountBind != null && !string.IsNullOrEmpty(ManageAccountBind.MName))
            {

                //指派的任务件数
                string strSql = string.Format(@"SELECT count(Number) AS num 
                            from  TaskView5
                            where  AssignTime >= '" + start + "' AND AssignTime < '" + end + "' and AssignedTo = '" + ManageAccountBind.MName + "' ");
                //var assingTable = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null);
                list.Add(UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null));
                //已完成的任务件数
                strSql = string.Format(@"SELECT count(Number) AS num 
                            from  TaskView5
                            where isFinished = 1 and  CompleteTime >= '" + start + "' AND CompleteTime < '" + end + "' and AssignedTo = '" + ManageAccountBind.MName + "'");
                //var finishedTable = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null);
                list.Add(UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null));
                //难度
                strSql = string.Format(@" select  name,
                            count(1) as value
                            from (select fld006314 name from TaskView5 where StartDate >= '" + start + "' AND StartDate < '" + end + "' and AssignedTo = '" + ManageAccountBind.MName + "') t group by name");
                list.Add(UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null));
                //是否延期
                strSql = string.Format(@"
                            select  name,
                            count(1) as value
                            from (select (case when DueDays>=0 then N'按时完成' else N'延期完成' end ) name from TaskView5 where CompleteTime >= '" + start + "' AND CompleteTime < '" + end + "' and AssignedTo = '" + ManageAccountBind.MName + "') t group by name");
                list.Add(UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null));
                ////进度
                DataTable dt1 = new DataTable();
                dt1.Columns.Add("unsubmit", Type.GetType("System.Int32"));//未提交
                dt1.Columns.Add("submit", Type.GetType("System.Int32"));//已提交
                dt1.Columns.Add("submitother", Type.GetType("System.Int32"));//提交到其他
                dt1.Rows.Add(dt1.NewRow());
                string d = date.Split('.')[0] + "年" + date.Split('.')[1] + "月";

                string strunsubmit = String.Join(",", UnitWork.Find<TaskView>(null).Select(q => q.Number).Select(x => $"'{x}'"));
                string strsubmitother = String.Join(",", UnitWork.Find<TaskView>(q => q.Month == d).Select(q => q.Number).Select(x => $"'{x}'"));
                strSql = string.Format(@"
                            select count(1) as value
                            from TaskView5 where CompleteTime >= '" + start + "' AND CompleteTime < '" + end + "' and duedate >= '" + start + "' AND duedate < '" + end + "' and AssignedTo = '" + ManageAccountBind.MName + "' and Number not in (" + strunsubmit + ") ");
                dt1.Rows[0]["unsubmit"] = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null).Rows.Count;
                strSql = string.Format(@"
                            select count(1) as value
                            from TaskView5 where CompleteTime >= '" + start + "' AND CompleteTime < '" + end + "' and duedate >= '" + start + "' AND duedate < '" + end + "' and AssignedTo = '" + ManageAccountBind.MName + "' and Number in (" + strsubmitother + ") ");
                dt1.Rows[0]["submit"] = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null).Rows.Count;

                strSql = string.Format(@"
                            select count(1) as value
                            from TaskView5 where CompleteTime >= '" + start + "' AND CompleteTime < '" + end + "' and duedate >= '" + start + "' AND duedate < '" + end + "' and AssignedTo = '" + ManageAccountBind.MName + "' and Number not in (" + strsubmitother + ") ");
                dt1.Rows[0]["submitother"] = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, strSql, CommandType.Text, null).Rows.Count;
                list.Add(dt1);
            }
         

            return list;
        }
        #endregion
    }
}
