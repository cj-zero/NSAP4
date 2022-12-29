using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenAuth.App.Nwcali
{
    public class NwcaliCertApp : OnlyUnitWorkBaeApp
    {
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        public NwcaliCertApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        public async Task AddAsync(NwcaliBaseInfo baseInfo)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var user = loginContext.User;
            if (baseInfo.Operator.Equals(user.Name))
            {
                baseInfo.OperatorId = user.Id;
            }
            else
            {
                //var u = await UnitWork.FindSingleAsync<User>(a => a.Name.Equals(baseInfo.Operator));
                //if (u is null)
                //    throw new CommonException("系统不存在当前校准人账号信息，请联系相关人员录入信息。", 400100);
                //baseInfo.OperatorId = u.Id;
                throw new CommonException("当前校准人信息与ERP账户信息不符，请确认后上传。", 400100);
            }
            await semaphoreSlim.WaitAsync();
            try
            {
                baseInfo.CertificateNumber = await CertificateNoGenerate("O");
                baseInfo.CreateTime = DateTime.Now;
                baseInfo.CreateUser = user.Name;
                baseInfo.CreateUserId = user.Id;
                //var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_NwcilSignAcount" && c.Name.Contains(user.Name)).Select(c => c.DtValue).FirstOrDefaultAsync();
                //if (category != null)
                //{
                //    var uinfo = await UnitWork.Find<UserSign>(c => c.UserName == category).FirstOrDefaultAsync();
                //    if (uinfo == null)
                //    {
                //        throw new CommonException("当前出证人暂无签名图片，请确认后上传。", 400100);
                //    }
                //    baseInfo.Issuer = uinfo.UserName;
                //    baseInfo.IssuerId = uinfo.UserId;
                //}
                //else
                //{
                //    baseInfo.Issuer = user.Name;
                //    baseInfo.IssuerId = user.Id;
                //}

                baseInfo.Issuer = user.Name;
                baseInfo.IssuerId = user.Id;

                var testerModel = await UnitWork.Find<OINS>(o => o.manufSN.Equals(baseInfo.TesterSn)).Select(o => o.itemCode).ToListAsync();
                if (testerModel != null && testerModel.Count == 1 && !testerModel.Contains("ZWJ"))
                {
                    if (testerModel.FirstOrDefault().Contains(baseInfo.TesterModel))
                        baseInfo.TesterModel = testerModel.FirstOrDefault();
                }
                await UnitWork.AddAsync(baseInfo);
                await UnitWork.SaveAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        public async Task AddProduceNwcaliBaseInfo(ProduceNwcaliBaseInfo baseInfo)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var user = loginContext.User;

            baseInfo.CreateTime = DateTime.Now;
            baseInfo.CreateUser = user.Name;
            baseInfo.CreateUserId = user.Id;
            await UnitWork.AddAsync(baseInfo);
            await UnitWork.SaveAsync();
        }

        public async Task UpdateTesterModel()
        {
            var query = await UnitWork.Find<NwcaliBaseInfo>(c => !c.TesterModel.Contains("-") && c.TesterSn.StartsWith("T")).ToListAsync();
            var sn = query.Select(c => c.TesterSn).ToList();
            var oins = await UnitWork.Find<OINS>(o => sn.Contains(o.manufSN)).Select(o => new { o.itemCode, o.manufSN }).ToListAsync();
            query.ForEach(c =>
            {
                var testerModel = oins.Where(o => o.manufSN.Equals(c.TesterSn)).Select(o => o.itemCode).ToList();
                if (testerModel != null && testerModel.Count == 1 && !testerModel.Contains("ZWJ"))
                {
                    if (testerModel.FirstOrDefault().Contains(c.TesterModel))
                        c.TesterModel = testerModel.FirstOrDefault();
                }
            });
            await UnitWork.BatchUpdateAsync(query.ToArray());
            await UnitWork.SaveAsync();
        }

        public async Task UpdateFilePath(string certNo, string path)
        {
            await UnitWork.UpdateAsync<NwcaliBaseInfo>(c => c.CertificateNumber == certNo, c => new NwcaliBaseInfo { CertPath = path });
            await UnitWork.SaveAsync();
        }

        public async Task<NwcaliBaseInfo> GetInfos(string certNo)
        {
            var info = await UnitWork.Find<NwcaliBaseInfo>(null).Include(b => b.NwcaliTurs).Include(b => b.NwcaliPlcDatas).Include(b => b.PcPlcs).Include(b => b.Etalons)
                .FirstOrDefaultAsync(b => b.CertificateNumber.Equals(certNo));
            return info;
        }
        public async Task<NwcaliBaseInfo> GetInfo(string certNo)
        {
            var info = await UnitWork.Find<NwcaliBaseInfo>(null).FirstOrDefaultAsync(b => b.CertificateNumber.Equals(certNo));
            if (info != null)
            {
                info.NwcaliTurs = await UnitWork.Find<NwcaliTur>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.NwcaliPlcDatas = await UnitWork.Find<NwcaliPlcData>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.PcPlcs = await UnitWork.Find<PcPlc>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.Etalons = await UnitWork.Find<Etalon>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
            }
            return info;
        }
        public async Task<NwcaliBaseInfo> GetInfoBySn(string testerSn)
        {
            var info = await UnitWork.Find<NwcaliBaseInfo>(null).FirstOrDefaultAsync(b => b.TesterSn.Equals(testerSn));
            if (info != null)
            {
                info.NwcaliTurs = await UnitWork.Find<NwcaliTur>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.NwcaliPlcDatas = await UnitWork.Find<NwcaliPlcData>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.PcPlcs = await UnitWork.Find<PcPlc>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.Etalons = await UnitWork.Find<Etalon>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
            }
            return info;
        }

        public async Task<List<NwcaliBaseInfo>> GetNwcaliList(List<string> certNo)
        {
            return await UnitWork.Find<NwcaliBaseInfo>(b => certNo.Contains(b.CertificateNumber)).ToListAsync();
        }

        public async Task<List<NwcaliBaseInfo>> GetInfoList(List<string> certNo)
        {
            var info = await UnitWork.Find<NwcaliBaseInfo>(b => certNo.Contains(b.CertificateNumber)).Include(b => b.NwcaliTurs).Include(b => b.NwcaliPlcDatas).Include(b => b.PcPlcs).Include(b => b.Etalons)
                .ToListAsync();
            return info;
        }

        public async Task<dynamic> GetPcPlcs(string guid)
        {
            var ids = await UnitWork.Find<PcPlc>(p => p.Guid.Equals(guid)).Select(o => o.NwcaliBaseInfoId).ToListAsync();
            var list = await UnitWork.Find<NwcaliBaseInfo>(b => ids.Contains(b.Id)).Select(a => new { CertNo = a.CertificateNumber, CalibrationDate = a.Time, ExpirationDate = a.ExpirationDate }).ToListAsync();
            return list;
        }
        /// <summary>
        /// 生成证书编号
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<string> CertificateNoGenerate(string type)
        {
            var now = DateTime.Now;
            var year = now.Year.ToString().Substring(3, 1);
            var month = now.Month.ToString();
            month = month == "10" ? "A" : month == "11" ? "B" : month == "12" ? "C" : month;
            var nos = await UnitWork.Find<NwcaliBaseInfo>(c => c.CertificateNumber.Contains($"NW{type}{year}{month}")).Select(b => b.CertificateNumber).ToListAsync();
            if (nos.Count == 0)
            {
                return $"NW{type}{year}{month}0001";
            }
            else
            {
                var no = nos.Max();
                var number = Convert.ToInt32(no.Substring(5, 4));
                number++;
                var numberStr = number.ToString().PadLeft(4, '0');
                return $"NW{type}{year}{month}{numberStr}";
            }
        }

        /// <summary>
        /// 校准报表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCalibrateReport(QueryCertReportReq req)
        {
            var result = new TableData();

            Repository.Domain.Org Org = new Repository.Domain.Org();
            List<User> user = new List<User>();
            if (req.Type <= 0)
            {
                result.Code = 500;
                result.Message = "校准类型有误！";
                return result;
            }

            if (!string.IsNullOrEmpty(req.Org))
            {
                Org = await UnitWork.Find<Repository.Domain.Org>(a => a.Name == req.Org).FirstOrDefaultAsync();
                if (Org == null)
                {
                    result.Code = 500;
                    result.Message = "部门名字有误！";
                    return result;
                }
                user = (from r in UnitWork.Find<Relevance>(a => a.Key == "UserOrg" && a.SecondId == req.Org)
                        join u in UnitWork.Find<User>(null) on r.FirstId equals u.Id
                        select u).ToList();
            }
            int sum = 0;
            List<string> userIdList = user.Select(a => a.Id).ToList();

            if (req.Type == 1)
            {
                var query = UnitWork.Find<ProductionSchedule>(a => a.NwcailStatus == 2)
                    .WhereIf(!string.IsNullOrEmpty(req.Name), a => a.NwcailOperator == req.Name)
                    .WhereIf(user.Count() > 0, a => userIdList.Contains(a.NwcailOperatorId))
                    .WhereIf(req.StartTime != null, c => c.NwcailTime.Value >= req.StartTime)
                    .WhereIf(req.EndTime != null, c => c.NwcailTime.Value <= req.EndTime)
                    .GroupBy(a => a.NwcailOperatorId)
                    .Select(a => new { id = a.Key, name = a.Max(b => b.NwcailOperator), num = a.Count() });

                sum = query.Sum(a => a.num);
                var list = await query.OrderByDescending(a => a.num)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();


                var data = (from l in list
                            join r in UnitWork.Find<Relevance>(a => a.Key == "UserOrg") on l.id equals r.FirstId
                            join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                            select new { id = l.id,name = l.name, num = l.num, orgName = o.Name }).ToList();
                result.Data = data;
                result.Extra = sum.ToString();

            }
            else
            {
                var query = UnitWork.Find<NwcaliBaseInfo>(c => c.Time < DateTime.Parse("2022-10-08"))
                    .WhereIf(!string.IsNullOrEmpty(req.Name), a => a.Operator == req.Name)
                    .WhereIf(user.Count() > 0, a => userIdList.Contains(a.OperatorId))
                    .WhereIf(req.StartTime != null, c => c.Time.Value >= req.StartTime)
                    .WhereIf(req.EndTime != null, c => c.Time.Value <= req.EndTime)
                    .GroupBy(a => a.OperatorId)
                    .Select(a => new { id = a.Key, name = a.Max(b => b.Operator), num = a.Count() });

                sum = query.Sum(a => a.num);
                var list = await query.OrderByDescending(a => a.num)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();


                var data = (from l in list
                            join r in UnitWork.Find<Relevance>(a => a.Key == "UserOrg") on l.id equals r.FirstId
                            join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                            select new { id = l.id, name = l.name, num = l.num, orgName = o.Name }).ToList();
                result.Data = data;
                result.Extra = sum.ToString();
            }

            return result;
        }

        /// <summary>
        /// 校准报表详情
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetCalibrateDetailReport(QueryCertReportReq req)
        {
            var result = new TableData();

            if (string.IsNullOrEmpty(req.Id))
            {
                result.Code = 500;
                result.Message = "人员ID有误！";
                return result;
            }
            if (req.Type<=0)
            {
                result.Code = 500;
                result.Message = "校准类型有误！";
                return result;
            }

            if (req.Type == 1)
            {
                var query = UnitWork.Find<ProductionSchedule>(a => a.NwcailStatus == 2)
                     .WhereIf(!string.IsNullOrEmpty(req.Name), a => a.NwcailOperator == req.Name)
                      .WhereIf(req.StartTime != null, c => c.NwcailTime.Value >= req.StartTime)
                     .WhereIf(req.EndTime != null, c => c.NwcailTime.Value <= req.EndTime)
                     .Where(a => a.NwcailOperatorId == req.Id);
                result.Count = query.Count();

                var listProduction = await query.OrderByDescending(a => a.NwcailTime)
               .Skip((req.page - 1) * req.limit)
               .Take(req.limit).ToListAsync();


                var listDocEntry = listProduction.Select(a => a.DocEntry).Distinct().ToList();

                List<ProductionCalibration> list = new List<ProductionCalibration>();

                var listOWOR = await UnitWork.Find<OWOR>(a => listDocEntry.Contains(a.DocEntry))
                    .Select(a=> new { U_WO_LTDW =a.U_WO_LTDW, ItemCode =a.ItemCode, DocEntry =a.DocEntry })
                    .ToListAsync();
                foreach (var item in listProduction)
                {
                    ProductionCalibration info = new ProductionCalibration();
                    info.GeneratorCode =item.GeneratorCode;
                    info.NwcailTime = item.NwcailTime;
                    info.ReceiveOperator = item.NwcailOperator;
                    var docId = item.DocEntry;
                    var owor = listOWOR.Where(a => a.DocEntry == docId).FirstOrDefault();
                    if (owor != null)
                    {
                        info.Department = owor.U_WO_LTDW;
                        info.TesterModel = owor.ItemCode;
                    }
                    else
                    {

                    }
                    list.Add(info);
                }
                result.Data = list;
            }
            else
            {
                var query = UnitWork.Find<NwcaliBaseInfo>(a => a.OperatorId == req.Id && a.Time < DateTime.Parse("2022-10-08"))
                    .WhereIf(!string.IsNullOrEmpty(req.Name), a => a.Operator == req.Name)
                    .WhereIf(req.StartTime != null, c => c.Time.Value >= req.StartTime)
                    .WhereIf(req.EndTime != null, c => c.Time.Value <= req.EndTime);
                result.Count = query.Count();
                var listNwcali = await query.OrderByDescending(a => a.Time)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();

                if (listNwcali.Count <= 0)
                {
                    return result;
                }
                var listTesterSn = listNwcali.Select(a => a.TesterSn).ToList();

                string strSql = @"select t4.SlpCode,t4.SlpName as 'Salesman',t3.DocEntry as'SalesOrder',t2.DocEntry as 'DeliveryNumber', t1.manufSN as 'TesterSn'from OINS t1
                    inner join (SELECT DocEntry,MIN(BaseEntry) as BaseEntry from DLN1 where BaseType=17  group by DocEntry )  t2 on t1.deliveryNo = t2.DocEntry 
                    inner join ORDR t3 on t2.BaseEntry = t3.DocEntry
                    inner join OSLP t4 on t3.SlpCode =t4.SlpCode ";

                for (int i = 0; i < listTesterSn.Count; i++)
                {
                    listTesterSn[i] = "'" + listTesterSn[i] + "'";
                }
                var propertyStr = string.Join(',', listTesterSn);
                strSql += $" where t1.manufSN in ({propertyStr})";

                var shipmentCalibration =await  UnitWork.Query<ShipmentCalibration_sql>(strSql).ToListAsync();

                List<ShipmentCalibration> list = new List<ShipmentCalibration>();
                foreach (var item in listNwcali)
                {
                    ShipmentCalibration info = new ShipmentCalibration();
                    info.TesterModel = item.TesterModel;
                    info.TesterSn = item.TesterSn;
                    info.Time = item.Time.ToString("yyyy-MM-dd HH:mm:ss");
                    info.Operator = item.Operator;
                    info.IsSuer = item.Issuer;
                    var salesInfo = shipmentCalibration.FirstOrDefault(a => a.TesterSn == item.TesterSn);
                    if (salesInfo!=null)
                    {
                        info.SalesOrder = salesInfo.SalesOrder;
                        info.DeliveryNumber = salesInfo.DeliveryNumber;
                        info.Salesman = salesInfo.Salesman;
                    }
                    list.Add(info);
                }
          
                result.Data = list;

            }

            return result;
        }

        /// <summary>
        /// 校准报表2.0
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetCalibrateReportNew(QueryCertReportReq req)
        {
            var result = new TableData();

            Repository.Domain.Org Org = new Repository.Domain.Org();
            List<User> user = new List<User>();
            if (req.Type <= 0)
            {
                result.Code = 500;
                result.Message = "校准类型有误！";
                return result;
            }

            if (!string.IsNullOrEmpty(req.Org))
            {
                Org = await UnitWork.Find<Repository.Domain.Org>(a => a.Name == req.Org).FirstOrDefaultAsync();
                if (Org == null)
                {
                    result.Code = 500;
                    result.Message = "部门名字有误！";
                    return result;
                }
                user = (from r in UnitWork.Find<Relevance>(a => a.Key == "UserOrg" && a.SecondId == req.Org)
                        join u in UnitWork.Find<User>(null) on r.FirstId equals u.Id
                        select u).ToList();
            }
            int sum = 0;
            List<string> userIdList = user.Select(a => a.Id).ToList();
            if (req.Type == 1)
            {

            }
            else if (req.Type == 2)
            {
                var query = await UnitWork.Find<NwcaliBaseInfo>(c => c.CreateTime >= DateTime.Parse("2022-10-08") && !string.IsNullOrWhiteSpace(c.StartTime.ToString()))
                    .WhereIf(!string.IsNullOrEmpty(req.Name), a => a.Operator == req.Name)
                    .WhereIf(user.Count() > 0, a => userIdList.Contains(a.OperatorId))
                    .WhereIf(req.StartTime != null, c => c.StartTime.Value >= req.StartTime.Value.Date)
                    .WhereIf(req.EndTime != null, c => c.StartTime.Value < req.EndTime.Value.AddDays(1).Date)
                    .WhereIf(req.CompleteStartTime != null, c => c.Time.Value >= req.CompleteStartTime.Value.Date)
                    .WhereIf(req.CompleteEndTime != null, c => c.Time.Value < req.CompleteEndTime.Value.AddDays(1).Date)
                    .WhereIf(!string.IsNullOrEmpty(req.TesterSn), a => a.TesterSn == req.TesterSn)
                    .WhereIf(!string.IsNullOrEmpty(req.TesterModel), a => a.TesterModel == req.TesterModel)
                    //.WhereIf(!string.IsNullOrEmpty(req.CalibrationStatus), a => a.CalibrationStatus == req.CalibrationStatus)
                    .Select(a => new { a.Time, a.StartTime, a.CalibrationStatus, a.Operator, a.OperatorId, a.TesterSn, a.CreateTime, a.TotalSeconds }).ToListAsync();
                var summary = query.GroupBy(c => c.TesterSn).Select(c => c.OrderByDescending(o => o.CreateTime).First()).ToList();
                var extra = new { Total = summary.Count(), OKCount = summary.Where(c => c.CalibrationStatus == "OK").Count(), NGCount = summary.Where(c => c.CalibrationStatus == "NG").Count() };
                summary = query.GroupBy(c => new { c.TesterSn, c.OperatorId }).Select(c => c.OrderByDescending(o => o.CreateTime).First()).ToList();
                //最新记录的状态
                if (!string.IsNullOrEmpty(req.CalibrationStatus))
                {
                    summary = summary.Where(c => c.CalibrationStatus == req.CalibrationStatus).ToList();
                }

                var userIds = summary.Select(c => c.OperatorId).ToList();
                var orgInfo = await (from r in UnitWork.Find<Relevance>(a => a.Key == "UserOrg")
                                     join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                                     where userIds.Contains(r.FirstId)
                                     select new { r.FirstId, o.Name, o.CascadeId }).ToListAsync();

                var summaryUser = summary.GroupBy(c => c.OperatorId).Select(c => new CalibrateReportResp
                {
                    Id = c.Key,
                    Name = c.First().Operator,
                    TotalCount = c.Count(),
                    OKCount = c.Where(w => w.CalibrationStatus == "OK").Count(),
                    NGCount = c.Where(w => w.CalibrationStatus == "NG").Count(),
                    OrgName = orgInfo.Where(w => w.FirstId == c.Key).FirstOrDefault()?.Name,
                    //Time = GetTime(c.Average(c => c.TotalSeconds.ToDouble()))
                    Time = c.Average(c => c.TotalSeconds.ToDouble())
                }).ToList();
                result.Extra = JsonHelper.Instance.Serialize(extra);
                result.Data = summaryUser;
            }
            return result;
        }

        /// <summary>
        /// 校准报表详情2.0
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetCalibrateDetailReportNew(QueryCertReportReq req)
        {
            var result = new TableData();

            if (string.IsNullOrEmpty(req.Id))
            {
                result.Code = 500;
                result.Message = "人员ID有误！";
                return result;
            }
            if (req.Type <= 0)
            {
                result.Code = 500;
                result.Message = "校准类型有误！";
                return result;
            }

            if (req.Type == 1)
            {

            }
            else if (req.Type == 2)
            {
                var query = await UnitWork.Find<NwcaliBaseInfo>(c => c.CreateTime >= DateTime.Parse("2022-10-08") && !string.IsNullOrWhiteSpace(c.StartTime.ToString())).Include(c => c.Etalons)
                    .WhereIf(!string.IsNullOrEmpty(req.Name), a => a.Operator == req.Name)
                    .WhereIf(!string.IsNullOrEmpty(req.Id), a => a.OperatorId == req.Id)
                    //.WhereIf(user.Count() > 0, a => userIdList.Contains(a.OperatorId))
                    .WhereIf(req.StartTime != null, c => c.StartTime.Value >= req.StartTime.Value.Date)
                    .WhereIf(req.EndTime != null, c => c.StartTime.Value < req.EndTime.Value.AddDays(1).Date)
                    .WhereIf(req.CompleteStartTime != null, c => c.Time.Value >= req.CompleteStartTime.Value.Date)
                    .WhereIf(req.CompleteEndTime != null, c => c.Time.Value < req.CompleteEndTime.Value.AddDays(1).Date)
                    .WhereIf(!string.IsNullOrEmpty(req.TesterSn), a => a.TesterSn == req.TesterSn)
                    .WhereIf(!string.IsNullOrEmpty(req.TesterModel), a => a.TesterModel == req.TesterModel)
                    //.WhereIf(!string.IsNullOrEmpty(req.CalibrationStatus), a => a.CalibrationStatus == req.CalibrationStatus)
                    .Select(a => new { a.Time, a.StartTime, a.CalibrationStatus, a.Operator, a.OperatorId, a.TesterSn, a.CreateTime, a.TotalSeconds, a.Issuer, a.TesterModel, a.CalibrationMode, a.Etalons }).ToListAsync();

                var listNwcali = query.GroupBy(c => c.TesterSn).Select(c => new
                {
                    c.First().TesterSn,
                    c.First().TesterModel,
                    //c.First().CalibrationStatus,
                    //Time = GetTime(c.Average(c => c.TotalSeconds.ToDouble())),
                    Time = c.Average(c => c.TotalSeconds.ToDouble()),
                    Count = c.Count(),
                    c.First().Issuer,
                    c.OrderByDescending(o => o.CreateTime).First().CalibrationStatus
                }).ToList();
                //最新记录的状态
                if (!string.IsNullOrEmpty(req.CalibrationStatus))
                {
                    listNwcali = listNwcali.Where(c => c.CalibrationStatus == req.CalibrationStatus).ToList();
                }
                result.Count = listNwcali.Count();
                listNwcali = listNwcali.Skip((req.page - 1) * req.limit)
                .Take(req.limit)
                .ToList();


                if (listNwcali.Count <= 0)
                {
                    return result;
                }
                var listTesterSn = listNwcali.Select(a => a.TesterSn).ToList();

                string strSql = @"select t4.SlpCode,t4.SlpName as 'Salesman',t3.DocEntry as'SalesOrder',t2.DocEntry as 'DeliveryNumber', t1.manufSN as 'TesterSn'from OINS t1
                    inner join (SELECT DocEntry,MIN(BaseEntry) as BaseEntry from DLN1 where BaseType=17  group by DocEntry )  t2 on t1.deliveryNo = t2.DocEntry 
                    inner join ORDR t3 on t2.BaseEntry = t3.DocEntry
                    inner join OSLP t4 on t3.SlpCode =t4.SlpCode ";

                for (int i = 0; i < listTesterSn.Count; i++)
                {
                    listTesterSn[i] = "'" + listTesterSn[i] + "'";
                }
                var propertyStr = string.Join(',', listTesterSn);
                strSql += $" where t1.manufSN in ({propertyStr})";

                var shipmentCalibration = await UnitWork.Query<ShipmentCalibration_sql>(strSql).ToListAsync();

                var calibration = from a in listNwcali
                                  join b in shipmentCalibration on a.TesterSn equals b.TesterSn into ab
                                  from b in ab.DefaultIfEmpty()
                                  select new
                                  {
                                      a.TesterSn,
                                      a.TesterModel,
                                      a.CalibrationStatus,
                                      a.Time,
                                      a.Count,
                                      a.Issuer,
                                      SalesOrder = b != null ? b.SalesOrder : null,
                                      DeliveryNumber = b != null ? b.DeliveryNumber : null,
                                      Salesman = b != null ? b.Salesman : null,
                                      Detail = query.Where(c => c.TesterSn == a.TesterSn).Select(c => new
                                      {
                                          c.StartTime,
                                          EndTime = c.Time,
                                          Time = GetTime(c.TotalSeconds.ToDouble()),
                                          c.CalibrationStatus,
                                          c.Operator,
                                          c.CalibrationMode,
                                          Standardizer = GetStandardizer(c.Etalons)
                                      }).ToList()
                                  };


                result.Data = calibration;
            }
            return result;
        }

        public string GetTime(double? seconds)
        {
            TimeSpan ts = TimeSpan.FromSeconds(Convert.ToDouble(seconds));
            string str = "";
            if (ts.Hours > 0)
            {
                str = ts.Hours.ToString() + "小时" + ts.Minutes.ToString() + "分钟" + ts.Seconds + "秒";
            }
            if (ts.Hours == 0 && ts.Minutes > 0)
            {
                str = ts.Minutes.ToString() + "分钟" + ts.Seconds + "秒";
            }
            if (ts.Hours == 0 && ts.Minutes == 0)
            {
                str = ts.Seconds + "秒";
            }
            return str;
        }

        public string GetStandardizer(List<Etalon> etalons)
        {
            StringBuilder sb = new StringBuilder();
            etalons.ForEach(c =>
            {
                sb.Append($"{c.AssetNo}({c.Name})\\");
            });
            return sb.ToString().TrimEnd('\\');
        }

        /// <summary>
        /// 第一签名人更改为庞远球/张平
        /// </summary>
        /// <param name="baseInfo">校准报表基础信息</param>
        /// <returns></returns>
        public async Task SetIssuser(NwcaliBaseInfo baseInfo)
        {
            var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_FirstSignAcount").Select(c => c.Name).ToListAsync();
            if (!category.Contains(baseInfo.Issuer))
            {
                string userName = category.FirstOrDefault();
                var uinfo = await UnitWork.Find<UserSign>(c => c.UserName == userName).FirstOrDefaultAsync();
                if (uinfo == null)
                { 
                    throw new CommonException("当前出证人暂无签名图片，请确认后上传。", 400100);
                }

                baseInfo.Issuer = uinfo.UserName;
                baseInfo.IssuerId = uinfo.UserId;
            }
        }
    }
}
