using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using MQTTnet.Extensions.ManagedClient;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.IO.Compression;
using MQTTnet.Protocol;
using System.Linq;
using Neware.LIMS.Mqtt.Model;
using Newtonsoft.Json.Linq;
//using Neware.LIMS.Model;

namespace Neware.LIMS.Mqtt
{
    class Program
    {
        private static IMqttClient mqttClient;
        private IMqttClientOptions clientOptions;
        //private MQTTOptions QTTOptions;
        public static async Task ConnectMqttServerAsync()
        {
            if (mqttClient == null)
            {
                mqttClient = new MqttFactory().CreateMqttClient();
                //var clientOptions = new MqttClientOptions
                //{
                //    ChannelOptions = new MqttClientTcpOptions
                //    {
                //        Server = AppSetting.AppConfig.RemoteServer.IP,
                //        Port = AppSetting.AppConfig.RemoteServer.Port,
                //    },
                //    Credentials = new MqttClientCredentials
                //    {
                //        Username = AppSetting.AppConfig.RemoteServer.UserName,
                //        Password = Encoding.UTF8.GetBytes(AppSetting.AppConfig.RemoteServer.Password)
                //    },
                //    ClientId = "ERP测试:ERP-Saas-Mqtt-2021.12-1.0" // Guid.NewGuid().ToString("n")
                //};
            }
            //连接处理
            mqttClient.UseConnectedHandler(async handle =>
            {
                //订阅主题
                foreach (var item in Topics.Array)
                {
                    var result = await mqttClient.SubscribeAsync(item, MqttQualityOfServiceLevel.AtLeastOnce);
                }
                //_ = await mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                //            .WithExactlyOnceQoS().WithTopic("edge_msg/#")
                //            .WithAtLeastOnceQoS()
                //            .Build());
                Console.WriteLine("连接成功");
            });
            //接收订阅消息
            mqttClient.UseApplicationMessageReceivedHandler(handle =>
            {
                if (handle.ApplicationMessage.Topic.Contains("edge_msg"))
                {
                    try
                    {
                        var json = Encoding.UTF8.GetString(handle.ApplicationMessage.Payload);
                        JObject jo = JObject.Parse(json);
                        bool isDevinfo = jo.Properties().Any(p => p.Name == "msg_type" && p.Value.ToString() == "3");
                        if (isDevinfo)
                        {
                            var srv_list = jo["chl_info"]["data"].ToList();
                            var srvIndex = 0;
                            //上位机
                            foreach (var item in srv_list)
                            {
                                srvIndex++;
                                Console.WriteLine("第"+ srvIndex + "台上位机IP：" + item["bts_server_ip"]+ "\t\t上位机GUID：" + item["srv_guid"]);
                                var mid_list = item["mid_list"].ToList();
                                if (mid_list != null)
                                {
                                    var midIndex = 0;
                                    foreach (var mid in mid_list)
                                    {
                                        midIndex++;
                                        Console.WriteLine("**************************************第" + midIndex + "条中位机数据**************************************");
                                        var low_list = mid["low_list"].ToList();
                                        var aux_low_list = mid["aux_low_list"].ToList();
                                        if (low_list != null)
                                        {
                                            var lowIndex = 0;
                                            var auxIndex = 0;
                                            foreach (var xwj in low_list)
                                            {
                                                lowIndex++;
                                                var pyh_chl_list = xwj["pyh_chl_list"].ToList();
                                                if (xwj["low_guid"] != null || pyh_chl_list!=null)
                                                {
                                                    Console.WriteLine("第" + midIndex + "台中位机信息设备id：" + mid["dev_uid"] +" 中位机Guid:"+ mid["mid_guid"] + "\t第" + lowIndex + "台下位机信息下位机号：" +xwj["low_no"] + " 下位机Guid:" + xwj["low_guid"]);
                                                    foreach (var phy_chl in pyh_chl_list)
                                                    {
                                                        Console.WriteLine("物理通道号:"+phy_chl+"除8取余数后:"+Convert.ToInt32(phy_chl)%8);

                                                    }
                                                }
                                            }
                                            foreach (var aux in aux_low_list)
                                            {
                                                auxIndex++;
                                                if (aux["low_guid"] != null)
                                                {
                                                    Console.WriteLine("第" + midIndex + "台中位机信息设备id：" + mid["dev_uid"] +" 中位机Guid:"+ mid["mid_guid"] + "\t第" + auxIndex + "台辅助通道辅助通道No:" +aux["low_no"] + " 辅助通道Guid:" + aux["low_guid"]);
                                                }
                                            }
                                            Console.WriteLine();                                        }
                                    }
                                }
                            }
                        }
                        //Console.WriteLine(DateTime.Now.ToString() + "：" + json + "\r\n");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("序列化MQ数据错误" + ex.Message);
                    }
                }
                else if (handle.ApplicationMessage.Topic.Contains("rt_data"))
                {
                    //var RT = eventArgs.ApplicationMessage.Payload.GzipResolve<RTVec>();
                    //await HandleAsync(RT);
                }

                //try
                //{
                //    string thistop = handle.ApplicationMessage.Topic;
                //    Console.WriteLine("标题：" + thistop);
                //    //return;
                //    if (thistop.Contains("edge_msg"))
                //    {
                //        MqttApplicationMessage message = new MqttApplicationMessage();
                //        message.Payload = handle.ApplicationMessage.Payload;
                //        //获取压缩流
                //        using (MemoryStream ms = new MemoryStream(handle.ApplicationMessage.Payload))
                //        {
                //            //解压流
                //            using (GZipStream gzs = new GZipStream(ms, CompressionMode.Decompress))
                //            {
                //                using (MemoryStream new_ms = new MemoryStream())
                //                {
                //                    //解压后赋值到新的流对象
                //                    gzs.CopyTo(new_ms);
                //                }
                //            }
                //        }
                //    }
                //}
                //catch (Exception ex)
                //{
                //    string aa = "";
                //}
            });
            mqttClient.UseDisconnectedHandler(handle =>
            {
                Console.WriteLine($"MQTT 断开连接");
            });

            if ((bool)mqttClient?.IsConnected)
            {
                return;
            }
            var options = new MqttClientOptionsBuilder()
                .WithClientId($"ERP测试:ERP-Saas-Mqtt-2021.12-1.0")//{Guid.NewGuid()}
                .WithTcpServer(AppSetting.AppConfig.RemoteServer.IP, AppSetting.AppConfig.RemoteServer.Port)
                .WithCredentials(AppSetting.AppConfig.RemoteServer.UserName, AppSetting.AppConfig.RemoteServer.Password)
                .WithCleanSession(false)
                .Build();
            try
            {
                var result = await mqttClient.ConnectAsync(options);
                Console.WriteLine(result.ResultCode);
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.Message);
            }
        }


        static async Task Main(string[] args)
        {
            try
            {
                string json = string.Empty;
                using (StreamReader sr = new StreamReader(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app.json")))
                {
                    json = sr.ReadToEnd();
                }
                AppSetting.AppConfig = JsonConvert.DeserializeObject<AppConfig>(json);

                _ = Task.Run(async () =>
                {

                    await ConnectMqttServerAsync();//应用程序作为客户端连接服务端
                                                   //await new Server().Run();//同时应用程序作为服务端发送订阅消息
                });

                while (mqttClient?.IsConnected != true)
                {
                    await Task.Delay(1000);
                    await ConnectMqttServerAsync();//尝试连接
                }

                while (true)
                {
                    //Console.WriteLine("执行中" + DateTime.Now);
                    //CSRedis.QuickHelperBase.se
                    await Task.Delay(2000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + ex.StackTrace);
            }
        }
    }
}
