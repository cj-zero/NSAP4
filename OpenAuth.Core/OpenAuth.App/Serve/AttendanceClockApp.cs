using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Npoi.Mapper;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class AttendanceClockApp : BaseApp<AttendanceClock>
    {
        private RevelanceManagerApp _revelanceApp;


        public AttendanceClockApp(IUnitWork unitWork, IRepository<AttendanceClock> repository,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryAttendanceClockListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("attendanceclock");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }
            var result = new TableData();
            var objs = UnitWork.Find<AttendanceClock>(null).Include(a=>a.AttendanceClockPictures);
            var ClockModels = objs.WhereIf(!string.IsNullOrEmpty(request.key), u => u.Id.Contains(request.key))
                .WhereIf(!string.IsNullOrEmpty(request.Name), u => u.Name.Contains(request.Name))
                .WhereIf(!string.IsNullOrEmpty(request.Org), u => u.Org.Contains(request.Org.ToUpper()))
                .WhereIf(!string.IsNullOrEmpty(request.VisitTo), u => u.VisitTo.Contains(request.VisitTo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Location),u=> u.Location.Contains(request.Location))
                .WhereIf(request.DateFrom != null && request.DateTo != null, u => u.ClockDate >= request.DateFrom && u.ClockDate < Convert.ToDateTime(request.DateTo).AddMinutes(1440))
                ;
            // 主管只能看到本部门的技术员的打卡记录
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")))
            {
                var userIds = _revelanceApp.Get(Define.USERORG, false, loginContext.Orgs.Select(o => o.Id).ToArray());
                ClockModels = ClockModels.Where(q => userIds.Contains(q.UserId));
            }
            var listobj = ClockModels.ToList();
            listobj.ForEach(s => s.ClockDate = s.ClockDate + s.ClockTime);

            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = listobj.OrderByDescending(u => u.ClockDate)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit);
            result.Count = listobj.Count();
            return result;

        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="id">考勤记录Id</param>
        /// <returns></returns>
        public async Task<AttendanceClockDetailsResp> GetDetails(string id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("attendanceclock");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }
            var obj = await UnitWork.Find<AttendanceClock>(null).Include(a => a.AttendanceClockPictures).FirstOrDefaultAsync();
            if(obj is null)
            {
                throw new Exception("找不到打卡记录");
            }
            var pitrureIds = obj.AttendanceClockPictures.Select(a => a.PictureId).ToList();
            var pictrues = await UnitWork.Find<UploadFile>(f => pitrureIds.Contains(f.Id)).ToListAsync();

            var result = obj.MapTo<AttendanceClockDetailsResp>();
            result.Files = pictrues.MapTo<List<UploadFileResp>>();

            return result;
        }

        public async Task Add(AddOrUpdateAttendanceClockReq req)
        {
            var obj = req.MapTo<AttendanceClock>();
            //todo:补充或调整自己需要的字段
            var u = await UnitWork.FindSingleAsync<AppUserMap>(a => a.AppUserId == req.AppUserId);
            if (u is null)
            {
                throw new CommonException("当前APP用户未绑定NSAP用户", 80001);
            }
            var user = await UnitWork.FindSingleAsync<User>(s => s.Id.Equals(u.UserID));
            obj.UserId = user.Id;
            obj.Name = user.Name;

            var orgId = _revelanceApp.Get(Define.USERORG, true, obj.UserId).FirstOrDefault();
            obj.OrgId = orgId;

            var org = await UnitWork.FindSingleAsync<OpenAuth.Repository.Domain.Org>(o => o.Id.Equals(orgId));
            obj.Org = org.Name;
            var o = await Repository.AddAsync(obj);
            var pistures = req.Pictures.MapToList<AttendanceClockPicture>();
            pistures.ForEach(p => p.AttendanceClockId = o.Id);
            await UnitWork.BatchAddAsync(pistures.ToArray());
            await UnitWork.SaveAsync();
        }

        public void Update(AddOrUpdateAttendanceClockReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<AttendanceClock>(u => u.Id == obj.Id, u => new AttendanceClock
            {
                Name = obj.Name,
                UserId = obj.UserId,
                Org = obj.Org,
                OrgId = obj.OrgId,
                ClockDate = obj.ClockDate,
                ClockTime = obj.ClockTime,
                Longitude = obj.Longitude,
                Latitude = obj.Latitude,
                Location = obj.Location,
                SpecificLocation = obj.SpecificLocation,
                VisitTo = obj.VisitTo,
                Remark = obj.Remark,
                PhoneId = obj.PhoneId,
                Ip = obj.Ip,
                //todo:补充或调整自己需要的字段
            });
            UnitWork.Save();
        }

        /// <summary>
        /// App技术员查询打卡记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AppGetClockHistory(AppGetClockHistoryReq req)
        {
            var result = new TableData();
            var query = UnitWork.Find<AttendanceClock>(c => c.AppUserId == req.AppUserId).Include(c => c.AttendanceClockPictures).OrderByDescending(c => c.ClockDate).ThenByDescending(c => c.ClockTime);


            var count = await query.CountAsync();
            var data = await query.Skip((req.page - 1) * req.limit).Take(req.limit)
                .ToListAsync();

            result.Count = count;
            result.Data = data;
            return result;
        }

    }
}