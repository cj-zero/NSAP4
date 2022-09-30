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
using OpenAuth.Repository;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using OpenAuth.App.ClientRelation.Response;
using System.Data;
using System.Text;
using System.Collections.Generic;
using SixLabors.ImageSharp.PixelFormats;
using Newtonsoft.Json.Linq;
using OpenAuth.Repository.Core;
using TencentCloud.Gaap.V20180529.Models;
using Newtonsoft.Json;
using System.Linq;
using Castle.Core.Internal;
using OpenAuth.App.ClientRelation.Request;
using Microsoft.Extensions.Logging;
using NSAP.Entity.Client;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using DocumentFormat.OpenXml.Math;

namespace OpenAuth.App.ClientRelation
{
    /// <summary>
    /// 客户（中间商与终端）关系操作
    /// </summary>
    public class ClientRelationApp : OnlyUnitWorkBaeApp
    {
        ServiceBaseApp _serviceBaseApp;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public ClientRelationApp(IUnitWork unitWork, IAuth auth, ServiceBaseApp serviceBaseApp) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;

        }

        /// <summary>
        /// 获取关系列表
        /// </summary>
        /// <param name="clientId">客户编号</param>
        /// <returns>返回关系信息</returns>
        public async Task<RelationGraphRsp> GetClientRelationList(string clientId)
        {
            RelationGraphRsp rgp = new RelationGraphRsp();
            rgp.rootId = clientId;
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var clientRel = await UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(u => u.ClientNo == clientId).FirstAsync();
            if (clientRel == null)
            {
                return rgp;
            }

            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("with recursive cte (ClientNo, ClientName,Flag,ParentNo,SubNo) as ( ");
            strSql.AppendFormat("  select     a.ClientNo, a.ClientName,a.Flag,a.ParentNo,a.SubNo  from  clientrelation a ");
            strSql.AppendFormat("  where      a.IsDelete = 0 && a.IsActive =1 && a.OperatorId = \"{1}\" &&  (LOCATE(\"{0}\", a.SubNo) !=0 || LOCATE(\"{0}\", a.ParentNo) !=0) ", clientId, loginContext.User.Id);
            strSql.AppendFormat("  union DISTINCT ");
            strSql.AppendFormat("  select     p.ClientNo, p.ClientName,p.Flag,p.ParentNo,p.SubNo from  clientrelation p ");
            strSql.AppendFormat("   inner join cte ");
            strSql.AppendFormat("           on p.IsDelete = 0 && p.IsActive =1 && p.OperatorId = \"{0}\"  && (LOCATE(cte.ClientNo, p.SubNo)  !=0 || LOCATE(cte.ClientNo, p.ParentNo)  !=0)   limit 100", loginContext.User.Id);
            strSql.AppendFormat(" ) ");
            strSql.AppendFormat(" select * from cte; ");
            List<RawGraph> rawGraphList = new List<RawGraph>();
            rawGraphList.AddRange(UnitWork.ExcuteSql<RawGraph>(ContextType.DefaultContextType, strSql.ToString(), CommandType.Text, null));
            if (rawGraphList.Count != 0)
            {
                //拼接前端需求数据
                var clientList = new List<KeyValuePair<string, string>>();
                for (int i = 0; i < rawGraphList.Count; i++)
                {
                    if (rawGraphList[i].Flag != 2)
                    {
                        clientList.Add(new KeyValuePair<string, string>(rawGraphList[i].ClientNo, i.ToString()));
                    }
                }

                foreach (var graph in rawGraphList)
                {
                    #region 节点
                    GraphNodes gn = new GraphNodes();
                    gn.Id = clientList.Find(a => a.Key == graph.ClientNo).Value;
                    gn.CardCode = graph.ClientNo;
                    gn.Text = graph.ClientName;
                    gn.flag = graph.Flag;
                    rgp.Nodes.Add(gn);
                    #endregion

                    #region 关系
                    if (!string.IsNullOrEmpty(graph.SubNo))
                    {
                        var subLinks = JsonConvert.DeserializeObject<JArray>(graph.SubNo);
                        foreach (var sublink in subLinks)
                        {
                            if (graph.Flag != 2)
                            {
                                rgp.Links.Add(new GraphLinks
                                {
                                    From = clientList.Find(a => a.Key == graph.ClientNo).Value,
                                    To = clientList.Find(a => a.Key == sublink.ToString()).Value,
                                });
                            }

                        }

                    }
                    #endregion
                }
            }
            return rgp;

        }

