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

namespace OpenAuth.App.Sap.Service
{
    public class SerialNumberApp : OnlyUnitWorkBaeApp
    {
        public SerialNumberApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        /// <summary>
        /// 查询制造商序列号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> Find(QuerySerialNumberListReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<OINS>(null)
                        join b in UnitWork.Find<CTR1>(null) on a.insID equals b.InsID
                        select new {
                            a,b
                        };
            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => q.a.manufSN.Contains(req.ManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.a.customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), q => q.a.custmrName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), q => q.a.itemCode.Contains(req.ItemCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), q => q.a.itemCode.Contains(req.ItemName))
                ;
            var query2 = query.Select(q => new
            {
                q.a.manufSN,
                q.a.internalSN,
                q.a.customer,
                q.a.custmrName,
                q.b.ContractID,
                dlvryDate = q.a.dlvryDate.Value.AddYears(1),
                q.a.itemCode,
                q.a.itemName
            });
            result.data = await query2//.OrderBy(u => u.Id)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync(); ///Select($"new ({propertyStr})");
            result.count = query2.Count();
            return result;
        }
    }
}
