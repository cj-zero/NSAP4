using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EdgeAPI;
using Grpc.Core;
using Grpc.Net.Client;
using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OpenAuth.App;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.WebApi.Model;
using Serilog;
using Response = Infrastructure.Response;

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// stepversion操作
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Nwcali")]
    public class StepVersionsController : ControllerBase
    {
        private readonly StepVersionApp _app;
        /// <summary>
        /// 通道控制
        /// </summary>
        /// <param name="arg"></param>
        /// <param name="cmd_type"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<string> ChannelControlCmdAsync([FromQuery] string cmd_type, string edge_guid, object arg)
        //public async Task<string> ChannelControlCmdAsync()
        {
            string result = "";
            var options = new List<ChannelOption> {
                new ChannelOption(ChannelOptions.MaxReceiveMessageLength,int.MaxValue)
            };

            var channel = new Grpc.Core.Channel($"{AppSetting.GrpcIP}:{AppSetting.GrpcPort}", ChannelCredentials.Insecure, options);
            DataService.DataServiceClient client = new DataService.DataServiceClient(channel);

            ChannelControlInput input = new ChannelControlInput() { edge_guid = edge_guid, control = new ChannelControlInput.ControlInput { arg = JsonConvert.SerializeObject(arg), cmd_type = cmd_type } };

            var json = JsonConvert.SerializeObject(input);

            EdgeAPI.Response res = null;

            res = client.ControlCmd(new Request { JsonParameter = Google.Protobuf.ByteString.CopyFromUtf8(json) });

            if (res != null && !res.Success)
            {
                Console.WriteLine("调用发生异常");
                //throw new KnownException(string.Format(_message.Show.异常_0_, res.Msg.ToStringUtf8()));
            }

            return result;// res.Msg.ToStringUtf8();
        }

        /// <summary>
        /// 是否存在中位机默认版本
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> IsExistsDefaultStepVersion()
        {
            var result = new TableData();
            try
            {
                result.Data = await _app.IsExistsDefaultStepVersion();
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{""}, 错误：{result.Message}");
            }

            return result;
        }

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

        //添加
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

        //修改
       [HttpPost]
        public Infrastructure.Response Update(AddOrUpdateStepVersionReq obj)
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
        public Task<TableData> Load([FromQuery]QueryStepversionListReq request)
        {
            return _app.Load(request);
        }
        
        /// <summary>
        /// 批量删除
        /// </summary>
        [HttpPost]
        public async Task<Infrastructure.Response> Delete([FromBody] List<int> ids)
        {
            var result = new Response();
            try
            {
                await _app.Delete(ids);

            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.InnerException?.Message ?? ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{ids.ToJson()}, 错误：{result.Message}");
            }

            return result;
        }

        public StepVersionsController(StepVersionApp app) 
        {
            _app = app;
        }
    }
}
