using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting;
using OpenAuth.App.Meeting.ModelDto;
using OpenAuth.App.Meeting.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Meeting
{
    /// <summary>
	/// 会议
	/// </summary>
	[Route("api/Meeting/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Meeting")]
    public class MeetingController : Controller
    {
        private readonly ServiceMeetingApp _serviceMeetingApp;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public MeetingController(IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth, ServiceMeetingApp serviceMeetingApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _serviceMeetingApp = serviceMeetingApp;
        }
        /// <summary>
        /// 添加展会 Ations=0：草稿  Ations=1:提交
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddMeetingData")]
        public async Task<Response<string>> AddMeetingData(AddMeetingDataReq AddModel)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceMeetingApp.AddMeetingData(AddModel);

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 展会列表
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Load")]
        public async Task<TableData> Load(LoadReq QueryModel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.Load(QueryModel, out rowcount);
                result.Count = rowcount;

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }

        /// <summary>
        /// 展会详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ExhibitionDetailById")]
        public async Task<Response<ExhibitionDetailDto>> ExhibitionDetailById(int Id)
        {

            var result = new Response<ExhibitionDetailDto>();
            try
            {
                result.Result = _serviceMeetingApp.ExhibitionDetailById(Id);


            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }

        /// <summary>
        /// 我创建的列表
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MyCreatedLoad")]
        public async Task<TableData> MyCreatedLoad(MyCreatedLoadReq QueryModel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.MyCreatedLoad(QueryModel, out rowcount);
                result.Count = rowcount;

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 报名人数查看列表
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MeetingUserList")]
        public async Task<TableData> MeetingUserList(MeetingUserListReq QueryModel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.MeetingUserList(QueryModel, out rowcount);
                result.Count = rowcount;

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 报名
        /// </summary>
        /// <param name="MeetingId"></param>
        /// <param name="Opinion"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MeetingUserApply")]
        public Response<bool> MeetingUserApply(int MeetingId, string Opinion)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingUserApply(MeetingId, Opinion);

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 调度-调度人
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Dispatcher")]
        public async Task<TableData> Dispatcher(DispatcherReq QueryModel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.Dispatcher(QueryModel, out rowcount);
                result.Count = rowcount;
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 调度-调度操作
        /// </summary>
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Scheduling")]
        public Response<bool> Scheduling(List<SchedulingReq> UpdateModel)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.Scheduling(UpdateModel);

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 我的创建详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MyCreatedDetails")]
        public async Task<Response<ExhibitionDetailDto>> MyCreatedDetails(int Id)
        {

            var result = new Response<ExhibitionDetailDto>();
            try
            {
                result.Result = _serviceMeetingApp.MyCreatedDetailsById(Id);


            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }

        /// <summary>
        /// 提交给我的
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubmittedLod")]
        public async Task<TableData> SubmittedLod(SubmittedReq QueryModel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.SubmittedLod(QueryModel, out rowcount);
                result.Count = rowcount;

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 提交给我的详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("SubmittedDetails")]
        public async Task<Response<SubmittedDetailsDto>> SubmittedDetails(int Id)
        {
            var result = new Response<SubmittedDetailsDto>();
            try
            {
                result.Result = _serviceMeetingApp.SubmittedDetails(Id);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 审批 Audit 0：驳回，1：通过
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Audit"></param>
        /// <param name="Opinion"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Approve")]
        public async Task<Response<bool>> Approve(int Id, int Audit, string Opinion)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.Approve(Id, Audit, Opinion);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 展会重新提交 Ations：2
        /// </summary>
        /// <param name="UpdateModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("Resubmit")]
        public async Task<Response<bool>> Resubmit(UpdateMeetingDataReq UpdateModel)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.Resubmit(UpdateModel);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 取消会议申请 Opinion：理由、原因
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Audit"></param>
        /// <param name="Opinion"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MeetingCanel")]
        public async Task<Response<bool>> MeetingCanel(int Id, string Opinion)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingCanel(Id, Opinion);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 历史报名信息
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MeetingUserHistory")]
        public async Task<Response<List<MeetingUserHistoryDto>>> MeetingUserHistory(MeetingUserHistoryReq QueryModel)
        {
            var result = new Response<List<MeetingUserHistoryDto>>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingUserHistory(QueryModel);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 报名取消申请
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Opinion"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MeetingUserCanel")]
        public async Task<Response<bool>> MeetingUserCanel(int Id, string Opinion)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingUserCanel(Id, Opinion);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 报名重新提交 
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Opinion"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MeetingUserResubmit")]
        public async Task<Response<bool>> MeetingUserResubmit(int Id, string Opinion)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingUserResubmit(Id, Opinion);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 删除草稿/我的创建
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MeetingDraftDelete")]
        public async Task<Response<bool>> MeetingDraftDelete(int Id)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingDraftDelete(Id);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 部门下拉列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("DepList")]
        public async Task<Response<List<TextVaule>>> DepList()
        {
            var result = new Response<List<TextVaule>>();
            try
            {
                result.Result = _serviceMeetingApp.DepList();
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 用户列表（根据部门id查）
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("UserList")]
        public async Task<Response<List<TextVaule>>> UserList(int Id)
        {
            var result = new Response<List<TextVaule>>();
            try
            {
                result.Result = _serviceMeetingApp.UserList(Id);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 当前登录用户ID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AuthStrategyContextUserID")]
        public async Task<Response<int>> AuthStrategyContextUserID()
        {
            var result = new Response<int>();
            try
            {
                result.Result = _serviceMeetingApp.AuthStrategyContextUserID();
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 当前登录用户部门ID
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("AuthStrategyContextDepID")]
        public async Task<Response<int>> AuthStrategyContextDepID()
        {
            var result = new Response<int>();
            try
            {
                result.Result = _serviceMeetingApp.AuthStrategyContextDepID();
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 调度会议下拉
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MeetingList")]
        public async Task<Response<List<TextVaule>>> MeetingList(int Id)
        {
            var result = new Response<List<TextVaule>>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingList(Id);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;

        }

        /// <summary>
        /// 展会重新提交 Ations：
        /// 3：更新草稿
        /// 4：草稿更新后提交
        /// </summary>
        /// <param name="UpdateModel"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("MeetingDraftResubmit")]
        public async Task<Response<bool>> MeetingDraftResubmit(UpdateMeetingDataReq UpdateModel)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingDraftResubmit(UpdateModel);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 审批流程步骤
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("ProcessSteps")]
        public async Task<Response<List<ProcessStepsDto>>> ProcessSteps(int? DraftId, int? MeetingId)
        {
            var result = new Response<List<ProcessStepsDto>>();
            try
            {
                result.Result = _serviceMeetingApp.ProcessSteps(DraftId, MeetingId);
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
