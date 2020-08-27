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
    /// <summary>
    /// 售后评价
    /// </summary>
    public class ServiceEvaluateApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryServiceEvaluateListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("serviceevaluate");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<ServiceEvaluate>(null)
                .WhereIf(request.ServiceOrderId != null, s => s.ServiceOrderId == request.ServiceOrderId)
                .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerId), s => s.CustomerId.Contains(request.CustomerId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.TechnicianId), s => s.TechnicianId.Equals(request.TechnicianId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.VisitPeopleId), s => s.VisitPeopleId.Equals(request.VisitPeopleId))
                .WhereIf(request.DateFrom != null && request.DateTo != null, s => s.CommentDate >= request.DateFrom && s.CommentDate <= request.DateTo)
                ;
            //if (!string.IsNullOrEmpty(request.key))
            //{
            //    objs = objs.Where(u => u.Id.Contains(request.key));
            //}
            
            // 主管只能看到本部门的技术员的评价
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")))
            {
                var userIds = _revelanceApp.Get(Define.USERORG, false, loginContext.Orgs.Select(o => o.Id).ToArray());
                objs = objs.Where(q => userIds.Contains(q.TechnicianId));
            }

            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = objs.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }

        /// <summary>
        /// App加载列表
        /// </summary>
        public async Task<TableData> AppLoad(QueryServiceEvaluateListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("serviceevaluate");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}


            var result = new TableData();
            var objs = UnitWork.Find<ServiceEvaluate>(null)
                .WhereIf(request.ServiceOrderId != null, s => s.ServiceOrderId == request.ServiceOrderId)
                .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerId), s => s.CustomerId.Contains(request.CustomerId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.TechnicianId), s => s.TechnicianId.Equals(request.TechnicianId))
                .WhereIf(!string.IsNullOrWhiteSpace(request.VisitPeopleId), s => s.VisitPeopleId.Equals(request.VisitPeopleId))
                .WhereIf(request.DateFrom != null && request.DateTo != null, s => s.CommentDate >= request.DateFrom && s.CommentDate < Convert.ToDateTime(request.DateTo).AddMinutes(1440))
                ;
            
            //if (!string.IsNullOrEmpty(request.key))
            //{
            //    objs = objs.Where(u => u.Id.Contains(request.key));
            //}


            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = await objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();//.Select($"new ({propertyStr})");
            result.Count = await objs.CountAsync();
            return result;
        }
        public async Task<ServiceEvaluate> Get(long id)
        {
            return await UnitWork.FindSingleAsync<ServiceEvaluate>(s => s.ServiceOrderId == id);
        }

        public async Task Add(AddOrUpdateServiceEvaluateReq req)
        {
            var obj = req.MapTo<ServiceEvaluate>();

            if (req.TechnicianAppId.HasValue)
            {
                var appUser = await UnitWork.Find<AppUserMap>(null).Include(a => a.User).FirstOrDefaultAsync(a => a.AppUserId == req.TechnicianAppId.Value);
                obj.Technician = appUser.User.Name;
                obj.TechnicianId = appUser.UserID;
            }
            else
            {
                var appUser = await UnitWork.FindSingleAsync<AppUserMap>(a => a.UserID == req.TechnicianId);
                obj.TechnicianAppId = appUser.AppUserId;
            }

            //todo:补充或调整自己需要的字段
            obj.CommentDate = DateTime.Now;
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.VisitPeople = user.Name == "APP" ? "" : user.Name;
            obj.VisitPeopleId = user.Id;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            await UnitWork.AddAsync<ServiceEvaluate, long>(obj);
            await UnitWork.SaveAsync();
        }
        public async Task AppAdd(APPAddServiceEvaluateReq req)
        {
            var order = await UnitWork.FindSingleAsync<ServiceOrder>(s => s.Id == req.ServiceOrderId);
            req.CustomerId = order.CustomerId;
            req.Cutomer = order.CustomerName;
            foreach (var technicianEvaluates in req.TechnicianEvaluates)
            {
                var obj = req.MapTo<ServiceEvaluate>();

                if (technicianEvaluates.TechnicianAppId.HasValue)
                {
                    var appUser = await UnitWork.Find<AppUserMap>(null).Include(a => a.User).FirstOrDefaultAsync(a => a.AppUserId == technicianEvaluates.TechnicianAppId.Value);
                    obj.Technician = appUser.User.Name;
                    obj.TechnicianId = appUser.UserID;
                    obj.TechnicianAppId = technicianEvaluates.TechnicianAppId;
                }
                obj.SchemeEffectiveness = technicianEvaluates.SchemeEffectiveness;
                obj.ServiceAttitude = technicianEvaluates.ServiceAttitude;
                obj.ResponseSpeed = technicianEvaluates.ResponseSpeed;
                //todo:补充或调整自己需要的字段
                obj.CommentDate = DateTime.Now;
                obj.CreateTime = DateTime.Now;
                var user = _auth.GetCurrentUser().User;
                obj.VisitPeople = user.Name == "APP" ? "" : user.Name;
                obj.VisitPeopleId = user.Id;
                obj.CreateUserId = user.Id;
                obj.CreateUserName = user.Name;
                await UnitWork.AddAsync<ServiceEvaluate, long>(obj);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId, o => new ServiceWorkOrder
            {
                Status = 8
            });
            await UnitWork.SaveAsync();
        }

        public async Task Update(AddOrUpdateServiceEvaluateReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            await UnitWork.UpdateAsync<ServiceEvaluate>(u => u.Id == obj.Id, u => new ServiceEvaluate
            {
                ResponseSpeed = obj.ResponseSpeed,
                SchemeEffectiveness = obj.SchemeEffectiveness,
                ServiceAttitude = obj.ServiceAttitude,
                ProductQuality = obj.ProductQuality,
                ServicePrice = obj.ServicePrice,
                Comment = obj.Comment
                //todo:补充或调整自己需要的字段
            });
            await UnitWork.SaveAsync();

        }

        public async Task Delete(long[] ids)
        {
            await UnitWork.DeleteAsync<ServiceEvaluate>(s => ids.Contains(s.Id));
        }

        public async Task<List<int>> GetTechnicianAppIds(int serviceOrderId)
        {
            return await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == serviceOrderId).Select(s => s.CurrentUserId.Value).Distinct().ToListAsync();
        }


        public ServiceEvaluateApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }
    }
}