        /// <summary>
        /// 新增客户获取数据源
        /// </summary>
        /// <returns></returns>
        public async Task<ClientPoolRsp> GetRelatedClients()
        {
            ClientPoolRsp cpr = new ClientPoolRsp();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var userid = loginContext.User.User_Id;
            //sale_id in sbo_user in nsap_base  equals  SlpCode in crm_ocrd in nsap_bone
            var sbouser = UnitWork.FindSingle<sbo_user>(a => a.user_id == userid && a.sbo_id == 1);
            StringBuilder strSql = new StringBuilder();
            strSql.AppendFormat("  SELECT CardCode as ClientNo, CardName as ClientName  from crm_ocrd where SlpCode =  \"{0}\" ", sbouser.sale_id);
            cpr.PoolList.AddRange(UnitWork.ExcuteSql<ClientsSource>(ContextType.NsapBoneDbContextType, strSql.ToString(), CommandType.Text, null));
            cpr.PoolCount = cpr.PoolList.Count;
            return cpr;
        }


        public async Task<bool> UpdateRelationsAfterSync(JobReq job)
        {
            bool result = true;
            // check legit request  72 添加业务伙伴
            var legitJob = UnitWork.FindSingle<wfa_job>(a => a.job_id == job.JobId && a.sync_stat == 4 && (a.job_type_id == 72));
            if (legitJob == null)
            {
                //add to log file to explain why 
                return false;
            }
            //self contained  or update relations
            var client = ByteExtension.ToDeSerialize<clientOCRD>(legitJob.job_data);
            var relatedClients = JsonConvert.DeserializeObject<List<ClientRelJob>>(client.EndCustomerName);
            //get add  ,update batch
            //List< OpenAuth.Repository.Domain.ClientRelation> addData = new List< OpenAuth.Repository.Domain.ClientRelation>();
            List<OpenAuth.Repository.Domain.ClientRelation> updateData = new List<OpenAuth.Repository.Domain.ClientRelation>();
            if (relatedClients.Exists(a => a.customerNo == legitJob.sbo_itf_return))
            {
                //self contained
                OpenAuth.Repository.Domain.ClientRelation cr = new Repository.Domain.ClientRelation
                {
                    ClientNo = legitJob.sbo_itf_return,
                    ClientName = client.CardName,
                    ParentNo = "[\"" + legitJob.sbo_itf_return + "\"]",
                    SubNo = "[\"" + legitJob.sbo_itf_return + "\"]",
                    Flag = 2,
                    ScriptFlag = 0,
                    IsDelete = 0,
                    IsActive = 1,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,

                };
                await UnitWork.AddAsync<OpenAuth.Repository.Domain.ClientRelation, int>(cr);
            }
            //find the script and activate the node
            var originRelation = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.JobId == job.JobId && a.ScriptFlag == 1);
            originRelation.IsActive = 1;
            originRelation.ScriptFlag = 0;
            updateData.Add(originRelation);
            //update parent node
            var parentRelatedNodes = UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(a => originRelation.SubNo.Contains(a.ClientNo) && a.IsActive == 1 && a.IsDelete == 0 && a.Operatorid == legitJob.user_id.ToString());
            foreach (var pnode in parentRelatedNodes)
            {
                var jsonPnode = JsonConvert.DeserializeObject<JArray>(pnode.ParentNo);
                jsonPnode.Add(legitJob.sbo_itf_return);
                pnode.ParentNo = JsonConvert.SerializeObject(jsonPnode);
                updateData.Add(pnode);
            }

