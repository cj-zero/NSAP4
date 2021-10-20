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
        /// 列表
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
        /// 我创建的列表
        /// </summary>
        /// <param name="Querymodel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("MyCreatedLoad")]
        public async Task<TableData> MyCreatedLoad(LoadReq Querymodel)
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
    }
}
