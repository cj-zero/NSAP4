﻿using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Response;
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
    public class LuckDrawController : Controller
    {
        private readonly AppLuckDrawApp _appLuckDrawApp;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="appScanCodeApp"></param>
        public LuckDrawController(AppLuckDrawApp appLuckDrawApp)
        {
            _appLuckDrawApp = appLuckDrawApp;
        }
        #region 售后报修抽奖中奖名单生成
        /*中奖规则：每周一从上周参与获动的用户中随机抽取20个中奖名额同一报修序列号只能参与一次*/
        /// <summary>
        /// 计算中奖名单
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
    }
}
