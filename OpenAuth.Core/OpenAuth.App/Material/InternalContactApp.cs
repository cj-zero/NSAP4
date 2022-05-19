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
using OpenAuth.Repository.Domain.Workbench;
using Microsoft.AspNetCore.Http;
using System.Linq.Expressions;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.App.Sap.BusinessPartner;
using System.Text.RegularExpressions;
using OpenAuth.App.Serve.Response;
using OpenAuth.Repository.Domain.NsapBone;
using System.Threading;

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 内部联络单
    /// </summary>
    public class InternalContactApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly WorkbenchApp _workbenchApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly BusinessPartnerApp _businessPartnerApp;
        private readonly ServiceOrderApp _serviceOrderApp;

        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        public InternalContactApp(IUnitWork unitWork, IAuth auth, FlowInstanceApp flowInstanceApp, WorkbenchApp workbenchApp, ModuleFlowSchemeApp moduleFlowSchemeApp, BusinessPartnerApp businessPartnerApp, ServiceOrderApp serviceOrderApp) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _workbenchApp = workbenchApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _businessPartnerApp = businessPartnerApp;
            _serviceOrderApp = serviceOrderApp;
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
            var query = UnitWork.Find<InternalContact>(null).Include(c => c.InternalContactDeptInfos)
                        //.WhereIf(loginContext.User.Account != Define.SYSTEM_USERNAME, c => c.CreateUserId == loginContext.User.Id)
                        .WhereIf(iw.Count > 0, c => iw.Contains(c.Id))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Id), c => c.IW == req.Id)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.Theme), c => c.Theme.Contains(req.Theme))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), c => c.CardCode == req.CardCode)
                        .WhereIf(!string.IsNullOrWhiteSpace(req.CreateUser), c => c.CreateUser == req.CreateUser)
                        .WhereIf(req.Status != null, c => c.Status == req.Status);

            if (req.PageType == 1)//我的提交
            {
                query = query.WhereIf(loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(c => c.Name == "内部联络单-查看全部"), c => c.CreateUserId == loginContext.User.Id);
            }
            else if (req.PageType == 2)//待审批
            {
                var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "内部联络单");
                var flowinstace = await UnitWork.Find<FlowInstance>(c => c.SchemeId == mf.FlowSchemeId && c.MakerList.Contains(loginContext.User.Id)).Select(c => c.Id).ToListAsync();
                query = query.Where(c => flowinstace.Contains(c.FlowInstanceId) && c.Status != 9 && c.Status != 5 && c.Status != 6 && c.Status != 11);
                //c.CreateUserId != loginContext.User.Id &&
            }
            else if (req.PageType == 3)//已审批
            {
                var instances = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.CreateUserId == loginContext.User.Id).Select(c => c.InstanceId).Distinct().ToListAsync();
                query = query.Where(c => instances.Contains(c.FlowInstanceId)  && c.Status != 9);
                //&& c.CreateUserId != loginContext.User.Id
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
            else if (req.PageType == 6)
            {
                query = query.Where(c => c.Status == 4 || c.Status == 7);//执行中或已完成
            }

            var resp = await query
                                .OrderByDescending(c => c.IW)
                                .Skip((req.page - 1) * req.limit)
                                .Take(req.limit)
                                .ToListAsync();
            result.Data = resp.Select(c => new
            {
                c.Id,
                c.IW,
                c.Theme,
                ReceiveOrg = string.Join(",", c.InternalContactDeptInfos.Where(o => o.Type == 1).Select(c => c.OrgName)),
                ExecOrg = string.Join(",", c.InternalContactDeptInfos.Where(o => o.Type == 2).Select(c => c.OrgName)),
                c.CardCode,
                c.CardName,
                c.CreateUser,
                c.CreateTime,
                c.ApproveTime,
                c.ExecTime,
                c.Status,
                c.IsTentative
            }).ToList();
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
                        //.Include(c => c.InternalContactAttchments)
                        //.Include(c => c.InternalContactBatchNumbers)
                        //.Include(c => c.InternalContactDeptInfos)
                        //.Include(c => c.InternalContactMaterials)
                        //.Include(c => c.InternalContactTasks)
                        //.Include(c => c.InternalContactServiceOrders)
                        .FirstOrDefaultAsync();
                    if (single!=null)
                    {
                        single.InternalContactAttchments = await UnitWork.Find<InternalContactAttchment>(c => c.InternalContactId == req.Id).ToListAsync();
                        single.InternalContactDeptInfos = await UnitWork.Find<InternalContactDeptInfo>(c => c.InternalContactId == req.Id).ToListAsync();
                        single.InternalContactMaterials = await UnitWork.Find<InternalContactMaterial>(c => c.InternalContactId == req.Id).ToListAsync();
                        single.InternalContactTasks = await UnitWork.Find<InternalContactTask>(c => c.InternalContactId == req.Id).ToListAsync();
                        single.InternalContactServiceOrders = await UnitWork.Find<InternalContactServiceOrder>(c => c.InternalContactId == req.Id).ToListAsync();
                    }
                    var icp = await UnitWork.Find<InternalContactProduction>(c => c.InternalContactId == req.Id).ToListAsync();
                    obj.Status = 11;//未提交
                    //obj.CardCode = string.Join(",", req.CardCodes);
                    //obj.CardName = string.Join(",", req.CardNames);
                    obj.CardCode = req.CardCodes;
                    obj.CardName = req.CardNames;
                    obj.Reason = string.Join(",", req.Reasons);
                    //obj.SaleOrderNo = string.Join(",", req.SaleOrderNo);
                    //obj.MaterialOrder = string.Join(",", req.MaterialOrder);
                    //obj.AdaptiveRange = string.Join(",", req.AdaptiveRanges);

                    #region 添加子表数据
                    obj.InternalContactAttchments = new List<InternalContactAttchment>();
                    //obj.InternalContactBatchNumbers = new List<InternalContactBatchNumber>();
                    obj.InternalContactDeptInfos = new List<InternalContactDeptInfo>();
                    //附件
                    req.Attchments.ForEach(c =>
                    {
                        obj.InternalContactAttchments.Add(new InternalContactAttchment { FileId = c, InternalContactId = (single != null && single.Id > 0) ? single.Id : 0 });
                    });
                    //批次号
                    //obj.InternalContactBatchNumbers.ForEach(c =>
                    //{
                    //    c.InternalContactId = (single != null && single.Id > 0) ? single.Id : 0;
                    //});
                    //物料
                    //obj.InternalContactMaterials.ForEach(c =>
                    //{
                    //    c.InternalContactId = (single != null && single.Id > 0) ? single.Id : 0;
                    //});

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
                            Count = c.OrgName == "S19" ? obj.InternalContactServiceOrders.Count() : obj.InternalContactTasks.Where(m => m.ProductionOrg == c.OrgName).Count(),
                            InternalContactId = (single != null && single.Id > 0) ? single.Id : 0
                        });
                    });

                    obj.InternalContactMaterials = new List<InternalContactMaterial>();
                    //物料信息
                    req.MaterialInfo.ForEach(c =>
                    {
                        obj.InternalContactMaterials.Add(new InternalContactMaterial()
                        {
                            InternalContactId = (single != null && single.Id > 0) ? single.Id : 0,
                            MaterialCode = c.MaterialCode,
                            Prefix = string.Join(",", c.Prefix),
                            Series = string.Join(",", c.Series),
                            VoltsStart = c.VoltsStart,
                            VoltseEnd = c.VoltseEnd,
                            AmpsStart = c.AmpsStart,
                            AmpsEnd = c.AmpsEnd,
                            Special = c.Special != null ? string.Join(",", c.Special) : null,
                            StartExcelTime = c.StartExcelTime,
                            EndExcelTime = c.EndExcelTime,
                            FromThemeList = c.FromThemeList,
                            SelectList = c.SelectList,
                            FromTheme = c.FromTheme,
                            CurrentUnit = c.CurrentUnit,
                            Fixture = c.Fixture != null ? string.Join(",", c.Fixture) : null
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

                        #region 子表删除重新添加
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
                        //if (obj.InternalContactBatchNumbers.Count > 0)
                        //{
                        //    await UnitWork.BatchDeleteAsync<InternalContactBatchNumber>(single.InternalContactBatchNumbers.ToArray());
                        //    //await UnitWork.DeleteAsync<InternalContactBatchNumber>(c => c.InternalContactId == single.Id);
                        //    //await UnitWork.SaveAsync();
                        //    await UnitWork.BatchAddAsync<InternalContactBatchNumber>(obj.InternalContactBatchNumbers.ToArray());
                        //}
                        if (obj.InternalContactMaterials.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<InternalContactMaterial>(single.InternalContactMaterials.ToArray());
                            await UnitWork.BatchAddAsync<InternalContactMaterial>(obj.InternalContactMaterials.ToArray());
                        }
                        if (obj.InternalContactTasks.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<InternalContactTask>(single.InternalContactTasks.ToArray());
                            await UnitWork.BatchAddAsync<InternalContactTask>(obj.InternalContactTasks.ToArray());
                        }
                        if (obj.InternalContactServiceOrders.Count > 0)
                        {
                            await UnitWork.BatchDeleteAsync<InternalContactServiceOrder>(single.InternalContactServiceOrders.ToArray());
                            await UnitWork.BatchAddAsync<InternalContactServiceOrder>(obj.InternalContactServiceOrders.ToArray());
                        }
                        if (obj.InternalContactProductions.Count > 0)
                        {
                            obj.InternalContactProductions.ForEach(c =>
                            {
                                c.InternalContactId = null;
                                c.InternalContactProductionDetails.ForEach(d =>
                                {
                                    d.InternalContactProductionId = null;
                                });
                            });
                            await UnitWork.BatchDeleteAsync<InternalContactProduction>(icp.ToArray());
                            await UnitWork.BatchAddAsync<InternalContactProduction>(obj.InternalContactProductions.ToArray());
                        }
                        obj.InternalContactAttchments = null;
                        obj.InternalContactBatchNumbers = null;
                        obj.InternalContactDeptInfos = null;

                        #endregion

                        await UnitWork.UpdateAsync(obj);
                        //设置测试环节执行人
                        //await _flowInstanceApp.ModifyNodeUser(single.FlowInstanceId, true, new string[] { obj.CheckApproveId }, obj.CheckApprove, false);
                        //await UnitWork.SaveAsync();
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
                        obj.IsTentative = false;

                        obj = await UnitWork.AddAsync<InternalContact, int>(obj);

                    }

                    if (!req.IsDraft)
                    {
                        if (single != null && !string.IsNullOrWhiteSpace(single.FlowInstanceId))
                        {
                            VerificationReq verificationReq = new VerificationReq
                            {
                                NodeRejectStep = "",
                                NodeRejectType = "0",
                                FlowInstanceId = single.FlowInstanceId,
                                VerificationFinally = "1",
                                VerificationOpinion = ""
                            };
                            await _flowInstanceApp.Verification(verificationReq);
                            if (single.Status == 10)
                            {
                                obj.Status = 1;//测试审批
                                //设置测试环节执行人
                                await _flowInstanceApp.ModifyNodeUser(single.FlowInstanceId, true, new string[] { obj.CheckApproveId }, obj.CheckApprove, false);
                            }
                            else
                            {
                                obj.Status = 10;//刘静审核
                            }

                            await UnitWork.UpdateAsync<InternalContact>(r => r.Id == obj.Id, r => new InternalContact { Status = obj.Status });
                        }
                        else
                        {
                            //创建流程
                            var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "内部联络单");
                            var flow = new AddFlowInstanceReq();
                            flow.SchemeId = mf.FlowSchemeId;
                            flow.FrmType = 2;
                            flow.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            flow.CustomName = $"内部联络单";
                            flow.FrmData = "";
                            obj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(flow);
                            obj.Status = 10;//刘静审核
                            await UnitWork.UpdateAsync<InternalContact>(r => r.Id == obj.Id, r => new InternalContact { FlowInstanceId = obj.FlowInstanceId, Status = obj.Status });

                            //设置测试环节执行人
                            //await _flowInstanceApp.ModifyNodeUser(obj.FlowInstanceId, true, new string[] { obj.CheckApproveId }, obj.CheckApprove, false);

                            await _workbenchApp.AddOrUpdate(new WorkbenchPending
                            {
                                OrderType = 5,
                                TerminalCustomer = obj.CardName,
                                TerminalCustomerId = obj.CardCode,
                                ServiceOrderId = 0,
                                ServiceOrderSapId = 0,
                                UpdateTime = obj.CreateTime,
                                Remark = "",
                                FlowInstanceId = obj.FlowInstanceId,
                                TotalMoney = 0,
                                Petitioner = obj.CreateUser,
                                SourceNumbers = Convert.ToInt32(obj.IW),
                                PetitionerId = obj.CreateUserId
                            });
                        }
                    }

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("添加内部联络单失败。" + ex.Message);
                }
            }
        }

        public async Task SendEmail(int id, string title = "")
        {
            var obj = await UnitWork.Find<InternalContact>(c => c.Id == id)
                            .Include(c => c.InternalContactDeptInfos)
                            .Include(c => c.InternalContactAttchments)
                            .Include(c => c.InternalContactBatchNumbers)
                            .FirstOrDefaultAsync();

            var orgIds = obj.InternalContactDeptInfos.Select(c => c.OrgId).ToList();
            var history = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.InstanceId == obj.FlowInstanceId).Select(c => c.CreateUserId).Distinct().ToListAsync();
            var userIds = await UnitWork.Find<Relevance>(c => orgIds.Contains(c.SecondId) && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
            //userIds.Add(obj.CheckApproveId);
            //userIds.Add(obj.DevelopApproveId);
            userIds.AddRange(history);
            userIds = userIds.Distinct().ToList();
            var userInfo = await UnitWork.Find<User>(c => userIds.Contains(c.Id) && c.Status == 0).ToListAsync();
            //增加样式
            var style = "<style>table{border-bottom:1px solid #ccc} td{border:1px solid #ccc;border-bottom:none}</style>";
            obj.Content = $"{style}{obj.Content}";
            userInfo.ForEach(async c =>
            {
                if (!string.IsNullOrWhiteSpace(c.Email))
                {
                    var mailuser = new List<MailUser>();
                    mailuser.Add(new MailUser { Name = c.Account, Address = c.Email });
                    await SebdEmail(obj, title, mailuser);
                }
            });
        }

        private async Task SebdEmail(InternalContact obj, string title, List<MailUser> mailUsers = null)
        {
            InternalContactEmailLog contactEmailLog = new InternalContactEmailLog();
            contactEmailLog.Receiver = mailUsers.FirstOrDefault().Name;
            contactEmailLog.InternalContactId = obj.Id;
            contactEmailLog.CreateTime = DateTime.Now;
            contactEmailLog.Result = "发送成功";
            contactEmailLog.Remark = title;
            try
            {
                MailRequest mailRequest = new MailRequest();
                mailRequest.Subject = obj.Theme;
                mailRequest.Priority = 1;
                mailRequest.FromUser = new MailUser { Name = "ERP4.0通知", Address = Define.MailAccount, Password = Define.MailPassword };
                mailRequest.ToUsers = mailUsers;
                //var orgIds = obj.InternalContactDeptInfos.Select(c => c.OrgId).ToList();
                //var userIds = await UnitWork.Find<Relevance>(c => orgIds.Contains(c.SecondId) && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                //userIds.Add(obj.CheckApproveId);
                //userIds.Add(obj.DevelopApproveId);
                //var userInfo = await UnitWork.Find<User>(c => userIds.Contains(c.Id) && c.Status == 0).ToListAsync();
                ////mailRequest.ToUsers = new List<MailUser> { new MailUser { Name = "licong", Address = "licong@neware.com.cn" } };
                //mailRequest.ToUsers = new List<MailUser>();
                //userInfo.ForEach(c =>
                //{
                //    if (!string.IsNullOrWhiteSpace(c.Email))
                //    {
                //        mailRequest.ToUsers.Add(new MailUser { Name = c.Account, Address = c.Email });
                //    }
                //});
                //附件
                mailRequest.Attachments = new List<MailAttachment>();
                if (obj.InternalContactAttchments.Count > 0)
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
            catch (Exception e)
            {
                contactEmailLog.Result = $"发送失败，{e.Message}";
            }
            await semaphoreSlim.WaitAsync();
            await UnitWork.AddAsync(contactEmailLog);
            await UnitWork.SaveAsync();
            semaphoreSlim.Release();
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
                            //.Include(c => c.InternalContactAttchments)
                            //.Include(c => c.InternalContactBatchNumbers)
                            .Include(c => c.InternalContactDeptInfos)
                            .Include(c => c.InternalContactMaterials)
                            //.Include(c => c.InternalContactTasks)
                            //.Include(c => c.InternalContactServiceOrders)
                            .FirstOrDefaultAsync();
            detail.InternalContactTasks=await UnitWork.Find<InternalContactTask>(c => c.InternalContactId == detail.Id).ToListAsync();
            detail.InternalContactServiceOrders = await UnitWork.Find<InternalContactServiceOrder>(c => c.InternalContactId == detail.Id).ToListAsync();
            detail.InternalContactBatchNumbers=await UnitWork.Find<InternalContactBatchNumber>(c => c.InternalContactId == detail.Id).ToListAsync();
            var internalContactProductions = await UnitWork.Find<InternalContactProduction>(c => c.InternalContactId == detail.Id).Include(c => c.InternalContactProductionDetails).ToListAsync();
            //操作历史
            var operationHistories = await UnitWork.Find<FlowInstanceOperationHistory>(c => c.InstanceId == detail.FlowInstanceId)
                .OrderBy(c => c.CreateDate).Select(h => new
                {
                    CreateTime = Convert.ToDateTime(h.CreateDate).ToString("yyyy.MM.dd HH:mm:ss"),
                    h.Remark,
                    IntervalTime = h.IntervalTime != null && h.IntervalTime > 0 ? h.IntervalTime / 60 : null,
                    h.CreateUserName,
                    h.Content,
                    h.ApprovalResult,
                }).ToListAsync();
            //var fileId = detail.InternalContactAttchments.Select(c => c.FileId).ToList();
            //var files = await UnitWork.Find<UploadFile>(c => fileId.Contains(c.Id)).ToListAsync();
            //var returnFile = files.MapTo<List<UploadFileResp>>();
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
                ExecTime = c.HandleTime,
                Count = c.Count
            });
            result.Data = new
            {
                detail.Types,
                detail.Id,
                detail.IW,
                detail.Theme,
                detail.IsTentative,
                //Files = returnFile,
                //CardCodes = !string.IsNullOrWhiteSpace(detail.CardCode) ? detail.CardCode.Split(",") : new string[] { },
                //CardNames = !string.IsNullOrWhiteSpace(detail.CardCode) ? detail.CardName.Split(",") : new string[] { },
                CardCodes = detail.CardCode,
                CardNames = detail.CardCode,
                detail.Status,
                detail.RdmsNo,
                //SaleOrderNo = !string.IsNullOrWhiteSpace(detail.SaleOrderNo) ? detail.SaleOrderNo.Split(",") : new string[] { },
                SaleOrderNo = detail.SaleOrderNo,
                detail.AdaptiveModel,
                detail.ProductionNo,
                AdaptiveRanges = detail.AdaptiveRange.Split(","),
                Reasons = detail.Reason.Split(","),
                MaterialOrder = !string.IsNullOrWhiteSpace(detail.MaterialOrder) ? detail.MaterialOrder.Split(",") : new string[] { },
                //BatchNumbers = detail.InternalContactBatchNumbers,
                BatchNumbers = detail.InternalContactBatchNumbers,
                detail.CheckApproveId,
                detail.CheckApprove,
                detail.DevelopApproveId,
                detail.DevelopApprove,
                InternalContactReceiveDepts = detail.InternalContactDeptInfos.Where(o => o.Type == 1).Select(c => new { c.OrgId, c.OrgName, c.UserId, c.UserName }).ToList(),
                InternalContactExecDepts = detail.InternalContactDeptInfos.Where(o => o.Type == 2).Select(c => new { c.OrgId, c.OrgName, c.UserId, c.UserName }).ToList(),
                detail.Content,
                reviceOrgList,
                execOrgList,
                InternalContactMaterials = detail.InternalContactMaterials,
                operationHistories,
                detail.InternalContactTasks,
                detail.InternalContactServiceOrders,
                InternalContactProductions = internalContactProductions,
                MaterialInfo = detail.InternalContactMaterials.Select(c => new QueryProductionOrderReq
                {
                    VoltsStart = c.VoltsStart,
                    VoltseEnd = c.VoltseEnd,
                    AmpsStart = c.AmpsStart,
                    AmpsEnd = c.AmpsEnd,
                    StartExcelTime = c?.StartExcelTime,
                    EndExcelTime = c?.EndExcelTime,
                    Prefix = !string.IsNullOrWhiteSpace(c.Prefix) ? c.Prefix.Split(",").ToList() : new List<string>(),
                    Series = !string.IsNullOrWhiteSpace(c.Series) ? c.Series.Split(",").ToList() : new List<string>(),
                    Special = !string.IsNullOrWhiteSpace(c.Special) ? c.Special.Split(",").ToList() : new List<string>(),
                    MaterialCode = c.MaterialCode,
                    FromThemeList = c.FromThemeList,
                    SelectList = c.SelectList,
                    FromTheme = c.FromTheme,
                    CurrentUnit = c.CurrentUnit,
                    Fixture = !string.IsNullOrWhiteSpace(c.Fixture) ? c.Fixture.Split(",").ToList() : new List<string>(),
                })
            };
            return result;
        }

        /// <summary>
        /// 查看工单
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <param name="org"></param>
        /// <returns></returns>
        public async Task<TableData> WorkOrder(int id, int type, string org)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            //var detail = await UnitWork.Find<InternalContact>(c => c.Id == id).FirstOrDefaultAsync();
            var InternalContactTasks = await UnitWork.Find<InternalContactTask>(c => c.InternalContactId == id).OrderBy(c => c.ItemCode).ToListAsync();
            var InternalContactServiceOrders = await UnitWork.Find<InternalContactServiceOrder>(c => c.InternalContactId == id).OrderBy(c => c.CardCode).ToListAsync();
            if (type == 1)
            {
                InternalContactTasks.ForEach(c =>
                {
                    if (string.IsNullOrWhiteSpace(c.ProductionId.ToString()))
                    {
                        c.ProductionOrg = null;
                    }
                });
                result.Data = new
                {
                    InternalContactTasks,
                    InternalContactServiceOrders
                };
            }
            else if (type == 2)
            {
                InternalContactTasks = InternalContactTasks.Where(c => c.ProductionOrg == org).OrderBy(c => c.ItemCode).ToList();
                InternalContactTasks.ForEach(c =>
                {
                    if (string.IsNullOrWhiteSpace(c.ProductionId.ToString()))
                    {
                        c.ProductionOrg = null;
                    }
                });
                result.Data = InternalContactTasks;
            }
            else
            {
                result.Data = InternalContactServiceOrders;
            }
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

            var internalContact = await UnitWork.Find<InternalContact>(c => c.Id == req.Id)
                                        .Include(c => c.InternalContactDeptInfos)
                                        .Include(c => c.InternalContactAttchments)
                                        .Include(c => c.InternalContactBatchNumbers)
                                        .FirstOrDefaultAsync();
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
                    internalContact.IsTentative = false;
                    await _flowInstanceApp.Verification(verificationReq);
                }
                else
                {
                    if (req.IsTentative)
                    {
                        //添加操作记录
                        FlowInstanceOperationHistory flowInstanceOperationHistory = new FlowInstanceOperationHistory
                        {
                            InstanceId = internalContact.FlowInstanceId,
                            CreateUserId = loginContext.User.Id,
                            CreateUserName = loginContext.User.Name,
                            CreateDate = DateTime.Now,
                            Content = flowinstace.ActivityName,
                            Remark = req.Remark,
                            ApprovalResult = "待定",
                            //ActivityId = flowInstance.ActivityId,
                        };
                        var fioh = await UnitWork.Find<FlowInstanceOperationHistory>(r => r.InstanceId.Equals(internalContact.FlowInstanceId)).OrderByDescending(r => r.CreateDate).FirstOrDefaultAsync();
                        if (fioh != null)
                        {
                            flowInstanceOperationHistory.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(fioh.CreateDate)).TotalSeconds);
                        }

                        await UnitWork.AddAsync(flowInstanceOperationHistory);
                        await UnitWork.UpdateAsync<InternalContact>(c => c.Id == req.Id, c => new InternalContact
                        {
                            IsTentative = true
                        }); ;
                    }
                    else
                    {
                        //if (flowinstace.ActivityName == "刘静审核")
                        //{
                        //    await _flowInstanceApp.Verification(verificationReq);
                        //    internalContact.Status = 1;
                        //    //设置研发环节执行人
                        //    await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, true, new string[] { internalContact.CheckApproveId }, internalContact.CheckApprove, false);
                        //}
                        //else 
                        if (loginContext.Roles.Any(c => c.Name.Equal("联络单测试审批")) && flowinstace.ActivityName == "测试审批")
                        {
                            await _flowInstanceApp.Verification(verificationReq);
                            internalContact.Status = 2;
                            //设置研发环节执行人
                            await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, true, new string[] { internalContact.DevelopApproveId }, internalContact.DevelopApprove, false);
                        }
                        else if (loginContext.Roles.Any(c => c.Name.Equal("联络单研发审批")) && flowinstace.ActivityName == "研发审批")
                        {
                            await _flowInstanceApp.Verification(verificationReq);
                            internalContact.Status = 3;
                        }
                        else if (loginContext.Roles.Any(c => c.Name.Equal("总经理")) && flowinstace.ActivityName == "总经理审批")
                        {
                            await _flowInstanceApp.Verification(verificationReq);
                            internalContact.Status = 4;

                            #region 发送邮件
                            //await SebdEmail(internalContact, "");
                            #endregion
                        }
                        else if (flowinstace.ActivityName.Contains("研发提交") && (internalContact.Status == 5 || internalContact.Status == 6))
                        {
                            await _flowInstanceApp.Verification(verificationReq);
                            internalContact.Status = 1;//驳回 撤回提交
                        }
                        internalContact.IsTentative = false;

                    }

                }

                await UnitWork.UpdateAsync<InternalContact>(c => c.Id == req.Id, c => new InternalContact
                {
                    Status = internalContact.Status,
                    IsTentative = internalContact.IsTentative,
                    ApproveTime = internalContact.Status == 4 ? DateTime.Now : internalContact.ApproveTime
                });

                //修改全局待处理
                await UnitWork.UpdateAsync<WorkbenchPending>(w => w.SourceNumbers == internalContact.IW.ToInt32() && w.OrderType == 5, w => new WorkbenchPending
                {
                    UpdateTime = DateTime.Now,
                });
            }
            else if (req.HanleType == 2)//执行
            {
                var loginOrg = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault();
                if (req.ExecType == 1)//接收
                {
                    await UnitWork.UpdateAsync<InternalContactDeptInfo>(c => c.InternalContactId == req.Id && c.UserId == loginContext.User.Id && c.Type == 1, c => new InternalContactDeptInfo
                    {
                        HandleTime = DateTime.Now
                    });
                }
                else if (req.ExecType == 2)//执行
                {
                    await UnitWork.UpdateAsync<InternalContactDeptInfo>(c => c.InternalContactId == req.Id && c.UserId == loginContext.User.Id && c.Type == 2, c => new InternalContactDeptInfo
                    {
                        HandleTime = DateTime.Now,
                        Content = req.Remark
                    });
                }
                //await UnitWork.SaveAsync();

                var dept = await UnitWork.Find<InternalContactDeptInfo>(c => c.InternalContactId == req.Id).AllAsync(c => c.HandleTime != null);
                if (dept)
                {
                    //全部执行完后
                    await UnitWork.UpdateAsync<InternalContact>(c => c.Id == req.Id, c => new InternalContact
                    {
                        Status = 7,
                        ExecTime = DateTime.Now
                    });
                }
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 生成服务单、任务单
        /// </summary>
        /// <returns></returns>
        public async Task GenerateWorkOrder(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var taskOrder = await UnitWork.Find<InternalContactTask>(c => c.InternalContactId == id).ToListAsync();
            var serviceOrder = await UnitWork.Find<InternalContactServiceOrder>(c => c.InternalContactId == id).ToListAsync();
            List<ServiceOrder> serviceOrdersList = new List<ServiceOrder>();
            var customerInfo = serviceOrder.GroupBy(c => c.CardCode).Select(c => new { CardCode = c.Key, List = c.ToList() }).ToList();
            //服务单
            foreach (var item in customerInfo)
            {
                string addr = "", area = "", city = "", province = "", longitude = "", latitude = "";
                var customer = await GetCardInfo(item.CardCode);
                var cnt = await UnitWork.Find<OCPR>(c => c.CardCode == item.CardCode).Select(c => new { c.Name, c.Tel1 }).FirstOrDefaultAsync();
                //var customer = customerRes.Data[0];
                if (!string.IsNullOrWhiteSpace(customer.Address2))
                {
                    var locationResult = AmapUtil.GetLocation(customer.Address2);
                    if (locationResult != null)
                    {
                        longitude = locationResult["result"]["location"]["lng"].ToString();
                        latitude = locationResult["result"]["location"]["lat"].ToString();
                        province = locationResult["result"]["addressComponent"]["province"].ToString();
                        city = locationResult["result"]["addressComponent"]["city"].ToString();
                        area = locationResult["result"]["addressComponent"]["district"].ToString();
                        addr = $"{locationResult["result"]["addressComponent"]["street"].ToString()}{locationResult["result"]["addressComponent"]["street_number"].ToString()}";
                    }
                }

                CustomerServiceAgentCreateOrderReq s = new CustomerServiceAgentCreateOrderReq()
                {
                    Addr = addr,
                    Address = customer.Address2,
                    AddressDesignator = "运达到",
                    Area = area,
                    City = city,
                    Province = province,
                    Longitude = !string.IsNullOrWhiteSpace(longitude) ? decimal.Parse(longitude) : 0,
                    Latitude = !string.IsNullOrWhiteSpace(latitude) ? decimal.Parse(latitude) : 0,
                    FromId = 8,
                    CustomerId = item.List.First().CardCode,
                    CustomerName = item.List.First().CardName,
                    TerminalCustomerId = item.List.First().CardCode,
                    TerminalCustomer = item.List.First().CardName,
                    IsSend = false,
                    Contacter = "",
                    NewestContacter = cnt.Name,
                    NewestContactTel = cnt.Tel1,
                    SalesMan = customer.SlpName,
                    Supervisor = item.List.First().Supervisor,
                    ServiceWorkOrders = item.List.Select(s => new AddServiceWorkOrderReq
                    {
                        Status = 1,
                        FeeType = 2,
                        FromTheme = s.FromTheme,
                        FromType = 1,
                        ManufacturerSerialNumber = s.MnfSerial,
                        MaterialCode = s.ItemCode,
                        MaterialDescription = s.ItemName,
                        Priority = 1,
                        Remark = ""
                    }).ToList()
                    //ServiceWorkOrders = new List<AddServiceWorkOrderReq>() {
                    //    new AddServiceWorkOrderReq{
                    //        Status=1,
                    //        FeeType=2,
                    //        FromTheme=child.FromTheme,
                    //        FromType=1,
                    //        ManufacturerSerialNumber=child.MnfSerial,
                    //        MaterialCode=child.ItemCode,
                    //        MaterialDescription=child.ItemName,
                    //        Priority=1,
                    //        Remark=""
                    //    }
                    //    }
                };
                var createResult = await _serviceOrderApp.CustomerServiceAgentCreateOrder(s);
                var serviceOrderId = createResult.Result;
                await UnitWork.UpdateAsync<InternalContactServiceOrder>(c => c.InternalContactId == id && c.CardCode == item.CardCode, c => new InternalContactServiceOrder
                {
                    ServiceOrderId = serviceOrderId
                });
                await UnitWork.SaveAsync();
                //foreach (var child in item.List)
                //{
                //}
            }
            ////维修单
            foreach (var item in taskOrder)
            {
                var supervisor = "";
                //生产部门派给对应主管，其他部门派给E3樊静涛
                if (item.ProductionOrg.Contains("P"))
                    supervisor = item.ProductionOrgManager;
                else
                    supervisor = "樊静涛";

                #region MyRegion
                //如有关联关系先删除
                var count = await UnitWork.Find<InternalContactTaskServiceOrder>(c => c.InternalContactTaskId == item.Id).CountAsync();
                if (count > 0)
                    await UnitWork.DeleteAsync<InternalContactTaskServiceOrder>(c => c.InternalContactTaskId == item.Id);
                List<InternalContactTaskServiceOrder> addlist = new List<InternalContactTaskServiceOrder>();
                //归属量多少生成多少维修单
                for (int i = 0; i < item.BelongQty; i++)
                {
                    var remark = "";
                    if (item.ProductionId != null)
                        remark = $"基于生产订单WO-{item.ProductionId}";
                    //else
                    //    remark = $"基于BOM，{item.ItemCode}";

                    CustomerServiceAgentCreateOrderReq s = new CustomerServiceAgentCreateOrderReq
                    {
                        Contacter = "",
                        CustomerId = "C37852",
                        CustomerName = "东莞新威检测技术有限公司",
                        FromId = 8,
                        TerminalCustomer = "东莞新威检测技术有限公司",
                        TerminalCustomerId = "C37852",
                        Supervisor = supervisor,
                        ServiceWorkOrders = new List<AddServiceWorkOrderReq>()
                        {
                            new AddServiceWorkOrderReq
                            {
                                FromTheme=item.FromTheme,
                                FromType=1,
                                Priority=1,
                                RepairMaterialCode=item.ItemCode,
                                ManufacturerSerialNumber="",
                                MaterialCode="",
                                Remark=remark
                            }
                        }
                    };
                    var createResult = await _serviceOrderApp.CISECreateServiceOrder(s);
                    var serviceOrderId = createResult.Result;
                    addlist.Add(new InternalContactTaskServiceOrder { InternalContactTaskId = item.Id, ServiceOrderId = serviceOrderId, IsFinish = false, InternalContactId = item.InternalContactId });
                }
                await UnitWork.BatchAddAsync(addlist.ToArray());

                #endregion

                //var remark = "";
                //if (item.ProductionId != null)
                //    remark = $"基于生产订单WO-{item.ProductionId}";
                //else
                //    remark = $"基于BOM，{item.ItemCode}";
                ////remark = $"基于BOM，{item.ItemCode}";

                //CustomerServiceAgentCreateOrderReq s = new CustomerServiceAgentCreateOrderReq
                //{
                //    Contacter = "",
                //    CustomerId = "C37852",
                //    CustomerName = "东莞新威检测技术有限公司",
                //    FromId = 8,
                //    TerminalCustomer = "东莞新威检测技术有限公司",
                //    TerminalCustomerId = "C37852",
                //    Supervisor = supervisor,
                //    ServiceWorkOrders = new List<AddServiceWorkOrderReq>()
                //        {
                //            new AddServiceWorkOrderReq
                //            {
                //                FromTheme=item.FromTheme,
                //                FromType=1,
                //                Priority=1,
                //                RepairMaterialCode=item.ItemCode,
                //                ManufacturerSerialNumber="",
                //                MaterialCode="",
                //                Remark=remark
                //            }
                //        }
                //};
                //var createResult = await _serviceOrderApp.CISECreateServiceOrder(s);
                //var serviceOrderId = createResult.Result;
                //await UnitWork.UpdateAsync<InternalContactTask>(c => c.Id == item.Id, c => new InternalContactTask
                //{
                //    ServiceOrderId = serviceOrderId
                //});

                await UnitWork.SaveAsync();
            }

            //等待CAP将服务单创建完，取到SAPID再反写
            await Task.Delay(5000);
            serviceOrder = await UnitWork.Find<InternalContactServiceOrder>(c => c.InternalContactId == id).ToListAsync();
            var serviceId = serviceOrder.Select(c => c.ServiceOrderId).ToList();
            var serviceOrderObj = await UnitWork.Find<ServiceOrder>(c => serviceId.Contains(c.Id)).Select(c => new { c.Id, c.U_SAP_ID }).ToListAsync();
            if (serviceOrder.Count > 0)
            {
                foreach (var item in serviceOrder)
                {
                    var sapId = serviceOrderObj.Where(c => c.Id == item.ServiceOrderId).FirstOrDefault()?.U_SAP_ID;
                    if (!string.IsNullOrWhiteSpace(sapId.ToString()))
                        item.ServiceOrderSapId = sapId;
                }
                await UnitWork.BatchUpdateAsync(serviceOrder.ToArray());
                await UnitWork.SaveAsync();
            }
        }

        /// <summary>
        /// 获取内联单内容
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="vestInOrg"></param>
        /// <returns></returns>
        public async Task<string> GetInternalContactContent(int serviceOrderId, int vestInOrg)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            string result = "";
            if (vestInOrg == 1)
            {
                result = await (from a in UnitWork.Find<InternalContact>(null)
                                join b in UnitWork.Find<InternalContactServiceOrder>(null) on a.Id equals b.InternalContactId
                                where b.ServiceOrderId == serviceOrderId
                                select a.Content).FirstOrDefaultAsync();
            }
            else
            {
                result = await (from a in UnitWork.Find<InternalContact>(null)
                                join b in UnitWork.Find<InternalContactTask>(null) on a.Id equals b.InternalContactId
                                //join c in UnitWork.Find<InternalContactTaskServiceOrder>(null) on b.Id equals c.InternalContactTaskId
                                where b.ServiceOrderId == serviceOrderId
                                select a.Content).FirstOrDefaultAsync();
            }
            return result;
        }

        public async Task<dynamic> GetCardInfo(string cardcode)
        {
            var query = await (from a in UnitWork.Find<OCRD>(null).WhereIf(!string.IsNullOrWhiteSpace(cardcode), q => q.CardCode.Contains(cardcode) || q.CardName.Contains(cardcode))
                               join b in UnitWork.Find<OSLP>(null) on a.SlpCode equals b.SlpCode into ab
                               from b in ab.DefaultIfEmpty()
                               join f in UnitWork.Find<OCRY>(null) on a.Country equals f.Code into af
                               from f in af.DefaultIfEmpty()
                               join g in UnitWork.Find<OCST>(null) on a.State1 equals g.Code into ag
                               from g in ag.DefaultIfEmpty()
                               select new
                               {
                                   //a.CardCode,
                                   //a.CardName,
                                   //a.CntctPrsn,
                                   //a.Phone1,
                                   b.SlpName,
                                   Address2 = $"{ f.Name ?? "" }{ g.Name ?? "" }{ a.MailCity ?? "" }{ a.MailBuildi ?? "" }"
                               }).FirstOrDefaultAsync();
            return query;

        }

        /// <summary>
        /// 获取客户信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> CustomerInfo(QueryCustomerOrder req)
        {
            TableData result = new TableData();
            var query = UnitWork.Find<OCRD>(null)
                .WhereIf(req.CardCode != null, c => req.CardCode.Contains(c.CardCode) || req.CardCode.Contains(c.CardName))
                .WhereIf(!string.IsNullOrWhiteSpace(req.CustomerId), c => c.CardCode == req.CustomerId)
                .Select(c => new { c.CardCode, c.CardName });
            result.Count = await query.CountAsync();
            result.Data = await query
                        .OrderBy(c => c.CardCode)
                        .Skip((req.page - 1) * req.limit)
                        .Take(req.limit)
                        .ToListAsync();
            return result;
        }

        /// <summary>
        /// 获取销售订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SaleOrderInfo(QueryCustomerOrder req)
        {
            TableData result = new TableData();
            var query = UnitWork.Find<ORDR>(null)
                        .WhereIf(req.CardCode != null, c => req.CardCode.Contains(c.CardCode))
                        .WhereIf(!string.IsNullOrWhiteSpace(req.SaleCode.ToString()), c => c.DocEntry == req.SaleCode)
                        .Select(c => new
                        {
                            c.DocEntry,
                            c.CardCode,
                            c.CardName
                        });
            result.Count = await query.CountAsync();
            result.Data = await query
                        .OrderByDescending(c => c.DocEntry)
                        .Skip((req.page - 1) * req.limit)
                        .Take(req.limit)
                        .ToListAsync();
            return result;
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
                var single = internalContact.Where(c => c.Status <= 6 || c.Status == 10).FirstOrDefault();
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
                //await SebdEmail(single, "<span style=\"font - size:larger\"><b>暂时停用以下联络单，请勿执行，等待通知</b></span><br><br>");
                await SendEmail(single.Id, " < span style =\"font - size:larger\"><b>暂时停用以下联络单，请勿执行，等待通知</b></span><br><br>");
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
                //await SebdEmail(single, "<span style=\"font - size:larger\"><b>重新启用以下联络单，请未完成的同事继续执行</b></span><br><br>");
                await SendEmail(single.Id, " <span style=\"font - size:larger\"><b>重新启用以下联络单，请未完成的同事继续执行</b></span><br><br>");
            }
            await UnitWork.SaveAsync();

        }

        ///// <summary>
        ///// 获取生产订单
        ///// </summary>
        ///// <returns></returns>
        //public async Task<TableData> GetProductionOrder(QueryProductionOrderReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();  
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    TableData result = new TableData();
        //    var query = ProductionOrderInfo(req);

        //    var queryData=await query.OrderByDescending(c => c.DocEntry)
        //                            .Skip((req.page - 1) * req.limit).Take(req.limit)
        //                            .ToListAsync();
        //    result.Count = await query.CountAsync();
        //    result.Data = queryData.Select(c =>
        //    {
        //        var orginfo = c.U_WO_LTDW.Split("-");
        //        return new
        //        {
        //            c.DocEntry,
        //            c.ItemCode,
        //            c.PartItemCode,
        //            c.PartPlannedQty,
        //            ProductionOrg = orginfo[0],
        //            ProductionOrgManager = orginfo.Count() > 1 ? orginfo[1] : "",
        //            WareHouse = c.wareHouse
        //        };
        //    });
        //    return result;
        //}

        ///// <summary>
        ///// 获取生产订单
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //private IQueryable<product_owor_wor1> ProductionOrderInfo(QueryProductionOrderReq req)
        //{
        //    string filter = "and (";
        //    string filterString = "";
        //    if (req.MaterialInfos.Count > 0)
        //    {
        //        if (req.QueryType == 1)
        //        {
        //            var itemcode = string.Join("','", req.MaterialInfos.Select(c => c.MaterialCode).ToList());
        //            filterString = $" b.ItemCode in ('{itemcode}') ";
        //        }
        //        else
        //        {
        //            req.MaterialInfos.ForEach(c =>
        //            {
        //                var wh = string.Join("','", c.WareHouse == null ? new string[] { } : c.WareHouse);
        //                if (!string.IsNullOrWhiteSpace(filterString))
        //                    filterString += " or ";
        //                filterString += $" (b.ItemCode='{c.MaterialCode}'  ";
        //                if (!string.IsNullOrWhiteSpace(wh))
        //                    filterString += $" and b.Warehouse in ('{wh}')";
        //                filterString += ")";
        //            });
        //        }
        //        filter += filterString + ")";
        //    }
        //    filter = !string.IsNullOrWhiteSpace(filterString) ? filter : "";
        //    //两年内的生产单
        //    var sql = @$"SELECT a.DocEntry,a.ItemCode,b.ItemCode as PartItemCode,a.PlannedQty,b.PlannedQty as PartPlannedQty,a.CmpltQty,a.U_WO_LTDW,b.wareHouse FROM product_owor a
        //                INNER JOIN product_wor1 b on a.DocEntry=b.DocEntry 
        //                where a.CreateDate >= DATE_SUB(date_format(now(),'%y-%m-%d 00:00:00'), INTERVAL 2 YEAR) {filter} ORDER BY DocEntry desc";
        //    var query = UnitWork.Query<product_owor_wor1>(sql).Select(c => new product_owor_wor1
        //                            {
        //                                DocEntry = c.DocEntry,
        //                                ItemCode = c.ItemCode,
        //                                PartItemCode = c.PartItemCode,
        //                                PlannedQty = c.PlannedQty,
        //                                PartPlannedQty = c.PartPlannedQty,
        //                                CmpltQty = c.CmpltQty, 
        //                                U_WO_LTDW = c.U_WO_LTDW,
        //                                wareHouse = c.wareHouse
        //                            })
        //                            .WhereIf(!string.IsNullOrWhiteSpace(req.ProductionOrg), c => c.U_WO_LTDW.Contains(req.ProductionOrg))
        //                            .WhereIf(!string.IsNullOrWhiteSpace(req.ProductionOrgManager), c => EF.Functions.Like(c.U_WO_LTDW, $"%{req.ProductionOrgManager}%"))
        //                            .WhereIf(req.ProductionId != null && req.ProductionId.Count > 0, c => req.ProductionId.Contains(c.DocEntry))
        //                            .WhereIf(!string.IsNullOrWhiteSpace(req.DocEntry.ToString()), c => c.DocEntry == req.DocEntry);
        //    return query;
        //}

        ///// <summary>
        ///// 获取未出货物料，归属部门
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetUndeliveredMaterial(QueryProductionOrderReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    TableData result = new TableData();
        //    //除C开头外的成品物料
        //    var finishedMaterialObj = await UnitWork.Find<FinishedMaterial>(null).Select(c => c.ItemCode).ToListAsync();
        //    List<SemiFinishedMaterial> FinishedMaterial = new List<SemiFinishedMaterial>();
        //    List<SemiFinishedMaterial> SemiFinishedMaterial = new List<SemiFinishedMaterial>();
            //var orderInfo = await ProductionOrderInfo(req).ToListAsync();
            //var auto = orderInfo.MapToList<SemiFinishedMaterial>();
        //    //完成数量为0即都未生产收货
        //    SemiFinishedMaterial.AddRange(auto.Where(c => c.CmpltQty == 0).ToList());

        //    //生产成品物料的生产单，有收货的成品
        //    FinishedMaterial.AddRange(auto.Where(c => (c.ItemCode.StartsWith("C") || finishedMaterialObj.Contains(c.ItemCode)) && c.CmpltQty != 0).ToList());
        //    //生产半成品的生产单，找到使用改半成品的成产单，有收货的半成品
        //    var noFinishedMaterial = auto.Where(c => !c.ItemCode.StartsWith("C") && !finishedMaterialObj.Contains(c.ItemCode) && c.CmpltQty != 0).ToList();
        //    if (noFinishedMaterial.Count > 0)
        //    {
        //        //半成品再去找使用生产成品的生产单
        //        var materialInfoReq = noFinishedMaterial.Select(c => new MaterialInfo { MaterialCode = c.ItemCode }).ToList();
        //        var newMaterial = await ProductionOrderInfo(new QueryProductionOrderReq { MaterialInfos = materialInfoReq, QueryType = 1 }).ToListAsync();
        //        var query = (from a in noFinishedMaterial
        //                     join b in newMaterial on a.ItemCode equals b.PartItemCode
        //                     select new SemiFinishedMaterial
        //                     {
        //                         DocEntry = b.DocEntry,
        //                         ItemCode = b.ItemCode,
        //                         PartItemCode = a.PartItemCode,
        //                         ChildEntry = a.DocEntry,
        //                         PlannedQty = b.PlannedQty,
        //                         CmpltQty = b.CmpltQty
        //                     }).ToList();
        //        FinishedMaterial.AddRange(query);
        //    }
        //    var groupDocEntry = FinishedMaterial.GroupBy(c => c.DocEntry).Select(c => new
        //    {
        //        DocEntry = c.Key,
        //        PartItemCode = string.Join(",", c.Select(s => s.PartItemCode).Distinct().ToList())
        //    }).ToList();

        //    var productionId = groupDocEntry.Select(c => c.DocEntry).ToList();
        //    //生产收货
        //    var ign1 = await UnitWork.Find<product_ign1>(c => productionId.Contains(c.BaseEntry.Value) && c.BaseType == 202).Select(c => new { c.BaseEntry, c.DocEntry, c.LineNum, c.Quantity }).ToListAsync();
        //    var filter = "";
        //    ign1.ForEach(c =>
        //    {
        //        if (!string.IsNullOrWhiteSpace(filter))
        //            filter += " or ";
        //        filter += $" (o.BaseEntry={c.DocEntry} and o.Quantity={c.Quantity} and o.BaseLinNum={c.LineNum}) ";
        //    });
        //    filter = !string.IsNullOrWhiteSpace(filter) ? $" or ({filter}) and o.status=0 and o.BaseType=59 " : "";
        //    //var sql = @$"select o.BaseEntry,o.Quantity,o.BaseLinNum,isnull(o.SuppSerial,'') as SuppSerial,o.ItemCode,'' as CardCode,'' as CardName,'' as Technician,'' as PartItemCode from OSRI o where 1!=1 {filter}";
        //    //var querySuppSerial = await UnitWork.FromSql<OSRIModel>(sql).ToListAsync();
        //    var sql = @$"select o.BaseEntry,o.Quantity,o.BaseLinNum,isnull(o.SuppSerial,'') as SuppSerial,o.ItemCode,o.WhsCode from OSRI o where 1!=1 {filter}";
        //    var querySuppSerial = await UnitWork.FromSql<OSRIModel>(sql).Select(c => new { c.BaseEntry, c.Quantity, c.BaseLinNum, c.SuppSerial, c.ItemCode, c.WhsCode }).ToListAsync();
        //    //有成品未出货的生产单
        //    var queryFinilished = (from a in ign1
        //                           join b in querySuppSerial on new { a.DocEntry, a.Quantity, a.LineNum } equals new { DocEntry = b.BaseEntry.Value, b.Quantity, LineNum = b.BaseLinNum.Value }
        //                           select new FinilishedItem { DocEntry = a.BaseEntry, PartPlannedQty = a.Quantity, PartItemCode = b.ItemCode, WareHouse = b.WhsCode, productionOrg = "E3", productionOrgManager = "樊静涛", PartQty = a.Quantity }).ToList();
        //    queryFinilished = queryFinilished.GroupBy(c => c.DocEntry).Select(c => c.First()).ToList();
        //    //查询条件下所有生产成品物料生产单
        //    FinishedMaterial.ForEach(c =>
        //    {
        //        //无收货
        //        if (c.CmpltQty == 0)
        //        {
        //            //半成品获取的成品生产单
        //            if (c.ChildEntry > 0)
        //                SemiFinishedMaterial.Add(auto.Where(o => o.DocEntry == c.ChildEntry).FirstOrDefault());
        //            else
        //                SemiFinishedMaterial.Add(c);
        //        }
        //        //部分收货
        //        else if (c.CmpltQty > 0 && c.CmpltQty < c.PlannedQty)
        //        {
        //            //成品是否出货
        //            var list = queryFinilished.Where(q => q.DocEntry == c.DocEntry).ToList();
        //            //半成品获取的成品生产单
        //            if (c.ChildEntry > 0)
        //            {
        //                var item = auto.Where(o => o.DocEntry == c.ChildEntry).FirstOrDefault();
        //                if (list.Count > 0)
        //                    item.FinilishedItems = list;
        //                SemiFinishedMaterial.Add(item);
        //            }
        //            else
        //            {
        //                //var list = queryFinilished.Where(q => q.DocEntry == c.DocEntry).ToList();
        //                if (list.Count > 0)
        //                    c.FinilishedItems = list;
        //                SemiFinishedMaterial.Add(c);
        //            }
        //        }
        //        else if (c.CmpltQty == c.PlannedQty)//全部收货
        //        {
        //            //成品是否出货
        //            var list = queryFinilished.Where(q => q.DocEntry == c.DocEntry).ToList();
        //            if (list.Count>0)//存在未交货成品
        //            {
        //                //半成品获取的成品生产单
        //                if (c.ChildEntry > 0)
        //                {
        //                    var item = auto.Where(o => o.DocEntry == c.ChildEntry).FirstOrDefault();
        //                    item.FinilishedItems = list;
        //                    SemiFinishedMaterial.Add(item);
        //                }
        //                else
        //                {
        //                    c.FinilishedItems = list;
        //                    SemiFinishedMaterial.Add(c);
        //                }

        //            }
        //        }
        //    });

        //    var data = SemiFinishedMaterial.GroupBy(c => c.DocEntry).Select(c =>
        //       {
        //           var orginfo = c.First().U_WO_LTDW.Split("-");
        //           return new
        //           {
        //               DocEntry = c.Key,
        //               c.First().FinilishedItems,
        //               c.First().ItemCode,
        //               c.First().PartItemCode,
        //               c.First().PartPlannedQty,
        //               c.First().PlannedQty,
        //               c.First().wareHouse,
        //               c.First().CmpltQty,
        //               PartQty = c.First().PartPlannedQty,
        //               ProductionOrg = orginfo[0],
        //               ProductionOrgManager = orginfo.Count() > 1 ? orginfo[1] : "",
        //           };
        //       });
        //    result.Data = data;
        //    return result;
        //}

        ///// <summary>
        ///// 获取已出货物料，设备序列号
        ///// </summary>
        ///// <param name="req"></param>
        ///// <returns></returns>
        //public async Task<TableData> GetShippedMaterial(QueryProductionOrderReq req)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    if (loginContext == null)
        //    {
        //        throw new CommonException("登录已过期", Define.INVALID_TOKEN);
        //    }
        //    TableData result = new TableData();
        //    //除C开头外的成品物料
        //    var finishedMaterialObj = await UnitWork.Find<FinishedMaterial>(null).Select(c => c.ItemCode).ToListAsync();
        //    List<product_owor_wor1> FinishedMaterial = new List<product_owor_wor1>();
        //    List<product_owor_wor1> SemiFinishedMaterial = new List<product_owor_wor1>();
        //    var orderInfo = await ProductionOrderInfo(req).ToListAsync();
        //    //生产成品物料的,有生产收货的生产单
        //    FinishedMaterial.AddRange(orderInfo.Where(c => (c.ItemCode.StartsWith("C") || finishedMaterialObj.Contains(c.ItemCode)) && c.CmpltQty > 0).ToList());
        //    //生产半成品的,有生产收货的生产单，找到使用改半成品的成产单
        //    var noFinishedMaterial = orderInfo.Where(c => !c.ItemCode.StartsWith("C") && !finishedMaterialObj.Contains(c.ItemCode) && c.CmpltQty > 0).ToList();
        //    if (noFinishedMaterial.Count>0)
        //    {
        //        //半成品再去找使用生产成品的生产单
        //        var materialInfoReq = noFinishedMaterial.Select(c =>new MaterialInfo {MaterialCode= c.ItemCode } ).ToList();
        //        var newMaterial = await ProductionOrderInfo(new QueryProductionOrderReq { MaterialInfos = materialInfoReq, QueryType = 1 }).ToListAsync();
        //        var query = (from a in noFinishedMaterial
        //                     join b in newMaterial on a.ItemCode equals b.PartItemCode
        //                     select new product_owor_wor1 { DocEntry = b.DocEntry,
        //                         PlannedQty = b.PlannedQty,
        //                         CmpltQty = b.CmpltQty, ItemCode = b.ItemCode, PartItemCode = a.PartItemCode }).ToList();
        //        FinishedMaterial.AddRange(query.Where(c => c.CmpltQty > 0).ToList());
        //    }
        //    var groupDocEntry = FinishedMaterial.GroupBy(c => c.DocEntry).Select(c => new
        //    {
        //        DocEntry = c.Key,
        //        PartItemCode = string.Join(",", c.Select(s => s.PartItemCode).Distinct().ToList())
        //    }).ToList();

        //    var productionId = groupDocEntry.Select(c => c.DocEntry).ToList();
        //    //生产收货
        //    var ign1 = await UnitWork.Find<product_ign1>(c => productionId.Contains(c.BaseEntry.Value) && c.BaseType == 202).Select(c => new { c.BaseEntry, c.DocEntry, c.LineNum, c.Quantity }).ToListAsync();
        //    var filter = "";
        //    ign1.ForEach(c =>
        //    {
        //        if (!string.IsNullOrWhiteSpace(filter))
        //            filter += " or ";
        //        filter += $" (o.BaseEntry={c.DocEntry} and o.Quantity={c.Quantity} and o.BaseLinNum={c.LineNum}) ";
        //    });

        //    //序列号
        //    //var osri = await UnitWork.Find<OSRI>(c => c.BaseType == 59 && ign1.Contains(c.BaseEntry.Value)).ToListAsync();
        //    var sql = @$"SELECT DISTINCT  
        //             o.BaseEntry,o.Quantity,o.BaseLinNum,o.SuppSerial,o.ItemCode,o.ItemName,o.WhsCode,c.CardCode,c.CardName,e.lastName + e.firstName as Technician,'' as PartItemCode
        //            from OSRI o
        //            INNER JOIN OITL a on a.ItemCode = o.ItemCode and a.DocType = 15 and o.BaseType = 59 and o.STATUS = 1
        //            INNER JOIN ITL1 b on a.LogEntry = b.LogEntry and o.SysSerial = b.SysNumber
        //            INNER JOIN ODLN c on a.DocEntry = c.DocEntry
        //            INNER JOIN OCRD d on c.CardCode = d.CardCode
        //            INNER JOIN OHEM e on d.DfTcnician = e.empID
        //            where ({filter})";
        //    var querySuppSerial = UnitWork.Query<OSRIModel>(sql)
        //                            .WhereIf(!string.IsNullOrWhiteSpace(req.ManufSN), c => c.SuppSerial == req.ManufSN)
        //                            .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), c => c.CardCode.Contains(req.CardCode))
        //                            .WhereIf(!string.IsNullOrWhiteSpace(req.CardName), c => c.CardName.Contains(req.CardName));
        //    result.Count = await querySuppSerial.CountAsync();
        //    var suppSerial = await querySuppSerial
        //                            .Skip((req.page - 1) * req.limit).Take(req.limit)
        //                            .ToListAsync();
        //    var data = suppSerial.Select(c =>
        //    {
        //        //根据交货找生产单
        //        var ign2 = ign1.Where(i => i.DocEntry == c.BaseEntry && i.LineNum == c.BaseLinNum && i.Quantity == c.Quantity).FirstOrDefault()?.BaseEntry;
        //        return new
        //        {
        //            BaseEntry = c.BaseEntry,
        //            Quantity = c.Quantity,
        //            BaseLinNum = c.BaseLinNum,
        //            SuppSerial = c.SuppSerial,
        //            ItemCode = c.ItemCode,
        //            c.ItemName,
        //            CardCode = c.CardCode,
        //            CardName = c.CardName,
        //            Technician = c.Technician,
        //            ServiceOrderId = "",
        //            Status = "",
        //            PartItemCode = groupDocEntry.Where(g => g.DocEntry == ign2).FirstOrDefault()?.PartItemCode
        //        };
        //    });
        //    //suppSerial.ForEach(c =>
        //    //{
        //    //    var ign2 = ign1.Where(i => i.DocEntry == c.BaseEntry && i.LineNum == c.BaseLinNum && i.Quantity == c.Quantity).FirstOrDefault()?.BaseEntry;
        //    //    c.PartItemCode = groupDocEntry.Where(g => g.DocEntry == ign2).FirstOrDefault()?.PartItemCode;
        //    //});
        //    result.Data = data;
        //    return result;
        //}

        /// <summary>
        /// 获取成品物料前缀
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetItemType()
        {
            TableData result = new TableData();
            result.Data = await UnitWork.Find<store_item_type>(c => c.valid == true && c.parent_id == 13).Select(c => new { Id = c.type_alias, Name = c.type_alias }).ToListAsync();
            return result;
        }

        /// <summary>
        /// 获取特殊要求、系列下拉框
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetItemTypeExtra()
        {
            TableData result = new TableData();
            //成品下
            var objs = await UnitWork.Find<store_itemtype_ufd1>(c => c.TypeID == 20 && c.Fld_nm == "U_TSYQ").Select(c => new { Id = c.FldValue, Name = c.FldValue }).ToListAsync();
            var catetory = await UnitWork.Find<Category>(c => c.TypeId == "SYS_MaterialSeries").Select(c => new { Id = c.DtValue, c.Name }).ToListAsync();
            result.Data = new
            {
                Series = catetory,//系列
                Special = objs//特殊要求
            };
            return result;
        }

        /// <summary>
        /// 获取设备生产情况
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> ProductionOrderInfo(List<QueryProductionOrderReq> req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var orgInfo = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault();
            TableData result = new TableData();
            Dictionary<string, string> mananer = new Dictionary<string, string>() 
            { 
                { "R2", "刘西林" }, { "R5", "刘西林" }, { "R22", "胡呈期" }, { "R28", "胡呈期" } , { "E1", "刘西林" }
            };
            var passageway = await UnitWork.Find<store_itemtype_ufd1>(c => c.TypeID == 20 && c.Fld_nm == "U_TDS").OrderBy(c => c.IndexID).Select(c => c.FldValue).ToListAsync();
            var catetory = await UnitWork.Find<Category>(c => c.TypeId == "SYS_ExtraPassageWay").Select(c => c.DtValue).ToListAsync();
            var catetoryOrg = await UnitWork.Find<Category>(c => c.TypeId == "SYS_InternalContactOrg").Select(c => c.Name).ToListAsync();
            catetory.Add("");
            var knowledgebases = await UnitWork.Find<KnowledgeBase>(k => k.Rank == 1 && k.IsNew == true && !string.IsNullOrWhiteSpace(k.Content)).ToListAsync();
            var tsyq = await UnitWork.Find<store_itemtype_ufd1>(c => c.TypeID == 20 && c.Fld_nm == "U_TSYQ").Select(c => c.FldValue).ToListAsync();
            List<InternalContactProductionReq> ReturnProductList = new List<InternalContactProductionReq>();
            List<InternalContactServiceOrder> contactServiceOrders = new List<InternalContactServiceOrder>();
            List<InternalContactTask> contactTasks = new List<InternalContactTask>();
            List<FinilishedItem> finilishedItems = new List<FinilishedItem>();
            #region 
            req.ForEach(e =>
            {
                List<string> sp = new List<string>();
                e.Special.ForEach(s =>
                {
                    sp.AddRange(tsyq.Where(c => c.Contains(s)).ToList());
                });
                e.Special = sp.Distinct().ToList();
                //Expression<Func<store_itemtype_ufd1, bool>> waitExp = c => (c.TypeID == 20 && c.Fld_nm == "U_TSYQ");
                ////var eps = QueryableExtension.True<store_itemtype_ufd1>();
                //e.Special.ForEach(c =>
                //{
                //    waitExp = waitExp.Or(o => o.FldValue.Contains(c));
                //});
                //var rte = UnitWork.Find(waitExp).ToList();
            });
            req.ForEach(e =>
            {
                List<product_owor_wor1> productList = new List<product_owor_wor1>();//成品生产订单
                List<InternalContactProductionReq> resplist = new List<InternalContactProductionReq>();
                List<string> itemcode = new List<string>();
                #region 匹配物料编码
                var regex = new List<string>();
                string where = "";
                //
                e.Prefix.ForEach(c =>
                {
                    regex.Add($"^{c}-[{string.Join("|", e.Series)}][0-999].*");
                });
                regex.ForEach(c =>
                {
                    if (!string.IsNullOrWhiteSpace(where))
                    {
                        where += " or ";
                    }
                    where += $"ItemCode REGEXP '{c}'";
                });
                var sql = $"SELECT * from materialrange where ({where}) and (Volt>={e.VoltsStart} and Volt<={e.VoltseEnd}) and (Amp>={e.AmpsStart} and Amp<={e.AmpsEnd} and Unit='{e.CurrentUnit}')";
                var queryItemCode = UnitWork.Query<MaterialRange>(sql).ToList();

                if ((e.Fixture == null || e.Fixture.Count == 0) && (e.Special == null || e.Special.Count == 0))
                {
                    //没有后缀条件 筛选量程范围内所有物料
                    itemcode.AddRange(queryItemCode.Select(c => c.ItemCode).ToList());
                }
                else
                {
                    //有后缀条件 按条件排列组合
                    queryItemCode = queryItemCode.GroupBy(c => string.Join("-", c.ItemCode.Split("-").Take(3))).Select(c => c.First()).ToList();
                    queryItemCode.ForEach(c =>
                    {
                        //var am = $"{c.Prefix}-{c.Volt}V{c.Amp}{c.Unit}";
                        var am = string.Join("-", c.ItemCode.Split('-').Take(3));
                        //夹具
                        if (e.Fixture != null && e.Fixture.Count > 0)
                        {
                            e.Fixture.ForEach(fi =>
                            {
                                var ft = $"{am}-{fi}";
                                itemcode.Add(ft);
                                //后缀
                                if (e.Special != null && e.Special.Count > 0)
                                {
                                    e.Special.ForEach(sp =>
                                    {
                                        var s = $"{ft}{sp}";
                                        itemcode.Add(s);
                                    });
                                }
                                else
                                {
                                    itemcode.Add(am);
                                }
                            });
                        }
                        else
                        {
                            //后缀
                            if (e.Special != null && e.Special.Count > 0)
                            {
                                e.Special.ForEach(sp =>
                                {
                                    var s = $"{am}{sp}";
                                    itemcode.Add(s);
                                });
                            }
                            else
                            {
                                itemcode.Add(am);
                            }
                        }
                    });
                }
                #region MyRegion

                //e.Prefix.ForEach(c =>
                //{
                //    var refix = c;
                //    e.Series.ForEach(s =>
                //    {
                //        var series = $"{refix}-{s}";
                //        //通道
                //        passageway.ForEach(p =>
                //        {
                //            var pg = $"{series}{p}";
                //            //额外通道 Tn n
                //            catetory.ForEach(ct =>
                //            {
                //                var ca = $"{pg}{ct}";
                //                for (int i = e.VoltsStart.Value; i <= e.VoltseEnd; i++)
                //                {
                //                    var v = $"{ca}-{i}V";
                //                    for (int j = e.AmpsStart.Value; j <= e.AmpsEnd; j++)
                //                    {
                //                        var am = $"{v}{j}{e.CurrentUnit}";
                //                        //夹具
                //                        if (e.Fixture != null && e.Fixture.Count > 0)
                //                        {
                //                            e.Fixture.ForEach(fi =>
                //                            {
                //                                var ft = $"{am}-{fi}";
                //                                itemcode.Add(ft);
                //                                //后缀
                //                                if (e.Special != null && e.Special.Count > 0)
                //                                {
                //                                    e.Special.ForEach(sp =>
                //                                    {
                //                                        var s = $"{ft}{sp}";
                //                                        itemcode.Add(s);
                //                                    });
                //                                }
                //                                else
                //                                {
                //                                    itemcode.Add(am);
                //                                }
                //                            });
                //                        }
                //                        else
                //                        {
                //                            //后缀
                //                            if (e.Special != null && e.Special.Count > 0)
                //                            {
                //                                e.Special.ForEach(sp =>
                //                                {
                //                                    var s = $"{am}{sp}";
                //                                    itemcode.Add(s);
                //                                });
                //                            }
                //                            else
                //                            {
                //                                itemcode.Add(am);
                //                            }
                //                        }
                //                    }
                //                }
                //            });
                //        });
                //    });
                //});
                //itemcode.Add(m.MaterialCode);
                #endregion
                #endregion

                #region 是否批量，BOM有被使用为已批量
                //过滤出正确的成品物料编码啊
                var storeoitm = UnitWork.Find<store_oitm>(c => c.ItemCode.StartsWith("C")).Distinct().Select(c => c.ItemCode).ToList();
                storeoitm = storeoitm.Intersect(itemcode).ToList();
                //有BOM的成品物料
                //var finlishedBomOrder = BomOrder(storeoitm);
                //var productOrder = GetProductionOrderInfo(new QueryProduction { StartExcelTime = e.StartExcelTime.Value, EndExcelTime = e.EndExcelTime.Value, Versions = finlishedBomOrder});
                //productOrder.ForEach(pr => { pr.FromTheme = e.FromTheme; });
                //productList.AddRange(productOrder);
                ////BOM有被使用的物料
                //var hasOrderItem = productOrder.GroupBy(c => new { c.ItemCode, c.Version }).Select(c => new Versions { ItemCode = c.Key.ItemCode, Version = c.Key.Version }).ToList();

                //有BOM的零件物料
                //var param = new List<string>() { e.MaterialCode };
                //var b01BomOrder =  BomOrder(param);
                //B01生产单
                var b01ProductOrder = GetProductionOrderInfo(new QueryProduction { StartExcelTime = e.StartExcelTime.Value, EndExcelTime = e.EndExcelTime.Value, ItemCode = e.MaterialCode });
                b01ProductOrder.ForEach(pr => { pr.FromTheme = e.FromTheme; });
                //productList.AddRange(productOrder);
                //var partItem= b01ProductOrder.GroupBy(c => new { c.ItemCode, c.Version }).Select(c => new Versions { ItemCode = c.Key.ItemCode, Version = c.Key.Version }).ToList();

                //finlishedBomOrder.AddRange(b01BomOrder);
                //hasOrderItem.AddRange(partItem);

                //有BOM却未被使用的物料(有无生产单),即没有批量
                //var exceptLs = finlishedBomOrder.Select(c=>c.ItemCode).ToList().Except(hasOrderItem.Select(c => c.ItemCode).ToList()).ToList();
                if (!(b01ProductOrder.Count != 0 && b01ProductOrder.Count > 0))
                {
                    var mananerName = "";
                    if (mananer.Keys.Contains(orgInfo.Name))
                        mananerName = mananer[orgInfo.Name];
                    //建任务单
                    //exceptLs.ForEach(c =>
                    //{
                    contactTasks.Add(new InternalContactTask
                    {
                        ItemCode = e.MaterialCode,
                        ProductionId = null,
                        ProductionOrg = orgInfo.Name,
                        ProductionOrgManager = mananerName,
                        WareHouse = "",
                        BelongQty = 1,
                        RectifyQty = 1,
                        FromTheme = e.FromTheme,
                        Remark = "基于BOM"
                    });
                    //});
                }

                //筛选出B01用在哪些成品生产订单上
                if (b01ProductOrder.Count > 0)
                {
                    //生产半成品物料的生产单,B01(半成品)->BTE/BT/BE(半成品)->C(成品)
                    var preProduct = b01ProductOrder.Where(c => !c.ItemCode.StartsWith("C")).Select(c => c.ItemCode).ToList();
                    var finishedProduct =  FinishedProduct(preProduct, e.StartExcelTime.Value, e.EndExcelTime.Value);
                    //包含成品则添加
                    var finishIt = finishedProduct.Where(c => c.ItemCode.StartsWith("C")).ToList();
                    finishIt.ForEach(fl => { fl.FromTheme = e.FromTheme; });
                    productList.AddRange(finishIt);
                    //若还包含半成品则再筛查一次，B01(半成品)->BTE/BT/BE(半成品)
                    preProduct = finishedProduct.Where(c => !c.ItemCode.StartsWith("C")).Select(c => c.ItemCode).ToList();
                    if (preProduct.Count > 0)
                    {
                        //BTE/BT/BE(半成品)->C(成品)
                        finishedProduct =  FinishedProduct(preProduct, e.StartExcelTime.Value, e.EndExcelTime.Value);
                        finishIt = finishedProduct.Where(c => c.ItemCode.StartsWith("C")).ToList();
                        finishIt.ForEach(fl => { fl.FromTheme = e.FromTheme; });
                        productList.AddRange(finishIt);
                    }
                    //筛选条件内的成品生产订单
                    productList = productList.Where(c => storeoitm.Contains(c.ItemCode)).ToList();

                    var onlyitem = productList.Select(c => c.ItemCode).Distinct().ToList();
                    finilishedItems.AddRange(onlyitem.Select(c => new FinilishedItem
                    {
                        ItemCode = c,
                        PartItemCode = e.MaterialCode
                    }).ToList());
                }
                #endregion

                #region 是否收货
                //B01
                //未收货,OpenQty > 0为未收货和部分未收货
                var unReceivedMaterial = b01ProductOrder.Where(c => c.OpenQty > 0).ToList();
                if (unReceivedMaterial.Count>0)
                {
                    var mananerName = "";
                    if (mananer.Keys.Contains(orgInfo.Name))
                        mananerName = mananer[orgInfo.Name];
                    //建任务单，R部门修改
                    unReceivedMaterial.ForEach(c =>
                    {
                        //var org = c.Org.Split("-");
                        contactTasks.Add(new InternalContactTask
                        {
                            ItemCode = e.MaterialCode,
                            ProductionId = c.ProductionId,
                            ProductionOrg = orgInfo.Name,
                            ProductionOrgManager = mananerName,
                            WareHouse = c.Warehouse,
                            BelongQty = (int?)c.OpenQty,
                            RectifyQty = (int?)c.OpenQty,
                            FromTheme = e.FromTheme,
                            Remark = "B01未收货"
                        });

                    });
                }

                //成品
                //已收货，部分/全部收货
                var receivedMaterial = productList.Where(c => c.CmpltQty > 0).ToList();
                //未收货
                //unReceivedMaterial = productList.Where(c => c.OpenQty > 0).ToList();
                //if (unReceivedMaterial.Count > 0)
                //{

                //    //建任务单，生产部门修改
                //    unReceivedMaterial.ForEach(c =>
                //    {
                //        var org = c.Org.Split("-");
                //        contactTasks.Add(new InternalContactTask
                //        {
                //            ItemCode = c.ItemCode,
                //            ProductionId = c.ProductionId,
                //            ProductionOrg = org[0],
                //            ProductionOrgManager = org.Count() > 1 ? org[1] : "",
                //            WareHouse = c.Warehouse,
                //            BelongQty = (int?)c.OpenQty,
                //            RectifyQty = (int?)c.OpenQty,
                //            FromTheme = e.FromTheme
                //        });

                //    });
                //}
                #endregion

                #region B01是否转储
                //仓库所属部门及主管
                var owhsBasics = UnitWork.Query<q_owhs_basics>(@"select a.WhsCode,a.WhsName,b.dep_alias as OrgName,user_nm as OrgManager 
                                            from nsap_bone.store_owhs  a
                                            LEFT JOIN nsap_base.base_dep b on a.dep_id = b.dep_id
                                            LEFT JOIN nsap_base.base_boss c ON c.dep_id = b.dep_id
                                            LEFT JOIN nsap_base.base_user d ON d.user_id = c.user_id
                                            where  a.sbo_id = 1").ToList();
                //是否在生产仓有库存
                decimal? onhand = 0;
                List<WarehouseBasics> oitw = new List<WarehouseBasics>();
                var sbo = UnitWork.Find<sbo_info>(c => c.sbo_id == 1).Select(c => c.is_open).FirstOrDefault();
                if (sbo)
                {
                    oitw = UnitWork.Find<OITW>(c => c.ItemCode == e.MaterialCode && c.OnHand > 0).Select(c => new WarehouseBasics { WhsCode = c.WhsCode, OnHand = c.OnHand }).ToList();
                }
                else
                {
                    oitw = UnitWork.Find<store_oitw>(c => c.ItemCode == e.MaterialCode && c.OnHand > 0).Select(c => new WarehouseBasics { WhsCode = c.WhsCode, OnHand = c.OnHand }).ToList();
                }
                foreach (var item in oitw)
                {
                    var owhsInfo = owhsBasics.Where(c => c.WhsCode == item.WhsCode).FirstOrDefault();
                    if (item.WhsCode == "37")//主仓库
                    {
                        //E3维修任务单
                        contactTasks.Add(new InternalContactTask
                        {
                            ItemCode = e.MaterialCode,
                            ProductionId = null,
                            ProductionOrg = "E3",
                            ProductionOrgManager = "樊静涛",
                            WareHouse = item.WhsCode,
                            BelongQty = (int?)item.OnHand,
                            RectifyQty = (int?)item.OnHand,
                            FromTheme = e.FromTheme,
                            Remark = "B01在37仓"
                        });
                    }
                    else if (owhsInfo.WhsName.StartsWith("P"))//生产部门
                    {
                        contactTasks.Add(new InternalContactTask
                        {
                            ItemCode = e.MaterialCode,
                            ProductionId = null,
                            ProductionOrg = owhsInfo.OrgName,
                            ProductionOrgManager = owhsInfo.OrgManager,
                            WareHouse = item.WhsCode,
                            BelongQty = (int?)item.OnHand,
                            RectifyQty = (int?)item.OnHand,
                            FromTheme = e.FromTheme,
                            Remark = "B01已转储"
                        });
                    }
                    else//其他仓库情况
                    {
                        contactTasks.Add(new InternalContactTask
                        {
                            ItemCode = e.MaterialCode,
                            ProductionId = null,
                            ProductionOrg = "E3",
                            ProductionOrgManager = "樊静涛",
                            WareHouse = item.WhsCode,
                            BelongQty = (int?)item.OnHand,
                            RectifyQty = (int?)item.OnHand,
                            FromTheme = e.FromTheme,
                            Remark = "B01在其他仓"
                        });
                    }
                }

                #endregion

                #region 是否有序列号
                //productList
                var productionId = receivedMaterial.Select(c => c.ProductionId).ToList();
                //生产收货
                var ign1 =  UnitWork.Find<product_ign1>(c => productionId.Contains(c.BaseEntry.Value) && c.BaseType == 202).Select(c => new { c.BaseEntry, c.DocEntry, c.LineNum, c.Quantity, c.WhsCode }).ToList();
                var filter = "";
                ign1.ForEach(c =>
                {
                    if (!string.IsNullOrWhiteSpace(filter))
                        filter += " or ";
                    filter += $" (o.BaseEntry={c.DocEntry} and o.Quantity={c.Quantity} and o.BaseLinNum={c.LineNum}) ";
                });


                if (!string.IsNullOrWhiteSpace(filter))
                {
                    var isHasSql = @$"SELECT o.BaseEntry,o.Quantity,o.BaseLinNum,ISNULL(o.SuppSerial, '') as SuppSerial,o.SysSerial,o.ItemCode,o.STATUS as Status from OSRI o
                                where ({filter}) and o.BaseType=59";
                    var query = UnitWork.Query<OSRIModel>(isHasSql).Select(c => new OSRIModel
                    {
                        BaseEntry = c.BaseEntry,
                        Quantity = c.Quantity,
                        BaseLinNum = c.BaseLinNum,
                        SuppSerial = c.SuppSerial,
                        ItemCode = c.ItemCode,
                        SysSerial = c.SysSerial,
                        Status = c.Status
                        //DocEntry = 0
                    }).ToList();
                    //没有序列号的生产单
                    var querygb = query.Where(c => c.Status == 0).GroupBy(c => new
                    {
                        c.BaseEntry,
                        c.Quantity,
                        c.BaseLinNum,
                        c.ItemCode
                    }).Select(c => new
                    {
                        c.First().BaseEntry,
                        c.First().Quantity,
                        c.First().BaseLinNum,
                        c.First().ItemCode,
                        Count = c.Count()
                    }).ToList();
                    if (querygb.Count > 0)
                    {
                        querygb.ForEach(c =>
                        {
                            //生产收货单
                            var ign2 = ign1.Where(i => i.DocEntry == c.BaseEntry && i.LineNum == c.BaseLinNum && i.Quantity == c.Quantity).FirstOrDefault();
                            //c.DocEntry = ign2;
                            //E3维修任务单
                            contactTasks.Add(new InternalContactTask
                            {
                                ItemCode = c.ItemCode,
                                ProductionId = ign2?.BaseEntry,
                                ProductionOrg = "E3",
                                ProductionOrgManager = "樊静涛",
                                WareHouse = ign2?.WhsCode,
                                BelongQty = c.Count,
                                RectifyQty = c.Count,
                                FromTheme = e.FromTheme,
                                Remark = "无序列号"
                            });
                        });
                    }

                    //有序列号的情况
                    //序列号详细
                    //var osri = await UnitWork.Find<OSRI>(c => c.BaseType == 59 && ign1.Contains(c.BaseEntry.Value)).ToListAsync();
                    var otherEn = query.Where(c => c.Status == 1).ToList();

                    var itemcodes = otherEn.Select(c => c.ItemCode).Distinct().ToList();
                    var sysSerial = otherEn.Select(c => c.SysSerial).ToList();

                    var qq = (from a in UnitWork.Find<OITL>(null)
                                           join b in UnitWork.Find<ITL1>(null) on a.LogEntry equals b.LogEntry
                                           join c in UnitWork.Find<ODLN>(null) on a.DocEntry equals c.DocEntry
                                           join d in UnitWork.Find<OCRD>(null) on c.CardCode equals d.CardCode
                                           join ee in UnitWork.Find<OHEM>(null) on d.DfTcnician equals ee.empID
                                           where itemcodes.Contains(a.ItemCode) && a.DocType == 15 && sysSerial.Contains(b.SysNumber)
                                           select new { a.ItemCode, a.ItemName, b.SysNumber, c.CardCode, c.CardName, Technician = ee.lastName + ee.firstName }).ToList();

                    var querySuppSerial = (from a in otherEn
                              join b in qq on new { a.ItemCode, SysNumber = a.SysSerial } equals new { b.ItemCode, b.SysNumber }
                              select new OSRIModel
                              {
                                  //BaseEntry = c.BaseEntry,
                                  Quantity = a.Quantity,
                                  //BaseLinNum = c.BaseLinNum,
                                  SuppSerial = a.SuppSerial,
                                  ItemCode = b.ItemCode,
                                  ItemName = b.ItemName,
                                  //WhsCode = c.WhsCode,
                                  CardCode = b.CardCode,
                                  CardName = b.CardName,
                                  Technician = b.Technician
                              }).ToList();

                    //var sqldetail = @$"SELECT DISTINCT  
                    //         o.BaseEntry,o.Quantity,o.BaseLinNum,o.SuppSerial,o.ItemCode,o.ItemName,o.WhsCode,c.CardCode,c.CardName,e.lastName + e.firstName as Technician
                    //        from OSRI o
                    //        INNER JOIN OITL a on a.ItemCode = o.ItemCode and a.DocType = 15 and o.BaseType = 59 and o.STATUS = 1
                    //        INNER JOIN ITL1 b on a.LogEntry = b.LogEntry and o.SysSerial = b.SysNumber
                    //        INNER JOIN ODLN c on a.DocEntry = c.DocEntry
                    //        INNER JOIN OCRD d on c.CardCode = d.CardCode
                    //        INNER JOIN OHEM e on d.DfTcnician = e.empID
                    //        where ({filter})";
                    //var querySuppSerial = UnitWork.Query<OSRIModel>(sqldetail).Select(c => new OSRIModel
                    //{
                    //    BaseEntry = c.BaseEntry,
                    //    Quantity = c.Quantity,
                    //    BaseLinNum = c.BaseLinNum,
                    //    SuppSerial = c.SuppSerial,
                    //    ItemCode = c.ItemCode,
                    //    ItemName = c.ItemName,
                    //    WhsCode = c.WhsCode,
                    //    CardCode = c.CardCode,
                    //    CardName = c.CardName,
                    //    Technician = c.Technician
                    //}).ToList();
                    if (querySuppSerial.Count > 0)
                    {
                        //服务单
                        querySuppSerial.ForEach(q =>
                        {
                            //var osrn = otherEn.Where(c => c.SysSerial == q.SysNumber).FirstOrDefault();
                            if (contactServiceOrders.Where(co => co.MnfSerial == q.SuppSerial).Count() == 0)
                            {
                                contactServiceOrders.Add(new InternalContactServiceOrder
                                {
                                    ItemCode = q.ItemCode,
                                    ItemName = q.ItemName,
                                    MnfSerial = q.SuppSerial,
                                    CardCode = q.CardCode,
                                    CardName = q.CardName,
                                    Supervisor = q.Technician,
                                    FromTheme = e.FromTheme
                                });
                            }
                        });
                    }

                    resplist = productList.Select(f =>
                     {
                         var ign3 = ign1.Where(ig => ig.BaseEntry == f.ProductionId).ToList();
                         List<InternalContactProductionDetailReq> internalContactProductionDetailReq = new List<InternalContactProductionDetailReq>();
                         if (ign3 != null && ign3.Count() > 0)
                         {
                             List<OSRIModel> osri3 = new List<OSRIModel>();
                             ign3.ForEach(ig =>
                             {
                                 osri3.AddRange(query.Where(os => os.BaseEntry == ig.DocEntry && os.Quantity == ig.Quantity && os.BaseLinNum == ig.LineNum).ToList());
                             });

                             if (osri3.Count() > 0)
                             {
                                 var detail = osri3.Select(o =>
                                 {
                                     return new InternalContactProductionDetailReq
                                     {
                                         MnfSerial = o.SuppSerial,
                                         Quantity = 1,
                                         WhsCode = o.Status == 0 ? string.Join(",", ign3.Select(i3 => i3.WhsCode).Distinct().ToList()) : "",
                                         OrgName = o.Status == 0 ? "E3" : "S19"
                                     };
                                 }).ToList();
                                 internalContactProductionDetailReq.AddRange(detail);
                             }
                             else
                             {
                                 internalContactProductionDetailReq.Add(new InternalContactProductionDetailReq { MnfSerial = "", WhsCode = string.Join(",", ign3.Select(i3 => i3.WhsCode).Distinct().ToList()), OrgName = "E3", Quantity = (int)ign3.Sum(i3 => i3.Quantity) });
                             }

                         }
                         return new InternalContactProductionReq
                         {
                             InternalContactProductionDetails = internalContactProductionDetailReq,
                             CmpltQty = (int)f.CmpltQty,
                             Code = f.Code,
                             FromTheme = f.FromTheme,
                             ItemCode = f.ItemCode,
                             ItemName = f.ItemName,
                             OpenQty = (int)f.OpenQty,
                             OrderType = f.OrderType,
                             OrgName = f.Org,
                             PlannedQty = (int)f.PlannedQty,
                             ProductionId = f.ProductionId,
                             Remark = f.Remark,
                             Version = f.Version,
                             Warehouse = f.Warehouse
                         };
                     }).ToList();
                }
                #endregion

                //ReturnProductList.AddRange(productList);
                //ReturnProductList.AddRange(b01ProductOrder);
                ReturnProductList.AddRange(resplist);
                ReturnProductList.AddRange(b01ProductOrder.Select(f => new InternalContactProductionReq
                {
                    CmpltQty = (int)f.CmpltQty,
                    Code = f.Code,
                    FromTheme = f.FromTheme,
                    ItemCode = f.ItemCode,
                    ItemName = f.ItemName,
                    OpenQty = (int)f.OpenQty,
                    OrderType = f.OrderType,
                    OrgName = f.Org,
                    PlannedQty = (int)f.PlannedQty,
                    ProductionId = f.ProductionId,
                    Remark = f.Remark,
                    Version = f.Version,
                    Warehouse = f.Warehouse
                }).ToList());
            });
            #endregion

            var orgName = contactTasks.Select(c => c.ProductionOrg).ToList();
            if (contactServiceOrders.Count > 0)
            {
                orgName.Add("S19");//有服务单增加
            }
            var checkOrg = UnitWork.Find<OpenAuth.Repository.Domain.Org>(c => orgName.Contains(c.Name)).Select(c => c.Id).ToList();
            var acceptOrg= UnitWork.Find<OpenAuth.Repository.Domain.Org>(c => catetoryOrg.Contains(c.Name)).Select(c => c.Id).ToList();
            //设置呼叫主题
            ReturnProductList.ForEach(q =>
            {
                q.Code = knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? q.ItemCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault();
                q.FromTheme = HandleFromTheme(q.FromTheme, q.Code);
            });

            contactTasks = contactTasks.GroupBy(c => new { c.BelongQty, c.FinishedQty, c.FromTheme, c.ItemCode, c.ProductionId, c.ProductionOrg, c.ProductionOrgManager, c.RectifyQty, c.WareHouse }).Select(c => c.First()).ToList();
            contactTasks.ForEach(q =>
            {
                var Code = knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? q.ItemCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault();
                q.FromTheme = HandleFromTheme(q.FromTheme, Code);
            });
            contactServiceOrders.ForEach(q =>
            {
                var Code = knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault() == null ? q.ItemCode.Substring(0, 1) == "M" ? "023" : "024" : knowledgebases.Where(k => Regex.IsMatch(q.ItemCode, k.Content)).Select(k => k.Code).FirstOrDefault();
                q.FromTheme = HandleFromTheme(q.FromTheme, Code);
            });
            result.Data = new
            {
                Count = ReturnProductList.Count(),
                ProductList = ReturnProductList.GroupBy(c => c.ProductionId).Select(c => c.First()).OrderBy(c => c.ItemCode).ToList(),
                InternalContactTask = contactTasks,
                InternalContactServiceOrder = contactServiceOrders,
                CheckOrg = checkOrg,
                acceptOrg,
                finilishedItems
            };
            return result;
        }

        /// <summary>
        /// 获取生产订单
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        private List<product_owor_wor1> GetProductionOrderInfo(QueryProduction req)
        {
            //string filter = "and (";
            //string filterString = "";
            //if (req.Versions.Count > 0)
            //{
            //        req.Versions.ForEach(c =>
            //        {
            //            if (!string.IsNullOrWhiteSpace(filterString))
            //                filterString += " or ";
            //            filterString += $" (a.ItemCode='{c.ItemCode}'";
            //            if (!string.IsNullOrWhiteSpace(c.Version))
            //                filterString += $" and a.U_BOM_BBH = '{c.Version}' ";
            //            filterString += ")";
            //        });
            //    filter += filterString + ")";
            //}
            //filter = !string.IsNullOrWhiteSpace(filterString) ? filter : "";
            //if (string.IsNullOrWhiteSpace(filter))
            //{
            //    return new List<product_owor_wor1>();
            //}
            var sql = @$"SELECT a.DocEntry as ProductionId,a.ItemCode,a.txtitemName as ItemName,a.PlannedQty,a.CmpltQty,a.U_WO_LTDW as Org,a.PlannedQty - a.CmpltQty as OpenQty,a.U_BOM_BBH as Version,Warehouse,Type as OrderType FROM product_owor a
                        where a.CreateDate >= '{req.StartExcelTime.Date}' and a.CreateDate<'{req.EndExcelTime.AddDays(1).Date}' and a.ItemCode='{req.ItemCode}'  ORDER BY DocEntry desc";
            var query = UnitWork.Query<product_owor_wor1>(sql).Select(c => new product_owor_wor1
            {
                ProductionId = c.ProductionId,
                ItemCode = c.ItemCode,
                ItemName = c.ItemName,
                PlannedQty = c.PlannedQty,
                CmpltQty = c.CmpltQty,
                Org = c.Org,
                OpenQty = c.OpenQty,
                Version = c.Version,
                Warehouse = c.Warehouse,
                OrderType = c.OrderType
            }).ToList();
            return query;
        }

        /// <summary>
        /// 半成品用在成品上的生产单
        /// </summary>
        /// <param name="preProduct"></param>
        /// <returns></returns>
        public List<product_owor_wor1> FinishedProduct(List<string> preProduct,DateTime start,DateTime end)
        {
            var productOrder3 = from a in UnitWork.Find<product_owor>(null)
                                join b in UnitWork.Find<product_wor1>(null) on new { a.sbo_id, a.DocEntry } equals new { b.sbo_id, b.DocEntry }
                                where preProduct.Contains(b.ItemCode) && a.CreateDate >= start.Date && a.CreateDate < end.AddDays(1).Date
                                select new product_owor_wor1
                                {
                                    ProductionId = a.DocEntry,
                                    ItemCode = a.ItemCode,
                                    ItemName = a.txtitemName,
                                    Org = a.U_WO_LTDW,
                                    PlannedQty = a.PlannedQty,//计划数量
                                    CmpltQty = a.CmpltQty,//已发货数量
                                    OpenQty = a.PlannedQty - a.CmpltQty//承诺数量 }
                                };
            return productOrder3.ToList();
        }

        /// <summary>
        /// 有BOM的物料
        /// </summary>
        /// <param name="itemcode"></param>
        /// <returns></returns>
        public List<Versions> BomOrder(List<string> itemcode)
        {
            var bomsql = @"SELECT T0.Code as ItemCode,T0.U_BBH as Version
                            FROM nsap_bone.store_oitt AS T0 
                            LEFT JOIN nsap_base.sbo_info AS T1 ON T0.sbo_id = T1.sbo_id
                            LEFT JOIN nsap_bone.store_oitm AS T2 ON T2.sbo_id = 1 AND T0.Code = T2.ItemCode 
                            where T0.IsDefaultVersion ='1'";
            //有BOM的物料
            var bomItem =  UnitWork.Query<product_owor_wor1>(bomsql).Where(c => itemcode.Contains(c.ItemCode)).Select(c => new Versions { ItemCode = c.ItemCode, Version = c.Version }).ToList();
            return bomItem;
        }

        public string HandleFromTheme(string data, string code)
        {
            var FromThemeJson = JsonHelper.Instance.Deserialize<List<FromThemeJsonResp>>(data);
            FromThemeJson.ForEach(c =>
            {
                c.code = $"{code}-{c.code}";
            });
            return JsonHelper.Instance.Serialize(FromThemeJson);
        }

        //public async Task<dynamic> ProcessMaterial()
        //{
            
        //}

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

        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        public List<string> UpdloadImg(IFormFileCollection files)
        {
            if (files.Count < 1)
                throw new Exception("文件为空。");
            //返回的文件地址
            List<string> filenames = new List<string>();
            var now = DateTime.Now;
            //文件存储路径
            var filePath = string.Format("/wwwroot/internalcontactImg/{0}/", now.ToString("yyyyMMdd"));
            //获取当前web目录
            var webRootPath = Directory.GetCurrentDirectory();
            if (!Directory.Exists(webRootPath + filePath))
            {
                Directory.CreateDirectory(webRootPath + filePath);
            }
            try
            {
                foreach (var item in files)
                {
                    if (item != null)
                    {
                        #region  图片文件的条件判断
                        //文件后缀
                        var fileExtension = Path.GetExtension(item.FileName);

                        //判断后缀是否是图片
                        const string fileFilt = ".gif|.jpg|.jpeg|.png";
                        if (fileExtension == null)
                        {
                            throw new Exception("上传的文件没有后缀。");
                        }
                        if (fileFilt.IndexOf(fileExtension.ToLower(), StringComparison.Ordinal) <= -1)
                        {
                            throw new Exception("请上传jpg、png、gif格式的图片。");
                        }

                        //判断文件大小    
                        //long length = item.Length;
                        //if (length > 1024 * 1024 * 2) //2M
                        //{
                        //    break;
                        //    //return Error("上传的文件不能大于2M");
                        //}

                        #endregion

                        var strDateTime = DateTime.Now.ToString("yyMMddhhmmssfff"); //取得时间字符串
                        var strRan = Convert.ToString(new Random().Next(100, 999)); //生成三位随机数
                        var saveName = strDateTime + strRan + fileExtension;

                        //插入图片数据                 
                        using (FileStream fs = System.IO.File.Create(webRootPath + filePath + saveName))
                        {
                            item.CopyTo(fs);
                            fs.Flush();
                        }
                        filenames.Add(filePath + saveName);
                    }
                }
                return filenames;
            }
            catch (Exception ex)
            {
                throw new Exception("上传失败。" + ex.Message);
            }
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

        #region Extension

        //public static IQueryable<product_wor1> GetFilterExpression<product_wor1>(this IQueryable<product_wor1> source, bool excute, Expression<Func<product_wor1, bool>> predicate, List<MaterialInfo> condition)
        //{
        //    if (excute)
        //    {
        //        condition.ForEach(c=>{
        //            predicate.Or<product_wor1>(t=>t.)
        //        })
        //    }

        //}
        #endregion

        public async Task MaterialSplit()
        {
            try
            {
                var oitm = await UnitWork.Find<store_oitm>(c => c.ItemCode.StartsWith("C") && c.sbo_id == Define.SBO_ID).Select(c => c.ItemCode).Distinct().ToListAsync();
                var material = await UnitWork.Find<MaterialRange>(null).Select(c => c.ItemCode).ToListAsync();
                oitm = oitm.Except(material).ToList();
                List<MaterialRange> sp = new List<MaterialRange>();
                foreach (var c in oitm)
                {
                    string? v = null, a = null;
                    var unit = "";
                    var aa = c.Split('-').Count() >= 3 ? c.Split('-')[2] : "";
                    if (aa.Contains("V"))
                    {
                        v = aa.Substring(0, aa.IndexOf("V"));
                    }
                    if (aa.Contains("A"))
                    {
                        unit = "A";
                        if (aa.Contains("mA"))
                        {
                            unit = "mA";
                        }
                        a = aa.Substring(aa.IndexOf("V") + 1, aa.IndexOf(unit) - aa.IndexOf("V") - 1);
                    }
                    sp.Add(new MaterialRange
                    {
                        Prefix = c.Split('-')[0],
                        ItemCode = c,
                        Volt = v,
                        Amp = a,
                        Unit = unit
                    });
                }
                if (sp.Count > 0)
                {
                    await UnitWork.BatchAddAsync<MaterialRange, int>(sp.ToArray());
                    await UnitWork.SaveAsync();
                }
            }
            catch (Exception e)
            {

                throw e;
            }
        }
    }
}
