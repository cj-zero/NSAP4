extern alias MySqlConnectorAlias;
using DotNetCore.CAP;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using OpenAuth.App.Interface;
using OpenAuth.App.Order.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.NsapBase;
using Microsoft.Data.SqlClient;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Extensions;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using OpenAuth.App.Order.ModelDto;
using OpenAuth.Repository.Domain.Sap;
using NSAP.Entity.Sales;
using Infrastructure.Helpers;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using static Infrastructure.Helpers.CommonHelper;
using NSAP.Entity.BillFlow;
using NSAP.Entity.Store;
using NSAP.Entity.Product;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;
using Infrastructure.Export;

namespace OpenAuth.App.Order
{
    /// <summary>
    /// 销售订单业务
    /// </summary>
    public partial class SalesDeliveryApp : OnlyUnitWorkBaeApp
    {
        private readonly ServiceSaleOrderApp _serviceSaleOrderApp;


        public SalesDeliveryApp(IUnitWork unitWork, IAuth auth, ServiceSaleOrderApp serviceSaleOrderApp) : base(unitWork, auth)
        {
            this._serviceSaleOrderApp = serviceSaleOrderApp;

        }
        public async Task<string> SalesDeliverySave(SalesOrderSaveReq salesOrderSaveReq, int UserID, int FuncID, string jobdata,
            string jobname, int SboID, string IsTemplate)
        {
            string result = "", className = "";
            if (jobname == "销售交货") { className = "NSAP.B1Api.BOneODLN"; }
            billDelivery Model = _serviceSaleOrderApp.GetDeliverySalesInfoNewNos(salesOrderSaveReq.JobId.ToString(), UserID);
            Model.DocStatus = "O";
            #region 必须都有关联订单，并且购买数量与关联订单数量一致,采购订单所有物料高于2次的采购历史，并且价格不高于历史最低价，则不需审批直接通过。
            bool PurPassAudit = false;
            #endregion
            byte[] job_data = ByteExtension.ToSerialize(Model);
            #region 售后人员(部门名称“售后”开头）下的销售订单如果没有设备（物料编号C开头),则审批流程改成呼叫中心审批
            #endregion
            //审批流单据金额
            double jobDocTotal = (double.Parse(Model.DocTotal) > 0 ? double.Parse(Model.DocTotal) : 0);
            if (!string.IsNullOrEmpty(salesOrderSaveReq.Ations.ToString()))
            {
                if (salesOrderSaveReq.Ations == OrderAtion.Draft)
                {
                    result = _serviceSaleOrderApp.WorkflowBuild(jobname, FuncID, UserID, job_data, Model.Remark, int.Parse(Model.SboId), Model.CardCode, Model.CardName, jobDocTotal, int.Parse(Model.billBaseType), int.Parse(Model.billBaseEntry), "BOneAPI", className);
                }
                if (salesOrderSaveReq.Ations == OrderAtion.Submit)
                {
                    result = _serviceSaleOrderApp.WorkflowBuild(jobname, FuncID, UserID, job_data, Model.Remark, int.Parse(Model.SboId), Model.CardCode, Model.CardName, jobDocTotal, int.Parse(Model.billBaseType), int.Parse(Model.billBaseEntry), "BOneAPI", className);
                    if (int.Parse(result) > 0)
                    {
                        int user_id = 0;

                        var par = _serviceSaleOrderApp.SaveJobPara(result, IsTemplate);
                        if (par == "1")
                        {
                            string _jobID = result;
                            if ("0" != _serviceSaleOrderApp.WorkflowSubmit(int.Parse(result), UserID, Model.Remark, "", user_id))
                            {
                                if (!(jobname == "采购订单" && Model.DocType == "S"))
                                {
                                    result = SaveProOrder(Model, int.Parse(_jobID)).ToString();
                                }
                                if (Model.serialNumber.Count > 0)
                                {
                                    if (UpdateSerialNumber(Model.serialNumber, int.Parse(_jobID))) { result = "1"; }
                                }
                            }
                            else { result = "0"; }
                        }
                        else { result = "0"; }
                    }

                }
                if (salesOrderSaveReq.Ations == OrderAtion.Resubmit)
                {
                    if (jobdata == "1")
                    {
                        if (bool.Parse(_serviceSaleOrderApp. UpdateAudit(salesOrderSaveReq.JobId, job_data, Model.Remark, Model.DocTotal, Model.CardCode, Model.CardName)))
                        {
                            result = _serviceSaleOrderApp.WorkflowSubmit(salesOrderSaveReq.JobId, UserID, Model.Remark, "", 0);
                        }
                    }
                    else
                    {
                        result = _serviceSaleOrderApp.WorkflowSubmit(salesOrderSaveReq.JobId, UserID, Model.Remark, "", 0);
                    }
                }
            }
            return result;
        }
        public int SaveProOrder(billDelivery Model, int jobid)
        {
            string SaleOrder = "0";
            string sfileCpbm = "";
            string sNum = "";
            string sTp = "";
            int bresult = 0;
            foreach (var item in Model.billSalesDetails)
            {
                sfileCpbm += item.ItemCode.Replace('★', '"') + ",";
                sNum += item.Quantity + ",";
                if (item.U_TDS == "0" && item.U_DL == "0")
                {
                    sTp += "0" + ",";
                }
                else
                {
                    sTp += "2" + ",";
                }
            }

            if (sfileCpbm != "" && sNum != "")
            {
                sfileCpbm = sfileCpbm.TrimEnd(',');
                sNum = sNum.TrimEnd(',');
                sTp = sTp.TrimEnd(',');
                bresult = WorkBillJob(Model.SboId, jobid, "0", sfileCpbm, SaleOrder, sNum, Model.WhsCode, sTp);
            }
            else
            {
                bresult = 1;
            }
            return bresult;
        }
        public int WorkBillJob(string sbo_id, int jobID, string proNum, string sfileCpbm, string jobidNew, string sNum, string WhsCode, string sTp)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat(" DELETE from {0}.store_drawing_job where  job_idMe ={1} and Typeid =3 AND SalesId={2} ;", "nsap_bone", jobID.ToString(), jobidNew);
            sql.AppendFormat(" INSERT INTO {0}.store_drawing_job(sbo_id, job_idMe, productNum, itemcode, SalesId , projhsl,WhsCode, Typeid, TypeTP, upd_date ) VALUES ", "nsap_bone");
            string[] itemcode = sfileCpbm.Split(',');
            string[] projhsl = sNum.Split(',');
            string[] _sTp = sTp.Split(',');
            for (int i = 0; i < itemcode.Length; i++)
            {
                sql.AppendFormat("( {0}, {1}, {2}, '{3}','{4}', {5} ,'{6}',{7} ,{8},  CURRENT_TIMESTAMP() ) ,",
                    sbo_id, jobID.ToString(), proNum, itemcode[i].Replace("\"", "\\\"").Replace("\'", "\\\'"), jobidNew, projhsl[i], WhsCode, "3", _sTp[i]);
            }
            sql.Remove(sql.Length - 1, 1);
            sql.AppendFormat(" ;");
            int rows = 0;
            try
            {
                rows = UnitWork.ExecuteSql(sql.ToString(), ContextType.NsapBaseDbContext);
            }
            catch
            {
                ;
            }

