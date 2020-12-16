﻿using System;
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


        public WfaEshopStatusApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork,auth)
        {
            _revelanceApp = app;
        }
      
        /// <summary>
        /// 根据商城注册电话查对应订单状态列表
        /// </summary>
        /// <param name="RegisterMobile"></param>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetOrderStatusByRegMobile (QryWfaEshopStatusListReq request)
        {
            var result = new TableData();
            //如果是客户类型则查该客户的列表
            if (request.QryType.ToString()=="1")
            {
                //查是否为客户
                var objclient = from a in UnitWork.Find<OCRD>(null)
                                join b in UnitWork.Find<OCPR>(null) on new { a.CardCode, a.CntctPrsn } equals new { b.CardCode, CntctPrsn = b.Name } into ab
                                from b in ab.DefaultIfEmpty()
                                select new { a, b };
                string cardcode = await objclient.Where(o => o.b.Cellolar.Equals(request.QryMobile)).Select(s => s.a.CardCode).FirstOrDefaultAsync();

                var qrystatus = UnitWork.Find<wfa_eshop_status>(o => o.card_code.Equals(cardcode)).Include(s => s.wfa_eshop_oqutdetails)
                    .WhereIf(! string.IsNullOrEmpty(request.QryStatus.ToString()), q=>q.cur_status.Equals(request.QryStatus))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.key), q => q.card_code.Equals(request.key) || q.card_name.Contains(request.key) || q.job_id.Equals(request.key)
                    || q.order_entry.Equals(Convert.ToInt32(request.key)) || q.quotation_entry.Equals(Convert.ToInt32(request.key)));
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
                     .WhereIf(!string.IsNullOrWhiteSpace(request.key), q => q.card_code.Equals(request.key) || q.card_name.Contains(request.key) || q.job_id.Equals(request.key)
                    || q.order_entry.Equals(Convert.ToInt32(request.key)) || q.quotation_entry.Equals(Convert.ToInt32(request.key)));
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
            var obj =await UnitWork.Find<wfa_eshop_status>(o => o.document_id.Equals(documentId)).Include(s => s.wfa_eshop_oqutdetails)
                .Include(d=>d.wfa_eshop_canceledstatuss).FirstOrDefaultAsync();

            var logsql = from a in UnitWork.Find<buy_opor>(null)
                                      join b in UnitWork.Find<sale_transport>(null) on new { a.sbo_id, a.DocEntry } equals new { sbo_id=b.SboId, DocEntry=b.Buy_DocEntry }
                                      join c in UnitWork.Find<sale_dln1>(null) on new {b.SboId,b.Base_DocEntry} equals new { SboId=c.sbo_id, Base_DocEntry=c.DocEntry }
                                      where b.Base_DocType == 24 && c.BaseType==17 && c.BaseEntry.Equals( obj.order_entry)
                                      select new { a, b,c };
            var logobj = await logsql.Select(o => new { CoName = o.a.CardName, ExpNum = o.a.LicTradNum, CreateDate = o.a.CreateDate }).Distinct().ToListAsync();
            var result = obj.MapTo<WfaEshopStatusResp>();
            result.wfa_eshop_Logs = logobj.MapToList< LogInfo>();
            return result;
        }
    }
}