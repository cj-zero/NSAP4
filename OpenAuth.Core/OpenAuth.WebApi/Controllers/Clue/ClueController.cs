﻿using Infrastructure;
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
using static OpenAuth.App.Clue.ModelDto.KuaiBaosHelper;

namespace OpenAuth.WebApi.Controllers.Clue
{
    /// <summary>
    /// 线索主数据
    /// </summary>
    [Route("api/Clue/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Clue")]
    public class ClueController : Controller
    {
        private readonly ClueApp _clueApp;
        private readonly FileApp _app;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClueController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClueApp clueApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clueApp = clueApp;
            this._app = app;
        }
        /// <summary>
        /// 线索列表
        /// </summary>
        /// <param name="clueListReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetClueListAsync")]
        public TableData GetClueListAsync(ClueListReq clueListReq)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _clueApp.GetClueListAsync(clueListReq, out rowcount);
                result.Count = rowcount;
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 新增线索
        /// </summary>
        /// <param name="addClueReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClueAsync")]
        public async Task<Response<string>> AddClueAsync(AddClueReq addClueReq)
        {
            var result = new Response<string>();
            try
            {
                result.Result = await _clueApp.AddClueAsync(addClueReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 线索详情
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClueByIdAsync")]
        public async Task<Response<ClueInfoDto>> ClueByIdAsync(int ClueId)
        {
            var result = new Response<ClueInfoDto>();
            try
            {
                result.Result = await _clueApp.ClueByIdAsync(ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 编辑更新线索
        /// </summary>
        /// <param name="updateClueReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("UpdateClueAsync")]
        public async Task<Response<bool>> UpdateClueAsync(UpdateClueReq updateClueReq)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.UpdateClueAsync(updateClueReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("DeleteClueByIdAsync")]
        public async Task<Response<bool>> DeleteClueByIdAsync(List<int> ClueId)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.DeleteClueByIdAsync(ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 解析地址
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetAddress")]
        public Response<KuaiBaoResponse> GetAddress(string address)
        {
            var result = new Response<KuaiBaoResponse>();
            try
            {
                result.Result = _clueApp.GetAddres(address);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 新增标签
        /// </summary>
        /// <param name="Tags"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddTag")]
        public async Task<Response<bool>> AddTag(AddTag Tags)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clueApp.AddTag(Tags);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取标签
        /// </summary>
        /// <param name="ClueId"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("GetClueTagById")]
        public async Task<Response<List<string>>> GetClueTagById(int ClueId)
        {
            var result = new Response<List<string>>();
            try
            {
                result.Result = await _clueApp.GetClueTagById(ClueId);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
