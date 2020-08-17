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


            var result = new TableData();
            var objs = UnitWork.Find<Certinfo>(null);
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key));
            }
            objs = objs.WhereIf(!string.IsNullOrEmpty(request.CertNo), u => u.CertNo.Contains(request.CertNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.AssetNo), u => u.AssetNo.Contains(request.AssetNo))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Model), u => u.Model.Contains(request.Model))
                       .WhereIf(!string.IsNullOrWhiteSpace(request.Sn), u => u.Sn.Contains(request.Sn))
                       .WhereIf(!(request.CalibrationDateFrom == null && request.CalibrationDateTo == null), u => u.CalibrationDate >= request.CalibrationDateFrom && u.CalibrationDate <= request.CalibrationDateTo)
                ;
            var list = await objs.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

            var fId = list.Select(l => l.FlowInstanceId);
            var fs = await UnitWork.Find<FlowInstance>(f => fId.Contains(f.Id)).ToListAsync();
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
            var objs = UnitWork.Find<Certinfo>(null).Include(c => c.FlowInstance).AsQueryable();
            objs = objs
                .Where(o => o.FlowInstance.MakerList == "1" || o.FlowInstance.MakerList.Contains(user.User.Id)) //待办事项
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
            result.Data = list.Select(c =>
            {
                return new CertinfoView
                {
                    Id = c.Id,
                    CertNo = c.CertNo,
                    ActivityName = c.FlowInstance?.ActivityName,
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
            var certInfo = await Repository.Find(c => c.Id.Equals(req.CertInfoId)).Include(c => c.FlowInstance).FirstOrDefaultAsync();

            if (certInfo is null)
                throw new CommonException("证书不存在", 80001);

            var list = new List<WordModel>();
            switch (req.Verification.VerificationFinally)
            {
                case "1":
                    #region 签名
                    if (certInfo.FlowInstance.ActivityName.Equals("待送审"))
                    {
                        _flowInstanceApp.Verification(req.Verification);
                        await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                        {
                            Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}送审证书。"
                        });
                        var signPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "yang.png");
                        list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 1, ValueData = signPath1 });
                    }
                    else if (certInfo.FlowInstance.ActivityName.Equals("待审核"))
                    {
                        _flowInstanceApp.Verification(req.Verification);
                        await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                        {
                            Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}审批通过。"
                        });
                        var signPath2 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "zhou.png");
                        list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 3, ValueData = signPath2 });
                    }
                    else if (certInfo.FlowInstance.ActivityName.Equals("待批准"))
                    {
                        _flowInstanceApp.Verification(req.Verification);
                        await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                        {
                            Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}批准证书。"
                        });
                        var signPath3 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "chen.png");
                        list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 3, YCellMark = 1, ValueData = signPath3 });
                        var signetPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "印章.png");
                        list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 3, YCellMark = 3, ValueData = signetPath });
                    }
                    #endregion
                    var templatePath = certInfo.CertPath;
                    var tagetPath = certInfo.CertPath; ;
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
                        Action = $"{DateTime.Now:yyyy.MM.dd HH:mm} {loginContext.User.Name}不同意证书。"
                    });
                    break;
                case "3":
                    _flowInstanceApp.Verification(req.Verification);
                    await _certOperationHistoryApp.AddAsync(new AddOrUpdateCertOperationHistoryReq
                    {
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


        public CertinfoApp(IUnitWork unitWork, IRepository<Certinfo> repository,
            RevelanceManagerApp app, IAuth auth, FlowInstanceApp flowInstanceApp, CertOperationHistoryApp certOperationHistoryApp) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
            _flowInstanceApp = flowInstanceApp;
            _certOperationHistoryApp = certOperationHistoryApp;
        }
    }
}