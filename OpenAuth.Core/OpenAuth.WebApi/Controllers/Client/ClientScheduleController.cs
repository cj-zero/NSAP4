using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Client;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Client
{
    /// <summary>
    /// 客户日程
    /// </summary>
    [Route("api/Client/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Client")]
    public class ClientScheduleController : Controller
    {
        private readonly ClientScheduleApp _clientScheduleApp;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClientScheduleController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClientScheduleApp clientScheduleApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clientScheduleApp = clientScheduleApp;
        }

        /// <summary>
        /// 新增日程
        /// </summary>
        /// <param name="addClueFollowUpReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClientFollowAsync")]
        public async Task<Infrastructure.Response> AddClientScheduleAsync(ClientSchedule clientSchedule, bool isAdd)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _clientScheduleApp.AddClientScheduleAsync(clientSchedule, isAdd);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        /// <summary>
        /// 日程列表
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClientFollowByCodeAsync")]
        public async Task<Response<List<ClientSchedule>>> ClientScheduleByCodeAsync(string CardCode)
        {
            var result = new Response<List<ClientSchedule>>();
            try
            {
                result.Result = await _clientScheduleApp.ClientScheduleByIdAsync(CardCode);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// 删除日程
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("DeleteFollowByCodeAsync")]
        public async Task<Response<bool>> DeleteScheduleByCodeAsync(int Id)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clientScheduleApp.DeleteScheduleByCodeAsync(Id);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

    }
}
