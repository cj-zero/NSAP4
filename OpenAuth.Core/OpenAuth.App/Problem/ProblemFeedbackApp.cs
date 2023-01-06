using System;
using System.Data;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using OpenAuth.App.Response;
using OpenAuth.App.Interface;
using OpenAuth.App.Problem.Request;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.App.CommonHelp;
using Microsoft.EntityFrameworkCore;

namespace OpenAuth.App.Problem
{
    public class ProblemFeedbackApp : OnlyUnitWorkBaeApp
    {
        private IUnitWork _UnitWork;
        private IAuth _auth;
        private UserDepartMsgHelp _userDepartMsgHelp;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public ProblemFeedbackApp(IUnitWork unitWork, IAuth auth, UserDepartMsgHelp userDepartMsgHelp) : base(unitWork, auth)
        {
            _UnitWork = unitWork;
            _auth = auth;
            _userDepartMsgHelp = userDepartMsgHelp;
        }

        /// <summary>
        /// 获取问题反馈列表
        /// </summary>
        /// <param name="req">问题反馈实体</param>
        /// <returns>返回问题反馈列表信息</returns>
        public async Task<TableData> GetProblemFeedbacks(QueryProblemReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            if (loginContext.Roles.Any(r => r.Name.Equal("ERP产品经理")))
            {
                var objs = UnitWork.Find<ProblemFeedback>(null).Include(r => r.problemFeedFiles).Include(r => r.problemFiles);
                var problemFeedbacks = (objs.WhereIf(!string.IsNullOrWhiteSpace(req.Id), r => r.Id == Convert.ToInt32(req.Id.Trim()))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.MenuModel), r => r.MenuModel.Contains(req.MenuModel))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.CreateUserName), r => r.CreateUserName.Contains(req.CreateUserName))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.UpdateUserName), r => r.UpdateUserName.Contains(req.UpdateUserName))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.ProblemSugg), r => r.ProblemSugg.Contains(req.ProblemSugg))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.ProcessStatus), r => r.ProcessStatus == req.ProcessStatus)
                                            .WhereIf(req.StartDate != null, r => r.CreateTime >= req.StartDate)
                                            .WhereIf(req.EndDate != null, r => r.CreateTime <= req.EndDate)).ToList();

                //通过反射将字段作为参数传入
                problemFeedbacks = (req.SortOrder == "asc" ? problemFeedbacks.OrderBy(r => r.GetType().GetProperty(req.SortName == "" || req.SortName == null ? "CreateTime" : Regex.Replace(req.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null)) : problemFeedbacks.OrderByDescending(r => r.GetType().GetProperty(req.SortName == "" || req.SortName == null ? "CreateTime" : Regex.Replace(req.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null))).ToList();

                //查询字典信息
                var problemTypes = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ProblemType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                var problemStatus = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ProblemStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                var problemFeedbackList = problemFeedbacks.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();

                //查询上传附件信息
                List<string> fileIds = new List<string>();
                foreach (ProblemFeedback problemFeedback in problemFeedbackList)
                {
                    fileIds.AddRange(problemFeedback.problemFiles.Select(r => r.ProblemFileId));
                    fileIds.AddRange(problemFeedback.problemFeedFiles.Select(r => r.FeedbackFileId));
                }

                var problemFiles = await UnitWork.Find<UploadFile>(r => fileIds.Contains(r.Id)).Select(r => new { r.Id, r.FileName, r.Extension }).ToListAsync();

                //问题反馈信息
                var problemFeedbackResp = (from a in problemFeedbackList
                                           join b in problemTypes on a.ProblemType equals b.DtValue
                                           join c in problemStatus on a.ProcessStatus equals c.DtValue
                                           select new
                                           {
                                               Id = a.Id,
                                               ProblemType = a.ProblemType,
                                               ProblemTypeName = b.Name,
                                               MenuModel = a.MenuModel,
                                               ProblemDesc = a.ProblemDesc,
                                               ProblemSugg = a.ProblemSugg,
                                               FeedbackReply = a.FeedbackReply,
                                               ProcessStatus = a.ProcessStatus,
                                               ProcessStatusName = c.Name,
                                               CreateUserId = a.CreateUserId,
                                               CreateDeptName = _userDepartMsgHelp.GetUserOrgName(a.CreateUserId),
                                               CreateUserName = a.CreateUserName,
                                               CreateTime = a.CreateTime,
                                               UpdateUserId = a.UpdateUserId,
                                               UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(a.UpdateUserId),
                                               UpdateUserName = a.UpdateUserName,
                                               UpdateTime = a.UpdateTime,
                                               ProblemFiles = from d in a.problemFiles
                                                              join e in problemFiles on d.ProblemFileId equals e.Id into de
                                                              from e in de.DefaultIfEmpty()
                                                              select new
                                                              {
                                                                  d.Id,
                                                                  d.ProblemFeedbackId,
                                                                  d.ProblemFileId,
                                                                  FileName = e == null ? "" : e.FileName,
                                                                  Extension = e == null ? "" : e.Extension
                                                              },
                                               ProblemFeedFiles = from d in a.problemFeedFiles
                                                                  join e in problemFiles on d.FeedbackFileId equals e.Id into de
                                                                  from e in de.DefaultIfEmpty()
                                                                  select new
                                                                  {
                                                                      d.Id,
                                                                      d.ProblemFeedbackId,
                                                                      d.FeedbackFileId,
                                                                      FileName = e == null ? "" : e.FileName,
                                                                      Extension = e == null ? "" : e.Extension
                                                                  }
                                           }).OrderByDescending(r => r.CreateTime).ToList();

                result.Count = problemFeedbacks.Count();
                result.Data = problemFeedbackResp;
            }
            else
            {
                var objs = UnitWork.Find<ProblemFeedback>(r => r.CreateUserId == loginUser.Id).Include(r => r.problemFeedFiles).Include(r => r.problemFiles);
                var problemFeedbacks = (objs.WhereIf(!string.IsNullOrWhiteSpace(req.Id), r => r.Id == Convert.ToInt32(req.Id.Trim()))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.MenuModel), r => r.MenuModel.Contains(req.MenuModel))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.CreateUserName), r => r.CreateUserName.Contains(req.CreateUserName))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.UpdateUserName), r => r.UpdateUserName.Contains(req.UpdateUserName))
                                            .WhereIf(!string.IsNullOrWhiteSpace(req.ProcessStatus), r => r.ProcessStatus == req.ProcessStatus)
                                            .WhereIf(req.StartDate != null, r => r.CreateTime >= req.StartDate)
                                            .WhereIf(req.EndDate != null, r => r.CreateTime <= req.EndDate)).ToList();

                //通过反射将字段作为参数传入
                problemFeedbacks = (req.SortOrder == "asc" ? problemFeedbacks.OrderBy(r => r.GetType().GetProperty(req.SortName == "" || req.SortName == null ? "CreateTime" : Regex.Replace(req.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null)) : problemFeedbacks.OrderByDescending(r => r.GetType().GetProperty(req.SortName == "" || req.SortName == null ? "CreateTime" : Regex.Replace(req.SortName, @"^\w", t => t.Value.ToUpper())).GetValue(r, null))).ToList();

                //查询字典信息
                var problemTypes = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ProblemType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                var problemStatus = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ProblemStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
                var problemFeedbackList = problemFeedbacks.Skip((req.page - 1) * req.limit).Take(req.limit).ToList();

                //查询上传附件信息
                List<string> fileIds = new List<string>();
                foreach (ProblemFeedback problemFeedback in problemFeedbackList)
                {
                    fileIds.AddRange(problemFeedback.problemFiles.Select(r => r.ProblemFileId));
                    fileIds.AddRange(problemFeedback.problemFeedFiles.Select(r => r.FeedbackFileId));
                }

                var problemFiles = await UnitWork.Find<UploadFile>(r => fileIds.Contains(r.Id)).Select(r => new { r.Id, r.FileName, r.Extension }).ToListAsync();

                //问题反馈信息
                var problemFeedbackResp = (from a in problemFeedbackList
                                           join b in problemTypes on a.ProblemType equals b.DtValue
                                           join c in problemStatus on a.ProcessStatus equals c.DtValue
                                           select new
                                           {
                                               Id = a.Id,
                                               ProblemType = a.ProblemType,
                                               ProblemTypeName = b.Name,
                                               MenuModel = a.MenuModel,
                                               ProblemDesc = a.ProblemDesc,
                                               ProblemSugg = a.ProblemSugg,
                                               FeedbackReply = a.FeedbackReply,
                                               ProcessStatus = a.ProcessStatus,
                                               ProcessStatusName = c.Name,
                                               CreateUserId = a.CreateUserId,
                                               CreateDeptName = _userDepartMsgHelp.GetUserOrgName(a.CreateUserId),
                                               CreateUserName = a.CreateUserName,
                                               CreateTime = a.CreateTime,
                                               UpdateUserId = a.UpdateUserId,
                                               UpdateDeptName = _userDepartMsgHelp.GetUserOrgName(a.UpdateUserId),
                                               UpdateUserName = a.UpdateUserName,
                                               UpdateTime = a.UpdateTime,
                                               ProblemFiles = from d in a.problemFiles
                                                              join e in problemFiles on d.ProblemFileId equals e.Id into de
                                                              from e in de.DefaultIfEmpty()
                                                              select new
                                                              {
                                                                  d.Id,
                                                                  d.ProblemFeedbackId,
                                                                  d.ProblemFileId,
                                                                  FileName = e == null ? "" : e.FileName,
                                                                  Extension = e == null ? "" : e.Extension
                                                              },
                                               ProblemFeedFiles = from d in a.problemFeedFiles
                                                                  join e in problemFiles on d.FeedbackFileId equals e.Id into de
                                                                  from e in de.DefaultIfEmpty()
                                                                  select new
                                                                  {
                                                                      d.Id,
                                                                      d.ProblemFeedbackId,
                                                                      d.FeedbackFileId,
                                                                      FileName = e == null ? "" : e.FileName,
                                                                      Extension = e == null ? "" : e.Extension
                                                                  }
                                           }).OrderByDescending(r => r.CreateTime).ToList();

                result.Count = problemFeedbacks.Count();
                result.Data = problemFeedbackResp;
            }

            return result;
        }

        /// <summary>
        /// 添加反馈意见
        /// </summary>
        /// <param name="obj">问题反馈表实体</param>
        /// <returns>返回添加结果</returns>
        public async Task<TableData> Add(ProblemFeedback obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var dbContext = UnitWork.GetDbContext<ProblemFeedback>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (string.IsNullOrEmpty(obj.MenuModel))
                    {
                        result.Code = 500;
                        result.Message = "添加失败：所属模块不能为空！";
                        return result;
                    }

                    if (string.IsNullOrEmpty(obj.ProblemType))
                    {
                        result.Code = 500;
                        result.Message = "添加失败：问题类型不能为空！";
                        return result;
                    }

                    if (string.IsNullOrEmpty(obj.ProblemDesc))
                    {
                        result.Code = 500;
                        result.Message = "添加失败：问题描述不能为空！";
                        return result;
                    }

                    obj.CreateTime = DateTime.Now;
                    obj.CreateUserId = loginUser.Id;
                    obj.CreateUserName = loginUser.Name;
                    obj.ProcessStatus = "1";
                    obj = await UnitWork.AddAsync<ProblemFeedback, int>(obj);
                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Code = 200;
                    result.Message = "添加成功";
                }
                catch (Exception ex)
                {
                    result.Code = 500;
                    result.Message = "添加失败";
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// 修改反馈意见
        /// </summary>
        /// <param name="obj">问题反馈表实体</param>
        /// <returns>返回修改结果</returns>
        public async Task<TableData> Update(ProblemFeedback obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var loginUser = loginContext.User;
            var result = new TableData();
            var dbContext = UnitWork.GetDbContext<ProblemFeedback>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (string.IsNullOrEmpty(obj.MenuModel))
                    {
                        result.Code = 500;
                        result.Message = "修改失败：所属模块不能为空！";
                        return result;
                    }

                    if (string.IsNullOrEmpty(obj.ProcessStatus))
                    {
                        result.Code = 500;
                        result.Message = "修改失败：处理状态不能为空！";
                        return result;
                    }

                    if (string.IsNullOrEmpty(obj.ProblemType))
                    {
                        result.Code = 500;
                        result.Message = "修改失败：问题类型不能为空！";
                        return result;
                    }

                    if (string.IsNullOrEmpty(obj.ProblemDesc))
                    {
                        result.Code = 500;
                        result.Message = "修改失败：问题描述不能为空！";
                        return result;
                    }

                    #region 删除
                    var problemFiles = await UnitWork.Find<ProblemFile>(r => r.ProblemFeedbackId == obj.Id).ToListAsync();
                    if (problemFiles != null && problemFiles.Count() > 0)
                    {
                        foreach (ProblemFile item in problemFiles)
                        {
                            await UnitWork.DeleteAsync<ProblemFile>(r => r.Id == item.Id);
                        }
                    }

                    var problemFeedFiles = await UnitWork.Find<ProblemFeedFile>(r => r.ProblemFeedbackId == obj.Id).ToListAsync();
                    if (problemFeedFiles != null && problemFeedFiles.Count() > 0)
                    {
                        foreach (ProblemFeedFile item in problemFeedFiles)
                        {
                            await UnitWork.DeleteAsync<ProblemFeedFile>(r => r.Id == item.Id);
                        }
                    }

                    await UnitWork.SaveAsync();
                    #endregion

                    #region 新增
                    if (obj.problemFiles.Count() > 0)
                    {
                        List<ProblemFile> problemFileList = new List<ProblemFile>();
                        foreach (ProblemFile fileItem in obj.problemFiles)
                        {
                            fileItem.ProblemFeedbackId = obj.Id;
                            problemFileList.Add(fileItem);
                        }

                        if (problemFileList != null && problemFileList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<ProblemFile, int>(problemFileList.ToArray());
                        }
                    }

                    if (obj.problemFeedFiles.Count() > 0)
                    {
                        List<ProblemFeedFile> problemFeedFileList = new List<ProblemFeedFile>();
                        foreach (ProblemFeedFile feedItem in obj.problemFeedFiles)
                        {
                            feedItem.ProblemFeedbackId = obj.Id;
                            problemFeedFileList.Add(feedItem);
                        }

                        if (problemFeedFileList != null && problemFeedFileList.Count > 0)
                        {
                            await UnitWork.BatchAddAsync<ProblemFeedFile, int>(problemFeedFileList.ToArray());
                        }
                    }

                    //清空旧数据
                    obj.problemFeedFiles.Clear();
                    obj.problemFiles.Clear();
                    await UnitWork.SaveAsync();
                    #endregion

                    //修改主表数据
                    await UnitWork.UpdateAsync<ProblemFeedback>(u => u.Id == obj.Id, u => new ProblemFeedback()
                    {
                        ProblemType = obj.ProblemType,
                        MenuModel = obj.MenuModel,
                        ProblemDesc = obj.ProblemDesc,
                        ProblemSugg = obj.ProblemSugg,
                        FeedbackReply = obj.FeedbackReply,
                        ProcessStatus = obj.ProcessStatus,
                        CreateUserId = obj.CreateUserId,
                        CreateUserName = obj.CreateUserName,
                        CreateTime = obj.CreateTime,
                        UpdateUserName = loginUser.Name,
                        UpdateUserId = loginUser.Id,
                        UpdateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                    await transaction.CommitAsync();
                    result.Code = 200;
                    result.Message = "修改成功";
                }
                catch (Exception ex)
                {
                    result.Code = 500;
                    result.Message = "修改失败";
                    await transaction.RollbackAsync();
                    throw new Exception(ex.Message.ToString());
                }
            }

            return result;
        }

        /// <summary>
        /// 获取问题反馈状态
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetProblemStatusList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var categorStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ProblemStatus")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            result.Data = categorStatusList;
            return result;
        }

        /// <summary>
        /// 获取问题反馈类型
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetProblemTypeList()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var categorStatusList = await UnitWork.Find<Category>(u => u.TypeId.Equals("SYS_ProblemType")).Select(u => new { u.DtValue, u.Name }).ToListAsync();
            result.Data = categorStatusList;
            return result;
        }
    }
}
