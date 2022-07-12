using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Interface
{
    /// <summary>
    /// 
    /// </summary>
    public interface IMqttSubscribe
    {
        /// <summary>
        /// 设备变更订阅
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        bool SubscribeEdgeMsg(byte[] payload);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        bool SubscribeRtData(byte[] payload);
    }
}
