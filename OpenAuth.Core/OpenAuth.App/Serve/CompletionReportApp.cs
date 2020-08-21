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
using System.Reactive;

namespace OpenAuth.App
{
    public class CompletionReportApp : BaseApp<CompletionReport>
    {
        private RevelanceManagerApp _revelanceApp;
        private readonly AppUserMapApp _appUserMapApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        public CompletionReportApp(IUnitWork unitWork, IRepository<CompletionReport> repository, AppUserMapApp appUserMapApp,
            RevelanceManagerApp app, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, ServiceOrderLogApp ServiceOrderLogApp) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
            _appUserMapApp = appUserMapApp;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _ServiceOrderLogApp = ServiceOrderLogApp;
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
            var completionReportId = o.Id;
            var pictures = req.Pictures.MapToList<CompletionReportPicture>();
            pictures.ForEach(r => r.CompletionReportId = o.Id);
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && string.IsNullOrEmpty(req.MaterialType) ? true : "其他设备".Equals(req.MaterialType) ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == req.MaterialType, s => new ServiceWorkOrder { Status = 7 });
            var workOrderList = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && string.IsNullOrEmpty(req.MaterialType) ? true : "其他设备".Equals(req.MaterialType) ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == req.MaterialType).ToList();
            List<int> workorder = new List<int>();
            foreach (var item in workOrderList)
            {
                workorder.Add(item.Id);
            }
            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员完成服务",
                Details = $"感谢您对新威的支持。您的服务已完成，如有疑问请及时拨打客服电话：8008308866，新威客服会全力帮您继续跟进",
                LogType = 1,
                ServiceOrderId = req.ServiceOrderId
            }, workorder);
            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员完成售后维修",
                Details = $"提交了《行为服务报告单》，完成了本次任务",
                LogType = 2,
                ServiceOrderId = req.ServiceOrderId
            }, workorder);
            //反写完工报告Id至工单
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && string.IsNullOrEmpty(req.MaterialType) ? true : string.IsNullOrEmpty(s.MaterialCode) ? "其他设备" == s.ManufacturerSerialNumber : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == req.MaterialType,
                o => new ServiceWorkOrder { CompletionReportId = completionReportId });
            //解除隐私号码绑定
            await UnbindProtectPhone(req.ServiceOrderId, req.MaterialType);
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
        /// <param name="MaterialType">当前技术员Id</param>
        /// <returns></returns>
        public async Task<CompletionReportDetailsResp> GetOrderWorkInfoForAdd(int serviceOrderId, int currentUserId, string MaterialType)
        {
            var result = new TableData();
            var obj = from a in UnitWork.Find<ServiceWorkOrder>(null)
                      join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                      from b in ab.DefaultIfEmpty()
                      select new { a, b };
            obj = obj.Where(o => o.b.Id == serviceOrderId && o.a.CurrentUserId == currentUserId && string.IsNullOrEmpty(MaterialType) ? true : "其他设备".Equals(MaterialType) ? o.a.MaterialCode == "其他设备" : o.a.MaterialCode.Substring(0, o.a.MaterialCode.IndexOf("-")) == MaterialType);
            var query = await obj.Select(q => new
            {
                q.b.U_SAP_ID,
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
                ProblemDescription = "故障描述：" + q.a.TroubleDescription + "；解决方案：" + q.a.Solution.Subject
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
        public async Task<CompletionReportDetailsResp> GetCompletionReportDetails(int serviceOrderId, int currentUserId, string MaterialType)
        {
            var result = new TableData();
            var obj = from c in UnitWork.Find<CompletionReport>(null)
                      join a in UnitWork.Find<ServiceWorkOrder>(null) on c.ServiceOrderId equals a.ServiceOrderId
                      join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into abc
                      from b in abc.DefaultIfEmpty()
                      select new { a, b, c };
            obj = obj.Where(o => o.a.ServiceOrderId == serviceOrderId && o.a.CurrentUserId == currentUserId && string.IsNullOrEmpty(MaterialType) ? true : (string.IsNullOrEmpty(o.a.MaterialCode) ? "其他设备" : o.a.MaterialCode.Substring(0, o.a.MaterialCode.IndexOf("-"))) == MaterialType);
            var query = await obj.Select(q => new
            {
                q.b.U_SAP_ID,
                q.c.FromTheme,
                q.a.CurrentUserId,
                q.c.CustomerId,
                q.c.CustomerName,
                q.c.TerminalCustomer,
                q.c.ServiceOrderId,
                q.c.Contacter,
                q.c.ContactTel,
                q.a.ManufacturerSerialNumber,
                q.a.MaterialCode,
                q.c.ProblemDescription,
                q.c.Id,
                q.c.Becity,
                q.c.BusinessTripDate,
                q.c.BusinessTripDays,
                q.c.Destination,
                q.c.ReplacementMaterialDetails,
                q.c.Legacy,
                q.c.Remark,
                q.c.EndDate,
                q.c.CompleteAddress,
                q.c.TechnicianId,
                q.c.TechnicianName
            }).FirstOrDefaultAsync();
            var thisworkdetail = query.MapTo<CompletionReportDetailsResp>();
            thisworkdetail.Files = new List<UploadFileResp>();
            if (thisworkdetail != null)
            {
                var pics = UnitWork.Find<CompletionReportPicture>(m => m.CompletionReportId == query.Id).Select(c => c.PictureId).ToList();
                var picfiles = await UnitWork.Find<UploadFile>(f => pics.Contains(f.Id)).ToListAsync();
                thisworkdetail.Files.AddRange(picfiles.MapTo<List<UploadFileResp>>());
            }
            return thisworkdetail;
        }
        /// <summary>
        /// 获取完工报告详情Web by zlg 2020.08.12
        /// </summary>
        /// <param name="CompletionReportId"></param>
        /// <param name="ServiceWorkOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetCompletionReportDetailsWeb(string CompletionReportId, int ServiceWorkOrderId)
        {
            var result = new TableData();
            var ServiceWorkOrderModel = UnitWork.Find<ServiceWorkOrder>(u => u.Id == ServiceWorkOrderId).FirstOrDefault();
            var Files = new List<UploadFileResp>();
            var pics = UnitWork.Find<CompletionReportPicture>(m => m.CompletionReportId == CompletionReportId).Select(c => c.PictureId).ToList();
            var picfiles = await UnitWork.Find<UploadFile>(f => pics.Contains(f.Id)).ToListAsync();
            Files.AddRange(picfiles.MapTo<List<UploadFileResp>>());
            var CompletionReportModel = UnitWork.Find<CompletionReport>(u => u.Id == CompletionReportId).Select(L => new
            {
                id = L.Id,
                ServiceOrderId = L.ServiceOrderId,
                CustomerId = L.CustomerId,
                CustomerName = L.CustomerName,
                TechnicianName = L.TechnicianName,
                TerminalCustomerId = L.TerminalCustomerId,
                TerminalCustomer = L.TerminalCustomer,
                Contacter = L.Contacter,
                ContactTel = L.ContactTel,
                Becity = L.Becity,
                Destination = L.Destination,
                BusinessTripDate = L.BusinessTripDate,
                EndDate = L.EndDate,
                CompleteAddress = L.CompleteAddress,
                BusinessTripDays = L.BusinessTripDays,
                WorkOrderNumber = ServiceWorkOrderModel.WorkOrderNumber,
                ManufacturerSerialNumber = ServiceWorkOrderModel.ManufacturerSerialNumber,
                MaterialCode = ServiceWorkOrderModel.MaterialCode,
                Files = Files,
                ProblemDescription = L.ProblemDescription,
                ReplacementMaterialDetails = L.ReplacementMaterialDetails,
                Legacy = L.Legacy,
                Remark = L.Remark
            }).FirstOrDefault();
            result.Data = CompletionReportModel;
            return result;
        }
        /// <summary>
        /// 添加完工报告Web  by zlg 2020.08.12
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddWeb(AddOrUpdateCompletionReportReq req)
        {
            var obj = req.MapTo<CompletionReport>();
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            // = user.Id;
            //obj.CreateUserName = user.Name;
            //todo:补充或调整自己需要的字段
            //保存完工报告
            var o = await Repository.AddAsync(obj);
            //保存图片
            var pictures = req.Pictures.MapToList<CompletionReportPicture>();
            pictures.ForEach(r => r.CompletionReportId = o.Id);
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();
            //修改工单状态及反写工单号
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId, s => new ServiceWorkOrder { Status = 7, CompletionReportId = o.Id });
            var workOrderList = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId).ToList();
            List<int> workorder = new List<int>();
            foreach (var item in workOrderList)
            {
                workorder.Add(item.Id);
            }
            //保存日志
            await _ServiceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq
            {
                Action = $"技术员于{DateTime.Now}结束上门服务",
                ActionType = "服务技术员上门服务中",
            }, workorder);
        }

        /// <summary>
        /// 解除绑定隐私号码
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <returns></returns>
        public async Task<bool> UnbindProtectPhone(int? ServiceOrderId, string MaterialType)
        {
            var result = new TableData();
            //获取技术员Id
            int? TechnicianId = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ServiceOrderId && "其他设备".Equals(MaterialType) ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == MaterialType).Distinct().FirstOrDefaultAsync())?.CurrentUserId;
            var query = from a in UnitWork.Find<AppUserMap>(null)
                        join b in UnitWork.Find<User>(null) on a.UserID equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };
            //获取技术员联系方式
            string TechnicianTel = await query.Where(w => w.a.AppUserId == TechnicianId).Select(s => s.b.Mobile).FirstOrDefaultAsync();
            //获取客户联系方式
            string custMobile = (await UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId).FirstOrDefaultAsync()).NewestContactTel;
            //判断当前操作角色 0客户 1技术员
            if (!AliPhoneNumberProtect.Unbind(custMobile, TechnicianTel))
            {
                return false;
            }
            return true;
        }
    }
}