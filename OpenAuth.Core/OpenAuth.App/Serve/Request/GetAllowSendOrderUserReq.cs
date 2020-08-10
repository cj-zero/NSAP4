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
        /// 经度
        /// </summary>
        public decimal Longitude { get; set; }

        /// <summary>
        /// 纬度
        /// </summary>
        public decimal Latitude { get; set; }

        /// <summary>
        /// 当前登陆者App用户Id
        /// </summary>
        public int CurrentUserId { get; set; }
    }
}
