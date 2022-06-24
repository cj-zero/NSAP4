using Microsoft.Extensions.Configuration;
using MQTTnet;
using MQTTnet.Client;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.MQTT
{
    /// <summary>
    /// 
    /// </summary>
    public class MqttNetClient
    {
        public static MqttClient mqttClient;
        private IMqttClientOptions options;
        public string clientId = string.Empty;
        private MqttConfig mqttConfig;
        public IConfiguration Configuration { get; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_mqttConfig"></param>
        /// <param name="receivedMessageHanddler"></param>
        /// <param name="configuration"></param>
        public MqttNetClient(MqttConfig _mqttConfig, EventHandler<MqttApplicationMessageReceivedEventArgs> receivedMessageHanddler, IConfiguration configuration)
        {
            mqttConfig = _mqttConfig;
            Configuration = configuration;
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient() as MqttClient;
            clientId = $"{mqttConfig.ClientIdentify}";
            //clientId = $"{mqttConfig.ClientIdentify}-{Md5.Encrypt(Guid.NewGuid().ToString())}" ;
            options = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttConfig.Server, _mqttConfig.Port)
                .WithCredentials(_mqttConfig.Username, _mqttConfig.Password)
                .WithClientId(clientId)
                .WithCleanSession(false)
                .WithCommunicationTimeout(TimeSpan.FromMinutes(10))
                .Build();

            if (receivedMessageHanddler != null)
            {
                //是服务器接收到消息时触发的事件，可用来响应特定消息
                mqttClient.ApplicationMessageReceived += receivedMessageHanddler;
            }
            //是客户端连接成功时触发的事件
            mqttClient.Connected += Connected;

            //是客户端断开连接时触发的事件
            mqttClient.Disconnected += Disconnected;

            mqttClient.ConnectAsync(options);
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            Log.Logger.Error($"Mqtt>>Disconnected【{clientId}】>>已断开连接,断开连接原因:{e.Exception.Message}");
            try
            {
                mqttClient.ConnectAsync(options);
                Log.Logger.Information($"{clientId}重连成功!");
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"{clientId}重连失败! message={ex.Message}");
            }
        }
        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connected(object sender, MqttClientConnectedEventArgs e)
        {
            Log.Logger.Information($"Mqtt>>Connected【{clientId}】>>连接成功!");
            //连接重新订阅
            try
            {
                string edgeKey = "EdgeGuidKeys";
                var redisConnectionString = Configuration.GetValue<string>("AppSetting:Cache:Redis");
                RedisHelper.Initialization(new CSRedis.CSRedisClient(redisConnectionString));
                mqttClient.SubscribeAsync("edge_msg/#");
                Log.Logger.Information($"{clientId}连接成功重新订阅生效 【topic=edge_msg/#】!");
                var edges = RedisHelper.Get(edgeKey);
                if (!string.IsNullOrWhiteSpace(edges))
                {
                    var edge_list = edges.Split(',');
                    foreach (var item in edge_list)
                    {
                        mqttClient.SubscribeAsync($"rt_data/subscribe_{item}");
                        Log.Logger.Information($"{clientId}连接成功重新订阅生效 rt_data/subscribe_{item}!");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"{clientId}连接成功重新订阅失败! message={ex.Message}");
            }
        }

        /// <summary>        
        /// 发送消息
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task PublishMessageAsync(string topic, string content)
        {
            var message = new MqttApplicationMessageBuilder();
            message.WithTopic(topic);
            message.WithPayload(content);
            message.WithAtMostOnceQoS();
            message.WithRetainFlag(false);
            await mqttClient.PublishAsync(message.Build());
        }

        /// <summary>
        /// 订阅Topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public async Task<object> SubscribeAsync(string topic)
        {
            return await mqttClient.SubscribeAsync(topic);
        }
    }
}
