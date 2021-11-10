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
        private static readonly string BaseCertDir = Path.Combine(Directory.GetCurrentDirectory(), "certs");
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
                    else if (c.FlowInstance.IsFinish == -1) c.FlowInstance.ActivityName = "撤回";
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
                                await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { ApprovalDirector = loginContext.User.Name, ApprovalDirectorId = loginContext.User.Id});
                                await UnitWork.SaveAsync();
                                await CreateNwcailFile(certNo);
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
                    Message.Append("报错编号"+baseInfo.CertificateNumber +"报错信息："+ e.Message);
                }
                if (request.Count > 1) 
                {
                    //await Task.Delay(30000);
                }
            }
            result.Message= string.IsNullOrWhiteSpace(Message.ToString()) ? "审批成功" : Message.ToString();
            return result;
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
                var model = await BuildModel(baseInfo);
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
                var path= Path.Combine(BaseCertDir, certNo);
                DirUtil.CheckOrCreateDir(path);
                var fullpath = Path.Combine(path, certNo + ".pdf");
                using (FileStream fs=new FileStream(fullpath, FileMode.Create))
                {
                    using (BinaryWriter bw = new BinaryWriter(fs))
                    {
                        bw.Write(datas, 0, datas.Length);
                    }
                }
                await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { PdfPath=fullpath });
                await UnitWork.SaveAsync();
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

                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("校准证书"));
                var fs = await UnitWork.Find<FlowInstance>(null)
                    .Where(o => o.SchemeId == mf.FlowSchemeId && (o.ActivityName.Contains("已完成") || o.IsFinish == 1))//校准证书已完成流程,IsFinish==1为流程改动前真实结束的数据
                    .ToListAsync();
                var fsid = fs.Select(f => f.Id).ToList();

                var cerlist = await UnitWork.Find<NwcaliBaseInfo>(o => fsid.Contains(o.FlowInstanceId)).ToListAsync();
                var test= cerlist.OrderByDescending(c => c.Time).GroupBy(c => c.TesterSn).Select(c => c.First()).ToList();

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

                #region 老数据
                //序列号下最新的校验证书
                var testNo = numList.Select(c => c.TesterSn).ToList();
                var old = await UnitWork.Find<Certinfo>(c=> testNo.Contains(c.Sn) && fsid.Contains(c.FlowInstanceId)).ToListAsync();
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
                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("校准证书"));
                var fs = await UnitWork.Find<FlowInstance>(null)
                    .Where(o => o.SchemeId == mf.FlowSchemeId && (o.ActivityName.Contains("已完成") || o.IsFinish == 1))//校准证书已完成流程,IsFinish==1为流程改动前真实结束的数据
                    .ToListAsync();
                var fsid = fs.Select(f => f.Id).ToList();

                var cerinfo = await UnitWork.Find<NwcaliBaseInfo>(o=>fsid.Contains(o.FlowInstanceId))
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
                var obj = await UnitWork.Find<Certinfo>(o => fsid.Contains(o.FlowInstanceId))
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

            var list = await UnitWork.Find<Entrustment>(c => c.Id == id).Include(c=>c.EntrustmentDetails).FirstOrDefaultAsync();
            list.EntrustmentDetails=list.EntrustmentDetails.OrderBy(c => c.Sort).ToList();
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
        /// 推送过期前一个月的校准证书关联的GUID
        /// </summary>
        /// <returns></returns>
        public async Task PushCertGuidToApp()
        {
            var plcguid = await UnitWork.FromSql<PcPlc>(@$"SELECT * from pcplc where timestampdiff(day,ExpirationDate,NOW())=30 or timestampdiff(day,ExpirationDate,NOW())=20 or timestampdiff(day,ExpirationDate,NOW())=10").Select(c => new { c.Guid, DueTime=c.ExpirationDate }).ToListAsync();

            var guid = await UnitWork.FromSql<Certplc>(@$"SELECT * from certplc where timestampdiff(day,ExpirationDate,NOW())=30 or timestampdiff(day,ExpirationDate,NOW())=20 or timestampdiff(day,ExpirationDate,NOW())=10").Select(c => new { Guid=c.PlcGuid, DueTime = c.ExpirationDate } ).ToListAsync();

            if (guid.Count>0) plcguid = plcguid.Concat(guid).ToList();


           await _serviceOrderApp.PushMessageToApp(0, "", "", "1", plcguid);
        }

        /// <summary>
        /// 拉取销售交货生成备料单
        /// </summary>
        /// <returns></returns>
        public async Task SynSalesDelivery()
        {
            //销售交货流程 并处于序列号选择环节
            //var deliveryJob = await UnitWork.Find<wfa_job>(c => c.job_type_id == 1 && c.job_nm == "销售交货" ).ToListAsync(); 
            var entrusted = await UnitWork.Find<Entrustment>(c => c.Status != 5).ToListAsync();
            var jobIds = entrusted.Select(c => c.JodId).ToList();//已经生成过的流程ID

            var deliveryList = await UnitWork.Find<wfa_job>(c => c.job_type_id == 1 && c.job_nm == "销售交货").Select(c => new { c.sbo_id, c.base_entry, c.base_type, c.job_data, c.job_id, c.step_id, c.job_state, c.sync_stat, c.sbo_itf_return }).ToListAsync();
            var deliveryJob = deliveryList.Where(c => !jobIds.Contains(c.job_id) && c.step_id == 455).ToList();//在选择序列号环节
            #region 生成备料单
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

                        single.Remark = (saleOrder.Comments + saleOrder.U_CPH + saleOrder.U_YSQX + saleOrder.U_YGMD  + saleOrder.U_SCBM).Trim();
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
                                    obj.Quantity = Convert.ToInt32(data.Quantity.Split(".")[0]);
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
            var finlishJob = deliveryList.Where(c => c.job_state == 3 && c.sync_stat == 4).Select(c => c.job_id).ToList();//选择了序列号/结束的交货流程并且同步完成
            var finlishEntrusted = entrusted.Where(c => finlishJob.Contains(c.JodId) && c.Status==1).ToList();
            for (int i = 0; i < finlishEntrusted.Count; i++)
            {
                var item = finlishEntrusted[i];
                var job = deliveryList.Where(c => c.job_id == item.JodId).FirstOrDefault();

                var serialNumber = await (from a in UnitWork.Find<OITL>(null)
                                          join b in UnitWork.Find<ITL1>(null) on a.LogEntry equals b.LogEntry into ab
                                          from b in ab.DefaultIfEmpty()
                                          join c in UnitWork.Find<OSRN>(null) on new { b.ItemCode, SysNumber = b.SysNumber.Value } equals new { c.ItemCode, c.SysNumber } into bc
                                          from c in bc.DefaultIfEmpty()
                                          where a.DocType == 15 && a.DefinedQty > 0 && a.DocNum.Value.ToString() == job.sbo_itf_return
                                          select new { a.DocType, a.DocNum, a.ItemCode, a.ItemName, b.SysNumber, c.MnfSerial, c.DistNumber }).ToListAsync();

                //var aa = serialNumber.GroupBy(c => c.ItemCode).ToList();
                int line = 0, sort = 0;
                List<EntrustmentDetail> detail = new List<EntrustmentDetail>();
                foreach (var groupItem in serialNumber.GroupBy(c => c.ItemCode).ToList())
                {
                    int line2 = 0;
                    var deletedt = await UnitWork.FindTrack<EntrustmentDetail>(c => c.EntrustmentId == item.Id).ToListAsync();
                    await UnitWork.DeleteAsync<EntrustmentDetail>(c => c.EntrustmentId == item.Id);
                    //await UnitWork.BatchDeleteAsync<EntrustmentDetail>(deletedt.ToArray());
                    await UnitWork.SaveAsync();
                    //var deleteSql = $"DELETE FROM entrustmentdetail WHERE EntrustmentId={item.Id}";
                    //UnitWork.ExecuteSql(deleteSql, ContextType.Nsap4NwcaliDbContextType);


                    if (groupItem.Key.StartsWith("CT") || groupItem.Key.StartsWith("CTE") || groupItem.Key.StartsWith("CE"))
                    {
                        ++line;
                        foreach (var items in groupItem)
                        {
                            ++line2;++sort;
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
                }) ;
                await UnitWork.SaveAsync();
            }
            #endregion

            #region 同步状态
            var calibration = entrusted.Where(c => c.Status >= 3).ToList();
            foreach (var item in calibration)
            {
                var details = await UnitWork.FindTrack<EntrustmentDetail>(c => c.EntrustmentId == item.Id).ToListAsync();
                var snids = details.Select(c => c.SerialNumber).ToList();
                var nwcert = await UnitWork.Find<NwcaliBaseInfo>(c => snids.Contains(c.TesterSn)).ToListAsync();
                if (nwcert != null && nwcert.Count>0)
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
        }

        private void SetStatus(ref List<EntrustmentDetail> detail,List<NwcaliBaseInfo> nwcert)
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
                return  (NSAP.Entity.Sales.billDelivery)bs.Deserialize(stream);
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
                        join d in UnitWork.Find< crm_ocrd >(null) on a.CardCode equals d.CardCode into ad
                        from d in ad.DefaultIfEmpty()
                        join b in UnitWork.Find<crm_ocry>(null) on a.Country equals b.Code into ab
                        from b in ab.DefaultIfEmpty()
                        join c in UnitWork.Find<crm_ocst>(null) on a.State equals c.Code into ac
                        from c in ac.DefaultIfEmpty()
                        where a.sbo_id == sboid && a.CardCode == cardcode && a.AdresType=="B" && a.Address== "开票到"
                        select new { d.CardCode,d.CardName, a.LineNum, Active = a.U_Active, a.AdresType, a.Address, Country = b.Name, State = c.Name, a.City, a.Building };
            return query.FirstOrDefault();
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


        /// <summary>
        /// 构建证书模板参数
        /// </summary>
        /// <param name="baseInfo">基础信息</param>
        /// <param name="turV">Tur电压数据</param>
        /// <param name="turA">Tur电流数据</param>
        /// <returns></returns>
        public async Task<CertModel> BuildModel(NwcaliBaseInfo baseInfo)
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
                    DueDate = DateStringConverter(baseInfo.Etalons[i].DueDate)
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
                    var data = item.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals(mode) && p.VerifyType.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.CommandedValue).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            var cvCHH = $"{l}-{cvData.Channel}";
                            var cvRange = cvData.Scale;
                            var cvIndication = cvData.MeasuredValue;
                            var cvMeasuredValue = cvData.StandardValue;
                            var cvError = (cvIndication - cvMeasuredValue) * 1000;
                            double cvAcceptance = 0;
                            var cvAcceptanceStr = "";
                            var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
                            var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
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
                            if (mode.Equals("Charge"))
                            {
                                model.ChargingVoltage.Add(new DataSheet
                                {
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
                            var CHH = $"{l}-{cvData.Channel}";
                            var Range = cvData.Scale;
                            var Indication = cvData.MeasuredValue;
                            var MeasuredValue = cvData.StandardValue;
                            var Error = Indication - MeasuredValue;
                            double Acceptance = 0;
                            var AcceptanceStr = "";

                            var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
                            var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
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

                            if (mode.Equals("Charge"))
                            {
                                model.ChargingCurrent.Add(new DataSheet
                                {
                                    Channel = CHH,
                                    Range = baseInfo.TesterModel.Contains("mA") ? Range.ToString() : ((double)Range / 1000).ToString(),
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
                                    Channel = CHH,
                                    Range = baseInfo.TesterModel.Contains("mA") ? Range.ToString() : ((double)Range / 1000).ToString(),
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
            model.ChargingVoltage = model.ChargingVoltage.OrderBy(s => s.Channel).ToList();
            #endregion

            #region Discharging Voltage
            CalculateVoltage("DisCharge", 9, int.Parse(CategoryObj.DtValue));
            model.DischargingVoltage = model.DischargingVoltage.OrderBy(s => s.Channel).ToList();
            #endregion

            #region Charging Current
            CalculateCurrent("Charge", 10, int.Parse(CategoryObj.Description), int.Parse(CategoryObj.DtCode));
            model.ChargingCurrent = model.ChargingCurrent.OrderBy(s => s.Channel).ToList();
            #endregion

            #region Discharging Current
            CalculateCurrent("DisCharge", 10, int.Parse(CategoryObj.Description), int.Parse(CategoryObj.DtCode));
            model.DischargingCurrent = model.DischargingCurrent.OrderBy(s => s.Channel).ToList();
            #endregion



            #endregion

            #region 签名
            var us = await _userSignApp.GetUserSignList(new QueryUserSignListReq { });
            var calibrationTechnician = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.Operator));
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
        public  async Task<string> BarcodeGenerate(string data)
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


        public CertinfoApp(IUnitWork unitWork, IRepository<Certinfo> repository,
            RevelanceManagerApp app, IAuth auth, FlowInstanceApp flowInstanceApp, CertOperationHistoryApp certOperationHistoryApp, ModuleFlowSchemeApp moduleFlowSchemeApp, NwcaliCertApp nwcaliCertApp, FileApp fileApp, UserSignApp userSignApp, ServiceOrderApp serviceOrderApp) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
            _flowInstanceApp = flowInstanceApp;
            _certOperationHistoryApp = certOperationHistoryApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _nwcaliCertApp = nwcaliCertApp;
            _userSignApp = userSignApp;
            _fileApp = fileApp;
            _serviceOrderApp = serviceOrderApp;
        }
    }
}