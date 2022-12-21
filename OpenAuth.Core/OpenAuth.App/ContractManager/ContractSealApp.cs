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
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Order;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.App.Order.Request;
using OpenAuth.App.CommonHelp;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App.ContractManager
{
    public class ContractSealApp : OnlyUnitWorkBaeApp
    {
        private UserDepartMsgHelp _userDepartMsgHelp;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public ContractSealApp(UserDepartMsgHelp userDepartMsgHelp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _userDepartMsgHelp = userDepartMsgHelp;
        }

        /// <summary>
        /// 获取印章列表
        /// </summary>
        /// <param name="request">查询印章实体数据</param>
        /// <returns>返回印章列表信息</returns>
        public async Task<TableData> GetContractSealList(QueryContractSealListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var objs = UnitWork.Find<ContractSeal>(null).Include(r => r.contractSealOperationHistoryList);
            var contractSeal = objs.WhereIf(!string.IsNullOrWhiteSpace(request.SealNo), r => r.SealNo.Contains(request.SealNo))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUserId), r => r.CreateUserId.Contains(request.CreateUserId))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.Remark), r => r.Remark.Contains(request.Remark))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.CompanyType), r => r.CompanyType.Equals(request.CompanyType))
                                    .WhereIf(!string.IsNullOrWhiteSpace(request.SealType), r => r.SealType.Equals(request.SealType))
                                    .WhereIf(request.StartDate != null, r => r.CreateTime >= request.StartDate)
                                    .WhereIf(request.EndDate != null, r => r.CreateTime <= request.EndDate);

            contractSeal = contractSeal.OrderByDescending(r => r.CreateTime);
            var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            var categorySealTypeList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractSealType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            List<string> fileids = contractSeal.Select(r => r.SealImageFileId).ToList();
            var contractFileList = await UnitWork.Find<UploadFile>(r => fileids.Contains(r.Id)).Select(u => new { u.FileName, u.Id, u.FilePath, u.FileType, u.FileSize, u.Extension, u.Enable, u.SortCode }).ToListAsync();
            var contractSealList = await contractSeal.Skip((request.page - 1) * request.limit)
              .Take(request.limit).ToListAsync();
            var contractSealReq = from a in contractSealList
                                  join b in categoryCompanyList on a.CompanyType equals b.DtValue
                                  join c in categorySealTypeList on a.SealType equals c.DtValue
                                  join d in contractFileList on a.SealImageFileId equals d.Id into ad
                                  from d in ad.DefaultIfEmpty()
                                  select new
                                  {
                                      a.Id,
                                      a.SealNo,
                                      a.CompanyType,
                                      a.SealType,
                                      a.SealName,
                                      a.SealImageFileId,
                                      a.IsEnable,
                                      a.CreateUserId,
                                      CreateUserName = a.CreateUserName,
                                      CreateDeptName = _userDepartMsgHelp.GetUserOrgName(a.CreateUserId),
                                      a.CreateTime,
                                      a.UpdateUserId,
                                      UpdateUserName = a.UpdateUserName,
                                      UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(a.UpdateUserId),
                                      a.UpdateTime,
                                      a.Remark,
                                      LastUserName = a.contractSealOperationHistoryList.Count() == 0 ? "" : a.contractSealOperationHistoryList.OrderByDescending(r => r.CreateTime).FirstOrDefault().CreateUserId,
                                      LastUseTime = a.contractSealOperationHistoryList.Count() == 0 ? "" : a.contractSealOperationHistoryList.OrderByDescending(r => r.CreateTime).FirstOrDefault().CreateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                                      SealUsedNum = a.contractSealOperationHistoryList.Count(),
                                      CompanyValue = b.DtValue,
                                      CompanyName = b.Name,
                                      SealTypeValue = c.DtValue,
                                      SealTypeName = c.Name,
                                      FileName = d == null ? "" : d.FileName,
                                      FilePath = d == null ? "" : d.FilePath,
                                      FileType = d == null ? "" : d.FileType,
                                      Extension = d == null ? "" : d.Extension,
                                      Enable = d == null ? false : d.Enable,
                                      SortCode = d == null ? 0 : d.SortCode
                                  };

            result.Count = await contractSeal.CountAsync();
            result.Data = contractSealReq;
            return result;
        }

        /// <summary>
        /// 获取印章详情
        /// </summary>
        /// <param name="sealId">印章Id</param>
        /// <returns>返回印章列表信息</returns>
        public async Task<TableData> GetContractSealDetail(string sealId)
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
                var contractSealModel = UnitWork.Find<ContractSeal>(r => r.Id == sealId).ToList();
                if (contractSealModel != null)
                {
                    var categoryCompanyList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractCompany")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var categorySealTypeList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractSealType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                    var contractFileList = await UnitWork.Find<UploadFile>(r => r.Id == contractSealModel.FirstOrDefault().SealImageFileId).Select(u => new { u.FileName, u.Id, u.FilePath, u.FileType, u.FileSize, u.Extension, u.Enable, u.SortCode }).ToListAsync();
                    var contractSealReq = from a in contractSealModel
                                          join b in categoryCompanyList on a.CompanyType equals b.DtValue
                                          join c in categorySealTypeList on a.SealType equals c.DtValue
                                          join d in contractFileList on a.SealImageFileId equals d.Id into ad
                                          from d in ad.DefaultIfEmpty()
                                          select new
                                          {
                                              a.Id,
                                              a.SealNo,
                                              a.CompanyType,
                                              a.SealType,
                                              a.SealName,
                                              a.SealImageFileId,
                                              a.IsEnable,
                                              a.CreateUserId,
                                              CreateUserName = a.CreateUserName,
                                              CreateDeptName = _userDepartMsgHelp.GetUserOrgName(a.CreateUserId),
                                              a.UpdateUserId,
                                              UpdateUserName = a.UpdateUserName,
                                              UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(a.UpdateUserId),
                                              a.CreateTime,
                                              a.UpdateTime,
                                              a.Remark,
                                              CompanyValue = b.DtValue,
                                              CompanyName = b.Name,
                                              SealTypeValue = c.DtValue,
                                              SealTypeName = c.Name,
                                              FileName = d == null ? "" : d.FileName,
                                              FilePath = d == null ? "" : d.FilePath,
                                              FileType = d == null ? "" : d.FileType,
                                              Extension = d == null ? "" : d.Extension,
                                              Enable = d == null ? false : d.Enable,
                                              SortCode = d == null ? 0 : d.SortCode
                                          };

                    result.Data = new
                    {
                        contractSeal = contractSealReq
                    };
                }
                else
                {
                    result.Message = "该印章不存在";
                }
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 查询印章操作历史记录
        /// </summary>
        /// <param name="req">印章操作历史记录实体</param>
        /// <returns>返回印章操作记录</returns>
        public async Task<TableData> GetSealOperationHistory(QuerySealHistoryReq req)
        {
            var result = new TableData();
            if (!string.IsNullOrEmpty(req.SealId))
            {
                //查询印章历史记录
                var contractSealOperationHistoryList = await UnitWork.Find<ContractSealOperationHistory>(r => r.ContractSealId == req.SealId).OrderByDescending(r => r.CreateTime).ToListAsync();

                //按照创建时间倒序排序
                contractSealOperationHistoryList = contractSealOperationHistoryList.OrderByDescending(r => r.CreateTime).ToList();
                result.Count = contractSealOperationHistoryList.Count();
                contractSealOperationHistoryList = contractSealOperationHistoryList.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();
                var contractSealOperationHistorys = contractSealOperationHistoryList.Select(r => new
                {
                    r.Id,
                    r.ContractSealId,
                    r.ContractNo,
                    r.ContractFinalNum,
                    r.CreateUserId,
                    r.CreateTime,
                    r.CreateUserName,
                    CreateDeptName = _userDepartMsgHelp.GetUserOrgName(r.CreateUserId),
                    r.OperationType
                });

                result.Data = contractSealOperationHistorys;
            }

            return result;
        }

        /// <summary>
        /// 新增印章
        /// </summary>
        /// <param name="obj">印章实体数据</param>
        /// <returns>成功返回操作成功/returns>
        public async Task<string> AddSeal(ContractSeal obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var dbContext = UnitWork.GetDbContext<ContractSeal>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    obj.Id = Guid.NewGuid().ToString();
                    obj.CreateTime = DateTime.Now;
                    obj.CreateUserId = loginUser.Id;
                    obj.CreateUserName = loginUser.Name;
                    obj.UpdateUserId = null;
                    obj.UpdateUserName = null;
                    obj.UpdateTime = null;

                    //印章编号唯一
                    var maxSealNo = UnitWork.Find<ContractSeal>(null).OrderByDescending(r => r.SealNo).Select(r => r.SealNo).FirstOrDefault();
                    if (!string.IsNullOrEmpty(maxSealNo))
                    {
                        maxSealNo = "YZ-" + (Convert.ToInt32(maxSealNo.Split('-')[1]) + 1).ToString().PadLeft(4, '0');
                    }
                    else
                    {
                        maxSealNo = "YZ-" + "0001";
                    }

                    obj.SealNo = maxSealNo;
                    obj = await UnitWork.AddAsync<ContractSeal, int>(obj);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("创建印章失败,请重试");
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 修改印章
        /// </summary>
        /// <param name="obj">印章实体数据</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> UpDateSeal(ContractSeal obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var dbContext = UnitWork.GetDbContext<ContractSeal>();
            using (var transaction = dbContext.Database.BeginTransactionAsync().ConfigureAwait(false).GetAwaiter().GetResult())
            {
                try
                {
                    //修改印章信息
                    await UnitWork.UpdateAsync<ContractSeal>(r => r.Id == obj.Id, r => new ContractSeal
                    {
                        UpdateTime = DateTime.Now,
                        UpdateUserId = loginContext.User.Id,
                        UpdateUserName = loginContext.User.Name,
                        SealNo = obj.SealNo,
                        SealName = obj.SealName,
                        CompanyType = obj.CompanyType,
                        SealType = obj.SealType,
                        SealImageFileId = obj.SealImageFileId,
                        IsEnable = obj.IsEnable,
                        Remark = obj.Remark,
                        CreateUserId = obj.CreateUserId,
                        CreateUserName = obj.CreateUserName,
                        CreateTime = obj.CreateTime
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("修改印章信息失败,请重试。");
                }
            }

            return "操作成功";
        }

        /// <summary>
        /// 删除印章
        /// </summary>
        /// <param name="sealId">印章Id</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> DeleteSeal(string sealId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var contractFile = UnitWork.Find<ContractFileType>(r => r.ContractSealId == sealId);
            if (contractFile != null && contractFile.Count() > 0)
            {
                throw new Exception("删除失败，该印章已经被使用。");
            }
            else
            {
                var loginUser = loginContext.User;
                StringBuilder remark = new StringBuilder();
                var contractSeal = UnitWork.Find<ContractSeal>(r => r.Id == sealId);
                if (contractSeal != null && contractSeal.Count() > 0)
                {
                    await UnitWork.DeleteAsync<ContractSeal>(q => q.Id == sealId);
                    remark.Append("删除编号为：" + contractSeal.FirstOrDefault().SealNo + " 的印章！");
                    await UnitWork.SaveAsync();
                    return "操作成功";
                }
                else
                {
                    throw new Exception("删除失败，该印章不存在。");
                }
            }
        }

        /// <summary>
        /// 批量删除印章
        /// </summary>
        /// <param name="sealIdList">印章Id集合</param>
        /// <returns>成功返回操作成功</returns>
        public async Task<string> DeleteSeals(List<string> sealIdList)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var contractFile = UnitWork.Find<ContractFileType>(r => sealIdList.Contains(r.ContractSealId));
            if (contractFile != null && contractFile.Count() > 0)
            {
                throw new Exception("删除失败，印章已经被使用。");
            }
            else
            {
                var contractSeal = UnitWork.Find<ContractSeal>(r => sealIdList.Contains(r.Id)).ToList();
                if (contractSeal != null && contractSeal.Count() > 0)
                {
                    await UnitWork.BatchDeleteAsync<ContractSeal>(contractSeal.ToArray());
                    await UnitWork.SaveAsync();
                    return "操作成功";
                }
                else
                {
                    throw new Exception("删除失败，该印章不存在。");
                }
            }
        }

        /// <summary>
        /// 获取所有印章类型
        /// </summary>
        /// <returns>返回印章信息</returns>
        public async Task<TableData> GetContractSealTypeList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var categorSealTypeList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ContractSealType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            result.Count = categorSealTypeList.Count();
            result.Data = categorSealTypeList;
            return result;
        }
    }
}