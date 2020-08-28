using System;
using System.Collections.Generic;
using System.Text;

namespace Neware.Cap
{
    public class CapSettings
    {
        /// <summary>
        /// 是否开启面板
        /// </summary>
        public bool UseDashboard { get; set; } = false;
        public RabbitMqSetting RabbitMq { get; set; }
        public string Version { get; set; }
        public string MySqlConnectionString { get; set; }
        /// <summary>
        /// 成功消息的过期时间（秒）。 当消息发送或者消费成功时候，在时间达到 SucceedMessageExpiredAfter 秒时候将会从 Persistent 中删除，你可以通过指定此值来设置过期的时间。
        /// </summary>
        public int SucceedMessageExpiredAfter { get; set; } = 24 * 3600;
        /// <summary>
        /// FailedRetryInterval
        /// </summary>
        public int FailedRetryInterval { get; set; } = 60;
        /// <summary>
        /// 消费者线程并行处理消息的线程数，当这个值大于1时，将不能保证消息执行的顺序。
        /// </summary>
        public int ConsumerThreadCount { get; set; } = 1;
        /// <summary>
        /// 重试的最大次数。当达到此设置值时，将不会再继续重试，通过改变此参数来设置重试的最大次数。
        /// </summary>
        public int FailedRetryCount { get; set; } = 50;
    }
    public class RabbitMqSetting
    {
        public string HostName { get; set; }

        public string Password { get; set; }
        public int Prot { get; set; }
        public string UserName { get; set; }
    }
}
