using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Clue.ModelDto;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting.ModelDto;
using OpenAuth.App.ProductModel.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Clue
{
    /// <summary>
    /// 线索日程
    /// </summary>
    [Route("api/Clue/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Clue")]
    public class ClueScheduleController : Controller
    {
        private readonly ClueApp _clueApp;
        private readonly FileApp _app;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClueScheduleController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClueApp clueApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clueApp = clueApp;
            this._app = app;
        }
      
        /// <summary>
        /// 新增日程
        /// </summary>
        /// <param name="addClueScheduleReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClueScheduleAsync")]
        public async Task<Response<string>> AddClueScheduleAsync(AddClueScheduleReq addClueScheduleReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _clueApp.AddClueScheduleAsync(addClueScheduleReq);
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
        /// <returns></returns>
        [HttpGet]
        [Route("UserListAsync")]
        public async Task<Response<List<TextVaule>>> UserListAsync()
        {
            var result = new Response<List<TextVaule>>();
            try
            {
                result.Result = _clueApp.UserListAsync();
            }
            catch (Exception ex)
            {
                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 日程列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClueScheduleByIdAsync")]
        public async Task<Response<List<ClueScheduleListDto>>> ClueScheduleByIdAsync(int ClueId)
        {
            var result = new Response<List<ClueScheduleListDto>>();
            try
            {
                result.Result = await _clueApp.ClueScheduleByIdAsync(ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
