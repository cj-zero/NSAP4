using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Infrastructure.Extensions;
using System.Reactive;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.App.Sap.BusinessPartner;
using Minio.DataModel;
using OpenAuth.App.Serve.Request;
using Microsoft.Extensions.Options;
using Npoi.Mapper;
using DotNetCore.CAP;
using System.Threading;
using Magicodes.ExporterAndImporter.Excel;
using Magicodes.ExporterAndImporter.Core;
using Aliyun.Acs.Core;
using OpenAuth.Repository;
using OpenAuth.App.SignalR;
using MessagePack.Formatters;
using NPOI.SS.Formula.Functions;
using Infrastructure.Test;
using NetOffice.Extensions.Conversion;
using OpenAuth.App.Serve.Response;
using RazorEngine.Compilation.ImpromptuInterface.InvokeExt;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace OpenAuth.App
{
    public class ServiceOrderApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly BusinessPartnerApp _businessPartnerApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private IOptions<AppSetting> _appConfiguration;
        private ICapPublisher _capBus;
        private HttpHelper _helper;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        private readonly SignalRMessageApp _signalrmessage;
        private readonly ServiceFlowApp _serviceFlowApp;
        public ServiceOrderApp(IUnitWork unitWork,
            RevelanceManagerApp app, ServiceOrderLogApp serviceOrderLogApp, BusinessPartnerApp businessPartnerApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, IOptions<AppSetting> appConfiguration, ICapPublisher capBus, ServiceOrderLogApp ServiceOrderLogApp, SignalRMessageApp signalrmessage, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _revelanceApp = app;
            _businessPartnerApp = businessPartnerApp;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
            _capBus = capBus;
            _ServiceOrderLogApp = ServiceOrderLogApp;
            _signalrmessage = signalrmessage;
            _serviceFlowApp = serviceFlowApp;
        }

        #region<<nSAP System>>
        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryServiceOrderListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("serviceorder");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }


            var result = new TableData();

            return result;
        }

        /// <summary>
        /// 获取服务单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ServiceOrderDetailsResp> GetDetails(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var obj = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(id))
                //.Include(s => s.ServiceWorkOrders).ThenInclude(s => s.CompletionReport).ThenInclude(c=>c.CompletionReportPictures)
                //.Include(s => s.ServiceWorkOrders).ThenInclude(s => s.CompletionReport).ThenInclude(c=>c.Solution)
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.Solution)
                .Include(s => s.ServiceOrderPictures).FirstOrDefaultAsync();

            var result = obj.MapTo<ServiceOrderDetailsResp>();
            var serviceOrderPictures = obj.ServiceOrderPictures.Select(s => new { s.PictureId, s.PictureType }).ToList();
            var serviceOrderPictureIds = serviceOrderPictures.Select(s => s.PictureId).ToList();
            var files = await UnitWork.Find<UploadFile>(f => serviceOrderPictureIds.Contains(f.Id)).ToListAsync();
            result.Files = files.MapTo<List<UploadFileResp>>();
            result.Files.ForEach(f => f.PictureType = serviceOrderPictures.Where(p => f.Id.Equals(p.PictureId)).Select(p => p.PictureType).FirstOrDefault());
            //result.ServiceWorkOrders.ForEach(async s => 
            //{
            //    if(s.CompletionReport != null)
            //    {
            //        var completionReportPictures = obj.ServiceWorkOrders.First(sw => sw.Id.Equals(s.Id))
            //                ?.CompletionReport?.CompletionReportPictures.Select(c => c.PictureId).ToList();

            //        var completionReportFiles = await UnitWork.Find<UploadFile>(f => completionReportPictures.Contains(f.Id)).ToListAsync();
            //        s.CompletionReport.Files = completionReportFiles.MapTo<List<UploadFileResp>>();
            //    }
            //});
            return result;
        }

        /// <summary>
        /// 修改服务单状态
        /// </summary>
        /// <param name="id">服务单Id</param>
        /// <param name="status">1-待确认 2-已确认 3-已取消</param>
        /// <returns></returns>
        public async Task ModifyServiceOrderStatus(int id, int status)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id.Equals(id), u => new ServiceOrder { Status = status });
            await UnitWork.SaveAsync();
            var statusStr = status == 2 ? "已确认" : status == 3 ? "已取消" : "待确认";
        }

        /// <summary>
        /// 修改服务工单状态
        /// </summary>
        /// <param name="id">工单Id</param>
        /// <param name="status">1-待处理 2-已排配 3-已外出 4-已挂起 5-已接收 6-已解决 7-已回访</param>
        /// <returns></returns>
        public async Task ModifyServiceWorkOrderStatus(int id, int status)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id.Equals(id), u => new ServiceWorkOrder { Status = status });
            await UnitWork.SaveAsync();

            var MaterialType = UnitWork.Find<ServiceWorkOrder>(s => s.Id.Equals(id)).Select(s => s.MaterialCode).Distinct().FirstOrDefault();
            MaterialType = "其他设备".Equals(MaterialType) ? "其他设备" : MaterialType.Substring(0, MaterialType.IndexOf("-"));

            var statusStr = status == 2 ? "已排配" : status == 3 ? "已外出" : status == 4 ? "已挂起" : status == 5 ? "已接收" : status == 6 ? "已解决" : status == 7 ? "已回访" : "待处理";
            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"修改服务工单单状态为:{statusStr}", ActionType = "修改服务工单状态", ServiceWorkOrderId = id, MaterialType = MaterialType });
        }
        /// <summary>
        /// 查询超时未处理的订单
        /// </summary>
        /// <returns></returns>
        public async Task<List<ServiceOrder>> FindTimeoutOrder(int hours = 1)
        {
            var query = UnitWork.Find<ServiceOrder>(null);
            new TimeSpan(24, 0, 0);
            query = query.Where(s => s.Status == 1);
            var list = await query.ToListAsync();
            list = list.Where(s => (DateTime.Now - s.CreateTime.Value).Hours > hours).ToList();
            return list;
        }
        /// <summary>
        /// 待确认服务呼叫列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UnConfirmedServiceOrderList(QueryServiceOrderListReq req)
        {
            var result = new TableData();
            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceOrderSNs)
                .Include(s => s.ServiceWorkOrders)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState) && Convert.ToInt32(req.QryState) > 0, q => q.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ServiceOrderSNs.Any(a => a.ManufSN.Contains(req.QryManufSN)))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                         //.WhereIf(Convert.ToInt32(req.QryState) == 2, q => !q.ServiceWorkOrders.All(q => q.Status != 1))
                         //.WhereIf(Convert.ToInt32(req.QryState) == 0, q => q.Status == 1 || (q.Status == 2 && !q.ServiceWorkOrders.All(q => q.Status != 1)))
                         .WhereIf(int.TryParse(req.key, out int id) || !string.IsNullOrWhiteSpace(req.key), s => (s.Id == id || s.CustomerName.Contains(req.key) || s.ServiceWorkOrders.Any(o => o.ManufacturerSerialNumber.Contains(req.key))))
            .OrderBy(r => r.CreateTime).Select(q => new
            {
                q.Id,
                q.CustomerId,
                q.CustomerName,
                q.Services,
                q.CreateTime,
                q.Contacter,
                q.ContactTel,
                q.NewestContacter,
                q.NewestContactTel,
                q.Supervisor,
                q.SalesMan,
                q.Status,
                q.ServiceOrderSNs.FirstOrDefault(a => a.ManufSN.Contains(req.QryManufSN)).ManufSN,
                q.ServiceOrderSNs.FirstOrDefault(a => a.ManufSN.Contains(req.QryManufSN)).ItemCode,
                q.Province,
                q.City,
                q.Area,
                q.Addr,
                q.U_SAP_ID,
            });

            var list = (await query
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync()).Select(s => new
            {
                s.Id,
                s.CustomerId,
                s.CustomerName,
                s.Services,
                CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                NewestContacter = string.IsNullOrEmpty(s.NewestContacter) ? s.Contacter : s.NewestContacter,
                NewestContactTel = string.IsNullOrEmpty(s.NewestContactTel) ? s.ContactTel : s.NewestContactTel,
                s.Supervisor,
                s.SalesMan,
                s.Status,
                s.ManufSN,
                s.ItemCode,
                s.Province,
                s.City,
                s.Area,
                s.Addr,
                s.U_SAP_ID
            });
            result.Data = list;
            result.Count = query.Count();
            return result;
        }

        /// <summary>
        /// 待确认服务申请信息
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ServiceOrderDetailsResp> GetUnConfirmedServiceOrderDetails(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var obj = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(id))
                .Include(s => s.ServiceOrderSNs)
                .Include(s => s.ServiceOrderPictures).FirstOrDefaultAsync();
            var result = obj.MapTo<ServiceOrderDetailsResp>();
            return result;
        }

        /// <summary>
        /// 创建工单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task CreateWorkOrder(UpdateServiceOrderReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var d = await _businessPartnerApp.GetDetails(request.CustomerId);
            var obj = request.MapTo<ServiceOrder>();
            obj.RecepUserName = loginContext.User.Name;
            obj.RecepUserId = loginContext.User.Id;
            obj.Status = 2;
            obj.SalesMan = d.SlpName;
            obj.SalesManId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.SlpName)))?.Id;
            obj.Supervisor = d.TechName;
            obj.SupervisorId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.TechName)))?.Id;
            var province = string.IsNullOrWhiteSpace(request.Province) ? obj.Province : request.Province;
            var city = string.IsNullOrWhiteSpace(request.City) ? obj.City : request.City;
            var area = string.IsNullOrWhiteSpace(request.Area) ? obj.Area : request.Area;
            var addr = string.IsNullOrWhiteSpace(request.Addr) ? obj.Addr : request.Addr;
            if (string.IsNullOrWhiteSpace(obj.TerminalCustomer) && string.IsNullOrWhiteSpace(obj.TerminalCustomerId))
            {
                obj.TerminalCustomer = obj.CustomerName;
                obj.TerminalCustomerId = obj.CustomerId;
            }
            if (string.IsNullOrWhiteSpace(obj.NewestContacter) && string.IsNullOrWhiteSpace(obj.NewestContactTel))
            {
                obj.NewestContacter = obj.Contacter;
                obj.NewestContactTel = obj.ContactTel;
            }
            await UnitWork.UpdateAsync<ServiceOrder>(o => o.Id.Equals(request.Id), s => new ServiceOrder
            {
                Status = 2,
                Addr = addr,
                Address = obj.Address,
                AddressDesignator = obj.AddressDesignator,
                //Services = obj.Services,
                Province = province,
                City = city,
                Area = area,
                CustomerId = obj.CustomerId,
                CustomerName = obj.CustomerName,
                Contacter = obj.Contacter,
                ContactTel = obj.ContactTel,
                NewestContacter = obj.NewestContacter,
                NewestContactTel = obj.NewestContactTel,
                FromId = obj.FromId,
                TerminalCustomerId = obj.TerminalCustomerId,
                TerminalCustomer = obj.TerminalCustomer,
                SalesMan = obj.SalesMan,
                SalesManId = obj.SalesManId,
                Supervisor = obj.Supervisor,
                SupervisorId = obj.SupervisorId,
                RecepUserName = loginContext.User.Name,
                RecepUserId = loginContext.User.Id
            });
            //获取"其他"问题类型及其子类
            var otherProblemType = await UnitWork.Find<ProblemType>(o => o.Name.Equals("其他") && string.IsNullOrWhiteSpace(o.ParentId)).FirstOrDefaultAsync();
            var ChildTypes = new List<ProblemType>();
            if (otherProblemType != null && !string.IsNullOrEmpty(otherProblemType.Id))
            {
                ChildTypes = await UnitWork.Find<ProblemType>(null).Where(o1 => o1.ParentId.Equals(otherProblemType.Id)).ToListAsync();
            }
            var AppUser = await UnitWork.Find<AppUserMap>(s => s.UserID == obj.SupervisorId).Include(s => s.User).FirstOrDefaultAsync();
            var AppUserId = await UnitWork.Find<AppUserMap>(s => s.UserID == loginContext.User.Id).Select(s => s.AppUserId).FirstOrDefaultAsync();
            //工单赋值
            obj.ServiceWorkOrders.ForEach(s =>
            {
                s.ServiceOrderId = obj.Id; s.SubmitDate = DateTime.Now; s.SubmitUserId = loginContext.User.Id; s.AppUserId = obj.AppUserId; s.Status = 1;
                s.SubmitDate = DateTime.Now;
                s.SubmitUserId = loginContext.User.Id;

                #region 问题类型是其他的子类型直接分配给售后主管
                //if (!string.IsNullOrEmpty(s.ProblemTypeId))
                //{
                //    if (ChildTypes.Count() > 0 && ChildTypes.Where(p => p.Id.Equals(s.ProblemTypeId)).ToList().Count() > 0)
                //    {
                //        if (AppUser != null)
                //        {
                //            s.CurrentUser = AppUser.User.Name;
                //            s.CurrentUserId = AppUser?.AppUserId;
                //            s.CurrentUserNsapId = obj.SupervisorId;
                //            s.Status = 2;
                //        }
                //    }
                //}
                #endregion
                if (s.FromType == 2)
                {
                    if (AppUser != null)
                    {
                        s.CurrentUser = loginContext.User.Name;
                        s.CurrentUserId = AppUserId;
                        s.CurrentUserNsapId = loginContext.User.Id;
                        s.Status = 7;
                        s.CompleteDate = DateTime.Now;
                    }
                }
            });
            await UnitWork.BatchAddAsync<ServiceWorkOrder, int>(obj.ServiceWorkOrders.ToArray());

            var pictures = request.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = obj.Id; p.PictureType = p.PictureType == 3 ? 3 : 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "已分配专属客服",
                Details = "已为您分配专属客服进行处理，如果有消息将第一时间通知您，请耐心等待",
                ServiceOrderId = request.Id,
                LogType = 1
            });
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "客服确认售后信息",
                Details = "经客服核实，您的问题需要技术员进行跟踪处理，即将为您分配技术员，请耐心等待",
                ServiceOrderId = request.Id,
                LogType = 1
            });
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "呼叫中心已确认售后",
                Details = "呼叫中心已确认客户的售后申请，请技术员尽快处理",
                ServiceOrderId = request.Id,
                LogType = 2
            });


            var MaterialType = string.Join(",", obj.ServiceWorkOrders.Select(s => s.MaterialCode == "其他设备" ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-"))).Distinct().ToArray());

            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"客服:{loginContext.User.Name}创建工单", ActionType = "创建工单", ServiceOrderId = obj.Id, MaterialType = MaterialType });
            #region 同步到SAP 并拿到服务单主键
            _capBus.Publish("Serve.ServcieOrder.CreateWorkNumber", obj.Id);

            #endregion
            //log日志与发送消息
            var assignedWorks = obj.ServiceWorkOrders.FindAll(o => obj.SupervisorId.Equals(o.CurrentUserNsapId));
            if (assignedWorks.Count() > 0)
            {
                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "技术员主管接单",
                    Details = "已为您分配专属技术员进行处理，感谢您的耐心等待",
                    LogType = 1,
                    ServiceOrderId = obj.Id,
                    ServiceWorkOrder = String.Join(',', assignedWorks.Select(o => o.Id).ToArray()),
                    MaterialType = "其他设备".Equals(assignedWorks.FirstOrDefault().MaterialCode) ? "其他设备" : assignedWorks.FirstOrDefault().MaterialCode.Substring(0, assignedWorks.FirstOrDefault().MaterialCode.IndexOf("-"))
                });

                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "技术员接单成功",
                    Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                    LogType = 2,
                    ServiceOrderId = obj.Id,
                    ServiceWorkOrder = String.Join(',', assignedWorks.Select(o => o.Id).ToArray()),
                    MaterialType = "其他设备".Equals(assignedWorks.FirstOrDefault().MaterialCode) ? "其他设备" : assignedWorks.FirstOrDefault().MaterialCode.Substring(0, assignedWorks.FirstOrDefault().MaterialCode.IndexOf("-"))
                });
                await _signalrmessage.SendSystemMessage(SignalRSendType.User, $"系统已自动分配了{assignedWorks.Count()}个新的售后服务，请尽快处理", new List<string>() { d.TechName });
            }
        }
        /// <summary>
        /// 删除一个工单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteWorkOrder(int id)
        {
            await UnitWork.DeleteAsync<ServiceWorkOrder>(s => s.Id.Equals(id));

            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 新增一个工单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task AddWorkOrder(AddServiceWorkOrderReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            //用信号量代替锁
            await semaphoreSlim.WaitAsync();
            try
            {
                var obj = request.MapTo<ServiceWorkOrder>();
                var ServiceWorkOrders = UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId.Equals(request.ServiceOrderId)).OrderByDescending(u => u.Id).ToList();
                var WorkOrderNumber = ServiceWorkOrders.First().WorkOrderNumber;
                int num = Convert.ToInt32(WorkOrderNumber.Substring(WorkOrderNumber.IndexOf("-") + 1));
                obj.WorkOrderNumber = WorkOrderNumber.Substring(0, WorkOrderNumber.IndexOf("-") + 1) + (num + 1);
                #region 如果问题类型是其他下面子类型,默认分配给技术主管
                //获取"其他"问题类型及其子类
                var theservice = await UnitWork.Find<ServiceOrder>(o => o.Id.Equals(obj.ServiceOrderId)).FirstOrDefaultAsync();
                //if (!string.IsNullOrEmpty(obj.ProblemTypeId))
                //{
                //    var otherProblemType = await UnitWork.Find<ProblemType>(o => o.Name.Equals("其他") && string.IsNullOrWhiteSpace(o.ParentId)).FirstOrDefaultAsync();
                //    var ChildTypes = new List<ProblemType>();
                //    if (otherProblemType != null && !string.IsNullOrEmpty(otherProblemType.Id))
                //    {
                //        ChildTypes = await UnitWork.Find<ProblemType>(null).Where(o1 => o1.ParentId.Equals(otherProblemType.Id)).ToListAsync();
                //    }
                //    var AppUser = await UnitWork.Find<AppUserMap>(s => s.UserID == theservice.SupervisorId).Include(s => s.User).FirstOrDefaultAsync();
                //    if (ChildTypes.Count() > 0 && ChildTypes.Where(p => p.Id.Equals(obj.ProblemTypeId)).ToList().Count() > 0)
                //    {
                //        if (AppUser != null)
                //        {
                //            obj.CurrentUser = theservice.Supervisor;
                //            obj.CurrentUserNsapId = theservice.SupervisorId;
                //            obj.CurrentUserId = AppUser.AppUserId;
                //            obj.Status = 2;
                //        }
                //    }
                    
                //}
                #endregion

                if (obj.FromType == 2)
                {
                    var AppUser = await UnitWork.Find<AppUserMap>(s => s.UserID == loginContext.User.Id).Include(s => s.User).FirstOrDefaultAsync();
                    obj.CurrentUser = loginContext.User.Name;
                    obj.CurrentUserNsapId = loginContext.User.Id;
                    obj.CurrentUserId = AppUser?.AppUserId;
                    obj.Status = 7;
                    obj.CompleteDate = DateTime.Now;
                }
                await UnitWork.AddAsync<ServiceWorkOrder, int>(obj);
                await UnitWork.SaveAsync();

                //log日志与发送消息

                var typename = "其他设备".Equals(obj.MaterialCode) ? "其他设备" : obj.MaterialCode.Substring(0, obj.MaterialCode.IndexOf("-"));
                await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"客服:{loginContext.User.Name}创建工单{obj.WorkOrderNumber}", ActionType = "创建工单", ServiceOrderId = request.ServiceOrderId, MaterialType = typename });


                if (!string.IsNullOrEmpty(obj.CurrentUserNsapId))
                {
                    await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                    {
                        Title = "技术员主管接单",
                        Details = "已为您分配专属技术员进行处理，感谢您的耐心等待",
                        LogType = 1,
                        ServiceOrderId = obj.ServiceOrderId,
                        ServiceWorkOrder = obj.Id.ToString(),
                        MaterialType = typename
                    });

                    await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                    {
                        Title = "技术员接单成功",
                        Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                        LogType = 2,
                        ServiceOrderId = obj.ServiceOrderId,
                        ServiceWorkOrder = obj.Id.ToString(),
                        MaterialType = typename
                    });
                    await _signalrmessage.SendSystemMessage(SignalRSendType.User, $"系统已自动分配了1个新的售后服务，请尽快处理", new List<string>() { theservice.Supervisor });
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// 修改工单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateWorkOrder(UpdateWorkOrderReq request)
        {
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id.Equals(request.Id), e => new ServiceWorkOrder
            {
                FeeType = request.FeeType,
                SolutionId = request.SolutionId,
                Remark = request.Remark,
                ProblemTypeId = request.ProblemTypeId,
                Priority = request.Priority,
                FromTheme = request.FromTheme,
                FromType = request.FromType
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 修改服务单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task ModifyServiceOrder(ModifyServiceOrderReq request)
        {
            await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id.Equals(request.Id), e => new ServiceOrder
            {
                NewestContacter = request.NewestContacter,
                NewestContactTel = request.NewestContactTel,
                Province = request.Province,
                City = request.City,
                Area = request.Area,
                Addr = request.Addr,
                AddressDesignator = request.AddressDesignator,
                Address = request.Address,
                TerminalCustomer = request.TerminalCustomer
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 派单页面左侧树
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UnsignedWorkOrderTree(QueryServiceOrderListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            var result = new TableData();
            var query = from a in UnitWork.Find<ServiceWorkOrder>(null)
                        join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };

            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.b.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.a.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.b.CustomerId.Contains(req.QryCustomer) || q.b.CustomerName.Contains(req.QryCustomer) || q.b.TerminalCustomer.Contains(req.QryCustomer) || q.b.TerminalCustomerId.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.b.RecepUserName.Contains(req.QryRecepUser))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.a.ProblemTypeId.Equals(req.QryProblemType))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), q => q.a.CurrentUser.Contains(req.QryTechName))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.b.CreateTime >= req.QryCreateTimeFrom && q.b.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.b.ContactTel.Equals(req.ContactTel) || q.b.NewestContactTel.Equals(req.ContactTel))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), q => q.b.Supervisor.Contains(req.QrySupervisor))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryMaterialCode), q => q.a.MaterialCode.Contains(req.QryMaterialCode))
                         .Where(q => q.b.U_SAP_ID != null && q.b.Status == 2 && q.a.FromType != 2);
            if (!string.IsNullOrWhiteSpace(req.QryStatusBar.ToString()) && req.QryStatusBar != 0)
            {
                if (req.QryStatusBar == 1)
                {
                    query = query.Where(q => q.a.Status >= 2 && q.a.Status < 7);
                }
                else
                {
                    query = query.Where(q => q.a.Status >= 7);
                }
            }
            if (req.QryState != "1")
            {
                query = query.Where(q => q.a.Status > 1);
            }
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.User.Account.Equals("lijianmei"))
            {
                if (loginContext.Roles.Any(r => r.Name.Equals("售后主管")))
                {
                    query = query.Where(q => q.b.SupervisorId.Equals(loginContext.User.Id) || q.a.CurrentUserNsapId.Equals(loginContext.User.Id));
                }
                else
                {
                    query = query.Where(q => q.a.CurrentUserNsapId.Equals(loginContext.User.Id));
                }
            }
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            var workorderlist = await query.OrderBy(r => r.a.CreateTime).Select(q => new
            {
                ServiceOrderId = q.b.Id,
                q.b.U_SAP_ID,
                MaterialType = q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-")) == null ? "其他设备" : (q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-")) == "" ? "其他设备" : q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-")))
            }).Distinct().ToListAsync();

            var grouplistsql = from c in workorderlist
                               group c by c.ServiceOrderId into g
                               let U_SAP_ID = g.Select(a => a.U_SAP_ID).First()
                               let MTypes = g.Select(o => o.MaterialType == "其他设备" ? "其他设备" : MaterialTypeModel.Where(u => u.TypeAlias == o.MaterialType).FirstOrDefault().TypeName).ToArray()
                               let WorkMTypes = g.Select(o => o.MaterialType == "其他设备" ? "其他设备" : o.MaterialType)
                               select new { ServiceOrderId = g.Key, U_SAP_ID, MaterialTypes = WorkMTypes, WorkMaterialTypes = MTypes };
            var grouplist = grouplistsql.ToList();

            result.Count = grouplistsql.Count();

            grouplist = grouplist.OrderByDescending(s => s.U_SAP_ID).Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToList();
            result.Data = grouplist;
            return result;
        }
        /*
        /// <summary>
        /// 呼叫服务（客服）左侧树
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceWorkOrderTree(QueryServiceOrderListReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<ServiceWorkOrder>(null)
                        join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };

            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceOrderId), q => q.b.Id.Equals(Convert.ToInt32(req.QryServiceOrderId)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.a.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.b.CustomerId.Contains(req.QryCustomer) || q.b.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.b.RecepUserName.Contains(req.QryRecepUser))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.a.ProblemTypeId.Equals(req.QryProblemType))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.a.CreateTime >= req.QryCreateTimeFrom && q.a.CreateTime <= req.QryCreateTimeTo);
            var workorderlist = await query.OrderBy(r => r.a.CreateTime).Select(q => new
            {
                ServiceOrderId = q.b.Id,
                ServiceWorkOrderId = q.a.Id
            }).Distinct().ToListAsync();

            var grouplistsql = from c in workorderlist
                               group c by c.ServiceOrderId into g
                               let WTypes = g.Select(o => o.ServiceWorkOrderId.ToString()).ToArray()
                               select new { ServiceOrderId = g.Key, WorkOrderId = WTypes };
            var grouplist = grouplistsql.ToList();

            result.Data = grouplist;
            return result;
        }
        */
        /// <summary>
        /// 客服新建服务单
        /// </summary>
        /// <returns></returns>
        public async Task CustomerServiceAgentCreateOrder(CustomerServiceAgentCreateOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var d = await _businessPartnerApp.GetDetails(req.CustomerId.ToUpper());
            var obj = req.MapTo<ServiceOrder>();
            obj.CustomerId = req.CustomerId.ToUpper();
            obj.TerminalCustomerId = req.TerminalCustomerId.ToUpper();
            obj.RecepUserName = loginContext.User.Name;
            obj.RecepUserId = loginContext.User.Id;
            obj.CreateUserId = loginContext.User.Id;
            obj.Status = 2;
            obj.SalesMan = d.SlpName;
            obj.SalesManId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.SlpName)))?.Id;
            //obj.Supervisor = d.TechName;
            obj.SupervisorId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(req.Supervisor)))?.Id;

            if (string.IsNullOrWhiteSpace(obj.NewestContacter) && string.IsNullOrWhiteSpace(obj.NewestContactTel))
            {
                obj.NewestContacter = obj.Contacter;
                obj.NewestContactTel = obj.ContactTel;
            }
            //获取"其他"问题类型及其子类
            var otherProblemType = await UnitWork.Find<ProblemType>(o => o.Name.Equals("其他") && string.IsNullOrWhiteSpace(o.ParentId)).FirstOrDefaultAsync();
            var ChildTypes = new List<ProblemType>();
            if (otherProblemType != null && !string.IsNullOrEmpty(otherProblemType.Id))
            {
                ChildTypes = await UnitWork.Find<ProblemType>(null).Where(o1 => o1.ParentId.Equals(otherProblemType.Id)).ToListAsync();
            }
            var AppUser = await UnitWork.Find<AppUserMap>(s => s.UserID == obj.SupervisorId).Include(s => s.User).FirstOrDefaultAsync();
            var AppUserId = await UnitWork.Find<AppUserMap>(s => s.UserID == loginContext.User.Id).Select(s => s.AppUserId).FirstOrDefaultAsync();
            obj.ServiceWorkOrders.ForEach(s =>
            {
                s.SubmitDate = DateTime.Now;
                s.SubmitUserId = loginContext.User.Id;
                #region 问题类型是其他的子类型直接分配给售后主管
                //if (!string.IsNullOrEmpty(s.ProblemTypeId))
                //{
                //    if (ChildTypes.Count() > 0 && ChildTypes.Where(p => p.Id.Equals(s.ProblemTypeId)).ToList().Count() > 0)
                //    {
                //        if (AppUser != null)
                //        {
                //            s.CurrentUser = AppUser.User.Name;
                //            s.CurrentUserId = AppUser?.AppUserId;
                //            s.CurrentUserNsapId = obj.SupervisorId;
                //            s.Status = 2;
                //        }
                //    }
                //}
                if (s.FromType == 2)
                {
                    s.CurrentUser = loginContext.User.Name;
                    s.CurrentUserId = AppUserId;
                    s.CurrentUserNsapId = loginContext.User.Id;
                    s.Status = 7;
                    s.CompleteDate = DateTime.Now;
                }
                #endregion
            });
            var e = await UnitWork.AddAsync<ServiceOrder, int>(obj);
            await UnitWork.SaveAsync();
            var pictures = req.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = e.Id; p.PictureType = p.PictureType == 3 ? 3 : 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();

            var MaterialType = string.Join(",", obj.ServiceWorkOrders.Select(s => s.MaterialCode == "其他设备" ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-"))).Distinct().ToArray());

            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"客服:{loginContext.User.Name}创建服务单", ActionType = "创建服务单", ServiceOrderId = e.Id, MaterialType = MaterialType });
            #region 同步到SAP 并拿到服务单主键

            _capBus.Publish("Serve.ServcieOrder.Create", obj.Id);
            #endregion
            //log日志与发送消息
            var assignedWorks = obj.ServiceWorkOrders.FindAll(o => obj.SupervisorId.Equals(o.CurrentUserNsapId));
            if (assignedWorks.Count() > 0)
            {
                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "技术员主管接单",
                    Details = "已为您分配专属技术员进行处理，感谢您的耐心等待",
                    LogType = 1,
                    ServiceOrderId = obj.Id,
                    ServiceWorkOrder = String.Join(',', assignedWorks.Select(o => o.Id).ToArray()),
                    MaterialType = "其他设备".Equals(assignedWorks.FirstOrDefault().MaterialCode) ? "其他设备" : assignedWorks.FirstOrDefault().MaterialCode.Substring(0, assignedWorks.FirstOrDefault().MaterialCode.IndexOf("-"))
                });

                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "技术员接单成功",
                    Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                    LogType = 2,
                    ServiceOrderId = obj.Id,
                    ServiceWorkOrder = String.Join(',', assignedWorks.Select(o => o.Id).ToArray()),
                    MaterialType = "其他设备".Equals(assignedWorks.FirstOrDefault().MaterialCode) ? "其他设备" : assignedWorks.FirstOrDefault().MaterialCode.Substring(0, assignedWorks.FirstOrDefault().MaterialCode.IndexOf("-"))
                });
                await _signalrmessage.SendSystemMessage(SignalRSendType.User, $"系统已自动分配了{assignedWorks.Count()}个新的售后服务，请尽快处理", new List<string>() { obj.Supervisor });
            }
        }
        /// <summary>
        /// 派单工单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceWorkOrderList(QueryServiceOrderListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var ids = await UnitWork.Find<ServiceWorkOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.Status.Equals(Convert.ToInt32(req.QryState)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ManufacturerSerialNumber.Contains(req.QryManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ProblemTypeId.Equals(req.QryProblemType))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromType), q => q.FromType.Equals(Convert.ToInt32(req.QryFromType)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), q => q.CurrentUser.Contains(req.QryTechName))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromTheme), q => q.FromTheme.Contains(req.QryFromTheme))
                .OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();

            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceWorkOrders)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer) || q.TerminalCustomerId.Contains(req.QryCustomer) || q.TerminalCustomer.Contains(req.QryCustomer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.RecepUserName.Contains(req.QryRecepUser))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.ContactTel.Contains(req.ContactTel) || q.NewestContactTel.Contains(req.ContactTel))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), q => q.Supervisor.Contains(req.QrySupervisor))
                .Where(q => ids.Contains(q.Id) && q.Status == 2);

            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.User.Account.Equals("wanghaitao") && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")))
            {
                if (loginContext.Roles.Any(r => r.Name.Equals("售后文员")))
                {
                    var orgs = loginContext.Orgs.Select(o => o.Id).ToArray();
                    var userIds = _revelanceApp.Get(Define.USERORG, false, orgs);
                    var sIds = await UnitWork.Find<ServiceWorkOrder>(q => userIds.Contains(q.CurrentUserNsapId)).OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                    query = query.Where(q => userIds.Contains(q.SupervisorId) || sIds.Contains(q.Id));
                }
                else
                {
                    var sIds = await UnitWork.Find<ServiceWorkOrder>(q => q.CurrentUser.Contains(loginContext.User.Name)).OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                    query = query.Where(q => q.SupervisorId.Equals(loginContext.User.Id) || sIds.Contains(q.Id));
                }
            }
            var resultsql = query.OrderByDescending(q => q.CreateTime).Select(q => new
            {
                ServiceOrderId = q.Id,
                q.CustomerId,
                q.CustomerName,
                q.TerminalCustomerId,
                q.TerminalCustomer,
                q.RecepUserName,
                q.Contacter,
                q.ContactTel,
                q.NewestContacter,
                q.NewestContactTel,
                q.Supervisor,
                q.SalesMan,
                TechName = "",
                q.U_SAP_ID,
                ServiceStatus = q.Status,
                ServiceCreateTime = q.CreateTime,
                ServiceWorkOrders = q.ServiceWorkOrders.Where(a => (string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId) || a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                && (string.IsNullOrWhiteSpace(req.QryState) || a.Status.Equals(Convert.ToInt32(req.QryState)))
                && (string.IsNullOrWhiteSpace(req.QryManufSN) || a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                //&& ((req.QryCreateTimeFrom == null || req.QryCreateTimeTo == null) || (a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime <= req.QryCreateTimeTo))
                && (string.IsNullOrWhiteSpace(req.QryFromType) || a.FromType.Equals(Convert.ToInt32(req.QryFromType)))
                && (string.IsNullOrWhiteSpace(req.QryTechName) || a.CurrentUser.Contains(req.QryTechName))
                && (string.IsNullOrWhiteSpace(req.QryProblemType) || a.ProblemTypeId.Equals(req.QryProblemType))
                && (string.IsNullOrWhiteSpace(req.QryFromTheme) || a.FromTheme.Contains(req.QryFromTheme))).ToList()
            });

            result.Data = await resultsql.Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync();
            result.Count = query.Count();
            return result;
        }

        /// <summary>
        /// 呼叫服务（客服)工单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UnsignedWorkOrderList(QueryServiceOrderListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var query = from a in UnitWork.Find<ServiceWorkOrder>(null)
                        join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<ProblemType>(null) on a.ProblemTypeId equals c.Id into ac
                        from c in ac.DefaultIfEmpty()
                        select new { a, b, c };

            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.b.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.a.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.b.CustomerId.Contains(req.QryCustomer) || q.b.CustomerName.Contains(req.QryCustomer) || q.b.TerminalCustomerId.Contains(req.QryCustomer) || q.b.TerminalCustomer.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.b.RecepUserName.Contains(req.QryRecepUser))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.a.ProblemTypeId.Contains(req.QryProblemType))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), q => q.a.CurrentUser.Contains(req.QryTechName))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.b.ContactTel.Equals(req.ContactTel) || q.b.NewestContactTel.Equals(req.ContactTel))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.a.CreateTime >= req.QryCreateTimeFrom && q.a.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                         .WhereIf(req.QryMaterialTypes != null && req.QryMaterialTypes.Count > 0, q => req.QryMaterialTypes.Contains(q.a.MaterialCode == "其他设备" ? "其他设备" : q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-"))))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), q => q.b.Supervisor.Contains(req.QrySupervisor))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryMaterialCode), q => q.a.MaterialCode.Contains(req.QryMaterialCode))
                         .Where(q => q.b.U_SAP_ID != null && q.b.Status == 2 && q.a.FromType != 2);

            if (!string.IsNullOrWhiteSpace(req.QryStatusBar.ToString()) && req.QryStatusBar != 0)
            {
                if (req.QryStatusBar == 1)
                {
                    query = query.Where(q => q.a.Status >= 2 && q.a.Status < 7);
                }
                else
                {
                    query = query.Where(q => q.a.Status >= 7);
                }
            }
            if (req.QryState != "1")
            {
                query = query.Where(q => q.a.Status > 1);
            }
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.User.Account.Equals("lijianmei"))
            {
                if (loginContext.Roles.Any(r => r.Name.Equals("售后主管")))
                {
                    query = query.Where(q => q.b.SupervisorId.Equals(loginContext.User.Id) || q.a.CurrentUserNsapId.Equals(loginContext.User.Id));
                }
                else
                {
                    query = query.Where(q => q.a.CurrentUserNsapId.Equals(loginContext.User.Id));
                }
            }

            var resultsql = query.OrderBy(r => r.a.Id).ThenBy(r => r.a.WorkOrderNumber).Select(q => new
            {
                ServiceOrderId = q.b.Id,
                q.a.Priority,
                q.a.FromType,
                q.a.Status,
                q.b.CustomerId,
                q.b.TerminalCustomerId,
                q.b.TerminalCustomer,
                q.b.CustomerName,
                q.a.FromTheme,
                q.a.CreateTime,
                q.b.RecepUserName,
                TechName = "",
                q.a.ManufacturerSerialNumber,
                q.a.MaterialCode,
                q.a.MaterialDescription,
                q.b.Contacter,
                q.b.ContactTel,
                q.b.Supervisor,
                q.b.SalesMan,
                ServiceWorkOrderId = q.a.Id,
                ProblemTypeName = q.c.Name,
                q.a.CurrentUserId,
                q.a.CurrentUser,
                q.a.CurrentUserNsapId,
                q.b.U_SAP_ID,
                q.a.WorkOrderNumber
            });


            result.Data =
            (await resultsql
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync());//.GroupBy(o => o.Id).ToList();
            result.Count = query.Count();
            return result;
        }


        /// <summary>
        /// 调出该客户代码近10个呼叫ID,及未关闭的近10个呼叫ID
        /// </summary>
        /// <returns></returns>
        public async Task<dynamic> GetCustomerNewestOrders(string code)
        {
            var newestOrder = await UnitWork.Find<ServiceOrder>(s => s.CustomerId.Equals(code)).OrderByDescending(s => s.CreateTime)
                .Select(s => new
                {
                    s.Id,
                    s.CustomerId,
                    s.CustomerName,
                    s.Services,
                    s.Status,
                    s.Contacter,
                    s.ContactTel,
                    s.NewestContacter,
                    s.NewestContactTel,
                    s.U_SAP_ID,
                    s.CreateTime
                })
                .Skip(0).Take(10).ToListAsync();
            var newestNotCloseOrder = await UnitWork.Find<ServiceOrder>(s => s.CustomerId.Equals(code) && s.Status == 2 && s.ServiceWorkOrders.Any(o => o.Status < 7)).OrderByDescending(s => s.CreateTime)
                .Select(s => new
                {
                    s.Id,
                    s.CustomerId,
                    s.CustomerName,
                    s.Services,
                    s.Status,
                    s.Contacter,
                    s.ContactTel,
                    s.NewestContacter,
                    s.NewestContactTel,
                    s.U_SAP_ID,
                    s.CreateTime
                })
                .Skip(0).Take(10).ToListAsync();
            return new { newestOrder, newestNotCloseOrder };
        }


        /// <summary>
        /// 回访服务单
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task ServiceOrderCallback(int serviceOrderId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var order = await UnitWork.Find<ServiceOrder>(s => s.Id == serviceOrderId).Include(s => s.ServiceWorkOrders).FirstOrDefaultAsync();
            if (order is null)
                throw new CommonException("服务单号不存在", 40004);
            var allCanCallback = order.ServiceWorkOrders.All(s => s.Status == 7);
            if (allCanCallback)
            {
                await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == serviceOrderId, o => new ServiceWorkOrder
                {
                    Status = 8
                });
            }
            else
            {
                throw new CommonException("无法回访此服务单，原因：还有工单尚未解决", 40005);
            }
        }

        /// <summary>
        /// 根据服务单id获取行为报告单数据 by zlg 2020.08.12
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        public TableData GetServiceOrder(string ServiceOrderId)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var userid = user.Id;
            var ServiceWorkOrderModel = UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId == Convert.ToInt32(ServiceOrderId) && u.Status >= 2 && u.Status <= 5 && u.CurrentUserNsapId == userid).OrderBy(u => u.Id).ToList();
            if (ServiceWorkOrderModel != null && ServiceWorkOrderModel.Count > 0)
            {
                var FirstServiceWorkOrder = ServiceWorkOrderModel.First();
                var ServiceOrderModel = UnitWork.Find<ServiceOrder>(u => u.Id == Convert.ToInt32(ServiceOrderId)).Select(u => new
                {
                    CurrentUser = FirstServiceWorkOrder.CurrentUser,
                    CustomerId = u.CustomerId,
                    id = u.Id,
                    ServiceWorkOrderId = ServiceWorkOrderModel.Select(u => new { u.WorkOrderNumber }).ToList(),
                    CustomerName = u.CustomerName,
                    NewestContacter = u.NewestContacter,
                    Contacter = u.Contacter,
                    TerminalCustomer = u.TerminalCustomer,
                    NewestContactTel = u.NewestContactTel,
                    ContactTel = u.ContactTel,
                    u.U_SAP_ID,
                    ManufacturerSerialNumber = ServiceWorkOrderModel.Select(u => new { u.ManufacturerSerialNumber }).ToList(),
                    MaterialCode = ServiceWorkOrderModel.Select(u => new { u.MaterialCode }).ToList(),
                    Description = FirstServiceWorkOrder.TroubleDescription + FirstServiceWorkOrder.ProcessDescription
                });
                result.Data = ServiceOrderModel;
            }
            return result;
        }
        /// <summary>
        /// 根据服务单id判断是否撤销服务单 by zlg 2020.08.13
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> UpDateServiceOrderStatus(int ServiceOrderId)
        {
            var user = _auth.GetCurrentUser().User;
            var obj = UnitWork.Find<ServiceOrder>(u => u.Id == ServiceOrderId).FirstOrDefault();
            TimeSpan timeSpan = DateTime.Now - Convert.ToDateTime(obj.CreateTime);
            var result = new TableData();
            if (timeSpan.TotalMinutes > 5)
            {
                result.Code = 500;
                result.Message = "服务单已超出撤销时间，不可撤销！";
            }
            else
            {
                await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id == ServiceOrderId, u => new ServiceOrder { Status = 3 });
                await UnitWork.DeleteAsync<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(ServiceOrderId));
                await UnitWork.SaveAsync();
                //保存日志
                await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq
                {
                    ServiceOrderId = ServiceOrderId,
                    Action = $"{user.Name}执行撤销操作，撤销ID为{ServiceOrderId}的服务单",
                    ActionType = "撤销操作",
                });
            }
            return result;
        }

        /// <summary>
        /// 服务呼叫各统计排行（包括售后部门、销售员、问题类型、接单员处理呼叫量
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceWorkOrderReport(QueryServiceOrderListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();

            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceWorkOrders)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.ServiceWorkOrders.Any(a => a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.ServiceWorkOrders.Any(a => a.Status.Equals(Convert.ToInt32(req.QryState))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ServiceWorkOrders.Any(a => a.ManufacturerSerialNumber.Contains(req.QryManufSN)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.RecepUserName.Contains(req.QryRecepUser))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ServiceWorkOrders.Any(a => a.ProblemTypeId.Equals(req.QryProblemType)))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.ContactTel.Contains(req.ContactTel) || q.NewestContactTel.Contains(req.ContactTel))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromType), q => q.ServiceWorkOrders.Any(a => a.FromType.Equals(Convert.ToInt32(req.QryFromType))));
            //.Where(q => q.Status == 2);
            ;
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")))
            {
                query = query.Where(q => q.SupervisorId.Equals(loginContext.User.Id));
            }
            var resultlist = new List<ServerOrderStatListResp>();
            var list1 = await query.Where(g => !string.IsNullOrWhiteSpace(g.Supervisor)).GroupBy(g => new { g.SupervisorId, g.Supervisor }).Select(q => new ServiceOrderReportResp
            {
                StatId = q.Key.SupervisorId,
                StatName = q.Key.Supervisor,
                ServiceCnt = q.Count()
            }).Where(w => w.ServiceCnt > 10).OrderByDescending(s => s.ServiceCnt).Skip(0).Take(20).ToListAsync();
            resultlist.Add(new ServerOrderStatListResp { StatType = "Supervisor", StatList = list1 });

            var list2 = await query.Where(g => !string.IsNullOrWhiteSpace(g.SalesMan)).GroupBy(g => new { g.SalesManId, g.SalesMan }).Select(q => new ServiceOrderReportResp
            {
                StatId = q.Key.SalesManId,
                StatName = q.Key.SalesMan,
                ServiceCnt = q.Count()
            }).OrderByDescending(s => s.ServiceCnt).Skip(0).Take(20).ToListAsync();
            resultlist.Add(new ServerOrderStatListResp { StatType = "SalesMan", StatList = list2 });

            var problemTypes = await query.Select(s => s.ServiceWorkOrders.Select(s => s.ProblemType).ToList()).ToListAsync();
            var l3 = new List<ProblemType>();
            foreach (var problemType in problemTypes)
            {
                l3.AddRange(problemType);
            }
            var list3 = l3.Where(g => g != null).GroupBy(g => new { g.Id, g.Name }).Select(q => new ServiceOrderReportResp
            {
                StatId = q.Key.Id,
                StatName = q.Key.Name,
                ServiceCnt = q.Count()
            }).OrderByDescending(s => s.ServiceCnt).Skip(0).Take(20).ToList();
            resultlist.Add(new ServerOrderStatListResp { StatType = "ProblemType", StatList = list3 });

            var list4 = await query.Where(g => !string.IsNullOrWhiteSpace(g.RecepUserName)).GroupBy(g => new { g.RecepUserId, g.RecepUserName }).Select(q => new ServiceOrderReportResp
            {
                StatId = q.Key.RecepUserId,
                StatName = q.Key.RecepUserName,
                ServiceCnt = q.Count()
            }).OrderByDescending(s => s.ServiceCnt).Skip(0).Take(20).ToListAsync();
            resultlist.Add(new ServerOrderStatListResp { StatType = "RecepUser", StatList = list4 });



            var Supervisorlist = (await query.Where(g => !string.IsNullOrWhiteSpace(g.Supervisor)).ToListAsync()).GroupBy(g => new { g.Supervisor, g.SupervisorId }).Select(s => new { s.Key, a = s.ToList() });

            var RecepStart = new List<ServiceOrderReportResp>();
            foreach (var item in Supervisorlist)
            {
                ServiceOrderReportResp sor = new ServiceOrderReportResp();
                sor.StatId = item.Key.SupervisorId;
                sor.StatName = item.Key.Supervisor;
                List<int> StatusCount = new List<int>();
                var ServiceWorkOrders = item.a.Where(q => (q.SupervisorId == null || q.SupervisorId.Equals(item.Key.SupervisorId)) && q.Supervisor.Equals(item.Key.Supervisor)).Select(q => q.ServiceWorkOrders).ToList();
                ServiceWorkOrders.ForEach(s => StatusCount.AddRange(s.Select(q => Convert.ToInt32(q.Status)).ToList()));
                var Status = StatusCount.GroupBy(s => s).ToList();
                List<ServiceOrderReportResp> ReportResp = new List<ServiceOrderReportResp>();
                for (int i = 1; i < 9; i++)
                {
                    var Statuslist = Status.Where(s => s.Key.Equals(i)).FirstOrDefault();
                    if (Statuslist != null && Statuslist.Count() > 0)
                    {
                        ReportResp.Add(new ServiceOrderReportResp
                        {
                            StatId = Statuslist.Key.ToString(),
                            ServiceCnt = Statuslist.Count()
                        });
                    }
                    else
                    {
                        ReportResp.Add(new ServiceOrderReportResp
                        {
                            StatId = i.ToString(),
                            ServiceCnt = 0
                        });
                    }
                }
                sor.ReportList = ReportResp;
                RecepStart.Add(sor);
            }

            resultlist.Add(new ServerOrderStatListResp { StatType = "RecepStart", StatList = RecepStart });

            result.Data = resultlist;
            return result;
        }


        /// <summary>
        /// 获取工单详情根据工单Id
        /// </summary>
        /// <param name="workOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetWorkOrderDetailById(int workOrderId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var data = await UnitWork.Find<ServiceWorkOrder>(s => s.Id == workOrderId).ToListAsync();
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 查询可以被派单的技术员列表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetAllowSendOrderUser(GetAllowSendOrderUserReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var orgs = loginContext.Orgs.Select(o => o.Id).ToArray();

            var tUsers = await UnitWork.Find<AppUserMap>(u => u.AppUserRole > 1).ToListAsync();
            var userIds = _revelanceApp.Get(Define.USERORG, false, orgs);
            var ids = userIds.Intersect(tUsers.Select(u => u.UserID));
            var users = await UnitWork.Find<User>(u => ids.Contains(u.Id) && u.Status == 0).WhereIf(!string.IsNullOrEmpty(req.key), u => u.Name.Equals(req.key)).ToListAsync();
            var us = users.Select(u => new { u.Name, AppUserId = tUsers.FirstOrDefault(a => a.UserID.Equals(u.Id)).AppUserId, u.Id });
            var appUserIds = tUsers.Where(u => userIds.Contains(u.UserID)).Select(u => u.AppUserId).ToList();

            var userCount = await UnitWork.Find<ServiceWorkOrder>(s => appUserIds.Contains(s.CurrentUserId) && s.Status.Value < 7)
                .Select(s => new { s.CurrentUserId, s.ServiceOrderId }).Distinct().GroupBy(s => s.CurrentUserId)
                .Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

            var userInfos = us.Select(u => new AllowSendOrderUserResp
            {
                Id = u.Id,
                Name = u.Name,
                Count = userCount.FirstOrDefault(s => s.Key.Equals(u.AppUserId))?.Count ?? 0,
                AppUserId = u.AppUserId,
            }).ToList();

            if (!string.IsNullOrWhiteSpace(req.CurrentUser)) userInfos = userInfos.Where(u => u.Name.Contains(req.CurrentUser)).ToList();

            var list = userInfos
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToList();
            result.Data = list;
            result.Count = userInfos.Count;
            return result;
        }

        /// <summary>
        /// 修改未生成的工单号
        /// </summary>
        /// <returns></returns>
        public async Task UpDateWorkOrderNumber()
        {
            var query = await UnitWork.Find<ServiceWorkOrder>(s => string.IsNullOrWhiteSpace(s.WorkOrderNumber)).ToListAsync();
            if (query.Count() > 0)
            {
                var ids = query.Where(s => s.ServiceOrderId != 0).Select(s => s.ServiceOrderId).Distinct().ToList();
                foreach (var item in ids)
                {
                    var ServiceOrder = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(item)).AsNoTracking().FirstOrDefaultAsync();
                    if (ServiceOrder != null)
                    {
                        var ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId.Equals(item)).AsNoTracking().ToListAsync();
                        int num = 0;
                        ServiceWorkOrders.ForEach(u => u.WorkOrderNumber = ServiceOrder.U_SAP_ID + "-" + ++num);
                        UnitWork.BatchUpdate<ServiceWorkOrder>(ServiceWorkOrders.ToArray());
                        await UnitWork.SaveAsync();
                    }
                }
            }
        }

        /// <summary>
        /// nSAP主管给技术员派单
        /// </summary>
        /// <returns></returns>
        public async Task nSAPSendOrders(SendOrdersReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var canSendOrder = await CheckCanTakeOrder(req.CurrentUserId);
            if (!canSendOrder)
            {
                throw new CommonException("技术员接单已经达到上限", 60001);
            }
            var u = await UnitWork.Find<AppUserMap>(s => s.AppUserId == req.CurrentUserId).Include(s => s.User).FirstOrDefaultAsync();
            var ServiceOrderModel = await UnitWork.Find<ServiceOrder>(s => s.Id == Convert.ToInt32(req.ServiceOrderId)).FirstOrDefaultAsync();

            var Model = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.ToString() == req.ServiceOrderId && req.QryMaterialTypes.Contains(s.MaterialCode == "其他设备" ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")))).Select(s => s.Id);
            var ids = await Model.ToListAsync();
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => ids.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUser = u.User.Name,
                CurrentUserNsapId = u.User.Id,
                CurrentUserId = req.CurrentUserId,
                Status = 2
            });

            await UnitWork.AddAsync<ServiceOrderParticipationRecord>(new ServiceOrderParticipationRecord
            {
                ServiceOrderId = ServiceOrderModel.Id,
                UserId = u.User.Id,
                SapId = ServiceOrderModel.U_SAP_ID,
                ReimburseType = 0,
                CreateTime = DateTime.Now

            });
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单",
                Details = "已为您分配专属技术员进行处理，感谢您的耐心等待",
                LogType = 1,
                ServiceOrderId = Convert.ToInt32(req.ServiceOrderId),
                ServiceWorkOrder = string.Join(",", ids.ToArray()),
                MaterialType = string.Join(",", req.QryMaterialTypes.ToArray())
            });

            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单成功",
                Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                LogType = 2,
                ServiceOrderId = Convert.ToInt32(req.ServiceOrderId),
                ServiceWorkOrder = string.Join(",", ids.ToArray()),
                MaterialType = string.Join(",", req.QryMaterialTypes.ToArray())
            });
            var WorkOrderNumbers = String.Join(',', await UnitWork.Find<ServiceWorkOrder>(s => ids.Contains(s.Id)).Select(s => s.WorkOrderNumber).ToArrayAsync());

            await _ServiceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"主管{loginContext.User.Name}给技术员{u.User.Name}派单{WorkOrderNumbers}", ActionType = "主管派单工单", MaterialType = string.Join(",", req.QryMaterialTypes.ToArray()) }, ids);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = Convert.ToInt32(req.ServiceOrderId), Content = $"主管{loginContext.User.Name}给技术员{u.User.Name}派单{WorkOrderNumbers}", AppUserId = 0 });
            await PushMessageToApp(req.CurrentUserId, "派单成功提醒", "您已被派有一个新的售后服务，请尽快处理");
        }

        /// <summary>
        /// 销售员呼叫服务
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SalesManServiceWorkOrderList(QueryServiceOrderListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var ids = await UnitWork.Find<ServiceWorkOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.Status.Equals(Convert.ToInt32(req.QryState)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ManufacturerSerialNumber.Contains(req.QryManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ProblemTypeId.Equals(req.QryProblemType))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromType), q => q.FromType.Equals(Convert.ToInt32(req.QryFromType)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), q => q.CurrentUser.Contains(req.QryTechName))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromTheme), q => q.FromTheme.Contains(req.QryFromTheme))
                .OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();

            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceWorkOrders)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.RecepUserName.Contains(req.QryRecepUser))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.ContactTel.Contains(req.ContactTel) || q.NewestContactTel.Contains(req.ContactTel))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), q => q.Supervisor.Contains(req.QrySupervisor))
                .Where(q => q.SalesManId.Equals(loginContext.User.Id) && q.CustomerId.Equals(q.TerminalCustomerId) && q.CustomerName.Equals(q.TerminalCustomer) && ids.Contains(q.Id) && q.Status == 2);

            var resultsql = query.OrderByDescending(q => q.CreateTime).Select(q => new
            {
                ServiceOrderId = q.Id,
                q.CustomerId,
                q.CustomerName,
                q.TerminalCustomerId,
                q.TerminalCustomer,
                q.RecepUserName,
                q.Contacter,
                q.ContactTel,
                q.NewestContacter,
                q.NewestContactTel,
                q.Supervisor,
                q.SalesMan,
                TechName = "",
                q.U_SAP_ID,
                ServiceStatus = q.Status,
                ServiceCreateTime = q.CreateTime,
                ServiceWorkOrders = q.ServiceWorkOrders.Where(a => (string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId) || a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                && (string.IsNullOrWhiteSpace(req.QryState) || a.Status.Equals(Convert.ToInt32(req.QryState)))
                && (string.IsNullOrWhiteSpace(req.QryManufSN) || a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                //&& ((req.QryCreateTimeFrom == null || req.QryCreateTimeTo == null) || (a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime <= req.QryCreateTimeTo))
                && (string.IsNullOrWhiteSpace(req.QryFromType) || a.FromType.Equals(Convert.ToInt32(req.QryFromType)))
                && (string.IsNullOrWhiteSpace(req.QryTechName) || a.CurrentUser.Contains(req.QryTechName))
                && (string.IsNullOrWhiteSpace(req.QryProblemType) || a.ProblemTypeId.Equals(req.QryProblemType))
                && (string.IsNullOrWhiteSpace(req.QryFromTheme) || a.FromTheme.Contains(req.QryFromTheme))).ToList()
            });

            result.Data = await resultsql.Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync();
            result.Count = query.Count();
            return result;
        }
        #endregion

        #region<<Common Methods>>
        /// <summary>
        /// 获取服务单图片Id列表
        /// </summary>
        /// <param name="id">服务单Id</param>
        /// <param name="type">1-客户上传 2-客服上传</param>
        /// <returns></returns>
        public async Task<List<UploadFileResp>> GetServiceOrderPictures(int id, int type)
        {
            var Pictures = await UnitWork.Find<ServiceOrderPicture>(p => p.ServiceOrderId.Equals(id))
               .WhereIf(type == 0, a => a.PictureType == 1 || a.PictureType == 2)
               .WhereIf(type > 0, b => b.PictureType.Equals(type))
               .Select(p => new { p.PictureId, p.PictureType }).ToListAsync();
            var idList = Pictures.Select(p => p.PictureId).ToList();
            var files = await UnitWork.Find<UploadFile>(f => idList.Contains(f.Id)).ToListAsync();
            var list = files.MapTo<List<UploadFileResp>>();
            list.ForEach(L => L.PictureType = Pictures.Where(p => L.Id.Equals(p.PictureId)).Select(f => f.PictureType).FirstOrDefault());
            return list;
        }

        /// <summary>
        /// 导出excel
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportExcel(QueryServiceOrderListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var query = UnitWork.Find<ServiceOrder>(null)
                .Include(s => s.ServiceWorkOrders).ThenInclude(c => c.ProblemType)
                .Include(a => a.ServiceWorkOrders).ThenInclude(b => b.Solution)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.ServiceWorkOrders.Any(a => a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.ServiceWorkOrders.Any(a => a.Status.Equals(Convert.ToInt32(req.QryState))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ServiceWorkOrders.Any(a => a.ManufacturerSerialNumber.Contains(req.QryManufSN)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.RecepUserName.Contains(req.QryRecepUser))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ServiceWorkOrders.Any(a => a.ProblemTypeId.Equals(req.QryProblemType)))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.ContactTel.Contains(req.ContactTel) || q.NewestContactTel.Contains(req.ContactTel))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromType), q => q.ServiceWorkOrders.Any(a => a.FromType.Equals(Convert.ToInt32(req.QryFromType))))
                .Where(q => q.Status == 2);

            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")))
            {
                query = query.Where(q => q.SupervisorId.Equals(loginContext.User.Id));
            }
            var resultsql = query.OrderByDescending(q => q.CreateTime).Select(q => new
            {
                ServiceOrderId = q.Id,
                q.CustomerId,
                q.CustomerName,
                q.TerminalCustomer,
                q.TerminalCustomerId,
                q.RecepUserName,
                q.Contacter,
                q.ContactTel,
                q.NewestContacter,
                q.NewestContactTel,
                q.Supervisor,
                q.SalesMan,
                TechName = "",
                q.U_SAP_ID,
                q.Services,
                q.FromId,
                q.AddressDesignator,
                q.Address,
                ServiceStatus = q.Status,
                ServiceCreateTime = q.CreateTime,
                ServiceWorkOrders = q.ServiceWorkOrders.Where(a => (string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId) || a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                && (string.IsNullOrWhiteSpace(req.QryState) || a.Status.Equals(Convert.ToInt32(req.QryState)))
                && (string.IsNullOrWhiteSpace(req.QryManufSN) || a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                //&& ((req.QryCreateTimeFrom == null || req.QryCreateTimeTo == null) || (a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime <= req.QryCreateTimeTo))
                && (string.IsNullOrWhiteSpace(req.QryFromType) || a.FromType.Equals(Convert.ToInt32(req.QryFromType)))).ToList()
            });

            var dataList = await resultsql.ToListAsync(); ;
            var statusDic = new Dictionary<int, string>()
            {
                //1-待处理 2-已排配 3-已外出 4-已挂起 5-已接收 6-已解决 7-已回访
                { 1, "待处理"},
                { 2, "已排配"},
                { 3, "已预约"},
                { 4, "已外出"},
                { 5, "已挂起"},
                { 6, "已接收"},
                { 7, "已解决"},
                { 8, "已回访"},
            };
            var list = new List<ServiceOrderExcelDto>();
            foreach (var serviceOrder in dataList)
            {
                foreach (var workOrder in serviceOrder.ServiceWorkOrders)
                {
                    list.Add(new ServiceOrderExcelDto
                    {
                        U_SAP_ID = serviceOrder.U_SAP_ID,
                        CustomerId = serviceOrder.CustomerId,
                        CustomerName = serviceOrder.CustomerName,
                        TerminalCustomer = serviceOrder.TerminalCustomer,
                        TerminalCustomerId = serviceOrder.TerminalCustomerId,
                        Contacter = serviceOrder.Contacter,
                        ContactTel = serviceOrder.ContactTel,
                        NewestContacter = serviceOrder.NewestContacter,
                        NewestContactTel = serviceOrder.NewestContactTel,
                        Supervisor = serviceOrder.Supervisor,
                        SalesMan = serviceOrder.SalesMan,
                        RecepUserName = serviceOrder.RecepUserName,
                        Service = serviceOrder.Services,
                        FromId = serviceOrder.FromId.Value == 1 ? "电话" : "App",
                        Status = serviceOrder.ServiceStatus == 1 ? "待确认" : serviceOrder.ServiceStatus == 2 ? "已确认" : "已取消",
                        AddressDesignator = serviceOrder.AddressDesignator,
                        Address = serviceOrder.Address,
                        WorkOrderNumber = workOrder.WorkOrderNumber,
                        FromTheme = workOrder.FromTheme,
                        MaterialCode = workOrder.MaterialCode,
                        MaterialDescription = workOrder.MaterialDescription,
                        ManufacturerSerialNumber = workOrder.ManufacturerSerialNumber,
                        InternalSerialNumber = workOrder.InternalSerialNumber,
                        Remark = workOrder.Remark,
                        FromType = workOrder.FromType.Value == 1 ? "提交呼叫" : "在线解答",
                        WarrantyEndDate = workOrder.WarrantyEndDate,
                        ProblemType = workOrder.ProblemType?.Description,
                        Priority = workOrder.Priority == 3 ? "高" : workOrder.Priority == 2 ? "中" : "低",
                        WorkOrderStatus = statusDic.GetValueOrDefault(workOrder.Status.Value),
                        CurrentUser = workOrder.CurrentUser,
                        SubmitDate = workOrder.CreateTime,
                        BookingDate = workOrder.BookingDate,
                        VisitTime = workOrder.VisitTime,
                        LiquidationDate = workOrder.LiquidationDate,
                        Solution = workOrder.Solution?.Subject,
                        TroubleDescription = workOrder.TroubleDescription,
                        ProcessDescription = workOrder.ProcessDescription
                    });
                }
            }
            IExporter exporter = new ExcelExporter();
            var bytes = await exporter.ExportAsByteArray(list);
            return bytes;
        }

        /// <summary>
        /// 获取技术员位置信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianLocation(GetTechnicianLocationReq req)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            int? currentTechnicianId = 0;
            var result = new TableData();
            if (req.TechnicianId > 0)
            {
                currentTechnicianId = req.TechnicianId;
            }
            else
            {
                currentTechnicianId = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId)
                    .WhereIf("其他设备".Equals(req.MaterialType), a => a.MaterialCode == "其他设备")
                    .WhereIf(!"其他设备".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType)
                    .FirstOrDefaultAsync())?.CurrentUserId;
            }
            data.Add("TechnicianId", currentTechnicianId);
            //获取技术员电话
            string Mobile = (await GetProtectPhone(req.ServiceOrderId, req.MaterialType, 0)).Data;
            data.Add("Mobile", Mobile);
            var locations = await UnitWork.Find<RealTimeLocation>(r => r.AppUserId == currentTechnicianId).OrderByDescending(o => o.CreateTime).Select(s => new
            {
                s.AppUserId,
                s.Addr,
                s.Area,
                s.City,
                CreateTime = s.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                s.Latitude,
                s.Longitude,
                s.Province,
                s.Id,
                TechnicianId = currentTechnicianId
            }).FirstOrDefaultAsync();
            data.Add("locations", locations);
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 获取待处理服务单总数
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetServiceOrderCount()
        {
            return await UnitWork.Find<ServiceOrder>(u => u.Status == 1).CountAsync();
        }

        /// <summary>
        /// 获取为派单工单总数
        /// </summary>
        /// <returns></returns>
        public async Task<List<IGrouping<string, ServiceOrder>>> GetServiceWorkOrderCount()
        {
            var result = new TableData();
            var model = UnitWork.Find<ServiceWorkOrder>(s => s.Status == 1).Select(s => s.ServiceOrderId).Distinct();
            var ids = await model.ToListAsync();
            var query = await UnitWork.Find<ServiceOrder>(s => ids.Contains(s.Id)).ToListAsync();
            var groub = query.GroupBy(s => s.Supervisor).ToList();

            return groub;
        }

        /// <summary>
        /// 获取隐私号码
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<TableData> GetProtectPhone(int ServiceOrderId, string MaterialType, int type)
        {
            var result = new TableData();
            string ProtectPhone = string.Empty;
            //获取技术员Id
            int? TechnicianId = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ServiceOrderId)
                .WhereIf("其他设备".Equals(MaterialType), w => w.MaterialCode == "其他设备")
                .WhereIf(!"其他设备".Equals(MaterialType), w => w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-")) == MaterialType).FirstOrDefaultAsync())?.CurrentUserId;
            var query = from a in UnitWork.Find<AppUserMap>(null)
                        join b in UnitWork.Find<User>(null) on a.UserID equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };
            //获取技术员联系方式
            string TechnicianTel = await query.Where(w => w.a.AppUserId == TechnicianId).Select(s => s.b.Mobile).FirstOrDefaultAsync();
            //获取客户联系方式
            var serviceOrderInfo = await UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId).FirstOrDefaultAsync();
            string custMobile = string.IsNullOrEmpty(serviceOrderInfo.NewestContactTel) ? serviceOrderInfo.ContactTel : serviceOrderInfo.NewestContactTel;
            if (string.IsNullOrEmpty(TechnicianTel) || string.IsNullOrEmpty(custMobile))
            {
                //判断当前操作角色 0客户 1技术员
                switch (type)
                {
                    case 0:
                        ProtectPhone = TechnicianTel;
                        break;
                    case 1:
                        ProtectPhone = custMobile;
                        break;
                }
            }
            else
            {
                //ProtectPhone = AliPhoneNumberProtect.bindAxb(custMobile, TechnicianTel);
                if (string.IsNullOrEmpty(ProtectPhone))
                {
                    if (type == 1)
                    {
                        ProtectPhone = custMobile;
                    }
                    else
                    {
                        ProtectPhone = TechnicianTel;
                    }
                }
            }
            result.Data = ProtectPhone;
            return result;
        }

        /// <summary>
        /// 判断用户是否到达接单上限
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<bool> CheckCanTakeOrder(int id)
        {
            int totalNum = int.Parse(GetSendOrderCount().Result?.DtValue);
            var count = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == id && s.Status.Value < 7).Select(s => s.ServiceOrderId).Distinct().CountAsync();
            return count < totalNum;
        }

        /// <summary>
        /// 获取配置表里的技术员可接单数
        /// </summary>
        /// <returns></returns>
        private async Task<Category> GetSendOrderCount()
        {
            return await UnitWork.Find<Category>(c => c.Enable == true && "Send_Order_Count".Equals(c.DtCode)).FirstOrDefaultAsync();
        }
        #endregion

        #region<<Message>>

        /// <summary>
        /// 发送聊天室消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SendServiceOrderMessage(SendServiceOrderMessageReq req)
        {
            string userId = string.Empty;
            string name = string.Empty;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (req.AppUserId == 0)
            {
                name = "系统";
            }
            else
            {
                //根据appUserId获取nSAP中的用户信息
                var query = from a in UnitWork.Find<AppUserMap>(null)
                            join b in UnitWork.Find<User>(null) on a.UserID equals b.Id into ab
                            from b in ab.DefaultIfEmpty()
                            select new { a, b };
                query = query.Where(o => o.a.AppUserId == req.AppUserId);
                var userInfo = await query.Select(q => new
                {
                    q.b.Id,
                    q.b.Name
                }).FirstOrDefaultAsync();
                if (userInfo == null && req.AppUserId != 0)
                {
                    throw new CommonException("您还未开通nSAP访问权限", 204);
                }
                userId = userInfo.Id;
                name = userInfo.Name;
            }
            var obj = req.MapTo<ServiceOrderMessage>();
            obj.CreateTime = DateTime.Now;
            obj.FroTechnicianId = userId;
            obj.FroTechnicianName = name;
            obj.Replier = name;
            obj.ReplierId = userId;
            await UnitWork.AddAsync<ServiceOrderMessage, int>(obj);
            await UnitWork.SaveAsync();
            string msgId = (await UnitWork.Find<ServiceOrderMessage>(s => s.AppUserId == req.AppUserId).OrderByDescending(o => o.CreateTime).FirstOrDefaultAsync()).Id;
            await SendMessageToRelatedUsers(req.Content, req.ServiceOrderId, req.AppUserId, msgId);
        }

        /// <summary>
        /// 发送消息给服务单相关人员
        /// </summary>
        /// <param name="Content"></param>
        /// <param name="ServiceOrderId"></param>
        /// <param name="FromUserId"></param>
        /// <param name="MessageId"></param>
        /// <returns></returns>
        public async Task SendMessageToRelatedUsers(string Content, int ServiceOrderId, int FromUserId, string MessageId)
        {
            //发给服务单客服/主管
            var serviceInfo = await UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId).FirstOrDefaultAsync();
            //客服Id
            string RecepUserId = serviceInfo.RecepUserId;
            if (!string.IsNullOrEmpty(RecepUserId))
            {
                var recepUserInfo = await UnitWork.Find<AppUserMap>(a => a.UserID == RecepUserId).FirstOrDefaultAsync();
                if (recepUserInfo != null && recepUserInfo.AppUserId > 0 && !recepUserInfo.AppUserId.Equals(FromUserId))
                {
                    var msgObj = new ServiceOrderMessageUser
                    {
                        CreateTime = DateTime.Now,
                        FromUserId = FromUserId.ToString(),
                        FroUserId = recepUserInfo.AppUserId.ToString(),
                        HasRead = false,
                        MessageId = MessageId
                    };
                    await UnitWork.AddAsync<ServiceOrderMessageUser, int>(msgObj);
                }
            }
            //主管Id
            string SupervisorId = serviceInfo.SupervisorId;
            if (!string.IsNullOrEmpty(SupervisorId))
            {
                var superUserInfo = await UnitWork.Find<AppUserMap>(a => a.UserID == SupervisorId).FirstOrDefaultAsync();
                if (superUserInfo != null && superUserInfo.AppUserId > 0 && !superUserInfo.AppUserId.Equals(FromUserId))
                {
                    var msgObj = new ServiceOrderMessageUser
                    {
                        CreateTime = DateTime.Now,
                        FromUserId = FromUserId.ToString(),
                        FroUserId = superUserInfo.AppUserId.ToString(),
                        HasRead = false,
                        MessageId = MessageId
                    };
                    await UnitWork.AddAsync<ServiceOrderMessageUser, int>(msgObj);
                }
            }
            //查询相关技术员Id
            var userList = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ServiceOrderId && s.CurrentUserId != FromUserId && s.CurrentUserId > 0).ToListAsync()).GroupBy(g => g.CurrentUserId)
                .Select(s => new { s.Key });
            foreach (var item in userList)
            {
                var msgObj = new ServiceOrderMessageUser();
                msgObj.CreateTime = DateTime.Now;
                msgObj.FromUserId = FromUserId.ToString();
                msgObj.FroUserId = item.Key.ToString();
                msgObj.HasRead = false;
                msgObj.MessageId = MessageId;
                await UnitWork.AddAsync<ServiceOrderMessageUser, int>(msgObj);
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取服务单消息内容详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrderMessage(GetServiceOrderMessageReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var query = UnitWork.Find<ServiceOrderMessage>(o => o.ServiceOrderId == req.ServiceOrderId);
            var resultsql = query.OrderByDescending(r => r.CreateTime).Select(s => new
            {
                s.Content,
                s.CreateTime,
                s.FroTechnicianName,
                s.AppUserId,
                s.Replier
            });

            result.Data =
            (await resultsql
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync()).OrderBy(o => o.CreateTime).Select(s => new
            {
                s.Content,
                CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                s.FroTechnicianName,
                s.AppUserId,
                s.Replier
            });
            result.Count = await query.CountAsync();
            await ReadMsg(req.CurrentUserId, req.ServiceOrderId);
            return result;
        }

        /// <summary>
        /// 消息已读
        /// </summary>
        /// <param name="currentUserId"></param>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task ReadMsg(int currentUserId, int serviceOrderId)
        {
            var msgList = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == serviceOrderId).ToListAsync();
            if (msgList != null)
            {
                string msgIds = string.Join(",", msgList.Select(s => s.Id).Distinct().ToArray());
                UnitWork.Update<ServiceOrderMessageUser>(s => msgIds.Contains(s.MessageId) && s.FroUserId == currentUserId.ToString(), u => new ServiceOrderMessageUser { HasRead = true });
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取服务单消息内容列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrderMessageList(GetServiceOrderMessageListReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前登陆者的nsapId
            var nsapUserId = (await UnitWork.Find<AppUserMap>(u => u.AppUserId == req.CurrentUserId).FirstOrDefaultAsync()).UserID;
            var queryService = from a in UnitWork.Find<ServiceWorkOrder>(null)
                               join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id
                               select new { a, b };

            var serviceIdList = await queryService.Where(w => w.a.Status < 7 && (w.b.SupervisorId == nsapUserId || w.a.CurrentUserId == req.CurrentUserId)).ToListAsync();
            if (serviceIdList != null)
            {
                string serviceIds = string.Join(",", serviceIdList.Select(s => s.b.Id).Distinct().ToArray());
                var query = from a in UnitWork.Find<ServiceOrderMessage>(null)
                            join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                            from b in ab.DefaultIfEmpty()
                            select new { a, b };
                var resultsql = query.Where(w => serviceIds.Contains(w.a.ServiceOrderId.ToString())).OrderByDescending(o => o.a.CreateTime).Select(s => new
                {
                    s.b.U_SAP_ID,
                    s.a.Content,
                    s.a.CreateTime,
                    s.a.FroTechnicianName,
                    s.a.AppUserId,
                    s.a.ServiceOrderId,
                    s.a.Replier,
                });

                result.Data =
                ((await resultsql
                .ToListAsync()).GroupBy(g => g.ServiceOrderId).Select(g => g.First())).Select(s => new { s.Content, CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"), s.FroTechnicianName, s.AppUserId, s.ServiceOrderId, s.Replier, s.U_SAP_ID });
            }
            return result;
        }


        /// <summary>
        /// 获取未读消息个数
        /// </summary>
        /// <param name="currentUserId">当前登陆者appid</param>
        /// <returns></returns>
        public async Task<TableData> GetMessageCount(int currentUserId)
        {
            var result = new TableData();
            var msgCount = (await UnitWork.Find<ServiceOrderMessageUser>(s => s.FroUserId == currentUserId.ToString() && s.HasRead == false).ToListAsync()).Count;
            result.Data = msgCount;
            return result;
        }


        /// <summary>
        /// 推送消息至新威智能app
        /// </summary>
        /// <param name="userId">app用户Id</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public async Task PushMessageToApp(int userId, string title, string content)
        {
            _helper.Post(new
            {
                UserId = userId,
                Title = title,
                Content = content
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "BbsCommunity/AppPushMsg");
        }

        #endregion

        #region<<Customer>>
        /// <summary>
        /// 售后进度列表(客户)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> AppLoad(AppQueryServiceOrderListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取设备类型列表
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(s => s.AppUserId == request.AppUserId)
                        .Include(s => s.ServiceWorkOrders)
                        .WhereIf(request.Type == 1, s => s.Status == 1) //待受理
                        .WhereIf(request.Type == 2, s => s.Status == 2 && s.ServiceWorkOrders.Any(a => a.Status < 7))//已受理
                        .WhereIf(request.Type == 3, q => q.ServiceWorkOrders.All(a => a.Status == 7) && q.ServiceWorkOrders.Count > 0) //待评价
                        .WhereIf(request.Type == 4, q => q.ServiceWorkOrders.All(a => a.Status == 8) && q.ServiceWorkOrders.Count > 0)//已评价
                        .Select(a => new
                        {
                            a.Id,
                            a.AppUserId,
                            a.Services,
                            a.CreateTime,
                            a.Status,
                            a.Province,
                            a.City,
                            a.Area,
                            a.Addr,
                            a.Contacter,
                            a.ContactTel,
                            NewestContacter = string.IsNullOrEmpty(a.NewestContacter) ? a.Contacter : a.NewestContacter,
                            NewestContactTel = string.IsNullOrEmpty(a.NewestContactTel) ? a.ContactTel : a.NewestContactTel,
                            a.CustomerName,
                            a.CustomerId,
                            a.U_SAP_ID,
                            a.ProblemTypeId,
                            a.ProblemTypeName,
                            ServiceWorkOrders = a.ServiceWorkOrders.Select(o => new
                            {
                                o.Id,
                                o.Status,
                                o.FromTheme,
                                ProblemType = o.ProblemType.Description,
                                o.ManufacturerSerialNumber,
                                o.MaterialCode,
                                o.CurrentUserId,
                                MaterialType = "其他设备".Equals(o.MaterialCode) ? "其他设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-"))
                            }).ToList(),
                        });


            var count = await query.CountAsync();
            var list = (await query
                .OrderByDescending(a => a.Id)
                .Skip((request.page - 1) * request.limit).Take(request.limit)
                .ToListAsync())
                .Select(a => new
                {
                    a.Id,
                    a.AppUserId,
                    a.Services,
                    CreateTime = a.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                    a.Status,
                    a.Province,
                    a.City,
                    a.Area,
                    a.Addr,
                    a.Contacter,
                    a.ContactTel,
                    a.NewestContacter,
                    a.NewestContactTel,
                    a.CustomerName,
                    a.CustomerId,
                    a.U_SAP_ID,
                    a.ProblemTypeId,
                    a.ProblemTypeName,
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                    {
                        MaterialType = s.Key,
                        MaterialTypeName = "其他设备".Equals(s.Key) ? "其他设备" : MaterialTypeModel.Where(m => m.TypeAlias == s.Key).FirstOrDefault().TypeName,
                        TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                        Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                        Count = s.Count(),
                        Orders = s.ToList()
                    }
                    ).ToList(),
                    WorkOrderState = a.ServiceWorkOrders.Distinct().OrderBy(o => o.Status).FirstOrDefault()?.Status
                });

            var result = new TableData();
            result.Count = count;
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 获取客户服务单详情/获取管理员服务单详情(已预约)
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<dynamic>> AppLoadServiceOrderDetails(AppQueryServiceOrderReq request)
        {
            var result = new Response<dynamic>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取技术员Id
            var workOrderInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId)
                  .WhereIf("其他设备".Equals(request.MaterialType), a => a.MaterialCode == "其他设备")
                  .WhereIf(!"其他设备".Equals(request.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == request.MaterialType)
                  .FirstOrDefaultAsync();
            var currentTechnicianId = workOrderInfo?.CurrentUserId;
            var status = workOrderInfo?.Status;
            //获取技术员电话
            string Mobile = (await GetProtectPhone(request.ServiceOrderId, request.MaterialType, 0)).Data;
            //获取技术员位置信息
            var locations = await UnitWork.Find<RealTimeLocation>(r => r.AppUserId == currentTechnicianId).OrderByDescending(o => o.CreateTime).Select(s => new
            {
                s.AppUserId,
                s.Addr,
                s.Area,
                s.City,
                CreateTime = s.CreateTime.ToString("yyyy.MM.dd HH:mm:ss"),
                s.Latitude,
                s.Longitude,
                s.Province,
                s.Id
            }).FirstOrDefaultAsync();
            var query = UnitWork.Find<ServiceOrder>(s => s.Id == request.ServiceOrderId)
                        .Select(a => new
                        {
                            a.Id,
                            a.AppUserId,
                            a.Services,
                            a.CreateTime,
                            a.Status,
                            a.U_SAP_ID,
                            a.Longitude,
                            a.Latitude
                        });
            if (locations == null)
            {
                var defaultlocation = new { AppUserId = currentTechnicianId, Addr = "未获取到位置信息", Latitude = string.Empty, Longitude = string.Empty, Province = string.Empty, City = string.Empty, Area = string.Empty, Id = 0, CreateTime = string.Empty };
                var newdata = (await query.ToListAsync()).Select(a => new
                {
                    a.Id,
                    a.AppUserId,
                    a.Services,
                    CreateTime = a.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                    a.Status,
                    a.U_SAP_ID,
                    a.Latitude,
                    a.Longitude,
                    WorkOrderStatus = status,
                    TechnicianLocation = defaultlocation,
                    TechnicianTel = Mobile,
                    TechnicianId = currentTechnicianId
                }).ToList();
                result.Result = newdata;
                return result;
            }
            var data = (await query.ToListAsync()).Select(a => new
            {
                a.Id,
                a.AppUserId,
                a.Services,
                CreateTime = a.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                a.Status,
                a.U_SAP_ID,
                a.Latitude,
                a.Longitude,
                WorkOrderStatus = status,
                TechnicianLocation = locations,
                TechnicianTel = Mobile,
                TechnicianId = currentTechnicianId
            }).ToList();
            result.Result = data;
            return result;
        }

        /// <summary>
        /// APP提交申请售后
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<ServiceOrder> Add(AddServiceOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var obj = req.MapTo<ServiceOrder>();
            obj.CustomerId = obj.CustomerId.ToUpper();
            obj.CreateTime = DateTime.Now;
            obj.CreateUserId = loginContext.User.Id;
            obj.RecepUserId = loginContext.User.Id;
            obj.RecepUserName = loginContext.User.Name;
            obj.Status = 1;
            obj.FromId = 6;//APP提交

            var obj2 = from a in UnitWork.Find<OCRD>(null)
                       join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                       from b in ab.DefaultIfEmpty()
                       join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                       from e in ae.DefaultIfEmpty()
                       select new { a, b, e };
            obj2 = obj2.Where(o => o.a.CardCode.Equals(obj.CustomerId));

            var query = obj2.Select(q => new
            {
                q.a.CardCode,
                q.a.CardName,
                q.a.CntctPrsn,
                q.a.Phone1,
                q.a.SlpCode,
                q.b.SlpName,
                TechID = q.a.DfTcnician,
                TechName = $"{q.e.lastName ?? ""}{q.e.firstName}"
            });
            var o2 = await query.FirstOrDefaultAsync();
            if (o2 != null)
            {
                obj.SalesMan = o2.SlpName;
                obj.Supervisor = o2.TechName;
            }

            var o = await UnitWork.AddAsync<ServiceOrder, int>(obj);
            await UnitWork.SaveAsync();
            var pictures = req.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = o.Id; p.PictureType = 1; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();
            #region 同步到SAP 并拿到服务单主键
            _capBus.Publish("Serve.ServcieOrder.CreateFromAPP", obj.Id);
            #endregion
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "提交信息成功",
                Details = "已收到您的反馈，正在为您分配客服中",
                ServiceOrderId = o.Id,
                LogType = 1
            });
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "客户发起售后",
                Details = "客户申请售后，请尽快处理",
                ServiceOrderId = o.Id,
                LogType = 2
            });
            var MaterialTypes = string.Join(",", obj.ServiceOrderSNs?.Select(s => s.ItemCode == "其他设备" ? "其他设备" : s.ItemCode.Substring(0, s.ItemCode.IndexOf("-"))).Distinct().ToArray());

            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"APP客户提交售后申请创建服务单", ActionType = "创建服务单", ServiceOrderId = o.Id, MaterialType = MaterialTypes });
            return o;
        }

        /// <summary>
        /// 获取设备列表中间页/售后详情页（客户）
        /// </summary>
        /// <param name="ServiceOrderId">服务单Id</param>
        /// <returns></returns>
        public async Task<TableData> GetAppCustServiceOrderDetails(int ServiceOrderId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取设备类型列表
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            //获取当前服务单中的工单是否都已提交完工报告
            var IsCanEvaluate = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ServiceOrderId && s.Status != 7).ToListAsync()).Count > 0 ? 0 : 1;
            var query = UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId)
                .Include(s => s.ServiceOrderSNs)
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType);
            var list = (await query
            .ToListAsync()).Select(s => new
            {
                s.Id,
                s.AppUserId,
                s.Services,
                s.Latitude,
                s.Longitude,
                s.Province,
                s.City,
                s.Area,
                s.Addr,
                s.Status,
                CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                NewestContacter = string.IsNullOrEmpty(s.NewestContacter) ? s.Contacter : s.NewestContacter,
                NewestContactTel = string.IsNullOrEmpty(s.NewestContactTel) ? s.ContactTel : s.NewestContactTel,
                s.CustomerName,
                s.Supervisor,
                s.SalesMan,
                s.U_SAP_ID,
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.ServiceWorkOrders.FirstOrDefault()?.ProblemType.Name : s.ProblemTypeName,
                ProblemTypeId = string.IsNullOrEmpty(s.ProblemTypeId) ? s.ServiceWorkOrders.FirstOrDefault()?.ProblemType.Id : s.ProblemTypeId,
                DeviceInfos = s.ServiceOrderSNs.GroupBy(o => "其他设备".Equals(o.ItemCode) ? "其他设备" : o.ItemCode.Substring(0, o.ItemCode.IndexOf("-"))).ToList()
                .Select(a => new
                {
                    MaterialType = a.Key,
                    UnitName = "台",
                    Count = a.Count(),
                    orders = a.ToList(),
                    Status = s.ServiceWorkOrders.FirstOrDefault(b => "其他设备".Equals(a.Key) ? b.MaterialCode == "其他设备" : b.MaterialCode.Contains(a.Key))?.Status,
                    MaterialTypeName = "其他设备".Equals(a.Key) ? "其他设备" : MaterialTypeModel.Where(m => m.TypeAlias == a.Key).FirstOrDefault().TypeName
                }),
                IsCanEvaluate //0不可评价 1可评价
            }).ToList();
            result.Data = list;
            return result;
        }
        #endregion

        #region<<Technician>>
        private static string GetServiceFromTheme(string fromtheme)
        {
            string result = string.Empty;
            if (!string.IsNullOrEmpty(fromtheme))
            {
                JArray jArray = (JArray)JsonConvert.DeserializeObject(fromtheme);
                foreach (var item in jArray)
                {
                    result += item["description"] + " ";
                }
            }
            return result;
        }
        /// <summary>
        /// 获取技术员设备类型列表/售后详情
        /// </summary>
        /// <param name="SapOrderId"></param>
        /// <param name="CurrentUserId"></param>
        /// <param name="MaterialType"></param>
        /// <returns></returns>
        public async Task<TableData> AppTechnicianLoad(int SapOrderId, int CurrentUserId, string MaterialType)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == CurrentUserId).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var ServiceOrderId = (await UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == SapOrderId).FirstOrDefaultAsync()).Id;
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            //获取当前服务单的售后进度
            var flowList = await UnitWork.Find<ServiceFlow>(s => s.ServiceOrderId == ServiceOrderId && s.Creater == userInfo.UserID && s.FlowType == 2).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed, s.MaterialType }).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == SapOrderId)
                        .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                        .Select(a => new
                        {
                            ServiceOrderId = a.Id,
                            a.CreateTime,
                            a.Province,
                            a.City,
                            a.Area,
                            a.Addr,
                            a.Supervisor,
                            a.SalesMan,
                            a.CustomerName,
                            a.ProblemTypeId,
                            a.ProblemTypeName,
                            NewestContacter = string.IsNullOrEmpty(a.NewestContacter) ? a.Contacter : a.NewestContacter,
                            NewestContactTel = string.IsNullOrEmpty(a.NewestContactTel) ? a.ContactTel : a.NewestContactTel,
                            AppCustId = a.AppUserId,
                            ServiceWorkOrders = a.ServiceWorkOrders.Where(w => w.CurrentUserId == CurrentUserId && (string.IsNullOrEmpty(MaterialType) ? true : "其他设备".Equals(MaterialType) ? w.MaterialCode == "其他设备" : w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-")) == MaterialType)).Select(o => new
                            {
                                o.Id,
                                o.Status,
                                o.FromTheme,
                                o.ManufacturerSerialNumber,
                                o.MaterialCode,
                                o.CurrentUserId,
                                MaterialType = o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                                o.ProblemType,
                                o.Priority,
                                o.ServiceMode
                            }).ToList()
                        });


            var count = await query.CountAsync();
            var list = (await query
                .ToListAsync())
                .Select(a => new
                {
                    a.ServiceOrderId,
                    CreateTime = a.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                    a.Province,
                    a.City,
                    a.Area,
                    a.Addr,
                    a.NewestContacter,
                    a.NewestContactTel,
                    a.AppCustId,
                    a.Supervisor,
                    a.SalesMan,
                    a.CustomerName,
                    ProblemTypeName = string.IsNullOrEmpty(a.ProblemTypeName) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType.Name : a.ProblemTypeName,
                    ProblemTypeId = string.IsNullOrEmpty(a.ProblemTypeId) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType.Id : a.ProblemTypeId,
                    Services = GetServiceFromTheme(a.ServiceWorkOrders.FirstOrDefault()?.FromTheme),
                    Priority = a.ServiceWorkOrders.FirstOrDefault()?.Priority == 3 ? "高" : a.ServiceWorkOrders.FirstOrDefault()?.Priority == 2 ? "中" : "低",
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                    {
                        MaterialType = string.IsNullOrEmpty(s.Key) ? "其他设备" : s.Key,
                        TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                        Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                        Count = s.Count(),
                        Orders = s.ToList(),
                        UnitName = "台",
                        MaterialTypeName = string.IsNullOrEmpty(s.Key) ? "其他设备" : MaterialTypeModel.Where(a => a.TypeAlias == s.Key).FirstOrDefault().TypeName,
                        ServiceMode = s.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                        flowinfo = flowList.Where(w => w.MaterialType == (string.IsNullOrEmpty(s.Key) ? "其他设备" : s.Key)).ToList()
                    }
                    ).ToList(),
                    flowInfo = flowList.Where(w => w.MaterialType == MaterialType).ToList()
                });
            result.Count = count;
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 获取技术员服务单详情
        /// </summary>
        /// <param name="SapOrderId">SapId</param>
        /// <param name="CurrentUserId">当前技术员App用户Id</param>
        /// <param name="MaterialType">设备类型</param>
        /// <returns></returns>
        public async Task<TableData> GetAppTechnicianServiceOrderDetails(int SapOrderId, int CurrentUserId, string MaterialType)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == CurrentUserId).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var ServiceOrderId = (await UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == SapOrderId).FirstOrDefaultAsync()).Id;
            //获取当前服务单的设备类型接单状态进度
            var workOrderInfo = await UnitWork.Find<ServiceWorkOrder>(null).Where(s => s.CurrentUserId == CurrentUserId && s.ServiceOrderId == ServiceOrderId)
                .WhereIf("其他设备".Equals(MaterialType), a => a.MaterialCode == "其他设备")
                .WhereIf(!"其他设备".Equals(MaterialType), o => o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")) == MaterialType).OrderBy(o => o.OrderTakeType).FirstOrDefaultAsync();
            //获取客户号码 做隐私处理
            string custMobile = (await GetProtectPhone(ServiceOrderId, MaterialType, 1)).Data;
            var query = UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == SapOrderId)
                .Include(s => s.ServiceWorkOrders);
            //获取当前设备类型的售后进度
            var flowInfo = await UnitWork.Find<ServiceFlow>(s => s.ServiceOrderId == ServiceOrderId && s.MaterialType == MaterialType && s.Creater == userInfo.UserID && s.FlowType == 2).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToListAsync();
            var list = (await query
                         .ToListAsync()).Select(s => new
                         {
                             s.Id,
                             s.AppUserId,
                             s.Services,
                             s.Latitude,
                             s.Longitude,
                             s.Province,
                             s.City,
                             s.Area,
                             s.Addr,
                             s.Status,
                             CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                             s.CustomerName,
                             s.Supervisor,
                             s.SalesMan,
                             s.U_SAP_ID,
                             s.ProblemTypeName,
                             s.ProblemTypeId,
                             NewestContacter = string.IsNullOrEmpty(s.NewestContacter) ? s.Contacter : s.NewestContacter,
                             NewestContactTel = string.IsNullOrEmpty(s.NewestContactTel) ? s.ContactTel : s.NewestContactTel,
                             custMobile,
                             workOrderInfo.OrderTakeType,
                             s.CustomerId,
                             workOrderInfo.ServiceMode,
                             flowInfo
                         }).ToList();
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 保存解决方案
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SaveWorkOrderSolution(SaveWorkOrderSolutionReq req)
        {
            //获取当前设备的工单集合
            var orderIds = await UnitWork.Find<ServiceWorkOrder>(null).Where(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId)
                .WhereIf("其他设备".Equals(req.MaterialType), a => a.MaterialCode == "其他设备")
                .WhereIf(!"其他设备".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType).Select(s => s.Id)
                .ToListAsync();
            List<int> workOrderIds = new List<int>();
            foreach (var id in orderIds)
            {
                workOrderIds.Add(id);
            }
            //获取解决方案名称
            var SolutionName = (await UnitWork.Find<Solution>(s => s.UseBy == 2 && s.Id == req.SolutionId).FirstOrDefaultAsync()).Subject;
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => workOrderIds.Contains(s.Id), o => new ServiceWorkOrder
            {
                ProcessDescription = SolutionName
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取售后详情（技术员）/获取已完成的服务单详情-受理确认（客户）
        /// </summary>
        /// <param name="ServiceOrderId">服务单Id</param>
        /// <returns></returns>
        public async Task<TableData> GetAppTechServiceOrderDetails(int ServiceOrderId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取设备类型列表
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId)
                .Include(s => s.ServiceOrderSNs)
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType);
            var list = (await query
            .ToListAsync()).Select(s => new
            {
                s.Id,
                s.AppUserId,
                s.Services,
                s.Latitude,
                s.Longitude,
                s.Province,
                s.City,
                s.Area,
                s.Addr,
                s.Status,
                CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                NewestContacter = string.IsNullOrEmpty(s.NewestContacter) ? s.Contacter : s.NewestContacter,
                NewestContactTel = string.IsNullOrEmpty(s.NewestContactTel) ? s.ContactTel : s.NewestContactTel,
                s.CustomerName,
                s.Supervisor,
                s.SalesMan,
                s.U_SAP_ID,
                s.CustomerId,
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.ServiceWorkOrders.FirstOrDefault()?.ProblemType.Name : s.ProblemTypeName,
                ProblemTypeId = string.IsNullOrEmpty(s.ProblemTypeId) ? s.ServiceWorkOrders.FirstOrDefault()?.ProblemType.Id : s.ProblemTypeId,
                DeviceInfos = s.ServiceWorkOrders.GroupBy(o => "其他设备".Equals(o.MaterialCode) ? "其他设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-"))).ToList()
                .Select(a => new
                {
                    MaterialType = a.Key,
                    UnitName = "台",
                    Count = a.Count(),
                    orders = a.Select(a => new { a.MaterialCode, a.ManufacturerSerialNumber, a.Id }).ToList(),
                    Status = s.ServiceWorkOrders.FirstOrDefault(b => "其他设备".Equals(a.Key) ? b.MaterialCode == "其他设备" : b.MaterialCode.Contains(a.Key))?.Status,
                    MaterialTypeName = "其他设备".Equals(a.Key) ? "其他设备" : MaterialTypeModel.Where(m => m.TypeAlias == a.Key).FirstOrDefault().TypeName
                })
            }).ToList();
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 选择接单类型
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> SaveOrderTakeType(SaveWorkOrderTakeTypeReq request)
        {
            var result = new TableData();
            var servicemode = 0;
            var orderIds = await UnitWork.Find<ServiceWorkOrder>(null).Where(s => s.ServiceOrderId == request.ServiceOrderId && s.CurrentUserId == request.CurrentUserId)
    .WhereIf("其他设备".Equals(request.MaterialType), a => a.MaterialCode == "其他设备")
    .WhereIf(!"其他设备".Equals(request.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == request.MaterialType).Select(s => s.Id)
    .ToListAsync();
            List<int> workOrderIds = new List<int>();
            foreach (var id in orderIds)
            {
                workOrderIds.Add(id);
            }
            //判断是再次拨打电话 则标记为电话服务 若不是则取当前工单设备类型的服务方式
            var workOrderInfo = await UnitWork.Find<ServiceWorkOrder>(s => workOrderIds.Contains(s.Id)).FirstOrDefaultAsync();
            if (workOrderInfo != null)
            {
                servicemode = workOrderInfo.ServiceMode == null ? 0 : (int)workOrderInfo.ServiceMode;
            }
            int status = 2;
            //拨打完电话 工单状态变为已预约
            if (request.Type == 3 || request.Type == 1)
            {
                status = 3;
                servicemode = 2;
                await UnitWork.UpdateAsync<ServiceWorkOrder>(s => workOrderIds.Contains(s.Id) && s.BookingDate == null, e => new ServiceWorkOrder { BookingDate = DateTime.Now });
            }
            else if (request.Type == 2)
            {
                servicemode = 1;
            }
            else if (request.Type > 4)
            {
                status = 4;
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => workOrderIds.Contains(s.Id), e => new ServiceWorkOrder
            {
                OrderTakeType = request.Type,
                Status = status,
                ServiceMode = servicemode
            });
            await UnitWork.SaveAsync();
            //添加流程信息
            var flowRequest = new AddOrUpdateServerFlowReq { AppUserId = request.CurrentUserId, FlowNum = request.Type, ServiceOrderId = request.ServiceOrderId, MaterialType = request.MaterialType };
            await _serviceFlowApp.AddOrUpdateServerFlow(flowRequest);
            //获取当前设备类型流程信息返回
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == request.CurrentUserId).Include(a => a.User).FirstOrDefaultAsync();
            if (userInfo.User == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var flowInfo = await UnitWork.Find<ServiceFlow>(w => w.ServiceOrderId == request.ServiceOrderId && w.MaterialType == request.MaterialType && w.FlowType == 1 && w.Creater == userInfo.User.Id).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToListAsync();
            result.Data = flowInfo;
            return result;
        }

        /// <summary>
        /// 获取工单服务详情（废弃）
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetAppServiceOrderDetail(QueryServiceOrderDetailReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            int Type = 0;
            //判断当前技术员是否已经接过单据 如果有则直接进入详情
            var count = UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == req.CurrentUserId && s.ServiceOrderId == req.ServiceOrderId).ToList().Count;
            if (count > 0)
            {
                Type = 1;
            }
            var result = new TableData();
            var query = UnitWork.Find<ServiceOrder>(s => s.Status == 2 && s.Id == req.ServiceOrderId)
                .Where(s => s.Id == req.ServiceOrderId && s.Status == 2)
                .Select(s => new
                {
                    s.Id,
                    s.Latitude,
                    s.Longitude,
                    s.Status,
                    s.Services,
                    s.CreateTime,
                    s.AppUserId,
                    s.Province,
                    s.City,
                    s.Area,
                    s.Addr,
                    Contacter = string.IsNullOrEmpty(s.NewestContacter) ? s.Contacter : s.NewestContacter,
                    ContacterTel = string.IsNullOrEmpty(s.NewestContactTel) ? s.ContactTel : s.NewestContactTel,
                    s.CustomerName,
                    s.Supervisor,
                    s.SalesMan,
                    s.U_SAP_ID,
                    MaterialInfo = s.ServiceWorkOrders.Where(o => o.ServiceOrderId == req.ServiceOrderId && Type == 1 ? o.CurrentUserId == req.CurrentUserId : o.Status == 1).Select(o => new
                    {
                        o.MaterialCode,
                        o.ManufacturerSerialNumber,
                        MaterialType = "其他设备".Equals(o.MaterialCode) ? "其他设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                        o.Status,
                        o.Id,
                        o.IsCheck,
                        o.OrderTakeType
                    })
                });

            var list = (await query
            .ToListAsync()).Select(s => new
            {
                s.Id,
                s.AppUserId,
                s.Services,
                s.Latitude,
                s.Longitude,
                s.Province,
                s.City,
                s.Area,
                s.Addr,
                s.Status,
                CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                s.Contacter,
                s.ContacterTel,
                s.CustomerName,
                s.Supervisor,
                s.SalesMan,
                s.U_SAP_ID,
                Distance = (req.Latitude == 0 || s.Latitude is null) ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(s.Latitude ?? 0), Convert.ToDouble(s.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)),
                s.MaterialInfo,
                ServiceWorkOrders = s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                .Select(s => new
                {
                    MaterialType = s.Key,
                    WorkOrderIds = string.Join(",", s.Select(i => i.Id))
                }),
                WorkOrderState = s.MaterialInfo.Distinct().OrderBy(o => o.Status).FirstOrDefault()?.Status,
                OrderTakeType = s.MaterialInfo.Distinct().OrderBy(o => o.OrderTakeType).FirstOrDefault()?.OrderTakeType
            }).ToList();
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 修改故障描述
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateWorkOrderDescription(UpdateWorkOrderDescriptionReq request)
        {
            var orderIds = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.CurrentUserId == request.CurrentUserId)
                .WhereIf("其他设备".Equals(request.MaterialType), a => a.MaterialCode == "其他设备")
                .WhereIf(!"其他设备".Equals(request.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == request.MaterialType)
                .ToListAsync()).Select(s => s.Id).ToList();
            List<int> workOrderIds = new List<int>();
            foreach (var id in orderIds)
            {
                workOrderIds.Add(id);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.CurrentUserId == request.CurrentUserId && orderIds.Contains(s.Id), e => new ServiceWorkOrder
            {
                TroubleDescription = request.Description
            });
            await UnitWork.SaveAsync();
        }


        /// <summary>
        /// 技术员预约工单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task BookingWorkOrder(BookingWorkOrderReq req)
        {
            var orderIds = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId)
                .WhereIf("其他设备".Equals(req.MaterialType), a => a.MaterialCode == "其他设备")
                .WhereIf(!"其他设备".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType)
                .ToListAsync()).Select(s => new { s.Id, s.WorkOrderNumber }).ToList();
            List<int> workOrderIds = new List<int>();
            foreach (var item in orderIds)
            {
                workOrderIds.Add(item.Id);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && workOrderIds.Contains(s.Id), o => new ServiceWorkOrder
            {
                BookingDate = req.BookingDate,
                OrderTakeType = 4,
                Status = 3,
                ServiceMode = 1
            });
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员预约服务时间",
                Details = $"为您分配的技术员预约了服务时间，请注意接听来电",
                LogType = 1,
                ServiceOrderId = req.ServiceOrderId,
                ServiceWorkOrder = string.Join(',', workOrderIds.ToArray()),
                MaterialType = req.MaterialType
            });

            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员预约时间成功",
                Details = $"已预约时间，请尽快完成电访，并安排你的服务行程。避免可能存在的行程风险问题",
                LogType = 2,
                ServiceOrderId = req.ServiceOrderId,
                ServiceWorkOrder = string.Join(',', workOrderIds.ToArray()),
                MaterialType = req.MaterialType
            });
            var username = UnitWork.Find<AppUserMap>(a => a.AppUserId.Equals(req.CurrentUserId)).Include(a => a.User).Select(a => a.User.Name).FirstOrDefault();

            await _ServiceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{username}预约工单{string.Join(",", orderIds.Select(s => s.WorkOrderNumber).ToArray())}", ActionType = "预约工单", MaterialType = req.MaterialType }, workOrderIds);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = req.ServiceOrderId, Content = "技术员已预约上门时间成功，请尽早安排行程", AppUserId = 0 });
            //添加流程信息
            var flowRequest = new AddOrUpdateServerFlowReq { AppUserId = req.CurrentUserId, FlowNum = 4, ServiceOrderId = req.ServiceOrderId, MaterialType = req.MaterialType };
            await _serviceFlowApp.AddOrUpdateServerFlow(flowRequest);
        }

        /// <summary>
        /// 技术员核对设备(正确/错误)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task CheckTheEquipment(CheckTheEquipmentReq req)
        {
            int orderTakeType = 0;
            //判断当前操作者是否有操作权限
            var order = await UnitWork.FindSingleAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId);
            if (order == null)
            {
                throw new CommonException("当前技术员无法核对此工单设备。", 9001);
            }
            var username = UnitWork.Find<AppUserMap>(a => a.AppUserId.Equals(req.CurrentUserId)).Include(a => a.User).Select(a => a.User.Name).FirstOrDefault();
            List<int> workOrderIds = new List<int>();
            //处理核对成功的设备信息
            if (!string.IsNullOrEmpty(req.CheckWorkOrderIds))
            {
                string[] checkArr = req.CheckWorkOrderIds.Split(',');
                if (checkArr.Length > 0)
                {
                    foreach (var itemcheck in checkArr)
                    {
                        workOrderIds.Add(int.Parse(itemcheck));
                        await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id == int.Parse(itemcheck), o => new ServiceWorkOrder
                        {
                            IsCheck = 1
                        });
                        var WorkNumber = await UnitWork.Find<ServiceWorkOrder>(s => s.Id == int.Parse(itemcheck)).Select(s => s.WorkOrderNumber).FirstOrDefaultAsync();
                        await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{username}核对工单{WorkNumber}设备（成功）", ActionType = "核对设备", ServiceWorkOrderId = int.Parse(itemcheck), MaterialType = req.MaterialType });
                    }
                }
            }
            //处理核对失败的设备信息
            if (!string.IsNullOrEmpty(req.ErrorWorkOrderIds))
            {
                string[] errorArr = req.ErrorWorkOrderIds.Split(',');
                if (errorArr.Length > 0)
                {
                    foreach (var itemerr in errorArr)
                    {
                        await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id == int.Parse(itemerr), o => new ServiceWorkOrder
                        {
                            IsCheck = 2,
                            Status = 3
                        });
                        var WorkNumber = await UnitWork.Find<ServiceWorkOrder>(s => s.Id == int.Parse(itemerr)).Select(s => s.WorkOrderNumber).FirstOrDefaultAsync();
                        await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{username}核对工单{WorkNumber}设备(错误)", ActionType = "核对设备", ServiceWorkOrderId = int.Parse(itemerr), MaterialType = req.MaterialType });
                    }
                }
            }
            else
            {
                //判断没有错误的设备信息并且没有待确认的设备再更新状态
                var applyCount = (await UnitWork.Find<SeviceTechnicianApplyOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.MaterialType.Contains(req.MaterialType) && s.TechnicianId == req.CurrentUserId).ToListAsync()).Count;
                if (applyCount == 0)
                {
                    orderTakeType = 5;
                    await UnitWork.UpdateAsync<ServiceWorkOrder>(s => workOrderIds.Contains(s.Id), o => new ServiceWorkOrder
                    {
                        IsCheck = 1,
                        Status = 4,
                        OrderTakeType = orderTakeType
                    });
                }
            }
            //更新上门时间
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => workOrderIds.Contains(s.Id) && s.VisitTime == null, o => new ServiceWorkOrder { VisitTime = DateTime.Now });
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员已外出",
                Details = $"技术员当前正在为您上门服务",
                LogType = 1,
                ServiceOrderId = req.ServiceOrderId,
                ServiceWorkOrder = req.CheckWorkOrderIds,
                MaterialType = req.MaterialType
            });
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员完成设备信息核对",
                Details = $"已核对设备，请尽快开始为客户提供高效优质的服务",
                LogType = 2,
                ServiceOrderId = req.ServiceOrderId,
                ServiceWorkOrder = req.CheckWorkOrderIds,
                MaterialType = req.MaterialType
            });
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = req.ServiceOrderId, Content = "技术员已核对设备，请完成维修任务", AppUserId = 0 });
            //添加流程
            if (orderTakeType == 5)
            {
                var flowRequest = new AddOrUpdateServerFlowReq { AppUserId = req.CurrentUserId, FlowNum = 5, ServiceOrderId = req.ServiceOrderId, MaterialType = req.MaterialType };
                await _serviceFlowApp.AddOrUpdateServerFlow(flowRequest);
            }
        }

        /// <summary>
        /// 获取技术员服务单工单列表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetAppTechnicianServiceWorkOrder(GetAppTechnicianServiceWorkOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var query = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.TechnicianId);
            var listQuery = query.WhereIf(req.Type == 1, s => s.Status > 1 && s.Status < 7)
                .WhereIf(req.Type == 2, s => s.Status >= 7)
                .Select(s => new
                {
                    s.Id,
                    ProblemType = s.ProblemType.Description,
                    s.Priority,
                    s.CreateTime,
                    s.InternalSerialNumber,
                    s.ManufacturerSerialNumber,
                    s.MaterialCode,
                    s.Status,
                    s.WorkOrderNumber,
                    MaterialType = s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-"))
                });
            var result = new TableData();
            var list = (await listQuery
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync())
            .GroupBy(s => s.MaterialType).Select(s => new
            {
                MaterialType = s.Key,
                Count = s.Count(),
                WorkOrders = s.ToList()
            });

            var count = await listQuery.CountAsync();
            result.Data = list;
            result.Count = count;
            return result;

        }

        /// <summary>
        /// 技术员接单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task TechnicianTakeOrder(TechnicianTakeOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //判断当前单据是否已经被接单
            var workOrderInfo = await UnitWork.Find<ServiceWorkOrder>(s => req.ServiceWorkOrderIds.Contains(s.Id) && s.Status > 1).ToListAsync();
            if (workOrderInfo.Count() > 0)
            {
                throw new CommonException("当前工单已被接单", 90005);
            }
            var b = await CheckCanTakeOrder(req.TechnicianId);

            if (!b)
            {
                throw new CommonException("当前技术员接单已满6单服务单", 90004);
            }
            int serviceOrderId = workOrderInfo.First().ServiceOrderId;
            var u = await UnitWork.Find<AppUserMap>(s => s.AppUserId == req.TechnicianId).Include(s => s.User).FirstOrDefaultAsync();
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => req.ServiceWorkOrderIds.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUser = u.User.Name,
                CurrentUserNsapId = u.User.Id,
                Status = 2,
                CurrentUserId = req.TechnicianId
            });
            await UnitWork.SaveAsync();

            var typename = "其他设备".Equals(workOrderInfo.FirstOrDefault().MaterialCode) ? "其他设备" : workOrderInfo.FirstOrDefault().MaterialCode.Substring(0, workOrderInfo.FirstOrDefault().MaterialCode.IndexOf("-"));
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单",
                Details = "已为您分配专属技术员进行处理，感谢您的耐心等待",
                LogType = 1,
                ServiceOrderId = serviceOrderId,
                ServiceWorkOrder = String.Join(',', req.ServiceWorkOrderIds.ToArray()),
                MaterialType = typename
            });

            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单成功",
                Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                LogType = 2,
                ServiceOrderId = serviceOrderId,
                ServiceWorkOrder = String.Join(',', req.ServiceWorkOrderIds.ToArray()),
                MaterialType = typename
            });
            var WorkOrderNumbers = String.Join(',', UnitWork.Find<ServiceWorkOrder>(s => req.ServiceWorkOrderIds.Contains(s.Id)).Select(s => s.WorkOrderNumber).ToArray());
            await _ServiceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员:{u.User.Name}接单工单：{WorkOrderNumbers}", ActionType = "技术员接单", MaterialType = typename }, req.ServiceWorkOrderIds);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = req.ServiceOrderId, Content = "技术员已接单成功，请尽快选择服务", AppUserId = 0 });
            await PushMessageToApp(req.TechnicianId, "接单成功提醒", "您已成功接取一个新的售后服务，请尽快处理");
        }

        /// <summary>
        /// 技术员工单池列表（暂不使用）
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceWorkOrderPool(TechnicianServiceWorkOrderPoolReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }


            var result = new TableData();
            var query = UnitWork.Find<ServiceOrder>(s => s.Status == 2)
                .Include(s => s.ServiceWorkOrders).Where(q => q.ServiceWorkOrders.Any(a => a.Status == 1) && !q.ServiceWorkOrders.Any(a => a.CurrentUserId == req.CurrentUserId))
                .WhereIf(int.TryParse(req.key, out int id) || !string.IsNullOrWhiteSpace(req.key), s => s.U_SAP_ID == id || s.CustomerName.Contains(req.key) || s.ServiceWorkOrders.Any(o => o.ManufacturerSerialNumber.Contains(req.key)))
                .Select(s => new
                {
                    s.Id,
                    s.Latitude,
                    s.Longitude,
                    s.Status,
                    s.Services,
                    s.CreateTime,
                    s.AppUserId,
                    s.Province,
                    s.City,
                    s.Area,
                    s.Addr,
                    s.U_SAP_ID,
                    ServiceWorkOrders = s.ServiceWorkOrders.Where(o => o.Status == 1).Select(o => new
                    {
                        o.Id,
                        o.AppUserId,
                        o.FromTheme,
                        ProblemType = o.ProblemType.Description,
                        o.CreateTime,
                        o.Status,
                        o.MaterialCode,
                        MaterialType = o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                        o.FeeType,
                        o.InternalSerialNumber,
                        o.ManufacturerSerialNumber,
                        o.Priority,
                        o.Remark,
                    })
                });

            var list = (await query.OrderByDescending(o => o.Id)
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync()).Select(a => new
            {
                a.Id,
                a.Latitude,
                a.Longitude,
                a.Status,
                a.Services,
                CreateTime = a.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                a.AppUserId,
                a.Province,
                a.City,
                a.Area,
                a.Addr,
                a.U_SAP_ID,
                Distance = (req.Latitude == 0 || a.Latitude is null) ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(a.Latitude ?? 0), Convert.ToDouble(a.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)),
                ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                {
                    MaterialType = s.Key,
                    Count = s.Count(),
                    Orders = s.ToList()
                }
                ).ToList()
            });

            var count = await query.CountAsync();
            result.Data = list;
            result.Count = count;
            return result;
        }

        /// <summary>
        /// 技术员查看服务单列表(技术员主页-工单池)
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceOrder(TechnicianServiceWorkOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.TechnicianId).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //获取设备信息
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == req.TechnicianId)
                .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
            //获取完工报告集合
            var completeReportList = await UnitWork.Find<CompletionReport>(w => serviceOrderIds.Contains((int)w.ServiceOrderId)).Select(s => new { s.ServiceOrderId, s.IsReimburse, s.Id, s.ServiceMode, MaterialType = s.MaterialCode == "其他设备" ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) }).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id))
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                .Include(s => s.ServiceFlows)
                .WhereIf(req.Type == 1, s => s.ServiceWorkOrders.All(a => a.OrderTakeType == 0))//待处理
                .WhereIf(req.Type == 2, s => s.ServiceWorkOrders.Any(a => a.Status > 1 && a.Status < 7 && a.OrderTakeType != 0))//进行中
                .WhereIf(req.Type == 3, s => s.ServiceWorkOrders.All(a => a.Status >= 7)) //已完成
                 .WhereIf(int.TryParse(req.key, out int id) || !string.IsNullOrWhiteSpace(req.key), s => s.U_SAP_ID == id || s.CustomerName.Contains(req.key) || s.ServiceWorkOrders.Any(o => o.ManufacturerSerialNumber.Contains(req.key)))
                .Select(s => new
                {
                    s.Id,
                    s.AppUserId,
                    Services = GetServiceFromTheme(s.ServiceWorkOrders.FirstOrDefault().FromTheme),
                    s.Latitude,
                    s.Longitude,
                    s.Province,
                    s.City,
                    s.Area,
                    s.Addr,
                    s.Contacter,
                    s.ContactTel,
                    s.NewestContacter,
                    s.NewestContactTel,
                    s.Status,
                    s.CreateTime,
                    s.U_SAP_ID,
                    s.CustomerId,
                    s.CustomerName,
                    s.TerminalCustomer,
                    Count = s.ServiceWorkOrders.Where(w => w.ServiceOrderId == s.Id && w.CurrentUserId == req.TechnicianId).Count(),
                    MaterialInfo = s.ServiceWorkOrders.Where(w => w.CurrentUserId == req.TechnicianId).Select(o => new
                    {
                        o.MaterialCode,
                        o.ManufacturerSerialNumber,
                        MaterialType = "其他设备".Equals(o.MaterialCode) ? "其他设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                        o.Status,
                        o.Id,
                        o.OrderTakeType,
                        o.ServiceMode
                    }),
                    s.ProblemTypeName,
                    ProblemType = s.ServiceWorkOrders.Select(s => s.ProblemType).FirstOrDefault(),
                    ServiceFlows = s.ServiceFlows.Where(w => w.Creater == userInfo.UserID && w.ServiceOrderId == s.Id && w.FlowType == 1).ToList()
                });

            var result = new TableData();
            var list = (await query.OrderByDescending(o => o.Id)
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync()).Select(s => new
            {
                s.Id,
                s.AppUserId,
                s.Services,
                s.Latitude,
                s.Longitude,
                s.Province,
                s.City,
                s.Area,
                s.Addr,
                NewestContacter = string.IsNullOrEmpty(s.NewestContacter) ? s.Contacter : s.NewestContacter,
                NewestContactTel = string.IsNullOrEmpty(s.NewestContactTel) ? s.ContactTel : s.NewestContactTel,
                s.Status,
                CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm"),
                s.U_SAP_ID,
                s.CustomerId,
                s.CustomerName,
                s.TerminalCustomer,
                Distance = (req.Latitude == 0 || s.Latitude is null) ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(s.Latitude ?? 0), Convert.ToDouble(s.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)),
                s.Count,
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.ProblemType.Name : s.ProblemTypeName,
                MaterialTypeQty = s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count,
                MaterialInfo = s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                .Select(o => new
                {
                    MaterialType = o.Key,
                    Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                    MaterialTypeName = "其他设备".Equals(o.Key) ? "其他设备" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                    OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                    ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                    flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList()
                }),
                IsReimburse = req.Type == 3 ? completeReportList.Where(w => w.ServiceOrderId == s.Id && w.ServiceMode == 1).FirstOrDefault() == null ? 0 : completeReportList.Where(w => w.ServiceOrderId == s.Id && w.ServiceMode == 1).FirstOrDefault().IsReimburse : 0,
                MaterialType = req.Type == 3 ? completeReportList.Where(w => w.ServiceOrderId == s.Id).FirstOrDefault() == null ? string.Empty : completeReportList.Where(w => w.ServiceOrderId == s.Id).FirstOrDefault().MaterialType : string.Empty
            }).ToList();

            var count = await query.CountAsync();
            result.Data = list;
            result.Count = count;
            return result;
        }

        #endregion

        #region<<Admin/Supervisor>>
        /// <summary>
        /// 获取管理员服务单列表（App）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AppUnConfirmedServiceOrderList(QueryAppServiceOrderListReq req)
        {
            var result = new TableData();
            int QryState = Convert.ToInt32(req.QryState);
            //获取主管的nsap用户Id
            var nsapUserId = (await UnitWork.Find<AppUserMap>(u => u.AppUserId == req.AppUserId).FirstOrDefaultAsync()).UserID;
            //判断账号是否为服务台帐号 若是则默认显示所有主管下的单据
            var nsapUserInfo = await UnitWork.Find<User>(u => u.Id == nsapUserId).FirstOrDefaultAsync();
            bool isAdmin = "lijianmei".Equals(nsapUserInfo.Account, StringComparison.OrdinalIgnoreCase);
            //获取设备类型列表
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(s => s.Status == 2 && s.CreateTime > Convert.ToDateTime("2020-08-01") && s.CreateTime != null && (isAdmin ? true : s.SupervisorId == nsapUserId)) //服务单已确认 
                         .Include(s => s.ServiceOrderSNs)
                         .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                         .WhereIf(QryState == 1, q => q.ServiceWorkOrders.Any(q => q.Status == 1))//待派单
                         .WhereIf(QryState == 2, q => q.ServiceWorkOrders.Any(q => q.Status > 1 && q.Status < 7))//已派单
                         .WhereIf(int.TryParse(req.key, out int id) || !string.IsNullOrWhiteSpace(req.key), s => (s.U_SAP_ID == id || s.U_SAP_ID == id || s.CustomerName.Contains(req.key) || s.ServiceWorkOrders.Any(o => o.ManufacturerSerialNumber.Contains(req.key))))
            .OrderBy(r => r.CreateTime).Select(q => new
            {
                q.Id,
                q.CustomerId,
                q.CustomerName,
                q.Services,
                q.CreateTime,
                q.Contacter,
                q.ContactTel,
                q.Supervisor,
                q.SalesMan,
                q.Status,
                q.Province,
                q.City,
                q.Area,
                q.Addr,
                q.U_SAP_ID,
                q.Longitude,
                q.Latitude,
                MaterialInfo = q.ServiceWorkOrders.Where(a => QryState > 0 ? QryState == 1 ? a.Status == 1 : a.Status > 1 && a.Status < 7 : true).Select(o => new
                {
                    o.MaterialCode,
                    o.ManufacturerSerialNumber,
                    MaterialType = "其他设备".Equals(o.MaterialCode) ? "其他设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                    o.Status,
                    o.Id,
                    o.ProblemType,
                    o.ProblemTypeId
                }),
                ServiceWorkOrders = q.ServiceWorkOrders.Where(w => !string.IsNullOrEmpty(w.MaterialCode)),
                q.ProblemTypeId,
                q.ProblemTypeName
            });

            //待派单按最新的单子在前排序
            if (QryState == 1)
            {
                query = query.OrderByDescending(o => o.Id);
            }
            var data = await query
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync();

            result.Data =
           data.Select(s => new
           {
               s.Id,
               s.CustomerId,
               s.CustomerName,
               s.Services,
               CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
               s.Contacter,
               s.ContactTel,
               s.Supervisor,
               s.SalesMan,
               s.Status,
               s.Province,
               s.City,
               s.Area,
               s.Addr,
               s.U_SAP_ID,
               s.Latitude,
               s.Longitude,
               WorkOrderCount = s.ServiceWorkOrders?.Count(),
               ServiceWorkOrders = s.MaterialInfo.GroupBy(o => o.MaterialType).ToList().Select(a => new
               {
                   MaterialType = a?.Key,
                   UnitName = "台",
                   Count = a?.Count(),
                   Status = s.ServiceWorkOrders?.FirstOrDefault(b => "其他设备".Equals(a.Key) ? b.MaterialCode == "其他设备" : b.MaterialCode.Contains(a.Key))?.Status,
                   MaterialTypeName = "其他设备".Equals(a.Key) ? "其他设备" : MaterialTypeModel?.Where(m => m.TypeAlias == a.Key)?.FirstOrDefault().TypeName,
                   TechnicianId = s.ServiceWorkOrders?.FirstOrDefault(b => "其他设备".Equals(a.Key) ? b.MaterialCode == "其他设备" : b.MaterialCode.Contains(a.Key))?.CurrentUserId,
               }),
               ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.MaterialInfo?.FirstOrDefault().ProblemType.Name : s.ProblemTypeName,
               ProblemTypeId = string.IsNullOrEmpty(s.ProblemTypeId) ? s.MaterialInfo?.FirstOrDefault()?.ProblemTypeId : s.ProblemTypeId
           });
            result.Count = query.Count();
            return result;
        }

        /// <summary>
        /// 获取设备类型列表（管理员）
        /// </summary>
        /// <param name="SapOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetAppAdminServiceOrderDetails(int SapOrderId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == SapOrderId)
                        .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                        .Select(a => new
                        {
                            ServiceOrderId = a.Id,
                            a.CreateTime,
                            a.Province,
                            a.City,
                            a.Area,
                            a.Addr,
                            NewestContacter = string.IsNullOrEmpty(a.NewestContacter) ? a.Contacter : a.NewestContacter,
                            NewestContactTel = string.IsNullOrEmpty(a.NewestContactTel) ? a.ContactTel : a.NewestContactTel,
                            AppCustId = a.AppUserId,
                            a.ProblemTypeId,
                            a.ProblemTypeName,
                            a.Services,
                            a.CustomerName,
                            a.Supervisor,
                            a.SalesMan,
                            a.Longitude,
                            a.Latitude,
                            ServiceWorkOrders = a.ServiceWorkOrders.Select(o => new
                            {
                                o.Id,
                                o.Status,
                                o.ManufacturerSerialNumber,
                                o.MaterialCode,
                                o.CurrentUserId,
                                MaterialType = o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                                o.ProblemType,
                                o.Priority
                            }).ToList()
                        });


            var count = await query.CountAsync();
            var list = (await query
                .ToListAsync())
                .Select(a => new
                {
                    a.ServiceOrderId,
                    CreateTime = a.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                    a.Province,
                    a.City,
                    a.Area,
                    a.Addr,
                    a.NewestContacter,
                    a.NewestContactTel,
                    a.AppCustId,
                    ProblemTypeName = string.IsNullOrEmpty(a.ProblemTypeName) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType.Name : a.ProblemTypeName,
                    ProblemTypeId = string.IsNullOrEmpty(a.ProblemTypeId) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType.Id : a.ProblemTypeId,
                    a.Services,
                    a.CustomerName,
                    a.Supervisor,
                    a.SalesMan,
                    a.Longitude,
                    a.Latitude,
                    Priority = a.ServiceWorkOrders.FirstOrDefault()?.Priority == 3 ? "高" : a.ServiceWorkOrders.FirstOrDefault()?.Priority == 2 ? "中" : "低",
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                    {
                        MaterialType = string.IsNullOrEmpty(s.Key) ? "其他设备" : s.Key,
                        TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                        Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                        Count = s.Count(),
                        UnitName = "台",
                        MaterialTypeName = string.IsNullOrEmpty(s.Key) ? "其他设备" : MaterialTypeModel.Where(a => a.TypeAlias == s.Key).FirstOrDefault().TypeName,
                        WorkOrders = s.Select(i => i.Id).ToList(),
                        Orders = s.Select(s => new { s.Id, s.MaterialCode, s.ManufacturerSerialNumber }).ToList()
                    }
                    ).ToList()
                });
            result.Count = count;
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 主管给技术员派单
        /// </summary>
        /// <returns></returns>
        public async Task SendOrders(SendOrdersReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var canSendOrder = await CheckCanTakeOrder(req.CurrentUserId);
            if (!canSendOrder)
            {
                throw new CommonException("技术员接单已经达到上限", 60001);
            }
            var u = await UnitWork.Find<AppUserMap>(s => s.AppUserId == req.CurrentUserId).Include(s => s.User).FirstOrDefaultAsync();
            var ServiceOrderModel = await UnitWork.Find<ServiceOrder>(s => s.Id == Convert.ToInt32(req.ServiceOrderId)).FirstOrDefaultAsync();

            var Model = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.ToString() == req.ServiceOrderId && req.QryMaterialTypes.Contains(s.MaterialCode == "其他设备" ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")))).Select(s => s.Id);
            var ids = await Model.ToListAsync();
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => ids.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUser = u.User.Name,
                CurrentUserNsapId = u.User.Id,
                CurrentUserId = req.CurrentUserId,
                Status = 2
            });

            await UnitWork.AddAsync<ServiceOrderParticipationRecord>(new ServiceOrderParticipationRecord
            {
                ServiceOrderId = ServiceOrderModel.Id,
                UserId = u.User.Id,
                SapId = ServiceOrderModel.U_SAP_ID,
                ReimburseType = 0,
                CreateTime = DateTime.Now

            });
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单",
                Details = "已为您分配专属技术员进行处理，感谢您的耐心等待",
                LogType = 1,
                ServiceOrderId = Convert.ToInt32(req.ServiceOrderId),
                ServiceWorkOrder = string.Join(",", ids.ToArray()),
                MaterialType = string.Join(",", req.QryMaterialTypes.ToArray())
            });

            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单成功",
                Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                LogType = 2,
                ServiceOrderId = Convert.ToInt32(req.ServiceOrderId),
                ServiceWorkOrder = string.Join(",", ids.ToArray()),
                MaterialType = string.Join(",", req.QryMaterialTypes.ToArray())
            });
            var WorkOrderNumbers = String.Join(',', await UnitWork.Find<ServiceWorkOrder>(s => ids.Contains(s.Id)).Select(s => s.WorkOrderNumber).ToArrayAsync());

            await _ServiceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"主管{loginContext.User.Name}给技术员{u.User.Name}派单{WorkOrderNumbers}", ActionType = "主管派单工单", MaterialType = string.Join(",", req.QryMaterialTypes.ToArray()) }, ids);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = Convert.ToInt32(req.ServiceOrderId), Content = $"主管{loginContext.User.Name}给技术员{u.User.Name}派单{WorkOrderNumbers}", AppUserId = 0 });
            await PushMessageToApp(req.CurrentUserId, "派单成功提醒", "您已被派有一个新的售后服务，请尽快处理");
        }

        /// <summary>
        /// 主管给技术员派单（转派）
        /// </summary>
        /// <returns></returns>
        public async Task TransferOrders(TransferOrdersReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var canSendOrder = await CheckCanTakeOrder(req.TechnicianId);
            if (!canSendOrder)
            {
                throw new CommonException("技术员接单已经达到上限", 60001);
            }
            var u = await UnitWork.Find<AppUserMap>(s => s.AppUserId == req.TechnicianId).Include(s => s.User).FirstOrDefaultAsync();
            var ServiceOrderModel = await UnitWork.Find<ServiceOrder>(s => s.Id == Convert.ToInt32(req.ServiceOrderId)).FirstOrDefaultAsync();

            var Model = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.ToString() == req.ServiceOrderId && req.MaterialType.Equals(s.MaterialCode == "其他设备" ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")))).Select(s => s.Id);
            var ids = await Model.ToListAsync();
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => ids.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUser = u.User.Name,
                CurrentUserNsapId = u.User.Id,
                CurrentUserId = req.TechnicianId
            });

            await UnitWork.AddAsync<ServiceOrderParticipationRecord>(new ServiceOrderParticipationRecord
            {
                ServiceOrderId = ServiceOrderModel.Id,
                UserId = u.User.Id,
                SapId = ServiceOrderModel.U_SAP_ID,
                ReimburseType = 0,
                CreateTime = DateTime.Now

            });
            await UnitWork.SaveAsync();
            var WorkOrderNumbers = String.Join(',', await UnitWork.Find<ServiceWorkOrder>(s => ids.Contains(s.Id)).Select(s => s.WorkOrderNumber).ToArrayAsync());

            await _ServiceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"主管{loginContext.User.Name}给技术员{u.User.Name}派单（转派）{WorkOrderNumbers}", ActionType = "主管派单工单", MaterialType = req.MaterialType }, ids);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = Convert.ToInt32(req.ServiceOrderId), Content = $"主管{loginContext.User.Name}给技术员{u.User.Name}派单（转派）{WorkOrderNumbers}", AppUserId = 0 });
            await PushMessageToApp(req.TechnicianId, "派单成功提醒", "您已被派有一个新的售后服务，请尽快处理");
        }

        /// <summary>
        /// 获取当前技术员剩余可接单数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> GetUserCanOrderCount(int id)
        {
            int totalNum = int.Parse(GetSendOrderCount().Result?.DtValue);
            var count = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == id && s.Status.Value < 7).Select(s => s.ServiceOrderId).Distinct().CountAsync();
            return totalNum - count;
        }

        /// <summary>
        /// 管理员关单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task CloseWorkOrder(CloseWorkOrderReq request)
        {
            var workOrderInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId).FirstOrDefaultAsync();
            string content = "关单通知<br>工单号：" + workOrderInfo.Id + "<br>序列号：" + (string.IsNullOrEmpty(workOrderInfo.ManufacturerSerialNumber) ? "无" : workOrderInfo.ManufacturerSerialNumber) + "<br>物料编码：" +
               (string.IsNullOrEmpty(workOrderInfo.MaterialCode) ? "无" : workOrderInfo.MaterialCode) + "<br>关单原因：" + request.Reason;
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId, u => new ServiceWorkOrder
            {
                Status = 7,
                ProcessDescription = workOrderInfo.ProcessDescription + content
            });
            await UnitWork.SaveAsync();
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = workOrderInfo.ServiceOrderId, Content = content, AppUserId = request.CurrentUserId });
        }

        /// <summary>
        /// 获取可接单技术员列表（App）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetAppAllowSendOrderUser(GetAllowSendOrderUserReq req)
        {
            Dictionary<string, object> userData = new Dictionary<string, object>();
            List<object> data = new List<object>();
            var result = new TableData();
            //1.根据app用户Id查询出nSap中的用户Id
            var userId = (await UnitWork.FindSingleAsync<AppUserMap>(a => a.AppUserId == req.CurrentUserId)).UserID;
            //2.取出nSAP中该用户对应的部门信息
            var orgs = _revelanceApp.Get(Define.USERORG, true, userId).ToArray();
            //3.取出nSAP用户与APP用户关联的用户信息（角色为技术员）
            var tUsers = await UnitWork.Find<AppUserMap>(u => u.AppUserRole > 1 && req.TechnicianId > 0 ? u.AppUserId != req.TechnicianId : true).ToListAsync();
            //4.获取定位信息（登录APP时保存的位置信息）
            var locations = (await UnitWork.Find<RealTimeLocation>(null).OrderByDescending(o => o.CreateTime).ToListAsync()).GroupBy(g => g.AppUserId).Select(s => s.First());
            //5.根据组织信息获取组织下的所有用户Id集合
            var userIds = _revelanceApp.Get(Define.USERORG, false, orgs);
            //6.取得相关用户信息
            var query = from a in UnitWork.Find<User>(u => userIds.Contains(u.Id) && u.Status == 0)
                        join b in UnitWork.Find<Relevance>(null) on a.Id equals b.FirstId into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<Repository.Domain.Org>(null) on b.SecondId equals c.Id into bc
                        from c in bc.DefaultIfEmpty()
                        select new { userId = a.Id, orgId = c.Id, orgName = c.Name, orgParentName = c.ParentName };
            var userInfo = (await query.ToListAsync()).Where(s => !string.IsNullOrEmpty(s.orgId) && orgs.Contains(s.orgId)).GroupBy(g => g.orgId).Select(s => new { s.Key, userids = string.Join(",", s.Select(i => i.userId)), orgName = s.Select(i => i.orgName).Distinct().FirstOrDefault() }).ToList();
            //7.循环遍历部门--用户获取用户信息 返回最后需要的信息结构
            foreach (var item in userInfo)
            {
                userData = new Dictionary<string, object>();
                var orgName = item.orgName;
                string uIds = item.userids;
                string orgId = item.Key;
                List<string> userlist = new List<string>();
                string[] userArr = uIds.Split(",");
                userArr.ForEach(u =>
                     userlist.Add(u)
                );
                var ids = userlist.Intersect(tUsers.Select(u => u.UserID));
                var users = await UnitWork.Find<User>(u => ids.Contains(u.Id)).WhereIf(!string.IsNullOrEmpty(req.key), u => u.Name.Contains(req.key)).ToListAsync();
                var us = users.Select(u => new { u.Name, AppUserId = tUsers.FirstOrDefault(a => a.UserID.Equals(u.Id)).AppUserId, u.Id });
                var appUserIds = tUsers.Where(u => userIds.Contains(u.UserID)).Select(u => u.AppUserId).ToList();

                var userCount = await UnitWork.Find<ServiceWorkOrder>(s => appUserIds.Contains(s.CurrentUserId) && s.Status.Value < 7)
                    .Select(s => new { s.CurrentUserId, s.ServiceOrderId }).Distinct().GroupBy(s => s.CurrentUserId)
                    .Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

                var userInfos = us.Select(u => new AllowSendOrderUserResp
                {
                    Id = u.Id,
                    Name = u.Name,
                    Count = userCount.FirstOrDefault(s => s.Key.Equals(u.AppUserId))?.Count ?? 0,
                    AppUserId = u.AppUserId,
                    Province = locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Province,
                    City = locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.City,
                    Area = locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Area,
                    Addr = locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Addr,
                    Distance = (req.Latitude == 0 || locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Latitude is null) ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Latitude ?? 0), Convert.ToDouble(locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude))
                }).ToList();
                userInfos = userInfos.OrderBy(o => o.Distance).ToList();
                userData.Add("orgId", orgId);
                userData.Add("orgName", orgName);
                userData.Add("userData", userInfos);
                data.Add(userData);
            }
            result.Data = data;
            result.Count = userInfo.Count;
            return result;
        }
        #endregion
    }
}