            return rows > 0 ? 1 : 0;
        }
        //修改已选择序列号状态
        public bool UpdateSerialNumber(IList<billSerialNumber> osrnlist, int submitjobid)
        {
            string strSql = string.Empty;
            int res = 0;
            foreach (billSerialNumber osrn in osrnlist)
            {
                foreach (billSerialNumberChooseItem serial in osrn.Details)
                {
                    strSql = string.Format("INSERT INTO {0}.store_osrn_alreadyexists (ItemCode,SysNumber,DistNumber,MnfSerial,IsChange,JobId) VALUES (?ItemCode,?SysNumber,?DistNumber,?MnfSerial,?IsChange,?JobId) ON Duplicate KEY UPDATE DistNumber=VALUES(DistNumber),MnfSerial=VALUES(MnfSerial),IsChange=VALUES(IsChange),JobId=VALUES(JobId)", "nsap_bone");

                    List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter> sqlParameters = new List<MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter>()
            {
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?ItemCode",  osrn.ItemCode),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?SysNumber",  serial.SysSerial),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?DistNumber",  serial.IntrSerial),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?MnfSerial",  serial.SuppSerial),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?IsChange","1"),
                new MySqlConnectorAlias::MySql.Data.MySqlClient.MySqlParameter("?JobId",submitjobid)
            };
                    //strSql = string.Format("UPDATE {0}.store_osrn_alreadyexists SET IsChange=1 WHERE ItemCode = '{1}' AND SysNumber ='{2}'", Sql.BOneDatabaseName, osrn.ItemCode.Replace("'", "''"), serial.SysSerial);
                    res = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, strSql, CommandType.Text, sqlParameters).Rows.Count;
                }
            }
            return res > 0 ? true : false;
        }
    }
}
