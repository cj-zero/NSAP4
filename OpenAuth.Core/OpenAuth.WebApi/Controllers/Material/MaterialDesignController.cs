using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Material.Response;
using OpenAuth.App.Order;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Extensions;
using OpenAuth.Repository.Interface;
using Serilog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenAuth.App.Material.MaterialDesignApp;

namespace OpenAuth.WebApi.Controllers.Material
{
    /// <summary>
    /// 物料设计（对接工程部系统）
    /// </summary>
    [Route("api/Material/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Material")]
    public class MaterialDesignController : ControllerBase
    {
        private readonly MaterialDesignApp _app;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        ServiceBaseApp _serviceBaseApp;
        IUnitWork UnitWork;
        IAuth _auth;
        public MaterialDesignController(MaterialDesignApp app, ServiceBaseApp _serviceBaseApp, IUnitWork UnitWork, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp)
        {
            _app = app;
            this._serviceBaseApp = _serviceBaseApp;
            this.UnitWork = UnitWork;
            this._auth = _auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
        }

        #region 物料设计弹窗
        /// <summary>
        /// 查看物料明细
        /// </summary>
        [HttpGet]
        public TableData GetItemCodeList(string DocNum, string tablename = "sale_rdr1")/*, string ations = "", string billPageurl = ""*/
        {
            TableData tableData = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            bool ViewSales = true;
            int SboId = _serviceBaseApp.GetUserNaspSboID(userId);
            string U_YFTCBL = "";
            string strSqlViewSales = string.Format("SELECT COUNT(*) FROM information_schema.columns WHERE table_schema='nsap_bone' AND table_name ='{0}' AND column_name='{1}'", tablename, "U_YFTCBL");
            string IsU_YFTCBL = UnitWork.ExecuteScalar(ContextType.NsapBaseDbContext, strSqlViewSales, CommandType.Text, null).ToString();
            if (!string.IsNullOrEmpty(IsU_YFTCBL) && IsU_YFTCBL != "0")
            {
                U_YFTCBL = ",IF(" + ViewSales + ",d.U_YFTCBL,0)";
            }
            StringBuilder stringBuilder = new StringBuilder();
            string strSql = string.Format(" SELECT  d.ItemCode,Dscription,Quantity ," +
                "IF(" + ViewSales + ",d.PriceBefDi,0)PriceBefDi," +

                "IF(" + ViewSales + ",DiscPrcnt,0)DiscPrcnt,d.U_PDXX," +
                "IF(" + ViewSales + ",d.U_XSTCBL,0)U_XSTCBL," +
                "IF(" + ViewSales + ",d.U_YWF,0)U_YWF," +
                "IF(" + ViewSales + ",d.U_FWF,0)U_FWF," +
                "IF(" + ViewSales + ",Price,0)Price,VatGroup," +
                "IF(" + ViewSales + ",PriceAfVAT,0)PriceAfVAT,");
            strSql += string.Format("IF(" + ViewSales + ",LineTotal,0) LineTotal," +
                "IF(" + ViewSales + ",TotalFrgn,0) TotalFrgn," +
                "IF(" + ViewSales + ",d.U_SCTCBL,0) U_SCTCBL," +
                "IF(" + ViewSales + ",StockPrice,0) StockPrice,d.U_YF,d.WhsCode,w.OnHand,d.VatPrcnt,d.LineNum,d.U_YFTCBL,");
            strSql += string.Format("m.IsCommited,m.OnOrder,m.U_TDS,m.U_DL,m.U_DY,d.DocEntry,d.OpenQty,m.U_JGF,");
            strSql += string.Format("(CASE m.QryGroup1 WHEN 'N' THEN 0 ELSE '0.5' END) AS QryGroup1,");
            strSql += string.Format("(CASE m.QryGroup2 WHEN 'N' THEN 0 ELSE '3' END) AS QryGroup2,");
            strSql += string.Format("(CASE m.QryGroup3 WHEN 'N' THEN 0 ELSE '2' END) AS QryGroup3");
            strSql += string.Format(",m.U_JGF1,IFNULL(m.U_YFCB,0)U_YFCB," +
                "IFNULL(d.U_SHJSDJ,0)U_SHJSDJ," +
                "IFNULL(d.U_SHJSJ,0) U_SHJSJ ," +
                "IFNULL(d.U_SHTC,0) U_SHTC," +
                "IFNULL(SumQuantity,0) as SumQuantity");
            strSql += string.Format(",(CASE m.QryGroup8 WHEN 'N' THEN 0 ELSE '1' END) AS QryGroup8");//3008n
            strSql += string.Format(",(CASE m.QryGroup9 WHEN 'N' THEN 0 ELSE '2' END) AS QryGroup9");//9系列
            strSql += string.Format(",(CASE m.QryGroup10 WHEN 'N' THEN 0 ELSE '1.5' END) AS QryGroup10");//ES系列 
            strSql += string.Format(",m.buyunitmsr,d.U_ZS");

            strSql += ",d.LineStatus,d.BaseEntry,d.BaseLine,d.BaseType,(CASE WHEN d.ItemCode REGEXP 'A605|A608|A313|A302|BT-4/8|BTE-4/8|BE-4/8|BA-4/8|M202|M203' THEN '--' ELSE d.ContractReviewCode END) U_RelDoc";
            strSql += string.Format(" FROM {0}." + tablename + " d", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_oitw w ON d.ItemCode=w.ItemCode AND d.WhsCode=w.WhsCode AND d.sbo_id=w.sbo_id", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_oitm m ON d.ItemCode=m.ItemCode AND d.sbo_id=m.sbo_id", "nsap_bone");
            strSql += string.Format(" LEFT JOIN (select d1.sbo_id,d1.BaseEntry ,d1.BaseLine,SUM(d1.Quantity) as SumQuantity from {0}.sale_DLN1 d1 inner join {0}.sale_odln d0 on d0.docentry=d1.docentry and d0.sbo_id=d1.sbo_id where d0.Canceled='N' AND d1.BaseType=17 and d1.BaseEntry=" + DocNum + " GROUP BY d1.sbo_id,d1.BaseEntry,d1.BaseLine) as T on d.sbo_id=T.sbo_id and d.DocEntry=T.BaseEntry and  d.LineNum=T.BaseLine  ", "nsap_bone");
            strSql += string.Format(" WHERE d.DocEntry=" + DocNum + " AND d.sbo_id={0}", SboId);
            strSql += string.Format(" and d.ItemCode REGEXP 'A605|A608|A313|A302|CT-4|CT-8|CE-4|CE-8|CE-7|CTE-4|CTE-8|CE-6|CA|CJE|CJ|CGE|CGE|BT-4/8|BTE-4/8|BE-4/8|BA-4/8|M202|M203|MGDW|MIGW|MGW|MGDW|MHW|MIHW|MCD|MFF|MFYHS|MWL|MJZJ|MFXJ|MXFC|MFHL|MET|MRBH|MRH|MRF|MFB|MFYB|MRZH|MFYZ|MYSFR|MFSF|MYSHC|MYHF|MRSH|MRHF|MXFS|MZZ|MDCIR|MJR|MJF|MTP|MJY|MJJ|MCH|MCJ|MZJ'");

            DataTable dts = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            //int itemindex = 0;
            if (tablename.ToLower() == "sale_rdr1")
            {
                foreach (DataRow tempr in dts.Rows)
                {
                    string statusSql = string.Format("select top 1 LineStatus from RDR1 where DocEntry={0} and LineNum={1}", DocNum, tempr["LineNum"].ToString());
                    object statusobj = UnitWork.ExecuteScalar(ContextType.SapDbContextType, statusSql.ToString(), CommandType.Text, null);
                    tempr["LineStatus"] = statusobj == null ? "" : statusobj.ToString();
                }
            }
            tableData.Data = dts.Tolist<OrderItemInfo>();
            return tableData;
        }

