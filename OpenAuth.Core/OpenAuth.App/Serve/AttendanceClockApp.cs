using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Export;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npoi.Mapper;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class AttendanceClockApp : BaseApp<AttendanceClock>
    {
        private RevelanceManagerApp _revelanceApp;
        private IOptions<AppSetting> _appConfiguration;
        private HttpHelper _helper;

        public AttendanceClockApp(IUnitWork unitWork, IRepository<AttendanceClock> repository,
            RevelanceManagerApp app, IAuth auth, IOptions<AppSetting> appConfiguration) : base(unitWork, repository, auth)
        {
            _appConfiguration=appConfiguration;
            _revelanceApp = app;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
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
            if (request.Org.Count > 0) request.Org.ForEach(c => c.ToUpper());
            var result = new TableData();
            var objs = UnitWork.Find<AttendanceClock>(null).Include(a => a.AttendanceClockPictures);
            var ClockModels = objs.WhereIf(!string.IsNullOrEmpty(request.key), u => u.Id.Contains(request.key))
                .WhereIf(!string.IsNullOrEmpty(request.Name), u => u.Name.Contains(request.Name))
                .WhereIf(request.Org.Count>0, u => request.Org.Contains(u.Org))
                .WhereIf(!string.IsNullOrEmpty(request.VisitTo), u => u.VisitTo.Contains(request.VisitTo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Location), u => u.Location.Contains(request.Location))
                .WhereIf(request.DateFrom != null && request.DateTo != null, u => u.ClockDate >= request.DateFrom && u.ClockDate < Convert.ToDateTime(request.DateTo).AddMinutes(1440))
                ;
            // 主管只能看到本部门的技术员的打卡记录
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")) && !loginContext.Roles.Any(r => r.Name.Equals("考勤人员")))
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
            if (obj is null)
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

            var orgIds = _revelanceApp.Get(Define.USERORG, true, obj.UserId).ToList();
            var org = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgIds.Contains(o.Id)).OrderByDescending(o => o.CascadeId).FirstOrDefaultAsync();
            if (org == null)
            {
                throw new CommonException("您未绑定部门，请联系管理员", 80002);
            }
            obj.OrgId = org.Id;
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
        /// App技术员当天签到和签退
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        public async Task<TableData> AppGetClockCurrentHistory(int AppUserId)
        {
            var result = new TableData();
            DateTime dt = DateTime.Now.Date;
            var SignIn =await UnitWork.Find<AttendanceClock>(c => c.AppUserId == AppUserId && c.ClockDate==dt && c.ClockType==1).Include(c => c.AttendanceClockPictures).OrderBy(c => c.ClockTime).Select(c=>new { c.Location,c.ClockDate,c.ClockTime}).FirstOrDefaultAsync();
            var SignOut = await UnitWork.Find<AttendanceClock>(c => c.AppUserId == AppUserId && c.ClockDate == dt && c.ClockType == 2).Include(c => c.AttendanceClockPictures).OrderByDescending(c => c.ClockTime).Select(c => new { c.Location, c.ClockDate, c.ClockTime }).FirstOrDefaultAsync();
            result.Data = new { SignIn , SignOut };
            return result;
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

        /// <summary>
        /// 导出考勤
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportAttendanceClock(QueryAttendanceClockListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (request.Org.Count > 0) request.Org.ForEach(c => c.ToUpper());
            #region 查询条件
            var objs = UnitWork.Find<AttendanceClock>(null).Include(a => a.AttendanceClockPictures);
            var ClockModels = objs.WhereIf(!string.IsNullOrEmpty(request.key), u => u.Id.Contains(request.key))
                .WhereIf(!string.IsNullOrEmpty(request.Name), u => u.Name.Contains(request.Name))
                .WhereIf(request.Org.Count > 0, u => request.Org.Contains(u.Org))
                .WhereIf(!string.IsNullOrEmpty(request.VisitTo), u => u.VisitTo.Contains(request.VisitTo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Location), u => u.Location.Contains(request.Location))
                .WhereIf(request.DateFrom != null && request.DateTo != null, u => u.ClockDate >= request.DateFrom && u.ClockDate < Convert.ToDateTime(request.DateTo).AddMinutes(1440))
                ;
            // 主管只能看到本部门的技术员的打卡记录
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")) && !loginContext.Roles.Any(r => r.Name.Equals("考勤人员")))
            {
                var userIds = _revelanceApp.Get(Define.USERORG, false, loginContext.Orgs.Select(o => o.Id).ToArray());
                ClockModels = ClockModels.Where(q => userIds.Contains(q.UserId));
            }
            var listobj = ClockModels.OrderBy(c => c.ClockDate).ThenBy(c => c.ClockTime).ToList();
            //listobj.ForEach(s => s.ClockDate = s.ClockDate + s.ClockTime);
            #endregion
            // Name 姓名,org 部门,ClockDate 打卡日期,ClockTime,location 详细地址,VisitTo 拜访对象,Remark 备注
            var AttendanceClockList = listobj.Select(u => new
            {
                姓名 = u.Name,
                部门 = u.Org,
                打卡日期 = (u.ClockDate + u.ClockTime).ToString("yyyy-MM-dd HH:mm:ss"),
                详细地址 = u.Location,
                拜访对象 = u.VisitTo,
                备注 = u.Remark
            }).ToList();
            return await ExportAllHandler.ExporterExcel(AttendanceClockList);
        }


        #region  App打卡提醒消息通知
        /// <summary>
        /// App打卡推送提醒
        /// </summary>
        /// <returns></returns>
        public async Task AppClockMessageNotic()
        {
            DateTime dt = DateTime.Now.Date;
            var serviceOrderUserList = await (from a in UnitWork.Find<ServiceWorkOrder>(null)
                                              join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id
                                              where a.Status >= 2 && a.Status <= 5 && b.VestInOrg != 2
                                              select a.CurrentUserId.Value).Distinct().ToListAsync();
            var hasClockUser = await UnitWork.Find<AttendanceClock>(null).Where(c => c.ClockDate == dt).Select(c => c.AppUserId).ToListAsync();
            var noticMessageUser = serviceOrderUserList.Except(hasClockUser).ToList();
            string title = "考勤打卡";
            string content = "您今天还未打卡签到,请立即前往>>";
            string payload= "{\"urlType\":1,\"url\":\"/pages/afterSale/mechanic/outWork\"}";
            var str = _helper.Post(new
            {
                userIds = noticMessageUser,
                title = title,
                content = content,
                payload = payload
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "Message/AppExternalMessagePush", "", "");

        }
        #endregion
    }
}