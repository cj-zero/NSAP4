using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Infrastructure.Extensions;
using System.Threading.Tasks;
using OpenAuth.App.DDVoice.Common;
using OpenAuth.App.DDVoice.EntityHelp;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.DDVoice
{
    public class DDVoiceApp : OnlyUnitWorkBaeApp
    {
        private DDSettingHelp _ddSettingHelp;
        private List<DDDepartMsgs> afterDeptIds = new List<DDDepartMsgs>();
        private List<DDDepartMsgs> originalDepart = new List<DDDepartMsgs>() { new DDDepartMsgs() { departId = 1, departName = "深圳市新威尔电子有限公司" } };
        private ILogger<DDVoiceApp> _logger;
        private IUnitWork _UnitWork;
        private IAuth _auth;
        private string access_token;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="ddSettingHelp"></param>
        /// <param name="logger"></param>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public DDVoiceApp(DDSettingHelp ddSettingHelp,ILogger<DDVoiceApp> logger, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _logger = logger;
            _UnitWork = unitWork;
            _auth = auth;
            _ddSettingHelp = ddSettingHelp;
            this.DDLogin(); 
        }

        /// <summary>
        /// 钉钉登录
        /// </summary>
        /// <returns>返回token</returns>
        public void DDLogin()
        {
            string appkey = _ddSettingHelp.GetDDKey("Appkey");//获取配置文件中钉钉Appkey
            string appsecret = _ddSettingHelp.GetDDKey("Appsecret");//获取配置文件中钉钉Appsecret

            //获取钉钉登录token
            DDLogin ddLogin = JsonConvert.DeserializeObject<DDLogin>(HttpHelpers.Get($"https://oapi.dingtalk.com/gettoken?appkey={appkey}&appsecret={appsecret}"));
            if (ddLogin != null && !string.IsNullOrEmpty(ddLogin.access_token))
            {
                access_token = ddLogin.access_token;
            }
        }

        /// <summary>
        /// 钉钉推送消息通知(仅支持文本消息推送)
        /// </summary>
        /// <param name="msgType">消息类型</param>
        /// <param name="remarks">文本消息</param>
        /// <param name="userIds">需要发送的用户Id</param>
        /// <returns>成功返回true，失败返回false</returns>
        public async Task DDSendMsg(string msgType, string remarks, string userIds)
        {
            var loginContext = _auth.GetCurrentUser();
            string agent_Id = _ddSettingHelp.GetDDKey("Agent_Id");
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            if (access_token != "")
            {
                try
                {
                    DDSendResult dDSendResult = new DDSendResult();
                    if (msgType == "text")
                    {
                        //钉钉推送文本消息实体
                        DDMsgBodyParam ddBodyParam = new DDMsgBodyParam()
                        {
                            agent_id = agent_Id,
                            userid_list = userIds,
                            dept_id_list = null,
                            to_all_user = false,
                            msg = new DDSendMsg()
                            {
                                msgtype = msgType,
                                text = new DDTextMsg()
                                {
                                    content = remarks,
                                }
                            }
                        };

                        //调用钉钉官方接口推送消息
                        dDSendResult = JsonConvert.DeserializeObject<DDSendResult>(HttpHelpers.HttpPostAsync($"https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token={access_token}", JsonConvert.SerializeObject(ddBodyParam)).Result);
                    }

                    //操作历史
                    await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                    {
                        MsgType = "钉钉工作通知",
                        MsgContent = dDSendResult.errmsg,
                        MsgResult = dDSendResult.errmsg == "ok" ? "成功" : "失败",
                        CreateName = loginContext.User.Name,
                        CreateUserId = loginContext.User.Id,
                        CreateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message.ToString());
                }
            }
        }

        /// <summary>
        /// 获取所有部门Id
        /// </summary>
        /// <param name="dept_ids">部门id集合</param>
        /// <returns>返回所有部门id</returns>
        public async Task<List<DDDepartMsgs>> DDDepartMsg(List<DDDepartMsgs> dept_ids)
        {
            if (access_token != "")
            {
                try
                {
                    DDDepartResult dDDepartResult = new DDDepartResult();
                    foreach (DDDepartMsgs dDDepartMsg in dept_ids.ToArray())
                    {
                        //钉钉推送文本消息实体
                        DDDepartParam dDDepartParam = new DDDepartParam()
                        {
                            dept_id = dDDepartMsg.departId,
                            language = "zh_CN"
                        };

                        //调用钉钉官方接口获取部门列表
                        dDDepartResult = JsonConvert.DeserializeObject<DDDepartResult>(HttpHelpers.HttpPostAsync($"https://oapi.dingtalk.com/topapi/v2/department/listsub?access_token={access_token}", JsonConvert.SerializeObject(dDDepartParam)).Result);

                        if (dDDepartResult.result != null && dDDepartResult.result.Count > 0)
                        {
                            foreach (DDDepartResultMsg result in dDDepartResult.result)
                            {
                                DDDepartMsgs ddmsg = new DDDepartMsgs();
                                ddmsg.departId = result.dept_id;
                                ddmsg.departName = result.name;
                                afterDeptIds.Add(ddmsg);
                                dept_ids.Add(ddmsg);
                                dept_ids.Remove(dDDepartMsg);
                            }
                        }
                        else
                        {
                            dept_ids.Remove(dDDepartMsg);
                        }
                    }

                    if (dept_ids.Count() > 0)
                    {
                        return await DDDepartMsg(dept_ids);
                    }
                    else
                    {
                        //去重获取所有部门Id
                        return afterDeptIds.Where((x, i) => afterDeptIds.FindIndex(s => s.departId == x.departId) == i).ToList();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message.ToString());
                    throw new Exception("获取部门列表失败");
                }
            }

            return afterDeptIds;
        }

        /// <summary>
        /// 获取钉钉部门用户最新信息
        /// </summary>
        /// <param name="departId">部门Id</param>
        /// <returns></returns>
        public async Task GetDDLasterDepartUserMsg(string departId)
        {
            var loginContext = _auth.GetCurrentUser();       
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //获取所有部门信息
            List<DDDepartMsgs> dDDepartMsgs = new List<DDDepartMsgs>();
            if (string.IsNullOrEmpty(departId))
            {
                dDDepartMsgs = await DDDepartMsg(originalDepart);
            }
            else
            {
                List<DDDepartMsgs> dDDepartMsgsList = new List<DDDepartMsgs>();
                dDDepartMsgsList.Add(new DDDepartMsgs() { departId = departId.ToLong(), departName = ""});
                dDDepartMsgs = await DDDepartMsg(dDDepartMsgsList);
            }

            //用户信息集合
            List<DDUserMsg> userMsgs = new List<DDUserMsg>();

            //部门信息集合
            List<DDDepartMsg> departMsgs = new List<DDDepartMsg>();

            //返回结果信息
            List<DDUserMsgs> dDUserMsgs = new List<DDUserMsgs>();
            try
            {
                if (dDDepartMsgs != null && dDDepartMsgs.Count() > 0)
                {
                    if (access_token != "")
                    {
                        foreach (DDDepartMsgs dDDepartMsg in dDDepartMsgs)
                        {
                            //返回结果实体
                            DDBaseUserResult dDBaseUserResult = new DDBaseUserResult();

                            //钉钉部门用户详细信息实体
                            DDDepartUserBaseParam dDDepartUserBaseParam = new DDDepartUserBaseParam()
                            {
                                dept_id = dDDepartMsg.departId,
                                cursor = 0,
                                size = 100,
                                order_field = null,
                                contain_access_limit = false,
                                language = "zh_CN"
                            };

                            //调用钉钉官方接口获取部门下用户信息
                            dDBaseUserResult = JsonConvert.DeserializeObject<DDBaseUserResult>(HttpHelpers.HttpPostAsync($"https://oapi.dingtalk.com/topapi/v2/user/list?access_token={access_token}", JsonConvert.SerializeObject(dDDepartUserBaseParam)).Result);

                            //状态为ok时加载用户信息
                            if (dDBaseUserResult.errmsg == "ok")
                            {
                                dDUserMsgs.InsertRange(dDUserMsgs.Count(), dDBaseUserResult.result.list);
                            }
                            else
                            {
                                //钉钉操作历史记录
                                await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                                {
                                    MsgType = "钉钉获取部门用户信息",
                                    MsgContent = dDDepartMsg.departId.ToString() + dDDepartMsg.departName + dDBaseUserResult.errmsg,
                                    MsgResult = dDBaseUserResult.errmsg == "ok" ? "成功" : "失败",
                                    CreateName = loginContext.User.Name,
                                    CreateUserId = loginContext.User.Id,
                                    CreateTime = DateTime.Now
                                });

                                _logger.LogError(dDDepartMsg.departId.ToString() + dDDepartMsg.departName + "获取用户信息失败");
                                break;
                            }
                        }

                        List<string> ddUserMsg = await UnitWork.Find<DDUserMsg>(null).Select(r => r.UserId).ToListAsync();
                        foreach (DDUserMsgs userItem in dDUserMsgs)
                        {  
                            //判定用户是否已经存在，不存在添加
                            if (!ddUserMsg.Contains(userItem.userid))
                            {
                                //循环获取用户信息
                                userMsgs.Add(new DDUserMsg()
                                {
                                    UserId = userItem.userid,
                                    UserName = userItem.name,
                                    UserPhone = userItem.mobile,
                                    IsBind = false,
                                    CreateUserId = loginContext.User.Id,
                                    CreateName = loginContext.User.Name,
                                    CreateTime = DateTime.Now
                                });

                                foreach (long departid in userItem.dept_id_list)
                                {
                                    DDDepartMsgParam dDDepartMsgParam = new DDDepartMsgParam()
                                    {
                                        dept_id = departid,
                                        language = "zh_CN"
                                    };

                                    //调用钉钉官方接口获取部门下用户信息
                                    DDDepartMsgResult dDDepartMsgResult = JsonConvert.DeserializeObject<DDDepartMsgResult>(HttpHelpers.HttpPostAsync($"https://oapi.dingtalk.com/topapi/v2/department/get?access_token={access_token}", JsonConvert.SerializeObject(dDDepartMsgParam)).Result);

                                    if (dDDepartMsgResult.errmsg == "ok")
                                    {
                                        departMsgs.Add(new DDDepartMsg()
                                        {
                                            UserId = userItem.userid,
                                            DepartId = departid.ToString(),
                                            DepartName = dDDepartMsgResult.result.name
                                        });
                                    }
                                }
                            }
                        }
                       
                        await UnitWork.BatchAddAsync<DDUserMsg>(userMsgs.ToArray());
                        await UnitWork.BatchAddAsync<DDDepartMsg>(departMsgs.ToArray());
                        await UnitWork.SaveAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message.ToString());
                throw new Exception(ex.Message.ToString());
            }
        }

        /// <summary>
        /// 自动绑定用户
        /// </summary>
        /// <returns>返回绑定结果</returns>
        public async Task<TableData> GetAutoDDBindUser()
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //查询部门用户信息
            List<DDNeedBindUserMsg> ddUsers = await UnitWork.Find<DDUserMsg>(r => r.IsBind == false).Select(r => new DDNeedBindUserMsg { UserId = r.UserId, UserName = r.UserName}).ToListAsync();
            var dbContext = UnitWork.GetDbContext<DDBindUser>();
            using (var transaction = await dbContext.Database.BeginTransactionAsync())
            {
                try
                {
                    if (ddUsers != null && ddUsers.Count() > 0)
                    {
                        List<DDNeedBindUserMsg> users = await UnitWork.Find<User>(null).Select(r => new DDNeedBindUserMsg { UserId = r.Id, UserName = r.Name }).ToListAsync();
                        List<DDBindUser> dDBindUsers = new List<DDBindUser>();
                        foreach (DDNeedBindUserMsg item in users)
                        {
                            //对用户名相同且不重名用户进行绑定
                            var needUsers = ddUsers.Where(r => r.UserName == item.UserName).ToList();
                            if (needUsers != null && needUsers.Count() == 1)
                            {
                                var ddBindUsers = await UnitWork.Find<DDBindUser>(r => r.UserId == item.UserId && r.DDUserId == (needUsers.FirstOrDefault()).UserId).ToListAsync();
                                if (ddBindUsers == null || ddBindUsers.Count() == 0)
                                {
                                    dDBindUsers.Add(new DDBindUser()
                                    {
                                        UserId = item.UserId,
                                        DDUserId = (needUsers.FirstOrDefault()).UserId
                                    });

                                    //将已经绑定的钉钉用户是否绑定改为true
                                    await UnitWork.UpdateAsync<DDUserMsg>(q => q.UserId == (needUsers.FirstOrDefault()).UserId, q => new DDUserMsg
                                    {
                                        IsBind = true
                                    });

                                    await UnitWork.SaveAsync();
                                }
                            }
                        }

                        //4.0用户绑定钉钉
                        if (dDBindUsers != null && dDBindUsers.Count() > 0)
                        {
                            await UnitWork.BatchAddAsync<DDBindUser>(dDBindUsers.ToArray());
                        }

                        await UnitWork.SaveAsync();
                        await transaction.CommitAsync();
                    }
                    else
                    {
                        result.Code = 500;
                        result.Message = "没有要绑定的钉钉用户";
                        return result;
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("自动绑定失败:" + ex.Message.ToString());
                }
            }

            result.Message = "自动绑定钉钉用户成功";
            return result;
        }

        /// <summary>
        /// 手动绑定用户
        /// </summary>
        /// <param name="ddUpdateBindUserParam">手动绑定用户参数实体</param>
        /// <returns>返回手动绑定结果</returns>
        public async Task<TableData> UpdateBindUser(DDUpdateBindUserParam ddUpdateBindUserParam)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            if (string.IsNullOrEmpty(ddUpdateBindUserParam.UserId) || string.IsNullOrEmpty(ddUpdateBindUserParam.DDUserId))
            {
                result.Code = 500;
                result.Message = "用户id或钉钉用户id不能为空";
            }
            else
            {
                var ddUsers = await UnitWork.Find<DDUserMsg>(r => r.UserId == ddUpdateBindUserParam.DDUserId && r.IsBind == true).ToListAsync();
                if (ddUsers != null && ddUsers.Count() > 0)
                {
                    result.Code = 500;
                    result.Message = "该用户id已经被绑定，不允许重复绑定";
                }
                else
                {
                    var dbContext = UnitWork.GetDbContext<DDBindUser>();
                    using (var transaction = await dbContext.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            //绑定用户
                            await UnitWork.AddAsync<DDBindUser>(new DDBindUser()
                            {
                                UserId = ddUpdateBindUserParam.UserId,
                                DDUserId = ddUpdateBindUserParam.DDUserId
                            });

                            //修改用户绑定状态
                            await UnitWork.UpdateAsync<DDUserMsg>(r => r.UserId == ddUpdateBindUserParam.DDUserId, r => new DDUserMsg()
                            {
                                IsBind = true
                            });

                            await UnitWork.SaveAsync();
                            await transaction.CommitAsync();
                            result.Message = "手动绑定成功";
                        }
                        catch (Exception ex)
                        {
                            await transaction.RollbackAsync();
                            result.Code = 500;
                            result.Message = "手动绑定失败：" + ex.Message.ToString();
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 解除绑定
        /// </summary>
        /// <param name="ddUserId">钉钉用户Id</param>
        /// <returns>返回解绑结果</returns>
        public async Task<TableData> DeleteBindUser(string ddUserId)
        {
            var result = new TableData();
            if (string.IsNullOrEmpty(ddUserId))
            {
                result.Code = 500;
                result.Message = "id不能为空";
            }
            else
            {
                await UnitWork.DeleteAsync<DDBindUser>(r => r.DDUserId == ddUserId);
                await UnitWork.UpdateAsync<DDUserMsg>(r => r.UserId == ddUserId, r => new DDUserMsg()
                {
                    IsBind = false
                });

                await UnitWork.SaveAsync();
                result.Message = "解绑成功";
            }

            return result;
        }

        /// <summary>
        /// 查询未绑定用户信息
        /// </summary>
        /// <param name="query">查询未绑定用户实体</param>
        /// <returns>返回未绑定用户信息</returns>
        public async Task<TableData> GetNotBindUser(QueryDDUserMsg query)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //条件查询未绑定用户信息
            var obj = UnitWork.Find<DDUserMsg>(r => r.IsBind == false);
            var ddUsers = (obj.WhereIf(!string.IsNullOrEmpty(query.UserId), r => r.UserId.Contains(query.UserId))
                                   .WhereIf(!string.IsNullOrEmpty(query.UserName), r => r.UserName.Contains(query.UserName))
                                   .WhereIf(!string.IsNullOrEmpty(query.UserPhone), r => r.UserPhone.Contains(query.UserPhone))).ToList();

            if (ddUsers != null && ddUsers.Count() > 0)
            {
                //查询未绑定部门信息
                var objDepart = UnitWork.Find<DDDepartMsg>(r => (ddUsers.Select(x => x.UserId).ToList()).Contains(r.UserId));
                var ddDeparts = objDepart.WhereIf(!string.IsNullOrEmpty(query.DepartName), r => r.DepartName.Contains(query.DepartName)).ToList();

                //合并部门
                List<QueryDDUserMsg> queryDDUserMsgs = new List<QueryDDUserMsg>();
                foreach (DDUserMsg item in ddUsers)
                {
                    string departName = string.Join(",", (ddDeparts.GroupBy(r => new { r.UserId, r.DepartName}).Where(r => r.Key.UserId == item.UserId).Select(r => r.Key.DepartName)).ToList());
                    queryDDUserMsgs.Add(new QueryDDUserMsg()
                    {
                        UserId = item.UserId,
                        UserName = item.UserName,
                        UserPhone = item.UserPhone,
                        DepartName = departName
                    });
                }

                result.Data = queryDDUserMsgs.Skip((query.page - 1) * query.limit).Take(query.limit).ToList();
                result.Count = ddUsers.Count();
            }
           
            return result;
        }

        /// <summary>
        /// 获取一级部门
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> GetOneLevelDeparts()
        {
            var result = new TableData();
            if (access_token != "")
            {
                try
                {
                    List<DDDepartMsgs> dDDepartMsgs = new List<DDDepartMsgs>();
                    DDDepartResult dDDepartResult = new DDDepartResult();

                    //钉钉推送文本消息实体
                    DDDepartParam dDDepartParam = new DDDepartParam()
                    {
                        dept_id = 1,
                        language = "zh_CN"
                    };

                    //调用钉钉官方接口获取部门列表
                    dDDepartResult = JsonConvert.DeserializeObject<DDDepartResult>(HttpHelpers.HttpPostAsync($"https://oapi.dingtalk.com/topapi/v2/department/listsub?access_token={access_token}", JsonConvert.SerializeObject(dDDepartParam)).Result);
                    _logger.LogError(dDDepartResult.errmsg);
                    if (dDDepartResult != null && dDDepartResult.errmsg == "ok")
                    {
                        foreach (DDDepartResultMsg item in dDDepartResult.result)
                        {
                            dDDepartMsgs.Add(new DDDepartMsgs()
                            {
                                departId = item.dept_id,
                                departName = item.name
                            });
                        }
                    }

                    result.Data = dDDepartMsgs;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message.ToString());
                    result.Message = ex.Message.ToString();
                    result.Code = 500;
                }
            }

            return result;
        }
    }
}
