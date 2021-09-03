using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 公共操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "General")]
    public class CommonController : ControllerBase
    {
        private CommonApp _app;
        public CommonController(CommonApp app)
        {
            _app = app;
        }

        /// <summary>
        /// 主页顶部数量汇总
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HomePageCardInfo()
        {
            return await _app.GetTopNumCard();
        }

        /// <summary>
        /// 首页图表统计
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> HomePageChartInfo()
        {
            return await _app.GetChartInfo();
        }
    }
}
