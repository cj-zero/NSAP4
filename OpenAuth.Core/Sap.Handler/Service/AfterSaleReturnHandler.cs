using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using Sap.Handler.Service.Request;
using SAPbobsCOM;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// 售后退料
        /// </summary>
        /// <returns></returns>
        [CapSubscribe("Serve.AfterSaleReturn.Create")]
        public async Task HandleAfterSaleReturn(AddOrUpdateQuotationReq obj)
        {
            var errorMsg = "";
            var eCode = 0;
            var eMesg = "";
            var docNum = "";
            SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oReturns);

            var quotation = await UnitWork.Find<Quotation>(q => q.Id.Equals(obj.QuotationMergeMaterialReqs.FirstOrDefault().QuotationId)).AsNoTracking()
              .Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(quotation.ServiceOrderId)).FirstOrDefaultAsync();
            var oCPR = await UnitWork.Find<OCPR>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId) && o.Active == "Y").FirstOrDefaultAsync();
            var slpcode = (await UnitWork.Find<OSLP>(o => o.SlpName.Equals(quotation.CreateUser)).FirstOrDefaultAsync())?.SlpCode;
            var ywy = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId)).Select(o => o.SlpCode).FirstOrDefaultAsync();
            var ordr = await UnitWork.Find<RDR1>(o => o.DocEntry.Equals(quotation.SalesOrderId)).Select(o => new { o.LineNum, o.ItemCode }).ToListAsync();
            //#region [添加主表信息]

            //DataTable dtRowsConn = AidTool.GetConnection(model.SboId);

            //if (dtRowsConn.Rows.Count > 0)

            //{

            //    sboname = dtRowsConn.Rows[0][0].ToString();

            //    sqlconn = dtRowsConn.Rows[0][1].ToString();

            //}

            dts.CardCode = serviceOrder.TerminalCustomerId;

            dts.Comments = quotation.Remark; 

            dts.ContactPersonCode = int.Parse(string.IsNullOrWhiteSpace(oCPR.CntctCode.ToString()) ? "0" : oCPR.CntctCode.ToString());

            dts.SalesPersonCode = (int)slpcode;

            //dts.NumAtCard = model.NumAtCard;

            //dts.PayToCode = model.PayToCode;

            //dts.ShipToCode = model.ShipToCode;

            //dts.DocCurrency = model.DocCur;

            //dts.DocDate = DateTime.Parse(model.DocDate);

            dts.DocDueDate = Convert.ToDateTime(quotation.DeliveryDate).AddDays(quotation.AcceptancePeriod == null ? 0 : (double)quotation.AcceptancePeriod);

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

            //    dts.PartialSupply = BoYesNoEnum.tYES;

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

            dts.UserFields.Fields.Item("U_EshopNo").Value = quotation.SalesOrderId.ToString();

            //if (!string.IsNullOrEmpty(model.U_SL) && model.U_SL != "")

            //{

            //    dts.UserFields.Fields.Item("U_SL").Value = model.U_SL;

            //}

            //if (!string.IsNullOrEmpty(model.U_YWY) && model.U_YWY != "")

            //{

            //    dts.UserFields.Fields.Item("U_YWY").Value = model.U_YWY;

            //}
            if (ywy != null)
            {
                dts.UserFields.Fields.Item("U_YWY").Value = ywy.ToString();
            }

            dts.DocType = BoDocumentTypes.dDocument_Items;//单据类型

            //if (!string.IsNullOrEmpty(model.DocType))

            //{

            //    if (model.DocType == "I")

            //        dts.DocType = BoDocumentTypes.dDocument_Items;

            //    if (model.DocType == "S")

            //        dts.DocType = BoDocumentTypes.dDocument_Service;

            //}

            //else { dts.DocType = BoDocumentTypes.dDocument_Items; }



            dts.Address2 = quotation.ShippingAddress;      //收货方

            dts.Address = quotation.CollectionAddress;       //收款方

            //if (!string.IsNullOrEmpty(model.CustomFields) && model.CustomFields != "{}")

            //{

            //    string[] filesName = model.CustomFields.Replace(",", "，").Replace("≮0≯", ",").Split(',');

            //    string[] filesValue = "".Split(',');

            //    for (int i = 0; i < filesName.Length; i++)

            //    {

            //        filesValue = filesName[i].Replace(":", "：").Replace("≮1≯", ":").Split(':');

            //        if (AidTool.IsExistCustomFields("ORDN", filesValue[0], sqlconn) && !string.IsNullOrEmpty(filesValue[1]))

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

            dts.Project = "";

            //dts.DiscountPercent = double.Parse(!string.IsNullOrEmpty(model.DiscPrcnt) ? model.DiscPrcnt : "0.00");

            dts.DocTotal = double.Parse(!string.IsNullOrWhiteSpace(quotation.TotalMoney.ToString()) ? quotation.TotalMoney.ToString() : "0.00");

            //#endregion


           
            #region [添加行明细]
            //if (model.DocType == "I")

            //{

            foreach (var item in  obj.QuotationMergeMaterialReqs)
            {
                var QuotationMergeMaterial = quotation.QuotationMergeMaterials.Where(q => q.Id.Equals(item.Id)).FirstOrDefault();

                errorMsg += string.Format("物料编码:[{0}]", QuotationMergeMaterial.MaterialCode);

                dts.Lines.ItemCode = QuotationMergeMaterial.MaterialCode.Replace("&#92;", "■");

                //if (!string.IsNullOrEmpty(dln1.BaseLine))

                //{

                //    dts.Lines.BaseEntry = int.Parse(dln1.BaseEntry == "" ? "-1" : dln1.BaseEntry);

                //    dts.Lines.BaseLine = int.Parse(dln1.BaseLine);

                //    dts.Lines.BaseType = int.Parse(dln1.BaseType == "" ? "-1" : dln1.BaseType);

                //}
                dts.Lines.BaseLine = (int)ordr.Where(o => o.ItemCode.Equals(QuotationMergeMaterial.MaterialCode)).FirstOrDefault()?.LineNum;
             
                dts.Lines.SalesPersonCode = (int)slpcode;

                dts.Lines.ItemDescription = QuotationMergeMaterial.MaterialDescription;

                dts.Lines.Quantity = System.Convert.ToDouble(item.InventoryQuantity);

                dts.Lines.UnitPrice = string.IsNullOrWhiteSpace(QuotationMergeMaterial.SalesPrice.ToString()) ? 0 : Convert.ToDouble(QuotationMergeMaterial.SalesPrice);            //单价

                dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(QuotationMergeMaterial.Discount.ToString()) ? 0 : Convert.ToDouble(QuotationMergeMaterial.Discount);     //折扣

                dts.Lines.Price = Convert.ToDouble(QuotationMergeMaterial.CostPrice);            //单价;

                dts.Lines.LineTotal = double.Parse(QuotationMergeMaterial.TotalPrice.ToString()) ;

                dts.Lines.WarehouseCode = item.WhsCode;

                //if (!string.IsNullOrEmpty(dln1.U_PDXX) && (dln1.U_PDXX == "AC220" || dln1.U_PDXX == "AC380" || dln1.U_PDXX == "AC110"))

                //{

                //    dts.Lines.UserFields.Fields.Item("U_PDXX").Value = dln1.U_PDXX;

                //}

                dts.Lines.VatGroup = "X0";
                dts.Lines.Add();
                #region 弃用
                //if (ExistNoDisNum(dln1.ItemCode, sqlconn))

                //{

                //foreach (billSerialNumber osrn in model.serialNumber)

                //{

                //    if (osrn.ItemCode == dln1.ItemCode)

                //    {

                //        int i = 0;

                //        foreach (billSerialNumberChooseItem serial in osrn.Details)

                //        {

                //            int sysnum = Convert.ToInt32(serial.SysSerial);

                //            dts.Lines.SerialNumbers.SystemSerialNumber = sysnum;

                //            dts.Lines.SerialNumbers.ManufacturerSerialNumber = serial.SuppSerial;

                //            dts.Lines.SerialNumbers.InternalSerialNumber = System.Convert.ToString(i + 3);

                //            dts.Lines.SerialNumbers.SetCurrentLine(i);

                //            dts.Lines.SerialNumbers.Add();

                //            errorMsg += string.Format("系统编号:[{0}],序列号:[{1}],制造商序列号:[{2}]-->[{3}]", serial.SysSerial, serial.SuppSerial, serial.IntrSerial, i);

                //            i++;

                //        }

                //    }

                //}

                ////}

                //dts.Lines.ItemCode = dln1.ItemCode.Replace("&#92;", "■");

                //if (!string.IsNullOrEmpty(dln1.BaseLine))

                //{

                //    dts.Lines.BaseEntry = int.Parse(dln1.BaseEntry == "" ? "-1" : dln1.BaseEntry);

                //    dts.Lines.BaseLine = int.Parse(dln1.BaseLine);

                //    dts.Lines.BaseType = int.Parse(dln1.BaseType == "" ? "-1" : dln1.BaseType);

                //}

                //dts.Lines.SalesPersonCode = int.Parse(model.SlpCode == "" ? "-1" : model.SlpCode);

                //dts.Lines.ItemDescription = dln1.Dscription;

                //dts.Lines.Quantity = System.Convert.ToDouble(dln1.Quantity);

                //dts.Lines.UnitPrice = double.Parse(string.IsNullOrWhiteSpace(dln1.PriceBefDi) ? "0" : dln1.PriceBefDi);            //单价

                //dts.Lines.Price = double.Parse(string.IsNullOrWhiteSpace(dln1.PriceBefDi) ? "0" : dln1.PriceBefDi);

                //dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(dln1.DiscPrcnt) ? 0.00 : double.Parse(dln1.DiscPrcnt);

                //dts.Lines.LineTotal = string.IsNullOrWhiteSpace(dln1.LineTotal) ? 0.00 : double.Parse(dln1.LineTotal);

                //dts.Lines.WarehouseCode = "01";

                //if (!string.IsNullOrEmpty(dln1.U_PDXX) && (dln1.U_PDXX == "AC220" || dln1.U_PDXX == "AC380" || dln1.U_PDXX == "AC110"))

                //{

                //    dts.Lines.UserFields.Fields.Item("U_PDXX").Value = dln1.U_PDXX;

                //}

                //dts.Lines.VatGroup = "X0";

                //dts.Lines.UserFields.Fields.Item("U_ZS").Value = dln1.U_ZS;

                //dts.Lines.Add();



                //}

                //if (!ExistNoDisNum(dln1.ItemCode, sqlconn))

                //{



                //}
                #endregion

            }


            #endregion

            var res = dts.Add();

            if (res != 0)

            {

                company.GetLastError(out eCode, out eMesg);

                errorMsg += string.Format("添加销售退货时调接口发生异常[异常代码:{1},异常信息:{2}]",eCode, eMesg);

            }
            else
            {
                company.GetNewObjectCode(out docNum);
                errorMsg += string.Format("调用接口添加销售退货操作成功,ID[{0}", docNum);

            }
        }
    }
}
