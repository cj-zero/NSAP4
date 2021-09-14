﻿using Autofac.Core;
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
        public ServiceOrderMessageApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration, ServiceOrderApp serviceOrderApp) : base(unitWork, auth)
        {
            _helper = new HttpHelper(appConfiguration.Value.AppPushMsgUrl);
            _serviceOrderApp = serviceOrderApp;
        }

        public async Task<dynamic> GetServiceOrderMessages(int serviceOrderId)
        {
            var messageList = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == serviceOrderId && s.FroTechnicianName!="系统")
                .Include(s=>s.ServiceOrderMessagePictures).ToListAsync();
            var ids = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(serviceOrderId)).Select(s => s.Id).ToListAsync();
            var ServiceOrderLogs = await UnitWork.Find<ServiceOrderLog>(s => s.ServiceOrderId.Equals(serviceOrderId) || ids.Contains((int)s.ServiceWorkOrderId)).ToListAsync();
            ServiceOrderLogs = ServiceOrderLogs.GroupBy(o => new { o.Action, o.CreateTime }).Select(o => o.First()).ToList();
            var loglist = ServiceOrderLogs.Select(s => new ServiceOrderMessage
            {
                CreateTime = s.CreateTime,
                Replier ="系统",
                Content = s.Action
            });
            messageList.AddRange(loglist);
            //var groupList = list.GroupBy(s => s.FroTechnicianName).ToList().Select(s => new { s.Key, Data = s.ToList() });

            return messageList.OrderByDescending(s => s.CreateTime).ToList();
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
        private async Task PushMessageToApp(int userId, string title, string content)
        {
            #region 获取token
            var erpUserId = await UnitWork.Find<User>(c => c.Account == "admin").Select(c => c.Id).FirstOrDefaultAsync();
            var appUserId = await UnitWork.Find<AppUserMap>(c => c.UserID == erpUserId).Select(c => c.AppUserId).FirstOrDefaultAsync();
            var key = System.Web.HttpUtility.UrlEncode(Encryption.EncryptRSA(appUserId.ToString()));
            var result = _helper.Get<Dictionary<string, string>>(new Dictionary<string, string> { { "ciphertext", key } }, "Account/GetUserInfoFromErp");
            var token = result["Data"];
            #endregion

            _helper.Post(new
            {
                UserId = userId,
                Title = title,
                Content = content
            }, "BbsCommunity/AppPushMsg", "ErpAuthorize", $"Neware {token}");
        }
    }
}
