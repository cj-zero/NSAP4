using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Threading.Tasks;
using Serilog;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using NStandard;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Order;
using OpenAuth.App.Response;
using OpenAuth.App.Order.Request;
using OpenAuth.App.ContractManager.Common;
using OpenAuth.App.Files;
using OpenAuth.App.SignalR;
using OpenAuth.App.DDVoice;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.App.ContractManager.Request;
using OpenAuth.App.CommonHelp;
using ICSharpCode.SharpZipLib.Zip;
using Minio;

namespace OpenAuth.App.ContractManager
{
    public class ContractApplyApp : BaseApp<UploadFile>
    {
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly ServiceSaleOrderApp _servcieSaleOrderApp;
        private readonly FlowSchemeApp _flowSchemeApp;
        private readonly DDVoiceApp _dDVoice;
        static Dictionary<int, DataTable> gRoles = new Dictionary<int, DataTable>();
        private readonly MinioClient _minioClient;
        private readonly FileApp _fileApp;
        private UserDepartMsgHelp _userDepartMsgHelp;
        private readonly IHubContext<MessageHub> _hubContext;
        private IFileStore _fileStore;
        private ILogger<FileApp> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="moduleFlowSchemeApp"></param>
        /// <param name="flowInstanceApp"></param>
        /// <param name="servcieSaleOrderApp"></param>
        /// <param name="auth"></param>
        public ContractApplyApp(IUnitWork unitWork, ModuleFlowSchemeApp moduleFlowSchemeApp, FlowInstanceApp flowInstanceApp,DDVoiceApp dDVoice,
            IRepository<UploadFile> repository, ServiceSaleOrderApp servcieSaleOrderApp, UserDepartMsgHelp userDepartMsgHelp, MinioClient minioClient, FileApp fileApp, FlowSchemeApp flowSchemeApp, IAuth auth, IFileStore fileStore, ILogger<FileApp> logger, IHubContext<MessageHub> hubContext) : base(unitWork, repository, auth)
        {
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _flowInstanceApp = flowInstanceApp;
            _servcieSaleOrderApp = servcieSaleOrderApp;
            _fileStore = fileStore;
            _logger = logger;
            _minioClient = minioClient;
            _fileApp = fileApp;
            _flowSchemeApp = flowSchemeApp;
            _hubContext = hubContext;
            _dDVoice = dDVoice;
            _userDepartMsgHelp = userDepartMsgHelp;
        }

