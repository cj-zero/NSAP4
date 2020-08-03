using Autofac.Core;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
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
        public ServiceOrderMessageApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

        public async Task<dynamic> GetServiceOrderMessages(int serviceOrderId)
        {
            var list = await UnitWork.Find<ServiceOrderMessage>(s => s.ServiceOrderId == serviceOrderId).ToListAsync();

            var groupList = list.GroupBy(s => s.FroTechnicianName).ToList().Select(s => new { s.Key, Data = s.ToList() });
            return groupList;
        }

        public async Task SendMessageToTechnician(SendMessageToTechnicianReq req)
        {
            var loginContext = _auth.GetCurrentUser();
            var obj = req.MapTo<ServiceOrderMessage>();
            obj.Replier = loginContext.User.Name;
            obj.ReplierId = loginContext.User.Id;
            obj.CreateTime = DateTime.Now;

            await UnitWork.AddAsync(obj);
            await UnitWork.SaveAsync();


            req.ServiceOrderMessagePictures.ForEach(p => { p.ServiceOrderMessageId = obj.Id; });
            await UnitWork.BatchAddAsync(req.ServiceOrderMessagePictures.ToArray());
            await UnitWork.SaveAsync();
        }

    }
}
