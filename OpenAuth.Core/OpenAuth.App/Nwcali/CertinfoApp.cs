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

            var properties = loginContext.GetProperties("certinfo");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }

            var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("校准证书"));
            var fs = await UnitWork.Find<FlowInstance>(null)
                .Where(o => o.IsFinish == 1 && o.SchemeId == mf.FlowSchemeId)//校准证书已结束流程
                .ToListAsync();
            var fsid = fs.Select(f => f.Id).ToList();
            var result = new TableData();
            var objs = UnitWork.Find<Certinfo>(null);
            objs = objs
                       .Where(o => fsid.Contains(o.FlowInstanceId))
                       .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertNo.Contains(request.CertNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.Model.Contains(request.Model))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.Sn.Contains(request.Sn))
                       .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.CalibrationDate >= request.CalibrationDateFrom && u.CalibrationDate <= request.CalibrationDateTo)
                ;
            var list = await objs.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

            list.ForEach(c =>
            {
                c.FlowInstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));
            });
            var view = list.Select(c =>
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
            properties.RemoveAll(a => a.Key.Equals("FlowInstance"));
            //properties.Add(new KeyDescription() { Key = "FlowInstanceActivityName", Browsable = true, Description = "FlowInstanceActivityName", Type = "String" });
            var propertyStr = string.Join(',', properties.Select(u => u.Key));
            result.columnHeaders = properties;
            result.Data = view;//list.AsQueryable().Select($"new ({propertyStr},FlowInstance)");
            result.Count = objs.Count();
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

            var instances = new List<string>();
            if( request.FlowStatus == 1   )
                instances = await UnitWork.Find<FlowInstanceTransitionHistory>(u => u.CreateUserId == user.User.Id)
                    .Select(u => u.InstanceId).Distinct().ToListAsync();

            var fs = await UnitWork.Find<FlowInstance>(null)
                .Where(o => o.MakerList == "1" || o.MakerList.Contains(user.User.Id))//待办事项
                .WhereIf(request.FlowStatus == 1, o => o.ActivityName == "待送审" || instances.Contains(o.Id))
                .WhereIf(request.FlowStatus == 2, o => o.ActivityName == "待审核" || o.ActivityName == "待批准")
                .ToListAsync();
            var fsid = fs.Select(f => f.Id).ToList();
            var objs = UnitWork.Find<Certinfo>(null);
            objs = objs
                .Where(o => fsid.Contains(o.FlowInstanceId))
                .WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertNo.Contains(request.CertNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.Model.Contains(request.Model))
                .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.Sn.Contains(request.Sn))
                .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.CalibrationDate >= request.CalibrationDateFrom && u.CalibrationDate <= request.CalibrationDateTo)
                ;
            result.Count = await objs.CountAsync();

            var list = await objs.OrderByDescending(u => u.CreateTime)
                    .Skip((request.page - 1) * request.limit)
                    .Take(request.limit).ToListAsync();
            list.ForEach(c =>
            {
                c.FlowInstance = fs.Find(f => f.Id.Equals(c.FlowInstanceId));
            });
            result.Data = list.Select(c =>
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

            if (certInfo is null)
                throw new CommonException("证书不存在", 80001);

            var b = await CheckCanOperation(certInfo.Id, loginContext.User.Name);
            if (!b || certInfo.Operator.Equals(loginContext.User.Name))
            {
                throw new CommonException("您无法操作此步骤。", 80011);
            }

            var flowInstance = await UnitWork.FindSingleAsync<FlowInstance>(c => c.Id.Equals(certInfo.FlowInstanceId));
            var list = new List<WordModel>();
            var nameDic = new Dictionary<string, string>()
            {
                { "肖淑惠","xiao.png" },
                { "覃金英","tan.png" },
                { "周定坤","zhou.png" },
                { "杨浩杰","yang.png" },
                { "陈大为","chen.png" },
            };
            switch (req.Verification.VerificationFinally)
            {
                case "1":
                    #region 签名
                    if (flowInstance.ActivityName.Equals("待送审"))
                    {
                        _flowInstanceApp.Verification(req.Verification);
                        await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                        {
                            CertInfoId = certInfo.Id,
                            Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}送审证书。"
                        });
                        await UnitWork.UpdateAsync<Certinfo>(c => c.Id.Equals(req.CertInfoId), o => new Certinfo { Operator = loginContext.User.Name, OperatorId = loginContext.User.Id });
                        await UnitWork.SaveAsync();
                        //var signPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", nameDic.GetValueOrDefault(loginContext.User.Name));
                        //list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 1, ValueData = signPath1 });
                    }
                    else if (flowInstance.ActivityName.Equals("待审核"))
                    {
                        _flowInstanceApp.Verification(req.Verification);
                        await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                        {
                            CertInfoId = certInfo.Id,
                            Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}审批通过。"
                        });
                        var signPath2 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", nameDic.GetValueOrDefault(loginContext.User.Name));
                        list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 3, ValueData = signPath2 });
                    }
                    else if (flowInstance.ActivityName.Equals("待批准"))
                    {
                        _flowInstanceApp.Verification(req.Verification);
                        await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                        {
                            CertInfoId = certInfo.Id,
                            Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}批准证书。"
                        });
                        var signPath3 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", nameDic.GetValueOrDefault(loginContext.User.Name));
                        list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 3, YCellMark = 1, ValueData = signPath3 });
                        //var signetPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "印章.png");
                        //list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 3, YCellMark = 3, ValueData = signetPath });
                    }
                    #endregion
                    var templatePath = certInfo.CertPath;
                    var tagetPath = certInfo.CertPath;
                    var result = WordHandler.DOCTemplateConvert(templatePath, tagetPath, list);

                    var pdfPath = WordHandler.DocConvertToPdf(certInfo.CertPath);
                    if (!pdfPath.Equals("false"))
                    {
                        certInfo.PdfPath = pdfPath;
                        await UpdateAsync(certInfo.MapTo<AddOrUpdateCertinfoReq>());
                    }
                    break;
                case "2":
                    _flowInstanceApp.Verification(req.Verification);
                    await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                    {
                        CertInfoId = certInfo.Id,
                        Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}不同意证书。"
                    });
                    break;
                case "3":
                    _flowInstanceApp.Verification(req.Verification);
                    await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                    {
                        CertInfoId = certInfo.Id,
                        Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}驳回证书。"
                    });
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