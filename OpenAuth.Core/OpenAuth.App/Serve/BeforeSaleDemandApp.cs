﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using Npoi.Mapper;
using OpenAuth.App.Flow;
using OpenAuth.App.Interface;
using OpenAuth.App.Reponse;
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
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        public BeforeSaleDemandApp(IUnitWork unitWork, IAuth auth, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
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
                        //.Include(c => c.FlowInstances)
                        //.Include(c => c.Beforesalefiles)
                        //.Include(c => c.Beforesaledemandprojects)
                        .Include(c => c.Beforesaledemandoperationhistories)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.ApplyUserName), c => c.ApplyUserName.Contains(req.ApplyUserName))
                        //.WhereIf(!string.IsNullOrWhiteSpace(req.ApplyUserId), c => c.ApplyUserName.Contains(req.ApplyUserId))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.KeyWord), k => k.BeforeDemandCode.Contains(req.KeyWord) || k.BeforeSaleDemandProjectName.Contains(req.KeyWord) || k.ApplyUserName.Contains(req.KeyWord))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.ApplyDateStart.ToString()), q => q.ApplyDate > req.ApplyDateStart)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.ApplyDateEnd.ToString()), q => q.ApplyDate < Convert.ToDateTime(req.ApplyDateEnd).AddDays(1))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.UpdateDateStart.ToString()), q => q.UpdateTime > req.UpdateDateStart)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.UpdateDateEnd.ToString()), q => q.UpdateTime < Convert.ToDateTime(req.UpdateDateEnd).AddDays(1));

            if (req.BeforeSaleDemandProjectId > 0)
            {
                query = query.Where(q => q.BeforeSaleDemandProjectId == req.BeforeSaleDemandProjectId);
            }

            //流程状态0-草稿 1-审批中 2-结束
            //数据状态0-草稿 1-销售提交需求 2-销售总助审批 3-需求组提交需求 4-研发总助审批 5-研发确认 6-总经理审批
            //7-立项 8-需求提交 9-研发提交10-测试提交11-实施提交12-客户验收(流程结束)13-驳回状态
            if (req.Status != null && req.Status == 0)
            {   //草稿状态
                query = query.Where(q => q.Status == 0);
            }
            else if (req.Status != null && req.Status == 1)
            {   //审批中
                query = query.Where(q => q.Status >= 1 && q.Status <= 11);
            }
            else if (req.Status != null && req.Status == 2)
            {  //已完成
                query = query.Where(q => q.Status >= 12);
            }

            if (req.PageType == 0)//所有流程:显示所有的当前账号有权限查看的流程，其中包括处理过的流程和等待当前人员处理的流程
            {
                //查询所有与当前用户相关的流程
                var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "售前需求");
                var flowinstaceIds = await UnitWork.Find<FlowInstance>(c => c.SchemeId == mf.FlowSchemeId && c.MakerList.Contains(loginContext.User.Id)).Select(c => c.Id).ToListAsync();
                flowinstaceIds.AddRange(await UnitWork.Find<FlowInstanceOperationHistory>(c => c.CreateUserId == loginContext.User.Id).Select(c => c.InstanceId).Distinct().ToListAsync());

                query = query.WhereIf(loginContext.User.Account != Define.SYSTEM_USERNAME, c => c.CreateUserId == loginContext.User.Id || c.ApplyUserId == loginContext.User.Id || c.FactDemandUserId == loginContext.User.Id || flowinstaceIds.Contains(c.FlowInstanceId));
            }
            else if (req.PageType == 1)//提给我的
            {
                var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "售前需求");
                var flowinstace = await UnitWork.Find<FlowInstance>(c => c.SchemeId == mf.FlowSchemeId && c.MakerList.Contains(loginContext.User.Id)).Select(c => c.Id).ToListAsync();
                query = query.Where(c => flowinstace.Contains(c.FlowInstanceId));
            }
            else if (req.PageType == 2)//我处理过的
            {
                var instances = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.CreateUserId == loginContext.User.Id).Select(c => c.InstanceId).Distinct().ToListAsync();
                query = query.Where(c => instances.Contains(c.FlowInstanceId));
            }

            var resp = await query
                                .OrderByDescending(c => c.Id)
                                .Skip((req.page - 1) * req.limit)
                                .Take(req.limit)
                                .ToListAsync();
            result.Data = resp.Select(c => new
            {
                c.Id,
                c.BeforeDemandCode,
                c.ApplyDate,
                c.ApplyUserId,
                c.ApplyUserName,
                c.CustomerId,
                c.CustomerName,
                c.CreateTime,
                c.BeforeSaleDemandProjectName,
                c.BeforeSaleDemandProjectId,
                CurrentProcessor = c.Beforesaledemandoperationhistories.OrderByDescending(x => x.CreateTime).FirstOrDefault().CreateUser + "—" + c.Beforesaledemandoperationhistories.OrderByDescending(x => x.CreateTime).FirstOrDefault().Action,
                c.UpdateTime,
                c.Status
            }).ToList();
            //List<string> fileids = new List<string>();
            //resp.ForEach(q => fileids.AddRange(q.Beforesalefiles.Select(f => f.FileId).ToList()));
            result.Count = await query.CountAsync();
            return result;
        }

        /// <summary>
        /// 获取详情-同时返回该流程的节点状态
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<BeforeSaleDemandResp> GetDetails(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var detail = await UnitWork.Find<BeforeSaleDemand>(c => c.Id == id)
                            .Include(c => c.BeforeSaleDemandOrders)
                            .Include(c => c.BeforeSaleDemandDeptInfos)
                            .Include(c => c.Beforesaledemandprojects)
                            .Include(c => c.Beforesaledemandoperationhistories)
                            .Include(c => c.BeforeSaleProSchedulings)
                            .Include(c => c.Beforesalefiles)
                            .FirstOrDefaultAsync();

            var result = detail.MapTo<BeforeSaleDemandResp>();
            //判断当前用户是否有页面的审核权限 默认否false
            result.IsHandle = false;
            if (result.Status > 0 && result.FlowInstanceId != null)
            {
                //查询流程实例 获取当前审核节点信息
                //判断当前用户是否有审核权限
                var flowInstance = await UnitWork.Find<FlowInstance>(f => f.Id.Equals(result.FlowInstanceId)).Select(f => new { f.Id, f.ActivityName, f.IsFinish }).FirstOrDefaultAsync();
                #region 判断当前用户是否有页面的审核权限
                if (detail.Status == 1 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-销售总助")) && flowInstance.ActivityName == "销售总助审批")
                {
                    result.IsHandle = true;
                }
                else if (detail.Status == 2 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-需求工程师")) && flowInstance.ActivityName == "需求组提交需求")
                {
                    result.IsHandle = true;
                }
                else if (detail.Status == 3 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发总助")) && flowInstance.ActivityName == "研发总助审批")
                {
                    result.IsHandle = true;
                }
                else if (detail.Status == 4 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发工程师")) && flowInstance.ActivityName == "研发确认")
                {
                    result.IsHandle = true;
                }
                else if (detail.Status == 5 && loginContext.Roles.Any(c => c.Name.Equal("总经理")) && flowInstance.ActivityName == "总经理审批")
                {
                    //开发投入预估（开发预估工期+测试预估工期）*预估开发成本
                    result.DevCost = result.PredictDevCost.Value * (result.BeforeSaleDemandDeptInfos.Sum(x => x.DevEstimate) + result.BeforeSaleDemandDeptInfos.Sum(x => x.TestEstimate));
                    result.IsHandle = true;
                }
                else if (detail.Status == 6 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发总助")) && flowInstance.ActivityName == "立项")
                {
                    result.IsHandle = true;
                }
                else if (detail.Status == 7 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-需求工程师")) && flowInstance.ActivityName == "需求提交")
                {
                    result.IsHandle = true;
                }
                else if (detail.Status == 8 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发工程师")) && flowInstance.ActivityName == "研发提交")
                {
                    result.IsHandle = true;
                }
                else if (detail.Status == 9 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-测试工程师")) && flowInstance.ActivityName == "测试提交")
                {
                    //1、如A01-5【研发确认】中选择了需开发人员实施，则流向A01-7【立项】指定的开发负责人，此时自动填充，且不可更改
                    //2、如不需要开发人员实施，由测试人员录入实际实施人员，
                    if (detail.IsDevDeploy == 1)
                    {
                    }
                    result.IsHandle = true;
                }
                else if (detail.Status == 10 && loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发工程师")) && flowInstance.ActivityName == "实施提交")
                {
                    result.IsHandle = true;
                }
                else if (detail.Status == 11 && detail.ApplyUserId.Equal(loginContext.User.Id) && flowInstance.ActivityName == "客户验收")
                { //1、节点人员：流程发起人
                  //2、审批节点，反馈是否验收成功和客户反馈即可
                    result.IsHandle = true;
                }
                else
                {
                    result.IsHandle = false;
                }
                #endregion
            }
            var beforesalefiles = detail.Beforesalefiles.Select(s => new { s.FileId, s.Type }).ToList();
            var beforesalefileIds = beforesalefiles.Select(s => s.FileId).ToList();
            var files = await UnitWork.Find<UploadFile>(f => beforesalefileIds.Contains(f.Id)).ToListAsync();
            result.Files = files.MapTo<List<UploadFileResp>>();
            result.Files.ForEach(f => f.PictureType = beforesalefiles.Where(p => f.Id.Equals(p.FileId)).Select(p => p.Type).FirstOrDefault());

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
            //if (req.BeforeSaleDemandOrders != null && req.BeforeSaleDemandOrders.Count == 0)
            //{
            //    throw new Exception("请关联单据！");
            //}
            if (req.DemandContents == null)
            {
                throw new Exception("请填写需求简述！");
            }

            var dbContext = UnitWork.GetDbContext<BeforeSaleDemand>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var single = await UnitWork.Find<BeforeSaleDemand>(c => c.Id == req.Id)
                        .Include(c => c.BeforeSaleDemandOrders)
                        .Include(c => c.Beforesalefiles)
                        .Include(c => c.Beforesaledemandoperationhistories)
                        .FirstOrDefaultAsync();
                    //关联单据
                    if (req.BeforeSaleDemandOrders != null && req.BeforeSaleDemandOrders.Count > 0)
                    {
                        req.BeforeSaleDemandOrders.ForEach(c =>
                        {
                            obj.BeforeSaleDemandOrders.Add(new BeforeSaleDemandOrders
                            {
                                CustomerId = c.CustomerId,
                                CustomerName = c.CustomerName,
                                MaterialCode = c.MaterialCode,
                                MaterialDescription = c.MaterialDescription,
                                Num = c.Num,
                                Type = c.Type,
                                BeforeSaleDemandId = (single != null && single.Id > 0) ? single.Id : 0
                            });
                        });
                    }
                    //售前需求申请附件
                    if (req.Attchments != null && req.Attchments.Count > 0)
                    {
                        req.Attchments.ForEach(c =>
                        {
                            obj.Beforesalefiles.Add(new BeforeSaleFiles { FileId = c, BeforeSaleDemandId = (single != null && single.Id > 0) ? single.Id : 0, Type = 0 });
                        });
                    }
                    //特别注意！！！！！
                    //草稿状态 只保存数据不创建流程
                    if (req.IsDraft)
                    {
                        obj.Status = 0;
                    }
                    else
                    {
                        obj.Status = 1;
                        //创建流程
                        var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "售前需求");
                        var flow = new AddFlowInstanceReq();
                        flow.SchemeId = mf.FlowSchemeId;
                        flow.FrmType = 2;
                        flow.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        flow.CustomName = $"售前需求申请{DateTime.Now}";
                        flow.FrmData = "";
                        //审批流程id
                        obj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(flow);
                    }
                    if (single != null && single.Id > 0)
                    {
                        obj.CreateTime = single.CreateTime;
                        obj.CreateUserId = single.CreateUserId;
                        obj.CreateUserName = single.CreateUserName;

                        obj.UpdateTime = DateTime.Now;
                        //子表删除重新添加
                        if (obj.Beforesalefiles.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<BeforeSaleFiles>(single.Beforesalefiles.ToArray());
                            await UnitWork.BatchAddAsync<BeforeSaleFiles>(obj.Beforesalefiles.ToArray());
                        }
                        if (obj.BeforeSaleDemandOrders.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<BeforeSaleDemandOrders>(single.BeforeSaleDemandOrders.ToArray());
                            await UnitWork.BatchAddAsync<BeforeSaleDemandOrders>(obj.BeforeSaleDemandOrders.ToArray());
                        }
                        obj.Beforesalefiles = null;
                        obj.BeforeSaleDemandOrders = null;
                        await UnitWork.UpdateAsync(obj);
                        await UnitWork.SaveAsync();
                    }
                    else
                    {
                        //添加售前需求申请流程
                        obj.UpdateTime = DateTime.Now;
                        obj.CreateTime = DateTime.Now;
                        obj.CreateUserId = loginContext.User.Id;
                        obj.CreateUserName = loginContext.User.Name;
                        obj.ApplyUserId = loginContext.User.Id;//申请人id
                        obj.ApplyUserName = loginContext.User.Name;//申请人
                        obj.ApplyDate = DateTime.Now;//申请日期
                        obj.BeforeDemandCode = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();//流程编号
                        obj = await UnitWork.AddAsync<BeforeSaleDemand, int>(obj);
                        //1、流程添加成功==>关联单据
                        //obj.BeforeSaleDemandOrders = req.BeforeSaleDemandOrders;
                        //await UnitWork.BatchAddAsync<BeforeSaleDemandOrders>(obj.BeforeSaleDemandOrders.ToArray());
                        //2、添加流程操作记录
                        //添加售前需求审批流程操作记录
                        //这样才能取到BeforeSaleDemandId = obj.Id
                        obj.Beforesaledemandoperationhistories = new List<BeforeSaleDemandOperationHistory>();
                        obj.Beforesaledemandoperationhistories.Add(new BeforeSaleDemandOperationHistory
                        {
                            Action = "发起",
                            CreateUser = loginContext.User.Name,
                            CreateUserId = loginContext.User.Id,
                            CreateTime = DateTime.Now,
                            BeforeSaleDemandId = obj.Id,
                            ApprovalStage = 1,
                            Remark = req.Remark
                        });
                        await UnitWork.BatchAddAsync<BeforeSaleDemandOperationHistory>(obj.Beforesaledemandoperationhistories.ToArray());
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
                    }
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
                                        .Include(b => b.Beforesaledemandoperationhistories)
                                        .Include(b => b.BeforeSaleDemandDeptInfos)
                                        .Include(b => b.Beforesaledemandprojects)
                                        .Include(b => b.BeforeSaleProSchedulings)
                                        .FirstOrDefaultAsync();
            var flowinstace = await UnitWork.Find<FlowInstance>(c => c.Id == beforeSaleDemand.FlowInstanceId).FirstOrDefaultAsync();
            var orgRole = await (from a in UnitWork.Find<Relevance>(c => c.Key == Define.ORGROLE)
                                 join b in UnitWork.Find<User>(null) on a.FirstId equals b.Id into ab
                                 from b in ab.DefaultIfEmpty()
                                 select new { a.SecondId, b }).ToListAsync();
            //审批
            VerificationReq verificationReq = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = beforeSaleDemand.FlowInstanceId,
                VerificationFinally = "1",
                VerificationOpinion = req.Remark //审核意见
            };
            if (req.IsReject)//驳回
            {
                verificationReq.VerificationFinally = "3";
                verificationReq.NodeRejectType = "1";
                beforeSaleDemand.Status = 13;//驳回状态
                await _flowInstanceApp.Verification(verificationReq);
            }
            else
            {
                if (loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-销售总助")) && flowinstace.ActivityName == "销售总助审批")
                {
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 2;
                    beforeSaleDemand.FactDemandUser = req.FactDemandUser;
                    beforeSaleDemand.FactDemandUserId = req.FactDemandUserId;
                    //设置需求环节执行人
                    await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, true, new string[] { req.FactDemandUserId }, req.FactDemandUser, false);
                }
                else if (loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-需求工程师")) && flowinstace.ActivityName == "需求组提交需求")
                {
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 3;
                    beforeSaleDemand.FirstConnects = req.FirstConnects;//初步沟通内容
                }
                else if (loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发总助")) && flowinstace.ActivityName == "研发总助审批")
                {
                    //确定开发部门
                    req.BeforeSaleDemandDeptInfos.ForEach(c =>
                    {
                        var user = orgRole.Where(o => o.SecondId == c.OrgId).FirstOrDefault();
                        beforeSaleDemand.BeforeSaleDemandDeptInfos.Add(new BeforeSaleDemandDeptInfo
                        {
                            OrgId = c.OrgId,
                            OrgName = c.OrgName,
                            UserId = user?.b?.Id,
                            UserName = user?.b?.Name,
                            BeforeSaleDemandId = beforeSaleDemand.Id
                        });
                    });
                    //运行时指定用户
                    verificationReq.NodeDesignateType = Setinfo.RUNTIME_SPECIAL_USER;
                    verificationReq.NodeDesignates = beforeSaleDemand.BeforeSaleDemandDeptInfos.Select(x => x.UserId).ToArray();
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 4;//研发总助=>确定开发部门
                    beforeSaleDemand.PredictDevCost = req.PredictDevCost;
                    //批量添加操作
                    if (req.BeforeSaleDemandDeptInfos != null && req.BeforeSaleDemandDeptInfos.Count > 0)
                    {
                        await UnitWork.BatchAddAsync(beforeSaleDemand.BeforeSaleDemandDeptInfos.ToArray());
                    }

                    //设置研发环节执行人【所选的研发部门负责人接收流程审批】
                    var devUserIds = beforeSaleDemand.BeforeSaleDemandDeptInfos.Select(x => x.UserId).ToArray();
                    var devUserNames = string.Join(',', beforeSaleDemand.BeforeSaleDemandDeptInfos.Select(x => x.UserName).ToArray());
                    await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, true, devUserIds, "", false);
                }
                else if (loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发工程师")) && flowinstace.ActivityName == "研发确认")
                {
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 5;
                    beforeSaleDemand.DevEstimate = req.DevEstimate;//开发预估
                    beforeSaleDemand.TestEstimate = req.TestEstimate;//测试预估
                    //研发确认 工期
                    await UnitWork.UpdateAsync<BeforeSaleDemandDeptInfo>(c => c.BeforeSaleDemandId == req.Id && c.UserId == loginContext.User.Id, c => new BeforeSaleDemandDeptInfo
                    {
                        DevEstimate = req.DevEstimate.Value,//开发预估
                        TestEstimate = req.TestEstimate.Value//测试预估
                    });
                    beforeSaleDemand.IsDevDeploy = req.IsDevDeploy;//是否需要开发支持
                }
                else if (loginContext.Roles.Any(c => c.Name.Equal("总经理")) && flowinstace.ActivityName == "总经理审批")
                {
                    //总经理审批
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 6;
                    beforeSaleDemand.IsCharge = req.IsCharge;//是否收费
                    beforeSaleDemand.DevCost = req.DevCost;//开发投入预估（开发预估工期+测试预估工期）*预估开发成本
                }
                else if (loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发总助")) && flowinstace.ActivityName == "立项")
                {
                    //立项时指定下一节点【需求提交】执行人
                    verificationReq.NodeDesignateType = Setinfo.RUNTIME_SPECIAL_USER;
                    verificationReq.NodeDesignates = new string[] { beforeSaleDemand.FactDemandUserId };
                    //项目立项 指定下一节点：
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 7;//项目立项
                    //1、是否关联项目
                    beforeSaleDemand.IsRelevanceProject = req.IsRelevanceProject;
                    if (req.IsRelevanceProject == 1)
                    {
                        beforeSaleDemand.BeforeSaleDemandProjectName = req.BeforeSaleDemandProjectName;
                        beforeSaleDemand.BeforeSaleProSchedulings = req.BeforeSaleProSchedulings;
                    }
                    else
                    {
                        beforeSaleDemand.Beforesaledemandprojects.Add(new BeforeSaleDemandProject
                        {
                            //创建项目
                            Status = 1,//项目立项
                            BeforeSaleDemandId = beforeSaleDemand.Id,
                            ProjectName = req.BeforeSaleDemandProjectName,
                            ProjectNum = "XM" + DateTime.Now.ToString("yyyyMMddHHmm"),//项目编号:XM202201202023
                            PromoterId = beforeSaleDemand.ApplyUserId,//项目发起人ID
                            PromoterName = beforeSaleDemand.ApplyUserName,//项目发起人
                            ReqUserId = beforeSaleDemand.FactDemandUserId,//需求负责人Id
                            ReqUserName = beforeSaleDemand.FactDemandUser,//需求负责人名字
                            DevUserId = req.BeforeSaleProSchedulings.Find(x => x.Stage == 1).UserId,//研发负责人
                            DevUserName = req.BeforeSaleProSchedulings.Find(x => x.Stage == 1).UserName,
                            TestUserId = req.BeforeSaleProSchedulings.Find(x => x.Stage == 2).UserId,//测试负责人
                            TestUserName = req.BeforeSaleProSchedulings.Find(x => x.Stage == 2).UserName,
                            FlowInstanceId = beforeSaleDemand.FlowInstanceId,//流程id
                            //需要研发实施，实施负责人为研发负责人
                            ExecutorUserId = beforeSaleDemand.IsDevDeploy.Value == 1 ? req.BeforeSaleProSchedulings.Find(x => x.Stage == 1).UserId : "",
                            ExecutorName = beforeSaleDemand.IsDevDeploy.Value == 1 ? req.BeforeSaleProSchedulings.Find(x => x.Stage == 1).UserName : "",
                            CreateTime = DateTime.Now,
                            CreateUserId = loginContext.User.Id,
                            CreateUserName = loginContext.User.Name
                        });
                        await UnitWork.BatchAddAsync<BeforeSaleDemandProject, int>(beforeSaleDemand.Beforesaledemandprojects.ToArray());
                        UnitWork.Save();
                    }
                    req.BeforeSaleProSchedulings.ForEach(c =>
                    {
                        beforeSaleDemand.BeforeSaleProSchedulings.Add(new BeforeSaleProScheduling
                        {
                            UserId = c.UserId,
                            UserName = c.UserName,
                            Stage = c.Stage,
                            StartDate = c.StartDate,
                            EndDate = c.EndDate,
                            CreateTime = DateTime.Now,
                            CreateUserId = loginContext.User.Id,
                            BeforeSaleDemandId = beforeSaleDemand.Id,
                            BeforeSaleDemandProjectId = beforeSaleDemand.Beforesaledemandprojects[0].Id
                        });
                    });
                    //更新流程申请表的项目id和项目名称
                    beforeSaleDemand.BeforeSaleDemandProjectId = beforeSaleDemand.Beforesaledemandprojects[0].Id;
                    beforeSaleDemand.BeforeSaleDemandProjectName = beforeSaleDemand.Beforesaledemandprojects[0].ProjectName;
                    //批量添加项目排期
                    if (req.BeforeSaleProSchedulings != null && req.BeforeSaleProSchedulings.Count > 0)
                    {
                        await UnitWork.BatchAddAsync<BeforeSaleProScheduling, int>(beforeSaleDemand.BeforeSaleProSchedulings.ToArray());
                    }
                }
                else if (loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-需求工程师")) && flowinstace.ActivityName == "需求提交")
                {
                    //需求提交时指定下一节点【研发提交】执行人
                    verificationReq.NodeDesignateType = Setinfo.RUNTIME_SPECIAL_USER;
                    verificationReq.NodeDesignates = new string[] { beforeSaleDemand.Beforesaledemandprojects.Find(x => x.BeforeSaleDemandId.Value == beforeSaleDemand.Id).DevUserId };
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 8;
                    beforeSaleDemand.Beforesaledemandprojects.First().Status = 2;
                    beforeSaleDemand.Beforesaledemandprojects.First().UpdateTime = DateTime.Now;
                    beforeSaleDemand.Beforesaledemandprojects.First().ProjectUrl = req.ProjectUrl;
                    beforeSaleDemand.Beforesaledemandprojects.First().ProjectDocURL = req.ProjectDocURL;
                    beforeSaleDemand.Beforesaledemandprojects.First().ActualStartDate = req.ActualStartDate;
                    beforeSaleDemand.Beforesaledemandprojects.First().SubmitDate = req.SubmitDate;
                    await UnitWork.UpdateAsync<BeforeSaleDemandProject>(beforeSaleDemand.Beforesaledemandprojects.First());
                }
                else if (loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-研发工程师")) && flowinstace.ActivityName == "研发提交")
                {
                    //研发提交时指定下一节点【测试提交】执行人
                    verificationReq.NodeDesignateType = Setinfo.RUNTIME_SPECIAL_USER;
                    verificationReq.NodeDesignates = new string[] { beforeSaleDemand.Beforesaledemandprojects.Find(x => x.BeforeSaleDemandId.Value == beforeSaleDemand.Id).TestUserId };
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 9;//研发提交=》实际研发开始日期
                    beforeSaleDemand.Beforesaledemandprojects.First().Status = 3;
                    beforeSaleDemand.Beforesaledemandprojects.First().UpdateTime = DateTime.Now;
                    beforeSaleDemand.Beforesaledemandprojects.First().ActualDevStartDate = req.ActualDevStartDate;
                    beforeSaleDemand.Beforesaledemandprojects.First().ActualDevEndDate = req.ActualDevEndDate;
                    await UnitWork.UpdateAsync<BeforeSaleDemandProject>(beforeSaleDemand.Beforesaledemandprojects.First());
                }
                else if (loginContext.Roles.Any(c => c.Name.Equal("需求反馈审批-测试工程师")) && flowinstace.ActivityName == "测试提交")
                {
                    //测试提交时指定下一节点【实施提交】执行人
                    verificationReq.NodeDesignateType = Setinfo.RUNTIME_SPECIAL_USER;
                    //1、如A01-5【研发确认】中选择了需开发人员实施，则流向A01-7【立项】指定的开发负责人，此时自动填充，且不可更改
                    //2、如不需要开发人员实施，有测试人员录入实际实施人员
                    var executorUserId = beforeSaleDemand.Beforesaledemandprojects.Find(x => x.BeforeSaleDemandId.Value == beforeSaleDemand.Id).DevUserId;
                    var executorName = beforeSaleDemand.Beforesaledemandprojects.Find(x => x.BeforeSaleDemandId.Value == beforeSaleDemand.Id).DevUserName;                    
                    if (beforeSaleDemand.IsDevDeploy.HasValue && beforeSaleDemand.IsDevDeploy == 1)
                    {
                        beforeSaleDemand.Beforesaledemandprojects.Find(x => x.BeforeSaleDemandId.Value == beforeSaleDemand.Id).ExecutorUserId = executorUserId;
                        beforeSaleDemand.Beforesaledemandprojects.Find(x => x.BeforeSaleDemandId.Value == beforeSaleDemand.Id).ExecutorName = executorName;
                    }
                    else
                    {
                        executorUserId = req.ExecutorUserId;
                        beforeSaleDemand.Beforesaledemandprojects.Find(x => x.BeforeSaleDemandId.Value == beforeSaleDemand.Id).ExecutorUserId = req.ExecutorUserId;
                        beforeSaleDemand.Beforesaledemandprojects.Find(x => x.BeforeSaleDemandId.Value == beforeSaleDemand.Id).ExecutorName = req.ExecutorName;
                    }
                    verificationReq.NodeDesignates = new string[] { executorUserId };//实施人员id
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 10;//测试提交=》实际测试开始日期
                    beforeSaleDemand.Beforesaledemandprojects.First().Status = 4;
                    beforeSaleDemand.Beforesaledemandprojects.First().UpdateTime = DateTime.Now;
                    beforeSaleDemand.Beforesaledemandprojects.First().ActualTestStartDate = req.ActualTestStartDate;
                    beforeSaleDemand.Beforesaledemandprojects.First().ActualTestEndDate = req.ActualTestEndDate;

                    await UnitWork.UpdateAsync<BeforeSaleDemandProject>(beforeSaleDemand.Beforesaledemandprojects.First());
                }
                else if (flowinstace.ActivityName == "实施提交" && beforeSaleDemand.Beforesaledemandprojects.Any() && beforeSaleDemand.Beforesaledemandprojects.First().ExecutorUserId == loginContext.User.Id)
                {
                    //实施提交时指定下一节点【客户验收】执行人：项目申请人
                    verificationReq.NodeDesignateType = Setinfo.RUNTIME_SPECIAL_USER;
                    verificationReq.NodeDesignates = new string[] { beforeSaleDemand.ApplyUserId };
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 11;
                    beforeSaleDemand.Beforesaledemandprojects.First().Status = 5;
                    beforeSaleDemand.Beforesaledemandprojects.First().UpdateTime = DateTime.Now;
                    await UnitWork.UpdateAsync<BeforeSaleDemandProject>(beforeSaleDemand.Beforesaledemandprojects.First());
                }
                //else if (loginContext.Roles.Any(c => c.Name.Equal("销售员")) && flowinstace.ActivityName == "客户验收")
                //客户验收是最后一个节点 即：当前用户是流程发起人才能进行客户验收操作 结束流程审核
                else if (loginContext.User.Id == flowinstace.CreateUserId && flowinstace.ActivityName == "客户验收")
                {
                    await _flowInstanceApp.Verification(verificationReq);
                    beforeSaleDemand.Status = 12;
                    beforeSaleDemand.Beforesaledemandprojects.First().Status = 6;
                    beforeSaleDemand.Beforesaledemandprojects.FirstOrDefault().UpdateTime = DateTime.Now;
                    await UnitWork.UpdateAsync<BeforeSaleDemandProject>(beforeSaleDemand.Beforesaledemandprojects.First());
                }
                else
                {
                    throw new Exception("您没有" + flowinstace.ActivityName + "节点的审核权限！");
                }
            }
            #region 添加售前需求审批流程操作记录
            BeforeSaleDemandOperationHistory beforeSaleDemandOperationHistory = new BeforeSaleDemandOperationHistory
            {
                Action = flowinstace.ActivityName,
                CreateUser = loginContext.User.Name,
                CreateUserId = loginContext.User.Id,
                CreateTime = DateTime.Now,
                BeforeSaleDemandId = req.Id,
                ApprovalStage = beforeSaleDemand.Status,
                Remark = req.Remark
            };
            var fioh = await UnitWork.Find<BeforeSaleDemandOperationHistory>(r => r.BeforeSaleDemandId.Equals(beforeSaleDemand.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            if (fioh != null)
            {
                beforeSaleDemandOperationHistory.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(fioh.CreateTime)).TotalSeconds);
            }
            await UnitWork.AddAsync(beforeSaleDemandOperationHistory);
            #endregion

            #region 添加审批附件
            if (req.Attchments != null && req.Attchments.Count > 0)
            {
                List<BeforeSaleFiles> beforeSaleFiles = new List<BeforeSaleFiles>();
                req.Attchments.ForEach(c =>
                {
                    beforeSaleFiles.Add(new BeforeSaleFiles { FileId = c, BeforeSaleDemandOperationHistoryId = beforeSaleDemandOperationHistory.Id, Type = 1 });
                });
                await UnitWork.BatchAddAsync<BeforeSaleFiles>(beforeSaleFiles.ToArray());
            }
            #endregion
            //异步执行更新售前需求申请表单详情信息
            beforeSaleDemand.UpdateTime = DateTime.Now;//更新时间
            await UnitWork.UpdateAsync<BeforeSaleDemand>(beforeSaleDemand);
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 获取售前需求申请流程操作记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetBeforeSaleDemandOperationHistory(QueryBeforeSaleDemandOperationHistoryListReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var listResp = new List<BeforeSaleDemandOperationHistoryResp>();

            var result = new TableData();
            var historyList = await UnitWork.Find<BeforeSaleDemandOperationHistory>(x => x.BeforeSaleDemandId == req.BeforeSaleDemandId)
                        .Include(c => c.BeforeSaleFiles).Where(c => c.BeforeSaleDemandId == req.BeforeSaleDemandId).OrderBy(r => r.CreateTime).ToListAsync();
            listResp = historyList.MapToList<BeforeSaleDemandOperationHistoryResp>();
            List<string> fileids = new List<string>();
            historyList.ForEach(q => fileids.AddRange(q.BeforeSaleFiles.Select(f => f.FileId).ToList()));
            var files = await UnitWork.Find<UploadFile>(f => fileids.Contains(f.Id)).ToListAsync();
            //遍历文件id
            listResp.ForEach(c =>
            {
                var fileids = historyList.FirstOrDefault(m => m.Id.Equals(c.Id)).BeforeSaleFiles.Select(p => p.FileId).ToArray();
                c.Files = files.Where(p => fileids.Contains(p.Id)).MapToList<UploadFileResp>();
            });
            result.Data = listResp;
            result.Count = listResp.Count;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
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