            //await UnitWork.BatchAddAsync<OpenAuth.Repository.Domain.ClientRelation, int>(addData.ToArray());
            await UnitWork.BatchUpdateAsync<OpenAuth.Repository.Domain.ClientRelation>(updateData.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 保存关系草稿
        /// </summary>
        /// <param name="jobScript"></param>
        /// <returns></returns>
        public async Task<bool> SaveScriptRelations(JobScriptReq jobScript)
        {
            bool result = true;
            //if exists then delete  the previous script and store in history table 
            bool existFlag = false;
            var existRelation = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.JobId == jobScript.JobId && a.ScriptFlag == 1);
            if (existRelation != null)
            {
                existRelation.IsActive = 0;
                existFlag = true;
                await UnitWork.AddAsync<OpenAuth.Repository.Domain.ClientRelHistory>(new ClientRelHistory
                {
                    ClientName = existRelation.ClientName,
                    ParentNo = existRelation.ParentNo,
                    SubNo = existRelation.SubNo,
                    Flag = existRelation.Flag,
                    ScriptFlag = existRelation.ScriptFlag,
                    IsDelete = existRelation.IsDelete,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    Creator = existRelation.Creator,
                    Creatorid = existRelation.Creatorid,
                    Updater = jobScript.Operator,
                    Updaterid = jobScript.Operatorid,
                    Operator = existRelation.Operator,
                    Operatorid = existRelation.Operatorid,
                    OperateType = 5,
                    JobId = existRelation.JobId
                });
            }

            await UnitWork.UpdateAsync<OpenAuth.Repository.Domain.ClientRelation>(existRelation);
            // unwarp the subno
            var subList = JsonConvert.DeserializeObject<List<ClientRelJob>>(jobScript.EndCustomerName);
            var subListNodes = subList.Select(a => a.customerNo).ToList();
            // add to db
            await UnitWork.AddAsync<OpenAuth.Repository.Domain.ClientRelation, int>(new Repository.Domain.ClientRelation
            {
                ClientName = jobScript.ClientName,
                SubNo = JsonConvert.SerializeObject(subListNodes),
                Flag = jobScript.Flag,
                ScriptFlag = 1,
                IsDelete = 0,
                IsActive = 1,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                Creator = jobScript.Operator,
                Creatorid = jobScript.Operatorid,
                Updater = jobScript.Operator,
                Updaterid = jobScript.Operatorid,
                //to be continued
                Operator = jobScript.Operator,
                Operatorid = jobScript.Operatorid,
                JobId = jobScript.JobId
            });
            await UnitWork.SaveAsync();
            return result;
        }


