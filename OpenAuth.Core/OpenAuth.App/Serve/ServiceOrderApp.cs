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
using Org.BouncyCastle.Ocsp;
using MySqlX.XDevAPI.Relational;
using System.Linq.Expressions;
using SharpDX.Direct3D11;
using System.Runtime.CompilerServices;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.App.Sap.BusinessPartner;
using Minio.DataModel;
using NPOI.SS.Formula.Functions;
using log4net.Core;
using OpenAuth.App.Serve.Request;
using CSRedis;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Options;
using SAP.API;

namespace OpenAuth.App
{
    public class ServiceOrderApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly ServiceOrderLogApp _serviceOrderLogApp;
        private readonly BusinessPartnerApp _businessPartnerApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceWorkOrderAPI _workAPI;
        private IOptions<AppSetting> _appConfiguration;
        private HttpHelper _helper;
        public ServiceOrderApp(IUnitWork unitWork,
            RevelanceManagerApp app, ServiceOrderLogApp serviceOrderLogApp, BusinessPartnerApp businessPartnerApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, IOptions<AppSetting> appConfiguration) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _revelanceApp = app;
            _serviceOrderLogApp = serviceOrderLogApp;
            _businessPartnerApp = businessPartnerApp;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
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
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                    {
                        MaterialType = s.Key,
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

            var data = await query.Select(a => new
            {
                a.Id,
                a.AppUserId,
                a.Services,
                a.CreateTime,
                a.Status,
                ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                {
                    MaterialType = s.Key,
                    Count = s.Count(),
                    Orders = s.ToList()
                }
                    ).ToList()
            }).FirstOrDefaultAsync();


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
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "提交成功",
                Details = "已收到您的反馈，正在为您分配客服中。",
                ServiceOrderId = o.Id
            });
            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "已分配专属客服",
                Details = "已为您分配专属客服进行处理，如有消息将第一时间通知您，请耐心等候。",
                ServiceOrderId = o.Id
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
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceOrderId), q => q.Id.Equals(Convert.ToInt32(req.QryServiceOrderId)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ServiceOrderSNs.Any(a => a.ManufSN.Contains(req.QryManufSN)))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime <= req.QryCreateTimeTo)
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
                q.ServiceOrderSNs.FirstOrDefault(a => a.ManufSN.Contains(req.QryManufSN)).ManufSN,
                q.ServiceOrderSNs.FirstOrDefault(a => a.ManufSN.Contains(req.QryManufSN)).ItemCode,
            });


            result.Data =
            (await query//.OrderBy(u => u.Id)
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync());//.GroupBy(o => o.Id).ToList();
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
            obj.Status = 2;
            obj.SalesMan = d.SlpName;
            obj.SalesManId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.SlpName)))?.Id;
            obj.Supervisor = d.TechName;
            obj.SupervisorId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.TechName)))?.Id;
            await UnitWork.UpdateAsync<ServiceOrder>(o => o.Id.Equals(request.Id), s => new ServiceOrder
            {
                Status = 2,
                Addr = obj.Addr,
                Address = obj.Address,
                AddressDesignator = obj.AddressDesignator,
                //Services = obj.Services,
                Province = obj.Province,
                City = obj.City,
                Area = obj.Area,
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
            obj.ServiceWorkOrders.ForEach(s => { s.ServiceOrderId = obj.Id; s.SubmitDate = DateTime.Now; s.SubmitUserId = loginContext.User.Id; s.AppUserId = obj.AppUserId; s.Status = 1; });
            await UnitWork.BatchAddAsync<ServiceWorkOrder, int>(obj.ServiceWorkOrders.ToArray());
            var pictures = request.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = request.Id; p.PictureType = 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();

            await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "客服确认售后信息",
                Details = "客服确认售后信息，将交至技术员。",
                ServiceOrderId = request.Id
            });
            await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"客服:{loginContext.User.Name}创建工单", ActionType = "创建工单", ServiceOrderId = obj.Id });

            #region 同步到SAP 并拿到服务单主键
            //if (obj.ServiceWorkOrders.Count > 0)
            //{
            //    ServiceWorkOrder firstwork = obj.ServiceWorkOrders[0];
            //    string sapEntry, errMsg;
            //    if(_workAPI.AddServiceWorkOrder(firstwork,out sapEntry,out errMsg))
            //    {
            //        await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id.Equals(request.Id), e => new ServiceOrder
            //        {
            //            U_SAP_ID = System.Convert.ToInt32(sapEntry)
            //        }) ;
            //        await UnitWork.SaveAsync();
            //    }
            //    else
            //    {
            //        throw new CommonException(errMsg, Define.INVALID_TOKEN);
            //    }
            //}

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
            var obj = request.MapTo<ServiceWorkOrder>();
            await UnitWork.AddAsync<ServiceWorkOrder, int>(obj);
            await UnitWork.SaveAsync();
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
            var result = new TableData();
            var query = from a in UnitWork.Find<ServiceWorkOrder>(null)
                        join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };

            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceOrderId), q => q.b.Id.Equals(Convert.ToInt32(req.QryServiceOrderId)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.a.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.b.CustomerId.Contains(req.QryCustomer) || q.b.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.b.RecepUserName.Contains(req.QryRecepUser))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.a.ProblemTypeId.Equals(req.QryProblemType))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.a.CreateTime >= req.QryCreateTimeFrom && q.a.CreateTime <= req.QryCreateTimeTo);
            var workorderlist = await query.OrderBy(r => r.a.CreateTime).Select(q => new
            {
                ServiceOrderId = q.b.Id,
                MaterialType = q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-"))
            }).Distinct().ToListAsync();

            var grouplistsql = from c in workorderlist
                               group c by c.ServiceOrderId into g
                               let MTypes = g.Select(o => o.MaterialType.ToString()).ToArray()
                               select new { ServiceOrderId = g.Key, MaterialTypes = MTypes };
            var grouplist = grouplistsql.ToList();

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
            obj.Status = 2;
            obj.SalesMan = d.SlpName;
            obj.SalesManId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.SlpName)))?.Id;
            obj.Supervisor = d.TechName;
            obj.SupervisorId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.TechName)))?.Id;

            var e = await UnitWork.AddAsync<ServiceOrder, int>(obj);
            await UnitWork.SaveAsync();
            var pictures = req.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = e.Id; p.PictureType = 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();

            await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"客服:{loginContext.User.Name}创建服务单", ActionType = "创建工单", ServiceOrderId = e.Id });
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
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceOrderId), q => q.Id.Equals(Convert.ToInt32(req.QryServiceOrderId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.ServiceWorkOrders.Any(a => a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.ServiceWorkOrders.Any(a => a.Status.Equals(Convert.ToInt32(req.QryState))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ServiceWorkOrders.Any(a => a.ManufacturerSerialNumber.Contains(req.QryManufSN)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.RecepUserName.Contains(req.QryRecepUser))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ServiceWorkOrders.Any(a => a.ProblemTypeId.Equals(req.QryProblemType)))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.ServiceWorkOrders.Any(a => a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime <= req.QryCreateTimeTo));

            var resultsql = query.Select(q => new
            {
                ServiceOrderId = q.Id,
                q.CustomerId,
                q.CustomerName,
                q.TerminalCustomer,
                q.RecepUserName,
                q.Contacter,
                q.ContactTel,
                q.Supervisor,
                q.SalesMan,
                TechName = "",
                ServiceStatus = q.Status,
                ServiceCreateTime = q.CreateTime,
                ServiceWorkOrders = q.ServiceWorkOrders.Where(a => (string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId) || a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                && (string.IsNullOrWhiteSpace(req.QryState) || a.Status.Equals(Convert.ToInt32(req.QryState)))
                && (string.IsNullOrWhiteSpace(req.QryManufSN) || a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                && ((req.QryCreateTimeFrom == null || req.QryCreateTimeTo == null) || (a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime <= req.QryCreateTimeTo))
                ).ToList()
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

            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceOrderId), q => q.b.Id.Equals(Convert.ToInt32(req.QryServiceOrderId)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.a.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.b.CustomerId.Contains(req.QryCustomer) || q.b.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.b.RecepUserName.Contains(req.QryRecepUser))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.c.Name.Contains(req.QryProblemType))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.a.CreateTime >= req.QryCreateTimeFrom && q.a.CreateTime <= req.QryCreateTimeTo)
                         .WhereIf(req.QryMaterialTypes != null && req.QryMaterialTypes.Count > 0, q => req.QryMaterialTypes.Contains(q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-"))));

            var resultsql = query.OrderBy(r => r.a.CreateTime).Select(q => new
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
                    MaterialInfo = s.ServiceWorkOrders.Where(o => o.ServiceOrderId == s.Id && o.CurrentUserId == req.TechnicianId).Select(o => new
                    {
                        o.Status
                    })
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
                s.CreateTime,
                Distance = req.Latitude == 0 ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(s.Latitude ?? 0), Convert.ToDouble(s.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)),
                WorkOrderStatus = s.MaterialInfo.Select(s => s.Status).Distinct().FirstOrDefault()
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
                .WhereIf(int.TryParse(req.key, out int id) || !string.IsNullOrWhiteSpace(req.key), s => s.Id == id || s.CustomerName.Contains(req.key) || s.ServiceWorkOrders.Any(o => o.ManufacturerSerialNumber.Contains(req.key)))
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
                a.CreateTime,
                a.AppUserId,
                a.Province,
                a.City,
                a.Area,
                a.Addr,
                Distance = req.Latitude == 0 ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(a.Latitude ?? 0), Convert.ToDouble(a.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)),
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
            var count = (await UnitWork.Find<ServiceWorkOrder>(s => req.ServiceWorkOrderIds.Contains(s.Id) && s.Status > 1).ToListAsync()).Count;
            if (count > 0)
            {
                throw new CommonException("当前工单已被接单", 90005);
            }
            var b = await CheckCanTakeOrder(req.TechnicianId);

            if (!b)
            {
                throw new CommonException("当前技术员接单已满6单服务单", 90004);
            }

            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => req.ServiceWorkOrderIds.Contains(s.Id), o => new ServiceWorkOrder
            {
                Status = 2,
                CurrentUserId = req.TechnicianId
            });
            await UnitWork.SaveAsync();

            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "移转至技术员",
                Details = "已为您分配技术员进行处理，如有消息将第一时间通知您，请耐心等候",
            }, req.ServiceWorkOrderIds);
            await _serviceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员:{req.TechnicianId}接单工单：{string.Join(",", req.ServiceWorkOrderIds)}", ActionType = "技术员接单" }, req.ServiceWorkOrderIds);
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
            }, "BbsCommunity/AppPushMsg");
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
            var orderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId.Equals(req.CurrentUserId)).Select(o => o.Id).ToListAsync();
            List<int> workOrderIds = new List<int>();
            foreach (var id in orderIds)
            {
                workOrderIds.Add(id);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId, o => new ServiceWorkOrder
            {
                BookingDate = req.BookingDate,
                OrderTakeType = 4
            });
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员预约上门时间",
                Details = $"技术员预约的{req.BookingDate} 上门服务，请耐心等候。",
            }, workOrderIds);
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
                        await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{req.CurrentUserId}核对工单{itemerr}设备(失败)", ActionType = "核对设备", ServiceWorkOrderId = int.Parse(itemerr) });
                    }
                }
            }
            await UnitWork.SaveAsync();
            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "技术员上门服务中",
                Details = $"技术员于{DateTime.Now}开始上门服务",
            }, obj);
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = req.ServiceOrderId, Content = "技术员已核对设备，请完成维修任务", AppUserId = 0 });
        }

        /// <summary>
        /// 查询可以被派单的技术员列表
        /// </summary>
        /// <returns></returns>
        public async Task<List<AllowSendOrderUserResp>> GetAllowSendOrderUser()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var orgs = loginContext.Orgs.Select(o => o.Id).ToArray();

            var tUsers = await UnitWork.Find<AppUserMap>(u => u.AppUserRole == 2).ToListAsync();
            var userIds = _revelanceApp.Get(Define.USERORG, false, orgs);
            var ids = userIds.Intersect(tUsers.Select(u => u.UserID));
            var users = await UnitWork.Find<User>(u => ids.Contains(u.Id)).ToListAsync();
            var us = users.Select(u => new { u.Name, AppUserId = tUsers.FirstOrDefault(a => a.UserID.Equals(u.Id)).AppUserId, u.Id });
            var appUserIds = tUsers.Where(u => userIds.Contains(u.UserID)).Select(u => u.AppUserId).ToList();

            var userCount = await UnitWork.Find<ServiceWorkOrder>(s => appUserIds.Contains(s.CurrentUserId) && s.Status.Value < 7)
                .Select(s => new { s.CurrentUserId, s.ServiceOrderId }).Distinct().GroupBy(s => s.CurrentUserId)
                .Select(g => new { g.Key, Count = g.Count() }).ToListAsync();

            var result = us.Select(u => new AllowSendOrderUserResp
            {
                Id = u.Id,
                Name = u.Name,
                Count = userCount.FirstOrDefault(s => s.Key.Equals(u.AppUserId))?.Count ?? 0,
                AppUserId = u.AppUserId
            }).ToList();

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
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => req.WorkOrderIds.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUserId = req.CurrentUserId,
                Status = 2
            });
            await _appServiceOrderLogApp.BatchAddAsync(new AddOrUpdateAppServiceOrderLogReq
            {
                Title = "移转至技术员",
                Details = "已为您分配技术员进行处理，如有消息将第一时间通知您，请耐心等候",
            }, req.WorkOrderIds);
            await _serviceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"主管{loginContext.User.Name}给技术员{req.CurrentUserId}派单{string.Join(",", req.WorkOrderIds)}", ActionType = "主管派单工单" }, req.WorkOrderIds);
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
                    s.NewestContactTel
                })
                .Skip(0).Take(10).ToListAsync();
            var newestNotCloseOrder = await UnitWork.Find<ServiceOrder>(s => s.CustomerId.Equals(code) && s.Status == 1).OrderByDescending(s => s.CreateTime)
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
                    s.NewestContactTel
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

            return count < 6;
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
                    MaterialInfo = s.ServiceWorkOrders.Where(o => o.ServiceOrderId == req.ServiceOrderId && Type == 1 ? o.CurrentUserId == req.CurrentUserId : o.Status == 1).Select(o => new
                    {
                        o.MaterialCode,
                        o.ManufacturerSerialNumber,
                        MaterialType = string.IsNullOrEmpty(o.MaterialCode) ? "无序列号设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
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
                s.CreateTime,
                s.Contacter,
                s.ContacterTel,
                s.CustomerName,
                s.Supervisor,
                s.SalesMan,
                Distance = req.Latitude == 0 ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(s.Latitude ?? 0), Convert.ToDouble(s.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)),
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
        /// 修改描述（故障/过程）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateWorkOrderDescription(UpdateWorkOrderDescriptionReq request)
        {
            switch (request.DescriptionType.ToLower())
            {
                case "trouble":
                    await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.CurrentUserId == request.CurrentUserId, e => new ServiceWorkOrder
                    {
                        TroubleDescription = request.Description
                    });
                    break;
                case "process":
                    await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.CurrentUserId == request.CurrentUserId, e => new ServiceWorkOrder
                    {
                        ProcessDescription = request.Description
                    });
                    break;
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 选择接单类型
        /// </summary>
        /// <returns></returns>
        public async Task SaveOrderTakeType(SaveWorkOrderTakeTypeReq request)
        {
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.Id && s.CurrentUserId == request.CurrentUserId, e => new ServiceWorkOrder
            {
                OrderTakeType = request.Type,
                Status = 3
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
            return 6 - count;
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
            var userList = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ServiceOrderId && s.CurrentUserId != FromUserId).ToListAsync()).GroupBy(g => g.CurrentUserId)
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
            .Take(req.limit).ToListAsync()).OrderBy(o => o.CreateTime);
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
                var query = UnitWork.Find<ServiceOrderMessage>(s => serviceIds.Contains(s.ServiceOrderId.ToString()));

                var resultsql = query.OrderByDescending(o => o.CreateTime).Select(s => new
                {
                    s.Content,
                    s.CreateTime,
                    s.FroTechnicianName,
                    s.AppUserId,
                    s.ServiceOrderId,
                    s.Replier
                });

                result.Data =
                (await resultsql
                .ToListAsync()).GroupBy(g => g.ServiceOrderId).Select(g => g.First());
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
        /// 提交错误设备信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> ApplyErrorDevices(ApplyErrorDevicesReq request)
        {
            var result = new TableData();
            string head = "技术员核对设备有误提交给呼叫中心的信息";
            string Content = string.Empty;

            foreach (Serve.Request.Device item in request.Devices)
            {
                Content += $"待编辑序列号: {item.manufacturerSerialNumber}<br>正确的序列号: {item.newNumber}<br>正确的物料编码: {item.newCode}<br>";

            }
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = request.ServiceOrderId, Content = head + Content, AppUserId = request.AppUserId });
            return result;
        }

        /// <summary>
        /// 管理员关单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task CloseWorkOrder(CloseWorkOrderReq request)
        {
            var workOrderInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.Id == request.Id).FirstOrDefaultAsync();
            string content = "关单通知<br>工单号：" + workOrderInfo.Id + "<br>序列号：" + (string.IsNullOrEmpty(workOrderInfo.ManufacturerSerialNumber) ? "无" : workOrderInfo.ManufacturerSerialNumber) + "<br>物料编码：" +
               (string.IsNullOrEmpty(workOrderInfo.MaterialCode) ? "无" : workOrderInfo.MaterialCode) + "<br>关单原因：" + request.Reason;
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id == request.Id, u => new ServiceWorkOrder
            {
                Status = 7,
                ProcessDescription = workOrderInfo.ProcessDescription + content
            });
            await UnitWork.SaveAsync();
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = workOrderInfo.ServiceOrderId, Content = content, AppUserId = request.CurrentUserId });
        }
    }
}