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
    /// 线索跟进
    /// </summary>
    [Route("api/Clue/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Clue")]
    public class ClueFollowUpController : Controller
    {
        private readonly ClueApp _clueApp;
        private readonly FileApp _app;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClueFollowUpController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClueApp clueApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clueApp = clueApp;
            this._app = app;
        }

        /// <summary>
        /// 新增跟进
        /// </summary>
        /// <param name="addClueFollowUpReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClueFollowAsync")]
        public async Task<Response<string>> AddClueFollowAsync(AddClueFollowUpReq addClueFollowUpReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _clueApp.AddClueFollowAsync(addClueFollowUpReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 新建客户跟联系人下拉框
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ContactsListAsync")]
        public async Task<Response<List<TextVaule>>> ContactsListAsync(int ClueId)
        {
            var result = new Response<List<TextVaule>>();
            try
            {
                result.Result = await _clueApp.ContactsListAsync(ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 跟进列表
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClueFollowByIdAsync")]
        public async Task<Response<List<ClueFollowUpListDto>>> ClueFollowByIdAsync(int ClueId)
        {
            var result = new Response<List<ClueFollowUpListDto>>();
            try
            {
                result.Result = await _clueApp.ClueFollowUpByIdAsync(ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 跟进详情
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("FollowByIdAsync")]
        public async Task<Response<ClueFollowUp>> FollowByIdAsync(int Id)
        {
            var result = new Response<ClueFollowUp>();
            try
            {
                result.Result = await _clueApp.FollowByIdAsync(Id);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 编辑更新跟进
        /// </summary>
        /// <param name="addClueFollowUpReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateClueFollowAsync")]
        public async Task<Response<bool>> UpdateClueFollowAsync(UpdateClueFollowUpReq updateClueFollowUpReq)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.UpdateClueFollowAsync(updateClueFollowUpReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 删除跟进
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteFollowByIdAsync")]
        public async Task<Response<bool>> DeleteFollowByIdAsync(List<int> Ids)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.DeleteFollowByIdAsync(Ids);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 跟进附件
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetClueFileByIdAsync")]
        public async Task<Response<List<ClueFileUploadDto>>> GetClueFileByIdAsync(int Id)
        {
            var result = new Response<List<ClueFileUploadDto>>();
            try
            {
                result.Result = await _clueApp.GetClueFileByIdAsync(Id);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
