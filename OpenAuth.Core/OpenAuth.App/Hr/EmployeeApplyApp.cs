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
using System.IO;
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
        /// <param name="Name"></param>
        /// <param name="Mobile"></param>
        /// <param name="AuditState"></param>
        /// <param name="PageIndex"></param>
        /// <param name="PageSize"></param>
        /// <param name="StartTime"></param>
        /// <param name="EndTime"></param>
        /// <returns></returns>
        public async Task<TableData> GetEmployeeApplyList(string Name, string Mobile, int PageIndex ,int PageSize, DateTime? StartTime , DateTime? EndTime)
        {
            var result = new TableData();
            var query = from a in UnitWork.Find<classroom_employee_apply_log>(null)
                          .WhereIf(!string.IsNullOrWhiteSpace(Name), a => a.Name.Contains(Name))
                          .WhereIf(!string.IsNullOrWhiteSpace(Mobile), a => a.Mobile.Contains(Mobile))
                          .WhereIf(StartTime.HasValue, a => a.CreateTime >= StartTime.Value)
                          .WhereIf(EndTime.HasValue, a => a.CreateTime <= EndTime.Value)
                        select a;

            result.Count = await query.CountAsync();
            var pageData = await query.OrderByDescending(zw=>zw.Id).Skip((PageIndex - 1) * PageSize).Take(PageSize).ToListAsync();

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
                CreateTime = zw.CreateTime.ToString(Defaults.DateTimeFormat)
            }).ToList();
            result.Data = obj;
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
                CreateTime = DateTime.Now,
                UpdateTime = null,
                UpdateUser = string.Empty
            };
            await UnitWork.AddAsync<classroom_employee_apply_log, int>(model);
            await UnitWork.SaveAsync();
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
            var fileExtension = Path.GetExtension(file.FileName);
            var fileName = "erp4-rom/resume/"+ DateTime.Now.ToString("yyyyMMddHHmmssfff") + fileExtension;
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
