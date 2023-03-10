using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.Repository.Domain.Hr;
using OpenAuth.App.Hr.Request;

namespace OpenAuth.App.Hr
{
    public class SubjectCourseApp : OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 用于事务操作
        /// </summary>
        /// <value>The unit work.</value>
        protected IUnitWork UnitWork;

        protected IAuth _auth;

        /// <summary>
        /// 必修课模块
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public SubjectCourseApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            UnitWork = unitWork;
            _auth = auth;
        }

        #region ERP
        /// <summary>
        /// 专题列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetSubjectListByErp(GetSubjectListByErpReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<classroom_subject>(null)
                                     .WhereIf(!string.IsNullOrWhiteSpace(req.Name), a => a.Name.Contains(req.Name))
                                     .WhereIf(!string.IsNullOrWhiteSpace(req.CreateUser), a => a.CreateUser.Contains(req.CreateUser))
                                     .WhereIf(req.StartTime != null, c => c.CreateTime >= req.StartTime)
                                     .WhereIf(req.EndTime != null, c => c.CreateTime <= req.EndTime)
                                     .WhereIf(req.State != null, c => c.State == req.State)
                                     select a;
            result.Count = await query.CountAsync();
            result.Data = await query.OrderBy(a=> a.Sort).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize).ToListAsync();
            return result;
        }

        /// <summary>
        /// 新增/修改专题
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AddOrModifySubjectByErp(AddOrEditSubjectReq req)
        {
            var result = new TableData();
            var subjectPack = await UnitWork.Find<classroom_subject>(null).Where(c => c.Name == req.Name).FirstOrDefaultAsync();
         
            var user = _auth.GetCurrentUser().User;
            if (req.Id > 0)
            {
                if (subjectPack != null && subjectPack.Id != req.Id )
                {
                    result.Code = 500;
                    result.Message = "专题名称已存在!";
                    return result;
                }

                var subject = await UnitWork.Find<classroom_subject>(null).Where(c => c.Id == req.Id).FirstOrDefaultAsync();
                if (subject != null)
                {
                    subject.Name = req.Name;
                    subject.State = req.State;
                    await UnitWork.UpdateAsync(subject);
                }
                await UnitWork.SaveAsync();
            }
            else
            {
                if (subjectPack != null)
                {
                    result.Code = 500;
                    result.Message = "专题名称已存在!";
                    return result;
                }


                int? sort = await UnitWork.Find<classroom_subject>(null).MaxAsync(a => (int?)a.Sort);
                if (sort == null)
                    sort = 0;
                classroom_subject info = new classroom_subject()
                {
                    Name = req.Name,
                    ViewNumbers = 0,
                    State = req.State,
                    CreateTime = DateTime.Now,
                    Sort = (int)sort + 1,
                    CreateUser = user.Name,
                };
                await UnitWork.AddAsync<classroom_subject, int>(info);
                await UnitWork.SaveAsync();
            }
            return result;
        }

        /// <summary>
        /// 删除专题
        /// </summary>
        /// <param name="subjectId">主题id</param>
        /// <returns></returns>
        public async Task<TableData> DelSubjectByErp(int subjectId)
        {
            var result = new TableData();

            var courseCount = await UnitWork.Find<classroom_subject_course>(null).CountAsync(zw => zw.SubjectId == subjectId);
            if(courseCount > 0)
            {
                result.Code = 500;
                result.Message = "专题下已存在课程，不允许删除!";
                return result;
            }

            var model = await UnitWork.Find<classroom_subject>(null).FirstOrDefaultAsync(c => c.Id == subjectId);
            await UnitWork.DeleteAsync(model);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 调整专题排序
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AdjustSubjectByErp(int oldId, int newId)
        {
            var result = new TableData();
            var oldInfo = await UnitWork.Find<classroom_subject>(null).FirstOrDefaultAsync(c => c.Id == oldId);
            var newInfo = await UnitWork.Find<classroom_subject>(null).FirstOrDefaultAsync(c => c.Id == newId);
            int sort = oldInfo.Sort;
            oldInfo.Sort = newInfo.Sort;
            newInfo.Sort = sort;
            await UnitWork.UpdateAsync(oldInfo);
            await UnitWork.UpdateAsync(newInfo);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 专题课程列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetSubjectCourseListByErp(GetSubjectCourseListByErpReq req)
        {
            var result = new TableData();
            var query =  (from a in UnitWork.Find<classroom_subject_course>(null)
                                     .WhereIf(req.State != null, c => c.State == req.State)
                                     .WhereIf(!string.IsNullOrWhiteSpace(req.Name), a => a.Name.Contains(req.Name))
                                     .WhereIf(!string.IsNullOrWhiteSpace(req.CreateUser), a => a.CreateUser.Contains(req.CreateUser))
                                     .WhereIf(req.StartTime != null, c => c.CreateTime >= req.StartTime)
                                     .WhereIf(req.EndTime != null, c => c.CreateTime <= req.EndTime)
                                     .Where(a => a.SubjectId == req.subjectId)
                                     select a);
            result.Count = await query.CountAsync();
            result.Data = await query.OrderBy(a => a.Sort).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize).ToListAsync();
            return result;
        }

        /// <summary>
        /// 新增/修改专题课程
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AddOrModifySubjectCourseByErp(AddOrEditSubjectCourseReq req)
        {
            var result = new TableData();
            var subjectPack = await UnitWork.Find<classroom_subject_course>(null).Where(c => c.Name == req.Name).FirstOrDefaultAsync();
        
            var user = _auth.GetCurrentUser().User;
            if (req.Id > 0)
            {
                if (subjectPack != null && subjectPack.Id != req.Id)
                {
                    result.Code = 500;
                    result.Message = "专题课程名称已存在!";
                    return result;
                }

                var subject = await UnitWork.Find<classroom_subject_course>(null).Where(c => c.Id == req.Id).FirstOrDefaultAsync();
                if (subject != null)
                {
                    subject.State = req.State;
                    subject.Name = req.Name;
                    subject.Type = req.Type;
                    subject.Content = req.Content;
                    if(req.State == 1 && !subject.ShelfTime.HasValue)
                    {
                        subject.ShelfTime = DateTime.Now;
                    }
                    await UnitWork.UpdateAsync(subject);
                    await UnitWork.SaveAsync();
                }
              
            }
            else
            {
                if (subjectPack != null)
                {
                    result.Code = 500;
                    result.Message = "专题课程名称已存在!";
                    return result;
                }

                int? sort = await UnitWork.Find<classroom_subject_course>(null).Where(a => a.SubjectId == req.SubjectId).MaxAsync(a => (int?)a.Sort);
                if (sort == null)
                    sort = 0;
                classroom_subject_course info = new classroom_subject_course()
                {
                    SubjectId = req.SubjectId,
                    Name = req.Name,
                    Type = req.Type,
                    Sort = (int)sort + 1,
                    Content = req.Content,
                    State = req.State,
                    ViewNumbers = 0, 
                    CreateTime = DateTime.Now,
                    CreateUser = user.Name,
                };
                if(req.State == 1)
                {
                    info.ShelfTime = DateTime.Now;
                }
                else
                {
                    info.ShelfTime = null;
                }
                await UnitWork.AddAsync<classroom_subject_course, int>(info);
                await UnitWork.SaveAsync();
            }
            return result;
        }

        /// <summary>
        /// 删除专题课程
        /// </summary>
        /// <param name="Id">Id</param>
        /// <returns></returns>
        public async Task<TableData> DelSubjectCourseByErp(int Id)
        {
            var result = new TableData();

            var model = await UnitWork.Find<classroom_subject_course>(null).FirstOrDefaultAsync(c => c.Id == Id);
            await UnitWork.DeleteAsync(model);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 调整专题课程排序
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> AdjustSubjectCourseByErp(int oldId, int newId)
        {
            var result = new TableData();
            var oldInfo = await UnitWork.Find<classroom_subject_course>(null).FirstOrDefaultAsync(c => c.Id == oldId);
            var newInfo = await UnitWork.Find<classroom_subject_course>(null).FirstOrDefaultAsync(c => c.Id == newId);
            int sort = oldInfo.Sort;
            oldInfo.Sort = newInfo.Sort;
            newInfo.Sort = sort;
            await UnitWork.UpdateAsync(oldInfo);
            await UnitWork.UpdateAsync(newInfo);
            await UnitWork.SaveAsync();
            return result;
        }
        #endregion




        #region App
        /// <summary>
        /// 专题列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="name"></param>
        /// <returns></returns>`
        public async Task<TableData> ClassroomSubjectList(int appUserId, string name)
        {
            var result = new TableData();
            List<classroom_subject_dto> obj = new List<classroom_subject_dto>();

            var subjectList = await (from a in UnitWork.Find<classroom_subject>(null)
                                     .WhereIf(!string.IsNullOrWhiteSpace(name), a => a.Name.Contains(name))
                                     .Where(a => a.State == 1)
                                     select a).ToListAsync();

            var courseList = await (from a in UnitWork.Find<classroom_subject_course>(null)
                                    .Where(a => a.State == 1)
                                    select a).ToListAsync();

            var userProgress = await (from a in UnitWork.Find<classroom_subject_course_user>(null)
                                      .Where(a => a.AppUserId == appUserId)
                                      select a).ToListAsync();

            foreach (var item in subjectList)
            {
                var courseCount = courseList.Where(a => a.SubjectId == item.Id).Count();

                var courseIdList = courseList.Select(a => a.Id).ToList();
                var userProgressCount = userProgress.Where(a => a.SubjectId == item.Id && a.IsComplete == true && courseIdList.Contains(a.SubjectCourseId)).Count();

                classroom_subject_dto info = new classroom_subject_dto();
                info.Id = item.Id;
                info.ViewNumbers = item.ViewNumbers;
                info.Name = item.Name;
                info.State = item.State;
                info.CreateTime = item.CreateTime;
                info.Sort = item.Sort;
                info.CreateUser = item.CreateUser;
                if (courseCount == 0)
                {
                    info.Schedule =0;
                    info.IsComplete = false;
                }
                else
                {
                    if (courseCount == userProgressCount)
                    {
                        info.Schedule = 100;
                        info.IsComplete = true;
                    }
                    else
                    {
                        info.Schedule = userProgressCount * 100 / courseCount;
                        info.IsComplete = false;
                    }
                }
                obj.Add(info);
            }

            List<classroom_subject_dto> subList1 = obj.Where(a => a.IsComplete == true).OrderBy(a => a.Sort).ToList();
            List<classroom_subject_dto> subList2 = obj.Where(a => a.IsComplete == false && a.Schedule > 0).OrderByDescending(a => a.Schedule).ToList();
            List<classroom_subject_dto> subList3 = obj.Where(a => a.IsComplete == false && a.Schedule == 0).OrderBy(a => a.Sort).ToList();
            obj.Clear();
            obj.AddRange(subList2);
            obj.AddRange(subList3);
            obj.AddRange(subList1);


            result.Data = obj;
            result.Count = obj.Count();
            return result;
        }

        /// <summary>
        /// 专题课程列表
        /// </summary>
        /// <param name="appUserId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<TableData> ClassroomSubjectCourseList(int appUserId,int subjectId,string name )
        {
            var result = new TableData();

            var courseList = await (from a in UnitWork.Find<classroom_subject_course>(null)
                                      .WhereIf(!string.IsNullOrWhiteSpace(name), a => a.Name.Contains(name))
                                     .Where(a => a.SubjectId == subjectId && a.State ==1)
                                    select a).ToListAsync();

            var userProgress = await (from a in UnitWork.Find<classroom_subject_course_user>(null)
                                      .Where(a => a.AppUserId == appUserId )
                                      select a).ToListAsync();

            var results = from c in courseList
                          join u in userProgress on c.Id equals u.SubjectCourseId into scs
                          from sc in scs.DefaultIfEmpty()
                          select new classroom_subject_course_dto
                          {
                              Id =c.Id,
                              Name = c.Name,
                              SubjectId = c.SubjectId,
                              ShelfTime = c.ShelfTime,
                              Type = c.Type,
                              Content = c.Content,
                              CreateTime = c.CreateTime,
                              CreateUser = c.CreateUser,
                              IsComplete = sc?.IsComplete,
                              ViewNumbers =c.ViewNumbers,
                              Sort =c.Sort,
                          };
            result.Data = results.OrderBy(a => a.Sort).ToList();
            result.Count = results.Count();
            return result;
        }

        /// <summary>
        /// 修改课程观看记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpdateCourseRecord(SubjectCourseRecordReq req)
        {
            var result = new TableData();
            try
            {
                var query = await (from a in UnitWork.Find<classroom_subject_course_user>(null)
                                    .Where(a => a.AppUserId == req.AppUserId && a.SubjectCourseId == req.SubjectCourseId)
                                   select a).FirstOrDefaultAsync();
                if (query == null)
                {
                    classroom_subject_course_user info = new classroom_subject_course_user();
                    info.AppUserId = req.AppUserId;
                    info.SubjectCourseId = req.SubjectCourseId;
                    info.SubjectId = req.SubjectId;
                    info.CreateTime = DateTime.Now;
                    info.IsComplete = req.IsComplete;
                    var exam = await UnitWork.AddAsync<classroom_subject_course_user, int>(info);
                    await UnitWork.SaveAsync();
                }
                else
                {
                    query.IsComplete = req.IsComplete;
                    await UnitWork.UpdateAsync(query);
                    await UnitWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                throw;
            }
            return result;
        }

        /// <summary>
        /// 修改专题观看记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpdateSubjectViewNumber(int subjectId)
        {
            var result = new TableData();
            try
            {
                var query = await (from a in UnitWork.Find<classroom_subject>(null)
                                    .Where(a => a.Id == subjectId)
                                   select a).FirstOrDefaultAsync();
                if (query != null)
                {
                    query.ViewNumbers++;
                    await UnitWork.UpdateAsync(query);
                    await UnitWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                throw;
            }
            return result;
        }

        /// <summary>
        /// 修改课程观看次数
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> UpdateCourseViewNumber(int courseId)
        {
            var result = new TableData();
            try
            {
                var query = await (from a in UnitWork.Find<classroom_subject_course>(null)
                                    .Where(a => a.Id == courseId)
                                   select a).FirstOrDefaultAsync();
                if (query != null)
                {
                    query.ViewNumbers++;
                    await UnitWork.UpdateAsync(query);
                    await UnitWork.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                throw;
            }
            return result;
        }
        #endregion

    }
}
