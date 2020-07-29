using System;
using System.Linq;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;


namespace OpenAuth.App
{
    public class CompletionReportApp : BaseApp<CompletionReport>
    {
        private RevelanceManagerApp _revelanceApp;
        private readonly AppUserMapApp _appUserMapApp;

        public CompletionReportApp(IUnitWork unitWork, IRepository<CompletionReport> repository, AppUserMapApp appUserMapApp,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
            _appUserMapApp = appUserMapApp;
        }
        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryCompletionReportListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("completionreport");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();
            var objs = UnitWork.Find<CompletionReport>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }


            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = objs.OrderBy(u => u.Id)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select($"new ({propertyStr})");
            result.Count = objs.Count();
            return result;
        }

        public async Task Add(AddOrUpdateCompletionReportReq req)
        {
            var obj = req.MapTo<CompletionReport>();

            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            //obj.CreateUserName = user.Name;
            //todo:补充或调整自己需要的字段
            var o = await Repository.AddAsync(obj);
            var pictures = req.Pictures.MapToList<CompletionReportPicture>();
            pictures.ForEach(r => r.CompletionReportId = o.Id);
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();
        }


        public void Update(AddOrUpdateCompletionReportReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<CompletionReport>(u => u.Id == obj.Id, u => new CompletionReport
            {
                ServiceWorkOrderId = obj.ServiceWorkOrderId,
                ServiceOrderId = obj.ServiceOrderId,
                FromTheme = obj.FromTheme,
                CustomerId = obj.CustomerId,
                CustomerName = obj.CustomerName,
                Contacter = obj.Contacter,
                ContactTel = obj.ContactTel,
                TerminalCustomer = obj.TerminalCustomer,
                MaterialCode = obj.MaterialCode,
                ManufacturerSerialNumber = obj.ManufacturerSerialNumber,
                TechnicianId = obj.TechnicianId,
                TechnicianName = obj.TechnicianName,
                BusinessTripDate = obj.BusinessTripDate,
                EndDate = obj.EndDate,
                BusinessTripDays = obj.BusinessTripDays,
                Becity = obj.Becity,
                Destination = obj.Destination,
                Longitude = obj.Longitude,
                Latitude = obj.Latitude,
                CompleteAddress = obj.CompleteAddress,
                ProblemDescription = obj.ProblemDescription,
                SolutionId = obj.SolutionId,
                ReplacementMaterialDetails = obj.ReplacementMaterialDetails,
                Legacy = obj.Legacy,
                Remark = obj.Remark,
                CreateTime = obj.CreateTime,
                CreateUserId = obj.CreateUserId,
                //UpdateTime = DateTime.Now,
                //UpdateUserId = user.Id,
                //UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }

        /// <summary>
        /// 填写完工报告单页面需要取到的服务工单信息。
        /// </summary>
        /// <param name="serviceOrderId">服务单ID</param>
        /// <param name="currentUserId">当前技术员Id</param>
        /// <returns></returns>
        public async Task<CompletionReportDetailsResp> GetOrderWorkInfoForAdd(int serviceOrderId, int currentUserId)
        {
            var result = new TableData();
            var obj = from a in UnitWork.Find<ServiceWorkOrder>(null)
                      join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                      from b in ab.DefaultIfEmpty()
                      select new { a, b };
            obj = obj.Where(o => o.b.Id == serviceOrderId && o.a.CurrentUserId == currentUserId);
            var query = await obj.Select(q => new
            {
                q.a.FromTheme,
                q.a.CurrentUserId,
                q.b.CustomerId,
                q.b.CustomerName,
                q.b.TerminalCustomer,
                q.a.ServiceOrderId,
                q.b.Contacter,
                q.b.ContactTel,
                q.a.ManufacturerSerialNumber,
                q.a.MaterialCode,
                ProblemDescription = "故障描述：" + q.a.TroubleDescription + "；过程描述：" + q.a.ProcessDescription
            }).FirstOrDefaultAsync();
            var thisworkdetail = query.MapTo<CompletionReportDetailsResp>();
            if (thisworkdetail.CurrentUserId != null)
            {
                int theuserid = thisworkdetail.CurrentUserId == null ? 0 : (int)thisworkdetail.CurrentUserId;
                thisworkdetail.TheNsapUser = await _appUserMapApp.GetFirstNsapUser(theuserid);
                thisworkdetail.TechnicianName = thisworkdetail.TheNsapUser == null ? "" : thisworkdetail.TheNsapUser.Name;
            }
            return thisworkdetail;
        }

        /// <summary>
        /// 获取完工报告详情
        /// </summary>
        /// <param name="serviceOrderId">服务单Id</param>
        /// <param name="currentUserId">当前技术员Id</param>
        /// <returns></returns>
        public async Task<CompletionReportDetailsResp> GetCompletionReportDetails(int serviceOrderId, int currentUserId)
        {
            var result = new TableData();
            var obj = from c in UnitWork.Find<CompletionReport>(null)
                      join a in UnitWork.Find<ServiceWorkOrder>(null) on c.ServiceOrderId equals a.ServiceOrderId
                      join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into abc
                      from b in abc.DefaultIfEmpty()
                      select new { a, b, c };
            obj = obj.Where(o => o.a.ServiceOrderId == serviceOrderId && o.a.CurrentUserId == currentUserId);
            var query = await obj.Select(q => new
            {
                q.a.FromTheme,
                q.a.CurrentUserId,
                q.b.CustomerId,
                q.b.CustomerName,
                q.b.TerminalCustomer,
                q.a.ServiceOrderId,
                q.b.Contacter,
                q.b.ContactTel,
                q.a.ManufacturerSerialNumber,
                q.a.MaterialCode,
                ProblemDescription = "故障描述：" + q.a.TroubleDescription + "；过程描述：" + q.a.ProcessDescription,
                q.c.Id
            }).FirstOrDefaultAsync();
            var thisworkdetail = query.MapTo<CompletionReportDetailsResp>();
            thisworkdetail.Files = new List<UploadFileResp>();
            if (thisworkdetail != null && thisworkdetail.TheNsapUser != null)
            {
                var pics = UnitWork.Find<CompletionReportPicture>(m => m.CompletionReportId.Equals(query.Id)).Select(c => c.PictureId).ToList();
                var picfiles = await UnitWork.Find<UploadFile>(f => pics.Contains(f.Id)).ToListAsync();
                thisworkdetail.Files.AddRange(picfiles.MapTo<List<UploadFileResp>>());
            }
            return thisworkdetail;
        }

    }
}