        /// <summary>
        /// 获取变更历史
        /// </summary>
        /// <returns></returns>
        public async Task<List<ClientRelHistory>> GetHistory()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var userid = loginContext.User.User_Id.ToString();
            var hisList = UnitWork.Find<OpenAuth.Repository.Domain.ClientRelHistory>(a => a.Updaterid == userid && a.IsDelete == 0).ToList();
            return hisList;
        }

        /// <summary>
        /// 关系变更
        /// </summary>
        /// <param name="resignReq"></param>
        /// <returns></returns>
        public async Task<bool> ResignRelations(ResignRelReq resignReq)
        {
            bool result = true;
            List< OpenAuth.Repository.Domain.ClientRelation> addData = new List< OpenAuth.Repository.Domain.ClientRelation>();
            List<OpenAuth.Repository.Domain.ClientRelation> updateData = new List<OpenAuth.Repository.Domain.ClientRelation>();
            List<OpenAuth.Repository.Domain.ClientRelHistory> addHistoryData = new List<OpenAuth.Repository.Domain.ClientRelHistory>();
            //broker  terminal
            //terminal , deactivate the origin operator, add new record for new operator, both store in the history table
            if (resignReq.flag == 0)
            {
                
                var legitRel = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.ClientNo == resignReq.ClientNo && a.IsActive == 1);
                if (resignReq.OperateType == 5 || resignReq.OperateType ==3 || resignReq.OperateType == 1)
                {
                    legitRel.IsActive = 0;
                    updateData.Add(legitRel);
                    addHistoryData.Add(new ClientRelHistory { 
                        CID=legitRel.Id,
                        ClientNo=legitRel.ClientNo,
                        ClientName=legitRel.ClientNo,
                        ParentNo=legitRel.ParentNo,
                        SubNo=legitRel.SubNo,
                        Flag=legitRel.Flag,
                        ScriptFlag=legitRel.ScriptFlag,
                        IsDelete=legitRel.IsDelete,
                        CreateDate=DateTime.Now,
                        UpdateDate=legitRel.UpdateDate,
                        Creator=legitRel.Creator,
                        Creatorid=legitRel.Creatorid,
                        Updater=legitRel.Updater,
                        Updaterid = legitRel.Updaterid,
                        Operator=legitRel.Operator,
                        Operatorid=legitRel.Operatorid,
                        OperateType= resignReq.OperateType,
                        JobId=legitRel.JobId
                    });
                    addData.Add(new Repository.Domain.ClientRelation { 
                        ClientNo=legitRel.ClientNo,
                        ClientName = legitRel.ClientName,
                        ParentNo="",
                        SubNo="",
                        Flag=0,
                        ScriptFlag =0,
                        IsActive =1,
                        IsDelete =0,
                        CreateDate=DateTime.Now,
                        UpdateDate = DateTime.Now,
                        Creator=resignReq.username,
                        Creatorid= resignReq.userid,
                        Updater= resignReq.username,
                        Updaterid = resignReq.userid,
                        Operatorid = resignReq.job_userid,
                        JobId = resignReq.jobid
                    });
                }

            }
            //broker,  if old relation exists and nothing changed ,just activate the client and store the record, otherwise add for the new ower with nothing attached
            if (resignReq.flag ==1)
            {
                var legitRel = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.ClientNo == resignReq.ClientNo && a.IsActive == 1);
                //find pervious deactivated clients
                var preRel = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.ClientNo == resignReq.ClientNo && a.IsActive == 0 && a.Operatorid == resignReq.job_userid);
                if (resignReq.OperateType == 0 || resignReq.OperateType == 2 || resignReq.OperateType == 4)
                {
                    #region  broker
                    legitRel.IsActive = 0;
                    updateData.Add(legitRel);
                    addHistoryData.Add(new ClientRelHistory
                    {
                        CID = legitRel.Id,
                        ClientNo = legitRel.ClientNo,
                        ClientName = legitRel.ClientName,
                        ParentNo = legitRel.ParentNo,
                        SubNo = legitRel.SubNo,
                        Flag = legitRel.Flag,
                        ScriptFlag = legitRel.ScriptFlag,
                        IsDelete = legitRel.IsDelete,
                        CreateDate = DateTime.Now,
                        UpdateDate = legitRel.UpdateDate,
                        Creator = legitRel.Creator,
                        Creatorid = legitRel.Creatorid,
                        Updater = legitRel.Updater,
                        Updaterid = legitRel.Updaterid,
                        Operator = legitRel.Operator,
                        Operatorid = resignReq.job_userid,
                        OperateType = resignReq.OperateType,
                        JobId = legitRel.JobId
                    });

                    if (preRel != null)
                    {
                        //recover the old relations if the terminals still intacted
                        var preSubNodesCount = JsonConvert.DeserializeObject<JArray>(preRel.SubNo).Count; 
                        var preSubNodes = UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(a=> preRel.SubNo.Contains(a.ClientNo) && a.IsDelete ==0&& a.IsActive ==1 && a.Operatorid == resignReq.job_userid).ToList();
                        if (preSubNodes.Count == preSubNodesCount)
                        {
                            // add the previous subnode 
                            addData.Add(new Repository.Domain.ClientRelation
                            {
                                ClientNo = legitRel.ClientNo,
                                ClientName = legitRel.ClientName,
                                ParentNo = "",
                                SubNo = preRel.SubNo,
                                Flag = preRel.Flag,
                                ScriptFlag = 0,
                                IsActive = 1,
                                IsDelete = 0,
                                CreateDate = DateTime.Now,
                                UpdateDate = DateTime.Now,
                                Creator = resignReq.username,
                                Creatorid = resignReq.userid,
                                Updater = resignReq.username,
                                Updaterid = resignReq.userid,
                                Operatorid = resignReq.job_userid,
                                Operator = resignReq.job_username,
                                JobId = resignReq.jobid
                            });
                        }
                    }
                    else
                    {
                        addData.Add(new Repository.Domain.ClientRelation
                        {
                            ClientNo = legitRel.ClientNo,
                            ClientName = legitRel.ClientName,
                            ParentNo = "",
                            SubNo = "",
                            Flag = legitRel.Flag,
                            ScriptFlag = 0,
                            IsActive = 1,
                            IsDelete = 0,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            Creator = resignReq.username,
                            Creatorid = resignReq.userid,
                            Updater = resignReq.username,
                            Updaterid = resignReq.userid,
                            Operatorid = resignReq.job_userid,
                            Operator = resignReq.job_username,
                            JobId = resignReq.jobid
                        });
                    }


                    #endregion
                }

            }

            await UnitWork.BatchAddAsync<OpenAuth.Repository.Domain.ClientRelation, int>(addData.ToArray());
            await UnitWork.BatchUpdateAsync<OpenAuth.Repository.Domain.ClientRelation>(updateData.ToArray());
            await UnitWork.BatchAddAsync<OpenAuth.Repository.Domain.ClientRelHistory, int>(addHistoryData.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }




    }
}
