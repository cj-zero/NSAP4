using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting;
using OpenAuth.App.Meeting.Request;
using OpenAuth.Repository.Interface;
using System;
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
    }
}
