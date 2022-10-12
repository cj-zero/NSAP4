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
using OpenAuth.Repository.Domain.Serve;
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
using OpenAuth.Repository.Extensions;
using OpenAuth.App.Sap.Request;
using System.Text.RegularExpressions;
using System.Data;

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
        private readonly UserManagerApp _userManagerApp;
        private readonly DbExtension _dbExtension;
        private readonly RevelanceManagerApp _revelanceManagerApp;

        public ServiceOrderApp(IUnitWork unitWork,
             RevelanceManagerApp app, ServiceOrderLogApp serviceOrderLogApp, BusinessPartnerApp businessPartnerApp,
             IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, IOptions<AppSetting> appConfiguration, ICapPublisher capBus,
             ServiceOrderLogApp ServiceOrderLogApp, SignalRMessageApp signalrmessage, ServiceFlowApp serviceFlowApp,
             UserManagerApp userManagerApp, DbExtension dbExtension, RevelanceManagerApp revelanceManagerApp) : base(unitWork, auth)
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
            _userManagerApp = userManagerApp;
            _dbExtension = dbExtension;
            _revelanceManagerApp = revelanceManagerApp;
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
            //.Include(s => s.ServiceWorkOrders).ThenInclude(s => s.CompletionReport).ThenInclude(c=>c.CompletionReportPictures)
            //.Include(s => s.ServiceWorkOrders).ThenInclude(s => s.CompletionReport).ThenInclude(c=>c.Solution)
            //.Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
            //.Include(s => s.ServiceWorkOrders).ThenInclude(s => s.Solution)
            var obj = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(id))
                .Include(s => s.ServiceOrderPictures).FirstOrDefaultAsync();
            obj.ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(w => w.ServiceOrderId == id).Include(w=>w.ProblemType).Include(w=>w.Solution).ToListAsync();
            //判断所有工单是否都已完成
            var notFinishcount = obj.ServiceWorkOrders.Where(w => w.ServiceOrderId == id && w.Status < 7).Count();
            var result = obj.MapTo<ServiceOrderDetailsResp>();
            result.IsFinish = notFinishcount > 0 ? false : true;
            var serviceOrderPictures = obj.ServiceOrderPictures.Select(s => new { s.PictureId, s.PictureType }).ToList();
            var serviceOrderPictureIds = serviceOrderPictures.Select(s => s.PictureId).ToList();
            var files = await UnitWork.Find<UploadFile>(f => serviceOrderPictureIds.Contains(f.Id)).ToListAsync();
            result.Files = files.MapTo<List<UploadFileResp>>();
            result.Files.ForEach(f => f.PictureType = serviceOrderPictures.Where(p => f.Id.Equals(p.PictureId)).Select(p => p.PictureType).FirstOrDefault());
            result.dailyReportNum = await UnitWork.Find<ServiceDailyReport>(s => s.ServiceOrderId == result.Id).CountAsync();
            if (result.VestInOrg == 3)
            {
                var completionReportObj = await UnitWork.Find<CompletionReport>(c => c.ServiceOrderId == result.Id).FirstOrDefaultAsync();
                result.Becity = completionReportObj?.Becity;
                result.Destination = completionReportObj?.Destination;
            }
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

            //为职员加上部门前缀
            var recepUserOrgInfo = await _userManagerApp.GetUserOrgInfo(result.RecepUserId);
            result.RecepUserDept = recepUserOrgInfo != null ? recepUserOrgInfo.OrgName : "";

            var salesManOrgInfo = await _userManagerApp.GetUserOrgInfo(result.SalesManId);
            result.SalesManDept = salesManOrgInfo != null ? salesManOrgInfo.OrgName : "";

            var superVisorOrgInfo = await _userManagerApp.GetUserOrgInfo(result.SupervisorId);
            result.SuperVisorDept = superVisorOrgInfo != null ? superVisorOrgInfo.OrgName : "";

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
            MaterialType = "无序列号".Equals(MaterialType) ? "无序列号" : MaterialType.Substring(0, MaterialType.IndexOf("-"));

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
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), s => s.Supervisor == req.QrySupervisor)
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QrySalesMan), s => s.SalesMan == req.QrySalesMan)
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
                q.FromAppUserId
            });

            var loginContext = _auth.GetCurrentUser();
            var user = loginContext.User;
            //如果是销售员角色,只能看到自己名下的客户
            if (loginContext.Roles.Select(r => r.Name).Contains("销售员") && !loginContext.Roles.Select(r => r.Name).Contains("呼叫中心"))
            {
                query = query.Where(q => q.SalesMan == user.Name);
            }

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
                s.U_SAP_ID,
                s.FromAppUserId
            });
            result.Data = list;
            result.Count = query.Count();
            return result;
        }

        /// <summary>
        /// 根据条件统计服务呼叫确认情况(按客户/售后主管/销售员分组)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UnConfirmedServiceInfo(QueryServiceOrderListReq req)
        {
            var result = new TableData();
            //根据条件过滤服务单数据
            var services = UnitWork.Find<ServiceOrder>(null)
                            .WhereIf(req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null, s =>
                            s.CreateTime >= req.QryCreateTimeFrom && s.CreateTime < req.QryCreateTimeTo.Value.AddDays(1))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), s => s.Status == int.Parse(req.QryState));

            //如果是销售员角色,只能看到自己名下的客户
            var loginContext = _auth.GetCurrentUser();
            var user = loginContext.User;
            var isSalesMan = loginContext.Roles.Select(r => r.Name).Contains("销售员");
            if (isSalesMan && !loginContext.Roles.Select(r => r.Name).Contains("呼叫中心"))
            {
                services = services.Where(q => q.SalesMan == user.Name);
            }

            //根据姓名获取用户部门名称
            Func<IEnumerable<string>, Task<List<UserResp>>> getOrgName = async x => await (from u in UnitWork.Find<User>(null)
                                                                                           join r in UnitWork.Find<Relevance>(null) on u.Id equals r.FirstId
                                                                                           join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                                                                                           where x.Contains(u.Name) && r.Key == Define.USERORG
                                                                                           orderby o.CascadeId descending
                                                                                           select new UserResp { Name = u.Name, OrgName = o.Name, CascadeId = o.CascadeId }).ToListAsync();

            //按客户分组
            var query1 = await services.GroupBy(s => s.CustomerId).Select(g => new { CustomerId = g.Key, count = g.Count() }).ToListAsync();
            //根据客户id去sap查询客户信息(服务单里面也存有,但是不准确)
            var d1 = await (from a in UnitWork.Find<OCRD>(null)
                            join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                            from b in ab.DefaultIfEmpty()
                            join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID into ae
                            from e in ae.DefaultIfEmpty()
                            where query1.Select(x => x.CustomerId).Contains(a.CardCode)
                            select new
                            {
                                CustomerId = a.CardCode,
                                CustomerName = a.CardName,
                                SalesMan = b == null ? "" : b.SlpName,
                                SuperVisor = e == null ? "" : e.lastName + e.firstName,
                            }).ToListAsync();
            var sales = await getOrgName(d1.Select(d => d.SalesMan).Distinct()); //获取部门信息
            //按姓名进行分组,最CascadeId最大的那条
            var salesInfo = from s in sales
                            join g in sales.GroupBy(s => s.Name).Select(g => new { Name = g.Key, CascadeId = g.Max(x => x.CascadeId) })
                            on new { s.Name, s.CascadeId } equals new { g.Name, g.CascadeId }
                            select s;
            var super = await getOrgName(d1.Select(d => d.SuperVisor).Distinct());  //获取部门信息
            var superInfo = from s in super
                            join g in super.GroupBy(s => s.Name).Select(g => new { Name = g.Key, CascadeId = g.Max(x => x.CascadeId) })
                            on new { s.Name, s.CascadeId } equals new { g.Name, g.CascadeId }
                            select s;
            var data1 = from q in query1
                        join d in d1 on q.CustomerId equals d.CustomerId
                        join s1 in salesInfo on d.SalesMan equals s1.Name into temp1
                        from t1 in temp1.DefaultIfEmpty()
                        join s2 in superInfo on d.SuperVisor equals s2.Name into temp2
                        from t2 in temp2.DefaultIfEmpty()
                        select new
                        {
                            d.CustomerId,
                            d.CustomerName,
                            SalesMan = (t1?.OrgName ?? "") + "-" + d.SalesMan, //销售员
                            SuperVisor = (t2?.OrgName ?? "") + "-" + d.SuperVisor, //售后主管
                            Count = q.count, //服务单数量
                        };

            //按售后主管分组
            var query2 = await services.GroupBy(s => s.Supervisor).Select(g => new { Supervisor = g.Key, count = g.Count() }).ToListAsync();
            //查询售后主管下有多少个客户
            var superVisors = (from a in UnitWork.Find<OCRD>(null)
                               join e in UnitWork.Find<OHEM>(null) on a.DfTcnician equals e.empID
                               where query2.Select(x => x.Supervisor).Contains(e.lastName + e.firstName)
                               select new { Supervisor = e.lastName + e.firstName, a.CardCode }).Distinct();
            var d2 = superVisors.GroupBy(x => x.Supervisor).Select(g => new
            {
                Supervisor = g.Key,
                CustomerCount = g.Count()
            });
            var o2 = await getOrgName(query2.Select(q => q.Supervisor));
            var supervisorOrg = from a in o2
                                join b in o2.GroupBy(x => x.Name).Select(g => new { Name = g.Key, CascadeId = g.Max(x => x.CascadeId) })
                                on new { a.Name, a.CascadeId } equals new { b.Name, b.CascadeId }
                                select a;
            var data2 = from q in query2
                        join d in d2 on q.Supervisor equals d.Supervisor
                        join o in supervisorOrg on q.Supervisor equals o.Name into temp
                        from t in temp.DefaultIfEmpty()
                        select new
                        {
                            Supervisor = (t?.OrgName ?? "") + "-" + d.Supervisor, //售后主管
                            d.CustomerCount, //服务的客户数量
                            Count = q.count //服务单数量
                        };

            //按销售员分组
            var query3 = await services.GroupBy(s => s.SalesMan).Select(g => new { SalesMan = g.Key, count = g.Count() }).ToListAsync();
            //查询销售员下有多少个客户
            var salesMans = (from a in UnitWork.Find<OCRD>(null)
                             join b in UnitWork.Find<OSLP>(o => query3.Select(x => x.SalesMan).Contains(o.SlpName)) on a.SlpCode equals b.SlpCode
                             select new { SalesMan = b.SlpName, a.CardCode }).Distinct();
            var d3 = salesMans.GroupBy(x => x.SalesMan).Select(g => new
            {
                SalesMan = g.Key,
                CustomerCount = g.Count()
            });
            var o3 = await getOrgName(query3.Select(q => q.SalesMan));
            var salesOrg = from a in o3
                           join b in o3.GroupBy(x => x.Name).Select(g => new { Name = g.Key, CascadeId = g.Max(x => x.CascadeId) })
                           on new { a.Name, a.CascadeId } equals new { b.Name, b.CascadeId }
                           select a;
            var data3 = from q in query3
                        join d in d3 on q.SalesMan equals d.SalesMan
                        join o in salesOrg on q.SalesMan equals o.Name into temp
                        from t in temp.DefaultIfEmpty()
                        select new
                        {
                            SalesMan = (t?.OrgName ?? "") + "-" + d.SalesMan, //销售员
                            d.CustomerCount, //服务的客户数量
                            Count = q.count //服务单数量
                        };

            var list = new List<dynamic>();
            list.Add(new { name = "Customer", data = data1.Distinct().OrderByDescending(d => d.Count) });
            list.Add(new { name = "Supervisor", data = data2.Distinct().OrderByDescending(d => d.Count) });
            list.Add(new { name = "SalesMan", data = data3.Distinct().OrderByDescending(d => d.Count) });

            result.Data = list;
            result.Count = services.Count();

            return result;
        }

        /// <summary>
        /// 统计服务单的状态(待确认,已确认,已取消)数量及占比
        /// 查询条件可选:日期范围、客户、售后主管
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<dynamic> GetServiceCallConfirmStatistics(QueryServiceOrderListReq req)
        {
            var result = new TableData();
            //查询服务单的数据,按确认状态分类
            var serviceData = UnitWork.Find<ServiceOrder>(null)
                .WhereIf(req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null,
                            s => s.CreateTime >= req.QryCreateTimeFrom && s.CreateTime < req.QryCreateTimeTo.Value.AddDays(1))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), s => s.CustomerName.Contains(req.QryCustomer) || s.CustomerId.Contains(req.QryCustomer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), s => s.Supervisor == req.QrySupervisor)
                            .GroupBy(s => s.Status)
                            .Select(g => new
                            {
                                g.Key,
                                count = g.Count()
                            });
            //在字典中查询服务单的各个状态
            var states = await UnitWork.Find<Category>(c => c.TypeId == "SYS_ServiceOrderStatus" && !string.IsNullOrWhiteSpace(c.DtValue))
                            .Select(x => new { x.DtValue, x.Name }).ToListAsync();
            //服务单总数
            var totalCount = serviceData.Sum(x => x.count);
            //在查询无数据的时候,数量和百分比显示为0
            var data = from s in states
                       join sd in serviceData on int.Parse(s.DtValue) equals sd.Key into temp
                       from t in temp.DefaultIfEmpty()
                       select new
                       {
                           s.Name,
                           count = t == null ? 0 : t.count,
                           percent = t == null ? "0.00%" : ((decimal)t.count / totalCount).ToString("P2") //保留两位小数
                       };

            result.Data = data;
            result.Count = totalCount;

            return result;
        }

        /// <summary>
        /// 统计解决方案类型
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetSolutionInfo(QueryServiceOrderListReq req)
        {
            var result = new TableData();
            if (req.QryCreateTimeFrom == null || req.QryCreateTimeTo == null)
            {
                req.QryCreateTimeFrom = DateTime.Now;
                req.QryCreateTimeTo = DateTime.Now;
            }
            //根据日期从数据库查询日报中的解决方案
            var processDescriptions = await UnitWork.Find<ServiceDailyReport>(null)
                .Where(s => s.CreateTime >= req.QryCreateTimeFrom && s.CreateTime < req.QryCreateTimeTo.Value.AddDays(1))
                .Select(s => s.ProcessDescription).ToListAsync();
            //处理json格式的解决方案数据
            var processInfoData = processDescriptions.Select(p => new
            {
                processCode = GetServiceTroubleAndSolution(p, "code"),
                processDescription = GetServiceTroubleAndSolution(p, "description")
            });
            //将多读多的关系转换为列表
            var query = new List<AddOrUpdateSolutionReq>();
            processInfoData.ForEach(p =>
            {
                for (int i = 0; i < p.processCode.Count(); i++)
                {
                    query.Add(new AddOrUpdateSolutionReq { Code = p.processCode[i], Descriptio = p.processDescription[i] });
                }
            });
            //统计,没有code的一律视为自定义
            var data = query.GroupBy(d => d.Code).Select(g => new
            {
                code = g.Key == "" ? "自定义" : g.Key,
                count = g.Count(),
                desc = g.Key == "" ? "自定义" : query.FirstOrDefault(x => x.Code == g.Key)?.Descriptio
            });
            //左连接,时间段内没有的,数量为0,方便前端显示
            var resultData = (from s in await UnitWork.Find<Solution>(null).Where(s => s.IsNew == true)
                             .Select(s => new { code = s.Descriptio + "-" + s.Code, desc = s.Subject }).ToListAsync()
                              join d in data on s.code equals d.code into temp
                              from t in temp.DefaultIfEmpty()
                              select new SolutionInfo
                              {
                                  Code = s.code,
                                  Count = t == null ? 0 : t.count,
                                  Desc = s.desc
                              }).ToList();
            //加上自定义的
            var custom = data.FirstOrDefault(d => d.code == "自定义");
            resultData.Add(new SolutionInfo
            {
                Code = "自定义",
                Count = custom == null ? 0 : custom.count,
                Desc = "自定义",
            });

            result.Data = resultData.OrderByDescending(r => r.Count);
            result.Count = resultData.Count();

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
            var log = await UnitWork.Find<ServiceOrderLog>(c => c.ServiceOrderId == id && c.ActionType == "撤销操作").FirstOrDefaultAsync();
            var result = obj.MapTo<ServiceOrderDetailsResp>();
            result.RevokeUser = log?.CreateUserName;
            result.RevokeTime = log?.CreateTime;
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
            if (obj.ServiceWorkOrders != null)
            {
                var expect = await CalculateRatio(obj.ServiceWorkOrders.FirstOrDefault()?.FromTheme);
                obj.ExpectServiceMode = expect.ExpectServiceMode;
                obj.ExpectRatio = expect.ExpectRatio;
            }
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
            obj.AllowOrNot = await IsAllowOrNo(new CustomerServiceAgentCreateOrderReq { TerminalCustomer = obj.TerminalCustomer, ServiceWorkOrders = request.ServiceWorkOrders });
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
                RecepUserId = loginContext.User.Id,
                AllowOrNot = obj.AllowOrNot,
                ExpectRatio = obj.ExpectRatio,
                ExpectServiceMode = obj.ExpectServiceMode
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


            var MaterialType = string.Join(",", obj.ServiceWorkOrders.Select(s => s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-"))).Distinct().ToArray());

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
                    MaterialType = "无序列号".Equals(assignedWorks.FirstOrDefault().MaterialCode) ? "无序列号" : assignedWorks.FirstOrDefault().MaterialCode.Substring(0, assignedWorks.FirstOrDefault().MaterialCode.IndexOf("-"))
                });

                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "技术员接单成功",
                    Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                    LogType = 2,
                    ServiceOrderId = obj.Id,
                    ServiceWorkOrder = String.Join(',', assignedWorks.Select(o => o.Id).ToArray()),
                    MaterialType = "无序列号".Equals(assignedWorks.FirstOrDefault().MaterialCode) ? "无序列号" : assignedWorks.FirstOrDefault().MaterialCode.Substring(0, assignedWorks.FirstOrDefault().MaterialCode.IndexOf("-"))
                });
                await _signalrmessage.SendSystemMessage(SignalRSendType.User, $"系统已自动分配了{assignedWorks.Count()}个新的售后服务，请尽快处理", new List<string>() { d.TechName });
            }
        }
        /// <summary>
        /// 删除一个工单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task DeleteWorkOrder(QueryServiceOrderListReq req)
        {
            if (req.QryAllowOrNot == -1) 
            {
                await UnitWork.UpdateAsync<ServiceOrder>(s=>s.Id==int.Parse(req.QryServiceOrderId),s=>new ServiceOrder { AllowOrNot=1});
            }
            var count=await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == int.Parse(req.QryServiceOrderId)).CountAsync();
            if(count<=1) throw new Exception("不可删除最后一个工单，如需删除请新建工单后重新删除。");
            await UnitWork.DeleteAsync<ServiceWorkOrder>(s => s.Id==int.Parse(req.QryServiceWorkOrderId));
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
                var ServiceWorkOrders = await UnitWork.Find<ServiceWorkOrder>(u => u.ServiceOrderId.Equals(request.ServiceOrderId)).OrderByDescending(u => u.Id).ToListAsync();
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
                var typename = "无序列号".Equals(obj.MaterialCode) ? "无序列号" : obj.MaterialCode.Substring(0, obj.MaterialCode.IndexOf("-"));
                if (obj.FromType == 2)
                {
                    var AppUser = await UnitWork.Find<AppUserMap>(s => s.UserID == loginContext.User.Id).Include(s => s.User).FirstOrDefaultAsync();
                    obj.CurrentUser = loginContext.User.Name;
                    obj.CurrentUserNsapId = loginContext.User.Id;
                    obj.CurrentUserId = AppUser?.AppUserId;
                    obj.Status = 7;
                    obj.CompleteDate = DateTime.Now;
                }
                else
                {
                    var workOrderObj = ServiceWorkOrders.Where(s => s.MaterialCode.Equals(typename) || (s.MaterialCode != "无序列号" && s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")).Equals(typename))).FirstOrDefault();
                    if (workOrderObj != null)
                    {
                        obj.CurrentUser = workOrderObj.CurrentUser;
                        obj.CurrentUserNsapId = workOrderObj.CurrentUserNsapId;
                        obj.CurrentUserId = workOrderObj.CurrentUserId;
                        obj.Status = workOrderObj.Status;
                    }
                    else
                    {
                        obj.Status = 1;
                    }
                }
                await UnitWork.AddAsync<ServiceWorkOrder, int>(obj);
                if (request.AllowOrNot == -1) 
                {
                    await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id == request.ServiceOrderId, s => new ServiceOrder { AllowOrNot = 1 });
                }
                await UnitWork.SaveAsync();

                //log日志与发送消息


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
        /// 修改终端客户
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task ModifyCustomer(ModifyServiceOrderReq request)
        {
            await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id.Equals(request.Id), e => new ServiceOrder
            {
                TerminalCustomerId = request.TerminalCustomerId,
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
                        where b.VestInOrg == 1 && b.AllowOrNot==0 && b.FromId!=8
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
                         .Where(q => q.b.U_SAP_ID != null && q.b.Status == 2 && q.a.FromType != 2 && q.b.VestInOrg == 1);
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
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-派送服务ID")))
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
                MaterialType = q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-")) == null ? "无序列号" : (q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-")) == "" ? "无序列号" : q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-")))
            }).Distinct().ToListAsync();

            var grouplistsql = from c in workorderlist
                               group c by c.ServiceOrderId into g
                               let U_SAP_ID = g.Select(a => a.U_SAP_ID).First()
                               let MTypes = g.Select(o => o.MaterialType == "无序列号" ? "无序列号" : MaterialTypeModel.Where(u => u.TypeAlias == o.MaterialType).FirstOrDefault().TypeName).ToArray()
                               let WorkMTypes = g.Select(o => o.MaterialType == "无序列号" ? "无序列号" : o.MaterialType)
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
        /// 判定是否需要业务员审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private async Task<int> IsAllowOrNo(CustomerServiceAgentCreateOrderReq req)
        {
            //大学学院客户过滤
            if (!req.TerminalCustomer.Contains("大学") && !req.TerminalCustomer.Contains("学院") && !req.TerminalCustomer.Contains("中科院") && req.ServiceWorkOrders.FirstOrDefault()?.FromType!=2)
            {
                if (req.ServiceWorkOrders.Select(s => s.ManufacturerSerialNumber).ToList().Contains("无序列号"))
                {
                    return 1;
                }
                else
                {
                    var mnfSerials = req.ServiceWorkOrders.Select(s => s.ManufacturerSerialNumber).ToList();
                    var warrantyDates = await UnitWork.Find<SalesOrderWarrantyDate>(s => mnfSerials.Contains(s.MnfSerial)).ToListAsync();
                    
                    foreach (var item in mnfSerials)
                    {
                        var warrantyDate = warrantyDates.Where(w => w.MnfSerial.Equals(item)).FirstOrDefault();
                        if (warrantyDate == null || warrantyDate.WarrantyPeriod < DateTime.Now) return 1;
                    }
                }
            }
            return 0;
        }
        /// <summary>
        /// 客服新建服务单
        /// </summary>
        /// <returns></returns>
        public async Task<Infrastructure.Response<int>> CustomerServiceAgentCreateOrder(CustomerServiceAgentCreateOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            Infrastructure.Response<int> result = new Infrastructure.Response<int>();
            #region 验证客户联系人，SAP没有则新增
            var contact = await UnitWork.Find<OCPR>(c => c.CardCode == req.TerminalCustomerId).Select(c => new { c.Name, c.Tel1 }).ToListAsync();
            if (!contact.Exists(c => c.Name == req.NewestContacter && c.Tel1 == req.NewestContactTel))
            {
                //姓名+电话组合不存在的情况而名字单独存在的情况下
                if (contact.Exists(c => c.Name == req.NewestContacter))
                {
                    result.Code = 500;
                    result.Message = "该客户已存在同名联系人。若手动修改了联系人或联系方式，请确保两个同时修改。";
                    return result;
                }
                else if (contact.Exists(c => c.Tel1 == req.NewestContactTel))
                {
                    result.Code = 500;
                    result.Message = "该客户已存在该联系方式。若手动修改了联系人或联系方式，请确保两个同时修改。";
                    return result;
                }
                else//名字和电话都不存在则新增
                {
                    AddCoustomerContact cc = new AddCoustomerContact()
                    {
                        CardCode = req.TerminalCustomerId,
                        NewestContacter = req.NewestContacter,
                        NewestContactTel = req.NewestContactTel,
                        Address = req.Province + req.City + req.Area + req.Addr
                    };
                    _capBus.Publish("Serve.OCPR.Create", cc);
                }
            }
            #endregion

            var loginUser = loginContext.User;
            var loginUserOrg = loginContext.Orgs.OrderByDescending(c => c.CascadeId).Select(c=>new UserResp { Name = "", Id = "", OrgId = c.Id, OrgName = c.Name, CascadeId = c.CascadeId }).FirstOrDefault();
            if (loginContext.User.Account == Define.USERAPP && req.AppUserId != null)
            {
                loginUser = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(req.AppUserId)).Include(u => u.User).Select(u => u.User).FirstOrDefaultAsync();
                loginUserOrg = await _userManagerApp.GetUserOrgInfo(loginUser.Id);
            }
            if (req.FromId == 8)//来源内联单
            {
                loginUser = await UnitWork.Find<User>(c => c.Account == Define.SYSTEM_USERNAME).FirstOrDefaultAsync();
            }
            var d = await _businessPartnerApp.GetDetails(req.TerminalCustomerId.ToUpper());
            var obj = req.MapTo<ServiceOrder>();
            obj.CustomerId = req.CustomerId.ToUpper();
            obj.TerminalCustomerId = req.TerminalCustomerId.ToUpper();
            obj.AllowOrNot = 0;
            obj.SalesManId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.SlpName)))?.Id;
            if (req.FromId == 8)//来源内联单
            {
                obj.RecepUserName = "刘静";
                obj.RecepUserId = "204c4bd6-c7c6-11ea-bc9e-54bf645e326d";
            }
            else
            {
                obj.RecepUserName = loginUser.Name;
                obj.RecepUserId = loginUser.Id;

                if (!obj.SalesManId.Equals(loginUser.Id))
                {
                    obj.AllowOrNot = await IsAllowOrNo(req);
                }
            }
            obj.CreateUserId = loginUser.Id;
            obj.Status = 2;
            obj.SalesMan = d.SlpName;
            obj.VestInOrg = 1;
            //obj.Supervisor = d.TechName;
            obj.SupervisorId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(req.Supervisor)))?.Id;
            if (string.IsNullOrWhiteSpace(obj.NewestContacter) && string.IsNullOrWhiteSpace(obj.NewestContactTel))
            {
                obj.NewestContacter = obj.Contacter;
                obj.NewestContactTel = obj.ContactTel;
            }
            var expect = await CalculateRatio(obj.ServiceWorkOrders.FirstOrDefault()?.FromTheme);
            obj.ExpectServiceMode = expect.ExpectServiceMode;
            obj.ExpectRatio = expect.ExpectRatio;
            #region 该客户在5天内已有服务ID
            //var workOrders = obj.ServiceWorkOrders.Select(s => new { s.ManufacturerSerialNumber, s.FromTheme }).ToList();
            //var msNumbers = workOrders.Select(s => s.ManufacturerSerialNumber).ToList();
            //var serviceOrders = await UnitWork.Find<ServiceWorkOrder>(s => msNumbers.Contains(s.ManufacturerSerialNumber) && s.Status < 7).Select(s =>new { s.ServiceOrderId,s.ManufacturerSerialNumber,s.FromTheme}).ToListAsync();
            //serviceOrders= serviceOrders.Where(s => s.FromTheme.Equals(workOrders.Where(w => w.ManufacturerSerialNumber.Equals(s.ManufacturerSerialNumber)).FirstOrDefault().FromTheme)).ToList();
            //var serviceOrderIds = serviceOrders.Select(s => s.ServiceOrderId).ToList();
            //var serviceOrderCount = await UnitWork.Find<ServiceOrder>(s => s.TerminalCustomer.Equals(obj.TerminalCustomer) && s.TerminalCustomerId.Equals(obj.TerminalCustomerId) && serviceOrderIds.Contains(s.Id) &&  s.Status == 2 && obj.NewestContacter.Equals(s.NewestContacter)&& obj.NewestContactTel.Equals(s.NewestContactTel)).Select(s=>s.CreateTime).ToListAsync();
            //var count=serviceOrderCount.Where(s => ((TimeSpan)(DateTime.Now - s)).Days < 5).Count();
            //if (count > 0)
            //{
            //    throw new Exception("该客户在5天内已有服务ID，请不要重复建单");
            //}
            #endregion
            //获取"其他"问题类型及其子类
            var otherProblemType = await UnitWork.Find<ProblemType>(o => o.Name.Equals("其他") && string.IsNullOrWhiteSpace(o.ParentId)).FirstOrDefaultAsync();
            var ChildTypes = new List<ProblemType>();
            if (otherProblemType != null && !string.IsNullOrEmpty(otherProblemType.Id))
            {
                ChildTypes = await UnitWork.Find<ProblemType>(null).Where(o1 => o1.ParentId.Equals(otherProblemType.Id)).ToListAsync();
            }
            var AppUser = await UnitWork.Find<AppUserMap>(s => s.UserID == obj.SupervisorId).Include(s => s.User).FirstOrDefaultAsync();
            var AppUserId = await UnitWork.Find<AppUserMap>(s => s.UserID == loginUser.Id).Select(s => s.AppUserId).FirstOrDefaultAsync();
            var isHasNum = false;
            var serialNumber = await CheckManufSn(obj.CustomerId);
            obj.ServiceWorkOrders.ForEach(s =>
            {
                if (s.ManufacturerSerialNumber== "无序列号" && loginUserOrg.OrgName!="S19")
                {
                    result.Code = 500;
                    result.Message = "非S19呼叫中心人员，不允许提交无序列号的呼叫。";
                    isHasNum = true;
                    //throw new Exception("");
                }
                if (string.IsNullOrWhiteSpace(s.FromTheme))
                {
                    result.Code = 500;
                    result.Message = "呼叫主题不能为空。";
                    isHasNum = true;
                }
                if (s.ManufacturerSerialNumber != "无序列号" && !serialNumber.Any(c => c.ManufSN == s.ManufacturerSerialNumber && c.ItemCode == s.MaterialCode))
                {
                    result.Code = 500;
                    result.Message = "序列号和物料编码不匹配，请关闭窗口重新选择。";
                    isHasNum = true;
                }
                s.SubmitDate = DateTime.Now;
                s.SubmitUserId = loginUser.Id;
                if (req.IsSend != null && (bool)req.IsSend)
                {
                    s.CurrentUser = loginUser.Name;
                    s.CurrentUserId = AppUserId;
                    s.CurrentUserNsapId = loginUser.Id;
                    s.Status = 2;
                }
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
                    s.CurrentUser = loginUser.Name;
                    s.CurrentUserId = AppUserId;
                    s.CurrentUserNsapId = loginUser.Id;
                    s.Status = 7;
                    s.CompleteDate = DateTime.Now;
                }
                #endregion
            });
            if (isHasNum) return result;

            var e = await UnitWork.AddAsync<ServiceOrder, int>(obj);
            await UnitWork.SaveAsync();
            var pictures = req.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = e.Id; p.PictureType = p.PictureType == 3 ? 3 : 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();

            var MaterialType = string.Join(",", obj.ServiceWorkOrders.Select(s => s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-"))).Distinct().ToArray());

            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"用户:{loginUser.Name}创建服务单", ActionType = "创建服务单", ServiceOrderId = e.Id, MaterialType = MaterialType });
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
                    MaterialType = "无序列号".Equals(assignedWorks.FirstOrDefault().MaterialCode) ? "无序列号" : assignedWorks.FirstOrDefault().MaterialCode.Substring(0, assignedWorks.FirstOrDefault().MaterialCode.IndexOf("-"))
                });

                await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                {
                    Title = "技术员接单成功",
                    Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                    LogType = 2,
                    ServiceOrderId = obj.Id,
                    ServiceWorkOrder = String.Join(',', assignedWorks.Select(o => o.Id).ToArray()),
                    MaterialType = "无序列号".Equals(assignedWorks.FirstOrDefault().MaterialCode) ? "无序列号" : assignedWorks.FirstOrDefault().MaterialCode.Substring(0, assignedWorks.FirstOrDefault().MaterialCode.IndexOf("-"))
                });
                await _signalrmessage.SendSystemMessage(SignalRSendType.User, $"系统已自动分配了{assignedWorks.Count()}个新的售后服务，请尽快处理", new List<string>() { obj.Supervisor });
            }
            result.Result = obj.Id;
            return result;
        }

        public async Task CehckContacter(AddCoustomerContact obj)
        {
            _capBus.Publish("Serve.OCPR.Create", obj);
        }

        /// <summary>
        /// 获取客户下序列号
        /// </summary>
        /// <param name="cardCode"></param>
        /// <returns></returns>
        public async Task<List<SerialNumberResp>> CheckManufSn(string cardCode)
        {
            var query = await UnitWork.Find<OINS>(c => c.customer == cardCode).Select(c => new SerialNumberResp { ManufSN = c.manufSN, ItemCode = c.itemCode, DeliveryNo = c.deliveryNo }).ToListAsync();

            var ServiceOinsModels =await UnitWork.Find<ServiceOins>(q => q.customer == cardCode).Select(q => new SerialNumberResp
            {
                ManufSN = q.manufSN,
                ItemCode = q.itemCode,
                DeliveryNo = q.deliveryNo
            }).ToListAsync();

            var MergeModels = query.Union(ServiceOinsModels);
            var mergelist = MergeModels.GroupBy(d => new { d.ManufSN, d.ItemCode, d.DeliveryNo }).Select(g => g.First()).ToList();
            return mergelist;
        }
        /// <summary>
        /// 工程部新建服务单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response<int>> CISECreateServiceOrder(CustomerServiceAgentCreateOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId != null)
            {
                var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(req.AppUserId)).Select(u => u.UserID).FirstOrDefaultAsync();
                if (userid == null)
                {
                    throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
                }
                loginUser = await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
            }
            if (req.FromId == 8)//来源内联单
            {
                loginUser = await UnitWork.Find<User>(c => c.Account == Define.SYSTEM_USERNAME).FirstOrDefaultAsync();
            }
            Infrastructure.Response<int> result = new Response<int>();
            var d = await _businessPartnerApp.GetDetails(req.CustomerId.ToUpper());
            var obj = req.MapTo<ServiceOrder>();
            obj.CustomerId = req.CustomerId.ToUpper();
            obj.TerminalCustomerId = req.TerminalCustomerId.ToUpper();
            if (req.FromId == 8)//来源内联单
            {
                obj.RecepUserName = "刘静";
                obj.RecepUserId = "204c4bd6-c7c6-11ea-bc9e-54bf645e326d";
            }
            else
            {
                obj.RecepUserName = loginUser.Name;
                obj.RecepUserId = loginUser.Id;
                req.Supervisor = "樊静涛";//默认售后主管为E3主管
            }
            var supervisorinfo = await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(req.Supervisor));
            obj.CreateUserId = loginUser.Id;
            obj.Status = 2;
            obj.SalesMan = d.SlpName;
            obj.SalesManId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.SlpName)))?.Id;
            obj.VestInOrg = 2;
            obj.Supervisor = req.Supervisor;
            obj.NewestContacter = loginUser.Name;
            obj.Contacter = loginUser.Name;
            //obj.Supervisor = d.TechName;
            obj.SupervisorId = supervisorinfo?.Id;
            #region 该客户在5天内已有服务ID
            //var workOrders = obj.ServiceWorkOrders.Select(s => new { s.ManufacturerSerialNumber, s.FromTheme }).ToList();
            //var msNumbers = workOrders.Select(s => s.ManufacturerSerialNumber).ToList();
            //var serviceOrders = await UnitWork.Find<ServiceWorkOrder>(s => msNumbers.Contains(s.ManufacturerSerialNumber) && s.Status < 7).Select(s => new { s.ServiceOrderId, s.ManufacturerSerialNumber, s.FromTheme }).ToListAsync();
            //serviceOrders = serviceOrders.Where(s => s.FromTheme.Equals(workOrders.Where(w => w.ManufacturerSerialNumber.Equals(s.ManufacturerSerialNumber)).FirstOrDefault().FromTheme)).ToList();
            //var serviceOrderIds = serviceOrders.Select(s => s.ServiceOrderId).ToList();
            //var serviceOrderCount = await UnitWork.Find<ServiceOrder>(s => s.TerminalCustomer.Equals(obj.TerminalCustomer) && s.TerminalCustomerId.Equals(obj.TerminalCustomerId) && serviceOrderIds.Contains(s.Id) && s.Status == 2).Select(s => s.CreateTime).ToListAsync();
            //var count = serviceOrderCount.Where(s => ((TimeSpan)(DateTime.Now - s)).Days < 5).Count();
            //if (count > 0)
            //{
            //    throw new Exception("该客户在5天内已有服务ID，请不要重复建单");
            //}
            #endregion
            if (string.IsNullOrWhiteSpace(obj.NewestContacter) && string.IsNullOrWhiteSpace(obj.NewestContactTel))
            {
                obj.NewestContacter = obj.Contacter;
                obj.NewestContactTel = obj.ContactTel;
            }
            //获取"其他"问题类型及其子类
            //var otherProblemType = await UnitWork.Find<ProblemType>(o => o.Name.Equals("其他") && string.IsNullOrWhiteSpace(o.ParentId)).FirstOrDefaultAsync();
            //var ChildTypes = new List<ProblemType>();
            //if (otherProblemType != null && !string.IsNullOrEmpty(otherProblemType.Id))
            //{
            //    ChildTypes = await UnitWork.Find<ProblemType>(null).Where(o1 => o1.ParentId.Equals(otherProblemType.Id)).ToListAsync();
            //}
            //var AppUser = await UnitWork.Find<AppUserMap>(s => s.UserID == obj.SupervisorId).Include(s => s.User).FirstOrDefaultAsync();
            var AppUserId = req.AppUserId;
            if (req.AppUserId == null)
            {
                AppUserId = await UnitWork.Find<AppUserMap>(s => s.UserID == loginUser.Id).Select(s => s.AppUserId).FirstOrDefaultAsync();
            }
            obj.ServiceWorkOrders.ForEach(s =>
            {
                s.SubmitDate = DateTime.Now;
                s.SubmitUserId = loginUser.Id;
                s.Status = 2;
                s.CurrentUserNsapId = req.FromId == 8 ? supervisorinfo?.Id : loginUser.Id;//来源内联单派给主管
                s.CurrentUserId = AppUserId;
                s.FromType = 1;
                s.FeeType = 1;
                s.CurrentUser = req.FromId == 8 ? supervisorinfo?.Name : loginUser.Name;
                s.OrderTakeType = 1;
            });
            var e = await UnitWork.AddAsync<ServiceOrder, int>(obj);
            await UnitWork.SaveAsync();
            var pictures = req.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = e.Id; p.PictureType = p.PictureType == 3 ? 3 : 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();

            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"工程部:{loginUser.Name}创建服务单", ActionType = "创建服务单", ServiceOrderId = e.Id });
            #region 同步到SAP 并拿到服务单主键
            _capBus.Publish("Serve.ServcieOrder.Create", obj.Id);
            #endregion
            result.Result = obj.Id;
            return result;
        }
        /// <summary>
        /// 新建行政单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task CreateAdministrativeOrder(CustomerServiceAgentCreateOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId != null)
            {
                var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(req.AppUserId)).Select(u => u.UserID).FirstOrDefaultAsync();
                if (userid == null)
                {
                    throw new CommonException("未绑定ERP账户", Define.INVALID_APPUser);
                }
                loginUser = await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
            }
            var d = await _businessPartnerApp.GetDetails(req.CustomerId.ToUpper());
            var obj = req.MapTo<ServiceOrder>();
            obj.CustomerId = req.CustomerId.ToUpper();
            obj.TerminalCustomerId = req.TerminalCustomerId.ToUpper();
            obj.RecepUserName = loginUser.Name;
            obj.RecepUserId = loginUser.Id;
            obj.CreateUserId = loginUser.Id;
            obj.Status = 2;
            obj.SalesMan = d.SlpName;
            obj.SalesManId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(d.SlpName)))?.Id;
            obj.VestInOrg = 3;
            obj.Supervisor = req.Supervisor;
            obj.NewestContacter = loginUser.Name;
            obj.Contacter = loginUser.Name;
            obj.Supervisor = d.TechName;
            obj.SupervisorId = (await UnitWork.FindSingleAsync<User>(u => u.Name.Equals(obj.Supervisor)))?.Id;
            if (string.IsNullOrWhiteSpace(obj.NewestContacter) && string.IsNullOrWhiteSpace(obj.NewestContactTel))
            {
                obj.NewestContacter = obj.Contacter;
                obj.NewestContactTel = obj.ContactTel;
            }
            var AppUserId = req.AppUserId;
            if (req.AppUserId == null)
            {
                AppUserId = await UnitWork.Find<AppUserMap>(s => s.UserID == loginUser.Id).Select(s => s.AppUserId).FirstOrDefaultAsync();
            }
            if (string.IsNullOrWhiteSpace(AppUserId.ToString()))
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            obj.ServiceWorkOrders.ForEach(s =>
            {
                s.SubmitDate = DateTime.Now;
                s.SubmitUserId = loginUser.Id;
                s.Status = 2;
                s.CurrentUserNsapId = loginUser.Id;
                s.CurrentUserId = AppUserId;
                s.FromType = 1;
                s.FeeType = 1;
                s.CurrentUser = loginUser.Name;
                s.OrderTakeType = 2;
                s.ServiceMode = 1;
            });
            var e = await UnitWork.AddAsync<ServiceOrder, int>(obj);
            await UnitWork.SaveAsync();
            var pictures = req.Pictures.MapToList<ServiceOrderPicture>();
            pictures.ForEach(p => { p.ServiceOrderId = e.Id; p.PictureType = p.PictureType == 3 ? 3 : 2; });
            await UnitWork.BatchAddAsync(pictures.ToArray());
            await UnitWork.SaveAsync();

            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"用户:{loginUser.Name}创建行政单", ActionType = "创建行政单", ServiceOrderId = e.Id });
            #region 同步到SAP 并拿到服务单主键
            _capBus.Publish("Serve.ServcieOrder.Create", obj.Id);
            #endregion
        }

        /// <summary>
        /// 重新同步至SAP，未获取到SAPID时用
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task RePulish(int? internalContactId, int? serviceOrderId = null)
        {
            if (serviceOrderId != null)
            {
                _capBus.Publish("Serve.ServcieOrder.Create", serviceOrderId);
            }
            else
            {
                var ids = await UnitWork.Find<InternalContactTaskServiceOrder>(c => c.InternalContactId == internalContactId).Select(c => c.ServiceOrderId).ToListAsync();
                var serviceOrder = await UnitWork.Find<ServiceOrder>(c => ids.Contains(c.Id) && string.IsNullOrWhiteSpace(c.U_SAP_ID.ToString())).Select(c => c.Id).ToListAsync();
                foreach (var item in serviceOrder)
                {
                    _capBus.Publish("Serve.ServcieOrder.Create", item);
                }
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
            var loginUser = loginContext.User;
            if (loginContext.User.Account == Define.USERAPP && req.AppUserId!=null) 
            {
                loginUser = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(req.AppUserId)).Include(u=>u.User).Select(u=>u.User).FirstOrDefaultAsync();
            }
            var result = new TableData();
            List<string> techName = new List<string>();
            List<int> status = new List<int>();
            if (!string.IsNullOrWhiteSpace(req.QryTechName))
                techName = req.QryTechName.Split(",").ToList();
            if (!string.IsNullOrWhiteSpace(req.QryStateList))
            {
                var num= req.QryStateList.Split(",");
                status = Array.ConvertAll(num, int.Parse).ToList();
            }

            var ids = await UnitWork.Find<ServiceWorkOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.Status.Equals(Convert.ToInt32(req.QryState)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryStateList), q => status.Contains(q.Status.Value))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceMode), q => q.ServiceMode.Equals(Convert.ToInt32(req.QryServiceMode)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ManufacturerSerialNumber.Contains(req.QryManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ProblemTypeId.Equals(req.QryProblemType))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromType), q => q.FromType.Equals(Convert.ToInt32(req.QryFromType)))
                //.WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), q => q.CurrentUser.Contains(req.QryTechName))
                .WhereIf(techName.Count > 0, q => techName.Contains(q.CurrentUser))
                //.WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                //.WhereIf(!string.IsNullOrWhiteSpace(req.QryFromTheme), q => q.FromTheme.Contains(req.QryFromTheme))
                .WhereIf(req.CompleteDate != null, q => q.CompleteDate > req.CompleteDate)
                .WhereIf(req.EndCompleteDate != null, q => q.CompleteDate < Convert.ToDateTime(req.EndCompleteDate).AddDays(1))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryMaterialCode), q => q.MaterialCode.Contains(req.QryMaterialCode))
                .OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();

            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceWorkOrders)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer) || q.TerminalCustomerId.Contains(req.QryCustomer) || q.TerminalCustomer.Contains(req.QryCustomer))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.RecepUserName.Contains(req.QryRecepUser))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.ContactTel.Contains(req.ContactTel) || q.NewestContactTel.Contains(req.ContactTel))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), q => q.Supervisor.Contains(req.QrySupervisor))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryVestInOrg), q => q.VestInOrg == Convert.ToInt32(req.QryVestInOrg))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryAllowOrNot.ToString()), q => q.AllowOrNot == req.QryAllowOrNot)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QrySalesMan), q => q.SalesMan == req.QrySalesMan)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromId), q => q.FromId == Convert.ToInt32(req.QryFromId))
                .WhereIf(string.IsNullOrWhiteSpace(req.QryFromId) && (req.QryVestInOrg == "1"|| req.QryVestInOrg == "2"), q => q.FromId != 8)//服务呼叫列表排除ECN
                .Where(q => ids.Contains(q.Id) && q.Status == 2);

            //根据部门筛选数据
            if (!string.IsNullOrWhiteSpace(req.QryDeptName))
            {
                //查询服务单中所有主管的名字
                var supervisorNames = await UnitWork.Find<ServiceOrder>(null).Select(s => s.Supervisor).Distinct().ToListAsync();

                //根据姓名获取用户部门名称
                Func<IEnumerable<string>, Task<List<UserResp>>> GetOrgName = async x =>
                           await (from u in UnitWork.Find<User>(null)
                                  join r in UnitWork.Find<Relevance>(null) on u.Id equals r.FirstId
                                  join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                                  where x.Contains(u.Name) && r.Key == Define.USERORG
                                  orderby o.CascadeId descending
                                  select new UserResp { Name = u.Name, OrgName = o.Name, CascadeId = o.CascadeId }).ToListAsync();
                var supervisorInfos = (await GetOrgName(supervisorNames)).Select(x => new ProcessingEfficiency { Dept = x.OrgName, SuperVisor = x.Name });

                //筛选出查询部门主管的名字
                var superVisors = supervisorInfos.Where(s => s.Dept == req.QryDeptName).Select(s => s.SuperVisor).ToList();
                var param1 = "";
                if (superVisors != null && superVisors.Count() > 0)
                {
                    var object1 = "";
                    foreach (var item in superVisors)
                    {
                        object1 += $",'{item}'";
                    }
                    param1 = $" and so.supervisor in ({object1.Substring(1)})";
                }
                else
                {
                    param1 = " and true = false";
                }

                if (req.QryCreateTimeFrom != null)
                {
                    param1 += $" and so.CreateTime >= '{req.QryCreateTimeFrom}'";
                }
                if (req.QryCreateTimeTo != null)
                {
                    param1 += $" and so.CreateTime < '{req.QryCreateTimeTo.Value.AddDays(1).Date}'";
                }

                string sql = $@"select t.Id
                            from(
	                            select so.Id,
	                            min(sw.Status) as status,
	                            min(so.CreateTime) as starttime,
	                            max(sw.CompleteDate) as endtime,
	                            max(so.Supervisor) as supervisor
	                            from serviceorder as so
	                            join serviceworkorder as sw 
	                            on so.Id = sw.ServiceOrderId
	                            where so.VestInOrg = 1
	                            and so.status = 2
                                {param1}
	                            group by so.Id
                            ) t where 1 = 1";

                var parameters = new List<object>();
                var finishData = _dbExtension.GetObjectDataFromSQL<ServiceOrderData>(sql, parameters.ToArray(), typeof(Nsap4ServeDbContext))?.ToList();
                query = query.Where(q => finishData.Select(x => x.Id).Contains(q.Id));
            }
            //根据处理时效区间筛选数据
            if (!string.IsNullOrWhiteSpace(req.FinishTimeInterval))
            {
                string sql = $@"select t.Id
                            from(
	                            select so.Id,
	                            min(sw.Status) as status,
	                            min(so.CreateTime) as starttime,
	                            max(sw.CompleteDate) as endtime,
	                            max(so.Supervisor) as supervisor
	                            from serviceorder as so
	                            join serviceworkorder as sw 
	                            on so.Id = sw.ServiceOrderId
	                            where so.VestInOrg = 1
	                            and so.status = 2
	                            group by so.Id
                            ) t where 1 = 1";
                var timeinterval = await UnitWork.Find<Category>(c => c.TypeId == "SYS_efficiency" && c.DtValue == req.FinishTimeInterval).Select(c => c.DtValue).FirstOrDefaultAsync();
                if (!string.IsNullOrEmpty(timeinterval) && timeinterval != "0")
                {
                    sql += " and t.status >= 7";
                    if (!string.IsNullOrWhiteSpace(timeinterval.Split(':')[0]))
                    {
                        var startPoint = timeinterval.Split(':')[0];
                        sql += $" and timestampdiff(minute,t.starttime,t.endtime) > 24*60*{startPoint}";
                    }
                    if (!string.IsNullOrWhiteSpace(timeinterval.Split(':')[1]))
                    {
                        var endPoint = timeinterval.Split(':')[1];
                        sql += $" and timestampdiff(minute,t.starttime,t.endtime) <= 24*60*{endPoint}";
                    }
                }
                else if (timeinterval == "0")
                {
                    sql += " and t.status < 7 or t.endtime is null";
                }

                var parameters = new List<object>();
                var finishData = _dbExtension.GetObjectDataFromSQL<ServiceOrderData>(sql, parameters.ToArray(), typeof(Nsap4ServeDbContext))?.ToList();
                query = query.Where(q => finishData.Select(x => x.Id).Contains(q.Id));
            }
            //根据处理时效区间筛选数据 小时
            if (!string.IsNullOrWhiteSpace(req.FinishResponseTime))
            {
                var param1 = "";
                var param2 = "";
                if (req.QryCreateTimeFrom != null)
                {
                    param1 += $" and so.CreateTime >= '{req.QryCreateTimeFrom}' ";
                }
                if (req.QryCreateTimeTo != null)
                {
                    param1 += $" and so.CreateTime < '{req.QryCreateTimeTo.Value.AddDays(1).Date}' ";
                }


                if (req.FinishResponseTime == "d1")
                {
                    param2 += $" and timestampdiff(HOUR,t.createtime,t.endtime) <= 24 ";
                }
                else if (req.FinishResponseTime == "d2")
                {
                    param2 += $" and timestampdiff(HOUR,t.createtime,t.endtime) >  24 and timestampdiff(minute,t.starttime,t.endtime) <=  48";
                }
                else if (req.FinishResponseTime == "d3")
                {
                    param2 += $" and timestampdiff(HOUR,t.createtime,t.endtime) >  48 and timestampdiff(minute,t.starttime,t.endtime) <=  72";
                }
                else if (req.FinishResponseTime == "d4")
                {
                    param2 += $" and timestampdiff(HOUR,t.createtime,t.endtime) >72 ";
                }

                //--min(sw.Status) as status,
                string sql1 = $@"select t.Id
                             from(
                              select so.Id,
                               max(so.Supervisor) as supervisor,
                               min(so.CreateTime) as createtime,
                               min(sw.AcceptTime) as endtime
                              from serviceorder as so
                              inner join serviceworkorder as sw 
                              on so.Id = sw.ServiceOrderId
                               where so.VestInOrg = 1
                               and so.Status = 2
                               {param1}  
                                and ISNULL(sw.AcceptTime)= FALSE
                                and not exists(
                                select 1
                                  from serviceworkorder as s
                                  where s.status >= 7
                                  and s.ServiceOrderId = so.Id
                                )
                               group by so.Id
                             ) t WHERE 1=1 {param2}";


                //var timeinterval = await UnitWork.Find<Category>(c => c.TypeId == "SYS_efficiency" && c.DtValue == req.FinishTimeInterval).Select(c => c.DtValue).FirstOrDefaultAsync();

                var parameters = new List<object>();
                //var finishData1 = _dbExtension.GetObjectDataFromSQL<ServiceOrderData>(sql1, parameters.ToArray(), typeof(Nsap4ServeDbContext))?.ToList();
                var finishData1 = UnitWork.ExcuteSqlTable(ContextType.Nsap4ServeDbContextType, sql1, CommandType.Text);
                List<int> list = new List<int>();
                foreach(DataRow item in finishData1.Rows)
                {
                    list.Add(Convert.ToInt32(item.ItemArray[0]));
                }
                query = query.Where(q => list.Contains(q.Id));
                //var query1 = query.Where(q => list.Contains(q.Id)).ToList();
            }
            //未完成的服务单所处时间区间筛选
            if (!string.IsNullOrWhiteSpace(req.TimeInterval))
            {
                //所有未完工的记录
                IQueryable<ServiceOrder> services = UnitWork.Find<ServiceOrder>(s => s.Status == 2
                                                        && s.ServiceWorkOrders.Any(sw => sw.Status < 7))
                                                    .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440));
                var interval = req.TimeInterval.Split('-');
                var currentTime = DateTime.Now;
                //开始间隔天数
                if (!string.IsNullOrWhiteSpace(interval[0]))
                {
                    var startPoint = double.Parse(interval[0]);
                    services = services.Where(s => currentTime.AddDays(-startPoint) > s.CreateTime);
                }
                //结束间隔天数
                if (!string.IsNullOrWhiteSpace(interval[1]))
                {
                    var endPoint = double.Parse(interval[1]);
                    services = services.Where(s => currentTime.AddDays(-endPoint) <= s.CreateTime);
                }

                var unFinishServiceIds = await services.Select(s => s.Id).Distinct().ToListAsync();
                query = query.Where(q => unFinishServiceIds.Contains(q.Id));
            }
            //已完成的服务单所处时间区间筛选
            if (!string.IsNullOrWhiteSpace(req.CompletedTimeInterval))
            {
                //所有已完工的记录
                //IQueryable<ServiceOrder> services = UnitWork.Find<ServiceOrder>(s => s.Status == 2 && s.VestInOrg == 1 && s.ServiceWorkOrders.Any(sw => sw.Status >= 7 && sw.AcceptTime != null && sw.CompleteDate != null))
                //                                    .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                //                                    .Include(c => c.ServiceWorkOrders);
                var serviceData = await (from so in UnitWork.Find<ServiceOrder>(s => s.Status == 2 && s.VestInOrg == 1 && s.ServiceWorkOrders.Any(sw => sw.Status >= 7 && sw.AcceptTime != null && sw.CompleteDate != null))
                                            .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                                            .Include(c => c.ServiceWorkOrders)
                                         select new
                                         {
                                             so.Id,
                                             ProcessingTime = (so.ServiceWorkOrders.FirstOrDefault().CompleteDate - so.ServiceWorkOrders.FirstOrDefault().AcceptTime).Value.TotalDays
                                         }).ToListAsync();
                var interval = req.CompletedTimeInterval.Split('-');
                //var currentTime = DateTime.Now;
                //开始间隔天数
                if (!string.IsNullOrWhiteSpace(interval[0]))
                {
                    var startPoint = double.Parse(interval[0]);
                    //services = services.Where(s => (s.ServiceWorkOrders.FirstOrDefault().CompleteDate.Value - s.ServiceWorkOrders.FirstOrDefault().AcceptTime.Value).Days > startPoint);
                    serviceData = serviceData.Where(c => c.ProcessingTime > startPoint).ToList();
                }
                //结束间隔天数
                if (!string.IsNullOrWhiteSpace(interval[1]))
                {
                    var endPoint = double.Parse(interval[1]);

                    //services = services.Where(s => (s.ServiceWorkOrders.FirstOrDefault().CompleteDate.Value - s.ServiceWorkOrders.FirstOrDefault().AcceptTime.Value).Days <= endPoint);
                    serviceData = serviceData.Where(c => c.ProcessingTime <= endPoint).ToList();
                }
                var unFinishServiceIds = serviceData.Select(s => s.Id).Distinct().ToList();
                //var unFinishServiceIds = await services.Select(s => s.Id).Distinct().ToListAsync();
                var query2 = await query.Where(q => unFinishServiceIds.Contains(q.Id)).CountAsync();
                query = query.Where(q => unFinishServiceIds.Contains(q.Id));
            }
            //未完工原因筛选
            if (!string.IsNullOrWhiteSpace(req.UnCompletedReason))
            {
                //在未完工历史表中没有记录的都是未填写
                if (req.UnCompletedReason == "14")
                {
                    var history = UnitWork.Find<ServiceUnCompletedReasonHistory>(null);
                    query = query.Where(q => q.ServiceWorkOrders.Any(sw => sw.Status < 7) && !history.Any(h => h.ServiceOrderId == q.Id));
                }
                else if (req.UnCompletedReason == "13") //取最新一次的填写记录,没有未完工原因id的,说明在新增时在字典中找不到,属于用户自己填写的理由,归类为其他
                {
                    var lastHistory = UnitWork.Find<ServiceUnCompletedReasonHistory>(null)
                        .GroupBy(s => s.ServiceOrderId).Select(g => new { ServiceOrderId = g.Key, Id = g.Max(x => x.Id) });
                    var unCompletedIds = await (from d in UnitWork.Find<ServiceUnCompletedReasonDetail>(null)
                                                join l in lastHistory on new { d.ServiceOrderId, Id = d.ServiceUnCompletedReasonHistoryId } equals new { l.ServiceOrderId, l.Id }
                                                where d.UnCompletedReasonId == ""
                                                select d.ServiceOrderId).Distinct().ToListAsync();
                    query = query.Where(q => q.ServiceWorkOrders.Any(sw => sw.Status < 7) && unCompletedIds.Contains(q.Id));
                }
                else if (req.UnCompletedReason == "0") //选项为全部时,不做任何过滤
                {
                }
                else
                {
                    //var unCompletedReasonId = UnitWork.Find<Category>(c => c.TypeId == "SYS_UnCompletedReason" && c.Name == req.UnCompletedReason).FirstOrDefault()?.DtValue;
                    var lastHistory = UnitWork.Find<ServiceUnCompletedReasonHistory>(null)
                        .GroupBy(s => s.ServiceOrderId).Select(g => new { ServiceOrderId = g.Key, Id = g.Max(x => x.Id) });
                    var unCompletedIds = await (from d in UnitWork.Find<ServiceUnCompletedReasonDetail>(null).WhereIf(!string.IsNullOrWhiteSpace(req.UnCompletedReason), d => d.UnCompletedReasonId == req.UnCompletedReason)
                                                join l in lastHistory on new { d.ServiceOrderId, Id = d.ServiceUnCompletedReasonHistoryId } equals new { l.ServiceOrderId, l.Id }
                                                select d.ServiceOrderId).Distinct().ToListAsync();
                    query = query.Where(q => q.ServiceWorkOrders.Any(sw => sw.Status < 7) && unCompletedIds.Contains(q.Id));
                }
            }
            if(!string.IsNullOrWhiteSpace(req.UrgedDept))
            {
                //查询服务单中所有主管的名字
                var supervisorNames = await UnitWork.Find<ServiceOrder>(null).Select(s => s.Supervisor).Distinct().ToListAsync();
                //根据姓名获取用户部门名称
                Func<IEnumerable<string>, Task<List<UserResp>>> GetOrgName = async x =>
                           await (from u in UnitWork.Find<User>(null)
                                  join r in UnitWork.Find<Relevance>(null) on u.Id equals r.FirstId
                                  join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                                  where x.Contains(u.Name) && r.Key == Define.USERORG
                                  orderby o.CascadeId descending
                                  select new UserResp { Name = u.Name, OrgName = o.Name, CascadeId = o.CascadeId }).ToListAsync();
                var supervisorInfos = (await GetOrgName(supervisorNames)).Select(x => new ProcessingEfficiency { Dept = x.OrgName, SuperVisor = x.Name });

                //筛选出查询部门主管的名字
                var superVisors = supervisorInfos.Where(s => s.Dept == req.UrgedDept).Select(s => s.SuperVisor).ToList();
                var serviceOrderIds = await (from s in UnitWork.Find<ServiceOrder>(null)
                                             join m in UnitWork.Find<ServiceOrderMessage>(null)
                                             .WhereIf(req.UrgedStartTime != null, x => x.CreateTime >= req.UrgedStartTime)
                                             .WhereIf(req.UrgedEndTime != null, x => x.CreateTime < req.UrgedEndTime.Value.AddMonths(1))
                                             on s.Id equals m.ServiceOrderId
                                             where superVisors.Contains(s.Supervisor) && m.Content.Contains("催办")
                                             select s.Id).Distinct().ToListAsync();
                query = query.Where(q => serviceOrderIds.Contains(q.Id));
            }
            //日报解决方案
            if (!string.IsNullOrWhiteSpace(req.QryProDescription))
            {
                var serviceOrderIds = await UnitWork.Find<ServiceDailyReport>(c => c.ProcessDescription.Contains(req.QryProDescription)).Select(c => new { c.ProcessDescription, c.ServiceOrderId }).ToListAsync();
                List<int> sids = new List<int>();
                serviceOrderIds.ForEach(c =>
                {
                    var gs = GetServiceTroubleAndSolution(c.ProcessDescription, "description");
                    gs.ForEach(r =>
                    {
                        if (r == req.QryProDescription)
                        {
                            sids.Add(c.ServiceOrderId.Value);
                        }
                    });
                });
                query = query.Where(q => sids.Contains(q.Id));
            }
            //技术员部门
            if (!string.IsNullOrWhiteSpace(req.QryTechOrgName))
            {

                var orgids = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.Name == req.QryTechOrgName).Select(o => o.Id).ToListAsync();
                //查看自己部门下的成员
                var orgUserIds = await UnitWork.Find<Relevance>(r => orgids.Contains(r.SecondId) && r.Key == Define.USERORG).Select(r => r.FirstId).ToListAsync();
                //根据员工id过滤服务单id
                var sIds = await UnitWork.Find<ServiceWorkOrder>(q => orgUserIds.Contains(q.CurrentUserNsapId)).Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                query = query.Where(q => sIds.Contains(q.Id));
            }
            //呼叫主题
            if (!string.IsNullOrWhiteSpace(req.QryFromTheme))
            {
                var theme = await UnitWork.Find<ServiceWorkOrder>(c => c.FromTheme.Contains(req.QryFromTheme)).Select(c => new { c.ServiceOrderId, c.FromTheme }).ToListAsync();
                List<int> sids = new List<int>();
                theme.ForEach(c =>
                {
                    var gs = GetServiceTroubleAndSolution(c.FromTheme, "description");
                    gs.ForEach(r =>
                    {
                        if (r== req.QryFromTheme)
                        {
                            sids.Add(c.ServiceOrderId);
                        }
                    });
                });
                query = query.Where(q => sids.Contains(q.Id));
            }

            //根据员工信息获取员工部门信息
            var user = loginContext.User;
            var orgInfo = await _userManagerApp.GetUserOrgInfo(user.Id); //部门信息
            var orgId = orgInfo.OrgId;
            var orgName = orgInfo.OrgName;

            var specialDepts = await UnitWork.Find<Category>(c => c.TypeId == "SYS_SpecialDept").Select(c => c.DtValue).ToListAsync();
            //在字典维护的特殊部门的主管,可以看到自己部门下员工承接的工单                                              
            if (specialDepts.Contains(orgName))
            {
                //获取所有部门的管理人员信息
                var deptManage = _revelanceManagerApp.GetDeptManager();
                //根据部门Id查找部门管理者中是否含有登录人的id
                var dept = deptManage.FirstOrDefault(d => d.OrgId == orgId);
                var isManager = deptManage.FirstOrDefault(d => d.OrgId == orgId)?.UserId.Any(x => x == user.Id);
                //主管可以查看自己部门下所有成员的服务单
                if (isManager != null && isManager.Value)
                {
                    var orgids = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => o.Name.Equals(orgName)).Select(o => o.Id).ToListAsync();
                    //查看自己部门下的成员
                    var orgUserIds = new List<string>();
                    orgUserIds.AddRange(await UnitWork.Find<Relevance>(r => orgids.Contains(r.SecondId) && r.Key == Define.USERORG).Select(r => r.FirstId).ToListAsync());
                    //根据员工id过滤服务单id
                    var sIds = await UnitWork.Find<ServiceWorkOrder>(q => orgUserIds.Contains(q.CurrentUserNsapId)).OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                    query = query.Where(q => sIds.Contains(q.Id));
                }
                else
                {
                    //非主管只能查看自己的
                    var sIds = await UnitWork.Find<ServiceWorkOrder>(q => q.CurrentUserNsapId == user.Id).OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                    query = query.Where(q => sIds.Contains(q.Id));
                }

            }
            else if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("工程主管")) && !loginContext.User.Account.Equals("wanghaitao") && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心")) && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-查看服务ID")))
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
                    var sIds = await UnitWork.Find<ServiceWorkOrder>(q => q.CurrentUserNsapId.Contains(loginUser.Id)).OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();

                    query = query.Where(q => q.SupervisorId.Equals(loginUser.Id) || sIds.Contains(q.Id) || q.SalesManId.Equals(loginUser.Id) || q.CreateUserId.Equals(loginUser.Id));
                }
            }

            var resultsql = query.OrderByDescending(q => q.CreateTime).Select(q => new ServiceWorkOrderList
            {
                ServiceOrderId = q.Id,
                CustomerId = q.CustomerId,
                CustomerName = q.CustomerName,
                TerminalCustomerId = q.TerminalCustomerId,
                TerminalCustomer = q.TerminalCustomer,
                RecepUserName = q.RecepUserName,
                Contacter = q.Contacter,
                ContactTel = q.ContactTel,
                NewestContacter = q.NewestContacter,
                NewestContactTel = q.NewestContactTel,
                Supervisor = q.Supervisor,
                SalesMan = q.SalesMan,
                //TechName = "",
                U_SAP_ID = q.U_SAP_ID,
                VestInOrg = q.VestInOrg,
                ServiceStatus = q.Status,
                ServiceCreateTime = q.CreateTime,
                AllowOrNot = q.AllowOrNot,
                Remark = q.Remark,
                FromId = q.FromId,
                ServiceWorkOrders = q.ServiceWorkOrders.Where(a => (string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId) || a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                && (string.IsNullOrWhiteSpace(req.QryState) || a.Status.Equals(Convert.ToInt32(req.QryState)))
                && (status.Count == 0 || status.Contains(a.Status.Value))
                && (string.IsNullOrWhiteSpace(req.QryServiceMode) || a.ServiceMode.Equals(Convert.ToInt32(req.QryServiceMode)))
                && (string.IsNullOrWhiteSpace(req.QryManufSN) || a.ManufacturerSerialNumber.Contains(req.QryManufSN))
                //&& ((req.QryCreateTimeFrom == null || req.QryCreateTimeTo == null) || (a.CreateTime >= req.QryCreateTimeFrom && a.CreateTime <= req.QryCreateTimeTo))
                && (string.IsNullOrWhiteSpace(req.QryFromType) || a.FromType.Equals(Convert.ToInt32(req.QryFromType)))
                && (string.IsNullOrWhiteSpace(req.QryTechName) || a.CurrentUser.Contains(req.QryTechName) || techName.Contains(a.CurrentUser))
                && (string.IsNullOrWhiteSpace(req.QryProblemType) || a.ProblemTypeId.Equals(req.QryProblemType))
                && (string.IsNullOrWhiteSpace(req.QryFromTheme) || a.FromTheme.Contains(req.QryFromTheme))
                && (req.CompleteDate == null || (a.CompleteDate > req.CompleteDate))
                && (req.EndCompleteDate == null || (a.CompleteDate < Convert.ToDateTime(req.EndCompleteDate).AddDays(1)))
                ).OrderBy(a => a.Status).ToList(),
            });

            var data = await resultsql.Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();

            //根据服务id进行分组,取最近的一次未完工原因提交记录
            var lastUncompletedId = UnitWork.Find<ServiceUnCompletedReasonHistory>(null)
                                        .Where(s => data.Select(d => d.ServiceOrderId).Contains(s.ServiceOrderId))
                                        .GroupBy(s => s.ServiceOrderId).Select(g => new { ServiceOrderId = g.Key, Id = g.Max(x => x.Id) });
            var lastUncompletedReason = await (from r in UnitWork.Find<ServiceUnCompletedReasonHistory>(null)
                                               join l in lastUncompletedId on r.Id equals l.Id
                                               select new
                                               {
                                                   r.ServiceOrderId,
                                                   r.Content
                                               }).ToListAsync();
            //根据服务id获取最近一次的未完工原因提交记录
            data.All(d =>
            {
                d.UnCompletedReason = lastUncompletedReason.FirstOrDefault(l => l.ServiceOrderId == d.ServiceOrderId)?.Content ?? "";
                return true;
            });
            data.ForEach(c =>
            {
                if (c.VestInOrg == 1)
                {
                    c.MaterialTypes = c.ServiceWorkOrders.Select(s => s.MaterialCode.IndexOf("-") == -1 ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-"))).Distinct().ToList();
                    //c.MaterialTypes = c.ServiceWorkOrders.Select(s => s.MaterialCode == "无序列号" ? "无序列号" : (s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == "" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")))).Distinct().ToList();
                }
            });
            result.Data = data;
            result.Count = query.Count();
            return result;
        }

        /// <summary>
        /// 根据服务单的工单状态分类,统计各个状态的数量及占比
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetWorkorderStatusStatistics(QueryServiceOrderListReq req)
        {
            var result = new TableData();

            var serviceData = from so in UnitWork.Find<ServiceOrder>(s => s.Status == 2)
                              .WhereIf(req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null, s => s.CreateTime >= req.QryCreateTimeFrom && s.CreateTime < req.QryCreateTimeTo.Value.AddDays(1))
                              .WhereIf(!string.IsNullOrWhiteSpace(req.QryVestInOrg), s => s.VestInOrg == Convert.ToInt32(req.QryVestInOrg))
                              .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), s => s.Supervisor == req.QrySupervisor)
                              join swo in UnitWork.Find<ServiceWorkOrder>(null)
                              .WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), s => s.CurrentUser == req.QryTechName)
                              on so.Id equals swo.ServiceOrderId
                              select new { so, swo };

            //因为服务单和工单是一对多的关系,所以各个状态的数量加起来可能会大于总数(即一个服务单可能有不同状态的工单)
            var statesStatistics = await (serviceData.Select(d => new { d.so.Id, d.swo.Status }).Distinct().GroupBy(x => x.Status).Select(q => new
            {
                q.Key,
                count = q.Count()
            })).ToListAsync();

            //因为分类表和上面的服务单不在同一个库,所以分开查
            //查询字典中的工单状态,dtValue为空表示全部状态,数据库中enable为0的表示可用,映射到布尔型的属性为false
            var states = await UnitWork.Find<Category>(c => c.TypeId == "SYS_ServiceWorkOrderStatus" && !string.IsNullOrWhiteSpace(c.DtValue) && c.Enable == false).OrderBy(c => c.SortNo).ToListAsync();

            //总数
            var totalCount = await serviceData.Select(d => d.so.Id).Distinct().CountAsync();
            var data = from s in states
                       join ss in statesStatistics
                       on int.Parse(s.DtValue) equals ss.Key into temp
                       from t in temp.DefaultIfEmpty()
                       select new
                       {
                           stateId = s.DtValue,
                           stateDesc = s.Name,
                           count = t == null ? 0 : t.count,
                           percent = t == null ? "0" : ((decimal)t.count / totalCount).ToString("P2") //保留两位小数
                       };
            result.Data = data;
            result.Count = totalCount;

            return result;
        }

        /// <summary>
        /// 统计各部门在一段时间内处理服务单的占比,以及每个部门内处理天数的统计情况
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServerCallEfficiency(QueryServiceOrderListReq req)
        {
            var result = new TableData();

            //只看客诉单,状态是已确认的,并且服务单下的工单都是已完成的(只要有一个工单未完成,整个服务单就算未完成)
            string dateParam = "";
            if(req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null)
            {
                dateParam = " and so.CreateTime >= {0} and so.CreateTime < {1}";
            }
            var sql = $@"select t.supervisor,
                        count(distinct case when t.endtime is not null	then t.Id end) as finishcount,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) <= 1 * 1440	then t.Id end) as d1,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) > 1 * 1440 and timestampdiff(minute,t.createtime,t.endtime) <= 2 * 1440	then t.Id end) as d2,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) > 2 * 1440 and timestampdiff(minute,t.createtime,t.endtime) <= 3 * 1440	then t.Id end) as d3,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) > 3 * 1440 and timestampdiff(minute,t.createtime,t.endtime) <= 4 * 1440	then t.Id end) as d4,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) > 4 * 1440 and timestampdiff(minute,t.createtime,t.endtime) <= 5 * 1440	then t.Id end) as d5,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) > 5 * 1440 and timestampdiff(minute,t.createtime,t.endtime) <= 6 * 1440	then t.Id end) as d6,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) > 6 * 1440 and timestampdiff(minute,t.createtime,t.endtime) <= 14 * 1440	then t.Id end) as d7_14,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) > 14 * 1440 and timestampdiff(minute,t.createtime,t.endtime) <= 30 * 1440	then t.Id end) as d15_30,
                        count(distinct case when t.endtime is not null and timestampdiff(minute,t.createtime,t.endtime) > 30 * 1440	then t.Id end) as d30
                        from(
	                        select so.Id,
                            max(so.Supervisor) as supervisor,
                            min(so.CreateTime) as createtime,
                            max(sw.CompleteDate) as endtime
	                        from serviceorder as so
	                        inner join serviceworkorder as sw 
	                        on so.Id = sw.ServiceOrderId
                            where so.VestInOrg = 1
                            and so.Status = 2
	                        {dateParam}
                            and not exists (
		                        select 1
                                from serviceworkorder as s
                                where s.status < 7
                                and s.ServiceOrderId = so.Id
                            )
                            group by so.Id
                        ) t
                        group by t.supervisor;";
            var parameters = new List<object>();
            if (req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null)
            {
                parameters.Add(req.QryCreateTimeFrom);
                parameters.Add(req.QryCreateTimeTo.Value.AddDays(1));
            }
            
            var finishData = _dbExtension.GetObjectDataFromSQL<ProcessingEfficiency>(sql, parameters.ToArray(), typeof(Nsap4ServeDbContext))?.ToList();
            finishData.ForEach(f => f.Dept = _userManagerApp.GetUserOrgInfo(null, f.SuperVisor).Result?.OrgName);

            //将同一部门下的数据合并
            var d1 = finishData.GroupBy(f => f.Dept).Select(g => new
            {
                dept = g.Key,
                dFinishCount = g.Sum(x => x.FinishCount),
                d1 = g.Sum(x => x.D1),
                d2 = g.Sum(x => x.D2),
                d3 = g.Sum(x => x.D3),
                d4 = g.Sum(x => x.D4),
                d5 = g.Sum(x => x.D5),
                d6 = g.Sum(x => x.D6),
                d7_14 = g.Sum(x => x.D7_14),
                d15_30 = g.Sum(x => x.D15_30),
                d30 = g.Sum(x => x.D30)
            });
            //每个部门总共有多少个服务单
            var superVisorCount = await (UnitWork.Find<ServiceOrder>(so => so.VestInOrg == 1 && so.Status == 2 && so.ServiceWorkOrders.Count() > 0)
                       .WhereIf(req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null, so => so.CreateTime >= req.QryCreateTimeFrom && so.CreateTime < req.QryCreateTimeTo.Value.AddDays(1))
                       .Select(x => new { x.Id, x.Supervisor }).Distinct()
                       .GroupBy(x => x.Supervisor).Select(g => new
                       {
                           //deptName = _userManagerApp.GetUserOrgInfo(null, g.Key).Result?.OrgName,
                           superVisor = g.Key,
                           dCount = g.Count()
                       })).ToListAsync();
            var deptGroupCount = superVisorCount.Select(x => new
            {
                x.superVisor,
                x.dCount,
                deptName = _userManagerApp.GetUserOrgInfo(null, x.superVisor).Result?.OrgName
            });
            var d2 = deptGroupCount.GroupBy(d => d.deptName).Select(g => new
            {
                dept = g.Key,
                dcount = g.Sum(x=>x.dCount)
            });
            //总数量
            var totalCount = await UnitWork.Find<ServiceOrder>(so => so.VestInOrg == 1 && so.Status == 2 && so.ServiceWorkOrders.Count() > 0)
                       .WhereIf(req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null, so => so.CreateTime >= req.QryCreateTimeFrom && so.CreateTime < req.QryCreateTimeTo.Value.AddDays(1))
                       .Select(x => x.Id).Distinct().CountAsync();
            var depts = new string[] { "SQ", "S7", "S12", "S14", "S15", "S29", "S36", "S37", "S32", "S20", "E3" }; //规定要查看的部门
            var data = from d in depts
                       join a in d1 on d equals a.dept into temp1
                       from a in temp1.DefaultIfEmpty()
                       join b in d2 on a?.dept equals b.dept into temp2
                       from b in temp2.DefaultIfEmpty()
                       select new
                       {
                           dept = d,
                           dcount = b != null ? b.dcount : 0,
                           dPer = b != null ? ((decimal)b.dcount / totalCount).ToString("P2") : "0.00%",
                           d1 = a != null ? a.d1 : 0,
                           d1Per = a != null ? ((decimal)a.d1 / b.dcount).ToString("P2") : "0.00%",
                           d2 = a != null ? a.d2 : 0,
                           d2Per = a != null ? ((decimal)a.d2 / b.dcount).ToString("P2") : "0.00%",
                           d3 = a != null ? a.d3 : 0,
                           d3Per = a != null ? ((decimal)a.d3 / b.dcount).ToString("P2") : "0.00%",
                           d4 = a != null ? a.d4 : 0,
                           d4Per = a != null ? ((decimal)a.d4 / b.dcount).ToString("P2") : "0.00%",
                           d5 = a != null ? a.d5 : 0,
                           d5Per = a != null ? ((decimal)a.d5 / b.dcount).ToString("P2") : "0.00%",
                           d6 = a != null ? a.d6 : 0,
                           d6Per = a != null ? ((decimal)a.d6 / b.dcount).ToString("P2") : "0.00%",
                           d7_14 = a != null ? a.d7_14 : 0,
                           d7_14Per = a != null ? ((decimal)a.d7_14 / b.dcount).ToString("P2") : "0.00%",
                           d15_30 = a != null ? a.d15_30 : 0,
                           d15_30Per = a != null ? ((decimal)a.d15_30 / b.dcount).ToString("P2") : "0.00%",
                           d30 = a != null ? a.d30 : 0,
                           d30Per = a != null ? ((decimal)a.d30 / b.dcount).ToString("P2") : "0.00%",
                           unFinish = (b != null && a != null) ? b.dcount - a.dFinishCount : 0,
                           unFinishPer = (b != null && a != null) ? ((decimal)(b.dcount - a.dFinishCount) / b.dcount).ToString("P2") : "0.00%"
                       };

            result.Data = data;
            result.Count = totalCount;

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServerCallEfficiencyTest(QueryServiceOrderListReq req)
        {
            var result = new TableData();
            //查询服务单和工单数据
            var query = await (from so in UnitWork.Find<ServiceOrder>(null)
                        .WhereIf(req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null, s => s.CreateTime >= req.QryCreateTimeFrom && s.CreateTime < req.QryCreateTimeTo.Value.AddDays(1))
                               join sw in UnitWork.Find<ServiceWorkOrder>(null)
                               on so.Id equals sw.ServiceOrderId
                               //类型是服务单,状态是已确认
                               where so.VestInOrg == 1 && so.Status == 2
                               group new { so, sw } by so.Id into g
                               select new
                               {
                                   ServiceOrderId = g.Key, //服务单id
                                   Status = g.Min(x => x.sw.Status), //工单状态
                                   StartTime = g.Min(x => x.so.CreateTime), //服务单创建时间为开始时间
                                   EndTime = g.Max(x => x.sw.CompleteDate), //工单中最大的时间为结束时间
                                   SuperVisor = g.Max(x => x.so.Supervisor) //售后主管
                               }).ToListAsync();
            //按售后主管进行分组统计
            var query2 = query.GroupBy(q => q.SuperVisor).Select(g => new ProcessingEfficiency
            {
                SuperVisor = g.Key,
                FinishCount = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null),//服务单和工单是一对多关系,如果工单中最小的状态都大于等于7,说明这个服务单下所有的工单都是已完成的,这个服务单才算完成(服务单对应的工单只要有一个未完成,整个服务单就算未完成,工单没有完工时间的,也算未完成)
                UnFinishCount = g.Count() - g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null),
                D1 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays <= 1),
                D2 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays > 1 && (x.EndTime - x.StartTime).Value.TotalDays <= 2),
                D3 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays > 2 && (x.EndTime - x.StartTime).Value.TotalDays <= 3),
                D4 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays > 3 && (x.EndTime - x.StartTime).Value.TotalDays <= 4),
                D5 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays > 4 && (x.EndTime - x.StartTime).Value.TotalDays <= 5),
                D6 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays > 5 && (x.EndTime - x.StartTime).Value.TotalDays <= 6),
                D7_14 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays > 6 && (x.EndTime - x.StartTime).Value.TotalDays <= 14),
                D15_30 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays > 14 && (x.EndTime - x.StartTime).Value.TotalDays <= 30),
                D30 = g.Count(x => x.Status != null && x.Status.Value >= 7 && x.EndTime != null && (x.EndTime - x.StartTime).Value.TotalDays > 30)
            });
            //将售后主管转换为所在部门
            var query3 = query2.ToList();
            query3.ForEach(f => f.Dept = _userManagerApp.GetUserOrgInfo(null, f.SuperVisor).Result?.OrgName);
            //将同一部门下的数据合并
            var query4 = query3.GroupBy(f => f.Dept).Select(g => new
            {
                dept = g.Key,
                Count = g.Sum(x => x.FinishCount) + g.Sum(x => x.UnFinishCount),
                FinishCount = g.Sum(x => x.FinishCount),
                UnFinishCount = g.Sum(x => x.UnFinishCount),
                d1 = g.Sum(x => x.D1),
                d2 = g.Sum(x => x.D2),
                d3 = g.Sum(x => x.D3),
                d4 = g.Sum(x => x.D4),
                d5 = g.Sum(x => x.D5),
                d6 = g.Sum(x => x.D6),
                d7_14 = g.Sum(x => x.D7_14),
                d15_30 = g.Sum(x => x.D15_30),
                d30 = g.Sum(x => x.D30)
            });

            var depts = new string[] { "SQ", "S7", "S12", "S14", "S15", "S29", "S36", "S37", "S32", "S20", "E3" }; //规定要查看的部门
            var data = from d in depts
                       join q in query4 on d equals q.dept into temp
                       from t in temp.DefaultIfEmpty()
                       select new
                       {
                           dept = d,
                           dcount = t != null ? t.Count : 0,
                           dPer = t != null ? ((decimal)t.Count / query.Count).ToString("P2") : "0.00%",
                           d1 = t != null ? t.d1 : 0,
                           d1Per = t != null ? ((decimal)t.d1 / t.Count).ToString("P2") : "0.00%",
                           d2 = t != null ? t.d2 : 0,
                           d2Per = t != null ? ((decimal)t.d2 / t.Count).ToString("P2") : "0.00%",
                           d3 = t != null ? t.d3 : 0,
                           d3Per = t != null ? ((decimal)t.d3 / t.Count).ToString("P2") : "0.00%",
                           d4 = t != null ? t.d4 : 0,
                           d4Per = t != null ? ((decimal)t.d4 / t.Count).ToString("P2") : "0.00%",
                           d5 = t != null ? t.d5 : 0,
                           d5Per = t != null ? ((decimal)t.d5 / t.Count).ToString("P2") : "0.00%",
                           d6 = t != null ? t.d6 : 0,
                           d6Per = t != null ? ((decimal)t.d6 / t.Count).ToString("P2") : "0.00%",
                           d7_14 = t != null ? t.d7_14 : 0,
                           d7_14Per = t != null ? ((decimal)t.d7_14 / t.Count).ToString("P2") : "0.00%",
                           d15_30 = t != null ? t.d15_30 : 0,
                           d15_30Per = t != null ? ((decimal)t.d15_30 / t.Count).ToString("P2") : "0.00%",
                           d30 = t != null ? t.d30 : 0,
                           d30Per = t != null ? ((decimal)t.d30 / t.Count).ToString("P2") : "0.00%",
                           unFinish = t != null ? t.UnFinishCount : 0,
                           unFinishPer = t != null ? ((decimal)t.UnFinishCount / t.Count).ToString("P2") : "0.00%"
                       };

            result.Data = data;
            result.Count = data.Count();

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="deptName"></param>
        /// <param name="timediff"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrderInfo(string deptName,string timediff)
        {
            var result = new TableData();

            //查询服务单中所有主管的名字
            var supervisorNames = await UnitWork.Find<ServiceOrder>(null).Select(s => s.Supervisor).Distinct().ToListAsync();

            //根据姓名获取用户部门名称
            Func<IEnumerable<string>, Task<List<UserResp>>> GetOrgName = async x =>
                       await (from u in UnitWork.Find<User>(null)
                              join r in UnitWork.Find<Relevance>(null) on u.Id equals r.FirstId
                              join o in UnitWork.Find<Repository.Domain.Org>(null) on r.SecondId equals o.Id
                              where x.Contains(u.Name) && r.Key == Define.USERORG
                              orderby o.CascadeId descending
                              select new UserResp { Name = u.Name, OrgName = o.Name, CascadeId = o.CascadeId }).ToListAsync();
            var supervisorInfos = (await GetOrgName(supervisorNames)).Select(x => new ProcessingEfficiency { Dept = x.OrgName, SuperVisor = x.Name });

            //筛选出查询部门主管的名字
            var superVisors = supervisorInfos.Where(s => s.Dept == deptName).Select(s => s.SuperVisor).ToList();
            var param1 = "";
            if(superVisors!=null && superVisors.Count() > 0)
            {
                var object1 = "";
                foreach (var item in superVisors)
                {
                    object1 += $",'{item}'";
                }
                param1 = $" and so.supervisor in ({object1.Substring(1)})";
            }
            else
            {
                param1 = " and true = false";
            }

            string sql = $@"select t.Id
                            from(
	                            select so.Id,
	                            min(sw.Status) as status,
	                            min(so.CreateTime) as starttime,
	                            max(sw.CompleteDate) as endtime,
	                            max(so.Supervisor) as supervisor
	                            from serviceorder as so
	                            join serviceworkorder as sw 
	                            on so.Id = sw.ServiceOrderId
	                            where so.VestInOrg = 1
	                            and so.status = 2
                                {param1}
	                            group by so.Id
                            ) t where 1 = 1";
            var timeinterval = await UnitWork.Find<Category>(c => c.TypeId == "SYS_efficiency" && c.DtValue == timediff).Select(c => c.DtValue).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(timeinterval) && timeinterval != "0")
            {
                sql += " and t.status >= 7";
                if (!string.IsNullOrWhiteSpace(timeinterval.Split(':')[0]))
                {
                    var startPoint = timeinterval.Split(':')[0];
                    sql += $" and timestampdiff(minute,t.starttime,t.endtime) > 24*60*{startPoint}";
                }
                if (!string.IsNullOrWhiteSpace(timeinterval.Split(':')[1]))
                {
                    var endPoint = timeinterval.Split(':')[1];
                    sql += $" and timestampdiff(minute,t.starttime,t.endtime) <= 24*60*{endPoint}";
                }
            }
            else if (timeinterval == "0")
            {
                sql += " and t.status < 7 or t.endtime is null";
            }

            var parameters = new List<object>();
            var finishData = _dbExtension.GetObjectDataFromSQL<ServiceOrderData>(sql, parameters.ToArray(), typeof(Nsap4ServeDbContext))?.ToList();

            result.Data = finishData;
            result.Count = finishData.Count();

            return result;
        }

        /// <summary>
        /// 统计未处理的服务单中,从建单到现在经过了多长时间
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetUnFinishServiceCallProcessingTime(QueryServiceOrderListReq req)
        {
            var result = new TableData();
            //查看未完成的服务单
            var serviceData = await (from so in UnitWork.Find<ServiceOrder>(s => s.Status == 2 && s.ServiceWorkOrders.Any(sw => sw.Status < 7) && s.FromId != 8)
                                     .WhereIf(!string.IsNullOrWhiteSpace(req.QryVestInOrg), s => s.VestInOrg == int.Parse(req.QryVestInOrg))
                                     .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), s => s.Supervisor == req.QrySupervisor)
                                     .WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), s => s.ServiceWorkOrders.Any(sw => sw.CurrentUser == req.QryTechName))
                                     select new
                                     {
                                         ProcessingTime = (DateTime.Now - so.CreateTime).Value.TotalDays
                                     }).ToListAsync();
            //在字典维护的时间区间
            var dateList = await UnitWork.Find<Category>(c => c.TypeId == "SYS_TimeInterval").OrderBy(x => x.SortNo).Select(x => new { x.Name, x.DtValue }).ToListAsync();
            var sects = dateList.Select(d => new
            {
                sectionName = d.Name,
                minValue = d.DtValue.Split('-')[0],
                maxValue = d.DtValue.Split('-')[1]
            });
            //按处理时间区间进行分类
            var processingData = serviceData.GroupBy(q =>
                    sects.FirstOrDefault(s => (!string.IsNullOrWhiteSpace(s.minValue) ? double.Parse(s.minValue) < q.ProcessingTime : true) && (!string.IsNullOrWhiteSpace(s.maxValue) ? q.ProcessingTime <= double.Parse(s.maxValue) : true))?.sectionName
                )
                .Select(g => new
                {
                    g.Key,
                    count = g.Count(),
                });
            var totalCount = serviceData.Count();
            //左连接,没有则为0
            var data = from d in dateList
                       join p in processingData on d.Name equals p.Key into temp
                       from t in temp.DefaultIfEmpty()
                       select new
                       {
                           day = d.Name,
                           count = t == null ? 0 : t.count,
                           per = t == null ? "0.00%" : ((decimal)t.count / totalCount).ToString("P2"),
                       };

            result.Data = data;
            result.Count = totalCount;

            return result;
        }

        /// <summary>
        /// 提交未完工原因
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task ServiceUnCompletedReason(AddServiceUnCompletedReasonReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            var user = loginContext.User;
            var currentTime = DateTime.Now;

            using var tran = UnitWork.GetDbContext<ServiceUnCompletedReasonHistory>().Database.BeginTransaction();
            try
            {
                //未完工原因历史记录
                var history = new Repository.Domain.Serve.ServiceUnCompletedReasonHistory
                {
                    ServiceOrderId = req.ServiceOrderId,
                    FroTechnicianId = user.Id,
                    FroTechnicianName = user.Name,
                    Content = req.Content,
                    CreateTime = currentTime,
                    CreateUserId = user.Id
                };

                await UnitWork.AddAsync(history);
                await UnitWork.SaveAsync();

                //未完工原因明细
                var content = req.Content.Replace("\n", "");
                var reasons = content.Trim().Split(';').Where(x => !string.IsNullOrWhiteSpace(x));
                var reasonsInfo = await UnitWork.Find<Category>(c => c.TypeId == "SYS_UnCompletedReason").Where(c => !string.IsNullOrWhiteSpace(c.DtValue)).Select(x => new { x.Name, x.DtValue }).ToListAsync();
                var details = reasons.Select(x => new Repository.Domain.Serve.ServiceUnCompletedReasonDetail
                {
                    ServiceOrderId = req.ServiceOrderId,
                    ServiceUnCompletedReasonHistoryId = history.Id,
                    UnCompletedReasonId = reasonsInfo.FirstOrDefault(r => r.Name == x.Split('.')[1])?.DtValue ?? "",
                    UnCompletedReasonName = x.Split('.')[1],
                    CreateTime = currentTime,
                    CreateUserId = user.Id
                });
                await UnitWork.BatchAddAsync<Repository.Domain.Serve.ServiceUnCompletedReasonDetail>(details.ToArray());
                await UnitWork.SaveAsync();

                tran.Commit();
            }
            catch (Exception ex)
            {
                tran.Rollback();
                throw ex;
            }
        }

        /// <summary>
        /// 根据服务单id获取未完工原因历史记录
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetUnCompletedReasonHistory(int serviceOrderId)
        {
            var result = new TableData();
            var data = await (from h in UnitWork.Find<Repository.Domain.Serve.ServiceUnCompletedReasonHistory>(s => s.ServiceOrderId == serviceOrderId).Include(s => s.ServiceUnCompletedReasonDetails)
                              select new
                              {
                                  h.CreateTime,
                                  h.FroTechnicianName,
                                  Content = h.ServiceUnCompletedReasonDetails.Select(x => x.UnCompletedReasonName),
                              }).ToListAsync();

            result.Data = data.OrderByDescending(d => d.CreateTime);
            result.Count = data.Count();

            return result;
        }

        /// <summary>
        /// 查看未完工原因统计
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetUnCompletedReasonInfo(QueryServiceOrderListReq req)
        {
            var result = new TableData();

            //所有未完工的服务
            var unCompletedService = UnitWork.Find<ServiceOrder>(s => s.Status == 2 && s.ServiceWorkOrders.Any(sw => sw.Status < 7))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryVestInOrg), s => s.VestInOrg == int.Parse(req.QryVestInOrg))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), s => s.Supervisor == req.QrySupervisor)
                .WhereIf(req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null, s => s.CreateTime >= req.QryCreateTimeFrom && s.CreateTime < req.QryCreateTimeTo.Value.AddDays(1))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), s => s.ServiceWorkOrders.Any(sw => sw.CurrentUser == req.QryTechName))
                .Select(s => s.Id);
            var totalCount = await unCompletedService.CountAsync();
            //在字典维护的未完工原因
            var reasonsInfo = await UnitWork.Find<Category>(c => c.TypeId == "SYS_UnCompletedReason").Where(c => !new string[] { "0", "13", "14" }.Contains(c.DtValue)).Select(x => new { x.Name, x.SortNo }).ToListAsync();
            var reasons = reasonsInfo.Select(r => r.Name);

            #region 有未完工原因的,并且未完工原因是在字典中维护的
            //一个服务单可填写多次未完工原因,统计时取最近的一次
            var serviceLastRecord = from h in UnitWork.Find<ServiceUnCompletedReasonHistory>(null)
                                    join u in unCompletedService on h.ServiceOrderId equals u
                                    group new { h, u } by h.ServiceOrderId into g
                                    select new
                                    {
                                        ServiceOrderId = g.Key,
                                        Id = g.Max(x => x.h.Id)
                                    };

            var query1 = from h1 in UnitWork.Find<ServiceUnCompletedReasonHistory>(null)
                         join q in serviceLastRecord on h1.Id equals q.Id
                         join d in UnitWork.Find<ServiceUnCompletedReasonDetail>(s => reasons.Contains(s.UnCompletedReasonName))
                         on new { h1.ServiceOrderId, ServiceUnCompletedReasonHistoryId = h1.Id } equals new { d.ServiceOrderId, d.ServiceUnCompletedReasonHistoryId }
                         group new { h1, d } by d.UnCompletedReasonName into g2
                         select new
                         {
                             reason = g2.Key,
                             count = g2.Count()
                         };

            var data = (from r in reasonsInfo
                        join q in query1 on r.Name equals q.reason into temp
                        from t in temp.DefaultIfEmpty()
                        select new
                        {
                            r.Name,
                            Count = t == null ? 0 : t.count,
                            Per = t == null ? "0.00%" : ((decimal)t.count / totalCount).ToString("P2"),
                            r.SortNo,
                        }).ToList();
            #endregion

            #region 有未完工原因的,未完工原因不在字典中维护的,归类为其他
            var otherCount = (from h1 in UnitWork.Find<ServiceUnCompletedReasonHistory>(null)
                              join q in serviceLastRecord on h1.Id equals q.Id
                              join d in UnitWork.Find<ServiceUnCompletedReasonDetail>(s => !reasons.Contains(s.UnCompletedReasonName))
                              on new { h1.ServiceOrderId, ServiceUnCompletedReasonHistoryId = h1.Id } equals new { d.ServiceOrderId, d.ServiceUnCompletedReasonHistoryId }
                              select d.ServiceOrderId).Distinct().Count();
            data.Add(new { Name = "其他", Count = otherCount, Per = (totalCount == 0 ? "0.00%" : ((decimal)otherCount / totalCount).ToString("P2")), SortNo = 13 });
            #endregion

            #region 没有未完工原因的,归类为未填写
            var notWriteCount = totalCount - serviceLastRecord.Count();
            data.Add(new { Name = "未填写", Count = notWriteCount, Per = totalCount == 0 ? "0.00%" : ((decimal)notWriteCount / totalCount).ToString("P2"), SortNo = 14 });
            #endregion

            result.Data = data.OrderBy(d => d.SortNo);
            result.Count = totalCount;

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
                        where b.AllowOrNot==0
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
                         .WhereIf(req.QryMaterialTypes != null && req.QryMaterialTypes.Count > 0, q => req.QryMaterialTypes.Contains(q.a.MaterialCode == "无序列号" ? "无序列号" : q.a.MaterialCode.Substring(0, q.a.MaterialCode.IndexOf("-"))))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), q => q.b.Supervisor.Contains(req.QrySupervisor))
                         .WhereIf(!string.IsNullOrWhiteSpace(req.QryMaterialCode), q => q.a.MaterialCode.Contains(req.QryMaterialCode))
                         .Where(q => q.b.U_SAP_ID != null && q.b.Status == 2 && q.a.FromType != 2 && q.b.VestInOrg == 1);

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
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("呼叫中心-派送服务ID")))
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
            var newestOrder = await UnitWork.Find<ServiceOrder>(s => s.CustomerId.Equals(code) && !string.IsNullOrWhiteSpace(s.CreateTime.ToString())).Include(s => s.ServiceWorkOrders).OrderByDescending(s => s.CreateTime)
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
                    s.CreateTime,
                    s.VestInOrg,
                    FromTheme = s.ServiceWorkOrders.FirstOrDefault().FromTheme,
                    WorkOrderStatus = s.ServiceWorkOrders.FirstOrDefault().Status,
                    CurrentUser = s.ServiceWorkOrders.FirstOrDefault().CurrentUser,
                    IsWarning = ((TimeSpan)(DateTime.Now - s.CreateTime)).Days <= 5 ? true : false,
                    Day = 5
                })
                .Skip(0).Take(10).ToListAsync();
            var newestNotCloseOrder = await UnitWork.Find<ServiceOrder>(s => s.CustomerId.Equals(code) && s.Status == 2 && !string.IsNullOrWhiteSpace(s.CreateTime.ToString()) && s.ServiceWorkOrders.Any(o => o.Status < 7)).Include(s => s.ServiceWorkOrders).OrderByDescending(s => s.CreateTime)
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
                    s.CreateTime,
                    s.VestInOrg,
                    FromTheme = s.ServiceWorkOrders.FirstOrDefault().FromTheme,
                    WorkOrderStatus = s.ServiceWorkOrders.FirstOrDefault().Status,
                    CurrentUser = s.ServiceWorkOrders.FirstOrDefault().CurrentUser,
                    IsWarning = ((TimeSpan)(DateTime.Now - s.CreateTime)).Days <= 5 ? true : false,
                    Day = 5
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
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpDateServiceOrderStatus(SendServiceOrderMessageReq req)
        {
            var user = _auth.GetCurrentUser().User;
            var obj = await UnitWork.Find<ServiceOrder>(u => u.Id == Convert.ToInt32(req.ServiceOrderId) && u.Status < 7).FirstOrDefaultAsync();
            if (obj == null)
            {
                throw new Exception("服务单已完成不可撤销。");
            }
            var num = await UnitWork.Find<Quotation>(q => q.ServiceOrderId == obj.Id).CountAsync();
            if (num > 0)
            {
                throw new Exception("该服务单已领料不可撤销。");
            }
            TimeSpan timeSpan = DateTime.Now - Convert.ToDateTime(obj.CreateTime);
            var result = new TableData();
            if (timeSpan.Days > 5)
            {
                throw new Exception("服务单已超出撤销时间不可撤销。");
            }
            else
            {
                await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id == Convert.ToInt32(req.ServiceOrderId), u => new ServiceOrder { Status = 3, Remark = req.Remark });
                await UnitWork.DeleteAsync<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(Convert.ToInt32(req.ServiceOrderId)));
                await UnitWork.SaveAsync();
                //保存日志
                await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq
                {
                    ServiceOrderId = Convert.ToInt32(req.ServiceOrderId),
                    Action = $"{user.Name}执行撤销操作，撤销ID为{ Convert.ToInt32(req.ServiceOrderId)}的服务单",
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

            List<int> status = new List<int>();
            if (!string.IsNullOrWhiteSpace(req.QryStateList))
            {
                var num = req.QryStateList.Split(",");
                status = Array.ConvertAll(num, int.Parse).ToList();
            }
            var query = UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceWorkOrders)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.ServiceWorkOrders.Any(a => a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.ServiceWorkOrders.Any(a => a.Status.Equals(Convert.ToInt32(req.QryState))))
                .WhereIf(status.Count() > 0, q => q.ServiceWorkOrders.Any(a => status.Contains(a.Status.Value)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceMode), q => q.ServiceWorkOrders.Any(a => a.ServiceMode.Equals(Convert.ToInt32(req.QryServiceMode))))
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
            //取消售后主管统计
            //var list1 = await query.Where(g => !string.IsNullOrWhiteSpace(g.Supervisor)).GroupBy(g => new { g.SupervisorId, g.Supervisor }).Select(q => new ServiceOrderReportResp
            //{
            //    StatId = q.Key.SupervisorId,
            //    StatName = q.Key.Supervisor,
            //    ServiceCnt = q.Count()
            //}).Where(w => w.ServiceCnt > 10).OrderByDescending(s => s.ServiceCnt).Skip(0).Take(20).ToListAsync();
            //resultlist.Add(new ServerOrderStatListResp { StatType = "Supervisor", StatList = list1 });

            var list2 = await query.Where(g => !string.IsNullOrWhiteSpace(g.SalesMan)).GroupBy(g => new { g.SalesManId, g.SalesMan }).Select(q => new ServiceOrderReportResp
            {
                StatId = q.Key.SalesManId,
                StatName = q.Key.SalesMan,
                ServiceCnt = q.Count()
            }).OrderByDescending(s => s.ServiceCnt).Skip(0).Take(20).ToListAsync();
            resultlist.Add(new ServerOrderStatListResp { StatType = "SalesMan", StatList = list2 });

            //var problemTypes = await query.Select(s => s.ServiceWorkOrders.Select(s => s.ProblemType).ToList()).ToListAsync();
            //var l3 = new List<ProblemType>();
            //foreach (var problemType in problemTypes)
            //{
            //    l3.AddRange(problemType);
            //}
            //var list3 = l3.Where(g => g != null).GroupBy(g => new { g.Id, g.Name }).Select(q => new ServiceOrderReportResp
            //{
            //    StatId = q.Key.Id,
            //    StatName = q.Key.Name,
            //    ServiceCnt = q.Count()
            //}).OrderByDescending(s => s.ServiceCnt).Skip(0).Take(20).ToList();
            //resultlist.Add(new ServerOrderStatListResp { StatType = "ProblemType", StatList = list3 });

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
        /// 根据时间段筛选问题类型数据,按问题类型、月份分组统计
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetProblemType(QueryServiceOrderListReq req)
        {
            var result = new TableData();

            DateTime startDate; //开始时间
            DateTime endDate; //结束时间
            if (req.QryCreateTimeFrom != null && req.QryCreateTimeTo != null)
            {
                startDate = req.QryCreateTimeFrom.Value;
                endDate = req.QryCreateTimeTo.Value;
            }
            else
            {
                startDate = DateTime.Now;
                endDate = DateTime.Now;
            }

            //计算两个日期的月份差
            var diffMonths = (endDate.Year - startDate.Year) * 12 + (endDate.Month - startDate.Month) + 1;
            //根据月份差生成连续月份
            var months = new List<string>();
            for (int i = 0; i < diffMonths; i++)
            {
                months.Add(startDate.AddMonths(i).ToString("yyyy-MM"));
            }

            //查询所有的问题类型
            var problemTypes = await UnitWork.Find<ProblemType>(null).Select(x => new { x.Id, x.Name }).Distinct().ToListAsync();
            //生成笛卡尔积(问题类型,月份)
            var problemMonth = from p in problemTypes
                               from m in months
                               select new
                               {
                                   ProblemTypeId = p.Id,
                                   ProblemTypeName = p.Name,
                                   Months = m
                               };
            //根据时间段查询问题类型,按问题类型、年月分组统计
            var sql = @"select sw.ProblemTypeId,date_format(sw.CreateTime,'%Y-%m') as months,count(*) as Num
	                    from serviceworkorder as sw 
	                    where sw.CreateTime >= {0}
	                    and sw.CreateTime < {1}
	                    and sw.ProblemTypeId is not null and sw.ProblemTypeId <> ''
	                    group by sw.ProblemTypeId,months";
            var parameters = new List<object>();
            parameters.Add(startDate);
            parameters.Add(endDate.AddMonths(1));
            var problems = _dbExtension.GetObjectDataFromSQL<ProblemTypeMonth>(sql, parameters.ToArray(), typeof(Nsap4ServeDbContext))?.ToList();

            //问题类型没有,统计则补0
            var data = from pm in problemMonth
                       join p in problems on new { pm.ProblemTypeId, pm.Months } equals new { p.ProblemTypeId, p.Months }
                       into temp
                       from t in temp.DefaultIfEmpty()
                       select new
                       {
                           pm.ProblemTypeId,
                           pm.ProblemTypeName,
                           pm.Months,
                           Num = t == null ? 0 : t.Num
                       };

            result.Data = data.OrderBy(d => d.ProblemTypeId).ThenBy(d => d.Months);
            result.Count = data.Sum(x => x.Num);

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
            var tUsers = await UnitWork.Find<AppUserMap>(null).WhereIf(!loginContext.Roles.Any(a => a.Name == "管理员" || a.Name == "呼叫中心"), w => (w.AppUserRole == 1 || w.AppUserRole == 2 || w.AppUserRole == 3 || w.AppUserRole == 6)).ToListAsync();
            var ids = tUsers.Select(u => u.UserID);
            var userIds = _revelanceApp.Get(Define.USERORG, false, orgs);
            if (!loginContext.Roles.Any(a => a.Name == "管理员" || a.Name == "呼叫中心"))
            {
                ids = userIds.Intersect(tUsers.Select(u => u.UserID));
            }
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

            var Model = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.ToString() == req.ServiceOrderId && req.QryMaterialTypes.Contains(s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")))).Select(s => s.Id);
            var ids = await Model.ToListAsync();
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => ids.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUser = u.User.Name,
                CurrentUserNsapId = u.User.Id,
                CurrentUserId = req.CurrentUserId,
                Status = 2,
                AcceptTime = DateTime.Now
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
                .Where(q => q.SalesManId.Equals(loginContext.User.Id) && ids.Contains(q.Id) && q.Status == 2);

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
        /// 一键重派
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task OneKeyResetServiceOrder(OneKeyResetServiceOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //判断当前服务单是否已重派过
            var isExist = await UnitWork.Find<ServiceOrderLog>(w => w.ServiceOrderId == req.serviceOrderId && w.ActionType == "一键重派").FirstOrDefaultAsync() == null ? false : true;
            if (isExist)
            {
                throw new CommonException("您已重派过该服务单，请勿重复操作", 60019);
            }
            var serviceOrderObj = await UnitWork.Find<ServiceOrder>(s => s.Id.Equals(req.serviceOrderId)).FirstOrDefaultAsync();
            var OrderTakeType = 0;
            if (serviceOrderObj.VestInOrg == 3)
            {
                OrderTakeType = 2;
            }
            var appUserId = (await UnitWork.Find<AppUserMap>(w => w.UserID == loginContext.User.Id).FirstOrDefaultAsync())?.AppUserId;
            //重置工单状态为已排配
            await UnitWork.UpdateAsync<ServiceWorkOrder>(w => w.ServiceOrderId == req.serviceOrderId, u => new ServiceWorkOrder { Status = 2, OrderTakeType = OrderTakeType, BookingDate = null, VisitTime = null, ServiceMode = 0, CompletionReportId = string.Empty, TroubleDescription = string.Empty, ProcessDescription = string.Empty, IsCheck = 0, CompleteDate = null });
            //删除相对应的流程数据
            await UnitWork.DeleteAsync<ServiceFlow>(c => c.ServiceOrderId == req.serviceOrderId);
            var U_SAP_ID = serviceOrderObj.U_SAP_ID;
            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"呼叫中心{loginContext.User.Name}一键重派服务单{U_SAP_ID}理由：{req.Message}", ActionType = "一键重派", ServiceOrderId = req.serviceOrderId });
            await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = req.serviceOrderId, Content = $"呼叫中心{loginContext.User.Name}一键重派服务单{U_SAP_ID}理由：{req.Message}", AppUserId = (int)appUserId });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 取消待确认服务单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task CancelServiceOrder(OneKeyResetServiceOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //判断当前服务单是否已确认
            var isExist = await UnitWork.Find<ServiceOrder>(s => s.Id == req.serviceOrderId && s.Status == 2).FirstOrDefaultAsync() == null ? false : true;
            if (isExist)
            {
                throw new CommonException("已确认不可取消", 60019);
            }
            await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id == req.serviceOrderId, s => new ServiceOrder { Status = 3, Remark = req.Message });
            //保存日志
            await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq
            {
                ServiceOrderId = Convert.ToInt32(req.serviceOrderId),
                Action = $"{loginContext.User.Name}执行撤销操作，撤销ID为{ Convert.ToInt32(req.serviceOrderId)}的服务单",
                ActionType = "撤销操作",
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 业务员审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SalesApproval(OneKeyResetServiceOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginContext.User.Account == Define.USERAPP && req.AppUserId != null)
            {
                loginUser = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(req.AppUserId)).Include(u => u.User).Select(u => u.User).FirstOrDefaultAsync();
            }
            //判断当前服务单是否已确认
            var isExist = await UnitWork.Find<ServiceOrder>(s => s.Id == req.serviceOrderId && s.SalesManId.Equals(loginUser.Id)).FirstOrDefaultAsync() == null ? true : false;
            var isRole = loginContext.Roles.Any(c => c.Name.Contains("呼叫中心"));
            if (isExist && !isRole)
            {
                throw new CommonException("暂无此服务单审批权限", 60019);
            }
            if ((bool)req.IsReject)
            {
                var action= $"用户:{loginUser.Name}驳回服务单,理由：{req.Message}";
                await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id == req.serviceOrderId, s => new ServiceOrder { AllowOrNot = -1, Status = 3, Remark = action });
                await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = action, ActionType = "驳回服务单", ServiceOrderId = req.serviceOrderId });
            }
            else 
            {
                await UnitWork.UpdateAsync<ServiceOrder>(s => s.Id == req.serviceOrderId, s => new ServiceOrder { AllowOrNot =0});
                await _ServiceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"用户:{loginUser.Name}同意服务单", ActionType = "同意服务单", ServiceOrderId = req.serviceOrderId });
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取服务单日报信息
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetErpTechnicianDailyReport(int ServiceOrderId, string startDate, string endDate, string UserId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //string creater = string.Empty;
            //if (!string.IsNullOrEmpty(reimburseId))
            //{
            //    //获取该报销单的创建者Id
            //    creater = (await UnitWork.Find<ReimburseInfo>(w => w.Id == Convert.ToInt32(reimburseId)).FirstOrDefaultAsync()).CreateUserId;
            //}
            //获取当月的所有日报信息
            var dailyReports = (await UnitWork.Find<ServiceDailyReport>(w => w.ServiceOrderId == ServiceOrderId)
                .WhereIf(!string.IsNullOrEmpty(startDate), w => w.CreateTime.Value.Date >= Convert.ToDateTime(startDate).Date)
                .WhereIf(!string.IsNullOrEmpty(endDate), w => w.CreateTime.Value.Date <= Convert.ToDateTime(endDate).Date)
                .WhereIf(!string.IsNullOrEmpty(UserId), w => w.CreateUserId == UserId)
                .ToListAsync()).Select(s => new ReportDetail { CreateTime = s.CreateTime, MaterialCode = s.MaterialCode, ManufacturerSerialNumber = s.ManufacturerSerialNumber, TroubleCode = GetServiceTroubleAndSolution(s.TroubleDescription, "code"), TroubleDescription = GetServiceTroubleAndSolution(s.TroubleDescription, "description"), ProcessCode = GetServiceTroubleAndSolution(s.ProcessDescription, "code"), ProcessDescription = GetServiceTroubleAndSolution(s.ProcessDescription, "description") }).OrderByDescending(o => o.CreateTime).ToList();
            var dailyReportDates = dailyReports.OrderByDescending(o => o.CreateTime).Select(s => s.CreateTime?.Date.ToString("yyyy-MM-dd")).Distinct().ToList();

            var data = dailyReports.GroupBy(g => g.CreateTime?.Date).Select(s => new ReportResult { DailyDate = s.Key?.Date.ToString("yyyy-MM-dd"), ReportDetails = s.ToList() }).ToList();
            result.Data = new DailyReportResp { DailyDates = dailyReportDates, ReportResults = data.OrderBy(d => d.DailyDate).ToList() };
            return result;
        }

        /// <summary>
        /// 按时间段导出日报数据,字段包括(序列号、物料编码、呼叫主题、问题描述、解决方案)
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportDailyReport(DateTime startDate, DateTime endDate)
        {
            //var result = new TableData();

            //var dailyReports = from s in UnitWork.Find<ServiceOrder>(null)
            //                   join sw in UnitWork.Find<ServiceWorkOrder>(null) on s.Id equals sw.ServiceOrderId
            //                   join sr in UnitWork.Find<ServiceDailyReport>(null) on new { ServiceOrderId = s.Id, sw.ManufacturerSerialNumber, sw.MaterialCode } equals new { ServiceOrderId = sr.ServiceOrderId.Value, sr.ManufacturerSerialNumber, sr.MaterialCode }
            //                   where s.Status == 2 && s.VestInOrg == 1 && s.CreateTime >= startDate && s.CreateTime < endDate.AddDays(1)
            //                   select new
            //                   {
            //                       sr.ManufacturerSerialNumber,
            //                       sr.MaterialCode,
            //                       sw.FromTheme,
            //                       sr.TroubleDescription,
            //                       sr.ProcessDescription,
            //                       s.CreateTime
            //                   };

            //var data = (await dailyReports.OrderBy(x => x.CreateTime).ToListAsync()).Select(x => new DailyReportDetail
            //{
            //    ManufacturerSerialNumber = x.ManufacturerSerialNumber,
            //    MaterialCode = x.MaterialCode,
            //    FromTheme = string.Join(";", GetServiceFromTheme(x.FromTheme)),
            //    TroubleDescription = string.Join(";", GetServiceTroubleAndSolution(x.TroubleDescription)),
            //    ProcessDescription = string.Join(";", GetServiceTroubleAndSolution(x.ProcessDescription))
            //}).ToList();

            //result.Count = await dailyReports.CountAsync();

            var query = from a in UnitWork.Find<ServiceEvaluate>(null)
                        join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };

            var objs = query.Where(x => x.a.CommentDate >= startDate && x.a.CommentDate < endDate.AddDays(1)).Select(s => new 
            {
                ServiceOrderId = s.a.ServiceOrderId.Value, //服务单号
                CustomerId = s.a.CustomerId, //客户代码
                Cutomer = s.a.Cutomer, //客户名称
                Contact = s.a.Contact, //联系人
                CaontactTel = s.a.CaontactTel, //联系电话

                Technician = s.a.Technician, //技术员

                ResponseSpeed = s.a.ResponseSpeed, //响应速度
                SchemeEffectiveness = s.a.SchemeEffectiveness, //方案有效性
                ServiceAttitude = s.a.ServiceAttitude, //服务态度
                ProductQuality = s.a.ProductQuality, //产品质量

                ServicePrice = s.a.ServicePrice, //服务价格
                Comment = s.a.Comment, //客户意见或建议
                VisitPeople = s.a.VisitPeople, //回访人
                CreateTime = s.b.CreateTime.Value, //呼叫日期
                CommentDate = s.a.CommentDate.Value, //评价日期
            }).ToList();

            var data = objs.Select(s => new DailyReportDetail
            {
                ServiceOrderId = s.ServiceOrderId, //服务单号
                CustomerId = s.CustomerId, //客户代码
                Cutomer = s.Cutomer, //客户名称
                Contact = s.Contact, //联系人
                CaontactTel = s.CaontactTel, //联系电话

                Technician = s.Technician, //技术员

                ResponseSpeed = s.ResponseSpeed == null ? "" : GetEvaluate(s.ResponseSpeed.Value), //响应速度
                SchemeEffectiveness = s.SchemeEffectiveness == null ? "" : GetEvaluate(s.SchemeEffectiveness.Value), //方案有效性
                ServiceAttitude = s.ServiceAttitude == null ? "" : GetEvaluate(s.ServiceAttitude.Value), //服务态度
                ProductQuality = s.ProductQuality == null ? "" : GetEvaluate(s.ProductQuality.Value), //产品质量

                ServicePrice = s.ServicePrice == null ? "" : GetEvaluate(s.ServicePrice.Value), //服务价格
                Comment = s.Comment, //客户意见或建议
                VisitPeople = s.VisitPeople, //回访人
                CreateTime = s.CreateTime.ToString("yyyy-MM-dd HH:mm:ss"), //呼叫日期
                CommentDate = s.CommentDate.ToString("yyyy-MM-dd HH:mm:ss"), //评价日期
            });

            IExporter exporter = new ExcelExporter();
            var bytes = await exporter.ExportAsByteArray<DailyReportDetail>(data.ToList());
            return bytes;
        }

        public string GetEvaluate(int a)
        {
            if (a == 0)
            {
                return "未统计";
            }
            else if (a == 1)
            {
                return "非常差";
            }
            else if (a == 2)
            {
                return "差";
            }
            else if (a == 3)
            {
                return "一般";
            }
            else if (a == 4)
            {
                return "满意";
            }
            else if (a == 5)
            {
                return "非常满意";
            }

            return "";
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

            List<string> techName = new List<string>();
            List<int> status = new List<int>();
            if (!string.IsNullOrWhiteSpace(req.QryTechName))
                techName = req.QryTechName.Split(",").ToList();
            if (!string.IsNullOrWhiteSpace(req.QryStateList))
            {
                var num = req.QryStateList.Split(",");
                status = Array.ConvertAll(num, int.Parse).ToList();
            }
            #region MyRegion
            //var query1 = await (from a in UnitWork.Find<ServiceOrder>(c => c.CreateTime >= DateTime.Parse("2022-05-01 00:00:00") && c.CreateTime < DateTime.Parse("2022-08-01 00:00:00") && c.VestInOrg == 1)
            //                    join b in UnitWork.Find<ServiceDailyReport>(null) on a.Id equals b.ServiceOrderId
            //                    select new { a.U_SAP_ID, a.TerminalCustomerId, a.TerminalCustomer, a.CreateTime, b.MaterialCode, b.ManufacturerSerialNumber, b.CreaterName, b.TroubleDescription, b.ProcessDescription }).ToListAsync();
            //var list = new List<ServiceOrderExcelDto>();
            //foreach (var serviceOrder in query1)
            //{
            //    list.Add(new ServiceOrderExcelDto
            //    {
            //        U_SAP_ID = serviceOrder.U_SAP_ID,
            //        TerminalCustomer = serviceOrder.TerminalCustomer,
            //        TerminalCustomerId = serviceOrder.TerminalCustomerId,
            //        MaterialCode = serviceOrder.MaterialCode,
            //        ManufacturerSerialNumber = serviceOrder.ManufacturerSerialNumber,
            //        CurrentUser = serviceOrder.CreaterName,
            //        SubmitDate = serviceOrder.CreateTime,
            //        TroubleDescription =string.Join(",", GetServiceTroubleAndSolution(serviceOrder.TroubleDescription, "description")) ,
            //        ProcessDescription = string.Join(",", GetServiceTroubleAndSolution(serviceOrder.ProcessDescription, "description")),
            //    });
            //}
            //IExporter exporter = new ExcelExporter();
            //var bytes = await exporter.ExportAsByteArray(list);
            #endregion

            var ids = await UnitWork.Find<ServiceWorkOrder>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.Status.Equals(Convert.ToInt32(req.QryState)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryStateList), q => status.Contains(q.Status.Value))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceMode), q => q.ServiceMode.Equals(Convert.ToInt32(req.QryServiceMode)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ManufacturerSerialNumber.Contains(req.QryManufSN))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ProblemTypeId.Equals(req.QryProblemType))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromType), q => q.FromType.Equals(Convert.ToInt32(req.QryFromType)))
                //.WhereIf(!string.IsNullOrWhiteSpace(req.QryTechName), q => q.CurrentUser.Contains(req.QryTechName))
                .WhereIf(techName.Count > 0, q => techName.Contains(q.CurrentUser))
                //.WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromTheme), q => q.FromTheme.Contains(req.QryFromTheme))
                .WhereIf(req.CompleteDate != null, q => q.CompleteDate > req.CompleteDate)
                .WhereIf(req.EndCompleteDate != null, q => q.CompleteDate < Convert.ToDateTime(req.EndCompleteDate).AddDays(1))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryMaterialCode), q => q.MaterialCode.Contains(req.QryMaterialCode))
                .OrderBy(s => s.CreateTime).Select(s => s.ServiceOrderId).Distinct().ToListAsync();

            var query = UnitWork.Find<ServiceOrder>(null)
                .Include(s => s.ServiceWorkOrders).ThenInclude(c => c.ProblemType)
                .Include(a => a.ServiceWorkOrders).ThenInclude(b => b.Solution)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryU_SAP_ID), q => q.U_SAP_ID.Equals(Convert.ToInt32(req.QryU_SAP_ID)))
                //.WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceWorkOrderId), q => q.ServiceWorkOrders.Any(a => a.Id.Equals(Convert.ToInt32(req.QryServiceWorkOrderId))))
                //.WhereIf(!string.IsNullOrWhiteSpace(req.QryState), q => q.ServiceWorkOrders.Any(a => a.Status.Equals(Convert.ToInt32(req.QryState))))
                //.WhereIf(status.Count > 0, q => q.ServiceWorkOrders.Any(a => status.Contains(a.Status.Value)))
                //.WhereIf(!string.IsNullOrWhiteSpace(req.QryServiceMode), q => q.ServiceWorkOrders.Any(a => a.ServiceMode.Equals(Convert.ToInt32(req.QryServiceMode))))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryCustomer), q => q.CustomerId.Contains(req.QryCustomer) || q.CustomerName.Contains(req.QryCustomer))
                //.WhereIf(!string.IsNullOrWhiteSpace(req.QryManufSN), q => q.ServiceWorkOrders.Any(a => a.ManufacturerSerialNumber.Contains(req.QryManufSN)))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryRecepUser), q => q.RecepUserName.Contains(req.QryRecepUser))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryVestInOrg), q => q.VestInOrg == Convert.ToInt32(req.QryVestInOrg))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QrySupervisor), q => q.Supervisor.Contains(req.QrySupervisor))
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryAllowOrNot.ToString()), q => q.AllowOrNot == req.QryAllowOrNot)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QrySalesMan), q => q.SalesMan == req.QrySalesMan)
                .WhereIf(!string.IsNullOrWhiteSpace(req.QryFromId), q => q.FromId == Convert.ToInt32(req.QryFromId))
                .WhereIf(string.IsNullOrWhiteSpace(req.QryFromId) && req.QryVestInOrg == "1", q => q.FromId != 8)//服务呼叫列表排除ECN
                                                                                                                 //.WhereIf(!string.IsNullOrWhiteSpace(req.QryProblemType), q => q.ServiceWorkOrders.Any(a => a.ProblemTypeId.Equals(req.QryProblemType)))
                .WhereIf(!(req.QryCreateTimeFrom is null || req.QryCreateTimeTo is null), q => q.CreateTime >= req.QryCreateTimeFrom && q.CreateTime < Convert.ToDateTime(req.QryCreateTimeTo).AddMinutes(1440))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ContactTel), q => q.ContactTel.Contains(req.ContactTel) || q.NewestContactTel.Contains(req.ContactTel))
                //.WhereIf(!string.IsNullOrWhiteSpace(req.QryFromType), q => q.ServiceWorkOrders.Any(a => a.FromType.Equals(Convert.ToInt32(req.QryFromType))))
                //.WhereIf(req.CompleteDate != null, q => q.ServiceWorkOrders.Any(s => s.CompleteDate > req.CompleteDate))
                //.WhereIf(req.EndCompleteDate != null, q => q.ServiceWorkOrders.Any(s => s.CompleteDate < Convert.ToDateTime(req.EndCompleteDate).AddDays(1)))
                //.WhereIf(techName.Count > 0, q => q.ServiceWorkOrders.Any(s => techName.Contains(s.CurrentUser)))
                .Where(q => ids.Contains(q.Id) && q.Status == 2);

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
                && (string.IsNullOrWhiteSpace(req.QryFromType) || a.FromType.Equals(Convert.ToInt32(req.QryFromType)))
                && (req.CompleteDate == null || (a.CompleteDate > req.CompleteDate))
                && (req.EndCompleteDate == null || (a.CompleteDate < Convert.ToDateTime(req.EndCompleteDate).AddDays(1)))).ToList()
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
            var catetory = await UnitWork.Find<Category>(c => c.TypeId == "SYS_CallTheSource").Select(c => new { c.DtValue, c.Name }).ToListAsync();
            List<ServiceWorkOrder> serviceWorkOrders = new List<ServiceWorkOrder>();
            dataList.ForEach(d => serviceWorkOrders.AddRange(d.ServiceWorkOrders));
            var completionReportIds = serviceWorkOrders.Select(s => s.CompletionReportId).ToList();
            var completionReports = await UnitWork.Find<CompletionReport>(c => completionReportIds.Contains(c.Id)).ToListAsync();

            var list = new List<ServiceOrderExcelDto>();
            foreach (var serviceOrder in dataList)
            {
                foreach (var workOrder in serviceOrder.ServiceWorkOrders)
                {
                    var FromThemeJson = JsonHelper.Instance.Deserialize<List<FromThemeJsonResp>>(workOrder?.FromTheme);
                    string FromTheme = "";
                    FromThemeJson.ForEach(f => FromTheme += f.description);
                    double interval = 0;
                    if (!string.IsNullOrWhiteSpace(workOrder.CreateTime.ToString()) && !string.IsNullOrWhiteSpace(workOrder.CompleteDate.ToString()))
                    {
                        TimeSpan ts = workOrder.CompleteDate.Value.Subtract(workOrder.CreateTime.Value);
                        interval = Math.Round(ts.TotalHours, 2);
                    }
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
                        FromId = catetory.Where(w => w.DtValue == serviceOrder.FromId.Value.ToString()).FirstOrDefault()?.Name,// serviceOrder.FromId.Value == 1 ? "电话" : "App",
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
                        CompleteDate = workOrder.CompleteDate,
                        TimeInterval = interval,
                        BookingDate = workOrder.BookingDate,
                        VisitTime = workOrder.VisitTime,
                        LiquidationDate = workOrder.LiquidationDate,
                        Solution = workOrder.Solution?.Subject,
                        TroubleDescription = workOrder.TroubleDescription,
                        ProcessDescription = workOrder.ProcessDescription,
                        CompletionReporRemark = completionReports.Where(c => c.Id.Equals(workOrder?.CompletionReportId)).FirstOrDefault()?.Remark
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
                    .WhereIf("无序列号".Equals(req.MaterialType), a => a.MaterialCode == "无序列号")
                    .WhereIf(!"无序列号".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType)
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
                .WhereIf("无序列号".Equals(MaterialType), w => w.MaterialCode == "无序列号")
                .WhereIf(!"无序列号".Equals(MaterialType), w => w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-")) == MaterialType).FirstOrDefaultAsync())?.CurrentUserId;
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
        /// 撤回聊天室消息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> RevocationMessage(SendServiceOrderMessageReq req)
        {
            TableData result = new TableData();
            string userId = string.Empty;
            string name = string.Empty;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (req.AppUserId != 0)
            {
                var UserId = (await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(req.AppUserId)).FirstOrDefaultAsync()).UserID;
                loginUser = await UnitWork.Find<User>(u => u.Id.Equals(UserId)).FirstOrDefaultAsync();
            }
            var messageObj = await UnitWork.Find<ServiceOrderMessage>(s => s.Id.Equals(req.MessageId) && s.ReplierId.Equals(loginUser.Id)).FirstOrDefaultAsync();
            if (messageObj != null)
            {
                if (Convert.ToDateTime(messageObj.CreateTime).AddDays(1) > DateTime.Now)
                {
                    await UnitWork.DeleteAsync<ServiceOrderMessage>(messageObj);
                    await UnitWork.DeleteAsync<ServiceOrderMessageUser>(s => s.MessageId.Equals(messageObj.Id));
                    await UnitWork.SaveAsync();
                    result.Code = 200;
                    result.Message = "撤回成功";
                }
                else
                {
                    result.Code = 500;
                    result.Message = "时间已超过二十四小时不可撤回";
                }
            }
            else
            {
                result.Code = 500;
                result.Message = "非本人消息不可撤销";
            }
            return result;
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
            var pictures = req.ServiceOrderMessagePictures;
            obj.ServiceOrderMessagePictures = null;
            await UnitWork.AddAsync<ServiceOrderMessage, int>(obj);
            await UnitWork.SaveAsync();
            if (pictures != null && pictures?.Count > 0)
            {
                pictures?.ForEach(p => { p.ServiceOrderMessageId = obj.Id; p.Id = Guid.NewGuid().ToString(); });
                await UnitWork.BatchAddAsync(pictures?.ToArray());
                await UnitWork.SaveAsync();
            }
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
            //获取App与Erp绑定关系
            var appuserMapInfo = await UnitWork.Find<AppUserMap>(null).ToListAsync();
            //客服Id
            string RecepUserId = serviceInfo.RecepUserId;
            if (!string.IsNullOrEmpty(RecepUserId))
            {
                var recepUserInfo = appuserMapInfo.Where(a => a.UserID == RecepUserId).FirstOrDefault();
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
                var superUserInfo = appuserMapInfo.Where(a => a.UserID == SupervisorId).FirstOrDefault();
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
            //销售用户Id
            string salesManId = serviceInfo.SalesManId;
            if (!string.IsNullOrEmpty(salesManId))
            {
                var salesManInfo = appuserMapInfo.Where(a => a.UserID == salesManId).FirstOrDefault();
                if (salesManInfo != null && salesManInfo.AppUserId > 0 && !salesManInfo.AppUserId.Equals(FromUserId))
                {
                    var msgObj = new ServiceOrderMessageUser
                    {
                        CreateTime = DateTime.Now,
                        FromUserId = FromUserId.ToString(),
                        FroUserId = salesManInfo.AppUserId.ToString(),
                        HasRead = false,
                        MessageId = MessageId
                    };
                    await UnitWork.AddAsync<ServiceOrderMessageUser, int>(msgObj);
                }
            }
            //查询相关技术员Id
            var userList = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ServiceOrderId && s.CurrentUserId != FromUserId && s.CurrentUserId > 0 && s.Status < 7).ToListAsync()).GroupBy(g => g.CurrentUserId)
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
            //获取聊天信息
            var messageList = UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == req.ServiceOrderId).Include(s => s.ServiceOrderMessagePictures);
            var resultsql = messageList.OrderByDescending(r => r.CreateTime).Select(s => new
            {
                s.Content,
                s.CreateTime,
                s.FroTechnicianName,
                s.AppUserId,
                s.Replier,
                s.ServiceOrderMessagePictures
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
                s.Replier,
                s.ServiceOrderMessagePictures
            });
            result.Count = await messageList.CountAsync();
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
            var msgList = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == serviceOrderId).Select(c => c.Id).ToListAsync();
            if (msgList != null)
            {
                //string msgIds = string.Join(",", msgList.Select(s => s.Id).Distinct().ToArray());
                UnitWork.Update<ServiceOrderMessageUser>(s => msgList.Contains(s.MessageId) && s.FroUserId == currentUserId.ToString(), u => new ServiceOrderMessageUser { HasRead = true });
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
            var nsapUserId = (await UnitWork.Find<AppUserMap>(u => u.AppUserId == req.CurrentUserId).FirstOrDefaultAsync())?.UserID;
            if (nsapUserId == null) 
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //技术员 主管 业务员
            var queryService = await (from a in UnitWork.Find<ServiceWorkOrder>(null)
                                      join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id
                                      where b.SupervisorId == nsapUserId || a.CurrentUserId == req.CurrentUserId || b.SalesManId == nsapUserId
                                      select new { b.Id, b.U_SAP_ID }).ToListAsync();
            var serviceIdList = queryService.Select(c => c?.Id).ToList();
            if (serviceIdList != null)
            {
                //string serviceIds = string.Join(",", serviceIdList.Select(s => s.b.Id).Distinct().ToArray());
                var query1 = await UnitWork.Find<ServiceOrderMessage>(c => serviceIdList.Contains(c.ServiceOrderId))
                                .Skip((req.page - 1) * req.limit)
                                .Take(req.limit)
                                .OrderByDescending(o => o.CreateTime)
                                .Select(s => new
                                {
                                    s.Content,
                                    s.CreateTime,
                                    s.FroTechnicianName,
                                    s.AppUserId,
                                    s.ServiceOrderId,
                                    s.Replier,
                                    s.Id
                                }).ToListAsync();

                //var query = from a in UnitWork.Find<ServiceOrderMessage>(null)
                //            join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                //            from b in ab.DefaultIfEmpty()
                //            select new { a, b };
                //var resultsql = query.Where(w => serviceIdList.Contains(w.a.ServiceOrderId.ToString())).OrderByDescending(o => o.a.CreateTime).Select(s => new
                //{
                //    s.b.U_SAP_ID,
                //    s.a.Content,
                //    s.a.CreateTime,
                //    s.a.FroTechnicianName,
                //    s.a.AppUserId,
                //    s.a.ServiceOrderId,
                //    s.a.Replier,
                //    s.a.Id
                //});
                var messageId = query1.Select(c => c.Id).ToList();
                var meassageUser = await UnitWork.Find<ServiceOrderMessageUser>(c => messageId.Contains(c.MessageId)).Select(c => new { c.MessageId, c.FroUserId, c.HasRead }).ToListAsync();

                result.Data = query1.GroupBy(g => g.ServiceOrderId).Select(s =>
                {
                    var first = s.First();
                    var messageid = s.Select(c => c.Id).ToList();
                    var hasRead = meassageUser.Where(c => messageid.Contains(c.MessageId) && c.FroUserId == req.CurrentUserId.ToString()).All(c => c.HasRead == true);//服务id下消息是否全部已读
                    var sapid = queryService.Where(q => q.Id == first.ServiceOrderId).FirstOrDefault();
                    return new
                    {
                        first.Content,
                        CreateTime = first.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                        first.FroTechnicianName,
                        first.AppUserId,
                        first.ServiceOrderId,
                        first.Replier,
                        U_SAP_ID = sapid?.U_SAP_ID,
                        HasRead = hasRead
                    };
                }).ToList();
                //result.Data =
                //((await resultsql
                //.ToListAsync()).GroupBy(g => g.ServiceOrderId).Select(s => 
                //{
                //    var first = s.First();
                //    var messageid = s.Select(c => c.Id).ToList();
                //    var hasRead = meassageUser.Where(c => messageid.Contains(c.MessageId) && c.FroUserId==req.CurrentUserId.ToString()).All(c => c.HasRead == true);//服务id下消息是否全部已读
                //    return new
                //    {
                //        first.Content,
                //        CreateTime = first.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                //        first.FroTechnicianName,
                //        first.AppUserId,
                //        first.ServiceOrderId,
                //        first.Replier,
                //        first.U_SAP_ID,
                //        HasRead = hasRead
                //    };
                //}));
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
            var msgCount = await UnitWork.Find<ServiceOrderMessageUser>(s => s.FroUserId == currentUserId.ToString() && s.HasRead == false).CountAsync();
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
            var timespan = DatetimeUtil.ToUnixTimestampBySeconds(DateTime.Now.AddMinutes(5));
            var text = $"NewareApiTokenDeadline:{timespan}";
            var aes = Encryption.AESEncrypt(text);

            _helper.Post(new
            {
                UserId = userId,
                Title = title,
                Content = content
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "BbsCommunity/AppPushMsg", "EncryToken", aes);
        }

        /// <summary>
        /// 推送消息至新威智能app，推送即将过期校准证书关联的guid
        /// </summary>
        /// <param name="userId">app用户Id</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <param name="type">消息类型</param>
        /// <param name="guid">下位机GUID</param>
        /// <returns></returns>
        public async Task<string> PushMessageToApp(int userId, string title, string content, string type, dynamic guidInfo)
        {
            var timespan = DatetimeUtil.ToUnixTimestampBySeconds(DateTime.Now.AddMinutes(5));
            var text = $"NewareApiTokenDeadline:{timespan}";
            var aes = Encryption.AESEncrypt(text);

            return _helper.Post(new
            {
                UserId = userId,
                Title = title,
                Content = content,
                Type = type,
                GuidInfos = guidInfo
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "BbsCommunity/AppPushMsg", "EncryToken", aes);
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
                                MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-"))
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
                        MaterialTypeName = "无序列号".Equals(s.Key) ? "无序列号" : MaterialTypeModel.Where(m => m.TypeAlias == s.Key).FirstOrDefault().TypeName,
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
                  .WhereIf("无序列号".Equals(request.MaterialType), a => a.MaterialCode == "无序列号")
                  .WhereIf(!"无序列号".Equals(request.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == request.MaterialType)
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
            if (string.IsNullOrWhiteSpace(req.Province) && string.IsNullOrWhiteSpace(req.City)) throw new Exception("地址错误，请核对地址后重新上传。");
            if (string.IsNullOrWhiteSpace(req.Addr) ||  string.IsNullOrWhiteSpace(req.Area) ) throw new Exception("地址错误，请核对地址后重新上传。");

            var obj = req.MapTo<ServiceOrder>();
            obj.CustomerId = obj.CustomerId.ToUpper();
            obj.CreateTime = DateTime.Now;
            obj.CreateUserId = loginContext.User.Id;
            obj.RecepUserId = loginContext.User.Id;
            obj.RecepUserName = loginContext.User.Name;
            obj.Status = 1;
            obj.FromAppUserId = req.AppUserId;
            obj.VestInOrg = 1;
            obj.FromId = 6;//APP提交
            if (obj.ServiceWorkOrders != null)
            {
                var expect = await CalculateRatio(obj.ServiceWorkOrders.FirstOrDefault()?.FromTheme);
                obj.ExpectServiceMode = expect.ExpectServiceMode;
                obj.ExpectRatio = expect.ExpectRatio;
            }

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
            var MaterialTypes = string.Join(",", obj.ServiceOrderSNs?.Select(s => s.ItemCode == "无序列号" ? "无序列号" : s.ItemCode.Substring(0, s.ItemCode.IndexOf("-"))).Distinct().ToArray());

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
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Name : s.ProblemTypeName,
                ProblemTypeId = string.IsNullOrEmpty(s.ProblemTypeId) ? s.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Id : s.ProblemTypeId,
                DeviceInfos = s.ServiceOrderSNs.GroupBy(o => "无序列号".Equals(o.ItemCode) ? "无序列号" : o.ItemCode.Substring(0, o.ItemCode.IndexOf("-"))).ToList()
                .Select(a => new
                {
                    MaterialType = a.Key,
                    UnitName = "台",
                    Count = a.Count(),
                    orders = a.ToList(),
                    Status = s.ServiceWorkOrders.FirstOrDefault(b => "无序列号".Equals(a.Key) ? b.MaterialCode == "无序列号" : b.MaterialCode.Contains(a.Key))?.Status,
                    MaterialTypeName = "无序列号".Equals(a.Key) ? "无序列号" : MaterialTypeModel.Where(m => m.TypeAlias == a.Key).FirstOrDefault().TypeName
                }),
                IsCanEvaluate //0不可评价 1可评价
            }).ToList();
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 获取客户快报信息
        /// </summary>
        /// <param name="currenUserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetCustServiceNews(int currenUserId)
        {
            var result = new TableData();
            var outData = new List<dynamic>();
            var workorderList = await UnitWork.Find<ServiceOrder>(w => w.AppUserId == currenUserId).Include(i => i.ServiceWorkOrders).Select(s => new
            {
                s.U_SAP_ID,
                ServiceWorkOrders = s.ServiceWorkOrders.Select(o => new
                {
                    o.Id,
                    o.Status,
                    MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                    o.OrderTakeType
                }).ToList(),
                s.Id
            }).ToListAsync();
            foreach (var item in workorderList)
            {
                Dictionary<string, object> newsList = new Dictionary<string, object>();
                if (item.ServiceWorkOrders.Count > 0 && item.U_SAP_ID != null)
                {
                    int status = (int)item.ServiceWorkOrders.Max(s => s.Status);
                    int orderTakeType = (int)item.ServiceWorkOrders.Max(s => s.OrderTakeType);
                    string materialType = item.ServiceWorkOrders.Where(w => w.Status == status && w.OrderTakeType == orderTakeType).Select(s => s.MaterialType).FirstOrDefault();
                    string conetnt = GetNewsContent(status, (int)item.U_SAP_ID, orderTakeType);
                    newsList.Add("sapId", item.U_SAP_ID);
                    newsList.Add("serviceOrderId", item.Id);
                    newsList.Add("materialType", materialType);
                    newsList.Add("status", status);
                    newsList.Add("content", conetnt);
                }
                else
                {
                    newsList.Add("sapId", item.U_SAP_ID);
                    newsList.Add("serviceOrderId", item.Id);
                    newsList.Add("materialType", "");
                    newsList.Add("status", 0);
                    newsList.Add("content", "你的服务单" + item.U_SAP_ID + "已提交成功，请耐心等客服接收");
                }
                outData.Add(newsList);
            }
            result.Data = outData;
            return result;
        }

        private string GetNewsContent(int status, int sapId, int orderTakeType)
        {
            string content = string.Empty;
            if (status < 3)
            {
                content = "你的服务单" + sapId + "客服已接收，请留意服务进度";
            }
            else
            {
                switch (orderTakeType)
                {
                    case 4:
                        content = "你的服务单" + sapId + "技术员已预约上门时间，请留意服务进度";
                        break;
                    case -1:
                        content = "你的服务单" + sapId + "已返厂维修，请保持电话畅通";
                        break;
                    case -2:
                        content = "你的服务单" + sapId + "设备已发回，请保持电话畅通";
                        break;
                    case 5:
                        content = "你的服务单" + sapId + "技术员已经上门，请保持电话畅通";
                        break;
                    default:
                        content = "你的服务单" + sapId + "正在服务中";
                        break;
                }
            }
            return content;
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
                            a.TerminalCustomer,
                            a.TerminalCustomerId,
                            a.CustomerId,
                            NewestContacter = string.IsNullOrEmpty(a.NewestContacter) ? a.Contacter : a.NewestContacter,
                            NewestContactTel = string.IsNullOrEmpty(a.NewestContactTel) ? a.ContactTel : a.NewestContactTel,
                            AppCustId = a.AppUserId,
                            Reamrk = a.ServiceWorkOrders.Count > 0 ? a.ServiceWorkOrders.FirstOrDefault().Remark : "",
                            ServiceWorkOrders = a.ServiceWorkOrders.Where(w => w.CurrentUserId == CurrentUserId && (string.IsNullOrEmpty(MaterialType) ? true : "无序列号".Equals(MaterialType) ? w.MaterialCode == "无序列号" : w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-")) == MaterialType)).Select(o => new
                            {
                                o.Id,
                                o.Status,
                                o.FromTheme,
                                o.ManufacturerSerialNumber,
                                o.MaterialCode,
                                o.MaterialDescription,
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
                    a.CustomerId,
                    a.TerminalCustomer,
                    a.TerminalCustomerId,
                    a.Reamrk,
                    ProblemTypeName = string.IsNullOrEmpty(a.ProblemTypeName) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Name : a.ProblemTypeName,
                    ProblemTypeId = string.IsNullOrEmpty(a.ProblemTypeId) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Id : a.ProblemTypeId,
                    Services = GetServiceFromTheme(a.ServiceWorkOrders.FirstOrDefault()?.FromTheme),
                    Priority = a.ServiceWorkOrders.FirstOrDefault()?.Priority == 3 ? "高" : a.ServiceWorkOrders.FirstOrDefault()?.Priority == 2 ? "中" : "低",
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                    {
                        MaterialType = string.IsNullOrEmpty(s.Key) ? "无序列号" : s.Key,
                        TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                        Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                        Count = s.Count(),
                        Orders = s.ToList(),
                        UnitName = "台",
                        MaterialTypeName = string.IsNullOrEmpty(s.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == s.Key).FirstOrDefault().TypeName,
                        ServiceMode = s.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                        flowinfo = flowList.Where(w => w.MaterialType == (string.IsNullOrEmpty(s.Key) ? "无序列号" : s.Key)).ToList()
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
                .WhereIf("无序列号".Equals(MaterialType), a => a.MaterialCode == "无序列号")
                .WhereIf(!"无序列号".Equals(MaterialType), o => o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")) == MaterialType).OrderBy(o => o.OrderTakeType).FirstOrDefaultAsync();
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
        /// 保存解决方案（2021.03.23 日报改版 废弃）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SaveWorkOrderSolution(SaveWorkOrderSolutionReq req)
        {
            //获取当前设备的工单集合
            var orderIds = await UnitWork.Find<ServiceWorkOrder>(null).Where(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId)
                .WhereIf("无序列号".Equals(req.MaterialType), a => a.MaterialCode == "无序列号")
                .WhereIf(!"无序列号".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType).Select(s => s.Id)
                .ToListAsync();
            List<int> workOrderIds = new List<int>();
            foreach (var id in orderIds)
            {
                workOrderIds.Add(id);
            }
            //获取解决方案名称
            var SolutionName = (await UnitWork.Find<Solution>(s => s.UseBy == 2 && s.Id == req.SolutionId).FirstOrDefaultAsync())?.Subject;
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
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Name : s.ProblemTypeName,
                ProblemTypeId = string.IsNullOrEmpty(s.ProblemTypeId) ? s.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Id : s.ProblemTypeId,
                DeviceInfos = s.ServiceWorkOrders.GroupBy(o => "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-"))).ToList()
                .Select(a => new
                {
                    MaterialType = a.Key,
                    UnitName = "台",
                    Count = a.Count(),
                    orders = a.Select(a => new { a.MaterialCode, a.ManufacturerSerialNumber, a.Id }).ToList(),
                    Status = s.ServiceWorkOrders.FirstOrDefault(b => "无序列号".Equals(a.Key) ? b.MaterialCode == "无序列号" : b.MaterialCode.Contains(a.Key))?.Status,
                    MaterialTypeName = "无序列号".Equals(a.Key) ? "无序列号" : MaterialTypeModel.Where(m => m.TypeAlias == a.Key).FirstOrDefault().TypeName
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
    .WhereIf("无序列号".Equals(request.MaterialType), a => a.MaterialCode == "无序列号")
    .WhereIf(!"无序列号".Equals(request.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == request.MaterialType).Select(s => s.Id)
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
            else if (request.Type == -1)//返厂维修
            {
                servicemode = 3;
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
            //流程信息
            var flowInfo = (await UnitWork.Find<ServiceFlow>(w => w.ServiceOrderId == request.ServiceOrderId && w.MaterialType == request.MaterialType && w.Creater == userInfo.User.Id).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed, s.FlowType }).ToListAsync()).GroupBy(g => g.FlowType).Select(s => new { s.Key, detail = s.ToList() });
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
                        MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
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
        /// 修改故障描述（2021.03.23 日报改版 废弃）
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task UpdateWorkOrderDescription(UpdateWorkOrderDescriptionReq request)
        {
            var orderIds = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.CurrentUserId == request.CurrentUserId)
                .WhereIf("无序列号".Equals(request.MaterialType), a => a.MaterialCode == "无序列号")
                .WhereIf(!"无序列号".Equals(request.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == request.MaterialType)
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
                .WhereIf("无序列号".Equals(req.MaterialType), a => a.MaterialCode == "无序列号")
                .WhereIf(!"无序列号".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType)
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

            var typename = "无序列号".Equals(workOrderInfo.FirstOrDefault().MaterialCode) ? "无序列号" : workOrderInfo.FirstOrDefault().MaterialCode.Substring(0, workOrderInfo.FirstOrDefault().MaterialCode.IndexOf("-"));
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
        /// 旧版本只有进行中和已完成 1进行中2已完成 无分页 默认全部显示
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceOrder(TechnicianServiceWorkOrderReq req)
        {
            //20201109 前台不显示暂时放开显示限制
            req.limit = 1000;
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
            var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == req.TechnicianId && s.FromType == 1)
                .WhereIf(req.Type == 1, s => s.Status.Value < 7 && s.Status.Value > 1)
                .WhereIf(req.Type == 2, s => s.Status.Value >= 7)
                .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(s => serviceOrderIds.Contains(s.Id))
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                .Include(s => s.ServiceFlows)
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
                        MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
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
                CreateTime = s.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                s.U_SAP_ID,
                s.CustomerId,
                s.CustomerName,
                s.TerminalCustomer,
                s.Count,
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.ProblemType?.Name : s.ProblemTypeName,
                MaterialTypeQty = s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count,
                MaterialInfo = s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                .Select(o => new
                {
                    MaterialType = o.Key,
                    Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                    MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                    OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                    ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                    flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).ToList()
                })
            }).ToList();

            var count = await query.CountAsync();
            result.Data = list;
            result.Count = count;
            return result;
        }


        /// <summary>
        /// 技术员查看服务单列表(技术员主页-工单池)
        /// 2020.12.17版本 此版本有待处理、进行中、已完成3种单据状态列表 Type：1待处理 2进行中 3已完成
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceOrderNew1(TechnicianServiceWorkOrderReq req)
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
            //获取当前技术员的服务单集合 排除在线解答的单子
            var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == req.TechnicianId && s.FromType == 1)
                .Select(s => s.ServiceOrderId).Distinct().ToListAsync();

            List<int> workIds = new List<int>();
            //获取转派的已完成的单据
            if (req.Type == 3)
            {
                var redeployList = await UnitWork.Find<ServiceRedeploy>(w => w.TechnicianId == req.TechnicianId).ToListAsync();
                var redeployIds = redeployList.Select(s => s.ServiceOrderId).Distinct().ToList();
                foreach (var item in redeployList)
                {
                    List<int> ids = item.WorkOrderIds.Split(',').Select(m => Convert.ToInt32(m)).ToList();
                    workIds = workIds.Concat(ids).ToList();
                }
                if (redeployIds.Count > 0)
                {
                    redeployIds.ForEach(f => serviceOrderIds.Add((int)f));
                }
            }
            //获取完工报告集合
            var completeReportList = await UnitWork.Find<CompletionReport>(w => serviceOrderIds.Contains((int)w.ServiceOrderId)).Select(s => new { s.ServiceOrderId, s.TechnicianId, s.IsReimburse, s.Id, s.ServiceMode, MaterialType = s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) }).ToListAsync();
            //获取我的报销单集合
            var reimburseList = await UnitWork.Find<ReimburseInfo>(r => r.CreateUserId == userInfo.UserID).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id) && w.AllowOrNot == 0 && w.VestInOrg == req.OrderType)
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                .Include(s => s.ServiceFlows)
                .WhereIf(req.Type == 1, s => s.ServiceWorkOrders.All(a => a.OrderTakeType == 0))//待处理 所有设备类型都未操作
                                                                                                //.WhereIf(req.Type == 1 && req.TechOrg == 2, s => s.VestInOrg!=2)//过滤掉E3的单
                .WhereIf(req.Type == 2, s => !s.ServiceWorkOrders.All(a => a.OrderTakeType == 0) && !s.ServiceWorkOrders.All(a => a.Status >= 7))//进行中 有任意一个设备类型进行了操作
                .WhereIf(req.Type == 3, s => s.ServiceWorkOrders.All(a => a.Status >= 7)) //已完成 所有设备类型都已完成
                 .WhereIf(int.TryParse(req.key, out int id) || !string.IsNullOrWhiteSpace(req.key), s => s.U_SAP_ID == id || s.CustomerName.Contains(req.key) || s.ServiceWorkOrders.Any(o => o.ManufacturerSerialNumber.Contains(req.key)))
                //.WhereIf(req.TechOrg != 2, s => s.VestInOrg == 1 || s.VestInOrg == 3)//E3技术员可查看e3的单
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
                    s.VestInOrg,
                    Reamrk = s.ServiceWorkOrders.Count > 0 ? s.ServiceWorkOrders.FirstOrDefault().Remark : "",
                    MaterialCode = s.VestInOrg == 2 ? s.ServiceWorkOrders.Where(c => c.ServiceOrderId == s.Id).FirstOrDefault().MaterialCode : "",
                    ManufacturerSerialNumber = s.VestInOrg == 2 ? s.ServiceWorkOrders.Where(c => c.ServiceOrderId == s.Id).FirstOrDefault().ManufacturerSerialNumber : "",
                    Count = s.ServiceWorkOrders.Where(w => w.ServiceOrderId == s.Id && w.CurrentUserId == req.TechnicianId).Count(),
                    MaterialInfo = s.ServiceWorkOrders.Where(w => req.Type == 3 && s.VestInOrg == 1 ? workIds.Contains(w.Id) : w.CurrentUserId == req.TechnicianId).Select(o => new
                    {
                        o.MaterialCode,
                        o.ManufacturerSerialNumber,
                        MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                        o.Status,
                        o.Id,
                        o.OrderTakeType,
                        o.ServiceMode,
                        o.Priority,
                        o.TransactionType,
                        o.FromTheme,
                        o.AcceptTime,
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
                s.Count,
                s.VestInOrg,
                s.MaterialCode,
                s.ManufacturerSerialNumber,
                s.Reamrk,
                AcceptTime= s.MaterialInfo.Select(s => s.AcceptTime).FirstOrDefault()?.ToString("yyyy.MM.dd HH:mm"),
                FromTheme = s.MaterialInfo.Select(s => s.FromTheme).FirstOrDefault(),
                TransactionType = s.MaterialInfo.Select(s => s.TransactionType).FirstOrDefault(),
                Priority = s.MaterialInfo.Select(s => s.Priority).FirstOrDefault(),
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.ProblemType?.Name : s.ProblemTypeName,
                MaterialTypeQty = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count : 0,
                MaterialInfo = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                .Select(o => new
                {
                    MaterialType = o.Key,
                    Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                    MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                    OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                    ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                    flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList()
                }) : new object(),
                IsReimburse = req.Type == 3 && (s.VestInOrg == 1 || s.VestInOrg == 3) ? completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == req.TechnicianId.ToString()).FirstOrDefault() == null ? 0 : completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == req.TechnicianId.ToString()).FirstOrDefault().IsReimburse : 0,
                MaterialType = req.Type == 3 && s.VestInOrg == 1 ? completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == req.TechnicianId.ToString()).FirstOrDefault() == null ? string.Empty : completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == req.TechnicianId.ToString()).OrderBy(o => o.ServiceMode).FirstOrDefault().MaterialType : string.Empty,
                ReimburseId = req.Type == 3 && (s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.Id).FirstOrDefault() : 0,
                RemburseStatus = req.Type == 3 && (s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.RemburseStatus).FirstOrDefault() : 0,
                RemburseIsRead = req.Type == 3 && (s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.IsRead).FirstOrDefault() : 0
            }).ToList();

            var count = await query.CountAsync();
            result.Data = list;
            result.Count = count;
            return result;
        }

        #region

        /// <summary>
        /// 技术员查看服务单列表(技术员主页-工单池)
        /// 2020.12.17版本 此版本有待处理、进行中、已完成3种单据状态列表 Type：1待处理 2进行中 3已完成
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceOrderNew(TechnicianServiceWorkOrderReq req)
        {
            var result = new TableData();
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
            //获取当前技术员的服务单集合 排除在线解答的单子
            var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == req.TechnicianId && s.FromType == 1)
                .Select(s => s.ServiceOrderId).Distinct().ToListAsync();

            List<int> workIds = new List<int>();
            //获取转派的已完成的单据
            if (req.Type == 3)
            {
                var redeployList = await UnitWork.Find<ServiceRedeploy>(w => w.TechnicianId == req.TechnicianId).ToListAsync();
                var redeployIds = redeployList.Select(s => s.ServiceOrderId).Distinct().ToList();
                foreach (var item in redeployList)
                {
                    List<int> ids = item.WorkOrderIds.Split(',').Select(m => Convert.ToInt32(m)).ToList();
                    workIds = workIds.Concat(ids).ToList();
                }
                if (redeployIds.Count > 0)
                {
                    redeployIds.ForEach(f => serviceOrderIds.Add((int)f));
                }
            }
            //获取完工报告集合
            var completeReportList = await UnitWork.Find<CompletionReport>(w => serviceOrderIds.Contains((int)w.ServiceOrderId)).Select(s => new { s.ServiceOrderId, s.TechnicianId, s.IsReimburse, s.Id, s.ServiceMode, MaterialType = s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) }).ToListAsync();
            //获取我的报销单集合
            var reimburseList = await UnitWork.Find<ReimburseInfo>(r => r.CreateUserId == userInfo.UserID).ToListAsync();

            var query = from s in UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id) && w.AllowOrNot == 0 && w.VestInOrg == req.OrderType)
                            join f in UnitWork.Find<ServiceWorkOrder>(null) on s.Id equals f.ServiceOrderId
                            select new
                            {
                                s.Id,
                                s.AppUserId,
                                s.U_SAP_ID,
                                s.CustomerName,
                                f.OrderTakeType,
                                f.Status,
                                f.ManufacturerSerialNumber
                            };
            query = query.WhereIf(int.TryParse(req.key, out int id) || !string.IsNullOrWhiteSpace(req.key), s => s.U_SAP_ID == id || s.CustomerName.Contains(req.key) || s.ManufacturerSerialNumber.Contains(req.key)); ;
            var queryGroup = (query.GroupBy(a => a.Id).Select(a => new
            {
                id = a.Key,
                maxOrderTakeType = a.Max(b => b.OrderTakeType),
                minOrderTakeType = a.Min(b => b.OrderTakeType),
                maxStatus = a.Max(b => b.Status),
                minStatus = a.Min(b => b.Status),
            }))
            .WhereIf(req.Type == 1, s => s.maxOrderTakeType == 0 && s.minOrderTakeType == 0)//待处理 所有设备类型都未操作
            .WhereIf(req.Type == 2, s => s.maxOrderTakeType != 0 && s.minStatus < 7)//进行中 有任意一个设备类型进行了操作                  
            .WhereIf(req.Type == 3, s => s.minStatus >= 7); //已完成 所有设备类型都已完成

            var count = queryGroup.Count();
            var listId = await queryGroup.OrderByDescending(o => o.id).Skip((req.page - 1) * req.limit)
            .Take(req.limit).Select(a => a.id).ToListAsync();

            var listOrder = await UnitWork.Find<ServiceOrder>(a => listId.Contains(a.Id)).ToListAsync();
            var listWorkOrder = await UnitWork.Find<ServiceWorkOrder>(a => listId.Contains(a.ServiceOrderId)).ToListAsync();
            var listFlow = await UnitWork.Find<ServiceFlow>(a => listId.Contains((int)a.ServiceOrderId)).ToListAsync();

            List<dynamic> listRes = new List<dynamic>();
            foreach (var item in listOrder)
            {
                var serviceOrder = listWorkOrder.Where(a => a.ServiceOrderId == item.Id).ToList();
                var MaterialInfo = serviceOrder.Where(w => req.Type == 3 && item.VestInOrg == 1 ? workIds.Contains(w.Id) : w.CurrentUserId == req.TechnicianId).Select(o => new
                {
                    o.MaterialCode,
                    o.ManufacturerSerialNumber,
                    MaterialType = string.IsNullOrEmpty(o.MaterialCode) ? "" :( o.MaterialCode.IndexOf("-") > 0 || o.MaterialCode == "无序列号") ? "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")):"",
                    o.Status,
                    o.Id,
                    o.OrderTakeType,
                    o.ServiceMode,
                    o.Priority,
                    o.TransactionType,
                    o.FromTheme,
                    o.AcceptTime,
                });


                var ProblemType = serviceOrder.FirstOrDefault()?.ProblemType;
                var flow = listFlow.Where(w => w.Creater == userInfo.UserID && w.ServiceOrderId == item.Id && w.FlowType == 1).ToList();

                dynamic info = new
                {
                    Id = item.Id,
                    AppUserId = item.AppUserId,
                    Services = GetServiceFromTheme(serviceOrder.FirstOrDefault()?.FromTheme),
                    Latitude = item.Latitude,
                    Longitude = item.Longitude,
                    Province = item.Province,
                    City = item.City,
                    Area = item.Area,
                    Addr = item.Addr,
                    //info.Contacter = item.Contacter,
                    //info.ContactTel = item.ContactTel,
                    NewestContacter = item.NewestContacter,
                    NewestContactTel = item.NewestContactTel,
                    Status = item.Status,
                    CreateTime = item.CreateTime?.ToString("yyyy.MM.dd HH:mm"),
                    U_SAP_ID = item.U_SAP_ID,
                    CustomerId = item.CustomerId,
                    CustomerName = item.CustomerName,
                    TerminalCustomer = item.TerminalCustomer,
                    Count = serviceOrder.Where(a => a.CurrentUserId == req.TechnicianId).Count(),
                    VestInOrg = item.VestInOrg,
                    MaterialCode = item.VestInOrg == 2 ? serviceOrder.FirstOrDefault()?.MaterialCode : "",
                    ManufacturerSerialNumber = item.VestInOrg == 2 ? serviceOrder.FirstOrDefault()?.ManufacturerSerialNumber : "",
                    Reamrk = serviceOrder.FirstOrDefault()?.Remark,
                    AcceptTime =MaterialInfo.Select(s => s.AcceptTime).FirstOrDefault()?.ToString("yyyy.MM.dd HH:mm"),
                    FromTheme =MaterialInfo.Select(s => s.FromTheme).FirstOrDefault() ,
                    TransactionType = MaterialInfo.Select(s => s.TransactionType).FirstOrDefault(),
                    Priority = MaterialInfo.Select(s => s.Priority).FirstOrDefault(),
                    ProblemTypeName = string.IsNullOrEmpty(item.ProblemTypeName) ? ProblemType?.Name : item.ProblemTypeName,
                    MaterialTypeQty = item.VestInOrg == 1 ? MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count : 0,
                    MaterialInfo = item.VestInOrg == 1 ? MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                 .Select(o => new
                 {
                     MaterialType = o.Key,
                     Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                     MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault()?.TypeName,
                     OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                     ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                     flowInfo = flow.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList()
                 }) : new object(),
                    IsReimburse = req.Type == 3 && (item.VestInOrg == 1 || item.VestInOrg == 3) ? completeReportList.Where(w => w.ServiceOrderId == item.Id && (w.ServiceMode == 1 || item.VestInOrg == 3) && w.TechnicianId == req.TechnicianId.ToString()).FirstOrDefault() == null ? 0 : completeReportList.Where(w => w.ServiceOrderId == item.Id && (w.ServiceMode == 1 || item.VestInOrg == 3) && w.TechnicianId == req.TechnicianId.ToString()).FirstOrDefault()?.IsReimburse : 0,
                    MaterialType = req.Type == 3 && item.VestInOrg == 1 ? completeReportList.Where(w => w.ServiceOrderId == item.Id && w.TechnicianId == req.TechnicianId.ToString()).FirstOrDefault() == null ? string.Empty : completeReportList.Where(w => w.ServiceOrderId == item.Id && w.TechnicianId == req.TechnicianId.ToString()).OrderBy(o => o.ServiceMode).FirstOrDefault()?.MaterialType : string.Empty,
                    ReimburseId = req.Type == 3 && (item.VestInOrg == 1 || item.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == item.Id && w.CreateUserId == userInfo.UserID).Select(s => s.Id).FirstOrDefault() : 0,
                    RemburseStatus = req.Type == 3 && (item.VestInOrg == 1 || item.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == item.Id && w.CreateUserId == userInfo.UserID).Select(s => s.RemburseStatus).FirstOrDefault() : 0,
                    RemburseIsRead = req.Type == 3 && (item.VestInOrg == 1 || item.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == item.Id && w.CreateUserId == userInfo.UserID).Select(s => s.IsRead).FirstOrDefault() : 0,
                };
                listRes.Add(info);
            }

            result.Data = listRes.OrderByDescending(a => a.Id).ToList();
            result.Count = count;
            return result;
        }
        #endregion
        /// <summary>
        /// 获取技术员单据数量
        /// </summary>
        /// <param name="TechnicianId"></param>
        /// <param name="TechType">技术员类型 1-普通 2-E3 </param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceOrderCount(int TechnicianId, int TechType)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            List<int> workIds = new List<int>();
            var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == TechnicianId && s.FromType == 1)
               .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
            var serviceWorkOrderList = await UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id) && w.AllowOrNot==0)
               .Include(s => s.ServiceWorkOrders)
               .WhereIf(TechType != 2, w => (w.VestInOrg == 1 || w.VestInOrg == 3))//普通技术员只查看呼叫中心数据
               .ToListAsync();
            //获取待处理单据数量
            var pendingQty = (serviceWorkOrderList.Where(s => s.ServiceWorkOrders.All(a => a.OrderTakeType == 0))
               .ToList()).Count;
            if (TechType == 2) pendingQty = (serviceWorkOrderList.Where(s => s.ServiceWorkOrders.All(a => a.OrderTakeType == 0) && s.VestInOrg != 2)
               .ToList()).Count;//未处理过滤掉E3的单

            //获取进行中的单据数量
            var goingQty = (serviceWorkOrderList
               .Where(s => !s.ServiceWorkOrders.All(a => a.OrderTakeType == 0) && !s.ServiceWorkOrders.All(a => a.Status >= 7))
               .ToList()).Count;

            //获取已完成的单据数量
            //获取转派的已完成的单据
            var redeployList = await UnitWork.Find<ServiceRedeploy>(w => w.TechnicianId == TechnicianId).ToListAsync();
            var redeployIds = redeployList.Select(s => s.ServiceOrderId).Distinct().ToList();
            foreach (var item in redeployList)
            {
                List<int> ids = item.WorkOrderIds.Split(',').Select(m => Convert.ToInt32(m)).ToList();
                workIds = workIds.Concat(ids).ToList();
            }
            if (redeployIds.Count > 0)
            {
                redeployIds.ForEach(f => serviceOrderIds.Add((int)f));
                serviceOrderIds = serviceOrderIds.Distinct().ToList();
            }
            var finishList = await UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id)).WhereIf(TechType != 2, w => (w.VestInOrg == 1 || w.VestInOrg == 3))//普通技术员只查看呼叫中心数据
               .Include(s => s.ServiceWorkOrders).ToListAsync();
            var finishQty = finishList
              .Where(s => s.ServiceWorkOrders.All(a => a.Status >= 7)).ToList().Count;

            //获取已报销的单据数量
            var isReimburseQty = (await UnitWork.Find<ReimburseInfo>(w => serviceOrderIds.Contains(w.ServiceOrderId) && w.RemburseStatus != 3).ToListAsync()).Count;
            //获取上门服务的完成单据数量
            var a = finishList
              .Where(s => s.ServiceWorkOrders.Any(a => a.ServiceMode == 1) && s.ServiceWorkOrders.All(a => a.Status >= 7))
              .Select(s => s.U_SAP_ID).ToList();
            var doorQty = (finishList
              .Where(s => s.ServiceWorkOrders.Any(a => a.ServiceMode == 1) && s.ServiceWorkOrders.All(a => a.Status >= 7))
              .ToList()).Count;
            result.Data = new { pendingQty, goingQty, finishQty, reimburseQty = doorQty - isReimburseQty };
            return result;
        }

        /// <summary>
        /// 获取技术员客诉、行政、维修单数量
        /// </summary>
        /// <param name="TechnicianId"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceOrderCount(int TechnicianId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == TechnicianId && s.FromType == 1)
               .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
            //获取转派的已完成的单据
            var redeployList = await UnitWork.Find<ServiceRedeploy>(w => w.TechnicianId == TechnicianId).ToListAsync();
            var redeployIds = redeployList.Select(s => s.ServiceOrderId).Distinct().ToList();
            if (redeployIds.Count > 0)
            {
                redeployIds.ForEach(f => serviceOrderIds.Add((int)f));
                serviceOrderIds = serviceOrderIds.Distinct().ToList();
            }

            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => c.AllowOrNot == 0 && serviceOrderIds.Contains(c.Id)).Select(c => c.VestInOrg).ToListAsync();
            var groupCount = serviceOrder.GroupBy(c => c).Select(c=>new {c.Key, Count=c.Count() }).ToList();
            result.Data = new { 
                CusQty = groupCount.Where(w => w.Key == 1).FirstOrDefault()?.Count,
                RepairQty = groupCount.Where(w => w.Key == 2).FirstOrDefault()?.Count,
                XZQty = groupCount.Where(w => w.Key == 3).FirstOrDefault()?.Count,
            };
            return result;
        }

        /// <summary>
        /// 获取技术员单据明细数量
        /// </summary>
        /// <param name="TechnicianId">技术员ID</param>
        /// <param name="orderType">1.客诉单 2.维修单 3.行政单</param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceOrderCountDetail(int TechnicianId,int orderType)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            List<int> workIds = new List<int>();
            var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == TechnicianId && s.FromType == 1)
               .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
            var serviceWorkOrderList = await UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id) && w.AllowOrNot == 0 && w.VestInOrg == orderType)
               .Include(s => s.ServiceWorkOrders)
               .ToListAsync();
            //获取待处理单据数量
            var pendingQty = (serviceWorkOrderList.Where(s => s.ServiceWorkOrders.All(a => a.OrderTakeType == 0))
               .ToList()).Count;
            //获取进行中的单据数量
            var goingQty = (serviceWorkOrderList
               .Where(s => !s.ServiceWorkOrders.All(a => a.OrderTakeType == 0) && !s.ServiceWorkOrders.All(a => a.Status >= 7))
               .ToList()).Count;

            //获取已完成的单据数量
            //获取转派的已完成的单据
            var redeployList = await UnitWork.Find<ServiceRedeploy>(w => w.TechnicianId == TechnicianId).ToListAsync();
            var redeployIds = redeployList.Select(s => s.ServiceOrderId).Distinct().ToList();
            if (redeployIds.Count > 0)
            {
                redeployIds.ForEach(f => serviceOrderIds.Add((int)f));
                serviceOrderIds = serviceOrderIds.Distinct().ToList();
            }
            var finishQty = await UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id) && w.AllowOrNot == 0 && w.VestInOrg == orderType && w.ServiceWorkOrders.All(s => s.Status >= 7)).CountAsync();

            int orderQty = 0, reportQty = 0;
            if (orderType==1)
            {
                //获取当前用户nsap用户信息
                var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == TechnicianId).Include(i => i.User).FirstOrDefaultAsync();
                if (userInfo == null)
                {
                    throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
                }
                orderQty = await UnitWork.Find<CommissionOrder>(c => c.Status == 4 && c.CreateUserId == userInfo.UserID).CountAsync();
                reportQty = await UnitWork.Find<CommissionReport>(c => c.CreateUserId == userInfo.UserID).CountAsync();
            }
            result.Data = new { pendingQty, goingQty, finishQty, orderQty, reportQty };
            return result;
        }

        /// <summary>
        /// 技术员结束维修
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task TechicianEndRepair(TechicianEndRepairReq req)
        {
            var orderIds = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId)
              .WhereIf("无序列号".Equals(req.MaterialType), a => a.MaterialCode == "无序列号")
              .WhereIf(!"无序列号".Equals(req.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == req.MaterialType)
              .ToListAsync()).Select(s => s.Id).ToList();
            List<int> workOrderIds = new List<int>();
            foreach (var id in orderIds)
            {
                workOrderIds.Add(id);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId && s.CurrentUserId == req.CurrentUserId && orderIds.Contains(s.Id), e => new ServiceWorkOrder
            {
                IsStopOrder = 1
            });
            await UnitWork.SaveAsync();
        }


        /// <summary>
        /// 获取该技术员下的当前服务单所有设备
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianServiceMaterials(GetTechnicianServiceMaterialsReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var knowledgebases = await UnitWork.Find<KnowledgeBase>(k => k.Rank == 1 && k.IsNew == true && !string.IsNullOrWhiteSpace(k.Content)).ToListAsync();
            var ManufacturerSerialNumbers = await UnitWork.Find<ServiceWorkOrder>(w => w.CurrentUserId == req.TechnicianId && w.ServiceOrderId == req.ServiceOrderId).Select(s => new 
            { 
                s.ManufacturerSerialNumber, 
                s.MaterialCode
            }).ToListAsync();

            var data = ManufacturerSerialNumbers.Select(s => new
            {
                s.ManufacturerSerialNumber,
                s.MaterialCode,
                Code = knowledgebases.Where(k => Regex.IsMatch(s.MaterialCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? s.MaterialCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(s.MaterialCode, k.Content)).Select(k => k.Code).FirstOrDefault()
            }).ToList();
            result.Data = data;
            return result;
        }


        /// <summary>
        /// 技术员填写日报
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddDailyReport(AddDailyReportReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //判断当天是否已经填写日报 再次填写则数据清空
            var serviceDailyReport = await UnitWork.Find<ServiceDailyReport>(w => w.ServiceOrderId == req.ServiceOrderId && w.CreateUserId == userInfo.UserID && w.CreateTime.Value.Date == DateTime.Now.Date).FirstOrDefaultAsync();
            if (serviceDailyReport != null)
            {
                await UnitWork.DeleteAsync(serviceDailyReport);
            }
            List<ServiceDailyReport> serviceDailyReports = new List<ServiceDailyReport>();
            if (req.dailyResults != null)
            {
                foreach (var item in req.dailyResults)
                {
                    serviceDailyReports.Add(new ServiceDailyReport
                    {
                        ServiceOrderId = req.ServiceOrderId,
                        MaterialCode = item.MaterialCode,
                        ManufacturerSerialNumber = item.ManufacturerSerialNumber,
                        TroubleDescription = item.TroubleDescription,
                        ProcessDescription = item.ProcessDescription,
                        CreateUserId = userInfo.UserID,
                        CreaterName = userInfo.User.Name,
                        CreateTime = DateTime.Now
                    });
                }
                await UnitWork.BatchAddAsync(serviceDailyReports.ToArray());
                await UnitWork.SaveAsync();
            }
        }

        /// <summary>
        /// 获取日报详情（根据日期过滤）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianDailyReport(GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.TechnicianId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //获取当前时间
            var endDate = DateTime.Now;
            //获取取当前月的第一天
            DateTime startDate = new DateTime(endDate.Year, endDate.Month, 1);
            //若传入了指定年月 则取这个年月的信息
            if (!string.IsNullOrEmpty(req.Date))
            {
                DateTime date = Convert.ToDateTime(req.Date);
                startDate = new DateTime(date.Year, date.Month, 1);
                endDate = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            }
            //获取当月的所有日报信息
            var dailyReports = (await UnitWork.Find<ServiceDailyReport>(w => w.CreateUserId == userInfo.UserID && w.ServiceOrderId == req.ServiceOrderId && w.CreateTime.Value.Date >= startDate && w.CreateTime.Value.Date <= endDate).ToListAsync()).Select(s => new ReportDetail { CreateTime = s.CreateTime, MaterialCode = s.MaterialCode, ManufacturerSerialNumber = s.ManufacturerSerialNumber, TroubleCode = GetServiceTroubleAndSolution(s.TroubleDescription,"code"), TroubleDescription = GetServiceTroubleAndSolution(s.TroubleDescription, "description"), ProcessCode = GetServiceTroubleAndSolution(s.ProcessDescription,"code"),ProcessDescription = GetServiceTroubleAndSolution(s.ProcessDescription, "description") }).ToList();
            var dailyReportDates = dailyReports.OrderBy(o => o.CreateTime).Select(s => s.CreateTime?.Date.ToString("yyyy-MM-dd")).Distinct().ToList();

            var data = dailyReports.GroupBy(g => g.CreateTime?.Date).Select(s => new ReportResult { DailyDate = s.Key?.Date.ToString("yyyy-MM-dd"), ReportDetails = s.ToList() }).ToList();
            result.Data = new DailyReportResp { DailyDates = dailyReportDates, ReportResults = data };
            return result;
        }

        /// <summary>
        /// 获取日报最新日期
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetNewestDailyReport(GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.TechnicianId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var dailyReports = await UnitWork.Find<ServiceDailyReport>(w => w.CreateUserId == userInfo.UserID && w.ServiceOrderId == req.ServiceOrderId).OrderByDescending(c => c.CreateTime).Select(c => c.CreateTime).FirstOrDefaultAsync();
            //var data = dailyReports.Value.ToString("yyyy-MM-dd HH:mm:ss");
            var data = dailyReports != null ? dailyReports.Value.ToString("yyyy-MM-dd HH:mm:ss") : null;
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 获取技术员当天是否有日报
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianCureentDailyReport(GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.TechnicianId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var dailyReports = await UnitWork.Find<ServiceDailyReport>(w => w.CreateUserId == userInfo.UserID && w.CreateTime.Value.Date == DateTime.Now.Date && w.ServiceOrderId==req.ServiceOrderId).FirstOrDefaultAsync();

            result.Data = true;
            if (dailyReports==null)
                result.Data = false;

            return result;
        }

        /// <summary>
        /// 获取技术员服务单是否有日报
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianIsHasDailyReport(GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.TechnicianId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var dailyReports = await UnitWork.Find<ServiceDailyReport>(w => w.CreateUserId == userInfo.UserID && w.ServiceOrderId == req.ServiceOrderId).FirstOrDefaultAsync();

            result.Data = true;
            if (dailyReports == null)
                result.Data = false;

            return result;
        }

        /// <summary>
        /// 判断有服务单的技术员当天是否填写日报
        /// </summary>
        /// <param name="TechnicianId"></param>
        /// <returns></returns>
        public async Task<TableData> TechnicianHasWriteDailyReport(int TechnicianId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId ==TechnicianId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var ServiceWorkOrder = await UnitWork.Find<ServiceWorkOrder>(c => c.CurrentUserNsapId == userInfo.UserID && c.Status >= 2 && c.Status <= 5).FirstOrDefaultAsync();
            if (ServiceWorkOrder!=null)
            {
                var dailyReports = await UnitWork.Find<ServiceDailyReport>(w => w.CreateUserId == userInfo.UserID && w.CreateTime.Value.Date == DateTime.Now.Date).FirstOrDefaultAsync();
                if (dailyReports==null)
                    result.Data = false;
                else
                    result.Data = true;
            }
            else
            {
                result.Data = true;
            }
            return result;
        }

        private List<string> GetServiceTroubleAndSolution(string data, string objectCode)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                JArray jArray = (JArray)JsonConvert.DeserializeObject(data);
                foreach (var item in jArray)
                {
                    result.Add(item[objectCode] == null ? "" : item[objectCode].ToString());
                }
            }
            return result;
        }

        /// <summary>
        /// 自定义问题描述/解决方案
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddProblemOrSolution(AddProblemOrSolutionReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var data = new PersonProblemAndSolution
            {
                Description = req.Description,
                Type = req.Type,
                CreaterId = userInfo.UserID,
                CreateTime = DateTime.Now
            };
            await UnitWork.AddAsync(data);
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取我自定义的问题描述和解决方案
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <param name="Type"></param>
        /// <returns></returns>
        public async Task<TableData> GetMyProblemOrSolution(int AppUserId, int Type)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == AppUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var data = await UnitWork.Find<PersonProblemAndSolution>(w => w.CreaterId == userInfo.UserID && w.Type == Type && w.IsDelete == 0).OrderByDescending(c => c.CreateTime).Take(5).Select(s => new { s.Description, s.Id }).ToListAsync();
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 清空自定义问题描述和解决方案
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task ClearProblemOrSolution(AddProblemOrSolutionReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var PersonProblemAndSolution = await UnitWork.Find<PersonProblemAndSolution>(c => c.CreaterId == userInfo.UserID && c.IsDelete == 0 && c.Type == req.Type).ToListAsync();
            PersonProblemAndSolution.ForEach(c => c.IsDelete = 1);
            await UnitWork.BatchUpdateAsync(PersonProblemAndSolution.ToArray());
            await UnitWork.SaveAsync();
        }
        /// <summary>
        /// 获取用户差旅费
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task<decimal> GetUserSubsides(string userId)
        {
            decimal subsidies = 0;
            var loginUser=await UnitWork.Find<User>(u => u.Id.Equals(userId)).FirstOrDefaultAsync();
            var orgids = await UnitWork.Find<Relevance>(r => r.Key == Define.USERORG && r.FirstId == userId).Select(r => r.SecondId).ToListAsync();
            var orgname = await UnitWork.Find<OpenAuth.Repository.Domain.Org>(o => orgids.Contains(o.Id)).OrderByDescending(o => o.CascadeId).Select(o => o.Name).ToListAsync();
            // && orgname.Contains(u.Name)
            var CategoryList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_TravellingAllowance")).Select(u => new { u.Name, u.DtValue, u.Description }).ToListAsync();
            CategoryList = CategoryList.Where(u => orgname.Contains(u.Name) || u.Description.Split(",").Contains(loginUser.Name)).ToList();
            if (CategoryList != null && CategoryList.Count() >= 1)
            {
                subsidies = Convert.ToDecimal(CategoryList.FirstOrDefault().DtValue);
            }
            else
            {
                subsidies = 50;
            }
            return subsidies;
        }

        /// <summary>
        /// 添加日费
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddDailyExpends(AddDailyExpendsReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            if (req.travelExpense.Days == 1)
            {
                var dailyReport = await UnitWork.Find<ServiceDailyReport>(w => w.ServiceOrderId == req.ServiceOrderId && w.CreateUserId == userInfo.UserID && w.CreateTime.Value.Date == DateTime.Now.Date).FirstOrDefaultAsync();
                if (dailyReport == null)
                {
                    throw new Exception("当天未填写行程日报，不允许申请差补");
                }

                //服务 字典维护的客户,不能申请日费
                var customerId = (await UnitWork.Find<ServiceOrder>(s => s.Id == req.ServiceOrderId).FirstOrDefaultAsync()).CustomerId;
                var unAddDailyExpendCustomers = await UnitWork.Find<Category>(c => c.TypeId == "SYS_CannotAddDailyExpendsCustomer").Select(x => x.Name).Distinct().ToListAsync();
                if (unAddDailyExpendCustomers.Contains(customerId))
                {
                    throw new Exception("服务该客户不能申请日费");
                }
            }

            //判断当天是否已经填写日费 再次填写则数据清空
            var serviceDailyExpend = await UnitWork.Find<ServiceDailyExpends>(w => w.ServiceOrderId == req.ServiceOrderId && w.CreateUserId == userInfo.UserID && w.CreateTime.Value.Day == DateTime.Now.Day && w.CreateTime.Value.Month == DateTime.Now.Month && w.CreateTime.Value.Year == DateTime.Now.Year).ToListAsync();
            if (serviceDailyExpend.Count > 0)
            {
                //同步删除附件
                var expendIds = serviceDailyExpend.Select(s => s.Id).ToList();
                var serviceDailyAttachments = await UnitWork.Find<DailyAttachment>(w => expendIds.Contains(w.ExpendId)).ToListAsync();
                if (serviceDailyAttachments != null)
                {
                    await UnitWork.BatchDeleteAsync(serviceDailyAttachments.ToArray());
                }
                await UnitWork.BatchDeleteAsync(serviceDailyExpend.ToArray());
                await UnitWork.SaveAsync();
            }
            //差旅费
            if (req.travelExpense.Days == 1)
            {
                var num = await UnitWork.Find<ServiceDailyExpends>(w => w.ServiceOrderId != req.ServiceOrderId && w.CreateUserId == userInfo.UserID && w.CreateTime.Value.Day == DateTime.Now.Day && w.CreateTime.Value.Month == DateTime.Now.Month && w.CreateTime.Value.Year == DateTime.Now.Year && w.DailyExpenseType == 1).CountAsync();
                if (num > 0)
                {
                    throw new Exception("已添加当天出差补贴，不可重复提交。");
                }
                //获取当前用户的差旅补贴值
                var subsidies = await GetUserSubsides(userInfo.UserID);
                var travelExpenseInfo = new ServiceDailyExpends { ServiceOrderId = req.ServiceOrderId, CreateTime = DateTime.Now, CreateUserId = userInfo.User.Id, CreateUserName = userInfo.User.Name, DailyExpenseType = 1, SerialNumber = 1, Days = 1, Money = subsidies, Remark = req.travelExpense.Remark, TotalMoney = subsidies };
                await UnitWork.AddAsync(travelExpenseInfo);
                await UnitWork.SaveAsync();
            }
            //交通费
            if (req.transportExpenses.Count > 0)
            {
                int i = 1;
                foreach (var item in req.transportExpenses)
                {
                    var transportExpenseInfo = new ServiceDailyExpends { ServiceOrderId = req.ServiceOrderId, CreateTime = DateTime.Now, CreateUserId = userInfo.User.Id, CreateUserName = userInfo.User.Name, DailyExpenseType = 2, SerialNumber = i, Money = item.Money, InvoiceNumber = item.InvoiceNumber, Remark = item.Remark, From = item.From, To = item.To, InvoiceTime = item.InvoiceTime, TrafficType = item.TrafficType, FeeType = item.FeeType, Transport = item.Transport, TotalMoney = item.Money, FromLat = item.FromLat, FromLng = item.FromLng, ToLat = item.ToLat, ToLng = item.ToLng };
                    if (item.ReimburseAttachments != null)
                    {
                        var fileIds = item.ReimburseAttachments.Select(s => s.FileId).ToList();
                        var files = await UnitWork.Find<UploadFile>(w => fileIds.Contains(w.Id)).ToListAsync();
                        var transportAttachments = item.ReimburseAttachments;
                        foreach (var attach in transportAttachments)
                        {
                            attach.AttachmentName = files.Where(w => w.Id == attach.FileId).FirstOrDefault()?.FileName;
                        }
                        transportExpenseInfo.ReimburseAttachment = JsonConvert.SerializeObject(transportAttachments);
                    }
                    var o = await UnitWork.AddAsync<ServiceDailyExpends, int>(transportExpenseInfo);
                    if (item.dailyAttachments != null && item.dailyAttachments.Count > 0)
                    {
                        var dailyAttachments = item.dailyAttachments.MapToList<DailyAttachment>();
                        dailyAttachments.ForEach(r => { r.ExpendId = o.Id; r.Type = 1; r.Id = Guid.NewGuid().ToString(); });
                        await UnitWork.BatchAddAsync(dailyAttachments.ToArray());
                    }
                }
                await UnitWork.SaveAsync();
            }
            //住宿费
            if (req.hotelExpenses.Count > 0)
            {
                int i = 1;
                foreach (var item in req.hotelExpenses)
                {
                    var hotelExpenseInfo = new ServiceDailyExpends { ServiceOrderId = req.ServiceOrderId, CreateTime = DateTime.Now, CreateUserId = userInfo.User.Id, CreateUserName = userInfo.User.Name, DailyExpenseType = 3, SerialNumber = i, Money = item.Money, InvoiceNumber = item.InvoiceNumber, Remark = item.Remark, InvoiceTime = item.InvoiceTime, FeeType = item.FeeType, Days = item.Days, TotalMoney = item.Days * item.Money };
                    if (item.ReimburseAttachments != null)
                    {
                        var fileIds = item.ReimburseAttachments.Select(s => s.FileId).ToList();
                        var files = await UnitWork.Find<UploadFile>(w => fileIds.Contains(w.Id)).ToListAsync();
                        var hotelAttachments = item.ReimburseAttachments;
                        foreach (var attach in hotelAttachments)
                        {
                            attach.AttachmentName = files.Where(w => w.Id == attach.FileId).FirstOrDefault()?.FileName;
                        }
                        hotelExpenseInfo.ReimburseAttachment = JsonConvert.SerializeObject(hotelAttachments);
                    }
                    var o = await UnitWork.AddAsync<ServiceDailyExpends, int>(hotelExpenseInfo);
                    if (item.dailyAttachments != null && item.dailyAttachments.Count > 0)
                    {
                        var dailyAttachments = item.dailyAttachments.MapToList<DailyAttachment>();
                        dailyAttachments.ForEach(r => { r.ExpendId = o.Id; r.Type = 2; r.Id = Guid.NewGuid().ToString(); });
                        await UnitWork.BatchAddAsync(dailyAttachments.ToArray());
                    }
                }
                await UnitWork.SaveAsync();
            }
            //其他费用
            if (req.otherExpenses.Count > 0)
            {
                int i = 1;
                foreach (var item in req.otherExpenses)
                {
                    var otherExpenseInfo = new ServiceDailyExpends { ServiceOrderId = req.ServiceOrderId, CreateTime = DateTime.Now, CreateUserId = userInfo.User.Id, CreateUserName = userInfo.User.Name, DailyExpenseType = 4, SerialNumber = i, Money = item.Money, InvoiceNumber = item.InvoiceNumber, Remark = item.Remark, InvoiceTime = item.InvoiceTime, FeeType = item.FeeType, ExpenseCategory = item.ExpenseCategory, TotalMoney = item.Money };
                    if (item.ReimburseAttachments != null)
                    {
                        var fileIds = item.ReimburseAttachments.Select(s => s.FileId).ToList();
                        var files = await UnitWork.Find<UploadFile>(w => fileIds.Contains(w.Id)).ToListAsync();
                        var otherAttachments = item.ReimburseAttachments;
                        foreach (var attach in otherAttachments)
                        {
                            attach.AttachmentName = files.Where(w => w.Id == attach.FileId).FirstOrDefault()?.FileName;
                        }
                        otherExpenseInfo.ReimburseAttachment = JsonConvert.SerializeObject(otherAttachments);
                    }
                    var o = await UnitWork.AddAsync<ServiceDailyExpends, int>(otherExpenseInfo);
                    if (item.dailyAttachments != null && item.dailyAttachments.Count > 0)
                    {
                        var dailyAttachments = item.dailyAttachments.MapToList<DailyAttachment>();
                        dailyAttachments.ForEach(r => { r.ExpendId = o.Id; r.Type = 3; r.Id = Guid.NewGuid().ToString(); });
                        await UnitWork.BatchAddAsync(dailyAttachments.ToArray());
                    }
                }
                await UnitWork.SaveAsync();
            }
        }

        /// <summary>
        /// 获取日费详情（根据日期过滤）
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianDailyExpend(GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.TechnicianId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //获取当前时间
            var endDate = DateTime.Now;
            //获取取当前月的第一天
            DateTime startDate = new DateTime(endDate.Year, endDate.Month, 1);
            //若传入了指定年月 则取这个年月的信息
            if (!string.IsNullOrEmpty(req.Date))
            {
                DateTime date = Convert.ToDateTime(req.Date);
                startDate = new DateTime(date.Year, date.Month, 1);
                endDate = new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month));
            }
            //获取当月的所有日费信息
            var dailyExpends = await UnitWork.Find<ServiceDailyExpends>(w => w.CreateUserId == userInfo.UserID && w.ServiceOrderId == req.ServiceOrderId && w.CreateTime.Value.Date >= startDate && w.CreateTime.Value.Date <= endDate).ToListAsync();
            var dailyExpendDates = dailyExpends.OrderBy(o => o.CreateTime).Select(s => s.CreateTime?.Date.ToString("yyyy-MM-dd")).Distinct().ToList();
            var data = dailyExpends.Where(w => w.CreateTime?.Date == Convert.ToDateTime(req.Date).Date).ToList();
            List<TransportExpense> transportExpenses = new List<TransportExpense>();
            List<HotelExpense> hotelExpenses = new List<HotelExpense>();
            List<OtherExpense> otherExpenses = new List<OtherExpense>();
            TravelExpense travelExpense = new TravelExpense();
            foreach (var item in data)
            {
                switch (item.DailyExpenseType)
                {
                    case 1:
                        travelExpense = item.MapTo<TravelExpense>();
                        break;
                    case 2:
                        var transportExpenseInfo = item.MapTo<TransportExpense>();
                        transportExpenseInfo.ReimburseAttachments = JsonConvert.DeserializeObject<List<ReimburseAttachmentResp>>(item.ReimburseAttachment);
                        transportExpenses.Add(transportExpenseInfo);
                        break;
                    case 3:
                        var hotelExpensesInfo = item.MapTo<HotelExpense>();
                        hotelExpensesInfo.ReimburseAttachments = JsonConvert.DeserializeObject<List<ReimburseAttachmentResp>>(item.ReimburseAttachment);
                        hotelExpenses.Add(hotelExpensesInfo);
                        break;
                    case 4:
                        var otherExpensesInfo = item.MapTo<OtherExpense>();
                        otherExpensesInfo.ReimburseAttachments = JsonConvert.DeserializeObject<List<ReimburseAttachmentResp>>(item.ReimburseAttachment);
                        otherExpenses.Add(otherExpensesInfo);
                        break;
                }
            }
            if (travelExpense.Money == null)
            {
                //获取当前用户的差旅补贴值
                var subsidies = await GetUserSubsides(userInfo.UserID);
                travelExpense = new TravelExpense { CreateTime = DateTime.Now, Days = 0, Money = subsidies, Remark = string.Empty };
            }
            var IsDailyExpend = (await UnitWork.Find<ServiceDailyExpends>(w => w.ServiceOrderId != req.ServiceOrderId && w.CreateUserId == userInfo.UserID && w.CreateTime.Value.Day == DateTime.Now.Day && w.CreateTime.Value.Month == DateTime.Now.Month && w.CreateTime.Value.Year == DateTime.Now.Year && w.DailyExpenseType == 1).CountAsync()) > 0 ? false : true;
            var dailyExpendResp = new DailyExpendResp { DailyDates = dailyExpendDates, TravelExpense = travelExpense, TransportExpenses = transportExpenses, HotelExpenses = hotelExpenses, OtherExpenses = otherExpenses, IsFinish = data.Count > 0, IsDailyExpend = IsDailyExpend };
            result.Data = dailyExpendResp;
            return result;
        }


        /// <summary>
        /// 获取日费汇总列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceDailyExpendSum(GetTechnicianDailyReportReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            string userId = loginContext.User.Id;
            if (req.TechnicianId > 0)
            {
                //获取当前用户nsap用户信息
                var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.TechnicianId).Include(i => i.User).FirstOrDefaultAsync();
                if (userInfo == null)
                {
                    throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
                }
                userId = userInfo.UserID;
            }
            //获取服务单下的所有日费信息
            var dailyExpendSums = await UnitWork.Find<ServiceDailyExpends>(w => w.CreateUserId == userId && w.ServiceOrderId == req.ServiceOrderId).ToListAsync();
            //获取所有日费附件信息
            var dailyExpendIds = dailyExpendSums.Select(s => s.Id).ToList();
            var dailyAttachments = await UnitWork.Find<DailyAttachment>(w => dailyExpendIds.Contains(w.ExpendId)).ToListAsync();

            var data = dailyExpendSums.Select(s => new { s.CreateTime, s.CreateUserId, s.CreateUserName, s.DailyExpenseType, s.Days, s.ExpenseCategory, s.FeeType, s.From, s.Id, s.InvoiceNumber, s.InvoiceTime, s.Money, s.Remark, s.SellerName, s.SerialNumber, s.ServiceOrderId, s.To, s.Transport, s.TrafficType, s.TotalMoney, s.ReimburseAttachment, s.FromLat, s.FromLng, s.ToLat, s.ToLng, DailyAttachments = dailyAttachments.Where(w => w.ExpendId == s.Id).ToList() }).OrderByDescending(o => o.CreateTime).ToList();

            List<TransportExpense> transportExpenses = new List<TransportExpense>();
            List<HotelExpense> hotelExpenses = new List<HotelExpense>();
            List<OtherExpense> otherExpenses = new List<OtherExpense>();
            List<TravelExpense> travelExpenses = new List<TravelExpense>();
            foreach (var item in data)
            {
                switch (item.DailyExpenseType)
                {
                    case 1:
                        var travelExpenseInfo = item.MapTo<TravelExpense>();
                        if (travelExpenses.Count(c=>c.CreateTime.ToString("yyyy-MM-dd")== travelExpenseInfo.CreateTime.ToString("yyyy-MM-dd"))==0)
                        {
                            travelExpenses.Add(travelExpenseInfo);
                        }
                        break;
                    case 2:
                        var transportExpenseInfo = item.MapTo<TransportExpense>();
                        transportExpenseInfo.ReimburseAttachments = JsonConvert.DeserializeObject<List<ReimburseAttachmentResp>>(item.ReimburseAttachment);
                        transportExpenses.Add(transportExpenseInfo);
                        break;
                    case 3:
                        var hotelExpensesInfo = item.MapTo<HotelExpense>();
                        hotelExpensesInfo.ReimburseAttachments = JsonConvert.DeserializeObject<List<ReimburseAttachmentResp>>(item.ReimburseAttachment);
                        hotelExpenses.Add(hotelExpensesInfo);
                        break;
                    case 4:
                        var otherExpensesInfo = item.MapTo<OtherExpense>();
                        otherExpensesInfo.ReimburseAttachments = JsonConvert.DeserializeObject<List<ReimburseAttachmentResp>>(item.ReimburseAttachment);
                        otherExpenses.Add(otherExpensesInfo);
                        break;
                }
            }
            var dailyExpendResp = new DailyExpendResp { TravelExpenses = travelExpenses, TransportExpenses = transportExpenses, HotelExpenses = hotelExpenses, OtherExpenses = otherExpenses };
            result.Data = dailyExpendResp;
            return result;
        }

        /// <summary>
        /// 获取工单池
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceWorkOrderPool(QueryServiceWorkOrderPoolReq req)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var serviceOrder1 = UnitWork.Find<ServiceOrder>(c => c.VestInOrg == 1 && c.AllowOrNot == 0 && c.Status == 2 && c.U_SAP_ID != null && c.Supervisor == "王海涛" && c.FromId != 8)
                                    .Include(c => c.ServiceWorkOrders)
                                    .WhereIf(req.ServiceMode != null, c => c.ExpectServiceMode == req.ServiceMode)
                                    .Where(c => c.ServiceWorkOrders.Any(s => s.Status == 1 && s.FromType != 2))
                                    .Select(c => new
                                    {
                                        Services = GetServiceFromTheme(c.ServiceWorkOrders.FirstOrDefault().FromTheme),
                                        c.U_SAP_ID,
                                        c.Id,
                                        c.FromId,
                                        c.ExpectServiceMode,
                                        c.ExpectRatio,
                                        Address = c.Province + c.City + c.Area + c.Addr,
                                        c.Latitude,
                                        c.Longitude,
                                        Count = c.ServiceWorkOrders.Count,
                                        c.ServiceWorkOrders.FirstOrDefault().MaterialCode,
                                    })
                                   ;
            var serviceOrder = await serviceOrder1.ToListAsync();
            //if (req.ServiceMode != null)
            //{
            //    serviceOrder = serviceOrder.Where(c => c.ExpectServiceMode == req.ServiceMode).ToList();
            //}
            var workOrder = serviceOrder.Select(c => new
            {
                //c.ServiceWorkOrders.FirstOrDefault()?.FromTheme,
                //Services = GetServiceFromTheme(c.ServiceWorkOrders.FirstOrDefault().FromTheme),
                //c.U_SAP_ID,
                //c.Id,
                //c.FromId,
                //c.ExpectServiceMode,
                //c.ExpectRatio,
                //Address = c.Province + c.City + c.Area + c.Addr,
                //c.ServiceWorkOrders.FirstOrDefault()?.MaterialCode,
                //Count = c.ServiceWorkOrders.Count,
                c.Services,
                c.U_SAP_ID,
                c.Id,
                c.FromId,
                c.ExpectServiceMode,
                c.ExpectRatio,
                c.Address ,
                c.MaterialCode,
                c.Count,
                Distance = (req.Latitude == 0 || c.Latitude is null) ? 0 : Math.Round(NauticaUtil.GetDistance(Convert.ToDouble(c.Latitude ?? 0), Convert.ToDouble(c.Longitude ?? 0), Convert.ToDouble(req.Latitude), Convert.ToDouble(req.Longitude)) / 1000)
            });
            if (req.Distance != null)
            {
                switch (req.Distance)
                {
                    case 1:
                        workOrder = workOrder.Where(c => c.Distance <= 30);
                        break;
                    case 2:
                        workOrder = workOrder.Where(c => c.Distance <= 50);
                        break;
                    case 3:
                        workOrder = workOrder.Where(c => c.Distance <= 100);
                        break;
                    case 4:
                        workOrder = workOrder.Where(c => c.Distance <= 200);
                        break;
                    case 5:
                        workOrder = workOrder.Where(c => c.Distance <= 300);
                        break;
                    case 6:
                        workOrder = workOrder.Where(c => c.Distance > 300);
                        break;
                    default:
                        break;
                }
            }
            workOrder = workOrder.Skip((req.page - 1) * req.limit).Take(req.limit).OrderBy(c => c.U_SAP_ID).ToList();
            result.Data = workOrder;
            return result;
        }

        /// <summary>
        /// 获取工单池数量
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> ServiceWorkOrderPoolCount()
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => c.VestInOrg == 1 && c.AllowOrNot == 0 && c.Status == 2 && c.U_SAP_ID != null && c.Supervisor == "王海涛" && c.FromId != 8)
                                    .Include(c => c.ServiceWorkOrders)
                                    .Where(c => c.ServiceWorkOrders.Any(s => s.Status == 1 && s.FromType != 2))
                                    .CountAsync();
            result.Data = serviceOrder;
            return result;
        }

        /// <summary>
        /// 抢单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> TakeOrder(TakeOrder req)
        {
            Infrastructure.Response response = new Infrastructure.Response();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).Include(i => i.User).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var canSendOrder = await CheckCanTakeOrder(req.AppUserId);
            if (!canSendOrder)
            {
                response.Code = 500;
                response.Message = "抱歉，您当前单据数量已到达上限，请完成单据后再进行抢单。";
                return response;
            }
            var maxTime = await UnitWork.Find<ServiceWorkOrder>(c => c.CurrentUserId == req.AppUserId && c.Status < 7).OrderByDescending(c => c.AcceptTime).Select(c => c.AcceptTime).FirstOrDefaultAsync();
            if (maxTime != null)
            {
                TimeSpan ts = DateTime.Now.Subtract(maxTime.Value);
                if (ts.Days > 30)
                {
                    response.Code = 500;
                    response.Message = "当前存在服务单滞留超过30天，需要完成该单据后才能抢单。";
                    return response;
                }
            }
            var model = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId).Select(c => new { c.Id, c.Status }).ToListAsync();
            if (model.All(c => c.Status != 1))
            {
                response.Code = 500;
                response.Message = "抱歉，您慢了一步，该单据已被其他人抢走。";
                return response;
            }
            try
            {
                var ServiceOrderModel = await UnitWork.Find<ServiceOrder>(s => s.Id == req.ServiceOrderId).FirstOrDefaultAsync();
                if (await RedisHelper.SetNxAsync(req.ServiceOrderId.ToString(), req.ServiceOrderId.ToString()))
                {
                    var ids = model.Select(c => c.Id).ToList();

                    await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId, o => new ServiceWorkOrder
                    {
                        CurrentUser = userInfo.User.Name,
                        CurrentUserNsapId = userInfo.User.Id,
                        CurrentUserId = req.AppUserId,
                        Status = 2,
                        AcceptTime = DateTime.Now
                    });

                    await UnitWork.AddAsync<ServiceOrderParticipationRecord>(new ServiceOrderParticipationRecord
                    {
                        ServiceOrderId = req.ServiceOrderId,
                        UserId = userInfo.User.Id,
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
                        MaterialType = ""
                    });

                    await _appServiceOrderLogApp.AddAsync(new AddOrUpdateAppServiceOrderLogReq
                    {
                        Title = "技术员接单成功",
                        Details = "已接单成功，请选择服务方式：远程服务或上门服务",
                        LogType = 2,
                        ServiceOrderId = Convert.ToInt32(req.ServiceOrderId),
                        ServiceWorkOrder = string.Join(",", ids.ToArray()),
                        MaterialType = ""
                    });
                    var WorkOrderNumbers = String.Join(',', await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == req.ServiceOrderId).Select(s => s.WorkOrderNumber).ToArrayAsync());

                    await _ServiceOrderLogApp.BatchAddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"技术员{userInfo.User.Name}抢单{WorkOrderNumbers}", ActionType = "技术员抢单", MaterialType = "" }, ids);
                    await SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = Convert.ToInt32(req.ServiceOrderId), Content = $"技术员{userInfo.User.Name}抢单{WorkOrderNumbers}", AppUserId = 0 });
                    await PushMessageToApp(req.AppUserId, "派单成功提醒", "您已被派有一个新的售后服务，请尽快处理");
                }
                else
                {
                    response.Code = 500;
                    response.Message = "抱歉，您慢了一步，该单据已被其他人抢走。";
                    return response;
                }
            }
            catch (Exception e)
            {
                response.Code = 500;
                response.Message = e.Message;
            }
            finally
            {
                await RedisHelper.DelAsync(req.ServiceOrderId.ToString());
            }
            return response;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task FromThemeForServiceMode()
        {
            var query = await (from a in UnitWork.Find<ServiceOrder>(null)
                               join b in UnitWork.Find<ServiceWorkOrder>(null) on a.Id equals b.ServiceOrderId
                               where a.VestInOrg == 1 && a.AllowOrNot == 0 && a.Status == 2 && b.FromType == 1 && b.ServiceMode > 0 && a.CreateTime >= DateTime.Parse("2021-05-13 18:37:43")
                               select new { a.Id, b.FromTheme, b.ServiceMode }).ToListAsync();

            List<FromThemeRelevant> ps = new List<FromThemeRelevant>();
            query.ForEach(c =>
            {
                var codes = GetServiceTroubleAndSolution(c.FromTheme, "code");
                codes.ForEach(f =>
                {
                    ps.Add(new FromThemeRelevant { FromThemeCode = f, ServiceMode = c.ServiceMode });
                });
            });

            var list = ps.GroupBy(c => new { c.FromThemeCode, c.ServiceMode }).Select(c => new FromThemeRelevant { FromThemeCode = c.Key.FromThemeCode, ServiceMode = c.Key.ServiceMode, Count = c.Count() }).ToList();

            var origin = await UnitWork.Find<FromThemeRelevant>(null).ToListAsync();
            await UnitWork.BatchDeleteAsync(origin.ToArray());
            await UnitWork.BatchAddAsync(list.ToArray());
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 计算预计服务方式及占比
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<(int? ExpectServiceMode, decimal? ExpectRatio)> CalculateRatio(string data)
        {
            var codes = GetServiceTroubleAndSolution(data, "code");
            var mode = await UnitWork.Find<FromThemeRelevant>(c => codes.Contains(c.FromThemeCode)).ToListAsync();
            var sum = mode.Sum(c => c.Count);
            var fianl = mode.GroupBy(c => c.ServiceMode).Select(c => new { c.Key, Count = Math.Round(Convert.ToDecimal(c.Sum(s => s.Count)) / sum * 100, 2) }).OrderByDescending(c => c.Count).FirstOrDefault();
            return (fianl?.Key, fianl?.Count);
        }

        public async Task BatchModify()
        {
            var serviceOrder = await UnitWork.Find<ServiceOrder>(c => c.VestInOrg == 1 && c.AllowOrNot == 0 && c.Status == 2 && c.U_SAP_ID != null && c.Supervisor == "王海涛")
                                    .Include(c => c.ServiceWorkOrders)
                                    .Where(c => c.ServiceWorkOrders.Any(s => s.Status == 1 && s.FromType != 2))
                                    .Select(c => new { c.Id, c.ServiceWorkOrders.FirstOrDefault().FromTheme })
                                    .ToListAsync();
            foreach (var item in serviceOrder)
            {
                var ts = await CalculateRatio(item.FromTheme);
                await UnitWork.UpdateAsync<ServiceOrder>(c => c.Id == item.Id, c => new ServiceOrder
                {
                    ExpectServiceMode = ts.ExpectServiceMode,
                    ExpectRatio = ts.ExpectRatio
                });
            }
            await UnitWork.SaveAsync();
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
            var query = UnitWork.Find<ServiceOrder>(s => s.Status == 2 && s.CreateTime > Convert.ToDateTime("2020-08-01") && s.CreateTime != null && (isAdmin ? true : s.SupervisorId == nsapUserId) && s.AllowOrNot==0) //服务单已确认 
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
                    MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
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
                    Status = s.ServiceWorkOrders?.FirstOrDefault(b => "无序列号".Equals(a.Key) ? b.MaterialCode == "无序列号" : b.MaterialCode.Contains(a.Key))?.Status,
                    MaterialTypeName = "无序列号".Equals(a.Key) ? "无序列号" : MaterialTypeModel?.Where(m => m.TypeAlias == a.Key)?.FirstOrDefault()?.TypeName,
                    TechnicianId = s.ServiceWorkOrders?.FirstOrDefault(b => "无序列号".Equals(a.Key) ? b.MaterialCode == "无序列号" : b.MaterialCode.Contains(a.Key))?.CurrentUserId,
                }),
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.MaterialInfo?.FirstOrDefault()?.ProblemType?.Name : s.ProblemTypeName,
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
                            a.TerminalCustomerId,
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
                                o.FromTheme,
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
                    a.TerminalCustomerId,
                    a.ServiceOrderId,
                    CreateTime = a.CreateTime?.ToString("yyyy.MM.dd HH:mm:ss"),
                    a.Province,
                    a.City,
                    a.Area,
                    a.Addr,
                    a.NewestContacter,
                    a.NewestContactTel,
                    a.AppCustId,
                    ProblemTypeName = string.IsNullOrEmpty(a.ProblemTypeName) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Name : a.ProblemTypeName,
                    ProblemTypeId = string.IsNullOrEmpty(a.ProblemTypeId) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Id : a.ProblemTypeId,
                    a.Services,
                    a.CustomerName,
                    a.Supervisor,
                    a.SalesMan,
                    a.Longitude,
                    a.Latitude,
                    Priority = a.ServiceWorkOrders.FirstOrDefault()?.Priority == 3 ? "高" : a.ServiceWorkOrders.FirstOrDefault()?.Priority == 2 ? "中" : "低",
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                    {
                        MaterialType = string.IsNullOrEmpty(s.Key) ? "无序列号" : s.Key,
                        TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                        Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                        Count = s.Count(),
                        UnitName = "台",
                        MaterialTypeName = string.IsNullOrEmpty(s.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == s.Key).FirstOrDefault().TypeName,
                        WorkOrders = s.Select(i => i.Id).ToList(),
                        Orders = s.Select(s => new { s.Id, s.MaterialCode, s.ManufacturerSerialNumber, s.FromTheme }).ToList()
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
            if (req.Type == 0)
            {
                //判断当期单据是否已被派单 若已派单则提示做转派操作
                var isSendOrder = (await UnitWork.Find<ServiceWorkOrder>(w => w.ServiceOrderId.ToString() == req.ServiceOrderId && req.QryMaterialTypes.Contains(w.MaterialCode == "无序列号" ? "无序列号" : w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-"))) && w.CurrentUserId > 0).ToListAsync())?.Count > 0 ? true : false;
                if (isSendOrder)
                {
                    throw new CommonException("当前工单已被派单，如需继续派单请走转派流程", 60003);
                }
            }
            var canSendOrder = await CheckCanTakeOrder(req.CurrentUserId);
            if (!canSendOrder)
            {
                throw new CommonException("技术员接单已经达到上限", 60001);
            }
            var u = await UnitWork.Find<AppUserMap>(s => s.AppUserId == req.CurrentUserId).Include(s => s.User).FirstOrDefaultAsync();

            //获取当前设备类型的技术员Id
            var erpUserIds = await UnitWork.Find<ServiceWorkOrder>(w => w.ServiceOrderId.ToString() == req.ServiceOrderId && req.QryMaterialTypes.Contains(w.MaterialCode == "无序列号" ? "无序列号" : w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-"))) && w.CurrentUserId > 0).Select(s => s.CurrentUserNsapId).Distinct().ToListAsync();
            var quotationcount = await UnitWork.Find<Quotation>(q => q.ServiceOrderId == Convert.ToInt32(req.ServiceOrderId) && erpUserIds.Contains(q.CreateUserId)).CountAsync();
            if (quotationcount > 0)
            {
                throw new CommonException("技术员已领料，不可转派", 60004);
            }
            var ServiceOrderModel = await UnitWork.Find<ServiceOrder>(s => s.Id == Convert.ToInt32(req.ServiceOrderId)).FirstOrDefaultAsync();

            var Model = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.ToString() == req.ServiceOrderId && req.QryMaterialTypes.Contains(s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")))).Select(s => s.Id);
            var ids = await Model.ToListAsync();
            var canTransfer = await CheckCanTransfer(req.CurrentUserId, Convert.ToInt32(req.ServiceOrderId), string.Empty, req.QryMaterialTypes);
            if (!canTransfer)
            {
                throw new CommonException("该技术员已有转派记录不可派单", 60002);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => ids.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUser = u.User.Name,
                CurrentUserNsapId = u.User.Id,
                CurrentUserId = req.CurrentUserId,
                Status = 2,
                AcceptTime = DateTime.Now
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
            //获取当前设备类型的技术员Id
            var erpUserIds = await UnitWork.Find<ServiceWorkOrder>(w => w.ServiceOrderId.ToString() == req.ServiceOrderId && req.MaterialType.Contains(w.MaterialCode == "无序列号" ? "无序列号" : w.MaterialCode.Substring(0, w.MaterialCode.IndexOf("-"))) && w.CurrentUserId > 0).Select(s => s.CurrentUserNsapId).Distinct().ToListAsync();
            var quotationcount = await UnitWork.Find<Quotation>(q => q.ServiceOrderId == Convert.ToInt32(req.ServiceOrderId) && erpUserIds.Contains(q.CreateUserId)).CountAsync();
            if (quotationcount > 0)
            {
                throw new CommonException("技术员已领料，不可转派", 60004);
            }
            var Model = UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.ToString() == req.ServiceOrderId && req.MaterialType.Equals(s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")))).Select(s => s.Id);
            var ids = await Model.ToListAsync();
            var canTransfer = await CheckCanTransfer(req.TechnicianId, Convert.ToInt32(req.ServiceOrderId), req.MaterialType, null);
            if (!canTransfer)
            {
                throw new CommonException("该技术员已有转派记录不可派单", 60002);
            }
            await UnitWork.UpdateAsync<ServiceWorkOrder>(s => ids.Contains(s.Id), o => new ServiceWorkOrder
            {
                CurrentUser = u.User.Name,
                CurrentUserNsapId = u.User.Id,
                CurrentUserId = req.TechnicianId,
                AcceptTime = DateTime.Now
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
                ProcessDescription = workOrderInfo.ProcessDescription + content,
                OrderTakeType = 7
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
            //3.取出nSAP用户与APP用户关联的用户信息（角色为技术员/售后主管）
            var tUsers = await UnitWork.Find<AppUserMap>(u => (u.AppUserRole == 1 || u.AppUserRole == 2) && req.TechnicianId > 0 ? u.AppUserId != req.TechnicianId : true).ToListAsync();
            //4.获取定位信息（登录APP时保存的位置信息）
            //var locations = (await UnitWork.Find<RealTimeLocation>(null).OrderByDescending(o => o.CreateTime).ToListAsync()).GroupBy(g => g.AppUserId).Select(s => s.First());
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
                var appUser = us.Select(c => c.AppUserId).ToList();
                var appUserIds = tUsers.Where(u => userIds.Contains(u.UserID)).Select(u => u.AppUserId).ToList();

                var locations = (await UnitWork.Find<RealTimeLocation>(c => appUser.Contains(c.AppUserId) && c.CreateTime >= DateTime.Now.AddDays(-7) && c.CreateTime <= DateTime.Now).ToListAsync()).GroupBy(c => c.AppUserId).Select(c => c.OrderByDescending(o => o.CreateTime).First()).ToList();

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
        /// 判断当前技术员是否有转派记录
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serviceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <param name="QryMaterialTypes"></param>
        /// <returns></returns>
        private async Task<bool> CheckCanTransfer(int id, int serviceOrderId, string MaterialType, List<string> QryMaterialTypes)
        {
            if (QryMaterialTypes != null)
            {
                return (await UnitWork.Find<ServiceRedeploy>(w => QryMaterialTypes.Contains(w.MaterialType) && w.ServiceOrderId == serviceOrderId && w.TechnicianId == id).ToListAsync()).Count > 0 ? false : true;
            }
            return (await UnitWork.Find<ServiceRedeploy>(w => w.MaterialType == MaterialType && w.ServiceOrderId == serviceOrderId && w.TechnicianId == id).ToListAsync()).Count > 0 ? false : true;
        }
        #endregion

        #region<<SalesMan>>
        /// <summary>
        /// 获取业务员工单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetSalesManServiceOrder(GetSalesManServiceOrderReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            { 
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == req.AppUserId).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            //获取设备信息
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            //获取当前业务员关联的服务单集合 及业务员创建的
            var serviceOrderIds = await UnitWork.Find<ServiceOrder>(s => (s.SalesManId == userInfo.UserID || s.CreateUserId == userInfo.UserID) && s.Status == 2 && s.VestInOrg == 1 && s.AllowOrNot == 0)
                .Select(s => s.Id).Distinct().ToListAsync();
            //获取该服务单的评价
            var serviceEvaluates = await UnitWork.Find<ServiceEvaluate>(w => serviceOrderIds.Contains((int)w.ServiceOrderId)).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id))
                .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                .Include(s => s.ServiceFlows)
                .WhereIf(req.Type == 1, s => s.ServiceWorkOrders.All(a => a.Status ==1))//待处理
                .WhereIf(req.Type == 2, s => s.ServiceWorkOrders.All(a => a.Status < 7 && a.Status>=2))//进行中 
                .WhereIf(req.Type == 3, s => s.ServiceWorkOrders.All(a => a.Status >= 7))//已完成 服务单中所有工单均已完成
                .WhereIf(int.TryParse(req.key, out int id) || !string.IsNullOrWhiteSpace(req.key), s => s.U_SAP_ID == id || s.CustomerName.Contains(req.key) || s.ServiceWorkOrders.Any(o => o.ManufacturerSerialNumber.Contains(req.key)))
                //.Where(s=>s.VestInOrg==1 && s.AllowOrNot==0)
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
                    MaterialInfo = s.ServiceWorkOrders.Select(o => new
                    {
                        o.CurrentUserId,
                        o.MaterialCode,
                        o.ManufacturerSerialNumber,
                        MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                        o.Status,
                        o.Id,
                        o.OrderTakeType,
                        o.ServiceMode
                    }),
                    s.ProblemTypeName,
                    ProblemType = s.ServiceWorkOrders.Select(s => s.ProblemType).FirstOrDefault(),
                    ServiceFlows = s.ServiceFlows.Where(w => w.ServiceOrderId == s.Id && w.FlowType == 1).ToList()
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
                ProblemTypeName = string.IsNullOrEmpty(s.ProblemTypeName) ? s.ProblemType?.Name : s.ProblemTypeName,
                MaterialTypeQty = s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count,
                MaterialInfo = s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                .Select(o => new
                {
                    MaterialType = o.Key,
                    Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                    MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                    OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                    ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                    flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList(),
                    TechnicianId = o.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault()
                }),
                IsCanEvaluate = serviceEvaluates.Where(w => w.ServiceOrderId == s.Id).ToList().Count > 0 ? 1 : 0,
                EvaluateId = serviceEvaluates.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).FirstOrDefault() == null ? 0 : serviceEvaluates.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).FirstOrDefault().Id
            }).ToList();

            var count = await query.CountAsync();
            result.Data = list;
            result.Count = count;
            return result;
        }

        /// <summary>
        /// 获取服务单设备类型列表（业务员查看）
        /// </summary>
        /// <param name="ServiceOrderId"></param>
        /// <param name="CurrentUserId"></param>
        /// <returns></returns>
        public async Task<TableData> AppSalesManLoad(int ServiceOrderId, int CurrentUserId)
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
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            //获取当前服务单的售后进度
            var flowList = await UnitWork.Find<ServiceFlow>(s => s.ServiceOrderId == ServiceOrderId && s.FlowType == 2).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed, s.MaterialType }).ToListAsync();
            var query = UnitWork.Find<ServiceOrder>(s => s.Id == ServiceOrderId)
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
                            a.TerminalCustomer,
                            a.TerminalCustomerId,
                            a.CustomerId,
                            NewestContacter = string.IsNullOrEmpty(a.NewestContacter) ? a.Contacter : a.NewestContacter,
                            NewestContactTel = string.IsNullOrEmpty(a.NewestContactTel) ? a.ContactTel : a.NewestContactTel,
                            AppCustId = a.AppUserId,
                            ServiceWorkOrders = a.ServiceWorkOrders.Select(o => new
                            {
                                o.Id,
                                o.Status,
                                o.FromTheme,
                                o.ManufacturerSerialNumber,
                                o.MaterialCode,
                                o.MaterialDescription,
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
                    a.CustomerId,
                    a.TerminalCustomer,
                    a.TerminalCustomerId,
                    ProblemTypeName = string.IsNullOrEmpty(a.ProblemTypeName) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Name : a.ProblemTypeName,
                    ProblemTypeId = string.IsNullOrEmpty(a.ProblemTypeId) ? a.ServiceWorkOrders.FirstOrDefault()?.ProblemType?.Id : a.ProblemTypeId,
                    Services = GetServiceFromTheme(a.ServiceWorkOrders.FirstOrDefault()?.FromTheme),
                    Priority = a.ServiceWorkOrders.FirstOrDefault()?.Priority == 3 ? "高" : a.ServiceWorkOrders.FirstOrDefault()?.Priority == 2 ? "中" : "低",
                    ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                    {
                        MaterialType = string.IsNullOrEmpty(s.Key) ? "无序列号" : s.Key,
                        TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                        Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                        Count = s.Count(),
                        Orders = s.ToList(),
                        UnitName = "台",
                        MaterialTypeName = string.IsNullOrEmpty(s.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == s.Key).FirstOrDefault().TypeName,
                        ServiceMode = s.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                        flowinfo = flowList.Where(w => w.MaterialType == (string.IsNullOrEmpty(s.Key) ? "无序列号" : s.Key)).ToList()
                    }
                    ).ToList()
                });
            result.Count = count;
            result.Data = list;
            return result;
        }


        /// <summary>
        /// 获取业务员关联服务单据数量
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        public async Task<TableData> GetSalesManServiceOrderCount(int AppUserId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == AppUserId).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            var serviceOrderIds = await UnitWork.Find<ServiceOrder>(s => (s.SalesManId == userInfo.UserID || s.CreateUserId == userInfo.UserID) && s.VestInOrg == 1 && s.AllowOrNot == 0 && s.Status == 2)
               .Select(s => s.Id).ToListAsync();
            result.Data = serviceOrderIds.Count;
            return result;
        }
        #endregion

        #region App服务单搜索
        /// <summary>
        /// App销售首页搜索服务单
        /// </summary>
        /// <param name="key"></param>
        /// <param name="role">0-普通用户 1-ERP用户 2-技术员 3-技术员管理员 4-客服 5-销售员 6-E3工程师</param>
        /// <param name="AppUserId"></param>
        /// <returns></returns>
        public async Task<TableData> AppSearchServiceOrder(string key,int role, int AppUserId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            int service_counts = 0;
            List<object> list = new List<object>();
            int page_size = 3;
            int.TryParse(key, out int id);
            if (id<=0)
            {
                result.Data = new { service = list, service_counts = service_counts };
                return result;
            }
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == AppUserId).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                if (role!=0)
                {
                    throw new CommonException("未绑定ERP账户", Define.INVALID_APPUser);
                }
            }
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            switch (role)
            {
                case 0:
                    var customer_order = UnitWork.Find<ServiceOrder>(s => s.AppUserId == AppUserId)
                                .Include(s => s.ServiceWorkOrders)
                                .Where(s => s.Status == 2)
                                .Where(s => s.U_SAP_ID.Value.ToString().Contains(id.ToString()))
                                .Where(q => q.ServiceWorkOrders.All(a => a.FromType == 1) && q.ServiceWorkOrders.Count > 0)
                                .Select(a => new
                                {
                                    a.Id,
                                    a.AppUserId,
                                    a.Status,
                                    a.U_SAP_ID,
                                    a.ProblemTypeId,
                                    a.ProblemTypeName,
                                    ServiceWorkOrders = a.ServiceWorkOrders.Select(o => new
                                    {
                                        o.Id,
                                        o.ServiceOrderId,
                                        o.Status,
                                        o.FromTheme,
                                        ProblemType = o.ProblemType.Description,
                                        o.ManufacturerSerialNumber,
                                        o.MaterialCode,
                                        o.CurrentUserId,
                                        o.OrderTakeType,
                                        MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-"))
                                    }).OrderByDescending(a => a.Id).ToList(),
                                });
                    list = (await customer_order
                        .Take(page_size)
                        .ToListAsync())
                        .Select(a => new
                        {
                            a.Id,
                            a.AppUserId,
                            a.Status,
                            a.U_SAP_ID,
                            ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                            {
                                MaterialType = s.Key,
                                MaterialTypeName = "无序列号".Equals(s.Key) ? "无序列号" : MaterialTypeModel.Where(m => m.TypeAlias == s.Key).FirstOrDefault().TypeName,
                                TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                                Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                                Count = s.Count(),
                                Orders = s.ToList()
                            }).ToList(),
                            WorkOrderState = a.ServiceWorkOrders.Distinct().OrderBy(o => o.Status).FirstOrDefault()?.Status,
                        }).ToList<object>();
                    service_counts = customer_order.Count() - list.Count();
                    result.Data = new {service= list,service_counts= service_counts };
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 6:
                    var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == AppUserId && s.FromType == 1)
                        .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                    var completeReportList = await UnitWork.Find<CompletionReport>(w => serviceOrderIds.Contains((int)w.ServiceOrderId)).Select(s => new { s.ServiceOrderId, s.TechnicianId, s.IsReimburse, s.Id, s.ServiceMode, MaterialType = s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) }).ToListAsync();
                    var reimburseList = await UnitWork.Find<ReimburseInfo>(r => r.CreateUserId == userInfo.UserID).ToListAsync();
                    var query = UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id) && w.AllowOrNot == 0)
                        .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                        .Include(s => s.ServiceFlows)
                        .Where(s => s.Status == 2)
                        .Where(s => s.U_SAP_ID.Value.ToString().Contains(id.ToString()))
                        .WhereIf(role!=6, s=>s.VestInOrg != 2)
                        .Select(s => new
                        {
                            s.Id,
                            s.AppUserId,
                            s.Status,
                            s.U_SAP_ID,
                            s.VestInOrg,
                            MaterialCode = s.VestInOrg == 2 ? s.ServiceWorkOrders.Where(c => c.ServiceOrderId == s.Id).FirstOrDefault().MaterialCode : "",
                            Count = s.ServiceWorkOrders.Where(w => w.ServiceOrderId == s.Id && w.CurrentUserId ==AppUserId).Count(),
                            MaterialInfo = s.ServiceWorkOrders.Where(w => w.CurrentUserId == AppUserId).Select(o => new
                            {
                                o.MaterialCode,
                                o.ManufacturerSerialNumber,
                                MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                                o.Status,
                                o.Id,
                                o.OrderTakeType,
                                o.ServiceMode,
                                o.Priority,
                                o.TransactionType,
                                o.FromTheme,
                                o.ServiceOrderId,
                            }),
                            ServiceFlows = s.ServiceFlows.Where(w => w.Creater == userInfo.UserID && w.ServiceOrderId == s.Id && w.FlowType == 1).ToList()
                        });
                    list = (await query.OrderByDescending(o => o.Id)
                    .Take(page_size).ToListAsync()).Select(s => new
                    {
                        s.Id,
                        s.AppUserId,
                        s.Status,
                        s.U_SAP_ID,
                        s.VestInOrg,
                        s.MaterialCode,
                        MaterialTypeQty = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count : 0,
                        MaterialInfo = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                        .Select(o => new
                        {
                            MaterialType = o.Key,
                            Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                            MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                            OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                            ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                            flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList()
                        }) : new object(),
                        IsReimburse = (s.VestInOrg == 1 || s.VestInOrg == 3) ? completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == AppUserId.ToString()).FirstOrDefault() == null ? 0 : completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == AppUserId.ToString()).FirstOrDefault().IsReimburse : 0,
                        MaterialType = s.VestInOrg == 1 ? completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == AppUserId.ToString()).FirstOrDefault() == null ? string.Empty : completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == AppUserId.ToString()).OrderBy(o => o.ServiceMode).FirstOrDefault().MaterialType : string.Empty,
                        ReimburseId =(s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.Id).FirstOrDefault() : 0,
                        RemburseStatus = (s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.RemburseStatus).FirstOrDefault() : 0,
                        RemburseIsRead = (s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.IsRead).FirstOrDefault() : 0,
                        orderStatus = s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.OrderTakeType == 0) == true ? 0 : (s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.Status >= 7) == true ? 2 : 1)
                    }).ToList<object>();
                    service_counts = query.Count() - list.Count();
                    result.Data = new { service = list, service_counts = service_counts };
                    break;
                case 5:
                    var customer_serviceOrderIds = await UnitWork.Find<ServiceOrder>(s => s.SalesManId == userInfo.UserID)
                        .Select(s => s.Id).Distinct().ToListAsync();
                    var serviceEvaluates = await UnitWork.Find<ServiceEvaluate>(w => customer_serviceOrderIds.Contains((int)w.ServiceOrderId)).ToListAsync();
                    var sale_order = UnitWork.Find<ServiceOrder>(w => customer_serviceOrderIds.Contains(w.Id) && w.Status == 2)
                        .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                        .Include(s => s.ServiceFlows)
                        .Where(s => s.U_SAP_ID.Value.ToString().Contains(id.ToString()))
                        .Where(s => s.VestInOrg == 1 && s.AllowOrNot == 0)
                        .Where(q => q.ServiceWorkOrders.All(a => a.CurrentUserId != AppUserId) && q.ServiceWorkOrders.Count > 0)
                        .Select(s => new
                        {
                            s.Id,
                            s.AppUserId,
                            s.Status,
                            s.U_SAP_ID,
                            s.VestInOrg,
                            Count =0,
                            MaterialInfo = s.ServiceWorkOrders.Select(o => new
                            {
                                o.CurrentUserId,
                                o.MaterialCode,
                                o.ManufacturerSerialNumber,
                                MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                                o.Status,
                                o.Id,
                                o.OrderTakeType,
                                o.ServiceMode,
                                o.ServiceOrderId
                            }),
                            ServiceFlows = s.ServiceFlows.Where(w => w.ServiceOrderId == s.Id && w.FlowType == 1).ToList()
                        });
                    var customer_list = (await sale_order.OrderByDescending(o => o.Id)
                    .ToListAsync()).Select(s => new
                    {
                        s.Id,
                        s.AppUserId,
                        s.U_SAP_ID,
                        s.Status,
                        s.VestInOrg,
                        s.Count,
                        MaterialTypeQty = s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count,
                        MaterialInfo = s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                        .Select(o => new
                        {
                            MaterialType = o.Key,
                            Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                            MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                            OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                            ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                            flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList(),
                            TechnicianId = o.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault()
                        }),
                        IsCanEvaluate = serviceEvaluates.Where(w => w.ServiceOrderId == s.Id).ToList().Count > 0 ? 1 : 0,
                        EvaluateId = serviceEvaluates.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).FirstOrDefault() == null ? 0 : serviceEvaluates.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).FirstOrDefault().Id,
                        type=1,
                        IsReimburse = 0,
                        MaterialType = string.Empty,
                        ReimburseId = 0,
                        RemburseStatus =  0,
                        RemburseIsRead = 0,
                        orderStatus = s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.OrderTakeType == 0) == true ? 0 : (s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.Status >= 7) == true ? 2 : 1)
                    }).ToList<object>();

                    var sale_serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == AppUserId && s.FromType == 1)
                        .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                    var sale_completeReportList = await UnitWork.Find<CompletionReport>(w => sale_serviceOrderIds.Contains((int)w.ServiceOrderId)).Select(s => new { s.ServiceOrderId, s.TechnicianId, s.IsReimburse, s.Id, s.ServiceMode, MaterialType = s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) }).ToListAsync();
                    var sale_reimburseList = await UnitWork.Find<ReimburseInfo>(r => r.CreateUserId == userInfo.UserID).ToListAsync();
                    var sale_query = UnitWork.Find<ServiceOrder>(w => sale_serviceOrderIds.Contains(w.Id) && w.AllowOrNot == 0)
                        .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                        .Include(s => s.ServiceFlows)
                        .Where(s => s.Status == 2 && s.VestInOrg != 2)
                        .Where(s => s.U_SAP_ID.Value.ToString().Contains(id.ToString()))
                        .Select(s => new
                        {
                            s.Id,
                            s.AppUserId,
                            s.U_SAP_ID,
                            s.Status,
                            s.VestInOrg,
                            MaterialCode = s.VestInOrg == 2 ? s.ServiceWorkOrders.Where(c => c.ServiceOrderId == s.Id).FirstOrDefault().MaterialCode : "",
                            Count = s.ServiceWorkOrders.Where(w => w.ServiceOrderId == s.Id && w.CurrentUserId == AppUserId).Count(),
                            MaterialInfo = s.ServiceWorkOrders.Where(w => w.CurrentUserId == AppUserId).Select(o => new
                            {
                                o.MaterialCode,
                                o.ManufacturerSerialNumber,
                                MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                                o.Status,
                                o.Id,
                                o.OrderTakeType,
                                o.ServiceMode,
                                o.Priority,
                                o.TransactionType,
                                o.FromTheme,
                                o.ServiceOrderId
                            }),
                            ServiceFlows = s.ServiceFlows.Where(w => w.Creater == userInfo.UserID && w.ServiceOrderId == s.Id && w.FlowType == 1).ToList(),
                        });
                    var sale_list = (await sale_query.OrderByDescending(o => o.Id)
                    .ToListAsync()).Select(s => new
                    {
                        s.Id,
                        s.AppUserId,
                        s.Status,
                        s.U_SAP_ID,
                        s.VestInOrg,
                        s.MaterialCode,
                        MaterialTypeQty = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count : 0,
                        MaterialInfo = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                        .Select(o => new
                        {
                            MaterialType = o.Key,
                            Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                            MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                            OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                            ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                            flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList()
                        }) : new object(),
                        IsCanEvaluate =  0,
                        EvaluateId =0,
                        type = 2,
                        IsReimburse = (s.VestInOrg == 1 || s.VestInOrg == 3) ? sale_completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == AppUserId.ToString()).FirstOrDefault() == null ? 0 : sale_completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == AppUserId.ToString()).FirstOrDefault().IsReimburse : 0,
                        MaterialType = s.VestInOrg == 1 ? sale_completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == AppUserId.ToString()).FirstOrDefault() == null ? string.Empty : sale_completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == AppUserId.ToString()).OrderBy(o => o.ServiceMode).FirstOrDefault().MaterialType : string.Empty,
                        ReimburseId = (s.VestInOrg == 1 || s.VestInOrg == 3) ? sale_reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.Id).FirstOrDefault() : 0,
                        RemburseStatus = (s.VestInOrg == 1 || s.VestInOrg == 3) ? sale_reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.RemburseStatus).FirstOrDefault() : 0,
                        RemburseIsRead = (s.VestInOrg == 1 || s.VestInOrg == 3) ? sale_reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.IsRead).FirstOrDefault() : 0,
                        orderStatus = s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.OrderTakeType == 0) == true ? 0 : (s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.Status >= 7) == true ? 2 : 1)
                    }).ToList<object>();
                    list=sale_list.Union(customer_list).Take(page_size).ToList();
                    service_counts = customer_list.Count() + sale_list.Count() - list.Count();
                    result.Data = new { service = list, service_counts = service_counts };
                    break;
                default:
                    break;
            }
            return result;
        }

        /// <summary>
        /// App销售首页搜索服务单
        /// </summary>
        /// <param name="key"></param>
        /// <param name="role">0-普通用户 1-ERP用户 2-技术员 3-技术员管理员 4-客服 5-销售员 6-E3工程师</param>
        /// <param name="AppUserId"></param>
        /// <param name="page_index"></param>
        /// <param name="page_size"></param>
        /// <returns></returns>
        public async Task<TableData> SearchServiceOrderById(string key, int role, int AppUserId,int page_index,int page_size)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            List<object> list = new List<object>();
            int.TryParse(key, out int id);
            if (id <= 0)
            {
                result.Data = new { service = list};
                return result;
            }
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == AppUserId).FirstOrDefaultAsync();
            if (userInfo == null)
            {
                if (role != 0)
                {
                    throw new CommonException("未绑定ERP账户", Define.INVALID_APPUser);
                }
            }
            var MaterialTypeModel = await UnitWork.Find<MaterialType>(null).Select(u => new { u.TypeAlias, u.TypeName }).ToListAsync();
            switch (role)
            {
                case 0:
                    var customer_order = UnitWork.Find<ServiceOrder>(s => s.AppUserId == AppUserId)
                                .Include(s => s.ServiceWorkOrders)
                                .Where(s => s.Status == 2)
                                .Where(s => s.U_SAP_ID.Value.ToString().Contains(id.ToString()))
                                .Where(q => q.ServiceWorkOrders.All(a => a.FromType == 1) && q.ServiceWorkOrders.Count > 0)
                                .Select(a => new
                                {
                                    a.Id,
                                    a.AppUserId,
                                    a.Status,
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
                                        MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-"))
                                    }).OrderByDescending(a => a.Id).ToList(),
                                });
                    list = (await customer_order
                        .Skip((page_index - 1) * page_size).Take(page_size)
                        .ToListAsync())
                        .Select(a => new
                        {
                            a.Id,
                            a.AppUserId,
                            a.Status,
                            a.U_SAP_ID,
                            ServiceWorkOrders = a.ServiceWorkOrders.GroupBy(o => o.MaterialType).Select(s => new
                            {
                                MaterialType = s.Key,
                                MaterialTypeName = "无序列号".Equals(s.Key) ? "无序列号" : MaterialTypeModel.Where(m => m.TypeAlias == s.Key).FirstOrDefault().TypeName,
                                TechnicianId = s.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault(),
                                Status = s.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                                Count = s.Count(),
                                Orders = s.ToList()
                            }).ToList(),
                            WorkOrderState = a.ServiceWorkOrders.Distinct().OrderBy(o => o.Status).FirstOrDefault()?.Status
                        }).ToList<object>();
                    result.Data = new { service = list};
                    break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 6:
                    var serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == AppUserId && s.FromType == 1)
                        .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                    var completeReportList = await UnitWork.Find<CompletionReport>(w => serviceOrderIds.Contains((int)w.ServiceOrderId)).Select(s => new { s.ServiceOrderId, s.TechnicianId, s.IsReimburse, s.Id, s.ServiceMode, MaterialType = s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) }).ToListAsync();
                    var reimburseList = await UnitWork.Find<ReimburseInfo>(r => r.CreateUserId == userInfo.UserID).ToListAsync();
                    var query = UnitWork.Find<ServiceOrder>(w => serviceOrderIds.Contains(w.Id) && w.AllowOrNot == 0)
                        .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                        .Include(s => s.ServiceFlows)
                        .Where(s => s.Status == 2)
                        .Where(s => s.U_SAP_ID.Value.ToString().Contains(id.ToString()))
                        .WhereIf(role != 6, s => s.VestInOrg != 2)
                        .Select(s => new
                        {
                            s.Id,
                            s.AppUserId,
                            s.Status,
                            s.U_SAP_ID,
                            s.VestInOrg,
                            MaterialCode = s.VestInOrg == 2 ? s.ServiceWorkOrders.Where(c => c.ServiceOrderId == s.Id).FirstOrDefault().MaterialCode : "",
                            Count = s.ServiceWorkOrders.Where(w => w.ServiceOrderId == s.Id && w.CurrentUserId == AppUserId).Count(),
                            MaterialInfo = s.ServiceWorkOrders.Where(w => w.CurrentUserId == AppUserId).Select(o => new
                            {
                                o.MaterialCode,
                                o.ManufacturerSerialNumber,
                                MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                                o.Status,
                                o.Id,
                                o.OrderTakeType,
                                o.ServiceMode,
                                o.Priority,
                                o.TransactionType,
                                o.FromTheme,
                                o.ServiceOrderId
                            }),
                            ServiceFlows = s.ServiceFlows.Where(w => w.Creater == userInfo.UserID && w.ServiceOrderId == s.Id && w.FlowType == 1).ToList()
                        });
                    list = (await query.OrderByDescending(o => o.Id)
                        .Skip((page_index - 1) * page_size).Take(page_size)
                    .ToListAsync()).Select(s => new
                    {
                        s.Id,
                        s.AppUserId,
                        s.Status,
                        s.U_SAP_ID,
                        s.VestInOrg,
                        s.MaterialCode,
                        MaterialTypeQty = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count : 0,
                        MaterialInfo = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                        .Select(o => new
                        {
                            MaterialType = o.Key,
                            Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                            MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                            OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                            ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                            flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList()
                        }) : new object(),
                        IsReimburse = (s.VestInOrg == 1 || s.VestInOrg == 3) ? completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == AppUserId.ToString()).FirstOrDefault() == null ? 0 : completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == AppUserId.ToString()).FirstOrDefault().IsReimburse : 0,
                        MaterialType = s.VestInOrg == 1 ? completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == AppUserId.ToString()).FirstOrDefault() == null ? string.Empty : completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == AppUserId.ToString()).OrderBy(o => o.ServiceMode).FirstOrDefault().MaterialType : string.Empty,
                        ReimburseId = (s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.Id).FirstOrDefault() : 0,
                        RemburseStatus = (s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.RemburseStatus).FirstOrDefault() : 0,
                        RemburseIsRead = (s.VestInOrg == 1 || s.VestInOrg == 3) ? reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.IsRead).FirstOrDefault() : 0,
                        orderStatus = s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.OrderTakeType == 0) == true ? 0 : (s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.Status >= 7) == true ? 2 : 1)
                    }).ToList<object>();
                    result.Data = new { service = list};
                    break;
                case 5:
                    var customer_serviceOrderIds = await UnitWork.Find<ServiceOrder>(s => s.SalesManId == userInfo.UserID)
                        .Select(s => s.Id).Distinct().ToListAsync();
                    var serviceEvaluates = await UnitWork.Find<ServiceEvaluate>(w => customer_serviceOrderIds.Contains((int)w.ServiceOrderId)).ToListAsync();
                    var sale_order = UnitWork.Find<ServiceOrder>(w => customer_serviceOrderIds.Contains(w.Id) && w.Status == 2)
                        .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                        .Include(s => s.ServiceFlows)
                        .Where(s => s.U_SAP_ID.Value.ToString().Contains(id.ToString()))
                        .Where(s => s.VestInOrg == 1 && s.AllowOrNot == 0)
                        .Where(q => q.ServiceWorkOrders.All(a => a.CurrentUserId != AppUserId) && q.ServiceWorkOrders.Count > 0)
                        .Select(s => new
                        {
                            s.Id,
                            s.AppUserId,
                            s.Status,
                            s.U_SAP_ID,
                            s.VestInOrg,
                            Count = 0,
                            MaterialInfo = s.ServiceWorkOrders.Select(o => new
                            {
                                o.CurrentUserId,
                                o.MaterialCode,
                                o.ManufacturerSerialNumber,
                                MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                                o.Status,
                                o.Id,
                                o.OrderTakeType,
                                o.ServiceMode,
                                o.ServiceOrderId
                            }),
                            ServiceFlows = s.ServiceFlows.Where(w => w.ServiceOrderId == s.Id && w.FlowType == 1).ToList()
                        });
                    var customer_list = (await sale_order.OrderByDescending(o => o.Id)
                    .ToListAsync()).Select(s => new
                    {
                        s.Id,
                        s.AppUserId,
                        s.U_SAP_ID,
                        s.Status,
                        s.VestInOrg,
                        s.Count,
                        MaterialTypeQty = s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count,
                        MaterialInfo = s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                        .Select(o => new
                        {
                            MaterialType = o.Key,
                            Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                            MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                            OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                            ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                            flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList(),
                            TechnicianId = o.ToList().Select(s => s.CurrentUserId).Distinct().FirstOrDefault()
                        }),
                        IsCanEvaluate = serviceEvaluates.Where(w => w.ServiceOrderId == s.Id).ToList().Count > 0 ? 1 : 0,
                        EvaluateId = serviceEvaluates.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).FirstOrDefault() == null ? 0 : serviceEvaluates.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).FirstOrDefault().Id,
                        type = 1,
                        IsReimburse = 0,
                        MaterialType = string.Empty,
                        ReimburseId = 0,
                        RemburseStatus = 0,
                        RemburseIsRead = 0,
                        orderStatus = s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.OrderTakeType == 0) == true ? 0 : (s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.Status >= 7) == true ? 2 : 1)
                    }).ToList<object>();

                    var sale_serviceOrderIds = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == AppUserId && s.FromType == 1)
                        .Select(s => s.ServiceOrderId).Distinct().ToListAsync();
                    var sale_completeReportList = await UnitWork.Find<CompletionReport>(w => sale_serviceOrderIds.Contains((int)w.ServiceOrderId)).Select(s => new { s.ServiceOrderId, s.TechnicianId, s.IsReimburse, s.Id, s.ServiceMode, MaterialType = s.MaterialCode == "无序列号" ? "无序列号" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) }).ToListAsync();
                    var sale_reimburseList = await UnitWork.Find<ReimburseInfo>(r => r.CreateUserId == userInfo.UserID).ToListAsync();
                    var sale_query = UnitWork.Find<ServiceOrder>(w => sale_serviceOrderIds.Contains(w.Id) && w.AllowOrNot == 0)
                        .Include(s => s.ServiceWorkOrders).ThenInclude(s => s.ProblemType)
                        .Include(s => s.ServiceFlows)
                        .Where(s => s.Status == 2 && s.VestInOrg != 2)
                        .Where(s => s.U_SAP_ID.Value.ToString().Contains(id.ToString()))
                        .Select(s => new
                        {
                            s.Id,
                            s.AppUserId,
                            s.U_SAP_ID,
                            s.Status,
                            s.VestInOrg,
                            MaterialCode = s.VestInOrg == 2 ? s.ServiceWorkOrders.Where(c => c.ServiceOrderId == s.Id).FirstOrDefault().MaterialCode : "",
                            Count = s.ServiceWorkOrders.Where(w => w.ServiceOrderId == s.Id && w.CurrentUserId == AppUserId).Count(),
                            MaterialInfo = s.ServiceWorkOrders.Where(w => w.CurrentUserId == AppUserId).Select(o => new
                            {
                                o.MaterialCode,
                                o.ManufacturerSerialNumber,
                                MaterialType = "无序列号".Equals(o.MaterialCode) ? "无序列号" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                                o.Status,
                                o.Id,
                                o.OrderTakeType,
                                o.ServiceMode,
                                o.Priority,
                                o.TransactionType,
                                o.FromTheme,
                                o.ServiceOrderId
                            }),
                            ServiceFlows = s.ServiceFlows.Where(w => w.Creater == userInfo.UserID && w.ServiceOrderId == s.Id && w.FlowType == 1).ToList()
                        });
                    var sale_list = (await sale_query.OrderByDescending(o => o.Id)
                    .ToListAsync()).Select(s => new
                    {
                        s.Id,
                        s.AppUserId,
                        s.Status,
                        s.U_SAP_ID,
                        s.VestInOrg,
                        s.MaterialCode,
                        MaterialTypeQty = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).Select(i => i.Key).ToList().Count : 0,
                        MaterialInfo = s.VestInOrg == 1 ? s.MaterialInfo.GroupBy(o => o.MaterialType).ToList()
                        .Select(o => new
                        {
                            MaterialType = o.Key,
                            Status = o.ToList().Select(s => s.Status).Distinct().FirstOrDefault(),
                            MaterialTypeName = "无序列号".Equals(o.Key) ? "无序列号" : MaterialTypeModel.Where(a => a.TypeAlias == o.Key).FirstOrDefault().TypeName,
                            OrderTakeType = o.ToList().Select(s => s.OrderTakeType).Distinct().FirstOrDefault(),
                            ServiceMode = o.ToList().Select(s => s.ServiceMode).Distinct().FirstOrDefault(),
                            flowInfo = s.ServiceFlows.Where(w => w.MaterialType.Equals(o.Key)).OrderBy(o => o.Id).Select(s => new { s.FlowNum, s.FlowName, s.IsProceed }).ToList()
                        }) : new object(),
                        IsCanEvaluate = 0,
                        EvaluateId = 0,
                        type = 2,
                        IsReimburse = (s.VestInOrg == 1 || s.VestInOrg == 3) ? sale_completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == AppUserId.ToString()).FirstOrDefault() == null ? 0 : sale_completeReportList.Where(w => w.ServiceOrderId == s.Id && (w.ServiceMode == 1 || s.VestInOrg == 3) && w.TechnicianId == AppUserId.ToString()).FirstOrDefault().IsReimburse : 0,
                        MaterialType = s.VestInOrg == 1 ? sale_completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == AppUserId.ToString()).FirstOrDefault() == null ? string.Empty : sale_completeReportList.Where(w => w.ServiceOrderId == s.Id && w.TechnicianId == AppUserId.ToString()).OrderBy(o => o.ServiceMode).FirstOrDefault().MaterialType : string.Empty,
                        ReimburseId = (s.VestInOrg == 1 || s.VestInOrg == 3) ? sale_reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.Id).FirstOrDefault() : 0,
                        RemburseStatus = (s.VestInOrg == 1 || s.VestInOrg == 3) ? sale_reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.RemburseStatus).FirstOrDefault() : 0,
                        RemburseIsRead = (s.VestInOrg == 1 || s.VestInOrg == 3) ? sale_reimburseList.Where(w => w.ServiceOrderId == s.Id && w.CreateUserId == userInfo.UserID).Select(s => s.IsRead).FirstOrDefault() : 0,
                        orderStatus = s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.OrderTakeType == 0) == true ? 0 : (s.MaterialInfo.Where(c => c.ServiceOrderId == s.Id).All(c => c.Status >= 7) == true ? 2 : 1)
                    }).ToList<object>();
                    list = sale_list.Union(customer_list)
                        .Skip((page_index - 1) * page_size).Take(page_size)
                        .ToList();
                    result.Data = new { service = list};
                    break;
                default:
                    break;
            }
            return result;
        }
        #endregion


        #region ProblemHelp
        public async Task<TableData> SetWorkOrderFinlish(string ids)
        {
            var idslist = ids.Split(",").ToList();
            var obj = await UnitWork.Find<ServiceOrder>(c => idslist.Contains(c.U_SAP_ID.ToString())).Select(c => c.Id).ToListAsync();
            var param = string.Join(",", obj);
            var sql = $"UPDATE serviceworkorder SET `Status`=7 where ServiceOrderId in ({param})";
            return new TableData
            {
                Data = sql
            };

        }
        #endregion

        public async Task TongBu()
        {
            var sql = $@"SELECT  * FROM OSCL where createDate>='2022-06-05 00:00:00' and createDate<='2022-06-27 00:00:00'";//and callid=124477
            var OSCLs = await UnitWork.Query<OSCL>(sql).Select(c => new { c.callID, c.createDate, c.custmrName, c.customer, c.descrption, c.priority, c.subject, c.itemCode, c.itemName, c.Telephone }).ToListAsync();
            //var OSCL = await UnitWork.Find<OSCL>(c => c.createDate >= DateTime.Parse("2022-06-05 00:00:000") && c.createDate <= DateTime.Parse("2022-06-27 00:00:000") && c.callID == 124477).Select(c => new { c.callID, c.createDate, c.custmrName, c.customer, c.descrption, c.priority, c.subject, c.itemCode, c.itemName }).FirstOrDefaultAsync();
            List<ServiceOrder> serviceOrders = new List<ServiceOrder>();
            foreach (var OSCL in OSCLs)
            {
                ServiceOrder serviceOrder = new ServiceOrder
                {
                    U_SAP_ID = OSCL.callID,
                    CreateTime = OSCL.createDate,
                    CustomerId = OSCL.customer,
                    CustomerName = OSCL.custmrName,
                    Remark = OSCL.descrption,
                    NewestContactTel= OSCL.Telephone,
                    AllowOrNot = 0,
                    VestInOrg = 1,
                    IsClose = false,
                    IsModified = false,
                    Status = 2,
                    ServiceWorkOrders = new List<ServiceWorkOrder>
                    {
                        new ServiceWorkOrder
                        {
                            Priority=OSCL.priority=="H"?3:OSCL.priority=="M"?2:1,
                            FeeType=1,
                            SubmitDate=OSCL.createDate,
                            Status=1,
                            FromTheme="[{}]",
                            FromType=1,
                            MaterialCode=!string.IsNullOrWhiteSpace(OSCL.itemCode) ?OSCL.itemCode:"",
                            MaterialDescription=OSCL.itemName,
                            CreateTime = OSCL.createDate,
                            //ServiceMode=1,
                            OrderTakeType=0,
                            IsCheck=0,
                            WorkOrderNumber=$"{OSCL.callID}-1",
                            BookingDate = null,
                            VisitTime = null,
                            LiquidationDate=null,
                            WarrantyEndDate=null,
                            Remark=OSCL.descrption
                        }
                    }
                };
                serviceOrders.Add(serviceOrder);
                await UnitWork.AddAsync<ServiceOrder, int>(serviceOrder);
            }
            //await UnitWork.BatchAddAsync<ServiceWorkOrder, int>(serviceOrders.ToArray());
            await UnitWork.SaveAsync();
        }
    }
}