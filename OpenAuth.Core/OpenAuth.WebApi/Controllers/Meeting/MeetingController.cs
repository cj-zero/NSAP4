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
        /// 添加展会
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("AddMeetingData")]
        public async Task<Response<string>> AddMeetingData(AddMeetingDataReq Addmodel)
        {
            var result = new Response<string>();
            try
            {
                result.Result = _serviceMeetingApp.AddMeetingData(Addmodel);

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
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Load")]
        public async Task<TableData> Load(LoadReq Querymodel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.Load(Querymodel, out rowcount);
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
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MyCreatedLoad")]
        public async Task<TableData> MyCreatedLoad(MyCreatedLoadReq Querymodel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.MyCreatedLoad(Querymodel, out rowcount);
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
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MeetingUserList")]
        public async Task<TableData> MeetingUserList(MeetingUserListReq Querymodel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.MeetingUserList(Querymodel, out rowcount);
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
        /// <returns></returns>
        [HttpGet]
        [Route("MeetingUserApply")]
        public Response<bool> MeetingUserApply(int MeetingId)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.MeetingUserApply(MeetingId);

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
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Dispatcher")]
        public Response<List<DispatcherDto>> Dispatcher(DispatcherReq Querymodel)
        {
            int rowcount = 0;
            var result = new Response<List<DispatcherDto>>();
            try
            {
                result.Result = _serviceMeetingApp.Dispatcher(Querymodel, out rowcount);

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
        public Response<bool> Scheduling(List<SchedulingReq> Updatemodel)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = _serviceMeetingApp.Scheduling(Updatemodel);

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
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("SubmittedLod")]
        public async Task<TableData> SubmittedLod(SubmittedReq Querymodel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _serviceMeetingApp.SubmittedLod(Querymodel, out rowcount);
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
        [HttpPost]
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
    }
}
