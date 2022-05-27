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

        /// <summary>
        /// 实例化
        /// </summary>
        /// <param name="_mqttConfig"></param>
        /// <param name="receivedMessageHanddler"></param>
        public MqttNetClient(MqttConfig _mqttConfig, EventHandler<MqttApplicationMessageReceivedEventArgs> receivedMessageHanddler
            )
        {
            mqttConfig = _mqttConfig;
            var factory = new MqttFactory();
            mqttClient = factory.CreateMqttClient() as MqttClient;
            clientId = $"MqttErpClient_{mqttConfig.ClientIdentify}";
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
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Disconnected(object sender, MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine($"Mqtt>>Disconnected【{clientId}】>>已断开连接");
            try
            {
                mqttClient.ConnectAsync(options);
                Console.WriteLine($"Mqtt>>Connected【{clientId}】>>连接成功");
            }
            catch (Exception)
            {
                Console.WriteLine($"Mqtt>>Disconnected【{clientId}】>>重连失败");
            }
        }
        /// <summary>
        /// 连接成功
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connected(object sender, MqttClientConnectedEventArgs e)
        {
            Console.WriteLine($"Mqtt>>Connected【{clientId}】>>连接成功");
            //连接重新订阅
            try
            {
                mqttClient.SubscribeAsync("edge_msg/#");
                Serilog.Log.Logger.Information($"{clientId}连接成功重新订阅生效 【topic=edge_msg/#】!");
            }
            catch (Exception ex)
            {
                Serilog.Log.Logger.Error($"{clientId}连接成功重新订阅失败 【topic=edge_msg/#】!", ex);
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
