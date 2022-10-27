using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using System.Threading.Tasks;
using OpenAuth.App.DDVoice.Common;
using OpenAuth.App.DDVoice.EntityHelp;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository.Domain;

namespace OpenAuth.App.DDVoice
{
    public class DDVoiceApp : OnlyUnitWorkBaeApp
    {
        private DDSettingHelp _ddSettingHelp;
        private List<DDDepartMsg> afterDeptIds = new List<DDDepartMsg>();
        private List<DDDepartMsg> originalDepart = new List<DDDepartMsg>() { new DDDepartMsg() { departId = 1, departName = "深圳市新威尔电子有限公司" } };
        private ILogger<DDVoiceApp> _logger;
        private IUnitWork _UnitWork;
        private IAuth _auth;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dDSettingHelp"></param>
        /// <param name="logger"></param>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public DDVoiceApp(DDSettingHelp ddSettingHelp,ILogger<DDVoiceApp> logger, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _logger = logger;
            _UnitWork = unitWork;
            _auth = auth;
            _ddSettingHelp = ddSettingHelp;
        }

        /// <summary>
        /// 钉钉登录
        /// </summary>
        /// <returns>返回token</returns>
        public async Task<string> DDLogin()
        {
            var loginContext = _auth.GetCurrentUser();
            string access_token = "";
            string appkey = _ddSettingHelp.GetDDKey("Appkey");//获取配置文件中钉钉Appkey
            string appsecret = _ddSettingHelp.GetDDKey("Appsecret");//获取配置文件中钉钉Appsecret
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //获取钉钉登录token
            DDLogin ddLogin = JsonConvert.DeserializeObject<DDLogin>(HttpHelpers.Get($"https://oapi.dingtalk.com/gettoken?appkey={appkey}&appsecret={appsecret}"));
            if (ddLogin == null || string.IsNullOrEmpty(ddLogin.access_token))
            {
                //钉钉获取token失败
                await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                {
                    MsgType = "钉钉获取token",
                    MsgContent = ddLogin.errmsg,
                    MsgResult = "失败",
                    CreateName = loginContext.User.Name,
                    CreateUserId = loginContext.User.Id,
                    CreateTime = DateTime.Now
                });
            }
            else
            {
                //钉钉获取token成功
                await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                {
                    MsgType = "钉钉获取token",
                    MsgContent = ddLogin.errmsg,
                    MsgResult = "成功",
                    CreateName = loginContext.User.Name,
                    CreateUserId = loginContext.User.Id,
                    CreateTime = DateTime.Now
                });

