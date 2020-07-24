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

namespace OpenAuth.App
{
    public class ServiceOrderApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly ServiceOrderLogApp _serviceOrderLogApp;
        private readonly BusinessPartnerApp _businessPartnerApp;
        public ServiceOrderApp(IUnitWork unitWork,
            RevelanceManagerApp app, ServiceOrderLogApp serviceOrderLogApp, BusinessPartnerApp businessPartnerApp, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
            _serviceOrderLogApp = serviceOrderLogApp;
            _businessPartnerApp = businessPartnerApp;
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
            var query = UnitWork.Find<ServiceOrder>(s => s.AppUserId.Equals(request.AppUserId))
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
                            ServiceWorkOrders = a.ServiceWorkOrders.Select(o => new
                            {
                                o.Id,o.Status,o.FromTheme,
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
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o=>o.MaterialType).Select(s=>new
                    {
                        MaterialType = s.Key,
                        Count = s.Count(),
                        Orders = s.ToList()
                    }
                    ).ToList()
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
        /// 查询超24小时为处理的订单
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
        public async Task<TableData> UnConfirmedServiceOrderList( QueryServiceOrderListReq req)
        {
            var result = new TableData();
            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceOrderSNs)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceOrderId), q => q.Id.Equals(Convert.ToInt32(req.QryServiceOrderId)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ServiceOrderSNs.Any(a=>a.ManufSN.Contains(req.QryManufSN)))
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
                .Include(s=>s.ServiceOrderSNs)
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
            await UnitWork.UpdateAsync<ServiceOrder>(o => o.Id.Equals(request.Id), s => new ServiceOrder { 
                    Status = 2,
                    Addr = obj.Addr,
                    Address = obj.Address,
                    AddressDesignator = obj.AddressDesignator,
                    Services = obj.Services,
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
            obj.ServiceWorkOrders.ForEach(s => { s.ServiceOrderId = obj.Id; s.SubmitDate = DateTime.Now; s.SubmitUserId = loginContext.User.Id; s.AppUserId = obj.AppUserId; });
            await UnitWork.BatchAddAsync<ServiceWorkOrder,int>(obj.ServiceWorkOrders.ToArray());
            var pictures = request.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = request.Id; p.PictureType = 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();

            await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"客服:{loginContext.User.Name}创建工单", ActionType = "创建工单", ServiceOrderId = obj.Id });
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
                Remark =request.Remark,
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
                ServiceOrderId=q.b.Id,
                MaterialType=q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-"))
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

            var e = await UnitWork.AddAsync<ServiceOrder,int>(obj);
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
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.ServiceWorkOrders.Any(a=> a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime <= req.QryCreateTimeTo));

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

            var query = UnitWork.Find<ServiceOrder>(s=>serviceOrderIds.Contains(s.Id))
                .Select(s=>new {
                    s.Id,
                    s.AppUserId,
                    s.Services,
                    s.Latitude,
                    s.Longitude,
                    s.Province,s.City,s.Area,s.Addr,s.Contacter,s.ContactTel,s.NewestContacter,s.NewestContactTel,
                    s.Status,
                    s.CreateTime
                });

            var result = new TableData();
            var list = (await query
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
                Distance = req.Latitude == 0 ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(s.Latitude ?? 0), Convert.ToDouble(s.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude))
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
                    ServiceWorkOrders = s.ServiceWorkOrders.Where(o=>o.Status == 1).Select(o=> new {
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

            var list = (await query
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
                Distance = req.Latitude == 0 ? 0 : NauticaUtil.GetDistance(Convert.ToDouble(a.Latitude ?? 0), Convert.ToDouble(a.Longitude??0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)),
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

            await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员:{req.TechnicianId}接单工单：{req.ServiceWorkOrderIds}", ActionType = "技术员接单", ServiceOrderId = req.ServiceWorkOrderIds });
        }

        /// <summary>
        /// 获取服务单图片Id列表
        /// </summary>
        /// <param name="id">报价单Id</param>
        /// <param name="type">1-客户上传 2-客服上传</param>
        /// <returns></returns>
        public async Task<List<UploadFileResp>> GetServiceOrderPictures(int id, int type)
        {
            var idList = await UnitWork.Find<ServiceOrderPicture>(p => p.ServiceOrderId.Equals(id) && p.PictureType.Equals(type)).Select(p=>p.PictureId).ToListAsync();
            var files = await UnitWork.Find<UploadFile>(f=>idList.Contains(f.Id)).ToListAsync();
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
            var order = await UnitWork.FindSingleAsync<ServiceWorkOrder>(s => s.Id.Equals(req.WorkOrderId));
            if(order != null)
            {
                if (order.CurrentUserId.Equals(req.CurrentUserId))
                {
                    await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id.Equals(req.WorkOrderId), o => new ServiceWorkOrder
                    {
                        BookingDate = req.BookingDate,
                        Status = 3
                    });
                    await UnitWork.SaveAsync();
                    await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{req.CurrentUserId}预约工单{req.WorkOrderId}", ActionType = "预约工单", ServiceWorkOrderId = req.WorkOrderId });
                }
                else
                {
                    throw new CommonException("当前技术员无法预约此工单。", 9001);
                }
            }
            else
            {
                throw new CommonException("当前工单号不存在。", 9002);
            }
        }

        /// <summary>
        /// 技术员核对设备
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task CheckTheEquipment(CheckTheEquipmentReq req)
        {

            var order = await UnitWork.FindSingleAsync<ServiceWorkOrder>(s => s.Id.Equals(req.WorkOrderId));
            if (order != null)
            {
                if (order.CurrentUserId.Equals(req.CurrentUserId))
                {
                    await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id.Equals(req.WorkOrderId), o => new ServiceWorkOrder
                    {
                        Status = 4
                    });
                    await UnitWork.SaveAsync();
                    await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{req.CurrentUserId}核对工单{req.WorkOrderId}设备", ActionType = "核对设备", ServiceWorkOrderId = req.WorkOrderId });
                }
                else
                {
                    throw new CommonException("当前技术员无法核对此工单设备。", 9001);
                }
            }
            else
            {
                throw new CommonException("当前工单号不存在。", 9002);
            }
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
            var orgs = loginContext.Orgs.Select(o=>o.Id).ToArray();

            var tUsers = await UnitWork.Find<AppUserMap>(u => u.AppUserRole == 2).ToListAsync();
            var userIds = _revelanceApp.Get(Define.USERORG, false, orgs);
            var ids = userIds.Intersect(tUsers.Select(u=>u.UserID));
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
                Count = userCount.FirstOrDefault(s=>s.Key.Equals(u.AppUserId))?.Count ?? 0,
                AppUserId = u.AppUserId
            }).ToList();

            return result;
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
                .Select(s=>new 
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
            .GroupBy(s=>s.MaterialType).Select(s=>new 
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
        /// 判断用户是否到达接单上限
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<bool> CheckCanTakeOrder(int id)
        {
            var count = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == id && s.Status.Value < 7).Select(s => s.ServiceOrderId).Distinct().CountAsync();

            return count < 6;
        }
    }
}