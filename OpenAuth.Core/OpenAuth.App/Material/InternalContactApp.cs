using Infrastructure;
using Infrastructure.Extensions;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Workbench;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Request;
using Infrastructure.Mail;
using System.IO;
using Infrastructure.Export;
using DinkToPdf;

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 内部联络单
    /// </summary>
    public class InternalContactApp: OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly WorkbenchApp _workbenchApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        public InternalContactApp(IUnitWork unitWork, IAuth auth, FlowInstanceApp flowInstanceApp, WorkbenchApp workbenchApp, ModuleFlowSchemeApp moduleFlowSchemeApp) :base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _workbenchApp = workbenchApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
        }

        /// <summary>
        /// 查询联络单列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> Load(QueryInternalContactReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();

            List<int> iw = new List<int>();
            if (!string.IsNullOrWhiteSpace(req.ReceiveOrg))
            {
                var ids = await UnitWork.Find<InternalContactDeptInfo>(c => c.OrgName == req.ReceiveOrg && c.Type == 1).Select(c => c.InternalContactId).ToListAsync();
                iw.AddRange(ids);
            }
            if (!string.IsNullOrWhiteSpace(req.ExecOrg))
            {
                var ids = await UnitWork.Find<InternalContactDeptInfo>(c => c.OrgName == req.ExecOrg && c.Type == 2).Select(c => c.InternalContactId).ToListAsync();
                iw.AddRange(ids);
            }
            var query = UnitWork.Find<InternalContact>(null).Include(c=>c.InternalContactDeptInfos)
                        //.WhereIf(loginContext.User.Account != Define.SYSTEM_USERNAME, c => c.CreateUserId == loginContext.User.Id)
                        .WhereIf(iw.Count > 0, c => iw.Contains(c.Id))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Id), c => c.IW == req.Id)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Theme), c => c.Theme.Contains(req.Theme))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), c => c.CardCode == req.CardCode)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.CreateUser), c => c.CreateUser == req.CreateUser)
                        .WhereIf(req.Status != null, c => c.Status == req.Status);

            if (req.PageType==1)//我的提交
            {
                query = query.WhereIf(loginContext.User.Account != Define.SYSTEM_USERNAME, c => c.CreateUserId == loginContext.User.Id);
            }
            else if (req.PageType == 2)//待审批
            {
                var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "内部联络单");
                var flowinstace = await UnitWork.Find<FlowInstance>(c => c.SchemeId == mf.FlowSchemeId && c.MakerList.Contains(loginContext.User.Id)).Select(c => c.Id).ToListAsync();
                query = query.Where(c => flowinstace.Contains(c.FlowInstanceId) && c.CreateUserId != loginContext.User.Id && c.Status != 9);

            }
            else if (req.PageType == 3)//已审批
            {
                var instances = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.CreateUserId == loginContext.User.Id).Select(c => c.InstanceId).Distinct().ToListAsync();
                query = query.Where(c => instances.Contains(c.FlowInstanceId) && c.CreateUserId!= loginContext.User.Id && c.Status != 9);
            }
            else if (req.PageType == 4)//待执行
            {
                var subdept = await UnitWork.Find<InternalContactDeptInfo>(c => c.UserId == loginContext.User.Id && c.HandleTime == null).Select(c => c.InternalContactId).ToListAsync();
                query = query.Where(c => subdept.Contains(c.Id) && (c.Status == 4 || c.Status == 8));//执行中或停用中
            }
            else if (req.PageType == 5)//已执行
            {
                var subdept = await UnitWork.Find<InternalContactDeptInfo>(c => c.UserId == loginContext.User.Id && c.HandleTime != null).Select(c => c.InternalContactId).ToListAsync();
                query = query.Where(c => subdept.Contains(c.Id) && c.Status != 9);//执行中或停用中
            }

            var resp= await query
                                .Skip((req.page - 1) * req.limit)
                                .Take(req.limit)
                                .ToListAsync();
            result.Data = resp.Select(c => new
            {
                c.Id,
                c.IW,
                c.Theme,
                ReceiveOrg=string.Join(",", c.InternalContactDeptInfos.Where(o => o.Type == 1).Select(c=>c.OrgName)),
                ExecOrg = string.Join(",", c.InternalContactDeptInfos.Where(o => o.Type == 2).Select(c => c.OrgName)),
                c.CardCode,
                c.CardName,
                c.CreateUser,
                c.CreateTime,
                c.ApproveTime,
                c.ExecTime,
                c.Status
            });
            result.Count = await query.CountAsync();
            return result;
        }

        /// <summary>
        /// 添加内部联络单
        /// </summary>
        /// <returns></returns>
        public async Task Add(AddOrUpdateInternalContactReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var obj = req.MapTo<InternalContact>();
            var dbContext = UnitWork.GetDbContext<InternalContact>();
            var orgRole = await (from a in UnitWork.Find<Relevance>(c => c.Key == Define.ORGROLE)
                                 join b in UnitWork.Find<User>(null) on a.FirstId equals b.Id into ab
                                 from b in ab.DefaultIfEmpty()
                                 select new { a.SecondId, b }).ToListAsync();

            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var single = await UnitWork.Find<InternalContact>(c => c.Id == req.Id)
                        .Include(c => c.InternalContactAttchments)
                        .Include(c => c.InternalContactBatchNumbers)
                        .Include(c => c.InternalContactDeptInfos)
                        .FirstOrDefaultAsync();

                    obj.Status = 1;
                    obj.CardCode = string.Join(",", req.CardCodes);
                    obj.CardName = string.Join(",", req.CardNames);
                    obj.Reason = string.Join(",", req.Reasons);
                    obj.AdaptiveRange = string.Join(",", req.AdaptiveRanges);

                    #region 添加子表数据
                    obj.InternalContactAttchments = new List<InternalContactAttchment>();
                    obj.InternalContactBatchNumbers = new List<InternalContactBatchNumber>();
                    obj.InternalContactDeptInfos = new List<InternalContactDeptInfo>();
                    //附件
                    req.Attchments.ForEach(c =>
                    {
                        obj.InternalContactAttchments.Add(new InternalContactAttchment { FileId = c, InternalContactId = (single != null && single.Id > 0) ? single.Id : 0 });
                    });
                    //批次号
                    req.BatchNumbers.ForEach(c =>
                    {
                        obj.InternalContactBatchNumbers.Add(new InternalContactBatchNumber { Number = c, InternalContactId = (single != null && single.Id > 0) ? single.Id : 0 });
                    });

                    //接收部门
                    req.InternalContactReceiveDepts.ForEach(c =>
                    {
                        var user = orgRole.Where(o => o.SecondId == c.OrgId).FirstOrDefault();
                        obj.InternalContactDeptInfos.Add(new InternalContactDeptInfo
                        {
                            OrgId = c.OrgId,
                            OrgName = c.OrgName,
                            UserId = user?.b?.Id,
                            UserName = user?.b?.Name,
                            Type = 1,
                            InternalContactId = (single != null && single.Id > 0) ? single.Id : 0
                        });
                    });
                    //执行部门
                    req.InternalContactExecDepts.ForEach(c =>
                    {
                        var user = orgRole.Where(o => o.SecondId == c.OrgId).FirstOrDefault();
                        obj.InternalContactDeptInfos.Add(new InternalContactDeptInfo
                        {
                            OrgId = c.OrgId,
                            OrgName = c.OrgName,
                            UserId = user?.b?.Id,
                            UserName = user?.b?.Name,
                            Type = 2,
                            InternalContactId = (single != null && single.Id > 0) ? single.Id : 0
                        });
                    });
                    #endregion

                    if (single != null && single.Id > 0)
                    {
                        obj.IW = single.IW;
                        obj.FlowInstanceId = single.FlowInstanceId;
                        obj.CreateTime = single.CreateTime;
                        obj.CreateUserId = single.CreateUserId;
                        obj.CreateUser = single.CreateUser;
                        //子表删除重新添加
                        if (obj.InternalContactAttchments.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<InternalContactAttchment>(single.InternalContactAttchments.ToArray());
                            await UnitWork.BatchAddAsync<InternalContactAttchment>(obj.InternalContactAttchments.ToArray());
                        }
                        if (obj.InternalContactDeptInfos.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<InternalContactDeptInfo>(single.InternalContactDeptInfos.ToArray());
                            await UnitWork.BatchAddAsync<InternalContactDeptInfo>(obj.InternalContactDeptInfos.ToArray());
                        }
                        if (obj.InternalContactBatchNumbers.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<InternalContactBatchNumber>(single.InternalContactBatchNumbers.ToArray());
                            //await UnitWork.DeleteAsync<InternalContactBatchNumber>(c => c.InternalContactId == single.Id);
                            //await UnitWork.SaveAsync();
                            await UnitWork.BatchAddAsync<InternalContactBatchNumber>(obj.InternalContactBatchNumbers.ToArray());
                        }
                        obj.InternalContactAttchments = null;
                        obj.InternalContactBatchNumbers = null;
                        obj.InternalContactDeptInfos = null;

                        await UnitWork.UpdateAsync(obj);

                        VerificationReq verificationReq = new VerificationReq
                        {
                            NodeRejectStep = "",
                            NodeRejectType = "0",
                            FlowInstanceId = single.FlowInstanceId,
                            VerificationFinally = "1",
                            VerificationOpinion = ""
                        };

                        await _flowInstanceApp.Verification(verificationReq);
                        await UnitWork.SaveAsync();
                    }
                    else
                    {
                        var ic = await UnitWork.Find<InternalContact>(null).OrderByDescending(c => c.IW).Select(c => c.IW).FirstOrDefaultAsync();
                        var icId = 2000;
                        if (!string.IsNullOrWhiteSpace(ic) && ic != null)
                            icId = Convert.ToInt32(ic) + 1;

                        //obj.Status = 1;
                        obj.IW = icId.ToString();
                        obj.CreateTime = DateTime.Now;
                        obj.CreateUserId = loginContext.User.Id;
                        obj.CreateUser = loginContext.User.Name;


                        //创建流程
                        var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "内部联络单");
                        var flow = new AddFlowInstanceReq();
                        flow.SchemeId = mf.FlowSchemeId;
                        flow.FrmType = 2;
                        flow.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        flow.CustomName = $"内部联络单{DateTime.Now}";
                        flow.FrmData = "";
                        obj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(flow);


                        obj = await UnitWork.AddAsync<InternalContact, int>(obj);

                        //设置测试环节执行人
                        await _flowInstanceApp.ModifyNodeUser(obj.FlowInstanceId, true, new string[] { obj.CheckApproveId }, obj.CheckApprove, false);

                        await UnitWork.SaveAsync();

                        #region 发送邮件
                        await SebdEmail(obj, "");
                        #endregion
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加内部联络单失败。" + ex.Message);
                }
            }
        }

        private async Task SebdEmail(InternalContact obj,string title)
        {
            MailRequest mailRequest = new MailRequest();
            mailRequest.Subject = obj.Theme;
            mailRequest.Priority = 1;
            mailRequest.FromUser = new MailUser { Name = "ERP4.0通知", Address = Define.MailAccount, Password = Define.MailPassword };
            var orgIds = obj.InternalContactDeptInfos.Select(c => c.OrgId).ToList();
            var userIds = await UnitWork.Find<Relevance>(c => orgIds.Contains(c.SecondId) && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
            userIds.Add(obj.CheckApproveId);
            userIds.Add(obj.DevelopApproveId);
            var userInfo = await UnitWork.Find<User>(c => userIds.Contains(c.Id)).ToListAsync();
            mailRequest.ToUsers = new List<MailUser> { new MailUser { Name = "licong", Address = "licong@neware.com.cn" } };
            //userInfo.ForEach(c =>
            //{
            //    mailRequest.ToUsers.Add(new MailUser { Name = c.Account, Address = c.Email });
            //}); 
            //附件
            mailRequest.Attachments = new List<MailAttachment>();
            if (obj.InternalContactAttchments.Count>0)
            {
                var fileIds = obj.InternalContactAttchments.Select(c => c.FileId).ToList();
                var file = await UnitWork.Find<UploadFile>(c => fileIds.Contains(c.Id)).ToListAsync();
                file.ForEach(c =>
                {
                    mailRequest.Attachments.Add(new MailAttachment
                    {
                        FilePath = Path.Combine("D:\\nsap4file", c.BucketName, c.FilePath),
                        FileName = c.FileName,
                        FileType = c.FileType
                    });
                });
            }
            #region content
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < obj.InternalContactBatchNumbers.Count; i++)
                sb.Append($"<b>批次号{(i + 1)}：</b><span>{obj.InternalContactBatchNumbers[i].Number}</span><br>");

            string content = $@"{title}
                                    <b>主题：</b><br>
                                    <span>{obj.Theme}</span><br>
                                    <b>IW号：</b><span>{obj.IW}</span><br>
                                    <b>重点客户代码：</b><span>{obj.CardCode}</span><br>
                                    <b>重点客户名称：</b><span>{obj.CardName}</span><br>
                                    <b>rdms项目号：</b><span>{obj.RdmsNo}</span><br>
                                    <b>适应型号：</b><span>{obj.AdaptiveModel}</span><br>
                                    <b>销售单号：</b><span>{obj.SaleOrderNo}</span><br>
                                    <b>生产单号：</b><span>{obj.ProductionNo}</span><br>
                                    {sb.ToString()}
                                    <b>测试审批：</b><span>{obj.CheckApprove}</span><br>
                                    <b>研发审批：</b><span>{obj.DevelopApprove}</span><br>
                                    <b>适应范围：</b><span>{obj.AdaptiveRange}</span><br>
                                    <b>变更原因：</b><span>{obj.Reason}</span><br>
                                    <b>查收部门：</b><span>{string.Join(",", obj.InternalContactDeptInfos.Where(c => c.Type == 1).Select(c => c.OrgName).ToList())}</span><br>
                                    <b>执行部门：</b><span>{string.Join(",", obj.InternalContactDeptInfos.Where(c => c.Type == 2).Select(c => c.OrgName).ToList())}</span><br>
                                    <b>变更内容：</b><br>
                                    {obj.Content}";
            #endregion
            mailRequest.CcUsers = new List<MailUser>();
            mailRequest.Contents = new List<MailContent> { new MailContent { Type = "html", Content = content } };
            await MailHelper.Sendmail(mailRequest);
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
            var detail = await UnitWork.Find<InternalContact>(c => c.Id == id)
                            .Include(c => c.InternalContactAttchments)
                            .Include(c => c.InternalContactBatchNumbers)
                            .Include(c => c.InternalContactDeptInfos)
                            .FirstOrDefaultAsync();
            //操作历史
            var operationHistories = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.InstanceId == detail.FlowInstanceId)
                .OrderBy(c => c.CreateDate).Select(h => new
            {
                CreateDate = Convert.ToDateTime(h.CreateDate).ToString("yyyy.MM.dd HH:mm:ss"),
                h.Remark,
                IntervalTime = h.IntervalTime != null && h.IntervalTime > 0 ? h.IntervalTime / 60 : null,
                h.CreateUserName,
                h.Content,
                h.ApprovalResult,
            }).ToListAsync();

            var reviceOrgList = detail.InternalContactDeptInfos.Where(c => c.Type == 1).Select(c => new
            {
                c.OrgName,
                Detail = c.HandleTime != null ? "已查收" : "",
                ReciveTime = c.HandleTime
            });
            var execOrgList = detail.InternalContactDeptInfos.Where(c => c.Type == 2).Select(c => new
            {
                c.OrgName,
                Detail = c.Content,
                ExecTime = c.HandleTime
            });
            result.Data = new
            {
                detail.Id,
                detail.IW,
                detail.Theme,
                CardCodes =!string.IsNullOrWhiteSpace(detail.CardCode)? detail.CardCode.Split(","):new string[] { },
                CardNames = !string.IsNullOrWhiteSpace(detail.CardCode) ? detail.CardName.Split(",") : new string[] { },
                detail.Status,
                detail.RdmsNo,
                detail.SaleOrderNo,
                detail.AdaptiveModel,
                detail.ProductionNo,
                AdaptiveRanges = detail.AdaptiveRange.Split(","),
                Reasons = detail.Reason.Split(","),
                BatchNumbers = detail.InternalContactBatchNumbers,
                detail.CheckApproveId,
                detail.CheckApprove,
                detail.DevelopApproveId,
                detail.DevelopApprove,
                InternalContactReceiveDepts = detail.InternalContactDeptInfos.Where(o => o.Type == 1).Select(c => new { c.OrgId, c.OrgName }).ToList(),
                InternalContactExecDepts = detail.InternalContactDeptInfos.Where(o => o.Type == 2).Select(c => new { c.OrgId, c.OrgName }).ToList(),
                detail.Content,
                reviceOrgList,
                execOrgList,
                operationHistories
            };
            return result;
        }

        /// <summary>
        /// 审批、执行
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationInternalContactReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var internalContact = await UnitWork.Find<InternalContact>(c => c.Id == req.Id).FirstOrDefaultAsync();
            var flowinstace = await UnitWork.Find<FlowInstance>(c => c.Id == internalContact.FlowInstanceId).FirstOrDefaultAsync();
            //审批
            if (req.HanleType == 1)
            {
                VerificationReq verificationReq = new VerificationReq
                {
                    NodeRejectStep = "",
                    NodeRejectType = "0",
                    FlowInstanceId = internalContact.FlowInstanceId,
                    VerificationFinally = "1",
                    VerificationOpinion = req.Remark
                };

                if (req.IsReject)//驳回
                {
                    verificationReq.VerificationFinally = "3";
                    verificationReq.NodeRejectType = "1";

                    internalContact.Status = 6;
                    await _flowInstanceApp.Verification(verificationReq);
                }
                else
                {
                    await _flowInstanceApp.Verification(verificationReq);
                    if (loginContext.Roles.Any(c => c.Name.Equal("联络单测试审批")) && flowinstace.ActivityName == "测试审批") 
                    {
                        internalContact.Status = 2;
                        //设置研发环节执行人
                        await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, true, new string[] { internalContact.DevelopApproveId }, internalContact.DevelopApprove, false);
                    } 
                    else if (loginContext.Roles.Any(c => c.Name.Equal("联络单研发审批")) && flowinstace.ActivityName == "研发审批") internalContact.Status = 3;
                    else if (loginContext.Roles.Any(c => c.Name.Equal("总经理")) && flowinstace.ActivityName == "总经理审批") internalContact.Status = 4;
                    else internalContact.Status = 1;//驳回 撤回提交
                }

                await UnitWork.UpdateAsync<InternalContact>(c => c.Id == req.Id, c => new InternalContact
                {
                    Status = internalContact.Status,
                    ApproveTime = internalContact.Status == 4 ? DateTime.Now : internalContact.ApproveTime
                });
            }
            else if (req.HanleType == 2)//执行
            {
                var loginOrg = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault();
                if (req.ExecType == 1)//接收
                {
                    await UnitWork.UpdateAsync<InternalContactDeptInfo>(c => c.InternalContactId == req.Id && c.OrgId == loginOrg.Id && c.Type == 1, c => new InternalContactDeptInfo
                    {
                        HandleTime = DateTime.Now
                    });
                }
                else if (req.ExecType == 2)//执行
                {
                    await UnitWork.UpdateAsync<InternalContactDeptInfo>(c => c.InternalContactId == req.Id && c.OrgId == loginOrg.Id && c.Type == 2, c => new InternalContactDeptInfo
                    {
                        HandleTime = DateTime.Now,
                        Content = req.Remark
                    });
                }
                //await UnitWork.SaveAsync();

                //var dept = await UnitWork.Find<InternalContactDeptInfo>(c => c.InternalContactId == req.Id).AllAsync(c => c.HandleTime != null);
                //if (dept)
                //{
                //    //全部执行完后
                //    await UnitWork.UpdateAsync<InternalContact>(c => c.Id == req.Id, c => new InternalContact
                //    {
                //        Status = 7
                //    });
                //}
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 撤销\过期\停用\启用
        /// </summary>
        /// <param name="internalContactId"></param>
        /// <param name="handleType">1-撤销 2-过期 3-停用 4-启用</param>
        /// <returns></returns>
        public async Task Revocation(int internalContactId,int handleType)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var internalContact = await UnitWork.Find<InternalContact>(c => c.Id == internalContactId)
                .Include(c => c.InternalContactBatchNumbers)
                .Include(c => c.InternalContactDeptInfos)
                .Include(c => c.InternalContactAttchments)
                .ToListAsync();
            if (handleType==1)//撤销
            {
                var single = internalContact.Where(c => c.Status <= 6).FirstOrDefault();
                if (single == null)
                {
                    throw new Exception("该联络单状态不可撤销。");
                }

                await UnitWork.UpdateAsync<InternalContact>(c => c.Id == internalContactId, c => new InternalContact
                {
                    Status = 5 //撤销
                });
                if (!string.IsNullOrWhiteSpace(single.FlowInstanceId))
                    await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = single.FlowInstanceId });

            }
            else if (handleType == 2)//过期
            {
                var single = internalContact.Where(c => c.Status == 7).FirstOrDefault();
                if (single == null)
                {
                    throw new Exception("该联络单状态不可过期。");
                }
                await UnitWork.UpdateAsync<InternalContact>(c => c.Id == internalContactId, c => new InternalContact
                {
                    Status = 9 //过期
                });
            }
            else if (handleType == 3)//停用
            {
                var single = internalContact.Where(c =>c.Status == 4).FirstOrDefault();
                if (single == null)
                {
                    throw new Exception("该联络单状态不可停用。");
                }
                await UnitWork.UpdateAsync<InternalContact>(c => c.Id == internalContactId, c => new InternalContact
                {
                    Status = 8 //停用
                });

                //添加操作记录
                FlowInstanceOperationHistory flowInstanceOperationHistory = new FlowInstanceOperationHistory
                {
                    InstanceId = single.FlowInstanceId,
                    CreateUserId = loginContext.User.Id,
                    CreateUserName = loginContext.User.Name,
                    CreateDate = DateTime.Now,
                    Content = $"停用",
                    Remark = "",
                    ApprovalResult = "停用",
                    //ActivityId = flowInstance.ActivityId,
                };
                //var fioh = await UnitWork.Find<FlowInstanceOperationHistory>(r => r.InstanceId.Equals(internalContact.FlowInstanceId)).OrderByDescending(r => r.CreateDate).FirstOrDefaultAsync();
                //if (fioh != null)
                //{
                //    flowInstanceOperationHistory.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(fioh.CreateDate)).TotalSeconds);
                //}
                await UnitWork.AddAsync(flowInstanceOperationHistory);


                //发送邮件 
                await SebdEmail(single, "<span style=\"font - size:larger\"><b>暂时停用以下联络单，请勿执行，等待通知</b></span><br><br>");
            }
            else if (handleType == 4)//启用
            {

                var single = internalContact.Where(c =>c.Status == 8).FirstOrDefault();
                if (single == null)
                {
                    throw new Exception("该联络单状态不需要启用。");
                }
                await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = single.FlowInstanceId });

                var history = await UnitWork.FindTrack<FlowInstanceOperationHistory>(c => c.InstanceId == single.FlowInstanceId && c.Content == "撤回").OrderByDescending(c => c.CreateDate).FirstOrDefaultAsync();
                if (history!=null)
                    await UnitWork.DeleteAsync<FlowInstanceOperationHistory>(history);//删除撤回记录

                await _flowInstanceApp.Verification(new VerificationReq
                {
                    NodeRejectStep = "",
                    NodeRejectType = "0",
                    FlowInstanceId = single.FlowInstanceId,
                    VerificationFinally = "1",
                    VerificationOpinion = ""
                });

                await UnitWork.UpdateAsync<InternalContact>(c => c.Id == single.Id, c => new InternalContact
                {
                    Status = 1
                });


                //发邮件
                await SebdEmail(single, "<span style=\"font - size:larger\"><b>重新启用以下联络单，请未完成的同事继续执行</b></span><br><br>");
            }
            await UnitWork.SaveAsync();

        }

        /// <summary>
        /// 打印内部联络单
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<byte[]> PrintInternalContact(string id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var internalId = int.Parse(id);
            var model = await UnitWork.Find<InternalContact>(c => c.Id == internalId).FirstOrDefaultAsync();
            if (model == null)
            {
                throw new Exception("数据不存在！");
            }

            var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "InternalContactHeader.html");
            var text = System.IO.File.ReadAllText(url);
            text = text.Replace("@Model.IW", model.IW.ToString());
            text = text.Replace("@Model.CreateTime", model.CreateTime.ToString("yyyy.MM.dd"));

            var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"InternalContactHeader{model.Id}.html");
            System.IO.File.WriteAllText(tempUrl, text, Encoding.Unicode);

            var contentUrl= Path.Combine(Directory.GetCurrentDirectory(), "Templates", "PrintInternalContact.html");
            var contentext = System.IO.File.ReadAllText(contentUrl);

            contentext = contentext.Replace("@Model.Theme", model.Theme.ToString());
            contentext = contentext.Replace("@Model.Content", model.Content.ToString());
            var contentTemp= Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"PrintInternalContact{model.Id}.html");
            System.IO.File.WriteAllText(contentTemp, contentext, Encoding.Unicode);


            var datas = await ExportAllHandler.Exporterpdf(model, $"PrintInternalContact{model.Id}.html", pdf =>
            {
                pdf.IsWriteHtml = true;
                pdf.PaperKind = PaperKind.A4;
                pdf.Orientation = Orientation.Portrait;
                pdf.IsEnablePagesCount = true;
                pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                //pdf.FooterSettings = new FooterSettings() { HtmUrl = footerTemp };
            });
            System.IO.File.Delete(tempUrl);
            System.IO.File.Delete(contentTemp);
            return datas;
        }

        public async Task AsyncData()
        {
            var contact = await (from a in UnitWork.Find<base_contact>(null)
                                 join b in UnitWork.Find<base_user>(null) on a.user_id.ToString() equals b.user_id.ToString() into ab
                                 from b in ab.DefaultIfEmpty()
                                 select new { a, b.user_nm }
                          ).ToListAsync();
            List<InternalContact> internalContacts = new List<InternalContact>();
            contact.ForEach(c =>
            {
                InternalContact contact1 = new InternalContact();
                contact1.InternalContactDeptInfos = new List<InternalContactDeptInfo>();
                var iw = "";
                if (c.a.topic.Contains("IW"))
                {
                    if (c.a.topic.Contains("IW-"))
                    {
                        var start = c.a.topic.IndexOf("IW-");
                        iw = c.a.topic.Substring(start+2, start + 4);
                    }
                    else
                    {
                        var top = c.a.topic.Replace(" ", "");
                        var start = c.a.topic.IndexOf("IW");
                        iw = c.a.topic.Substring(start+2, start + 4);
                    }
                }
                var reciveorg = c.a.add_dep.Split("、");
                foreach (var item in reciveorg)
                {
                    contact1.InternalContactDeptInfos.Add(new InternalContactDeptInfo { Type = 1, OrgName = item });
                }
                contact1.Theme = c.a.topic;
                contact1.AdaptiveModel = c.a.adapt_model;
                contact1.CreateTime = c.a.ciiDate;
                contact1.CreateUser = c.user_nm;
                contact1.CreateUserId = c.a.user_id.ToString();
                contact1.Content = c.a.content;
                contact1.ApproveTime = c.a.up_dt;
                contact1.IW = iw;
                var rang = "";
                var reason = "";
                if (c.a.new_import==1)
                {
                    rang = "新品导入";
                }
                if (c.a.lib_import == 1)
                {
                    rang += (!string.IsNullOrWhiteSpace(rang) ? "," : "") + "在库品导入";
                }
                if (c.a.ship_import == 1)
                {
                    rang += (!string.IsNullOrWhiteSpace(rang) ? "," : "") + "已出货设备导入";
                }
                if (c.a.customer_need == 1)
                {
                    reason = "客户需求";
                }
                if (c.a.rd_need == 1)
                {
                    reason += (!string.IsNullOrWhiteSpace(reason) ? "," : "") + "研发需求";
                }
                contact1.AdaptiveRange = rang;
                contact1.Reason = reason;
                contact1.Status = 7;
                contact1.RdmsNo = "";
                contact1.FlowInstanceId = "";

                internalContacts.Add(contact1);
            });

            await UnitWork.BatchAddAsync<InternalContact, int>(internalContacts.ToArray());
            await UnitWork.SaveAsync();
        }
    }
}
