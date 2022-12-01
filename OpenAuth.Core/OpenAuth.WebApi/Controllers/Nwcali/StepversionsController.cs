using System;
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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Nwcali;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="app"></param>
        /// <param name="dataServiceClient"></param>
        public StepVersionsController(StepVersionApp app, DataService.DataServiceClient dataServiceClient)
        {
            _app = app;
            _dataServiceClient = dataServiceClient;
        }

        /// <summary>
        /// 查询工步文件详情
        /// </summary>
        /// <param name="id">工步文件id</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetStepVersionDetails(int id)
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
        public async Task<Response> Add(AddOrUpdateStepVersionReq obj)
        {
            var result = new Response();
            try
            {
                obj.StepVersionName = obj.StepVersionName.ToUpper();
                if (obj.SeriesName == "6" || obj.SeriesName == "7")
                {
                    if (string.IsNullOrWhiteSpace(obj.FilePath2) || string.IsNullOrWhiteSpace(obj.FilePath))
                    {
                        result.Code = 500;
                        result.Message = $"{obj.SeriesName}系列必须上传两个工步文件!";
                        return result;
                    }
                    if (string.IsNullOrWhiteSpace(obj.FileName) || string.IsNullOrWhiteSpace(obj.FileName))
                    {
                        result.Code = 500;
                        result.Message = $"未填写工步名称!";
                        return result;
                    }
                    if (obj.FirstStart != 1 && obj.FirstStart != 2)
                    {
                        result.Code = 500;
                        result.Message = $"优先启动工步设置异常!";
                        return result;
                    }
                }
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
        public async Task<Response> Update(AddOrUpdateStepVersionReq obj)
        {
            var result = new Response();
            try
            {
                obj.StepVersionName = obj.StepVersionName.ToUpper();
                if (obj.SeriesName == "6" || obj.SeriesName == "7")
                {
                    if (string.IsNullOrWhiteSpace(obj.FilePath2) || string.IsNullOrWhiteSpace(obj.FilePath))
                    {
                        result.Code = 500;
                        result.Message = $"{obj.SeriesName}系列必须上传两个工步文件!";
                        return result;
                    }
                    if (string.IsNullOrWhiteSpace(obj.FileName) || string.IsNullOrWhiteSpace(obj.FileName))
                    {
                        result.Code = 500;
                        result.Message = $"未填写工步名称!";
                        return result;
                    }
                    if (obj.FirstStart != 1 && obj.FirstStart != 2)
                    {
                        result.Code = 500;
                        result.Message = $"优先启动工步设置异常!";
                        return result;
                    }
                }
                await _app.Update(obj);
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
        /// 工步列表
        /// </summary>
        /// <param name="SeriesName"></param>
        /// <param name="Current"></param>
        /// <param name="Voltage"></param>
        /// <param name="CurrentUnit"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> DingTalkStepList(string SeriesName, decimal Current, decimal Voltage, string CurrentUnit)
        {
            var result = new TableData();
            try
            {
                CurrentUnit = string.IsNullOrWhiteSpace(CurrentUnit) ? "" : CurrentUnit;
                if (CurrentUnit.ToUpper().Equals("MA"))
                {
                    Current /= 1000;
                }
                if (string.IsNullOrWhiteSpace(SeriesName))
                {
                    result.Code = 500;
                    result.Message = $"工步获取缺少工步系列参数!";
                    return result;
                }
                return await _app.DingTalkStepList(SeriesName, Current, Voltage);
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
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> SyncDeviceList()
        {
            var result = new TableData();
            try
            {
                var edge_list = await _app.EdgeGuidList();
                if (!edge_list.Data.Any())
                {
                    result.Code = 500;
                    result.Message = "当前部门还未上传过任何边缘计算数据!";
                    return result;
                }
                string EdgeGuid = edge_list.Data.FirstOrDefault();
                var json_str = JsonConvert.SerializeObject(new { app_id = "", app_secret = "", edge_guid = EdgeGuid });
                var request = new Request { JsonParameter = Google.Protobuf.ByteString.CopyFromUtf8(json_str) };
                var res = await _dataServiceClient.GetDevInfoAsync(request);
                if (res == null || !res.Success)
                {
                    result.Code = 500;
                    result.Message = res == null ? "设备同步失败!" : Encoding.UTF8.GetString(res.Msg.Memory.ToArray());
                    return result;
                }
                var lowCounts = res.Data.MapDf.Where(c => c.Key == "low_guid").Select(c => c.Value).FirstOrDefault().VBytes.ToList().Count;
                List<BtsDeviceResp> list = new List<BtsDeviceResp>();
                for (var i = 0; i < lowCounts; i++)
                {
                    BtsDeviceResp btsDeviceResp = new BtsDeviceResp();
                    btsDeviceResp.edge_guid = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "edge_guid").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.srv_guid = Encoding.UTF8.GetString(res.Data.MapDf.Where(c => c.Key == "bts_server_guid").Select(c => c.Value).FirstOrDefault().VBytes[i].Memory.ToArray());
                    btsDeviceResp.unit_id = Convert.ToInt32(res.Data.MapDf.Where(c => c.Key == "unit_id").Select(c => c.Value).FirstOrDefault().VUint32[i]);
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
        #endregion


        #region 工步启动相关
        /// <summary>
        /// 获取启动列表
        /// </summary>
        /// <param name="GeneratorCode"></param>
        /// <param name="FilterType">过滤类型 1:生产码  2:下位机</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CanStartTestList(string GeneratorCode, int FilterType)
        {
            var result = new TableData();
            try
            {
                return await _app.CanStartTestList(GeneratorCode, FilterType);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                return result;
            }
        }


        /// <summary>
        /// 获取重启动列表
        /// </summary>
        /// <param name="GeneratorCode"></param>
        /// <param name="FilterType">过滤类型 1:生产码  2:下位机</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> CanResStartTestList(string GeneratorCode, int FilterType)
        {
            var result = new TableData();
            try
            {
                return await _app.CanResStartTestList(GeneratorCode, FilterType);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                return result;
            }
        }

        /// <summary>
        /// 启动测试
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> StartTest(ChannelControlReq model)
        {
            var result = new TableData();
            List<DeviceTestResponse> deviceTestResponses = new List<DeviceTestResponse>();
            #region 参数判断
            if (model.FilterType == 1)
            {
                if (model.GeneratorCode.Count <= 0)
                {
                    result.Code = 500;
                    result.Message = $"请选择需要启动的生产码!";
                    return result;
                }
            }
            else
            {
                if (model.lowDeviceLists.Count <= 0)
                {
                    result.Code = 500;
                    result.Message = $"请选择需要启动的下位机!";
                    return result;
                }
            }
            if (model.SeriesName == "6" || model.SeriesName == "7")
            {
                if (string.IsNullOrWhiteSpace(model.FilePath2) || string.IsNullOrWhiteSpace(model.FilePath))
                {
                    result.Code = 500;
                    result.Message = $"{model.SeriesName}系列缺少工步文件启动失败!";
                    return result;
                }
                if (model.TestType == null || model.TestType <= 0)
                {
                    result.Code = 500;
                    result.Message = $"请选择启动模式!";
                    return result;
                }
                if (model.FirstStart != 1 && model.FirstStart != 2)
                {
                    result.Code = 500;
                    result.Message = $"{model.SeriesName}系列工步未设置优先启动!";
                    return result;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.FilePath))
                {
                    result.Code = 500;
                    result.Message = $"缺少工步文件启动失败!";
                    return result;
                }
            }
            #endregion
            try
            {
                #region 设备数据
                List<StartDeviceListResp> deviceList = new List<StartDeviceListResp>();
                if (model.FilterType == 1)
                {
                    deviceList = _app.DeviceListByCode(model.GeneratorCode).Result;
                }
                else
                {
                    deviceList = _app.DeviceListByLow(model.lowDeviceLists).Result;
                }
                #endregion

                #region 工步数据
                var FilePathContent = _app.StepContent(model.FilePath);
                if (FilePathContent.Code != 200)
                {
                    result.Code = FilePathContent.Code;
                    result.Message = $"{model.FilePath}{FilePathContent.Message}";
                    return result;
                }
                var FilePathContent2 = new StepContentResp();
                if (model.SeriesName == "6" || model.SeriesName == "7")
                {
                    var FilePath2Content = _app.StepContent(model.FilePath2);
                    if (FilePath2Content.Code != 200)
                    {
                        result.Code = FilePath2Content.Code;
                        result.Message = $"{model.FilePath2}{FilePath2Content.Message}";
                        return result;
                    }
                    FilePathContent2 = FilePath2Content.Data;
                }
                #endregion

                #region 停止测试
                var canStopList = _app.CanStopTestList(deviceList);
                if (canStopList.Data != null)
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
                #endregion

                #region 启动数据
                if (model.SeriesName == "6" || model.SeriesName == "7")
                {
                    var res = _app.ShortCircuitStart(deviceList, FilePathContent.Data.stepCount, FilePathContent.Data.stepData, FilePathContent2.stepCount, FilePathContent2.stepData, model.FirstStart, model.TestType);
                    if (res.Code != 200)
                    {
                        result.Code = res.Code;
                        result.Message = res.Message;
                        return result;
                    }
                    deviceTestResponses = res.Data;
                }
                else
                {
                    var res = _app.NormalStart(deviceList, FilePathContent.Data.stepCount, FilePathContent.Data.stepData);
                    if (res.Code != 200)
                    {
                        result.Code = res.Code;
                        result.Message = res.Message;
                        return result;
                    }
                    deviceTestResponses = res.Data;
                }
                #endregion
                foreach (var item in deviceTestResponses)
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
                    startTestResp.FileIds = model.FileIds;
                    list.Add(startTestResp);
                    var successList = await _app.SaveTestResult(list);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"测试启动异常{e.Message},参数:{JsonConvert.SerializeObject(model)}");
                result.Code = 500;
                result.Message = e.Message;
                return result;
            }
            return result;
        }



        /// <summary>
        /// 重新启动测试
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> RestartTest(ChannelControlReq model)
        {
            var result = new TableData();
            List<DeviceTestResponse> deviceTestResponses = new List<DeviceTestResponse>();
            #region 参数判断
            if (model.FilterType == 1)
            {
                if (model.GeneratorCode.Count <= 0)
                {
                    result.Code = 500;
                    result.Message = $"请选择需要重启动的生产码!";
                    return result;
                }
            }
            else
            {
                if (model.lowDeviceLists.Count <= 0)
                {
                    result.Code = 500;
                    result.Message = $"请选择需要重启动的下位机!";
                    return result;
                }
            }
            if (model.SeriesName == "6" || model.SeriesName == "7")
            {
                if (string.IsNullOrWhiteSpace(model.FilePath2) || string.IsNullOrWhiteSpace(model.FilePath))
                {
                    result.Code = 500;
                    result.Message = $"{model.SeriesName}系列缺少工步文件重启动失败!";
                    return result;
                }
                if (model.TestType == null || model.TestType <= 0)
                {
                    result.Code = 500;
                    result.Message = $"请选择启动模式!";
                    return result;
                }
                if (model.FirstStart != 1 && model.FirstStart != 2)
                {
                    result.Code = 500;
                    result.Message = $"{model.SeriesName}系列工步未设置优先启动!";
                    return result;
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(model.FilePath))
                {
                    result.Code = 500;
                    result.Message = $"缺少工步文件启动失败!";
                    return result;
                }
            }
            #endregion

            try
            {
                #region 设备数据
                List<StartDeviceListResp> deviceList = new List<StartDeviceListResp>();
                if (model.FilterType == 1)
                {
                    deviceList = _app.DeviceListByCode(model.GeneratorCode).Result;
                }
                else
                {
                    deviceList = _app.DeviceListByLow(model.lowDeviceLists).Result;
                }
                #endregion

                #region 工步数据
                var FilePathContent = _app.StepContent(model.FilePath);
                if (FilePathContent.Code != 200)
                {
                    result.Code = FilePathContent.Code;
                    result.Message = $"{model.FilePath}{FilePathContent.Message}";
                    return result;
                }
                var FilePathContent2 = new StepContentResp();
                if (model.SeriesName == "6" || model.SeriesName == "7")
                {
                    var FilePath2Content = _app.StepContent(model.FilePath2);
                    if (FilePath2Content.Code != 200)
                    {
                        result.Code = FilePath2Content.Code;
                        result.Message = $"{model.FilePath2}{FilePath2Content.Message}";
                        return result;
                    }
                    FilePathContent2 = FilePath2Content.Data;
                }
                #endregion

                #region 停止测试
                var canStopList = _app.CanStopTestList(deviceList);
                if (canStopList.Data != null)
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
                #endregion

                #region 启动数据
                if (model.SeriesName == "6" || model.SeriesName == "7")
                {
                    var res = _app.ShortCircuitStart(deviceList, FilePathContent.Data.stepCount, FilePathContent.Data.stepData, FilePathContent2.stepCount, FilePathContent2.stepData, model.FirstStart, model.TestType);
                    if (res.Code != 200)
                    {
                        result.Code = res.Code;
                        result.Message = res.Message;
                        return result;
                    }
                    deviceTestResponses = res.Data;
                }
                else
                {
                    var res = _app.NormalStart(deviceList, FilePathContent.Data.stepCount, FilePathContent.Data.stepData);
                    if (res.Code != 200)
                    {
                        result.Code = res.Code;
                        result.Message = res.Message;
                        return result;
                    }
                    deviceTestResponses = res.Data;
                }
                #endregion
                foreach (var item in deviceTestResponses)
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
                    startTestResp.FileIds = model.FileIds;
                    list.Add(startTestResp);
                    var successList = await _app.SaveTestResult(list);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error($"重启测试异常{e.Message},参数:{JsonConvert.SerializeObject(model)}");
                result.Code = 500;
                result.Message = e.Message;
                return result;
            }
            return result;
        }

        /// <summary>
        /// 工步提取
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        [HttpGet]
        public TableData GetStepContent(string FilePath)
        {
            var result = new TableData();
            var xmlCpntent = XMLHelper.GetXDocument(FilePath).ToString();
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
            result.Data = step;
            return result;
        }

        /// <summary>
        /// 根据量程获取系数
        /// </summary>
        /// <param name="rangeCurrArray">多个量程英文逗号隔开</param>
        /// <returns></returns>
        [HttpGet]
        public TableData StepCoefficient(string rangeCurrArray)
        {
            var result = new TableData();
            var currArray = _app.MaxCurrent(rangeCurrArray);
            result.Data = _app.GetCurFactor(currArray);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public TableData sk()
        {
            var result = new TableData();
            result.Data = _app.WmsAccessToken();
            return result;
        }
        #endregion
    }
}
