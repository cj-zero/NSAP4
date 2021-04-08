using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class RealTimeLocationApp : BaseApp<RealTimeLocation>
    {
        private RevelanceManagerApp _revelanceApp;
        public RealTimeLocationApp(IUnitWork unitWork, IRepository<RealTimeLocation> repository,
    RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(int ServiceOrderId, string UserId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var map = await UnitWork.Find<AppUserMap>(w => w.UserID == UserId).FirstOrDefaultAsync();
            if (map == null)
            {
                throw new CommonException("当前用户未绑定App", Define.INVALID_TOKEN);
            }
            string startDate = string.Empty;
            string endDate = string.Empty;
            //获取当前服务单下技术员填写的日报信息
            var dailyReports = await UnitWork.Find<ServiceDailyReport>(w => w.ServiceOrderId == ServiceOrderId && w.CreateUserId == UserId).Select(s => s.CreateTime).ToListAsync();
            if (dailyReports != null)
            {
                startDate = dailyReports.Min()?.ToString("yyyy-MM-dd 00:00:00");
                endDate = dailyReports.Max()?.ToString("yyyy-MM-dd 23:59:59");
            }
            else
            {
                //获取完工报告下的出差开始时间与出差结束时间
                var completeReports = await UnitWork.Find<CompletionReport>(w => w.ServiceOrderId == ServiceOrderId && w.CreateUserId == UserId).FirstOrDefaultAsync();
                if (completeReports != null)
                {
                    if (completeReports.BusinessTripDate == null || completeReports.EndDate == null)
                    {
                        startDate = completeReports.CreateTime?.ToString("yyyy-MM-dd 00:00:00");
                        endDate = completeReports.CreateTime?.ToString("yyyy-MM-dd 23:59:59");
                    }
                    else
                    {
                        startDate = completeReports.BusinessTripDate?.ToString("yyyy-MM-dd 00:00:00");
                        endDate = completeReports.EndDate?.ToString("yyyy-MM-dd 23:59:59");
                    }

                }
            }
            var result = new TableData();
            var objs = await UnitWork.Find<RealTimeLocation>(w => w.AppUserId == (int)map.AppUserId && w.CreateTime >= Convert.ToDateTime(startDate) && w.CreateTime <= Convert.ToDateTime(endDate)).OrderBy(o => o.CreateTime).Select(s => new { s.Latitude, s.Longitude, s.CreateTime }).ToListAsync();
            var data = objs.GroupBy(g => g.CreateTime.Date).Select(s => new { date = s.Key, list = s.ToList() }).ToList();
            result.Count = objs.Count();
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 添加定位信息
        /// </summary>
        /// <param name="req"></param>
        public async Task Add(AddOrUpdaterealtimelocationReq req)
        {
            var obj = req.MapTo<RealTimeLocation>();
            obj.CreateTime = DateTime.Now;
            //todo:补充或调整自己需要的字段
            //判断是否已存在记录 若存在则做更新操作
            //var locations = await UnitWork.Find<RealTimeLocation>(r => r.AppUserId == req.AppUserId).FirstOrDefaultAsync();
            //if (locations != null)
            //{
            //    obj.Id = locations.Id;
            //    Repository.Update(obj);
            //}
            //else
            //{
            Repository.Add(obj);
            //}
        }
    }
}