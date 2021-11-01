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
using System.Linq;
using OpenAuth.Repository;
using System.Data;

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
        public string AddMeetingData(AddMeetingDataReq AddModel)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            OpenAuth.Repository.Domain.Serve.Meeting meeting = new OpenAuth.Repository.Domain.Serve.Meeting
            {
                Name = AddModel.Name,
                Title = AddModel.Title,
                Introduce = AddModel.Introduce,
                StartTime = AddModel.StartTime,
                EndTime = AddModel.EndTime,
                Address = AddModel.Address,
                ApplyUserId = AddModel.ApplyUserId,
                ApplyUser = AddModel.ApplyUser,
                DempId = AddModel.DempId,
                ApplyDempName = AddModel.ApplyDempName,
                Contact = AddModel.Contact,
                FollowPerson = AddModel.FollowPerson,
                SponsorUnit = AddModel.SponsorUnit,
                GuideUnit = AddModel.GuideUnit,
                ApplyReason = AddModel.ApplyReason,
                ConferenceScale = AddModel.ConferenceScale,
                UserNumberLimit = AddModel.UserNumberLimit,
                Funds = AddModel.Funds,
                Position = AddModel.Position,
                MeasureOfArea = AddModel.MeasureOfArea,
                ProductType = AddModel.ProductType,
                IsDinner = AddModel.IsDinner,
                BulidType = AddModel.BulidType,
                IsSign = AddModel.IsSign,
                Remark = AddModel.Remark,
                CreateUser = loginUser.Name,
                CreateTime = DateTime.Now
            };
            if (AddModel.Ations == MeetingAtion.Draft)
            {
                meeting.Status = 0;
            }
            else if (AddModel.Ations == MeetingAtion.Submit)
            {
                meeting.Status = 1;
            }
            var data = UnitWork.Add<OpenAuth.Repository.Domain.Serve.Meeting, int>(meeting);
            UnitWork.Save();
            MeetingDraft draft = new MeetingDraft();
            draft.Base_entry = data.Id;
            draft.CreateUser = loginUser.Name;
            draft.Type = 0;
            draft.CreateTime = DateTime.Now;
            draft.Name = "展会申请";
            draft.Remark = data.Remark;
            if (AddModel.Ations == MeetingAtion.Draft)
            {
                draft.Step = 0;
            }
            else if (AddModel.Ations == MeetingAtion.Submit)
            {
                draft.Step = 1;

            }
            UnitWork.Add<MeetingDraft, int>(draft);
            UnitWork.Save();

            if (draft.Step == 1)
            {
                var meetingdraftlog = new MeetingDraftlog();
                meetingdraftlog.DraftId = draft.Id;
                meetingdraftlog.CreateUser = loginUser.Name;
                meetingdraftlog.CreateTime = DateTime.Now;
                meetingdraftlog.Type = 1;
                meetingdraftlog.Log = "发起申请";
                UnitWork.Add<MeetingDraftlog, int>(meetingdraftlog);
            }
            if (AddModel.FileList != null && AddModel.FileList.Count > 0)
            {
                foreach (var item in AddModel.FileList)
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
            }
            UnitWork.Save();
            return "1";
        }
        /// <summary>
        /// 报名
        /// </summary>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public bool MeetingUserApply(int MeetingId, string Opinion)
        {
            var loginContext = _auth.GetCurrentUser();
            var userId = _serviceBaseApp.GetUserNaspId();
            var depId = _serviceBaseApp.GetSalesDepID(userId);
            var depName = _serviceBaseApp.GetSalesDepname(userId.ToString(), "1");
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            if (UnitWork.IsExist<MeetingUser>(zw => zw.MeetingId == MeetingId && zw.UserId == userId && zw.DempId == depId && !zw.IsDelete))
            {
                return false;
            }
            var loginUser = loginContext.User;
            MeetingUser user = new MeetingUser();
            user.MeetingId = MeetingId;
            user.Name = loginUser.Name;
            user.Status = 0;
            user.UserId = userId;
            user.CreateTime = DateTime.Now;
            user.CreateUser = loginUser.Name;
            user.DempId = depId;
            user.DempName = depName;
            user.Remark = Opinion;
            UnitWork.Add<MeetingUser, int>(user);
            UnitWork.Save();
            MeetingDraft draft = new MeetingDraft();
            draft.Base_entry = user.Id;
            draft.CreateUser = loginUser.Name;
            draft.Type = 1;
            draft.CreateTime = DateTime.Now;
            draft.Name = "展会报名";
            draft.Step = 1;
            draft.Remark = Opinion;
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
        public bool Scheduling(List<SchedulingReq> UpdateModel)
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

            foreach (var item in UpdateModel)
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
        public List<DispatcherDto> Dispatcher(DispatcherReq QueryModel, out int rowcount)
        {
            Expression<Func<MeetingUser, bool>> exps = t => true;
            exps = exps.And(t => !t.IsDelete && t.MeetingId == QueryModel.MeetingId);
            if (!string.IsNullOrWhiteSpace(QueryModel.Name))
            {
                exps = exps.And(t => t.Name.Contains(QueryModel.Name));

            }

            var meetingUser = UnitWork.Find(QueryModel.page, QueryModel.limit, "", exps);
            rowcount = UnitWork.GetCount(exps);
            return meetingUser.MapToList<DispatcherDto>();
        }


        /// <summary>
        /// 报名人数查看列表
        /// </summary>
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        public List<MeetingUserListDto> MeetingUserList(MeetingUserListReq QueryModel, out int rowcount)
        {
            Expression<Func<MeetingUser, bool>> exps = t => true;
            exps = exps.And(t => !t.IsDelete && t.MeetingId == QueryModel.MeetingId);
            if (!string.IsNullOrWhiteSpace(QueryModel.Name))
            {
                exps = exps.And(t => t.Name.Contains(QueryModel.Name));

            }
            if (!string.IsNullOrEmpty(QueryModel.StartTime))
            {
                DateTime startTime;
                DateTime.TryParse(QueryModel.StartTime, out startTime);
                exps = exps.And(t => t.CreateTime >= startTime);
            }
            if (!string.IsNullOrEmpty(QueryModel.EndTime))
            {
                DateTime endTime;
                DateTime.TryParse(QueryModel.EndTime, out endTime);
                exps = exps.And(t => t.CreateTime <= endTime);
            }
            var meetingUser = UnitWork.Find(QueryModel.page, QueryModel.limit, "", exps);
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
        public List<QueryListDto> Load(LoadReq QueryModel, out int rowcount)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            Expression<Func<OpenAuth.Repository.Domain.Serve.Meeting, bool>> exp = t => true;
            exp = exp.And(t => !t.IsDelete);
            //exp = exp.And(t => t.Status == 3);
            exp = exp.And(t => t.CreateUser == loginUser.Name);

            if (!string.IsNullOrEmpty(QueryModel.Name))
            {
                exp = exp.And(t => t.Name == QueryModel.Name);

            }
            if (!string.IsNullOrEmpty(QueryModel.StartTime))
            {
                DateTime startTime;
                DateTime.TryParse(QueryModel.StartTime, out startTime);
                exp = exp.And(t => t.StartTime >= startTime);
            }
            if (!string.IsNullOrEmpty(QueryModel.EndTime))
            {
                DateTime endTime;
                DateTime.TryParse(QueryModel.EndTime, out endTime);
                exp = exp.And(t => t.EndTime <= endTime);
            }
            exp = exp.And(t => t.IsDinner == true);
            if (QueryModel.DempId != 0)
            {
                exp = exp.And(t => t.DempId == QueryModel.DempId);
            }
            if (!string.IsNullOrEmpty(QueryModel.ApplyUser))
            {
                exp = exp.And(t => t.ApplyUser == QueryModel.ApplyUser);
            }
            if (!string.IsNullOrEmpty(QueryModel.FollowPerson))
            {
                exp = exp.And(t => t.FollowPerson == QueryModel.FollowPerson);
            }
            if (QueryModel.Status != 0)
            {
                exp = exp.And(t => t.Status == QueryModel.Status);
            }
            var objs = UnitWork.Find<OpenAuth.Repository.Domain.Serve.Meeting>(QueryModel.page, QueryModel.limit, "", exp);
            var list = objs.MapToList<OpenAuth.Repository.Domain.Serve.Meeting>();
            rowcount = UnitWork.GetCount(exp);
            var data = new List<QueryListDto>();
            foreach (var obj in list)
            {
                var nes = new QueryListDto();

                nes.Address = obj.Address;
                nes.Name = obj.Name;
                nes.StartTime = obj.StartTime.ToString("yyyy-MM-dd");
                nes.EndTime = obj.EndTime.ToString("yyyy-MM-dd");
                nes.Status = obj.Status;
                nes.IsDinner = obj.IsDinner;
                nes.ApplyDempName = obj.ApplyDempName;
                nes.ApplyUser = obj.ApplyUser;
                nes.FollowPerson = obj.FollowPerson;
                nes.AddressType = obj.AddressType;
                nes.Id = obj.Id;
                var obe = UnitWork.GetCount<MeetingUser>(x => x.MeetingId == obj.Id && x.Status == 1);
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
        public List<MyCreatedLoadListDto> MyCreatedLoad(MyCreatedLoadReq QueryModel, out int rowcount)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            Expression<Func<MeetingDraft, bool>> exp = t => true;
            exp = exp.And(t => t.CreateUser == loginUser.Name && !t.IsDelete);
            if (QueryModel.JobId != 0)
            {
                exp = exp.And(t => t.Id == QueryModel.JobId);

            }
            if (QueryModel.Type != -1)
            {
                exp = exp.And(t => t.Type == QueryModel.Type);

            }
            if (!string.IsNullOrWhiteSpace(QueryModel.JobName))
            {
                exp = exp.And(t => t.Name.Contains(QueryModel.JobName));

            }
            var objs = UnitWork.Find<MeetingDraft>(QueryModel.page, QueryModel.limit, "", exp);
            var list = objs.MapToList<MeetingDraft>();
            rowcount = UnitWork.GetCount(exp);
            var data = new List<MyCreatedLoadListDto>();
            foreach (var obj in list)
            {
                var nes = new MyCreatedLoadListDto();
                Expression<Func<Repository.Domain.Serve.Meeting, bool>> exps = e => true;
                if (!string.IsNullOrWhiteSpace(QueryModel.MeetingName))
                {
                    exps = exps.And(e => e.Name.Contains(QueryModel.MeetingName));
                }
                if (QueryModel.AddressType != -1)
                {
                    exps = exps.And(e => e.AddressType == QueryModel.AddressType);
                }
                if (!string.IsNullOrEmpty(QueryModel.StartTime))
                {
                    DateTime startTime;
                    DateTime.TryParse(QueryModel.StartTime, out startTime);
                    exps = exps.And(e => e.StartTime >= startTime);
                }
                if (!string.IsNullOrEmpty(QueryModel.EndTime))
                {
                    DateTime endTime;
                    DateTime.TryParse(QueryModel.EndTime, out endTime);
                    exps = exps.And(e => e.EndTime <= endTime);
                }
                if (obj.Type == 0)
                {
                    exps = exps.And(e => e.Id == obj.Base_entry);
                    var meeting = UnitWork.FindSingle<Repository.Domain.Serve.Meeting>(exps);
                    nes.Address = meeting.Address;
                    nes.AddressType = meeting.AddressType;
                    nes.Id = obj.Id;
                    nes.Name = obj.Name;
                    nes.Remark = obj.Remark;
                    nes.Step = obj.Step;
                    nes.Type = obj.Type;
                    nes.UpdateTime = obj.UpdateTime;
                    nes.MeetingName = meeting.Name;
                    nes.Base_entry = obj.Base_entry;
                    nes.CreateTime = obj.CreateTime;
                    nes.CreateUser = obj.CreateUser;
                }
                else if (obj.Type == 1)
                {
                    var meetinguser = UnitWork.FindSingle<Repository.Domain.Serve.MeetingUser>(q => q.Id == obj.Base_entry);
                    exps = exps.And(e => e.Id == meetinguser.MeetingId);
                    var meeting = UnitWork.FindSingle<Repository.Domain.Serve.Meeting>(exps);
                    nes.Address = meeting.Address;
                    nes.AddressType = meeting.AddressType;
                    nes.Id = obj.Id;
                    nes.Name = obj.Name;
                    nes.Remark = obj.Remark;
                    nes.Step = obj.Step;
                    nes.Type = obj.Type;
                    nes.UpdateTime = obj.UpdateTime;
                    nes.MeetingName = meeting.Name;
                    nes.Base_entry = obj.Base_entry;
                    nes.CreateTime = obj.CreateTime;
                    nes.CreateUser = obj.CreateUser;
                }

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
        public List<SubmittedDto> SubmittedLod(SubmittedReq QueryModel, out int rowcount)
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
            if (QueryModel.JobId != 0)
            {
                exp = exp.And(t => t.Id == QueryModel.JobId);
            }
            if (QueryModel.Type != -1)
            {
                exp = exp.And(t => t.Type == QueryModel.Type);
            }

            if (!string.IsNullOrWhiteSpace(QueryModel.JobName))
            {
                exp = exp.And(t => t.Name.Contains(QueryModel.JobName));
            }
            var objs = UnitWork.Find<MeetingDraft>(QueryModel.page, QueryModel.limit, "", exp);
            var list = objs.MapToList<MeetingDraft>();
            rowcount = UnitWork.GetCount(exp);
            var data = new List<SubmittedDto>();
            foreach (var obj in list)
            {
                var nes = new SubmittedDto();
                Expression<Func<Repository.Domain.Serve.Meeting, bool>> exps = e => true;
                if (!string.IsNullOrWhiteSpace(QueryModel.MeetingName))
                {
                    exps = exps.And(e => e.Name.Contains(QueryModel.MeetingName));
                }
                if (QueryModel.AddressType != -1)
                {
                    exps = exps.And(e => e.AddressType == QueryModel.AddressType);
                }
                if (!string.IsNullOrEmpty(QueryModel.StartTime))
                {
                    DateTime startTime;
                    DateTime.TryParse(QueryModel.StartTime, out startTime);
                    exps = exps.And(e => e.StartTime >= startTime);
                }
                if (!string.IsNullOrEmpty(QueryModel.EndTime))
                {
                    DateTime endTime;
                    DateTime.TryParse(QueryModel.EndTime, out endTime);
                    exps = exps.And(e => e.EndTime <= endTime);
                }

                if (loginUser.Name == "骆灵芝")
                {
                    if (obj.Type == 0)
                    {
                        exps = exps.And(e => e.Id == obj.Base_entry);
                        var meeting = UnitWork.FindSingle<Repository.Domain.Serve.Meeting>(exps);

                        if (meeting != null)
                        {
                            if (obj.Step==2)
                            {
                                nes.Name = obj.Name;
                                nes.Remark = obj.Remark;
                                nes.Step = obj.Step;
                                nes.Type = obj.Type;
                                nes.UpdateTime = obj.UpdateTime;
                                nes.StartTime = meeting.StartTime;
                                nes.EndTime = meeting.EndTime;
                                nes.Id = obj.Id;
                                nes.MeetingName = meeting.Name;
                                nes.Address = meeting.Address;
                                nes.AddressType = meeting.AddressType;
                                nes.Base_entry = obj.Base_entry;
                                nes.CreateTime = obj.CreateTime;
                                nes.CreateUser = obj.CreateUser;
                                data.Add(nes);
                            }
                         
                        }
                    }
                    if (obj.Type == 1)
                    {
                        var meetuser = UnitWork.FindSingle<MeetingUser>(q => q.Id == obj.Base_entry);
                        exps = exps.And(e => e.Id == meetuser.MeetingId);
                        var meeting = UnitWork.FindSingle<Repository.Domain.Serve.Meeting>(exps);
                        if (meeting != null)
                        {
                            if (obj.Step==2)
                            {
                                nes.Name = obj.Name;
                                nes.Remark = obj.Remark;
                                nes.Step = obj.Step;
                                nes.Type = obj.Type;
                                nes.UpdateTime = obj.UpdateTime;
                                nes.StartTime = meeting.StartTime;
                                nes.EndTime = meeting.EndTime;
                                nes.Id = obj.Id;
                                nes.MeetingName = meeting.Name;
                                nes.Address = meeting.Address;
                                nes.AddressType = meeting.AddressType;
                                nes.Base_entry = obj.Base_entry;
                                nes.CreateTime = obj.CreateTime;
                                nes.CreateUser = obj.CreateUser;
                                data.Add(nes);
                            }
                       
                        }

                    }
                }
                else
                {
                    if (obj.Type == 0)
                    {
                        exps = exps.And(e => e.Id == obj.Base_entry);
                        exps = exps.And(e => e.DempId == depId);
                        var meeting = UnitWork.FindSingle<Repository.Domain.Serve.Meeting>(exps);
                        if (meeting != null)
                        {
                            if (obj.Step==1)
                            {
                                nes.Name = obj.Name;
                                nes.Remark = obj.Remark;
                                nes.Step = obj.Step;
                                nes.Type = obj.Type;
                                nes.UpdateTime = obj.UpdateTime;
                                nes.StartTime = meeting.StartTime;
                                nes.EndTime = meeting.EndTime;
                                nes.Id = obj.Id;
                                nes.MeetingName = meeting.Name;
                                nes.Address = meeting.Address;
                                nes.AddressType = meeting.AddressType;
                                nes.Base_entry = obj.Base_entry;
                                nes.CreateTime = obj.CreateTime;
                                nes.CreateUser = obj.CreateUser;
                                data.Add(nes);
                            }
                           
                        }

                    }
                    if (obj.Type == 1)
                    {
                        var meetuser = UnitWork.FindSingle<MeetingUser>(q => q.Id == obj.Base_entry);
                        exps = exps.And(e => e.Id == meetuser.MeetingId);
                        exps = exps.And(e => e.DempId == depId);
                        var meeting = UnitWork.FindSingle<Repository.Domain.Serve.Meeting>(exps);
                        if (meeting != null)
                        {
                            if (obj.Step==1)
                            {
                                nes.Name = obj.Name;
                                nes.Remark = obj.Remark;
                                nes.Step = obj.Step;
                                nes.Type = obj.Type;
                                nes.UpdateTime = obj.UpdateTime;
                                nes.StartTime = meeting.StartTime;
                                nes.EndTime = meeting.EndTime;
                                nes.Id = obj.Id;
                                nes.MeetingName = meeting.Name;
                                nes.Address = meeting.Address;
                                nes.AddressType = meeting.AddressType;
                                nes.Base_entry = obj.Base_entry;
                                nes.CreateTime = obj.CreateTime;
                                nes.CreateUser = obj.CreateUser;
                                data.Add(nes);
                            }
                           
                        }

                    }
                }

            }

            return data;
        }
        /// <summary>
        /// 提交给我的详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public SubmittedDetailsDto SubmittedDetails(int Id)
        {
            var data = new SubmittedDetailsDto();
            var meetingdraft = UnitWork.FindSingle<MeetingDraft>(zw => zw.Id == Id);
            var meeting = new OpenAuth.Repository.Domain.Serve.Meeting();
            if (meetingdraft.Type == 1)
            {
                var meetinguser = UnitWork.FindSingle<MeetingUser>(zw => zw.Id == meetingdraft.Base_entry);
                meeting = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(zw => zw.Id == meetinguser.MeetingId);
                data.meetingUser.CreateTime = meetinguser.CreateTime;
                data.meetingUser.Name = meetinguser.Name;
                data.meetingUser.Remark = meetinguser.Remark;
            }
            if (meetingdraft.Type == 0)
            {
                meeting = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(zw => zw.Id == meetingdraft.Base_entry);
            }
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
            data.LatestApplicationList = new List<LatestApplicationDto>();
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
            data.FileList = new List<FileDto>();
            foreach (var item in file)
            {
                var FileDto = new FileDto();
                item.CopyTo(FileDto);
                data.FileList.Add(FileDto);
            }
            var meetingOpreateLog = UnitWork.Find<MeetingOpreateLog>(q => q.MeetingId == meeting.Id);
            data.MeetingOpreateLog = new List<MeetingOpreateLogDto>();
            data.FieldsList = new List<FieldsDto>();
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
                if (meeting.CancelReason != null && meeting.CancelReason != list.CancelReason)
                {
                    var Fields = new FieldsDto();
                    Fields.Field = "CancelReason";
                    data.FieldsList.Add(Fields);
                }

            }

            return data;
        }
        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Audit"></param>
        /// <param name="Opinion"></param>
        /// <returns></returns>
        public bool Approve(int Id, int Audit, string Opinion)
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
            var meetingdraft = UnitWork.FindSingle<MeetingDraft>(q => q.Id == Id);

            var meetingdraftlog = new MeetingDraftlog();
            meetingdraftlog.DraftId = Id;
            if (meetingdraft.Step == 1)
            {
                if (Audit == 0)
                {
                    meetingdraft.Step = 3;
                    meetingdraft.Opinion = Opinion;
                    UnitWork.Update(meetingdraft);
                    meetingdraftlog.CreateUser = loginUser.Name;
                    meetingdraftlog.CreateTime = DateTime.Now;
                    meetingdraftlog.Type = 1;
                    meetingdraftlog.Log = "主管审核：审批驳回";
                    if (meetingdraft.Type == 0)
                    {
                        var meet = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(q => q.Id == meetingdraft.Base_entry);
                        meet.Status = 4;
                        UnitWork.Update(meet);
                    }
                    else if (meetingdraft.Type == 1)
                    {
                        var meetuser = UnitWork.FindSingle<MeetingUser>(q => q.Id == meetingdraft.Base_entry);
                        meetuser.Status = -1;
                        UnitWork.Update(meetuser);

                    }

                }
                else if (Audit == 1)
                {
                    meetingdraft.Step = 2;
                    meetingdraft.Opinion = Opinion;
                    UnitWork.Update(meetingdraft);
                    meetingdraftlog.CreateUser = loginUser.Name;
                    meetingdraftlog.CreateTime = DateTime.Now;
                    meetingdraftlog.Type = 1;
                    meetingdraftlog.Log = "主管审核：审批通过";
                    if (meetingdraft.Type == 0)
                    {
                        var meet = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(q => q.Id == meetingdraft.Base_entry);
                        meet.Status = 2;
                        UnitWork.Update(meet);
                    }
                    if (meetingdraft.Type == 1)
                    {
                        var meetuser = UnitWork.FindSingle<MeetingUser>(q => q.Id == meetingdraft.Base_entry);
                        meetuser.Status = 1;
                        UnitWork.Update(meetuser);
                    }

                }
            }
            else if (meetingdraft.Step == 2)
            {
                if (Audit == 0)
                {
                    meetingdraft.Step = 3;
                    meetingdraft.Opinion = Opinion;
                    UnitWork.Update(meetingdraft);
                    meetingdraftlog.CreateUser = loginUser.Name;
                    meetingdraftlog.CreateTime = DateTime.Now;
                    meetingdraftlog.Type = 1;
                    meetingdraftlog.Log = loginUser.Name + "OR审核：审批驳回";
                    if (meetingdraft.Type == 0)
                    {
                        var meet = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(q => q.Id == meetingdraft.Base_entry);
                        meet.Status = 4;
                        UnitWork.Update(meet);
                    }
                    else if (meetingdraft.Type == 1)
                    {
                        var meetuser = UnitWork.FindSingle<MeetingUser>(q => q.Id == meetingdraft.Base_entry);
                        meetuser.Status = -1;
                        UnitWork.Update(meetuser);

                    }
                }
                else if (Audit == 1)
                {
                    meetingdraft.Step = 4;
                    meetingdraft.Opinion = Opinion;
                    UnitWork.Update(meetingdraft);
                    meetingdraftlog.CreateUser = loginUser.Name;
                    meetingdraftlog.CreateTime = DateTime.Now;
                    meetingdraftlog.Type = 1;
                    meetingdraftlog.Log = loginUser.Name + "OR审核：审批通过";
                    if (meetingdraft.Type == 0)
                    {
                        var meet = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(q => q.Id == meetingdraft.Base_entry);
                        meet.Status = 3;
                        UnitWork.Update(meet);
                    }
                    else if (meetingdraft.Type == 1)
                    {
                        var meetuser = UnitWork.FindSingle<MeetingUser>(q => q.Id == meetingdraft.Base_entry);
                        meetuser.Status = 1;
                        UnitWork.Update(meetuser);

                    }
                }
            }
            UnitWork.Add<MeetingDraftlog, int>(meetingdraftlog);
            UnitWork.Save();
            return true;
        }
        /// <summary>
        /// 会议重新提交
        /// </summary>
        /// <param name="UpdateModel"></param>
        /// <returns></returns>

        public bool Resubmit(UpdateMeetingDataReq UpdateModel)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;

            if (UpdateModel.Ations == MeetingAtion.Resubmit)
            {
                var data = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(q => q.Id == UpdateModel.Id);
                if (data.Status == 3)
                {
                    if (UpdateModel.Name != data.Name || UpdateModel.StartTime != data.StartTime || UpdateModel.EndTime != data.EndTime || UpdateModel.Address != data.Address || UpdateModel.Funds != data.Funds || UpdateModel.ApplyReason != data.ApplyReason)
                    {
                        data.Status = 1;
                        var meetdraft = UnitWork.FindSingle<MeetingDraft>(q => q.Base_entry == UpdateModel.Id);
                        meetdraft.Step = 1;
                        meetdraft.UpdateTime = DateTime.Now;
                        meetdraft.UpdateUser = loginUser.Name;
                        UnitWork.Update(meetdraft);
                        var meetdraftlog = new MeetingDraftlog();
                        meetdraftlog.CreateTime = DateTime.Now;
                        meetdraftlog.CreateUser = loginUser.Name;
                        meetdraftlog.DraftId = meetdraft.Id;
                        meetdraftlog.Type = 1;
                        meetdraftlog.Log = "更变已通过状态重新提交审核";
                        UnitWork.Add<MeetingDraftlog, int>(meetdraftlog);

                    }
                }
                if (data.Status == 4)
                {
                    data.Status = 1;
                    var meetdraft = UnitWork.FindSingle<MeetingDraft>(q => q.Base_entry == UpdateModel.Id);
                    meetdraft.Step = 1;
                    meetdraft.UpdateTime = DateTime.Now;
                    meetdraft.UpdateUser = loginUser.Name;
                    UnitWork.Update(meetdraft);
                    var meetdraftlog = new MeetingDraftlog();
                    meetdraftlog.CreateTime = DateTime.Now;
                    meetdraftlog.CreateUser = loginUser.Name;
                    meetdraftlog.DraftId = meetdraft.Id;
                    meetdraftlog.Type = 1;
                    meetdraftlog.Log = "更变已驳回状态重新提交审核";
                    UnitWork.Add<MeetingDraftlog, int>(meetdraftlog);

                }
                data.Name = UpdateModel.Name;
                data.Title = UpdateModel.Title;
                data.Introduce = UpdateModel.Introduce;
                data.StartTime = UpdateModel.StartTime;
                data.EndTime = UpdateModel.EndTime;
                data.Address = UpdateModel.Address;
                data.ApplyUserId = UpdateModel.ApplyUserId;
                data.ApplyUser = UpdateModel.ApplyUser;
                data.DempId = UpdateModel.DempId;
                data.ApplyDempName = UpdateModel.ApplyDempName;
                data.Contact = UpdateModel.Contact;
                data.FollowPerson = UpdateModel.FollowPerson;
                data.SponsorUnit = UpdateModel.SponsorUnit;
                data.GuideUnit = UpdateModel.GuideUnit;
                data.ApplyReason = UpdateModel.ApplyReason;
                data.ConferenceScale = UpdateModel.ConferenceScale;
                data.UserNumberLimit = UpdateModel.UserNumberLimit;
                data.Funds = UpdateModel.Funds;
                data.Position = UpdateModel.Position;
                data.MeasureOfArea = UpdateModel.MeasureOfArea;
                data.ProductType = UpdateModel.ProductType;
                data.IsDinner = UpdateModel.IsDinner;
                data.BulidType = UpdateModel.BulidType;
                data.IsSign = UpdateModel.IsSign;
                data.Remark = UpdateModel.Remark;
                data.UpdateUser = loginUser.Name;
                data.UpdateTime = DateTime.Now;
                UnitWork.Update(data);
                MeetingOpreateLog opreateLog = new MeetingOpreateLog();
                opreateLog.Log = "修改会议内容";
                opreateLog.Json = JsonHelper.Instance.Serialize(UpdateModel);
                opreateLog.CreateUser = loginUser.Name;
                opreateLog.CreateTime = DateTime.Now;
                opreateLog.MeetingId = UpdateModel.Id;
                opreateLog.Type = 1;
                UnitWork.Add<MeetingOpreateLog, int>(opreateLog);

                UnitWork.Save();
            }

            return true;
        }
        /// <summary>
        /// 取消会议申请
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Opinion"></param>
        /// <returns></returns>
        public bool MeetingCanel(int Id, string Opinion)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var meeting = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(q => q.Id == Id);
            meeting.CancelReason = Opinion;
            //meeting.Status = 6;//状态更变为取消
            meeting.UpdateTime = DateTime.Now;
            meeting.UpdateUser = loginUser.Name;
            meeting.CancelTime = DateTime.Now;
            UnitWork.Update(meeting);
            MeetingDraft draft = new MeetingDraft();
            draft.Base_entry = meeting.Id;
            draft.CreateUser = loginUser.Name;
            draft.Type = 0;
            draft.CreateTime = DateTime.Now;
            draft.Name = "展会取消申请";
            draft.Step = 1;
            draft.Remark = meeting.CancelReason;
            UnitWork.Add<MeetingDraft, int>(draft);
            UnitWork.Save();
            return true;
        }

        /// <summary>
        /// 历史报名信息
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        public List<MeetingUserHistoryDto> MeetingUserHistory(MeetingUserHistoryReq QueryModel)
        {
            var data = new List<MeetingUserHistoryDto>();
            Expression<Func<OpenAuth.Repository.Domain.Serve.MeetingUser, bool>> exp = t => true;
            exp = exp.And(t => !t.IsDelete);
            exp = exp.And(t => t.UserId == QueryModel.UserId);
            if (!string.IsNullOrEmpty(QueryModel.StartTime))
            {
                DateTime startTime;
                DateTime.TryParse(QueryModel.StartTime, out startTime);
                exp = exp.And(t => t.CancelTime >= startTime);
            }
            if (!string.IsNullOrEmpty(QueryModel.EndTime))
            {
                DateTime endTime;
                DateTime.TryParse(QueryModel.EndTime, out endTime);
                exp = exp.And(t => t.CancelTime <= endTime);
            }
            var meetuser = UnitWork.Find(exp);
            var list = meetuser.MapToList<MeetingUser>();
            foreach (var item in list)
            {
                var scon = new MeetingUserHistoryDto();
                Expression<Func<OpenAuth.Repository.Domain.Serve.Meeting, bool>> exps = t => true;
                exps = exps.And(t => !t.IsDelete);
                exps = exps.And(t => t.Id == item.MeetingId);
                if (!string.IsNullOrWhiteSpace(QueryModel.MeetingName))
                {
                    exps = exps.And(t => t.Name.Contains(QueryModel.MeetingName));

                }
                if (!string.IsNullOrWhiteSpace(QueryModel.Address))
                {
                    exps = exps.And(t => t.Address.Contains(QueryModel.Address));

                }
                if (QueryModel.AddressType != -1)
                {
                    exps = exps.And(t => t.AddressType == QueryModel.AddressType);

                }
                var meeting = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(exps);

                scon.MeetingName = meeting.Name;
                scon.Name = item.Name;
                scon.StartTime = meeting.StartTime;
                scon.EndTime = meeting.EndTime;
                scon.Address = meeting.Address;
                scon.AddressType = meeting.AddressType;
                scon.CreateTime = item.CreateTime;
                data.Add(scon);

            }
            return data;
        }
        public bool MeetingUserCanel(int Id, string Opinion)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var meetingUser = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.MeetingUser>(q => q.Id == Id);
            meetingUser.CancelReason = Opinion;
            //meeting.Status = 6;//状态更变为取消
            meetingUser.CancelTime = DateTime.Now;
            UnitWork.Update(meetingUser);
            MeetingDraft draft = new MeetingDraft();
            draft.Base_entry = meetingUser.Id;
            draft.CreateUser = loginUser.Name;
            draft.Type = 1;
            draft.CreateTime = DateTime.Now;
            draft.Name = "展会报名取消申请";
            draft.Step = 1;
            draft.Remark = meetingUser.CancelReason;
            UnitWork.Add<MeetingDraft, int>(draft);
            UnitWork.Save();
            return true;
        }
        /// <summary>
        /// 报名重新提交
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Opinion"></param>
        /// <returns></returns>
        public bool MeetingUserResubmit(int Id, string Opinion)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            var meetingdraft = UnitWork.FindSingle<MeetingDraft>(q => q.Id == Id);
            meetingdraft.Remark = Opinion;
            meetingdraft.Step = 1;
            UnitWork.Update(meetingdraft);
            var meetuser = UnitWork.FindSingle<MeetingUser>(q => q.Id == meetingdraft.Base_entry);
            meetuser.Remark = Opinion;
            UnitWork.Update(meetuser);
            MeetingOpreateLog opreateLog = new MeetingOpreateLog();
            opreateLog.Log = "修改报名备注";
            opreateLog.Json = JsonHelper.Instance.Serialize(Opinion);
            opreateLog.CreateUser = loginUser.Name;
            opreateLog.CreateTime = DateTime.Now;
            opreateLog.MeetingId = meetuser.MeetingId;
            opreateLog.Type = 2;
            UnitWork.Add<MeetingOpreateLog, int>(opreateLog);
            UnitWork.Save();
            return true;
        }
        /// <summary>
        /// 我创建的、草稿删除
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public bool MeetingDraftDelete(int Id)
        {
            var meetdraft = UnitWork.FindSingle<MeetingDraft>(q => q.Id == Id);
            meetdraft.IsDelete = true;
            UnitWork.Update(meetdraft);
            UnitWork.Save(); return true;
        }
        /// <summary>
        /// 部门下拉列表
        /// </summary>
        /// <returns></returns>
        public List<TextVaule> DepList()
        {
            var list = new List<TextVaule>();
            var data = UnitWork.Find<Repository.Domain.base_dep>(q => true);
            var asc = data.MapToList<Repository.Domain.base_dep>();
            list = asc.Select(m => new TextVaule
            {
                Text = m.dep_nm,
                Value = m.dep_id
            }).ToList();
            return list;
        }
        /// <summary>
        /// 根据部门id查用户列表
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public List<TextVaule> UserList(int Id)
        {
            var list = new List<TextVaule>();
            //var userDetails = UnitWork.Find<Repository.Domain.base_user_detail>(null);
            //var data = userDetails.Where(q => q.dep_id == Id);
            //var asc = data.MapToList<Repository.Domain.base_user_detail>();
            var test = UnitWork.ExcuteSql<Repository.Domain.base_user_detail>(ContextType.NsapBaseDbContext, $"SELECT user_id FROM base_user_detail WHERE dep_id={Id}", CommandType.Text, null);
            foreach (var item in test)
            {
                var scon = UnitWork.FindSingle<Repository.Domain.base_user>(q => q.user_id == item.user_id);
                if (scon != null)
                {
                    var nes = new TextVaule();
                    nes.Text = scon.user_nm;
                    nes.Value = (int)scon.user_id;
                    list.Add(nes);
                }

            }

            return list;
        }
        /// <summary>
        /// 当前登录用户ID
        /// </summary>
        /// <returns></returns>
        public int AuthStrategyContextUserID()
        {

            return _serviceBaseApp.GetUserNaspId();
        }
        /// <summary>
        /// 当前登录用户部门ID
        /// </summary>
        /// <returns></returns>
        public int AuthStrategyContextDepID()
        {
            var userId = _serviceBaseApp.GetUserNaspId();
            return _serviceBaseApp.GetSalesDepID(userId);

        }
        /// <summary>
        /// 调度会议下拉
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public List<TextVaule> MeetingList(int Id)
        {
            var data = new List<TextVaule>();
            var meeting = UnitWork.Find<Repository.Domain.Serve.Meeting>(q => q.Id != Id && q.Status == 3 && !q.IsDelete);
            var list = meeting.MapToList<Repository.Domain.Serve.Meeting>();
            foreach (var item in list)
            {
                var scon = new TextVaule();
                scon.Text = item.Name;
                scon.Value = item.Id;
                data.Add(scon);
            }
            return data;
        }

        /// <summary>
        /// 展会草稿修改
        /// </summary>
        /// <param name="UpdateModel"></param>
        /// <returns></returns>
        public bool MeetingDraftResubmit(UpdateMeetingDataReq UpdateModel)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            bool result = false;
            var meetingdraft = UnitWork.FindSingle<MeetingDraft>(q => q.Id == UpdateModel.Id);
            if (UpdateModel.Ations == MeetingAtion.DraftUpdate)
            {
                var data = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(q => q.Id == meetingdraft.Base_entry);
                data.Name = UpdateModel.Name;
                data.Title = UpdateModel.Title;
                data.Introduce = UpdateModel.Introduce;
                data.StartTime = UpdateModel.StartTime;
                data.EndTime = UpdateModel.EndTime;
                data.Address = UpdateModel.Address;
                data.ApplyUserId = UpdateModel.ApplyUserId;
                data.ApplyUser = UpdateModel.ApplyUser;
                data.DempId = UpdateModel.DempId;
                data.ApplyDempName = UpdateModel.ApplyDempName;
                data.Contact = UpdateModel.Contact;
                data.FollowPerson = UpdateModel.FollowPerson;
                data.SponsorUnit = UpdateModel.SponsorUnit;
                data.GuideUnit = UpdateModel.GuideUnit;
                data.ApplyReason = UpdateModel.ApplyReason;
                data.ConferenceScale = UpdateModel.ConferenceScale;
                data.UserNumberLimit = UpdateModel.UserNumberLimit;
                data.Funds = UpdateModel.Funds;
                data.Position = UpdateModel.Position;
                data.MeasureOfArea = UpdateModel.MeasureOfArea;
                data.ProductType = UpdateModel.ProductType;
                data.IsDinner = UpdateModel.IsDinner;
                data.BulidType = UpdateModel.BulidType;
                data.IsSign = UpdateModel.IsSign;
                data.Remark = UpdateModel.Remark;
                data.UpdateUser = loginUser.Name;
                data.UpdateTime = DateTime.Now;
                UnitWork.Update(data);
                MeetingOpreateLog opreateLog = new MeetingOpreateLog();
                opreateLog.Log = "修改会议内容";
                opreateLog.Json = JsonHelper.Instance.Serialize(UpdateModel);
                opreateLog.CreateUser = loginUser.Name;
                opreateLog.CreateTime = DateTime.Now;
                opreateLog.MeetingId = UpdateModel.Id;
                opreateLog.Type = 1;
                UnitWork.Add<MeetingOpreateLog, int>(opreateLog);
                UnitWork.Save();
            }
            if (UpdateModel.Ations == MeetingAtion.DrafSubmit)
            {
                var data = UnitWork.FindSingle<OpenAuth.Repository.Domain.Serve.Meeting>(q => q.Id == meetingdraft.Base_entry);
                data.Name = UpdateModel.Name;
                data.Title = UpdateModel.Title;
                data.Introduce = UpdateModel.Introduce;
                data.StartTime = UpdateModel.StartTime;
                data.EndTime = UpdateModel.EndTime;
                data.Address = UpdateModel.Address;
                data.ApplyUserId = UpdateModel.ApplyUserId;
                data.ApplyUser = UpdateModel.ApplyUser;
                data.DempId = UpdateModel.DempId;
                data.ApplyDempName = UpdateModel.ApplyDempName;
                data.Contact = UpdateModel.Contact;
                data.FollowPerson = UpdateModel.FollowPerson;
                data.SponsorUnit = UpdateModel.SponsorUnit;
                data.GuideUnit = UpdateModel.GuideUnit;
                data.ApplyReason = UpdateModel.ApplyReason;
                data.ConferenceScale = UpdateModel.ConferenceScale;
                data.UserNumberLimit = UpdateModel.UserNumberLimit;
                data.Funds = UpdateModel.Funds;
                data.Position = UpdateModel.Position;
                data.MeasureOfArea = UpdateModel.MeasureOfArea;
                data.ProductType = UpdateModel.ProductType;
                data.IsDinner = UpdateModel.IsDinner;
                data.BulidType = UpdateModel.BulidType;
                data.IsSign = UpdateModel.IsSign;
                data.Remark = UpdateModel.Remark;
                data.UpdateUser = loginUser.Name;
                data.UpdateTime = DateTime.Now;
                data.Status = 1;
                UnitWork.Update(data);
                MeetingOpreateLog opreateLog = new MeetingOpreateLog();
                opreateLog.Log = "修改会议内容";
                opreateLog.Json = JsonHelper.Instance.Serialize(UpdateModel);
                opreateLog.CreateUser = loginUser.Name;
                opreateLog.CreateTime = DateTime.Now;
                opreateLog.MeetingId = UpdateModel.Id;
                opreateLog.Type = 1;
                UnitWork.Add<MeetingOpreateLog, int>(opreateLog);
                meetingdraft.Step = 1;
                UnitWork.Update(meetingdraft);
                var meetingdraftlog = new MeetingDraftlog();
                meetingdraftlog.DraftId = meetingdraft.Id;
                meetingdraftlog.CreateUser = loginUser.Name;
                meetingdraftlog.CreateTime = DateTime.Now;
                meetingdraftlog.Type = 1;
                meetingdraftlog.Log = "发起申请";
                UnitWork.Add<MeetingDraftlog, int>(meetingdraftlog);
                UnitWork.Save();

            }
            return result;
        }

        /// <summary>
        /// 审批流程记录
        /// </summary>
        /// <param name="draftId"></param>
        /// <param name="meetingId"></param>
        /// <returns></returns>
        public List<ProcessStepsDto> ProcessSteps(int? draftId, int? meetingId)
        {
            var data = new List<ProcessStepsDto>();
            if (draftId != 0 && draftId != null)
            {
                var meetdraftlog = UnitWork.Find<MeetingDraftlog>(q => q.DraftId == draftId);
                var list = meetdraftlog.MapToList<MeetingDraftlog>();
                foreach (var item in list)
                {
                    var scon = new ProcessStepsDto();
                    item.CopyTo(scon);
                    data.Add(scon);
                }
            }
            if (meetingId != 0 && meetingId != null)
            {
                var meetdaft = UnitWork.FindSingle<MeetingDraft>(q => q.Base_entry == meetingId);
                var meetdraftlog = UnitWork.Find<MeetingDraftlog>(q => q.DraftId == meetdaft.Id);
                var list = meetdraftlog.MapToList<MeetingDraftlog>();
                foreach (var item in list)
                {
                    var scon = new ProcessStepsDto();
                    item.CopyTo(scon);
                    data.Add(scon);
                }
            }
            return data;
        }

    }
}
