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

namespace OpenAuth.App
{
    public class ServiceOrderApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly ServiceOrderLogApp _serviceOrderLogApp;
        private readonly BusinessPartnerApp _businessPartnerApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private IOptions<AppSetting> _appConfiguration;
        private ICapPublisher _capBus;
        private HttpHelper _helper;
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        public ServiceOrderApp(IUnitWork unitWork,
            RevelanceManagerApp app, ServiceOrderLogApp serviceOrderLogApp, BusinessPartnerApp businessPartnerApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, IOptions<AppSetting> appConfiguration, ICapPublisher capBus, ServiceOrderLogApp ServiceOrderLogApp) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _revelanceApp = app;
            _serviceOrderLogApp = serviceOrderLogApp;
            _businessPartnerApp = businessPartnerApp;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
            _capBus = capBus;
            _ServiceOrderLogApp = ServiceOrderLogApp;
        }
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
        /// app查询服务单列表
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
            var query = UnitWork.Find<ServiceOrder>(s => s.AppUserId == request.AppUserId)
                        .Include(s => s.ServiceWorkOrders)
                        .WhereIf(request.Type == 1, q => q.ServiceWorkOrders.All(a => a.Status > 6) && q.ServiceWorkOrders.Count > 0)
                        .WhereIf(request.Type == 0, q => q.ServiceWorkOrders.Any(a => a.Status < 7) || q.Status == 1)
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
                            a.NewestContacter,
                            a.NewestContactTel,
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
                            }).ToList()
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
                        TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                        Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                        Count = s.Count(),
                        Orders = s.ToList()
                    }
                    ).ToList(),
                    WorkOrderState = a.ServiceWorkOrders.Distinct().OrderBy(o => o.Status).FirstOrDefault()?.Status,
                });

            var result = new TableData();
            result.Count = count;
            result.Data = list;
            return result;
        }

        /// <summary>
        /// app查询服务单详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Response<dynamic>> AppLoadServiceOrderDetails(AppQueryServiceOrderReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var query = UnitWork.Find<ServiceOrder>(s => s.AppUserId.Equals(request.AppUserId) && s.Id.Equals(request.ServiceOrderId))
                        .Select(a => new
                        {
                            a.Id,
                            a.AppUserId,
                            a.Services,
                            a.CreateTime,
                            a.Status,
                            a.U_SAP_ID,
                            ServiceWorkOrders = a.ServiceWorkOrders.Select(o => new
                            {
                                o.Id,
                                o.Status,
                                o.FromTheme,
                                ProblemType = o.ProblemType.Description,
                                o.ManufacturerSerialNumber,
                                o.MaterialCode,
                                o.CurrentUserId,
                                MaterialType = o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-"))
                            }).ToList()
                        });

            var data = (await query.ToListAsync()).Select(a => new
            {
                a.Id,
                a.AppUserId,
                a.Services,
                CreateTime = a.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                a.Status,
                a.U_SAP_ID,
                ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                {
                    MaterialType = s.Key,
                    Count = s.Count(),
                    Orders = s.ToList()
                })
            }).ToList();


            var result = new Response<dynamic>();
            result.Result = data;
            return result;
        }

        /// <summary>
        /// APP提交服务单
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
            obj.CreateTime = DateTime.Now;
            obj.CreateUserId = loginContext.User.Id;
            obj.RecepUserId = loginContext.User.Id;
            obj.RecepUserName = loginContext.User.Name;
            obj.Status = 1;


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
            return o;
        }

        /// <summary>
        /// 获取服务单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ServiceOrder> GetDetails(int id)
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
            var serviceOrderPictureIds = obj.ServiceOrderPictures.Select(s => s.PictureId).ToList();
            var files = await UnitWork.Find<UploadFile>(f => serviceOrderPictureIds.Contains(f.Id)).ToListAsync();
            result.Files = files.MapTo<List<UploadFileResp>>();
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
            return obj;
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

            var statusStr = status == 2 ? "已排配" : status == 3 ? "已外出" : status == 4 ? "已挂起" : status == 5 ? "已接收" : status == 6 ? "已解决" : status == 7 ? "已回访" : "待处理";
            await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"修改服务工单单状态为:{statusStr}", ActionType = "修改服务工单状态", ServiceWorkOrderId = id });
        }
        /// <summary>
        /// 查询超24小时未处理的订单
        /// </summary>
        /// <returns></returns>
        public async Task<List<ServiceOrder>> FindTimeoutOrder()
        {
            var query = UnitWork.Find<ServiceOrder>(null);
            query = query.Where(s => s.Status == 1 && (DateTime.Now - s.CreateTime).Value.Hours >= 24);
            return await query.ToListAsync();
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
                s.Contacter,
                s.ContactTel,
                s.NewestContacter,
                s.NewestContactTel,
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
                TerminalCustomer = obj.TerminalCustomer,
                SalesMan = obj.SalesMan,
                SalesManId = obj.SalesManId,
                Supervisor = obj.Supervisor,
                SupervisorId = obj.SupervisorId,
            });
            obj.ServiceWorkOrders.ForEach(s =>
            {
                s.ServiceOrderId = obj.Id; s.SubmitDate = DateTime.Now; s.SubmitUserId = loginContext.User.Id; s.AppUserId = obj.AppUserId; s.Status = 1;
                s.SubmitDate = DateTime.Now;
                s.SubmitUserId = loginContext.User.Id;
                if (s.FromType == 2)
                    s.Status = 7;
            });
            await UnitWork.BatchAddAsync<ServiceWorkOrder, int>(obj.ServiceWorkOrders.ToArray());
            var pictures = request.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = request.Id; p.PictureType = 2; });
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
            await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"客服:{loginContext.User.Name}创建工单", ActionType = "创建工单", ServiceOrderId = obj.Id });

            #region 同步到SAP 并拿到服务单主键
            _capBus.Publish("Serve.ServcieOrder.CreateWorkNumber", obj.Id);

            #endregion
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
            //用信号量代替锁
            await semaphoreSlim.WaitAsync();
            try
            {
                var obj = request.MapTo<ServiceWorkOrder>();
                var ServiceWorkOrders = UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId.Equals(request.ServiceOrderId)).OrderByDescending(u => u.Id).ToList();
                var WorkOrderNumber = ServiceWorkOrders.First().WorkOrderNumber;
                int num = Convert.ToInt32(WorkOrderNumber.Substring(WorkOrderNumber.IndexOf("-") + 1));
                obj.WorkOrderNumber = WorkOrderNumber.Substring(0, WorkOrderNumber.IndexOf("-") + 1) + (num + 1);
                await UnitWork.AddAsync<ServiceWorkOrder, int>(obj);
                await UnitWork.SaveAsync();
                //baseInfo.CertificateNumber = await CertificateNoGenerate("O");
                //await _certinfoApp.AddAsync(new AddOrUpdateCertinfoReq() { CertNo = baseInfo.CertificateNumber });
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
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.b.CustomerId.Contains(req.QryCustomer) || q.b.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.b.RecepUserName.Contains(req.QryRecepUser))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.a.ProblemTypeId.Equals(req.QryProblemType))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.a.CreateTime >= req.QryCreateTimeFrom && q.a.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                         .Where(q => q.b.U_SAP_ID != null);

            if (loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                query = query.Where(q => q.b.SupervisorId.Equals(loginContext.User.Id));
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
                               let WorkMTypes=g.Select(o => o.MaterialType == "其他设备" ? "其他设备": o.MaterialType)
                               select new { ServiceOrderId = g.Key, U_SAP_ID, MaterialTypes = WorkMTypes, WorkMaterialTypes= MTypes };
            var grouplist = grouplistsql.ToList();

            result.Count = grouplistsql.Count();

            grouplist = grouplist.OrderBy(s => s.U_SAP_ID).Skip((req.page - 1) * req.limit)
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
            var d = await _businessPartnerApp.GetDetails(req.CustomerId);
            var obj = req.MapTo<ServiceOrder>();
            obj.RecepUserName = loginContext.User.Name;
            obj.RecepUserId = loginContext.User.Id;
            obj.Status = 2;
            obj.SalesMan = d.SlpName;
            obj.SalesManId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.SlpName)))?.Id;
            obj.Supervisor = d.TechName;
            obj.SupervisorId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.TechName)))?.Id;
            obj.ServiceWorkOrders.ForEach(s =>
            {
                if (s.FromType == 2)
                    s.Status = 7;
            });
            var e = await UnitWork.AddAsync<ServiceOrder, int>(obj);
            await UnitWork.SaveAsync();
            var pictures = req.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = e.Id; p.PictureType = 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();

            await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"客服:{loginContext.User.Name}创建服务单", ActionType = "创建工单", ServiceOrderId = e.Id });
            #region 同步到SAP 并拿到服务单主键

            _capBus.Publish("Serve.ServcieOrder.Create", obj.Id);
            #endregion
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

            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceWorkOrders)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.ServiceWorkOrders.Any(a => a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.ServiceWorkOrders.Any(a => a.Status.Equals(Convert.ToInt32(req.QryState))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ServiceWorkOrders.Any(a => a.ManufacturerSerialNumber.Contains(req.QryManufSN)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.RecepUserName.Contains(req.QryRecepUser))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ServiceWorkOrders.Any(a => a.ProblemTypeId.Equals(req.QryProblemType)))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q=>q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.ContactTel.Contains(req.ContactTel) || q.NewestContactTel.Contains(req.ContactTel))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromType), q => q.ServiceWorkOrders.Any(a => a.FromType.Equals(Convert.ToInt32(req.QryFromType))))
                .Where(q => q.Status == 2);
            ;
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
                && (string.IsNullOrWhiteSpace(req.QryFromType) || a.FromType.Equals(Convert.ToInt32(req.QryFromType)))).ToList()
            });

            result.Data = await resultsql.Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync(); ;
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
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.b.CustomerId.Contains(req.QryCustomer) || q.b.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.b.RecepUserName.Contains(req.QryRecepUser))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.c.Name.Contains(req.QryProblemType))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.a.CreateTime >= req.QryCreateTimeFrom && q.a.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                         .WhereIf(req.QryMaterialTypes != null && req.QryMaterialTypes.Count > 0, q => req.QryMaterialTypes.Contains(q.a.MaterialCode== "其他设备" ? "其他设备": q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-"))))
                         .Where(q => q.a.FromType != 2);

            if (loginContext.User.Account != Define.SYSTEM_USERNAME)
            {
                query = query.Where(q => q.b.SupervisorId.Equals(loginContext.User.Id));
            }

            var resultsql = query.OrderBy(r => r.a.Id).ThenBy(r => r.a.WorkOrderNumber).Select(q => new
            {
                ServiceOrderId = q.b.Id,
                q.a.Priority,
                q.a.FromType,
                q.a.Status,
                q.b.CustomerId,
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
        /// 技术员查看服务单列表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceOrder(TechnicianServiceWorkOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == req.TechnicianId)
                .WhereIf(req.Type == 1, s => s.Status.Value < 7 && s.Status.Value > 1)
                .WhereIf(req.Type == 2, s => s.Status.Value >= 7)
                .Select(s => s.ServiceOrderId).Distinct().ToListAsync();

            var query = UnitWork.Find<ServiceOrder>(s => serviceOrderIds.Contains(s.Id))
                .Select(s => new
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
                    s.Contacter,
                    s.ContactTel,
                    s.NewestContacter,
                    s.NewestContactTel,
                    s.Status,
                    s.CreateTime,
                    s.U_SAP_ID,
                    Count = s.ServiceWorkOrders.Where(w => w.ServiceOrderId == s.Id && w.CurrentUserId == req.TechnicianId).Count(),
                    s.ProblemTypeName
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
                s.Contacter,
                s.ContactTel,
                s.NewestContacter,
                s.NewestContactTel,
                s.Status,
                CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                s.U_SAP_ID,
                Distance = (req.Latitude == 0 || s.Latitude is null) ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(s.Latitude ?? 0), Convert.ToDouble(s.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)),
                s.Count,
                s.ProblemTypeName
            }).ToList();

            var count = await query.CountAsync();
            result.Data = list;
            result.Count = count;
            return result;
        }


        /// <summary>
        /// 技术员工单池列表
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

            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单",
                Details = "已为您分配专属技术员进行处理，感谢您的耐心等待",
                LogType = 1,
                ServiceOrderId = serviceOrderId
            }, req.ServiceWorkOrderIds);

            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单成功",
                Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                LogType = 2,
                ServiceOrderId = serviceOrderId
            }, req.ServiceWorkOrderIds);
            await _serviceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员:{u.User.Name}接单工单：{string.Join(",", req.ServiceWorkOrderIds)}", ActionType = "技术员接单" }, req.ServiceWorkOrderIds);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = req.ServiceOrderId, Content = "技术员已接单成功，请尽快选择服务", AppUserId = 0 });
            await PushMessageToApp(req.TechnicianId, "接单成功提醒", "您已成功接取一个新的售后服务，请尽快处理");
        }

        /// <summary>
        /// 推送消息至新威智能app
        /// </summary>
        /// <param name="userId">app用户Id</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        private async Task PushMessageToApp(int userId, string title, string content)
        {
            _helper.Post(new
            {
                UserId = userId,
                Title = title,
                Content = content
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "BbsCommunity/AppPushMsg");
        }

        /// <summary>
        /// 获取服务单图片Id列表
        /// </summary>
        /// <param name="id">服务单Id</param>
        /// <param name="type">1-客户上传 2-客服上传</param>
        /// <returns></returns>
        public async Task<List<UploadFileResp>> GetServiceOrderPictures(int id, int type)
        {
            var idList = await UnitWork.Find<ServiceOrderPicture>(p => p.ServiceOrderId.Equals(id) && p.PictureType.Equals(type)).Select(p => p.PictureId).ToListAsync();
            var files = await UnitWork.Find<UploadFile>(f => idList.Contains(f.Id)).ToListAsync();
            var list = files.MapTo<List<UploadFileResp>>();
            return list;
        }

        /// <summary>
        /// 技术员预约工单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task BookingWorkOrder(BookingWorkOrderReq req)
        {
            var orderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && "其他设备".Equals(req.MaterialType) ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == req.MaterialType).Select(o => o.Id).ToListAsync();
            List<int> workOrderIds = new List<int>();
            foreach (var id in orderIds)
            {
                workOrderIds.Add(id);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && "其他设备".Equals(req.MaterialType) ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == req.MaterialType, o => new ServiceWorkOrder
            {
                BookingDate = req.BookingDate,
                OrderTakeType = 4,
                Status = 3
            });
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员预约服务时间",
                Details = $"为您分配的技术员预约了服务时间，请注意接听来电",
                LogType = 1,
                ServiceOrderId = req.ServiceOrderId
            });

            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员预约时间成功",
                Details = $"已预约时间，请尽快完成电访，并安排你的服务行程。避免可能存在的行程风险问题",
                LogType = 2,
                ServiceOrderId = req.ServiceOrderId
            });
            await _serviceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{req.CurrentUserId}预约工单{string.Join(",", workOrderIds)}", ActionType = "预约工单" }, workOrderIds);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = req.ServiceOrderId, Content = "技术员已预约上门时间成功，请尽早安排行程", AppUserId = 0 });

        }

        /// <summary>
        /// 技术员核对设备(正确/错误)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task CheckTheEquipment(CheckTheEquipmentReq req)
        {
            //判断当前操作者是否有操作权限
            var order = await UnitWork.FindSingleAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId);
            if (order == null)
            {
                throw new CommonException("当前技术员无法核对此工单设备。", 9001);
            }
            List<int> obj = new List<int>();
            //处理核对成功的设备信息
            if (!string.IsNullOrEmpty(req.CheckWorkOrderIds))
            {
                string[] checkArr = req.CheckWorkOrderIds.Split(',');
                if (checkArr.Length > 0)
                {
                    foreach (var itemcheck in checkArr)
                    {
                        obj.Add(int.Parse(itemcheck));
                        await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id == int.Parse(itemcheck), o => new ServiceWorkOrder
                        {
                            IsCheck = 1,
                            Status = 4
                        });
                        await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{req.CurrentUserId}核对工单{itemcheck}设备（成功）", ActionType = "核对设备", ServiceWorkOrderId = int.Parse(itemcheck) });
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
                        await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{req.CurrentUserId}核对工单{itemerr}设备(错误)", ActionType = "核对设备", ServiceWorkOrderId = int.Parse(itemerr) });
                    }
                }
            }
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员已外出",
                Details = $"技术员当前正在为您上门服务",
                LogType = 1,
                ServiceOrderId = req.ServiceOrderId
            }, obj);
            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员完成设备信息核对",
                Details = $"已核对设备，请尽快开始为客户提供高效优质的服务",
                LogType = 2,
                ServiceOrderId = req.ServiceOrderId
            }, obj);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = req.ServiceOrderId, Content = "技术员已核对设备，请完成维修任务", AppUserId = 0 });
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

            var tUsers = await UnitWork.Find<AppUserMap>(u => u.AppUserRole == 2).ToListAsync();
            var locations = (await UnitWork.Find<RealTimeLocation>(null).OrderByDescending(o => o.CreateTime).ToListAsync()).GroupBy(g => g.AppUserId).Select(s => s.First());
            var userIds = _revelanceApp.Get(Define.USERORG, false, orgs);
            var ids = userIds.Intersect(tUsers.Select(u => u.UserID));
            var users = await UnitWork.Find<User>(u => ids.Contains(u.Id)).WhereIf(!string.IsNullOrEmpty(req.key), u => u.Name.Equals(req.key)).ToListAsync();
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
                Distance = (req.Latitude == 0 || locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Latitude is null) ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Latitude ?? 0), Convert.ToDouble(locations.FirstOrDefault(f => f.AppUserId == u.AppUserId)?.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude))
            }).ToList();

            userInfos = userInfos.OrderBy(o => o.Distance).ToList();
            var list = userInfos.OrderBy(o => o.Distance)
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToList();
            result.Data = list;
            result.Count = userInfos.Count;
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
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => req.WorkOrderIds.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUser = u.User.Name,
                CurrentUserNsapId = u.User.Id,
                CurrentUserId = req.CurrentUserId,
                Status = 2
            });
            await UnitWork.SaveAsync();
            var serviceOrderId = (await UnitWork.Find<ServiceWorkOrder>(s => req.WorkOrderIds.Contains(s.Id)).FirstOrDefaultAsync()).ServiceOrderId;
            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单",
                Details = "已为您分配专属技术员进行处理，感谢您的耐心等待",
                LogType = 1,
                ServiceOrderId = serviceOrderId
            }, req.WorkOrderIds);

            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员接单成功",
                Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                LogType = 2,
                ServiceOrderId = serviceOrderId
            }, req.WorkOrderIds);
            await _serviceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"主管{loginContext.User.Name}给技术员{u.User.Name}派单{string.Join(",", req.WorkOrderIds)}", ActionType = "主管派单工单" }, req.WorkOrderIds);
            await PushMessageToApp(req.CurrentUserId, "派单成功提醒", "您已被派有一个新的售后服务，请尽快处理");
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
        /// 判断用户是否到达接单上限
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<bool> CheckCanTakeOrder(int id)
        {
            var count = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == id && s.Status.Value < 7).Select(s => s.ServiceOrderId).Distinct().CountAsync();

            return count < ServiceWorkOrder.canOrderQty;
        }


        /// <summary>
        /// 获取工单服务详情
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
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.CurrentUserId == request.CurrentUserId && "其他设备".Equals(request.MaterialType) ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == request.MaterialType, e => new ServiceWorkOrder
            {
                ProcessDescription = request.Description
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 选择接单类型
        /// </summary>
        /// <returns></returns>
        public async Task SaveOrderTakeType(SaveWorkOrderTakeTypeReq request)
        {
            int status = 2;
            if (request.Type == 3)
            {
                status = 3;
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.CurrentUserId == request.CurrentUserId && (string.IsNullOrEmpty(s.MaterialCode) ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-"))) == request.MaterialType, e => new ServiceWorkOrder
            {
                OrderTakeType = request.Type,
                Status = status
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取描述
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetWorkOrderDescription(QueryWorkOrderDescriptionReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var query = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId)
                        .Select(a => new
                        {
                            a.TroubleDescription,
                            a.ProcessDescription
                        }).FirstOrDefaultAsync();
            result.Data = query;
            return result;
        }

        /// <summary>
        /// 获取当前技术员剩余可接单数
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<int> GetUserCanOrderCount(int id)
        {
            var count = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == id && s.Status.Value < 7).Select(s => s.ServiceOrderId).Distinct().CountAsync();
            return ServiceWorkOrder.canOrderQty - count;
        }


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
        private async Task SendMessageToRelatedUsers(string Content, int ServiceOrderId, int FromUserId, string MessageId)
        {
            //发给服务单客服/主管
            var serviceInfo = await UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId).FirstOrDefaultAsync();
            //客服Id
            string RecepUserId = serviceInfo.RecepUserId;
            if (!string.IsNullOrEmpty(RecepUserId))
            {
                var recepUserInfo = await UnitWork.Find<AppUserMap>(a => a.UserID == RecepUserId).FirstOrDefaultAsync();
                if (recepUserInfo != null && recepUserInfo.AppUserId > 0)
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
                if (superUserInfo != null && superUserInfo.AppUserId > 0)
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
        private async Task ReadMsg(int currentUserId, int serviceOrderId)
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
            var serviceIdList = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == req.CurrentUserId && s.Status < 7).ToListAsync();
            if (serviceIdList != null)
            {
                string serviceIds = string.Join(",", serviceIdList.Select(s => s.ServiceOrderId).Distinct().ToArray());
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
            var query = from a in UnitWork.Find<ServiceOrderMessage>(null)
                        join b in UnitWork.Find<ServiceOrderMessageUser>(null) on a.Id equals b.MessageId
                        select new { a, b };
            query = query.Where(q => q.b.FroUserId == currentUserId.ToString() && q.b.HasRead == false)
                .Where(q => UnitWork.Find<ServiceWorkOrder>(null).Any(a => a.ServiceOrderId == q.a.ServiceOrderId && a.Status < 7 && a.CurrentUserId == currentUserId));
            var msgCount = (await query.ToListAsync()).Count;
            result.Data = msgCount;
            return result;
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
            var tUsers = await UnitWork.Find<AppUserMap>(u => u.AppUserRole == 2).ToListAsync();
            //4.获取定位信息（登录APP时保存的位置信息）
            var locations = (await UnitWork.Find<RealTimeLocation>(null).OrderByDescending(o => o.CreateTime).ToListAsync()).GroupBy(g => g.AppUserId).Select(s => s.First());
            //5.根据组织信息获取组织下的所有用户Id集合
            var userIds = _revelanceApp.Get(Define.USERORG, false, orgs);
            //6.取得相关用户信息
            var query = from a in UnitWork.Find<User>(u => userIds.Contains(u.Id))
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

        /// <summary>
        /// 获取服务单详情（管理员）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>

        public async Task<TableData> GetAdminServiceOrderDetail(QueryServiceOrderDetailReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var query = UnitWork.Find<ServiceOrder>(s => s.Id == req.ServiceOrderId)
                .Include(s => s.ServiceOrderSNs)
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
                    MaterialInfo = s.ServiceWorkOrders.Where(o => o.ServiceOrderId == req.ServiceOrderId && o.Status == 1).Select(o => new
                    {
                        o.MaterialCode,
                        o.ManufacturerSerialNumber,
                        MaterialType = "其他设备".Equals(o.MaterialCode) ? "其他设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                        o.Status,
                        o.Id,
                        o.IsCheck,
                        o.OrderTakeType
                    }),
                    s.ServiceOrderSNs
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
                OrderTakeType = s.MaterialInfo.Distinct().OrderBy(o => o.OrderTakeType).FirstOrDefault()?.OrderTakeType,
                s.ServiceOrderSNs
            }).ToList();
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 获取待确认/已确认服务单列表（App）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AppUnConfirmedServiceOrderList(QueryAppServiceOrderListReq req)
        {
            var result = new TableData();
            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceOrderSNs)
                .Include(s => s.ServiceWorkOrders)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState) && Convert.ToInt32(req.QryState) > 0, q => q.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(Convert.ToInt32(req.QryState) == 2, q => !q.ServiceWorkOrders.All(q => q.Status != 1))
                         .WhereIf(Convert.ToInt32(req.QryState) == 0, q => q.Status == 1 || (q.Status == 2 && !q.ServiceWorkOrders.All(q => q.Status != 1)))
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
                q.Supervisor,
                q.SalesMan,
                q.Status,
                q.ServiceOrderSNs.FirstOrDefault().ManufSN,
                q.ServiceOrderSNs.FirstOrDefault().ItemCode,
                q.Province,
                q.City,
                q.Area,
                q.Addr,
                q.U_SAP_ID
            });

            result.Data =
            (await query//.OrderBy(u => u.Id)
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync()).Select(s => new
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
                s.ManufSN,
                s.ItemCode,
                s.Province,
                s.City,
                s.Area,
                s.Addr,
                s.U_SAP_ID
            });
            result.Count = query.Count();
            return result;
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
                   .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.ServiceWorkOrders.Any(a => a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440)))
                   .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.ContactTel.Contains(req.ContactTel) || q.NewestContactTel.Contains(req.ContactTel))
                   .WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), q => q.ServiceWorkOrders.Any(a => a.CurrentUser.Contains(req.QryTechName)))
                   .Where(q => q.Status == 2)
                   ;
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
                && ((req.QryCreateTimeFrom == null || req.QryCreateTimeTo == null) || (a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime <= req.QryCreateTimeTo))
                  ).ToList()
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
                        SubmitDate = workOrder.SubmitDate,
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
                currentTechnicianId = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && "其他设备".Equals(req.MaterialType) ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == req.MaterialType).Distinct().FirstOrDefaultAsync())?.CurrentUserId;
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
        /// 获取客户提交的服务单详情
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
            var query = UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId)
                .Include(s => s.ServiceOrderSNs)
                .Include(s => s.ServiceWorkOrders);
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
                s.NewestContacter,
                s.CustomerName,
                s.Supervisor,
                s.SalesMan,
                s.U_SAP_ID,
                s.ProblemTypeName,
                s.ProblemTypeId,
                s.NewestContactTel,
                ServiceOrderSNs = s.ServiceOrderSNs.GroupBy(o => "其他设备".Equals(o.ItemCode) ? "其他设备" : o.ItemCode.Substring(0, o.ItemCode.IndexOf("-"))).ToList()
                .Select(a => new
                {
                    MaterialType = a.Key,
                    UnitName = "台",
                    Count = a.Count(),
                    orders = a.ToList(),
                    Status = s.ServiceWorkOrders.FirstOrDefault(b => "其他设备".Equals(a.Key) ? b.MaterialCode == "其他设备" : b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == a.Key)?.Status
                })
            }).ToList();
            result.Data = list;
            return result;
        }


        /// <summary>
        /// 获取技术员设备类型列表
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
            var query = UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == SapOrderId)
                        .Include(s => s.ServiceWorkOrders)
                        .Select(a => new
                        {
                            ServiceOrderId = a.Id,
                            a.CreateTime,
                            a.Province,
                            a.City,
                            a.Area,
                            a.Addr,
                            a.NewestContacter,
                            a.NewestContactTel,
                            AppCustId = a.AppUserId,
                            ServiceWorkOrders = a.ServiceWorkOrders.Where(w => w.CurrentUserId == CurrentUserId && string.IsNullOrEmpty(MaterialType) ? true : (string.IsNullOrEmpty(w.MaterialCode) ? "其他设备" : w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-"))) == MaterialType).Select(o => new
                            {
                                o.Id,
                                o.Status,
                                o.FromTheme,
                                ProblemType = o.ProblemType.Description,
                                o.ManufacturerSerialNumber,
                                o.MaterialCode,
                                o.CurrentUserId,
                                MaterialType = o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-"))
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
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                    {
                        MaterialType = string.IsNullOrEmpty(s.Key) ? "其他设备" : s.Key,
                        TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                        Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                        Count = s.Count(),
                        Orders = s.ToList(),
                        UnitName = "台",
                        MaterialTypeName = string.Empty
                    }
                    ).ToList()
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
            var ServiceOrderId = (await UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == SapOrderId).FirstOrDefaultAsync()).Id;
            //获取客户号码 做隐私处理
            string custMobile = (await GetProtectPhone(ServiceOrderId, MaterialType, 1)).Data;
            var query = UnitWork.Find<ServiceOrder>(s => s.U_SAP_ID == SapOrderId)
                .Include(s => s.ServiceWorkOrders);
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
                s.NewestContacter,
                custMobile,
                OrderTakeType = s.ServiceWorkOrders.Where(o => o.ServiceOrderId == s.Id && o.CurrentUserId == CurrentUserId && "其他设备".Equals(MaterialType) ? o.MaterialCode == "其他设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")) == MaterialType).Select(s => s.OrderTakeType).Distinct().FirstOrDefault()
            }).ToList();
            result.Data = list;
            return result;
        }
        /// <summary>
        /// 获取待处理服务单总数
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetServiceOrderCount() 
        {
            return UnitWork.Find<ServiceOrder>(u => u.Status == 1).Count();
        }
        /// <summary>
        /// 获取为派单工单总数
        /// </summary>
        /// <returns></returns>
        public async Task<int> GetServiceWorkOrderCount()
        {
            return UnitWork.Find<ServiceWorkOrder>(u => u.Status == 1).Count();
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
            int? TechnicianId = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ServiceOrderId && string.IsNullOrEmpty(s.MaterialCode) ? "其他设备" == s.ManufacturerSerialNumber : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == MaterialType).Distinct().FirstOrDefaultAsync())?.CurrentUserId;
            var query = from a in UnitWork.Find<AppUserMap>(null)
                        join b in UnitWork.Find<User>(null) on a.UserID equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };
            //获取技术员联系方式
            string TechnicianTel = await query.Where(w => w.a.AppUserId == TechnicianId).Select(s => s.b.Mobile).FirstOrDefaultAsync();
            //获取客户联系方式
            string custMobile = (await UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId).FirstOrDefaultAsync()).NewestContactTel;
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
                ProtectPhone = AliPhoneNumberProtect.bindAxb(custMobile, TechnicianTel);
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

    }
}