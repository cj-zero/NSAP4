using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using OpenAuth.App.Request;
using OpenAuth.App.Nwcali.Request;
using Infrastructure.Extensions;
using OpenAuth.App.Serve.Response;
using OpenAuth.Repository.Domain.Serve;
using OpenAuth.Repository;
using System.Data;
using OpenAuth.Repository.Domain.Settlement;
using System.Linq.Expressions;

namespace OpenAuth.App.Serve
{
    public class BlameBelongApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp; 
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly UserManagerApp _userManagerApp;
        private readonly OrgManagerApp _orgManagerApp;
        public BlameBelongApp(IUnitWork unitWork, IAuth auth, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp, UserManagerApp userManagerApp, OrgManagerApp orgManagerApp) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _userManagerApp = userManagerApp;
            _orgManagerApp = orgManagerApp;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> Load(QueryBlameBelongReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var loginUser = loginContext.User;

            Expression<Func<BlameBelong, bool>> exp = t => false;
            if (req.QryBasis != null)
            {
                req.QryBasis.ForEach(c =>
                {
                    exp = PredicateBuilder.Or(exp, t => t.Basis.Contains(c));
                });
            }
            var query = UnitWork.Find<BlameBelong>(null)
                            .WhereIf(req.Id != null, c => c.Id.ToString().Contains(req.Id.ToString()))
                            .WhereIf(req.QryBasis != null, exp)
                            .WhereIf(!string.IsNullOrWhiteSpace(req.CardCode), c => c.CardCode.Contains(req.CardCode) || c.CardName.Contains(req.CardCode))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.StartDate.ToString()), r => r.CreateTime > req.StartDate)
                            .WhereIf(!string.IsNullOrWhiteSpace(req.EndDate.ToString()), r => r.CreateTime < Convert.ToDateTime(req.EndDate).AddDays(1))
                            .WhereIf(req.QryDocType != null, c => c.DocType == req.QryDocType)
                            .WhereIf(req.SaleOrderId != null, c => c.SaleOrderId == req.SaleOrderId)
                            .WhereIf(req.OrderNo != null, c => c.OrderNo.ToString().Contains(req.OrderNo.ToString()))
                            .WhereIf(!string.IsNullOrWhiteSpace(req.VestinOrg), c => c.VestinOrg.Contains(req.VestinOrg))
                            //.WhereIf(req.Status != null, c => c.Status == req.Status)
                            ;
            if (req.Status != null)
            {
                if (req.Status == 7)
                {
                    query = query.Where(c => c.Status < 6);
                }
                else
                {
                    query = query.Where(c => c.Status == req.Status);
                }
            }


            if (req.PageType == 1)//我的提交
            {
                query = query.WhereIf(loginContext.User.Account != Define.SYSTEM_USERNAME, c => c.CreateUserId == loginContext.User.Id);
            }
            else if (req.PageType == 2)//按灯单据
            {
                if (loginUser.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("按灯单据查看")))
                {
                    var orgRole = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.Key == Define.ORGROLE && c.FirstId == loginContext.User.Id).FirstOrDefaultAsync();
                    if (orgRole != null)//查看本部下数据
                    {
                        var orgId = orgRole.SecondId;
                        var ids = await UnitWork.Find<BlameBelongOrg>(c => c.OrgId == orgId && c.IsHistory == false).Select(c => c.BlameBelongId).ToListAsync();
                        //var userIds = await UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(c => c.SecondId == orgId && c.Key == Define.USERORG).Select(c => c.FirstId).ToListAsync();
                        query = query.Where(r => ids.Contains(r.Id));
                    }
                }
            }
            else if (req.PageType == 3)//待处理
            {
                var mf = _moduleFlowSchemeApp.Get(c => c.Module.Name == "按灯");
                var flowinstace = await UnitWork.Find<FlowInstance>(c => c.SchemeId == mf.FlowSchemeId && c.MakerList.Contains(loginContext.User.Id)).Select(c => c.Id).ToListAsync();
                query = query.Where(c => flowinstace.Contains(c.FlowInstanceId));
            }
            else if (req.PageType == 4)//已处理
            {
                var instances = await UnitWork.Find<BlameBelongHistory>(c => c.CreateUserId == loginContext.User.Id).Select(c => c.BlameBelongId).Distinct().ToListAsync();
                query = query.Where(c => instances.Contains(c.Id));

            }

            result.Count = await query.CountAsync();
            result.Data = await query.OrderByDescending(c => c.CreateTime).Skip((req.page - 1) * req.limit).Take(req.limit).ToListAsync();
            return result;
        }

        /// <summary>
        /// 提交申请
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> Add(AddOrUpdateBlameBelongReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            Infrastructure.Response result = new Infrastructure.Response();

            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP && req.AppUserId > 0)
            {
                loginUser = await GetUserId(Convert.ToInt32(req.AppUserId));
            }
            var isExist = await UnitWork.Find<BlameBelong>(null).FirstOrDefaultAsync(c => c.DocType == req.DocType && c.OrderNo == req.OrderNo && c.SerialNumber == req.SerialNumber);
            if (isExist != null && req.DocType != 4)
            {
                if (isExist.Id != req.Id)
                {
                    result.Code = 500;
                    result.Message = "单据类型或单据号错误。";
                    return result;
                }
            }
            var obj = req.MapTo<BlameBelong>();

            if (obj.DocType == 1)
            {
                var seriviceOrder = await UnitWork.Find<ServiceOrder>(c => c.U_SAP_ID == req.OrderNo).Select(c => new { c.Id, c.TerminalCustomerId, c.TerminalCustomer, c.U_SAP_ID }).FirstOrDefaultAsync();
                if (seriviceOrder == null)
                {
                    result.Code = 500;
                    result.Message = "单据类型或单据号错误。";
                    return result;
                }
                //var manufSN = await UnitWork.Find<ServiceWorkOrder>(c => c.ServiceOrderId == seriviceOrder.Id).Select(c => c.ManufacturerSerialNumber).FirstOrDefaultAsync();

                var query = await (from a in UnitWork.Find<OINS>(null)
                                   join b in UnitWork.Find<DLN1>(null) on a.deliveryNo equals b.DocEntry into ab
                                   from b in ab.DefaultIfEmpty()
                                   join c in UnitWork.Find<OWOR>(null) on b.DocEntry equals c.OriginAbs into bc
                                   from c in bc.DefaultIfEmpty()
                                   where a.manufSN == req.SerialNumber && b.BaseType == 17 && b.BaseEntry > 0
                                   select new { b.BaseEntry, c.DocEntry, c.U_WO_LTDW }).FirstOrDefaultAsync();
                obj.SaleOrderId = query?.BaseEntry;
                obj.ProductionOrg = query?.U_WO_LTDW;
                obj.CardCode = seriviceOrder.TerminalCustomerId;
                obj.CardName = seriviceOrder.TerminalCustomer;
                obj.OrderNo = seriviceOrder.U_SAP_ID;
            }
            else if (obj.DocType == 2)
            {
                var isExists = await UnitWork.Find<OWOR>(null).AnyAsync(c => c.DocEntry == req.OrderNo);
                if (!isExists)
                {
                    result.Code = 500;
                    result.Message = "单据类型或单据号错误。";
                    return result;
                }

                var query = await (from a in UnitWork.Find<OWOR>(null)
                                   join b in UnitWork.Find<ORDR>(null) on a.OriginAbs equals b.DocEntry into ab
                                   from b in ab.DefaultIfEmpty()
                                   select new { a.U_WO_LTDW, b.DocEntry, b.CardCode, b.CardName }).FirstOrDefaultAsync();

                obj.SaleOrderId = query?.DocEntry;
                obj.ProductionOrg = query?.U_WO_LTDW;
                obj.CardCode = query.CardCode;
                obj.CardName = query.CardName;

            }
            else if (obj.DocType == 3)
            {
                var ordr = await UnitWork.Find<ORDR>(c => c.DocEntry == req.SaleOrderId).Select(c => new { c.CardCode, c.CardName }).FirstOrDefaultAsync();

                obj.CardCode = ordr?.CardCode;
                obj.CardName = ordr?.CardName;
            }

            obj.BlameBelongOrgs = new List<BlameBelongOrg>();
            req.OrgInfos.ForEach(c =>
            {
                obj.BlameBelongOrgs.Add(new BlameBelongOrg { OrgId = c.OrgId, OrgName = c.OrgName });
            });
            var orgIds = obj.BlameBelongOrgs.Select(c => c.OrgId);
            var relevance = await (from a in UnitWork.Find<OpenAuth.Repository.Domain.Relevance>(null)
                                   join b in UnitWork.Find<User>(null) on a.FirstId equals b.Id
                                   where a.Key == Define.ORGROLE && orgIds.Contains(a.SecondId)
                                   select new { a.SecondId, b.Id, b.Name }).ToListAsync();

            obj.BlameBelongOrgs.ForEach(c =>
            {
                var user = relevance.Where(r => r.SecondId == c.OrgId).FirstOrDefault();
                c.Manager = user?.Name;
                c.ManagerId = user?.Id;
                c.HandleUserName = user?.Name;
                c.HandleUserId = user?.Id;
            });

            var manager = obj.BlameBelongOrgs.Where(c => string.IsNullOrWhiteSpace(c.Manager)).FirstOrDefault();
            if (manager != null)
            {
                result.Code = 500;
                result.Message = $"部门{manager.OrgName}暂未设置部门主管。";
                return result;
            }
            obj.BlameBelongFiles = new List<BlameBelongFile>();
            req.FileIds.ForEach(c =>
            {
                obj.BlameBelongFiles.Add(new BlameBelongFile { FileId = c });
            });

            var dbContext = UnitWork.GetDbContext<BlameBelong>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    var single = await UnitWork.Find<BlameBelong>(c => c.Id == obj.Id).FirstOrDefaultAsync();
                    if (single != null && single.Id > 0)
                    {
                        //删除
                        await UnitWork.DeleteAsync<BlameBelongOrg>(c => c.BlameBelongId == single.Id);
                        await UnitWork.DeleteAsync<BlameBelongFile>(c => c.BlameBelongId == single.Id);
                        //新增
                        obj.BlameBelongOrgs.ForEach(c => { c.BlameBelongId = single.Id; });
                        obj.BlameBelongFiles.ForEach(c => { c.BlameBelongId = single.Id; });
                        await UnitWork.BatchAddAsync(obj.BlameBelongOrgs.ToArray());
                        await UnitWork.BatchAddAsync(obj.BlameBelongFiles.ToArray());

                        await UnitWork.UpdateAsync<BlameBelong>(c => c.Id == obj.Id, c => new BlameBelong
                        {
                            Basis = obj.Basis,
                            Description = obj.Basis,
                            DocType = obj.DocType,
                            OrderNo = obj.OrderNo,
                            SerialNumber = obj.SerialNumber,
                            VestinOrg = obj.VestinOrg,
                            ProductionOrg = obj.ProductionOrg,
                            SaleOrderId = obj.SaleOrderId,
                            CardCode = obj.CardCode,
                            CardName = obj.CardName,
                            AffectMoney = obj.AffectMoney,
                            Status = 2,
                        });
                        VerificationReq verificationReq = new VerificationReq
                        {
                            NodeRejectStep = "",
                            NodeRejectType = "0",
                            FlowInstanceId = single.FlowInstanceId,
                            VerificationFinally = "1",
                            VerificationOpinion = ""
                        };
                        obj.FlowInstanceId = single.FlowInstanceId;
                        await _flowInstanceApp.Verification(verificationReq);
                        await UnitWork.SaveAsync();
                    }
                    else
                    {
                        obj.CreateTime = DateTime.Now;
                        obj.CreateUser = loginUser.Name;
                        obj.CreateUserId = loginUser.Id;
                        //创建流程
                        var mf = await UnitWork.Find<FlowScheme>(c => c.SchemeName == "按灯").FirstOrDefaultAsync();
                        var flow = new AddFlowInstanceReq();
                        flow.SchemeId = mf.Id;
                        flow.FrmType = 2;
                        flow.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                        flow.CustomName = $"按灯";
                        flow.FrmData = "{\"IsAgree\": \"" + 1 + "\"}";
                        obj.FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(flow);
                        obj.Status = 2;

                        obj = await UnitWork.AddAsync<BlameBelong, int>(obj);
                        await UnitWork.SaveAsync();

                    }

                    //设置环节执行人
                    var userIds = obj.BlameBelongOrgs.Select(c => c.ManagerId).ToArray();
                    var userName = string.Join(",", obj.BlameBelongOrgs.Select(c => c.Manager).ToArray());
                    await _flowInstanceApp.ModifyNodeUser(obj.FlowInstanceId, true, userIds, userName, false);


                    //记录操作日志
                    await UnitWork.AddAsync<BlameBelongHistory>(new BlameBelongHistory
                    {
                        Action = "提交",
                        CreateUser = loginUser.Name,
                        CreateUserId = loginUser.Id,
                        CreateTime = DateTime.Now,
                        BlameBelongId = obj.Id
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("发起申请失败。" + ex.Message);
                }
            }

            return result;
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private async Task<User> GetUserId(int AppId)
        {
            var userid = await UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefaultAsync();

            return await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 获取采购单下销售单
        /// </summary>
        /// <returns></returns>
        public async Task<Infrastructure.Response<dynamic>> GetSaleOrderOrSN(int id, int type)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            Infrastructure.Response<dynamic> result = new Infrastructure.Response<dynamic>();
            if (type==1)
            {
                var userId = (await UnitWork.Find<NsapUserMap>(n => n.UserID == loginContext.User.Id).FirstOrDefaultAsync())?.NsapUserId;
                var slpCode = (await UnitWork.Find<sbo_user>(s => s.user_id == userId && s.sbo_id == Define.SBO_ID).FirstOrDefaultAsync())?.sale_id;
                if (slpCode == null)
                {
                    result.Code = 500;
                    result.Message = $"未绑定销售员code。";
                    return result;
                }
                var query = await (from a in UnitWork.Find<buy_porrel>(null)
                                   join b in UnitWork.Find<sale_ordr>(null) on a.RelDoc_Entry equals b.DocEntry
                                   where a.RelDoc_Type == 1 && a.sbo_id == 1 && a.POR_Entry == id && b.SlpCode == slpCode
                                   select b.DocEntry).ToListAsync();
                result.Result = query;
            }
            else if (type == 2)
            {
                var serviceOrderId = await UnitWork.Find<ServiceOrder>(c => (c.U_SAP_ID == id || c.Id == id) && c.Status == 2 && c.VestInOrg == 1).Select(c => c.Id).FirstOrDefaultAsync();
                if (!(serviceOrderId > 0))
                {
                    result.Code = 500;
                    result.Message = $"服务ID输入有误。";
                    return result;
                }
                result.Result = await UnitWork.Find<ServiceWorkOrder>(c => c.ServiceOrderId == serviceOrderId).Select(c => c.ManufacturerSerialNumber).ToListAsync();
            }
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
            TableData result = new TableData();
            var loginUser = loginContext.User;
            var query = await UnitWork.Find<BlameBelong>(c => c.Id == id).Include(c => c.BlameBelongOrgs).FirstOrDefaultAsync();
            var blameBelongResp = query.MapTo<BlameBelongResp>();
            blameBelongResp.VestinOrg = string.Join(",", blameBelongResp.BlameBelongOrgs.Where(c => c.IsHistory != null && c.IsHistory == false).Select(c => c.OrgName).ToList());
            //附件
            var fileIds = await UnitWork.Find<BlameBelongFile>(c => c.BlameBelongId == blameBelongResp.Id).Select(c => c.FileId).ToListAsync();
            var files = await UnitWork.Find<UploadFile>(f => fileIds.Contains(f.Id)).ToListAsync();
            blameBelongResp.Files = files.MapTo<List<UploadFileResp>>();

            var bborgids = blameBelongResp.BlameBelongOrgs.Select(c => c.Id).ToList();
            var orgfile = await UnitWork.Find<BlameBelongOrgFile>(c => bborgids.Contains(c.BlameBelongOrgId)).ToListAsync();
            //files = await UnitWork.Find<UploadFile>(f => fileIds.Contains(f.Id)).ToListAsync();
            blameBelongResp.BlameBelongOrgs.ForEach(c =>
            {
                var ids = orgfile.Where(o => o.BlameBelongOrgId == c.Id).Select(o => o.FileId).ToList();
                if (ids.Count > 0)
                {
                    var file = UnitWork.Find<UploadFile>(f => ids.Contains(f.Id)).ToList();
                    c.Files = file.MapTo<List<UploadFileResp>>();
                }
            });

            if (!string.IsNullOrWhiteSpace(blameBelongResp.FlowInstanceId))
            {
                blameBelongResp.FlowPathResp = await _flowInstanceApp.FlowPathRespList(null, blameBelongResp.FlowInstanceId);
            }
            //操作历史
            var operationHistories = await UnitWork.Find<BlameBelongHistory>(c => c.BlameBelongId == blameBelongResp.Id).OrderBy(c => c.CreateTime).ToListAsync();
            result.Data = new
            {
                blameBelongResp,
                operationHistories
            };
            return result;
        }

        /// <summary>
        /// 移交
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task TransferPerson(AccraditationBlameBelongReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var dbContext = UnitWork.GetDbContext<BlameBelong>();
            var handle = await UnitWork.Find<BlameBelongOrg>(c => c.Id == req.HandleId).FirstOrDefaultAsync();
            if (req.UserName == handle.HandleUserName)
            {
                throw new Exception("请勿移交给自己。");
            }
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    //await UnitWork.UpdateAsync<BlameBelongOrg>(c => c.Id == req.HandleId, c => new BlameBelongOrg
                    //{
                    //    Manager = req.UserName,
                    //    ManagerId = req.UserId
                    //});
                    var makerlist = await UnitWork.Find<FlowInstance>(c => c.Id == req.FlowInstanceId).Select(c => c.MakerList).FirstOrDefaultAsync();
                    makerlist = makerlist.Replace(loginUser.Id, req.UserId);

                    await UnitWork.UpdateAsync<FlowInstance>(c => c.Id == req.FlowInstanceId, c => new FlowInstance { MakerList = makerlist, });
                    await UnitWork.UpdateAsync<BlameBelongOrg>(c => c.Id == req.HandleId, c => new BlameBelongOrg { HandleUserId = req.UserId, HandleUserName = req.UserName });

                    var seleoh = await UnitWork.Find<BlameBelongHistory>(r => r.BlameBelongId.Equals(req.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    //记录操作日志
                    await UnitWork.AddAsync<BlameBelongHistory>(new BlameBelongHistory
                    {
                        Action = "责任部门审核",
                        CreateUser = loginContext.User.Name,
                        CreateUserId = loginContext.User.Id,
                        CreateTime = DateTime.Now,
                        BlameBelongId = req.Id.Value,
                        ApprovalResult = $"移交{req.UserName}",
                        IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds)
                    });
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("移交失败。" + ex.Message);
                }
            }
        }

        /// <summary>
        /// 审核
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Accraditation(AccraditationBlameBelongReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var dbContext = UnitWork.GetDbContext<BlameBelong>();

            var blameBelong = await UnitWork.Find<BlameBelong>(c => c.Id == req.Id).Include(c => c.BlameBelongOrgs).FirstOrDefaultAsync();
            var flowinstace = await UnitWork.Find<FlowInstance>(c => c.Id == blameBelong.FlowInstanceId).FirstOrDefaultAsync();

            var maker = flowinstace.MakerList;
            var vestOrg = string.Join(",", blameBelong.BlameBelongOrgs.Where(c => c.IsHistory != null && c.IsHistory == false).Select(c => c.OrgName));
            var seleoh = await UnitWork.Find<BlameBelongHistory>(r => r.BlameBelongId.Equals(req.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            VerificationReq verificationReq = new VerificationReq
            {
                NodeRejectStep = "",
                NodeRejectType = "0",
                FlowInstanceId = flowinstace.Id,
                VerificationFinally = "1",
                VerificationOpinion = ""
            };
            try
            {
                if (flowinstace.ActivityName == "责任部门审核")
                {
                    //不是历史处理 当前处理的一条
                    var forOrg = blameBelong.BlameBelongOrgs.Where(c => (c.IsHistory != null && c.IsHistory == false) && c.Id == req.HandleId).FirstOrDefault();
                    if (forOrg != null)
                    {
                        if (forOrg.AppealTime == 2 && req.Idea == 2)
                        {
                            throw new Exception("已被HR驳回两次，不允许再申诉。");
                        }
                        await UnitWork.UpdateAsync<BlameBelongOrg>(c => c.Id == forOrg.Id, c => new BlameBelongOrg
                        {
                            OrgIdea = req.Idea,
                            Description = req.Description,
                            IsHandle = true
                        });

                        await UnitWork.DeleteAsync<BlameBelongOrgFile>(c => c.BlameBelongOrgId == req.HandleId);
                        //保存文件
                        List<BlameBelongOrgFile> orgFiles = new List<BlameBelongOrgFile>();
                        req.Files.ForEach(c =>
                        {
                            orgFiles.Add(new BlameBelongOrgFile { FileId = c, BlameBelongOrgId = req.HandleId });
                        });
                        await UnitWork.BatchAddAsync(orgFiles.ToArray());

                        //记录操作日志
                        await UnitWork.AddAsync<BlameBelongHistory>(new BlameBelongHistory
                        {
                            Action = "责任部门审核",
                            CreateUser = $"{forOrg.OrgName}-{loginContext.User.Name}",
                            CreateUserId = loginContext.User.Id,
                            CreateTime = DateTime.Now,
                            BlameBelongId = req.Id.Value,
                            ApprovalResult = req.Idea == 2 ? "申诉" : "同意",
                            IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds)
                        });

                        var pass = blameBelong.BlameBelongOrgs.Where(c => (c.IsHistory != null && c.IsHistory == false) && c.Id != req.HandleId).All(c => c.IsHandle == true);
                        //其他部门都处理了才流转下一步
                        if (pass)
                        {
                            await _flowInstanceApp.Verification(verificationReq);

                            var loadUser = await _userManagerApp.LoadByRoleName("按灯流程-人事审核");

                            blameBelong.Status = 3;
                            pass = blameBelong.BlameBelongOrgs.Where(c => (c.IsHistory != null && c.IsHistory == false) && c.Id != req.HandleId).All(c => c.OrgIdea == 1);
                            if (pass && req.Idea == 1)//责任部门全部同意，设置下环节自动通过
                            {
                                await UnitWork.UpdateAsync<FlowInstance>(c => c.Id == flowinstace.Id, c => new FlowInstance { MakerList = "1" });//自动同意前 将审批人设为全部
                                //var sql = $"update flowinstance set MakerList='1' where Id='{flowinstace.Id}'";
                                //UnitWork.ExecuteNonQuery(ContextType.DefaultContextType, CommandType.Text, sql);
                                await Task.Delay(500);
                                await _flowInstanceApp.Verification(verificationReq);

                                //下一环节 根据不同单据 流转不同的人
                                await SetExecutor(blameBelong.DocType, flowinstace.Id);

                                blameBelong.Status = 4;
                                //记录操作日志
                                await UnitWork.AddAsync<BlameBelongHistory>(new BlameBelongHistory
                                {
                                    Action = "人事审核",
                                    CreateUser = $"{loadUser.FirstOrDefault().Name}",
                                    CreateUserId = loadUser.FirstOrDefault().Id,
                                    CreateTime = DateTime.Now,
                                    BlameBelongId = req.Id.Value,
                                    ApprovalResult = "系统默认同意",
                                    IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds)
                                });
                            }
                            else
                            {
                                //设置下一步执行人
                                var userids = loadUser.Select(c => c.Id).ToArray();
                                var names = string.Join(",", loadUser.Select(c => c.Name));
                                await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, true, userids, names, false, false);
                            }

                            await UnitWork.UpdateAsync<BlameBelong>(c => c.Id == blameBelong.Id, c => new BlameBelong { Status = blameBelong.Status });
                        }
                        else//未流转下一步 则去除已操作人
                        {
                            maker = maker.Replace(loginUser.Id, "");
                            await UnitWork.UpdateAsync<FlowInstance>(c => c.Id == flowinstace.Id, c => new FlowInstance { MakerList = maker });
                        }

                    }
                }
                else if (flowinstace.ActivityName == "人事审核")
                {
                    #region 表单数据处理
                    List<string> makerId = new List<string>();
                    List<string> makerName = new List<string>();
                    foreach (var item in req.HandleLists)
                    {
                        var currentOrg = blameBelong.BlameBelongOrgs.Where(c => (c.IsHistory != null && c.IsHistory == false) && c.Id == item.Id).FirstOrDefault();
                        var approvalResult = "";
                        if (item.HrIdea == 1)//同意
                        {
                            await UnitWork.UpdateAsync<BlameBelongOrg>(c => c.Id == item.Id, c => new BlameBelongOrg
                            {
                                HrIdea = item.HrIdea,
                                IsHistory = true,
                            });
                            approvalResult = $"同意{currentOrg.OrgName}";

                        }
                        else if (item.HrIdea == 2)//移交
                        {
                            //新增一条移交部门
                            var mananger = await _orgManagerApp.GetOrgManagerNew(item.TransferOrgId);
                            if (mananger == null)
                            {
                                throw new Exception($"部门{item.TransferOrg}暂未设置部门主管。");
                            }
                            await UnitWork.UpdateAsync<BlameBelongOrg>(c => c.Id == item.Id, c => new BlameBelongOrg
                            {
                                HrIdea = item.HrIdea,
                                IsHistory = true,
                                TransferOrg = item.TransferOrg,
                                TransferOrgId = item.TransferOrgId
                            });
                            await UnitWork.AddAsync<BlameBelongOrg>(new BlameBelongOrg { BlameBelongId = blameBelong.Id, OrgId = item.TransferOrgId, OrgName = item.TransferOrg, Manager = mananger.Name, ManagerId = mananger.Id, HandleUserId = mananger.Id, HandleUserName = mananger.Name });
                            makerId.Add(mananger.Id); makerName.Add(mananger.Name);

                            vestOrg = vestOrg.Replace(currentOrg.OrgName, item.TransferOrg);
                            approvalResult = $"移交{item.TransferOrg}";
                        }
                        else if (item.HrIdea == 3)//驳回
                        {
                            var time = blameBelong.BlameBelongOrgs.Where(c => c.Id == item.Id).Select(c => c.AppealTime).FirstOrDefault();
                            await UnitWork.UpdateAsync<BlameBelongOrg>(c => c.Id == item.Id, c => new BlameBelongOrg
                            {
                                AppealTime = time + 1
                            });
                            makerId.Add(currentOrg.HandleUserId); makerName.Add(currentOrg.HandleUserName);

                            approvalResult = $"驳回{currentOrg.OrgName}";
                        }

                        //记录操作日志
                        await UnitWork.AddAsync<BlameBelongHistory>(new BlameBelongHistory
                        {
                            Action = "人事审核",
                            CreateUser = loginContext.User.Name,
                            CreateUserId = loginContext.User.Id,
                            CreateTime = DateTime.Now,
                            BlameBelongId = req.Id.Value,
                            ApprovalResult = approvalResult,
                            IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds)
                        });

                    }
                    #endregion

                    #region 流程流转处理
                    if (req.HandleLists.Any(c => c.HrIdea == 2) || req.HandleLists.Any(c => c.HrIdea == 3))
                    {
                        verificationReq.VerificationFinally = "3";
                        verificationReq.NodeRejectType = "0";
                        await _flowInstanceApp.Verification(verificationReq);

                        //设置移交后处理人 已处理的部门不需要再处理
                        await _flowInstanceApp.ModifyNodeUser(flowinstace.Id, true, makerId.ToArray(), string.Join(",", makerName), false);

                        blameBelong.Status = 2;
                    }
                    else if (req.HandleLists.All(c => c.HrIdea == 1))
                    {
                        var handleid = req.HandleLists.Select(c => c.Id).ToList();
                        //hr 同意申诉 并且其他部门审核意见都为同意 流转到责任金额判断
                        var flag = blameBelong.BlameBelongOrgs.Where(c => (c.IsHistory != null && c.IsHistory == false) && !handleid.Contains(c.Id));
                        if (flag.Count() > 0 && flag.All(c => c.OrgIdea == 1))
                        {
                            await _flowInstanceApp.Verification(verificationReq);
                            //下一环节 根据不同单据 流转不同的人
                            await SetExecutor(blameBelong.DocType, flowinstace.Id);

                            blameBelong.Status = 4;
                        }
                        //hr同意申诉的数量 等于部门的数量 则直接结束
                        else if (req.HandleLists.Count == blameBelong.BlameBelongOrgs.Where(c => (c.IsHistory != null && c.IsHistory == false)).Count())
                        {
                            //var fromdata = "{\"IsAgree\": \"" + 0 + "\"}";
                            //修改流程执行路由
                            var sql = "update flowinstance set FrmData='{\"IsAgree\": \"" + 0 + "\"}'";
                            sql += $" where Id='{flowinstace.Id}'";
                            UnitWork.ExecuteNonQuery(ContextType.DefaultContextType, CommandType.Text, sql);
                            await _flowInstanceApp.Verification(verificationReq);
                            blameBelong.Status = 6;
                        }
                    }
                    #endregion

                    await UnitWork.UpdateAsync<BlameBelong>(c => c.Id == blameBelong.Id, c => new BlameBelong
                    {
                        Status = blameBelong.Status,
                        VestinOrg = vestOrg
                    });
                }
                else if (flowinstace.ActivityName == "责任金额判断")
                {
                    foreach (var item in req.HandleLists)
                    {
                        await UnitWork.UpdateAsync<BlameBelongOrg>(c => c.Id == item.Id, c => new BlameBelongOrg
                        {
                            Amount = item.Amount
                        });
                    }
                    await _flowInstanceApp.Verification(verificationReq);
                    await UnitWork.UpdateAsync<BlameBelong>(c => c.Id == blameBelong.Id, c => new BlameBelong
                    {
                        Status = 5
                    });
                    //记录操作日志
                    await UnitWork.AddAsync<BlameBelongHistory>(new BlameBelongHistory
                    {
                        Action = "责任金额判断",
                        CreateUser = loginContext.User.Name,
                        CreateUserId = loginContext.User.Id,
                        CreateTime = DateTime.Now,
                        BlameBelongId = req.Id.Value,
                        ApprovalResult = "同意",
                        IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds)
                    });
                }
                else if (flowinstace.ActivityName == "出纳")
                {
                    await _flowInstanceApp.Verification(verificationReq);
                    await UnitWork.UpdateAsync<BlameBelong>(c => c.Id == blameBelong.Id, c => new BlameBelong
                    {
                        Status = 6
                    });
                }
                await UnitWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("提交失败。" + ex.Message);
            }
        }


        public async Task SetExecutor(int? type,string flowinstaceId)
        {
            var userids = new string[] { };
            var names = "";
            if (type == 1)
            {
                var loadUser = await _userManagerApp.LoadByRoleName("按灯流程-服务单审核");
                userids = loadUser.Select(c => c.Id).ToArray();
                names = string.Join(",", loadUser.Select(c => c.Name));
            }
            else if (type == 2 || type == 3)
            {
                var loadUser = await _userManagerApp.LoadByRoleName("按灯流程-采购生产审核");
                userids = loadUser.Select(c => c.Id).ToArray();
                names = string.Join(",", loadUser.Select(c => c.Name));

            }
            else if (type == 4)
            {
                var loadUser = await _userManagerApp.LoadByRoleName("按灯流程-其他事务审核");
                userids = loadUser.Select(c => c.Id).ToArray();
                names = string.Join(",", loadUser.Select(c => c.Name));
            }
            await _flowInstanceApp.ModifyNodeUser(flowinstaceId, true, userids, names, false);
        }

        /// <summary>
        /// 获取责任金额
        /// </summary>
        /// <param name="blameBelongId"></param>
        /// <returns></returns>
        public async Task<TableData> GetLiabilityAmount(int blameBelongId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();

            var obj = await UnitWork.Find<BlameBelong>(c => c.Id == blameBelongId && c.DocType == 1).Include(c => c.BlameBelongOrgs).FirstOrDefaultAsync();
            if (obj == null)
            {
                result.Code = 500;
                result.Data = "未获取到单据";
            }
            var count = obj.BlameBelongOrgs.Where(c => c.IsHistory != null && c.IsHistory == false).Count();
            decimal? amount = 0;

            amount += await UnitWork.Find<ReimburseInfo>(c => c.ServiceOrderSapId == obj.OrderNo).SumAsync(c => c.TotalMoney);
            var outs = await UnitWork.Find<OutsourcExpenses>(c => c.ServiceOrderSapId == obj.OrderNo).Select(c => c.OutsourcId).FirstOrDefaultAsync();
            amount += await UnitWork.Find<Outsourc>(c => c.Id == outs).SumAsync(c => c.TotalMoney);
            if (amount != 0)
            {
                amount = Math.Round((amount / count).Value, 2);
            }
            result.Data = amount;
            return result;
        }

        /// <summary>
        /// 撤回
        /// </summary>
        /// <param name="blameBelongId"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> Revocation(int? blameBelongId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            Infrastructure.Response result = new Infrastructure.Response();
            var obj = await UnitWork.Find<BlameBelong>(c => c.Id == blameBelongId && c.Status == 2).Include(c => c.BlameBelongOrgs).FirstOrDefaultAsync();
            if (obj != null)
            {
                var check = obj.BlameBelongOrgs.Where(c => c.IsHistory != null && c.IsHistory == false).All(c=> c.IsHandle == false);
                if (check)
                {
                    var seleoh = await UnitWork.Find<BlameBelongHistory>(r => r.BlameBelongId == blameBelongId.Value).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = obj.FlowInstanceId });
                    await UnitWork.UpdateAsync<BlameBelong>(c => c.Id == blameBelongId, c => new BlameBelong
                    {
                        Status = 1
                    });
                    //记录操作日志
                    await UnitWork.AddAsync<BlameBelongHistory>(new BlameBelongHistory
                    {
                        Action = "撤回",
                        CreateUser = loginContext.User.Name,
                        CreateUserId = loginContext.User.Id,
                        CreateTime = DateTime.Now,
                        BlameBelongId = blameBelongId.Value,
                        ApprovalResult = "撤回",
                        IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh?.CreateTime)).TotalSeconds)
                    });
                    await UnitWork.SaveAsync();
                }
                else
                {
                    result.Code = 500;
                    result.Message = "责任部门已处理，不能撤回。";
                }
            }
            else {
                result.Code = 500;
                result.Message = "当前状态不能撤回。";
            }
            return result;
        }

        /// <summary>
        /// 单据是否已存在
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response<bool>> CheckAndeng(int id, int type)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            Infrastructure.Response<bool> response = new Infrastructure.Response<bool>();
            var isexist = await UnitWork.Find<BlameBelong>(c => c.DocType == type && c.OrderNo == id).AnyAsync();
            response.Result = isexist;
            return response;
        }

        /// <summary>
        /// 出纳收款
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task MakeCollection(List<AccraditationBlameBelongReq> req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            try
            {
                foreach (var item in req)
                {
                    var blameBelong = await UnitWork.Find<BlameBelong>(c => c.Id == item.Id).Include(c => c.BlameBelongOrgs).FirstOrDefaultAsync();
                    var flowinstace = await UnitWork.Find<FlowInstance>(c => c.Id == blameBelong.FlowInstanceId).FirstOrDefaultAsync();


                    var seleoh = await UnitWork.Find<BlameBelongHistory>(r => r.BlameBelongId.Equals(item.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    VerificationReq verificationReq = new VerificationReq
                    {
                        NodeRejectStep = "",
                        NodeRejectType = "0",
                        FlowInstanceId = flowinstace.Id,
                        VerificationFinally = "1",
                        VerificationOpinion = ""
                    };

                    if (flowinstace.ActivityName == "出纳")
                    {
                        await _flowInstanceApp.Verification(verificationReq);
                        await UnitWork.UpdateAsync<BlameBelong>(c => c.Id == blameBelong.Id, c => new BlameBelong
                        {
                            Status = 6
                        });

                        //记录操作日志
                        await UnitWork.AddAsync<BlameBelongHistory>(new BlameBelongHistory
                        {
                            Action = "出纳",
                            CreateUser = loginContext.User.Name,
                            CreateUserId = loginContext.User.Id,
                            CreateTime = DateTime.Now,
                            BlameBelongId = item.Id.Value,
                            ApprovalResult = "同意",
                            IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds)
                        });
                        await UnitWork.SaveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("提交失败。" + ex.Message);
            }
        }
    }
}
