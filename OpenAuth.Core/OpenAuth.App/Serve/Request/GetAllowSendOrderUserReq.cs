using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenAuth.App.Serve.Request
{
    public class GetAllowSendOrderUserReq : PageReq
    {
        /// <summary>
        /// 客户位置的经度
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// 客户位置的纬度
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// 当前登陆者App用户Id
        /// </summary>
        public int CurrentUserId { get; set; }

        /// <summary>
        /// 技术员名称
        /// </summary>
        public string CurrentUser { get; set; }

        /// <summary>
        /// 原技术员Id
        /// </summary>
        public int TechnicianId { get; set; }

        /// <summary>
        /// 呼叫主题
        /// </summary>
        public string themeCode { get; set; }=String.Empty;
        
    }
}
