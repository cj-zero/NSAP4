using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.Sap.Request;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Extensions;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.Sap.BusinessPartner
{
    public class BusinessPartnerApp : OnlyUnitWorkBaeApp
    {


        public BusinessPartnerApp(IUnitWork unitWork, IAuth auth) : base(unitWork,auth)
        {
        }

        public async Task<TableData> Load(QueryBusinessPartnerListReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<OCRD>(null)
                        join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<OCRY>(null) on a.Country equals c.Code into ac
                        from c in ac.DefaultIfEmpty()
                        join d in UnitWork.Find<OCST>(null) on a.State1 equals d.Code into ad
                        from d in ad.DefaultIfEmpty()
                        join e in UnitWork.Find<OCRY>(null) on a.MailCountr equals e.Code into ae
                        from e in ae.DefaultIfEmpty()
                        select new { a, b, c, d, e };
            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.CardCodeOrCardName), q => q.a.CardCode.Contains(req.CardCodeOrCardName) || q.a.CardName.Contains(req.CardCodeOrCardName));
            var query2 = query.Select(q => new {
                            q.a.CardCode, q.a.CardName, q.a.CntctPrsn, q.b.SlpName, q.a.Currency, q.a.Balance,
                            Address = $"{ q.a.ZipCode ?? "" }{ q.c.Name ?? "" }{ q.d.Name ?? "" }{ q.a.City ?? ""}{ q.a.Building ?? "" }",
                            Address2 = $"{ q.a.MailZipCod ?? "" }{ q.e.Name ?? "" }{ q.d.Name ?? "" }{ q.a.MailCity ?? "" }{ q.a.MailBuildi ?? "" }",
                            q.a.U_FPLB,
                            q.a.SlpCode
                        });


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = await query2//.OrderBy(u => u.Id)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();///Select($"new ({propertyStr})");
            result.Count = query2.Count();
            return result;
        }
        public async Task<TableData> Get(QueryBusinessPartnerListReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<OCRD>(null)
                        join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<OCRG>(null) on (int)a.GroupCode equals c.GroupCode into ac
                        from c in ac.DefaultIfEmpty()
                        join d in UnitWork.Find<OIDC>(null) on a.Indicator equals d.Code into ad
                        from d in ad.DefaultIfEmpty()
                        join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                        from e in ae.DefaultIfEmpty()
                        join f in UnitWork.Find<OCRY>(null) on a.Country equals f.Code into af
                        from f in af.DefaultIfEmpty()
                        join g in UnitWork.Find<OCST>(null) on a.State1 equals g.Code into ag
                        from g in ag.DefaultIfEmpty()
                        select new { a, b, c, d, e, f, g };
            List<string> carCode = new List<string>();
            if (!string.IsNullOrWhiteSpace(req.ManufSN))
            {
                carCode = await UnitWork.Find<OINS>(null).Where(o => o.manufSN.Contains(req.ManufSN)).Select(o => o.customer).ToListAsync();
                if (carCode.Count==0) {
                    carCode = await UnitWork.Find<ServiceOins>(s => s.manufSN.Contains(req.ManufSN)).Select(s => s.customer).ToListAsync() ;
                }
            } 

            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.CardCodeOrCardName), q => q.a.CardCode.Contains(req.CardCodeOrCardName) || q.a.CardName.Contains(req.CardCodeOrCardName))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => carCode.Contains(q.a.CardCode))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.slpName), q => q.b.SlpName.Contains(req.slpName));
                         
            var query2 = await query.Select(q => new {
                            q.a.CardCode, q.a.CardName, q.a.CntctPrsn, q.b.SlpName, q.a.Currency,
                            Balance = q.a.Balance ?? 0m,
                            Technician = $"{q.e.lastName??""}{q.e.firstName}",
                            Address = $"{ q.a.ZipCode ?? "" }{ q.f.Name ?? "" }{ q.g.Name ?? "" }{ q.a.City ?? "" }{ q.a.Building ?? "" }",
                            Address2 = $"{ q.a.MailZipCod ?? "" }{ q.f.Name ?? "" }{ q.g.Name ?? "" }{ q.a.MailCity ?? "" }{ q.a.MailBuildi ?? "" }",
                            q.a.Phone1, q.a.Cellular,q.a.DNotesBal,q.a.OrdersBal,
                            q.a.OprCount,q.a.UpdateDate,q.a.DfTcnician,
                            BalanceTotal = 0.00m,
                            q.a.validFor,q.a.validFrom,q.a.validTo,q.a.ValidComm,q.a.frozenFor,q.a.frozenFrom,q.a.frozenTo,q.a.FrozenComm,q.a.QryGroup2,q.a.QryGroup3,
                            q.c.GroupName,q.a.Free_Text,
                            q.a.U_FPLB,
                            q.a.SlpCode
                        }).ToListAsync();

            if (!string.IsNullOrWhiteSpace(req.Technician)) query2= query2.Where(q => q.Technician.Contains(req.Technician)).ToList();
            if(!string.IsNullOrWhiteSpace(req.Address)) query2= query2.Where(q => q.Address2.Contains(req.Address)).ToList();
                        
            //query3.Where(q=>q.Technician.Contains(req.Technician));
            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = query2
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit);
            result.Count = query2.Count();
           
            return result;
        }

        /// <summary>
        /// 服务呼叫查询单个客户信息 by zlg 2020.09.15
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        public async Task<TableData> GetBusinessAssociate(string CardCode)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<OCRD>(null)
                        join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<OCRG>(null) on (int)a.GroupCode equals c.GroupCode into ac
                        from c in ac.DefaultIfEmpty()
                        join d in UnitWork.Find<OIDC>(null) on a.Indicator equals d.Code into ad
                        from d in ad.DefaultIfEmpty()
                        join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                        from e in ae.DefaultIfEmpty()
                        join f in UnitWork.Find<OCRY>(null) on a.Country equals f.Code into af
                        from f in af.DefaultIfEmpty()
                        join g in UnitWork.Find<OCST>(null) on a.State1 equals g.Code into ag
                        from g in ag.DefaultIfEmpty()
                        select new { a, b, c, d, e, f, g };
            query = query.Where(q => q.a.CardCode.Equals(CardCode));

            var BusinessAssociate = await query.Select(q => new {
                q.a.CardCode,
                q.a.CardName,
                q.a.CntctPrsn,
                q.b.SlpName,
                q.a.Currency,
                Balance = q.a.Balance ?? 0m,
                Technician = $"{q.e.lastName ?? ""}{q.e.firstName}",
                Address = $"{ q.a.ZipCode ?? "" }{ q.f.Name ?? "" }{ q.g.Name ?? "" }{ q.a.City ?? "" }{ q.a.Building ?? "" }",
                Address2 = $"{ q.a.MailZipCod ?? "" }{ q.f.Name ?? "" }{ q.g.Name ?? "" }{ q.a.MailCity ?? "" }{ q.a.MailBuildi ?? "" }",
                q.a.Phone1,
                q.a.Cellular,
                q.a.DNotesBal,
                q.a.OrdersBal,
                q.a.OprCount,
                q.a.UpdateDate,
                q.a.DfTcnician,
                BalanceTotal = 0.00m,
                q.a.validFor,
                q.a.validFrom,
                q.a.validTo,
                q.a.ValidComm,
                q.a.frozenFor,
                q.a.frozenFrom,
                q.a.frozenTo,
                q.a.FrozenComm,
                q.a.QryGroup2,
                q.a.QryGroup3,
                q.c.GroupName,
                q.a.Free_Text,
                q.a.U_FPLB,
                q.a.SlpCode
            }).FirstOrDefaultAsync();
            result.Data = BusinessAssociate;
            return result;
        }
        /// <summary>
        /// 通过客户编码得到业务伙伴名称，联系人与地址列表
        /// </summary>
        /// <param name="cardCode">客户编码</param>
        /// <returns></returns>
        public async Task<BusinessPartnerDetailsResp> GetDetails(string cardCode)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("businesspartner");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}
            var obj = from a in UnitWork.Find<OCRD>(null)
                      join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                      from b in ab.DefaultIfEmpty()
                      join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                      from e in ae.DefaultIfEmpty()
                      select new { a, b, e };
            obj = obj.Where(o => o.a.CardCode.Equals(cardCode));

            var query = obj.Select(q => new
            {
                q.a.CardCode,
                q.a.CardName,
                q.a.CntctPrsn,
                q.a.Phone1,
                q.a.SlpCode,
                q.b.SlpName,
                TechID= q.a.DfTcnician,
                TechName= $"{q.e.lastName ?? ""}{q.e.firstName}",
                CntctPrsnList = UnitWork.Find<OCPR>(null).Where(o => o.CardCode.Equals(q.a.CardCode)).ToList(),
                AddressList = UnitWork.Find<CRD1>(null).Where(o => o.CardCode.Equals(q.a.CardCode)).ToList(),
            });

            var rltList = await query.FirstOrDefaultAsync();
            var result = rltList.MapTo<BusinessPartnerDetailsResp>();
            return result;
        }
    }
}
