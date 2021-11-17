using DotNetCore.CAP;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using SAPbobsCOM;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenAuth.Repository.Domain.Sap;
using System.Reactive;
using Sap.Handler.Service.Request;
using NSAP.Entity.Sales;
using System.Data;
using Infrastructure.Extensions;

namespace Sap.Handler.Service
{
    /// <summary>
    /// 销售报价单同步SAP
    /// </summary>
    public class ServiceSaleOrderHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;

        private string docNum = "";
        private string jobiD = "0";
        private string sboname = "";
        private string sqlconn = "";
        private billDelivery model = null;
        public ServiceSaleOrderHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }
        [CapSubscribe("Serve.ServiceSaleOrder.Create")]
        public async Task HandleServiceOrder(int jobID)
        {

            //
            model = new billDelivery();
            bool Result = false;
            string errorMsg = string.Empty;
            int eCode = 0;
            string eMesg = string.Empty;
            StringBuilder allerror = new StringBuilder();
            int res = 0;
            if (company != null)
            {
                try
                {
                    SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oQuotations);
                    #region [添加主表信息]
                    string sqlStr = string.Format("SELECT sql_db,sql_conn FROM {0}.sbo_info WHERE sbo_id={1}", "nsap_base", 1);
                    DataTable dtRowsConn = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, sqlStr, CommandType.Text, null);
                    if (dtRowsConn.Rows.Count > 0)
                    {
                        sboname = dtRowsConn.Rows[0][0].ToString();
                        sqlconn = dtRowsConn.Rows[0][1].ToString();
                    }
                    dts.CardCode = model.CardCode;
                    if (!string.IsNullOrEmpty(model.CntctCode))
                    {
                        dts.ContactPersonCode = int.Parse(model.CntctCode);
                    }
                    dts.SalesPersonCode = int.Parse(model.SlpCode == "" ? "-1" : model.SlpCode);

                    dts.Comments = model.Comments;
                    dts.NumAtCard = model.NumAtCard;
                    dts.PayToCode = model.PayToCode;
                    dts.ShipToCode = model.ShipToCode;
                    dts.DocCurrency = model.DocCur;
                    dts.DocDate = DateTime.Parse(model.DocDate);
                    dts.DocDueDate = DateTime.Parse(model.DocDueDate);
                    if (!string.IsNullOrEmpty(model.TrnspCode))
                    {
                        dts.TransportationCode = Convert.ToInt32(model.TrnspCode);
                    }
                    if (!string.IsNullOrEmpty(model.OwnerCode))
                    {
                        dts.DocumentsOwner = Convert.ToInt32(model.OwnerCode);
                    }
                    if (model.PartSupply == "true")
                    {
                        dts.PartialSupply = BoYesNoEnum.tYES;
                    }
                    else
                    {
                        dts.PartialSupply = BoYesNoEnum.tNO;
                    }
                    if (!string.IsNullOrEmpty(model.U_FPLB) && model.U_FPLB != "")
                    {
                        dts.UserFields.Fields.Item("U_FPLB").Value = model.U_FPLB;
                    }
                    if (!string.IsNullOrEmpty(model.U_SL) && model.U_SL != "")
                    {
                        dts.UserFields.Fields.Item("U_SL").Value = model.U_SL;
                    }
                    if (!string.IsNullOrEmpty(model.U_YWY) && model.U_YWY != "")
                    {
                        dts.UserFields.Fields.Item("U_YWY").Value = model.U_YWY;
                    }
                    if (!string.IsNullOrEmpty(model.DocType))
                    {
                        if (model.DocType == "I")
                            dts.DocType = BoDocumentTypes.dDocument_Items;
                        if (model.DocType == "S")
                            dts.DocType = BoDocumentTypes.dDocument_Service;
                    }
                    else { dts.DocType = BoDocumentTypes.dDocument_Items; }

                    dts.Address2 = model.Address2;      //收货方
                    dts.Address = model.Address;        //收款方
                    if (!string.IsNullOrEmpty(model.CustomFields) && model.CustomFields != "{}")
                    {
                        string[] filesName = model.CustomFields.Replace("≮0≯", "∏").Split('∏');
                        string[] filesValue = "".Split(',');
                        for (int i = 0; i < filesName.Length; i++)
                        {
                            filesValue = filesName[i].Replace(":", "：").Replace("≮1≯", ":").Split(':');
                        }
                    }
                    if (!string.IsNullOrEmpty(model.GroupNum))
                    {
                        dts.PaymentGroupCode = int.Parse(model.GroupNum);   //付款条款
                    }
                    dts.Indicator = model.Indicator;    // 标识
                    dts.PaymentMethod = model.PeyMethod;    //付款方式
                    dts.FederalTaxID = model.LicTradNum;  //国税编号
                    dts.Project = "";
                    dts.DiscountPercent = double.Parse(!string.IsNullOrEmpty(model.DiscPrcnt) ? model.DiscPrcnt : "0.00");
                    dts.DocTotal = double.Parse(!string.IsNullOrEmpty(model.DocTotal) ? model.DocTotal : "0.00");
                    //拟取消订单号
                    if (!string.IsNullOrEmpty(model.U_New_ORDRID) && model.U_New_ORDRID != "")
                    {
                        dts.UserFields.Fields.Item("U_New_ORDRID").Value = model.U_New_ORDRID;
                    }
                    errorMsg += string.Format("调用接口添加销售报价单主数据[{0}]-->", jobID);
                    //关联商城订单号
                    if (!string.IsNullOrEmpty(model.U_EshopNo))
                    {
                        dts.UserFields.Fields.Item("U_EshopNo").Value = model.U_EshopNo;
                    }
                    errorMsg += string.Format("调用接口添加销售订单主数据[{0}]", jobID);
                    #endregion

                    #region [添加行明细]
                    if (model.DocType == "I")
                    {
                        foreach (billSalesDetails dln1 in model.billSalesDetails)
                        {
                            if (model.DocType == "I")
                            {
                                dts.Lines.ItemCode = dln1.ItemCode.Replace("&#92;", "■");
                                dts.Lines.ItemDescription = dln1.Dscription;
                                dts.Lines.Quantity = string.IsNullOrWhiteSpace(dln1.Quantity) ? '1' : double.Parse(dln1.Quantity);
                                dts.Lines.WarehouseCode = dln1.WhsCode == "" ? "01" : dln1.WhsCode;
                            }
                            dts.Lines.UnitPrice = double.Parse(string.IsNullOrWhiteSpace(dln1.PriceBefDi) ? "0" : dln1.PriceBefDi);            //单价
                            dts.Lines.Price = double.Parse(string.IsNullOrWhiteSpace(dln1.PriceBefDi) ? "0" : dln1.PriceBefDi);
                            dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(dln1.DiscPrcnt) ? 0.00 : double.Parse(dln1.DiscPrcnt);
                            dts.Lines.LineTotal = string.IsNullOrWhiteSpace(dln1.LineTotal) ? 0.00 : double.Parse(dln1.LineTotal);

                            if (!string.IsNullOrEmpty(dln1.U_PDXX) && (dln1.U_PDXX == "AC220" || dln1.U_PDXX == "AC380" || dln1.U_PDXX == "AC110"))
                            {
                                dts.Lines.UserFields.Fields.Item("U_PDXX").Value = dln1.U_PDXX;
                            }
                            dts.Lines.VatGroup = "X0";
                            dts.Lines.UserFields.Fields.Item("U_ZS").Value = dln1.U_ZS;
                            dts.Lines.UserFields.Fields.Item("U_RelDoc").Value = dln1.U_RelDoc.Trim();
                            dts.Lines.Add();
                        }
                    }
                    else
                    {
                        foreach (billSalesAcctCode oact in model.billSalesAcctCode)
                        {
                            dts.Lines.AccountCode = oact.AcctCode;
                            dts.Lines.ItemDescription = oact.Details;
                            dts.Lines.UnitPrice = double.Parse(string.IsNullOrWhiteSpace(oact.Price) ? "0" : oact.Price);            //单价
                            dts.Lines.DiscountPercent = double.Parse(string.IsNullOrWhiteSpace(oact.DiscPrcnt) ? "0" : oact.DiscPrcnt);     //折扣
                            dts.Lines.VatGroup = "X0";
                            dts.Lines.Add();
                        }
                    }
                    errorMsg += string.Format("调用接口添加销售报价单行明细[{0}]-->", jobID);
                    #endregion
                    res = dts.Add();
                    if (res != 0)
                    {
                        company.GetLastError(out eCode, out eMesg);
                        errorMsg += string.Format("添加销售报价单:({0})时调接口发生异常[异常代码:{1},异常信息:{2}]-->", jobID, eCode, eMesg);
                        Result = false;
                    }
                    else
                    {
                        company.GetNewObjectCode(out docNum);
                        errorMsg += string.Format("调用接口添加销售报价单操作成功,ID[{0},帐套ID{1}]-->", jobID, model.SboId);
                        //int resultCountJob = AidTool.GetSboReturn(docNum, jobID);
                        string sql = string.Format("UPDATE nsap_base.wfa_job SET sbo_itf_return={0} WHERE job_id={1}", docNum, jobID);
                        int resultCountJob = UnitWork.ExecuteSql(sql, ContextType.NsapBaseDbContext);
                        if (resultCountJob > 0) { errorMsg += string.Format("调用接口修改wfa_job表成功,jobID[{0}]-->", jobID); }
                        else { errorMsg += string.Format("销售报价单调用接口修改wfa_job表失败,jobID[{0}]-->", jobID); }
                        Result = true;
                    }
                }
                catch (Exception e)
                {
                    errorMsg += string.Format("添加销售报价单:({0})时调接口发生异常(eCode:{1})-->", jobID, e);
                    Result = false;
                }
            }
        }
    }
}
