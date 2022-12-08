using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DinkToPdf;
using Infrastructure;
using Infrastructure.Export;
using Infrastructure.Extensions;
using Infrastructure.Wrod;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Npoi.Mapper;
using NPOI.SS.Formula.Atp;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali;
using OpenAuth.App.Nwcali.Models;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Org.BouncyCastle.Ocsp;
using NSAP.Entity;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository;
using System.Text.RegularExpressions;
using Infrastructure.Excel;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Nwcali.Response;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Extensions.Options;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using OpenAuth.App.DDVoice.Common;
using Microsoft.Extensions.Logging;

namespace OpenAuth.App
{
    public class CertinfoApp : BaseApp<Certinfo>
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly CertOperationHistoryApp _certOperationHistoryApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly NwcaliCertApp _nwcaliCertApp;
        private readonly FileApp _fileApp;
        private readonly UserSignApp _userSignApp;
        private readonly ServiceOrderApp _serviceOrderApp;
        private readonly StepVersionApp _stepVersionApp;
        private readonly IOptions<AppSetting> _appConfiguration;
        private static readonly string BaseCertDir = Path.Combine(Directory.GetCurrentDirectory(), "certs");
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        private ILogger<CertinfoApp> _logger;
        private DDSettingHelp _ddSettingHelp;
        private static readonly Dictionary<int, double> PoorCoefficients = new Dictionary<int, double>()
        {
            { 2, 1.13 },
            { 3, 1.69 },
            { 4, 2.06 },
            { 5, 2.33 }
        };
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
                .Where(o => o.SchemeId == mf.FlowSchemeId && (o.ActivityName.Contains("已完成") || o.IsFinish == 1))//校准证书已完成流程,IsFinish==1为流程改动前真实结束的数据
                .Select(o => new { o.Id, o.ActivityName, o.IsFinish })
                .ToListAsync();
            var fsid = fs.Select(f => f.Id).ToList();
            var result = new TableData();
            var certObjs = UnitWork.Find<NwcaliBaseInfo>(null)
                       .Where(o => fsid.Contains(o.FlowInstanceId))
                       .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertificateNumber.Contains(request.CertNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.TesterModel.Contains(request.Model))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.TesterSn.Contains(request.Sn))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), u => u.Operator.Contains(request.Operator))
                       .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.Time >= request.CalibrationDateFrom && u.Time <= request.CalibrationDateTo);
            ;
            var certList = await certObjs.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

            var view = certList.Select(c =>
            {
                var flowinstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));
                return new CertinfoView
                {
                    Id = c.Id,
                    CertNo = c.CertificateNumber,
                    ActivityName = flowinstance?.ActivityName,
                    IsFinish = flowinstance?.IsFinish,
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
            var objs = UnitWork.Find<Certinfo>(null)
                        .Where(o => fsid.Contains(o.FlowInstanceId))
                       .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertNo.Contains(request.CertNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.Model.Contains(request.Model))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.Sn.Contains(request.Sn))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), u => u.Operator.Contains(request.Operator))
                       .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.CalibrationDate >= request.CalibrationDateFrom && u.CalibrationDate <= request.CalibrationDateTo);

            var take = certList.Count == 0 ? request.limit : request.limit - certList.Count;
            var page = certCount1 / request.limit;
            var skip = certList.Count == 0 ? (request.page - page) * request.limit : 0;
            var certCount2 = await objs.CountAsync();
            if (certList.Count == 0 || certList.Count < request.limit)
            {
                var list = await objs.OrderByDescending(u => u.CreateTime)
                    .Skip(skip)
                    .Take(take).ToListAsync();

                var view2 = list.Select(c =>
                {
                    var flowinstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));
                    return new CertinfoView
                    {
                        Id = c.Id,
                        CertNo = c.CertNo,
                        EncryptCertNo = Encryption.Encrypt(c.CertNo),
                        ActivityName = flowinstance?.ActivityName,
                        IsFinish = flowinstance?.IsFinish,
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
            if (request.ActivityStatus == 1) activeName = "待审核";
            else if (request.ActivityStatus == 2) activeName = "待批准";

            var fs = await UnitWork.Find<FlowInstance>(f => f.SchemeId == mf.FlowSchemeId)
                .WhereIf(request.FlowStatus != 1, o => o.MakerList == "1" || o.MakerList.Contains(user.User.Id))//待办事项
                .WhereIf(request.FlowStatus == 1, o => (o.ActivityName == "待送审" || instances.Contains(o.Id)) && (o.ActivityName != "结束" && o.ActivityName != "已完成"))
                .WhereIf(request.FlowStatus == 2, o => (o.ActivityName == "待审核" || o.ActivityName == "待批准") && o.IsFinish == 0) //证书审核只显示进行中待核审和待批准
                .WhereIf(!string.IsNullOrWhiteSpace(activeName), o => o.ActivityName == activeName)
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
                .Where(c => c.CalibrationStatus != "NG")
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
                else if (c.FlowInstance.IsFinish == -1) c.FlowInstance.ActivityName = "撤回";
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
                    RejectContent = rejectcontent,
                    Issuer = !string.IsNullOrWhiteSpace(c.Issuer) ? c.Issuer : c.Operator
                };
            });
            var certCount1 = await certObjs.CountAsync();
            #region Certinfo已废弃

            //var objs = UnitWork.Find<Certinfo>(null);

            //objs = objs
            //    .Where(o => fsid.Contains(o.FlowInstanceId))
            //    .WhereIf(request.FlowStatus == 1, u => u.Operator.Equals(user.User.Name))
            //    .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertNo.Contains(request.CertNo))
            //    .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
            //    .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.Model.Contains(request.Model))
            //    .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.Sn.Contains(request.Sn))
            //    .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), u => u.Operator.Contains(request.Operator))
            //    .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.CalibrationDate >= request.CalibrationDateFrom && u.CalibrationDate <= request.CalibrationDateTo);

            //if (request.FlowStatus == 1)
            //{
            //    objs = objs.Where(u => u.Operator.Equals(user.User.Name));
            //}
            //var take = certList.Count == 0 ? request.limit : request.limit - certList.Count;
            //var page = certCount1 / request.limit;
            //var skip = certList.Count == 0 ? (request.page - page) * request.limit : 0;
            //var certCount2 = await objs.CountAsync();
            //if (certList.Count == 0 || certList.Count < request.limit)
            //{

            //    var list = await objs.OrderByDescending(u => u.CreateTime)
            //            .Skip(skip)
            //            .Take(take).ToListAsync();
            //    list.ForEach(c =>
            //    {
            //        c.FlowInstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));

            //    //驳回/撤回状态
            //        if (c.FlowInstance.IsFinish == 4) c.FlowInstance.ActivityName = "驳回";
            //        else if (c.FlowInstance.IsFinish == -1) c.FlowInstance.ActivityName = "撤回";
            //    });
            //    var view2 = list.Select(c =>
            //    {
            //    //增加驳回原因
            //        var rejectcontent = "";
            //        var his = UnitWork.Find<FlowInstanceOperationHistory>(h => h.InstanceId.Equals(c.FlowInstanceId) && h.Content.Contains("驳回")).OrderByDescending(h => h.CreateDate).FirstOrDefault();
            //        rejectcontent = his != null ? his.Content.Split("：")[1] : "";
            //        return new CertinfoView
            //        {
            //            Id = c.Id,
            //            CertNo = c.CertNo,
            //            ActivityName = c.FlowInstance?.ActivityName,
            //            IsFinish = c.FlowInstance?.IsFinish,
            //            CreateTime = c.CreateTime,
            //            AssetNo = c.AssetNo,
            //            CalibrationDate = c.CalibrationDate,
            //            ExpirationDate = c.ExpirationDate,
            //            Model = c.Model,
            //            Operator = c.Operator,
            //            Sn = c.Sn,
            //            FlowInstanceId = c.FlowInstanceId,
            //            RejectContent = rejectcontent
            //        };
            //    }).ToList();
            //    view = view.Concat(view2);
            //}
            #endregion

            result.Data = view.OrderByDescending(d => d.CreateTime).ToList();
            result.Count = certCount1;
            return result;
        }

        /// <summary>
        /// 批量删除校准证书
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> DeleteCertinfo(List<string> ids)
        {
            var response = new Infrastructure.Response();

            //信息存储在两个表中(新：NwcaliBaseInfo，旧：Certinfo,页面显示的结果是两个表的并集,因此前端传过来的id两个表都有可能)
            var query1 = UnitWork.Find<NwcaliBaseInfo>(null).Where(x => ids.Contains(x.Id));

            var nwInfos = await query1.Select(x => new { x.Id, x.FlowInstanceId }).ToListAsync();
            //流程实例id
            var flowInstanceIds = nwInfos.Select(n => n.FlowInstanceId).ToList();

            using var tran = UnitWork.GetDbContext<NwcaliBaseInfo>().Database.BeginTransaction();
            //删除记录及删除字表和流程实例
            try
            {
                await UnitWork.DeleteAsync<NwcaliBaseInfo>(x => nwInfos.Select(n => n.Id).Contains(x.Id));
                await UnitWork.DeleteAsync<Etalon>(x => nwInfos.Select(n => n.Id).Contains(x.NwcaliBaseInfoId));
                await UnitWork.DeleteAsync<NwcaliPlcData>(x => nwInfos.Select(n => n.Id).Contains(x.NwcaliBaseInfoId));
                await UnitWork.DeleteAsync<Repository.Domain.NwcaliTur>(x => nwInfos.Select(n => n.Id).Contains(x.NwcaliBaseInfoId));
                await UnitWork.DeleteAsync<PcPlc>(x => nwInfos.Select(n => n.Id).Contains(x.NwcaliBaseInfoId));

                await UnitWork.SaveAsync();

                await tran.CommitAsync();

                //提交没问题后,删除流程实例
                await UnitWork.DeleteAsync<FlowInstance>(f => flowInstanceIds.Contains(f.Id));
                await UnitWork.SaveAsync();
            }
            catch (Exception ex)
            {
                await tran.RollbackAsync();

                response.Code = 500;
                response.Message = ex?.Message ?? ex?.InnerException?.Message ?? "";

                throw new Exception(ex?.Message ?? ex?.InnerException?.Message ?? "");
            }

            return response;
        }

        /// <summary>
        /// 多个序列号获取证书
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<TableData> GetCertNoByMultiNum(List<string> serialNumber)
        {
            TableData result = new TableData();
            var query = await (from a in UnitWork.Find<NwcaliBaseInfo>(c => !string.IsNullOrWhiteSpace(c.ApprovalDirectorId))
                               join b in UnitWork.Find<PcPlc>(c => serialNumber.Contains(c.Guid) && c.ExpirationDate >= DateTime.Now)
                               on a.Id equals b.NwcaliBaseInfoId
                               select new NwcailView { CertNo = a.CertificateNumber, No = b.No, TesterModel = a.TesterModel, CalibrationDate = a.Time, ExpirationDate = a.ExpirationDate }).ToListAsync();

            result.Data = query;
            var certNos = await UnitWork.Find<Certplc>(p => serialNumber.Contains(p.PlcGuid)).Select(c => c.CertNo).ToListAsync();
            if (certNos.Count > 0)
            {
                var nos = string.Join("','", certNos);
                var certinfo = await UnitWork.FromSql<Certinfo>($@"SELECT a.* from certinfo a JOIN erp4.flowinstance b on a.FlowInstanceId=b.Id
                                    where CertNo in ('{nos}') and ActivityName='结束'").Select(c => new NwcailView { CertNo = c.CertNo, No = null, TesterModel = c.Model, CalibrationDate = c.CalibrationDate, ExpirationDate = c.ExpirationDate }).ToListAsync();

                if (certinfo.Count > 0) result.Data = query.Concat(certinfo);
            }
            return result;
        }

        /// <summary>
        /// 单个guid获取证书
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<TableData> GetCertListBySingle(string serialNumber)
        {
            TableData result = new TableData();
            var query = await (from a in UnitWork.Find<NwcaliBaseInfo>(c => !string.IsNullOrWhiteSpace(c.ApprovalDirectorId))
                               join b in UnitWork.Find<PcPlc>(c => serialNumber == c.Guid)
                               on a.Id equals b.NwcaliBaseInfoId
                               select new NwcailView { CertNo = a.CertificateNumber, No = b.No, TesterModel = a.TesterModel, CalibrationDate = a.Time, ExpirationDate = a.ExpirationDate }).ToListAsync();

            result.Data = query;
            var certNos = await UnitWork.Find<Certplc>(p => serialNumber.Contains(p.PlcGuid)).Select(c => c.CertNo).ToListAsync();
            if (certNos.Count > 0)
            {
                var nos = string.Join("','", certNos);
                var certinfo = await UnitWork.FromSql<Certinfo>($@"SELECT a.* from certinfo a JOIN erp4.flowinstance b on a.FlowInstanceId=b.Id
                                    where CertNo in ('{nos}') and ActivityName='结束'").Select(c => new NwcailView { CertNo = c.CertNo, No = null, TesterModel = c.Model, CalibrationDate = c.CalibrationDate, ExpirationDate = c.ExpirationDate }).ToListAsync();

                if (certinfo.Count > 0) result.Data = query.Concat(certinfo);
            }
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
                    if (!b && loginContext.User.Account != "zhoudingkun")
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
                                await _flowInstanceApp.Verification(req.Verification);
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
                                await _flowInstanceApp.Verification(req.Verification);
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
                                await _flowInstanceApp.Verification(req.Verification);
                                await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                                {
                                    CertInfoId = id,
                                    Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}批准证书。"
                                });
                                await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { ApprovalDirector = loginContext.User.Name, ApprovalDirectorId = loginContext.User.Id });
                                await UnitWork.SaveAsync();
                                await CreateNwcailFile(certNo);
                            }
                            else if (flowInstance.ActivityName.Equals("开始") && flowInstance.IsFinish == -1)
                            {
                                await _flowInstanceApp.Verification(req.Verification);
                                await _flowInstanceApp.Verification(req.Verification);
                                await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                                {
                                    CertInfoId = id,
                                    Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}送审证书。"
                                });
                            }
                            #endregion
                            break;
                        case "2":
                            await _flowInstanceApp.Verification(req.Verification);
                            await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                            {
                                CertInfoId = id,
                                Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}不同意证书。"
                            });
                            break;
                        case "3":
                            await _flowInstanceApp.Verification(req.Verification);
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
                            //await _flowInstanceApp.Verification(req.Verification);
                            await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = req.Verification.FlowInstanceId, Description = "" });
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
                    Message.Append("报错编号" + baseInfo.CertificateNumber + "报错信息：" + e.Message);
                }
                if (request.Count > 1)
                {
                    //await Task.Delay(30000);
                }
            }
            result.Message = string.IsNullOrWhiteSpace(Message.ToString()) ? "审批成功" : Message.ToString();
            return result;
        }


        public async Task CreateNwcailFileHelper()
        {
            //var res = await UnitWork.Find<NwcaliBaseInfo>(c => !string.IsNullOrWhiteSpace(c.ApprovalDirectorId) && string.IsNullOrWhiteSpace(c.CNASPdfPath)).ToListAsync();
            //foreach (var item in res)
            //{
            //    await CreateNwcailFile(item.CertificateNumber);
            //}
            var res = await UnitWork.Find<NwcaliBaseInfo>(c => !string.IsNullOrWhiteSpace(c.ApprovalDirectorId) &&
            string.IsNullOrWhiteSpace(c.CNASPdfPath) && !c.TesterModel.Contains("V0")).OrderByDescending(c => c.Time).Take(1000).ToListAsync();
            foreach (var item in res)
            {
                await CreateNwcailFile(item.CertificateNumber);
            }
        }

        /// <summary>
        /// 批量生成证书文件
        /// </summary>
        /// <param name="certNo"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> BatchCreateNwcailFile(List<string> certNo)
        {
            foreach (var item in certNo)
            {
                await CreateNwcailFile(item);
            }
            return new Infrastructure.Response();
        }


        /// <summary>
        /// 保存证书pdf
        /// </summary>
        /// <param name="certNo"></param>
        /// <returns></returns>
        public async Task CreateNwcailFile(string certNo)
        {
            var baseInfo = await _nwcaliCertApp.GetInfo(certNo);
            if (baseInfo != null)
            {
                try
                {
                    var folderYear = DateTime.Now.ToString("yyyy");
                    var basePath = Path.Combine("D:\\nsap4file", "nwcail", folderYear, baseInfo.CertificateNumber);
                    //if (!string.IsNullOrEmpty(baseInfo.CertPath))
                    //{
                    //    basePath = baseInfo.CertPath.Substring(0, baseInfo.CertPath.LastIndexOf('\\'));
                    //}
                    var model = await BuildModel(baseInfo);
                    #region 生成英文版
                    var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Header.html");
                    var text = System.IO.File.ReadAllText(url);
                    text = text.Replace("@Model.Data.BarCode", model.BarCode);
                    var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                    System.IO.File.WriteAllText(tempUrl, text);
                    var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Footer.html");
                    var datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate.cshtml", pdf =>
                    {
                        pdf.IsWriteHtml = true;
                        pdf.PaperKind = PaperKind.A4;
                        pdf.Orientation = Orientation.Portrait;
                        pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                        pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 2.812, HtmUrl = footerUrl };
                    });
                    System.IO.File.Delete(tempUrl);
                    //DirUtil.CheckOrCreateDir(basePath);
                    //var fullpath = Path.Combine(basePath, $"{certNo}_EN" + ".pdf");
                    //using (FileStream fs = new FileStream(fullpath, FileMode.Create))
                    //{
                    //    using (BinaryWriter bw = new BinaryWriter(fs))
                    //    {
                    //        bw.Write(datas, 0, datas.Length);
                    //    }
                    //}
                    Stream stream1 = new MemoryStream(datas);
                    #endregion

                    #region 生成中文版
                    //获取委托单
                    var entrustment = await GetEntrustment(model.CalibrationCertificate.TesterSn);
                    model.CalibrationCertificate.EntrustedUnit = entrustment?.CertUnit;
                    model.CalibrationCertificate.EntrustedUnitAdress = entrustment?.CertCountry + entrustment?.CertProvince + entrustment?.CertCity + entrustment?.CertAddress;
                    //委托日期需小于校准日期
                    if (entrustment != null && !string.IsNullOrWhiteSpace(entrustment.EntrustedDate.ToString()) && entrustment?.EntrustedDate > DateTime.Parse(model.CalibrationCertificate.CalibrationDate))
                        entrustment.EntrustedDate = entrustment.EntrustedDate.Value.AddDays(-2);

                    model.CalibrationCertificate.EntrustedDate = !string.IsNullOrWhiteSpace(entrustment?.EntrustedDate.ToString()) ? entrustment?.EntrustedDate.Value.ToString("yyyy年MM月dd日") : "";
                    model.CalibrationCertificate.CalibrationDate = DateTime.Parse(model.CalibrationCertificate.CalibrationDate).ToString("yyyy年MM月dd日");
                    var temp = Math.Round(Convert.ToDecimal(model.CalibrationCertificate.Temperature), 1);
                    model.CalibrationCertificate.Temperature = temp.ToString("0.0");
                    foreach (var item in model.MainStandardsUsed)
                    {
                        if (!string.IsNullOrWhiteSpace(item.DueDate))
                            item.DueDate = DateTime.Parse(item.DueDate).ToString("yyyy-MM-dd");
                        if (item.Name.Contains(","))
                        {
                            var split = item.Name.Split(",");
                            //item.EnName = split[0];
                            item.Name = split[0];
                        }
                        item.Characterisics = item.Characterisics.Replace("Urel", "<i>U</i><sub>rel</sub>").Replace("k=", "<i>k</i>=");
                    }

                    url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Header.html");
                    text = System.IO.File.ReadAllText(url);
                    text = text.Replace("@Model.Data.BarCode", model.BarCode);
                    tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                    System.IO.File.WriteAllText(tempUrl, text);
                    footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Footer.html");
                    datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate CNAS.cshtml", pdf =>
                    {
                        pdf.IsWriteHtml = true;
                        pdf.PaperKind = PaperKind.A4;
                        pdf.Orientation = Orientation.Portrait;
                        pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };//2.812
                        pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 0, HtmUrl = footerUrl };
                    });
                    System.IO.File.Delete(tempUrl);
                    //DirUtil.CheckOrCreateDir(basePath);
                    var fullPathCnas = Path.Combine(basePath, $"{certNo}_CNAS" + ".pdf");
                    //using (FileStream fs = new FileStream(fullPathCnas, FileMode.Create))
                    //{
                    //    using (BinaryWriter bw = new BinaryWriter(fs))
                    //    {
                    //        bw.Write(datas, 0, datas.Length);
                    //    }
                    //}
                    Stream stream2 = new MemoryStream(datas);
                    #endregion

                    await semaphoreSlim.WaitAsync();
                    //上传华为云
                    var fileResp = await _fileApp.UploadFileToHuaweiOBS($"nwcail/{baseInfo.CertificateNumber}/{baseInfo.CertificateNumber}_EN.pdf", null, stream1);
                    var fileRespCn = await _fileApp.UploadFileToHuaweiOBS($"nwcail/{baseInfo.CertificateNumber}/{baseInfo.CertificateNumber}_CNAS.pdf", null, stream2);

                    await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { PdfPath = fileResp.FilePath, CNASPdfPath = fileRespCn.FilePath });

                    //生成证书文件后删除校准数据
                    await UnitWork.DeleteAsync<Etalon>(x => x.NwcaliBaseInfoId == baseInfo.Id);
                    await UnitWork.DeleteAsync<NwcaliPlcData>(x => x.NwcaliBaseInfoId == baseInfo.Id);
                    await UnitWork.DeleteAsync<Repository.Domain.NwcaliTur>(x => x.NwcaliBaseInfoId == baseInfo.Id);

                    await UnitWork.SaveAsync();
                    semaphoreSlim.Release();
                }
                catch (Exception e)
                {

                    throw e;
                }
            }
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
            var saleMan = await UnitWork.Find<crm_oslp>(c => c.SlpName == loginContext.User.Name).Select(c => c.SlpCode).FirstOrDefaultAsync();
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
                                           select new { c.MnfSerial, a.ItemCode, a.DocEntry, a.BaseEntry, a.DocType, a.CreateDate, a.BaseType };

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
                });
                result.Data = resultData;
                result.Count = dataCount;
            }
            else if (request.PageStatus == 2)//设备列表
            {
                var numList = await manufacturerSerialNumber
                    .WhereIf(!string.IsNullOrWhiteSpace(request.SalesOrderId), c => c.BaseEntry == int.Parse(request.SalesOrderId))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.TesterModel), c => c.ItemCode.Contains(request.TesterModel))
                    .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), c => c.MnfSerial.Contains(request.ManufacturerSerialNumbers))
                    .Select(c => new NwcaliBaseInfo { TesterSn = c.MnfSerial, TesterModel = c.ItemCode }).ToListAsync();

                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("校准证书"));
                var fsid = await UnitWork.Find<FlowInstance>(null)
                    .Where(o => o.SchemeId == mf.FlowSchemeId && (o.ActivityName.Contains("已完成") || o.IsFinish == 1))//校准证书已完成流程,IsFinish==1为流程改动前真实结束的数据
                    .Select(o => o.Id)
                    .ToListAsync();
                //var fsid = fs.Select(f => f.Id).ToList();

                var cerlist = await UnitWork.Find<NwcaliBaseInfo>(o => fsid.Contains(o.FlowInstanceId)).ToListAsync();
                var test = cerlist.OrderByDescending(c => c.Time).GroupBy(c => c.TesterSn).Select(c => c.First()).ToList();

                var devicelist1 = from a in numList
                                  join b in cerlist on a.TesterSn equals b.TesterSn into ab
                                  from b in ab.DefaultIfEmpty()
                                  select new NwcaliBaseInfo
                                  {
                                      TesterSn = a.TesterSn,
                                      TesterModel = a.TesterModel,
                                      AssetNo = b?.AssetNo,
                                      CertificateNumber = b?.CertificateNumber,
                                      Time = b?.Time,
                                      ExpirationDate = b?.ExpirationDate,
                                      Operator = b?.Operator,
                                      PdfPath = b?.PdfPath
                                  };

                devicelist1 = devicelist1.OrderByDescending(c => c.Time).GroupBy(c => c.TesterSn).Select(c => c.First()).ToList();

                if (!string.IsNullOrWhiteSpace(request.CertNo))
                    devicelist1 = devicelist1.Where(c => !string.IsNullOrWhiteSpace(c.CertificateNumber) && c.CertificateNumber.Contains(request.CertNo)).ToList();
                if (!(request.StartCalibrationDate == null && request.EndCalibrationDate == null))
                    devicelist1 = devicelist1.Where(c => c.Time >= request.StartCalibrationDate && c.Time <= request.EndCalibrationDate).ToList();

                #region 老数据
                //序列号下最新的校验证书
                var testNo = numList.Select(c => c.TesterSn).ToList();
                var old = await UnitWork.Find<Certinfo>(c => testNo.Contains(c.Sn) && fsid.Contains(c.FlowInstanceId)).ToListAsync();
                if (old.Count > 0)
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
                result.Data = devicelist1.Select(c =>
                {
                    return new
                    {
                        TesterSn = c.TesterSn,
                        TesterModel = c.TesterModel,
                        AssetNo = c.AssetNo,
                        CertificateNumber = c.CertificateNumber,
                        Time = c.Time,
                        ExpirationDate = c.ExpirationDate,
                        Operator = c.Operator,
                        PdfPath = c.PdfPath
                    };
                })
                .OrderByDescending(c => c.Time)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit)
                .ToList();
            }
            else if (request.PageStatus == 3)//证书列表
            {
                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("校准证书"));
                var fsid = await UnitWork.Find<FlowInstance>(null)
                    .Where(o => o.SchemeId == mf.FlowSchemeId && (o.ActivityName.Contains("已完成") || o.IsFinish == 1))//校准证书已完成流程,IsFinish==1为流程改动前真实结束的数据
                    .Select(c => c.Id)
                    .ToListAsync();
                //var fsid = fs.Select(f => f.Id).ToList();

                var cerinfo = await UnitWork.Find<NwcaliBaseInfo>(o => fsid.Contains(o.FlowInstanceId))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), c => c.TesterSn.Contains(request.ManufacturerSerialNumbers))
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
                         PdfPath = c.PdfPath
                     };
                 });

                #region 老数据
                var obj = await UnitWork.Find<Certinfo>(o => fsid.Contains(o.FlowInstanceId))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.ManufacturerSerialNumbers), c => c.Sn.Equals(request.ManufacturerSerialNumbers))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CertNo), c => c.CertNo.Contains(request.CertNo))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.Operator), c => c.Operator.Contains(request.Operator))
                                .WhereIf(!(request.StartCalibrationDate == null && request.EndCalibrationDate == null), c => c.CalibrationDate >= request.StartCalibrationDate && c.CalibrationDate <= request.EndCalibrationDate)
                                .ToListAsync();
                if (obj.Count > 0)
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

        /// <summary>
        /// 重新生成证书数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<List<NwcaliBaseInfo>> ReFillNwcailData(string certno = "")
        {
            var nwcail = await UnitWork.Find<NwcaliBaseInfo>(c => !string.IsNullOrWhiteSpace(c.CertPath)).WhereIf(!string.IsNullOrWhiteSpace(certno), c => c.CertificateNumber == certno).Include(c => c.Etalons).ToListAsync();

            foreach (var item in nwcail)
            {
                //被删除则新增
                if (item.Etalons.Count == 0)
                {
                    var handler = new ExcelHandler(item.CertPath);

                    var baseInfo = handler.GetBaseInfo<NwcaliBaseInfo>(sheet =>
                    {
                        var baseInfo = new NwcaliBaseInfo();
                        var timeRow = sheet.GetRow(1);
                        baseInfo.Time = DateTime.Parse(timeRow.GetCell(1).StringCellValue);
                        var testIntervalRow = sheet.GetRow(30);
                        baseInfo.TestInterval = testIntervalRow.GetCell(1).StringCellValue;
                        #region 标准器设备信息
                        var etalonsNameRow = sheet.GetRow(18);
                        var etalonsCharacteristicsRow = sheet.GetRow(19);
                        var etalonsAssetNoRow = sheet.GetRow(20);
                        var etalonsCertificateNoRow = sheet.GetRow(22);
                        var etalonsCalibrationEntity = sheet.GetRow(21);
                        var etalonsDueDateRow = sheet.GetRow(23);
                        for (int i = 1; i < etalonsNameRow.LastCellNum; i++)
                        {
                            if (string.IsNullOrWhiteSpace(etalonsNameRow.GetCell(i).StringCellValue))
                                break;
                            try
                            {
                                baseInfo.Etalons.Add(new Etalon
                                {
                                    NwcaliBaseInfoId = item.Id,
                                    Name = etalonsNameRow.GetCell(i).StringCellValue,
                                    Characteristics = etalonsCharacteristicsRow.GetCell(i).StringCellValue,
                                    AssetNo = etalonsAssetNoRow.GetCell(i).StringCellValue,
                                    CertificateNo = etalonsCertificateNoRow.GetCell(i).StringCellValue,
                                    DueDate = etalonsDueDateRow.GetCell(i).StringCellValue,
                                    CalibrationEntity = etalonsCalibrationEntity.GetCell(i).StringCellValue
                                });
                            }
                            catch
                            {
                                break;
                            }
                        }
                        #endregion

                        #region 下位机
                        var pclCommentRow = sheet.GetRow(31);
                        var pclNoRow = sheet.GetRow(32);
                        var pclGuidRow = sheet.GetRow(33);
                        for (int i = 1; i < pclNoRow.LastCellNum; i++)
                        {
                            if (string.IsNullOrWhiteSpace(pclGuidRow.GetCell(i)?.StringCellValue))
                                continue;
                            try
                            {
                                baseInfo.PcPlcs.Add(new PcPlc
                                {
                                    Comment = pclCommentRow.GetCell(i).StringCellValue,
                                    No = Convert.ToInt32(pclNoRow.GetCell(i).StringCellValue),
                                    Guid = pclGuidRow.GetCell(i).StringCellValue,
                                    CalibrationDate = baseInfo.Time,
                                    ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time.Value.ToString(), baseInfo.TestInterval))
                                });
                            }
                            catch (Exception ex)
                            {
                                break;
                            }
                        }
                        #endregion
                        return baseInfo;
                    });

                    var turV = handler.GetNwcaliTur("电压");
                    var turA = handler.GetNwcaliTur("电流");
                    var tv = turV.Select(v => new Repository.Domain.NwcaliTur { NwcaliBaseInfoId = item.Id, DataType = 1, Range = v.Range, TestPoint = v.TestPoint, Tur = v.Tur, UncertaintyContributors = v.UncertaintyContributors, SensitivityCoefficient = v.SensitivityCoefficient, Value = v.Value, Unit = v.Unit, Type = v.Type, Distribution = v.Distribution, Divisor = v.Divisor, StdUncertainty = v.StdUncertainty, DegreesOfFreedom = v.DegreesOfFreedom, SignificanceCheck = v.SignificanceCheck }).ToList();
                    var ta = turA.Select(v => new Repository.Domain.NwcaliTur { NwcaliBaseInfoId = item.Id, DataType = 2, Range = v.Range, TestPoint = v.TestPoint, Tur = v.Tur, UncertaintyContributors = v.UncertaintyContributors, SensitivityCoefficient = v.SensitivityCoefficient, Value = v.Value, Unit = v.Unit, Type = v.Type, Distribution = v.Distribution, Divisor = v.Divisor, StdUncertainty = v.StdUncertainty, DegreesOfFreedom = v.DegreesOfFreedom, SignificanceCheck = v.SignificanceCheck }).ToList();
                    baseInfo.NwcaliTurs.AddRange(tv);
                    baseInfo.NwcaliTurs.AddRange(ta);

                    try
                    {
                        foreach (var plc in baseInfo.PcPlcs)
                        {
                            var list = handler.GetNWCaliPLCData($"下位机{plc.No}");
                            baseInfo.NwcaliPlcDatas.AddRange(list.Select(l => new NwcaliPlcData
                            {
                                NwcaliBaseInfoId = item.Id,
                                PclNo = plc.No,
                                DataType = 1,
                                VerifyType = l.Verify_Type,
                                VoltsorAmps = l.VoltsorAmps,
                                Channel = l.Channel,
                                Mode = l.Mode,
                                Range = l.Range,
                                Point = l.Point,
                                CommandedValue = l.Commanded_Value,
                                MeasuredValue = l.Measured_Value,
                                Scale = l.Scale,
                                StandardTotalU = l.Standard_total_U,
                                StandardValue = l.Standard_Value
                            }));
                            var list2 = handler.GetNWCaliPLCRepetitiveMeasurementData($"下位机{plc.No}重复性测量");
                            if (list2.Count > 0)
                                baseInfo.NwcaliPlcDatas.AddRange(list2.Select(l => new NwcaliPlcData
                                {
                                    NwcaliBaseInfoId = item.Id,
                                    PclNo = plc.No,
                                    DataType = 2,
                                    VerifyType = l.Verify_Type,
                                    VoltsorAmps = l.VoltsorAmps,
                                    Channel = l.Channel,
                                    Mode = l.Mode,
                                    Range = l.Range,
                                    Point = l.Point,
                                    CommandedValue = l.Commanded_Value,
                                    MeasuredValue = l.Measured_Value,
                                    Scale = l.Scale,
                                    StandardTotalU = l.Standard_total_U,
                                    StandardValue = l.Standard_Value
                                }));
                        }
                        //await _nwcaliCertApp.AddAsync(baseInfo);
                        await UnitWork.BatchAddAsync(baseInfo.Etalons.ToArray());
                        await UnitWork.BatchAddAsync(baseInfo.NwcaliTurs.ToArray());
                        await UnitWork.BatchAddAsync(baseInfo.NwcaliPlcDatas.ToArray());
                        await UnitWork.SaveAsync();
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            return nwcail;
        }

        /// <summary>
        /// 查询委托单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> LoadEntrustment(QueryEntrustmentReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var query = UnitWork.Find<Entrustment>(null)
                .WhereIf(request.Id != null, c => c.Id == request.Id)
                .WhereIf(!string.IsNullOrWhiteSpace(request.EntrustedUnit), c => c.EntrustedUnit.Contains(request.EntrustedUnit))
                .WhereIf(request.SaleId != null, c => c.SaleId == request.SaleId)
                .WhereIf(request.Status != null, c => c.Status == request.Status)
                .WhereIf(request.StartTime != null, c => c.CreateDate >= request.StartTime.Value.Date)
                .WhereIf(request.EndTime != null, c => c.CreateDate < request.EndTime.Value.AddDays(1).Date);

            query = query.OrderByDescending(r => r.UpdateDate);
            var list = await query.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            result.Data = list;
            result.Count = await query.CountAsync();
            return result;
        }

        /// <summary>
        /// 获取委托单详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetail(int id)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();

            var list = await UnitWork.Find<Entrustment>(c => c.Id == id).Include(c => c.EntrustmentDetails).FirstOrDefaultAsync();
            list.EntrustmentDetails = list.EntrustmentDetails.OrderBy(c => c.Sort).ToList();
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 处理委托单/更新状态
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<Infrastructure.Response> HandleEntrusted(HandleEntrustedReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new Infrastructure.Response();
            if (request.IsUpdateStatus)//更新状态
            {
                if (request.Type == "3")
                {
                    await UnitWork.UpdateAsync<EntrustmentDetail>(c => c.Id == request.EntrustmentId, c => new EntrustmentDetail
                    {
                        Status = 2//超出校准范围
                    });
                }
                else
                {
                    var detail = new List<EntrustmentDetail>();
                    if (request.Type == "1")
                        detail = await UnitWork.Find<EntrustmentDetail>(c => c.EntrustmentId == request.Id).ToListAsync();
                    else if (request.Type == "2")
                        detail = await UnitWork.Find<EntrustmentDetail>(c => c.Id == request.EntrustmentId).ToListAsync();

                    var snids = detail.Select(c => c.SerialNumber).ToList();
                    var nwcert = await UnitWork.Find<NwcaliBaseInfo>(c => snids.Contains(c.TesterSn)).ToListAsync();
                    SetStatus(ref detail, nwcert);
                    await UnitWork.BatchUpdateAsync(detail.ToArray());
                }
            }
            else
            {
                if (request.Status == 2)//接受委托单
                {
                    await UnitWork.UpdateAsync<Entrustment>(c => c.Id == request.Id, c => new Entrustment
                    {
                        ReceiptDate = DateTime.Now,
                        ReceiptUserId = request.ReceiptUserId,
                        ReceiptUser = request.ReceiptUser,
                        Status = 3//待校准
                    });
                }
                else if (request.Status == 4 || request.Status == 3)
                {
                    await UnitWork.UpdateAsync<Entrustment>(c => c.Id == request.Id, c => new Entrustment
                    {
                        Status = 5//已完成
                    });
                }
            }
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 根据序列号获取委托单
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        public async Task<Entrustment> GetEntrustment(string serialNumber)
        {
            return await UnitWork.Find<Entrustment>(c => c.EntrustmentDetails.Any(e => e.SerialNumber == serialNumber)).FirstOrDefaultAsync();
        }

        /// <summary>
        /// 推送过期前一个月的校准证书关联的GUID
        /// </summary>
        /// <returns></returns>
        public async Task PushCertGuidToApp()
        {
            var plcguid = await UnitWork.FromSql<PcPlc>(@$"SELECT * from pcplc where timestampdiff(day,ExpirationDate,NOW())=30 or timestampdiff(day,ExpirationDate,NOW())=20 or timestampdiff(day,ExpirationDate,NOW())=10").Select(c => new { c.Guid, DueTime = c.ExpirationDate }).ToListAsync();

            var guid = await UnitWork.FromSql<Certplc>(@$"SELECT * from certplc where timestampdiff(day,ExpirationDate,NOW())=30 or timestampdiff(day,ExpirationDate,NOW())=20 or timestampdiff(day,ExpirationDate,NOW())=10").Select(c => new { Guid = c.PlcGuid, DueTime = c.ExpirationDate }).ToListAsync();

            if (guid.Count > 0) plcguid = plcguid.Concat(guid).ToList();


            await _serviceOrderApp.PushMessageToApp(0, "", "", "1", plcguid);
        }

        /// <summary>
        /// 拉取销售交货生成备料单
        /// </summary>
        /// <returns></returns>

        public async Task SynSalesDelivery()
        {
            //销售交货流程 并处于序列号选择环节
            var entrusted = await UnitWork.Find<Entrustment>(c => c.Status != 5).Select(r => new { r.Id, r.JodId, r.Status, r.ContactsId, r.Contacts }).ToListAsync();
            var jobIds = entrusted.Select(c => c.JodId).ToList();//已经生成过的流程ID
            var deliveryList = await UnitWork.Find<wfa_job>(c => c.job_type_id == 1 && c.job_nm == "销售交货" && c.step_id == 455).Select(c => c.job_id).ToListAsync();
            var deliveryJob = await UnitWork.Find<wfa_job>(c => !jobIds.Contains(c.job_id) && deliveryList.Contains(c.job_id)).Select(c => new { c.sbo_id, c.base_entry, c.base_type, c.job_data, c.job_id, c.step_id, c.job_state, c.sync_stat, c.sbo_itf_return }).ToListAsync();//在选择序列号环节

            #region 生成备料单
            try
            {
                if (deliveryJob.Count > 0)
                {
                    foreach (var item in deliveryJob)
                    {
                        var saleOrder = await UnitWork.Find<sale_ordr>(c => c.sbo_id == item.sbo_id && c.DocEntry == item.base_entry).FirstOrDefaultAsync();
                        if (saleOrder != null)
                        {
                            var model = DeSerialize(item.job_data);
                            Entrustment single = new Entrustment();
                            var saler = await UnitWork.Find<crm_oslp>(c => c.SlpCode == saleOrder.SlpCode && c.sbo_id == saleOrder.sbo_id).FirstOrDefaultAsync();

                            //获取部门
                            var query = from a in UnitWork.Find<User>(c => c.Name == saler.SlpName)
                                        join b in UnitWork.Find<Relevance>(c => c.Key == Define.USERORG) on a.Id equals b.FirstId into ab
                                        from b in ab.DefaultIfEmpty()
                                        join c in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on b.SecondId equals c.Id into bc
                                        from c in bc.DefaultIfEmpty()
                                        select new { a.Id, UserName = a.Name, OrgName = c.Name, c.CascadeId };

                            var OrgNameList = await query.OrderByDescending(o => o.CascadeId).FirstOrDefaultAsync();
                            single.ContactsId = OrgNameList?.Id;
                            single.Contacts = OrgNameList?.UserName;
                            single.ContactsOrg = OrgNameList?.OrgName;
                            single.Phone = saler?.Memo;

                            //获取开票到地址
                            var entrustedUnit = GetAddress("C00550", saleOrder.sbo_id);//新威尔
                            var certUnit = GetAddress(saleOrder.CardCode, saleOrder.sbo_id);//新威尔
                            single.EntrustedUnit = entrustedUnit?.CardName;//????
                            single.ECountry = entrustedUnit?.Country;
                            single.EProvince = entrustedUnit?.State;
                            single.ECity = entrustedUnit?.City;
                            single.EAddress = entrustedUnit?.Building;
                            single.CertUnit = saleOrder?.CardName;
                            single.CertCountry = certUnit?.Country;
                            single.CertProvince = certUnit?.State;
                            single.CertCity = certUnit?.City;
                            single.CertAddress = certUnit?.Building;
                            single.CreateDate = DateTime.Now;
                            single.SaleId = saleOrder.DocEntry;
                            //单据备注+设备编号/箱号+验收期限+系统操作者+生产部门
                            if (!string.IsNullOrWhiteSpace(saleOrder.Comments))
                                saleOrder.Comments += ",";
                            if (!string.IsNullOrWhiteSpace(saleOrder.U_CPH))
                                saleOrder.U_CPH += ",";
                            if (!string.IsNullOrWhiteSpace(saleOrder.U_YSQX))
                                saleOrder.U_YSQX += ",";
                            if (!string.IsNullOrWhiteSpace(saleOrder.U_YGMD))
                                saleOrder.U_YGMD += ",";
                            if (!string.IsNullOrWhiteSpace(saleOrder.U_SCBM))
                                saleOrder.U_SCBM += ",";

                            single.Remark = (saleOrder.Comments + saleOrder.U_CPH + saleOrder.U_YSQX + saleOrder.U_YGMD + saleOrder.U_SCBM).Trim();
                            single.Status = 1;
                            single.UpdateDate = DateTime.Now;
                            single.JodId = item.job_id;

                            //物料明细
                            if (model.DocType == "I")
                            {
                                single.EntrustmentDetails = new List<EntrustmentDetail>();
                                int i = 0;
                                foreach (var data in model.billSalesDetails)
                                {
                                    if (data.ItemCode.StartsWith("CT") || data.ItemCode.StartsWith("CTE") || data.ItemCode.StartsWith("CE"))
                                    {
                                        ++i;
                                        EntrustmentDetail obj = new EntrustmentDetail();
                                        obj.LineNum = i.ToString();
                                        obj.EntrustmentId = single.Id;
                                        obj.ItemName = data.Dscription.Split(',')[0].Split('-')[0];
                                        obj.ItemCode = data.ItemCode;
                                        obj.Quantity = string.IsNullOrEmpty(data.Quantity) ? 0 : Convert.ToInt32(data.Quantity.Split(".")[0]);
                                        obj.Sort = i;

                                        single.EntrustmentDetails.Add(obj);
                                    }
                                }
                            }

                            single = await UnitWork.AddAsync<Entrustment, int>(single);
                            await UnitWork.SaveAsync();
                        }
                    }
                }
                #endregion

                #region 生成委托单
                var finlishJobs = await UnitWork.Find<wfa_job>(null).Where(c => c.job_type_id == 1 && c.job_nm == "销售交货" && c.job_state == 3 && c.sync_stat == 4).Select(c => c.job_id).ToListAsync();//选择了序列号/结束的交货流程并且同步完成
                var finlishEntrusted = entrusted.Where(c => finlishJobs.Contains(c.JodId) && c.Status == 1).ToList();//选择了序列号/结束的交货流程并且同步完成
                if (deliveryJob.Count > 0)
                {
                    for (int i = 0; i < finlishEntrusted.Count; i++)
                    {
                        var item = finlishEntrusted[i];
                        var job = deliveryJob.Where(c => c.job_id == item.JodId).FirstOrDefault();
                        if (job != null)
                        {
                            var serialNumber = await (from a in UnitWork.Find<OITL>(null)
                                                      join b in UnitWork.Find<ITL1>(null) on a.LogEntry equals b.LogEntry into ab
                                                      from b in ab.DefaultIfEmpty()
                                                      join c in UnitWork.Find<OSRN>(null) on new { b.ItemCode, SysNumber = b.SysNumber.Value } equals new { c.ItemCode, c.SysNumber } into bc
                                                      from c in bc.DefaultIfEmpty()
                                                      where a.DocType == 15 && a.DefinedQty > 0 && a.DocNum.Value.ToString() == job.sbo_itf_return
                                                      select new { a.DocType, a.DocNum, a.ItemCode, a.ItemName, b.SysNumber, c.MnfSerial, c.DistNumber }).ToListAsync();

                            int line = 0, sort = 0;
                            List<EntrustmentDetail> detail = new List<EntrustmentDetail>();
                            foreach (var groupItem in serialNumber.GroupBy(c => c.ItemCode).ToList())
                            {
                                int line2 = 0;
                                var deleteData = await UnitWork.Find<EntrustmentDetail>(x => x.EntrustmentId == item.Id)?.ToArrayAsync();
                                if (deleteData != null && deleteData.Count() > 0)
                                {
                                    try
                                    {
                                        await UnitWork.BatchDeleteAsync<EntrustmentDetail>(deleteData);
                                        await UnitWork.SaveAsync();
                                    }
                                    catch (DbUpdateConcurrencyException ex)
                                    {
                                        throw new Exception("数据删除异常", ex);
                                    }
                                }

                                if (groupItem.Key.StartsWith("CT") || groupItem.Key.StartsWith("CTE") || groupItem.Key.StartsWith("CE"))
                                {
                                    ++line;
                                    foreach (var items in groupItem)
                                    {
                                        ++line2; ++sort;
                                        EntrustmentDetail entrustmentDetail = new EntrustmentDetail();
                                        entrustmentDetail.EntrustmentId = item.Id;
                                        entrustmentDetail.ItemCode = groupItem.Key;
                                        entrustmentDetail.ItemName = items.ItemName.Split(',')[0].Split('-')[0];
                                        entrustmentDetail.SerialNumber = items.MnfSerial;
                                        entrustmentDetail.Quantity = 1;
                                        entrustmentDetail.LineNum = line + "-" + line2;
                                        entrustmentDetail.Sort = sort;
                                        entrustmentDetail.Id = Guid.NewGuid().ToString();
                                        detail.Add(entrustmentDetail);
                                    }
                                }
                            }

                            await UnitWork.BatchAddAsync<EntrustmentDetail>(detail.ToArray());
                            await UnitWork.UpdateAsync<Entrustment>(c => c.Id == item.Id, c => new Entrustment
                            {
                                DeliveryId = job.sbo_itf_return,
                                Status = 2,//待处理
                                EntrustedUserId = item.ContactsId,
                                EntrustedUser = item.Contacts,
                                EntrustedDate = DateTime.Now,
                                UpdateDate = DateTime.Now
                            });
                            await UnitWork.SaveAsync();
                        }
                    }
                }

                #endregion

                #region 同步状态
                var calibration = entrusted.Where(c => c.Status >= 3).ToList();
                foreach (var item in calibration)
                {
                    var details = await UnitWork.FindTrack<EntrustmentDetail>(c => c.EntrustmentId == item.Id).ToListAsync();
                    var snids = details.Select(c => c.SerialNumber).ToList();
                    var nwcert = await UnitWork.Find<NwcaliBaseInfo>(c => snids.Contains(c.TesterSn)).ToListAsync();
                    if (nwcert != null && nwcert.Count > 0)
                    {
                        var status = 4;//校准中
                        SetStatus(ref details, nwcert);
                        //全部为已校准 则为完成
                        if (details.All(c => c.Status > 1))
                            status = 5;//已完成

                        await UnitWork.BatchUpdateAsync(details.ToArray());
                        await UnitWork.UpdateAsync<Entrustment>(c => c.Id == item.Id, c => new Entrustment
                        {
                            Status = status,
                            UpdateDate = DateTime.Now
                        });
                    }
                }
                #endregion

                #region 调用接口
                string tokens = System.Web.HttpUtility.UrlEncode(_ddSettingHelp.GetCalibrationKey("Token"));

                //获取状态2的委托单据
                List<Entrustment> entrustments = await UnitWork.Find<Entrustment>(r => r.Status == 2 && r.UpdateDate >= Convert.ToDateTime("2022-12-06")).Include(r => r.EntrustmentDetails).ToListAsync();

                //获取校准委托单组件信息
                CalibrationResult calibrations = JsonConvert.DeserializeObject<CalibrationResult>(HttpHelpers.Get($"http://121.37.222.129:1666/api/Calibration/GetCalibrationFields?Token={tokens}"));

                //校准委托单数据提交
                if (calibrations.status == 200)
                {
                    foreach (Entrustment entrustment in entrustments)
                    {
                        List<SubmitData> submitDatas = new List<SubmitData>();
                        if (entrustment.EntrustmentDetails != null && entrustment.EntrustmentDetails.Count() > 0)
                        {
                            foreach (CalibrationGroups item in calibrations.data)
                            {
                                SubmitData submitData = new SubmitData();
                                switch (item.field_tag)
                                {
                                    case "EntOrderNo":
                                        submitData.key = item.field_id;
                                        submitData.value = "";
                                        break;
                                    case "EntOrderDate":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.EntrustedDate == null ? DateTime.Now : entrustment.EntrustedDate;
                                        break;
                                    case "SaleOrderDate":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.SaleId == null ? 0 : entrustment.SaleId;
                                        break;
                                    case "Submitter":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.Contacts;
                                        break;
                                    case "SubmittingUnit":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.EntrustedUnit;
                                        break;
                                    case "SubmittingUnitTel":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.Phone;
                                        break;
                                    case "SubmittingUnitAdd":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.ECountry + entrustment.EAddress;
                                        break;
                                    case "CertificationUnit":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.CertUnit;
                                        break;
                                    case "CertificationUnitTel":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.Phone;
                                        break;
                                    case "CertificationUnitAdd":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.CertCountry + entrustment.CertProvince + entrustment.CertCity + entrustment.CertAddress;
                                        break;
                                    case "Remarks":
                                        submitData.key = item.field_id;
                                        submitData.value = entrustment.Remark;
                                        break;
                                    case "TimeReq":
                                        submitData.key = item.field_id;
                                        submitData.value = "普通服务（五个工作日）";
                                        break;
                                    case "LabelNo":
                                        submitData.key = item.field_id;
                                        submitData.value = "出厂编号";
                                        break;
                                    case "CertificateNo":
                                        submitData.key = item.field_id;
                                        submitData.value = "出场编号";
                                        break;
                                    case "NextCalibrationDate":
                                        submitData.key = item.field_id;
                                        submitData.value = "需要下次校准时间";
                                        break;
                                    case "ConclusionJud":
                                        submitData.key = item.field_id;
                                        submitData.value = "无结论判定";
                                        break;
                                    case "HandlingMethod":
                                        submitData.key = item.field_id;
                                        submitData.value = "不同意代送";
                                        break;
                                    case "CollectionMethod":
                                        submitData.key = item.field_id;
                                        submitData.value = "自取";
                                        break;
                                    case "MeasurementUnit":
                                        submitData.key = item.field_id;
                                        submitData.value = "国家法定计量单位";
                                        break;
                                    case "CalibrationStaff":
                                        submitData.key = item.field_id;
                                        submitData.value = "";
                                        break;
                                    default:
                                        if (item.child_list != null && item.child_list.Count() > 0)
                                        {
                                            SubmitDataContainChild submirDataContainChild = new SubmitDataContainChild();
                                            submirDataContainChild.key = item.field_id;
                                            List<SubmitDataChild> submitDataChildren = new List<SubmitDataChild>();
                                            foreach (CalibrationGroups itemChild in item.child_list)
                                            {
                                                SubmitDataChild submitDataChild = new SubmitDataChild();
                                                foreach (EntrustmentDetail entrustmentDetail in entrustment.EntrustmentDetails)
                                                {
                                                    switch (itemChild.field_tag)
                                                    {
                                                        case "InstrumentName":
                                                            submitDataChild.key = itemChild.field_id;
                                                            submitDataChild.value.Add(entrustmentDetail.ItemName);
                                                            break;
                                                        case "SpecificatModel":
                                                            submitDataChild.key = itemChild.field_id;
                                                            submitDataChild.value.Add(entrustmentDetail.ItemCode);
                                                            break;
                                                        case "SN":
                                                            submitDataChild.key = itemChild.field_id;
                                                            submitDataChild.value.Add(entrustmentDetail.SerialNumber);
                                                            break;
                                                        case "Attachment":
                                                            submitDataChild.key = itemChild.field_id;
                                                            submitDataChild.value.Add("");
                                                            break;
                                                        case "AppearanceDes":
                                                            submitDataChild.key = itemChild.field_id;
                                                            submitDataChild.value.Add("正常");
                                                            break;
                                                        case "CalibrationReq":
                                                            submitDataChild.key = itemChild.field_id;
                                                            submitDataChild.value.Add("");
                                                            break;
                                                        case "CertificateSta":
                                                            submitDataChild.key = itemChild.field_id;
                                                            submitDataChild.value.Add(entrustmentDetail.Status.ToString());
                                                            break;
                                                    }
                                                }

                                                submitDataChildren.Add(submitDataChild);
                                            }

                                            submirDataContainChild.value = submitDataChildren;
                                            submitData.key = submirDataContainChild.key;
                                            submitData.value = submirDataContainChild.value;
                                        }
                                        break;
                                }

                                submitDatas.Add(submitData);
                            }
                        }
                        else
                        {
                            _logger.LogError("委托单物料明细为空");
                        }

                        ControlDataList controlDataList = new ControlDataList();
                        controlDataList.controls_data_list = submitDatas;
                        if (controlDataList.controls_data_list.Count() > 0)
                        {
                            //调用校准委托单数据提交接口
                            ReturnResult returnResult = JsonConvert.DeserializeObject<ReturnResult>(HttpHelpers.HttpPostAsync($"http://121.37.222.129:1666/api/Calibration/SubmitCalibrationFormData?Token={tokens}", JsonConvert.SerializeObject(controlDataList)).Result);
                            if (returnResult.status != 200)
                            {
                                _logger.LogError("委托单调用2接口失败：" + returnResult.message);
                            }
                            else
                            {
                                await UnitWork.UpdateAsync<Entrustment>(c => c.Id == entrustment.Id, c => new Entrustment
                                {
                                    Status = 6,
                                    UpdateDate = DateTime.Now
                                });
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogError("委托单调用1接口失败：" + calibrations.message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("委托单调用接口失败：" + ex.Message.ToString());
            }
            #endregion
        }

        private void SetStatus(ref List<EntrustmentDetail> detail, List<NwcaliBaseInfo> nwcert)
        {
            foreach (var item in detail)
            {
                var single = nwcert.Where(c => c.TesterSn == item.SerialNumber).OrderByDescending(c => c.CreateTime).FirstOrDefault();
                if (single != null)
                {
                    if (!string.IsNullOrWhiteSpace(single.ApprovalDirectorId))
                        item.Status = 1;//待审核
                    else
                        item.Status = 3;//已校准
                }
                else
                    item.Status = 0;//待上传
            }
        }

        public static NSAP.Entity.Sales.billDelivery DeSerialize(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return null;
            using (MemoryStream stream = new MemoryStream())
            {
                IFormatter bs = new BinaryFormatter();
                stream.Write(bytes, 0, bytes.Length);
                stream.Seek(0, SeekOrigin.Begin);
                return (NSAP.Entity.Sales.billDelivery)bs.Deserialize(stream);
            }
        }

        /// <summary>
        /// 获取客户开票到地址
        /// </summary>
        /// <param name="cardcode"></param>
        /// <param name="sboid"></param>
        /// <returns></returns>
        public dynamic GetAddress(string cardcode, int sboid)
        {
            var query = from a in UnitWork.Find<crm_crd1>(null)
                        join d in UnitWork.Find<crm_ocrd>(null) on new { a.CardCode, a.Address } equals new { d.CardCode, Address = d.BillToDef } into ad
                        from d in ad.DefaultIfEmpty()
                        join b in UnitWork.Find<crm_ocry>(null) on a.Country equals b.Code into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<crm_ocst>(null) on a.State equals c.Code into ac
                        from c in ac.DefaultIfEmpty()
                        where d.sbo_id == sboid && a.CardCode == cardcode // && a.AdresType=="B" && a.Address== "开票到"
                        select new { d.CardCode, d.CardName, a.LineNum, Active = a.U_Active, a.AdresType, a.Address, Country = b.Name, State = c.Name, a.City, a.Building };
            return query.Where(r => r.CardCode != null).FirstOrDefault();
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
        private async Task<bool> CheckCanOperation(string id, string name, string operate)
        {
            //撤回操作不验证
            if (operate.Equals("4")) return true;
            var history = await UnitWork.Find<CertOperationHistory>(c => c.CertInfoId.Equals(id)).ToListAsync();
            var rejectTime = history.Where(c => c.Action.Contains("驳回")).OrderByDescending(c => c.CreateTime).Select(c => c.CreateTime).FirstOrDefault();
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
            var objs = await UnitWork.Find<Category>(c => Model.Contains(c.Name) && c.TypeId.Equals("SYS_CalibrationCertificateType")).FirstOrDefaultAsync();
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
                        select new { id = a.Id, plcGuid = a.Guid, materialCode = b.TesterModel, TesterSn = b.TesterSn };

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
                        select new { id = a.Id, plcGuid = a.Guid, certNo = b.CertificateNumber, calibrationDate = a.CalibrationDate, b.Operator, expirationDate = a.ExpirationDate };

            result.Data = await query.ToListAsync();
            return result;
        }

        #region 品质
        /// <summary>
        /// 查询生产订单
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> ProductionOrderList(QueryCertinfoListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var loginOrg = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault();
            var query = from a in UnitWork.Find<product_owor>(c => (c.ItemCode.StartsWith("C") || c.ItemCode.StartsWith("BT") || c.ItemCode.StartsWith("BTE") 
                        || c.ItemCode.StartsWith("BE") || c.ItemCode.StartsWith("BBE") || c.ItemCode.StartsWith("BA")) && c.CreateDate >= DateTime.Parse("2021-01-01"))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.ProductionNo.ToString()), c => EF.Functions.Like(c.DocEntry, $"%{request.ProductionNo}%"))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.SaleOrderNo.ToString()), c => EF.Functions.Like(c.OriginAbs, $"%{request.SaleOrderNo}%"))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.ItemCode), c => c.ItemCode.Contains(request.ItemCode))
                        .WhereIf(!string.IsNullOrWhiteSpace(request.Status), c => c.Status == request.Status)
                        join b in UnitWork.Find<sale_ordr>(null) on new { a.OriginAbs, a.sbo_id } equals new { OriginAbs = b.DocEntry, b.sbo_id } into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<crm_oslp>(null)
                        on new { b.SlpCode, b.sbo_id } equals new { SlpCode = (short?)c.SlpCode, sbo_id = c.sbo_id.Value } into bc
                        from c in bc.DefaultIfEmpty()
                        select new { a.DocEntry, a.ItemCode, ItemName = a.txtitemName, a.PlannedQty, a.CmpltQty, Finish = "", OrgName = a.U_WO_LTDW, SaleOrderNo = a.OriginAbs, c.SlpName, a.Status };
            if (loginContext.User.Account != Define.SYSTEM_USERNAME && !loginContext.Roles.Any(r => r.Name.Equals("生产订单跟进")))
            {
                query = query.Where(c => c.OrgName.Contains(loginOrg.Name));
            }
            query = query.WhereIf(!string.IsNullOrWhiteSpace(request.SlpName), c => EF.Functions.Like(c.SlpName, $"%{request.SlpName}%"));
            result.Count = await query.CountAsync();
            result.Data = await query.OrderByDescending(c => c.DocEntry).Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            return result;
        }

        /// <summary>
        /// 生产单物料详情
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        public async Task<TableData> MaterialDetail(int docEntry)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            List<string> itemcode = new List<string>();
            var category = await UnitWork.Find<Category>(c => c.TypeId == "SYS_B01").Select(c => c.DtValue).ToListAsync();
            var query = await UnitWork.Find<product_wor1>(c => c.DocEntry == docEntry && c.ItemCode.StartsWith("B01")).Select(c => c.ItemCode).ToListAsync();
            category.ForEach(c =>
            {
                var regex = "^" + c + "-[0-9].";
                itemcode.AddRange(query.Where(q => Regex.IsMatch(q, regex)).ToList());
            });
            //分别取一条
            var data = itemcode.GroupBy(c => string.Join("-", c.Split('-').Take(2))).Select(c => c.First()).Take(4).ToList();
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 生成生产唯一码
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        public async Task GenerateWO()
        {
            var product_owor = await UnitWork.Find<product_owor>(c => (c.ItemCode.StartsWith("C") || c.ItemCode.StartsWith("BT") || c.ItemCode.StartsWith("BTE") || c.ItemCode.StartsWith("BE")) && c.CreateDate >= DateTime.Parse("2021-01-01")).Select(c => new { c.DocEntry, c.PlannedQty }).ToListAsync();
            var schedule = await UnitWork.Find<ProductionSchedule>(null).Select(c => c.DocEntry).Distinct().ToListAsync();
            product_owor = product_owor.Where(c => !schedule.Contains(c.DocEntry)).ToList();
            if (product_owor.Count > 0)
            {
                List<ProductionSchedule> schedules = new List<ProductionSchedule>();
                foreach (var item in product_owor)
                {
                    var count = Convert.ToInt32(item.PlannedQty.Value);
                    for (int i = 1; i <= count; i++)
                    {
                        var wo = $"WO-{item.DocEntry}-{count}-{i}";
                        schedules.Add(new ProductionSchedule { DocEntry = item.DocEntry, GeneratorCode = wo, ProductionStatus = 2, DeviceStatus = 1, NwcailStatus = 1, ReceiveStatus = 1, ReceiveLocation = "", SortNo = i });
                    }
                }
                await UnitWork.BatchAddAsync(schedules.ToArray());
                await UnitWork.SaveAsync();
            }
        }

        /// <summary>
        /// 生产单基本信息
        /// </summary>
        /// <param name="docEntry"></param>
        /// <returns></returns>
        public async Task<TableData> GetOworDetail(int docEntry)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var query = from a in UnitWork.Find<product_owor>(null)
                        join b in UnitWork.Find<store_owhs>(null) on new { a.sbo_id, WhsCode = a.Warehouse } equals new { sbo_id = b.sbo_id.Value, b.WhsCode } into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<crm_ocrd>(null) on new { a.sbo_id, a.CardCode } equals new { sbo_id = c.sbo_id.Value, c.CardCode } into ac
                        from c in ac.DefaultIfEmpty()
                        join d in UnitWork.Find<sale_rdr1>(null) on new { a.sbo_id, a.ItemCode, DocEntry = a.OriginAbs.Value } equals new { d.sbo_id, d.ItemCode, d.DocEntry } into ad
                        from d in ad.DefaultIfEmpty()
                        join f in UnitWork.Find<sale_ordr>(null) on new { a.OriginAbs, a.sbo_id } equals new { OriginAbs = f.DocEntry, f.sbo_id } into af
                        from f in af.DefaultIfEmpty()
                        join e in UnitWork.Find<crm_oslp>(null) on new { f.SlpCode, f.sbo_id } equals new { SlpCode = (short?)e.SlpCode, sbo_id = e.sbo_id.Value } into fe
                        from e in fe.DefaultIfEmpty()
                        where a.sbo_id == Define.SBO_ID && a.DocEntry == docEntry
                        select new { a.ItemCode, a.U_BOM_BBH, a.txtitemName, a.PlannedQty, SaleOrderNo = a.OriginAbs, a.CardCode, a.Comments, a.PostDate, a.DueDate, a.CmpltQty, c.CardName, b.WhsName, a.Status, Quantity = d == null ? 0 : d.Quantity, e.SlpName };
            result.Data = query;
            return result;
        }

        /// <summary>
        /// 生产进度
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetScheduleInfo(QueryProductionScheduleReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            var schedule = await UnitWork.Find<ProductionSchedule>(c => c.DocEntry == req.DocEntry)
                .WhereIf(!string.IsNullOrWhiteSpace(req.GeneratorCode), c => c.GeneratorCode == req.GeneratorCode)
                .WhereIf(!string.IsNullOrWhiteSpace(req.ReceiveStatus), c => c.ReceiveStatus == int.Parse(req.ReceiveStatus))
                .WhereIf(!string.IsNullOrWhiteSpace(req.ProductionStatus), c => c.ProductionStatus == int.Parse(req.ProductionStatus))
                .ToListAsync();
            schedule.ForEach(c =>
            {
                var nwcail = c.NwcailStatus != 2 ? CheckCalibration(c.GeneratorCode) : (c.NwcailStatus, c.NwcailOperatorId, c.NwcailOperator, c.NwcailTime);
                var device = c.DeviceStatus != 3 ? BakingMachine(c.DocEntry.Value, c.GeneratorCode) : (c.DeviceStatus, c.DeviceOperatorId, c.DeviceOperator, c.DeviceTime);

                c.DeviceStatus = device.DeviceStatus;
                c.DeviceOperatorId = device.DeviceOperatorId;
                c.DeviceOperator = device.DeviceOperator;
                c.DeviceTime = device.DeviceTime;
                c.NwcailStatus = nwcail.NwcailStatus;
                c.NwcailOperatorId = nwcail.NwcailOperatorId;
                c.NwcailOperator = nwcail.NwcailOperator;
                c.NwcailTime = nwcail.NwcailTime;
            });

            await UnitWork.BatchUpdateAsync(schedule.ToArray());
            await UnitWork.SaveAsync();
            if (!string.IsNullOrWhiteSpace(req.DeviceStatus))
            {
                schedule = schedule.Where(c => c.DeviceStatus == int.Parse(req.DeviceStatus)).ToList();
            }
            if (!string.IsNullOrWhiteSpace(req.NwcailStatus))
            {
                schedule = schedule.Where(c => c.NwcailStatus == int.Parse(req.NwcailStatus)).ToList();
            }

            result.Count = schedule.Count();
            result.Data = schedule.Select(c => new
            {
                c.GeneratorCode,
                c.ProductionStatus,
                c.DeviceOperator,
                c.DeviceStatus,
                DeviceTime = c.DeviceTime?.ToString("yyyy.MM.dd HH:mm"),
                c.NwcailStatus,
                c.NwcailOperator,
                NwcailTime = c.NwcailTime?.ToString("yyyy.MM.dd HH:mm"),
                c.ReceiveNo,
                c.ReceiveOperator,
                c.ReceiveStatus,
                ReceiveTime = c.ReceiveTime?.ToString("yyyy.MM.dd HH:mm"),
                c.SortNo
            }).OrderBy(c => c.SortNo).Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            return result;
        }

        /// <summary>
        /// 生产唯一码下 烤机过的下位机guid
        /// </summary>
        /// <param name="wo"></param>
        /// <returns></returns>
        public List<string> GetLowGuid(string wo)
        {
            List<string> guidList = new List<string>();
            var newlog = UnitWork.Find<DeviceTestLog>(c => c.GeneratorCode == wo).OrderByDescending(c => c.Id).FirstOrDefault();
            if (newlog != null)
            {
                //最新环境下 最新通道测试记录
                var guidSql = $@"select LowGuid from devicetestlog where id in(
                                select MAX(Id) id from devicetestlog where EdgeGuid='{newlog.EdgeGuid}' and SrvGuid='{newlog.SrvGuid}' and DevUid={newlog.DevUid} AND GeneratorCode='{wo}'
                                group by EdgeGuid,SrvGuid,DevUid,UnitId,ChlId)
                                group by LowGuid";
                guidList = UnitWork.Query<DeviceTestLog>(guidSql).Select(c => c.LowGuid).ToList();
            }
            return guidList;
        }

        /// <summary>
        /// 获取下位机及烤机状态
        /// </summary>
        /// <param name="wo"></param>
        /// <returns></returns>
        public async Task<TableData> GetLowGuidAndStatus(string wo)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            List<GuidStatus> guidStatuses = new List<GuidStatus>();
            var guidList = GetLowGuid(wo);
            if (guidList.Count > 0)
            {
                foreach (var item in guidList)
                {
                    var resList = await GetChlIdResult(item);
                    var status = resList.Sum(c => c.ErrCount) > 0 ? 1 : 0;
                    guidStatuses.Add(new GuidStatus { Guid = item, Status = status });
                }
            }
            result.Data = guidStatuses;
            return result;
        }

        /// <summary>
        /// 获取通道结果
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public async Task<List<CheckResultDto>> GetChlIdResult(string guid)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            TableData result = new TableData();
            List<CheckResultDto> checkResult = new List<CheckResultDto>();

            //下位机最新的烤机环境下烤机记录
            var newlog = UnitWork.Find<DeviceTestLog>(c => c.LowGuid == guid).OrderByDescending(c => c.Id).FirstOrDefault();
            if (newlog != null)
            {
                var channel = UnitWork.Find<DeviceTestLog>(c => c.EdgeGuid == newlog.EdgeGuid && c.SrvGuid == newlog.SrvGuid && c.DevUid == newlog.DevUid && c.UnitId == newlog.UnitId && c.LowGuid == guid).ToList();
                //通道最新测试ID
                var channelQuery = channel.GroupBy(c => c.ChlId).Select(c => c.OrderByDescending(o => o.TestId).First()).ToList();
                var channelCount = 0;
                var url = "https://analytics.neware.com.cn/";
                HttpHelper httpHelper = new HttpHelper(url);
                foreach (var item in channelQuery)
                {
                    //获取每个通道测试任务id
                    var checktask = $"select EdgeGuid,SrvGuid,DevUid,UnitId,ChlId,TestId,TaskId from devicechecktask where EdgeGuid='{item.EdgeGuid}' and SrvGuid='{item.SrvGuid}' and DevUid={item.DevUid} and UnitId={item.UnitId} and ChlId={item.ChlId} and TestId={item.TestId}";
                    var checktaskQuery = UnitWork.Query<DeviceCheckTask>(checktask).Select(c => c.TaskId).FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(checktaskQuery))
                    {
                        var taskurl = $"api/DataCheck/TaskResult?id={checktaskQuery}";
                        Dictionary<string, string> dic = null;
                        //获取通道烤机结果
                        var taskResult = httpHelper.Get(dic, taskurl);
                        JObject resObj = JObject.Parse(taskResult);
                        if (resObj["status"] == null || resObj["status"].ToString() != "200")
                        {
                            continue;
                        }
                        if (resObj["data"] != null)
                        {
                            var checkDto = JsonHelper.Instance.Deserialize<CheckResultDto>(resObj["data"].ToString());
                            checkDto.ChlId = item.ChlId;
                            checkDto.CheckItems = checkDto.CheckItems.Where(s => s.CheckType != 3 && s.CheckType != 4).ToList();
                            checkDto.ErrCount = checkDto.CheckItems.Sum(c => c.ErrCount);
                            checkResult.Add(checkDto);
                        }
                    }
                }
            }
            result.Data = checkResult;
            return checkResult;
        }


        /// <summary>
        /// 烤机记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> BakingMachineRecord(QueryBakingMachineRecordReq req)
        {
            TableData result = new TableData();
            List<object> list = new List<object>();
            List<long> productionOrder = new List<long>();
            List<long> productionOrderList = new List<long>();
            if (req.OriginAbs != 0)
            {
                productionOrder = await UnitWork.Find<product_owor>(null).Where(c => c.OriginAbs == req.OriginAbs).Select(c => (long)c.DocEntry).ToListAsync();
            }
            if (!string.IsNullOrWhiteSpace(req.ItemCode))
            {
                productionOrderList = await UnitWork.Find<product_owor>(null).Where(c => c.ItemCode.Contains(req.ItemCode)).Select(c => (long)c.DocEntry).ToListAsync();
            }
            string urls = "http://service.neware.cloud/common/DevGuidBySn";
            HttpHelper helper = new HttpHelper(urls);
            List<WmsLowGuidResp> wmsLowGuids = new List<WmsLowGuidResp>();
            //sn 获取guid
            if (!string.IsNullOrWhiteSpace(req.Sn))
            {
                var b01List = new List<string>();
                b01List.Add(req.Sn);
                var wmsAccessToken = _stepVersionApp.WmsAccessToken();
                if (string.IsNullOrWhiteSpace(wmsAccessToken))
                {
                    throw new Exception($"WMS token 获取失败!");
                }
                var datastr = helper.PostAuthentication(b01List.ToArray(), urls, wmsAccessToken);
                JObject dataObj = JObject.Parse(datastr);
                try
                {
                    wmsLowGuids = JsonConvert.DeserializeObject<List<WmsLowGuidResp>>(JsonConvert.SerializeObject(dataObj["data"]["devBindInfo"]));
                }
                catch (Exception ex)
                {
                    result.Code = 500;
                    result.Message = $"wms sn获取guid失败! message={ex.Message}";
                    return result;
                }
            }
            var wmsGuidList = wmsLowGuids.Select(c => c.devGuid).ToList();
            var query = (from a in UnitWork.Find<DeviceTestLog>(null)
                         join b in UnitWork.Find<DeviceCheckTask>(null) on new { a.EdgeGuid, a.SrvGuid, a.DevUid, a.UnitId, a.TestId, a.ChlId, a.LowGuid } equals new { b.EdgeGuid, b.SrvGuid, b.DevUid, b.UnitId, b.TestId, b.ChlId, b.LowGuid }
                         where a.CreateTime >= req.StartTime && a.CreateTime <= req.EndTime
                         select new { a.Id, a.OrderNo, a.GeneratorCode, a.Department, a.MidGuid, a.LowGuid, a.DevUid, a.UnitId, a.ChlId, a.TestId, a.CreateTime, b.TaskId, a.CreateUser })
                       .WhereIf(req.OriginAbs != 0, c => productionOrder.Contains(c.OrderNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.GeneratorCode), c => c.GeneratorCode.Contains(req.GeneratorCode))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), c => productionOrderList.Contains(c.OrderNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.Sn), c => wmsGuidList.Contains(c.LowGuid))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.Operator), c => c.CreateUser.Contains(req.Operator))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.OrgName), c => c.Department.ToUpper() == req.OrgName.ToUpper())
                       .WhereIf(!string.IsNullOrWhiteSpace(req.Guid), c => c.LowGuid.Contains(req.Guid) || c.MidGuid.Contains(req.Guid));
            result.Count = query.Count();
            var taskList = req.State == 0 ? query.OrderByDescending(c => c.Id).Skip((req.page - 1) * req.limit).Take(req.limit).ToList() : query.OrderByDescending(c => c.Id).ToList();
            var orderIds = taskList.Select(c => c.OrderNo).Distinct().ToList();
            var taskIds = taskList.Where(c => !string.IsNullOrWhiteSpace(c.TaskId)).Select(c => c.TaskId).Distinct().ToList();
            var orderList = await UnitWork.Find<product_owor>(null).Where(c => orderIds.Contains(c.DocEntry)).Select(c => new { c.DocEntry, c.OriginAbs, c.ItemCode }).ToListAsync();
            string url = $"{_appConfiguration.Value.AnalyticsUrl}api/check/report";
            object re = null;
            switch (req.State)
            {
                case 0:
                    re = null;
                    break;
                case 1:
                    re = true;
                    break;
                case 2:
                    re = false;
                    break;
            }
            var taskData = helper.Post(new
            {
                PageSize = req.limit,
                Page = req.State != 0 ? req.page : 1,
                Result = re,
                TaskIDs = taskIds
            }, url, "", "");
            JObject taskObj = JObject.Parse(taskData);
            if (taskObj == null || taskObj["status"].ToString() != "200")
            {
                result.Code = 500;
                result.Message = $"数据分析烤机列表接口异常!";
                return result;
            }
            //guid 获取sn TO DO
            var wmsAccessToken2 = _stepVersionApp.WmsAccessToken();
            if (string.IsNullOrWhiteSpace(wmsAccessToken2))
            {
                throw new Exception($"wms guid获取sn token 获取失败!");
            }
            string url2 = "http://service.neware.cloud/common/DevSnByGuid";
            var guids = taskList.Select(c => c.LowGuid).Distinct().ToArray();
            var datastr2 = helper.PostAuthentication(guids, url2, wmsAccessToken2);
            JObject dataObj2 = JObject.Parse(datastr2);
            List<WmsLowGuidResp> wmsSnGuids = new List<WmsLowGuidResp>();
            try
            {
                wmsSnGuids = JsonConvert.DeserializeObject<List<WmsLowGuidResp>>(JsonConvert.SerializeObject(dataObj2["data"]["devBindInfo"]));
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = $"wms guid获取sn失败! message={ex.Message}";
                return result;
            }
            if (req.State == 1 || req.State == 2)
            {
                result.Count = Convert.ToInt32(taskObj["data"]["Total"]);
            }
            foreach (var item in taskList)
            {
                var records = taskObj["data"]["records"].FirstOrDefault(c => c["taskID"].ToString() == item.TaskId);
                var orderInfo = orderList.FirstOrDefault(c => c.DocEntry == item.OrderNo);
                var snInfo = wmsSnGuids.FirstOrDefault(c => c.devGuid == item.LowGuid);
                if ((req.State == 1 || req.State == 2) && records == null)
                {
                    continue;
                }
                list.Add(new
                {
                    OriginAbs = orderInfo.OriginAbs == 0 ? "" : orderInfo.OriginAbs.ToString(),
                    ItemCode = orderInfo == null ? "" : orderInfo.ItemCode,
                    item.GeneratorCode,
                    Department= item.CreateUser== "杨想来"?"P8":item.Department,
                    item.TaskId,
                    item.MidGuid,
                    item.LowGuid,
                    item.DevUid,
                    item.UnitId,
                    item.ChlId,
                    item.TestId,
                    item.CreateTime,
                    item.CreateUser,
                    begin = records == null ? "" :Convert.ToInt64(records["begin"]).GetTimeSpmpToDate().ToString("yyyy.MM.dd HH:mm:ss"),
                    end = records == null ? "" : Convert.ToInt64(records["end"]).GetTimeSpmpToDate().ToString("yyyy.MM.dd HH:mm:ss"),
                    result = records == null ? "" : (records["result"].ToString().Equal("OK") ? "通过" : "失败"),
                    power = records == null ? 0 : Math.Round((Convert.ToDecimal(records["power"]) / 1000), 6),
                    carbon = records == null ? 0 : records["carbon"],
                    duration = records == null ? 0 : records["duration"],
                    sn = snInfo == null ? "" : snInfo.sn
                });
            }
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 获取烤机失败原因
        /// </summary>
        /// <param name="id">检测任务id</param>
        /// <returns></returns>
        public TableData BakingMachineFailMessage(long id)
        {
            TableData result = new TableData();
            List<object> list = new List<object>();
            string url = $"{_appConfiguration.Value.AnalyticsUrl}api/DataCheck/TaskResult?id={id}";
            HttpHelper helper = new HttpHelper(url);
            Dictionary<string, string> dic = null;
            var taskResult = helper.Get(dic, url);
            JObject resObj = JObject.Parse(taskResult);
            if (resObj["status"] == null || resObj["status"].ToString() != "200")
            {
                result.Message = $"任务【{id}】通道获取检测结果失败!";
                result.Code = 500;
                return result;
            }
            var m = resObj["data"]["CheckItems"];
            foreach (var item in m)
            {
                if ((int)item["ErrCount"]>0)
                {
                    list.AddRange(item["Records"]);
                }
            }
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 导出烤机记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<byte[]> ExportBakingMachineRecord(QueryBakingMachineRecordReq req)
        {
            TableData result = new TableData();
            List<ExportBakingMachineRecordResp> list = new List<ExportBakingMachineRecordResp>();
            List<long> productionOrder = new List<long>();
            List<long> productionOrderList = new List<long>();
            if (req.OriginAbs != 0)
            {
                productionOrder = await UnitWork.Find<product_owor>(null).Where(c => c.OriginAbs == req.OriginAbs).Select(c => (long)c.DocEntry).ToListAsync();
            }
            if (!string.IsNullOrWhiteSpace(req.ItemCode))
            {
                productionOrderList = await UnitWork.Find<product_owor>(null).Where(c => c.ItemCode.Contains(req.ItemCode)).Select(c => (long)c.DocEntry).ToListAsync();
            }
            string urls = "http://service.neware.cloud/common/DevGuidBySn";
            HttpHelper helper = new HttpHelper(urls);
            List<WmsLowGuidResp> wmsLowGuids = new List<WmsLowGuidResp>();
            if (!string.IsNullOrWhiteSpace(req.Sn))
            {
                var b01List = new List<string>();
                b01List.Add(req.Sn);
                var wmsAccessToken = _stepVersionApp.WmsAccessToken();
                if (string.IsNullOrWhiteSpace(wmsAccessToken))
                {
                    throw new Exception($"WMS token 获取失败  {urls}!");
                }
                var datastr = helper.PostAuthentication(b01List.ToArray(), urls, wmsAccessToken);
                JObject dataObj = JObject.Parse(datastr);
                try
                {
                    wmsLowGuids = JsonConvert.DeserializeObject<List<WmsLowGuidResp>>(JsonConvert.SerializeObject(dataObj["data"]["devBindInfo"]));
                }
                catch (Exception ex)
                {
                    throw new Exception($"wms sn获取guid失败! message={ex.Message}");
                }
            }
            var wmsGuidList = wmsLowGuids.Select(c => c.devGuid).ToList();
            var taskList = (from a in UnitWork.Find<DeviceTestLog>(null)
                            join b in UnitWork.Find<DeviceCheckTask>(null) on new { a.EdgeGuid, a.SrvGuid, a.DevUid, a.UnitId, a.TestId, a.ChlId, a.LowGuid } equals new { b.EdgeGuid, b.SrvGuid, b.DevUid, b.UnitId, b.TestId, b.ChlId, b.LowGuid }
                            where a.CreateTime >= req.StartTime && a.CreateTime <= req.EndTime
                            select new { a.Id, a.OrderNo, a.GeneratorCode, a.Department, a.MidGuid, a.LowGuid, a.DevUid, a.UnitId, a.ChlId, a.TestId, a.CreateTime, b.TaskId, a.CreateUser })
                       .WhereIf(req.OriginAbs != 0, c => productionOrder.Contains(c.OrderNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.GeneratorCode), c => c.GeneratorCode.Contains(req.GeneratorCode))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.ItemCode), c => productionOrderList.Contains(c.OrderNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.Sn), c => wmsGuidList.Contains(c.LowGuid))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.Operator), c => c.CreateUser.Contains(req.Operator))
                       .WhereIf(!string.IsNullOrWhiteSpace(req.OrgName), c => c.Department.ToUpper() == req.OrgName.ToUpper())
                       .WhereIf(!string.IsNullOrWhiteSpace(req.Guid), c => c.LowGuid.Contains(req.Guid) || c.MidGuid.Contains(req.Guid))
                       .OrderByDescending(c => c.Id);
            var orderIds = taskList.Select(c => c.OrderNo).Distinct().ToList();
            var taskIds = taskList.Where(c => !string.IsNullOrWhiteSpace(c.TaskId)).Select(c => c.TaskId).Distinct().ToList();
            var orderList = await UnitWork.Find<product_owor>(null).Where(c => orderIds.Contains(c.DocEntry)).Select(c => new { c.DocEntry, c.OriginAbs, c.ItemCode }).ToListAsync();
            string url = $"{_appConfiguration.Value.AnalyticsUrl}api/check/report";
            object re = null;
            switch (req.State)
            {
                case 0:
                    re = null;
                    break;
                case 1:
                    re = true;
                    break;
                case 2:
                    re = false;
                    break;
            }
            var taskData = helper.Post(new
            {
                PageSize = taskList.Count(),
                Page = 1,
                Result = re,
                TaskIDs = taskIds
            }, url, "", "");
            JObject taskObj = JObject.Parse(taskData);
            if (taskObj == null || taskObj["status"].ToString() != "200")
            {
                throw new Exception($"数据分析烤机列表接口异常check/report!");
            }
            var wmsAccessToken2 = _stepVersionApp.WmsAccessToken();
            if (string.IsNullOrWhiteSpace(wmsAccessToken2))
            {
                throw new Exception($"wms guid获取sn token 获取失败!");
            }
            string url2 = "http://service.neware.cloud/common/DevSnByGuid";
            var guids = taskList.Select(c => c.LowGuid).Distinct().ToArray();
            var datastr2 = helper.PostAuthentication(guids, url2, wmsAccessToken2);
            JObject dataObj2 = JObject.Parse(datastr2);
            List<WmsLowGuidResp> wmsSnGuids = new List<WmsLowGuidResp>();
            try
            {
                wmsSnGuids = JsonConvert.DeserializeObject<List<WmsLowGuidResp>>(JsonConvert.SerializeObject(dataObj2["data"]["devBindInfo"]));
            }
            catch (Exception ex)
            {
                throw new Exception($"wms guid获取sn失败!{url2} message={ex.Message}");
            }

            foreach (var item in taskList)
            {
                var records = taskObj["data"]["records"].FirstOrDefault(c => c["taskID"].ToString() == item.TaskId);
                var orderInfo = orderList.FirstOrDefault(c => c.DocEntry == item.OrderNo);
                var snInfo = wmsSnGuids.FirstOrDefault(c => c.devGuid == item.LowGuid);
                list.Add(new ExportBakingMachineRecordResp
                {
                    OriginAbs = orderInfo.OriginAbs == 0 ? "" : orderInfo.OriginAbs.ToString(),
                    ItemCode = orderInfo == null ? "" : orderInfo.ItemCode,
                    GeneratorCode = item.GeneratorCode,
                    Department = item.CreateUser == "杨想来" ? "P8" : item.Department,
                    CreateUser = item.CreateUser,
                    TaskId = item.TaskId,
                    MidGuid = item.MidGuid,
                    LowGuid = item.LowGuid,
                    DevUid = item.DevUid,
                    UnitId = item.UnitId,
                    ChlId = item.ChlId,
                    TestId = item.TestId,
                    begin = records == null ? "" : Convert.ToInt64(records["begin"]).GetTimeSpmpToDate().ToString("yyyy.MM.dd HH:mm:ss"),
                    end = records == null ? "" : Convert.ToInt64(records["end"]).GetTimeSpmpToDate().ToString("yyyy.MM.dd HH:mm:ss"),
                    result = records == null ? "" : (records["result"].ToString().Equal("OK") ? "通过" : "失败"),
                    power = records == null ? "" : Math.Round((Convert.ToDecimal(records["power"])/1000),6).ToString(),
                    carbon = records == null ? "" : records["carbon"].ToString(),
                    duration = records == null ? "" : records["duration"].ToString(),
                    sn = snInfo == null ? "" : snInfo.sn
                });
            }
            IExporter exporter = new ExcelExporter();
            var bytes = await exporter.ExportAsByteArray(list);
            return bytes;
        }

        /// <summary>
        /// 校准绩效明细表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> CalibrationPerformanceReport(QueryCalibrationPerformanceReq req)
        {
            TableData result = new TableData();
            List<object> list = new List<object>();
            string url = $"{_appConfiguration.Value.AnalyticsReportUrl}api/Calibration/c-report2";
            HttpHelper helper = new HttpHelper(url);
            var taskData = helper.Post(new
            {
                beginTime = ((DateTimeOffset)req.StartTime.Value).ToUnixTimeSeconds(),
                endTime = ((DateTimeOffset)req.EndTime.Value).ToUnixTimeSeconds(),
                pageSize = req.limit,
                page = req.page
            }, url, "", "");
            JObject taskObj = JObject.Parse(taskData);
            if (taskObj == null || taskObj["code"].ToString() != "1001")
            {
                result.Code = 500;
                result.Message = $"{url} 校准绩效明细表接口异常!";
                return result;
            }
            result.Count = Convert.ToInt32(taskObj["data"]["total"]);
            var erpUserIds = taskObj["data"]["records"].Select(c => c["userId"].ToString()).ToList().Distinct();
            var userList = await (from b in UnitWork.Find<User>(null)
                                  join c in UnitWork.Find<Relevance>(null) on b.Id equals c.FirstId
                                  join d in UnitWork.Find<Repository.Domain.Org>(null) on c.SecondId equals d.Id
                                  where erpUserIds.Contains(b.Id) && c.Key == Define.USERORG
                                  select new { userName = b.Name, orgName = d.Name, b.Id }).ToListAsync();
            foreach (var item in taskObj["data"]["records"])
            {
                var userInfo = userList.Where(c => c.Id == item["userId"].ToString()).FirstOrDefault();
                list.Add(new
                {
                    userId = item["userId"].ToString(),
                    userName = userInfo == null ? "" : userInfo.userName,
                    orgName = userInfo == null ? "" : userInfo.orgName,
                    devCount = item["devCount"].ToString(),
                    taskOk = item["taskOk"].ToString(),
                    taskNg = item["taskNg"].ToString(),
                    taskAuto = item["taskAuto"].ToString(),
                    taskHd = item["taskHd"].ToString(),
                    devAuto = item["devAuto"].ToString(),
                    devHd = item["devHd"].ToString(),
                    autoSpend = item["autoSpend"].ToString(),
                    hdSpend = item["hdSpend"].ToString(),
                    autoAvgSpend = item["autoAvgSpend"].ToString(),
                    hdAvgSpend = item["hdAvgSpend"].ToString(),
                    autoChl = item["autoChl"].ToString(),
                    hdChl = item["hdChl"].ToString(),
                });
            }
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 导出校准绩效明细表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportCalibrationPerformanceReport(QueryCalibrationPerformanceReq req)
        {
            TableData result = new TableData();
            List<CalibrationPerformanceResp> list = new List<CalibrationPerformanceResp>();
            string url = $"{_appConfiguration.Value.AnalyticsReportUrl}api/Calibration/c-report2";
            HttpHelper helper = new HttpHelper(url);
            var taskData = helper.Post(new
            {
                beginTime = ((DateTimeOffset)req.StartTime.Value).ToUnixTimeSeconds(),
                endTime = ((DateTimeOffset)req.EndTime.Value).ToUnixTimeSeconds(),
                pageSize = 500,
                page =1
            }, url, "", "");
            JObject taskObj = JObject.Parse(taskData);
            if (taskObj == null || taskObj["code"].ToString() != "1001")
            {
                throw new Exception($"{url} 校准绩效明细表接口异常!");
            }
            var erpUserIds = taskObj["data"]["records"].Select(c => c["userId"].ToString()).ToList().Distinct();
            var userList = await (from b in UnitWork.Find<User>(null)
                                  join c in UnitWork.Find<Relevance>(null) on b.Id equals c.FirstId
                                  join d in UnitWork.Find<Repository.Domain.Org>(null) on c.SecondId equals d.Id
                                  where erpUserIds.Contains(b.Id) && c.Key == Define.USERORG
                                  select new { userName = b.Name, orgName = d.Name, b.Id }).ToListAsync();
            foreach (var item in taskObj["data"]["records"])
            {
                var userInfo = userList.Where(c => c.Id == item["userId"].ToString()).FirstOrDefault();
                list.Add(new CalibrationPerformanceResp
                {
                    userName = userInfo == null ? "" : userInfo.userName,
                    orgName = userInfo == null ? "" : userInfo.orgName,
                    devCount = item["devCount"].ToString(),
                    taskOk = item["taskOk"].ToString(),
                    taskNg = item["taskNg"].ToString(),
                    taskAuto = item["taskAuto"].ToString(),
                    taskHd = item["taskHd"].ToString(),
                    devAuto = item["devAuto"].ToString(),
                    devHd = item["devHd"].ToString(),
                    autoSpend = item["autoSpend"].ToString(),
                    hdSpend = item["hdSpend"].ToString(),
                    autoAvgSpend = item["autoAvgSpend"].ToString(),
                    hdAvgSpend = item["hdAvgSpend"].ToString(),
                    autoChl = item["autoChl"].ToString(),
                    hdChl = item["hdChl"].ToString(),
                });
            }
            IExporter exporter = new ExcelExporter();
            var bytes = await exporter.ExportAsByteArray(list);
            return bytes;
        }

        /// <summary>
        /// 校准明细报表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> CalibrationReport(QueryCalibrationReportReq req)
        {
            TableData result = new TableData();
            List<object> list = new List<object>();
            List<string> userIds = new List<string>();
            List<string> sns = new List<string>();
            if (!string.IsNullOrWhiteSpace(req.OrgId))
            {
                var query = await UnitWork.Find<Relevance>(null).Where(c => c.Key == Define.USERORG && c.SecondId == req.OrgId).Select(c => c.FirstId).ToListAsync();
                userIds.AddRange(query);
            }
            if (!string.IsNullOrWhiteSpace(req.Operator))
            {
                var query = await UnitWork.Find<User>(null).Where(c => c.Name.Contains(req.Operator)).Select(c => c.Id).ToListAsync();
                userIds.AddRange(query);
            }
            if (req.SalesOrder>0)
            {
                string strSql = @"select t4.SlpCode,t4.SlpName as 'Salesman',t3.DocEntry as'SalesOrder',t2.DocEntry as 'DeliveryNumber', t1.manufSN as 'TesterSn'from OINS t1
                    inner join (SELECT DocEntry,MIN(BaseEntry) as BaseEntry from DLN1 where BaseType=17  group by DocEntry )  t2 on t1.deliveryNo = t2.DocEntry 
                    inner join ORDR t3 on t2.BaseEntry = t3.DocEntry
                    inner join OSLP t4 on t3.SlpCode =t4.SlpCode where t3.DocEntry="+ req.SalesOrder;
                var shipmentCalibration = await UnitWork.Query<ShipmentCalibration_sql>(strSql).ToListAsync();
                sns = shipmentCalibration.Select(c => c.TesterSn).ToList();
            }
            var ids = userIds.Distinct();
            string url = $"{_appConfiguration.Value.AnalyticsReportUrl}api/Calibration/c-report-page";
            HttpHelper helper = new HttpHelper(url);
            var taskData = helper.Post(new
            {
                sns=sns,
                passportIDs = ids,
                beginTime = ((DateTimeOffset)req.StartTime.Value).ToUnixTimeSeconds(),
                endTime = ((DateTimeOffset)req.EndTime.Value).ToUnixTimeSeconds(),
                pageSize =req.limit,
                page=req.page,
                taskType=req.taskType==0?null:req.taskType
            }, url, "", "");
            JObject taskObj = JObject.Parse(taskData);
            if (taskObj == null || taskObj["code"].ToString() != "1001")
            {
                result.Code = 500;
                result.Message = $"{url},获取人员校准报表接口异常!";
                return result;
            }
            result.Count = Convert.ToInt32(taskObj["data"]["total"]);
            if (result.Count<=0)
            {
                result.Data = list;
                return result;
            }
            var erpUserIds = taskObj["data"]["records"].Select(c =>c["userId"].ToString()).ToList().Distinct();
            var userList = await (from b in UnitWork.Find<User>(null)
                                  join c in UnitWork.Find<Relevance>(null) on b.Id equals c.FirstId
                                  join d in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on c.SecondId equals d.Id
                                  where erpUserIds.Contains(b.Id) && c.Key == Define.USERORG
                                  select new { userName = b.Name, orgName = d.Name,b.Id}).ToListAsync();
            var taskIds = taskObj["data"]["records"].Select(c => c["taskId"].ToString()).ToList().Distinct();
            var codelist = await (from a in UnitWork.Find<DeviceCheckTask>(null)
                                  join b in UnitWork.Find<DeviceTestLog>(null) on new { a.EdgeGuid, a.SrvGuid, a.DevUid, a.UnitId, a.TestId, a.ChlId, a.LowGuid } equals new { b.EdgeGuid, b.SrvGuid, b.DevUid, b.UnitId, b.TestId, b.ChlId, b.LowGuid }
                                  where taskIds.Contains(a.TaskId)
                                  select new { b.GeneratorCode, a.TaskId }).ToListAsync();
            //获取销售信息
            List<ShipmentCalibration_sql> saleInfoList = new List<ShipmentCalibration_sql>();
            var serialNoList = taskObj["data"]["records"].Where(c => c["serialNo"] != null).Select(c => c["serialNo"].ToString()).Distinct().ToList();
            if (serialNoList.Count>0)
            {
                string querySql = @"select t4.SlpCode,t4.SlpName as 'Salesman',t3.DocEntry as'SalesOrder',t2.DocEntry as 'DeliveryNumber', t1.manufSN as 'TesterSn'from OINS t1
                    inner join (SELECT DocEntry,MIN(BaseEntry) as BaseEntry from DLN1 where BaseType=17  group by DocEntry )  t2 on t1.deliveryNo = t2.DocEntry 
                    inner join ORDR t3 on t2.BaseEntry = t3.DocEntry
                    inner join OSLP t4 on t3.SlpCode =t4.SlpCode ";
                for (int i = 0; i < serialNoList.Count; i++)
                {
                    serialNoList[i] = "'" + serialNoList[i] + "'";
                }
                var propertyStr = string.Join(',', serialNoList);
                querySql += $" where t1.manufSN in ({propertyStr})";
                saleInfoList = await UnitWork.Query<ShipmentCalibration_sql>(querySql).ToListAsync();
            }
            //校准信息
            var lowGuids = taskObj["data"]["records"].Select(c => c["lowGuid"].ToString()).ToList();

            var nwcalibase = await (from a in UnitWork.Find<PcPlc>(null)
                                    join b in UnitWork.Find<NwcaliBaseInfo>(null) on a.NwcaliBaseInfoId equals b.Id
                                    where lowGuids.Contains(a.Guid)
                                    select new { b.Issuer,b.IssuerId, a.Guid, b.TesterModel }).Distinct().ToListAsync();
            foreach (var item in taskObj["data"]["records"])
            {
                var userInfo = userList.Where(c => c.Id == item["userId"].ToString()).FirstOrDefault();
                var codeInfo = codelist.Where(c => c.TaskId == item["taskId"].ToString()).FirstOrDefault();
                var nwcalibaseinfo = item["serialNo"] == null ? null: nwcalibase.Where(c => c.Guid == item["lowGuid"].ToString()).FirstOrDefault();
                var saleInfo = item["serialNo"] == null ? null: saleInfoList.Where(c => c.TesterSn == item["serialNo"].ToString()).FirstOrDefault();
                list.Add(new
                {
                    userId = item["userId"].ToString(),
                    taskSubId = item["taskSubId"].ToString(),
                    chlId = item["chlId"].ToString(),
                    beginTime = DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item["beginTime"])).LocalDateTime,
                    endTime =DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item["endTime"])).LocalDateTime,
                    beginTimeStamp = Convert.ToInt64(item["beginTime"]),
                    endTimeStamp = Convert.ToInt64(item["endTime"]),
                    lowGuid = item["lowGuid"].ToString(),
                    lowVer = item["lowVer"].ToString(),
                    midGuid = item["lowVer"].ToString(),
                    midVer = item["lowVer"].ToString(),
                    assetInfo = item["assetInfo"].ToString(),
                    conclusion = item["conclusion"].ToString(),
                    duration = item["duration"].ToString(),
                    userName = userInfo == null ? "" : userInfo.userName,
                    orgName = userInfo == null ? "" : userInfo.orgName,
                    generatorCode = codeInfo == null ? "" : codeInfo.GeneratorCode,
                    taskType= item["taskType"].ToString(),
                    serialNo= item["serialNo"]==null?"": item["serialNo"].ToString(),
                    salesOrder = saleInfo==null?"": saleInfo.SalesOrder.ToString(),
                    Salesman = saleInfo == null ? "" : saleInfo.Salesman,
                    DeliveryNumber = saleInfo == null ? "" : saleInfo.DeliveryNumber.ToString(),
                    Issuer= nwcalibaseinfo==null?"": nwcalibaseinfo.Issuer,
                    TesterModel= nwcalibaseinfo == null ? "" : nwcalibaseinfo.TesterModel
                });
            }
            result.Data = list;
            return result;
        }


        /// <summary>
        /// 导出校准明细报表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportCalibrationReport(QueryCalibrationReportReq req)
        {
            List<ExportCalibrationReportResp> list = new List<ExportCalibrationReportResp>();
            List<string> userIds = new List<string>();
            List<string> sns = new List<string>();
            if (!string.IsNullOrWhiteSpace(req.OrgId))
            {
                var query = await UnitWork.Find<Relevance>(null).Where(c => c.Key == Define.USERORG && c.SecondId == req.OrgId).Select(c => c.FirstId).ToListAsync();
                userIds.AddRange(query);
            }
            if (!string.IsNullOrWhiteSpace(req.Operator))
            {
                var query = await UnitWork.Find<User>(null).Where(c => c.Name.Contains(req.Operator)).Select(c => c.Id).ToListAsync();
                userIds.AddRange(query);
            }
            if (req.SalesOrder > 0)
            {
                string strSql = @"select t4.SlpCode,t4.SlpName as 'Salesman',t3.DocEntry as'SalesOrder',t2.DocEntry as 'DeliveryNumber', t1.manufSN as 'TesterSn'from OINS t1
                    inner join (SELECT DocEntry,MIN(BaseEntry) as BaseEntry from DLN1 where BaseType=17  group by DocEntry )  t2 on t1.deliveryNo = t2.DocEntry 
                    inner join ORDR t3 on t2.BaseEntry = t3.DocEntry
                    inner join OSLP t4 on t3.SlpCode =t4.SlpCode where t3.DocEntry=" + req.SalesOrder;
                var shipmentCalibration = await UnitWork.Query<ShipmentCalibration_sql>(strSql).ToListAsync();
                sns = shipmentCalibration.Select(c => c.TesterSn).ToList();
            }
            var ids = userIds.Distinct();
            string url = $"{_appConfiguration.Value.AnalyticsReportUrl}api/Calibration/c-report";
            HttpHelper helper = new HttpHelper(url);
            var taskData = helper.Post(new
            {
                sns=sns,
                passportIDs = ids,
                beginTime = req.StartTime.Value.GetTimeStamp() - 28800,
                endTime = req.EndTime.Value.GetTimeStamp() - 28800,
                taskType = req.taskType == 0 ? null : req.taskType
            }, url, "", "");
            JObject taskObj = JObject.Parse(taskData);
            if (taskObj == null || taskObj["code"].ToString() != "1001")
            {
                throw new Exception($"获取人员校准报表接口异常!");
            }
            if (taskObj["data"].Count()<=0)
            {
                throw new Exception($"暂无数据!");
            }
            var erpUserIds = taskObj["data"].Select(c => c["userId"].ToString()).Distinct().ToList();
            var userList = await (from b in UnitWork.Find<User>(null)
                                  join c in UnitWork.Find<Relevance>(null) on b.Id equals c.FirstId
                                  join d in UnitWork.Find<OpenAuth.Repository.Domain.Org>(null) on c.SecondId equals d.Id
                                  where erpUserIds.Contains(b.Id) && c.Key == Define.USERORG
                                  select new { userName = b.Name, orgName = d.Name, b.Id }).ToListAsync();
            var taskIds = taskObj["data"].Select(c => c["taskId"].ToString()).Distinct().ToList();
            var codelist = await (from a in UnitWork.Find<DeviceCheckTask>(null)
                                  join b in UnitWork.Find<DeviceTestLog>(null) on new { a.EdgeGuid, a.SrvGuid, a.DevUid, a.UnitId, a.TestId, a.ChlId, a.LowGuid } equals new { b.EdgeGuid, b.SrvGuid, b.DevUid, b.UnitId, b.TestId, b.ChlId, b.LowGuid }
                                  where taskIds.Contains(a.TaskId)
                                  select new { b.GeneratorCode, a.TaskId }).ToListAsync();
            //获取销售信息
            List<ShipmentCalibration_sql> saleInfoList = new List<ShipmentCalibration_sql>();
            var serialNoList = taskObj["data"].Where(c => c["serialNo"] != null).Select(c => c["serialNo"].ToString()).Distinct().ToList();
            if (serialNoList.Count > 0)
            {
                string querySql = @"select t4.SlpCode,t4.SlpName as 'Salesman',t3.DocEntry as'SalesOrder',t2.DocEntry as 'DeliveryNumber', t1.manufSN as 'TesterSn'from OINS t1
                    inner join (SELECT DocEntry,MIN(BaseEntry) as BaseEntry from DLN1 where BaseType=17  group by DocEntry )  t2 on t1.deliveryNo = t2.DocEntry 
                    inner join ORDR t3 on t2.BaseEntry = t3.DocEntry
                    inner join OSLP t4 on t3.SlpCode =t4.SlpCode ";
                for (int i = 0; i < serialNoList.Count; i++)
                {
                    serialNoList[i] = "'" + serialNoList[i] + "'";
                }
                var propertyStr = string.Join(',', serialNoList);
                querySql += $" where t1.manufSN in ({propertyStr})";
                saleInfoList = await UnitWork.Query<ShipmentCalibration_sql>(querySql).ToListAsync();
            }
            var lowGuids = taskObj["data"].Select(c => c["lowGuid"].ToString()).ToList();
            var nwcalibase = await (from a in UnitWork.Find<PcPlc>(null)
                                    join b in UnitWork.Find<NwcaliBaseInfo>(null) on a.NwcaliBaseInfoId equals b.Id
                                    where lowGuids.Contains(a.Guid)
                                    select new { b.Issuer, b.IssuerId, a.Guid, b.TesterModel }).Distinct().ToListAsync();
            foreach (var item in taskObj["data"])
            {
                var userInfo = userList.Where(c => c.Id == item["userId"].ToString()).FirstOrDefault();
                var codeInfo = codelist.Where(c => c.TaskId == item["taskId"].ToString()).FirstOrDefault();
                var nwcalibaseinfo = item["serialNo"] == null ? null : nwcalibase.Where(c => c.Guid == item["lowGuid"].ToString()).FirstOrDefault();
                var saleInfo = item["serialNo"] == null ? null : saleInfoList.Where(c => c.TesterSn == item["serialNo"].ToString()).FirstOrDefault();
                list.Add(new ExportCalibrationReportResp
                {
                    taskSubId = item["taskSubId"].ToString(),
                    chlId = item["chlId"].ToString(),
                    beginTime = (DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item["beginTime"])).LocalDateTime).ToString("yyyy-MM-dd HH:mm:ss"),
                    endTime = (DateTimeOffset.FromUnixTimeSeconds(Convert.ToInt64(item["endTime"])).LocalDateTime).ToString("yyyy-MM-dd HH:mm:ss"),
                    lowGuid = item["lowGuid"].ToString(),
                    lowVer = item["lowVer"].ToString(),
                    conclusion = item["conclusion"].ToString(),
                    duration = item["duration"].ToString(),
                    userName = userInfo == null ? "" : userInfo.userName,
                    orgName = userInfo == null ? "" : userInfo.orgName,
                    generatorCode = codeInfo == null ? "" : codeInfo.GeneratorCode,
                    taskType = item["taskType"].ToString()=="0"?"默认":(item["taskType"].ToString() == "1"? "自动校准" : "手动校准"),
                    TesterModel = nwcalibaseinfo == null ? "" : nwcalibaseinfo.TesterModel,
                    serialNo = item["serialNo"] == null ? "" : item["serialNo"].ToString(),
                    assetInfo = item["assetInfo"].ToString(),
                    Issuer = nwcalibaseinfo == null ? "" : nwcalibaseinfo.Issuer,
                    salesOrder = saleInfo == null ? "" : saleInfo.SalesOrder.ToString(),
                    DeliveryNumber = saleInfo == null ? "" : saleInfo.DeliveryNumber.ToString(),
                    Salesman = saleInfo == null ? "" : saleInfo.Salesman
                });
            }
            IExporter exporter = new ExcelExporter();
            var bytes = await exporter.ExportAsByteArray(list);
            return bytes;
        }

        /// <summary>
        /// 烤机结果
        /// </summary>
        /// <param name="docEntry"></param>
        /// <param name="wo"></param>
        /// <returns></returns>
        public (int Status, string UserId, string User, DateTime? Time) BakingMachine(int docEntry, string wo)
        {
            var guids = GetLowGuid(wo);
            var result = 1;//待烤机
            string u1 = "", u2 = "";
            DateTime? date = null;
            ////下位机数量
            //var wor1 = UnitWork.Find<product_wor1>(c => c.DocEntry == docEntry && c.ItemCode.Contains("XWJ")).Sum(c => c.PlannedQty);
            //if (guids.Count == 0)
            //{
            //    return (result, "", "", null);
            //}

            #region MyRegion

            //var newlog = UnitWork.Find<DeviceTestLog>(c => c.GeneratorCode == wo).OrderByDescending(c => c.Id).FirstOrDefault();
            //if (newlog != null)
            //{
            //    var url = "https://analytics.neware.com.cn/";
            //    HttpHelper httpHelper = new HttpHelper(url);
            //    var guidSuccessCount = 0;
            //    var err = 0;
            //    //最新环境下 最新下位机测试记录
            //    var guidSql = $@"select LowGuid,EdgeGuid,SrvGuid,DevUid,UnitId from devicetestlog where id in(
            //                    select MAX(Id) id from devicetestlog where EdgeGuid='{newlog.EdgeGuid}' and SrvGuid='{newlog.SrvGuid}' and DevUid={newlog.DevUid} AND GeneratorCode='{wo}'
            //                    group by EdgeGuid,SrvGuid,DevUid,UnitId,ChlId)
            //                    group by LowGuid";
            //    var guidList = UnitWork.Query<DeviceTestLog>(guidSql).Select(c => new { c.EdgeGuid, c.SrvGuid, c.DevUid, c.UnitId }).ToList();
            //    var guidCount = 0;
            //    foreach (var item in guidList)
            //    {
            //        var taskurl = $"api/DataCheck/TotalErr?edgeGuid={item.EdgeGuid}&srvGuid={item.SrvGuid}&devUid={item.DevUid}&unitId={item.UnitId}";
            //        Dictionary<string, string> dic = null;
            //        //获取下位机烤机结果
            //        var taskResult = httpHelper.Get(dic, taskurl);
            //        JObject resObj = JObject.Parse(taskResult);
            //        if (resObj["status"] == null || resObj["status"].ToString() != "200")
            //        {
            //            err = 4;
            //            break;
            //        }
            //        if (resObj["data"] != null)
            //        {
            //            int.TryParse(resObj["data"].ToString(), out int errCount);
            //            //sbyte.TryParse(resObj["data"]["Status"].ToString(), out sbyte taskStatus);
            //            if (errCount == 0)
            //                guidCount++;
            //            else if (errCount > 0)
            //            {
            //                err = 4;
            //                break;
            //            }
            //        }
            //    }

            //    if (err == 4)
            //    {
            //        return (err, "", "", null);//烤机异常
            //    }
            //    //通过数 == 所有guid数
            //    if (guidCount == guidList.Count())
            //    {
            //        result = 3;//烤机通过
            //        var last = UnitWork.Find<DeviceTestLog>(c => guids.Contains(c.LowGuid)).OrderByDescending(c => c.CreateTime).FirstOrDefault();
            //        u1 = last.CreateUserId;
            //        u2 = last.CreateUser;
            //        date = DateTime.Now;
            //    }
            //}
            //else
            //{
            //    return (result, "", "", null);
            //}
            #endregion


            if (guids.Count > 0)
            {
                var url = "https://analytics.neware.com.cn/";
                HttpHelper httpHelper = new HttpHelper(url);
                var guidSuccessCount = 0;
                var err = 0;
                List<int> dts = new List<int>();
                foreach (var guid in guids)
                {
                    List<string> taskId = new List<string>();
                    //var taskInfo=await UnitWork.Find<>
                    //下位机最新的烤机环境下烤机记录
                    var newlog = UnitWork.Find<DeviceTestLog>(c => c.LowGuid == guid).OrderByDescending(c => c.Id).FirstOrDefault();
                    if (newlog != null)
                    {
                        var channel = UnitWork.Find<DeviceTestLog>(c => c.EdgeGuid == newlog.EdgeGuid && c.SrvGuid == newlog.SrvGuid && c.DevUid == newlog.DevUid && c.UnitId == newlog.UnitId && c.LowGuid == guid).ToList();
                        //通道最新测试ID
                        var channelQuery = channel.GroupBy(c => c.ChlId).Select(c => c.OrderByDescending(o => o.TestId).First()).ToList();
                        var channelCount = 0;
                        foreach (var item in channelQuery)
                        {
                            //获取每个通道测试任务id
                            var checktask = $"select EdgeGuid,SrvGuid,DevUid,UnitId,ChlId,TestId,TaskId from devicechecktask where EdgeGuid='{item.EdgeGuid}' and SrvGuid='{item.SrvGuid}' and DevUid={item.DevUid} and UnitId={item.UnitId} and ChlId={item.ChlId} and TestId={item.TestId}";
                            var checktaskQuery = UnitWork.Query<DeviceCheckTask>(checktask).Select(c => c.TaskId).FirstOrDefault();
                            taskId.Add(checktaskQuery);

                            #region MyRegion
                            //if (!string.IsNullOrWhiteSpace(checktaskQuery))
                            //{
                            //    var taskurl = $"api/DataCheck/TaskResult?id={checktaskQuery}";
                            //    Dictionary<string, string> dic = null;
                            //    //获取通道烤机结果
                            //    var taskResult = httpHelper.Get(dic, taskurl);
                            //    JObject resObj = JObject.Parse(taskResult);
                            //    if (resObj["status"] == null || resObj["status"].ToString() != "200")
                            //    {
                            //        err = 4;
                            //        break;
                            //    }
                            //    if (resObj["data"] != null)
                            //    {
                            //        int.TryParse(resObj["data"]["ErrCount"].ToString(), out int errCount);
                            //        sbyte.TryParse(resObj["data"]["Status"].ToString(), out sbyte taskStatus);
                            //        if (errCount == 0 && taskStatus == 2)
                            //            channelCount++;
                            //        else if (errCount > 0)
                            //        {
                            //            err = 4;
                            //            break;
                            //        }
                            //    }
                            //}
                            //else
                            //{
                            //    //checkResp.Message = "烤机任务ID尚未创建。";
                            //}
                            #endregion
                        }

                        //获取guid下所有通道烤机结果
                        var param = new { IDs = taskId };
                        var taskurl = $"api/DataCheck/TaskResult";
                        var taskResult = httpHelper.Post(param, taskurl, "", "");
                        JObject resObj = JObject.Parse(taskResult);
                        if (resObj["status"] == null || resObj["status"].ToString() != "200")
                        {
                            err = 4;
                            break;
                        }
                        if (resObj["data"] != null)
                        {
                            var checkDto = JsonHelper.Instance.Deserialize<List<CheckResultDto>>(resObj["data"].ToString());
                            dts.InsertRange(dts.Count(), checkDto.Select(r => (int)r.LastTime).ToList());
                            if (checkDto.Any(c => c.Status == 0))
                            {

                            }
                            else
                            {
                                var checkItem = checkDto.Select(c => new CheckResultDto
                                {
                                    Status = c.Status,
                                    ErrCount = c.CheckItems.Where(s => s.CheckType != 3 && s.CheckType != 4).Sum(s => s.ErrCount)
                                }).ToList();
                                var errCount = checkItem.Sum(c => c.ErrCount);
                                if (errCount > 0)
                                {
                                    err = 4;
                                }
                                else
                                {
                                    if (checkItem.All(c => c.Status == 2))
                                    {
                                        guidSuccessCount++;
                                    }
                                }
                            }
                        }


                        if (err == 4)
                        {
                            result = err;//烤机异常
                            break;
                        }
                        ////烤机通道通过数=总通道数
                        //if (channelCount == channelQuery.Count)
                        //    guidSuccessCount++;
                    }
                }
                if (err == 4)
                {
                    return (result, "", "", null);
                }
                //通过数==所有guid数
                if (guidSuccessCount == guids.Count)
                {
                    result = 3;//烤机通过
                    var last = UnitWork.Find<DeviceTestLog>(c => guids.Contains(c.LowGuid)).OrderByDescending(c => c.CreateTime).FirstOrDefault();
                    u1 = last.CreateUserId;
                    u2 = last.CreateUser;
                    date = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1)).AddSeconds((long)dts.Max());
                }
                else
                    result = 2;//烤机中
            }
            else
            {
                //result = 2;//烤机中
            }
            return (result, u1, u2, date);
        }

        /// <summary>
        /// 生产唯一码下设备是否校准
        /// </summary>
        /// <param name="wo"></param>
        /// <returns></returns>
        public (int Status, string UserId, string User, DateTime? Time) CheckCalibration(string wo)
        {
            var result = 1;
            var lowGuid = GetLowGuid(wo);
            if (lowGuid.Count > 0)
            {
                var machine = UnitWork.Find<MachineInfo>(c => lowGuid.Contains(c.Guid)).ToList();
                if (machine.Count > 0)
                {
                    //下位机数量是否等于下位机校准证书数量
                    if (lowGuid.Count <= machine.Count)
                    {
                        result = 2;
                        var last = machine.OrderByDescending(c => c.CreateTime).FirstOrDefault();
                        return (result, last.CreateUserId, last.CreateUser, last.CalibrationTime);
                    }
                }
            }
            return (result, "", "", null);
        }

        /// <summary>
        /// 生产收货
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task ProductionReceipt(List<ReceiveReq> list)
        {
            var nsapId = list.Select(c => c.Operator).ToList();
            var userInfo = from a in UnitWork.Find<User>(null)
                           join b in UnitWork.Find<NsapUserMap>(null) on a.Id equals b.UserID
                           where nsapId.Contains(b.NsapUserId)
                           select new { b.NsapUserId, a.Id, a.Name };
            foreach (var item in list)
            {
                var user = userInfo.Where(c => c.NsapUserId == item.Operator).FirstOrDefault();
                string id = "", name = "";
                if (user != null)
                {
                    id = user.Id; name = user.Name;
                }
                await UnitWork.UpdateAsync<ProductionSchedule>(c => c.GeneratorCode == item.GeneratorCode, c => new ProductionSchedule
                {
                    ReceiveNo = item.ReceiveNo,
                    ReceiveStatus = 2,
                    ReceiveOperatorId = id,
                    ReceiveOperator = name,
                    ReceiveTime = item.OperateTime
                });
            }
            await UnitWork.SaveAsync();
        }

        #endregion



        /// <summary>
        /// 构建证书模板参数
        /// </summary>
        /// <param name="baseInfo">基础信息</param>
        /// <param name="turV">Tur电压数据</param>
        /// <param name="turA">Tur电流数据</param>
        /// <returns></returns>
        public async Task<CertModel> BuildModel(NwcaliBaseInfo baseInfo, string type = "")
        {
            var list = new List<WordModel>();
            var model = new CertModel();
            var plcData = baseInfo.NwcaliPlcDatas.Where(d => d.DataType == 1).ToList();
            var plcRepetitiveMeasurementData = baseInfo.NwcaliPlcDatas.Where(d => d.DataType == 2).ToList();
            var turV = baseInfo.NwcaliTurs.Where(d => d.DataType == 1).ToList();
            var turA = baseInfo.NwcaliTurs.Where(d => d.DataType == 2).ToList();
            #region 页眉
            var barcode = await BarcodeGenerate(baseInfo.CertificateNumber);
            model.BarCode = barcode;
            #endregion
            #region Calibration Certificate
            model.CalibrationCertificate.CertificatenNumber = baseInfo.CertificateNumber;
            model.CalibrationCertificate.TesterMake = baseInfo.TesterMake;
            model.CalibrationCertificate.CalibrationDate = DateStringConverter(baseInfo.Time.Value.ToString());
            model.CalibrationCertificate.TesterModel = baseInfo.TesterModel;
            model.CalibrationCertificate.CalibrationDue = ConvertTestInterval(baseInfo.Time.Value.ToString(), baseInfo.TestInterval);
            model.CalibrationCertificate.TesterSn = baseInfo.TesterSn;
            model.CalibrationCertificate.DataType = "";
            model.CalibrationCertificate.AssetNo = baseInfo.AssetNo == "0" ? "------" : baseInfo.AssetNo;
            model.CalibrationCertificate.SiteCode = baseInfo.SiteCode;
            model.CalibrationCertificate.Temperature = baseInfo.Temperature;
            model.CalibrationCertificate.RelativeHumidity = baseInfo.RelativeHumidity;
            #endregion
            #region Main Standards Used
            for (int i = 0; i < baseInfo.Etalons.Count; i++)
            {
                model.MainStandardsUsed.Add(new MainStandardsUsed
                {
                    Name = baseInfo.Etalons[i].Name,
                    Characterisics = baseInfo.Etalons[i].Characteristics,
                    AssetNo = baseInfo.Etalons[i].AssetNo,
                    CertificateNo = baseInfo.Etalons[i].CertificateNo,
                    DueDate = DateStringConverter(baseInfo.Etalons[i].DueDate),
                    CalibrationEntity = baseInfo.Etalons[i].CalibrationEntity
                });
            }
            #endregion

            #region Uncertainty Budget
            var plcGroupData = plcData.GroupBy(d => d.PclNo);
            var plcRepetitiveMeasurementGroupData = plcRepetitiveMeasurementData.GroupBy(d => d.PclNo);
            var plc = plcGroupData.First();
            var plcrmd = plcRepetitiveMeasurementGroupData.First();
            var v = plc.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals("Charge") && p.VerifyType.Equals("Post-Calibration")).GroupBy(p => p.Channel).First().ToList();
            var sv = v.Select(s => s.CommandedValue).OrderBy(s => s).ToList();
            sv.Sort();
            var vscale = sv[(sv.Count - 1) / 2];


            var c = plc.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals("Charge") && p.VerifyType.Equals("Post-Calibration")).OrderByDescending(a => a.Scale).GroupBy(p => p.Scale).First().ToList();
            var cv = c.Select(c => c.CommandedValue).OrderBy(s => s).ToList();
            cv.Sort();
            var cscale = cv[(cv.Count - 1) / 2];
            if (type != "cnas")
            {
                #region T.U.R. Table
                //电压
                var vPoint = turV.Select(v => v.TestPoint).Distinct().OrderBy(v => v).ToList();
                var vPointIndex = (vPoint.Count - 1) / 2;
                var vSpec = v.First().Scale * baseInfo.RatedAccuracyV * 1000;
                var u95_1 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex - 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
                var a = turV.Where(v => v.TestPoint == vPoint[vPointIndex]).ToList();
                var u95_2 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
                var u95_3 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex + 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
                var tur_1 = (2 * vSpec / 1000) / (2 * u95_1);
                var tur_2 = (2 * vSpec / 1000) / (2 * u95_2);
                var tur_3 = (2 * vSpec / 1000) / (2 * u95_3);
                model.TurTables.Add(new TurTable { Number = "1", Point = $"{vPoint[vPointIndex - 1]}V", Spec = $"±{vSpec}mV", U95Standard = u95_1.ToString("e3") + "V", TUR = tur_1.ToString("f2") });
                model.TurTables.Add(new TurTable { Number = "2", Point = $"{vPoint[vPointIndex]}V", Spec = $"±{vSpec}mV", U95Standard = u95_2.ToString("e3") + "V", TUR = tur_2.ToString("f2") });
                model.TurTables.Add(new TurTable { Number = "3", Point = $"{vPoint[vPointIndex + 1]}V", Spec = $"±{vSpec}mV", U95Standard = u95_3.ToString("e3") + "V", TUR = tur_3.ToString("f2") });
                //电流
                var cPoint = turA.Select(v => v.TestPoint).Distinct().OrderBy(v => v).ToList();
                var cPointIndex = cPoint.IndexOf(cscale / 1000); //(cPoint.Count - 1) / 2;
                var cSpec = c.First().Scale * baseInfo.RatedAccuracyC;
                var U95_4turA = turA;
                if (turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1] && v.Tur != 0).ToList().Count > 2)
                {
                    U95_4turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1] && v.Tur != 0).GroupBy(t => t.UncertaintyContributors).Select(t => t.First()).ToList();
                    if (U95_4turA.Count > 2)
                    {
                        U95_4turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1] && v.Tur != 0).OrderBy(t => t.Range).Take(2).ToList();
                    }
                }
                else
                {
                    U95_4turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1] && v.Tur != 0).ToList();
                }
                var u95_4 = 2 * Math.Sqrt(U95_4turA.Sum(v => Math.Pow(v.StdUncertainty, 2)));
                var U95_5turA = turA;
                if (turA.Where(v => v.TestPoint == cPoint[cPointIndex] && v.Tur != 0).ToList().Count > 2)
                {
                    U95_5turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex] && v.Tur != 0).GroupBy(t => t.UncertaintyContributors).Select(t => t.First()).ToList();
                    if (U95_5turA.Count > 2)
                    {
                        U95_5turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex] && v.Tur != 0).OrderBy(t => t.Range).Take(2).ToList();
                    }
                }
                else
                {
                    U95_5turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex] && v.Tur != 0).ToList();
                }
                var u95_5 = 2 * Math.Sqrt(U95_5turA.Sum(v => Math.Pow(v.StdUncertainty, 2)));
                var U95_6turA = turA;
                if (turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1] && v.Tur != 0).ToList().Count > 2)
                {
                    U95_6turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1] && v.Tur != 0).GroupBy(t => t.UncertaintyContributors).Select(t => t.First()).ToList();
                    if (U95_6turA.Count > 2)
                    {
                        U95_6turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1] && v.Tur != 0).OrderBy(t => t.Range).Take(2).ToList();
                    }
                }
                else
                {
                    U95_6turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1] && v.Tur != 0).ToList();
                }
                var u95_6 = 2 * Math.Sqrt(U95_6turA.Sum(v => Math.Pow(v.StdUncertainty, 2)));
                var tur_4 = (2 * cSpec) / (2 * u95_4 * 1000);
                var tur_5 = (2 * cSpec) / (2 * u95_5 * 1000);
                var tur_6 = (2 * cSpec) / (2 * u95_6 * 1000);

                model.TurTables.Add(new TurTable { Number = "4", Point = $"{cPoint[cPointIndex - 1]}A", Spec = $"±{cSpec}mA", U95Standard = u95_4.ToString("e3") + "A", TUR = tur_4.ToString("f2") });
                model.TurTables.Add(new TurTable { Number = "5", Point = $"{cPoint[cPointIndex]}A", Spec = $"±{cSpec}mA", U95Standard = u95_5.ToString("e3") + "A", TUR = tur_5.ToString("f2") });
                model.TurTables.Add(new TurTable { Number = "6", Point = $"{cPoint[cPointIndex + 1]}A", Spec = $"±{cSpec}mA", U95Standard = u95_6.ToString("e3") + "A", TUR = tur_6.ToString("f2") });

                #endregion
            }

            #region Uncertainty Budget Table
            #region Voltage
            var vv = 2 * (double)v.FirstOrDefault().Scale / Math.Pow(2, baseInfo.VoltmeterBits);
            var vstd = 2 * (double)v.FirstOrDefault().Scale / (Math.Pow(2, baseInfo.VoltmeterBits) * Math.Sqrt(12));
            var voltageUncertaintyBudgetTable = new UncertaintyBudgetTable();
            voltageUncertaintyBudgetTable.Value = $"{vscale}V";
            voltageUncertaintyBudgetTable.TesterResolutionValue = vv.ToString("e3");
            voltageUncertaintyBudgetTable.TesterResolutionStdUncertainty = vstd.ToString("e3");
            var vmdcv = plcrmd.Where(d => d.CommandedValue.Equals(vscale) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals("Charge") && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
            double vror;
            if (vmdcv.Count >= 6)//贝塞尔公式法
            {
                var vavg = vmdcv.Sum(c => c.StandardValue) / vmdcv.Count;
                vror = Math.Sqrt(vmdcv.Select(c => Math.Pow(c.StandardValue - vavg, 2)).Sum() / (vmdcv.Count - 1));
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingValue = vror.ToString("e3");
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = vror.ToString("e3");
            }
            else//极差法
            {
                var poorCoefficient = PoorCoefficients[vmdcv.Count];
                var vmdsv = vmdcv.Select(c => c.StandardValue).ToList();
                var R = vmdsv.Max() - vmdsv.Min();
                var u2 = R / poorCoefficient;
                vror = u2;
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingValue = u2.ToString("e3");
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = u2.ToString("e3");
            }
            turV = turV.Where(v => v.TestPoint == vscale).ToList();
            var combinedUncertaintyV = Math.Sqrt(turV.Sum(v => Math.Pow(v.StdUncertainty, 2)) + Math.Pow(vstd, 2) + Math.Pow(vror, 2));
            voltageUncertaintyBudgetTable.CombinedUncertainty = combinedUncertaintyV.ToString("e3");
            voltageUncertaintyBudgetTable.CombinedUncertaintySignificance = "100.000%";
            voltageUncertaintyBudgetTable.CoverageFactor = baseInfo.K.ToString(); ;
            voltageUncertaintyBudgetTable.ExpandedUncertainty = (baseInfo.K * combinedUncertaintyV).ToString("e3");
            for (int i = 0; i < turV.Count; i++)
            {
                var data = new UncertaintyBudgetTable.UncertaintyBudgetTableData();
                var tv = turV[i];
                data.UncertaintyContributors = tv.UncertaintyContributors;
                data.Value = tv.Value.ToString("e3");
                data.SensitivityCoefficient = tv.SensitivityCoefficient.ToString();
                data.Unit = tv.Unit;
                data.Type = tv.Type;
                data.Distribution = tv.Distribution;
                if (tv.UncertaintyContributors.Equals("Resolution"))
                {
                    data.CoverageFactor = Math.Sqrt(12).ToString("f3");
                }
                else
                {
                    data.CoverageFactor = tv.Divisor.ToString();
                }
                data.StdUncertainty = tv.StdUncertainty.ToString("e3");
                data.Significance = (Math.Pow(tv.StdUncertainty, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3");
                voltageUncertaintyBudgetTable.Datas.Add(data);
            }
            voltageUncertaintyBudgetTable.TesterResolutionSignificance = (Math.Pow(vstd, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3");
            voltageUncertaintyBudgetTable.RepeatabilityOfReadingSignificance = (Math.Pow(vror, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3");
            model.VoltageUncertaintyBudgetTables = voltageUncertaintyBudgetTable;
            #endregion

            #region Current
            var currentUncertaintyBudgetTable = new UncertaintyBudgetTable();
            var cvv = 2 * (double)c.FirstOrDefault().Scale / 1000 / Math.Pow(2, baseInfo.AmmeterBits);
            var cstd = 2 * (double)c.FirstOrDefault().Scale / 1000 / (Math.Pow(2, baseInfo.AmmeterBits) * Math.Sqrt(12));
            currentUncertaintyBudgetTable.Value = $"{cscale}mA";
            currentUncertaintyBudgetTable.TesterResolutionValue = cvv.ToString("e3");
            currentUncertaintyBudgetTable.TesterResolutionStdUncertainty = cstd.ToString("e3");
            var cmdcv = plcrmd.Where(d => d.CommandedValue.Equals(cscale) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals("Charge") && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
            double cror;
            if (cmdcv.Count >= 6)//贝塞尔公式法
            {
                var cavg = cmdcv.Sum(c => c.StandardValue) / cmdcv.Count / 1000;
                cror = Math.Sqrt(cmdcv.Select(c => Math.Pow(c.StandardValue / 1000 - cavg, 2)).Sum() / (cmdcv.Count - 1));
                currentUncertaintyBudgetTable.RepeatabilityOfReadingValue = cror.ToString("e3");
                currentUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = cror.ToString("e3");
            }
            else//极差法
            {
                var poorCoefficient = PoorCoefficients[cmdcv.Count];
                var cmdsv = cmdcv.Select(c => c.StandardValue / 1000).ToList();
                var R = cmdsv.Max() - cmdsv.Min();
                var u2 = R / poorCoefficient;
                cror = u2;
                currentUncertaintyBudgetTable.RepeatabilityOfReadingValue = u2.ToString("e3");
                currentUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = u2.ToString("e3");
            }

            turA = turA.Where(a => a.TestPoint * 1000 == cscale).ToList();
            if (turA.Count > 2)
            {
                var turAOne = turA.GroupBy(t => t.UncertaintyContributors).Select(t => t.First()).ToList();
                if (turAOne.Count > 2)
                {
                    turA = turA.OrderBy(t => t.Range).Take(2).ToList();
                }
                else
                {
                    turA = turAOne;
                }
            }
            var combinedUncertaintyA = Math.Sqrt(turA.Sum(c => Math.Pow(c.StdUncertainty, 2)) + Math.Pow(cstd, 2) + Math.Pow(cror, 2));
            currentUncertaintyBudgetTable.CombinedUncertainty = combinedUncertaintyA.ToString("e3");
            currentUncertaintyBudgetTable.CombinedUncertaintySignificance = "100.000%";
            currentUncertaintyBudgetTable.CoverageFactor = baseInfo.K.ToString(); ;
            currentUncertaintyBudgetTable.ExpandedUncertainty = (baseInfo.K * combinedUncertaintyA).ToString("e3");

            for (int i = 0; i < turA.Count; i++)
            {
                var data = new UncertaintyBudgetTable.UncertaintyBudgetTableData();
                var ta = turA[i];

                data.UncertaintyContributors = ta.UncertaintyContributors;
                data.Value = ta.Value.ToString("e3");
                data.SensitivityCoefficient = ta.SensitivityCoefficient.ToString();
                data.Unit = ta.Unit;
                data.Type = ta.Type;
                data.Distribution = ta.Distribution;
                data.CoverageFactor = ta.Divisor.ToString();
                data.StdUncertainty = ta.StdUncertainty.ToString("e3");
                data.Significance = (Math.Pow(ta.StdUncertainty, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3");
                currentUncertaintyBudgetTable.Datas.Add(data);
            }
            currentUncertaintyBudgetTable.TesterResolutionSignificance = (Math.Pow(cstd, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3");
            currentUncertaintyBudgetTable.RepeatabilityOfReadingSignificance = (Math.Pow(cror, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3");
            model.CurrentUncertaintyBudgetTables = currentUncertaintyBudgetTable;
            #endregion

            #endregion
            #endregion

            #region Data Sheet
            void CalculateVoltage(string mode, int tableIndex, int DecimalPlace)
            {
                int j = 0;
                int l = 1;
                foreach (var item in plcGroupData)
                {
                    var data = item.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals(mode) && p.VerifyType.Equals("Post-Calibration")).GroupBy(d => d.Channel).ToList();
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.CommandedValue).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            //var cvCHH = $"{l}-{cvData.Channel}";
                            var cvCHH = $"{item.Key}-{cvData.Channel}";
                            var cvRange = cvData.Scale;
                            var cvIndication = cvData.MeasuredValue;
                            var cvMeasuredValue = cvData.StandardValue;
                            var cvError = (cvIndication - cvMeasuredValue) * 1000;
                            double cvAcceptance = 0;
                            var cvAcceptanceStr = "";
                            var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
                            var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration") && d.Scale.Equals(cvData.Scale)).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
                            double ror;
                            if (baseInfo.RepetitiveMeasurementsCount >= 6)//贝塞尔公式法
                            {
                                var vavg = mdcv.Sum(c => c.StandardValue) / mdcv.Count;
                                ror = Math.Sqrt(mdcv.Select(c => Math.Pow(c.StandardValue - vavg, 2)).Sum() / (mdcv.Count - 1));
                            }
                            else//极差法
                            {
                                var poorCoefficient = PoorCoefficients[mdcv.Count];
                                var mdsv = mdcv.Select(c => c.StandardValue).ToList();
                                var R = mdsv.Max() - mdsv.Min();
                                var u2 = R / poorCoefficient;
                                ror = u2;
                            }
                            //计算不确定度
                            var cvUncertaintyStr = (baseInfo.K * 1000 * Math.Sqrt(Math.Pow(cvData.StandardTotalU / 2, 2) + Math.Pow(vstd, 2) + Math.Pow(ror, 2))).ToString("G2");
                            var cvUncertainty = double.Parse(cvUncertaintyStr);
                            var T = double.Parse((cvData.Scale * baseInfo.RatedAccuracyV * 1000).ToString("G2"));
                            var cvConclustion = "";
                            //计算接受限
                            if (baseInfo.AcceptedTolerance.Equals("0"))
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyV * 1000;
                                cvAcceptance = accpetedTolerance;
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("1"))
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyV * 1000 - cvUncertainty;
                                cvAcceptance = accpetedTolerance;
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("M2%"))
                            {
                                var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale * baseInfo.RatedAccuracyV * 2 / (2 * cvUncertainty / 1000)) - 0.54);
                                if (m2 < 0)
                                {
                                    var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyV * 1000;
                                    cvAcceptance = accpetedTolerance;
                                }
                                else
                                {
                                    var accpetedTolerance = (cvData.Scale * baseInfo.RatedAccuracyV * 1000 - cvUncertainty) * m2;
                                    cvAcceptance = accpetedTolerance;
                                }
                            }
                            else//默认为0的处理方法
                            {

                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyV * 1000;
                                cvAcceptance = accpetedTolerance;
                            }
                            //约分
                            //var (IndicationReduce, MeasuredValueReduce, ErrorReduce, AcceptanceReduce, UncertaintyReduce) = ReduceVoltage(cvIndication, cvMeasuredValue, cvError, cvAcceptance, cvUncertainty);

                            var IndicationReduce = cvIndication.ToString($"F{DecimalPlace + 3}");
                            var MeasuredValueReduce = cvMeasuredValue.ToString($"F{DecimalPlace + 3}");
                            var ErrorReduce = (Convert.ToDouble(IndicationReduce) * 1000 - Convert.ToDouble(MeasuredValueReduce) * 1000).ToString($"F{DecimalPlace}");
                            var AcceptanceReduce = cvAcceptance.ToString($"F{DecimalPlace}");
                            var UncertaintyReduce = cvUncertainty.ToString($"F{DecimalPlace}");

                            cvAcceptanceStr = $"±{AcceptanceReduce}";
                            //计算判定结果
                            if (baseInfo.AcceptedTolerance.Equals("0"))
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    cvConclustion = "P";
                                }
                                else
                                {
                                    cvConclustion = "F";
                                }
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("1"))
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    cvConclustion = "P";
                                }
                                else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
                                {
                                    cvConclustion = "P*";
                                }
                                else
                                {
                                    cvConclustion = "F";
                                }
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("M2%"))
                            {
                                var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale * baseInfo.RatedAccuracyV * 2 / (2 * cvUncertainty / 1000)) - 0.54);
                                if (m2 < 0)
                                {
                                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                    {
                                        cvConclustion = "P";
                                    }
                                    else
                                    {
                                        cvConclustion = "F";
                                    }
                                }
                                else
                                {
                                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                    {
                                        cvConclustion = "P";
                                    }
                                    else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
                                    {
                                        cvConclustion = "P*";
                                    }
                                    else
                                    {
                                        cvConclustion = "F";
                                    }
                                }
                            }
                            else//默认为0的处理方法
                            {

                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    cvConclustion = "P";
                                }
                                else
                                {
                                    cvConclustion = "F";
                                }
                            }

                            if (mode.Equals("Charge"))
                            {
                                model.ChargingVoltage.Add(new DataSheet
                                {
                                    Sort1 = item.Key,
                                    Sort2 = cvData.Channel,
                                    Channel = cvCHH,
                                    Range = cvRange.ToString(),
                                    Indication = IndicationReduce,
                                    MeasuredValue = MeasuredValueReduce,
                                    Error = ErrorReduce,
                                    Acceptance = cvAcceptanceStr,
                                    Uncertainty = UncertaintyReduce,
                                    Conclusion = cvConclustion
                                });
                            }
                            else
                            {
                                model.DischargingVoltage.Add(new DataSheet
                                {
                                    Sort1 = item.Key,
                                    Sort2 = cvData.Channel,
                                    Channel = cvCHH,
                                    Range = cvRange.ToString(),
                                    Indication = IndicationReduce,
                                    MeasuredValue = MeasuredValueReduce,
                                    Error = ErrorReduce,
                                    Acceptance = cvAcceptanceStr,
                                    Uncertainty = UncertaintyReduce,
                                    Conclusion = cvConclustion
                                });
                            }
                            j++;
                        }
                    }
                    l++;
                }
            }
            void CalculateCurrent(string mode, int tableIndex, int DecimalPlace, int Cunit)
            {
                int j = 0;
                int l = 1;
                foreach (var item in plcGroupData)
                {
                    var data = item.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals(mode) && p.VerifyType.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.Scale).ThenBy(dd => dd.CommandedValue).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            //var CHH = $"{l}-{cvData.Channel}";
                            var CHH = $"{item.Key}-{cvData.Channel}";
                            var Range = cvData.Scale;
                            var Indication = cvData.MeasuredValue;
                            var MeasuredValue = cvData.StandardValue;
                            var Error = Indication - MeasuredValue;
                            double Acceptance = 0;
                            var AcceptanceStr = "";

                            var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
                            var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration") && d.Scale.Equals(cvData.Scale)).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
                            double ror;
                            if (baseInfo.RepetitiveMeasurementsCount >= 6)//贝塞尔公式法
                            {
                                var avg = mdcv.Sum(c => c.StandardValue) / mdcv.Count / 1000;
                                ror = Math.Sqrt(mdcv.Select(c => Math.Pow(c.StandardValue / 1000 - avg, 2)).Sum() / (mdcv.Count - 1));
                            }
                            else//极差法
                            {
                                var poorCoefficient = PoorCoefficients[mdcv.Count];
                                var mdsv = mdcv.Select(c => c.StandardValue / 1000).ToList();
                                var R = mdsv.Max() - mdsv.Min();
                                var u2 = R / poorCoefficient;
                                ror = u2;
                            }
                            //计算不确定度
                            var UncertaintyStr = (baseInfo.K * 1000 * Math.Sqrt(Math.Pow(cvData.StandardTotalU / 2, 2) + Math.Pow(cstd, 2) + Math.Pow(ror, 2))).ToString();
                            var Uncertainty = double.Parse(UncertaintyStr);
                            var T = double.Parse((cvData.Scale * baseInfo.RatedAccuracyC).ToString("G2"));
                            var Conclustion = "";
                            //计算接受限
                            if (baseInfo.AcceptedTolerance.Equals("0"))
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC;
                                Acceptance = accpetedTolerance;
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("1"))
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC - Uncertainty;
                                Acceptance = accpetedTolerance;
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("M2%"))
                            {
                                var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale * baseInfo.RatedAccuracyC * 2 / (2 * Uncertainty / 1000)) - 0.54);
                                if (m2 < 0)
                                {
                                    var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC;
                                    Acceptance = accpetedTolerance;
                                }
                                else
                                {
                                    var accpetedTolerance = (cvData.Scale * baseInfo.RatedAccuracyC - Uncertainty) * m2;
                                    Acceptance = accpetedTolerance;
                                }
                            }
                            else//默认为0的处理方法
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC;
                                Acceptance = accpetedTolerance;
                            }
                            //约分
                            //var (IndicationReduce, MeasuredValueReduce, ErrorReduce, AcceptanceReduce, UncertaintyReduce) = ReduceCurrent(Math.Abs(Indication), Math.Abs(MeasuredValue), Error, Acceptance, Uncertainty);
                            string IndicationReduce = "", MeasuredValueReduce = "", ErrorReduce = "", AcceptanceReduce = "", UncertaintyReduce = "";

                            IndicationReduce = (Math.Abs(Indication) / Math.Pow(1000, Cunit)).ToString($"F{DecimalPlace + 3}");
                            MeasuredValueReduce = (Math.Abs(MeasuredValue) / Math.Pow(1000, Cunit)).ToString($"F{DecimalPlace + 3}");
                            ErrorReduce = (Convert.ToDouble(IndicationReduce) * 1000 - Convert.ToDouble(MeasuredValueReduce) * 1000).ToString($"F{DecimalPlace}");
                            AcceptanceReduce = (Acceptance / Math.Pow(1000, Cunit - 1)).ToString($"F{DecimalPlace}");
                            UncertaintyReduce = (Uncertainty / Math.Pow(1000, Cunit - 1)).ToString($"F{DecimalPlace}");

                            AcceptanceStr = $"±{AcceptanceReduce}";
                            //计算判定结果
                            if (baseInfo.AcceptedTolerance.Equals("0"))
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    Conclustion = "P";
                                }
                                else
                                {
                                    Conclustion = "F";
                                }
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("1"))
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    Conclustion = "P";
                                }
                                else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
                                {
                                    Conclustion = "P*";
                                }
                                else
                                {
                                    Conclustion = "F";
                                }
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("M2%"))
                            {
                                var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale / 1000 * baseInfo.RatedAccuracyC * 2 / (2 * Uncertainty / 1000)) - 0.54);
                                if (m2 < 0)
                                {
                                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                    {
                                        Conclustion = "P";
                                    }
                                    else
                                    {
                                        Conclustion = "F";
                                    }
                                }
                                else
                                {
                                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                    {
                                        Conclustion = "P";
                                    }
                                    else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
                                    {
                                        Conclustion = "P*";
                                    }
                                    else
                                    {
                                        Conclustion = "F";
                                    }
                                }
                            }
                            else//默认为0的处理方法
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    Conclustion = "P";
                                }
                                else
                                {
                                    Conclustion = "F";
                                }
                            }

                            if (mode.Equals("Charge"))
                            {
                                model.ChargingCurrent.Add(new DataSheet
                                {
                                    Sort1 = item.Key,
                                    Sort2 = cvData.Channel,
                                    Channel = CHH,
                                    Range = baseInfo.TesterModel.Contains("mA") ? Range.ToString() : ((decimal)Range / 1000).ToString(),
                                    Indication = IndicationReduce,
                                    MeasuredValue = MeasuredValueReduce,
                                    Error = ErrorReduce,
                                    Acceptance = AcceptanceStr,
                                    Uncertainty = UncertaintyReduce,
                                    Conclusion = Conclustion
                                });
                            }
                            else
                            {
                                model.DischargingCurrent.Add(new DataSheet
                                {
                                    Sort1 = item.Key,
                                    Sort2 = cvData.Channel,
                                    Channel = CHH,
                                    Range = baseInfo.TesterModel.Contains("mA") ? Range.ToString() : ((decimal)Range / 1000).ToString(),
                                    Indication = IndicationReduce,
                                    MeasuredValue = MeasuredValueReduce,
                                    Error = ErrorReduce,
                                    Acceptance = AcceptanceStr,
                                    Uncertainty = UncertaintyReduce,
                                    Conclusion = Conclustion
                                });
                            }
                            j++;
                        }
                    }
                    l++;
                }
            }

            var CategoryObj = await GetCategory(baseInfo.TesterModel);

            #region Charging Voltage
            CalculateVoltage("Charge", 8, int.Parse(CategoryObj.DtValue));
            model.ChargingVoltage = model.ChargingVoltage.OrderBy(s => s.Sort1).ThenBy(s => s.Sort2).ToList();
            #endregion

            #region Discharging Voltage
            CalculateVoltage("DisCharge", 9, int.Parse(CategoryObj.DtValue));
            model.DischargingVoltage = model.DischargingVoltage.OrderBy(s => s.Sort1).ThenBy(s => s.Sort2).ToList();
            #endregion

            #region Charging Current
            CalculateCurrent("Charge", 10, int.Parse(CategoryObj.Description), int.Parse(CategoryObj.DtCode));
            model.ChargingCurrent = model.ChargingCurrent.OrderBy(s => s.Sort1).ThenBy(s => s.Sort2).ToList();
            #endregion

            #region Discharging Current
            CalculateCurrent("DisCharge", 10, int.Parse(CategoryObj.Description), int.Parse(CategoryObj.DtCode));
            model.DischargingCurrent = model.DischargingCurrent.OrderBy(s => s.Sort1).ThenBy(s => s.Sort2).ToList();
            //放电不确定度改为充电不确定度
            for (int i = 0; i < model.DischargingCurrent.Count; i++)
            {
                model.DischargingCurrent[i].Uncertainty = model.ChargingCurrent[i].Uncertainty;
            }
            #endregion



            #endregion

            #region 签名
            var us = await _userSignApp.GetUserSignList(new QueryUserSignListReq { });
            //if (baseInfo.Operator == "肖淑惠" || baseInfo.Operator == "阙勤勤")
            //{
            //    var name = await UnitWork.Find<Category>(c => c.TypeId == "SYS_CalibrationCertificateSign").Select(c => c.Name).FirstOrDefaultAsync();
            //    baseInfo.Operator = name;
            //}
            var calibrationTechnician = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.Issuer));
            if (calibrationTechnician != null)
            {
                model.CalibrationTechnician = await GetSignBase64(calibrationTechnician.PictureId);
            }
            var technicalManager = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.TechnicalManager));
            if (technicalManager != null)
            {
                model.TechnicalManager = await GetSignBase64(technicalManager.PictureId);
            }
            var approvalDirector = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.ApprovalDirector));
            if (approvalDirector != null)
            {
                model.ApprovalDirector = await GetSignBase64(approvalDirector.PictureId);
            }
            #endregion
            return model;
        }

        /// <summary>
        /// 获取签名base64字符串
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        private async Task<string> GetSignBase64(string fileId)
        {
            var file = await _fileApp.GetFileAsync(fileId);
            if (file != null)
            {
                using (var fs = await _fileApp.GetFileStreamAsync(file.BucketName, file.FilePath))
                {
                    var bytes = new byte[fs.Length];
                    fs.Position = 0;
                    await fs.ReadAsync(bytes, 0, bytes.Length);
                    var base64str = Convert.ToBase64String(bytes);
                    return base64str;
                }
            }
            throw new Exception($"用户未配置签名。");
        }

        /// <summary>
        /// 生成证书一维码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> BarcodeGenerate(string data)
        {
            System.Drawing.Font labelFont = new System.Drawing.Font("OCRB", 11f, FontStyle.Bold);//
            BarcodeLib.Barcode b = new BarcodeLib.Barcode
            {
                IncludeLabel = true,
                LabelFont = labelFont
            };
            Image img = b.Encode(BarcodeLib.TYPE.CODE128, data, Color.Black, Color.White, 131, 50);

            DirUtil.CheckOrCreateDir(Path.Combine(BaseCertDir, data));
            using (var stream = new MemoryStream())
            {
                img.Save(stream, ImageFormat.Png);
                var bytes = new byte[stream.Length];
                stream.Position = 0;
                await stream.ReadAsync(bytes, 0, bytes.Length);
                var base64str = Convert.ToBase64String(bytes);
                return base64str;
            }
        }

        /// <summary>
        /// 将日期转成英文格式
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string DateStringConverter(string date)
        {
            return DateTime.Parse(date).ToString("MMM-dd-yyyy", new System.Globalization.CultureInfo("en-us"));
        }

        /// <summary>
        /// 计算复校间隔时间
        /// </summary>
        /// <param name="date"></param>
        /// <param name="testInterval"></param>
        /// <returns></returns>
        private static string ConvertTestInterval(string date, string testInterval)
        {
            var y = testInterval.Substring(testInterval.Length - 1, 1);
            if (y.Equals("年"))
            {
                var years = Convert.ToInt32(testInterval[0..^1]);
                var dateTime = DateTime.Parse(date).AddYears(years).AddDays(-1).ToString();
                return DateStringConverter(dateTime);
            }
            if (y.Equals("月"))
            {
                var months = Convert.ToInt32(testInterval[0..^1]);
                var dateTime = DateTime.Parse(date).AddMonths(months).AddDays(-1).ToString();
                return DateStringConverter(dateTime);
            }
            return string.Empty;
        }


        public CertinfoApp(IUnitWork unitWork, IRepository<Certinfo> repository, ILogger<CertinfoApp> logger, DDSettingHelp dDSettingHelp,
            RevelanceManagerApp app, IAuth auth, FlowInstanceApp flowInstanceApp, CertOperationHistoryApp certOperationHistoryApp, ModuleFlowSchemeApp moduleFlowSchemeApp, NwcaliCertApp nwcaliCertApp, FileApp fileApp, UserSignApp userSignApp, ServiceOrderApp serviceOrderApp, StepVersionApp stepVersionApp, IOptions<AppSetting> appConfiguration) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
            _flowInstanceApp = flowInstanceApp;
            _certOperationHistoryApp = certOperationHistoryApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _nwcaliCertApp = nwcaliCertApp;
            _userSignApp = userSignApp;
            _fileApp = fileApp;
            _serviceOrderApp = serviceOrderApp;
            _stepVersionApp = stepVersionApp;
            _appConfiguration = appConfiguration;
            _ddSettingHelp = dDSettingHelp;
            _logger = logger;
        }
    }
}