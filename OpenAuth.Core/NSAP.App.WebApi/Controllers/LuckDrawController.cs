using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSAP.App.WebApi.Controllers
{
    /// <summary>
    /// App抽奖
    /// </summary>
    [Route("ErpAppApi/[controller]/[action]")]
    [ApiController]
    public class LuckDrawController : Controller
    {
        private readonly AppLuckDrawApp _appLuckDrawApp;
        private readonly AppUserBindApp _app;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appLuckDrawApp"></param>
        /// <param name="app"></param>
        public LuckDrawController(AppLuckDrawApp appLuckDrawApp, AppUserBindApp app)
        {
            _appLuckDrawApp = appLuckDrawApp;
            _app = app;
        }

        #region 售后报修抽奖中奖名单生成
        /// <summary>
        /// 计算中奖名单
        /// 中奖规则：每周一从上周参与获动的用户中随机抽取20个中奖名额
        /// 同一服务Id参与一次，不同服务id相同序列号和呼叫主题也只参与一次,相同序列编号呼叫主题被其他的呼叫主题全包含也只参与一次
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> LuckyDrawForRepair()
        {
            var result = new TableData();
            try
            {
                result = await _appLuckDrawApp.LuckyDrawForRepair();
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// 获取App用户列表
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> AppUserList(AppUserReq model)
        {
            var result = new TableData();
            try
            {
                result = await _app.AppUserList(model);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取App用户列表带部门
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UserList(AppUserReq model)
        {
            var result = new TableData();
            try
            {
                result = await _app.UserList(model);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取部门用户列表
        /// </summary>
        /// <param name="AppUserId"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DepartmentUserList(int AppUserId,int pageIndex=1,int pageSize=10)
        {
            var result = new TableData();
            try
            {
                result = await _app.DepartmentUserList(AppUserId,pageIndex, pageSize);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }
            return result;
        }
    }
}
