﻿using Infrastructure;
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
            result.data = await query2//.OrderBy(u => u.Id)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();///Select($"new ({propertyStr})");
            result.count = query2.Count();
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
            var carCode = "";
            if (!string.IsNullOrWhiteSpace(req.ManufSN))
            {
                carCode = await UnitWork.Find<OINS>(null).Where(o => o.manufSN.Contains(req.ManufSN)).Select(o => o.customer).FirstOrDefaultAsync();
            }

            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.CardCodeOrCardName), q => q.a.CardCode.Contains(req.CardCodeOrCardName) || q.a.CardName.Contains(req.CardCodeOrCardName))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => q.a.CardCode.Equals(carCode));
                ;
            var query2 = query.Select(q => new {
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
                        });


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.data = await query2//.OrderBy(u => u.Id)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();///Select($"new ({propertyStr})");
            result.count = query.Count();
            return result;
        }
    }
}