using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using Sap.Handler.Sap;
using SAPbobsCOM;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sap.Handler.Service
{
    public class SellOrderSapHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;

        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

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
            var oCPRs = await UnitWork.Find<OCPR>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId) && o.Active == "Y").ToListAsync();
            var oCPR = oCPRs.Where(o => o.Name.Equals(serviceOrder.NewestContacter)).FirstOrDefault() == null ? oCPRs.FirstOrDefault() : oCPRs.Where(o => o.Name.Equals(serviceOrder.NewestContacter)).FirstOrDefault();
            var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(quotation.CreateUserId)).FirstOrDefaultAsync())?.NsapUserId;
            var slpcode = (await UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefaultAsync())?.sale_id;
            var ywy = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId)).Select(o => o.SlpCode).FirstOrDefaultAsync();
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

                    dts.DocDueDate = Convert.ToDateTime(quotation.DeliveryDate);

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

                    if (!string.IsNullOrWhiteSpace(ywy.ToString()))
                    {
                        dts.UserFields.Fields.Item("U_YWY").Value = ywy.ToString();
                    }

                    dts.DocType = BoDocumentTypes.dDocument_Items;//单据类型



                    dts.Address2 = quotation.ShippingAddress;      //收货方

                    dts.Address = quotation.CollectionAddress;       //收款方

                    if (quotation.DeliveryMethod != null && !string.IsNullOrEmpty(quotation.DeliveryMethod))

                    {
                        var DeliveryMethod = categoryList.Where(c => c.TypeId.Equals("SYS_DeliveryMethod") && c.DtValue.Equals(quotation.DeliveryMethod.ToString())).FirstOrDefault()?.DtCode;
                        dts.PaymentGroupCode = DeliveryMethod != null ? int.Parse(DeliveryMethod) : 2;  //付款条件

                    }

                    dts.Indicator = categoryList.Where(c => c.TypeId.Equals("SYS_InvoiceCompany") && c.DtValue.Equals(quotation.InvoiceCompany.ToString())).FirstOrDefault()?.DtCode;    // 标识

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

                        dts.Lines.WarehouseCode = dln1.WhsCode; //仓库编号

                        //if (!string.IsNullOrEmpty(dln1.BaseLine))

                        //{

                        //    dts.Lines.BaseEntry = int.Parse(dln1.BaseEntry == "" ? "-1" : dln1.BaseEntry); //报价单号

                        //    dts.Lines.BaseLine = int.Parse(dln1.BaseLine);

                        //    dts.Lines.BaseType = 23;//-1

                        //}

                        dts.Lines.SalesPersonCode = (int)slpcode; ; //销售员编号

                        //dts.Lines.UnitPrice = double.Parse(string.IsNullOrWhiteSpace(dln1.DiscountPrices.ToString()) ? "0" : dln1.DiscountPrices.ToString());            //单价

                        dts.Lines.Price = double.Parse(string.IsNullOrWhiteSpace(dln1.DiscountPrices.ToString()) ? "0" : dln1.DiscountPrices.ToString());

                        //dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(dln1.Discount.ToString()) ? 0.00 : double.Parse(dln1.Discount.ToString());//折扣率

                        //dts.Lines.LineTotal = string.IsNullOrWhiteSpace(dln1.TotalPrice.ToString()) ? 0.00 : double.Parse(dln1.TotalPrice.ToString());//总计

                        //dts.Lines.DiscountPercent = double.Parse(dln1.Discount == null ? "0" : (100 - dln1.Discount).ToString());     //折扣

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
                //用信号量代替锁
                await semaphoreSlim.WaitAsync();
                try
                {
                    //如果同步成功则修改SellOrder
                    UnitWork.Update<Quotation>(q => q.Id == quotation.Id, q => new Quotation
                    {
                        SalesOrderId = int.Parse(docNum)
                    });
                    UnitWork.Save();
                    var quotationObj = UnitWork.Find<Quotation>(q => q.Id == quotation.Id).FirstOrDefault();
                    if (string.IsNullOrWhiteSpace(quotationObj.SalesOrderId.ToString()))
                    {
                        Log.Logger.Error($"反写4.0失败，SAP_ID：{docNum}", typeof(SellOrderSapHandler));
                    }
                    else
                    {
                        Log.Logger.Warning($"反写4.0成功，SAP_ID：{quotationObj.SalesOrderId}", typeof(SellOrderSapHandler));
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"反写4.0失败，SAP_ID：{docNum}失败原因:{ex.Message}", typeof(SellOrderSapHandler));
                }
                finally
                {
                    semaphoreSlim.Release();
                    await HandleSellOrderERP(quotation.Id);
                }
                
                Log.Logger.Warning($"同步成功，SAP_ID：{docNum}", typeof(SellOrderSapHandler));
            }
            if (!string.IsNullOrWhiteSpace(allerror.ToString()))
            {
                Log.Logger.Error(allerror.ToString(), typeof(SellOrderSapHandler));
                throw new Exception(allerror.ToString());
            }

        }

        [CapSubscribe("Serve.SellOrder.ERPCreate")]
        public async Task HandleSellOrderERP(int theQuotationId)
        {
            string message = "";
            var quotation = await UnitWork.Find<Quotation>(q => q.Id.Equals(theQuotationId)).AsNoTracking()
              .Include(q => q.QuotationProducts).ThenInclude(q => q.QuotationMaterials).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
            var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(quotation.ServiceOrderId)).FirstOrDefaultAsync();
            var oCPR = await UnitWork.Find<OCPR>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId) && o.Active == "Y").FirstOrDefaultAsync();
            var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID.Equals(quotation.CreateUserId)).FirstOrDefaultAsync())?.NsapUserId;
            var slpcode = (await UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefaultAsync())?.sale_id;
            var ocrdObj = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId)).Select(o => new { o.SlpCode ,o.CardName}).FirstOrDefaultAsync();
            List<string> typeids = new List<string> { "SYS_MaterialInvoiceCategory", "SYS_MaterialTaxRate", "SYS_InvoiceCompany", "SYS_DeliveryMethod" };
            var categoryList = await UnitWork.Find<Category>(c => typeids.Contains(c.TypeId)).ToListAsync();
            var dbContext = UnitWork.GetDbContext<sale_ordr>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {

                    sale_ordr ordr = new sale_ordr()
                    {
                        sbo_id = Define.SBO_ID,
                        DocEntry = (int)quotation.SalesOrderId,
                        CardCode = serviceOrder.TerminalCustomerId, //客户id
                        CardName = ocrdObj.CardName,//客户名称
                        SlpCode = (short)slpcode, //销售人代码
                        CntctCode = int.Parse(string.IsNullOrWhiteSpace(oCPR.CntctCode.ToString()) ? "0" : oCPR.CntctCode.ToString()),//联系人
                        Comments = quotation.Remark, //备注
                        U_YWY = ocrdObj.SlpCode.ToString(),//业务员
                        U_FPLB = categoryList.Where(c => c.TypeId.Equals("SYS_MaterialInvoiceCategory") && c.DtValue.Equals(quotation.InvoiceCategory.ToString())).FirstOrDefault()?.Name,
                        //U_ShipName = categoryList.Where(c => c.TypeId.Equals("SYS_DeliveryMethod") && c.DtValue.Equals(quotation.DeliveryMethod.ToString())).FirstOrDefault()?.DtCode,
                        Address = quotation.CollectionAddress,
                        U_SL = categoryList.Where(c => c.TypeId.Equals("SYS_MaterialTaxRate") && c.DtValue.Equals(quotation.TaxRate.ToString())).FirstOrDefault()?.Name,
                        Address2 = quotation.ShippingAddress,
                        DocTotal = decimal.Parse(!string.IsNullOrWhiteSpace(quotation.TotalMoney.ToString()) ? quotation.TotalMoney.ToString() : "0.00"),
                        Indicator = categoryList.Where(c => c.TypeId.Equals("SYS_InvoiceCompany") && c.DtValue.Equals(quotation.InvoiceCompany.ToString())).FirstOrDefault()?.DtCode,
                        DocDate = DateTime.Now,
                        DocDueDate = Convert.ToDateTime(quotation.DeliveryDate),
                        DocType = "I",
                        PartSupply = "Y",
                        Printed = "N",
                        DocStatus = "O",
                        Handwrtten = "N",
                        Ref1 = quotation.SalesOrderId.ToString(),
                        GroupNum = short.Parse(categoryList.Where(c => c.TypeId.Equals("SYS_DeliveryMethod") && c.DtValue.Equals(quotation.DeliveryMethod.ToString())).FirstOrDefault()?.DtCode),
                        TaxDate = DateTime.Now,
                        DocRate = 1,//后期增加货币需要更改
                        DocCur = "RMB",//后期增加货币需要更改
                        DocNum = (uint)quotation.SalesOrderId,
                        UpdateDate = DateTime.Now,
                        CANCELED = "N",
                        U_ERPFrom = "4"
                    };
                    ordr = await UnitWork.AddAsync<sale_ordr, int>(ordr);
                    var lineNums = await UnitWork.Find<RDR1>(o => o.DocEntry == quotation.SalesOrderId).Select(o => new { o.LineNum, o.ItemCode, o.Price }).ToListAsync();
                    foreach (QuotationMergeMaterial dln1 in quotation.QuotationMergeMaterials)
                    {
                        var lineNum = 0;
                        if (lineNums.Where(o => o.ItemCode.Equals(dln1.MaterialCode)).Count() > 1)
                        {
                            if (lineNums.Where(o => o.ItemCode.Equals(dln1.MaterialCode) && o.Price == dln1.DiscountPrices).Count() <= 0)
                            {
                                lineNum = (int)lineNums.Where(o => o.ItemCode.Equals(dln1.MaterialCode) && o.Price == decimal.Parse(Convert.ToDecimal(dln1.DiscountPrices).ToString("#0.00"))).FirstOrDefault()?.LineNum;
                            }
                            else
                            {
                                lineNum = (int)lineNums.Where(o => o.ItemCode.Equals(dln1.MaterialCode) && o.Price == dln1.DiscountPrices).FirstOrDefault()?.LineNum;
                            }
                        }
                        else
                        {
                            lineNum = (int)lineNums.Where(o => o.ItemCode.Equals(dln1.MaterialCode)).FirstOrDefault()?.LineNum;
                        }

                        await UnitWork.AddAsync<sale_rdr1, int>(new sale_rdr1
                        {
                            sbo_id = Define.SBO_ID,
                            DocEntry = (int)quotation.SalesOrderId,
                            LineNum = lineNum,
                            ItemCode = dln1.MaterialCode,
                            LineStatus = "O",
                            Rate = 1,//后期增加货币需要更改
                            Currency = "RMB",
                            PriceBefDi = dln1.DiscountPrices,
                            StockPrice = dln1.CostPrice,
                            OpenQty = dln1.Count,
                            OpenSum = dln1.DiscountPrices * dln1.Count,
                            AcctCode = "600101",
                            OrderedQty = dln1.Count,
                            PackQty = dln1.Count,
                            OpenCreQty = dln1.Count,
                            BaseCard = serviceOrder.TerminalCustomerId,
                            StockValue = dln1.CostPrice * dln1.Count,
                            GTotal = dln1.DiscountPrices * dln1.Count,
                            LineVat = 0,
                            VisOrder = (int)lineNums.Where(o => o.ItemCode.Equals(dln1.MaterialCode)).FirstOrDefault()?.LineNum,
                            GrssProfit = (dln1.DiscountPrices - dln1.CostPrice) * dln1.Count,
                            ObjType = "17",
                            Dscription = dln1.MaterialDescription,
                            Quantity = dln1.Count,
                            Price = dln1.DiscountPrices,
                            LineTotal = dln1.DiscountPrices * dln1.Count,
                            WhsCode = dln1.WhsCode,
                            DocDate = DateTime.Now,
                            unitMsr = dln1.Unit
                        });
                    }
                    var itemcodes = quotation.QuotationMergeMaterials.Select(q => q.MaterialCode).ToList();
                    var WhsCodes = quotation.QuotationMergeMaterials.Select(q => q.WhsCode).Distinct().ToList();
                    var oitwList = await UnitWork.Find<OITW>(o => itemcodes.Contains(o.ItemCode) && WhsCodes.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.IsCommited, o.OnHand, o.OnOrder }).ToListAsync();
                    foreach (var item in oitwList)
                    {
                        var WhsCode = quotation.QuotationMergeMaterials.Where(q => q.MaterialCode.Equals(item.ItemCode)).FirstOrDefault().WhsCode;
                        await UnitWork.UpdateAsync<store_oitw>(o => o.sbo_id == Define.SBO_ID && o.ItemCode == item.ItemCode && o.WhsCode == WhsCode, o => new store_oitw
                        {
                            OnHand = item.OnHand,
                            IsCommited = item.IsCommited,
                            OnOrder = item.OnOrder
                        });
                    }
                    var oitmList = await UnitWork.Find<OITM>(o => itemcodes.Contains(o.ItemCode)).Select(o => new { o.ItemCode, o.IsCommited, o.OnHand, o.OnOrder, o.LastPurCur, o.LastPurPrc, o.LastPurDat, o.UpdateDate }).ToListAsync();
                    foreach (var item in oitmList)
                    {
                        await UnitWork.UpdateAsync<store_oitm>(o => o.sbo_id == Define.SBO_ID && o.ItemCode == item.ItemCode, o => new store_oitm
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
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    Log.Logger.Debug($"同步3.0成功，SAP_ID：{quotation.SalesOrderId}", typeof(SellOrderSapHandler));
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    message = $"同步3.0失败，SAP_ID：{quotation.SalesOrderId}" + ex.Message;
                    Log.Logger.Error($"同步3.0失败，SAP_ID：{quotation.SalesOrderId}" + ex.Message, typeof(SellOrderSapHandler));
                }
            }
            if (message != "")
            {
                throw new Exception(message.ToString());
            }
        }

        [CapSubscribe("Serve.SellOrder.Cancel")]
        public async Task HandleCancelSellOrder(int theQuotationId)
        {
            string message = "", eMesg = "";
            int eCode = 0;
            var quotation = await UnitWork.Find<Quotation>(q => q.Id.Equals(theQuotationId)).AsNoTracking().FirstOrDefaultAsync();
            SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oOrders);

            dts.GetByKey(Convert.ToInt32(quotation.SalesOrderId));
            var res = dts.Cancel();
            if (res != 0)
            {
                company.GetLastError(out eCode, out eMesg);
                message += string.Format("取消销售订单:([单号{2}])时调接口发生异常[异常代码:{0},异常信息:{1}]", eCode, eMesg, quotation.SalesOrderId);
                Log.Logger.Error(message.ToString(), typeof(SellOrderSapHandler));
                throw new Exception(message);
            }
            else
            {
                //如果同步成功则修改SellOrder
                await UnitWork.UpdateAsync<Quotation>(q => q.Id.Equals(quotation.Id), q => new Quotation
                {
                    QuotationStatus = -1,
                    UpDateTime = DateTime.Now
                });
                await UnitWork.SaveAsync();
                Log.Logger.Warning($"取消成功，SAP_ID：{quotation.SalesOrderId}", typeof(SellOrderSapHandler));
            }

        }

    }
}
