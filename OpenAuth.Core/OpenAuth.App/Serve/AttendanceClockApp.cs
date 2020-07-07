using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
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
            var objs = UnitWork.Find<AttendanceClock>(null);
            objs = objs.WhereIf(!string.IsNullOrEmpty(request.key), u => u.Id.Contains(request.key));


            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.count = objs.Count();
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

        public void Add(AddOrUpdateAttendanceClockReq req)
        {
            var obj = req.MapTo<AttendanceClock>();
            //todo:补充或调整自己需要的字段
            var user = _auth.GetCurrentUser().User;
            Repository.Add(obj);
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

        }
            
    }
}