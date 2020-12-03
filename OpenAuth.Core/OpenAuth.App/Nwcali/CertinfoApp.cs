using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Wrod;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using NPOI.SS.Formula.Atp;
using OpenAuth.App.Interface;
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
            var fs = await UnitWork.Find<FlowInstance>(null)
                .Where(o => o.IsFinish == 1 && o.SchemeId == mf.FlowSchemeId)//校准证书已结束流程
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

            var fs = await UnitWork.Find<FlowInstance>(f => f.SchemeId == mf.FlowSchemeId)
                .WhereIf(request.FlowStatus != 1, o => o.MakerList == "1" || o.MakerList.Contains(user.User.Id))//待办事项
                //.WhereIf(request.FlowStatus == 1, o => o.ActivityName == "待送审" || instances.Contains(o.Id))
                .WhereIf(request.FlowStatus == 2, o => o.ActivityName == "待审核" || o.ActivityName == "待批准")
                .ToListAsync();
            var fsid = fs.Select(f => f.Id).ToList();
            var certObjs = UnitWork.Find<NwcaliBaseInfo>(null);
            certObjs = certObjs
                .Where(o => fsid.Contains(o.FlowInstanceId) || o.Operator.Equals(user.User.Name))
                //.WhereIf(request.FlowStatus == 1, u => u.Operator.Equals(user.User.Name))
                .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertificateNumber.Contains(request.CertNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.TesterModel.Contains(request.Model))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.TesterSn.Contains(request.Sn))
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
            var objs = UnitWork.Find<Certinfo>(null);
            objs = objs
                .Where(o => fsid.Contains(o.FlowInstanceId))
                .WhereIf(request.FlowStatus == 1, u => u.Operator.Equals(user.User.Name))
                .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertNo.Contains(request.CertNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.Model.Contains(request.Model))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.Sn.Contains(request.Sn))
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
                }).ToList(); 
                view = view.Concat(view2);
            }
            result.Data = view.OrderByDescending(d=>d.CreateTime).ToList();
            result.Count = certCount2 + certCount1;
            return result;
        }

        /// <summary>
        /// 证书审批操作
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task CertVerification(CertVerificationReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var certInfo = await Repository.Find(c => c.Id.Equals(req.CertInfoId)).FirstOrDefaultAsync();

            var baseInfo = await UnitWork.Find<NwcaliBaseInfo>(null).FirstOrDefaultAsync(c => c.Id == req.CertInfoId);
            if (certInfo is null && baseInfo is null)
                throw new CommonException("证书不存在", 80001);
            var id = certInfo is null ? baseInfo.Id : certInfo.Id;
            var certNo = certInfo is null ? baseInfo.CertificateNumber : certInfo.CertNo;
            var b = await CheckCanOperation(id, loginContext.User.Name);
            if (!b)
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
                    await _flowInstanceApp.DeleteAsync(f => f.Id == req.Verification.FlowInstanceId);
                    var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name == "校准证书");
                    var request = new AddFlowInstanceReq();
                    request.SchemeId = mf.FlowSchemeId;
                    request.FrmType = 2;
                    request.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                    request.CustomName = $"校准证书{certNo}审批";
                    request.FrmData = $"{{\"certNo\":\"{certNo}\",\"cert\":[{{\"key\":\"{DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString()}\",\"url\":\"/Cert/DownloadCertPdf/{certNo}\",\"percent\":100,\"status\":\"success\",\"isImg\":false}}]}}";
                    var newFlowId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(request);
                    await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { FlowInstanceId = newFlowId });
                    break;
                default:
                    break;
            }

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
        private async Task<bool> CheckCanOperation(string id, string name)
        {
            var h = await UnitWork.FindSingleAsync<CertOperationHistory>(c => c.CertInfoId.Equals(id) && c.Action.Contains(name));
            return h is null;
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