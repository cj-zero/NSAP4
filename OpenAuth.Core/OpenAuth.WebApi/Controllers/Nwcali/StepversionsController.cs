﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using EdgeAPI;
using Infrastructure;
using Infrastructure.Helpers;
using Infrastructure.MQTT;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Nwcali.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
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
        private readonly DataService.DataServiceClient _dataServiceClient;
        private readonly MqttNetClient _mqttNetClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="dataServiceClient"></param>
        /// <param name="mqttNetClient"></param>
        public StepVersionsController(StepVersionApp app, DataService.DataServiceClient dataServiceClient, MqttNetClient mqttNetClient)
        {
            _app = app;
            _dataServiceClient = dataServiceClient;
            _mqttNetClient = mqttNetClient;
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
                obj.StepVersionName = obj.StepVersionName.ToUpper();
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
                obj.StepVersionName = obj.StepVersionName.ToUpper();
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
                StepVersionName = StepVersionName.ToUpper();
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
                var res = await _app.ChannelControlAsync(model, stepCount, step_data);
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
                        Log.Logger.Error($"{testData}", ex);
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
                    string topic = $"rt_data/subscribe_{item.EdgeGuid}";
                    string key = $"{_mqttNetClient.clientId}{item.EdgeGuid}";
                    if (!RedisHelper.Exists(key))
                    {
                        await RedisHelper.SetAsync(key, topic, 86400);
                    }
                    var successList = await _app.SaveTestResult(list);
                    await _mqttNetClient.SubscribeAsync(topic);
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
        public async Task<TableData> BakeMachineList(int page, int limit, string GeneratorCode, int type, string key)
        {
            var result = new TableData();
            try
            {
                if (type == 1 && string.IsNullOrWhiteSpace(GeneratorCode))
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
                var json_str = JsonConvert.SerializeObject(new { app_id = "", app_secret = "", edge_guid = EdgeGuid });
                var request = new Request { JsonParameter = Google.Protobuf.ByteString.CopyFromUtf8(json_str) };
                var res = await _dataServiceClient.GetDevInfoAsync(request);
                if (res == null || !res.Success)
                {
                    result.Code = 500;
                    result.Message = res == null ? "设备同步失败!" : Encoding.UTF8.GetString(res.Msg.Memory.ToArray());
                    return result;
                }
                var lowCounts= res.Data.MapDf.Where(c => c.Key == "low_guid").Select(c => c.Value).FirstOrDefault().VBytes.ToList().Count;
                List<BtsDeviceResp> list = new List<BtsDeviceResp>();
                for (var i=0;i<lowCounts;i++)
                {
                    BtsDeviceResp btsDeviceResp=new BtsDeviceResp();
                    btsDeviceResp.edge_guid = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "edge_guid").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.srv_guid = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "bts_server_guid").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.unit_id= Convert.ToInt32(res.Data.MapDf.Where(c => c.Key == "unit_id").Select(c => c.Value).FirstOrDefault().VUint32[i]);
                    btsDeviceResp.bts_server_ip = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "bts_server_ip").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.low_guid = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "low_guid").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.mid_version = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "mid_version").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.bts_server_version = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "bts_server_version").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.production_serial = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "production_serial").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.bts_ids = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "channel_list").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray()).Split(',').ToList();
                    btsDeviceResp.low_no = Convert.ToInt32(res.Data.MapDf.Where(c => c.Key == "low_no").Select(c => c.Value).FirstOrDefault().VUint32[i]);
                    btsDeviceResp.low_version = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "low_version").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.mid_guid = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "mid_guid").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.range_volt = res.Data.MapDf.Where(c => c.Key == "range_volt").Select(c => c.Value).FirstOrDefault().VFloat[i].ToString();
                    btsDeviceResp.dev_uid = Convert.ToInt32(res.Data.MapDf.Where(c => c.Key == "dev_uid").Select(c => c.Value).FirstOrDefault().VUint32[i]);
                    btsDeviceResp.bts_type = Convert.ToInt32(res.Data.MapDf.Where(c => c.Key == "bts_type").Select(c => c.Value).FirstOrDefault().VUint32[i]);
                    btsDeviceResp.range_curr_array = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "range_curr_array").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    list.Add(btsDeviceResp);
                }
                return await _app.SyncDeviceList(list, EdgeGuid);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                return result;
            }
        }

        /// <summary>
        /// 当前扫码对应订单在线已启动测试需要重启
        /// </summary>
        /// <param name="GeneratorCode"></param>
        /// <param name="ItemCode"></param>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> NeedRestartList(string GeneratorCode,string ItemCode, int page, int limit)
        {

            var result = new TableData();
            try
            {
                return await _app.NeedRestartList(GeneratorCode, ItemCode,page, limit);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                return result;
            }
        }
        /// <summary>
        /// 重新启动测试
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> RestartTest(CanStopTestReq model)
        {
            var result = new TableData();
            try
            {
                //停止
                var canStopList= await _app.CanStopTestList(model.stopTests);
                if (canStopList.Data!=null)
                {
                    foreach (var item in canStopList.Data)
                    {
                        var testJson = JsonConvert.SerializeObject(item);
                        var request = new Request { JsonParameter = Google.Protobuf.ByteString.CopyFromUtf8(testJson) };
                        var testRes = _dataServiceClient.ControlCmd(request);
                        string testData = Encoding.UTF8.GetString(testRes.Msg.Memory.ToArray());
                        if (!testRes.Success)
                        {
                            Log.Logger.Error($"停止测试异常{testData}");
                            result.Code = 500;
                            result.Message = testData;
                            return result;
                        }
                    }
                }
                //启动
                var xmlCpntent = XMLHelper.GetXDocument(model.xmlpath).ToString();
                StringReader Reader = new StringReader(xmlCpntent);
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(Reader);
                string work_path = $"{AppDomain.CurrentDomain.BaseDirectory}step\\";
                Directory.CreateDirectory(work_path);
                string filename = DateTime.Now.ToString("yyyyMMddHHmmssffff");
                string dir = $"{work_path}{filename}.xml";
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
                var res = await _app.RestartTest(model.stopTests, stepCount, step_data);
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
                        Log.Logger.Error($"{testData}", ex);
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
                    string topic = $"rt_data/subscribe_{item.EdgeGuid}";
                    string key = $"{_mqttNetClient.clientId}{item.EdgeGuid}";
                    if (!RedisHelper.Exists(key))
                    {
                        await RedisHelper.SetAsync(key, topic, 86400);
                    }
                    var successList = await _app.SaveTestResult(list);
                    await _mqttNetClient.SubscribeAsync(topic);
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
