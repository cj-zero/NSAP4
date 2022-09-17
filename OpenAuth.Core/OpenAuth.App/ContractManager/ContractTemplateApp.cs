using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using NStandard;
using OpenAuth.App.Files;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App.ContractManager
{
    public class ContractTemplateApp : OnlyUnitWorkBaeApp
    {
        private IFileStore _fileStore;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public ContractTemplateApp(IUnitWork unitWork, IAuth auth, IFileStore fileStore) : base(unitWork, auth)
        {
            _fileStore = fileStore;
        }

        /// <summary>
        /// 获取合同模板列表
        /// </summary>
        /// <param name="request">合同模板查询实体数据</param>
        /// <returns>返回合同模板列表信息</returns>
        public async Task<TableData> GetContractTemplateList(QueryContractTemplateListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var objs = UnitWork.Find<ContractTemplate>(null);
            var contractTemplate = objs.WhereIf(!string.IsNullOrWhiteSpace(request.TemplateNo), r => r.TemplateNo.Contains(request.TemplateNo))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserId), r => r.CreateUserId.Contains(request.CreateUserId))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.TemplateRemark), r => r.TemplateRemark.Contains(request.TemplateRemark))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompanyType), r => r.CompanyType.Equals(request.CompanyType))
                                    .WhereIf(request.StartDate != null, r => r.CreateTime >= request.StartDate)
                                    .WhereIf(request.EndDate != null, r => r.CreateTime <= request.EndDate);

            contractTemplate = contractTemplate.OrderByDescending(r => r.CreateTime);
            var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            var contractTemplateList = await contractTemplate.Skip((request.page - 1) * request.limit).Take(request.limit).ToListAsync();
            List<string> fileids = contractTemplate.Select(r => r.TemplateFileId).ToList();
            var contractFileList = await UnitWork.Find<UploadFile>(r => fileids.Contains(r.Id)).Select(u => new { u.FileName, u.Id, u.FilePath, }).ToListAsync();
            var contractTemplateListReq = from a in contractTemplateList
                                          join b in categoryCompanyList on a.CompanyType equals b.DtValue
                                          join c in contractFileList on a.TemplateFileId equals c.Id into ac
                                          from c in ac.DefaultIfEmpty()
                                          select new
                                          {
                                              a.Id,
                                              a.CompanyType,
                                              a.TemplateNo,
                                              a.TemplateRemark,
                                              a.TemplateFileId,
                                              a.CreateUserId,
                                              a.CreateTime,
                                              a.UpdateTime,
                                              a.UpdateUserId,
                                              a.DownLoadNum,
                                              CompanyValue = b.DtValue,
                                              CompanyName = b.Name,
                                              FileName = c == null ? "" : c.FileName,
                                              FileId = c == null ? "" : c.Id,
                                              FilePath = c == null ? "" : c.FilePath
                                           };

            result.Count = await contractTemplate.CountAsync();
            result.Data = contractTemplateListReq;
            return result;
        }

        /// <summary>
        /// 获取合同模板详情信息
        /// </summary>
        /// <param name="templateId">合同模板Id</param>
        /// <returns>返回合同模板详情信息</returns>
        public async Task<TableData> GetContractTemplateDetail(string templateId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var contractTemplate = UnitWork.Find<ContractTemplate>(r => r.Id == templateId).ToList();
            if (contractTemplate != null)
            {
                var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                var contractFileList = await UnitWork.Find<UploadFile>(u => u.Id.Equals(contractTemplate.FirstOrDefault().TemplateFileId) && u.Enable == false).Select(u => new { u.FileName, u.Id, u.FilePath, }).ToListAsync();
                var contractTempalteReq = from a in contractTemplate
                                          join b in categoryCompanyList on a.CompanyType equals b.DtValue
                                          join c in contractFileList on a.TemplateFileId equals c.Id into ac
                                          from c in ac.DefaultIfEmpty()
                                          select new
                                         {
                                             a.Id,
                                             a.CompanyType,
                                             a.TemplateNo,
                                             a.TemplateRemark,
                                             a.TemplateFileId,
                                             a.CreateUserId,
                                             a.CreateTime,
                                             a.UpdateUserId,
                                             a.UpdateTime,
                                             a.DownLoadNum,
                                             CompanyValue = b.DtValue,
                                             CompanyName = b.Name,
                                             FileId = c == null ? "" : c.Id,
                                             FileName = c == null ? "" : c.FileName,
                                             FilePath = c == null ? "" : c.FilePath
                                         };

                result.Data = contractTempalteReq;
            }
            else
            {
                result.Message = "该模板不存在";
            }

            return result;
        }

        /// <summary>
        /// 新增模板
        /// </summary>
        /// <param name="obj">合同模板实体数据</param>
        /// <returns>成功返回操作成功/returns>
        public async Task<string> AddTemplate(ContractTemplate obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var dbContext = UnitWork.GetDbContext<ContractTemplate>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    obj.Id = Guid.NewGuid().ToString();
                    obj.CreateTime = DateTime.Now;
                    obj.CreateUserId = loginUser.Name;
                    obj.UpdateTime = null;

                    //模板编号唯一
                    var maxTemplateNo = UnitWork.Find<ContractTemplate>(null).OrderByDescending(r => r.TemplateNo).Select(r => r.TemplateNo).FirstOrDefault();
                    if (!string.IsNullOrEmpty(maxTemplateNo))
                    {
                        maxTemplateNo = "HTMB-" + (Convert.ToInt32(maxTemplateNo.Split('-')[1]) + 1).ToString().PadLeft(3, '0');
                    }
                    else
                    {
                        maxTemplateNo = "HTMB-" + "001";
                    }

                    obj.TemplateNo = maxTemplateNo;
                    obj = await UnitWork.AddAsync<ContractTemplate, int>(obj);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("创建模板失败,请重试");
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 修改模板
        /// </summary>
        /// <param name="obj">合同模板实体数据</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> UpDateTemplate(ContractTemplate obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var dbContext = UnitWork.GetDbContext<ContractTemplate>();
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    //修改模板信息
                    await UnitWork.UpdateAsync<ContractTemplate>(r => r.Id == obj.Id, r => new ContractTemplate
                    {
                        UpdateTime = DateTime.Now,
                        UpdateUserId = loginContext.User.Name,
                        TemplateNo = obj.TemplateNo,
                        TemplateFileId = obj.TemplateFileId,
                        TemplateRemark = obj.TemplateRemark,
                        DownLoadNum = obj.DownLoadNum,
                        CreateUserId = obj.CreateUserId,                       
                        CreateTime = obj.CreateTime
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("修改模板信息失败,请重试。");
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="templateId">合同模板Id</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> DeleteTemplate(string templateId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var contractSeal = UnitWork.Find<ContractTemplate>(r => r.Id == templateId);
            if (contractSeal != null && contractSeal.Count() > 0)
            {
                await UnitWork.DeleteAsync<ContractTemplate>(q => q.Id == templateId);
                await UnitWork.SaveAsync();
                return "操作成功";
            }
            else
            {
                throw new Exception("删除失败，该合同模板不存在。");
            }
        }

        /// <summary>
        /// 批量删除合同
        /// </summary>
        /// <param name="templateIdList">合同模板Id集合</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> DeleteTemplates(List<string> templateIdList)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var contractTemplate = UnitWork.Find<ContractTemplate>(r => templateIdList.Contains(r.Id)).ToList();
            if (contractTemplate != null && contractTemplate.Count() > 0)
            {
                await UnitWork.BatchDeleteAsync<ContractTemplate>(contractTemplate.ToArray());
                await UnitWork.SaveAsync();
                return "操作成功";
            }
            else
            {
                throw new Exception("删除失败，该合同模板不存在。");
            }
        }

        /// <summary>
        /// 更改文件下载次数
        /// </summary>
        /// <param name="contractId"></param>
        /// <returns>成功返回true，失败返回false</returns>
        public async Task<bool> GetDownloadNum(string contractId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var objs = await UnitWork.Find<ContractTemplate>(r => r.Id == contractId).FirstOrDefaultAsync();
            try
            {
                string sql = string.Format("UPDATE erp4_serve.contracttemplate SET DownLoadNum={0},UpdateUserId='{1}',UpdateTime='{2}' WHERE Id= '" + contractId + "'", objs.DownLoadNum + 1,loginUser.Name,DateTime.Now);
                int resultCountJob = UnitWork.ExecuteSql(sql, ContextType.NsapBaseDbContext);
                if (resultCountJob > 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                ex.ToString();
            }

            return false;
        }
    }
}