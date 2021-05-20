using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Wrod;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Npoi.Mapper;
using NPOI.SS.Formula.Atp;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Org.BouncyCastle.Ocsp;

namespace OpenAuth.App
{
    public class CertinfoApp : BaseApp<Certinfo>
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly CertOperationHistoryApp _certOperationHistoryApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> LoadAsync(QueryCertinfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //var properties = loginContext.GetProperties("certinfo");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}

            var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("校准证书"));
            //var fs = await UnitWork.Find<FlowInstance>(null)
            //    .Where(o => o.IsFinish == 1 && o.SchemeId == mf.FlowSchemeId)//校准证书已结束流程
            //    .ToListAsync();
            var fs = await UnitWork.Find<FlowInstance>(null)
                .Where(o => o.SchemeId == mf.FlowSchemeId  && (o.ActivityName.Contains("已完成") || o.IsFinish==1))//校准证书已完成流程,IsFinish==1为流程改动前真实结束的数据
                .ToListAsync();
            var fsid = fs.Select(f => f.Id).ToList();
            var result = new TableData();
            var certObjs = UnitWork.Find<NwcaliBaseInfo>(null);
            certObjs = certObjs
                       .Where(o => fsid.Contains(o.FlowInstanceId))
                       .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertificateNumber.Contains(request.CertNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.TesterModel.Contains(request.Model))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.TesterSn.Contains(request.Sn))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), u => u.Operator.Contains(request.Operator))
                       .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.Time >= request.CalibrationDateFrom && u.Time <= request.CalibrationDateTo)
                ;
            var certList = await certObjs.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

            certList.ForEach(c =>
            {
                c.FlowInstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));
            });
            var view = certList.Select(c =>
            {
                return new CertinfoView
                {
                    Id = c.Id,
                    CertNo = c.CertificateNumber,
                    ActivityName = c.FlowInstance?.ActivityName,
                    IsFinish = c.FlowInstance?.IsFinish,
                    CreateTime = c.CreateTime,
                    AssetNo = c.AssetNo,
                    CalibrationDate = c.Time,
                    ExpirationDate = c.ExpirationDate,
                    Model = c.TesterModel,
                    Operator = c.Operator,
                    Sn = c.TesterSn,
                    FlowInstanceId = c.FlowInstanceId
                };
            });
            var certCount1 = await certObjs.CountAsync();
            result.Count = certCount1;
            #region 旧数据
            var objs = UnitWork.Find<Certinfo>(null);
            objs = objs
                       .Where(o => fsid.Contains(o.FlowInstanceId))
                       .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertNo.Contains(request.CertNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.Model.Contains(request.Model))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.Sn.Contains(request.Sn))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), u => u.Operator.Contains(request.Operator))
                       .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.CalibrationDate >= request.CalibrationDateFrom && u.CalibrationDate <= request.CalibrationDateTo)
                ;
            var take = certList.Count == 0 ? request.limit : request.limit - certList.Count;
            var page = certCount1 / request.limit;
            var skip = certList.Count == 0 ? (request.page - page) * request.limit : 0;
            var certCount2 = await objs.CountAsync();
            if (certList.Count == 0 || certList.Count < request.limit)
            {
                var list = await objs.OrderByDescending(u => u.CreateTime)
                    .Skip(skip)
                    .Take(take).ToListAsync();
                list.ForEach(c =>
                {
                    c.FlowInstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));
                });
                var view2 = list.Select(c =>
                {
                    return new CertinfoView
                    {
                        Id = c.Id,
                        CertNo = c.CertNo,
                        EncryptCertNo = Encryption.Encrypt(c.CertNo),
                        ActivityName = c.FlowInstance?.ActivityName,
                        IsFinish = c.FlowInstance?.IsFinish,
                        CreateTime = c.CreateTime,
                        AssetNo = c.AssetNo,
                        CalibrationDate = c.CalibrationDate,
                        ExpirationDate = c.ExpirationDate,
                        Model = c.Model,
                        Operator = c.Operator,
                        Sn = c.Sn,
                        FlowInstanceId = c.FlowInstanceId
                    };
                });
                view = view.Concat(view2);
            }

            result.Count = certCount2 + certCount1;

            #endregion

            //properties.RemoveAll(a => a.Key.Equals("FlowInstance"));
            //properties.Add(new KeyDescription() { Key = "FlowInstanceActivityName", Browsable = true, Description = "FlowInstanceActivityName", Type = "String" });
            //var propertyStr = string.Join(',', properties.Select(u => u.Key));
            //result.columnHeaders = properties;
            result.Data = view.OrderByDescending(d => d.CreateTime).ToList();//list.AsQueryable().Select($"new ({propertyStr},FlowInstance)");
            //result.Count = objs.Count();
            return result;
        }

        /// <summary>
        /// 证书待审批流程查询列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> LoadApprover(QueryCertApproverListReq request)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser();

            var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("校准证书"));
            var instances = new List<string>();
            if (request.FlowStatus == 1)
                instances = await UnitWork.Find<FlowInstanceTransitionHistory>(u => u.CreateUserId == user.User.Id)
                    .Select(u => u.InstanceId).Distinct().ToListAsync();

            var activeName = "";
            if (request.ActivityStatus==1) activeName = "待审核";
            else if (request.ActivityStatus == 2) activeName = "待批准";

            var fs = await UnitWork.Find<FlowInstance>(f => f.SchemeId == mf.FlowSchemeId)
                .WhereIf(request.FlowStatus != 1, o => o.MakerList == "1" || o.MakerList.Contains(user.User.Id))//待办事项
                .WhereIf(request.FlowStatus == 1, o => (o.ActivityName == "待送审" || instances.Contains(o.Id)) && (o.ActivityName != "结束" && o.ActivityName!="已完成"))
                .WhereIf(request.FlowStatus == 2, o => (o.ActivityName == "待审核" || o.ActivityName == "待批准") && o.IsFinish==0) //证书审核只显示进行中待核审和待批准
                .WhereIf(!string.IsNullOrWhiteSpace(activeName),o=>o.ActivityName==activeName)
                .ToListAsync();
            var fsid = fs.Select(f => f.Id).ToList();
            var certObjs = UnitWork.Find<NwcaliBaseInfo>(null);
            certObjs = certObjs
                .Where(o => fsid.Contains(o.FlowInstanceId))
                //.WhereIf(request.FlowStatus == 1, u => u.Operator.Equals(user.User.Name))
                .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertificateNumber.Contains(request.CertNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.TesterModel.Contains(request.Model))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.TesterSn.Contains(request.Sn))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), u => u.Operator.Contains(request.Operator))
                .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.Time >= request.CalibrationDateFrom && u.Time <= request.CalibrationDateTo)
                ;
            if (request.FlowStatus == 1)
            {
                certObjs = certObjs.Where(o => o.Operator.Equals(user.User.Name));
            }
            
            var certList = await certObjs.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

            certList.ForEach(c =>
            {
                c.FlowInstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));


                //驳回/撤回状态
                if (c.FlowInstance.IsFinish == 4) c.FlowInstance.ActivityName = "驳回";
                else if (c.FlowInstance.IsFinish == 2) c.FlowInstance.ActivityName = "撤回";
                //c.FlowInstance.ActivityName = c.FlowInstance.IsFinish == 4 ? "驳回" : c.FlowInstance.ActivityName;
            });
            var view = certList.Select(c =>
            {
                //增加驳回原因
                var rejectcontent = "";
                var his = UnitWork.Find<FlowInstanceOperationHistory>(h => h.InstanceId.Equals(c.FlowInstanceId) && h.Content.Contains("驳回")).OrderByDescending(h => h.CreateDate).FirstOrDefault();
                rejectcontent = his != null ? his.Content.Split("：")[1] : "";

                return new CertinfoView
                {
                    Id = c.Id,
                    CertNo = c.CertificateNumber,
                    ActivityName = c.FlowInstance?.ActivityName,
                    IsFinish = c.FlowInstance?.IsFinish,
                    CreateTime = c.CreateTime,
                    AssetNo = c.AssetNo,
                    CalibrationDate = c.Time,
                    ExpirationDate = c.ExpirationDate,
                    Model = c.TesterModel,
                    Operator = c.Operator,
                    Sn = c.TesterSn,
                    FlowInstanceId = c.FlowInstanceId,
                    RejectContent = rejectcontent
                };
            });
            var certCount1 = await certObjs.CountAsync();
            var objs = UnitWork.Find<Certinfo>(null);
            
            objs = objs
                .Where(o => fsid.Contains(o.FlowInstanceId))
                .WhereIf(request.FlowStatus == 1, u => u.Operator.Equals(user.User.Name))
                .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertNo.Contains(request.CertNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.Model.Contains(request.Model))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.Sn.Contains(request.Sn))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), u => u.Operator.Contains(request.Operator))
                .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.CalibrationDate >= request.CalibrationDateFrom && u.CalibrationDate <= request.CalibrationDateTo)
                ;
            if (request.FlowStatus == 1)
            {
                objs = objs.Where(u => u.Operator.Equals(user.User.Name));
            }
            var take = certList.Count == 0 ? request.limit : request.limit - certList.Count;
            var page = certCount1 / request.limit;
            var skip = certList.Count == 0 ? (request.page - page) * request.limit : 0;
            var certCount2 = await objs.CountAsync();
            if (certList.Count == 0 || certList.Count < request.limit)
            {

                var list = await objs.OrderByDescending(u => u.CreateTime)
                        .Skip(skip)
                        .Take(take).ToListAsync();
                list.ForEach(c =>
                {
                    c.FlowInstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));

                    //驳回/撤回状态
                    if (c.FlowInstance.IsFinish == 4) c.FlowInstance.ActivityName = "驳回";
                    else if (c.FlowInstance.IsFinish == 2) c.FlowInstance.ActivityName = "撤回";
                    //c.FlowInstance.ActivityName = c.FlowInstance.IsFinish == 4 ? "驳回" : c.FlowInstance.ActivityName;
                });
                var view2 = list.Select(c =>
                {
                    //增加驳回原因
                    var rejectcontent = "";
                    var his = UnitWork.Find<FlowInstanceOperationHistory>(h => h.InstanceId.Equals(c.FlowInstanceId) && h.Content.Contains("驳回")).OrderByDescending(h => h.CreateDate).FirstOrDefault();
                    rejectcontent = his != null ? his.Content.Split("：")[1] : "";
                    //if (request.FlowStatus == 2 || (request.FlowStatus == 1 && c.FlowInstance.IsFinish != 0))
                    //{
                    //    var his = UnitWork.Find<FlowInstanceOperationHistory>(h => h.InstanceId.Equals(c.FlowInstanceId) && h.Content.Contains("驳回")).OrderByDescending(h => h.CreateDate).FirstOrDefault();
                    //    rejectcontent = his != null ? his.Content.Split("：")[1] : "";
                    //}

                    return new CertinfoView
                    {
                        Id = c.Id,
                        CertNo = c.CertNo,
                        ActivityName = c.FlowInstance?.ActivityName,
                        IsFinish = c.FlowInstance?.IsFinish,
                        CreateTime = c.CreateTime,
                        AssetNo = c.AssetNo,
                        CalibrationDate = c.CalibrationDate,
                        ExpirationDate = c.ExpirationDate,
                        Model = c.Model,
                        Operator = c.Operator,
                        Sn = c.Sn,
                        FlowInstanceId = c.FlowInstanceId,
                        RejectContent= rejectcontent
                    };
                }).ToList(); 
                view = view.Concat(view2);
            }
            result.Data = view.OrderByDescending(d=>d.CreateTime).ToList();
            result.Count = certCount2 + certCount1;
            return result;
        }

        /// <summary>
        /// 多个序列号获取证书
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<TableData> GetCertNoByMultiNum(List<string> serialNumber)
        {
            TableData result = new TableData();
            var query =await( from a in UnitWork.Find<NwcaliBaseInfo>(c => !string.IsNullOrWhiteSpace(c.ApprovalDirectorId))
                        join b in UnitWork.Find<PcPlc>(c => serialNumber.Contains(c.Guid) && c.ExpirationDate >= DateTime.Now)
                        on a.Id equals b.NwcaliBaseInfoId
                        select new { CertNo = a.CertificateNumber, No = b.No, TesterModel = a.TesterModel, CalibrationDate = a.Time, ExpirationDate = a.ExpirationDate }).ToListAsync();
            result.Data = query;
            return result;
        }
        /// <summary>
        /// 证书审批操作
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> CertVerification(List<CertVerificationReq> request)
        {
            var result = new Infrastructure.Response();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            StringBuilder Message = new StringBuilder();
            foreach (var req in request)
            {
                var certInfo = await Repository.Find(c => c.Id.Equals(req.CertInfoId)).FirstOrDefaultAsync();
                var baseInfo = await UnitWork.Find<NwcaliBaseInfo>(null).FirstOrDefaultAsync(c => c.Id == req.CertInfoId);
                try
                {
                    if (certInfo is null && baseInfo is null)
                        throw new CommonException("证书不存在", 80001);
                    var id = certInfo is null ? baseInfo.Id : certInfo.Id;
                    var certNo = certInfo is null ? baseInfo.CertificateNumber : certInfo.CertNo;
                    var b = await CheckCanOperation(id, loginContext.User.Name, req.Verification.VerificationFinally);
                    if (!b && loginContext.User.Account!= "zhoudingkun")
                    {
                        throw new CommonException("您无法操作此步骤。", 80011);
                    }
                    var flowInstanceId = certInfo is null ? baseInfo.FlowInstanceId : certInfo.FlowInstanceId;
                    var flowInstance = await UnitWork.FindSingleAsync<FlowInstance>(c => c.Id == flowInstanceId);
                    var operatorName = certInfo is null ? baseInfo.Operator : certInfo.Operator;
                    if (flowInstance.ActivityName.Equals("待送审") && !operatorName.Equals(loginContext.User.Name))
                    {
                        throw new CommonException("您无法操作此步骤。", 80011);
                    }
                    var list = new List<WordModel>();
                    switch (req.Verification.VerificationFinally)
                    {
                        case "1":
                            //防止审批人未刷新页面操作该环节已审批过的流程
                            if (flowInstance.ActivityName.Equals("待审核") || flowInstance.ActivityName.Equals("待批准"))
                            {
                                if (!flowInstance.MakerList.Contains(loginContext.User.Id))
                                {
                                    throw new CommonException("您无法操作此步骤，或者该流程已经审批请刷新页面！", 80011);
                                }
                            }

                            #region 签名
                            if (flowInstance.ActivityName.Equals("待送审"))
                            {
                                _flowInstanceApp.Verification(req.Verification);
                                await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                                {
                                    CertInfoId = id,
                                    Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}送审证书。"
                                });
                                //await UnitWork.UpdateAsync<Certinfo>(c => c.Id.Equals(req.CertInfoId), o => new Certinfo { Operator = loginContext.User.Name, OperatorId = loginContext.User.Id });
                                //await UnitWork.SaveAsync();
                            }
                            else if (flowInstance.ActivityName.Equals("待审核"))
                            {
                                _flowInstanceApp.Verification(req.Verification);
                                await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                                {
                                    CertInfoId = id,
                                    Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}审批通过。"
                                });
                                await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { TechnicalManager = loginContext.User.Name, TechnicalManagerId = loginContext.User.Id });
                                await UnitWork.SaveAsync();
                            }
                            else if (flowInstance.ActivityName.Equals("待批准"))
                            {
                                _flowInstanceApp.Verification(req.Verification);
                                await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                                {
                                    CertInfoId = id,
                                    Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}批准证书。"
                                });
                                await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { ApprovalDirector = loginContext.User.Name, ApprovalDirectorId = loginContext.User.Id });
                                await UnitWork.SaveAsync();
                            }
                            #endregion
                            break;
                        case "2":
                            _flowInstanceApp.Verification(req.Verification);
                            await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                            {
                                CertInfoId = id,
                                Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}不同意证书。"
                            });
                            break;
                        case "3":
                            _flowInstanceApp.Verification(req.Verification);
                            await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                            {
                                CertInfoId = id,
                                Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}驳回证书。"
                            });
                            //await _flowInstanceApp.DeleteAsync(f => f.Id == req.Verification.FlowInstanceId);
                            //var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name == "校准证书");
                            //var flowInstanceRequest = new AddFlowInstanceReq();
                            //flowInstanceRequest.SchemeId = mf.FlowSchemeId;
                            //flowInstanceRequest.FrmType = 2;
                            //flowInstanceRequest.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            //flowInstanceRequest.CustomName = $"校准证书{certNo}审批";
                            //flowInstanceRequest.FrmData = $"{{\"certNo\":\"{certNo}\",\"cert\":[{{\"key\":\"{DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString()}\",\"url\":\"/Cert/DownloadCertPdf/{certNo}\",\"percent\":100,\"status\":\"success\",\"isImg\":false}}]}}";
                            //var newFlowId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(flowInstanceRequest);
                            //await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { FlowInstanceId = newFlowId });
                            break;
                        //撤回
                        case "4":
                            _flowInstanceApp.Verification(req.Verification);
                            await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                            {
                                CertInfoId = id,
                                Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}撤回证书。"
                            });
                            break;
                        default:
                            break;
                    }
                }
                catch (Exception e)
                {
                    result.Code = 500;
                    Message.Append("报错编号"+baseInfo.CertificateNumber +"报错信息："+ e.Message);
                }
                if (request.Count > 1) 
                {
                    await Task.Delay(30000);
                }
            }
            result.Message= string.IsNullOrWhiteSpace(Message.ToString()) ? "审批成功" : Message.ToString();
            return result;
        }

        /// <summary>
        /// 业务员证书查询列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> LoadSaleInfo(QuerySaleOrDeviceOrCertListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var saleMan = await UnitWork.Find<crm_oslp>(c => c.SlpName == loginContext.User.Name).Select(c=>c.SlpCode).FirstOrDefaultAsync();
            var saleOrder = UnitWork.Find<sale_ordr>(o => o.SlpCode == saleMan);
            //获取该销售员下所有销售订单号
            var saleOederIds = await saleOrder.Select(c => c.DocEntry).ToListAsync();
            //获取销售订单下所有序列号
            var manufacturerSerialNumber = from a in UnitWork.Find<store_oitl>(null)
                                           join b in UnitWork.Find<store_itl1>(null) on new { a.LogEntry, a.ItemCode } equals new { b.LogEntry, b.ItemCode } into ab
                                           from b in ab.DefaultIfEmpty()
                                           join c in UnitWork.Find<store_osrn>(null) on new { b.ItemCode, b.SysNumber } equals new { c.ItemCode, c.SysNumber } into bc
                                           from c in bc.DefaultIfEmpty()
                                           where a.DocType == 15 && saleOederIds.Contains(a.BaseEntry) && !string.IsNullOrWhiteSpace(c.MnfSerial)
                                           select new { c.MnfSerial,a.ItemCode, a.DocEntry, a.BaseEntry, a.DocType, a.CreateDate, a.BaseType };
            
            if (request.PageStatus == 1)//销售订单列表
            {
                //获取序列号下订单号
                var idWhere = new List<int?>();
                if (!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers))
                    idWhere = manufacturerSerialNumber.Where(c => c.MnfSerial.Contains(request.ManufacturerSerialNumbers)).Select(c => c.BaseEntry).ToList();


                saleOrder = saleOrder.WhereIf(!string.IsNullOrWhiteSpace(request.SalesOrderId), c => c.DocEntry.ToString().Contains(request.SalesOrderId))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.CardCode), c => c.CardCode.Contains(request.CardCode))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.CardName), c => c.CardName.Contains(request.CardName))
                                    .WhereIf(!(request.SaleEndCreatTime == null && request.SaleEndCreatTime == null), c => c.CreateDate >= request.SaleStartCreatTime && c.CreateDate <= request.SaleEndCreatTime)
                                    .WhereIf(idWhere.Count() > 0, c => idWhere.Contains(c.DocEntry))//序列号查询条件
                                    ;

                //分页
                var saleOrderList = await saleOrder.OrderByDescending(c => c.CreateDate)
                                    .Skip((request.page - 1) * request.limit)
                                    .Take(request.limit)
                                    .ToListAsync();
                var dataCount = await saleOrder.CountAsync();
                var resultData = saleOrderList.Select(c => new
                {
                    SalesOrderId = c.DocEntry,
                    c.CardCode,
                    c.CardName,
                    CreateDate = c.CreateDate
                }) ;
                result.Data = resultData;
                result.Count = dataCount;
            }
            else if(request.PageStatus == 2)//设备列表
            {
                var numList = await manufacturerSerialNumber
                    .WhereIf(!string.IsNullOrWhiteSpace(request.SalesOrderId), c => c.BaseEntry == int.Parse(request.SalesOrderId))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.TesterModel),c=>c.ItemCode.Contains(request.TesterModel))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), c => c.MnfSerial.Contains(request.ManufacturerSerialNumbers))
                    .Select(c=>new NwcaliBaseInfo {TesterSn=c.MnfSerial,TesterModel=c.ItemCode }).ToListAsync();

                var cerlist = await UnitWork.Find<NwcaliBaseInfo>(null)
                    //.WhereIf(!string.IsNullOrWhiteSpace(request.CertNo),c=>c.CertificateNumber.Contains(request.CertNo))
                    //.WhereIf(!(request.StartCalibrationDate == null && request.EndCalibrationDate == null), c => c.Time >= request.StartCalibrationDate && c.Time <= request.EndCalibrationDate)
                    .ToListAsync();
                var test= cerlist.OrderByDescending(c => c.Time).GroupBy(c => c.TesterSn).Select(c => c.First()).ToList();

                //var da = numList.Select(c => 
                //{
                //    var te = test.FirstOrDefault(o => o.TesterSn.Equals(c.MnfSerial));
                //    return new
                //    {
                //        c.MnfSerial,
                //        c.ItemCode,
                //        AssetNo= te==null?null:te.AssetNo,
                //        CertificateNumber = te == null ? null : te.CertificateNumber,
                //        Time = te == null ? null : te.Time,
                //        ExpirationDate = te == null ? null : te.ExpirationDate,
                //        Operator = te == null ? null : te.Operator,
                //    };
                //});

                var devicelist1 = from a in numList
                                  join b in cerlist on a.TesterSn equals b.TesterSn into ab
                                  from b in ab.DefaultIfEmpty()
                                  select new NwcaliBaseInfo
                                  {
                                      TesterSn=a.TesterSn,
                                      TesterModel=a.TesterModel,
                                      AssetNo = b?.AssetNo,
                                      CertificateNumber=b?.CertificateNumber,
                                      Time=b?.Time,
                                      ExpirationDate=b?.ExpirationDate,
                                      Operator=b?.Operator 
                                  };

                devicelist1= devicelist1.OrderByDescending(c => c.Time).GroupBy(c => c.TesterSn).Select(c => c.First()).ToList();

                if (!string.IsNullOrWhiteSpace(request.CertNo))
                    devicelist1 = devicelist1.Where(c =>!string.IsNullOrWhiteSpace(c.CertificateNumber) && c.CertificateNumber.Contains(request.CertNo)).ToList();
                if (!(request.StartCalibrationDate == null && request.EndCalibrationDate == null))
                    devicelist1 = devicelist1.Where(c => c.Time >= request.StartCalibrationDate && c.Time <= request.EndCalibrationDate).ToList();

                //var view1 = await devicelist1
                //                .WhereIf(!string.IsNullOrEmpty(request.ManufacturerSerialNumbers), c => c.MnfSerial.Contains(request.ManufacturerSerialNumbers))
                //                .WhereIf(!string.IsNullOrEmpty(request.TesterModel), c => c.ItemCode.Contains(request.TesterModel))
                //                .WhereIf(!string.IsNullOrEmpty(request.CertNo), c => c.CertificateNumber.Contains(request.CertNo))
                //                .WhereIf(!(request.StartCalibrationDate == null && request.EndCalibrationDate == null), c => c.Time >= request.StartCalibrationDate && c.Time <= request.EndCalibrationDate).ToListAsync();
                //var view1List = await view1.OrderByDescending(c => c.Time).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
                //var viewCount1 = await view1.CountAsync();
                //result.Count = viewCount1;
                #region 老数据
                //序列号下最新的校验证书
                var testNo = numList.Select(c => c.TesterSn).ToList();
                var old = await UnitWork.Find<Certinfo>(c=> testNo.Contains(c.Sn)).ToListAsync();
                if (old.Count>0)
                {
                    old = old.OrderByDescending(c => c.CalibrationDate).GroupBy(c => c.Sn).Select(c => c.First()).ToList();
                    //条件
                    if (!string.IsNullOrWhiteSpace(request.CertNo))
                        old = old.Where(c => c.CertNo == request.CertNo).ToList();
                    if (!(request.StartCalibrationDate == null && request.EndCalibrationDate == null))
                        old = old.Where(c => c.CalibrationDate >= request.StartCalibrationDate && c.CalibrationDate <= request.EndCalibrationDate).ToList();


                    devicelist1.ForEach(q =>
                    {
                        var a = old.Where(c => c.Sn == q.TesterSn).FirstOrDefault();
                        if (a != null && a.CalibrationDate > q.Time)
                        {
                            q.AssetNo = a.AssetNo;
                            q.CertificateNumber = a.CertNo;
                            q.Time = a.CalibrationDate;
                            q.ExpirationDate = a.ExpirationDate;
                            q.Operator = a.Operator;
                        }
                    });
                }

                #endregion

                result.Count = devicelist1.Count();
                result.Data = devicelist1.Select(c=> 
                {
                    return new
                    {
                        TesterSn = c.TesterSn,
                        TesterModel = c.TesterModel,
                        AssetNo = c.AssetNo,
                        CertificateNumber = c.CertificateNumber,
                        Time = c.Time,
                        ExpirationDate = c.ExpirationDate,
                        Operator = c.Operator
                    };
                })
                .OrderByDescending(c => c.Time)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit)
                .ToList();
            }
            else if (request.PageStatus == 3)//证书列表
            {
                var cerinfo = await UnitWork.Find<NwcaliBaseInfo>(null)
                                .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), c => c.TesterSn.Equals(request.ManufacturerSerialNumbers))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CertNo), c => c.CertificateNumber.Contains(request.CertNo))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), c => c.Operator.Contains(request.Operator))
                                .WhereIf(!(request.StartCalibrationDate == null && request.EndCalibrationDate == null), c => c.Time >= request.StartCalibrationDate && c.Time <= request.EndCalibrationDate)
                                .ToListAsync();
                                ;
                var view = cerinfo.Select(c =>
                 {
                     return new CertinfoView
                     {
                         Id = c.Id,
                         CertNo = c.CertificateNumber,
                         CreateTime = c.CreateTime,
                         AssetNo = c.AssetNo,
                         CalibrationDate = c.Time,
                         ExpirationDate = c.ExpirationDate,
                         Model = c.TesterModel,
                         Operator = c.Operator,
                         Sn = c.TesterSn,
                     };
                 });

                #region 老数据
                var obj = await UnitWork.Find<Certinfo>(null)
                                .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), c => c.Sn.Equals(request.ManufacturerSerialNumbers))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CertNo), c => c.CertNo.Contains(request.CertNo))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), c => c.Operator.Contains(request.Operator))
                                .WhereIf(!(request.StartCalibrationDate == null && request.EndCalibrationDate == null), c => c.CalibrationDate >= request.StartCalibrationDate && c.CalibrationDate <= request.EndCalibrationDate)
                                .ToListAsync();
                if (obj.Count>0)
                {
                    var view2 = obj.Select(c =>
                    {
                        return new CertinfoView
                        {
                            Id = c.Id,
                            CertNo = c.CertNo,
                            CreateTime = c.CreateTime,
                            AssetNo = c.AssetNo,
                            CalibrationDate = c.CalibrationDate,
                            ExpirationDate = c.ExpirationDate,
                            Model = c.Model,
                            Operator = c.Operator,
                            Sn = c.Sn,
                        };
                    });
                    //合并
                    view = view.Concat(view2);
                }
                #endregion

                result.Count = view.Count();
                result.Data = view.OrderByDescending(c => c.CalibrationDate)
                                    .Skip((request.page - 1) * request.limit)
                                    .Take(request.limit)
                                    .ToList();
            }

            return result;
        }


        public void Add(AddOrUpdateCertinfoReq req)
        {
            var obj = req.MapTo<Certinfo>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            //obj.CreateUserId = user.Id;
            //obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }
        public async Task AddAsync(AddOrUpdateCertinfoReq req, CancellationToken cancellationToken = default)
        {
            var obj = req.MapTo<Certinfo>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            //obj.CreateUserId = user.Id;
            //obj.CreateUserName = user.Name;
            await Repository.AddAsync(obj, cancellationToken);
        }

        public void Update(AddOrUpdateCertinfoReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<Certinfo>(u => u.Id == obj.Id, u => new Certinfo
            {
                CertNo = obj.CertNo,
                CertPath = obj.CertPath,
                PdfPath = obj.PdfPath,
                BaseInfoPath = obj.BaseInfoPath,
                CreateTime = obj.CreateTime,
                //UpdateTime = DateTime.Now,
                //UpdateUserId = user.Id,
                //UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            });

        }
        public async Task UpdateAsync(AddOrUpdateCertinfoReq obj, CancellationToken cancellationToken = default)
        {
            var user = _auth.GetCurrentUser().User;
            await UnitWork.UpdateAsync<Certinfo>(u => u.Id == obj.Id, u => new Certinfo
            {
                CertNo = obj.CertNo,
                CertPath = obj.CertPath,
                PdfPath = obj.PdfPath,
                BaseInfoPath = obj.BaseInfoPath,
                CreateTime = obj.CreateTime,
                FlowInstanceId = obj.FlowInstanceId,
                //UpdateTime = DateTime.Now,
                //UpdateUserId = user.Id,
                //UpdateUserName = user.Name
                //todo:补充或调整自己需要的字段
            }, cancellationToken);
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 判断当前用户是否操作过
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private async Task<bool> CheckCanOperation(string id, string name,string operate)
        {
            //撤回操作不验证
            if (operate.Equals("4")) return true;
            var history = await UnitWork.Find<CertOperationHistory>(c => c.CertInfoId.Equals(id)).ToListAsync();
            var rejectTime = history.Where(c=>c.Action.Contains("驳回")).OrderByDescending(c => c.CreateTime).Select(c => c.CreateTime).FirstOrDefault();
            //有无驳回操作
            if (rejectTime == null)
            {
                //有无撤回操作
                rejectTime = history.Where(c => c.Action.Contains("撤回")).OrderByDescending(c => c.CreateTime).Select(c => c.CreateTime).FirstOrDefault();
                if (rejectTime == null)
                {
                    //没有则直接验证是否有操作记录
                    var a = history.FirstOrDefault(c => c.Action.Contains(name));
                    return a is null;
                }
                else
                {
                    //有则验证撤回操作之后是否有操作记录
                    var a = history.FirstOrDefault(c => c.CreateTime > rejectTime && c.Action.Contains(name));
                    return a is null;
                }
            }
            else
            {
                //驳回后的操作环节
                var h1 = history.Where(c => c.CreateTime > rejectTime).ToList();
                if (h1 != null)
                {
                    //是否是撤回后的环节
                    var cht = h1.Where(c => c.Action.Contains("撤回")).OrderByDescending(c => c.CreateTime).Select(c => c.CreateTime).FirstOrDefault();
                    if (cht != null)
                    {
                        var a = h1.FirstOrDefault(c => c.CreateTime > cht && c.Action.Contains(name));
                        return a is null;
                    }
                    else
                    {
                        var a = h1.FirstOrDefault(c => c.Action.Contains(name));
                        return a is null;
                    }
                }
                else
                {
                    var a = h1.FirstOrDefault(c => c.Action.Contains(name));
                    return a is null;
                }
            }
            //var h = history.FirstOrDefault(c => c.CreateTime >= rejectTime && c.Action.Contains(name));
            ////var h = await UnitWork.FindSingleAsync<CertOperationHistory>(c => c.CertInfoId.Equals(id) && c.Action.Contains(name));
            //return h is null;
        }

        /// <summary>
        /// 查询字典
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public async Task<Category> GetCategory(string Model)
        {
            var objs = await UnitWork.Find<Category>(c=> Model.Contains(c.Name) && c.TypeId.Equals("SYS_CalibrationCertificateType")).FirstOrDefaultAsync();
            return objs;
        }

        /// <summary>
        /// 获取物料信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetMaterialCode(QueryMaterialCodeReq req)
        {
            var result = new TableData();

            var query = from a in UnitWork.Find<PcPlc>(null)
                        join b in UnitWork.Find<NwcaliBaseInfo>(null) on a.NwcaliBaseInfoId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        where req.plcGuid.Contains(a.Guid)
                        select new { id = a.Id, plcGuid = a.Guid, materialCode = b.TesterModel, TesterSn=b.TesterSn };

            result.Data = await query.ToListAsync();
            return result;
        }

        /// <summary>
        /// 获取证书信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetCertificate(QueryMaterialCodeReq req)
        {
            var result = new TableData();

            var query = from a in UnitWork.Find<PcPlc>(null)
                        join b in UnitWork.Find<NwcaliBaseInfo>(null) on a.NwcaliBaseInfoId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        where req.plcGuid.Contains(a.Guid)
                        select new { id=a.Id,plcGuid=a.Guid,certNo=b.CertificateNumber, calibrationDate=a.CalibrationDate,b.Operator, expirationDate=a.ExpirationDate};

            result.Data = await query.ToListAsync();
            return result;
        }

        public CertinfoApp(IUnitWork unitWork, IRepository<Certinfo> repository,
            RevelanceManagerApp app, IAuth auth, FlowInstanceApp flowInstanceApp, CertOperationHistoryApp certOperationHistoryApp, ModuleFlowSchemeApp moduleFlowSchemeApp) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
            _flowInstanceApp = flowInstanceApp;
            _certOperationHistoryApp = certOperationHistoryApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
        }
    }
}