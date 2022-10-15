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
        private ILogger<ClientRelationApp> _logger;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public ClientRelationApp(IUnitWork unitWork, IAuth auth, ILogger<ClientRelationApp> logger, ServiceBaseApp serviceBaseApp) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;
            _logger = logger;
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

            var clientRel = await UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(u => u.ClientNo == clientId && u.Operatorid == loginContext.User.Id && u.IsActive ==1&&u.IsDelete ==0).FirstAsync();
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
                                if (clientList.Find(a => a.Key == sublink.ToString()).Value != null && clientList.Find(a => a.Key == graph.ClientNo).Value != null)
                                {
                                    rgp.Links.Add(new GraphLinks
                                    {
                                        From = clientList.Find(a => a.Key == graph.ClientNo).Value,
                                        To = clientList.Find(a => a.Key == sublink.ToString()).Value,
                                    });
                                }
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

        /// <summary>
        /// 审核通过，同步成功后更新关系
        /// </summary>
        /// <param name="job"></param>
        /// <returns></returns>
        public async Task<bool> UpdateRelationsAfterSync(JobReq job)
        {
            
            bool result = true;
            // check legit request  72 添加业务伙伴
            var legitJob = UnitWork.FindSingle<wfa_job>(a => a.job_id == job.JobId && a.sync_stat == 4 && (a.job_type_id == 72));
            if (legitJob == null)
            {
                //add to log file to explain why 
                _logger.LogError("审核通过，同步成功后更新关系未找到对应的Job,请求参数为" + JsonConvert.SerializeObject(job)) ;
                return false;
            }
            //self contained  or update relations
            //var client = ByteExtension.ToDeSerialize<clientOCRD>(legitJob.job_data);
            //client.EndCustomerName = "[{\"customerNo\":\"C36031\",\"customerName\":\"aalims\"},{\"customerNo\":\"C02810\",\"customerName\":\"青岛澳德龙电子有限公司\"}]";
            var jobRelation = UnitWork.FindSingle<OpenAuth.Repository.Domain.JobClientRelation>(a => a.Jobid == job.JobId && a.IsDelete == 0);
            if (jobRelation == null)
            {
                //add to log file to explain why 
                _logger.LogError("审核通过，同步成功后更新关系未找到对应表JobClientRelation的Job关系,请求参数为" + JsonConvert.SerializeObject(job));
                return false;
            }
            var relatedClients = JsonConvert.DeserializeObject<List<ClientRelJob>>(jobRelation.Terminals);
            //get add  ,update batch
            List<OpenAuth.Repository.Domain.ClientRelation> updateData = new List<OpenAuth.Repository.Domain.ClientRelation>();
            List<OpenAuth.Repository.Domain.ClientRelHistory> addHistoryData = new List<OpenAuth.Repository.Domain.ClientRelHistory>();
            //find the script and activate the node
            var originRelation = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.JobId == job.JobId && a.ScriptFlag == 1);
            if (!string.IsNullOrEmpty(jobRelation.Terminals) && relatedClients.Exists(a => a.customerNo == legitJob.sbo_itf_return))
            {
                //self contained
                var selfNode = relatedClients.Where(a => a.customerNo == legitJob.sbo_itf_return).FirstOrDefault();
                OpenAuth.Repository.Domain.ClientRelation cr = new Repository.Domain.ClientRelation
                {
                    ClientNo = legitJob.sbo_itf_return,
                    ClientName = selfNode.customerName,
                    ParentNo = "[\"" + legitJob.sbo_itf_return + "\"]",
                    SubNo = "[\"" + legitJob.sbo_itf_return + "\"]",
                    Flag = 2,
                    ScriptFlag = 0,
                    IsDelete = 0,
                    IsActive = 1,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    Creator = originRelation.Creator,
                    Creatorid = originRelation.Creatorid,
                    Updater = originRelation.Updater,
                    Updaterid = originRelation.Updaterid,
                    Operator = originRelation.Operator,
                    Operatorid = originRelation.Operatorid
                };
                await UnitWork.AddAsync<OpenAuth.Repository.Domain.ClientRelation, int>(cr);
            }
       
            if (originRelation != null)
            {
                //add history data 
                addHistoryData.Add(new ClientRelHistory
                {
                    CID = originRelation.Id,
                    ClientNo = job.ClientNo,
                    ClientName = originRelation.ClientName,
                    ParentNo = originRelation.ParentNo,
                    SubNo = originRelation.SubNo,
                    Flag = originRelation.Flag,
                    ScriptFlag = originRelation.ScriptFlag,
                    IsDelete = originRelation.IsDelete,
                    CreateDate = DateTime.Now,
                    UpdateDate = originRelation.UpdateDate,
                    Creator = originRelation.Creator,
                    Creatorid = originRelation.Creatorid,
                    Updater = originRelation.Updater,
                    Updaterid = originRelation.Updaterid,
                    Operator = originRelation.Operator,
                    Operatorid = originRelation.Operatorid,
                    OperateType = 6,
                    JobId = job.JobId
                });
                originRelation.IsActive = 1;
                originRelation.ScriptFlag = 0;
                originRelation.ClientNo = job.ClientNo;
                originRelation.UpdateDate = DateTime.Now;
                updateData.Add(originRelation);
          

                //update parent node
                //切换4.0用户id 
                var erpid = UnitWork.FindSingle<User>(u => u.User_Id == legitJob.user_id);
                var parentRelatedNodes = UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(a => originRelation.SubNo.Contains(a.ClientNo) &&  a.ClientNo != legitJob.sbo_itf_return && a.IsActive == 1 && a.IsDelete == 0 && a.ScriptFlag == 0 && a.Operatorid == erpid.Id).ToList();

                if (parentRelatedNodes.Count > 0)
                {
                    foreach (var pnode in parentRelatedNodes)
                    {
                        addHistoryData.Add(new ClientRelHistory
                        {
                            CID = pnode.Id,
                            ClientNo = pnode.ClientNo,
                            ClientName = pnode.ClientName,
                            ParentNo = pnode.ParentNo,
                            SubNo = pnode.SubNo,
                            Flag = pnode.Flag,
                            ScriptFlag = pnode.ScriptFlag,
                            IsDelete = pnode.IsDelete,
                            CreateDate = DateTime.Now,
                            UpdateDate = pnode.UpdateDate,
                            Creator = pnode.Creator,
                            Creatorid = pnode.Creatorid,
                            Updater = pnode.Updater,
                            Updaterid = pnode.Updaterid,
                            Operator = pnode.Operator,
                            Operatorid = pnode.Operatorid,
                            OperateType = 6,
                            JobId = pnode.JobId
                        });
                        JArray jsonPnode = new JArray();
                        if (!string.IsNullOrEmpty(pnode.ParentNo))
                        {
                            jsonPnode = JsonConvert.DeserializeObject<JArray>(pnode.ParentNo);
                        }
                        else
                        {
                            jsonPnode = JsonConvert.DeserializeObject<JArray>("[]");
                        }
                        jsonPnode.Add(legitJob.sbo_itf_return);
                        pnode.ParentNo = JsonConvert.SerializeObject(jsonPnode);
                        updateData.Add(pnode);
                    }
                }

            }
            else
            {
                _logger.LogError("审核通过，同步成功后更新关系未找到对应的草稿关系,请求参数为" + JsonConvert.SerializeObject(job));
                return false;
            }
            //await UnitWork.BatchAddAsync<OpenAuth.Repository.Domain.ClientRelation, int>(addData.ToArray());
            await UnitWork.BatchAddAsync<OpenAuth.Repository.Domain.ClientRelHistory, int>(addHistoryData.ToArray());
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
                await UnitWork.UpdateAsync<OpenAuth.Repository.Domain.ClientRelation>(existRelation);
            }


            // unwarp the subno
            var subListNodes = new List<string>();
            if (!string.IsNullOrEmpty(jobScript.EndCustomerName))
            {
                var subList = JsonConvert.DeserializeObject<List<ClientRelJob>>(jobScript.EndCustomerName);
                subListNodes.AddRange(subList.Select(a => a.customerNo).ToList());
            }
            var  finalSubnodes = subListNodes.Count ==0 ? "" : JsonConvert.SerializeObject(subListNodes);

            //update the script jobrelation
            if (jobScript.Initial == 1)
            {
                var jobrelations = UnitWork.FindSingle<OpenAuth.Repository.Domain.JobClientRelation>(a => a.Jobid == jobScript.JobId && a.IsDelete == 0);
                jobrelations.Terminals = jobScript.EndCustomerName;
                await UnitWork.UpdateAsync<JobClientRelation>(jobrelations);
            }
            

            // add to db
            
            await UnitWork.AddAsync<OpenAuth.Repository.Domain.ClientRelation, int>(new Repository.Domain.ClientRelation
            {
                ClientName = jobScript.ClientName,
                SubNo = finalSubnodes,
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
                
                var legitRel = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.ClientNo == resignReq.ClientNo && a.IsActive == 1&& a.IsDelete ==0);
                if (resignReq.OperateType == 5 || resignReq.OperateType ==3 || resignReq.OperateType == 1)
                {
                    if (legitRel != null)
                    {
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
                            Operatorid = legitRel.Operatorid,
                            OperateType = resignReq.OperateType,
                            JobId = legitRel.JobId
                        });    
                    }
                    addData.Add(new Repository.Domain.ClientRelation
                    {
                        ClientNo = resignReq.ClientNo,
                        ClientName = resignReq.ClientName,
                        ParentNo = "",
                        SubNo = "",
                        Flag = 0,
                        ScriptFlag = 0,
                        IsActive = 1,
                        IsDelete = 0,
                        CreateDate = DateTime.Now,
                        UpdateDate = DateTime.Now,
                        Creator = resignReq.username,
                        Creatorid = resignReq.userid,
                        Updater = resignReq.username,
                        Updaterid = resignReq.userid,
                        Operator = resignReq.job_username,
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
                    if (legitRel != null)
                    {
                        legitRel.IsActive = 0;
                        updateData.Add(legitRel);
                        // delete the old relations of  parentNode and subNode
                        //var attachedNodes = UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(a=>a.IsDelete == 0 && (a.ParentNo.Contains(resignReq.ClientNo) || a.SubNo.Contains(resignReq.ClientNo) && a.IsActive ==1)).ToList();
                        //foreach (var anode in attachedNodes)
                        //{
                        //    if (anode.ParentNo.Contains(resignReq.ClientNo))
                        //    {
                        //        var pnodes = JsonConvert.DeserializeObject<JArray>(anode.ParentNo);
                        //        pnodes.Where(i => i.Type == JTokenType.String && (string)i == resignReq.ClientNo).ToList().ForEach(i => i.Remove());
                        //        anode.ParentNo = JsonConvert.SerializeObject(pnodes);
                        //    }
                        //    if (anode.SubNo.Contains(resignReq.ClientNo))
                        //    {
                        //        var snodes = JsonConvert.DeserializeObject<JArray>(anode.SubNo);
                        //        snodes.Where(i => i.Type == JTokenType.String && (string)i == resignReq.ClientNo).ToList().ForEach(i => i.Remove());
                        //        anode.SubNo = JsonConvert.SerializeObject(snodes);
                        //    }
                        //}
                       // updateData.AddRange(attachedNodes);
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
                    }

                    if (preRel != null)
                    {
                        //recover the old relations if the terminals still intacted
                        var preSubNodesCount = JsonConvert.DeserializeObject<JArray>(preRel.SubNo).Count; 
                        var preSubNodes = UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(a=> preRel.SubNo.Contains(a.ClientNo) && a.IsDelete ==0&& a.IsActive ==1 && a.Operatorid == resignReq.job_userid).ToList();
                        //若残存对应关系，则应该添加进去
                        if (preSubNodes.Count == preSubNodesCount)
                        {
                            // recover the previous subnode 
                            addData.Add(new Repository.Domain.ClientRelation
                            {
                                ClientNo = legitRel.ClientNo,
                                ClientName = legitRel.ClientName,
                                ParentNo = "",
                                SubNo = preRel.SubNo,
                                Flag = 1,
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
                        if (preSubNodes.Count != preSubNodesCount && preSubNodes.Count > 0)
                        {
                            //recover the residual nodes 
                            addData.Add(new Repository.Domain.ClientRelation
                            {
                                ClientNo = legitRel.ClientNo,
                                ClientName = legitRel.ClientName,
                                ParentNo = "",
                                SubNo = JsonConvert.SerializeObject(preSubNodes),
                                Flag = 1,
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
                            }); ;
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
                            Flag = 1,
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

        /// <summary>
        /// 业务员修改客户
        /// </summary>
        /// <param name="resignReq"></param>
        /// <returns></returns>
        public async Task<bool> ResignTerminals(ResignOper resignReq)
        {

            bool result = true;
            List<OpenAuth.Repository.Domain.ClientRelation> updateData = new List<OpenAuth.Repository.Domain.ClientRelation>();
            List<OpenAuth.Repository.Domain.ClientRelHistory> addHistoryData = new List<OpenAuth.Repository.Domain.ClientRelHistory>();
            var existRel = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.ClientNo == resignReq.ClientNo && a.IsDelete ==0 && a.IsActive ==1);

            //add history
            var hisClient = new ClientRelHistory
            {
                CID = existRel.Id,
                ClientNo = existRel.ClientNo,
                ClientName = existRel.ClientName,
                ParentNo = existRel.ParentNo,
                SubNo = existRel.SubNo,
                Flag = existRel.Flag,
                ScriptFlag = existRel.ScriptFlag,
                IsDelete = existRel.IsDelete,
                CreateDate = DateTime.Now,
                UpdateDate = existRel.UpdateDate,
                Creator = existRel.Creator,
                Creatorid = existRel.Creatorid,
                Updater = existRel.Updater,
                Updaterid = existRel.Updaterid,
                Operator = existRel.Operator,
                Operatorid = existRel.Operatorid,
                OperateType = 7,
                JobId = existRel.JobId
            };
            addHistoryData.Add(hisClient);
            var perviousNodes = UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(a => existRel.SubNo.Contains(a.ClientNo) && a.IsDelete == 0 && a.IsActive == 1 && a.ScriptFlag == 0 && a.Operatorid == existRel.Operatorid).ToList();
            // unwarp the subno
            var  oldSubNodes = JsonConvert.DeserializeObject<JArray>(existRel.SubNo);
            var afterNodes = new List<string>();
            if (!string.IsNullOrEmpty(resignReq.TerminalList))
            {
                var subList = JsonConvert.DeserializeObject<List<ClientRelJob>>(resignReq.TerminalList);
                afterNodes.AddRange(subList.Select(a => a.customerNo).ToList());
            }
 
            //update subnode
          
            var existNodes = UnitWork.Find<OpenAuth.Repository.Domain.ClientRelation>(a => afterNodes.Contains(a.ClientNo) && a.IsDelete == 0 && a.IsActive == 1 && a.ScriptFlag == 0 && a.Operatorid == existRel.Operatorid).ToList();
            //remove parent node
            foreach (var pnode in perviousNodes)
            {
                if (!afterNodes.Contains(pnode.ClientNo))
                {
                    var phisClient = new ClientRelHistory
                    {
                        CID = pnode.Id,
                        ClientNo = pnode.ClientNo,
                        ClientName = pnode.ClientName,
                        ParentNo = pnode.ParentNo,
                        SubNo = pnode.SubNo,
                        Flag = pnode.Flag,
                        ScriptFlag = pnode.ScriptFlag,
                        IsDelete = pnode.IsDelete,
                        CreateDate = DateTime.Now,
                        UpdateDate = pnode.UpdateDate,
                        Creator = pnode.Creator,
                        Creatorid = pnode.Creatorid,
                        Updater = pnode.Updater,
                        Updaterid = pnode.Updaterid,
                        Operator = pnode.Operator,
                        Operatorid = pnode.Operatorid,
                        OperateType = 7,
                        JobId = existRel.JobId
                    };
                    addHistoryData.Add(phisClient);
                    JArray jsonPnode = new JArray();
                    if (!string.IsNullOrEmpty(pnode.ParentNo))
                    {
                        jsonPnode = JsonConvert.DeserializeObject<JArray>(pnode.ParentNo);
                    }
                    else
                    {
                        jsonPnode = JsonConvert.DeserializeObject<JArray>("[]");
                    }

                    if (pnode.ClientNo == existRel.ClientNo)
                    {
                        pnode.IsActive = 0;
                    }
                    else
                    {
                        jsonPnode.Where(i => i.Type == JTokenType.String && (string)i == existRel.ClientNo).ToList().ForEach(i => i.Remove());
                    }
                    
                    pnode.ParentNo = JsonConvert.SerializeObject(jsonPnode);
                    updateData.Add(pnode);
                }
            }
            //add parent node
            foreach (var enode in existNodes)
            {
                if (!existRel.SubNo.Contains(enode.ClientNo))
                {
                    if (enode.ClientNo == existRel.ClientNo)
                    {
                        //add self contained code
                        OpenAuth.Repository.Domain.ClientRelation cr = new Repository.Domain.ClientRelation
                        {
                            ClientNo = existRel.ClientNo,
                            ClientName = existRel.ClientName,
                            ParentNo = "[\"" + existRel.ClientNo + "\"]",
                            SubNo = "[\"" + existRel.ClientNo + "\"]",
                            Flag = 2,
                            ScriptFlag = 0,
                            IsDelete = 0,
                            IsActive = 1,
                            CreateDate = DateTime.Now,
                            UpdateDate = DateTime.Now,
                            Creator = existRel.Creator,
                            Creatorid = existRel.Creatorid,
                            Updater = existRel.Updater,
                            Updaterid = existRel.Updaterid,
                            Operator = existRel.Operator,
                            Operatorid = existRel.Operatorid,
                            JobId = existRel.JobId
                        };
                        await UnitWork.AddAsync<OpenAuth.Repository.Domain.ClientRelation, int>(cr);
                        continue;
                    }
                    var ehisClient = new ClientRelHistory
                    {
                        CID = enode.Id,
                        ClientNo = enode.ClientNo,
                        ClientName = enode.ClientName,
                        ParentNo = enode.ParentNo,
                        SubNo = enode.SubNo,
                        Flag = enode.Flag,
                        ScriptFlag = enode.ScriptFlag,
                        IsDelete = enode.IsDelete,
                        CreateDate = DateTime.Now,
                        UpdateDate = enode.UpdateDate,
                        Creator = enode.Creator,
                        Creatorid = enode.Creatorid,
                        Updater = enode.Updater,
                        Updaterid = enode.Updaterid,
                        Operator = enode.Operator,
                        Operatorid = enode.Operatorid,
                        OperateType = 7,
                        JobId = existRel.JobId
                    };
                    addHistoryData.Add(ehisClient);
                    JArray jsonPnode = new JArray();
                    if (!string.IsNullOrEmpty(enode.ParentNo))
                    {
                        jsonPnode = JsonConvert.DeserializeObject<JArray>(enode.ParentNo);
                    }
                    else
                    {
                        jsonPnode = JsonConvert.DeserializeObject<JArray>("[]");
                    }
                  
           
                     jsonPnode.Add(existRel.ClientNo);
                 
                    
                    enode.ParentNo = JsonConvert.SerializeObject(jsonPnode);
                    updateData.Add(enode);
                }
            }

            existRel.SubNo = JsonConvert.SerializeObject(afterNodes);
            existRel.UpdateDate = DateTime.Now;
            updateData.Add(existRel);
            await UnitWork.BatchUpdateAsync<OpenAuth.Repository.Domain.ClientRelation>(updateData.ToArray());
            await UnitWork.BatchAddAsync<OpenAuth.Repository.Domain.ClientRelHistory, int>(addHistoryData.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }

        /// <summary>
        /// 同步更新关系
        /// </summary>
        /// <returns></returns>
        public async Task<bool> SyncRelations()
        {
            // get latest 3 minutes updated job(jobtype = 72)
           
            var updatedRelationJob = UnitWork.Find<wfa_job>(a => a.job_type_id == 72 &&  a.sync_stat ==4 && a.upd_dt>=DateTime.Now.AddMinutes(-3) ).ToList();
            foreach (var relationJob in updatedRelationJob)
            {
                var client = ByteExtension.ToDeSerialize<clientOCRD>(relationJob.job_data);
                if (relationJob.job_nm == "添加业务伙伴")
                {
                    await  UpdateRelationsAfterSync(new OpenAuth.App.ClientRelation.Request.JobReq
                    {
                        ClientNo = relationJob.sbo_itf_return,
                        JobId = (int)relationJob.job_id
                    });
                }
                if (relationJob.job_nm == "修改业务伙伴")
                {
                    if (client.is_reseller == "Y")
                    {
                        //修改数据源
                        int compareJobid = (int)relationJob.job_id;
                        var jobRelation = UnitWork.FindSingle<OpenAuth.Repository.Domain.JobClientRelation>(a => a.Jobid == compareJobid && a.IsDelete == 0);
                        await ResignTerminals(new ResignOper
                        {
                            ClientNo = client.CardCode,
                            TerminalList = jobRelation.Terminals
                        });
                    }
                }

            }

            return true;
        }

        /// <summary>
        /// 添加4.0关系
        /// </summary>
        /// <param name="jrr"></param>
        /// <returns></returns>
        public async Task<bool> AddJobRelations(AddJobRelReq jrr)
        {
            if (!jrr.Terminals.Contains("C"))
            {
                jrr.Terminals = "";
            }
            await UnitWork.AddAsync<OpenAuth.Repository.Domain.JobClientRelation, int>(new JobClientRelation { 
               Jobid = jrr.Jobid,
               Terminals = jrr.Terminals,
               IsDelete =0,
               CreateDate = DateTime.Now,
               Creator = jrr.Creator,
               CreatorId = jrr.CreatorId
            });
            await UnitWork.SaveAsync();
            return true;
        }

        /// <summary>
        /// 获取终端关系
        /// </summary>
        /// <param name="clientNo"></param>
        /// <param name="flag"></param>
        /// <returns></returns>
        public async Task<JobClientRelation> GetTerminals(string clientNo, int flag)
        {
            JobClientRelation result = new JobClientRelation();
            int queryId ;
            if (flag == 0)
            {
                var relatedRelation = UnitWork.FindSingle<OpenAuth.Repository.Domain.ClientRelation>(a => a.ClientNo == clientNo && a.IsDelete == 0 && a.IsActive == 1);
                if (relatedRelation == null)
                {
                    //add to log file to explain why 
                    _logger.LogError("获取终端关系，未找到对应的Job,请求参数为:" + JsonConvert.SerializeObject(clientNo));
                    return result;
                }
                queryId = relatedRelation.JobId;
            }
            else
            {
                queryId = Convert.ToInt32(clientNo);
            }
            result = await UnitWork.FindSingleAsync<OpenAuth.Repository.Domain.JobClientRelation>(a => a.Jobid == queryId && a.IsDelete == 0);
            return result;
        }


    }
}
