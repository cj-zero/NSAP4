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
using OpenAuth.Repository.Domain.Serve;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Clue
{
    /// <summary>
    /// 线索联系人
    /// </summary>
    [Route("api/Clue/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Clue")]
    public class ClueContactsController : Controller
    {
        private readonly ClueApp _clueApp;
        private readonly FileApp _app;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClueContactsController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClueApp clueApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clueApp = clueApp;
            this._app = app;
        }
        /// <summary>
        /// 联系人列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClueContactsByIdAsync")]
        public TableData ClueContactsByIdAsync(int ClueId)
        {
            int rowcount = 0;

            var result = new TableData();
            try
            {
                result.Data = _clueApp.ClueContactsByIdAsync(ClueId, out rowcount);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 联系人详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ContactsByIdInfoAsync")]
        public async Task<Response<ClueContacts>> ContactsByIdInfoAsync(int Id)
        {
            var result = new Response<ClueContacts>();
            try
            {
                result.Result = await _clueApp.ContactsByIdInfoAsync(Id);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 新增联系人
        /// </summary>
        /// <param name="addClueContactsReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClueContactsAsync")]
        public async Task<Response<string>> AddClueContactsAsync(AddClueContactsReq addClueContactsReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _clueApp.AddClueContactsAsync(addClueContactsReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 设置默认联系人
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("IsDefaultClueContactsAsync")]
        public async Task<Response<bool>> IsDefaultClueContactsAsync(int Id, int ClueId)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.IsDefaultClueContactsAsync(Id, ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 编辑更新联系人
        /// </summary>
        /// <param name="updateClueContactsReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateClueContactsAsync")]
        public async Task<Response<bool>> UpdateClueContactsAsync(UpdateClueContactsReq updateClueContactsReq)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.UpdateClueContactsAsync(updateClueContactsReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 删除联系人
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("DeleteContactsByIdAsync")]
        public async Task<Response<string>> DeleteContactsByIdAsync(List<int> Ids)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _clueApp.DeleteContactsByIdAsync(Ids);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
