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
using System.Text.RegularExpressions;

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
                 .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSNOrItemCode), q => q.a.itemCode.Contains(req.ManufSNOrItemCode) || q.a.manufSN.Contains(req.ManufSNOrItemCode))
                 ;

            var query2 = query.Select(q => new 
            {
                ManufSN = q.a.manufSN,
                InternalSN = q.a.internalSN,
                Customer = q.a.customer,
                CustmrName = q.a.custmrName,
                ContractID = q.b.ContractID,
                DlvryDate = q.a.dlvryDate.Value.AddYears(1),
                ItemCode = q.a.itemCode,
                ItemName = q.a.itemName,
            });

            var qqq = UnitWork.Find<ServiceOins>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => q.manufSN.Contains(req.ManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), q => q.customer.Contains(req.CardName) || q.custmrName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), q => q.itemCode.Contains(req.ItemCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), q => q.itemCode.Contains(req.ItemName)).Select(q => new 
                {
                    ManufSN = q.manufSN,
                    InternalSN = q.internalSN,
                    Customer = q.customer,
                    CustmrName = q.custmrName,
                    ContractID = q.contract.Value,
                    DlvryDate = q.dlvryDate.Value.AddYears(1),
                    ItemCode = q.itemCode,
                    ItemName = q.itemName
                });
            var knowledgebases = await UnitWork.Find<KnowledgeBase>(k => k.Rank == 1 && k.IsNew == true && !string.IsNullOrWhiteSpace(k.Content)).ToListAsync();

            if (!string.IsNullOrWhiteSpace(req.CardCode))
            {
                var data1 = await query2.ToListAsync();
                var data2 = await qqq.ToListAsync();
                data1.AddRange(data2);
                var SerialNumbers = data1.GroupBy(d => new{ d.ManufSN,d.ItemCode}).Select(g => g.First()).ToList();
                var serials= SerialNumbers.Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToList();
                result.Data = serials.Select(q => new
                {
                    q.ContractID,
                    q.CustmrName,
                    q.Customer,
                    q.DlvryDate,
                    q.InternalSN,
                    q.ItemCode,
                    q.ItemName,
                    q.ManufSN,
                    Code = knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? q.ItemCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault()
                }).ToList();
                result.Count = SerialNumbers.Count();
                return result;
            }
            var objs= await query2//.OrderBy(u => u.Id)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync(); ///Select($"new ({propertyStr})");
            result.Data = objs.Select(q => new
            {
                q.ContractID,
                q.CustmrName,
                q.Customer,
                q.DlvryDate,
                q.InternalSN,
                q.ItemCode,
                q.ItemName,
                q.ManufSN,
                Code = knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? q.ItemCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault()
            }).ToList();
            result.Count = await query2.CountAsync();

            if (result.Count == 0)
            {

                objs = await qqq//.OrderBy(u => u.Id)
                    .Skip((req.page - 1) * req.limit)
                    .Take(req.limit).ToListAsync(); ///Select($"new ({propertyStr})");
                result.Data = objs.Select(q => new
                {
                    q.ContractID,
                    q.CustmrName,
                    q.Customer,
                    q.DlvryDate,
                    q.InternalSN,
                    q.ItemCode,
                    q.ItemName,
                    q.ManufSN,
                    Code = knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? q.ItemCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault()
                }).ToList();
                result.Count = await qqq.CountAsync();
            }

            return result;
        }

        #region 新威智能App使用 若修改请告知谢谢！！！
        /// <summary>
        /// 查询制造商序列号(App 已生成服务单)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AppGet(QueryAppSerialNumberListReq req)
        {
            var result = new TableData();
            //查询当前服务单已选择的制造商序列号
            var manufSNsList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId).ToListAsync();
            string manufSNs = string.Join(",", manufSNsList.Select(s => s.ManufacturerSerialNumber).Distinct().ToArray());
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
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.a.customer.ToUpper().Contains(req.CardCode.ToUpper()))
                .WhereIf(!string.IsNullOrEmpty(req.key), q => q.a.itemCode.ToUpper().Contains(req.key.ToUpper()) || q.a.manufSN.ToUpper().Contains(req.key.ToUpper()))
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
            var query3 = UnitWork.Find<ServiceOins>(null)
           .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.customer.ToUpper().Contains(req.CardCode.ToUpper()))
                .WhereIf(!string.IsNullOrEmpty(req.key), q => q.itemCode.ToUpper().Contains(req.key.ToUpper()) || q.manufSN.ToUpper().Contains(req.key.ToUpper()))
                .WhereIf(!string.IsNullOrEmpty(manufSNs), q => !manufSNs.Contains(q.manufSN))
                .WhereIf(req.ManufSNs.Count > 0 && req.ManufSNs != null, q => !req.ManufSNs.Contains(q.manufSN))
                .WhereIf(!string.IsNullOrEmpty(technicianApplys), q => !technicianApplys.Contains(q.manufSN)).Select(q => new SerialNumberListResp
                {
                    ManufSN = q.manufSN,
                    InternalSN = q.internalSN,
                    Customer = q.customer,
                    CustmrName = q.custmrName,
                    ContractID = q.contract.Value,
                    DlvryDate = q.dlvryDate.Value.AddYears(1),
                    ItemCode = q.itemCode,
                    ItemName = q.itemName
                });
            result.Data = await query2
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();
            result.Count = await query2.CountAsync();
            if (result.Count == 0)
            {

                result.Data = await query3
                    .Skip((req.page - 1) * req.limit)
                    .Take(req.limit).ToListAsync();
                result.Count = await query3.CountAsync();
            }
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
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.a.customer.ToUpper().Contains(req.CardCode.ToUpper()))
                .WhereIf(!string.IsNullOrEmpty(req.key), q => q.a.itemCode.ToUpper().Contains(req.key.ToUpper()) || q.a.manufSN.ToUpper().Contains(req.key.ToUpper()))
                .WhereIf(req.ManufSNs.Count > 0 && req.ManufSNs != null, q => !req.ManufSNs.Contains(q.a.manufSN.ToUpper()))
                ;
            var query2 = query.Select(q => new
            {
                q.a.manufSN,
                q.a.internalSN,
                q.a.customer,
                q.a.custmrName,
                q.b.ContractID,
                DlvryDate = q.a.dlvryDate.Value.AddYears(1),
                q.a.itemCode,
                q.a.itemName
            });
            var query3 = UnitWork.Find<ServiceOins>(null)
              .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.customer.ToUpper().Contains(req.CardCode.ToUpper()))
              .WhereIf(req.ManufSNs.Count > 0 && req.ManufSNs != null, q => !req.ManufSNs.Contains(q.manufSN.ToUpper()))
              .WhereIf(!string.IsNullOrEmpty(req.key), q => q.itemCode.ToUpper().Contains(req.key.ToUpper()) || q.manufSN.ToUpper().Contains(req.key.ToUpper())).Select(q => new SerialNumberListResp
              {
                  ManufSN = q.manufSN,
                  InternalSN = q.internalSN,
                  Customer = q.customer,
                  CustmrName = q.custmrName,
                  ContractID = q.contract.Value,
                  DlvryDate = q.dlvryDate.Value.AddYears(1),
                  ItemCode = q.itemCode,
                  ItemName = q.itemName
              });
            var knowledgebases = await UnitWork.Find<KnowledgeBase>(k => k.Rank == 1 && k.IsNew == true && !string.IsNullOrWhiteSpace(k.Content)).ToListAsync();

            var objs= await query2
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();
            result.Data = objs.Select(c => new
            {
                c.manufSN,
                c.internalSN,
                c.customer,
                c.custmrName,
                c.ContractID,
                c.DlvryDate,
                c.itemCode,
                c.itemName,
                Code = knowledgebases.Where(k => Regex.IsMatch(c.itemCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? c.itemCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(c.itemCode, k.Content)).Select(k => k.Code).FirstOrDefault()
            }).ToList();
            result.Count = await query2.CountAsync();
            if (result.Count == 0)
            {
                var obj= await query3
                    .Skip((req.page - 1) * req.limit)
                    .Take(req.limit).ToListAsync();
                result.Data = obj.Select(c => new
                {
                    c.ManufSN,
                    c.InternalSN,
                    c.Customer,
                    c.CustmrName,
                    c.ContractID,
                    c.DlvryDate,
                    c.ItemCode,
                    c.ItemName,
                    Code = knowledgebases.Where(k => Regex.IsMatch(c.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? c.ItemCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(c.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault()
                }).ToList();
                result.Count = await query3.CountAsync();
            }
            return result;
        }
        #endregion
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
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), q => q.a.customer.Contains(req.CardName.ToUpper()) || q.a.custmrName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.a.customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), q => q.a.itemCode.Contains(req.ItemCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), q => q.a.itemName.Contains(req.ItemName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSNOrItemCode), q => q.a.itemCode.Contains(req.ManufSNOrItemCode) || q.a.manufSN.Contains(req.ManufSNOrItemCode))
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
                ServiceFee = q.a.itemCode,
                SlpName = q.s.SlpName,
                CntrctStrt = q.a.cntrctStrt,
                CntrctEnd = q.a.cntrctEnd,
                CreateDate = q.a.createDate
            });
            int count = await query2.CountAsync();
            if (count == 0 || !string.IsNullOrWhiteSpace(req.CardCode) || !string.IsNullOrWhiteSpace(req.CardName))
            {
                var slpq = from b in UnitWork.Find<OCRD>(null)
                           join s in UnitWork.Find<OSLP>(null) on b.SlpCode equals s.SlpCode into ac
                           from s in ac.DefaultIfEmpty()
                           select new
                           {
                               b.CardCode,
                               s.SlpName
                           };
                var slpList = await slpq.ToListAsync();

                var ServiceOinsModels = UnitWork.Find<ServiceOins>(null).Select(q => new SerialNumberResp
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
                    ServiceFee = q.itemCode,
                    SlpName = "",
                    CntrctStrt = q.cntrctStrt,
                    CntrctEnd = q.cntrctEnd,
                    CreateDate = q.createDate
                })
                .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), q => q.ManufSN.Contains(req.ManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), q => q.Customer.Contains(req.CardCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), q => q.Customer.Contains(req.CardName.ToUpper()) || q.CustmrName.Contains(req.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), q => q.ItemCode.Contains(req.ItemCode))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ItemName), q => q.ItemName.Contains(req.ItemName));

                var MergeModels = query2.ToList().Union(ServiceOinsModels.ToList());

                //var data1 = await query2.ToListAsync();
                //var data2 = await qqq.ToListAsync();
                //data2.ForEach(o =>
                //{
                //    o.SlpName = slpList.Where(s => s.CardCode.Equals(o.Customer)).FirstOrDefault().SlpName;
                //});
                //data1.AddRange(data2);

                MergeModels = MergeModels.GroupBy(d => new { d.ManufSN, d.ItemCode,d.DeliveryNo }).Select(g => g.First()).ToList();

                var DataList = MergeModels.OrderBy(o => o.Customer).ThenBy(o=>o.ManufSN).Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToList();
                DataList.ForEach(o =>
                {
                    o.ServiceFee = GetServiceMoney(o.ServiceFee);
                    o.SlpName = slpList.FirstOrDefault(s => s.CardCode.Equals(o.Customer)).SlpName;
                });
                result.Data = DataList;
                result.Count = MergeModels.Count();
                return result;
                //var qlist= await qqq//.OrderBy(u => u.Id)
                //    .Skip((req.page - 1) * req.limit)
                //    .Take(req.limit).ToListAsync();
                //qlist.ForEach(o =>
                //{
                //    o.SlpName =slpList.Where(s => s.CardCode.Equals(o.Customer)).FirstOrDefault().SlpName;
                //});
                //result.Data = qlist;
                //result.Count = await qqq.CountAsync();
            }
            else
            {
                var queryList = await query2.OrderBy(o => o.Customer).ThenBy(o => o.ManufSN)//.OrderBy(u => u.Id)
               .Skip((req.page - 1) * req.limit)
               .Take(req.limit).ToListAsync(); ///Select($"new ({propertyStr})");
                queryList.ForEach(o =>
                {
                    o.ServiceFee = GetServiceMoney(o.ServiceFee);
                });
                result.Data = queryList;
                result.Count = await query2.CountAsync();
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

        /// <summary>
        /// 根据客户代码获取已购买的设备信息
        /// </summary>
        /// <param name="customerCode"></param>
        /// <returns></returns>
        public async Task<TableData> GetEquipments(QuerySerialNumberListReq request)
        {
            var result = new TableData();

            var serials = new List<SerialInfo>();
            //根据交货单号和物料编码找到物料单价(本地)
            var query1 = from o in UnitWork.Find<OINS>(null).Where(o => o.customer == request.CardCode)
                         join d in UnitWork.Find<DLN1>(null) //发货明细表
                         on new { DocEntry = o.delivery, ItemCode = o.itemCode } equals new { d.DocEntry, d.ItemCode } into temp
                         from t in temp.DefaultIfEmpty()
                         select new SerialInfo
                         {
                             SerialNum = o.manufSN,
                             MaterialCode = o.itemCode,
                             MaterialDesc = o.itemName,
                             PurchaseAmount = t == null ? (decimal)0.00 : (t.LineTotal / t.Quantity).Value,
                             PurchaseTime = o.dlvryDate.Value,
                             WarrantyTime = o.dlvryDate.Value.AddYears(1) //保修期为1年
                         };
            serials.AddRange(query1);
            //如果sap没记录则查询
            if (query1.Count() == 0)
            {
                var query2 = await UnitWork.Find<service_oins>(null).Where(o => o.customer == request.CardCode)
                            .Select(o => new
                            {
                                o.deliveryNo,
                                SerialNum = o.manufSN,
                                MaterialCode = o.itemCode,
                                MaterialDesc = o.itemName,
                                PurchaseTime = o.dlvryDate,
                                WarrantyTime = o.dlvryDate.Value.AddYears(1) //保修期为1年
                            }).ToListAsync();

                var query3 = (from q in query2
                              join d in UnitWork.Find<DLN1>(null)
                              on new { DocEntry = q.deliveryNo, ItemCode = q.MaterialCode } equals new { d.DocEntry, d.ItemCode } into temp
                              from t in temp.DefaultIfEmpty()
                              select new SerialInfo
                              {
                                  SerialNum = q.SerialNum,
                                  MaterialCode = q.MaterialCode,
                                  MaterialDesc = q.MaterialDesc,
                                  PurchaseAmount = t == null ? (decimal)0.00 : (t.LineTotal / t.Quantity).Value,
                                  PurchaseTime = q.PurchaseTime.Value,
                                  WarrantyTime = q.WarrantyTime //保修期为1年
                              }).ToList();
                serials.AddRange(query3);
            }

            //客户信息
            var customerInfo = await (from a in UnitWork.Find<OINS>(null)
                                      join b in UnitWork.Find<OCRD>(null) on a.customer equals b.CardCode into temp1
                                      from t1 in temp1.DefaultIfEmpty()
                                      join f in UnitWork.Find<OCRY>(null) on t1.Country equals f.Code into temp2
                                      from t2 in temp2.DefaultIfEmpty()
                                      join g in UnitWork.Find<OCST>(null) on t1.State1 equals g.Code into temp3
                                      from t3 in temp3.DefaultIfEmpty()
                                      where a.customer == request.CardCode
                                      select new
                                      {
                                          a.customer,
                                          a.custmrName,
                                          Address = $"{ t2.Name ?? "" }{ t3.Name ?? "" }{ t1.City ?? "" }{ t1.Building ?? "" }",
                                          Address2 = $"{ t2.Name ?? "" }{ t3.Name ?? "" }{ t1.MailCity ?? "" }{ t1.MailBuildi ?? "" }",
                                      }).FirstOrDefaultAsync();
            result.Data = new
            {
                customer = customerInfo?.customer,
                custmrName = customerInfo?.custmrName,
                Address = customerInfo?.Address ?? customerInfo?.Address2 ?? "",
                totalAccount = serials.Sum(s => s.PurchaseAmount).ToString("N2"),
                Serials = serials.Select(s => new
                {
                    s.SerialNum,
                    s.MaterialCode,
                    s.MaterialDesc,
                    s.PurchaseAmount,
                    s.PurchaseTime,
                    s.WarrantyTime,
                }).OrderByDescending(s => s.PurchaseTime).Skip((request.page - 1) * request.limit).Take(request.limit),
            };
            result.Count = serials.Count();

            return result;
        }

        /// <summary>
        /// 根据交货号获取交货设备的序列号与中、下位机版本
        /// </summary>
        /// <param name="deliveryNo"></param>
        /// <param name="sboId"></param>
        /// <returns></returns>
        public async Task<TableData> GetSerialNoAndXWJVersionByDeliveryNo(string deliveryNo)
        {
            int sboId = 1;
            var result = new TableData();
            var devs = new List<DeliveryLableInfo>();
            var query1 = from a in UnitWork.Find<ITL1>(null)
                         join b in UnitWork.Find<OITL>(null) on a.LogEntry equals b.LogEntry
                         join r in UnitWork.Find<OSRN>(null) on new { ItemCode = a.ItemCode, SysNumber = a.SysNumber.Value } equals new { r.ItemCode, r.SysNumber }
                         where b.DocType == 15 && b.DocEntry.ToString() == deliveryNo
                         select new DeliveryLableInfo
                         {
                             LineNum = Convert.ToInt32(b.DocLine),
                             ItemCode = a.ItemCode,
                             ItemType = a.ItemCode.StartsWith("CT-ZWJ") ? "ZWJ" : "XWJ",
                             OrderNo = Convert.ToInt32(b.BaseEntry),
                             SerialNo = r.MnfSerial,
                             ProductNo = UnitWork.Find<OWOR>(null).Where(o => o.ItemCode == a.ItemCode && o.OriginAbs.ToString() == b.BaseEntry.ToString()).Select(p => p.DocEntry.ToString()).FirstOrDefault()
                         };
            devs.AddRange(query1);
            //获取生产单与中/下位机版本
            var query2 = devs.Where(w => !string.IsNullOrEmpty(w.ProductNo)).GroupBy(g => new { g.ProductNo }).Select(g => g.First()).ToList();
            foreach (DeliveryLableInfo thisE in query2)
            {
                #region 获取中、下位机版本
                var itemVer = "";
                var proobj = await UnitWork.Find<product_wor1>(null).Where(o => o.DocEntry.ToString() == thisE.ProductNo && o.sbo_id == sboId).Select(p => new { ItemCode = p.ItemCode }).ToListAsync();
                if (thisE.ItemType == "ZWJ")
                {
                    Regex r = new Regex(@"(B01-)(BTS-|BFGS-)(ZWJ-)(\d+)*");
                    proobj = proobj.Where(w => r.IsMatch(w.ItemCode)).ToList();
                    if (proobj.Count > 0)
                    {
                        itemVer= proobj[0].ItemCode;
                    }
                }
                else
                {
                    Regex r = new Regex(@"(B01-)(BTS-|BGS-|BFGS-)?(XWJ-)(M-|SMB-)?(\d+)*");
                    var xwjobj = proobj.Where(w => r.IsMatch(w.ItemCode)).ToList();
                    //下位机可能是放在半成品里面
                    if (xwjobj.Count > 0)
                    {
                        itemVer = xwjobj[0].ItemCode;
                    }
                    else
                    {//如果成品设备里面没有下位机，则查找生产单里半成品生产订单明细
                        var BCPobj = proobj.Where(w => w.ItemCode.StartsWith("BT-") || w.ItemCode.StartsWith("BTE-") || w.ItemCode.StartsWith("BE-")).ToList();
                        if (BCPobj.Count > 0)
                        {
                            proobj = await (from a in UnitWork.Find<product_owor>(null).Where( p=>p.CDocEntry == thisE.ProductNo && p.ItemCode == BCPobj[0].ItemCode)
                                            join b in UnitWork.Find<product_wor1>(null) on new { DocEntry = a.DocEntry, sbo_id = a.sbo_id } equals new { b.DocEntry, b.sbo_id }
                                            select new
                                            {
                                                ItemCode = b.ItemCode
                                            }).ToListAsync();
                            if (proobj.Count > 0)
                            {
                                xwjobj = proobj.Where(w => r.IsMatch(w.ItemCode)).ToList();
                                if (xwjobj.Count > 0)
                                {
                                    itemVer = xwjobj[0].ItemCode;
                                }
                            }
                        }
                    }
                }
                #endregion
                if (!string.IsNullOrEmpty(itemVer))
                {
                    var devs1 = devs.Where(w => w.ProductNo == thisE.ProductNo).ToList();
                    foreach (DeliveryLableInfo resultE in devs1)
                    {
                        resultE.SubItemVer = itemVer;
                    }
                }
            }
            result.Count = devs.Count;
            result.Data = devs;
            return result;
        }
    }
}
