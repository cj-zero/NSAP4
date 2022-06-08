using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using DotNetCore.CAP;
using OpenAuth.Repository.Interface;
using SAPbobsCOM;
using Sap.Handler.Service.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using Serilog;
using Infrastructure;
using Infrastructure.Extensions;
using Sap.Handler.Sap;
using Microsoft.EntityFrameworkCore;

namespace Sap.Handler.Service
{
    public class ProductReceiptSapHandler
    {
        private readonly IUnitWork UnitWork;
        private readonly Company company;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        public ProductReceiptSapHandler(IUnitWork unitWork, Company company)
        {
            UnitWork = unitWork;
            this.company = company;
        }

        public async Task ProductReceiptHandle(AddOrUpdProductReceiptReq obj)
        {
            int eCode;
            string eMesg = "";
            string docNum = string.Empty;
            try
            {
                if (obj.ProductReceiptDetailReqs != null)//存在物料明细
                {
                    //var quotation = await UnitWork.Find<Quotation>(q => q.Id.Equals(obj.QuotationMergeMaterialReqs.FirstOrDefault().QuotationId)).Include(q => q.QuotationMergeMaterials).FirstOrDefaultAsync();
                    //var serviceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(quotation.ServiceOrderId)).FirstOrDefaultAsync();

                    SAPbobsCOM.Documents dts = (SAPbobsCOM.Documents)company.GetBusinessObject(SAPbobsCOM.BoObjectTypes.oDeliveryNotes);
                    #region [添加主表信息]
                    dts.DocDate = Convert.ToDateTime(obj.DocDate.Value);
                    dts.Comments = obj.Comments.ToString();
                    dts.JournalMemo = obj.JrnlMemo.ToString();
                    //扩展信息
                    if (!string.IsNullOrEmpty(obj.U_CPH))
                    {
                        dts.UserFields.Fields.Item("U_CPH").Value = obj.U_CPH;
                    }
                    if (!string.IsNullOrEmpty(obj.U_YSQX))
                    {
                        dts.UserFields.Fields.Item("U_YSQX").Value = obj.U_YSQX;
                    }
                    if (!string.IsNullOrEmpty(obj.U_ShipName))
                    {
                        dts.UserFields.Fields.Item("U_ShipName").Value = obj.U_ShipName;
                    }
                    if (!string.IsNullOrEmpty(obj.U_SMAZ))
                    {
                        dts.UserFields.Fields.Item("U_SMAZ").Value = obj.U_SMAZ;
                    }
                    //系统操作者
                    if (!string.IsNullOrEmpty(obj.U_YGMD) && obj.U_YGMD != "")
                    {
                        dts.UserFields.Fields.Item("U_YGMD").Value = obj.U_YGMD;
                    }
                    //领退料部门
                    if (!string.IsNullOrEmpty(obj.U_PRX_TkNo) && obj.U_PRX_TkNo != "")
                    {
                        dts.UserFields.Fields.Item("U_PRX_TkNo").Value = obj.U_PRX_TkNo;
                    }
                    #endregion

                    #region [添加行明细]
                    decimal TotalMoney = 0;
                    foreach (var ign1 in obj.ProductReceiptDetailReqs)
                    {
                        dts.Lines.BaseEntry = ign1.BaseEntry==null? -1:ign1.BaseEntry.Value;
                        dts.Lines.BaseLine = ign1.LineNum;
                        dts.Lines.ItemDescription = ign1.Dscription;
                        dts.Lines.Quantity = double.Parse(ign1.Quantity.Value.ToString());
                        if (ign1.Price>0)
                        {
                            dts.Lines.Price = double.Parse(ign1.Price.ToString());
                        }
                        dts.Lines.WarehouseCode = ign1.WhsCode;
                        dts.Lines.Add();
                    }

                    #endregion
                    var res = dts.Add();
                    if (res != 0)
                    {
                        company.GetLastError(out eCode, out eMesg);
                        Log.Logger.Warning("添加生产收货到SAP时异常！错误代码：" + eCode + "错误信息：" + eMesg);
                    }
                    else
                    {
                        company.GetNewObjectCode(out docNum);
                    }
                }
                else
                {
                    eMesg = "添加生产收货到SAP时异常！错误代码：入库量为零";
                    Log.Logger.Warning("添加生产收货到SAP时异常！错误代码：入库量为零");
                }
            }
            catch (Exception e)
            {
                eMesg = "调用接口添加生产收货时异常：" + e.ToString() + "";
                Log.Logger.Warning("调用接口添加生产收货时异常：" + e.ToString() + "");
            }
            if (!string.IsNullOrWhiteSpace(eMesg))
            {
                throw new Exception(eMesg.ToString());
            }
            //用信号量代替锁
            await semaphoreSlim.WaitAsync();
            try
            {
                await ProductReceiptHandleERP3(int.Parse(docNum), obj);
            }
            catch (Exception e)
            {

                Log.Logger.Warning(e.Message);
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task ProductReceiptHandleERP3(int? docNum,AddOrUpdProductReceiptReq obj)
        {
           string eMsg = string.Empty;

            var OIGNmodel = UnitWork.Find<OIGN>(o => o.DocEntry == docNum).FirstOrDefault();
            var IGN1model = UnitWork.Find<IGN1>(o => o.DocEntry == docNum).ToList();

            #region 添加主数据,行明细
            product_oign theoign=OIGNmodel.MapTo<product_oign>();
            List<product_ign1> theignList = IGN1model.MapToList<product_ign1>();
            theoign.sbo_id = Define.SBO_ID;
            theignList.ForEach(s => s.sbo_id = Define.SBO_ID);
            await UnitWork.AddAsync<product_oign, int>(theoign);
            await UnitWork.BatchAddAsync<product_ign1, int>(theignList.ToArray());

            #endregion

            #region 修改库存量
            List<string> itemcodes = theignList.Select(s => s.ItemCode).ToList();
            List<string> WhsCodes = theignList.Select(s => s.WhsCode).ToList();
            var oitwList = await UnitWork.Find<OITW>(o => itemcodes.Contains(o.ItemCode) && WhsCodes.Contains(o.WhsCode)).Select(o => new { o.ItemCode, o.IsCommited, o.OnHand, o.OnOrder }).ToListAsync();
            foreach (var item in oitwList)
            {
                var WhsCode = theignList.Where(q => q.ItemCode.Equals(item.ItemCode)).FirstOrDefault().WhsCode;
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
            #endregion

            #region OITL
            var oitlModel = await UnitWork.Find<OITL>(o => o.DocType == 59 && o.DocEntry == docNum).ToListAsync();
            List<store_oitl> theoitl = oitlModel.MapToList<store_oitl>();
            theoitl.ForEach(s => s.sbo_id = Define.SBO_ID);
            await UnitWork.BatchAddAsync<store_oitl, int>(theoitl.ToArray());
            var logentrys= oitlModel.Select(l => l.LogEntry).ToList();
            var itl1Model = await UnitWork.Find<ITL1>(o => logentrys.Contains(o.LogEntry)).ToListAsync();
            List<store_itl1> theitl1 = itl1Model.MapToList<store_itl1>();
            theitl1.ForEach(s => s.sbo_id = Define.SBO_ID);
            await UnitWork.BatchAddAsync<store_itl1, int>(theitl1.ToArray());
            #endregion

            #region 修改生产订单
            var oworEntrys = IGN1model.Select(s => s.BaseEntry).ToList();
            var oworModel = await UnitWork.Find<OWOR>(o => oworEntrys.Contains(o.DocEntry)).Select(o=>new {o.DocEntry,o.Status,o.Type,o.CmpltQty,o.RjctQty }).ToListAsync();
            foreach(var item in oworModel)
            {
                await UnitWork.UpdateAsync<product_owor>(o => o.sbo_id == Define.SBO_ID && o.DocEntry == item.DocEntry, o => new product_owor
                {
                    Status = item.Status,
                    Type = item.Type,
                    CmpltQty = item.CmpltQty,
                    RjctQty = item.RjctQty
                });
                //生产明细
                var wor1Model=await UnitWork.Find<WOR1>(o => o.DocEntry==item.DocEntry).Select(o=> new {o.DocEntry,o.ItemCode,o.BaseQty,o.PlannedQty,o.IssuedQty }).ToListAsync();
                foreach(var item1 in wor1Model)
                {
                    await UnitWork.UpdateAsync<product_wor1>(o => o.sbo_id == Define.SBO_ID && o.DocEntry == item1.DocEntry && o.ItemCode == item1.ItemCode, o => new product_wor1
                    {
                        BaseQty = item1.BaseQty,
                        PlannedQty = item1.PlannedQty,
                        IssuedQty = item1.IssuedQty
                    });
                }
            }
            await UnitWork.SaveAsync();
            Log.Logger.Warning("添加生产收货到erp3.0成功");
            #endregion

        }
    }
}
