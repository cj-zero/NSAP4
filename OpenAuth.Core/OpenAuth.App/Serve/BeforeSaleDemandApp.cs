using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Workbench;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Workbench;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class BeforeSaleDemandApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly WorkbenchApp _workbenchApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        public BeforeSaleDemandApp(IUnitWork unitWork, IAuth auth, FlowInstanceApp flowInstanceApp, WorkbenchApp workbenchApp, ModuleFlowSchemeApp moduleFlowSchemeApp) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _workbenchApp = workbenchApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryBeforeSaleDemandListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var query = UnitWork.Find<BeforeSaleDemand>(null)
                        .Include(c => c.Beforesalefiles)
                        .Include(c => c.Beforesaledemandprojects)
                        .Include(c => c.Beforesaledemandoperationhistories)
                        //.WhereIf(loginContext.User.Account != Define.SYSTEM_USERNAME, c => c.CreateUserId == loginContext.User.Id)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.ApplyUserName), c => c.ApplyUserName.Contains(req.ApplyUserName))
                        //.WhereIf(!string.IsNullOrWhiteSpace(req.ApplyUserId), c => c.ApplyUserName.Contains(req.ApplyUserId))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.KeyWord), k => k.BeforeDemandCode.Contains(req.KeyWord) || k.BeforeSaleDemandProjectName.Contains(req.KeyWord) || k.ApplyUserName.Contains(req.KeyWord))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.ApplyDateStart.ToString()), q => q.ApplyDate > req.ApplyDateStart)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.ApplyDateEnd.ToString()), q => q.ApplyDate < Convert.ToDateTime(req.ApplyDateEnd).AddDays(1))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.UpdateDateStart.ToString()), q => q.UpdateTime > req.UpdateDateStart)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.UpdateDateEnd.ToString()), q => q.UpdateTime < Convert.ToDateTime(req.UpdateDateEnd).AddDays(1))
                        .WhereIf(req.Status != null, c => c.Status == req.Status);

            if (req.PageType == 0)//所有流程:显示所有的当前账号有权限查看的流程，其中包括处理过的流程和等待当前人员处理的流程
            {
                query = query.WhereIf(loginContext.User.Account != Define.SYSTEM_USERNAME, c => c.CreateUserId == loginContext.User.Id || c.ApplyUserId == loginContext.User.Id || c.FactDemandUserId == loginContext.User.Id);
            }
            else if (req.PageType == 1)//提给我的
            {
                var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "需求列表");
                var flowinstace = await UnitWork.Find<FlowInstance>(c => c.SchemeId == mf.FlowSchemeId && c.MakerList.Contains(loginContext.User.Id)).Select(c => c.Id).ToListAsync();
                query = query.Where(c => flowinstace.Contains(c.FlowInstanceId) && c.CreateUserId != loginContext.User.Id);

            }
            else if (req.PageType == 2)//我处理过的
            {
                var instances = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.CreateUserId == loginContext.User.Id).Select(c => c.InstanceId).Distinct().ToListAsync();
                query = query.Where(c => instances.Contains(c.FlowInstanceId) && c.CreateUserId != loginContext.User.Id);
            }

            var resp = await query.OrderByDescending(c => c.BeforeDemandCode).Skip((req.page - 1) * req.limit)
                .Take(req.limit).ToListAsync();
            result.Data = resp.ToList();
            List<string> fileids = new List<string>();
            resp.ForEach(q => fileids.AddRange(q.Beforesalefiles.Select(f => f.FileId).ToList()));
            result.Count = await query.CountAsync();
            return result;
        }

        /// <summary>
        /// 获取详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var detail = await UnitWork.Find<BeforeSaleDemand>(c => c.Id == id)
                            .Include(c => c.BeforeSaleDemandOrders)
                            .Include(c => c.Beforesaledemandprojects)
                            .Include(c => c.Beforesaledemandoperationhistories)
                            .Include(c => c.Beforesalefiles)
                            .FirstOrDefaultAsync();
            result.Data = detail;
            return result;
        }
        /// <summary>
        /// 添加售前需求申请流程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateBeforeSaleDemandReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var obj = req.MapTo<BeforeSaleDemand>();
            if (string.IsNullOrWhiteSpace(req.CustomerId) || string.IsNullOrWhiteSpace(req.CustomerName))
            {
                throw new Exception("请选择客户！");
            }
            if (req.BeforeSaleDemandOrders != null && req.BeforeSaleDemandOrders.Count == 0)
            {
                throw new Exception("请关联单据！");
            }
            if (req.DemandContents == null)
            {
                throw new Exception("请填写需求简述！");
            }

            var dbContext = UnitWork.GetDbContext<BeforeSaleDemand>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                //销售添加表单数据 
                try
                {
                    //草稿状态 只保存数据 不创建流程
                    if (req.IsDraft)
                    {
                        obj.Status = 0;
                    }
                    else
                    {
                        obj.Status = 1;
                        //创建流程
                        var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "需求反馈");
                        var flow = new AddFlowInstanceReq();
                        flow.SchemeId = mf.FlowSchemeId;
                        flow.FrmType = 2;
                        flow.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        flow.CustomName = $"售前需求申请{DateTime.Now}";
                        flow.FrmData = "";
                        //审批流程id
                        obj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(flow);
                    }
                    //添加售前需求申请流程
                    obj.CreateTime = DateTime.Now;
                    obj.CreateUserId = loginContext.User.Id;
                    obj.CreateUserName = loginContext.User.Name;
                    obj = await UnitWork.AddAsync<BeforeSaleDemand, int>(obj);

                    //1、流程添加成功==>关联单据
                    obj.BeforeSaleDemandOrders = req.BeforeSaleDemandOrders;
                    await UnitWork.BatchAddAsync<BeforeSaleDemandOrders>(obj.BeforeSaleDemandOrders.ToArray());
                    //2、添加流程操作记录
                    BeforeSaleDemandOperationHistory beforeSaleDemandOperationHistory = new BeforeSaleDemandOperationHistory
                    {
                        Action = "发起",
                        CreateUser = loginContext.User.Name,
                        CreateUserId = loginContext.User.Id,
                        CreateTime = DateTime.Now,
                        BeforeSaleDemandId = obj.Id,
                        ApprovalStage = 1
                    };
                    beforeSaleDemandOperationHistory = await UnitWork.AddAsync<BeforeSaleDemandOperationHistory>(beforeSaleDemandOperationHistory);
                    //3、关联保存需求附件信息
                    if (req.Attchments != null && req.Attchments.Count > 0)
                    {
                        req.Attchments.ForEach(c =>
                        {
                            obj.Beforesalefiles.Add(new BeforeSaleFiles { FileId = c, BeforeSaleDemandId = obj.Id, Type = 0 });
                        });
                        await UnitWork.BatchAddAsync<BeforeSaleFiles>(obj.Beforesalefiles.ToArray());
                    }
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加售前需求申请流程失败。" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 售前需求申请流程审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationBeforeSaleDemandReq req)
        {

            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var beforeSaleDemand = await UnitWork.Find<BeforeSaleDemand>(b => b.Id == req.Id)
                                        .Include(b => b.Beforesaledemandprojects)
                                        .Include(b => b.Beforesalefiles)
                                        .Include(b => b.BeforeSaleProSchedulings)
                                        .Include(b => b.Beforesaledemandoperationhistories)
                                        .FirstOrDefaultAsync();
            var flowinstace = await UnitWork.Find<FlowInstance>(c => c.Id == beforeSaleDemand.FlowInstanceId).FirstOrDefaultAsync();
            //审批
            VerificationReq verificationReq = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = beforeSaleDemand.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = req.Remark
            };

            if (req.IsReject)//驳回
            {
                verificationReq.VerificationFinally = "3";
                verificationReq.NodeRejectType = "1";
                beforeSaleDemand.Status = 8;//驳回状态
                await _flowInstanceApp.Verification(verificationReq);
            }
            else
            {
                if (req.IsDraft)
                {
                    //添加操作记录
                    FlowInstanceOperationHistory flowInstanceOperationHistory = new FlowInstanceOperationHistory
                    {
                        InstanceId = beforeSaleDemand.FlowInstanceId,
                        CreateUserId = loginContext.User.Id,
                        CreateUserName = loginContext.User.Name,
                        CreateDate = DateTime.Now,
                        Content = flowinstace.ActivityName,
                        Remark = req.Remark,
                        ApprovalResult = "保存草稿",
                        //ActivityId = flowInstance.ActivityId,
                    };
                    var fioh = await UnitWork.Find<FlowInstanceOperationHistory>(r => r.InstanceId.Equals(beforeSaleDemand.FlowInstanceId)).OrderByDescending(r => r.CreateDate).FirstOrDefaultAsync();
                    if (fioh != null)
                    {
                        flowInstanceOperationHistory.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(fioh.CreateDate)).TotalSeconds);
                    }
                    //添加操作记录
                    BeforeSaleDemandOperationHistory beforeSaleDemandOperationHistory = new BeforeSaleDemandOperationHistory
                    {
                        Action = "保存草稿",
                        CreateUser = loginContext.User.Name,
                        CreateUserId = loginContext.User.Id,
                        CreateTime = DateTime.Now,
                        BeforeSaleDemandId = req.Id,
                        ApprovalStage = 0,
                        ApprovalResult = "保存草稿",
                    };
                    await UnitWork.AddAsync(flowInstanceOperationHistory);
                    await UnitWork.UpdateAsync<BeforeSaleDemand>(c => c.Id == req.Id, c => new BeforeSaleDemand
                    {
                        IsDraft = true
                    });
                }
                else
                {
                    if (loginContext.Roles.Any(c => c.Name.Equal("售前需求申请销售总助审批")) && flowinstace.ActivityName == "销售总助审批")
                    {
                        await _flowInstanceApp.Verification(verificationReq);
                        beforeSaleDemand.Status = 2;
                        //设置需求环节执行人
                        await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, false, new string[] { beforeSaleDemand.FactDemandUserId }, "", false);
                    }
                    else if (loginContext.Roles.Any(c => c.Name.Equal("售前需求申请销售测试审批")) && flowinstace.ActivityName == "需求初期沟通")
                    {
                        await _flowInstanceApp.Verification(verificationReq);
                        beforeSaleDemand.Status = 3;
                    }
                    else if (loginContext.Roles.Any(c => c.Name.Equal("售前需求申请研发总助审批")) && flowinstace.ActivityName == "研发总助审批")
                    {
                        await _flowInstanceApp.Verification(verificationReq);
                        beforeSaleDemand.Status = 4;
                        //设置研发环节执行人
                        await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, false, new string[] { beforeSaleDemand.FactDemandUserId }, "", false);
                    }
                    else if (loginContext.Roles.Any(c => c.Name.Equal("售前需求申请研发审批")) && flowinstace.ActivityName == "研发审批")
                    {
                        await _flowInstanceApp.Verification(verificationReq);
                        beforeSaleDemand.Status = 5;
                    }
                    else if (loginContext.Roles.Any(c => c.Name.Equal("总经理")) && flowinstace.ActivityName == "总经理审批")
                    {
                        await _flowInstanceApp.Verification(verificationReq);
                        beforeSaleDemand.Status = 4;
                    }
                    else if (flowinstace.ActivityName == "提交" && (beforeSaleDemand.Status == 5 || beforeSaleDemand.Status == 6))
                    {
                        await _flowInstanceApp.Verification(verificationReq);
                        beforeSaleDemand.Status = 1;//驳回 撤回提交
                    }
                    //beforeSaleDemand.IsTentative = false;
                }
            }

            await UnitWork.UpdateAsync<BeforeSaleDemand>(c => c.Id == req.Id, c => new BeforeSaleDemand
            {
                Status = beforeSaleDemand.Status,
                FirstConnects = beforeSaleDemand.FirstConnects,
                IsDevDeploy = beforeSaleDemand.IsDevDeploy,
                IsRelevanceProject = beforeSaleDemand.IsRelevanceProject
            });

            //修改全局待处理
            await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == beforeSaleDemand.Id && w.OrderType == 6, w => new WorkbenchPending
            {
                UpdateTime = DateTime.Now,
            });
            await UnitWork.SaveAsync();
        }

        public void Update(AddOrUpdateBeforeSaleDemandReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<BeforeSaleDemand>(u => u.Id.Equals(obj.Id), u => new BeforeSaleDemand
            {
                BeforeDemandCode = obj.BeforeDemandCode,
                ApplyUserId = obj.ApplyUserId,
                ApplyUserName = obj.ApplyUserName,
                ApplyDate = obj.ApplyDate,
                ExpectUserId = obj.ExpectUserId,
                ExpectDate = obj.ExpectDate,
                ExpectUserName = obj.ExpectUserName,
                FactDemandUserId = obj.FactDemandUserId,
                FactDemandUser = obj.FactDemandUser,
                CustomerId = obj.CustomerId,
                CustomerName = obj.CustomerName,
                CustomerLinkMan = obj.CustomerLinkMan,
                LinkManPhone = obj.LinkManPhone,
                FlowInstanceId = obj.FlowInstanceId,
                Status = obj.Status,
                DemandContents = obj.DemandContents,
                FirstConnects = obj.FirstConnects,
                PredictDevCost = obj.PredictDevCost,
                DevEstimate = obj.DevEstimate,
                TestEstimate = obj.TestEstimate,
                Remark = obj.Remark,
                IsDevDeploy = obj.IsDevDeploy,
                IsRelevanceProject = obj.IsRelevanceProject,
                CreateUserName = obj.CreateUserName,
                CreateUserId = obj.CreateUserId,
                CreateTime = obj.CreateTime,
                UpdateTime = DateTime.Now
                //todo:补充或调整自己需要的字段
            });

        }

    }
}