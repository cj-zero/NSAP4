using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        /// <summary>
        /// 添加服务行为报告单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateCompletionReportReq req)
        {
            //添加之前判断是否有报告提交记录 若有则删除之前的完工报告
            var everCompletionReport = await UnitWork.Find<CompletionReport>(w => w.ServiceOrderId == req.ServiceOrderId && w.TechnicianId == req.CurrentUserId.ToString())
                .WhereIf("无序列号".Equals(req.MaterialType), a => a.MaterialCode == "无序列号")
                .WhereIf(!"无序列号".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType)
                .FirstOrDefaultAsync();
            var obj = req.MapTo<CompletionReport>();

            obj.CreateTime = DateTime.Now;
            //获取当前登陆者的nsap用户Id
            var nsapInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.CurrentUserId).Include(a => a.User).FirstOrDefaultAsync());
            obj.CreateUserId = nsapInfo.User.Id;
            obj.IsReimburse = 1;
            obj.TechnicianId = req.CurrentUserId.ToString();
            //obj.CreateUserName = user.Name;
            //todo:补充或调整自己需要的字段
            var o = await Repository.AddAsync(obj);
            var completionReportId = o.Id;
            var pictures = req.Pictures.MapToList<CompletionReportPicture>();
            pictures.ForEach(r => r.CompletionReportId = o.Id);
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();
            var workOrderList = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId)
    .WhereIf("无序列号".Equals(req.MaterialType), a => a.MaterialCode == "无序列号")
    .WhereIf(!"无序列号".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType)
    .ToListAsync());
            List<int> workorder = new List<int>();
            foreach (var item in workOrderList)
            {
                workorder.Add(item.Id);
            }
            if (req.IsRedeploy == 1)//若转派则将当前设备类型置为初始未派单状态
            {
                //记录转派记录
                var redeployInfo = new ServiceRedeploy
                {
                    ServiceOrderId = req.ServiceOrderId,
                    MaterialType = req.MaterialType,
                    TechnicianId = workOrderList.Select(s => s.CurrentUserId).FirstOrDefault(),
                    CreateTime = DateTime.Now,
                    WorkOrderIds = string.Join(",", workorder)
                };
                await UnitWork.AddAsync(redeployInfo);
                await UnitWork.SaveAsync();
                await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && workorder.Contains(s.Id),
                 o => new ServiceWorkOrder { Status = 1, OrderTakeType = 0, CurrentUser = string.Empty, CurrentUserId = 0, CurrentUserNsapId = string.Empty, BookingDate = null, VisitTime = null, ServiceMode = 0, CompletionReportId = string.Empty, TroubleDescription = string.Empty, ProcessDescription = string.Empty, IsCheck = 0, CompleteDate = null });
                //删除相对应的流程数据
                await UnitWork.DeleteAsync<ServiceFlow>(c => c.ServiceOrderId == req.ServiceOrderId && c.MaterialType == req.MaterialType);
            }
            //判断为非草稿提交 则修改对应状态和发送消息
            if (req.IsDraft == 0)
            {
                await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && workorder.Contains(s.Id), s => new ServiceWorkOrder { Status = 7 });
                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "技术员完成服务",
                    Details = $"感谢您对新威的支持。您的服务已完成，如有疑问请及时拨打客服电话：8008308866，新威客服会全力帮您继续跟进",
                    LogType = 1,
                    ServiceOrderId = req.ServiceOrderId,
                    ServiceWorkOrder = string.Join(',', workorder.ToArray()),
                    MaterialType = req.MaterialType
                });
                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "技术员完成售后维修",
                    Details = $"提交了《行为服务报告单》，完成了本次任务",
                    LogType = 2,
                    ServiceOrderId = req.ServiceOrderId,
                    ServiceWorkOrder = string.Join(',', workorder.ToArray()),
                    MaterialType = req.MaterialType
                });
                await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员:{nsapInfo.User.Name}完成了服务", ActionType = "完成服务单", ServiceOrderId = req.ServiceOrderId, MaterialType = req.MaterialType });
                //反写完工报告Id至工单
                await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && workorder.Contains(s.Id),
                    o => new ServiceWorkOrder { CompletionReportId = completionReportId, CompleteDate = DateTime.Now });
                //获取当前服务单下的所有消息Id集合
                var msgList = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == req.ServiceOrderId).Select(s => s.Id).ToListAsync();
                //清空消息为已读
                await UnitWork.UpdateAsync<ServiceOrderMessageUser>(s => s.FroUserId == req.CurrentUserId.ToString() && msgList.Contains(s.MessageId), e => new ServiceOrderMessageUser { HasRead = true });
            }
            //解除隐私号码绑定
            //await UnbindProtectPhone(req.ServiceOrderId, req.MaterialType);
            //删除草稿
            if (everCompletionReport != null)
            {
                await UnitWork.DeleteAsync<CompletionReport>(c => c.Id == everCompletionReport.Id);
            }
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
        /// <param name="MaterialType">当前技术员Id</param>
        /// <returns></returns>
        public async Task<CompletionReportDetailsResp> GetOrderWorkInfoForAdd(int serviceOrderId, int currentUserId, string MaterialType)
        {
            var thisworkdetail = new CompletionReportDetailsResp();
            //先查找是否之前填过完工报告（草稿）若有则拉取草稿报告单 否则取默认带出的数据
            var everCompletionReport = await UnitWork.Find<CompletionReport>(w => w.ServiceOrderId == serviceOrderId && w.TechnicianId == currentUserId.ToString())
               .WhereIf("无序列号".Equals(MaterialType), a => a.MaterialCode == "无序列号")
               .WhereIf(!"无序列号".Equals(MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == MaterialType)
               .FirstOrDefaultAsync();
            if (everCompletionReport != null)
            {
                thisworkdetail = everCompletionReport.MapTo<CompletionReportDetailsResp>();
                //获取工单中的问题类型和解决方案
                var troubleAndProcessInfo = await UnitWork.Find<ServiceWorkOrder>(w => w.ServiceOrderId == serviceOrderId && w.CurrentUserId == currentUserId)
               .WhereIf("无序列号".Equals(MaterialType), a => a.MaterialCode == "无序列号")
               .WhereIf(!"无序列号".Equals(MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == MaterialType).FirstOrDefaultAsync();
                thisworkdetail.TroubleDescription = troubleAndProcessInfo?.TroubleDescription;
                thisworkdetail.ProcessDescription = troubleAndProcessInfo?.ProcessDescription;
            }
            else
            {
                var obj = from a in UnitWork.Find<ServiceWorkOrder>(null)
                          join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                          from b in ab.DefaultIfEmpty()
                          select new { a, b };
                obj = obj.Where(o => o.b.Id == serviceOrderId && o.a.CurrentUserId == currentUserId)
                    .WhereIf("无序列号".Equals(MaterialType), q => q.a.MaterialCode.Equals("无序列号"))
                    .WhereIf(!"无序列号".Equals(MaterialType), q => q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-")) == MaterialType);
                var query = await obj.Select(q => new
                {
                    q.b.U_SAP_ID,
                    q.a.FromTheme,
                    q.a.CurrentUserId,
                    q.b.CustomerId,
                    q.b.CustomerName,
                    q.b.TerminalCustomer,
                    q.a.ServiceOrderId,
                    Contacter = string.IsNullOrEmpty(q.b.NewestContacter) ? q.b.Contacter : q.b.NewestContacter,
                    ContactTel = string.IsNullOrEmpty(q.b.NewestContactTel) ? q.b.ContactTel : q.b.NewestContactTel,
                    q.a.ManufacturerSerialNumber,
                    q.a.MaterialCode,
                    q.a.TroubleDescription,
                    q.a.ProcessDescription,
                    q.b.TerminalCustomerId,
                    q.a.ServiceMode
                }).FirstOrDefaultAsync();
                thisworkdetail = query.MapTo<CompletionReportDetailsResp>();
            }
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
        /// <param name="MaterialType">当前技术员Id</param>
        /// <returns></returns>
        public async Task<CompletionReportDetailsResp> GetCompletionReportDetails(int serviceOrderId, int currentUserId, string MaterialType)
        {
            var result = new TableData();
            var obj = from c in UnitWork.Find<CompletionReport>(null)
                      join a in UnitWork.Find<ServiceWorkOrder>(null) on c.ServiceOrderId equals a.ServiceOrderId
                      join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into abc
                      from b in abc.DefaultIfEmpty()
                      select new { a, b, c };
            obj = obj.Where(o => o.c.ServiceOrderId == serviceOrderId && o.c.TechnicianId == currentUserId.ToString())
                     .WhereIf("无序列号".Equals(MaterialType), q => q.c.MaterialCode.Equals("无序列号"))
                     .WhereIf(!"无序列号".Equals(MaterialType), q => q.c.MaterialCode.Substring(0, q.c.MaterialCode.IndexOf("-")) == MaterialType);
            var query = await obj.Select(q => new
            {
                q.b.U_SAP_ID,
                q.c.FromTheme,
                q.a.CurrentUserId,
                q.c.CustomerId,
                q.c.CustomerName,
                q.c.TerminalCustomer,
                q.c.ServiceOrderId,
                Contacter = string.IsNullOrEmpty(q.b.NewestContacter) ? q.b.Contacter : q.b.NewestContacter,
                ContactTel = string.IsNullOrEmpty(q.b.NewestContactTel) ? q.b.ContactTel : q.b.NewestContactTel,
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
                q.c.TechnicianName,
                q.a.TroubleDescription,
                q.a.ProcessDescription,
                q.c.ServiceMode
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
        /// <param name="ServiceOrderId"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetCompletionReportDetailsWeb(int ServiceOrderId, string UserId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            var CompletionReports = await UnitWork.Find<CompletionReport>(u => u.ServiceOrderId == ServiceOrderId).Include(u => u.CompletionReportPictures).ToListAsync();
            var ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(w => w.ServiceOrderId.Equals(ServiceOrderId)).ToListAsync();
            var U_SAP_ID = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(ServiceOrderId)).Select(s => s.U_SAP_ID).FirstOrDefaultAsync();
            if (!loginContext.Roles.Any(r => r.Name.Equals("售后主管")) && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")))
            {
                if (UserId == null)
                {
                    CompletionReports = CompletionReports.Where(c => c.CreateUserId.Equals(loginContext.User.Id)).ToList();
                    ServiceWorkOrders = ServiceWorkOrders.Where(c => c.CurrentUserNsapId.Equal(loginContext.User.Id)).ToList();
                }
            }
            if (UserId != null)
            {
                CompletionReports = CompletionReports.Where(c => c.CreateUserId.Equals(UserId)).ToList();
            }

            var CompletionReportResps = CompletionReports.MapToList<CompletionReportDetailsResp>();
            var Materialworks = ServiceWorkOrders.Select(w => w.MaterialCode == "无序列号" ? "无序列号" : w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-"))).ToList();

            Materialworks = Materialworks.Distinct().ToList();
            var MaterialTypes = await UnitWork.Find<MaterialType>(m => Materialworks.Contains(m.TypeAlias)).ToListAsync();
            List<string> fileids = new List<string>();
            CompletionReports.ForEach(c => fileids.AddRange(c.CompletionReportPictures.Select(p => p.PictureId).ToArray()));
            var picfiles = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();

            CompletionReportResps.ForEach(c =>
            {
                var fileids = CompletionReports.FirstOrDefault(m => m.Id.Equals(c.Id)).CompletionReportPictures.Select(p => p.PictureId).ToArray();
                c.Files = picfiles.Where(p => fileids.Contains(p.Id)).MapToList<UploadFileResp>();
                var worklist = ServiceWorkOrders.Where(w => w.CompletionReportId == c.Id).ToList();
                if (worklist != null && worklist.Count > 0)
                {
                    c.ServiceWorkOrders = worklist.MapToList<WorkCompletionReportResp>();
                    c.ProcessDescription = worklist.FirstOrDefault().ProcessDescription;
                    c.TroubleDescription = worklist.FirstOrDefault().TroubleDescription;
                    c.U_SAP_ID = U_SAP_ID.ToString();
                }
                c.MaterialCodeTypeName = c.MaterialCode == "无序列号" ? "无序列号" : MaterialTypes.FirstOrDefault(m => m.TypeAlias.Equal(c.MaterialCode.Substring(0, c.MaterialCode.IndexOf("-")))).TypeName;
            });

            Materialworks = ServiceWorkOrders.Where(s => string.IsNullOrWhiteSpace(s.CompletionReportId)).Select(w => w.MaterialCode == "无序列号" ? "无序列号" : w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-"))).ToList();

            Materialworks = Materialworks.Distinct().ToList();
            foreach (var item in Materialworks)
            {
                CompletionReportResps.Add(new CompletionReportDetailsResp
                {
                    MaterialCodeTypeName = item == "无序列号" ? "无序列号" : MaterialTypes.FirstOrDefault(m => m.TypeAlias.Equal(item)).TypeName
                });
            }
            result.Data = CompletionReportResps;
            return result;
        }
        /// <summary>
        /// 添加完工报告Web  by zlg 2020.08.12
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddWeb(AddOrUpdateCompletionReportReq req)
        {
            //var obj = req.MapTo<CompletionReport>();
            //obj.CreateTime = DateTime.Now;
            //var user = _auth.GetCurrentUser().User;
            //obj.CreateUserId = user.Id;
            ////保存完工报告
            //var o = await Repository.AddAsync(obj);
            ////保存图片
            //var pictures = req.Pictures.MapToList<CompletionReportPicture>();
            //pictures.ForEach(r => r.CompletionReportId = o.Id);
            //await UnitWork.BatchAddAsync(pictures.ToArray());
            //await UnitWork.SaveAsync();
            ////修改工单状态及反写工单号
            //await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId, s => new ServiceWorkOrder { Status = 7, CompletionReportId = o.Id });
            //var workOrderList = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId).ToList();
            //List<int> workorder = new List<int>();
            //foreach (var item in workOrderList)
            //{
            //    workorder.Add(item.Id);
            //}
            ////保存日志
            //await _ServiceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq
            //{
            //    Action = $"技术员完成售后维修",
            //    ActionType = $"{user.Name}提交了《行为服务报告单》，完成了本次任务",
            //}, workorder);
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
            int? TechnicianId = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ServiceOrderId).ToListAsync()).Where(s => "无序列号".Equals(MaterialType) ? s.MaterialCode == "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == MaterialType).FirstOrDefault()?.CurrentUserId;
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