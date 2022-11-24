using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Material;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Material.Response;
using OpenAuth.App.Order;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.App.Order.Request;
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
        private readonly IOptions<AppSetting> _appConfiguration;
        private readonly MaterialDesignApp _app;
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;
        ServiceBaseApp _serviceBaseApp;
        IUnitWork UnitWork;
        IAuth _auth;
        public MaterialDesignController(MaterialDesignApp app, ServiceBaseApp _serviceBaseApp, IUnitWork UnitWork, IAuth _auth, ServiceSaleOrderApp serviceSaleOrderApp, IOptions<AppSetting> appConfiguration)
        {
            _app = app;
            this._serviceBaseApp = _serviceBaseApp;
            this.UnitWork = UnitWork;
            this._auth = _auth;
            _serviceSaleOrderApp = serviceSaleOrderApp;
            _appConfiguration = appConfiguration;
        }

        #region 物料设计弹窗
        /// <summary>
        /// 查看物料明细
        /// </summary>
        [HttpPost]
        public TableData GetItemCodeList(SalesOrderListReq model)/*, string ations = "", string billPageurl = ""*/
        {
            string tablename = "sale_rdr1";
            TableData tableData = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            bool ViewSales = true;
            int SboId = _serviceBaseApp.GetUserNaspSboID(userId);

            StringBuilder stringBuilder = new StringBuilder();
            string strSql = string.Format(" SELECT  d.ItemCode,Dscription,d.Quantity ," +
                         "IF(" + ViewSales + ",Price,0)Price," +
                         "IF(" + ViewSales + ",LineTotal,0) LineTotal," +
                         "IF(" + ViewSales + ", StockPrice, 0) StockPrice,");
            strSql += string.Format("d.WhsCode,w.OnHand,");
            strSql += string.Format("d.U_ZS");
            strSql += ",(CASE WHEN d.ItemCode REGEXP 'A605|A608|A313|A302|BT-4/8|BTE-4/8|BE-4/8|BA-4/8|M202|M203|A405' THEN '--' ELSE d.ContractReviewCode END) U_RelDoc";
            strSql += string.Format(" FROM {0}." + tablename + " d", "nsap_bone");
            strSql += string.Format(" LEFT JOIN {0}.store_oitw w ON d.ItemCode=w.ItemCode AND d.WhsCode=w.WhsCode AND d.sbo_id=w.sbo_id", "nsap_bone");
            strSql += string.Format(" left join manage_screening m on d.DocEntry = m.DocEntry and d.ItemCode = m.ItemCode and d.U_ZS = m.U_ZS and d.Quantity = m.Quantity", "erp4_serve");
            strSql += string.Format(" WHERE m.DocEntry is null and d.DocEntry=" + model.DocEntry + " AND d.sbo_id={0}", SboId);
            strSql += string.Format(" and d.ItemCode REGEXP 'A605|A608|A313|A302|CT-4|CT-8|CE-4|CE-8|CE-7|CTE-4|CTE-8|CE-6|CA|CJE|CJ|CGE|CGE|BT-4/8|BTE-4/8|BE-4/8|BA-4/8|M202|M203|MGDW|MIGW|MGW|MGDW|MHW|MIHW|MCD|MFF|MFYHS|MWL|MJZJ|MFXJ|MXFC|MFHL|MET|MRBH|MRH|MRF|MFB|MFYB|MRZH|MFYZ|MYSFR|MFSF|MYSHC|MYHF|MRSH|MRHF|MXFS|MZZ|MDCIR|MJR|MJF|MTP|MJY|MJJ|MCH|MCJ|MZJ' ");
            tableData.Count = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, strSql.ToString(), CommandType.Text, null).Rows.Count;

            strSql += string.Format(" order by d.ItemCode limit {0} ,{1} ", (model.page - 1) * model.limit, model.limit);
            DataTable dts = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, strSql.ToString(), CommandType.Text, null);

            tableData.Data = dts.Tolist<OrderItemInfo>();
            return tableData;
        }

        /// <summary>
        /// 获取物料关联合约评审单
        /// </summary>
        [HttpGet]
        public TableData GetContractReviewList(string DocNum, string CardCode)/*, string ations = "", string billPageurl = ""*/
        {
            TableData tableData = new TableData();
            var userId = _serviceBaseApp.GetUserNaspId();
            int SboId = _serviceBaseApp.GetUserNaspSboID(userId);

            StringBuilder stringBuilder = new StringBuilder();
            string strSql = string.Format("select a.contract_id contract_Id,a.CardCode,  a.CardName, a.apply_dt Apply_dt,a.upd_dt ");
            strSql += string.Format(" FROM nsap_bone.sale_contract_review a");
            strSql += string.Format(" WHERE a.sbo_id = {0} AND a.ItemCode='" + DocNum.Replace("'", "\\'") + "' AND a.CardCode = '" + CardCode + "' AND a.apply_dt > DATE_SUB(CURDATE(), INTERVAL 6 MONTH)", SboId);
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
        public async Task<Infrastructure.Response> SubmitItemCodeList(int DocEntry, List<SubmitItemCode> ItemCodeList, int SboId = 1)
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
        /// 查看提交历史记录
        /// </summary>
        [HttpGet]
        public TableData GetHistList(string DocEntry)
        {
            var result = new TableData();
            var list = UnitWork.Find<ManageScreeningHistory>(q => q.DocEntry == DocEntry).OrderByDescending(q => q.SubmitTime).ToList();
            result.Data = list;
            result.Count = list.Count;
            return result;
        }

        /// <summary>
        /// 管理图纸文件路径
        /// </summary>
        /// <param name="AddDrawingFiles"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddDrawingFiles")]
        public async Task<Infrastructure.Response> AddDrawingFiles(UpdateManageScreen manageScreen)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _app.AddDrawingFiles(manageScreen);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// 图纸编码同步
        /// </summary>
        /// <param name="AddDrawingFiles"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("ItemCodeSync")]
        public async Task<Infrastructure.Response> ItemCodeSync(List<int> ids)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _app.ItemCodeSync(ids);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// 判断订单是否有合同号
        /// </summary>
        [HttpGet]
        public TableData haveContact(string DocNum)/*, string ations = "", string billPageurl = ""*/
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
            result.Data = flag;
            return result;
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
        public AdvanceData GetAdvanceDetail(string docentry, string itemcode)
        {
            AdvanceData advanceData = new AdvanceData();

            List<DataTable> list = new List<DataTable>();
            string sql = "  select * from ( select RecordGuid,CreatedDate, fld005508 DocEntry, max(_System_Progress) progress,fld005506 itemCode,_System_objNBS ProjectNo from OBJ162 group by RecordGuid, fld005508,_System_objNBS,fld005506,CreatedDate) a ";
            sql += "where a.DocEntry = 'SE-" + docentry + "' and itemCode = '" + itemcode.Replace("'","''") + "'";
            DataTable dts = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, sql.ToString(), CommandType.Text, null);
            list.Add(dts);
            if (dts != null && dts.Rows.Count > 0)
            {
                advanceData.ProjectNo = dts.Rows[0]["ProjectNo"].ToString();
                advanceData.progress = dts.Rows[0]["progress"].ToString();
                advanceData.CreatedDate = dts.Rows[0]["CreatedDate"].ToString();

                string guid = dts.Rows[0]["RecordGuid"].ToString();
                string sql1 = "SELECt t.isFinished,T.Subject, T.Complete, t.CaseRecGuid  FROM Tasks  as t where t.isDeleted = 0  and t.CaseRecGuid = '" + guid + "' order by t.StageId ";
                DataTable dts1 = UnitWork.ExcuteSqlTable(ContextType.ManagerDbContext, sql1.ToString(), CommandType.Text, null);
                advanceData.dt = dts1;
            }
            string sql2 = "select * from manage_screening where DocEntry = '" + docentry + "' and ItemCode = '" + itemcode.Replace("'", "\\'") + "' ";
            DataTable dts2 = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql2.ToString(), CommandType.Text, null);
            if (dts2 != null && dts2.Rows.Count > 0)
            {
                advanceData.SubmitTime = dts2.Rows[0]["SubmitTime"].ToString();
            }

            return advanceData;

        }
        /// <summary>
        /// 查看合约评审单
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="VersionNo"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetContractFile")]
        public TableData GetContractFile(string ContractId)
        {
            TableData result = new TableData();
            HttpHelper httpHelper = new HttpHelper(_appConfiguration.Value.ERP3Url);
            var resultApi = httpHelper.Get<Dictionary<string, string>>(new Dictionary<string, string> { { "ContractId", ContractId } }, "/spv/exportcontractreview.ashx");
            if (resultApi["msg"] == "success")
            {
                var url = resultApi["url"].Replace("192.168.0.208", "218.17.149.195");
                result.Data = url;
            }
            else
            {
                result.Code = 500;
                result.Message = resultApi["msg"];
            }
            return result;
        }

        public class AdvanceData
        {
            public string ProjectNo { get; set; }
            public string SubmitTime { get; set; }
            public string progress { get; set; }
            public DataTable dt { get; set; }
            public string CreatedDate { get; set; }
        }
        #endregion

        /// <summary>
        /// 物料过账记录
        /// </summary>
        [HttpPost]
        public async Task<TableData> GetMaterialRecord(MaterialRecordReq req)
        {

            var result = new TableData();
            try
            {
                return await _app.GetMaterialRecord(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }


        #region 工程部考勤
        /// <summary>
        /// 统计分析页面
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        [HttpPost]
        public TableData TaskView(TaskViewReq request)
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
                TableData dts = _app.TaskView(request, SboID);
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
        /// 提交至月度统计
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubmitMonth")]
        public async Task<Infrastructure.Response> SubmitMonth(submitMonth req)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _app.SubmitMonth(req);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }

        /// <summary>
        /// 统计分析页面
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        [HttpGet]
        public List<DataTable> DataView(string date)
        {
            return _app.DataView(date);

        }

        /// <summary>
        /// 统计分析页面（个人）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        [HttpGet]
        public List<DataTable> DataViewOwner(string date, string name)
        {
            return _app.DataViewOwner(date, name);

        }
        #endregion

        #region WMS接口对接
        /// <summary>
        /// WMS接口对接
        /// </summary>
        /// <param name="ProductNo"></param>
        /// <param name="ItemName"></param>
        /// <returns></returns>
        [HttpGet]
        public DataTable PostWMSFileUrl(string ProductNo, string ItemName)
        {
            //参数：生产订单号，图纸编码
            //返回：版本号、图纸文件路径、样机 / 批量
            string sql = string.Format(@" select n.VersionNo,n.FileUrl, n.IsDemo
                                         from erp4_serve.manage_screening n
                                         left join nsap_bone.product_owor s on n.DocEntry = s.OriginNum and n.ItemCode = s.ItemCode where 1 = 1");
            if (!string.IsNullOrWhiteSpace(ProductNo))
            {
                sql += " and s.DocEntry = '" + ProductNo + "'";
            }
            if (!string.IsNullOrWhiteSpace(ItemName))
            {
                sql += " and n.ItemName = '" + ItemName + "'";
            }
            return UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql, CommandType.Text, null);
        }
        #endregion

        /// <summary>
        /// 测试获取文件
        /// </summary>
        /// <param name="dir"></param>
        /// <returns></returns>
        [HttpGet]
        public List<System.IO.FileInfo> getFile(System.IO.DirectoryInfo dir)
        {
            List<System.IO.FileInfo> fileList = new List<System.IO.FileInfo>();
            System.IO.FileInfo[] allfile = dir.GetFiles();
            foreach (System.IO.FileInfo file in allfile)
            {
                fileList.Add(file);
            }
            System.IO.DirectoryInfo[] allDir = dir.GetDirectories();
            foreach (System.IO.DirectoryInfo d in allDir)
            {
                getFile(d);
            }
            return fileList;
        }
    }
}