                access_token = ddLogin.access_token;
            }

            await UnitWork.SaveAsync();
            return access_token;
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
            string access_token = await DDLogin();
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
        public async Task<List<DDDepartMsg>> DDDepartMsg(List<DDDepartMsg> dept_ids)
        {
            string access_token = await DDLogin();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            if (access_token != "")
            {
                try
                {
                    DDDepartResult dDDepartResult = new DDDepartResult();
                    foreach (DDDepartMsg dDDepartMsg in dept_ids.ToArray())
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
                                DDDepartMsg ddmsg = new DDDepartMsg();
                                ddmsg.departId = result.dept_id;
                                ddmsg.departName = result.name;
                                afterDeptIds.Add(ddmsg);
                                dept_ids.Add(ddmsg);
                                dept_ids.Remove(dDDepartMsg);
                            }
                        }
                        else
                        {
                            //去重获取所有部门Id
                            return afterDeptIds.Where((x, i) => afterDeptIds.FindIndex(s => s.departId == x.departId) == i).ToList();
                        }
                    }

                    //钉钉操作历史记录
                    await UnitWork.AddAsync<DDSendMsgHitory>(new DDSendMsgHitory()
                    {
                        MsgType = "钉钉获取部门列表",
                        MsgContent = dDDepartResult.errmsg,
                        MsgResult = dDDepartResult.errmsg == "ok" ? "成功" : "失败",
                        CreateName = loginContext.User.Name,
                        CreateUserId = loginContext.User.Id,
                        CreateTime = DateTime.Now
                    });

                    await UnitWork.SaveAsync();
                    return await DDDepartMsg(dept_ids);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message.ToString());
                }
            }

            return afterDeptIds;
        }

        /// <summary>
        /// 获取钉钉部门用户最新信息
        /// </summary>
        /// <returns></returns>
        public async Task GetDDLasterDepartUserMsg()
        {
            var loginContext = _auth.GetCurrentUser();       
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //获取部门用户信息，如果存在删除，拉取最新的部门用户信息
            List<DDUserDepartMsg> ddUser = await UnitWork.Find<DDUserDepartMsg>(null).ToListAsync();
            if (ddUser != null && ddUser.Count() > 0)
            {
                await DeleteUserDepart(ddUser);
            }

            //获取用户和钉钉用户绑定信息，如果存在删除，重新绑定
            List<DDBindUser> ddBindUsers = await UnitWork.Find<DDBindUser>(null).ToListAsync();
            if (ddBindUsers != null && ddBindUsers.Count() > 0)
            {
                await DeleteDDBindUser(ddBindUsers);
            }

            //部门用户信息集合
            List<DDUserDepartMsg> departUserMsgs = new List<DDUserDepartMsg>();

            //获取所有部门信息
            List<DDDepartMsg> dDDepartMsgs = await DDDepartMsg(originalDepart);
            try
            {
                if (dDDepartMsgs != null && dDDepartMsgs.Count() > 0)
                {
                    //获取token
                    string access_token = await DDLogin();
                    if (access_token != "")
                    {
                        foreach (DDDepartMsg dDDepartMsg in dDDepartMsgs)
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
                                foreach (DDUserMsgs userItem in dDBaseUserResult.result.list)
                                {
                                    //循环获取用户信息
                                    DDUserDepartMsg dDUserDepartMsg = new DDUserDepartMsg()
                                    {
                                        DepartId = dDDepartMsg.departId,
                                        DepartName = dDDepartMsg.departName,
                                        UserId = userItem.userid,
                                        UserName = userItem.name,
                                        UserPhone = userItem.mobile,
                                        IsBind = false,
                                        CreateUserId = loginContext.User.Id,
                                        CreateName = loginContext.User.Name,
                                        CreateTime = DateTime.Now
                                    };

                                    departUserMsgs.Add(dDUserDepartMsg);
                                }
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

                        await UnitWork.BatchAddAsync<DDUserDepartMsg>(departUserMsgs.ToArray());
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
        public async Task<string> GetAutoDDBindUser()
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            //查询部门用户信息
            List<DDNeedBindUserMsg> ddUsers = await UnitWork.Find<DDUserDepartMsg>(r => r.IsBind == false).Select(r => new DDNeedBindUserMsg { UserId = r.UserId, UserName = r.UserName}).ToListAsync();
            var dbContext = UnitWork.GetDbContext<ContractApply>();
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
                                dDBindUsers.Add(new DDBindUser()
                                {
                                    UserId = item.UserId,
                                    DDUserId = (needUsers.FirstOrDefault()).UserId
                                });
                            }
                        }

                        //4.0用户绑定钉钉
                        if (dDBindUsers != null && dDBindUsers.Count() > 0)
                        {
                            await UnitWork.BatchAddAsync<DDBindUser>(dDBindUsers.ToArray());
                            foreach (DDBindUser dDBindUser in dDBindUsers)
                            {
                                //将已经绑定的钉钉用户是否绑定改为true
                                await UnitWork.UpdateAsync<DDUserDepartMsg>(q => q.UserId == dDBindUser.DDUserId, q => new DDUserDepartMsg
                                {
                                    IsBind = true
                                });

                                await UnitWork.SaveAsync();
                            }
                        }

                        await UnitWork.SaveAsync();
                        await transaction.CommitAsync();
                    }
                    else
                    {
                        return "没有要绑定的钉钉用户";
                    }
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("自动绑定失败:" + ex.Message.ToString());
                }
            }

            return "自动绑定钉钉用户成功";
        }

        /// <summary>
        /// 删除钉钉用户部门信息
        /// </summary>
        /// <param name="dDUserDepartMsgs">部门用户信息实体</param>
        /// <returns></returns>
        public async Task DeleteUserDepart(List<DDUserDepartMsg> dDUserDepartMsgs)
        {
            await UnitWork.BatchDeleteAsync<DDUserDepartMsg>(dDUserDepartMsgs.ToArray());
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 删除绑定钉钉用户
        /// </summary>
        /// <param name="dDBindUsers">钉钉绑定用户实体</param>
        /// <returns></returns>
        public async Task DeleteDDBindUser(List<DDBindUser> dDBindUsers)
        {
            await UnitWork.BatchDeleteAsync<DDBindUser>(dDBindUsers.ToArray());
            await UnitWork.SaveAsync();
        }
    }
}
