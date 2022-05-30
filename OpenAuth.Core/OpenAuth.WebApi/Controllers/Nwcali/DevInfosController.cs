using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Response;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 设备与生产订单关联表相关操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Nwcali")]
    public class DevInfosController : ControllerBase
    {
        private readonly DevInfoApp _app;

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="app"></param>
        public DevInfosController(DevInfoApp app)
        {
            _app = app;
        }

        #region 烤机相关

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="Topic">订阅的主题</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> ReceiveMessage(string Topic)
        {
            return await _app.ReceiveMessage(Topic);
        }

        /// <summary>
        /// 边缘计算在线未绑定设备列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> OnlineDeviceList(int page,int limit)
        {
            var result = new TableData();
            try
            {
                return await _app.OnlineDeviceList(page,limit);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }


        /// <summary>
        /// 边缘计算在线已绑定设备列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> OnlineDeviceBindList(int page, int limit)
        {
            var result = new TableData();
            try
            {
                return await _app.OnlineDeviceBindList(page, limit);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 绑定设备
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> BindDevice(BindDeviceReq model)
        {
            var result = new TableData();
            try
            {
                return await _app.BindDevice(model);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 解绑设备
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> UnBindDevice(UnBindDeviceReq model)
        {
            var result = new TableData();
            try
            {
                return await _app.UnBindDevice(model);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 当前生产码关联订单未绑定设备列表
        /// </summary>
        /// <param name="GeneratorCode">生产码</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> NoBindDeviceList(string GeneratorCode)
        {
            var result = new TableData();
            try
            {
                return await _app.NoBindDeviceList(GeneratorCode);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 烤机扫码
        /// </summary>
        /// <param name="GeneratorCode"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> ProductOrderList(string GeneratorCode)
        {
            var result = new TableData();
            try
            {
                return await _app.ProductOrderList(GeneratorCode);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }
        #endregion
    }
}
