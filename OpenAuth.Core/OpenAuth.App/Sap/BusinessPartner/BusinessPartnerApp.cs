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

namespace OpenAuth.App.Sap.BusinessPartner
{
    public class BusinessPartnerApp
    {

        /// <summary>
        /// 用于事务操作
        /// </summary>
        /// <value>The unit work.</value>
        protected IUnitWork UnitWork;

        protected IAuth _auth;

        public BusinessPartnerApp(IUnitWork unitWork, IAuth auth)
        {
            UnitWork = unitWork;
            _auth = auth;
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
            result.count = query.Count();
            return result;
        }
    }
}