        /// <summary>
        /// 获取物料关联合约评审单
        /// </summary>
        [HttpGet]
        public TableData GetContractReviewList(string DocNum)/*, string ations = "", string billPageurl = ""*/
        {
            TableData tableData = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            int SboId = _serviceBaseApp.GetUserNaspSboID(userId);

            StringBuilder stringBuilder = new StringBuilder();
            string strSql = string.Format("select a.contract_id contract_Id,a.CardCode,  a.CardName, a.apply_dt Apply_dt,a.upd_dt ");
            strSql += string.Format(" FROM nsap_bone.sale_contract_review a");
            strSql += string.Format(" WHERE a.sbo_id = {0} AND a.ItemCode='" + DocNum + "' AND a.apply_dt > DATE_SUB(CURDATE(), INTERVAL 6 MONTH)", SboId);
            DataTable dts = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql.ToString(), CommandType.Text, null);
            tableData.Data = dts.Tolist<ReturnContractReview>();
            return tableData;
        }


        /// <summary>
        /// 将评审单号回写到物料表
        /// </summary>
        [HttpGet]
        public async Task<Infrastructure.Response> BindReviewCode(int DocEntry, string ItemCode, string ReviewCode, int SboId = 1)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _app.BindReviewCode(SboId, DocEntry, ItemCode, ReviewCode);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }
            return response;
        }

        /// <summary>
        /// 提交物料设计到筛选列表
        /// </summary>
        [HttpPost]
        public async Task<Infrastructure.Response> SubmitItemCodeList(int DocEntry, List<string> ItemCodeList, int SboId = 1)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _app.SubmitItemCodeList(SboId, DocEntry, ItemCodeList);
            }
            catch (Exception ex)
            {
                response.Message = ex.InnerException?.Message ?? ex.Message ?? "";
                response.Code = 500;
            }
            return response;
        }
        #endregion
        #region 设计项目筛选
        /// <summary>
        /// 设计项目筛选（type=1 已筛选 type=0 未筛选）
        /// </summary>
        [HttpPost]
        public TableData GridDataBind(SalesOrderMaterialReq request)// int SboId, int? SalesOrderId, string ItemCode, string CardCode, int Type)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var UserID = _serviceBaseApp.GetUserNaspId();
            var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);
            var result = new TableData();
            try
            {
                TableData dts = _app.ForScreeningViewInfo(request, SboID);
                result.Data = dts;
                result.Count = dts.Count;// rowCount;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message ?? "";
            }

            return result;
        }


        /// <summary>
        /// 管理图纸文件路径
        /// </summary>
        /// <param name="AddDrawingFiles"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddDrawingFiles")]
        public async Task<Infrastructure.Response> AddDrawingFiles(List<int> ids,string url)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _app.AddDrawingFiles(ids, url);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }



        /// <summary>
        /// 提交物料设计到manager系统
        /// </summary>
        //[HttpPost]
        //public Infrastructure.Response PostDataToManager(List<MaterialDes> list)// int SboId, int? SalesOrderId, string ItemCode, string CardCode, int Type)
        //{
        //    var response = new Infrastructure.Response();
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    var loginUser = loginContext.User;
        //    var UserID = _serviceBaseApp.GetUserNaspId();
        //    var SboID = _serviceBaseApp.GetUserNaspSboID(UserID);

        //    return _app.PostDataToManager(SboID, list);
        //}

        #endregion

        #region 数据回写显示
        /// <summary>
        /// 获取总进度
        /// </summary>
        /// <param name="docentry"></param>
        /// <param name="itemcode"></param>
        /// <returns></returns>
        [HttpGet]
        public List<DataTable> GetAdvanceDetail(string docentry, string itemcode)
        {
            List<DataTable> list = new List<DataTable>();
            string sql = "  select * from ( select RecordGuid, fld005508 DocEntry, max(_System_Progress) progress,fld005506 itemCode from OBJ162 group by RecordGuid, fld005508,_System_objNBS,fld005506) a ";
            sql += "where a.DocEntry = 'SE-" + docentry + "' and itemCode = '" + itemcode + "'";
            DataTable dts = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, sql.ToString(), CommandType.Text, null);
            list.Add(dts);
            if (dts != null && dts.Rows.Count > 0)
            {
                string guid = dts.Rows[0]["RecordGuid"].ToString();
                string sql1 = "SELECt t.isFinished,T.Subject, T.Complete, t.CaseRecGuid  FROM Tasks  as t where t.isDeleted = 0  and t.CaseRecGuid = '" + guid + "' ";
                DataTable dts1 = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, sql1.ToString(), CommandType.Text, null);
                list.Add(dts1);
            }
            return list;

        }
        #endregion
    }
}
