using Infrastructure;
using Infrastructure.Export;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NPOI.HPSF;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using OpenAuth.App.ClientRelation;
using OpenAuth.App.Clue.ModelDto;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Order;
using OpenAuth.App.Response;
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Material;
using OpenAuth.Repository.Domain.Sap;
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
            strSql.AppendFormat("select * from manageaccountbind u  where LOCATE(u.MAccount , \"{0}\")  > 0 and  u.IsDelete = 0 ", JsonConvert.SerializeObject(MaterialIds).Replace(@"""", ""));
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
        /// 校验提交统计是否合法
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<List<string>> LegitCheckUtility(LegitCheckRequest req)
        {
            List<string> passport = new List<string>();
            var queryBindUtility = UnitWork.Find<ManageAccountBind>(a => req.checkList.Contains(a.MName)  && a.IsDelete == 0 && a.DutyFlag == 1 ).ToList();
            if (queryBindUtility != null)
            {
                foreach (var item in req.checkList)
                {
                    if (!queryBindUtility.Exists(a=>a.MName== item))
                    {
                        passport.Add(item);
                    }
                }
            }
            return passport;
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
            
            string start = Convert.ToDateTime(req.Month + "-01").ToString("yyyy-MM-dd");
             string end = Convert.ToDateTime(req.Month + "-01").AddMonths(1).ToString("yyyy-MM-dd");

            //StringBuilder strSql = new StringBuilder();
            //strSql.AppendFormat("select AssignedTo,count(case when t.fld006314 = N'一般' then 1 else null end)*0.5 as Low    ,count(case when t.fld006314 = N'中等' then 1 else null end) as Medium     ,count(case when t.fld006314 = N'较高' then 1 else null end)*2 as High   ,count(case when t.fld006314 = N'超高' then 1 else null end)*3 as Top from  (select AssignedTo, fld006314,AssignDate  from TaskView5) as  t  where  AssignDate   >= '" + start + "' AND AssignDate  <='" + end + "'  group by AssignedTo   ");
            //var personalAssignList = UnitWork.ExcuteSql<SerieManageDataRaw>(ContextType.ManagerDbContext, strSql.ToString(), CommandType.Text);
            //StringBuilder strFSql = new StringBuilder();
            //strFSql.AppendFormat("select AssignedTo,count(case when t.fld006314 = N'一般' then 1 else null end)*0.5 as Low    ,count(case when t.fld006314 = N'中等' then 1 else null end) as Medium     ,count(case when t.fld006314 = N'较高' then 1 else null end)*2 as High   ,count(case when t.fld006314 = N'超高' then 1 else null end)*3 as Top from  (select AssignedTo, fld006314,AssignDate  from TaskView5) as  t  where  CompleteTime  >= '" + start + "' AND CompleteTime  <='" + end + "'  group by AssignedTo    ORDER BY CompleteCount DESC ");
            //var personalFList = UnitWork.ExcuteSql<SerieManageDataRaw>(ContextType.ManagerDbContext, strFSql.ToString(), CommandType.Text);

            #region rejected code v1
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("select AssignedTo,count(Number) as Total ,sum(case when isFinished = 1 then 1 else 0 end) as CompleteCount from  (select AssignedTo, Number, isFinished,AssignDate  from TaskView5) as  t where  AssignDate   >= '" + start + "' AND AssignDate  <='" + end + "'  group by AssignedTo  ORDER BY CompleteCount DESC ");
            var personalAssignList = UnitWork.ExcuteSql<SerieManageData>(ContextType.ManagerDbContext, strSql.ToString(), CommandType.Text);
            StringBuilder strFSql = new StringBuilder();
            strFSql.AppendFormat("select AssignedTo,count(Number) as Total ,sum(case when isFinished = 1 then 1 else 0 end) as CompleteCount from   (select AssignedTo, Number, isFinished,CompleteTime  from TaskView5) as  t  where  CompleteTime  >= '" + start + "' AND CompleteTime  <='" + end + "'  group by AssignedTo    ORDER BY CompleteCount DESC ");
            var personalFList = UnitWork.ExcuteSql<SerieManageData>(ContextType.ManagerDbContext, strFSql.ToString(), CommandType.Text);
            #endregion

            // get personals with their level for which the qualified line and excel line needed
            var personalNames = personalAssignList.Select(u => u.AssignedTo).ToList();
            var personalFNames = personalFList.Select(u => u.AssignedTo).ToList();
            StringBuilder strSqlbind = new StringBuilder();
            StringBuilder strSqlFbind = new StringBuilder();
            strSqlbind.AppendFormat("select * from manageaccountbind u  where (LOCATE(u.MName , \"{0}\")  > 0  ||  LOCATE(u.MName , \"{1}\")  > 0  )and u.DutyFlag = 1 and  Level is not null ", JsonConvert.SerializeObject(personalNames).Replace(@"""", ""),JsonConvert.SerializeObject(personalFNames).Replace(@"""", ""));
            var erp4BindList = UnitWork.ExcuteSql<ManageAccountBind>(ContextType.DefaultContextType, strSqlbind.ToString(), CommandType.Text, null).ToList();
            var legitPersonalNameList = erp4BindList.Select(u => u.MName).ToList();
            var legitPersonalNameFList = erp4BindList.Select(u => u.MName).ToList();
            var legitPersonals = personalAssignList.Where(u => legitPersonalNameList.Contains(u.AssignedTo)).ToList();
            var legitFPersonals = personalFList.Where(u => legitPersonalNameFList.Contains(u.AssignedTo)).ToList();
            //concat data and order as the will
            var FinalPersonals = new List<SerieManageData>();
            FinalPersonals.AddRange(legitPersonals);
            FinalPersonals.AddRange(legitFPersonals);
           var FinalPersonalSort=  FinalPersonals.OrderByDescending(a => a.CompleteCount);
            dcr.XData = FinalPersonalSort.Select(a => a.AssignedTo).Distinct().ToList();
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
                if (bindLevel != null)
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

                    if (legitPersonals.Where(u => u.AssignedTo == xitem).FirstOrDefault() != null)
                    {
                        serieDataTotal.SerieVal.Add(legitPersonals.Where(u => u.AssignedTo == xitem).FirstOrDefault().Total);
                    }
                    if (legitPersonals.Where(u => u.AssignedTo == xitem).FirstOrDefault() == null)
                    {
                        serieDataTotal.SerieVal.Add(0);
                    }
                    //serieDataTotal.SerieVal.Add(legitPersonals.Where(u => u.Owner == xitem).FirstOrDefault().Total);
                    if (legitFPersonals.Where(u => u.AssignedTo == xitem).FirstOrDefault() != null)
                    {
                        serieDataComplete.SerieVal.Add(legitFPersonals.Where(u => u.AssignedTo == xitem).FirstOrDefault().CompleteCount);
                    }
                    if (legitFPersonals.Where(u => u.AssignedTo == xitem).FirstOrDefault() == null)
                    {
                        serieDataComplete.SerieVal.Add(0);
                    }
                }

            }
            dcr.YData.Add(serieRuleQualified);
            dcr.YData.Add(serieRuleExcel);
            dcr.YData.Add(serieDataTotal);
            dcr.YData.Add(serieDataComplete);
            //replace MNAME with LNAME
            var mnameList  = FinalPersonalSort.Select(a => a.AssignedTo).Distinct().ToList();
            dcr.XData = new List<string>();
            foreach (var mitem in mnameList)
            {
                var bindLname = erp4BindList.Where(a => a.MName == mitem).FirstOrDefault();
                if (bindLname!=null)
                {
                    dcr.XData.Add(bindLname.LName);
                }
                else
                {
                    dcr.XData.Add(mitem);
                }
            }

            return dcr;
        }

        /// <summary>
        /// 考勤任务明细
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<DutyDetailsRsp> DutyDetailsTableUtility(DutyChartRequest req)
        {
            DutyDetailsRsp ddr = new DutyDetailsRsp();
            StringBuilder strSql = new StringBuilder();
            string start = Convert.ToDateTime(req.Month + "-01").ToString("yyyy-MM-dd");
            string end = Convert.ToDateTime(req.Month + "-01").AddMonths(1).ToString("yyyy-MM-dd");
            strSql.AppendFormat("SELECT Owner as Name,Number as PartNum, objNBS as Theme, StageName as TaskName, fld005506 as ProductModel, Complete as Completion,fld006314 as DiffcultDegree, Status as Status, DueDate as DueDate, DueDays, AssignedBy as Assigner,AssignedTo as Assignee, CreatedBy as Creator,CreatedDate   as CreateDateTime , Owner, AssignTime as AssignTime, StartDate as BeginTime,CompleteTime as EndTime from TaskView5 where  AssignDate   >= '" + start + "' AND AssignDate  <='" + end + "'   ORDER BY CreatedDate DESC  OFFSET  " + ((req.page - 1) * req.limit).ToString() + "   ROWS  FETCH NEXT  " + req.limit.ToString() + "  ROWS ONLY    ");
            // +" OFFSET " + ((query.page - 1) * query.limit).ToString() + " ROWS  FETCH NEXT " + query.limit.ToString() + " ROWS ONLY  ";
 
             var personalAssignList = UnitWork.ExcuteSql<DutyDetails>(ContextType.ManagerDbContext, strSql.ToString(), CommandType.Text);

            var countquery = "select count(1) count  from TaskView5  where  AssignDate   >= '" + start + "' AND AssignDate  <='" + end + "'  ";
            var countList = UnitWork.ExcuteSql<CardCountDto>(ContextType.ManagerDbContext, countquery.ToString(), CommandType.Text, null);
            ddr.Total = countList.FirstOrDefault().count;
            // order by complete num and create time 
            StringBuilder strSqlOrder = new StringBuilder();
            strSqlOrder.AppendFormat("select AssignedTo,count(Number) as Total ,sum(case when isFinished = 1 then 1 else 0 end) as CompleteCount from  TaskView5 where  AssignDate   >= '" + start + "' AND AssignDate  <='" + end + "'  group by AssignedTo  ORDER BY CompleteCount DESC ");
            var personalOrderList = UnitWork.ExcuteSql<SerieManageData>(ContextType.ManagerDbContext, strSqlOrder.ToString(), CommandType.Text);
            foreach (var perOrder in personalOrderList)
            {
                var perAssignList = personalAssignList.Where(u => u.Owner == perOrder.AssignedTo).ToList();
                if (perAssignList != null)
                {
                    ddr.ddr.AddRange(perAssignList);
                }
                
            }

            //ddr.ddr.AddRange(personalAssignList);
            return ddr;
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

        /// <summary>
        /// 保存评分表
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<bool> SaveRateDetails(DetailExportSaveData req)
        {
            bool result = true;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            RateDetail addItem = new RateDetail {
                CreateDate = DateTime.Now,
                Time = req.Time,
                Data = JsonConvert.SerializeObject(req.detports),
                Creator = loginContext.User.Name,
                Creatorid = loginContext.User.Id,
                UpdateDate = DateTime.Now,
                Updaterid = loginContext.User.Id,
                IsDelete = 0
            };
            await UnitWork.AddAsync<RateDetail, int>(addItem);
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 获取对应月份归档记录
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<ArchiveData> GetArchives(string req)
        {
            ArchiveData archiveData = new ArchiveData();
            var adata = UnitWork.FindSingle<RateDetail>(a => a.Time == req);
            if (adata!=null)
            {
                archiveData.ArchiveDatas = adata.Data;
                archiveData.ArchiveFlag = true;
            }
            else
            {
                archiveData.ArchiveFlag = false;
            }
            return archiveData;
        }

        public  TableData GetDataA(MaterialDataReq req)
        {
            var result = new TableData();
            string sql = string.Format(@"SELECT  * from TaskView5  WHERE fld005506 in   ( {0} )  ", String.Join(",",req.Alpha.Select(i => $"'{i.Replace("\'","\"")}'")));
            //sql += req;
            var modeldata = UnitWork.ExcuteSql<statisticsTable>(ContextType.ManagerDbContext, sql, CommandType.Text, null).ToList();
            //List<statisticsTableSpec> finalList = new List<statisticsTableSpec>();
            //foreach (var item in req.Alpha)
            //{
            //    var relateItem = modeldata.Where(a => a.fld005506 == item).ToList();
            //    if (relateItem.Count !=0)
            //    {
            //        finalList.Add(new statisticsTableSpec
            //        {
            //            code = item,
            //            data = relateItem
            //        });
                    
            //    }

            //}
            result.Data = modeldata.ToList();
            return result;
        }

        public TableData GetDataB(MaterialDataReq req)
        {
            var result = new TableData();
            string sql = string.Format(@"SELECT t.TaskId,t.UserCreatedId,t.Subject,t.StartDate,t.DueDate,t.hasReminder,t.StatusId,t.PriorityId,t.Complete,t.isFinished,t.isPrivate,t.isDeleted,
t.AssignDate,t.CreatedDate,t.AssignedBy,t.CaseRecGuid,t.RecordGuid,t.TaskNBS,t.TaskOwnerId,t.TimeAllocated,CompletedDate as CompleteTime,u.FirstNameAndLastName as ownername,DueHours=DATEDIFF(hh,case when t.isFinished=1 then  ISNULL(t.CompletedDate,getdate()) else GETDATE() end,t.duedate) , WorkHours=DATEDIFF(hh,t.StartDate,t.DueDate) from Tasks t
left JOIN UsersAndGroups u on u.UserID = t.TaskOwnerId
    WHERE TaskNBS    in   ( {0} )  ", String.Join(",", req.Alpha.Select(i => $"'{i}'")));
            //sql += req;
            var modeldata = UnitWork.ExcuteSql<statisticsTableB>(ContextType.ManagerDbContext, sql, CommandType.Text, null).ToList();
            result.Data = modeldata.ToList();
            return result;
        }

        public TableData GetDataC(MaterialDataReq req)
        {
            var result = new TableData();
            string sql = string.Format(@"SELECT  * from alpha  WHERE fld005506 in   ( {0} )  ", String.Join(",", req.Alpha.Select(i => $"'{i.Replace("\'", "\"")}'")));
            //sql += req;
            var specJob = UnitWork.ExcuteSql<AlphaView>(ContextType.ManagerDbContext, sql.ToString(), CommandType.Text, null);
            result.Data = specJob.ToList();
            return result;
        }

    }

}
