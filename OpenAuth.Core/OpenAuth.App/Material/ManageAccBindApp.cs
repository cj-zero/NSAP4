using Infrastructure;
using Infrastructure.Export;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.HPSF;
using OpenAuth.App.ClientRelation;
using OpenAuth.App.Clue.ModelDto;
using OpenAuth.App.Interface;
using OpenAuth.App.Order;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Material;
using OpenAuth.Repository.Domain.View;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reactive.Joins;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace OpenAuth.App.Material
{
    public class ManageAccBindApp : OnlyUnitWorkBaeApp
    {
        ServiceBaseApp _serviceBaseApp;
        private ILogger<ManageAccBindApp> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public ManageAccBindApp(IUnitWork unitWork, IAuth auth, ILogger<ManageAccBindApp> logger, ServiceBaseApp serviceBaseApp) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;
            _logger = logger;
        }

        /// <summary>
        /// 获取4.0用户数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<UserManageUtilityRsp> GetUserUtilityList(UserManageUtilityRequest req)
        {
            UserManageUtilityRsp umur = new UserManageUtilityRsp();
            StringBuilder strSql = new StringBuilder();
            StringBuilder strSqlCount = new StringBuilder();
            strSqlCount.AppendFormat("select count(1) as count  from usermanageutility u  ");
            //strSql.AppendFormat("select * from usermanageutility u where LOCATE(u.Name , \"{0}\")  > 0  or LOCATE(u.deptName , \"{1}\")  > 0 LIMIT  {2}  {3} ", req.SlpName,req.deptName,(req.page-1)*req.Limit, req.Limit);
            strSql.AppendFormat("select * from usermanageutility u ");
            if (!string.IsNullOrEmpty(req.SlpName) || !string.IsNullOrEmpty(req.deptName))
            {
                if (!string.IsNullOrEmpty(req.SlpName) && string.IsNullOrEmpty(req.deptName))
                {
                    strSql.AppendFormat(" where LOCATE(\"{0}\" , u.Name)  > 0   ", req.SlpName);
                    strSqlCount.AppendFormat(" where LOCATE(\"{0}\" , u.Name)  > 0   ", req.SlpName);
                }
                if (string.IsNullOrEmpty(req.SlpName) && !string.IsNullOrEmpty(req.deptName))
                {
                    strSql.AppendFormat(" where LOCATE(\"{0}\" , u.deptName)  > 0   ", req.deptName);
                    strSqlCount.AppendFormat(" where LOCATE(\"{0}\" , u.deptName)  > 0   ", req.deptName);
                }
                if (!string.IsNullOrEmpty(req.SlpName) && !string.IsNullOrEmpty(req.deptName))
                {
                    strSql.AppendFormat(" where LOCATE(\"{0}\" , u.Name)  > 0  or LOCATE( \"{1}\" ,u.deptName)  > 0 ", req.SlpName, req.deptName);
                    strSqlCount.AppendFormat(" where LOCATE(\"{0}\" , u.Name)  > 0  or LOCATE( \"{1}\" ,u.deptName)  > 0 ", req.SlpName, req.deptName);
                }

            }
            strSql.AppendFormat("  LIMIT  {0} , {1} ", (req.page - 1) * req.Limit, req.Limit);
            var erp4UserList = UnitWork.ExcuteSql<UserManageUtilityView>(ContextType.DefaultContextType, strSql.ToString(), CommandType.Text, null);
            var erp4UserCount = UnitWork.ExcuteSql<CardCountDto>(ContextType.DefaultContextType, strSqlCount.ToString(), CommandType.Text, null);
            umur.Count = erp4UserCount.FirstOrDefault().count;
            umur.muv.AddRange(erp4UserList);
            return umur;
        }

        /// <summary>
        /// 获取绑定4.0账号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<BindUtilityRep> GetBindUtilityList(BindUtilityRequest req)
        {
            BindUtilityRep bur = new BindUtilityRep();
            StringBuilder strSql = new StringBuilder();
            StringBuilder strSqlMaterial = new StringBuilder();
            List<ManageAccountBind> bindList = new List<ManageAccountBind>();
            //get material user data 
            // strSqlMaterial.AppendFormat("select UserID,UserName,FirstNameAndLastName  from UsersAndGroups u where Enabled = 1 and IsDeleted = 0  ORDER BY UserID  OFFSET   {0}  ROWS  FETCH NEXT   {1}  ROWS ONLY   ", (req.page - 1) * req.Limit, req.Limit);
            strSqlMaterial.AppendFormat("select UserID,UserName,FirstNameAndLastName  from UsersAndGroups u where Enabled = 1 and IsDeleted = 0  ");
            if (!string.IsNullOrEmpty(req.query))
            {
                strSqlMaterial.AppendFormat(" and (u.UserName like N'%{0}%' or  u.FirstNameAndLastName like N'%{0}%' ) ", req.query);
            }
            strSqlMaterial.AppendFormat("  ORDER BY UserID  OFFSET   {0}  ROWS  FETCH NEXT   {1}  ROWS ONLY   ", (req.page - 1) * req.Limit, req.Limit);
            var MaterialUserList = UnitWork.ExcuteSql<MaterialUsers>(ContextType.ManagerDbContext, strSqlMaterial.ToString(), CommandType.Text, null);
            var MaterialIds = MaterialUserList.Select(u => u.UserID).ToList();
            strSql.AppendFormat("select * from manageaccountbind u  where LOCATE(u.MAccount , \"{0}\")  > 0", JsonConvert.SerializeObject(MaterialIds).Replace(@"""", ""));
            var erp4BindList = UnitWork.ExcuteSql<ManageAccountBind>(ContextType.DefaultContextType, strSql.ToString(), CommandType.Text, null).ToList();
            //concat bindData
            foreach (var muser in MaterialUserList)
            {
                if (erp4BindList.Exists(u=>u.MAccount == muser.UserID.ToString()))
                {
                    var specBinding = erp4BindList.Where(u => u.MAccount == muser.UserID.ToString()).FirstOrDefault();
                    bindList.Add(specBinding);
                }
                else
                {
                    bindList.Add(new ManageAccountBind { 
                        MAccount = muser.UserID.ToString(),
                        MName=muser.FirstNameAndLastName==null? muser.UserName: muser.FirstNameAndLastName
                    });
                }
            }
            StringBuilder strSqlCount = new StringBuilder();
            strSqlCount.AppendFormat("select count(1) as count from UsersAndGroups  where Enabled = 1 and IsDeleted = 0   ");
            if (!string.IsNullOrEmpty(req.query))
            {
                strSqlCount.AppendFormat(" and (UserName like N'%{0}%' or  FirstNameAndLastName like N'%{0}%' ) ", req.query);
            }
            var erp4BindCount = UnitWork.ExcuteSql<CardCountDto>(ContextType.ManagerDbContext, strSqlCount.ToString(), CommandType.Text, null);
            bur.Count = erp4BindCount.FirstOrDefault().count;
            bur.mab.AddRange(bindList);
            return bur;
        }

        /// <summary>
        ///  修改绑定4.0账号
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<bool> UpdateBindUtility(BindUtilityUpdateRequest req)
        {
            bool result = true;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var queryBindUtility = UnitWork.FindSingle<ManageAccountBind>(a => a.MAccount == req.MAccount && a.IsDelete == 0 );
            if (queryBindUtility != null)
            {
                queryBindUtility.LAccount = req.LAccount;
                queryBindUtility.LName = req.LName;
                queryBindUtility.DutyFlag = req.DutyFlag;
                queryBindUtility.Level = req.Level;
                queryBindUtility.UpdateDate = DateTime.Now;
                queryBindUtility.Updaterid = loginContext.User.Id;
                queryBindUtility.Updater = loginContext.User.Name;
                queryBindUtility.IsDelete = req.IsDelete;
                await UnitWork.UpdateAsync<ManageAccountBind>(queryBindUtility);
            }
            else
            {
                await UnitWork.AddAsync<ManageAccountBind>(new ManageAccountBind { 
                    MAccount = req.MAccount,
                    MName = req.MName,
                    LName = req.LName,
                    LAccount=req.LAccount,
                    DutyFlag = req.DutyFlag,
                    Level = req.Level,
                    UpdateDate = DateTime.Now,
                    CreateDate = DateTime.Now,
                    Creatorid = loginContext.User.Id,
                    Creator = loginContext.User.Name,
                    IsDelete =0
                });
            }
            
            await UnitWork.SaveAsync();
            return result;
        }


        /// <summary>
        /// 考勤柱状图数据
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<DutyChartResponse> DutyChartUtility(DutyChartRequest req)
        {
            DutyChartResponse dcr = new DutyChartResponse();
            // get due time personals
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("select Owner,OwnerId,count(Number) as Total ,sum(case when Complete = 1 then 1 else 0 end) as CompleteCount from  TaskView5 where  duedate  >= '2022-10-01' AND duedate  <= '2022-10-31' group by Owner ");
            var personalAssignList = UnitWork.ExcuteSql<SerieManageData>(ContextType.ManagerDbContext, strSql.ToString(),CommandType.Text);
            StringBuilder strFSql = new StringBuilder();
            strFSql.AppendFormat("select Owner,OwnerId,count(Number) as Total ,sum(case when Complete = 1 then 1 else 0 end) as CompleteCount from  TaskView5 where  CompleteTime  >= '2022-10-01' AND CompleteTime  <= '2022-10-31' group by Owner ");
            var personalFList = UnitWork.ExcuteSql<SerieManageData>(ContextType.ManagerDbContext, strSql.ToString(), CommandType.Text);
            // get personals with their level for which the qualified line and excel line needed
            var personalNames = personalAssignList.Select(u => u.Owner).ToList();
            var personalFNames = personalFList.Select(u => u.Owner).ToList();
            StringBuilder strSqlbind = new StringBuilder();
            StringBuilder strSqlFbind = new StringBuilder();
            strSqlbind.AppendFormat("select * from manageaccountbind u  where LOCATE(u.MName , \"{0}\")  > 0  and u.DutyFlag = 1 and  Level is not null ", JsonConvert.SerializeObject(personalNames).Replace(@"""", ""));
            strSqlFbind.AppendFormat("select * from manageaccountbind u  where LOCATE(u.MName , \"{0}\")  > 0  and u.DutyFlag = 1 and  Level is not null ", JsonConvert.SerializeObject(personalFNames).Replace(@"""", ""));
            var erp4BindList = UnitWork.ExcuteSql<ManageAccountBind>(ContextType.DefaultContextType, strSqlbind.ToString(), CommandType.Text, null).ToList();
            var erp4BindFList = UnitWork.ExcuteSql<ManageAccountBind>(ContextType.DefaultContextType, strSqlFbind.ToString(), CommandType.Text, null).ToList();
            var legitPersonalNameList = erp4BindList.Select(u => u.MName).ToList();
            var legitPersonalNameFList = erp4BindFList.Select(u => u.MName).ToList();
            var legitPersonals = personalAssignList.Where(u => legitPersonalNameList.Contains(u.Owner)).ToList();
            var legitFPersonals = personalFList.Where(u => legitPersonalNameFList.Contains(u.Owner)).ToList();
            //concat data and order as the will
            dcr.XData = legitPersonals.Select(a => a.Owner).ToList();
            SerieData serieRuleQualified = new SerieData();
            serieRuleQualified.Name = "合格件数";
            SerieData serieRuleExcel = new SerieData();
            serieRuleExcel.Name = "满分件数";
            SerieData serieDataTotal = new SerieData();
            serieDataTotal.Name = "指派任务数";
            SerieData serieDataComplete = new SerieData();
            serieDataComplete.Name = "已完成数";
            foreach (var xitem in dcr.XData)
            {
                var bindLevel = erp4BindList.Where(a => a.MName == xitem).FirstOrDefault();
                if (bindLevel!=null)
                {
                    if (bindLevel.Level == "1")
                    {
                        serieRuleQualified.SerieVal.Add(20);
                        serieRuleExcel.SerieVal.Add(30);
                    }
                    if (bindLevel.Level == "2")
                    {
                        serieRuleQualified.SerieVal.Add(15);
                        serieRuleExcel.SerieVal.Add(25);
                    }
                    if (bindLevel.Level == "3")
                    {
                        serieRuleQualified.SerieVal.Add(10);
                        serieRuleExcel.SerieVal.Add(15);
                    }
                }
                serieDataTotal.SerieVal.Add(legitPersonals.Where(u => u.Owner == xitem).FirstOrDefault().Total);
                serieDataTotal.SerieVal.Add(legitFPersonals.Where(u => u.Owner == xitem).FirstOrDefault().CompleteCount);
            }
            dcr.YData.Add(serieRuleQualified);
            dcr.YData.Add(serieRuleExcel);
            dcr.YData.Add(serieDataTotal);
            dcr.YData.Add(serieDataComplete);
            return dcr;
        }

        /// <summary>
        /// 评分表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<RateTableResponse> RateTableUtility(DutyChartRequest req)
        {
            RateTableResponse rtr = new RateTableResponse();

            return rtr;
        }

        /// <summary>
        /// 导出评分表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<byte[]> ExportRateTableUtility(List<RateTableExport> req)
        {
            return await ExportAllHandler.ExporterExcel(req);
            //return File(ExportAllHandler.ExporterExcel(req), "application/octet-stream", "test.xlsx");
        }


    }

}
