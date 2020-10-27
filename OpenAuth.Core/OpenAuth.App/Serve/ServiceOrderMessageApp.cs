using Autofac.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
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
            var list = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == serviceOrderId).Include(s=>s.ServiceOrderMessagePictures).OrderByDescending(s => s.CreateTime).ToListAsync();

            //var groupList = list.GroupBy(s => s.FroTechnicianName).ToList().Select(s => new { s.Key, Data = s.ToList() });

            return list;
        }

        public async Task SendMessageToTechnician(SendMessageToTechnicianReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            var obj = req.MapTo<ServiceOrderMessage>();
            obj.AppUserId = 0;
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
            await _serviceOrderApp.SendMessageToRelatedUsers(req.Content, (int)req.ServiceOrderId, 0, obj.Id);
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
            _helper.Post(new
            {
                UserId = userId,
                Title = title,
                Content = content
            }, "BbsCommunity/AppPushMsg");
        }
    }
}
