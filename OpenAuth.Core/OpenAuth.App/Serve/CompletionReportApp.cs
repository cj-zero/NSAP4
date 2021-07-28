using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Serve;
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
        public async Task<TableData> Load(QueryCompletionReportListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && request.CurrentUserId != null)
            {
                var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(request.CurrentUserId)).Select(u => u.UserID).FirstOrDefaultAsync();
                loginUser = await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
            }
            var result = new TableData();
            var sericeWorkOrderList = UnitWork.Find<ServiceWorkOrder>(s => s.CompletionReportId != null && s.CompleteDate != null).Include(s=>s.ServiceOrder).Where(s=>s.ServiceOrder.VestInOrg==2)
                                       .WhereIf(!string.IsNullOrWhiteSpace(request.MaterialCode), s => s.MaterialCode.Contains(request.MaterialCode));
            if (loginContext.User.Account == Define.USERAPP && request.CurrentUserId != null)
            {
                sericeWorkOrderList = sericeWorkOrderList.WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumber), s => s.ManufacturerSerialNumber.Equals(request.ManufacturerSerialNumber));
            }
            else
            {
                sericeWorkOrderList = sericeWorkOrderList.WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumber), s => s.ManufacturerSerialNumber.Contains(request.ManufacturerSerialNumber));
            }
            //if (!loginContext.Roles.Any(r => r.Name.Equals("工程主管"))&& !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心"))) 
            //{
            //    sericeWorkOrderList=sericeWorkOrderList.Where(s => s.CurrentUserNsapId.Equals(loginUser.Id));
            //}
            var completionReportIds = await sericeWorkOrderList.Skip((request.page - 1) * request.limit).Take(request.limit).Select(s => s.CompletionReportId).ToListAsync();


            var query = from a in UnitWork.Find<CompletionReport>(c => completionReportIds.Contains(c.Id)).Include(c => c.ChangeTheMaterials).Include(c => c.CompletionReportPictures)
                       join b in UnitWork.Find<ServiceOrder>(null).Include(s=>s.ServiceWorkOrders) on a.ServiceOrderId equals b.Id into ab
                       from b in ab.DefaultIfEmpty()
                       select new { a,b};
            var objs = await query.ToListAsync();
            Dictionary<string, string> changeTheMaterials = new Dictionary<string, string>();
            objs.ForEach(o => { var material = ""; o.a.ChangeTheMaterials.ForEach(c=>material+=c.Material+"*"+c.Count+","); changeTheMaterials.Add(o.a.Id, material); });
            result.Data = objs.Select(o => new
            {
                MaterialCode=o.b.ServiceWorkOrders.FirstOrDefault()?.MaterialCode,
                o.a.CreateTime,
                o.a.FaultPhenomenon,
                o.a.CauseOfDefect,
                o.a.Responsibility,
                o.a.ChangeTheLocation,
                ChangeTheMaterials = (changeTheMaterials.GetValue(o.a.Id).ToString()).Substring(0, changeTheMaterials.GetValue(o.a.Id).ToString().Length - 1),
                Pictures =o.a.CompletionReportPictures.Select(c=>c.PictureId),
                o.b.U_SAP_ID,
                o.a.TechnicianName
            }) ;
            result.Count = await sericeWorkOrderList.CountAsync();
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
            var workOrderList = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId)
