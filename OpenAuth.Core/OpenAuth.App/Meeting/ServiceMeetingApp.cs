using Infrastructure;
using Microsoft.Extensions.Logging;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting.Request;
using System;
using OpenAuth.Repository.Domain.Serve;
using System.Collections.Generic;
using System.Text;
using OpenAuth.App.Meeting.ModelDto;
using OpenAuth.Repository.Interface;
using System.Linq.Expressions;
using AutoMapper;

namespace OpenAuth.App.Meeting
{
    public partial class ServiceMeetingApp : OnlyUnitWorkBaeApp
    {
        private IMapper _mapper;
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly AppServiceOrderLogApp _appServiceOrderLogApp;
        private readonly ServiceOrderLogApp _ServiceOrderLogApp;
        private readonly ServiceFlowApp _serviceFlowApp;
        ServiceBaseApp _serviceBaseApp;
        private ILogger<ServiceMeetingApp> _logger;

        public ServiceMeetingApp(IMapper mapper, IUnitWork unitWork, ILogger<ServiceMeetingApp> logger, RevelanceManagerApp app, ServiceBaseApp serviceBaseApp, ServiceOrderLogApp serviceOrderLogApp, IAuth auth, AppServiceOrderLogApp appServiceOrderLogApp, ServiceOrderLogApp ServiceOrderLogApp, ServiceFlowApp serviceFlowApp) : base(unitWork, auth)
        {
            _logger = logger;
            _appServiceOrderLogApp = appServiceOrderLogApp;
            _ServiceOrderLogApp = ServiceOrderLogApp;
            _serviceFlowApp = serviceFlowApp;
            _serviceBaseApp = serviceBaseApp;
            _mapper = mapper;
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
                ApplyUserId = addmodel.ApplyUserId,
                ApplyUser = addmodel.ApplyUser,
                DempId = addmodel.DempId,
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
            if (addmodel.Ations == MeetingAtion.Draft)
            {
                meeting.Status = 0;
            }
            else if (addmodel.Ations == MeetingAtion.Submit)
            {
                meeting.Status = 1;
            }
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
            MeetingDraft draft = new MeetingDraft();
            draft.Base_entry = data.Id;
            draft.CreateUser = loginUser.Name;
            draft.Type = 1;
            draft.CreateTime = DateTime.Now;
            draft.Name = "展会申请";
            draft.Remark = data.Remark;
            if (addmodel.Ations == MeetingAtion.Draft)
            {
                draft.Step = 0;
            }
            else if (addmodel.Ations == MeetingAtion.Submit)
            {
                draft.Step = 1;
            }
            UnitWork.Add<MeetingDraft, int>(draft);
            UnitWork.Save();
            return "1";
        }

        public List<QueryListDto> Load(LoadReq querymodel, out int rowcount)
        {

            Expression<Func<Meetings, bool>> exp = t => true;
            exp = exp.And(t => !t.IsDelete);
            exp = exp.And(t => t.Status == 3);
            if (!string.IsNullOrEmpty(querymodel.Name))
            {
                exp = exp.And(t => t.Name == querymodel.Name);

            }
            if (!string.IsNullOrEmpty(querymodel.StartTime))
            {
                DateTime startTime;
                DateTime.TryParse(querymodel.StartTime, out startTime);
                exp = exp.And(t => t.StartTime <= startTime);
            }
            if (!string.IsNullOrEmpty(querymodel.EndTime))
            {
                DateTime endTime;
                DateTime.TryParse(querymodel.EndTime, out endTime);
                exp = exp.And(t => t.EndTime <= endTime);
            }
            exp = exp.And(t => t.IsDinner == true);
            if (querymodel.DempId != 0)
            {
                exp = exp.And(t => t.DempId == querymodel.DempId);
            }
            if (!string.IsNullOrEmpty(querymodel.ApplyUser))
            {
                exp = exp.And(t => t.ApplyUser == querymodel.ApplyUser);
            }
            if (!string.IsNullOrEmpty(querymodel.FollowPerson))
            {
                exp = exp.And(t => t.FollowPerson == querymodel.FollowPerson);
            }
            if (querymodel.Status != 0)
            {
                exp = exp.And(t => t.Status == querymodel.Status);
            }
            var objs = UnitWork.Find<Meetings>(querymodel.page, querymodel.limit, "", exp);
            var list = objs.MapToList<Meetings>();
            rowcount = UnitWork.GetCount(exp);
            var data = new List<QueryListDto>();
            foreach (var obj in list)
            {
                var nes = new QueryListDto();
                obj.CopyTo(nes);
                data.Add(nes);
            }
            return data;
        }
    }
}
