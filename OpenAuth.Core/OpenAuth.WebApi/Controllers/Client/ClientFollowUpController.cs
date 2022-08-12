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
    /// 客户跟进
    /// </summary>
    [Route("api/Client/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Client")]
    public class ClientFollowUpController : Controller
    {
        private readonly ClientInfoApp _clientApp;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClientFollowUpController(IUnitWork UnitWork, FileApp app, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClientInfoApp clientApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clientApp = clientApp;
        }

        /// <summary>
        /// 新增跟进
        /// </summary>
        /// <param name="addClueFollowUpReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("AddClientFollowAsync")]
        public async Task<Infrastructure.Response> AddClientFollowAsync(ClientFollowUp clientFollowUp, bool isAdd)
        {
            var response = new Infrastructure.Response();
            try
            {
                response = await _clientApp.AddClientFollowAsync(clientFollowUp, isAdd);
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
            }
            return response;
        }


        /// <summary>
        /// 跟进列表
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClientFollowByCodeAsync")]
        public async Task<Response<List<ClientFollowUp>>> ClientFollowByCodeAsync(string CardCode)
        {
            var result = new Response<List<ClientFollowUp>>();
            try
            {
                result.Result = await _clientApp.ClientFollowUpByIdAsync(CardCode);
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
        [HttpGet]
        [Route("DeleteFollowByCodeAsync")]
        public async Task<Response<bool>> DeleteFollowByCodeAsync(int Id)
        {
            var result = new Response<bool>();
            try
            {
                result.Result = await _clientApp.DeleteFollowByCodeAsync(Id);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }


        /// <summary>
        /// 常用语列表
        /// </summary>
        /// <param name="CardCode"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("ClientFollowUpPhraseAsync")]
        public async Task<Response<List<ClientFollowUpPhrase>>> ClientFollowUpPhraseAsync()
        {
            var result = new Response<List<ClientFollowUpPhrase>>();
            try
            {
                result.Result = await _clientApp.ClientFollowUpPhraseAsync();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

    }
}