        /// <summary>
        /// 获取合同列表
        /// </summary>
        /// <param name="request">合同申请单查询实体数据</param>
        /// <returns>返回合同列表信息</returns>
        public async Task<TableData> GetContractApplyList(QueryContractApplyListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            try
            {
                var file = UnitWork.Find<ContractFile>(null);
                if (loginContext.Roles.Any(r => r.Name.Equal("合同管理权限")))
                {
                    var objs = request.FileTypes == null || request.FileTypes.Count() <= 0 ? UnitWork.Find<ContractApply>(null).Include(r => r.ContractFileTypeList).Include(r => r.contractOperationHistoryList) : UnitWork.Find<ContractApply>(null).Include(r => r.ContractFileTypeList).Include(r => r.contractOperationHistoryList).Where(r => r.ContractFileTypeList.Any(x => request.FileTypes.Contains(x.FileType)));
                    var contractApply = (objs.WhereIf(!string.IsNullOrWhiteSpace(request.ContractNo), r => r.ContractNo.Contains(request.ContractNo))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerCodeOrName), r => r.CustomerCode.Contains(request.CustomerCodeOrName) || r.CustomerName.Contains(request.CustomerCodeOrName))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.CompanyType), r => r.CompanyType.Equals(request.CompanyType))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.QuotationNo), r => r.QuotationNo.Contains(request.QuotationNo))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.SaleNo), r => r.SaleNo.Contains(request.SaleNo))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.CreateName), r => r.CreateName.Contains(request.CreateName))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.ItemNo), r => r.ItemNo.Contains(request.ItemNo))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.ItemName), r => r.ItemName.Contains(request.ItemName))
                                            .WhereIf(request.StartDate != null, r => r.CreateTime >= request.StartDate)
                                            .WhereIf(request.EndDate != null, r => r.CreateTime <= request.EndDate)
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.ContractStatus), r => r.ContractStatus == request.ContractStatus)).ToList();

                    //通过反射将字段作为参数传入
                    contractApply = (request.SortOrder == "asc" ? contractApply.OrderBy(r => r.GetType().GetProperty(request.SortName == "" || request.SortName == null ? "CreateTime" : Regex.Replace(request.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null)) : contractApply.OrderByDescending(r => r.GetType().GetProperty(request.SortName == "" || request.SortName == null ? "CreateTime" : Regex.Replace(request.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null))).ToList();

                    //查询字典基本信息
                    var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var categoryContractList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var categoryStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    List<string> contractId = contractApply.Select(r => r.Id).ToList();
                    var contractFile = UnitWork.Find<ContractFileType>(r => contractId.Contains(r.ContractApplyId));
                    var contractApplyList = contractApply.Skip((request.page - 1) * request.limit).Take(request.limit).ToList();
                    var contractResp = from a in contractApplyList
                                       join b in categoryCompanyList on a.CompanyType equals b.DtValue
                                       join c in categoryContractList on a.ContractType equals c.DtValue
                                       join d in categoryStatusList on a.ContractStatus equals d.DtValue
                                       select new
                                       {
                                           a,
                                           CompanyDtValue = b.DtValue,
                                           CompanyName = b.Name,
                                           ContractDtValue = c.DtValue,
                                           ContractName = c.Name,
                                           StatusDtValue = d.DtValue,
                                           StatusName = d.Name
                                       };

                    var contractRespList = contractResp.Select(r => new
                    {
                        Id = r.a.Id,
                        ContractNo = r.a.ContractNo,
                        CustomerCode = r.a.CustomerCode,
                        CustomerName = r.a.CustomerName,
                        CompanyType = r.a.CompanyType,
                        ContractType = r.a.ContractType,
                        QuotationNo = r.a.QuotationNo,
                        SaleNo = r.a.SaleNo,
                        IsDraft = r.a.IsDraft,
                        IsUseCompanyTemplate = r.a.IsUseCompanyTemplate,
                        IsUploadOriginal = r.a.IsUploadOriginal,
                        FlowInstanceId = r.a.FlowInstanceId,
                        DownloadNumber = r.a.DownloadNumber,
                        U_SAP_ID = r.a.U_SAP_ID,
                        ItemNo = r.a.ItemNo,
                        ItemName = r.a.ItemName,
                        ContractStatus = r.a.ContractStatus,
                        Remark = r.a.Remark,
                        CreateId = r.a.CreateId,
                        CreateName = r.a.CreateName,
                        DeptName = _userDepartMsgHelp.GetUserOrgName(r.a.CreateId),
                        CreateTime = r.a.CreateTime,
                        UpdateTime = r.a.UpdateTime,
                        CompanyDtValue = r.CompanyDtValue,
                        CompanyName = r.CompanyName,
                        ContractDtValue = r.ContractDtValue,
                        ContractName = r.ContractName,
                        StatusDtValue = r.StatusDtValue,
                        StatusName = r.StatusName,
                        ContractFileTypeList = r.a.ContractFileTypeList.Select(x => new
                        {
                            x.Id,
                            x.ContractApplyId,
                            x.ContractSealId,
                            x.FileType,
                            x.ContractOriginalId,
                            x.FileNum,
                            x.Remark,
                            ContractFileList = file.Where(f => f.ContractFileTypeId == x.Id)
                        }),
                        ContractHistoryList = r.a.contractOperationHistoryList.OrderByDescending(o => o.CreateTime)
                    }).OrderByDescending(r => r.CreateTime).ToList();

                    result.Count = contractApply.Count();
                    result.Data = contractRespList;
                }
                else
                {
                    var objs = request.FileTypes == null || request.FileTypes.Count() <= 0 ? UnitWork.Find<ContractApply>(r => r.CreateId == loginUser.Id).Include(r => r.ContractFileTypeList).Include(r => r.contractOperationHistoryList) : UnitWork.Find<ContractApply>(r => r.CreateId == loginUser.Id).Include(r => r.ContractFileTypeList).Include(r => r.contractOperationHistoryList).Where(r => r.ContractFileTypeList.Any(x => request.FileTypes.Contains(x.FileType)));
                    var contractApply = (objs.WhereIf(!string.IsNullOrWhiteSpace(request.ContractNo), r => r.ContractNo.Contains(request.ContractNo))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerCodeOrName), r => r.CustomerCode.Contains(request.CustomerCodeOrName) || r.CustomerName.Contains(request.CustomerCodeOrName))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.CompanyType), r => r.CompanyType.Equals(request.CompanyType))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.QuotationNo), r => r.QuotationNo.Contains(request.QuotationNo))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.SaleNo), r => r.SaleNo.Contains(request.SaleNo))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.CreateName), r => r.CreateName.Contains(request.CreateName))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.ItemNo), r => r.ItemNo.Contains(request.ItemNo))
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.ItemName), r => r.ItemName.Contains(request.ItemName))
                                            .WhereIf(request.StartDate != null, r => r.CreateTime >= request.StartDate)
                                            .WhereIf(request.EndDate != null, r => r.CreateTime <= request.EndDate)
                                            .WhereIf(!string.IsNullOrWhiteSpace(request.ContractStatus), r => r.ContractStatus == request.ContractStatus)).ToList();

                    //通过反射将字段作为参数传入
                    contractApply = (request.SortOrder == "asc" ? contractApply.OrderBy(r => r.GetType().GetProperty(request.SortName == "" || request.SortName == null ? "CreateTime" : Regex.Replace(request.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null)) : contractApply.OrderByDescending(r => r.GetType().GetProperty(request.SortName == "" || request.SortName == null ? "CreateTime" : Regex.Replace(request.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null))).ToList();

                    //查询字典基本信息
                    var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var categoryContractList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var categoryStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    List<string> contractId = contractApply.Select(r => r.Id).ToList();
                    var contractFile = UnitWork.Find<ContractFileType>(r => contractId.Contains(r.ContractApplyId));
                    var contractApplyList = contractApply.Skip((request.page - 1) * request.limit).Take(request.limit).ToList();
                    var contractResp = from a in contractApplyList
                                       join b in categoryCompanyList on a.CompanyType equals b.DtValue
                                       join c in categoryContractList on a.ContractType equals c.DtValue
                                       join d in categoryStatusList on a.ContractStatus equals d.DtValue
                                       select new
                                       {
                                           a,
                                           CompanyDtValue = b.DtValue,
                                           CompanyName = b.Name,
                                           ContractDtValue = c.DtValue,
                                           ContractName = c.Name,
                                           StatusDtValue = d.DtValue,
                                           StatusName = d.Name
                                       };

                    var contractRespList = contractResp.Select(r => new
                    {
                        Id = r.a.Id,
                        ContractNo = r.a.ContractNo,
                        CustomerCode = r.a.CustomerCode,
                        CustomerName = r.a.CustomerName,
                        CompanyType = r.a.CompanyType,
                        ContractType = r.a.ContractType,
                        QuotationNo = r.a.QuotationNo,
                        SaleNo = r.a.SaleNo,
                        IsDraft = r.a.IsDraft,
                        IsUseCompanyTemplate = r.a.IsUseCompanyTemplate,
                        IsUploadOriginal = r.a.IsUploadOriginal,
                        FlowInstanceId = r.a.FlowInstanceId,
                        DownloadNumber = r.a.DownloadNumber,
                        U_SAP_ID = r.a.U_SAP_ID,
                        ItemNo = r.a.ItemNo,
                        ItemName = r.a.ItemName,
                        ContractStatus = r.a.ContractStatus,
                        Remark = r.a.Remark,
                        CreateId = r.a.CreateId,
                        CreateName = r.a.CreateName,
                        DeptName = _userDepartMsgHelp.GetUserOrgName(r.a.CreateId),
                        CreateTime = r.a.CreateTime,
                        UpdateTime = r.a.UpdateTime,
                        CompanyDtValue = r.CompanyDtValue,
                        CompanyName = r.CompanyName,
                        ContractDtValue = r.ContractDtValue,
                        ContractName = r.ContractName,
                        StatusDtValue = r.StatusDtValue,
                        StatusName = r.StatusName,
                        ContractFileTypeList = r.a.ContractFileTypeList.Select(x => new
                        {
                            x.Id,
                            x.ContractApplyId,
                            x.ContractSealId,
                            x.FileType,
                            x.ContractOriginalId,
                            x.FileNum,
                            x.Remark,
                            ContractFileList = file.Where(f => f.ContractFileTypeId == x.Id)
                        }),
                        ContractHistoryList = r.a.contractOperationHistoryList.OrderByDescending(o => o.CreateTime)
                    }).ToList();

                    result.Count = contractApply.Count();
                    result.Data = contractRespList;
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message.ToString();
                result.Code = 500;
            }
            return result;
        }

        /// <summary>
        /// 加载合同申请单详情
        /// </summary>
        /// <param name="contractApplyId">合同申请单Id</param>
        /// <returns>成功返回详情数据，失败抛出异常</returns>
        public async Task<TableData> GetDetails(string contractApplyId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            try
            {
                var contractApply = await UnitWork.Find<ContractApply>(r => r.Id == contractApplyId).ToListAsync();
                var contractFileTypeList = await UnitWork.Find<ContractFileType>(r => r.ContractApplyId == contractApplyId).Include(r => r.contractFileList).ToListAsync();
                var contractOperationHistoryList = await UnitWork.Find<ContractOperationHistory>(r => r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).ToListAsync();
                List<ContractHistory> contractHistories  = contractOperationHistoryList.Select(r => new ContractHistory
                {
                    Id = r.Id,
                    ContractApplyId = r.ContractApplyId,
                    Action = r.Action,
                    DeptName = (_userDepartMsgHelp.GetUserOrgName(r.CreateUserId)),
                    CreateUser = r.CreateUser,
                    CreateUserId = r.CreateUserId,
                    CreateTime = r.CreateTime,
                    IntervalTime = r.IntervalTime,
                    ApprovalResult = r.ApprovalResult,
                    Remark = r.Remark,
                    ApprovalStage = r.ApprovalStage
                }).ToList();

                if (contractApply != null && contractApply.Count() > 0)
                {
                    var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var categoryContractList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var categoryStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var categoryFileTypeList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();

                    #region 申请单主表信息
                    var contractApplyReq = from a in contractApply
                                           join b in categoryCompanyList on a.CompanyType equals b.DtValue
                                           join c in categoryContractList on a.ContractType equals c.DtValue
                                           join d in categoryStatusList on a.ContractStatus equals d.DtValue
                                           select new
                                           {
                                               a.Id,
                                               a.ContractNo,
                                               a.CustomerCode,
                                               a.CustomerName,
                                               a.CompanyType,
                                               a.ContractType,
                                               a.QuotationNo,
                                               a.SaleNo,
                                               a.IsDraft,
                                               a.IsUseCompanyTemplate,
                                               a.FlowInstanceId,
                                               a.DownloadNumber,
                                               a.U_SAP_ID,
                                               a.ItemNo,
                                               a.ItemName,
                                               a.ContractStatus,
                                               a.IsUploadOriginal,
                                               a.Remark,
                                               a.CreateId,
                                               DeptName = _userDepartMsgHelp.GetUserOrgName(a.CreateId),
                                               CreateName = a.CreateName,
                                               a.CreateTime,
                                               a.UpdateTime,
                                               CompanyDtValue = b.DtValue,
                                               CompanyName = b.Name,
                                               ContractDtValue = c.DtValue,
                                               ContractName = c.Name,
                                               StatusDtValue = d.DtValue,
                                               StatusName = d.Name
                                           };
                    #endregion

                    #region 审批步骤
                    var contract = contractApply.FirstOrDefault();
                    List<ApprovalNodeReq> nodeList = await GetNodeMsg(contract, contractApplyId);
                    #endregion

                    if (contractFileTypeList.Count() > 0)
                    {
                        var contractOriginalFileList = await UnitWork.Find<UploadFile>(r => (contractFileTypeList.Select(r => r.ContractOriginalId)).Contains(r.Id)).Select(u => new { u.FileName, u.Id, u.FilePath, u.FileType, u.FileSize, u.Extension }).ToListAsync();
                        List<string> fileIdList = new List<string>();
                        foreach (ContractFileType item in contractFileTypeList)
                        {
                            var fileList = await UnitWork.Find<ContractFile>(r => r.ContractFileTypeId == item.Id).ToListAsync();
                            foreach (ContractFile file in fileList)
                            {
                                fileIdList.Add(file.FileId);
                            }
                        }

                        var contractFileList = await UnitWork.Find<UploadFile>(r => fileIdList.Contains(r.Id)).Select(u => new { u.FileName, u.Id, u.FilePath, u.FileType, u.FileSize, u.Extension }).ToListAsync();

                        #region 申请单文件表信息
                        var contractTypeFileReq = from a in contractFileTypeList
                                                  join c in contractOriginalFileList on a.ContractOriginalId equals c.Id into ac
                                                  from c in ac.DefaultIfEmpty()
                                                  join d in categoryFileTypeList on a.FileType equals d.DtValue
                                                  select new
                                                  {
                                                      a.Id,
                                                      a.ContractApplyId,
                                                      a.ContractSealId,
                                                      a.FileType,
                                                      a.ContractOriginalId,
                                                      a.FileNum,
                                                      a.Remark,
                                                      ContractFileList = from b in a.contractFileList
                                                                         join e in contractFileList on b.FileId equals e.Id into be
                                                                         from e in be.DefaultIfEmpty()
                                                                         orderby b.CreateUploadTime descending
                                                                         select new
                                                                         {
                                                                             b.Id,
                                                                             b.ContractFileTypeId,
                                                                             b.FileId,
                                                                             b.IsSeal,
                                                                             b.IsFinalContract,
                                                                             b.CreateUploadId,
                                                                             CreateUploadName = b.CreateUploadName,
                                                                             CreateDeptName = _userDepartMsgHelp.GetUserOrgName(b.CreateUploadId),
                                                                             b.CreateUploadTime,
                                                                             b.UpdateUserId,
                                                                             UpdateUserName = b.UpdateUserName,
                                                                             UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(b.UpdateUserId),
                                                                             b.UpdateUserTime,
                                                                             FileName = e == null ? "" : e.FileName,
                                                                             FilePath = e == null ? "" : e.FilePath,
                                                                             FileType = e == null ? "" : e.FileType,
                                                                             FileSize = e == null ? 0 : e.FileSize,
                                                                             Extension = e == null ? "" : e.Extension
                                                                         },
                                                      OriginalFileName = c == null ? "" : c.FileName,
                                                      OriginalFilePath = c == null ? "" : c.FilePath,
                                                      OriginalFileType = c == null ? "" : c.FileType,
                                                      OriginalFileSize = c == null ? 0 : c.FileSize,
                                                      OriginalExtension = c == null ? "" : c.Extension,
                                                      CategoryFileType = d.DtValue,
                                                      CategoryFileName = d.Name
                                                  };
                        #endregion

                        result.Data = new
                        {
                            ContractApplyReq = contractApplyReq,
                            ContractOperationHistoryList = contractHistories,
                            ContractFileTypeReq = contractTypeFileReq,
                            ContractStatusReq = nodeList
                        };
                    }
                    else
                    {
                        result.Data = new
                        {
                            ContractApplyReq = contractApplyReq,
                            ContractOperationHistoryList = contractHistories,
                            ContractFileTypeReq = new List<ContractFileType>(),
                            ContractStatusReq = nodeList
                        };
                    }
                }
                else
                {
                    result.Message = "该申请单不存在";
                }
            }
            catch (Exception ex)
            {
                throw new Exception("合同申请单详情查询失败,请重试");
            }

            return result;
        }

        /// <summary>
        /// 获取客户/报价单/销售订单合同信息
        /// </summary>
        /// <param name="ModuleType">模块类型（客户-KHXQ，销售报价单-XSBJD，销售订单-XSDD）</param>
        /// <param name="ModuleCode">模块编码</param>
        /// <returns>返回客户详情/销售报价单/销售订单模块对应的合同申请单</returns>
        public async Task<TableData> GetCardOrOrderDraftOrSaleOrderContract(string ModuleType, string ModuleCode)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            if (string.IsNullOrEmpty(ModuleCode) || string.IsNullOrEmpty(ModuleType))
            {
                result.Code = 500;
                result.Message = "模块编码或模块类型不允许为空";
            }
            else
            {
                List<ContractApplyMsgHelp> contractApplyMsgHelps = new List<ContractApplyMsgHelp>();
                var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                var categoryContractList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                var categoryStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                var categoryFileTypeList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                if (ModuleType == "XSBJD")
                {
                    //获取销售报价单关联的合同申请单
                    List<ContractApplyMsgHelp> contractApplyMsgHelpList = await UnitWork.Find<ContractApply>(r => r.QuotationNo.Contains(ModuleCode)).Select(r => new ContractApplyMsgHelp
                    {
                        Id = r.Id,
                        ContractNo = r.ContractNo,
                        QuotationNo = r.QuotationNo,
                        ContractType = r.ContractType,
                        CompanyType = r.CompanyType,
                        CreateId = r.CreateId,
                        CreateName = r.CreateName,
                        CreateTime = r.CreateTime.ToString(),
                        ContractStatus = r.ContractStatus
                    }).ToListAsync();

                    foreach (ContractApplyMsgHelp item in contractApplyMsgHelpList)
                    {
                        if (!string.IsNullOrEmpty(item.QuotationNo))
                        {
                            if (GetModuleCode(item.QuotationNo, ModuleCode) == item.QuotationNo)
                            {
                                contractApplyMsgHelps.Add(item);
                            }
                        }
                    }
                }
                else if (ModuleType == "XSDD")
                {
                    //获取销售订单关联的合同申请单
                    List<ContractApplyMsgHelp> contractApplyMsgHelpList = await UnitWork.Find<ContractApply>(r => r.SaleNo.Contains(ModuleCode)).Select(r => new ContractApplyMsgHelp
                    {
                        Id = r.Id,
                        ContractNo = r.ContractNo,
                        ContractType = r.ContractType,
                        CompanyType = r.CompanyType,
                        CreateId = r.CreateId,
                        CreateName = r.CreateName,
                        CreateTime = r.CreateTime.ToString(),
                        ContractStatus = r.ContractStatus,
                        SaleNo = r.SaleNo
                    }).ToListAsync();

                    foreach (ContractApplyMsgHelp item in contractApplyMsgHelpList)
                    {
                        if (!string.IsNullOrEmpty(item.SaleNo))
                        {
                            if (GetModuleCode(item.SaleNo, ModuleCode) == item.SaleNo)
                            {
                                contractApplyMsgHelps.Add(item);
                            }
                        }
                    }
                }
                else
                {
                    //获取客户关联的合同申请单
                    contractApplyMsgHelps = await UnitWork.Find<ContractApply>(r => r.CustomerCode == ModuleCode).Select(r => new ContractApplyMsgHelp
                    {
                        Id = r.Id,
                        ContractNo = r.ContractNo,
                        ContractType = r.ContractType,
                        CompanyType = r.CompanyType,
                        CreateId = r.CreateId,
                        CreateName = r.CreateName,
                        CreateTime = r.CreateTime.ToString(),
                        ContractStatus = r.ContractStatus
                    }).ToListAsync();
                }

                if (contractApplyMsgHelps != null && contractApplyMsgHelps.Count() > 0)
                {
                    //查询字典
                    var contractResps = (from a in contractApplyMsgHelps
                                         join b in categoryCompanyList on a.CompanyType equals b.DtValue
                                         join c in categoryContractList on a.ContractType equals c.DtValue
                                         join d in categoryStatusList on a.ContractStatus equals d.DtValue
                                         select new ContractApplyMsgHelp
                                         {
                                             Id = a.Id,
                                             ContractNo = a.ContractNo,
                                             ContractType = a.ContractType,
                                             ContractTypeName = c.Name,
                                             CreateName = a.CreateName,
                                             CreateTime = a.CreateTime,
                                             CompanyType = a.CompanyType,
                                             CompanyName = b.Name,
                                             ContractStatus = a.ContractStatus,
                                             ContractStatusName = d.Name,
                                             CreateId = a.CreateId
                                         }).ToList();

                    //查找合同申请单对应的合同文件类型
                    var contractFileTypes = await UnitWork.Find<ContractFileType>(null).Select(r => new { r.ContractApplyId, r.FileType }).ToListAsync();
                    var contractFiles = (from a in contractResps
                                         join b in contractFileTypes on a.Id equals b.ContractApplyId into ab
                                         from b in ab.DefaultIfEmpty()
                                         select new ContractMsgHelp
                                         {
                                             ContractNo = a.ContractNo,
                                             ContractTypeName = a.ContractTypeName,
                                             FileTypeName = b == null ? "" : b.FileType,
                                             CreateId = a.CreateId,
                                             CreateName = a.CreateName,
                                             CreateTime = a.CreateTime,
                                             CompanyName = a.CompanyName,
                                             ContractStatusName = a.ContractStatusName
                                         }).ToList();

                    //查询文件类型名称
                    contractFiles = (from a in contractFiles
                                     join b in categoryFileTypeList on a.FileTypeName equals b.DtValue into ab
                                     from b in ab.DefaultIfEmpty()
                                     select new ContractMsgHelp
                                     {
                                         ContractNo = a.ContractNo,
                                         ContractTypeName = a.ContractTypeName,
                                         FileTypeName = b == null ? "" : b.Name,
                                         CreateId = a.CreateId,
                                         CreateName = a.CreateName,
                                         DeptName = _userDepartMsgHelp.GetUserOrgName(a.CreateId),
                                         CreateTime = a.CreateTime,
                                         CompanyName = a.CompanyName,
                                         ContractStatusName = a.ContractStatusName
                                     }).ToList();

                    result.Data = contractFiles;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取模块编码
        /// </summary>
        /// <param name="OldNo">旧编号</param>
        /// <param name="ModuleCode">模块编码</param>
        /// <returns>当旧编码包括模块编码时，返回旧编码，否则返回模块编码</returns>
        public string GetModuleCode(string OldNo, string ModuleCode)
        {
            if (OldNo.Contains(","))
            {
                List<string> oldNos = OldNo.Split(',').ToList();
                var moduleCodes = oldNos.Where(r => r == ModuleCode).ToList();
                if (moduleCodes != null && moduleCodes.Count() > 0)
                {
                    return OldNo;
                }
            }

            return ModuleCode;
        }

        /// <summary>
        /// 获取审批步骤
        /// </summary>
        /// <param name="contract">合同实体数据</param>
        /// <param name="contractApplyId">合同申请单Id</param>
        /// <returns>返回合同申请各个节点信息</returns>
        public async Task<List<ApprovalNodeReq>> GetNodeMsg(ContractApply contract, string contractApplyId)
        {
            string userName = "";
            var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("订单合同申请"));
            string flowId = mf.FlowSchemeId;
            FlowScheme scheme = null;
            if (!string.IsNullOrEmpty(flowId))
            {
                scheme = await _flowSchemeApp.GetAsync(flowId);
            }

            if (scheme == null)
            {
                throw new Exception("该流程模板已不存在，请重新设计流程");
            }

            List<ApprovalNodeReq> nodeList = new List<ApprovalNodeReq>();
            ApprovalNodeReq nodeReq = new ApprovalNodeReq();
            nodeReq.NodeName = "提交";
            nodeReq.NodeStatus = contract.ContractStatus == "1" ? "1" : (contract.ContractStatus == "2" ? "2" : "");
            nodeReq.NodeUser = contract.CreateName;
            nodeList.Add(nodeReq);
            if (contract.ContractType == "1")
            {
                if ((contract.ContractStatus == "1" || contract.ContractStatus == "2" || contract.ContractStatus == "3") && !contract.IsUseCompanyTemplate)
                {
                    var roles = await UnitWork.Find<Role>(r => r.Name == "法务人员").Select(r => new { r.Id }).ToListAsync();
                    List<string> roleId = roles.Select(r => r.Id).ToList();
                    var userole = await UnitWork.Find<Relevance>(u => roleId.Contains(u.SecondId) && u.Key == Define.USERROLE).ToListAsync();
                    var userList = await UnitWork.Find<User>(null).ToListAsync();
                    var users = from userRole in userole
                                join user in userList on userRole.FirstId equals user.Id into temp
                                from c in temp.Where(u => u.Id != null)
                                select new
                                {
                                    c.Id,
                                    c.Name
                                };

                    int count = 0;
                    foreach (var userItem in users)
                    {
                        if (users.Count() == 1)
                        {
                            userName = userItem.Name;
                        }
                        else
                        {
                            userName = count == 0 ? userItem.Name : userName + "," + userItem.Name;
                            count++;
                        }
                    }

                    ApprovalNodeReq nodeFWReq = new ApprovalNodeReq();
                    nodeFWReq.NodeName = "法务审批";
                    nodeFWReq.NodeStatus = "3";
                    nodeFWReq.NodeUser = "等待" + userName + "审批";
                    nodeList.Add(nodeFWReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "等待骆灵芝/吴秋丽审批";
                    nodeList.Add(nodeZZReq);
                }
                else if (contract.ContractStatus == "5" && !contract.IsUseCompanyTemplate)
                {
                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => r.ApprovalStage == "5" && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = "法务审批";
                    nodeFJReq.NodeStatus = "3";
                    nodeFJReq.NodeUser = historyData == null ? "" : historyData.CreateUser + "已审批";
                    nodeList.Add(nodeFJReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "等待骆灵芝/吴秋丽审批";
                    nodeList.Add(nodeZZReq);
                }
                else if ((contract.ContractStatus == "6" || contract.ContractStatus == "7" || contract.ContractStatus == "-1") && !contract.IsUseCompanyTemplate)
                {
                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => r.ApprovalStage == "5" && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = "法务审批";
                    nodeFJReq.NodeStatus = "3";
                    nodeFJReq.NodeUser = historyData == null ? "" : historyData.CreateUser + "已审批";
                    nodeList.Add(nodeFJReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "骆灵芝/吴秋丽已审批";
                    nodeList.Add(nodeZZReq);
                }

                ApprovalNodeReq nodeGZReq = new ApprovalNodeReq();
                nodeGZReq.NodeName = "待盖章";
                nodeGZReq.NodeStatus = "6";
                nodeGZReq.NodeUser = "";
                nodeList.Add(nodeGZReq);

                ApprovalNodeReq nodeSCReq = new ApprovalNodeReq();
                nodeSCReq.NodeName = "待上传原件";
                nodeSCReq.NodeStatus = "7";
                nodeSCReq.NodeUser = "";
                nodeList.Add(nodeSCReq);

                ApprovalNodeReq nodeEndReq = new ApprovalNodeReq();
                nodeEndReq.NodeName = "结束";
                nodeEndReq.NodeStatus = "-1";
                nodeEndReq.NodeUser = "";
                nodeList.Add(nodeEndReq);
            }
            else if (contract.ContractType == "2")
            {
                if (contract.ContractStatus == "1" || contract.ContractStatus == "2" || contract.ContractStatus == "4")
                {
                    List<Role> roles = await UnitWork.Find<Role>(r => r.Name == "法务人员" || r.Name == "售前工程师").ToListAsync();
                    List<string> roleId = roles.Select(r => r.Id).ToList();
                    var userole = await UnitWork.Find<Relevance>(u => roleId.Contains(u.SecondId) && u.Key == Define.USERROLE).ToListAsync();
                    var userList = await UnitWork.Find<User>(null).ToListAsync();
                    var users = from userRole in userole
                                join user in userList on userRole.FirstId equals user.Id into temp
                                from c in temp.Where(u => u.Id != null)
                                select new
                                {
                                    c.Id,
                                    c.Name
                                };

                    int count = 0;
                    foreach (var userItem in users)
                    {
                        if (users.Count() == 1)
                        {
                            userName = userItem.Name;
                        }
                        else
                        {
                            userName = count == 0 ? userItem.Name : userName + "," + userItem.Name;
                            count++;
                        }
                    }

                    ApprovalNodeReq nodeFWReq = new ApprovalNodeReq();
                    nodeFWReq.NodeName = "法务审批&技术审批";
                    nodeFWReq.NodeStatus = "4";
                    nodeFWReq.NodeUser = "等待" + userName + "审批";
                    nodeList.Add(nodeFWReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "等待骆灵芝/吴秋丽审批";
                    nodeList.Add(nodeZZReq);
                }
                else if (contract.ContractStatus == "8")
                {
                    List<Role> roles = await UnitWork.Find<Role>(r => r.Name == "售前工程师").ToListAsync();
                    List<string> roleId = roles.Select(r => r.Id).ToList();
                    var userole = await UnitWork.Find<Relevance>(u => roleId.Contains(u.SecondId) && u.Key == Define.USERROLE).ToListAsync();
                    var userList = await UnitWork.Find<User>(null).ToListAsync();
                    var users = from userRole in userole
                                join user in userList on userRole.FirstId equals user.Id into temp
                                from c in temp.Where(u => u.Id != null)
                                select new
                                {
                                    c.Id,
                                    c.Name
                                };

                    int count = 0;
                    foreach (var userItem in users)
                    {
                        if (users.Count() == 1)
                        {
                            userName = userItem.Name;
                        }
                        else
                        {
                            userName = count == 0 ? userItem.Name : userName + "," + userItem.Name;
                            count++;
                        }
                    }

                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => r.ApprovalStage == "8" && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = "法务审批&技术审批";
                    nodeFJReq.NodeStatus = "8";
                    nodeFJReq.NodeUser = historyData.CreateUser + "已审批，等待" + userName + "审批";
                    nodeList.Add(nodeFJReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "等待骆灵芝/吴秋丽审批";
                    nodeList.Add(nodeZZReq);
                }
                else if (contract.ContractStatus == "9")
                {
                    var roles = await UnitWork.Find<Role>(r => r.Name == "法务人员").Select(r => new { r.Id }).FirstOrDefaultAsync();
                    var userRoleList = await UnitWork.Find<Relevance>(u => u.SecondId == roles.Id && u.Key == Define.USERROLE).ToListAsync();
                    var userList = await UnitWork.Find<User>(null).ToListAsync();
                    var users = from userRole in userRoleList
                                join user in userList on userRole.FirstId equals user.Id into temp
                                from c in temp.Where(u => u.Id != null)
                                select new
                                {
                                    c.Id,
                                    c.Name
                                };

                    int count = 0;
                    foreach (var userItem in users)
                    {
                        if (users.Count() == 1)
                        {
                            userName = userItem.Name;
                        }
                        else
                        {
                            userName = count == 0 ? userItem.Name : userName + "," + userItem.Name;
                            count++;
                        }
                    }

                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => r.ApprovalStage == "9" && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = "法务审批&技术审批";
                    nodeFJReq.NodeStatus = "9";
                    nodeFJReq.NodeUser = historyData.CreateUser + "已审批，等待" + userName + "审批";
                    nodeList.Add(nodeFJReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "等待骆灵芝/吴秋丽审批";
                    nodeList.Add(nodeZZReq);
                }
                else if (contract.ContractStatus == "5")
                {
                    int count = 0;
                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => (r.ApprovalStage == "8" || r.ApprovalStage == "9") && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).ToListAsync();
                    foreach (ContractOperationHistory item in historyData)
                    {
                        userName = count == 0 ? item.CreateUser : userName + "," + item.CreateUser;
                        count++;
                    }

                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = "法务审批&技术审批";
                    nodeFJReq.NodeStatus = "8";
                    nodeFJReq.NodeUser = userName + "已审批";
                    nodeList.Add(nodeFJReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "等待骆灵芝/吴秋丽审批";
                    nodeList.Add(nodeZZReq);
                }
                else if (contract.ContractStatus == "6" || contract.ContractStatus == "7" || contract.ContractStatus == "-1")
                {
                    int count = 0;
                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => (r.ApprovalStage == "8" || r.ApprovalStage == "9" || r.ApprovalStage == "5") && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).ToListAsync();
                    foreach (ContractOperationHistory item in historyData)
                    {
                        userName = count == 0 ? item.CreateUser : userName + "," + item.CreateUser;
                        count++;
                    }

                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = "法务审批&技术审批";
                    nodeFJReq.NodeStatus = "8";
                    nodeFJReq.NodeUser = userName + "已审批";
                    nodeList.Add(nodeFJReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "骆灵芝/吴秋丽已审批";
                    nodeList.Add(nodeZZReq);
                }

                ApprovalNodeReq nodeGZReq = new ApprovalNodeReq();
                nodeGZReq.NodeName = "待盖章";
                nodeGZReq.NodeStatus = "6";
                nodeGZReq.NodeUser = "";
                nodeList.Add(nodeGZReq);

                ApprovalNodeReq nodeSCReq = new ApprovalNodeReq();
                nodeSCReq.NodeName = "待上传原件";
                nodeSCReq.NodeStatus = "7";
                nodeSCReq.NodeUser = "";
                nodeList.Add(nodeSCReq);

                ApprovalNodeReq nodeEndReq = new ApprovalNodeReq();
                nodeEndReq.NodeName = "结束";
                nodeEndReq.NodeStatus = "-1";
                nodeEndReq.NodeUser = "";
                nodeList.Add(nodeEndReq);
            }
            else if (contract.ContractType == "3")
            {
                if (contract.ContractStatus == "1" || contract.ContractStatus == "2" || contract.ContractStatus == "10")
                {
                    var roles = await UnitWork.Find<Role>(r => r.Name == "法务人员").Select(r => new { r.Id }).ToListAsync();
                    List<string> roleId = roles.Select(r => r.Id).ToList();
                    var userole = await UnitWork.Find<Relevance>(u => roleId.Contains(u.SecondId) && u.Key == Define.USERROLE).ToListAsync();
                    var userList = await UnitWork.Find<User>(null).ToListAsync();
                    var users = from userRole in userole
                                join user in userList on userRole.FirstId equals user.Id into temp
                                from c in temp.Where(u => u.Id != null)
                                select new
                                {
                                    c.Id,
                                    c.Name
                                };

                    int count = 0;
                    foreach (var userItem in users)
                    {
                        if (users.Count() == 1)
                        {
                            userName = userItem.Name;
                        }
                        else
                        {
                            userName = count == 0 ? userItem.Name : userName + "," + userItem.Name;
                            count++;
                        }
                    }

                    ApprovalNodeReq nodeFWReq = new ApprovalNodeReq();
                    nodeFWReq.NodeName = "法务/总助审批";
                    nodeFWReq.NodeStatus = "10";
                    nodeFWReq.NodeUser = "等待(" + userName + ")或骆灵芝审批";
                    nodeList.Add(nodeFWReq);
                }
                else if (contract.ContractStatus == "-1")
                {
                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => r.ApprovalStage == "-1" && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = historyData.Action;
                    nodeFJReq.NodeStatus = "-1";
                    nodeFJReq.NodeUser = historyData == null ? "" : historyData.CreateUser + "已审批";
                    nodeList.Add(nodeFJReq);
                }

                ApprovalNodeReq nodeEndReq = new ApprovalNodeReq();
                nodeEndReq.NodeName = "结束";
                nodeEndReq.NodeStatus = "-1";
                nodeEndReq.NodeUser = "";
                nodeList.Add(nodeEndReq);
            }
            else
            {
                if ((contract.ContractStatus == "1" || contract.ContractStatus == "2" || contract.ContractStatus == "12") && !contract.IsUseCompanyTemplate)
                {
                    var roles = await UnitWork.Find<Role>(r => r.Name == "售前工程师").Select(r => new { r.Id }).ToListAsync();
                    List<string> roleId = roles.Select(r => r.Id).ToList();
                    var userole = await UnitWork.Find<Relevance>(u => roleId.Contains(u.SecondId) && u.Key == Define.USERROLE).ToListAsync();
                    var userList = await UnitWork.Find<User>(null).ToListAsync();
                    var users = from userRole in userole
                                join user in userList on userRole.FirstId equals user.Id into temp
                                from c in temp.Where(u => u.Id != null)
                                select new
                                {
                                    c.Id,
                                    c.Name
                                };

                    int count = 0;
                    foreach (var userItem in users)
                    {
                        if (users.Count() == 1)
                        {
                            userName = userItem.Name;
                        }
                        else
                        {
                            userName = count == 0 ? userItem.Name : userName + "," + userItem.Name;
                            count++;
                        }
                    }

                    ApprovalNodeReq nodeFWReq = new ApprovalNodeReq();
                    nodeFWReq.NodeName = "技术审批";
                    nodeFWReq.NodeStatus = "12";
                    nodeFWReq.NodeUser = "等待" + userName + "审批";
                    nodeList.Add(nodeFWReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "等待骆灵芝/吴秋丽审批";
                    nodeList.Add(nodeZZReq);
                }
                else if (contract.ContractStatus == "5" && !contract.IsUseCompanyTemplate)
                {
                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => r.ApprovalStage == "5" && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = "技术审批";
                    nodeFJReq.NodeStatus = "12";
                    nodeFJReq.NodeUser = historyData == null ? "" : historyData.CreateUser + "已审批";
                    nodeList.Add(nodeFJReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "等待骆灵芝/吴秋丽审批";
                    nodeList.Add(nodeZZReq);
                }
                else if ((contract.ContractStatus == "6" || contract.ContractStatus == "7" || contract.ContractStatus == "-1") && !contract.IsUseCompanyTemplate)
                {
                    var historyData = await UnitWork.Find<ContractOperationHistory>(r => r.ApprovalStage == "5" && r.ContractApplyId == contractApplyId).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                    ApprovalNodeReq nodeFJReq = new ApprovalNodeReq();
                    nodeFJReq.NodeName = "技术审批";
                    nodeFJReq.NodeStatus = "12";
                    nodeFJReq.NodeUser = historyData == null ? "" : historyData.CreateUser + "已审批";
                    nodeList.Add(nodeFJReq);

                    ApprovalNodeReq nodeZZReq = new ApprovalNodeReq();
                    nodeZZReq.NodeName = "总助审批";
                    nodeZZReq.NodeStatus = "5";
                    nodeZZReq.NodeUser = "骆灵芝/吴秋丽已审批";
                    nodeList.Add(nodeZZReq);
                }

                ApprovalNodeReq nodeGZReq = new ApprovalNodeReq();
                nodeGZReq.NodeName = "待盖章";
                nodeGZReq.NodeStatus = "6";
                nodeGZReq.NodeUser = "";
                nodeList.Add(nodeGZReq);

                ApprovalNodeReq nodeSCReq = new ApprovalNodeReq();
                nodeSCReq.NodeName = "待上传原件";
                nodeSCReq.NodeStatus = "7";
                nodeSCReq.NodeUser = "";
                nodeList.Add(nodeSCReq);

                ApprovalNodeReq nodeEndReq = new ApprovalNodeReq();
                nodeEndReq.NodeName = "结束";
                nodeEndReq.NodeStatus = "-1";
                nodeEndReq.NodeUser = "";
                nodeList.Add(nodeEndReq);
            }

            return nodeList;
        }

        /// <summary>
        /// 撤回合同申请单
        /// </summary>
        /// <param name="req">撤回合同申请单实体</param>
        /// <returns></returns>
        public async Task ReCallContract(QueryReCallContractReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            List<string> statuList = new List<string>() { "1", "2", "6", "7", "-1", "11" };
            var quotationObj = await UnitWork.Find<ContractApply>(q => q.Id == req.ContractId).FirstOrDefaultAsync();
            if (statuList.Contains(quotationObj.ContractStatus))
            {
                throw new Exception("该合同申请单状态不可撤销。");
            }

            //记录撤回历史操作记录
            ContractOperationHistory contractHis = new ContractOperationHistory();
            var seleoh = await UnitWork.Find<ContractOperationHistory>(r => r.ContractApplyId.Equals(req.ContractId)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
            contractHis.Action = "撤回";
            contractHis.ApprovalResult = "撤回合同申请单";
            contractHis.CreateUser = loginContext.User.Name;
            contractHis.CreateUserId = loginContext.User.Id;
            contractHis.CreateTime = DateTime.Now;
            contractHis.ContractApplyId = req.ContractId;
            contractHis.Remark = req.Remarks;
            contractHis.ApprovalStage = "11";
            contractHis.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds);
            await UnitWork.AddAsync<ContractOperationHistory>(contractHis);

            //调用撤回审批流方法
            if (!string.IsNullOrWhiteSpace(quotationObj.FlowInstanceId))
            {
                await _flowInstanceApp.ReCall(new RecallFlowInstanceReq { FlowInstanceId = quotationObj.FlowInstanceId });
            }

            //更改申请单合同申请单状态为撤回
            await UnitWork.UpdateAsync<ContractApply>(q => q.Id == req.ContractId, q => new ContractApply
            {
                ContractStatus = "11",
                FlowInstanceId = ""
            });

            await UnitWork.SaveAsync();

            //singlR推送消息
            await SendSinglRMsg(quotationObj.ContractStatus, quotationObj.ContractNo, req.Remarks, loginContext.User);

            //撤回申请单钉钉通知
            await SendDDReCallMsg(quotationObj.ContractStatus, quotationObj.ContractNo, req.Remarks, loginContext.User);
        }

        /// <summary>
        /// 钉钉通知撤回消息
        /// </summary>
        /// <param name="contractStatus">合同单状态</param>
        /// <param name="contractNo">合同单据号</param>
        /// <param name="recallRemark">撤回备注</param>
        /// <param name="user">业务员</param>
        /// <returns></returns>
        public async Task SendDDReCallMsg(string contractStatus, string contractNo, string recallRemark, User user)
        {
            //添加当前合同申请单状态对应的审批角色
            List<string> roles = new List<string>();
            switch (contractStatus)
            {
                case "3":
                    roles.Add("法务人员");
                    break;
                case "4":
                    roles.Add("法务人员");
                    roles.Add("售前工程师");
                    break;
                case "5":
                    roles.Add("销售总助");
                    break;
                case "8":
                    roles.Add("售前工程师");
                    break;
                case "9":
                    roles.Add("法务人员");
                    break;
                case "10":
                    roles.Add("法务人员");
                    roles.Add("销售总助");
                    break;
                case "12":
                    roles.Add("售前工程师");
                    break;
            }

            if (roles.Count() > 0)
            {
                //查询角色Id
                List<string> roleMsgs = await UnitWork.Find<Role>(r => roles.Contains(r.Name)).Select(r => r.Id).ToListAsync();

                //查询角色和用户的对应关系
                List<string> relevances = await UnitWork.Find<Relevance>(r => roleMsgs.Contains(r.SecondId)).Select(r => r.FirstId).ToListAsync();

                //查询用户绑定的钉钉用户Id
                List<string> ddBindUsers = await UnitWork.Find<DDBindUser>(r => relevances.Contains(r.UserId)).Select(r => r.DDUserId).ToListAsync();

                //绑定钉钉用户不为空时，通过钉钉推送消息
                if (ddBindUsers != null && ddBindUsers.Count() > 0)
                {
                    string userIds = string.Join(",", ddBindUsers);
                    string remarks = "合同单号为" + contractNo + "申请单已被业务员-" + _userDepartMsgHelp.GetUserOrgName(user.Id) + user.Name + "-撤回，撤回理由：" + recallRemark;
                    await _dDVoice.DDSendMsg("text", remarks, userIds);
                }
                else
                {
                    //钉钉推送操作历史记录
                    await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                    {
                        MsgType = "钉钉推送撤回合同申请单消息",
                        MsgContent = contractNo + "推送失败,不存在钉钉绑定用户",
                        MsgResult = "失败",
                        CreateName = user.Name,
                        CreateUserId = user.Id,
                        CreateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                }
            }
        }

        /// <summary>
        /// singlR推送消息
        /// </summary>
        /// <param name="contractStatus">合同申请单状态</param>
        /// <param name="contractNo">合同申请单号</param>
        /// <param name="remarks">撤回备注</param>
        /// <param name="user">业务员</param>
        /// <returns></returns>
        public async Task SendSinglRMsg(string contractStatus, string contractNo, string remarks, User user)
        {
            //添加当前合同申请单状态对应的审批角色
            List<string> roles = new List<string>();
            switch (contractStatus)
            {
                case "3":
                    roles.Add("法务人员");
                    break;
                case "4":
                    roles.Add("法务人员");
                    roles.Add("售前工程师");
                    break;
                case "5":
                    roles.Add("销售总助");
                    break;
                case "8":
                    roles.Add("售前工程师");
                    break;
                case "9":
                    roles.Add("法务人员");
                    break;
                case "10":
                    roles.Add("法务人员");
                    roles.Add("销售总助");
                    break;
                case "12":
                    roles.Add("售前工程师");
                    break;
            }

            //当角色不为空时，singlR推送撤回原因消息
            if (roles.Count() > 0)
            {
                foreach (string item in roles)
                {
                    await _hubContext.Clients.Groups(item).SendAsync("ContractMessage", _userDepartMsgHelp.GetUserOrgName(user.Id) + user.Name, $"单号为" + contractNo + "申请单已撤回，撤回理由：" + remarks);
                }

                //SinglR推送操作历史记录
                await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                {
                    MsgType = "SinglR推送消息",
                    MsgContent = contractNo + "推送成功",
                    MsgResult = "成功",
                    CreateName = user.Name,
                    CreateUserId = user.Id,
                    CreateTime = DateTime.Now
                });
            }
            else
            {
                //SinglR推送操作历史记录
                await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                {
                    MsgType = "SinglR推送消息",
                    MsgContent = contractNo + "推送失败",
                    MsgResult = "失败",
                    CreateName = user.Name,
                    CreateUserId = user.Id,
                    CreateTime = DateTime.Now
                });
            }

            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 新增合同申请单
        /// </summary>
        /// <param name="obj">合同申请单实体数据</param>
        /// <returns>成功返回操作成功/returns>
        public async Task<string> Add(ContractApply obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            if (string.IsNullOrEmpty(obj.CompanyType) || string.IsNullOrEmpty(obj.ContractType))
            {
                throw new Exception("所属公司或文件类型不能为空");
            }

            if (obj.ContractFileTypeList == null || obj.ContractFileTypeList.Count() == 0)
            {
                throw new Exception("合同文件类型不能为空");
            }

            if (obj.ContractType == "3" && (string.IsNullOrEmpty(obj.CustomerCode) || string.IsNullOrEmpty(obj.SaleNo)))
            {
                throw new Exception("工程设计类申请，客户代码及销售订单号必填");
            }

            if (!string.IsNullOrEmpty(obj.CustomerCode))
            {
                var ocrds = await UnitWork.Find<OCRD>(r => r.CardCode == obj.CustomerCode).ToListAsync();
                if (ocrds == null && ocrds.Count() == 0)
                {
                    throw new Exception("当前客户无效");
                }
            }

            obj.ContractFileTypeList.ForEach(r => r.ContractApplyId = r.ContractApplyId);
            obj.CustomerName = string.IsNullOrEmpty(obj.CustomerCode) ? "" : (await UnitWork.Find<OCRD>(r => r.CardCode == obj.CustomerCode).FirstOrDefaultAsync()).CardName;
            var dbContext = UnitWork.GetDbContext<ContractApply>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    obj.Id = Guid.NewGuid().ToString();
                    obj.UpdateTime = null;
                    obj.DownloadNumber = 0;
                    obj.ContractStatus = "1";
                    obj.IsUploadOriginal = false;
                    obj.CreateTime = DateTime.Now;
                    obj.CreateId = loginUser.Id;
                    obj.CreateName = loginUser.Name;

                    //合同申请单单号设置
                    var maxContractNo = UnitWork.Find<ContractApply>(null).OrderByDescending(r => r.ContractNo).Select(r => r.ContractNo).FirstOrDefault();
                    if (!string.IsNullOrEmpty(maxContractNo) && maxContractNo.Split('-')[1].ToString() == DateTime.Now.ToString("yyyyMMdd"))
                    {
                        maxContractNo = "XW-" + DateTime.Now.ToString("yyyyMMdd") + "-" + (Convert.ToInt32(maxContractNo.Split('-')[2]) + 1).ToString().PadLeft(3, '0');
                    }
                    else
                    {
                        maxContractNo = "XW-" + DateTime.Now.ToString("yyyyMMdd") + "-001";
                    }

                    obj.ContractNo = maxContractNo;

                    //保存文件
                    if (obj.ContractFileTypeList.Count() != 0)
                    {
                        List<ContractFileType> contractFileTypeList = new List<ContractFileType>();
                        foreach (ContractFileType item in obj.ContractFileTypeList)
                        {
                            if (string.IsNullOrEmpty(item.FileType) || string.IsNullOrEmpty(item.ContractSealId))
                            {
                                throw new Exception("文件类型或印章Id不能为空");
                            }

                            if (item.FileType == "3" && string.IsNullOrEmpty(obj.ItemName))
                            {
                                throw new Exception("文件类型包含标书，项目名称必填");
                            }

                            if (item.FileType == "8" && string.IsNullOrEmpty(obj.Remark))
                            {
                                throw new Exception("文件类型包含采购合同时，备注不能为空");
                            }

                            item.Id = Guid.NewGuid().ToString();
                            item.ContractApplyId = obj.Id;
                            contractFileTypeList.Add(item);
                            List<ContractFile> contractFileList = new List<ContractFile>();
                            foreach (ContractFile contractFile in item.contractFileList)
                            {
                                contractFile.Id = Guid.NewGuid().ToString();
                                contractFile.ContractFileTypeId = item.Id;
                                contractFile.IsSeal = false;
                                contractFile.CreateUploadId = loginUser.Id;
                                contractFile.CreateUploadName = loginUser.Name;
                                contractFile.CreateUploadTime = DateTime.Now;
                                contractFileList.Add(contractFile);
                            }

                            if (contractFileList != null && contractFileList.Count > 0)
                            {
                                await UnitWork.BatchAddAsync<ContractFile>(contractFileList.ToArray());
                            }
                            else
                            {
                                throw new Exception("合同文件不能为空");
                            }
                        }

                        if (contractFileTypeList != null && contractFileTypeList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<ContractFileType>(contractFileTypeList.ToArray());
                        }
                        else
                        {
                            throw new Exception("合同文件类型不能为空");
                        }
                    }

                    //清空旧数据
                    obj.ContractFileTypeList.Clear();

                    //判断是否存为草稿
                    if (!obj.IsDraft)
                    {
                        //当合同类型为商务合同，并且使用了公司合同模板，提交直接进入待盖章，不走审批流
                        if (obj.ContractType == "1" && obj.IsUseCompanyTemplate)
                        {
                            obj.ContractStatus = "6";
                            obj = await UnitWork.AddAsync<ContractApply, int>(obj);
                            await UnitWork.AddAsync<ContractOperationHistory>(new ContractOperationHistory
                            {
                                Action = "商务合同使用了公司合同模板，直接进入待盖章",
                                CreateUser = loginUser.Name,
                                CreateUserId = loginUser.Id,
                                CreateTime = DateTime.Now,
                                ApprovalStage = "6",
                                ContractApplyId = obj.Id
                            });

                            await UnitWork.SaveAsync();
                        }
                        else
                        {
                            //创建合同管理审批流程
                            var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("订单合同申请"));
                            var afir = new AddFlowInstanceReq();
                            afir.SchemeId = mf.FlowSchemeId;
                            afir.FrmType = 2;
                            afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                            afir.CustomName = $"合同管理审批流程" + DateTime.Now;
                            afir.FrmData = "{\"ContractApplyId\":\"" + obj.Id + "\",\"IsContractType\":\"" + obj.ContractType + "\"}";
                            obj.FlowInstanceId = _flowInstanceApp.CreateInstanceAndGetIdAsync(afir).ConfigureAwait(false).GetAwaiter().GetResult();
                            obj.ContractStatus = ContractMethodHelp.GetConfigTypeStatus(obj.ContractType);
                            obj = await UnitWork.AddAsync<ContractApply, int>(obj);
                            await UnitWork.AddAsync<ContractOperationHistory>(new ContractOperationHistory
                            {
                                Action = "提交合同申请单",
                                ApprovalStage = obj.ContractStatus,
                                CreateUser = loginUser.Name,
                                CreateUserId = loginUser.Id,
                                CreateTime = DateTime.Now,
                                ContractApplyId = obj.Id
                            });

                            await UnitWork.SaveAsync();
                        }
                    }
                    else
                    {
                        //添加合同申请单草稿
                        obj = await UnitWork.AddAsync<ContractApply, int>(obj);
                        await UnitWork.AddAsync<ContractOperationHistory>(new ContractOperationHistory
                        {
                            Action = "创建草稿",
                            CreateUser = loginUser.Name,
                            CreateUserId = loginUser.Id,
                            CreateTime = DateTime.Now,
                            ApprovalStage = "1",
                            ContractApplyId = obj.Id
                        });

                        await UnitWork.SaveAsync();
                    }

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message.ToString());
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 修改合同申请单
        /// </summary>
        /// <param name="obj">合同申请单实体数据</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> UpDate(ContractApply obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            if (string.IsNullOrEmpty(obj.CompanyType) || string.IsNullOrEmpty(obj.ContractType))
            {
                throw new Exception("所属公司或文件类型不能为空");
            }

            if (obj.ContractFileTypeList == null || obj.ContractFileTypeList.Count() == 0)
            {
                throw new Exception("合同文件不能为空");
            }

            var dbContext = UnitWork.GetDbContext<ContractApply>();
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    #region 删除
                    var contractFileType = await UnitWork.Find<ContractFileType>(r => r.ContractApplyId == obj.Id).ToListAsync();
                    foreach (ContractFileType item in contractFileType)
                    {
                        await UnitWork.DeleteAsync<ContractFile>(r => r.ContractFileTypeId == item.Id);
                    }

                    await UnitWork.DeleteAsync<ContractFileType>(r => r.ContractApplyId == obj.Id);
                    await UnitWork.SaveAsync();
                    #endregion

                    #region 新增  
                    List<ContractFileType> contractFileTypeList = new List<ContractFileType>();
                    foreach (ContractFileType item in obj.ContractFileTypeList)
                    {
                        if (string.IsNullOrEmpty(item.FileType) || string.IsNullOrEmpty(item.ContractSealId))
                        {
                            throw new Exception("文件类型或印章Id不能为空");
                        }

                        if (item.FileType == "3" && string.IsNullOrEmpty(obj.ItemName))
                        {
                            throw new Exception("文件类型包含标书，项目编号和项目名称必填");
                        }

                        if (item.FileType == "8" && string.IsNullOrEmpty(obj.Remark))
                        {
                            throw new Exception("文件类型包含采购合同时，备注不能为空");
                        }

                        item.Id = Guid.NewGuid().ToString();
                        item.ContractApplyId = obj.Id;
                        List<ContractFile> contractFileList = new List<ContractFile>();
                        foreach (ContractFile contractFile in item.contractFileList)
                        {
                            ContractFile file = new ContractFile();
                            file.ContractFileTypeId = item.Id;
                            file.IsSeal = contractFile.IsSeal;
                            file.FileId = contractFile.FileId;
                            file.IsFinalContract = contractFile.IsFinalContract;
                            file.CreateUploadId = contractFile.CreateUploadId == "" ? loginUser.Id : contractFile.CreateUploadId;
                            file.CreateUploadName = contractFile.CreateUploadName;
                            file.CreateUploadTime = contractFile.CreateUploadTime == null ? DateTime.Now : contractFile.CreateUploadTime;
                            file.UpdateUserId = loginUser.Id;
                            file.UpdateUserName = loginUser.Name;
                            file.UpdateUserTime = DateTime.Now;
                            contractFileList.Add(file);
                        }

                        if (contractFileList != null && contractFileList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<ContractFile>(contractFileList.ToArray());
                        }
                        else
                        {
                            throw new Exception("合同文件不能为空");
                        }

                        item.contractFileList = contractFileList;
                        contractFileTypeList.Add(item);
                    }

                    if (contractFileTypeList != null && contractFileTypeList.Count > 0)
                    {
                        await UnitWork.BatchAddAsync<ContractFileType>(contractFileTypeList.ToArray());
                    }
                    else
                    {
                        throw new Exception("合同文件类型不能为空");
                    }

                    //清空旧数据
                    obj.ContractFileTypeList.Clear();
                    await UnitWork.SaveAsync();
                    #endregion

                    if (obj.IsDraft)
                    {
                        await UnitWork.UpdateAsync<ContractApply>(u => u.Id == obj.Id, u => new ContractApply
                        {
                            UpdateTime = DateTime.Now,
                            ContractNo = obj.ContractNo,
                            CustomerCode = obj.CustomerCode,
                            CustomerName = obj.CustomerName,
                            CompanyType = obj.CompanyType,
                            ContractType = obj.ContractType,
                            QuotationNo = obj.QuotationNo,
                            U_SAP_ID = obj.U_SAP_ID,
                            ItemNo = obj.ItemNo,
                            ItemName = obj.ItemName,
                            SaleNo = obj.SaleNo,
                            IsDraft = obj.IsDraft,
                            IsUploadOriginal = obj.IsUploadOriginal,
                            IsUseCompanyTemplate = obj.IsUseCompanyTemplate,
                            FlowInstanceId = obj.FlowInstanceId,
                            DownloadNumber = obj.DownloadNumber,
                            ContractStatus = "1",
                            Remark = obj.Remark,
                            CreateId = obj.CreateId,
                            CreateName = obj.CreateName,
                            CreateTime = obj.CreateTime
                        });

                        await UnitWork.SaveAsync();
                    }
                    else
                    {
                        //当合同类型为商务合同，并且使用了公司合同模板，提交直接进入待盖章，不走审批流
                        if (obj.ContractType == "1" && obj.IsUseCompanyTemplate)
                        {
                            //修改合同申请单
                            await UnitWork.UpdateAsync<ContractApply>(r => r.Id == obj.Id, r => new ContractApply
                            {
                                UpdateTime = DateTime.Now,
                                ContractNo = obj.ContractNo,
                                CustomerCode = obj.CustomerCode,
                                CustomerName = obj.CustomerName,
                                CompanyType = obj.CompanyType,
                                ContractType = obj.ContractType,
                                QuotationNo = obj.QuotationNo,
                                U_SAP_ID = obj.U_SAP_ID,
                                ItemName = obj.ItemName,
                                ItemNo = obj.ItemNo,
                                SaleNo = obj.SaleNo,
                                IsDraft = obj.IsDraft,
                                IsUploadOriginal = obj.IsUploadOriginal,
                                IsUseCompanyTemplate = obj.IsUseCompanyTemplate,
                                FlowInstanceId = obj.FlowInstanceId,
                                DownloadNumber = obj.DownloadNumber,
                                ContractStatus = "6",
                                Remark = obj.Remark,
                                CreateId = obj.CreateId,
                                CreateName = obj.CreateName,
                                CreateTime = obj.CreateTime
                            });

                            await UnitWork.AddAsync<ContractOperationHistory>(new ContractOperationHistory
                            {
                                Action = "商务合同使用了公司合同模板，直接进入待盖章",
                                CreateUser = loginUser.Name,
                                CreateUserId = loginUser.Id,
                                CreateTime = DateTime.Now,
                                ApprovalStage = "6",
                                ContractApplyId = obj.Id
                            });

                            await UnitWork.SaveAsync();
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(obj.FlowInstanceId))
                            {
                                //添加流程
                                var mf = _moduleFlowSchemeApp.Get(m => m.Module.Name.Equals("订单合同申请"));
                                var afir = new AddFlowInstanceReq();
                                afir.SchemeId = mf.FlowSchemeId;
                                afir.FrmType = 2;
                                afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                                afir.CustomName = $"合同管理审批流程" + DateTime.Now;
                                afir.FrmData = "{\"ContractApplyId\":\"" + obj.Id + "\",\"IsContractType\":\"" + obj.ContractType + "\"}";
                                obj.FlowInstanceId = _flowInstanceApp.CreateInstanceAndGetIdAsync(afir).ConfigureAwait(false).GetAwaiter().GetResult();
                            }
                            else
                            {
                                _flowInstanceApp.Start(new StartFlowInstanceReq { FlowInstanceId = obj.FlowInstanceId }).ConfigureAwait(false).GetAwaiter().GetResult();
                            }

                            //修改合同申请单
                            await UnitWork.UpdateAsync<ContractApply>(r => r.Id == obj.Id, r => new ContractApply
                            {
                                UpdateTime = DateTime.Now,
                                ContractNo = obj.ContractNo,
                                CustomerCode = obj.CustomerCode,
                                CustomerName = obj.CustomerName,
                                CompanyType = obj.CompanyType,
                                ContractType = obj.ContractType,
                                QuotationNo = obj.QuotationNo,
                                U_SAP_ID = obj.U_SAP_ID,
                                ItemNo = obj.ItemNo,
                                ItemName = obj.ItemName,
                                SaleNo = obj.SaleNo,
                                IsDraft = obj.IsDraft,
                                IsUploadOriginal = obj.IsUploadOriginal,
                                IsUseCompanyTemplate = obj.IsUseCompanyTemplate,
                                FlowInstanceId = obj.FlowInstanceId,
                                DownloadNumber = obj.DownloadNumber,
                                ContractStatus = ContractMethodHelp.GetConfigTypeStatus(obj.ContractType),
                                Remark = obj.Remark,
                                CreateId = obj.CreateId,
                                CreateName = obj.CreateName,
                                CreateTime = obj.CreateTime
                            });

                            await UnitWork.AddAsync<ContractOperationHistory>(new ContractOperationHistory
                            {
                                Action = "提交合同申请单",
                                CreateUser = loginUser.Name,
                                CreateUserId = loginUser.Id,
                                CreateTime = DateTime.Now,
                                ApprovalStage = ContractMethodHelp.GetConfigTypeStatus(obj.ContractType),
                                ContractApplyId = obj.Id
                            });
                        }

                        await UnitWork.SaveAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message.ToString());
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 删除合同申请单
        /// </summary>
        /// <param name="contractId">合同申请单Id</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> Delete(string contractId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            StringBuilder remark = new StringBuilder();
            var contractApply = await UnitWork.Find<ContractApply>(r => r.Id == contractId).Where(r => r.ContractStatus == "1").Include(r => r.ContractFileTypeList).ToListAsync();
            if (contractApply != null && contractApply.Count() > 0)
            {
                var delfiles = await UnitWork.Find<ContractFileType>(f => f.ContractApplyId == contractId).Include(f => f.contractFileList).ToListAsync();
                if (delfiles.Count() > 0)
                {
                    foreach (ContractFileType item in delfiles)
                    {
                        if (item.contractFileList.Count() > 0)
                        {
                            await UnitWork.BatchDeleteAsync<ContractFile>(item.contractFileList.ToArray());
                        }
                    }

                    await UnitWork.BatchDeleteAsync<ContractFileType>(delfiles.ToArray());
                }

                await UnitWork.DeleteAsync<ContractApply>(q => q.Id == contractId);
                remark.Append("删除单号为：" + contractApply.FirstOrDefault().ContractNo + " 的合同申请单！");
                await UnitWork.AddAsync<ReimurseOperationHistory>(new ReimurseOperationHistory
                {
                    Action = "删除合同申请单",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    Remark = remark.ToString()
                });

                await UnitWork.SaveAsync();
                return "操作成功";
            }
            else
            {
                throw new Exception("删除失败，只能删除待提交状态的申请单,请重试。");
            }
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        /// <param name="contractIdList">合同申请单Id集合</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> Deletes(List<string> contractIdList)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            StringBuilder remark = new StringBuilder();
            try
            {
                var contractApply = await UnitWork.Find<ContractApply>(r => contractIdList.Contains(r.Id)).Where(r => r.ContractStatus == "1").Include(r => r.ContractFileTypeList).ToListAsync();
                if (contractApply != null && contractApply.Count() > 0)
                {
                    foreach (ContractApply item in contractApply)
                    {
                        var delfiles = await UnitWork.Find<ContractFileType>(f => f.ContractApplyId.Equals(item.Id)).Include(f => f.contractFileList).ToListAsync();
                        if (delfiles != null && delfiles.Count() > 0)
                        {
                            foreach (ContractFileType file in delfiles)
                            {
                                if (file.contractFileList != null && file.contractFileList.Count() > 0)
                                {
                                    await UnitWork.BatchDeleteAsync<ContractFile>(file.contractFileList.ToArray());
                                }
                            }

                            await UnitWork.BatchDeleteAsync<ContractFileType>(delfiles.ToArray());
                        }

                        await UnitWork.DeleteAsync<ContractApply>(q => q.Id == item.Id);
                        remark.Append("批量删除合同申请单！");
                    }

                    await UnitWork.AddAsync<ReimurseOperationHistory>(new ReimurseOperationHistory
                    {
                        Action = "删除合同申请单",
                        CreateUser = loginUser.Name,
                        CreateUserId = loginUser.Id,
                        CreateTime = DateTime.Now,
                        Remark = remark.ToString()
                    });

                    await UnitWork.SaveAsync();
                    return "操作成功";
                }
                else
                {
                    throw new Exception("删除失败，只能删除待提交状态的申请单,请重试。");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message.ToString());
            }

        }

        /// <summary>
        /// 审批合同申请单 
        /// </summary>
        /// <param name="req">合同申请审批实体数据</param>
        /// <returns>成功返回200，失败抛出异常</returns>
        public async Task<TableData> Accraditation(AccraditationContractApplyReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            if (string.IsNullOrEmpty(req.contractApply.CompanyType) || string.IsNullOrEmpty(req.contractApply.ContractType))
            {
                result.Message = "合同申请单所属公司或合同类型不能为空。";
                Log.Logger.Error($"参数{req}");
                result.Code = 500;
                return result;
            }

            if (req.contractApply.ContractFileTypeList == null || req.contractApply.ContractFileTypeList.Count() == 0)
            {
                result.Message = "合同申请单合同文件类型不能为空。";
                result.Code = 500;
                Log.Logger.Error($"参数{req}");
                return result;
            }

            var obj = await UnitWork.Find<ContractApply>(r => r.Id == req.Id).Include(r => r.ContractFileTypeList).FirstOrDefaultAsync();
            if (!string.IsNullOrEmpty(obj.ContractStatus))
            {
                if (Convert.ToInt32(obj.ContractStatus) < 2)
                {
                    result.Message = "合同申请单未提交，不可操作。";
                    result.Code = 500;
                    Log.Logger.Error($"参数{req}");
                    return result;
                }

                if (obj.ContractStatus == "11")
                {
                    result.Message = "合同申请单已经被撤回，停止审批。";
                    result.Code = 500;
                    Log.Logger.Error($"参数{req}");
                    return result;
                }
            }
            else
            {
                result.Message = "合同申请单不存在，不可操作。";
                result.Code = 500;
                Log.Logger.Error($"参数{req}");
                return result;
            }

            //审批时判断文件类型
            if (!req.IsReject)
            {
                result = ApprovalNodeJudge(req.Id);
            }

            if (result.Code == 500)
            {
                return result;
            }

            try
            {
                ContractOperationHistory contractHis = new ContractOperationHistory();
                ContractSign contractSign = new ContractSign();
                obj.UpdateTime = DateTime.Now;
                if (loginContext.Roles.Any(r => r.Name.Equals("法务人员")) && obj.ContractStatus == "3")
                {
                    if (obj.ContractStatus == "5")
                    {
                        result.Message = "合同申请单已经由法务人员审批成功，无需再次审批。";
                        result.Code = 500;
                        return result;
                    }

                    contractHis.Action = "法务人员审批";
                    obj.ContractStatus = "5";
                }

                if (loginContext.Roles.Any(r => r.Name.Equals("售前工程师")) && obj.ContractStatus == "12")
                {
                    if (obj.ContractStatus == "5")
                    {
                        result.Message = "合同申请单已经由售前工程师审批审批成功，无需再次审批。";
                        result.Code = 500;
                        return result;
                    }

                    contractHis.Action = "售前工程师审批";
                    obj.ContractStatus = "5";
                }

                if (loginContext.Roles.Any(r => r.Name.Equals("法务人员")) && obj.ContractStatus == "10")
                {
                    if (obj.ContractStatus == "-1")
                    {
                        result.Message = "合同申请单已经由法务人员审批审批成功，无需再次审批。";
                        result.Code = 500;
                        return result;
                    }

                    contractHis.Action = "法务人员审批";
                    obj.ContractStatus = "-1";
                }

                if (loginContext.Roles.Any(r => r.Name.Equals("销售总助")) && obj.ContractStatus == "10")
                {
                    if (obj.ContractStatus == "-1")
                    {
                        result.Message = "合同申请单已经由销售总助审批审批成功，无需再次审批。";
                        result.Code = 500;
                        return result;
                    }

                    contractHis.Action = "总助审批";
                    obj.ContractStatus = "-1";
                }

                if (obj.ContractStatus == "4")
                {
                    if (loginContext.Roles.Any(r => r.Name.Equals("法务人员")))
                    {
                        if (obj.ContractStatus == "8")
                        {
                            result.Message = "合同申请单已经由法务人员审批成功，无需再次审批。";
                            result.Code = 500;
                            return result;
                        }

                        contractHis.Action = "会签-法务完成审批，待售前工程师审批";
                        obj.ContractStatus = "8";
                        contractSign.ContractApplyId = obj.Id;
                        contractSign.TogetherSignRole = "法务人员";
                        contractSign.ApprovalOrReject = req.IsReject ? 2 : 1;
                        contractSign.FlowInstanceId = obj.FlowInstanceId;
                        await SaveContractSign(contractSign);
                    }

                    if (loginContext.Roles.Any(r => r.Name.Equals("售前工程师")))
                    {
                        if (obj.ContractStatus == "9")
                        {
                            result.Message = "合同申请单已经由售前工程师审批成功，无需再次审批。";
                            result.Code = 500;
                            return result;
                        }

                        contractHis.Action = "会签-售前工程师完成审批，待法务审批";
                        obj.ContractStatus = "9";
                        contractSign.ContractApplyId = obj.Id;
                        contractSign.TogetherSignRole = "售前工程师";
                        contractSign.ApprovalOrReject = req.IsReject ? 2 : 1;
                        contractSign.FlowInstanceId = obj.FlowInstanceId;
                        await SaveContractSign(contractSign);
                    }
                }

                if (obj.ContractStatus == "8")
                {
                    if (loginContext.Roles.Any(r => r.Name.Equals("售前工程师")))
                    {
                        if (obj.ContractStatus == "9")
                        {
                            result.Message = "合同申请单已经由售前工程师审批成功，无需再次审批。";
                            result.Code = 500;
                            return result;
                        }

                        contractHis.Action = "会签-售前工程师完成审批，待法务审批";
                        obj.ContractStatus = "9";
                        contractSign.ContractApplyId = obj.Id;
                        contractSign.TogetherSignRole = "售前工程师";
                        contractSign.ApprovalOrReject = req.IsReject ? 2 : 1;
                        contractSign.FlowInstanceId = obj.FlowInstanceId;
                        await SaveContractSign(contractSign);
                    }
                }

                if (obj.ContractStatus == "9")
                {
                    if (loginContext.Roles.Any(r => r.Name.Equals("法务人员")))
                    {
                        if (obj.ContractStatus == "8")
                        {
                            result.Message = "合同申请单已经由法务人员审批成功，无需再次审批。";
                            result.Code = 500;
                            return result;
                        }

                        contractHis.Action = "会签-法务完成审批，待售前工程师审批";
                        obj.ContractStatus = "8";
                        contractSign.ContractApplyId = obj.Id;
                        contractSign.TogetherSignRole = "法务人员";
                        contractSign.ApprovalOrReject = req.IsReject ? 2 : 1;
                        contractSign.FlowInstanceId = obj.FlowInstanceId;
                        await SaveContractSign(contractSign);
                    }
                }

                //当法务人员和技术人员同时会签时才能将合同申请单传递给总助审批
                var objs = await UnitWork.Find<ContractSign>(r => r.ContractApplyId == obj.Id && r.ApprovalOrReject == 1 && r.FlowInstanceId == obj.FlowInstanceId).GroupBy(r => new { r.ContractApplyId, r.TogetherSignRole, r.ApprovalOrReject, r.FlowInstanceId }).Select(r => new { r.Key.ContractApplyId, r.Key.TogetherSignRole, r.Key.ApprovalOrReject, r.Key.FlowInstanceId }).ToListAsync();
                if (objs != null && objs.Count() >= 2)
                {
                    contractHis.Action = "会签-法务&售前工程师同时完成审批";
                    obj.ContractStatus = "5";
                }

                if (loginContext.Roles.Any(r => r.Name.Equals("销售总助")) && obj.ContractStatus == "5")
                {
                    if (obj.ContractStatus == "6")
                    {
                        result.Message = "合同申请单已经由销售总助审批成功，无需再次审批。";
                        result.Code = 500;
                        return result;
                    }

                    contractHis.Action = "总助审批";
                    obj.ContractStatus = "6";
                }

                VerificationReq VerificationReqModle = new VerificationReq
                {
                    NodeRejectStep = "",
                    NodeRejectType = "0",
                    FlowInstanceId = obj.FlowInstanceId,
                    VerificationFinally = "1",
                    VerificationOpinion = "同意",
                };

                if (req.IsReject)
                {
                    if (obj.ContractStatus == "2")
                    {
                        result.Message = "合同申请单已经驳回。";
                        result.Code = 500;
                        return result;
                    }

                    VerificationReqModle.VerificationFinally = "3";
                    VerificationReqModle.VerificationOpinion = req.Remark;
                    VerificationReqModle.NodeRejectType = "1";
                    contractHis.ApprovalResult = "驳回";
                    req.contractApply.FlowInstanceId = "";

                    //节点权限验证
                    await _flowInstanceApp.Verification(VerificationReqModle);
                    obj.ContractStatus = "2";
                }
                else
                {
                    if (obj.ContractStatus == "6")
                    {
                        req.contractApply.FlowInstanceId = "";
                        contractHis.Action = "已结束";
                        contractHis.ApprovalResult = "审批结束，待盖章";
                        List<string> ids = new List<string>();
                        ids.Add(obj.FlowInstanceId);
                        await _flowInstanceApp.DeleteAsync(ids.ToArray());
                    }
                    else
                    {
                        contractHis.ApprovalResult = "同意";
                        await _flowInstanceApp.Verification(VerificationReqModle);
                    }
                }

                #region 删除
                var contractFileType = await UnitWork.Find<ContractFileType>(r => r.ContractApplyId == obj.Id).ToListAsync();
                foreach (ContractFileType item in contractFileType)
                {
                    await UnitWork.DeleteAsync<ContractFile>(r => r.ContractFileTypeId == item.Id);
                }

                await UnitWork.DeleteAsync<ContractFileType>(r => r.ContractApplyId == obj.Id);
                await UnitWork.SaveAsync();
                #endregion

                #region 新增  
                List<ContractFileType> contractFileTypeList = new List<ContractFileType>();
                foreach (ContractFileType item in req.contractApply.ContractFileTypeList)
                {
                    if (item.FileType == "" || item.ContractSealId == "")
                    {
                        throw new Exception("文件类型或印章Id不能为空");
                    }

                    item.Id = Guid.NewGuid().ToString();
                    item.ContractApplyId = obj.Id;
                    List<ContractFile> contractFileList = new List<ContractFile>();
                    foreach (ContractFile contractFile in item.contractFileList)
                    {
                        ContractFile file = new ContractFile();
                        file.ContractFileTypeId = item.Id;
                        file.IsSeal = contractFile.IsSeal;
                        file.FileId = contractFile.FileId;
                        file.IsFinalContract = contractFile.IsFinalContract;
                        file.CreateUploadId = contractFile.CreateUploadId == "" ? loginContext.User.Id : contractFile.CreateUploadId;
                        file.CreateUploadName = contractFile.CreateUploadName;
                        file.CreateUploadTime = contractFile.CreateUploadTime == null ? DateTime.Now : contractFile.CreateUploadTime;
                        file.UpdateUserId = loginContext.User.Id;
                        file.UpdateUserName = loginContext.User.Name;
                        file.UpdateUserTime = DateTime.Now;
                        contractFileList.Add(file);
                    }

                    if (contractFileList != null && contractFileList.Count > 0)
                    {
                        await UnitWork.BatchAddAsync<ContractFile>(contractFileList.ToArray());
                    }

                    item.contractFileList = contractFileList;
                    contractFileTypeList.Add(item);
                }

                if (contractFileTypeList != null && contractFileTypeList.Count > 0)
                {
                    await UnitWork.BatchAddAsync<ContractFileType>(contractFileTypeList.ToArray());
                }

                //清空旧数据
                req.contractApply.ContractFileTypeList.Clear();
                await UnitWork.SaveAsync();
                #endregion

                req.contractApply.ContractStatus = obj.ContractStatus;
                req.contractApply.UpdateTime = DateTime.Now;
                req.FlowInstanceId = obj.FlowInstanceId;
                await UnitWork.UpdateAsync<ContractApply>(req.contractApply);

                //操作历史记录
                var seleoh = await UnitWork.Find<ContractOperationHistory>(r => r.ContractApplyId.Equals(obj.Id)).OrderByDescending(r => r.CreateTime).FirstOrDefaultAsync();
                contractHis.CreateUser = loginContext.User.Name;
                contractHis.CreateUserId = loginContext.User.Id;
                contractHis.CreateTime = DateTime.Now;
                contractHis.ContractApplyId = obj.Id;
                contractHis.Remark = req.Remark;
                contractHis.ApprovalStage = obj.ContractStatus;
                contractHis.IntervalTime = Convert.ToInt32((DateTime.Now - Convert.ToDateTime(seleoh.CreateTime)).TotalSeconds);

                await UnitWork.AddAsync<ContractOperationHistory>(contractHis);
                await UnitWork.SaveAsync();

                result.Message = "审核通过";
                result.Code = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.Message.ToString();
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 添加会签人员角色
        /// </summary>
        /// <param name="req">会签实体数据</param>
        /// <returns>成功返回为空，失败抛出异常</returns>
        public async Task SaveContractSign(ContractSign req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var dbContext = UnitWork.GetDbContext<ContractSign>();
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    await UnitWork.AddAsync<ContractSign>(new ContractSign
                    {
                        ContractApplyId = req.ContractApplyId,
                        TogetherSignRole = req.TogetherSignRole,
                        ApprovalOrReject = req.ApprovalOrReject,
                        FlowInstanceId = req.FlowInstanceId,
                        CreateUserId = loginContext.User.Id,
                        CreateName = loginContext.User.Name,
                        CreateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("保存会签人员失败。");
                }
            }
        }

        /// <summary>
        /// 获取销售订单信息
        /// </summary>
        /// <param name="rowCount">行总计</param>
        /// <param name="model">模型</param>
        /// <param name="type">类型</param>
        /// <param name="ViewFull">视图</param>
        /// <param name="ViewSelf">视图</param>
        /// <param name="UserID">UserID</param>
        /// <param name="SboID">SboID</param>
        /// <param name="ViewSelfDepartment">视图</param>
        /// <param name="DepID">DepID</param>
        /// <param name="ViewCustom">视图</param>
        /// <param name="ViewSales">视图</param>
        /// <param name="sqlcont">sql连接</param>
        /// <param name="sboname">sbo名称</param>
        /// <returns>成功返回数据信息，失败抛出异常信息</returns>
        public DataTable SelectBillListInfo_ORDR(List<Role> roles, out int rowCount, SalesOrderListReq model, string type, bool ViewFull, bool ViewSelf, int UserID, int SboID, bool ViewSelfDepartment, int DepID, bool ViewCustom, bool ViewSales, string sqlcont, string sboname)
        {
            bool IsSql = true;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty; int uSboId = SboID;
            if (!string.IsNullOrEmpty(model.sortname) && !string.IsNullOrEmpty(model.sortorder))
                sortString = string.Format("{0} {1}", model.sortname, model.sortorder.ToUpper());

            string dRowData = string.Empty;
            DataTable rDataRowsSlp = _servcieSaleOrderApp.GetSboSlpCodeId(UserID, SboID);
            if (!roles.Any(r => r.Name.Equals("管理员")))
            {
                filterString += string.Format(" a.SlpCode = '{0}' AND ", rDataRowsSlp.Rows[0][0].ToString());
            }

            #region 搜索条件
            if (!string.IsNullOrEmpty(model.DocEntry))
            {
                filterString += string.Format("a.DocEntry LIKE '{0}' AND ", model.DocEntry.FilterSQL().Trim());

            }

            if (!string.IsNullOrEmpty(model.CardCode))
            {
                filterString += string.Format("(a.CardCode LIKE '%{0}%' OR a.CardName LIKE '%{0}%') AND ", model.CardCode.FilterWildCard());

            }

            if (!string.IsNullOrEmpty(model.DocStatus))
            {
                if (model.DocStatus == "ON") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') AND "); }
                if (model.DocStatus == "OY") { filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') AND "); }
                if (model.DocStatus == "CY") { filterString += string.Format(" a.CANCELED = 'Y' AND "); }
                if (model.DocStatus == "CN") { filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N') AND "); }
                if (model.DocStatus == "NC") { filterString += string.Format(" a.CANCELED = 'N' AND "); }
            }

            if (!string.IsNullOrEmpty(model.Comments))
            {
                filterString += string.Format("a.Comments LIKE '%{0}%' AND ", model.Comments.FilterWildCard());
            }

            if (!string.IsNullOrEmpty(model.SlpName))
            {
                filterString += string.Format("c.SlpName LIKE '%{0}%' AND ", model.SlpName.FilterSQL().Trim());
            }

            if (!string.IsNullOrEmpty(model.ToCompany))
            {
                filterString += string.Format("a.Indicator = '{0}' AND ", model.ToCompany);
            }

            if (!string.IsNullOrEmpty(model.ReceiptStatus))
            {
                if (model.ReceiptStatus == "Y")
                {
                    filterString += string.Format("a.U_DocRCTAmount>0.00 AND ");
                }
                else if (model.ReceiptStatus == "W")
                {
                    filterString += string.Format("a.U_DocRCTAmount =0.00 AND ");
                }
            }
            #endregion

            #region 判断权限
            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows = _servcieSaleOrderApp.GetSboSlpCodeIds(DepID, SboID);
                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }

                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);

                    filterString += string.Format(") OR d.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }

                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(")) AND ");
                }
            }
            else if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string slpTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    filterString += string.Format(" (a.SlpCode = {0}) AND ", slpCode);// OR d.DfTcnician={1}   , slpTcnician  不允许售后查看业务员的单
                }
                else
                {
                    filterString = string.Format(" (a.SlpCode = {0}) AND ", 0);
                }
            }
            #endregion

            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);

            if (IsSql)
            {
                DataTable thistab = _servcieSaleOrderApp.SelectBillListInfo_ORDRNew(out rowCount, model.limit, model.page, filterString, sortString, line, ViewCustom, ViewSales, sqlcont, sboname, SboID);
                return thistab;
            }
            else
            {
                return SelectBillViewInfo(out rowCount, model.limit, model.page, model.query, model.sortname, model.sortorder, type, ViewFull, ViewSelf, UserID, uSboId, ViewSelfDepartment, DepID, ViewCustom, ViewSales);
            }
        }

        /// <summary>
        /// 查看视图信息
        /// </summary>
        /// <param name="rowCount">行总计</param>
        /// <param name="pageSize">页面大小</param>
        /// <param name="pageIndex">位置</param>
        /// <param name="filterQuery">过滤查询</param>
        /// <param name="sortname">排序字段</param>
        /// <param name="sortorder">正序/逆序</param>
        /// <param name="type">类型</param>
        /// <param name="ViewFull">视图</param>
        /// <param name="ViewSelf">视图</param>
        /// <param name="UserID">UserID</param>
        /// <param name="SboID">SboID</param>
        /// <param name="ViewSelfDepartment">视图</param>
        /// <param name="DepID">DepID</param>
        /// <param name="ViewCustom">视图</param>
        /// <param name="ViewSales">视图</param>
        /// <returns>成功返回数据信息，失败抛出异常信息</returns>
        public DataTable SelectBillViewInfo(out int rowCount, int pageSize, int pageIndex, string filterQuery, string sortname, string sortorder, string type, bool ViewFull, bool ViewSelf, int UserID, int SboID, bool ViewSelfDepartment, int DepID, bool ViewCustom, bool ViewSales)
        {
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            if (!string.IsNullOrEmpty(sortname) && !string.IsNullOrEmpty(sortorder))
                sortString = string.Format("{0} {1}", sortname, sortorder.ToUpper());

            #region 搜索条件
            if (!string.IsNullOrEmpty(filterQuery))
            {
                string[] fields = filterQuery.Split('`');
                string[] p = fields[0].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.sbo_id = {0} AND ", p[1]);
                }
                p = fields[1].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("{0} LIKE '{1}' AND ", p[0], p[1].FilterSQL().Trim());
                }
                p = fields[2].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("({0} LIKE '%{1}%' OR a.CardName LIKE '%{1}%') AND ", p[0], p[1].FilterWildCard());
                }
                p = fields[3].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    if (p[1] == "ON")
                    {
                        filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') AND ");
                    }

                    if (p[1] == "OY")
                    {
                        filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') AND ");
                    }
                }
                else
                {
                    filterString += string.Format(" (a.DocStatus = 'O' AND (a.Printed = 'N' OR a.Printed = 'Y') AND a.CANCELED = 'N') AND ");
                }

                p = fields[4].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("a.Comments LIKE '%{0}%' AND ", p[1].FilterWildCard());
                }

                p = fields[5].Split(':');
                if (!string.IsNullOrEmpty(p[1]))
                {
                    filterString += string.Format("{0} LIKE '%{1}%' AND ", p[0], p[1].FilterSQL().Trim());
                }

                if (type == "ODLN")
                {
                    p = fields[6].Split(':');
                    if (!string.IsNullOrEmpty(p[1]))
                    {
                        filterString += string.Format("a.GroupNum = {1} AND ", p[0], p[1]);
                    }

                    string[] p7 = fields[7].Split(':');
                    string[] p8 = fields[8].Split(':');
                    if (!string.IsNullOrEmpty(p8[1]))
                    {
                        filterString += " (a.CreateDate BETWEEN '" + p7[1].FilterSQL().Trim() + "' AND '" + p8[1].FilterSQL().Trim() + "') AND ";
                    }
                }
            }
            else
            {
                filterString += string.Format("a.sbo_id={0} AND ", SboID);
            }
            #endregion

            #region 根据不同的单据显示不同的内容
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "OQUT") { type = "sale_oqut"; line = "sale_qut1"; }//销售报价单
                else if (type == "ORDR") { type = "sale_ordr"; line = "sale_rdr1"; }//销售订单
                else if (type == "ODLN") { type = "sale_odln"; line = "sale_dln1"; }//销售交货单
                else if (type == "OINV") { type = "sale_oinv"; line = "sale_inv1"; }//应收发票
                else if (type == "ORDN") { type = "sale_ordn"; line = "sale_rdn1"; }//销售退货单
                else if (type == "ORIN") { type = "sale_orin"; line = "sale_rin1"; }//应收贷项凭证
                else if (type == "OPQT") { type = "buy_opqt"; line = "buy_pqt1"; }//采购报价单
                else if (type == "OPOR") { type = "buy_opor"; line = "buy_por1"; }//采购订单
                else if (type == "OPDN") { type = "buy_opdn"; line = "buy_pdn1"; }//采购收货单
                else if (type == "OPCH") { type = "buy_opch"; line = "buy_pch1"; }//应付发票
                else if (type == "ORPD") { type = "buy_orpd"; line = "buy_rpd1"; }//采购退货单
                else if (type == "ORPC") { type = "buy_orpc"; line = "buy_rpc1"; }//应付贷项凭证
                else { type = "sale_oqut"; line = "sale_qut1"; }
            }
            #endregion

            #region 判断权限
            string arr_roles = GetRolesName(UserID);
            if ((line.Contains("buy")) && ((!arr_roles.Contains("物流文员")) && (!arr_roles.Contains("系统管理员"))))//若不含有物流文员角色，则则屏蔽运输采购单
            {
                filterString += string.Format(" d.QryGroup1='N' AND ");
            }

            if (ViewSelfDepartment && !ViewFull)
            {
                DataTable rDataRows;
                DataTable rdepRows = new DataTable();
                if (line == "buy_por1" || line == "buy_pdn1" || line == "buy_pqt1" || line == "buy_pch1" || line == "buy_rpd1" || line == "buy_rpc1")
                {
                    rdepRows = _servcieSaleOrderApp.GetDep_map(DepID);
                }
                if (rdepRows.Rows.Count > 0)
                {
                    string dep_ids = string.Empty;
                    foreach (DataRow item in rdepRows.Rows)
                    {
                        dep_ids += item[0].ToString() + ",";
                    }
                    rDataRows = _servcieSaleOrderApp.GetSboSlpCodeIds(dep_ids.TrimEnd(','), SboID);
                }
                else
                {
                    rDataRows = _servcieSaleOrderApp.GetSboSlpCodeIds(DepID, SboID);
                }

                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR d.DfTcnician IN (");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }
                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(")) AND ");
                }

            }

            if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                DataTable rDataRowsSlp = _servcieSaleOrderApp.GetSboSlpCodeId(UserID, SboID);
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string DfTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    filterString += string.Format(" (a.SlpCode = {0}) AND ", slpCode);// OR d.DfTcnician={1}  , DfTcnician  不允许售后查看业务员的单
                }
                else
                {
                    filterString = string.Format(" (a.SlpCode = {0}) AND ", 0);
                }
            }
            #endregion

            if (!string.IsNullOrEmpty(filterString))
                filterString = filterString.Substring(0, filterString.Length - 5);
            DataTable dt = _servcieSaleOrderApp.SelectBillViewInfoNos(out rowCount, pageSize, pageIndex, filterString, sortString, type, line, ViewCustom, ViewSales);
            if (type.ToLower() == "sale_odln")
            {
                foreach (DataRow odlnrow in dt.Rows)
                {
                    string docnum = odlnrow["DocEntry"].ToString();
                    DataTable thist = _servcieSaleOrderApp.GetSalesDelivery_PurchaseOrderList(docnum, SboID.ToString());
                    string buyentry = "";
                    string transname = "";
                    string transid = "";
                    double transsum = 0.00;
                    string tempname = "";
                    string transDocTotal = "";
                    for (int i = 0; i < thist.Rows.Count; i++)
                    {
                        transsum += double.Parse(thist.Rows[i]["DocTotal"].ToString());// 交货对应采购单总金额
                                                                                       //快递单号，对应采购单编号
                        if (string.IsNullOrEmpty(buyentry))
                        {
                            buyentry = thist.Rows[i]["Buy_DocEntry"].ToString();
                            transid = string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString();
                            tempname = thist.Rows[i]["CardName"].ToString();
                            transname = tempname;
                            transDocTotal = thist.Rows[i]["DocTotal"].ToString();
                        }
                        else
                        {
                            buyentry += ";" + thist.Rows[i]["Buy_DocEntry"].ToString();
                            transid += ";" + (string.IsNullOrEmpty(thist.Rows[i]["LicTradNum"].ToString()) ? "000000000" : thist.Rows[i]["LicTradNum"].ToString());
                            //物流公司名称如果连续重复，则只显示第一个
                            if (tempname != thist.Rows[i]["CardName"].ToString())
                                tempname = thist.Rows[i]["CardName"].ToString();
                            else
                                tempname = "";
                            transname += ";;" + tempname;
                            transDocTotal += ";" + thist.Rows[i]["DocTotal"].ToString();
                        }
                    }
                    odlnrow["BuyDocEntry"] = buyentry;
                    odlnrow["TransportName"] = transname;
                    odlnrow["TransportID"] = transid;
                    odlnrow["TransportSum"] = transsum.ToString() + ";" + transDocTotal;
                }
            }

            if (type.ToLower() == "sale_ordr")
            {
                dt.Columns.Add("billStatus", typeof(string));
                dt.Columns.Add("bonusStatus", typeof(string));
                dt.Columns.Add("proStatus", typeof(string));
                dt.Columns.Add("IndicatorName", typeof(string));
                dt.Columns.Add("EmpAcctWarn", typeof(string));
                string bonustypeid = _servcieSaleOrderApp.GetJobTypeByUrl("sales/SalesBonus.aspx");
                string bonusatypeid = _servcieSaleOrderApp.GetJobTypeByUrl("sales/BonusAfterSales.aspx");
                string protypeid = _servcieSaleOrderApp.GetJobTypeByUrl("product/ProductionOrder.aspx");
                string protypeid_cp = _servcieSaleOrderApp.GetJobTypeByUrl("product/ProductionOrder_CP.aspx");
                string typeidstr = bonustypeid + "," + bonusatypeid + "," + protypeid + "," + protypeid_cp;
                foreach (DataRow ordrrow in dt.Rows)
                {
                    string orderid = ordrrow["DocEntry"].ToString();
                    ordrrow["billStatus"] = _servcieSaleOrderApp.GetBillStatusByOrderId(orderid, SboID.ToString());
                    DataTable jobtab = _servcieSaleOrderApp.GetJobStateForDoc(orderid, typeidstr, SboID.ToString());
                    DataRow[] bonusrows = jobtab.Select("job_type_id=" + bonustypeid + " or job_type_id=" + bonusatypeid);
                    DataRow[] prorows = jobtab.Select("job_type_id=" + protypeid + " or job_type_id=" + protypeid_cp, "upd_dt desc");
                    ordrrow["bonusStatus"] = "";
                    ordrrow["proStatus"] = "";
                    if (bonusrows.Length > 0)
                    {
                        ordrrow["bonusStatus"] = bonusrows[0]["job_state"].ToString();
                    }
                    if (prorows.Length > 0)
                    {
                        ordrrow["proStatus"] = prorows[0]["job_state"].ToString();
                    }
                    ordrrow["IndicatorName"] = _servcieSaleOrderApp.GetBillIndicatorByOrderId(orderid, SboID.ToString());
                    ordrrow["EmpAcctWarn"] = _servcieSaleOrderApp.GetEmptyAcctByOrderId(orderid, SboID.ToString());
                }
            }

            if (type.ToLower() == "buy_opor")
            {
                dt.Columns.Add("PurchaseBillNo", typeof(string));
                dt.Columns.Add("IndicatorName", typeof(string));
                foreach (DataRow temprow in dt.Rows)
                {
                    string indicator = temprow["Indicator"].ToString();
                    string taxstr = _servcieSaleOrderApp.GetTaxNoByPO(temprow["DocEntry"].ToString(), SboID.ToString());
                    temprow["PurchaseBillNo"] = taxstr;
                    temprow["IndicatorName"] = _servcieSaleOrderApp.GetToCompanyName(SboID.ToString(), indicator);
                }
            }

            return dt;
        }

        /// <summary>
        /// 获取角色名称
        /// </summary>
        /// <param name="userID">用户Id</param>
        /// <returns>成功返回角色名称，失败返回空</returns>
        public static string GetRolesName(int userID)
        {
            try
            {
                DataTable dTable = null;
                lock (gRoles)
                {
                    gRoles.TryGetValue(userID, out dTable);
                }
                if (dTable != null && dTable.Rows.Count > 0)
                {
                    return string.Join(",", dTable.AsEnumerable().Select(r => r[1].ToString().Trim()).ToArray());
                }
                return string.Empty;
            }
            catch { return string.Empty; }
        }

        /// <summary>
        /// 获取销售报价单信息
        /// </summary>
        /// <param name="pageSize">页码</param>
        /// <param name="pageIndex">页码位置</param>
        /// <param name="query">查询</param>
        /// <param name="type">类型</param>
        /// <param name="ViewFull">视图</param>
        /// <param name="ViewSelf">视图</param>
        /// <param name="UserID">用户Id</param>
        /// <param name="SboID">SboId</param>
        /// <param name="ViewSelfDepartment">视图</param>
        /// <param name="DepID">DepId</param>
        /// <param name="ViewCustom">视图</param>
        /// <param name="ViewSales">视图</param>
        /// <param name="sqlcont">数据连接</param>
        /// <param name="sboname">sbo名称</param>
        /// <returns>成功返回数据信息，失败抛出异常信息</returns>
        public TableData SelectOrderDraftInfo(List<Role> roles, int pageSize, int pageIndex, QuerySalesQuotationReq query, string type, bool ViewFull, bool ViewSelf, int UserID, int SboID, bool ViewSelfDepartment, int DepID, bool ViewCustom, bool ViewSales, string sqlcont, string sboname)
        {
            TableData tableData = null;
            int rowCount = 0;
            bool IsSql = true;
            string sortString = string.Empty;
            string filterString = string.Empty;
            string line = string.Empty;
            int uSboId = SboID;

            //排序
            if (string.IsNullOrWhiteSpace(query.SortName))
            {
                sortString = string.Format("{0} {1}", "a.docentry", "desc".ToUpper());
            }
            else
            {
                sortString = string.Format("{0} {1}", query.SortName, query.SortOrder);
            }

            string dRowData = string.Empty;

            System.Data.DataTable rDataRowsSlp = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT sale_id,tech_id FROM nsap_base.sbo_user WHERE user_id={UserID} AND sbo_id={SboID}", CommandType.Text, null); ;
            if (!roles.Any(r => r.Name.Equals("管理员")))
            {
                filterString += string.Format(" a.SlpCode = '{0}' AND ", rDataRowsSlp.Rows[0][0].ToString());
            }

            #region 搜索条件
            //账
            if (query.Sboid > 0)
            {
                if (query.Sboid == SboID)
                {
                    IsSql = true;
                }
                else
                {
                    IsSql = false;
                }
            }

            //单号条件
            if (!string.IsNullOrWhiteSpace(query.DocEntry))
            {
                filterString += string.Format("a.DocEntry LIKE '{0}' AND ", query.DocEntry.Trim());
            }

            if (!string.IsNullOrWhiteSpace(query.CardCode))
            {
                filterString += string.Format("(a.CardCode LIKE '%{0}%' OR a.CardName LIKE '%{0}%') AND ", query.CardCode.Trim());
            }

            if (!string.IsNullOrWhiteSpace(query.DocStatus))
            {
                if (query.DocStatus == "ON")
                {
                    filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'N' AND a.CANCELED = 'N') AND ");
                }

                if (query.DocStatus == "OY")
                {
                    filterString += string.Format(" (a.DocStatus = 'O' AND a.Printed = 'Y' AND a.CANCELED = 'N') AND ");
                }

                if (query.DocStatus == "CY")
                {
                    filterString += string.Format(" a.CANCELED = 'Y' AND ");
                }

                if (query.DocStatus == "CN")
                {
                    filterString += string.Format(" (a.DocStatus = 'C' AND a.CANCELED = 'N') AND ");
                }
            }
            else
            {
                filterString += string.Format(" (a.CANCELED = 'N') AND ");
            }

            if (!string.IsNullOrWhiteSpace(query.Comments))
            {
                filterString += string.Format("a.Comments LIKE '%{0}%' AND ", query.Comments);
            }

            if (!string.IsNullOrWhiteSpace(query.SlpName))
            {
                filterString += string.Format("c.SlpName LIKE '%{0}%' AND ", query.SlpName.Trim());
            }

            //时间区间
            if (!string.IsNullOrWhiteSpace(query.FirstTime) && !string.IsNullOrWhiteSpace(query.LastTime))
            {
                filterString += string.Format("a.UpdateDate BETWEEN '{0}' AND '{1}' AND ", query.FirstTime, query.LastTime);
            }

            if (type == "ORDR")
            {
                if (!string.IsNullOrWhiteSpace(query.Indicator))
                {
                    filterString += string.Format("a.Indicator = '{0}' AND ", query.Indicator);
                }
            }
            #endregion

            #region 根据不同的单据显示不同的内容
            if (!string.IsNullOrEmpty(type))
            {
                if (type == "OQUT") { type = "OQUT"; line = "QUT1"; }//销售报价单
                else if (type == "ORDR") { type = "ORDR"; line = "RDR1"; }//销售订单
                else if (type == "ODLN") { type = "ODLN"; line = "DLN1"; }//销售交货单
                else if (type == "OINV") { type = "OINV"; line = "INV1"; }//应收发票
                else if (type == "ORDN") { type = "ORDN"; line = "RDN1"; }//销售退货单
                else if (type == "ORIN") { type = "ORIN"; line = "RIN1"; }//应收贷项凭证
                else if (type == "OPQT") { type = "OPQT"; line = "PQT1"; }//采购报价单
                else if (type == "OPOR") { type = "OPOR"; line = "POR1"; }//采购订单
                else if (type == "OPDN") { type = "OPDN"; line = "PDN1"; }//采购收货单
                else if (type == "OPCH") { type = "OPCH"; line = "PCH1"; }//应付发票
                else if (type == "ORPD") { type = "ORPD"; line = "RPD1"; }//采购退货单
                else if (type == "ORPC") { type = "ORPC"; line = "RPC1"; }//应付贷项凭证
                else { type = "OQUT"; line = "QUT1"; }
            }
            #endregion

            #region 判断权限
            string arr_roles = UnitWork.ExcuteSql<RolesDto>(ContextType.NsapBaseDbContext, $"SELECT a.role_id Id,b.role_nm Name FROM nsap_base.base_user_role AS a INNER JOIN nsap_base.base_role AS b ON a.role_id=b.role_id WHERE a.user_id={UserID}", CommandType.Text, null).FirstOrDefault()?.Name;
            if ((line == "PQT1" || line == "POR1" || line == "PDN1" || line == "PCH1" || line == "RPD1" || line == "RPC1") && ((!arr_roles.Contains("物流文员")) && (!arr_roles.Contains("系统管理员"))))//若不含有物流文员角色，则则屏蔽运输采购单
            {
                filterString += string.Format(" d.QryGroup1='N' AND ");
            }

            if (ViewSelfDepartment && !ViewFull)
            {
                System.Data.DataTable rDataRows;
                DataTable rdepRows = new DataTable();
                if (line == "POR1" || line == "PDN1" || line == "PQT1" || line == "PCH1" || line == "RPD1" || line == "RPC1")
                {
                    //查询部门映射关系表
                    rdepRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT dep_id2 FROM nsap_bone.base_dep_map  WHERE dep_id1={DepID} and map_type_id=1", CommandType.Text, null);
                }

                if (rdepRows.Rows.Count > 0)
                {
                    string dep_ids = string.Empty;
                    foreach (DataRow item in rdepRows.Rows)
                    {
                        dep_ids += item[0].ToString() + ",";
                    }

                    //部门查询
                    rDataRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT a.sale_id,a.tech_id FROM nsap_base.sbo_user a LEFT JOIN nsap_base.base_user_detail b ON a.user_id = b.user_id WHERE b.dep_id in ({dep_ids.TrimEnd(',')}) AND a.sbo_id = {SboID}", CommandType.Text, null);
                }
                else
                {
                    //根据部门ID获取销售员ID
                    rDataRows = UnitWork.ExcuteSqlTable(ContextType.NsapBaseDbContext, $"SELECT a.sale_id,a.tech_id FROM nsap_base.sbo_user a LEFT JOIN nsap_base.base_user_detail b ON a.user_id = b.user_id WHERE b.dep_id = {DepID} AND a.sbo_id = {SboID}", CommandType.Text, null);
                }

                if (rDataRows.Rows.Count > 0)
                {
                    filterString += string.Format(" (a.SlpCode IN(");
                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][0]);
                    }

                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(") OR d.DfTcnician IN (");

                    for (int i = 0; i < rDataRows.Rows.Count; i++)
                    {
                        filterString += string.Format("{0},", rDataRows.Rows[i][1]);
                    }

                    if (!string.IsNullOrEmpty(filterString))
                        filterString = filterString.Substring(0, filterString.Length - 1);
                    filterString += string.Format(")) AND ");
                }
            }
            else if (ViewSelf && !ViewFull && !ViewSelfDepartment)
            {
                if (rDataRowsSlp.Rows.Count > 0)
                {
                    string slpCode = rDataRowsSlp.Rows[0][0].ToString();
                    string slpTcnician = rDataRowsSlp.Rows[0][1].ToString();
                    filterString += string.Format(" (a.SlpCode = {0}) AND ", slpCode);// OR d.DfTcnician={1}   , slpTcnician  不允许售后查看业务员的单
                }
                else
                {
                    filterString = string.Format(" (a.SlpCode = {0}) AND ", 0);
                }
            }
            #endregion

            if (!string.IsNullOrEmpty(filterString))
            {
                filterString = filterString.Substring(0, filterString.Length - 5);
            }

            if (IsSql)
            {
                //视图查询数据
                tableData = _servcieSaleOrderApp.SelectOrdersInfo(out rowCount, pageSize, pageIndex, filterString, sortString, type, line, ViewCustom, ViewSales, sqlcont, sboname);
            }

            return tableData;
        }

        /// <summary>
        /// 获取印章列表
        /// </summary>
        /// <returns>返回印章列表信息</returns>
        public async Task<TableData> GetContractSealList(string companyType)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var categorySealList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractSealType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            var contractSealList = string.IsNullOrEmpty(companyType) ? await UnitWork.Find<ContractSeal>(r => r.IsEnable == true).ToListAsync() : await UnitWork.Find<ContractSeal>(r => r.IsEnable == true).Where(r => r.CompanyType == companyType).ToListAsync();
            var contractSeal = from a in contractSealList
                               join b in categorySealList on a.SealType equals b.DtValue
                               select new
                               {
                                   a.Id,
                                   a.SealNo,
                                   a.SealName,
                                   a.SealType,
                                   a.CompanyType,
                                   SealTypeValue = b.DtValue,
                                   SealTypeName = b.Name
                               };

            var result = new TableData();
            result.Count = contractSealList.Count();
            result.Data = contractSeal;
            return result;
        }

        /// <summary>
        /// 获取所有所属公司
        /// </summary>
        /// <returns>返回所属公司信息</returns>
        public async Task<TableData> GetContractCompanyList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            result.Count = categoryCompanyList.Count();
            result.Data = categoryCompanyList;
            return result;
        }

        /// <summary>
        /// 获取所有合同类型
        /// </summary>
        /// <returns>返回合同类型信息</returns>
        public async Task<TableData> GetContractTypeList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var categorTypeList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            result.Count = categorTypeList.Count();
            result.Data = categorTypeList.OrderBy(r => r.DtValue).ToList();
            return result;
        }

        /// <summary>
        /// 获取所有节点状态
        /// </summary>
        /// <returns>返回节点状态信息</returns>
        public async Task<TableData> GetContractStatusList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var categorStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            result.Count = categorStatusList.Count();
            result.Data = categorStatusList;
            return result;
        }

        /// <summary>
        /// 获取所有文件类型
        /// </summary>
        /// <param name="contractType">合同类型</param>
        /// <returns>返回文件类型信息</returns>
        public async Task<TableData> GetContractFileTypeList(string contractType)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            if (string.IsNullOrEmpty(contractType))
            {
                var categorFileTypeList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                result.Count = categorFileTypeList.Count();
                result.Data = categorFileTypeList;
            }
            else
            {
                List<Category> categorFileTypes = new List<Category>();
                switch (contractType)
                {
                    case "1":
                        List<string> contractTypes = new List<string>() { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "14", "15" };
                        categorFileTypes = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType") && contractTypes.Contains(u.DtValue)).ToListAsync();
                        result.Count = categorFileTypes.Count();
                        result.Data = categorFileTypes.OrderBy(r => r.SortNo).Select(u => new { u.DtValue, u.Name }).ToList();
                        break;
                    case "2":
                        categorFileTypes = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType") && (u.DtValue == "11")).ToListAsync();
                        result.Count = categorFileTypes.Count();
                        result.Data = categorFileTypes.OrderBy(r => r.SortNo).Select(u => new { u.DtValue, u.Name }).ToList();
                        break;
                    case "3":
                        categorFileTypes = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType") && u.DtValue == "13").ToListAsync();
                        result.Count = categorFileTypes.Count();
                        result.Data = categorFileTypes.OrderBy(r => r.SortNo).Select(u => new { u.DtValue, u.Name }).ToList();
                        break;
                    default:
                        categorFileTypes = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType") && u.DtValue == "12").ToListAsync();
                        result.Count = categorFileTypes.Count();
                        result.Data = categorFileTypes.OrderBy(r => r.SortNo).Select(u => new { u.DtValue, u.Name }).ToList();
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// 上传原始合同
        /// </summary>
        /// <param name="obj">合同申请单实体数据</param>
        /// <returns>返回文件上传实体数据集合，失败抛出异常</returns>
        public async Task<TableData> AddOriginal(ContractApply obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var loginUser = loginContext.User;
            var dbContext = UnitWork.GetDbContext<ContractApply>();
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    #region 删除
                    var contractFileType = await UnitWork.Find<ContractFileType>(r => r.ContractApplyId == obj.Id).ToListAsync();
                    foreach (ContractFileType item in contractFileType)
                    {
                        await UnitWork.DeleteAsync<ContractFile>(r => r.ContractFileTypeId == item.Id);
                    }

                    await UnitWork.DeleteAsync<ContractFileType>(r => r.ContractApplyId == obj.Id);
                    await UnitWork.SaveAsync();
                    #endregion

                    #region 新增  
                    List<ContractFileType> contractFileTypeList = new List<ContractFileType>();
                    foreach (ContractFileType item in obj.ContractFileTypeList)
                    {
                        if (string.IsNullOrEmpty(item.FileType) || string.IsNullOrEmpty(item.ContractSealId))
                        {
                            throw new Exception("文件类型或印章Id不能为空");
                        }

                        if (string.IsNullOrEmpty(item.ContractOriginalId))
                        {
                            throw new Exception("结束合同申请单时合同原件不能为空");
                        }

                        item.Id = Guid.NewGuid().ToString();
                        item.ContractApplyId = obj.Id;
                        List<ContractFile> contractFileList = new List<ContractFile>();
                        foreach (ContractFile contractFile in item.contractFileList)
                        {
                            ContractFile file = new ContractFile();
                            file.ContractFileTypeId = item.Id;
                            file.IsSeal = contractFile.IsSeal;
                            file.FileId = contractFile.FileId;
                            file.IsFinalContract = contractFile.IsFinalContract;
                            file.CreateUploadId = contractFile.CreateUploadId == "" ? loginContext.User.Id : contractFile.CreateUploadId;
                            file.CreateUploadName = contractFile.CreateUploadName;
                            file.CreateUploadTime = contractFile.CreateUploadTime == null ? DateTime.Now : contractFile.CreateUploadTime;
                            file.UpdateUserId = loginContext.User.Id;
                            file.UpdateUserName = loginContext.User.Name;
                            file.UpdateUserTime = DateTime.Now;
                            contractFileList.Add(file);
                        }

                        if (contractFileList != null && contractFileList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<ContractFile>(contractFileList.ToArray());
                        }

                        item.contractFileList = contractFileList;
                        contractFileTypeList.Add(item);
                    }

                    if (contractFileTypeList != null && contractFileTypeList.Count > 0)
                    {
                        await UnitWork.BatchAddAsync<ContractFileType>(contractFileTypeList.ToArray());
                    }

                    //清空旧数据
                    obj.ContractFileTypeList.Clear();
                    await UnitWork.SaveAsync();
                    #endregion

                    //修改合同申请单
                    await UnitWork.UpdateAsync<ContractApply>(r => r.Id == obj.Id, r => new ContractApply
                    {
                        UpdateTime = DateTime.Now,
                        ContractNo = obj.ContractNo,
                        CustomerCode = obj.CustomerCode,
                        CustomerName = obj.CustomerName,
                        CompanyType = obj.CompanyType,
                        ContractType = obj.ContractType,
                        QuotationNo = obj.QuotationNo,
                        SaleNo = obj.SaleNo,
                        IsDraft = obj.IsDraft,
                        IsUploadOriginal = true,
                        IsUseCompanyTemplate = obj.IsUseCompanyTemplate,
                        FlowInstanceId = obj.FlowInstanceId,
                        DownloadNumber = obj.DownloadNumber,
                        U_SAP_ID = obj.U_SAP_ID,
                        ItemNo = obj.ItemNo,
                        ItemName = obj.ItemName,
                        ContractStatus = "-1",
                        Remark = obj.Remark,
                        CreateId = obj.CreateId,
                        CreateName = obj.CreateName,
                        CreateTime = obj.CreateTime
                    });

                    await UnitWork.AddAsync<ContractOperationHistory>(new ContractOperationHistory
                    {
                        Action = "上传合同原件",
                        CreateUser = loginUser.Name,
                        CreateUserId = loginUser.Id,
                        CreateTime = DateTime.Now,
                        ApprovalStage = "-1",
                        ContractApplyId = obj.Id
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();

                    result.Message = "原始合同上传成功，结束合同申请";
                    result.Code = 200;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Message = ex.Message.ToString();
                    result.Code = 500;
                }
            }
            return result;
        }

        /// <summary>
        /// 上传文件
        /// </summary>
        /// <param name="file">表单文件</param>
        /// <returns>成功返回文件上传实体数据，失败抛出异常</returns>
        public async Task<UploadFileResp> AddFile(IFormFile file)
        {
            if (file != null)
            {
                _logger.LogInformation("收到新文件: " + file.FileName);
                _logger.LogInformation("收到新文件: " + file.Length);
            }
            else
            {
                _logger.LogWarning("收到新文件为空");
            }
            var uploadResult = await _fileStore.UploadFile(file);
            uploadResult.CreateUserName = _auth.GetCurrentUser().User.Name;
            uploadResult.CreateUserId = Guid.Parse(_auth.GetCurrentUser().User.Id);
            var a = await Repository.AddAsync(uploadResult);
            return uploadResult.MapTo<UploadFileResp>();
        }

        /// <summary>
        /// 上传电子盖章文件
        /// </summary>
        /// <param name="file"></param>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<UploadFileResp> UploadSealFile(IFormFile file, QueryEleSealReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            if (file != null)
            {
                _logger.LogInformation("收到新文件: " + file.FileName);
                _logger.LogInformation("收到新文件: " + file.Length);
            }
            else
            {
                _logger.LogWarning("收到新文件为空");
            }

            var uploadResult = await _fileStore.UploadFile(file);
            uploadResult.CreateUserName = _auth.GetCurrentUser().User.Name;
            uploadResult.CreateUserId = Guid.Parse(_auth.GetCurrentUser().User.Id);
            await Repository.AddAsync(uploadResult);
            var objs = UnitWork.Find<ContractApply>(r => r.Id == req.ContractApplyId).FirstOrDefault();

            //所有合同盖章完成后更改状态
            string sqlContractFile = string.Format("UPDATE erp4_serve.contractfile SET FileId='" + uploadResult.Id + "' WHERE FileId='" + req.ContractFileId + "' AND ContractApplyId = '" + req.ContractApplyId + "'");
            int resultCountFile = UnitWork.ExecuteSql(sqlContractFile, ContextType.NsapBaseDbContext);
            var contractFileList = UnitWork.Find<ContractFileType>(r => r.ContractApplyId == req.ContractApplyId);
            if (contractFileList == null || contractFileList.Count() == 0)
            {
                string sqlContract = string.Format("UPDATE erp4_serve.contractapply SET ContractStatus='7',IsSeal=0  WHERE Id='" + req.ContractApplyId + "'");
                int resultCountJob = UnitWork.ExecuteSql(sqlContract, ContextType.NsapBaseDbContext);
                var contractFileSeal = UnitWork.Find<ContractFileType>(r => r.Id == req.ContractApplyId).FirstOrDefault();
                await UnitWork.AddAsync<ContractSealOperationHistory>(new ContractSealOperationHistory
                {
                    ContractSealId = contractFileSeal.ContractSealId,
                    ContractNo = objs.ContractNo,
                    CreateUserId = loginUser.Id,
                    CreateUserName = loginUser.Name,
                    CreateTime = DateTime.Now,
                    ContractFinalNum = (UnitWork.Find<ContractFileType>(r => r.ContractApplyId == req.ContractApplyId).GroupBy(r => new { r.ContractApplyId, contractFileSeal.ContractSealId })).Count(),
                    OperationType = "电子盖章"
                });

                await UnitWork.SaveAsync();
            }

            return uploadResult.MapTo<UploadFileResp>();
        }

        /// <summary>
        /// 盖章
        /// </summary>
        /// <param name="obj">合同申请单实体信息</param>
        /// <returns>成功返回200，失败返回500并抛出异常</returns>
        public async Task<TableData> SetContractSeal(ContractApply obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var dbContext = UnitWork.GetDbContext<ContractApply>();
            var result = new TableData();
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    #region 删除
                    var contractFileType = await UnitWork.Find<ContractFileType>(r => r.ContractApplyId == obj.Id).ToListAsync();
                    foreach (ContractFileType item in contractFileType)
                    {
                        await UnitWork.DeleteAsync<ContractFile>(r => r.ContractFileTypeId == item.Id);
                    }

                    await UnitWork.DeleteAsync<ContractFileType>(r => r.ContractApplyId == obj.Id);
                    await UnitWork.SaveAsync();
                    #endregion

                    #region 新增  
                    List<ContractFileType> contractFileTypeList = new List<ContractFileType>();
                    foreach (ContractFileType item in obj.ContractFileTypeList)
                    {
                        if (item.FileType == "" || item.ContractSealId == "")
                        {
                            throw new Exception("文件类型或印章Id不能为空");
                        }

                        item.Id = Guid.NewGuid().ToString();
                        item.ContractApplyId = obj.Id;
                        List<ContractFile> contractFileList = new List<ContractFile>();
                        foreach (ContractFile contractFile in item.contractFileList)
                        {
                            ContractFile file = new ContractFile();
                            file.ContractFileTypeId = item.Id;
                            file.IsSeal = contractFile.IsSeal;
                            file.FileId = contractFile.FileId;
                            file.IsFinalContract = contractFile.IsFinalContract;
                            file.CreateUploadId = contractFile.CreateUploadId == "" ? loginContext.User.Id : contractFile.CreateUploadId;
                            file.CreateUploadName = contractFile.CreateUploadName;
                            file.CreateUploadTime = contractFile.CreateUploadTime == null ? DateTime.Now : contractFile.CreateUploadTime;
                            file.UpdateUserId = loginContext.User.Id;
                            file.UpdateUserName = loginContext.User.Name;
                            file.UpdateUserTime = DateTime.Now;
                            contractFileList.Add(file);
                        }

                        if (contractFileList != null && contractFileList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<ContractFile>(contractFileList.ToArray());
                        }

                        item.contractFileList = contractFileList;
                        contractFileTypeList.Add(item);
                    }

                    if (contractFileTypeList != null && contractFileTypeList.Count > 0)
                    {
                        await UnitWork.BatchAddAsync<ContractFileType>(contractFileTypeList.ToArray());
                    }

                    //清空旧数据
                    obj.ContractFileTypeList.Clear();
                    await UnitWork.SaveAsync();
                    #endregion

                    //修改合同申请单
                    await UnitWork.UpdateAsync<ContractApply>(r => r.Id == obj.Id, r => new ContractApply
                    {
                        UpdateTime = DateTime.Now,
                        ContractNo = obj.ContractNo,
                        CustomerCode = obj.CustomerCode,
                        CustomerName = obj.CustomerName,
                        CompanyType = obj.CompanyType,
                        ContractType = obj.ContractType,
                        QuotationNo = obj.QuotationNo,
                        SaleNo = obj.SaleNo,
                        IsDraft = obj.IsDraft,
                        IsUseCompanyTemplate = obj.IsUseCompanyTemplate,
                        IsUploadOriginal = obj.IsUploadOriginal,
                        FlowInstanceId = obj.FlowInstanceId,
                        DownloadNumber = obj.DownloadNumber,
                        U_SAP_ID = obj.U_SAP_ID,
                        ItemName = obj.ItemName,
                        ItemNo = obj.ItemNo,
                        ContractStatus = "7",
                        Remark = obj.Remark,
                        CreateId = obj.CreateId,
                        CreateName = obj.CreateName,
                        CreateTime = obj.CreateTime
                    });

                    await UnitWork.AddAsync<ContractOperationHistory>(new ContractOperationHistory
                    {
                        Action = "合同申请单线下盖章",
                        CreateUser = loginUser.Name,
                        CreateUserId = loginUser.Id,
                        CreateTime = DateTime.Now,
                        ApprovalStage = "6",
                        ContractApplyId = obj.Id
                    });

                    var contractFilesList = await UnitWork.Find<ContractFileType>(r => r.ContractApplyId == obj.Id).Select(r => new ContractFileQuery { Id = r.Id, ContractSealId = r.ContractSealId }).ToListAsync();
                    foreach (ContractFileQuery item in contractFilesList.Distinct())
                    {
                        await UnitWork.AddAsync<ContractSealOperationHistory>(new ContractSealOperationHistory
                        {
                            ContractSealId = item.ContractSealId,
                            ContractNo = obj.ContractNo,
                            CreateUserId = loginUser.Id,
                            CreateUserName = loginUser.Name,
                            CreateTime = DateTime.Now,
                            ContractFinalNum = (await UnitWork.Find<ContractFile>(r => r.ContractFileTypeId == item.Id && r.IsFinalContract == true).ToListAsync()).Count(),
                            OperationType = "线下盖章"
                        });
                    }

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    result.Message = ex.Message.ToString();
                    result.Code = 500;
                    return result;
                }
            }

            result.Code = 200;
            result.Message = "操作成功";
            return result;
        }

        /// <summary>
        /// 电子盖章(无效)
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> SetContractEleSeal(QueryEleSealReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            string path;
            string newFileName = "";
            UploadFile uploadResult = new UploadFile();

            //获取合同申请单
            var objs = UnitWork.Find<ContractApply>(r => r.Id == req.ContractApplyId).FirstOrDefault();
            if (objs.ContractStatus != "6")
            {
                result.Message = "当前节点不允许盖章，请在总助审批完成后盖章";
                result.Code = 500;
                return result;
            }

            //获取文件流
            var contractFile = await UnitWork.Find<UploadFile>(r => r.Id == req.ContractFileId).FirstOrDefaultAsync();
            var documentStream = await _fileStore.DownloadFile(contractFile.BucketName, contractFile.FilePath);
            var imageFile = await UnitWork.Find<UploadFile>(r => r.Id == req.ImageId).FirstOrDefaultAsync();
            Stream imageStream = await _fileStore.DownloadFile(imageFile.BucketName, imageFile.FilePath);

            //文档添加印章
            bool isSuccess = true;
            if (isSuccess)
            {
                //将文件上传至MinIO服务器
                path = System.IO.Directory.GetCurrentDirectory() + "\\" + newFileName;
                uploadResult = await UploadMinIO(path, newFileName.Split('.')[0].ToString(), contractFile.Extension);
                uploadResult.CreateUserName = loginUser.Name;
                uploadResult.CreateUserId = Guid.Parse(loginUser.Id);
                await Repository.AddAsync(uploadResult);
                await UnitWork.AddAsync<ContractOperationHistory>(new ContractOperationHistory
                {
                    Action = contractFile.FileName + "合同文件电子盖章",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    ApprovalStage = "6",
                    ContractApplyId = req.ContractApplyId
                });

                //删除本地服务器文件
                //await DeleteFile(path);

                //所有合同盖章完成后更改状态
                string sqlContractFile = string.Format("UPDATE erp4_serve.contractfile SET FileId='" + uploadResult.Id + "' WHERE FileId='" + req.ContractFileId + "' AND ContractApplyId = '" + req.ContractApplyId + "'");
                int resultCountFile = UnitWork.ExecuteSql(sqlContractFile, ContextType.NsapBaseDbContext);
                var contractFileList = UnitWork.Find<ContractFileType>(r => r.ContractApplyId == req.ContractApplyId);
                if (contractFileList == null || contractFileList.Count() == 0)
                {
                    string sqlContract = string.Format("UPDATE erp4_serve.contractapply SET ContractStatus='7',IsSeal=0  WHERE Id='" + req.ContractApplyId + "'");
                    int resultCountJob = UnitWork.ExecuteSql(sqlContract, ContextType.NsapBaseDbContext);
                    var contractFileSeal = UnitWork.Find<ContractFileType>(r => r.Id == req.ContractApplyId).FirstOrDefault();
                    await UnitWork.AddAsync<ContractSealOperationHistory>(new ContractSealOperationHistory
                    {
                        ContractSealId = contractFileSeal.ContractSealId,
                        ContractNo = objs.ContractNo,
                        CreateUserId = loginUser.Id,
                        CreateUserName = loginUser.Name,
                        CreateTime = DateTime.Now,
                        ContractFinalNum = (UnitWork.Find<ContractFileType>(r => r.ContractApplyId == req.ContractApplyId).GroupBy(r => new { r.ContractApplyId, contractFileSeal.ContractSealId })).Count(),
                        OperationType = "电子盖章"
                    });

                    await UnitWork.SaveAsync();
                }

                if (resultCountFile > 0)
                {
                    result.Message = "电子盖章成功";
                    result.Code = 200;
                }
            }
            else
            {
                result.Message = "电子盖章失败";
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 更新文件下载次数
        /// </summary>
        /// <param name="contractId">合同申请单Id</param>
        /// <param name="fileId">文件Id</param>
        /// <returns>成功返回true，失败返回false</returns>
        public async Task<bool> GetFileDownloadNum(string contractId, string fileId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var objs = await UnitWork.Find<ContractApply>(r => r.Id == contractId).FirstOrDefaultAsync();
            var dbContext = UnitWork.GetDbContext<ContractApply>();
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    //更新下载次数
                    await UnitWork.UpdateAsync<ContractApply>(q => q.Id == contractId, q => new ContractApply { DownloadNumber = objs.DownloadNumber + 1 });

                    //文件下载记录
                    await UnitWork.AddAsync<ContractDownLoadFileHis>(new ContractDownLoadFileHis
                    {
                        ContractApplyId = contractId,
                        FileId = fileId,
                        CreateUserId = loginUser.Id,
                        CreateName = loginUser.Name,
                        CreateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                }
            }

            return false;
        }

        /// <summary>
        /// 压缩文件为zip压缩包
        /// </summary>
        /// <param name="contractId">申请表Id</param>
        /// <param name="dictZip">文件id，文件路径字典</param>
        /// <return>成功返回文件流，失败抛出异常</return>
        public async Task<TableData> ZipFilesAsync(string contractId, Dictionary<string, string> dictZip)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var loginUser = loginContext.User;
            var objs = await UnitWork.Find<ContractApply>(r => r.Id == contractId).FirstOrDefaultAsync();
            string number = objs.ContractNo + "-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            string zipfile = $"Templates/files/{ number + ".zip"}";
            try
            {
                List<ContractDownLoadFileHis> contractDownLoadFileHiss = new List<ContractDownLoadFileHis>();
                #region 本地服务器生成压缩包
                ZipOutputStream s = new ZipOutputStream(System.IO.File.Create(zipfile));
                s.SetLevel(6);
                foreach (string file in dictZip.Keys)
                {
                    var fs = new MemoryStream();
                    var fileMsg = await _fileApp.GetFileAsync(file);
                    await _minioClient.GetObjectAsync(fileMsg.BucketName, dictZip[file], s => s.CopyTo(fs));
                    fs.Position = 0;
                    byte[] buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, buffer.Length);
                    var name = Path.GetFileName(dictZip[file]);
                    ZipEntry entry = new ZipEntry(name);
                    entry.DateTime = DateTime.Now;
                    entry.Size = fs.Length;
                    fs.Close();
                    s.PutNextEntry(entry);
                    s.Write(buffer, 0, buffer.Length);

                    //记录文件下载历史
                    ContractDownLoadFileHis contractDownLoadFileHis = new ContractDownLoadFileHis();
                    contractDownLoadFileHis.ContractApplyId = contractId;
                    contractDownLoadFileHis.FileId = file;
                    contractDownLoadFileHis.CreateUserId = loginUser.Id;
                    contractDownLoadFileHis.CreateName = loginUser.Name;
                    contractDownLoadFileHis.CreateTime = DateTime.Now;
                    contractDownLoadFileHiss.Add(contractDownLoadFileHis);
                }

                s.Finish();
                s.Close();
                #endregion

                #region 将压缩包上传至MinIO服务器
                string path = System.IO.Directory.GetCurrentDirectory() + "\\Templates\\files\\" + number + ".zip";
                var uploadResult = await UploadMinIO(path, number, ".zip");
                uploadResult.CreateUserName = loginUser.Name;
                uploadResult.CreateUserId = Guid.Parse(loginUser.Id);
                #endregion

                //删除本地服务器文件
                await DeleteFile(path);

                //批量保存文件下载历史记录
                if (contractDownLoadFileHiss.Count() > 0)
                {
                    await UnitWork.BatchAddAsync<ContractDownLoadFileHis>(contractDownLoadFileHiss.ToArray());
                }

                await UnitWork.SaveAsync();
                result.Data = await Repository.AddAsync(uploadResult);
                result.Code = 200;
            }
            catch (Exception ex)
            {
                result.Message = ex.ToString();
                result.Code = 500;
            }

            return result;
        }

        /// <summary>
        /// 压缩包上传至MinIO服务器
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <param name="name">文件名</param>
        /// <param name="fileType">文件类型</param>
        /// <returns>成功返回上传文件实体数据，失败抛出异常</returns>
        public async Task<UploadFile> UploadMinIO(string path, string name, string fileType)
        {
            FileStream fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            byte[] bt = new byte[fileStream.Length];
            fileStream.Read(bt, 0, bt.Length);
            Stream st = new MemoryStream(bt);
            FileStreamResult fileResult = new FileStreamResult(st, "application/x-zip-compressed");
            fileResult.FileDownloadName = name + fileType;
            var ms = new MemoryStream();
            fileResult.FileStream.CopyTo(ms);
            IFormFile ff = new FormFile(ms, 0, ms.Length, name, name + fileType);
            fileStream.Close();
            st.Close();
            var uploadResult = await _fileStore.UploadFile(ff);
            return uploadResult;
        }

        /// <summary>
        /// 删除本地服务器文件
        /// </summary>
        /// <param name="path">文件路径</param>
        /// <returns></returns>
        public async Task DeleteFile(string path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
        }

        /// <summary>
        /// 获取提交给我的
        /// </summary>
        /// <param name="request">合同申请单查询实体</param>
        /// <returns>成功返回数据集，失败抛出异常</returns>
        public async Task<TableData> GetMyAccraditation(QueryContractApplyListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            List<string> statusList = new List<string>();
            if (loginContext.Roles.Any(r => r.Name.Equals("法务人员")))
            {
                statusList.Add("3");
                statusList.Add("4");
                statusList.Add("9");
                statusList.Add("10");
            }

            if (loginContext.Roles.Any(r => r.Name.Equals("售前工程师")))
            {
                if (!statusList.Contains("4"))
                {
                    statusList.Add("4");
                }

                statusList.Add("8");
                statusList.Add("12");
            }

            if (loginContext.Roles.Any(r => r.Name.Equals("销售总助")))
            {
                statusList.Add("5");
                statusList.Add("10");
            }

            var objs = UnitWork.Find<ContractApply>(r => statusList.Contains(r.ContractStatus)).Include(r => r.ContractFileTypeList).Include(r => r.contractOperationHistoryList);
            var contractApply = (objs.WhereIf(!string.IsNullOrWhiteSpace(request.ContractNo), r => r.ContractNo.Contains(request.ContractNo))
                                     .WhereIf(!string.IsNullOrWhiteSpace(request.CustomerCodeOrName), r => r.CustomerCode.Contains(request.CustomerCodeOrName) || r.CustomerName.Contains(request.CustomerCodeOrName))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompanyType), r => r.CompanyType.Equals(request.CompanyType))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.QuotationNo), r => r.QuotationNo.Contains(request.QuotationNo))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.SaleNo), r => r.SaleNo.Contains(request.SaleNo))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.ItemNo), r => r.ItemNo.Contains(request.ItemNo))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.ItemName), r => r.ItemName.Contains(request.ItemName))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.CreateName), r => r.CreateName.Contains(request.CreateName))
                                    .WhereIf(request.StartDate != null, r => r.CreateTime >= request.StartDate)
                                    .WhereIf(request.EndDate != null, r => r.CreateTime <= request.EndDate)
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.ContractStatus), r => r.ContractStatus == request.ContractStatus)).ToList();

            //通过反射将字段作为参数传入
            contractApply = (request.SortOrder == "asc" ? contractApply.OrderBy(r => r.GetType().GetProperty(request.SortName == "" || request.SortName == null ? "CreateTime" : Regex.Replace(request.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null)) : contractApply.OrderByDescending(r => r.GetType().GetProperty(request.SortName == "" || request.SortName == null ? "CreateTime" : Regex.Replace(request.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null))).ToList();

            //查询字典信息
            var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            var categoryContractList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            var categoryStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            List<string> contractId = contractApply.Select(r => r.Id).ToList();
            var contractFile = UnitWork.Find<ContractFileType>(r => contractId.Contains(r.ContractApplyId));
            var contractApplyList = contractApply.Skip((request.page - 1) * request.limit).Take(request.limit).ToList();
            var contractResp = from a in contractApplyList
                               join b in categoryCompanyList on a.CompanyType equals b.DtValue
                               join c in categoryContractList on a.ContractType equals c.DtValue
                               join d in categoryStatusList on a.ContractStatus equals d.DtValue
                               select new
                               {
                                   a,
                                   CompanyDtValue = b.DtValue,
                                   CompanyName = b.Name,
                                   ContractDtValue = c.DtValue,
                                   ContractName = c.Name,
                                   StatusDtValue = d.DtValue,
                                   StatusName = d.Name
                               };

            var contractRespList = contractResp.Select(r => new
            {
                Id = r.a.Id,
                ContractNo = r.a.ContractNo,
                CustomerCode = r.a.CustomerCode,
                CustomerName = r.a.CustomerName,
                CompanyType = r.a.CompanyType,
                ContractType = r.a.ContractType,
                QuotationNo = r.a.QuotationNo,
                SaleNo = r.a.SaleNo,
                IsDraft = r.a.IsDraft,
                IsUploadOriginal = r.a.IsUploadOriginal,
                IsUseCompanyTemplate = r.a.IsUseCompanyTemplate,
                FlowInstanceId = r.a.FlowInstanceId,
                DownloadNumber = r.a.DownloadNumber,
                U_SAP_ID = r.a.U_SAP_ID,
                ItemNo = r.a.ItemNo,
                ItemName = r.a.ItemName,
                ContractStatus = r.a.ContractStatus,
                Remark = r.a.Remark,
                CreateId = r.a.CreateId,
                CreateName = r.a.CreateName,
                DeptName = _userDepartMsgHelp.GetUserOrgName(r.a.CreateId),
                CreateTime = r.a.CreateTime,
                UpdateTime = r.a.UpdateTime,
                CompanyDtValue = r.CompanyDtValue,
                CompanyName = r.CompanyName,
                ContractDtValue = r.ContractDtValue,
                ContractName = r.ContractName,
                StatusDtValue = r.StatusDtValue,
                StatusName = r.StatusName,
                ContractFileTypeList = r.a.ContractFileTypeList,
                ContractHistoryList = r.a.contractOperationHistoryList
            }).ToList();

            result.Count = contractApply.Count();
            result.Data = contractRespList;
            return result;
        }

        /// <summary>
        /// 通过文件类型判断
        /// </summary>
        /// <param name="contractId">合同申请id</param>
        /// <returns>成功返回200，失败返回500</returns>
        public TableData ApprovalNodeJudge(string contractId)
        {
            var result = new TableData();
            var obj = UnitWork.Find<ContractApply>(r => r.Id == contractId).Include(r => r.ContractFileTypeList).FirstOrDefault();
            var categoryFileList = UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType")).Select(u => new { u.DtValue, u.Name });
            result.Code = 200;
            foreach (ContractFileType item in obj.ContractFileTypeList)
            {
                switch (item.FileType)
                {
                    case "1":
                        if (obj.CustomerCode == "")
                        {
                            result.Message = categoryFileList.Where(r => r.DtValue == "1").FirstOrDefault().Name + "客户不允许为空！";
                            result.Code = 500;
                        }
                        break;
                    case "2":
                        if (obj.CustomerCode == "")
                        {
                            result.Message = categoryFileList.Where(r => r.DtValue == "2").FirstOrDefault().Name + "客户不允许为空！";
                            result.Code = 500;
                        }
                        break;
                    case "3":
                        if (obj.CustomerCode == "")
                        {
                            result.Message = categoryFileList.Where(r => r.DtValue == "3").FirstOrDefault().Name + "客户不允许为空！";
                            result.Code = 500;
                        }
                        break;
                    case "4":
                        if (obj.CustomerCode == "")
                        {
                            result.Message = categoryFileList.Where(r => r.DtValue == "4").FirstOrDefault().Name + "客户不允许为空！";
                            result.Code = 500;
                        }
                        break;
                    case "5":
                        if (obj.CustomerCode == "")
                        {
                            result.Message = categoryFileList.Where(r => r.DtValue == "5").FirstOrDefault().Name + "客户不允许为空！";
                            result.Code = 500;
                        }
                        break;
                    case "6":
                        if (obj.CustomerCode == "" || obj.SaleNo == "")
                        {
                            result.Message = categoryFileList.Where(r => r.DtValue == "6").FirstOrDefault().Name + "客户和销售单号不允许为空！";
                            result.Code = 500;
                        }
                        break;
                    case "7":
                        if (obj.CustomerCode == "" || obj.QuotationNo == "")
                        {
                            result.Message = categoryFileList.Where(r => r.DtValue == "7").FirstOrDefault().Name + "客户和报价单号不允许为空！";
                            result.Code = 500;
                        }
                        break;
                    case "9":
                        if (obj.CustomerCode == "")
                        {
                            result.Message = categoryFileList.Where(r => r.DtValue == "9").FirstOrDefault().Name + "客户不允许为空！";
                            result.Code = 500;
                        }
                        break;
                    default:
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// 获取合同文件下载历史记录
        /// </summary>
        /// <param name="contractId">合同Id</param>
        /// <returns>返回合同文件下载历史记录</returns>
        public async Task<TableData> GetDownLoadFileHis(QueryContractDownLoadFilesReq req)
        {
            var result = new TableData();

            //查询该合同申请单文件下载记录
            var objs = (await UnitWork.Find<ContractDownLoadFileHis>(r => r.ContractApplyId == req.ContractId).ToListAsync()).OrderByDescending(r => r.CreateTime).ToList();

            //查询已下载文件Id的文件类型Id
            var contractfiles = await UnitWork.Find<ContractFile>(r => (objs.Select(x => x.FileId).ToList()).Contains(r.FileId)).Select(r => new { r.ContractFileTypeId, r.FileId }).ToListAsync();

            //查询字典中文件类型名称
            var categoryFileTypeList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractFileType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();

            //查询文件类型
            var contractFileTypes = await UnitWork.Find<ContractFileType>(r => (contractfiles.Select(x => x.ContractFileTypeId).ToList()).Contains(r.Id)).Select(r => new { r.Id, r.FileType }).ToListAsync();

            //查询Mino文件存储信息
            var contractFile = await UnitWork.Find<UploadFile>(r => (objs.Select(x => x.FileId).ToList()).Contains(r.Id)).Select(u => new { u.FileName, u.Id, u.FilePath, u.FileType, u.FileSize, u.Extension }).ToListAsync();

            //获取合同文件下载基本信息
            var contracts = from a in objs
                            join b in contractfiles on a.FileId equals b.FileId
                            join c in contractFileTypes on b.ContractFileTypeId equals c.Id
                            join d in categoryFileTypeList on c.FileType equals d.DtValue
                            select new
                            {
                                a.Id,
                                a.FileId,
                                a.CreateUserId,
                                a.CreateName,
                                a.CreateTime,
                                c.FileType,
                                d.Name
                            };

            //文件历史下载记录信息
            var contractDownLoadFileHiss = (from a in contracts
                                            join b in contractFile on a.FileId equals b.Id
                                            into ab
                                            from b in ab.DefaultIfEmpty()
                                            select new ContractDownLoadFiles
                                            {
                                                FileTypeName = a.Name,
                                                FileName = b.FileName,
                                                CreateName = a.CreateName,
                                                DeptName = _userDepartMsgHelp.GetUserOrgName(a.CreateUserId),
                                                CreateTime = (Convert.ToDateTime(a.CreateTime)).ToString("yyyy.MM.dd hh:mm:ss")
                                            }).ToList();

            contractDownLoadFileHiss = (req.SortOrder == "asc" ? contractDownLoadFileHiss.OrderBy(r => r.GetType().GetProperty(req.SortName == "" || req.SortName == null ? "CreateTime" : Regex.Replace(req.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null)) : contractDownLoadFileHiss.OrderByDescending(r => r.GetType().GetProperty(req.SortName == "" || req.SortName == null ? "CreateTime" : Regex.Replace(req.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null))).ToList();

            result.Count = contracts.Count();
            result.Data = contractDownLoadFileHiss.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
            return result;
        }
    }
}