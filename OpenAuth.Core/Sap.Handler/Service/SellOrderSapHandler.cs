using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using SAPbobsCOM;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            StringBuilder allerror = new StringBuilder();
            int eCode;
            string eMesg;
            string docNum = string.Empty;
            var quotation = await UnitWork.Find<Quotation>(q => q.Id.Equals(theQuotationId)).AsNoTracking()
               .Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(quotation.ServiceOrderId)).FirstOrDefaultAsync();
            var oCPR = await UnitWork.Find<OCPR>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId) && o.Active=="Y").FirstOrDefaultAsync();
            var slpcode = (await UnitWork.Find<OSLP>(o => o.SlpName.Equals(quotation.CreateUser)).FirstOrDefaultAsync())?.SlpCode;
            var ywy = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId)).Select(o=>o.SlpCode).FirstOrDefaultAsync();
            List<string> typeids = new List<string> { "SYS_MaterialInvoiceCategory", "SYS_MaterialTaxRate", "SYS_InvoiceCompany", "SYS_DeliveryMethod" };
            var categoryList = await UnitWork.Find<Category>(c => typeids.Contains(c.TypeId)).ToListAsync();
            try
            {
                if (quotation != null)
                {
                    SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

                    //#region [添加主表信息]

                    dts.CardCode = serviceOrder.TerminalCustomerId; //客户id

                    dts.Comments = quotation.Remark; //备注

                    dts.ContactPersonCode = int.Parse(string.IsNullOrWhiteSpace(oCPR.CntctCode.ToString()) ? "0" : oCPR.CntctCode.ToString());//联系人代码

                    dts.SalesPersonCode = (int)slpcode; //销售人代码

                    //dts.NumAtCard = model.NumAtCard;

                    //dts.PayToCode = model.PayToCode;//支付代码

                    //dts.ShipToCode = model.ShipToCode;//购物代码

                    //dts.DocCurrency = quotation.MoneyMeans == "1" ? "RMB" : "";//货币

                    //dts.DocDate = DateTime.Parse(model.DocDate);

                    dts.DocDueDate =Convert.ToDateTime(quotation.DeliveryDate).AddDays(quotation.AcceptancePeriod==null?0: (double)quotation.AcceptancePeriod);

                    //if (!string.IsNullOrEmpty(model.TrnspCode))

                    //{

                    //    dts.TransportationCode = Convert.ToInt32(model.TrnspCode);

                    //}

                    //if (!string.IsNullOrEmpty(quotation.AcquisitionWay))

                    //{

                    //    dts.DocumentsOwner = Convert.ToInt32(quotation.AcquisitionWay);

                    //}

                    //if (model.PartSupply == "true" || model.PartSupply == "Y")

                    //{

                    //    

                    //}
                    //else

                    //{

                    //    dts.PartialSupply = BoYesNoEnum.tNO;

                    //}
                    dts.PartialSupply = BoYesNoEnum.tYES;



                    if (quotation.InvoiceCategory != null) //发票类别

                    {
                        var name = categoryList.Where(c => c.TypeId.Equals("SYS_MaterialInvoiceCategory") && c.DtValue.Equals(quotation.InvoiceCategory.ToString())).FirstOrDefault()?.Name;
                        dts.UserFields.Fields.Item("U_FPLB").Value = name; 

                    }

                    if (quotation.TaxRate != null) //税率

                    {
                        var U_SL=categoryList.Where(c => c.TypeId.Equals("SYS_MaterialTaxRate") && c.DtValue.Equals(quotation.TaxRate.ToString())).FirstOrDefault()?.Name;
                        dts.UserFields.Fields.Item("U_SL").Value = U_SL; 

                    }

                    if (ywy != null) 
                    {
                        dts.UserFields.Fields.Item("U_YWY").Value = ywy.ToString();
                    }

                    dts.DocType = BoDocumentTypes.dDocument_Items;//单据类型



                    dts.Address2 = quotation.ShippingAddress;      //收货方

                    dts.Address = quotation.CollectionAddress;       //收款方

                    if (quotation.DeliveryMethod!=null &&!string.IsNullOrEmpty(quotation.DeliveryMethod))

                    {
                        var DeliveryMethod=categoryList.Where(c => c.TypeId.Equals("SYS_DeliveryMethod") && c.DtValue.Equals(quotation.DeliveryMethod.ToString())).FirstOrDefault()?.DtValue;
                        dts.PaymentGroupCode = DeliveryMethod != null?int.Parse(DeliveryMethod): 2;  //付款条件

                    }

                    dts.Indicator = categoryList.Where(c => c.TypeId.Equals("SYS_InvoiceCompany") && c.DtValue.Equals(quotation.InvoiceCompany.ToString())).FirstOrDefault()?.DtCode ;    // 标识

                    //dts.PaymentMethod = quotation.DeliveryMethod;    //付款方式

                    //dts.FederalTaxID = model.LicTradNum;  //国税编号

                    dts.Project = "";

                    //dts.DiscountPercent = double.Parse(!string.IsNullOrEmpty(model.DiscPrcnt) ? model.DiscPrcnt : "0.00");

                    dts.DocTotal = double.Parse(!string.IsNullOrWhiteSpace(quotation.TotalMoney.ToString()) ? quotation.TotalMoney.ToString() : "0.00");

                    //errorMsg += string.Format("调用接口添加销售订单主数据[{0}]", jobID);

                    //#endregion

                    #region [添加行明细]
                    foreach (QuotationMergeMaterial dln1 in quotation.QuotationMergeMaterials)

                    {

                        dts.Lines.ItemCode = dln1.MaterialCode.Replace("&#92;", "■");

                        dts.Lines.ItemDescription = dln1.MaterialDescription;

                        dts.Lines.Quantity = string.IsNullOrWhiteSpace(dln1.Count.ToString()) ? '1' : double.Parse(dln1.Count.ToString());

                        dts.Lines.WarehouseCode = "37"; //仓库编号默认37仓

                        //if (!string.IsNullOrEmpty(dln1.BaseLine))

                        //{

                        //    dts.Lines.BaseEntry = int.Parse(dln1.BaseEntry == "" ? "-1" : dln1.BaseEntry); //报价单号

                        //    dts.Lines.BaseLine = int.Parse(dln1.BaseLine);

                        //    dts.Lines.BaseType = 23;//-1

                        //}

                        dts.Lines.SalesPersonCode = (int)slpcode;; //销售员编号

                        dts.Lines.UnitPrice = double.Parse(string.IsNullOrWhiteSpace(dln1.SalesPrice.ToString()) ? "0" : dln1.SalesPrice.ToString());            //单价

                        dts.Lines.Price = double.Parse(string.IsNullOrWhiteSpace(dln1.SalesPrice.ToString()) ? "0" : dln1.SalesPrice.ToString());

                        dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(dln1.Discount.ToString()) ? 0.00 : double.Parse(dln1.Discount.ToString());//折扣率

                        dts.Lines.LineTotal = string.IsNullOrWhiteSpace(dln1.TotalPrice.ToString()) ? 0.00 : double.Parse(dln1.TotalPrice.ToString());//总计

                        dts.Lines.DiscountPercent = double.Parse(dln1.Discount == null ? "0" : (100 - dln1.Discount).ToString());     //折扣

                        //if (!string.IsNullOrEmpty(dln1.U_PDXX) && (dln1.U_PDXX == "AC220" || dln1.U_PDXX == "AC380" || dln1.U_PDXX == "AC110"))

                        //{

                        //    dts.Lines.UserFields.Fields.Item("U_PDXX").Value = dln1.U_PDXX;//配电选项

                        //}

                        dts.Lines.VatGroup = "X0";

                        //dts.Lines.UserFields.Fields.Item("U_ZS").Value = dln1.U_ZS;//赠送

                        dts.Lines.Add();

                    }

                    #endregion

                    var res = dts.Add();
                    if (res != 0)
                    {
                        company.GetLastError(out eCode, out eMesg);
                        allerror.Append("添加销售订单到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                    }
                    else
                    {
                        company.GetNewObjectCode(out docNum);
                    }
                }
            }
            catch (Exception e)
            {
                allerror.Append("调用SBO接口添加销售订单时异常：" + e.ToString() + "");
            }

            if (!string.IsNullOrEmpty(docNum))
            {
                //如果同步成功则修改SellOrder
                await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(quotation.Id), q => new Quotation { 
                    SalesOrderId=int.Parse(docNum)
                });
                Log.Logger.Warning($"同步成功，SAP_ID：{docNum}", typeof(SellOrderSapHandler));
            }
            if (!string.IsNullOrWhiteSpace(allerror.ToString()))
            {
                Log.Logger.Error(allerror.ToString(), typeof(SellOrderSapHandler));
            }

        }

    }
}
