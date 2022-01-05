using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Clue.Request;
using OpenAuth.App.Interface;
using OpenAuth.App.ProductModel.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Interface;
using System;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.Clue
{
    [Route("api/Clue/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Clue")]
    public class ClueController : Controller
    {
        private readonly ClueApp _clueApp;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ClueController(IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth, ClueApp clueApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _clueApp = clueApp;
        }
        /// <summary>
        /// 线索列表
        /// </summary>
        /// <param name="clueListReq"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetClueListAsync")]
        public async Task<TableData> GetClueListAsync(ClueListReq clueListReq)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = await _clueApp.GetClueListAsync(clueListReq, out rowcount);
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
                result.Result= await _clueApp.AddClueAsync(addClueReq);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
