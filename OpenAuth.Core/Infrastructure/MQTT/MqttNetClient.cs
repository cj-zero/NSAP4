using MQTTnet;
using MQTTnet.Client;
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
        private string clientId = string.Empty;
        private MqttConfig mqttConfig;
        //缓存自行注入
        //private readonly ICache _iCache;

        /// <summary>
        /// 实例化 
        /// </summary>
        /// <param name="server">服务器地址</param>
        /// <param name="port">端口号</param>
        /// <param name="topic">订阅主题</param>
        /// <param name="clientIdPre">客户端id前缀</param>
        public MqttNetClient(MqttConfig _mqttConfig, EventHandler<MqttApplicationMessageReceivedEventArgs> receivedMessageHanddler
            )
        {
            mqttConfig = _mqttConfig;
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient() as MqttClient;
            //clientId = "MQTTErpClient:" + Guid.NewGuid();
            clientId = "subscribe_mqtt915022021416";
            //实例化一个MqttClientOptionsBulider
            options = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttConfig.Server, _mqttConfig.Port)
                .WithCredentials(_mqttConfig.Username, _mqttConfig.Password)
                .WithClientId(clientId)
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
        /// <param name="e"></param>
        /// <returns></returns>
        private void Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine($"Mqtt>>Disconnected【{clientId}】>>已断开连接");
            try
            {
                mqttClient.ConnectAsync(options);
            }
            catch (Exception)
            {
                Console.WriteLine($"Mqtt>>Disconnected【{clientId}】>>重连失败");
            }
        }

        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private void Connected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine($"Mqtt>>Connected【{clientId}】>>连接成功");
            //连接重新订阅
            if (mqttConfig.TopicList != null)
            {
                foreach (var topic in mqttConfig.TopicList)
                {
                    mqttClient.SubscribeAsync(topic);
                }
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
