using System;
using System.Linq;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System.Threading.Tasks;
using OpenAuth.Repository.Domain.Sap;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Extensions;

namespace OpenAuth.App
{
    public class WfaEshopStatusApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        public enum CurStatus
        {
            Submit = 0,
            PendingShip = 1,
            Shipping = 2,
            Complete = 3
        }

        public WfaEshopStatusApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }

        /// <summary>
        /// 根据商城注册电话查对应订单状态列表
        /// </summary>
        /// <param name="RegisterMobile"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetOrderStatusByRegMobile(QryWfaEshopStatusListReq request)
        {
            var result = new TableData();
            //如果是客户类型则查该客户的列表
            if (request.QryType.ToString() == "1")
            {
                //查是否为客户
                var objclient = from a in UnitWork.Find<OCRD>(null)
                                join b in UnitWork.Find<OCPR>(null) on new { a.CardCode, a.CntctPrsn } equals new { b.CardCode, CntctPrsn = b.Name } into ab
                                from b in ab.DefaultIfEmpty()
                                select new { a, b };
                string cardcode = await objclient.Where(o => o.b.Cellolar.Equals(request.QryMobile) || o.b.Tel1.Equals(request.QryMobile) || o.b.Tel2.Equals(request.QryMobile)).Select(s => s.a.CardCode).FirstOrDefaultAsync();

                var qrystatus = UnitWork.Find<wfa_eshop_status>(o => o.card_code.Equals(cardcode)).Include(s => s.wfa_eshop_oqutdetails)
                    .WhereIf(!string.IsNullOrEmpty(request.QryStatus.ToString()), q => q.cur_status.Equals(request.QryStatus))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.key), q => q.card_code.Equals(request.key) || q.card_name.Contains(request.key) || q.job_id.ToString().Equals(request.key)
                    || q.order_entry.ToString().Equals(request.key) || q.quotation_entry.ToString().Equals(request.key));
                var resultStatus = qrystatus.OrderByDescending(o => o.first_createdate).Select(q => new
                {
                    q.job_id,
                    q.card_code,
                    q.card_name,
                    q.cur_status,
                    q.document_id,
                    q.wfa_eshop_oqutdetails
                });
                result.Data = await resultStatus.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
                result.Count = qrystatus.Count();
                return result;
            }
            else
            {
                //如果不是客户，则查对应业务员
                var objuser = from a in UnitWork.Find<sbo_user>(null)
                              join b in UnitWork.Find<base_user>(null) on a.user_id equals b.user_id into ab
                              from b in ab.DefaultIfEmpty()
                              select new { a, b };
                var saleId = await objuser.Where(o => o.a.sale_id > 0 && (o.b.mobile).ToString().Equals(request.QryMobile)).Select(s => s.a.sale_id).FirstOrDefaultAsync();
                if (saleId > 0)
                {
                    var qrystatus = UnitWork.Find<wfa_eshop_status>(o => o.slp_code.Equals(Convert.ToInt32(saleId))).Include(s => s.wfa_eshop_oqutdetails)
                     .WhereIf(!string.IsNullOrEmpty(request.QryStatus.ToString()), q => q.cur_status.Equals(request.QryStatus))
                     .WhereIf(!string.IsNullOrWhiteSpace(request.key), q => q.card_code.Equals(request.key) || q.card_name.Contains(request.key) || q.job_id.ToString().Equals(request.key)
                    || q.order_entry.ToString().Equals(request.key) || q.quotation_entry.ToString().Equals(request.key));
                    var resultStatus = qrystatus.OrderByDescending(o => o.first_createdate).Select(q => new
                    {
                        q.job_id,
                        q.card_code,
                        q.card_name,
                        q.cur_status,
                        q.document_id,
                        q.wfa_eshop_oqutdetails
                    });
                    result.Data = await resultStatus.Skip((request.page - 1) * request.limit)
                    .Take(request.limit).ToListAsync();
                    result.Count = qrystatus.Count();
                }
                return result;
            }
        }

        /// <summary>
        /// 根据主键获取相应进度明细信息
        /// </summary>
        /// <param name="documentId"></param>
        /// <returns></returns>
        public async Task<WfaEshopStatusResp> GetStatusInfoById(int documentId)
        {
            var obj = await UnitWork.Find<wfa_eshop_status>(o => o.document_id.Equals(documentId)).Include(s => s.wfa_eshop_oqutdetails)
                .Include(d => d.wfa_eshop_canceledstatuss).FirstOrDefaultAsync();
            var result = obj.MapTo<WfaEshopStatusResp>();

            //已发货,已结束状态赋上物流信息
            if (obj.cur_status == (int)CurStatus.Shipping || obj.cur_status == (int)CurStatus.Complete)
            {
                var logsql = from a in UnitWork.Find<buy_opor>(null)
                             join b in UnitWork.Find<sale_transport>(null) on new { a.sbo_id, a.DocEntry } equals new { sbo_id = b.SboId, DocEntry = b.Buy_DocEntry }
                             join c in UnitWork.Find<sale_dln1>(null) on new { b.SboId, b.Base_DocEntry } equals new { SboId = c.sbo_id, Base_DocEntry = c.DocEntry }
                             where b.Base_DocType == 24 && c.BaseType == 17 && a.CANCELED == "N" && c.BaseEntry.Equals(obj.order_entry)
                             select new { a, b, c };
                var logobj = await logsql.Select(o => new { CoName = o.a.CardName, ExpNum = o.a.LicTradNum, CreateDate = o.a.CreateDate }).Distinct().ToListAsync();
                result.wfa_eshop_Logs = logobj.MapToList<LogInfo>();
            }
            return result;
        }

        /// <summary>
        /// 根据客户编码取得对应业务员手机号
        /// </summary>
        /// <param name="cardCode"></param>
        /// <param name="sboId"></param>
        /// <returns></returns>
        public async Task<SlpInfoOfClientResp> GetSalesPersonTelByCardCode(string cardCode)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //业务员Id
            var objslpstr = from a in UnitWork.Find<crm_ocrd>(null)
                            join b in UnitWork.Find<crm_oslp>(null) on new { a.sbo_id, a.SlpCode } equals new { sbo_id = b.sbo_id, SlpCode = b.SlpCode }
                            where a.sbo_id.Equals(1) && a.CardCode.Equals(cardCode)
                            select new { a, b };
            var objslp = await objslpstr.Select(o => new { o.a.SlpCode, o.b.SlpName, o.a.sbo_id }).FirstOrDefaultAsync();
            if (objslp != null && objslp.SlpCode > 0)
            {
                var objuser = from a in UnitWork.Find<sbo_user>(null)
                              join b in UnitWork.Find<base_user>(null) on a.user_id equals b.user_id into ab
                              from b in ab.DefaultIfEmpty()
                              join c in UnitWork.Find<base_user_detail>(null) on b.user_id equals c.user_id into bc
                              from c in bc.DefaultIfEmpty()
                              select new { a, b, c };
                var telobj = await objuser.Where(o => o.a.sbo_id == objslp.sbo_id && o.a.sale_id == objslp.SlpCode && o.b.user_nm.Equals(objslp.SlpName))
                    .Select(q => new
                    {
                        UserName = q.b.user_nm,
                        Sex = q.c.sex == 1 ? "女" : "男",
                        Email = q.b.email,
                        Mobile = q.b.mobile
                    }).FirstOrDefaultAsync();
                var result = telobj.MapTo<SlpInfoOfClientResp>();
                return result;
            }
            else
            {
                return new SlpInfoOfClientResp();
            }
        }
        /// <summary>
        /// 通知物流已签收状态
        /// </summary>
        /// <param name="EshopPOrderNo">商城订单号</param>
        /// <returns></returns>
        public async Task<TableData> UpdateShipStatusForOrder(string EshopPOrderNo)
        {
            var result = new TableData();
            var thisorder = await UnitWork.Find<sale_ordr>(w => w.U_EshopNo == EshopPOrderNo && w.sbo_id.Equals(1) && w.CANCELED=="N").FirstOrDefaultAsync();
            if (thisorder != null && thisorder.DocEntry > 0)
            {
                await UnitWork.UpdateAsync<sale_ordr>(w => w.DocEntry == thisorder.DocEntry && w.sbo_id == thisorder.sbo_id && w.CANCELED == "N", u => new sale_ordr { U_ShipStatus = 1 });
                await UnitWork.SaveAsync();
                result.Message = "更新成功";
            }
            else
            {
                result.Code = 400;
                result.Message = "没有找到相关记录";
            }
            return result;
        }
        /// <summary>
        /// 保存商城订单传来的开票信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpdateEshopBillInfo(AddOrUpdatesale_ordr_pluginReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            if (!string.IsNullOrEmpty(req.EshopOrderNo))
            {
                var thisorder = await UnitWork.Find<sale_ordr>(w => w.U_EshopNo == req.EshopOrderNo && w.sbo_id.Equals(1) && w.CANCELED == "N").FirstOrDefaultAsync();
                if (thisorder != null && thisorder.DocEntry > 0)
                {
                    var newobj = new sale_ordr_plugin();
                    var theorderplugin = await UnitWork.Find<sale_ordr_plugin>(w => w.DocEntry == thisorder.DocEntry && w.sbo_id == thisorder.sbo_id).FirstOrDefaultAsync();
                    if (theorderplugin != null && theorderplugin.DocEntry > 0)
                    {
                        await UnitWork.UpdateAsync<sale_ordr_plugin>(w => w.DocEntry == thisorder.DocEntry && w.sbo_id == thisorder.sbo_id, u => new sale_ordr_plugin
                        {
                            InvoiceTitle = req.InvoiceTitle,
                            InvoiceName = req.InvoiceName,
                            InvoiceTaxSignNo = req.InvoiceTaxSignNo,
                            InvoiceReceivePhone = req.InvoiceReceivePhone,
                            InvoiceReceiveEmail = req.InvoiceReceiveEmail,
                            InvoiceReceiveAddress = req.InvoiceReceiveAddress
                        });
                    }
                    else
                    {
                        var newplugin = new sale_ordr_plugin
                        {
                            DocEntry = thisorder.DocEntry,
                            sbo_id = thisorder.sbo_id,
                            InvoiceTitle = req.InvoiceTitle,
                            InvoiceName = req.InvoiceName,
                            InvoiceTaxSignNo = req.InvoiceTaxSignNo,
                            InvoiceReceivePhone = req.InvoiceReceivePhone,
                            InvoiceReceiveEmail = req.InvoiceReceiveEmail,
                            InvoiceReceiveAddress = req.InvoiceReceiveAddress
                        };
                        newobj = await UnitWork.AddAsync<sale_ordr_plugin, int>(newplugin);
                    }
                    await UnitWork.SaveAsync();
                    result.Data = newobj.DocEntry;
                    result.Message = "保存成功";
                }
                else
                {
                    result.Code = 400;
                    result.Message = "没有找到相关记录";
                }
            }
            else
            {
                result.Code = 0;
                result.Message = "缺少关键字商城订单号 EshopOrderNo";
            }
            return result;
        }

        public string Eshop_AddOrderStatusFlow()
        {
            return "";
        }
        /// <summary>
        /// 通过客户电话查找客户信息与对应销售员信息
        /// </summary>
        /// <param name="mobile">客户手机号</param>
        /// <returns></returns>
        public async Task<SlpInfoOfClientResp> GetClientSlpInfoByClientMobile(string mobile)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //业务员Id
            var objslpstr = from t in UnitWork.Find<crm_tel> (null)
                            join a in UnitWork.Find<crm_ocrd>(null) on new { t.sbo_id, t.CardCode } equals new { sbo_id = a.sbo_id, CardCode = a.CardCode }
                            join b in UnitWork.Find<crm_oslp>(null) on new { a.sbo_id, a.SlpCode } equals new { sbo_id = b.sbo_id, SlpCode = b.SlpCode } 
                            where a.sbo_id.Equals(1) && t.Phone.Equals(mobile)
                            select new { t,a, b };
            var objslp = await objslpstr.Select(o => new { o.a.SlpCode, o.b.SlpName, o.a.sbo_id,o.t.CardCode,o.a.CardName }).FirstOrDefaultAsync();
            if (objslp != null && objslp.SlpCode > 0)
            {
                SlpInfoOfClientResp selResult = new SlpInfoOfClientResp();
                selResult.CardCode = objslp.CardCode;
                selResult.CardName = objslp.CardName;
                var objuser = from a in UnitWork.Find<sbo_user>(null)
                              join b in UnitWork.Find<base_user>(null) on a.user_id equals b.user_id into ab
                              from b in ab.DefaultIfEmpty()
                              join c in UnitWork.Find<base_user_detail>(null) on b.user_id equals c.user_id into bc
                              from c in bc.DefaultIfEmpty()
                              select new { a, b, c };
                var telobj = await objuser.Where(o => o.a.sbo_id == objslp.sbo_id && o.a.sale_id == objslp.SlpCode && o.b.user_nm.Equals(objslp.SlpName))
                    .Select(q => new
                    {
                        UserName = q.b.user_nm,
                        Mobile = q.b.mobile
                    }).FirstOrDefaultAsync();
                selResult.UserName = telobj.UserName;
                selResult.Mobile = telobj.Mobile.ToString();
                return selResult;
            }
            else
            {
                return new SlpInfoOfClientResp();
            }
        }

    }
}