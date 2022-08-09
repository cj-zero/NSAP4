using Common;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.HuaweiOBS;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAuth.App.Hr;
using OpenAuth.App.Hr.Request;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    ///  职工申请相关
    /// </summary>
    public class EmployeeApplyApp : OnlyUnitWorkBaeApp
    {
        private IOptions<AppSetting> _appConfiguration;
        private HttpHelper _helper;
        private ILogger<EmployeeApplyApp> _logger;

        /// <summary>
        /// 职工申请
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="appConfiguration"></param>
        /// <param name="logger"></param>
        public EmployeeApplyApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration, ILogger<EmployeeApplyApp> logger) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
            _helper = new HttpHelper(_appConfiguration.Value.AppPushMsgUrl);
            _logger = logger;
        }


        #region erp

        /// <summary>
        /// 职工申请列表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetEmployeeApplyList(GetEmployeeApplyListReq req)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<classroom_employee_apply_log>(null)
                          .WhereIf(!string.IsNullOrWhiteSpace(req.Name), a => a.Name.Contains(req.Name))
                          .WhereIf(!string.IsNullOrWhiteSpace(req.Mobile), a => a.Mobile.Contains(req.Mobile))
                          .WhereIf(req.StartTime.HasValue, a => a.CreateTime >= req.StartTime.Value)
                          .WhereIf(req.EndTime.HasValue, a => a.CreateTime <= req.EndTime.Value)
                          .WhereIf(req.AuditState > 0, a => a.AuditState == req.AuditState)
                        select a;

            result.Count = await query.CountAsync();
            var pageData = await query.OrderByDescending(zw=>zw.Id).Skip((req.pageIndex - 1) * req.pageSize).Take(req.pageSize).ToListAsync();

            List<EmployeeApplyDto> obj = pageData.Select(zw => new EmployeeApplyDto {
                Id = zw.Id,
                Name = zw.Name,
                Mobile = zw.Mobile,
                Sex = zw.Sex,
                Age = zw.Age,
                AppUserId = zw.AppUserId,
                GraduationSchool = zw.GraduationSchool,
                ResumeFileName = zw.ResumeFileName,
                ResumeFilePath = zw.ResumeFilePath,
                AuditState = zw.AuditState,
                CreateTime = zw.CreateTime.ToString(Defaults.DateTimeFormat)
            }).ToList();
            result.Data = obj;
            return result;
        }

        /// <summary>
        /// 职工申请审核
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> EmployeeApplyAudit(EmployeeApplyAuditReq req)
        {
            var result = new TableData();
            var user = _auth.GetCurrentUser().User;
            var query = await UnitWork.Find<classroom_employee_apply_log>(null).FirstOrDefaultAsync(c => c.Id == req.id);
            if (query != null)
            {
                query.AuditState = req.auditState;
                query.UpdateUser = user.Name;
                query.UpdateTime = DateTime.Now;
            }
            await UnitWork.UpdateAsync(query);
            await UnitWork.SaveAsync();
            if (req.auditState == (int)AuditStateEnum.Rejected)
            {
                // 消息推送
            }
            return result;
        }

        #endregion

        #region App

        /// <summary>
        /// 员工加入申请提交
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> EmployeeApply(EmployeeApplyReq req)
        {
            var result = new TableData();
            var query = await UnitWork.Find<classroom_employee_apply_log>(null).Where(c => c.AppUserId == req.AppUserId).OrderByDescending(c => c.Id).FirstOrDefaultAsync();
            if (query != null)
            {
                if (query.AuditState == (int)AuditStateEnum.Ban)
                {
                    result.Code = 500;
                    result.Message = "您已被管理员封禁,请联系管理员进行解封!";
                    return result;
                }
                else if (query.AuditState == (int)AuditStateEnum.ToBeReviewed)
                {
                    result.Code = 500;
                    result.Message = "您已提交过申请,管理员正在审核!";
                    return result;
                }
                else if (query.AuditState == (int)AuditStateEnum.Normal)
                {
                    result.Code = 500;
                    result.Message = "您提交的申请已被查阅，请耐心等待HR联系您!";
                    return result;
                }
                else if (query.AuditState == (int)AuditStateEnum.Rejected)
                {
                    var age = GetAgeByBirthdate(req.Birthdate);
                    classroom_employee_apply_log model = new classroom_employee_apply_log
                    {
                        Name = req.Name,
                        Mobile = req.Mobile,
                        Sex = req.Sex,
                        Age = age,
                        Birthdate = req.Birthdate,
                        AppUserId = req.AppUserId,
                        GraduationSchool = req.GraduationSchool,
                        ResumeFilePath = req.ResumeFilePath,
                        ResumeFileName = req.ResumeFileName,
                        AuditState = (int)AuditStateEnum.ToBeReviewed,
                        CreateTime = DateTime.Now,
                        UpdateTime = null,
                        UpdateUser = string.Empty
                    };
                    await UnitWork.AddAsync<classroom_employee_apply_log, int>(model);
                    await UnitWork.SaveAsync();
                }
            }
            else
            {
                var age = GetAgeByBirthdate(req.Birthdate);
                classroom_employee_apply_log model = new classroom_employee_apply_log
                {
                    Name = req.Name,
                    Mobile = req.Mobile,
                    Sex = req.Sex,
                    Age = age,
                    Birthdate = req.Birthdate,
                    AppUserId = req.AppUserId,
                    GraduationSchool = req.GraduationSchool,
                    ResumeFilePath = req.ResumeFilePath,
                    ResumeFileName = req.ResumeFileName,
                    AuditState = (int)AuditStateEnum.ToBeReviewed,
                    CreateTime = DateTime.Now,
                    UpdateTime = null,
                    UpdateUser = string.Empty
                };
                await UnitWork.AddAsync<classroom_employee_apply_log, int>(model);
                await UnitWork.SaveAsync();
            }
            return result;
        }


        /// <summary>
        /// 上传简历到华为云obs
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async Task<TableData> UploadResumeToHuaweiOBS(IFormFile file)
        {
            var result = new TableData();
            var obsHelper = new HuaweiOBSHelper();
            var fileName = "erp4-rom/resume/"+ DateTime.Now.ToString("yyyyMMddHHmmss") + file.FileName;
            var stream = file?.OpenReadStream();
            var response = obsHelper.PutObject(fileName, null, stream, out string objectKey);
            dynamic obj = new
            {
                FileName = objectKey.Replace("erp4-rom/resume/",""),
                FilePath = response.ObjectUrl
            };
            result.Data = obj;
            return result;
        }

        /// <summary>
        /// 年龄计算
        /// </summary>
        /// <param name="birthdate"></param>
        /// <returns></returns>
        public int GetAgeByBirthdate(DateTime birthdate)
        {
            DateTime now = DateTime.Now;
            int age = now.Year - birthdate.Year;
            if (now.Month < birthdate.Month || (now.Month == birthdate.Month && now.Day < birthdate.Day))
            {
                age--;
            }
            return age < 0 ? 0 : age;

        }

        #endregion

    }
}
