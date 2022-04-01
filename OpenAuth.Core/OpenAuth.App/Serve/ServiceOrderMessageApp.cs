using Autofac.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Serve.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Serve
{
    public class ServiceOrderMessageApp : OnlyUnitWorkBaeApp
    {
        private HttpHelper _helper;
        private readonly ServiceOrderApp _serviceOrderApp;
        private IOptions<AppSetting> _appConfiguration;
        private readonly UserManagerApp _userManagerApp;

        public ServiceOrderMessageApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration, ServiceOrderApp serviceOrderApp, UserManagerApp userManagerApp) : base(unitWork, auth)
        {
            _helper = new HttpHelper(appConfiguration.Value.AppPushMsgUrl);
            _serviceOrderApp = serviceOrderApp;
            _appConfiguration = appConfiguration;
            _userManagerApp = userManagerApp;
        }

        public async Task<dynamic> GetServiceOrderMessages(int serviceOrderId)
        {
            var messageList = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == serviceOrderId && s.FroTechnicianName != "系统")
                .Include(s => s.ServiceOrderMessagePictures).ToListAsync();
            var ids = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(serviceOrderId)).Select(s => s.Id).ToListAsync();
            var ServiceOrderLogs = await UnitWork.Find<ServiceOrderLog>(s => s.ServiceOrderId.Equals(serviceOrderId) || ids.Contains((int)s.ServiceWorkOrderId)).ToListAsync();
            ServiceOrderLogs = ServiceOrderLogs.GroupBy(o => new { o.Action, o.CreateTime }).Select(o => o.First()).ToList();
            var loglist = ServiceOrderLogs.Select(s => new ServiceOrderMessage
            {
                CreateTime = s.CreateTime,
                Replier = "系统",
                Content = s.Action
            });
            messageList.AddRange(loglist);
            //var groupList = list.GroupBy(s => s.FroTechnicianName).ToList().Select(s => new { s.Key, Data = s.ToList() });

            return messageList.OrderByDescending(s => s.CreateTime).ToList();
        }

        /// <summary>
        /// 撤回信息
        /// </summary>
        /// <returns></returns>
        public async Task<Infrastructure.Response> RecallMessage(string messageId)
        {
            var response = new Infrastructure.Response();

            var loginContext = _auth.GetCurrentUser();
            var userId = loginContext.User.Id ?? "";
            var orgInfo = await _userManagerApp.GetUserOrgInfo(userId); //部门信息
            if (orgInfo?.OrgName != "S19")
            {
                response.Code = 500;
                response.Message = "非S19人员不能撤回信息";
            }
            var obj = await UnitWork.Find<ServiceOrderMessage>(null).FirstOrDefaultAsync(s => s.Id == messageId);
            if (obj != null)
            {
                if (userId != obj.ReplierId)
                {
                    response.Code = 500;
                    response.Message = "只能撤回自己创建的消息";
                }

                await UnitWork.DeleteAsync<ServiceOrderMessage>(s => s.Id == messageId);
                await UnitWork.SaveAsync();
            }
            else
            {
                response.Code = 500;
                response.Message = "只能撤回自己创建的消息";
            }

            return response;
        }

        public async Task SendMessageToTechnician(SendMessageToTechnicianReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            var userMap = await UnitWork.Find<AppUserMap>(u => u.UserID.Equals(loginContext.User.Id)).FirstOrDefaultAsync();
            if (userMap == null)
            {
                throw new CommonException("需要绑定App账户后才可发消息", Define.INVALID_APPUser);
            }
            var obj = req.MapTo<ServiceOrderMessage>();
            req.AppUserId = Convert.ToInt32(userMap.AppUserId);
            obj.Replier = loginContext.User.Name;
            obj.ReplierId = loginContext.User.Id;
            obj.CreateTime = DateTime.Now;
            var pictures = req.ServiceOrderMessagePictures;
            obj.ServiceOrderMessagePictures = null;
            if (req.MessageType==1)
            {
                var num = await UnitWork.Find<ServiceOrderMessage>(c => c.ServiceOrderId == req.ServiceOrderId && c.MessageType == 1).CountAsync();
                obj.Content = $"{obj.Replier}发起第{(num + 1)}次催办";
            } 
            await UnitWork.AddAsync(obj);
            await UnitWork.SaveAsync();
            if (pictures != null && pictures?.Count > 0)
            {
                pictures?.ForEach(p => { p.ServiceOrderMessageId = obj.Id; p.Id = Guid.NewGuid().ToString(); });
                await UnitWork.BatchAddAsync(pictures?.ToArray());
                await UnitWork.SaveAsync();
            }
            //发送消息给相关人员
            await _serviceOrderApp.SendMessageToRelatedUsers(req.Content, (int)req.ServiceOrderId, Convert.ToInt32(userMap.AppUserId), obj.Id);
            await PushMessageToApp(req.AppUserId, "服务单消息", $"{obj.Replier}给你发送消息：{obj.Content}");
        }

        /// <summary>
        /// 推送消息至新威智能app
        /// </summary>
        /// <param name="userId">app用户Id</param>
        /// <param name="title">消息标题</param>
        /// <param name="content">消息内容</param>
        /// <returns></returns>
        public async Task PushMessageToApp(int userId, string title, string content)
        {
            var timespan = DatetimeUtil.ToUnixTimestampBySeconds(DateTime.Now.AddMinutes(5));
            var text = $"NewareApiTokenDeadline:{timespan}";
            var aes = Encryption.AESEncrypt(text);

            _helper.Post(new
            {
                UserId = userId,
                Title = title,
                Content = content
            }, (string.IsNullOrEmpty(_appConfiguration.Value.AppVersion) ? string.Empty : _appConfiguration.Value.AppVersion + "/") + "BbsCommunity/AppPushMsg", "EncryToken", aes);
        }
    }
}
