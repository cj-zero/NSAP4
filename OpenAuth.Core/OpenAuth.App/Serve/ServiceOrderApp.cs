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

namespace OpenAuth.App
{
    public class ServiceOrderApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        private ServiceOrderLogApp _serviceOrderLogApp;

        public ServiceOrderApp(IUnitWork unitWork,
            RevelanceManagerApp app, ServiceOrderLogApp serviceOrderLogApp, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
            _serviceOrderLogApp = serviceOrderLogApp;
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
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o=>o.MaterialType).Select(s=>new
                    {
                        MaterialType = s.Key,
                        Count = s.Count(),
                        Orders = s.ToList()
                    }
                    ).ToList()
                });

            var result = new TableData();
            result.count = count;
            result.data = list;
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
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.CompletionReport).ThenInclude(c=>c.CompletionReportPictures)
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.CompletionReport).ThenInclude(c=>c.Solution)
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.Solution)
                .Include(s => s.ServiceOrderPictures).FirstOrDefaultAsync();
            var result = obj.MapTo<ServiceOrderDetailsResp>();
            var serviceOrderPictureIds = obj.ServiceOrderPictures.Select(s => s.PictureId).ToList();
            var files = await UnitWork.Find<UploadFile>(f => serviceOrderPictureIds.Contains(f.Id)).ToListAsync();
            result.Files = files.MapTo<List<UploadFileResp>>();
            result.ServiceWorkOrders.ForEach(async s => 
            {
                if(s.CompletionReport != null)
                {
                    var completionReportPictures = obj.ServiceWorkOrders.First(sw => sw.Id.Equals(s.Id))
                            ?.CompletionReport?.CompletionReportPictures.Select(c => c.PictureId).ToList();

                    var completionReportFiles = await UnitWork.Find<UploadFile>(f => completionReportPictures.Contains(f.Id)).ToListAsync();
                    s.CompletionReport.Files = completionReportFiles.MapTo<List<UploadFileResp>>();
                }
            });
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
                q.ServiceOrderSNs.FirstOrDefault(a => a.ManufSN.Contains(req.QryManufSN)).ManufSN,
                q.ServiceOrderSNs.FirstOrDefault(a => a.ManufSN.Contains(req.QryManufSN)).ItemCode,
            });


            result.data =
            (await query//.OrderBy(u => u.Id)
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync());//.GroupBy(o => o.Id).ToList();
            result.count = query.Count();
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
            var obj = request.MapTo<ServiceOrder>();
            obj.Status = 2;
            await UnitWork.UpdateAsync<ServiceOrder>(o => o.Id.Equals(request.Id), s => obj); 
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
            }).ToListAsync();
            result.data = workorderlist;
            return result;
        }

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
            var obj = req.MapTo<ServiceOrder>();
            obj.Status = 2;
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
                        select new { a, b, c};

            query = query.WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceOrderId), q => q.b.Id.Equals(Convert.ToInt32(req.QryServiceOrderId)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.a.Status.Equals(Convert.ToInt32(req.QryState)))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.b.CustomerId.Contains(req.QryCustomer) || q.b.CustomerName.Contains(req.QryCustomer))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.b.RecepUserName.Contains(req.QryRecepUser))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.c.Name.Contains(req.QryProblemType))
                         .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.a.CreateTime >= req.QryCreateTimeFrom && q.a.CreateTime <= req.QryCreateTimeTo)
                         .WhereIf(req.QryMaterialTypes != null && req.QryMaterialTypes.Count > 0, q=>req.QryMaterialTypes.Contains( q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-"))));

            var resultsql = query.OrderBy(r => r.a.CreateTime).Select(q => new
            {
                ServiceOrderId=q.b.Id,
                ServiceWorkOrderId=q.a.Id, 
                q.a.Priority,
                ProblemTypeName=q.c.Name,
                q.a.Status,
                q.b.CustomerId,
                q.b.CustomerName,
                q.a.FromTheme,
                q.a.CreateTime,
                q.b.RecepUserName,
                q.a.ManufacturerSerialNumber,
                q.a.MaterialCode,
                q.a.MaterialDescription
            });


            result.data =
            (await resultsql
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync());//.GroupBy(o => o.Id).ToList();
            result.count = query.Count();
            return result;
        }

        /// <summary>
        /// 技术员查看工单列表
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceWorkOrder(TechnicianServiceWorkOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var query = UnitWork.Find<ServiceWorkOrder>(s=>s.Status == req.TechnicianId).Select(s=>new 
            { 
                s.Id, s.AppUserId, s.FromTheme,
                ProblemType = s.ProblemType.Description,
                s.CreateTime,
                s.Status,
                s.ServiceOrder.Latitude,
                s.ServiceOrder.Longitude,
                s.MaterialCode, 
            });
            var list = await query
            .Skip((req.page - 1) * req.limit)
            .Take(req.limit).ToListAsync();
            var count = await query.CountAsync();
            result.data = list;
            result.data = count;
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
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.Id.Equals(req.ServiceWorkOrderId), o => new ServiceWorkOrder
            {
                Status = 2,
                CurrentUserId = req.TechnicianId
            });
            await UnitWork.SaveAsync();

            await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员:{req.TechnicianId}接单工单：{req.ServiceWorkOrderId}", ActionType = "技术员接单", ServiceOrderId = req.ServiceWorkOrderId });
        }

    }
}