using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sap.Handler.Service
{
    public class SellOrderSapHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;

        public SellOrderSapHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }

        [CapSubscribe("Serve.SellOrder.Create")]
        public async Task HandleSellOrder(int theQuotationId)
        {
            var query = await UnitWork.Find<Quotation>(q => q.Id.Equals(theQuotationId)).AsNoTracking()
               .Include(q=>q.QuotationProducts).ThenInclude(q=>q.QuotationMaterials).Include(q=>q.QuotationMergeMaterials).FirstOrDefaultAsync();

            SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

            //#region [添加主表信息]

            //DataTable dtRowsConn = AidTool.GetConnection(model.SboId);

            //if (dtRowsConn.Rows.Count > 0)
            //{

            //    sboname = dtRowsConn.Rows[0][0].ToString();

            //    sqlconn = dtRowsConn.Rows[0][1].ToString();

            //}

            //dts.CardCode = model.CardCode;

            //dts.Comments = model.Comments;

            //dts.ContactPersonCode = int.Parse(model.CntctCode == "" ? "0" : model.CntctCode);

            //dts.SalesPersonCode = int.Parse(model.SlpCode == "" ? "-1" : model.SlpCode);

            //dts.NumAtCard = model.NumAtCard;

            //dts.PayToCode = model.PayToCode;

            //dts.ShipToCode = model.ShipToCode;

            //dts.DocCurrency = model.DocCur;

            //dts.DocDate = DateTime.Parse(model.DocDate);

            //dts.DocDueDate = DateTime.Parse(model.DocDueDate);

            //if (!string.IsNullOrEmpty(model.TrnspCode))

            //{

            //    dts.TransportationCode = Convert.ToInt32(model.TrnspCode);

            //}

            //if (!string.IsNullOrEmpty(model.OwnerCode))

            //{

            //    dts.DocumentsOwner = Convert.ToInt32(model.OwnerCode);

            //}

            //if (model.PartSupply == "true" || model.PartSupply == "Y")

            //{

            //    dts.PartialSupply = BoYesNoEnum.tYES;

            //}

            //else

            //{

            //    dts.PartialSupply = BoYesNoEnum.tNO;

            //}

            //if (!string.IsNullOrEmpty(model.U_FPLB) && model.U_FPLB != "")

            //{

            //    dts.UserFields.Fields.Item("U_FPLB").Value = model.U_FPLB;

            //}

            //if (!string.IsNullOrEmpty(model.U_SL) && model.U_SL != "")

            //{

            //    dts.UserFields.Fields.Item("U_SL").Value = model.U_SL;

            //}

            //if (!string.IsNullOrEmpty(model.U_YWY) && model.U_YWY != "")

            //{

            //    dts.UserFields.Fields.Item("U_YWY").Value = model.U_YWY;

            //}

            //if (!string.IsNullOrEmpty(model.DocType))

            //{

            //    if (model.DocType == "I")

            //        dts.DocType = BoDocumentTypes.dDocument_Items;

            //    if (model.DocType == "S")

            //        dts.DocType = BoDocumentTypes.dDocument_Service;

            //}

            //else { dts.DocType = BoDocumentTypes.dDocument_Items; }



            //dts.Address2 = model.Address2;      //收货方

            //dts.Address = model.Address;        //收款方

            //if (!string.IsNullOrEmpty(model.CustomFields) && model.CustomFields != "{}")

            //{

            //    string[] filesName = model.CustomFields.Replace(",", "，").Replace("≮0≯", ",").Split(',');

            //    string[] filesValue = "".Split(',');

            //    for (int i = 0; i < filesName.Length; i++)

            //    {

            //        filesValue = filesName[i].Replace(":", "：").Replace("≮1≯", ":").Split(':');

            //        if (AidTool.IsExistCustomFields("ORDR", filesValue[0], sqlconn) && !string.IsNullOrEmpty(filesValue[1]))

            //        {

            //            dts.UserFields.Fields.Item(filesValue[0]).Value = filesValue[1];

            //        }

            //    }

            //}

            //if (!string.IsNullOrEmpty(model.GroupNum))

            //{

            //    dts.PaymentGroupCode = int.Parse(model.GroupNum);   //付款条款

            //}

            //dts.Indicator = model.Indicator;    // 标识

            //dts.PaymentMethod = model.PeyMethod;    //付款方式

            //dts.FederalTaxID = model.LicTradNum;  //国税编号

            //dts.Project = "";

            //dts.DiscountPercent = double.Parse(!string.IsNullOrEmpty(model.DiscPrcnt) ? model.DiscPrcnt : "0.00");

            //dts.DocTotal = double.Parse(!string.IsNullOrEmpty(model.DocTotal) ? model.DocTotal : "0.00");

            ////拟取消订单号

            //if (!string.IsNullOrEmpty(model.U_New_ORDRID) && model.U_New_ORDRID != "")

            //{

            //    dts.UserFields.Fields.Item("U_New_ORDRID").Value = model.U_New_ORDRID;

            //}

            ////关联商城订单号

            //if (!string.IsNullOrEmpty(model.U_EshopNo))

            //{

            //    dts.UserFields.Fields.Item("U_EshopNo").Value = model.U_EshopNo;

            //}

            //errorMsg += string.Format("调用接口添加销售订单主数据[{0}]", jobID);

            //#endregion

            //#region [添加行明细]

            //if (model.DocType == "I")

            //{

            //    foreach (billSalesDetails dln1 in model.billSalesDetails)

            //    {

            //        dts.Lines.ItemCode = dln1.ItemCode.Replace("&#92;", "■");

            //        dts.Lines.ItemDescription = dln1.Dscription;

            //        dts.Lines.Quantity = string.IsNullOrEmpty(dln1.Quantity) ? '1' : double.Parse(dln1.Quantity);

            //        dts.Lines.WarehouseCode = dln1.WhsCode == "" ? "01" : dln1.WhsCode;



            //        if (!string.IsNullOrEmpty(dln1.BaseLine))

            //        {

            //            dts.Lines.BaseEntry = int.Parse(dln1.BaseEntry == "" ? "-1" : dln1.BaseEntry);

            //            dts.Lines.BaseLine = int.Parse(dln1.BaseLine);

            //            dts.Lines.BaseType = 23;

            //        }

            //        dts.Lines.SalesPersonCode = int.Parse(model.SlpCode == "" ? "-1" : model.SlpCode);

            //        dts.Lines.UnitPrice = double.Parse(string.IsNullOrWhiteSpace(dln1.PriceBefDi) ? "0" : dln1.PriceBefDi);            //单价

            //        dts.Lines.Price = double.Parse(string.IsNullOrWhiteSpace(dln1.PriceBefDi) ? "0" : dln1.PriceBefDi);

            //        dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(dln1.DiscPrcnt) ? 0.00 : double.Parse(dln1.DiscPrcnt);

            //        dts.Lines.LineTotal = string.IsNullOrWhiteSpace(dln1.LineTotal) ? 0.00 : double.Parse(dln1.LineTotal);

            //        string DiscountPercent = "";

            //        if (dln1.DiscPrcnt == null)

            //        {

            //            DiscountPercent = "0";

            //        }

            //        else

            //        {

            //            if (dln1.DiscPrcnt == "")

            //            {

            //                DiscountPercent = "0";

            //            }

            //            else

            //            {

            //                DiscountPercent = dln1.DiscPrcnt;

            //            }

            //        }

            //        dts.Lines.DiscountPercent = double.Parse(DiscountPercent);     //折扣

            //        //dts.Lines.DiscountPercent = double.Parse(dln1.DiscPrcnt == "" ? "0" : dln1.DiscPrcnt);     //折扣

            //        if (!string.IsNullOrEmpty(dln1.U_PDXX) && (dln1.U_PDXX == "AC220" || dln1.U_PDXX == "AC380" || dln1.U_PDXX == "AC110"))

            //        {

            //            dts.Lines.UserFields.Fields.Item("U_PDXX").Value = dln1.U_PDXX;

            //        }

            //        dts.Lines.VatGroup = "X0";

            //        dts.Lines.UserFields.Fields.Item("U_ZS").Value = dln1.U_ZS;

            //        dts.Lines.UserFields.Fields.Item("U_RelDoc").Value = dln1.U_RelDoc.Trim();

            //        dts.Lines.Add();

            //    }



            //}

            //else

            //{

            //    foreach (billSalesAcctCode oact in model.billSalesAcctCode)

            //    {

            //        if (!string.IsNullOrEmpty(oact.BaseEntry))

            //        {

            //            dts.Lines.BaseEntry = int.Parse(oact.BaseEntry == "" ? "-1" : oact.BaseEntry);

            //            dts.Lines.BaseLine = int.Parse(oact.BaseLine);

            //            dts.Lines.BaseType = 23;

            //        }

            //        dts.Lines.AccountCode = oact.AcctCode;

            //        dts.Lines.ItemDescription = oact.Details;

            //        dts.Lines.UnitPrice = double.Parse(oact.Price == "" ? "0" : oact.Price);            //单价

            //        dts.Lines.DiscountPercent = double.Parse(oact.DiscPrcnt == "" ? "0" : oact.DiscPrcnt);     //折扣

            //        dts.Lines.VatGroup = "X0";

            //        dts.Lines.Add();

            //    }

            //}

            //#endregion

            //res = dts.Add();
        }

    }
}
