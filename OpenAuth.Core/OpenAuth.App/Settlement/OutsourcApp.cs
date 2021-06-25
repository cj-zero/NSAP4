using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Const;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Settlement.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Settlement;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class OutsourcApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            List<int?> outsourcIds = new List<int?>();
            if (!string.IsNullOrWhiteSpace(request.ServiceOrderSapId))
            {
                outsourcIds.AddRange(await UnitWork.Find<outsourcexpenses>(o => o.ServiceOrderSapId == int.Parse(request.ServiceOrderSapId)).Select(o => o.OutsourcId).Distinct().ToListAsync());
            }
            if (!string.IsNullOrWhiteSpace(request.Customer))
            {
                outsourcIds.AddRange(await UnitWork.Find<outsourcexpenses>(o => o.TerminalCustomer.Contains(request.Customer) || o.TerminalCustomerId.Contains(request.Customer)).Select(o => o.OutsourcId).Distinct().ToListAsync());
            }
            var result = new TableData();
            var query = UnitWork.Find<outsourc>(null);
            #region 筛选条件
            //var schemeContent = await .FirstOrDefaultAsync();
            List<string> Lines = new List<string>();
            List<string> flowInstanceIds = new List<string>();
            var lineId = "";
            var SchemeContent = await UnitWork.Find<FlowScheme>(f => f.SchemeName.Equals("个人代理结算")).Select(f => f.SchemeContent).FirstOrDefaultAsync();
            SchemeContentJson schemeJson = JsonHelper.Instance.Deserialize<SchemeContentJson>(SchemeContent);
            if (request.PageType != null && request.PageType > 0)
            {
                if (loginContext.Roles.Any(r => r.Name.Equals("客服主管")))
                {
                    lineId = schemeJson.Nodes.Where(n => n.name.Equals("客服主管审批")).FirstOrDefault()?.id;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("总经理")))
                {
                    lineId = schemeJson.Nodes.Where(n => n.name.Equals("总经理审批")).FirstOrDefault()?.id;
                }
                else if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                {
                    lineId = schemeJson.Nodes.Where(n => n.name.Equals("财务支付")).FirstOrDefault()?.id;
                }
            }
            switch (request.PageType)
            {
                case 1:

                    Lines.Add(lineId);
                    break;
                case 2:
                    List<string> lineIds = new List<string>();
                    var lineIdTo = lineId;
                    foreach (var item in schemeJson.Lines)
                    {
                        if (schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to != null)
                        {
                            lineIdTo = schemeJson.Lines.Where(l => l.from.Equals(lineIdTo)).FirstOrDefault()?.to;
                            lineIds.Add(lineIdTo);
                        }
                        else
                        {
                            break;
                        }
                    }
                    Lines.AddRange(lineIds);
                    break;
                case 3:
                    Lines.Add(lineId);
                    break;
                case 4:
                    if (loginContext.Roles.Any(r => r.Name.Equals("出纳")))
                    {
                        Lines.Add(schemeJson.Nodes.Where(n => n.name.Equals("结束")).FirstOrDefault()?.id);
                    }
                    break;
                default:
                    query = query.Where(q => q.CreateUserId.Equals(loginContext.User.Id));
                    break;
            }
            if (Lines.Count > 0)
            {
                flowInstanceIds = await UnitWork.Find<FlowInstance>(f => Lines.Contains(f.ActivityId)).Select(s => s.Id).ToListAsync();
                query = query.Where(q => flowInstanceIds.Contains(q.FlowInstanceId));
            }

            #endregion
            query = query.WhereIf(!string.IsNullOrWhiteSpace(request.CreateName), q => q.CreateUser.Contains(request.CreateName))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.OutsourcId), q => q.Id == int.Parse(request.OutsourcId))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Customer), q => outsourcIds.Contains(q.Id))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.ServiceOrderSapId), q => outsourcIds.Contains(q.Id))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.StartTime.ToString()), q => q.CreateTime > request.StartTime)
                       .WhereIf(!string.IsNullOrWhiteSpace(request.EndTime.ToString()), q => q.CreateTime < Convert.ToDateTime(request.EndTime).AddDays(1));
            var outsourcList = await query.Include(o => o.outsourcexpenses).OrderByDescending(o => o.UpdateTime).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            var serviceOrderIds = outsourcList.Select(o => o.outsourcexpenses.FirstOrDefault().ServiceOrderId).ToList();
            var serviceWorkOrder = await UnitWork.Find<ServiceWorkOrder>(s => serviceOrderIds.Contains(s.ServiceOrderId)).ToListAsync();
            var flowInstanceList = await UnitWork.Find<FlowInstance>(f => outsourcList.Select(o => o.FlowInstanceId).ToList().Contains(f.Id)).ToListAsync();
            result.Count = await query.CountAsync();
            List<dynamic> outsourcs = new List<dynamic>();
            outsourcList.ForEach(o =>
            {
                var outsourcexpensesObj = o.outsourcexpenses.FirstOrDefault();
                var serviceWorkOrderObj = serviceWorkOrder.Where(s => s.ServiceOrderId == outsourcexpensesObj?.ServiceOrderId && s.CurrentUserNsapId.Equals(o.CreateUserId)).FirstOrDefault();
                outsourcs.Add(new
                {
                    o.Id,
                    o.ServiceMode,
                    UpdateTime = Convert.ToDateTime(o.UpdateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    CreateTime = Convert.ToDateTime(o.CreateTime).ToString("yyyy.MM.dd HH:mm:ss"),
                    outsourcexpensesObj?.ServiceOrderSapId,
                    outsourcexpensesObj?.TerminalCustomer,
                    outsourcexpensesObj?.TerminalCustomerId,
                    serviceWorkOrderObj?.FromTheme,
                    serviceWorkOrderObj?.ManufacturerSerialNumber,
                    serviceWorkOrderObj?.MaterialCode,
                    StatusName = o.FlowInstanceId == null ? "未提交" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.IsFinish == FlowInstanceStatus.Rejected ? "驳回" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName == "开始" ? "未提交" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName == "结束" ? "已支付" : flowInstanceList.Where(f => f.Id.Equals(o.FlowInstanceId)).FirstOrDefault()?.ActivityName,
                    PayTime = o.PayTime != null ? Convert.ToDateTime(o.PayTime).ToString("yyyy.MM.dd HH:mm:ss") : null,
                    o.TotalMoney,
                    o.CreateUser,
                    o.Remark,
                    IsRejected = o.IsRejected ? "是" : null
                });
            });
            result.Data = outsourcs;
            return result;
        }

        /// <summary>
        /// 获取所有需要结算服务单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetServiceOrder(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            List<int> serviceOrderIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(request.OutsourcId))
            {
                serviceOrderIds = await UnitWork.Find<outsourcexpenses>(o => o.OutsourcId == int.Parse(request.OutsourcId)).Select(o => (int)o.ServiceOrderId).ToListAsync();
            }
            //var serviceOrderIds = await UnitWork.Find<outsourcexpenses>(null).Select(o => (int)o.ServiceOrderId).ToListAsync();
            var query = from a in UnitWork.Find<CompletionReport>(c => c.CreateUserId.Equals(loginContext.User.Id) && (c.IsReimburse <= 1 || serviceOrderIds.Contains((int)c.ServiceOrderId)))
                        join b in UnitWork.Find<ServiceOrder>(null).Include(s => s.ServiceWorkOrders) on a.ServiceOrderId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                            //where !serviceOrderIds.Contains(b.Id)
                        select new { a, b };
            //&& ((c.CreateTime.Value.Year == DateTime.Now.Year) || (c.CreateTime.Value.Year == DateTime.Now.Year - 1 && c.CreateTime.Value.Month == 12 && DateTime.Now.Month == 1))
            if (!string.IsNullOrWhiteSpace(request.Month.ToString()))
            {
                query = query.Where(c => c.a.CreateTime.Value.Month == request.Month);
            }
            else
            {
                query = query.WhereIf((bool)request.IsMonth, c => c.a.CreateTime.Value.Month == DateTime.Now.Month && c.a.CreateTime.Value.Year == DateTime.Now.Year);
                if (DateTime.Now.Month == 1)
                {
                    query = query.WhereIf(!(bool)request.IsMonth, c => c.a.CreateTime.Value.Month == 12 && c.a.CreateTime.Value.Year == DateTime.Now.Year - 1);
                }
                else
                {
                    query = query.WhereIf(!(bool)request.IsMonth, c => c.a.CreateTime.Value.Month == DateTime.Now.Month - 1);
                }
            }
            var serviceOrderList = await query.Select(q => new { q.b.Id, q.b.U_SAP_ID, q.b.TerminalCustomer, q.b.TerminalCustomerId, q.b.ServiceWorkOrders }).OrderByDescending(u => u.Id).ToListAsync();
            serviceOrderList = serviceOrderList.GroupBy(s => s.U_SAP_ID).Select(s => s.First()).ToList();
            List<dynamic> objs = new List<dynamic>();
            serviceOrderIds = serviceOrderList.Select(s => s.Id).ToList();
            var serviceEvaluate = await UnitWork.Find<ServiceEvaluate>(s => serviceOrderIds.Contains((int)s.ServiceOrderId)).ToListAsync();
            serviceOrderList.ForEach(s =>
            {
                var count = s.ServiceWorkOrders.Where(w => (w.Status < 7 || w.ServiceMode != request.ServiceMode) && w.CurrentUserNsapId.Equals(loginContext.User.Id)).Count();
                count += serviceEvaluate.Where(s => (s.ServicePrice + s.ServiceAttitude + s.SchemeEffectiveness + s.ProductQuality + s.ResponseSpeed) / 5 < 3).Count();
                if (count <= 0 && s.ServiceWorkOrders.Where(w => w.CurrentUserNsapId.Equals(loginContext.User.Id)).Count() > 0)
                {
                    var serviceWorkOrderObj = s.ServiceWorkOrders.FirstOrDefault();
                    objs.Add(new
                    {
                        ServiceOrderId = s.Id,
                        s.TerminalCustomer,
                        s.TerminalCustomerId,
                        ServiceOrderSapId = s.U_SAP_ID,
                        serviceWorkOrderObj?.FromTheme,
                        serviceWorkOrderObj?.ManufacturerSerialNumber,
                        serviceWorkOrderObj?.MaterialCode,
                        serviceWorkOrderObj?.Remark
                    });
                }
            });
            result.Data = objs.Skip((request.page - 1) * request.limit).Take(request.limit).ToList();
            result.Count = objs.Count();
            return result;
        }

        /// <summary>
        /// 获取服务单详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> ServiceOrderDetails(QueryoutsourcListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            if (request.ServiceMode == 1)
            {
                var serviceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => request.ServiceOrderIds.Contains(s.ServiceOrderId)).ToListAsync();
                var serviceDailyReportList = await UnitWork.Find<ServiceDailyReport>(s => request.ServiceOrderIds.Contains(s.ServiceOrderId)).ToListAsync();
                result.Data = new
                {
                    ServiceWorkOrderList = serviceWorkOrderList.Select(s => new
                    {
                        s.ManufacturerSerialNumber,
                        s.MaterialCode,
                        s.MaterialDescription,
                        s.FromTheme,
                        s.Remark,
                        s.CreateTime
                    }).OrderBy(s => s.CreateTime).ToList(),
                    ServiceDailyReportList = serviceDailyReportList.Select(s => new
                    {
                        s.CreateTime,
                        s.ManufacturerSerialNumber,
                        s.MaterialCode,
                        ProcessDescription = GetServiceTroubleAndSolution(s.ProcessDescription),
                        TroubleDescription = GetServiceTroubleAndSolution(s.TroubleDescription)
                    }).OrderBy(s => s.CreateTime).ToList()
                };
            }
            else
            {
                var serviceOrderIds = await UnitWork.Find<ServiceOrder>(s => request.ServiceOrderIds.Contains(s.Id)).ToListAsync();
                var outsourcIds = await UnitWork.Find<outsourc>(o => o.CreateUserId.Equals(loginContext.User.Id)).WhereIf(!string.IsNullOrWhiteSpace(request.OutsourcId), e => e.Id != int.Parse(request.OutsourcId)).Select(s => s.Id).ToListAsync();
                var thisMonth = await UnitWork.Find<outsourcexpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month && e.ExpensesType == 4 && e.IsOverseas == false).CountAsync();
                var lastMonth = await UnitWork.Find<outsourcexpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month - 1 && e.ExpensesType == 4 && e.IsOverseas == false).CountAsync();
                var number = 0;
                var completionReportList = await UnitWork.Find<CompletionReport>(c => request.ServiceOrderIds.Contains(c.ServiceOrderId)).ToListAsync();
                var globalarea = await UnitWork.Find<GlobalArea>(g => g.AreaLevel == "3" && g.Pid == "99").Select(g => g.AreaName).ToListAsync();
                var money = 0;
                request.ServiceOrderIds.ForEach(s =>
                {
                    var completionReportObj = completionReportList.Where(c => c.ServiceOrderId == s).OrderByDescending(c => c.CreateTime).FirstOrDefault();
                    if (completionReportObj.CreateTime.Value.Month == DateTime.Now.Month)
                    {
                        number = thisMonth++;
                    }
                    else if (completionReportObj.CreateTime.Value.Month == DateTime.Now.Month - 1)
                    {
                        number = lastMonth++;
                    }
                    var Province = serviceOrderIds.Where(o => o.Id == s).FirstOrDefault()?.Province;
                    if (globalarea.Contains(Province) || Province=="海外")
                    {
                        money += 50;
                    }
                    else
                    {
                        money += Calculation(number);
                    }

                });
                result.Data = new { TotalMoney = money };
            }
            return result;
        }

        /// <summary>
        /// 查询结算单详情
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(QueryoutsourcListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var outsourcObj = await UnitWork.Find<outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.outsourcexpenses).ThenInclude(o => o.outsourcexpensespictures).FirstOrDefaultAsync();
            var History = await UnitWork.Find<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(outsourcObj.FlowInstanceId)).OrderBy(f => f.CreateDate).ToListAsync();
            var StatusName = (await UnitWork.Find<FlowInstance>(f => outsourcObj.FlowInstanceId.Equals(f.Id)).FirstOrDefaultAsync())?.ActivityName;
            var OperationHistorys = History.Select(h => new
            {
                CreateDate = Convert.ToDateTime(h.CreateDate).ToString("yyyy.MM.dd HH:mm:ss"),
                h.Remark,
                IntervalTime = h.IntervalTime != null && h.IntervalTime > 0 ? h.IntervalTime / 60 : null,
                h.CreateUserName,
                h.Content,
                h.ApprovalResult,
            });
            if (outsourcObj.ServiceMode == 1)
            {
                var serviceOrderObj = await UnitWork.Find<ServiceOrder>(s => s.Id == outsourcObj.outsourcexpenses.FirstOrDefault().ServiceOrderId).Include(s => s.ServiceWorkOrders).FirstOrDefaultAsync();
                var serviceDailyReportList = await UnitWork.Find<ServiceDailyReport>(s => outsourcObj.outsourcexpenses.FirstOrDefault().ServiceOrderId == s.ServiceOrderId).ToListAsync();
                result.Data = new
                {
                    ServiceWorkOrderList = serviceOrderObj.ServiceWorkOrders.Where(s => s.CurrentUserNsapId.Equals(outsourcObj.CreateUserId)).Select(s => new
                    {
                        s.ManufacturerSerialNumber,
                        s.MaterialCode,
                        s.MaterialDescription,
                        s.FromTheme,
                        s.Remark,
                        CreateTime = Convert.ToDateTime(s.CreateTime).ToString("yyyy.MM.dd HH:mm:ss")
                    }).OrderBy(s => s.CreateTime).ToList(),
                    outsourcObj.outsourcexpenses,
                    outsourcObj.outsourcexpenses.FirstOrDefault()?.ServiceOrderId,
                    outsourcObj.outsourcexpenses.FirstOrDefault()?.ServiceOrderSapId,
                    outsourcObj.Id,
                    serviceOrderObj.TerminalCustomer,
                    serviceOrderObj.TerminalCustomerId,
                    serviceOrderObj.SalesMan,
                    serviceOrderObj.NewestContactTel,
                    serviceOrderObj.NewestContacter,
                    serviceOrderObj.Address,
                    serviceOrderObj.Supervisor,
                    outsourcObj.CreateUser,
                    outsourcObj.ServiceMode,
                    StatusName,
                    ServiceDailyReportList = serviceDailyReportList.Select(s => new
                    {
                        s.CreateTime,
                        s.ManufacturerSerialNumber,
                        s.MaterialCode,
                        ProcessDescription = GetServiceTroubleAndSolution(s.ProcessDescription),
                        TroubleDescription = GetServiceTroubleAndSolution(s.TroubleDescription)
                    }).OrderBy(s => s.CreateTime).ToList(),
                    OperationHistorys
                };
            }
            else
            {
                var servicerOrderIds = outsourcObj.outsourcexpenses.Select(o => o.ServiceOrderId).ToList();
                var serviceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => servicerOrderIds.Contains(s.ServiceOrderId) && s.CurrentUserNsapId.Equals(outsourcObj.CreateUserId)).ToListAsync();
                serviceWorkOrderList = serviceWorkOrderList.GroupBy(s => s.ServiceOrderId).Select(s => s.FirstOrDefault()).ToList();
                result.Data = new
                {
                    ServiceOrder = outsourcObj.outsourcexpenses.Select(o => new
                    {
                        outsourcexpensesId = o.Id,
                        o.ServiceOrderId,
                        o.ServiceOrderSapId,
                        o.TerminalCustomer,
                        o.TerminalCustomerId,
                        serviceWorkOrderList.Where(s => s.ServiceOrderId == o.ServiceOrderId).FirstOrDefault()?.FromTheme,
                        serviceWorkOrderList.Where(s => s.ServiceOrderId == o.ServiceOrderId).FirstOrDefault()?.ManufacturerSerialNumber,
                        serviceWorkOrderList.Where(s => s.ServiceOrderId == o.ServiceOrderId).FirstOrDefault()?.MaterialCode,
                        serviceWorkOrderList.Where(s => s.ServiceOrderId == o.ServiceOrderId).FirstOrDefault()?.Remark
                    }).OrderBy(s=>s.ServiceOrderSapId).ToList(),
                    Month = outsourcObj.outsourcexpenses.FirstOrDefault()?.CompleteTime.Value.Month,
                    outsourcObj.ServiceMode,
                    outsourcObj.Id,
                    outsourcObj.TotalMoney,
                    outsourcObj.Remark,
                    outsourcObj.CreateUser,
                    StatusName,
                    OperationHistorys
                };
            }

            return result;
        }

        /// <summary>
        /// 添加结算
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateoutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var obj =await Condition(req);
            obj.CreateTime = DateTime.Now;
            obj.CreateUser = loginUser.Name;
            obj.CreateUserId = loginUser.Id;
            obj.outsourcexpenses.ForEach(o => { o.Id = Guid.NewGuid().ToString(); o.outsourcexpensespictures.ForEach(p => p.Id = Guid.NewGuid().ToString()); });
            var serviceOrderIds = obj.outsourcexpenses.Select(s => s.ServiceOrderId).Distinct().ToList();
            //事务保证数据一致
            var dbContext = UnitWork.GetDbContext<outsourc>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var outsourcObj = await UnitWork.AddAsync<outsourc, int>(obj);
                    await UnitWork.SaveAsync();
                    if (!req.IsDraft)
                    {
                        //创建结算流程
                        var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("个人代理结算"));
                        var afir = new AddFlowInstanceReq();
                        afir.SchemeId = mf.FlowSchemeId;
                        afir.FrmType = 2;
                        afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        afir.CustomName = $"个人代理结算单";
                        afir.FrmData = "{\"ID\":\"" + outsourcObj.Id + "\"}";
                        afir.OrgId = loginContext.Orgs.OrderBy(o => o.CascadeId).FirstOrDefault()?.Id;
                        var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        await UnitWork.UpdateAsync<outsourc>(r => r.Id == outsourcObj.Id, r => new outsourc { FlowInstanceId = FlowInstanceId });
                    }
                    await UnitWork.UpdateAsync<CompletionReport>(c => serviceOrderIds.Contains(c.ServiceOrderId) && c.CreateUserId.Equals(loginUser.Id), c => new CompletionReport { IsReimburse = 4 });
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加结算单失败。请重试" + ex.Message);
                }

            }

        }
        /// <summary>
        /// 修改结算
        /// </summary>
        /// <param name="req"></param>
        public async Task Update(AddOrUpdateoutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var obj = await Condition(req);
            obj.Id = (int)req.outsourcId;
            var dbContext = UnitWork.GetDbContext<outsourc>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var outsourcObj = await UnitWork.Find<outsourc>(o => o.Id == req.outsourcId).Include(o => o.outsourcexpenses).ThenInclude(o => o.outsourcexpensespictures).FirstOrDefaultAsync();
                    #region 删除重新新增
                    List<outsourcexpensespicture> pictureList = new List<outsourcexpensespicture>();
                    outsourcObj.outsourcexpenses.ForEach(o => pictureList.AddRange(o.outsourcexpensespictures));
                    await UnitWork.BatchDeleteAsync<outsourcexpensespicture>(pictureList.ToArray());
                    await UnitWork.BatchDeleteAsync<outsourcexpenses>(outsourcObj.outsourcexpenses.ToArray());
                    await UnitWork.SaveAsync();
                    obj.outsourcexpenses.ForEach(o => { o.OutsourcId = req.outsourcId; o.outsourcexpensespictures.ForEach(p => { p.Id = Guid.NewGuid().ToString(); p.OutsourcExpensesId = o.Id; }); });
                    await UnitWork.BatchAddAsync<outsourcexpenses>(obj.outsourcexpenses.ToArray());
                    await UnitWork.SaveAsync();
                    #endregion
                    if (!req.IsDraft)
                    {
                        if (string.IsNullOrWhiteSpace(obj.FlowInstanceId))
                        {
                            //创建结算流程
                            var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("个人代理结算"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            afir.CustomName = $"个人代理结算单";
                            afir.FrmData = "{\"ID\":\"" + outsourcObj.Id + "\"}";
                            afir.OrgId = loginContext.Orgs.OrderBy(o => o.CascadeId).FirstOrDefault()?.Id;
                            obj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                        }
                        else
                        {
                            await _flowInstanceApp.Start(new StartFlowInstanceReq { FlowInstanceId = obj.FlowInstanceId });
                        }

                    }
                    
                    await UnitWork.UpdateAsync<outsourc>(o => o.Id == req.outsourcId, u => new outsourc
                    {
                        TotalMoney = obj.TotalMoney,
                        FlowInstanceId = obj.FlowInstanceId,
                        UpdateTime = DateTime.Now,
                        //todo:补充或调整自己需要的字段
                    });
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加结算单失败。请重试" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationOutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var outsourcObj = await UnitWork.Find<outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.outsourcexpenses).FirstOrDefaultAsync();
            VerificationReq VerificationReqModle = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = outsourcObj.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = req.Remark,
            };
            if (req.IsReject)
            {
                VerificationReqModle.VerificationFinally = "3";
                VerificationReqModle.VerificationOpinion = req.Remark;
                VerificationReqModle.NodeRejectType = "1";
                await _flowInstanceApp.Verification(VerificationReqModle);
                await UnitWork.DeleteAsync<outsourcexpenses>(o => o.OutsourcId == outsourcObj.Id && o.ExpensesType == 3);
            }
            else
            {
                var flowInstanceObj = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(outsourcObj.FlowInstanceId)).FirstOrDefaultAsync();
                await _flowInstanceApp.Verification(VerificationReqModle);
                if (flowInstanceObj.ActivityName.Equals("客服主管审批") && !string.IsNullOrWhiteSpace(req.Money) && outsourcObj.ServiceMode==1)
                {
                    if (outsourcObj.TotalMoney <= 0) 
                    {
                        await UnitWork.UpdateAsync<outsourcexpenses>(o => o.ExpensesType == 3 && o.OutsourcId == outsourcObj.Id, o => new outsourcexpenses { Money = decimal.Parse(req.Money)});
                    }
                    else
                    {
                        var outsourcexpensesObj = outsourcObj.outsourcexpenses.FirstOrDefault();
                        await UnitWork.AddAsync<outsourcexpenses>(new outsourcexpenses
                        {
                            ExpensesType = 3,
                            Money = decimal.Parse(req.Money),
                            ServiceOrderId = outsourcexpensesObj?.ServiceOrderId,
                            ServiceOrderSapId = outsourcexpensesObj.ServiceOrderSapId,
                            TerminalCustomer = outsourcexpensesObj.TerminalCustomer,
                            TerminalCustomerId = outsourcexpensesObj.TerminalCustomerId,
                            OutsourcId = outsourcObj.Id
                        });
                    }
                    outsourcObj.TotalMoney += decimal.Parse(req.Money);
                }
                if (flowInstanceObj.ActivityName.Equals("总经理审批") && outsourcObj.ServiceMode == 2)
                {
                    var outsourcIds = await UnitWork.Find<outsourc>(o => o.CreateUserId.Equals(outsourcObj.CreateUserId) && o.Id!= outsourcObj.Id).Select(s => s.Id).ToListAsync();
                    var thisMonth = await UnitWork.Find<outsourcexpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month && e.ExpensesType == 4 && e.SerialNumber != null && e.SerialNumber > 0 && e.IsOverseas == false).CountAsync();
                    var lastMonth = await UnitWork.Find<outsourcexpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month - 1 && e.ExpensesType == 4 && e.SerialNumber != null && e.SerialNumber > 0 && e.IsOverseas == false).CountAsync();
                    outsourcObj.TotalMoney = 0;
                    outsourcObj.outsourcexpenses.ForEach(o =>
                    {
                        if (o.CompleteTime.Value.Month == DateTime.Now.Month)
                        {
                            o.SerialNumber = thisMonth + 1;
                            thisMonth++;
                        }
                        else if (o.CompleteTime.Value.Month == DateTime.Now.Month - 1)
                        {
                            o.SerialNumber = lastMonth + 1;
                            lastMonth++;
                        }
                        if (o.IsOverseas)
                        {
                            o.SerialNumber = 0;
                            o.Money = 50;
                        }
                        else
                        {
                            o.Money = Calculation((int)o.SerialNumber);
                        }

                        outsourcObj.TotalMoney += o.Money;
                    });
                    await UnitWork.BatchUpdateAsync<outsourcexpenses>(outsourcObj.outsourcexpenses.ToArray());
                }
                if (flowInstanceObj.ActivityName.Equals("财务支付"))
                {
                    outsourcObj.PayTime = DateTime.Now;
                }
            }
            await UnitWork.UpdateAsync<outsourc>(r => r.Id == outsourcObj.Id, r => new outsourc
            {
                //Status = returnNoteStatus,
                UpdateTime = DateTime.Now,
                TotalMoney = outsourcObj.TotalMoney,
                PayTime = outsourcObj.PayTime
            });
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 驳回单个
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ASingleRejection(AccraditationOutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var dbContext = UnitWork.GetDbContext<outsourc>();
            var result = new TableData();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var outsourcObj = await UnitWork.Find<outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.outsourcexpenses).FirstOrDefaultAsync();
                    var outsourcexpensesObj = outsourcObj.outsourcexpenses.Where(o => o.Id.Equals(req.outsourcexpensesId)).FirstOrDefault();
                    await UnitWork.DeleteAsync<outsourcexpenses>(outsourcexpensesObj);
                    await UnitWork.SaveAsync();
                    outsourcObj.outsourcexpenses = outsourcObj.outsourcexpenses.Where(o => o.Id != req.outsourcexpensesId).ToList();
                    var thisMonth = await UnitWork.Find<outsourcexpenses>(e => e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month && e.ExpensesType == 4).CountAsync();
                    var lastMonth = await UnitWork.Find<outsourcexpenses>(e => e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month - 1 && e.ExpensesType == 4).CountAsync();
                    var completionReportList = await UnitWork.Find<CompletionReport>(c => outsourcObj.outsourcexpenses.Select(o => o.ServiceOrderId).ToList().Contains(c.ServiceOrderId)).ToListAsync();
                    outsourcObj.TotalMoney = 0;
                    outsourcObj.outsourcexpenses.ForEach(o =>
                    {
                        o.CompleteTime = completionReportList.Where(c => c.ServiceOrderId == o.ServiceOrderId).OrderByDescending(c => c.CreateTime).FirstOrDefault()?.CreateTime;
                        if (o.CompleteTime.Value.Month == DateTime.Now.Month)
                        {
                            o.SerialNumber = thisMonth + 1;
                            thisMonth++;
                        }
                        else if (o.CompleteTime.Value.Month == DateTime.Now.Month - 1)
                        {
                            o.SerialNumber = lastMonth + 1;
                            lastMonth++;
                        }
                        o.Money = Calculation((int)o.SerialNumber);
                        outsourcObj.TotalMoney += o.Money;
                    });
                    await UnitWork.BatchUpdateAsync<outsourcexpenses>(outsourcObj.outsourcexpenses.ToArray());

                    await UnitWork.UpdateAsync<outsourc>(r => r.Id == outsourcObj.Id, r => new outsourc
                    {

                        //Status = returnNoteStatus,
                        UpdateTime = DateTime.Now,
                        TotalMoney = outsourcObj.TotalMoney,
                        IsRejected = true
                    });

                    await UnitWork.UpdateAsync<CompletionReport>(c => c.CreateUserId.Equals(outsourcObj.CreateUserId) && c.ServiceOrderId == outsourcexpensesObj.ServiceOrderId, c => new CompletionReport { IsReimburse = 1 });
                    #region 增加驳回记录
                    FlowInstanceOperationHistory flowInstanceOperationHistory = new FlowInstanceOperationHistory
                    {
                        InstanceId = outsourcObj.FlowInstanceId,
                        CreateUserId = loginContext.User.Id,
                        CreateUserName = loginContext.User.Name,
                        CreateDate = DateTime.Now,
                        Content = "驳回服务单" + outsourcexpensesObj.ServiceOrderSapId,
                        Remark = req.Remark,
                        ApprovalResult = "驳回",
                    };
                    var fioh = await UnitWork.Find<FlowInstanceOperationHistory>(r => r.InstanceId.Equals(outsourcObj.FlowInstanceId)).OrderByDescending(r => r.CreateDate).FirstOrDefaultAsync();
                    if (fioh != null)
                    {
                        flowInstanceOperationHistory.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(fioh.CreateDate)).TotalSeconds);
                    }
                    await UnitWork.AddAsync(flowInstanceOperationHistory);
                    #endregion
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Data = new { TotalMoney = outsourcObj.TotalMoney };
                    return result;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("驳回失败。请重试" + ex.Message);
                }
            }


        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Delete(QueryoutsourcListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var outsourcObj = await UnitWork.Find<outsourc>(o => o.Id == int.Parse(req.OutsourcId)).Include(o => o.outsourcexpenses).ThenInclude(o => o.outsourcexpensespictures).FirstOrDefaultAsync();

            if (outsourcObj != null)
            {
                var status = true;
                if (!string.IsNullOrWhiteSpace(outsourcObj.FlowInstanceId))
                {
                    status = (await UnitWork.Find<FlowInstance>(f => f.Id.Equals(outsourcObj.FlowInstanceId)).FirstOrDefaultAsync())?.ActivityName == "开始" ? true : false;
                    await UnitWork.DeleteAsync<FlowInstanceTransitionHistory>(f => f.InstanceId.Equals(outsourcObj.FlowInstanceId));
                    await UnitWork.DeleteAsync<FlowInstanceOperationHistory>(f => f.InstanceId.Equals(outsourcObj.FlowInstanceId));
                    await UnitWork.DeleteAsync<FlowInstance>(f => f.Id.Equals(outsourcObj.FlowInstanceId));
                }
                if (status)
                {
                    List<outsourcexpensespicture> pictureList = new List<outsourcexpensespicture>();
                    outsourcObj.outsourcexpenses.ForEach(o => pictureList.AddRange(o.outsourcexpensespictures));
                    await UnitWork.BatchDeleteAsync<outsourcexpensespicture>(pictureList.ToArray());
                    await UnitWork.BatchDeleteAsync<outsourcexpenses>(outsourcObj.outsourcexpenses.ToArray());
                    await UnitWork.DeleteAsync<outsourc>(outsourcObj);
                    var serviceOrderids = outsourcObj.outsourcexpenses.Select(o => o.ServiceOrderId).Distinct().ToList();
                    await UnitWork.UpdateAsync<CompletionReport>(c => c.CreateUserId.Equals(outsourcObj.CreateUserId) && serviceOrderids.Contains(c.ServiceOrderId), c => new CompletionReport { IsReimburse = 1 });
                    await UnitWork.SaveAsync();
                }
                else
                {
                    throw new Exception("当前状态不可删除。");
                }

            }
            else
            {
                throw new Exception("当前状态不可删除。");
            }

        }

        /// <summary>
        /// 删除单个费用
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task DeleteExpenses(QueryoutsourcListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var outsourcexpensesObj = await UnitWork.Find<outsourcexpenses>(o => o.Id.Equals(req.OutsourcExpensesId)).Include(o => o.outsourcexpensespictures).FirstOrDefaultAsync();

            if (outsourcexpensesObj != null)
            {
                await UnitWork.BatchDeleteAsync<outsourcexpensespicture>(outsourcexpensesObj.outsourcexpensespictures.ToArray());
                await UnitWork.DeleteAsync<outsourcexpenses>(outsourcexpensesObj);
                await UnitWork.UpdateAsync<outsourc>(o => o.Id == outsourcexpensesObj.OutsourcId, o => new outsourc { TotalMoney = o.TotalMoney - outsourcexpensesObj.Money });
                await UnitWork.SaveAsync();
            }

        }


        /// <summary>
        /// 新增修改通用接口
        /// </summary>
        /// <returns name="req"></returns>
        public async Task<outsourc> Condition(AddOrUpdateoutsourcReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var obj = req.MapTo<outsourc>();
            obj.UpdateTime = DateTime.Now;
            obj.IsRejected = false;
            var completionReportList = await UnitWork.Find<CompletionReport>(c => obj.outsourcexpenses.Select(o => o.ServiceOrderId).ToList().Contains(c.ServiceOrderId)).ToListAsync();
            obj.TotalMoney = 0;
            if (obj.ServiceMode == 1)
            {
                var completionReportObj = completionReportList.Where(c => c.ServiceOrderId == req.ServiceOrderId).OrderByDescending(c => c.CreateTime).FirstOrDefault();

                if (obj.outsourcexpenses==null||obj.outsourcexpenses.Count() <= 0) 
                {
                    obj.outsourcexpenses.Add(new outsourcexpenses
                    {
                        ExpensesType = 3,
                        Money = 0,
                        ServiceOrderId = completionReportObj?.ServiceOrderId,
                        ServiceOrderSapId = req.ServiceOrderSapId,
                        TerminalCustomer = completionReportObj.TerminalCustomer,
                        TerminalCustomerId = completionReportObj.TerminalCustomerId,
                    });
                }
                obj.outsourcexpenses.ForEach(o =>
                {
                    obj.TotalMoney += o.Money;
                    o.CompleteTime = completionReportObj?.CreateTime;
                    o.TerminalCustomer = completionReportObj.TerminalCustomer;
                    o.TerminalCustomerId = completionReportObj.TerminalCustomerId;
                });
            }
            else
            {
                var serviceOrderIds = await  UnitWork.Find<ServiceOrder>(s => obj.outsourcexpenses.Select(o => o.ServiceOrderId).ToList().Contains(s.Id)).ToListAsync();
                var outsourcIds=await UnitWork.Find<outsourc>(o => o.CreateUserId.Equals(loginUser.Id)).WhereIf(!string.IsNullOrWhiteSpace(req.outsourcId.ToString()), e => e.Id != req.outsourcId).Select(s => s.Id).ToListAsync();
                var thisMonth = await UnitWork.Find<outsourcexpenses>(e => outsourcIds.Contains((int)e.OutsourcId)&&e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month && e.ExpensesType == 4 && e.IsOverseas == false).CountAsync();
                var lastMonth = await UnitWork.Find<outsourcexpenses>(e => outsourcIds.Contains((int)e.OutsourcId) && e.CompleteTime.Value.Year == DateTime.Now.Year && e.CompleteTime.Value.Month == DateTime.Now.Month - 1 && e.ExpensesType == 4 && e.IsOverseas == false).CountAsync();
                var globalarea = await UnitWork.Find<GlobalArea>(g => g.AreaLevel == "3" && g.Pid == "99").Select(g=>g.AreaName).ToListAsync();
                var number = 0;
                obj.outsourcexpenses.ForEach(o =>
                {
                    var completionReportObj = completionReportList.Where(c => c.ServiceOrderId == o.ServiceOrderId).OrderByDescending(c => c.CreateTime).FirstOrDefault();
                    o.ExpensesType = 4;
                    o.CompleteTime = completionReportObj?.CreateTime;
                    o.TerminalCustomer = completionReportObj.TerminalCustomer;
                    o.TerminalCustomerId = completionReportObj.TerminalCustomerId;
                    if (o.CompleteTime.Value.Month == DateTime.Now.Month)
                    {
                        number = thisMonth++;
                    }
                    else if (o.CompleteTime.Value.Month == DateTime.Now.Month - 1)
                    {
                        number = lastMonth++;
                    }
                    var Province = serviceOrderIds.Where(s => s.Id == o.ServiceOrderId).FirstOrDefault()?.Province;
                    if (globalarea.Contains(Province) || Province == "海外")
                    {
                        o.Money = 50;
                        o.IsOverseas = true;
                    }
                    else
                    {
                        o.Money = Calculation(number);
                        o.IsOverseas = false;
                    }
                    obj.TotalMoney += o.Money;
                });
            }
            return obj;
        }

        /// <summary>
        /// 计算金额
        /// </summary>
        /// <param name="SerialNumber"></param>
        /// <returns></returns>
        public int Calculation(int SerialNumber)
        {
            var Money = 0;
            if (SerialNumber > 300)
            {
                Money = 60;
            }
            else if (SerialNumber > 250)
            {
                Money = 50;
            }
            else if (SerialNumber > 200)
            {
                Money = 45;
            }
            else if (SerialNumber > 150)
            {
                Money = 40;
            }
            else if (SerialNumber > 100)
            {
                Money = 35;
            }
            else
            {
                Money = 30;
            }
            return Money;
        }

        /// <summary>
        /// 解析完工报告json
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private List<string> GetServiceTroubleAndSolution(string data)
        {
            List<string> result = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                JArray jArray = (JArray)JsonConvert.DeserializeObject(data);
                foreach (var item in jArray)
                {
                    result.Add(item["description"].ToString());
                }
            }
            return result;
        }

        public OutsourcApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
        }
    }
}