.WhereIf("无序列号".Equals(req.MaterialType), a => a.MaterialCode == "无序列号")
.WhereIf(!"无序列号".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType)
.ToListAsync());
            //获取最新的工单服务方式 这么做的原因是因为一键重派后可能拉取之前的完工报告的服务方式 而重派之后做单时选了其他的服务方式
            obj.ServiceMode = workOrderList.FirstOrDefault()?.ServiceMode;
            obj.CreateTime = DateTime.Now;
            //获取当前登陆者的nsap用户Id
            var nsapInfo = (await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.CurrentUserId).Include(a => a.User).FirstOrDefaultAsync());
            obj.CreateUserId = nsapInfo.User.Id;
            obj.IsReimburse = 1;
            obj.TechnicianId = req.CurrentUserId.ToString();

            int reumburseCount = await UnitWork.Find<ReimburseInfo>(r => r.ServiceOrderId.Equals(obj.ServiceOrderId) && r.CreateUserId.Equals(nsapInfo.User.Id)).CountAsync();
            obj.IsReimburse = reumburseCount > 0 ? 2 : 1;
            //获取出发日期与结束日期
            //1.先判断是否填写了日报 若填写了日报则取日报的最小和最大日期作为参数
            DateTime startDate = DateTime.Now;
            DateTime endDate = DateTime.Now;
            var dailyReports = await UnitWork.Find<ServiceDailyReport>(w => w.ServiceOrderId == obj.ServiceOrderId && w.CreateUserId == nsapInfo.User.Id).Select(s => s.CreateTime).ToListAsync();
            if (dailyReports != null && dailyReports.Count > 0)
            {
                startDate = (DateTime)dailyReports.Min();
                endDate = (DateTime)dailyReports.Max();
            }
            obj.BusinessTripDate = startDate;
            obj.EndDate = endDate;
            //obj.CreateUserName = user.Name;
            //todo:补充或调整自己需要的字段
            var o = await Repository.AddAsync(obj);
            var completionReportId = o.Id;
            var pictures = req.Pictures.MapToList<CompletionReportPicture>();
            pictures.ForEach(r => r.CompletionReportId = o.Id);
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();
            List<int> workorder = new List<int>();
            foreach (var item in workOrderList)
            {
                workorder.Add(item.Id);
            }
            string logMessage = $"技术员:{nsapInfo.User.Name}完成了服务";
            if (req.IsRedeploy == 1)//若转派则将当前设备类型置为初始未派单状态
            {
                throw new Exception("此功能暂时停用，如需转派请联系主管");
                //记录转派记录
                //var redeployInfo = new ServiceRedeploy
                //{
                //    ServiceOrderId = req.ServiceOrderId,
                //    MaterialType = req.MaterialType,
                //    TechnicianId = workOrderList.Select(s => s.CurrentUserId).FirstOrDefault(),
                //    CreateTime = DateTime.Now,
                //    WorkOrderIds = string.Join(",", workorder)
                //};
                //await UnitWork.AddAsync(redeployInfo);
                //await UnitWork.SaveAsync();
                //await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && workorder.Contains(s.Id),
                // o => new ServiceWorkOrder { Status = 1, OrderTakeType = 0, CurrentUser = string.Empty, CurrentUserId = 0, CurrentUserNsapId = string.Empty, BookingDate = null, VisitTime = null, ServiceMode = 0, CompletionReportId = string.Empty, TroubleDescription = string.Empty, ProcessDescription = string.Empty, IsCheck = 0, CompleteDate = null });
                ////删除相对应的流程数据
                //await UnitWork.DeleteAsync<ServiceFlow>(c => c.ServiceOrderId == req.ServiceOrderId && c.MaterialType == req.MaterialType);
                //logMessage = $"技术员: { nsapInfo.User.Name}申请转派该服务单并填写了完工报告";
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
                await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = logMessage, ActionType = "完成服务单", ServiceOrderId = req.ServiceOrderId, MaterialType = req.MaterialType });
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
        /// <summary>
        /// 修改完工报告(E3)
        /// </summary>
        /// <param name="obj"></param>
        public async Task CISEUpdate(AddOrUpdateCompletionReportReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var serviceOrderObj = await UnitWork.Find<ServiceOrder>(s => s.Id == obj.ServiceOrderId).Include(s => s.ServiceWorkOrders).FirstOrDefaultAsync();
            var workOrderList = serviceOrderObj.ServiceWorkOrders.Where(s => s.CurrentUserId == obj.CurrentUserId).ToList();
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && obj.CurrentUserId != null)
            {
                var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(obj.CurrentUserId)).Select(u => u.UserID).FirstOrDefaultAsync();
                loginUser = await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
            }
            await UnitWork.UpdateAsync<CompletionReport>(c => c.Id == obj.Id, c => new CompletionReport
            {
                Responsibility = obj.Responsibility,
                FaultPhenomenon = obj.FaultPhenomenon,
                CauseOfDefect = obj.CauseOfDefect,
                ChangeTheLocation = obj.ChangeTheLocation,
            });
            await UnitWork.DeleteAsync<CompletionReportPicture>(c => obj.DelFileIds.Contains(c.PictureId));
            await UnitWork.DeleteAsync<ChangeTheMaterial>(c => obj.DelChangeTheMaterialIds.Contains(c.Id));
            if (obj.Pictures.Count() > 0) 
            {
                var pictureList = obj.Pictures.MapToList<CompletionReportPicture>();
                pictureList.ForEach(p => p.CompletionReportId = obj.Id);
                await UnitWork.BatchAddAsync<CompletionReportPicture>(pictureList.ToArray());
            }
            var addchangeTheMaterial = obj.ChangeTheMaterials.Where(c =>string.IsNullOrWhiteSpace(c.CompletionReportId)).ToList();
            var updatechangeTheMaterial = obj.ChangeTheMaterials.Where(c =>!string.IsNullOrWhiteSpace(c.CompletionReportId)).ToList();
            if (addchangeTheMaterial.Count > 0) 
            {
                addchangeTheMaterial.ForEach(c => c.CompletionReportId = obj.Id);
                await UnitWork.BatchAddAsync<ChangeTheMaterial>(addchangeTheMaterial.ToArray());
            }
            if (updatechangeTheMaterial.Count > 0)
            {
                var changeTheMaterialList=await UnitWork.Find<ChangeTheMaterial>(c => c.CompletionReportId.Equals(obj.Id)).ToListAsync();
                var updatechangeTheMaterialList = new List<ChangeTheMaterial>();
                updatechangeTheMaterial.ForEach(c =>
                {
                    var changeTheMaterialObj = changeTheMaterialList.Where(t => t.Id.Equals(c.Id)).FirstOrDefault();
                    if (c.Count != changeTheMaterialObj.Count && !c.Material.Equals(changeTheMaterialObj.Material)) 
                    {
                        updatechangeTheMaterialList.Add(c);
                    }
                });
                if (updatechangeTheMaterialList.Count > 0) 
                {
                    await UnitWork.BatchUpdateAsync<ChangeTheMaterial>(updatechangeTheMaterialList.ToArray());
                }
            }
            await UnitWork.SaveAsync();

            string logMessage = $"工程部:{loginUser.Name}完成了服务";
            //判断为非草稿提交 则修改对应状态和发送消息
            if (obj.IsDraft == 0)
            {
                List<int> workorder = new List<int>();
                foreach (var item in workOrderList)
                {
                    workorder.Add(item.Id);
                }
                if (workOrderList.FirstOrDefault()?.Status < 7)
                {
                    await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                    {
                        Title = logMessage,
                        Details = $"提交了《行为服务报告单》，完成了本次任务",
                        LogType = 2,
                        ServiceOrderId = obj.ServiceOrderId,
                        ServiceWorkOrder = string.Join(',', workorder.ToArray()),
                        MaterialType = obj.MaterialType
                    });
                    await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = logMessage, ActionType = "完成服务单", ServiceOrderId = obj.ServiceOrderId });
                    //反写完工报告Id至工单
                    await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == obj.ServiceOrderId && s.CurrentUserId == obj.CurrentUserId && workorder.Contains(s.Id),
                        o => new ServiceWorkOrder { CompletionReportId = obj.Id, CompleteDate = DateTime.Now, Status = 7 });
                    //获取当前服务单下的所有消息Id集合
                    var msgList = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == obj.ServiceOrderId).Select(s => s.Id).ToListAsync();
                    //清空消息为已读
                    await UnitWork.UpdateAsync<ServiceOrderMessageUser>(s => s.FroUserId == obj.CurrentUserId.ToString() && msgList.Contains(s.MessageId), e => new ServiceOrderMessageUser { HasRead = true });
                }
                else
                {
                    await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                    {
                        Title = $"工程主管{loginUser.Name}修改完工报告",
                        Details = $"修改了《行为服务报告单》",
                        LogType = 2,
                        ServiceOrderId = obj.ServiceOrderId,
                        ServiceWorkOrder = string.Join(',', workorder.ToArray()),
                        MaterialType = obj.MaterialType
                    });
                }

            }
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
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == currentUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
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
                thisworkdetail.ServiceMode = troubleAndProcessInfo?.ServiceMode;
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
            //获取当前服务单的日报数量
            int reportCount = (await UnitWork.Find<ServiceDailyReport>(w => w.ServiceOrderId == serviceOrderId && w.CreateUserId == userInfo.UserID).Select(s => s.CreateTime.Value.Date).Distinct().ToListAsync()).Count;
            thisworkdetail.DailyReportNum = reportCount;
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
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == currentUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
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
            //获取当前服务单的日报数量
            int reportCount = (await UnitWork.Find<ServiceDailyReport>(w => w.ServiceOrderId == serviceOrderId && w.CreateUserId == userInfo.UserID).Select(s => s.CreateTime.Value.Date).Distinct().ToListAsync()).Count;
            thisworkdetail.DailyReportNum = reportCount;
            return thisworkdetail;
        }
        /// <summary>
        /// 获取完工报告详情Web by zlg 2020.08.12
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetCompletionReportDetailsWeb(int ServiceOrderId, string UserId, int? currentUserId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && currentUserId != null)
            {
                var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(currentUserId)).Select(u => u.UserID).FirstOrDefaultAsync();
                loginUser = await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
            }
            var CompletionReports = await UnitWork.Find<CompletionReport>(u => u.ServiceOrderId == ServiceOrderId).Include(u => u.CompletionReportPictures).Include(u => u.ChangeTheMaterials).ToListAsync();
            var ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(w => w.ServiceOrderId.Equals(ServiceOrderId)).ToListAsync();
            var serviceOrderObj = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(ServiceOrderId)).Select(s => new { s.U_SAP_ID, s.VestInOrg }).FirstOrDefaultAsync();
            if (!loginContext.Roles.Any(r => r.Name.Equals("售后主管")) && !loginContext.Roles.Any(r => r.Name.Equals("工程主管")) && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")))
            {
                if (UserId == null)
                {
                    CompletionReports = CompletionReports.Where(c => c.CreateUserId.Equals(loginUser.Id)).ToList();
                    ServiceWorkOrders = ServiceWorkOrders.Where(c => c.CurrentUserNsapId.Equal(loginUser.Id)).ToList();
                }
            }
            if (UserId != null)
            {
                CompletionReports = CompletionReports.Where(c => c.CreateUserId.Equals(UserId)).ToList();
            }
            if (CompletionReports.Count()<=0) 
            {
                throw new Exception("暂无完工报告");
            }

            if (serviceOrderObj.VestInOrg == 2)
            {
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
                        c.U_SAP_ID = serviceOrderObj.U_SAP_ID.ToString();
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
            }
            else if(serviceOrderObj.VestInOrg == 2)
            {
                var fileids = CompletionReports.FirstOrDefault().CompletionReportPictures.Select(p => p.PictureId).ToArray();
                var picfiles = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
                var files = picfiles.MapToList<UploadFileResp>();
                result.Data = CompletionReports.Select(c => new
                {
                    c.Id,
                    c.Responsibility,
                    c.FaultPhenomenon,
                    c.CauseOfDefect,
                    c.ChangeTheLocation,
                    //c.ChangeTheLocation,
                    //ServiceWorkOrders= ServiceWorkOrders.Select(s=>new { 
                    //    s.WorkOrderNumber,
                    //    s.ManufacturerSerialNumber,
                    //    s.MaterialCode,
                    //    s.Priority,
                    //    s.FromTheme,
                    //    s.Remark
                    //}),
                    c.ChangeTheMaterials,
                    files = files
                });
            }
            else if (serviceOrderObj.VestInOrg == 3)
            {
                var fileids = CompletionReports.FirstOrDefault().CompletionReportPictures.Select(p => p.PictureId).ToArray();
                var picfiles = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
                var files = picfiles.MapToList<UploadFileResp>();
                result.Data = CompletionReports.Select(c => new
                {
                    c.Id,
                    c.Destination,
                    c.Becity,
                    c.BusinessTripDays,
                    files = files
                });
            }

            return result;
        }
        /// <summary>
        /// 添加完工报告(E3专用)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task CISEAdd(AddOrUpdateCompletionReportReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.CurrentUserId != null)
            {
                var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(req.CurrentUserId)).Select(u => u.UserID).FirstOrDefaultAsync();
                loginUser = await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
            }
            //添加之前判断是否有报告提交记录 若有则删除之前的完工报告
            //var everCompletionReport = await UnitWork.Find<CompletionReport>(w => w.ServiceOrderId == req.ServiceOrderId && w.TechnicianId == req.CurrentUserId.ToString()).FirstOrDefaultAsync();

            var obj = req.MapTo<CompletionReport>();
            var serviceOrderObj = await UnitWork.Find<ServiceOrder>(s => s.Id == req.ServiceOrderId).Include(s => s.ServiceWorkOrders).FirstOrDefaultAsync();
            var workOrderList = serviceOrderObj.ServiceWorkOrders.Where(s => s.CurrentUserId == req.CurrentUserId).ToList();
            //获取最新的工单服务方式 这么做的原因是因为一键重派后可能拉取之前的完工报告的服务方式 而重派之后做单时选了其他的服务方式
            obj.CustomerName = serviceOrderObj.CustomerName;
            obj.CustomerId = serviceOrderObj.CustomerId;
            obj.TerminalCustomerId = serviceOrderObj.TerminalCustomerId;
            obj.TerminalCustomer = serviceOrderObj.TerminalCustomer;
            obj.Contacter = serviceOrderObj.NewestContacter;
            obj.ContactTel = serviceOrderObj.NewestContactTel;
            obj.ServiceMode = workOrderList.FirstOrDefault()?.ServiceMode;
            obj.CreateTime = DateTime.Now;
            obj.FromTheme = workOrderList.FirstOrDefault()?.FromTheme;
            //获取当前登陆者的nsap用户Id
            obj.CreateUserId = (workOrderList.FirstOrDefault())?.CurrentUserNsapId;
            obj.TechnicianId = req.CurrentUserId.ToString();
            obj.TechnicianName = loginUser.Name;
            obj.IsReimburse=serviceOrderObj.VestInOrg==3?  1: 3;
            obj.CompletionReportPictures = req.Pictures.MapToList<CompletionReportPicture>();
            //obj.CreateUserName = user.Name;
            //todo:补充或调整自己需要的字段
            var completionReport = await Repository.AddAsync(obj);
            List<int> workorder = new List<int>();
            foreach (var item in workOrderList)
            {
                workorder.Add(item.Id);
            }
            string logMessage = $"用户:{loginUser.Name}完成了服务";
            //判断为非草稿提交 则修改对应状态和发送消息
            if (req.IsDraft == 0)
            {
                if (workOrderList.FirstOrDefault()?.Status < 7)
                {
                    await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                    {
                        Title = logMessage,
                        Details = $"提交了《行为服务报告单》，完成了本次任务",
                        LogType = 2,
                        ServiceOrderId = req.ServiceOrderId,
                        ServiceWorkOrder = string.Join(',', workorder.ToArray()),
                        MaterialType = req.MaterialType
                    });
                    await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = logMessage, ActionType = "完成服务单", ServiceOrderId = req.ServiceOrderId });
                    //反写完工报告Id至工单
                    await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && workorder.Contains(s.Id),
                        o => new ServiceWorkOrder { CompletionReportId = completionReport.Id, CompleteDate = DateTime.Now, Status = 7 });
                    //获取当前服务单下的所有消息Id集合
                    var msgList = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == req.ServiceOrderId).Select(s => s.Id).ToListAsync();
                    //清空消息为已读
                    await UnitWork.UpdateAsync<ServiceOrderMessageUser>(s => s.FroUserId == req.CurrentUserId.ToString() && msgList.Contains(s.MessageId), e => new ServiceOrderMessageUser { HasRead = true });
                }
                else
                {
                    await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                    {
                        Title = $"用户{loginUser.Name}修改完工报告",
                        Details = $"用户修改了《行为服务报告单》",
                        LogType = 2,
                        ServiceOrderId = req.ServiceOrderId,
                        ServiceWorkOrder = string.Join(',', workorder.ToArray()),
                        MaterialType = req.MaterialType
                    });
                }

            }
            //删除草稿
            //if (everCompletionReport != null)
            //{
            //    await UnitWork.DeleteAsync<CompletionReportPicture>(c => c.CompletionReportId == everCompletionReport.Id);
            //    await UnitWork.DeleteAsync<ChangeTheMaterial>(c => c.CompletionReportId == everCompletionReport.Id);
            //    await UnitWork.DeleteAsync<CompletionReport>(c => c.Id == everCompletionReport.Id);
            //}
            await UnitWork.SaveAsync();
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