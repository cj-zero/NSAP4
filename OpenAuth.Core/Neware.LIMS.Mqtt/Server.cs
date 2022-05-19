using MQTTnet;
using MQTTnet.Client.Receiving;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Neware.LIMS.Mqtt
{
    public class Server
    {
        public async Task Run()
        {
            MqttServerClass serverClass = new MqttServerClass();
            await serverClass.StartMqttServer();
            while (true)
            {
                await Task.Delay(2000);
            }
        }
    }


    public class MqttServerClass
    {
        private IMqttServer mqttServer;
        private List<MqttApplicationMessage> messages = new List<MqttApplicationMessage>();

        public async Task StartMqttServer()
        {
            try
            {
                if (mqttServer == null)
                {
                    var optionsBuilder = new MqttServerOptionsBuilder()
                    .WithDefaultEndpoint()
                    .WithDefaultEndpointPort(AppSetting.AppConfig.LocalServer.Port)
                    //连接拦截器
                    .WithConnectionValidator(
                                c =>
                                {
                                    var flag = c.Username == AppSetting.AppConfig.LocalServer.UserName && c.Password == AppSetting.AppConfig.LocalServer.Password;
                                    if (!flag)
                                    {
                                        c.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                                        return;
                                    }
                                    //设置代码为 Success
                                    c.ReasonCode = MqttConnectReasonCode.Success;
                                    //instances.Add(new UserInstance()  //缓存到内存的List集合当中
                                    //{
                                    //    ClientId = c.ClientId,
                                    //    UserName = c.Username,
                                    //    Password = c.Password
                                    //});
                                })
                    //订阅拦截器
                    .WithSubscriptionInterceptor(
                                 c =>
                                 {
                                     if (c == null) return;
                                     c.AcceptSubscription = true;
                                 })
                    //应用程序消息拦截器
                    .WithApplicationMessageInterceptor(
                                 c =>
                                 {
                                     if (c == null) return;
                                     c.AcceptPublish = true;
                                 })
                    //clean sesison是否生效
                    .WithPersistentSessions();

                    mqttServer = new MqttFactory().CreateMqttServer();

                    //客户端断开连接拦截器
                    //mqttServer.UseClientDisconnectedHandler(c =>
                    //{
                    //    //var user = instances.FirstOrDefault(t => t.ClientId == c.ClientId);
                    //    //if (user != null)
                    //    //{
                    //    //    instances.Remove(user);
                    //    //}
                    //});

                    //服务开始
                    mqttServer.StartedHandler = new MqttServerStartedHandlerDelegate(OnMqttServerStarted);
                    //服务停止
                    mqttServer.StoppedHandler = new MqttServerStoppedHandlerDelegate(OnMqttServerStopped);
                    //客户端连接
                    mqttServer.ClientConnectedHandler = new MqttServerClientConnectedHandlerDelegate(OnMqttServerClientConnected);
                    //客户端断开连接（此事件会覆盖拦截器）
                    mqttServer.ClientDisconnectedHandler = new MqttServerClientDisconnectedHandlerDelegate(OnMqttServerClientDisconnected);
                    //客户端订阅
                    mqttServer.ClientSubscribedTopicHandler = new MqttServerClientSubscribedHandlerDelegate(OnMqttServerClientSubscribedTopic);
                    //客户端取消订阅
                    mqttServer.ClientUnsubscribedTopicHandler = new MqttServerClientUnsubscribedTopicHandlerDelegate(OnMqttServerClientUnsubscribedTopic);
                    //服务端收到消息
                    mqttServer.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnMqttServerApplicationMessageReceived);

                    await mqttServer.StartAsync(optionsBuilder.Build());

                    //主动发送消息到客户端
                    //await mqttServer.PublishAsync(new
                    //     MqttApplicationMessage
                    //{
                    //    Topic = "testtopic",
                    //    Payload = Encoding.UTF8.GetBytes("dsdsd")
                    //});
                    //mqttServer.GetClientStatusAsync();
                    //mqttServer.GetRetainedApplicationMessagesAsync();
                    //mqttServer.GetSessionStatusAsync();

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"MQTT Server start fail.>{ex.Message}");
            }
        }
        private void OnMqttServerStarted(EventArgs e)
        {
            if (mqttServer.IsStarted)
            {
                Console.WriteLine("MQTT服务启动完成！");
            }
        }
        private void OnMqttServerStopped(EventArgs e)
        {
            if (!mqttServer.IsStarted)
            {
                Console.WriteLine("MQTT服务停止完成！");
            }
        }
        private void OnMqttServerClientConnected(MqttServerClientConnectedEventArgs e)
        {
            Console.WriteLine($"客户端[{e.ClientId}]已连接");
        }
        private void OnMqttServerClientDisconnected(MqttServerClientDisconnectedEventArgs e)
        {
            Console.WriteLine($"客户端[{e.ClientId}]已断开连接！");
        }
        private void OnMqttServerClientSubscribedTopic(MqttServerClientSubscribedTopicEventArgs e)
        {
            Console.WriteLine($"客户端[{e.ClientId}]已成功订阅主题[{e.TopicFilter}]！");
        }
        private void OnMqttServerClientUnsubscribedTopic(MqttServerClientUnsubscribedTopicEventArgs e)
        {
            Console.WriteLine($"客户端[{e.ClientId}]已成功取消订阅主题[{e.TopicFilter}]！");
        }
        private void OnMqttServerApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            messages.Add(e.ApplicationMessage);
            Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:fff"));
            Console.WriteLine($"客户端[{e.ClientId}]>> Topic[{e.ApplicationMessage.Topic}] Payload[{Encoding.UTF8.GetString(e.ApplicationMessage.Payload ?? new byte[] { })}] Qos[{e.ApplicationMessage.QualityOfServiceLevel}] Retain[{e.ApplicationMessage.Retain}]");
        }
    }
}
