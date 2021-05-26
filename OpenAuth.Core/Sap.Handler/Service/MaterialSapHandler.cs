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
using System.Threading.Tasks;

namespace Sap.Handler.Service
{
    public class MaterialSapHandler : ICapSubscribe
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;

        public MaterialSapHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }

        [CapSubscribe("Serve.SalesOfDelivery.Create")]
        public async Task SalesOfDelivery(AddOrUpdateQuotationReq obj)
        {
            int eCode;
            string eMesg = "";
            string docNum = string.Empty;
            try
            {
                if (obj.QuotationMergeMaterialReqs != null)
                {
                    var quotation = await UnitWork.Find<Quotation>(q => q.Id.Equals(obj.QuotationMergeMaterialReqs.FirstOrDefault().QuotationId)).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
                    var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(quotation.ServiceOrderId)).FirstOrDefaultAsync();
                    SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                    var oCPR = await UnitWork.Find<OCPR>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId) && o.Active == "Y").FirstOrDefaultAsync();
                    var slpcode = (await UnitWork.Find<OSLP>(o => o.SlpName.Equals(quotation.CreateUser)).FirstOrDefaultAsync())?.SlpCode;
                    var ordr = await UnitWork.Find<RDR1>(o => o.DocEntry.Equals(quotation.SalesOrderId)).Select(o => new { o.LineNum, o.ItemCode,o.Price }).ToListAsync();
                    var ywy = await UnitWork.Find<OCRD>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId)).Select(o => o.SlpCode).FirstOrDefaultAsync();
                    List<string> typeids = new List<string> { "SYS_MaterialInvoiceCategory", "SYS_MaterialTaxRate", "SYS_InvoiceCompany", "SYS_DeliveryMethod" };
                    var categoryList = await UnitWork.Find<Category>(c => typeids.Contains(c.TypeId)).ToListAsync();
                    #region [添加主表信息]

                    //DataTable dtRowsConn = AidTool.GetConnection(model.SboId);

                    //if (dtRowsConn.Rows.Count > 0)

                    //{

                    //    sboname = dtRowsConn.Rows[0][0].ToString();

                    //    sqlconn = dtRowsConn.Rows[0][1].ToString();

                    //}

                    dts.CardCode = serviceOrder.TerminalCustomerId;

                    dts.DocDate = DateTime.Now;
                    dts.TaxDate= DateTime.Now;

                    dts.Comments = quotation.Remark +"基于销售订单"+ quotation.SalesOrderId;

                    dts.ContactPersonCode = int.Parse(string.IsNullOrWhiteSpace(oCPR.CntctCode.ToString()) ? "0" : oCPR.CntctCode.ToString()); ;

                    dts.SalesPersonCode = (int)slpcode;

                    //dts.NumAtCard = model.NumAtCard;

                    //dts.PayToCode = model.PayToCode;

                    //dts.ShipToCode = model.ShipToCode;

                    //dts.DocCurrency = model.DocCur;

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
                        dts.Lines.ItemCode = materials.MaterialCode.Replace("&#92;", "■");

                        dts.Lines.ItemDescription = materials.MaterialDescription;

                        dts.Lines.Quantity = string.IsNullOrEmpty(dln1.SentQuantity.ToString()) ? '1' : double.Parse(dln1.SentQuantity.ToString());

                        dts.Lines.WarehouseCode = materials.WhsCode;

                        dts.Lines.BaseEntry = (int)quotation?.SalesOrderId;

                        dts.Lines.BaseLine = (int)ordr.Where(o=>o.ItemCode.Equals(materials.MaterialCode) && o.Price==materials.DiscountPrices).FirstOrDefault()?.LineNum;

                        dts.Lines.BaseType = 17;

                        dts.Lines.SalesPersonCode = (int)slpcode;

                        //dts.Lines.UnitPrice = string.IsNullOrWhiteSpace(materials.DiscountPrices.ToString()) ? 0 : Convert.ToDouble(materials.DiscountPrices);            //单价

                        dts.Lines.Price = double.Parse(string.IsNullOrWhiteSpace(materials.DiscountPrices.ToString()) ? "0" : materials.DiscountPrices.ToString());

                        //dts.Lines.LineTotal = double.Parse(Convert.ToDecimal(materials.DiscountPrices * dln1.SentQuantity).ToString("#0.00"));//string.IsNullOrWhiteSpace(materials.TotalPrice.ToString()) ? 0.00 : double.Parse(materials.TotalPrice.ToString());//总计
                        TotalMoney +=materials.MaterialType==1?Convert.ToDecimal(Convert.ToDecimal(materials.DiscountPrices * dln1.SentQuantity).ToString("#0.00")):0;
                        //dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(materials.Discount.ToString()) ? 0 : Convert.ToDouble(materials.Discount);     //折扣

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
                        Log.Logger.Warning("添加销售交货到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                    }
                    else
                    {
                        company.GetNewObjectCode(out docNum);
                        Log.Logger.Warning("添加销售交货到SAP成功");
                        if (TotalMoney > 0) 
                        {
                            var userCount = await UnitWork.Find<amountinarear>(a => a.UserId.Equals(quotation.CreateUserId)).FirstOrDefaultAsync();
                            if (userCount != null)
                            {
                                await UnitWork.UpdateAsync<amountinarear>(a => a.Id.Equals(userCount.Id), a => new amountinarear { Money = a.Money + TotalMoney });

                            }
                            else
                            {
                                userCount = await UnitWork.AddAsync<amountinarear>(new amountinarear { Money = TotalMoney, Id = Guid.NewGuid().ToString(), UserId = quotation.CreateUserId, UserName = quotation.CreateUser });
                            }
                            await UnitWork.AddAsync<amountinarearlog>(new amountinarearlog { Id = Guid.NewGuid().ToString(), Money = TotalMoney, PlusOrMinus = true, SalesOrderId = quotation.SalesOrderId, CreateTime = DateTime.Now, CreateUserId = quotation.CreateUserId, CreateUserName = quotation.CreateUser, AmountInArearId = userCount.Id, Liaison = int.Parse(docNum), Remark = "销售交货增加挂账金额" });
                        }
                    }
                }
                else 
                {
                    eMesg = "添加销售交货到SAP时异常！错误代码：出库量为零";
                    Log.Logger.Warning("添加销售交货到SAP时异常！错误代码：出库量为零");
                }
            }
            catch (Exception e)
            {
                eMesg = "调用接口添加销售交货时异常：" + e.ToString() + "";
                Log.Logger.Warning("调用接口添加销售交货时异常：" + e.ToString() + "");
            }
            if (!string.IsNullOrWhiteSpace(eMesg)) 
            {
                throw new Exception(eMesg.ToString());
            }
            try
            {
                await HandleSalesOfDeliveryERP(int.Parse(docNum));
            }
            catch (Exception e)
            {

                Log.Logger.Warning(e.Message);
            }
            
        }

        [CapSubscribe("Serve.SalesOfDelivery.ERPCreate")]
        public async Task HandleSalesOfDeliveryERP(int? docNum)
        {
            string Message = "";
            try
            {
                var ODLNmodel = await UnitWork.Find<ODLN>(o => o.DocEntry == docNum).ToListAsync();
                var DLN1model = await UnitWork.Find<DLN1>(o => o.DocEntry == docNum).ToListAsync();
                foreach (var item in ODLNmodel)
                {
                    if (item.PartSupply == "true") { item.PartSupply = "Y"; } else { item.PartSupply = "N"; }

                    if (item.DocCur == "") { item.DocCur = "RMB"; }

                    //if (dtRowODLN.Rows[0][3].ToString() == "") { model.DocRate = 1; }

                    if (item.DocTotal == null) { item.DocTotal = 0; }

                    if (item.OwnerCode == null) { item.OwnerCode = -1; }

                    if (item.SlpCode == null) { item.SlpCode = -1; }
                }


                List<sale_odln> sale_Odln = ODLNmodel.MapToList<sale_odln>();
                List<sale_dln1> sale_Dln1 = DLN1model.MapToList<sale_dln1>();
                sale_Odln.ForEach(s => s.sbo_id = Define.SBO_ID);
                sale_Dln1.ForEach(s => s.sbo_id = Define.SBO_ID);
                await UnitWork.BatchAddAsync<sale_odln, int>(sale_Odln.ToArray());
                await UnitWork.BatchAddAsync<sale_dln1, int>(sale_Dln1.ToArray());

                List<string> itemcodes = sale_Dln1.Select(s => s.ItemCode).ToList();
                List<string> WhsCodes = sale_Dln1.Select(s => s.WhsCode).ToList();
                var oitwList = await UnitWork.Find<OITW>(o => itemcodes.Contains(o.ItemCode) && WhsCodes.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.IsCommited, o.OnHand, o.OnOrder }).ToListAsync();
                foreach (var item in oitwList)
                {
                    var WhsCode = sale_Dln1.Where(q => q.ItemCode.Equals(item.ItemCode)).FirstOrDefault().WhsCode;
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
                Log.Logger.Warning("添加销售交货到erp3.0成功");
            }
            catch (Exception e)
            {
                Message = $"添加销售交货:{docNum}到erp3.0时异常！错误信息：" + e.Message;
                Log.Logger.Warning($"添加销售交货:{docNum}到erp3.0时异常！错误信息：" + e.Message);
            }
            if (Message != "") 
            {
                throw new Exception(Message.ToString());
            }
        }
    }
}
