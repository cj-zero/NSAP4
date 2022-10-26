using DotNetCore.CAP;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using Sap.Handler.Sap;
using Sap.Handler.Service.Request;
using SAPbobsCOM;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sap.Handler.Service
{
    public class AfterSaleReturnHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;
        public AfterSaleReturnHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }

        #region 售后退料
        /// <summary>
        /// 售后退料
        /// </summary>
        /// <returns></returns>
        //[CapSubscribe("Serve.AfterSaleReturn.Create")]
        //public async Task HandleAfterSaleReturn(AddOrUpdateQuotationReq obj)
        //{
        //var errorMsg = "";
        //var eCode = 0;
        //var eMesg = "";
        //var docNum = "";
        //SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oReturns);
        //bool IsReturnNote = false;
        //string remark = string.Empty;
        //List<int> quotationIds = new List<int>();
        //int quotationId = obj.QuotationMergeMaterialReqs.FirstOrDefault().QuotationId == null ? 0 : (int)obj.QuotationMergeMaterialReqs.FirstOrDefault().QuotationId;
        //int returnNoteId = obj.QuotationMergeMaterialReqs.FirstOrDefault().ReturnNoteId == null ? 0 : (int)obj.QuotationMergeMaterialReqs.FirstOrDefault().ReturnNoteId;
        //double money = 0;
        //if (returnNoteId > 0)
        //{
        //    var returnNoteInfo = await UnitWork.Find<ReturnNote>(w => w.Id == returnNoteId).FirstOrDefaultAsync();
        //    remark = returnNoteInfo.Remark;
        //    IsReturnNote = true;
        //    money = (double)returnNoteInfo.TotalMoney;
        //    string StockOutIds = (await UnitWork.Find<ReturnNote>(w => w.Id == returnNoteId).FirstOrDefaultAsync()).StockOutId;
        //    var arr = StockOutIds.Split(",");
        //    for (int i = 0; i < arr.Length; i++)
        //    {
        //        if (!quotationIds.Contains(Convert.ToInt32(arr[i])))
        //        {
        //            quotationIds.Add(Convert.ToInt32(arr[i]));
        //        }
        //    }
        //}
        //else
        //{
        //    quotationIds.Add(quotationId);
        //}
        //var quotation = await UnitWork.Find<Quotation>(q => quotationIds.Contains(q.Id)).AsNoTracking().ToListAsync();
        ////获取所有领料清单
        //var qutationMaterials = await UnitWork.Find<QuotationMergeMaterial>(w => quotationIds.Contains((int)w.QuotationId)).ToListAsync();
        //var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(quotation.FirstOrDefault().ServiceOrderId)).FirstOrDefaultAsync();
        //var oCPR = await UnitWork.Find<OCPR>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId) && o.Active == "Y").FirstOrDefaultAsync();
        //var slpcode = (await UnitWork.Find<OSLP>(o => o.SlpName.Equals(quotation.FirstOrDefault().CreateUser)).FirstOrDefaultAsync())?.SlpCode;
        //var ywy = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId)).Select(o => o.SlpCode).FirstOrDefaultAsync();
        ////获取所有领料单的销售单
        //var salesOrderIds = quotation.Select(s => s.SalesOrderId).Distinct().ToList();
        //var ordr = await UnitWork.Find<RDR1>(o => salesOrderIds.Contains(o.DocEntry)).Select(o => new { o.LineNum, o.ItemCode }).ToListAsync();
        ////#region [添加主表信息]

        ////DataTable dtRowsConn = AidTool.GetConnection(model.SboId);

        ////if (dtRowsConn.Rows.Count > 0)

        ////{

        ////    sboname = dtRowsConn.Rows[0][0].ToString();

        ////    sqlconn = dtRowsConn.Rows[0][1].ToString();

        ////}

        //dts.CardCode = serviceOrder.TerminalCustomerId;

        //dts.Comments = IsReturnNote ? remark : quotation.FirstOrDefault().Remark;

        //dts.ContactPersonCode = int.Parse(string.IsNullOrWhiteSpace(oCPR.CntctCode.ToString()) ? "0" : oCPR.CntctCode.ToString());

        //dts.SalesPersonCode = (int)slpcode;

        ////dts.NumAtCard = model.NumAtCard;

        ////dts.PayToCode = model.PayToCode;

        ////dts.ShipToCode = model.ShipToCode;

        ////dts.DocCurrency = model.DocCur;

        ////dts.DocDate = DateTime.Parse(model.DocDate);

        //dts.DocDueDate = Convert.ToDateTime(quotation.FirstOrDefault().DeliveryDate).AddDays(quotation.FirstOrDefault().AcceptancePeriod == null ? 0 : (double)quotation.FirstOrDefault().AcceptancePeriod);

        ////dts.TaxDate = DateTime.Parse(model.TaxDate);

        ////if (!string.IsNullOrEmpty(model.TrnspCode))

        ////{

        ////    dts.TransportationCode = Convert.ToInt32(model.TrnspCode);

        ////}

        ////if (!string.IsNullOrEmpty(model.OwnerCode))

        ////{

        ////    dts.DocumentsOwner = Convert.ToInt32(model.OwnerCode);

        ////}

        ////if (model.PartSupply == "true")

        ////{

        ////    dts.PartialSupply = BoYesNoEnum.tYES;

        ////}

        ////else

        ////{

        ////    dts.PartialSupply = BoYesNoEnum.tNO;

        ////}
        //dts.PartialSupply = BoYesNoEnum.tYES;

        ////if (!string.IsNullOrEmpty(model.U_FPLB) && model.U_FPLB != "")

        ////{

        ////    dts.UserFields.Fields.Item("U_FPLB").Value = model.U_FPLB;

        ////}

        //dts.UserFields.Fields.Item("U_EshopNo").Value = string.Join(",", salesOrderIds);

        ////if (!string.IsNullOrEmpty(model.U_SL) && model.U_SL != "")

        ////{

        ////    dts.UserFields.Fields.Item("U_SL").Value = model.U_SL;

        ////}

        ////if (!string.IsNullOrEmpty(model.U_YWY) && model.U_YWY != "")

        ////{

        ////    dts.UserFields.Fields.Item("U_YWY").Value = model.U_YWY;

        ////}
        //if (ywy != null)
        //{
        //    dts.UserFields.Fields.Item("U_YWY").Value = ywy.ToString();
        //}

        //dts.DocType = BoDocumentTypes.dDocument_Items;//单据类型

        ////if (!string.IsNullOrEmpty(model.DocType))

        ////{

        ////    if (model.DocType == "I")

        ////        dts.DocType = BoDocumentTypes.dDocument_Items;

        ////    if (model.DocType == "S")

        ////        dts.DocType = BoDocumentTypes.dDocument_Service;

        ////}

        ////else { dts.DocType = BoDocumentTypes.dDocument_Items; }



        //dts.Address2 = IsReturnNote ? string.Empty : quotation.FirstOrDefault().ShippingAddress;      //收货方

        //dts.Address = IsReturnNote ? string.Empty : quotation.FirstOrDefault().CollectionAddress;       //收款方

        ////if (!string.IsNullOrEmpty(model.CustomFields) && model.CustomFields != "{}")

        ////{

        ////    string[] filesName = model.CustomFields.Replace(",", "，").Replace("≮0≯", ",").Split(',');

        ////    string[] filesValue = "".Split(',');

        ////    for (int i = 0; i < filesName.Length; i++)

        ////    {

        ////        filesValue = filesName[i].Replace(":", "：").Replace("≮1≯", ":").Split(':');

        ////        if (AidTool.IsExistCustomFields("ORDN", filesValue[0], sqlconn) && !string.IsNullOrEmpty(filesValue[1]))

        ////        {

        ////            dts.UserFields.Fields.Item(filesValue[0]).Value = filesValue[1];

        ////        }

        ////    }

        ////}

        ////if (!string.IsNullOrEmpty(model.GroupNum))

        ////{

        ////    dts.PaymentGroupCode = int.Parse(model.GroupNum);   //付款条款

        ////}

        ////dts.Indicator = model.Indicator;    // 标识

        ////dts.PaymentMethod = model.PeyMethod;    //付款方式

        ////dts.FederalTaxID = model.LicTradNum;  //国税编号

        //dts.Project = "";

        ////dts.DiscountPercent = double.Parse(!string.IsNullOrEmpty(model.DiscPrcnt) ? model.DiscPrcnt : "0.00");

        //dts.DocTotal = IsReturnNote ? money : double.Parse(!string.IsNullOrWhiteSpace(quotation.FirstOrDefault().TotalMoney.ToString()) ? quotation.FirstOrDefault().TotalMoney.ToString() : "0.00");

        ////#endregion



        //#region [添加行明细]
        ////if (model.DocType == "I")

        ////{

        //foreach (var item in obj.QuotationMergeMaterialReqs)
        //{
        //    var QuotationMergeMaterial = qutationMaterials.Where(q => q.Id.Equals(item.Id)).FirstOrDefault();

        //    errorMsg += string.Format("物料编码:[{0}]", QuotationMergeMaterial.MaterialCode);

        //    dts.Lines.ItemCode = QuotationMergeMaterial.MaterialCode.Replace("&#92;", "■");

        //    //if (!string.IsNullOrEmpty(dln1.BaseLine))

        //    //{

        //    //    dts.Lines.BaseEntry = int.Parse(dln1.BaseEntry == "" ? "-1" : dln1.BaseEntry);

        //    //    dts.Lines.BaseLine = int.Parse(dln1.BaseLine);

        //    //    dts.Lines.BaseType = int.Parse(dln1.BaseType == "" ? "-1" : dln1.BaseType);

        //    //}
        //    dts.Lines.BaseLine = (int)ordr.Where(o => o.ItemCode.Equals(QuotationMergeMaterial.MaterialCode)).FirstOrDefault()?.LineNum;

        //    dts.Lines.SalesPersonCode = (int)slpcode;

        //    dts.Lines.ItemDescription = QuotationMergeMaterial.MaterialDescription;

        //    dts.Lines.Quantity = System.Convert.ToDouble(item.InventoryQuantity);

        //    dts.Lines.UnitPrice = string.IsNullOrWhiteSpace(QuotationMergeMaterial.SalesPrice.ToString()) ? 0 : Convert.ToDouble(QuotationMergeMaterial.SalesPrice);            //单价

        //    dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(QuotationMergeMaterial.Discount.ToString()) ? 0 : Convert.ToDouble(QuotationMergeMaterial.Discount);     //折扣

        //    dts.Lines.Price = Convert.ToDouble(QuotationMergeMaterial.CostPrice);            //单价;

        //    dts.Lines.LineTotal = double.Parse(QuotationMergeMaterial.TotalPrice.ToString());

        //    dts.Lines.WarehouseCode = item.WhsCode;

        //    //if (!string.IsNullOrEmpty(dln1.U_PDXX) && (dln1.U_PDXX == "AC220" || dln1.U_PDXX == "AC380" || dln1.U_PDXX == "AC110"))

        //    //{

        //    //    dts.Lines.UserFields.Fields.Item("U_PDXX").Value = dln1.U_PDXX;

        //    //}

        //    dts.Lines.VatGroup = "X0";
        //    dts.Lines.Add();
        //    #region 弃用
        //    //if (ExistNoDisNum(dln1.ItemCode, sqlconn))

        //    //{

        //    //foreach (billSerialNumber osrn in model.serialNumber)

        //    //{

        //    //    if (osrn.ItemCode == dln1.ItemCode)

        //    //    {

        //    //        int i = 0;

        //    //        foreach (billSerialNumberChooseItem serial in osrn.Details)

        //    //        {

        //    //            int sysnum = Convert.ToInt32(serial.SysSerial);

        //    //            dts.Lines.SerialNumbers.SystemSerialNumber = sysnum;

        //    //            dts.Lines.SerialNumbers.ManufacturerSerialNumber = serial.SuppSerial;

        //    //            dts.Lines.SerialNumbers.InternalSerialNumber = System.Convert.ToString(i + 3);

        //    //            dts.Lines.SerialNumbers.SetCurrentLine(i);

        //    //            dts.Lines.SerialNumbers.Add();

        //    //            errorMsg += string.Format("系统编号:[{0}],序列号:[{1}],制造商序列号:[{2}]-->[{3}]", serial.SysSerial, serial.SuppSerial, serial.IntrSerial, i);

        //    //            i++;

        //    //        }

        //    //    }

        //    //}

        //    ////}

        //    //dts.Lines.ItemCode = dln1.ItemCode.Replace("&#92;", "■");

        //    //if (!string.IsNullOrEmpty(dln1.BaseLine))

        //    //{

        //    //    dts.Lines.BaseEntry = int.Parse(dln1.BaseEntry == "" ? "-1" : dln1.BaseEntry);

        //    //    dts.Lines.BaseLine = int.Parse(dln1.BaseLine);

        //    //    dts.Lines.BaseType = int.Parse(dln1.BaseType == "" ? "-1" : dln1.BaseType);

        //    //}

        //    //dts.Lines.SalesPersonCode = int.Parse(model.SlpCode == "" ? "-1" : model.SlpCode);

        //    //dts.Lines.ItemDescription = dln1.Dscription;

        //    //dts.Lines.Quantity = System.Convert.ToDouble(dln1.Quantity);

        //    //dts.Lines.UnitPrice = double.Parse(string.IsNullOrWhiteSpace(dln1.PriceBefDi) ? "0" : dln1.PriceBefDi);            //单价

        //    //dts.Lines.Price = double.Parse(string.IsNullOrWhiteSpace(dln1.PriceBefDi) ? "0" : dln1.PriceBefDi);

        //    //dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(dln1.DiscPrcnt) ? 0.00 : double.Parse(dln1.DiscPrcnt);

        //    //dts.Lines.LineTotal = string.IsNullOrWhiteSpace(dln1.LineTotal) ? 0.00 : double.Parse(dln1.LineTotal);

        //    //dts.Lines.WarehouseCode = "01";

        //    //if (!string.IsNullOrEmpty(dln1.U_PDXX) && (dln1.U_PDXX == "AC220" || dln1.U_PDXX == "AC380" || dln1.U_PDXX == "AC110"))

        //    //{

        //    //    dts.Lines.UserFields.Fields.Item("U_PDXX").Value = dln1.U_PDXX;

        //    //}

        //    //dts.Lines.VatGroup = "X0";

        //    //dts.Lines.UserFields.Fields.Item("U_ZS").Value = dln1.U_ZS;

        //    //dts.Lines.Add();



        //    //}

        //    //if (!ExistNoDisNum(dln1.ItemCode, sqlconn))

        //    //{



        //    //}
        //    #endregion

        //}


        //#endregion

        //var res = dts.Add();

        //if (res != 0)

        //{

        //    company.GetLastError(out eCode, out eMesg);

        //    errorMsg += string.Format("添加销售退货时调接口发生异常[异常代码:{1},异常信息:{2}]", eCode, eMesg);

        //}
        //else
        //{
        //    company.GetNewObjectCode(out docNum);
        //    errorMsg += string.Format("调用接口添加销售退货操作成功,ID[{0}", docNum);

        //}
        //}
        #endregion

        /// <summary>
        /// 应收贷项凭证
        /// </summary>
        /// <returns></returns>
        [CapSubscribe("Serve.ReceiptCreditVouchers.Create")]
        public void HandleReceiptCreditVouchers(AddOrUpdateQuotationReq obj)
        {
            var errorMsg = "";
            var eCode = 0;
            var eMesg = "";
            var docNum = "";
            var quotation =  UnitWork.Find<Quotation>(q => q.SalesOrderId == obj.SalesOrderId).Include(q => q.QuotationMergeMaterials).FirstOrDefault();
            var serviceOrder =  UnitWork.Find<ServiceOrder>(s => s.Id.Equals(quotation.ServiceOrderId)).FirstOrDefault();
            var oCPR =  UnitWork.Find<OCPR>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId) && o.Active == "Y").FirstOrDefault();
            var userId = ( UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(quotation.CreateUserId)).FirstOrDefault())?.NsapUserId;
            var slpcode = ( UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefault())?.sale_id;
            if (string.IsNullOrWhiteSpace(slpcode.ToString()))
            {
                slpcode = ( UnitWork.Find<OSLP>(o => o.SlpName.Equals(quotation.CreateUser)).FirstOrDefault())?.SlpCode;
            }
            var inv1 =  UnitWork.Find<INV1>(o => o.DocEntry.Equals(obj.InvoiceDocEntry)).Select(o => new { o.LineNum, o.ItemCode, o.Price ,o.BaseEntry}).ToList();
            var ywy =  UnitWork.Find<OCRD>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId)).Select(o => o.SlpCode).FirstOrDefault();
            List<string> typeids = new List<string> { "SYS_MaterialInvoiceCategory", "SYS_MaterialTaxRate", "SYS_InvoiceCompany", "SYS_DeliveryMethod" };
            var categoryList =  UnitWork.Find<Category>(c => typeids.Contains(c.TypeId)).ToList();
            SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oCreditNotes);

            #region [添加主表信息]

            //DataTable dtRowsConn = AidTool.GetConnection(model.SboId);

            //if (dtRowsConn.Rows.Count > 0)

            //{

            //    sboname = dtRowsConn.Rows[0][0].ToString();

            //    sqlconn = dtRowsConn.Rows[0][1].ToString();

            //}

            dts.CardCode = serviceOrder.TerminalCustomerId;

            dts.DocDate = DateTime.Now;
            dts.TaxDate = DateTime.Now;

            dts.Comments = quotation.Remark + "基于销售订单" + quotation.SalesOrderId + "基于销售交货"+ inv1.FirstOrDefault()?.BaseEntry+ "基于应收发票" + obj.InvoiceDocEntry;

            dts.ContactPersonCode = int.Parse(string.IsNullOrWhiteSpace(oCPR.CntctCode.ToString()) ? "0" : oCPR.CntctCode.ToString());

            dts.SalesPersonCode = (int)slpcode;

            //dts.NumAtCard = model.NumAtCard;

            //dts.PayToCode = model.PayToCode;

            //dts.ShipToCode = model.ShipToCode;

            dts.DocCurrency = "RMB";

            //dts.DocDate = DateTime.Parse(model.DocDate);

            dts.DocDueDate = DateTime.Now;

            //dts.TaxDate = DateTime.Parse(model.TaxDate);

            //if (!string.IsNullOrEmpty(model.TrnspCode))

            //{

            //    dts.TransportationCode = Convert.ToInt32(model.TrnspCode);

            //}

            //if (!string.IsNullOrEmpty(model.OwnerCode))

            //{

            //    dts.DocumentsOwner = Convert.ToInt32(model.OwnerCode);

            //}

            //if (model.PartSupply == "true")

            //{



            //}

            //else

            //{

            //    dts.PartialSupply = BoYesNoEnum.tNO;

            //}
            dts.PartialSupply = BoYesNoEnum.tYES;

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

            if (!string.IsNullOrWhiteSpace(ywy.ToString()))
            {
                dts.UserFields.Fields.Item("U_YWY").Value = ywy.ToString();
            }
            if (!string.IsNullOrWhiteSpace(quotation.InvoiceCategory)) //发票类别

            {
                var name = categoryList.Where(c => c.TypeId.Equals("SYS_MaterialInvoiceCategory") && c.DtValue.Equals(quotation.InvoiceCategory.ToString())).FirstOrDefault()?.Name;
                dts.UserFields.Fields.Item("U_FPLB").Value = name;

            }

            if (!string.IsNullOrWhiteSpace(quotation.TaxRate)) //税率

            {
                var U_SL = categoryList.Where(c => c.TypeId.Equals("SYS_MaterialTaxRate") && c.DtValue.Equals(quotation.TaxRate.ToString())).FirstOrDefault()?.Name;
                dts.UserFields.Fields.Item("U_SL").Value = U_SL;

            }

            //if (!string.IsNullOrEmpty(model.DocType))

            //{

            //    if (model.DocType == "I")

            //        dts.DocType = BoDocumentTypes.dDocument_Items;

            //    if (model.DocType == "S")

            //        dts.DocType = BoDocumentTypes.dDocument_Service;

            //}else { dts.DocType = BoDocumentTypes.dDocument_Items; }
            dts.DocType = BoDocumentTypes.dDocument_Items;


            dts.Address2 = quotation.ShippingAddress;      //收货方

            dts.Address = quotation.CollectionAddress;       //收款方

            //if (!string.IsNullOrEmpty(model.CustomFields) && model.CustomFields != "{}")

            //{

            //    string[] filesName = model.CustomFields.Replace("≮0≯", ",").Split(',');

            //    string[] filesValue = "".Split(',');

            //    for (int i = 0; i < filesName.Length; i++)

            //    {

            //        filesValue = filesName[i].Replace("≮1≯", ":").Split(':');

            //        if (AidTool.IsExistCustomFields("ODLN", filesValue[0], sqlconn) && !string.IsNullOrEmpty(filesValue[1]))

            //        {

            //            dts.UserFields.Fields.Item(filesValue[0]).Value = filesValue[1];

            //        }

            //    }

            //}

            //if (!string.IsNullOrEmpty(model.GroupNum))

            //{

            //    dts.PaymentGroupCode = int.Parse(model.GroupNum);   //付款条款

            //}
            if (quotation.DeliveryMethod != null && !string.IsNullOrEmpty(quotation.DeliveryMethod))

            {
                var DeliveryMethod = categoryList.Where(c => c.TypeId.Equals("SYS_DeliveryMethod") && c.DtValue.Equals(quotation.DeliveryMethod.ToString())).FirstOrDefault()?.DtCode;
                dts.PaymentGroupCode = DeliveryMethod != null ? int.Parse(DeliveryMethod) : 2;  //付款条件

            }

            dts.Indicator = categoryList.Where(c => c.TypeId.Equals("SYS_InvoiceCompany") && c.DtValue.Equals(quotation.InvoiceCompany.ToString())).FirstOrDefault()?.DtCode;    // 标识


            //dts.Indicator = model.Indicator;    // 标识

            //dts.PaymentMethod = model.PeyMethod;    //付款方式

            //dts.FederalTaxID = model.LicTradNum;  //国税编号

            dts.Project = "";

            //errorMsg += string.Format("调用接口添加销售交货主数据[{0}]", jobID);

            #endregion

            #region [添加行明细]
            decimal TotalMoney = 0;
            foreach (var dln1 in obj.QuotationMergeMaterialReqs)
            {
                var materials = quotation.QuotationMergeMaterials.Where(q => q.Id.Equals(dln1.Id)).FirstOrDefault();
                dts.Lines.ItemCode = dln1.MaterialCode.Replace("&#92;", "■");

                dts.Lines.ItemDescription = dln1.MaterialDescription;

                dts.Lines.Quantity = string.IsNullOrEmpty(dln1.InventoryQuantity.ToString()) ? '0' : double.Parse(dln1.InventoryQuantity.ToString());

                dts.Lines.WarehouseCode = dln1.WhsCode;

                if (dln1.MaterialCode == materials.MaterialCode)
                {
                    dts.Lines.BaseEntry = (int)obj?.InvoiceDocEntry;

                    dts.Lines.BaseLine = (int)inv1.Where(o => o.ItemCode.Equals(materials.MaterialCode) && (o.Price == materials.DiscountPrices || o.Price == decimal.Parse(Convert.ToDecimal(materials.DiscountPrices).ToString("#0.00")))).FirstOrDefault()?.LineNum;

                    dts.Lines.BaseType = 13;
                }

                dts.Lines.SalesPersonCode = (int)slpcode;

                //dts.Lines.UnitPrice = string.IsNullOrWhiteSpace(materials.DiscountPrices.ToString()) ? 0 : Convert.ToDouble(materials.DiscountPrices);            //单价

                dts.Lines.Price = double.Parse(string.IsNullOrWhiteSpace(materials.DiscountPrices.ToString()) ? "0" : materials.DiscountPrices.ToString());

                //dts.Lines.LineTotal = double.Parse(Convert.ToDecimal(materials.DiscountPrices * dln1.InventoryQuantity).ToString("#0.00"));//总计
                //dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(materials.Discount.ToString()) ? 0 : Convert.ToDouble(materials.Discount);     //折扣
                TotalMoney += materials.MaterialType == 1 ? decimal.Parse(Convert.ToDecimal(materials.DiscountPrices * dln1.InventoryQuantity).ToString("#0.00")) : 0;//总计
                                                                                                                                                                      //if (!string.IsNullOrEmpty(dln1.U_PDXX) && (dln1.U_PDXX == "AC220" || dln1.U_PDXX == "AC380" || dln1.U_PDXX == "AC110"))

                //{

                //    dts.Lines.UserFields.Fields.Item("U_PDXX").Value = dln1.U_PDXX;

                //}

                dts.Lines.VatGroup = "X0";

                dts.Lines.Add();

            }

            #endregion
            var res = dts.Add();

            if (res != 0)

            {

                company.GetLastError(out eCode, out eMesg);
                Log.Logger.Error("添加销售交货到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                throw new Exception(errorMsg);
            }
            else
            {
                company.GetNewObjectCode(out docNum);
                Log.Logger.Warning("添加销售交货到SAP成功");
                HandleReceiptCreditVouchersERP(docNum, obj.InvoiceDocEntry);
                if (TotalMoney > 0)
                {
                    var userCount = UnitWork.Find<amountinarear>(a => a.UserId.Equals(quotation.CreateUserId)).FirstOrDefault();
                    if (userCount != null)
                    {
                        UnitWork.Update<amountinarear>(a => a.Id.Equals(userCount.Id), a => new amountinarear { Money = a.Money - TotalMoney });
                        UnitWork.Add<amountinarearlog>(new amountinarearlog { Id = Guid.NewGuid().ToString(), Money = TotalMoney, PlusOrMinus = false, SalesOrderId = quotation.SalesOrderId, CreateTime = DateTime.Now, CreateUserId = quotation.CreateUserId, CreateUserName = quotation.CreateUser, AmountInArearId = userCount.Id, Liaison = int.Parse(docNum), Remark = "应收贷项凭证减少挂账金额" });
                    }
                }
            }

        }

        /// <summary>
        /// 应收贷项凭证同步ERP3.0
        /// </summary>
        /// <returns></returns>
        [CapSubscribe("Serve.ReceiptCreditVouchers.ERPCreate")]
        public void HandleReceiptCreditVouchersERP(string docNum, int? InvoiceDocEntry)
        {
            string Message = "";
            try
            {

                //应收贷项凭证
                var ORINmodel = UnitWork.Find<ORIN>(o => o.DocEntry == int.Parse(docNum)).AsNoTracking().FirstOrDefault();
                var RIN1model = UnitWork.Find<RIN1>(o => o.DocEntry == int.Parse(docNum)).AsNoTracking().ToList();

                if (ORINmodel.PartSupply == "true") { ORINmodel.PartSupply = "Y"; } else { ORINmodel.PartSupply = "N"; }

                if (ORINmodel.DocCur == "") { ORINmodel.DocCur = "RMB"; }

                //if (dtRowODLN.Rows[0][3].ToString() == "") { model.DocRate = 1; }

                if (ORINmodel.DocTotal == null) { ORINmodel.DocTotal = 0; }

                if (ORINmodel.OwnerCode == null) { ORINmodel.OwnerCode = -1; }

                if (ORINmodel.SlpCode == null) { ORINmodel.SlpCode = -1; }
                sale_orin sale_Orin = ORINmodel.MapTo<sale_orin>();
                List<sale_rin1> sale_Rin1 = RIN1model.MapToList<sale_rin1>();

                sale_Orin.sbo_id = Define.SBO_ID;
                sale_Rin1.ForEach(s => s.sbo_id = Define.SBO_ID);
                UnitWork.Add<sale_orin, int>(sale_Orin);
                UnitWork.BatchAdd<sale_rin1, int>(sale_Rin1.ToArray());
                UnitWork.Save();
                //仓库数量
                List<string> itemcodes = sale_Rin1.Select(s => s.ItemCode).ToList();
                List<string> WhsCodes = sale_Rin1.Select(s => s.WhsCode).ToList();
                var oitwList =  UnitWork.Find<OITW>(o => itemcodes.Contains(o.ItemCode) && WhsCodes.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.IsCommited, o.OnHand, o.OnOrder }).AsNoTracking().ToList();
                foreach (var item in oitwList)
                {
                    var WhsCode = sale_Rin1.Where(q => q.ItemCode.Equals(item.ItemCode)).FirstOrDefault().WhsCode;
                    UnitWork.Update<store_oitw>(o => o.sbo_id == Define.SBO_ID && o.ItemCode == item.ItemCode && o.WhsCode == WhsCode, o => new store_oitw
                    {
                        OnHand = item.OnHand,
                        IsCommited = item.IsCommited,
                        OnOrder = item.OnOrder
                    });
                }
                var oitmList =  UnitWork.Find<OITM>(o => itemcodes.Contains(o.ItemCode)).Select(o => new { o.ItemCode, o.IsCommited, o.OnHand, o.OnOrder, o.LastPurCur, o.LastPurPrc, o.LastPurDat, o.UpdateDate }).AsNoTracking().ToList();
                foreach (var item in oitmList)
                {
                    UnitWork.Update<store_oitm>(o => o.sbo_id == Define.SBO_ID && o.ItemCode == item.ItemCode, o => new store_oitm
                    {
                        OnHand = item.OnHand,
                        IsCommited = item.IsCommited,
                        OnOrder = item.OnOrder,
                        LastPurDat = item.LastPurDat,
                        LastPurPrc = item.LastPurPrc,
                        LastPurCur = item.LastPurCur,
                        UpdateDate = item.UpdateDate
                    });
                }
                UnitWork.Save();
                //应收发票
                var OINVmodel =  UnitWork.Find<OINV>(o => o.DocEntry == InvoiceDocEntry).AsNoTracking().FirstOrDefault();
                var INV1model =  UnitWork.Find<INV1>(o => o.DocEntry == InvoiceDocEntry).AsNoTracking().ToList();
                sale_oinv sale_Oinv = OINVmodel.MapTo<sale_oinv>();
                List<sale_inv1> sale_Inv1 = INV1model.MapToList<sale_inv1>();
                List<uint> docenrtys = new List<uint>(), lineNums = new List<uint>();
                sale_Oinv.sbo_id = Define.SBO_ID;
                sale_Inv1.ForEach(s => { s.sbo_id = Define.SBO_ID; docenrtys.Add(s.DocEntry); lineNums.Add(s.LineNum); });
                var oinvCount =  UnitWork.Find<sale_oinv>(s => s.sbo_id == Define.SBO_ID && s.DocEntry == sale_Oinv.DocEntry).Count();
                var inv1Count =  UnitWork.Find<sale_inv1>(s => s.sbo_id == Define.SBO_ID && docenrtys.Contains(s.DocEntry) && lineNums.Contains(s.LineNum)).Count();
                if (oinvCount > 0)
                {
                     UnitWork.Update<sale_oinv>(sale_Oinv);
                }
                else
                {
                    UnitWork.Add<sale_oinv, int>(sale_Oinv);
                }
                if (inv1Count > 0)
                {
                    UnitWork.BatchUpdate<sale_inv1>(sale_Inv1.ToArray());
                }
                else
                {
                     UnitWork.BatchAddAsync<sale_inv1, int>(sale_Inv1.ToArray());
                }
                UnitWork.Save();
                Log.Logger.Warning("添加应收贷项凭证到erp3.0成功");
            }
            catch (Exception e)
            {
                Message = $"添加应收贷项凭证:{docNum}到erp3.0时异常！错误信息：" + e.Message;
                Log.Logger.Warning($"添加应收贷项凭证:{docNum}到erp3.0时异常！错误信息：" + e.Message);
            }
            if (Message != "")
            {
                throw new Exception(Message.ToString());
            }

        }
    }
}
