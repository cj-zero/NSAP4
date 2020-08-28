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
using OpenAuth.App.Response;

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
                        join b in UnitWork.Find<CTR1>(null) on a.insID equals b.InsID into ab
                        from b in ab.DefaultIfEmpty()
                        select new
                        {
                            a,
                            b
                        };
            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => q.a.manufSN.Contains(req.ManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), q => q.a.customer.Contains(req.CardName) || q.a.custmrName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.a.customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), q => q.a.itemCode.Contains(req.ItemCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), q => q.a.itemCode.Contains(req.ItemName))
                .WhereIf(!string.IsNullOrEmpty(req.ManufSNOrItemCode), q => q.a.itemCode.Contains(req.ManufSNOrItemCode) || q.a.manufSN.Contains(req.ManufSNOrItemCode))
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
                q.a.itemName,
            });
            result.Data = await query2//.OrderBy(u => u.Id)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync(); ///Select($"new ({propertyStr})");
            result.Count = await query2.CountAsync();

            if (result.Count == 0)
            {
                var qqq = UnitWork.Find<ServiceOins>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => q.manufSN.Contains(req.ManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), q => q.customer.Contains(req.CardName) || q.custmrName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), q => q.itemCode.Contains(req.ItemCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), q => q.itemCode.Contains(req.ItemName)).Select(q => new
                {
                    q.manufSN,
                    q.internalSN,
                    q.customer,
                    q.custmrName,
                    ContractID = q.contract,
                    dlvryDate = q.dlvryDate.Value.AddYears(1),
                    q.itemCode,
                    q.itemName
                });
                result.Data = await qqq//.OrderBy(u => u.Id)
                    .Skip((req.page - 1) * req.limit)
                    .Take(req.limit).ToListAsync(); ///Select($"new ({propertyStr})");
                result.Count = await qqq.CountAsync();
            }

            return result;
        }

        /// <summary>
        /// 查询制造商序列号(App 已生成服务单)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AppGet(QueryAppSerialNumberListReq req)
        {
            var result = new TableData();
            //查询当前服务单已选择的制造商序列号
            var manufSNsList = await UnitWork.Find<ServiceOrderSerial>(s => s.ServiceOrderId == req.ServiceOrderId).ToListAsync();
            string manufSNs = string.Join(",", manufSNsList.Select(s => s.ManufSN).Distinct().ToArray());
            //查询技术员已经提交的制造商序列号
            var technicianApplyList = await UnitWork.Find<SeviceTechnicianApplyOrder>(s => s.ServiceOrderId == req.ServiceOrderId).ToListAsync();
            string technicianApplys = string.Join(",", technicianApplyList.Select(s => s.ManufSN).Distinct().ToArray());
            var query = from a in UnitWork.Find<OINS>(null)
                        join b in UnitWork.Find<CTR1>(null) on a.insID equals b.InsID into ab
                        from b in ab.DefaultIfEmpty()
                        select new
                        {
                            a,
                            b
                        };
            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.a.customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrEmpty(req.key), q => q.a.itemCode.Contains(req.key) || q.a.manufSN.Contains(req.key))
                .WhereIf(!string.IsNullOrEmpty(manufSNs), q => !manufSNs.Contains(q.a.manufSN))
                .WhereIf(req.ManufSNs.Count > 0 && req.ManufSNs != null, q => !req.ManufSNs.Contains(q.a.manufSN))
                .WhereIf(!string.IsNullOrEmpty(technicianApplys), q => !technicianApplys.Contains(q.a.manufSN))
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
            result.Data = await query2
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();
            result.Count = await query2.CountAsync();
            return result;
        }

        /// <summary>
        /// 查询制造商序列号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AppFind(QueryAppSerialNumberListReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<OINS>(null)
                        join b in UnitWork.Find<CTR1>(null) on a.insID equals b.InsID into ab
                        from b in ab.DefaultIfEmpty()
                        select new
                        {
                            a,
                            b
                        };
            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.a.customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrEmpty(req.key), q => q.a.itemCode.Contains(req.key) || q.a.manufSN.Contains(req.key))
                .WhereIf(req.ManufSNs.Count > 0 && req.ManufSNs != null, q => !req.ManufSNs.Contains(q.a.manufSN))
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
            result.Data = await query2
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();
            result.Count = await query2.CountAsync();
            return result;
        }

        /// <summary>
        /// 得到序列号交易列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetContractList(QuerySerialNumberListReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<OINS>(null)
                        join b in UnitWork.Find<OCRD>(null) on a.customer equals b.CardCode into ab
                        from b in ab.DefaultIfEmpty()
                        join s in UnitWork.Find<OSLP>(null) on b.SlpCode equals s.SlpCode into ac
                        from s in ac.DefaultIfEmpty()
                        select new
                        {
                            a,
                            b,
                            s
                        };
            query = query
                .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => q.a.manufSN.Contains(req.ManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), q => q.a.customer.Contains(req.CardName) || q.a.custmrName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.a.customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), q => q.a.itemCode.Contains(req.ItemCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), q => q.a.itemName.Contains(req.ItemName))
                .WhereIf(!string.IsNullOrEmpty(req.ManufSNOrItemCode), q => q.a.itemCode.Contains(req.ManufSNOrItemCode) || q.a.manufSN.Contains(req.ManufSNOrItemCode))
                ;
            var query2 = query.Select(q => new SerialNumberResp
            {
                InsID = q.a.insID,
                Customer = q.a.customer,
                CustmrName = q.a.custmrName,
                ManufSN = q.a.manufSN,
                InternalSN = q.a.internalSN,
                ItemCode = q.a.itemCode,
                ItemName = q.a.itemName,
                ManufDate = q.a.manufDate,
                DeliveryNo = q.a.deliveryNo,
                DlvryDate = q.a.dlvryDate,
                ContractId = q.a.contract,
                ServiceFee = GetServiceMoney(q.a.itemCode),
                SlpName = q.s.SlpName,
                CntrctStrt = q.a.cntrctStrt,
                CntrctEnd = q.a.cntrctEnd,
                CreateDate = q.a.createDate
            });
            result.Data = await query2//.OrderBy(u => u.Id)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync(); ///Select($"new ({propertyStr})");
            result.Count = await query2.CountAsync();

            if (result.Count == 0)
            {
                var slpq= from b in UnitWork.Find<OCRD>(null) 
                        join s in UnitWork.Find<OSLP>(null) on b.SlpCode equals s.SlpCode into ac
                        from s in ac.DefaultIfEmpty()
                             select new
                             {
                                 b.CardCode,
                                 s.SlpName
                             };
                var slpList = await slpq.ToListAsync();

                var qqq = UnitWork.Find<ServiceOins>(null).Select(q => new SerialNumberResp
                {
                    InsID = q.insID,
                    Customer = q.customer,
                    CustmrName = q.custmrName,
                    ManufSN = q.manufSN,
                    InternalSN = q.internalSN,
                    ItemCode = q.itemCode,
                    ItemName = q.itemName,
                    ManufDate = q.manufDate,
                    DeliveryNo = q.deliveryNo,
                    DlvryDate = q.dlvryDate,
                    ContractId = q.contract,
                    ServiceFee = GetServiceMoney(q.itemCode),
                    SlpName = "",
                    CntrctStrt = q.cntrctStrt,
                    CntrctEnd = q.cntrctEnd,
                    CreateDate = q.createDate
                })
               .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => q.ManufSN.Contains(req.ManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.Customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), q => q.Customer.Contains(req.CardName) || q.CustmrName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), q => q.ItemCode.Contains(req.ItemCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), q => q.ItemName.Contains(req.ItemName));
                
                var qlist= await qqq//.OrderBy(u => u.Id)
                    .Skip((req.page - 1) * req.limit)
                    .Take(req.limit).ToListAsync();
                qlist.ForEach(o =>
                {
                    o.SlpName = slpList.Where(s => s.CardCode.Equals(o.Customer)).FirstOrDefault().SlpName;
                });
                result.Data = qlist;
                result.Count = await qqq.CountAsync();
            }

            return result;
        }

        private static string GetServiceMoney(string sItemCode)
        {
            string[] stype = sItemCode.Split('-');

            if (stype.Length < 3) return "";
            //CT-3008-5V10mA-S4
            string sVal = string.Empty;
            int num = 0;
            string[] s2;
            Single voltage = 0;
            Single Curr = 0;

            try
            {
                sVal = stype[1].Substring(stype[1].Length - 3);
                num = Convert.ToInt16(sVal);
                s2 = stype[2].Split('V');
                if (s2[0].Substring(s2[0].Length - 1) == "M")
                {
                    voltage = Convert.ToSingle(s2[0].Remove(s2[0].Length - 1)) / 1000;
                }
                else
                {
                    voltage = Convert.ToInt16(s2[0]);
                }
                if (s2[1].IndexOf("MA") > 0)
                {
                    Curr = Convert.ToSingle(s2[1].Remove(s2[1].IndexOf("MA"))) / 1000;
                }
                else
                {
                    Curr = Convert.ToInt16(s2[1].Remove(s2[1].IndexOf("A")));
                }
            }
            catch { }
            string sMoney = (num * voltage * Curr / 20).ToString();    //0.05
            return sMoney;
        }

    }
}
