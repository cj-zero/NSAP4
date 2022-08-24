using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
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
                var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_NwcilSignAcount" && c.Name.Contains(user.Name)).Select(c => c.DtValue).FirstOrDefaultAsync();
                if (category != null)
                {
                    var uinfo = await UnitWork.Find<UserSign>(c => c.UserName == category).FirstOrDefaultAsync();
                    if (uinfo == null)
                    {
                        throw new CommonException("当前出证人暂无签名图片，请确认后上传。", 400100);
                    }
                    baseInfo.Issuer = uinfo.UserName;
                    baseInfo.IssuerId = uinfo.UserId;
                }
                else
                {
                    baseInfo.Issuer = user.Name;
                    baseInfo.IssuerId = user.Id;
                }
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
            if (!string.IsNullOrEmpty(req.Org))
            {
                Org = await UnitWork.Find<Repository.Domain.Org>(a => a.Name == req.Org).FirstOrDefaultAsync();
                if (Org == null)
                {
                    result.Code = 500;
                    result.Message = "部门名字有误！";
                    return result;
                }
                user = (from r in UnitWork.Find<Relevance>(a => a.Key == "UserRole" && a.SecondId == req.Org)
                        join u in UnitWork.Find<User>(null) on r.FirstId equals u.Id
                        select u).ToList();
            }
            int sum = 0;
            List<string> userIdList = user.Select(a => a.Id).ToList();

            if (req.Type == 1)
            {
                var query = UnitWork.Find<ProductionSchedule>(a => a.NwcailStatus == 2)
                    .WhereIf(string.IsNullOrEmpty(req.Name), a => a.DeviceOperator == req.Name)
                    .WhereIf(user.Count() > 0, a => userIdList.Contains(a.DeviceOperatorId))
                    .WhereIf(req.StartTime != null, c => c.NwcailTime.Value >= req.StartTime)
                    .WhereIf(req.EndTime != null, c => c.NwcailTime.Value < req.EndTime)
                    .GroupBy(a => a.DeviceOperatorId)
                    .Select(a => new { id = a.Key, name = a.Max(b => b.DeviceOperator), num = a.Count() });

                sum = query.Sum(a => a.num);
                var list = await query.OrderByDescending(a => a.num)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();


                var data = (from l in list
                            join r in UnitWork.Find<Relevance>(a => a.Key == "UserRole") on l.id equals r.FirstId
                            join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                            select new { l, orgName = o.Name }).ToList();
                result.Data = data;
                result.Extra = sum.ToString();

            }
            else
            {
                var query = UnitWork.Find<NwcaliBaseInfo>(null)
                    .WhereIf(string.IsNullOrEmpty(req.Name), a => a.Operator == req.Name)
                    .WhereIf(user.Count() > 0, a => userIdList.Contains(a.OperatorId))
                    .WhereIf(req.StartTime != null, c => c.Time.Value >= req.StartTime)
                    .WhereIf(req.EndTime != null, c => c.Time.Value < req.EndTime)
                    .GroupBy(a => a.OperatorId)
                    .Select(a => new { id = a.Key, name = a.Max(b => b.Operator), num = a.Count() });

                sum = query.Sum(a => a.num);
                var list = await query.OrderByDescending(a => a.num)
                .Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();


                var data = (from l in list
                            join r in UnitWork.Find<Relevance>(a => a.Key == "UserRole") on l.id equals r.FirstId
                            join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                            select new { l, orgName = o.Name }).ToList();
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



            if (req.Type == 1)
            {
                var query = await UnitWork.Find<ProductionSchedule>(a => a.NwcailStatus == 2)
                     .WhereIf(string.IsNullOrEmpty(req.Name), a => a.DeviceOperator == req.Name)
                      .WhereIf(req.StartTime != null, c => c.NwcailTime.Value >= req.StartTime)
                     .WhereIf(req.EndTime != null, c => c.NwcailTime.Value < req.EndTime)
                     .ToListAsync();
                var listDocEntry = query.Select(a => a.DocEntry).ToList();

                List<ProductionCalibrationResp> list = new List<ProductionCalibrationResp>();

                var listOWOR = await UnitWork.Find<OWOR>(a => listDocEntry.Contains(a.DocEntry)).ToListAsync();
                foreach (var item in query)
                {
                    ProductionCalibrationResp info = new ProductionCalibrationResp();
                    info.GeneratorCode =item.GeneratorCode;
                    info.NwcailTime = item.NwcailTime;
                    info.ReceiveOperator = item.ReceiveOperator;
                    var owor = listOWOR.Where(a => a.DocEntry == item.DocEntry).FirstOrDefault();
                    if (owor != null)
                    {
                        info.Department = owor.U_WO_LTDW;
                        info.TesterModel = owor.ItemCode;
                    }
                    list.Add(info);
                }
                result.Data = list;
            }
            else
            {
                var query = await UnitWork.Find<NwcaliBaseInfo>(null)
                    .WhereIf(string.IsNullOrEmpty(req.Name), a => a.Operator == req.Name)
                    .WhereIf(req.StartTime != null, c => c.Time.Value >= req.StartTime)
                    .WhereIf(req.EndTime != null, c => c.Time.Value < req.EndTime)
                    .ToListAsync();
                if (query.Count <= 0)
                {
                    return result;
                }
                var listTesterSn = query.Select(a => a.TesterSn).ToList();

                string strSql = @"select t4.SlpCode,t4.SlpName as 'Salesman',t3.DocEntry as'SalesOrder',t2.DocEntry as 'DeliveryNumber', t1.manufSN as 'TesterSn' from OINS t1
                    inner join (SELECT DocEntry,MIN(BaseEntry) as BaseEntry from DLN1 where BaseType=17  group by DocEntry )  t2 on t1.deliveryNo = t2.DocEntry 
                    inner join ORDR t3 on t2.BaseEntry = t3.DocEntry
                    inner join OSLP t4 on t3.SlpCode =t4.SlpCode ";
              
                for (int i = 0; i < listTesterSn.Count; i++)
                {
                    listTesterSn[i] = "'" + listTesterSn[i] + "'";
                }
                var propertyStr = string.Join(',', listTesterSn);
                strSql += $" where t1.manufSN in ({propertyStr})";

                var shipmentCalibration = await UnitWork.Query<ShipmentCalibrationResp>(strSql).ToListAsync();

                foreach (var item in shipmentCalibration)
                {
                    var info = query.Where(a => a.TesterSn == item.TesterSn).FirstOrDefault();
                    if (info != null)
                    {
                        item.TesterModel = info.TesterModel;
                        item.TesterSn = info.TesterSn;
                        item.Time = info.Time.ToString("yyyy-MM-dd HH:mm:ss");
                        item.Operator = info.Operator;
                        item.GiveWitness = info.Operator;
                    }
                }
                result.Data = shipmentCalibration;

            }

            return result;
        }
    }
}
