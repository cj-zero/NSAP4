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
            OpenAuth.Repository.Domain.Serve.Meeting meeting = new OpenAuth.Repository.Domain.Serve.Meeting
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
            var data = UnitWork.Add<OpenAuth.Repository.Domain.Serve.Meeting, int>(meeting);
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
        /// <summary>
        /// 报名
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public bool MeetingUserApply(int meetingId)
        {
            var loginContext = _auth.GetCurrentUser();
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = _serviceBaseApp.GetSalesDepID(userId);
            var depName = _serviceBaseApp.GetSalesDepname(userId.ToString(), "1");
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (UnitWork.IsExist<MeetingUser>(zw => zw.MeetingId == meetingId && zw.UserId == userId && zw.DempId == depId && !zw.IsDelete))
            {
                return false;
            }
            var loginUser = loginContext.User;
            MeetingUser user = new MeetingUser();
            user.MeetingId = meetingId;
            user.Name = loginUser.Name;
            user.Status = 0;
            user.UserId = userId;
            user.CreateTime = DateTime.Now;
            user.DempId = depId;
            user.DempName = depName;
            UnitWork.Add<MeetingUser, int>(user);
            UnitWork.Save();
            MeetingDraft draft = new MeetingDraft();
            draft.Base_entry = user.Id;
            draft.CreateUser = loginUser.Name;
            draft.Type = 1;
            draft.CreateTime = DateTime.Now;
            draft.Name = "展会报名";
            draft.Step = 1;
            UnitWork.Add<MeetingDraft, int>(draft);
            UnitWork.Save();

            return true;
        }
        /// <summary>
        /// 我的创建详情根据单号查询
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ExhibitionDetailDto MyCreatedDetailsById(int Id)
        {
            var list = new ExhibitionDetailDto();
            var meetdraft = UnitWork.FindSingle<MeetingDraft>(q => q.Id == Id);
            if (meetdraft.Type == 0)
            {
                var objs = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(t => !t.IsDelete && t.Id == meetdraft.Base_entry);
                list = objs.MapTo<ExhibitionDetailDto>();
                Expression<Func<MeetingFile, bool>> exps = t => true;
                exps = exps.And(t => !t.IsDelete && t.MeetingId == objs.Id);
                var filelist = UnitWork.Find(exps);
                var s = filelist.MapToList<FileDto>();
                list.FileList.AddRange(s);
            }
            else if (meetdraft.Type == 1)
            {
                var meetuser = UnitWork.FindSingle<MeetingUser>(q => q.Id == Id);
                var objs = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(t => !t.IsDelete && t.Id == meetuser.MeetingId);
                list = objs.MapTo<ExhibitionDetailDto>();
                Expression<Func<MeetingFile, bool>> exps = t => true;
                exps = exps.And(t => !t.IsDelete && t.MeetingId == objs.Id);
                var filelist = UnitWork.Find(exps);
                var s = filelist.MapToList<FileDto>();
                list.FileList.AddRange(s);
            }
            return list;
        }




        /// <summary>
        /// 调度-调度操作
        /// </summary>
        /// <param name="Updatemodel"></param>
        /// <returns></returns>
        public bool Scheduling(List<SchedulingReq> Updatemodel)
        {
            var loginContext = _auth.GetCurrentUser();
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = _serviceBaseApp.GetSalesDepID(userId);
            var depName = _serviceBaseApp.GetSalesDepname(userId.ToString(), "1");
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;

            foreach (var item in Updatemodel)
            {
                MeetingOpreateLog opreateLog = new MeetingOpreateLog();

                opreateLog.MeetingId = item.oldMeetingId;
                MeetingDispatch dispatch = new MeetingDispatch();
                opreateLog.Json = JsonHelper.Instance.Serialize(item);
                opreateLog.Type = 1;
                opreateLog.Log += loginUser.Name + "进行调度操作，将" + JsonHelper.Instance.Serialize(item.SchedulingerList) + "从原会议ID为" + item.oldMeetingId + "调度到会议ID为" + item.MeetingId + "\r";
                opreateLog.CreateUser = loginUser.Name;
                UnitWork.Add<MeetingOpreateLog, int>(opreateLog);

                dispatch.FromMeetingId = item.oldMeetingId;
                dispatch.ToMeetingId = item.MeetingId;
                dispatch.Reason = item.Reason;
                dispatch.CreateUser = loginUser.Name;
                dispatch.CreateTime = DateTime.Now;
                foreach (var scon in item.SchedulingerList)
                {
                    var objs = UnitWork.FindSingle<MeetingUser>(q => q.MeetingId == item.oldMeetingId && q.UserId == scon.UserId);
                    objs.IsDelete = true;
                    UnitWork.Update<MeetingUser>(objs);
                    MeetingUser user = new MeetingUser();
                    user.MeetingId = item.MeetingId;
                    user.Name = scon.Name;
                    user.Status = 0;
                    user.UserId = scon.UserId;
                    user.CreateTime = DateTime.Now;
                    user.DempId = objs.DempId;
                    user.DempName = objs.DempName;
                    UnitWork.Add<MeetingUser, int>(user);
                }
                dispatch.UserJson = JsonHelper.Instance.Serialize(item.SchedulingerList);
                UnitWork.Add<MeetingDispatch, int>(dispatch);

            }
            UnitWork.Save();

            return true;
        }


        /// <summary>
        /// 调度-调度人
        /// </summary>
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        public List<DispatcherDto> Dispatcher(DispatcherReq Querymodel, out int rowcount)
        {
            Expression<Func<MeetingUser, bool>> exps = t => true;
            exps = exps.And(t => !t.IsDelete && t.MeetingId == Querymodel.MeetingId);
            if (!string.IsNullOrWhiteSpace(Querymodel.Name))
            {
                exps = exps.And(t => t.Name.Contains(Querymodel.Name));

            }

            var meetingUser = UnitWork.Find(Querymodel.page, Querymodel.limit, "", exps);
            rowcount = UnitWork.GetCount(exps);
            return meetingUser.MapToList<DispatcherDto>();
        }
        /// <summary>
        /// 报名人数查看列表
        /// </summary>
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        public List<MeetingUserListDto> MeetingUserList(MeetingUserListReq Querymodel, out int rowcount)
        {
            Expression<Func<MeetingUser, bool>> exps = t => true;
            exps = exps.And(t => !t.IsDelete && t.MeetingId == Querymodel.MeetingId);
            var meetingUser = UnitWork.Find(Querymodel.page, Querymodel.limit, "", exps);
            rowcount = UnitWork.GetCount(exps);
            return meetingUser.MapToList<MeetingUserListDto>();
        }
        /// <summary>
        /// 展会详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ExhibitionDetailDto ExhibitionDetailById(int Id)
        {
            Expression<Func<OpenAuth.Repository.Domain.Serve.Meeting, bool>> exp = t => true;
            exp = exp.And(t => !t.IsDelete && t.Id == Id);
            var objs = UnitWork.FindSingle(exp);
            var list = objs.MapTo<ExhibitionDetailDto>();
            Expression<Func<MeetingFile, bool>> exps = t => true;
            exps = exps.And(t => !t.IsDelete && t.MeetingId == Id);
            var filelist = UnitWork.Find(exps);
            var s = filelist.MapToList<FileDto>();
            list.FileList.AddRange(s);
            return list;
        }
        /// <summary>
        /// 展会列表
        /// </summary>
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        public List<QueryListDto> Load(LoadReq querymodel, out int rowcount)
        {

            Expression<Func<OpenAuth.Repository.Domain.Serve.Meeting, bool>> exp = t => true;
            exp = exp.And(t => !t.IsDelete);
            //exp = exp.And(t => t.Status == 3);
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
            var objs = UnitWork.Find<OpenAuth.Repository.Domain.Serve.Meeting>(querymodel.page, querymodel.limit, "", exp);
            var list = objs.MapToList<OpenAuth.Repository.Domain.Serve.Meeting>();
            rowcount = UnitWork.GetCount(exp);
            var data = new List<QueryListDto>();
            foreach (var obj in list)
            {
                var nes = new QueryListDto();

                nes.Address = obj.Address;
                nes.Name = obj.Name;
                nes.StartTime = obj.StartTime;
                nes.EndTime = obj.EndTime;
                nes.Status = obj.Status;
                nes.IsDinner = obj.IsDinner;
                nes.ApplyDempName = obj.ApplyDempName;
                nes.ApplyUser = obj.ApplyUser;
                nes.FollowPerson = obj.FollowPerson;
                var obe = UnitWork.GetCount<MeetingDraft>(x => x.Base_entry == obj.Id);
                nes.number = obe;
                data.Add(nes);
            }
            return data;
        }
        /// <summary>
        /// 我创建的列表
        /// </summary>
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        public List<MyCreatedLoadListDto> MyCreatedLoad(MyCreatedLoadReq querymodel, out int rowcount)
        {

            Expression<Func<MeetingDraft, bool>> exp = t => true;
            if (querymodel.JobId != 0)
            {
                exp = exp.And(t => t.Id == querymodel.JobId);

            }
            if (querymodel.Type != -1)
            {
                exp = exp.And(t => t.Type == querymodel.Type);

            }
            if (querymodel.Step != -1)
            {
                exp = exp.And(t => t.Step == querymodel.Step);

            }
            if (!string.IsNullOrWhiteSpace(querymodel.JobName))
            {
                exp = exp.And(t => t.Name.Contains(querymodel.JobName));

            }
            if (!string.IsNullOrWhiteSpace(querymodel.Remark))
            {
                exp = exp.And(t => t.Remark.Contains(querymodel.JobName));

            }
            if (querymodel.Base_entry != 0)
            {
                exp = exp.And(t => t.Base_entry == querymodel.Base_entry);

            }
            var objs = UnitWork.Find<MeetingDraft>(querymodel.page, querymodel.limit, "", exp);
            var list = objs.MapToList<MeetingDraft>();
            rowcount = UnitWork.GetCount(exp);
            var data = new List<MyCreatedLoadListDto>();
            foreach (var obj in list)
            {
                var nes = new MyCreatedLoadListDto();
                obj.CopyTo(nes);
                data.Add(nes);
            }
            return data;
        }
        /// <summary>
        /// 提交给我的
        /// </summary>
        /// <param name="querymodel"></param>
        /// <param name="rowcount"></param>
        /// <returns></returns>
        public List<SubmittedDto> SubmittedLod(SubmittedReq querymodel, out int rowcount)
        {
            var loginContext = _auth.GetCurrentUser();
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = _serviceBaseApp.GetSalesDepID(userId);
            var depName = _serviceBaseApp.GetSalesDepname(userId.ToString(), "1");
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            Expression<Func<MeetingDraft, bool>> exp = t => true;
            if (querymodel.JobId != 0)
            {
                exp = exp.And(t => t.Id == querymodel.JobId);

            }
            if (querymodel.Type != -1)
            {
                exp = exp.And(t => t.Type == querymodel.Type);

            }
            if (querymodel.Step != -1)
            {
                exp = exp.And(t => t.Step == querymodel.Step);

            }
            if (!string.IsNullOrWhiteSpace(querymodel.JobName))
            {
                exp = exp.And(t => t.Name.Contains(querymodel.JobName));

            }
            if (!string.IsNullOrWhiteSpace(querymodel.Remark))
            {
                exp = exp.And(t => t.Remark.Contains(querymodel.JobName));

            }
            if (querymodel.Base_entry != 0)
            {
                exp = exp.And(t => t.Base_entry == querymodel.Base_entry);

            }
            var objs = UnitWork.Find<MeetingDraft>(querymodel.page, querymodel.limit, "", exp);
            var list = objs.MapToList<MeetingDraft>();
            rowcount = UnitWork.GetCount(exp);
            var data = new List<SubmittedDto>();
            foreach (var obj in list)
            {
                var nes = new SubmittedDto();
                var meetinguser = UnitWork.FindSingle<MeetingUser>(q => q.Id == obj.Base_entry);
                if (meetinguser.DempId == depId)
                {
                    obj.CopyTo(nes);
                    data.Add(nes);
                }

            }
            return data;
        }
        /// <summary>
        /// 提交给我的详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SubmittedDetailsDto SubmittedDetails(int id)
        {
            var data = new SubmittedDetailsDto();
            var meetingdraft = UnitWork.FindSingle<MeetingDraft>(zw => zw.Id == id);
            var meeting = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(zw => zw.Id == meetingdraft.Base_entry);
            data.Id = meetingdraft.Id;
            data.Introduce = meeting.Introduce;
            data.Name = meeting.Name;
            data.StartTime = meeting.StartTime;
            data.EndTime = meeting.EndTime;
            data.Title = meeting.Title;
            data.SponsorUnit = meeting.SponsorUnit;
            data.GuideUnit = meeting.GuideUnit;
            data.AddressType = meeting.AddressType;
            data.Address = meeting.Address;
            data.DempId = meeting.DempId;
            data.ApplyDempName = meeting.ApplyDempName;
            data.ApplyUser = meeting.ApplyUser;
            data.Contact = meeting.Contact;
            data.ApplyReason = meeting.ApplyReason;
            data.FollowPerson = meeting.FollowPerson;
            data.Funds = meeting.Funds;
            data.UserNumberLimit = meeting.UserNumberLimit;
            data.ConferenceScale = meeting.ConferenceScale;
            data.MeasureOfArea = meeting.MeasureOfArea;
            data.Position = meeting.Position;
            data.ProductType = meeting.ProductType;
            data.Remark = meeting.Remark;
            data.IsDinner = meeting.IsDinner;
            data.BulidType = meeting.BulidType;
            var meetinglist = UnitWork.Find<Repository.Domain.Serve.Meeting>(q => q.CreateUser == meeting.CreateUser);
            foreach (var item in meetinglist)
            {
                var eson = new LatestApplicationDto();
                eson.Applicant = item.CreateUser;
                eson.ApplicationTime = item.CreateTime;
                eson.StartTime = item.StartTime;
                eson.EndTime = item.EndTime;
                eson.Id = item.Id;
                eson.Name = item.Name;
                data.LatestApplicationList.Add(eson);
            }
            var file = UnitWork.Find<MeetingFile>(q => q.MeetingId == meeting.Id);
            foreach (var item in file)
            {
                var FileDto = new FileDto();
                item.CopyTo(FileDto);
                data.FileList.Add(FileDto);
            }
            var meetingOpreateLog = UnitWork.Find<MeetingOpreateLog>(q => q.MeetingId == meeting.Id);
            foreach (var item in meetingOpreateLog)
            {
                var messi = new MeetingOpreateLogDto();
                item.CopyTo(messi);
                data.MeetingOpreateLog.Add(messi);
                var list = JsonHelper.Instance.Deserialize<Repository.Domain.Serve.Meeting>(item.Json);
                if (meeting.Address != list.Address)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "Address";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.AddressType != list.AddressType)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "AddressType";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.Name != list.Name)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "Name";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.Title != list.Title)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "Title";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.Introduce != list.Introduce)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "Introduce";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.StartTime != list.StartTime)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "StartTime";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.EndTime != list.EndTime)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "EndTime";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.ApplyUser != list.ApplyUser)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "ApplyUser";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.ApplyDempName != list.ApplyDempName)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "ApplyDempName";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.Contact != list.Contact)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "Contact";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.FollowPerson != list.FollowPerson)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "FollowPerson";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.ApplyReason != list.ApplyReason)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "ApplyReason";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.SponsorUnit != list.SponsorUnit)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "SponsorUnit";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.ConferenceScale != list.ConferenceScale)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "ConferenceScale";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.UserNumberLimit != list.UserNumberLimit)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "UserNumberLimit";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.Funds != list.Funds)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "Funds";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.Position != list.Position)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "Position";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.MeasureOfArea != list.MeasureOfArea)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "MeasureOfArea";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.ProductType != list.ProductType)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "ProductType";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.IsDinner != list.IsDinner)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "IsDinner";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.BulidType != list.BulidType)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "BulidType";
                    data.FieldsList.Add(Fields);
                }
                if (meeting.Remark != list.Remark)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "Remark";
                    data.FieldsList.Add(Fields);
                }

            }

            return data;
        }
        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="id"></param>
        /// <param name="opinion"></param>
        /// <returns></returns>
        public bool Approve(int id, string opinion)
        {
            var loginContext = _auth.GetCurrentUser();
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = _serviceBaseApp.GetSalesDepID(userId);
            var depName = _serviceBaseApp.GetSalesDepname(userId.ToString(), "1");
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var meetingdraft = UnitWork.FindSingle<MeetingDraft>(q => q.Id == id);

            var meetingdraftlog = new MeetingDraftlog();
            meetingdraftlog.DraftId = id;
            if (meetingdraft.Step == 1)
            {
                if (id == 0)
                {
                    meetingdraft.Step = 3;
                    meetingdraft.Opinion = opinion;
                    UnitWork.Update(meetingdraft);
                    meetingdraftlog.CreateUser = loginUser.Name;
                    meetingdraftlog.CreateTime = DateTime.Now;
                    meetingdraftlog.Type = 1;
                    meetingdraftlog.Log = loginUser.Name + "——操作：审批驳回";
                }
                else if (id == 1)
                {
                    meetingdraft.Step = 2;
                    meetingdraft.Opinion = opinion;
                    UnitWork.Update(meetingdraft);
                    meetingdraftlog.CreateUser = loginUser.Name;
                    meetingdraftlog.CreateTime = DateTime.Now;
                    meetingdraftlog.Type = 1;
                    meetingdraftlog.Log = loginUser.Name + "——操作：审批通过";
                }
            }
            else if (meetingdraft.Step == 2)
            {
                if (id == 0)
                {
                    meetingdraft.Step = 3;
                    meetingdraft.Opinion = opinion;
                    UnitWork.Update(meetingdraft);
                    meetingdraftlog.CreateUser = loginUser.Name;
                    meetingdraftlog.CreateTime = DateTime.Now;
                    meetingdraftlog.Type = 1;
                    meetingdraftlog.Log = loginUser.Name + "——操作：审批驳回";
                }
                else if (id == 1)
                {
                    meetingdraft.Step = 4;
                    meetingdraft.Opinion = opinion;
                    UnitWork.Update(meetingdraft);
                    meetingdraftlog.CreateUser = loginUser.Name;
                    meetingdraftlog.CreateTime = DateTime.Now;
                    meetingdraftlog.Type = 1;
                    meetingdraftlog.Log = loginUser.Name + "——操作：审批通过";
                }
            }

            UnitWork.Save();
            return true;
        }

    }
}
