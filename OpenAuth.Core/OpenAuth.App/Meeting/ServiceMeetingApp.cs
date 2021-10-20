using Infrastructure;
using Microsoft.Extensions.Logging;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting.Request;
using OpenAuth.Repository.Interface;
using System;
using OpenAuth.Repository.Domain.Serve;

using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Meeting
{
    public partial class ServiceMeetingApp : OnlyUnitWorkBaeApp
    {

        private readonly RevelanceManagerApp _revelanceApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private readonly ServiceFlowApp _serviceFlowApp;
        ServiceBaseApp _serviceBaseApp;
        private ILogger<ServiceMeetingApp> _logger;

        public ServiceMeetingApp(IUnitWork unitWork, ILogger<ServiceMeetingApp> logger, RevelanceManagerApp app, ServiceBaseApp serviceBaseApp, ServiceOrderLogApp serviceOrderLogApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, ServiceOrderLogApp ServiceOrderLogApp, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
            _logger = logger;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _ServiceOrderLogApp = ServiceOrderLogApp;
            _serviceFlowApp = serviceFlowApp;
            _serviceBaseApp = serviceBaseApp;
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="addmodel"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public string AddMeetingData(AddMeetingDataReq addmodel)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            Meetings meeting = new Meetings
            {
                Name = addmodel.Name,
                Title = addmodel.Title,
                Introduce = addmodel.Introduce,
                StartTime = addmodel.StartTime,
                EndTime = addmodel.EndTime,
                Address = addmodel.Address,
                Status = addmodel.Status,
                ApplyUserId = addmodel.ApplyUserId,
                ApplyUser = addmodel.ApplyUser,
                DempIdtId = addmodel.DempIdtId,
                ApplyDempName = addmodel.ApplyDempName,
                Contact = addmodel.Contact,
                FollowPerson = addmodel.FollowPerson,
                SponsorUnit = addmodel.SponsorUnit,
                GuideUnit = addmodel.GuideUnit,
                ApplyReason = addmodel.ApplyReason,
                ConferenceScale = addmodel.ConferenceScale,
                UserNumberLimit = addmodel.UserNumberLimit,
                Funds = addmodel.Funds,
                Position = addmodel.Position,
                MeasureOfArea = addmodel.MeasureOfArea,
                ProductType = addmodel.ProductType,
                IsDinner = addmodel.IsDinner,
                BulidType = addmodel.BulidType,
                IsSign = addmodel.IsSign,
                Remark = addmodel.Remark,
                CreateUser = loginUser.Name,
                CreateTime = DateTime.Now
            };
            var data = UnitWork.Add<Meetings, int>(meeting);
            UnitWork.Save();

            foreach (var item in addmodel.FileList)
            {
                MeetingFile file = new MeetingFile
                {
                    FileUrl = item.FileUrl,
                    Name = item.Name,
                    Type = item.Type,
                    Remake = item.Remake,
                    UploadTime = item.UploadTime,
                    MeetingId = data.Id

                };
                UnitWork.Add<MeetingFile, int>(file);

            }
            UnitWork.Save();
            return "1";
        }
    }
}
