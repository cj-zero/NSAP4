using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EdgeAPI;
using Grpc.Core;
using Grpc.Net.Client;
using Infrastructure;
using Infrastructure.Helpers;
using Infrastructure.MQTT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Nwcali.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.WebApi.Model;
using Serilog;
using Response = Infrastructure.Response;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 烤机工步文件相关操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Nwcali")]
    public class StepVersionsController : ControllerBase
    {
        private readonly StepVersionApp _app;
        private readonly EdgeAPI.DataService.DataServiceClient _dataServiceClient;
        private readonly MqttNetClient _mqttNetClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="dataServiceClient"></param>
        public StepVersionsController(StepVersionApp app, EdgeAPI.DataService.DataServiceClient dataServiceClient, MqttNetClient mqttNetClient)
        {
            _app = app;
            _dataServiceClient = dataServiceClient;
            _mqttNetClient = mqttNetClient;
        }

        /// <summary>
        /// 通道控制
        /// </summary>
        /// <param name="cmd_type">命令类型</param>
        /// <param name="edge_guid">边缘计算GUID</param>
        /// <param name="arg">测试参数</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ChannelControlCmdAsync([FromQuery] string cmd_type, string edge_guid, object arg)
        {
            var result = new TableData();
            try
            {
                var options = new List<ChannelOption> { new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue) };

                var channel = new Grpc.Core.Channel($"{AppSetting.GrpcIP}:{AppSetting.GrpcPort}", ChannelCredentials.Insecure, options);
                DataService.DataServiceClient client = new DataService.DataServiceClient(channel);

                ChannelControlInput input = new ChannelControlInput() { edge_guid = edge_guid, control = new ChannelControlInput.ControlInput { arg = JsonConvert.SerializeObject(arg), cmd_type = cmd_type } };

                var json = JsonConvert.SerializeObject(input);

                EdgeAPI.Response res = null;

                res = await client.ControlCmdAsync(new Request { JsonParameter = Google.Protobuf.ByteString.CopyFromUtf8(json) });

                if (res != null && !res.Success)
                {
                    result.Code = 500;
                    result.Message = "调用gRPC发生异常：" + res.Msg.ToStringUtf8();
                }
                result.Data = res.Msg.ToStringUtf8();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{""}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 查询工步文件详情
        /// </summary>
        /// <param name="id">工步文件id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetStepVersionDetails([FromQuery] int id)
        {
            var result = new TableData();

            try
            {
                return await _app.GetDetails(id);
            }
            catch (Exception ex)
            {

                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{id}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 添加烤机工步文件
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Infrastructure.Response> Add(AddOrUpdateStepVersionReq obj)
        {
            var result = new Infrastructure.Response();
            try
            {
                await _app.Add(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{obj.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        /// <summary>
        /// 修改烤机工步文件
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [HttpPost]
        public Response Update(AddOrUpdateStepVersionReq obj)
        {
            var result = new Infrastructure.Response();
            try
            {
                _app.Update(obj);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        [HttpGet]
        public Task<TableData> Load([FromQuery] QueryStepversionListReq request)
        {
            return _app.Load(request);
        }

        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public async Task<Response> Delete(StepversionReq model)
        {
            var result = new Response();
            try
            {
                await _app.Delete(model.ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
            }

            return result;
        }

        #region 钉钉烤机
        /// <summary>
        /// 主题订阅
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> SubscribeAsync(string topic)
        {
            return await _mqttNetClient.SubscribeAsync(topic);
        }

        /// <summary>
        /// 工步模板列表
        /// </summary>
        /// <param name="SeriesName">系列名称</param>
        /// <param name="StepVersionName">工步型号名称</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DingTalkStepList(string SeriesName, string StepVersionName)
        {
            var result = new TableData();
            try
            {
                return await _app.DingTalkStepList(SeriesName, StepVersionName);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 烤机启动测试
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> ChannelControlAsync(ChannelControlReq model)
        {
            var result = new TableData();
            try
            {
                var xmlCpntent = XMLHelper.GetXDocument(model.xmlpath).ToString();
                StringReader Reader = new StringReader(xmlCpntent);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Reader);
                string work_path = $"{AppDomain.CurrentDomain.BaseDirectory}step\\";
                Directory.CreateDirectory(work_path);
                string filename = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                string dir = $"{work_path}{filename}.xml";
                Log.Logger.Information($"工步文件路径：{dir}");
                xmlDoc.Save(dir);
                if (!System.IO.File.Exists(dir))
                {
                    result.Code = 500;
                    result.Message = "工步文件读取失败!";
                    return result;
                }
                var step = Common.XmlStep.LoadStepFile(dir);
                if (step == null)
                {
                    result.Code = 500;
                    result.Message = "工步文件异常,无法解析!";
                    return result;
                }
                System.IO.File.Delete(dir);
                int stepCount = step.ListStep.Count();
                string step_data = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlCpntent.ToString()));
                var res = await _app.ChannelControlAsync(model, stepCount,step_data);
                var deviceTestList = res.Data;
                foreach (var item in deviceTestList)
                {
                    List<StartTestResp> list = new List<StartTestResp>();
                    var testJson = JsonConvert.SerializeObject(item.canTestDeviceResp);
                    var request = new Request { JsonParameter = Google.Protobuf.ByteString.CopyFromUtf8(testJson) };
                    var testRes = _dataServiceClient.ControlCmd(request);
                    string testData = Encoding.UTF8.GetString(testRes.Msg.Memory.ToArray());
                    StartTestResp startTestResp = new StartTestResp();
                    try
                    {
                        startTestResp = JsonConvert.DeserializeObject<StartTestResp>(testData);
                    }
                    catch (Exception ex)
                    {
                        Log.Logger.Error($"{testData}",ex);
                        result.Code = 500;
                        result.Message = testData;
                        return result;
                    }
                    startTestResp.GeneratorCode = item.GeneratorCode;
                    startTestResp.EdgeGuid = item.EdgeGuid;
                    startTestResp.BtsServerIp = item.BtsServerIp;
                    startTestResp.MidGuid = item.MidGuid;
                    startTestResp.LowGuid = item.LowGuid;
                    startTestResp.SrvGuid = item.SrvGuid;
                    startTestResp.Department = item.Department;
                    startTestResp.stepCount = item.stepCount;
                    startTestResp.MaxRange = item.MaxRange;
                    list.Add(startTestResp);
                    string key = $"rt_data/subscribe_{ item.EdgeGuid}";
                    var successList = await _app.SaveTestResult(list);
                    await _mqttNetClient.SubscribeAsync(key);
                }
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
            }
            return result;
        }

        /// <summary>
        /// 烤机清单
        /// </summary>
        /// <param name="page">分页索引</param>
        /// <param name="limit">分页大小</param>
        /// <param name="GeneratorCode">生产码</param>
        /// <param name="type">获取方式：1-当前订单 2-所有订单</param>
        /// <param name="key"></param> 
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> BakeMachineList(int page, int limit, string GeneratorCode, int type,string key)
        {

            var result = new TableData();
            try
            {
                if (type==1 && string.IsNullOrWhiteSpace(GeneratorCode))
                {
                    result.Code = 500;
                    result.Message = "当前订单查询缺少生产码!";
                    return result;
                }
                return await _app.BakeMachineList(page, limit, GeneratorCode, type, key);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                return result;
            }
        }

        /// <summary>
        /// 烤机结果校验
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<object> DeviceTestCheckResult()
        {
            return await _app.DeviceTestCheckResult();
        }

        /// <summary>
        /// 同步设备数据
        /// </summary>
        /// <param name="EdgeGuid"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SyncDeviceList(string EdgeGuid)
        {

            var result = new TableData();
            try
            {
                var json_str = JsonConvert.SerializeObject(new { app_id ="", app_secret ="", edge_guid = EdgeGuid });
                var request = new Request { JsonParameter = Google.Protobuf.ByteString.CopyFromUtf8(json_str) };
                var res = await _dataServiceClient.GetDevInfoAsync(request);
                var edge_guid=Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "edge_guid").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                var bts_server_guid = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "bts_server_guid").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                var bts_server_ip= Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "bts_server_ip").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                var channel_list = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "channel_list").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                var low_no = res.Data.MapDf.Where(c => c.Key == "low_no").Select(c => c.Value).FirstOrDefault().VUint32.ToArray().FirstOrDefault();
                var low_version= Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "low_version").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                var mid_version = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "mid_version").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                var mid_guid= Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "mid_guid").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                var range_curr_array= Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "range_curr_array").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                var low_guid= Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "low_guid").Select(c => c.Value).FirstOrDefault().VBytes.FirstOrDefault().Memory.ToArray());
                if (res==null || !res.Success)
                {
                    result.Code = 500;
                    result.Message = res==null?"设备同步失败!":Encoding.UTF8.GetString(res.Msg.Memory.ToArray());
                    return result;
                }
                return result;
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                return result;
            }
        }
        #endregion
    }
}
