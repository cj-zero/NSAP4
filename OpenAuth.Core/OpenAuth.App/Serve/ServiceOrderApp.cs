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

namespace OpenAuth.App
{
    public class ServiceOrderApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        public ServiceOrderApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
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

        public async Task Add(AddServiceOrderReq req)
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
            obj.ServiceWorkOrders.ForEach(swo => 
            {
                swo.CreateTime = DateTime.Now;
                swo.SubmitUserId = loginContext.User.Id;
            });

            var o = await UnitWork.AddAsync(obj);
            var pictures = req.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => p.ServiceOrderId = o.Id);
            await UnitWork.BatchAddAsync(pictures.ToArray());
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
    }
}