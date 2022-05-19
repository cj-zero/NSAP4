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
        /// 
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        void SubscribeAsyncResult(string topic, byte[] payload);
    }
}
