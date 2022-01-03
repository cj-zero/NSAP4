using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
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

        public async Task<NwcaliBaseInfo> GetInfo(string certNo)
        {
            var info = await UnitWork.Find<NwcaliBaseInfo>(null).Include(b => b.NwcaliTurs).Include(b => b.NwcaliPlcDatas).Include(b => b.PcPlcs).Include(b => b.Etalons)
                .FirstOrDefaultAsync(b => b.CertificateNumber.Equals(certNo));
            return info;
        }

        public async Task<List<NwcaliBaseInfo>> GetNwcaliList(List<string> certNo)
        {
            return await UnitWork.Find<NwcaliBaseInfo>(b => certNo.Contains(b.CertificateNumber)).ToListAsync();
        }

        public async Task<List<NwcaliBaseInfo>> GetInfoList(List<string> certNo)
        {
            var info = await UnitWork.Find<NwcaliBaseInfo>(b=> certNo.Contains(b.CertificateNumber)).Include(b => b.NwcaliTurs).Include(b => b.NwcaliPlcDatas).Include(b => b.PcPlcs).Include(b => b.Etalons)
                .ToListAsync();
            return info;
        }

        public async Task<dynamic> GetPcPlcs(string guid)
        {
            var ids = await UnitWork.Find<PcPlc>(p => p.Guid.Equals(guid)).Select(o=>o.NwcaliBaseInfoId).ToListAsync();
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
            var nos = await UnitWork.Find<NwcaliBaseInfo>(c => c.CertificateNumber.Contains($"NW{type}{year}{month}")).Select(b=>b.CertificateNumber).ToListAsync();
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
    }
}
