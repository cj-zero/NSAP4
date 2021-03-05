﻿using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
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
            string eMesg;
            string docNum = string.Empty;
            try
            {
                if (obj.QuotationMergeMaterialReqs != null)
                {
                    var quotation = await UnitWork.Find<Quotation>(q => q.Id.Equals(obj.QuotationMergeMaterialReqs.FirstOrDefault().QuotationId)).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
                    var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(quotation.ServiceOrderId)).FirstOrDefaultAsync();
                    SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                    var oCPR = await UnitWork.Find<OCPR>(o => o.CardCode.Equals(serviceOrder.TerminalCustomerId) && o.Name.Equals(serviceOrder.NewestContacter)).FirstOrDefaultAsync();
                    #region [添加主表信息]

                    //DataTable dtRowsConn = AidTool.GetConnection(model.SboId);

                    //if (dtRowsConn.Rows.Count > 0)

                    //{

                    //    sboname = dtRowsConn.Rows[0][0].ToString();

                    //    sqlconn = dtRowsConn.Rows[0][1].ToString();

                    //}

                    dts.CardCode = serviceOrder.TerminalCustomerId;

                    dts.DocDate = quotation.CreateTime;

                    dts.Comments = quotation.Remark;

                    dts.ContactPersonCode = int.Parse(string.IsNullOrWhiteSpace(oCPR.CntctCode.ToString()) ? "0" : oCPR.CntctCode.ToString()); ;

                    //dts.SalesPersonCode = int.Parse(model.SlpCode == "" ? "-1" : model.SlpCode);

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

                    //dts.Indicator = model.Indicator;    // 标识

                    //dts.PaymentMethod = model.PeyMethod;    //付款方式

                    //dts.FederalTaxID = model.LicTradNum;  //国税编号

                    dts.Project = "";

                    //errorMsg += string.Format("调用接口添加销售交货主数据[{0}]", jobID);

                    #endregion



                    #region [添加行明细]
                    int num = 0;
                    foreach (var dln1 in obj.QuotationMergeMaterialReqs)
                    {
                        var materials = quotation.QuotationMergeMaterials.Where(q => q.Id.Equals(dln1.Id)).FirstOrDefault();
                        dts.Lines.ItemCode = materials.MaterialCode.Replace("&#92;", "■");

                        dts.Lines.ItemDescription = materials.MaterialDescription;

                        dts.Lines.Quantity = string.IsNullOrEmpty(dln1.SentQuantity.ToString()) ? '1' : double.Parse(dln1.SentQuantity.ToString());

                        dts.Lines.WarehouseCode = "37";

                        dts.Lines.BaseEntry = (int)quotation?.SalesOrderId;

                        dts.Lines.BaseLine = ++num;

                        dts.Lines.BaseType = 17;

                        //dts.Lines.SalesPersonCode = int.Parse(model.SlpCode == "" ? "-1" : model.SlpCode);

                        dts.Lines.UnitPrice = string.IsNullOrWhiteSpace(materials.SalesPrice.ToString()) ? 0 : Convert.ToDouble(materials.SalesPrice);            //单价

                        dts.Lines.DiscountPercent = string.IsNullOrWhiteSpace(materials.Discount.ToString()) ? 0 : Convert.ToDouble(materials.Discount);     //折扣

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
                        Log.Logger.Warning("添加销售交货到SAP成功");
                    }
                }
                else 
                {
                    Log.Logger.Warning("添加销售交货到SAP时异常！错误代码：出库量为零");
                }
                

            }

            catch (Exception e)
            {
                Log.Logger.Warning("调用接口添加销售交货时异常：" + e.ToString() + "");
            }

        }

    }
}