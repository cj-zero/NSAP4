using Infrastructure;
using Infrastructure.MQTT;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Nwcali
{
    /// <summary>
    /// 
    /// </summary>
    public class MqttSubscribeApp :IMqttSubscribe
    {
        private readonly IOptions<AppSetting> _appConfiguration;
        private readonly MqttNetClient _mqttClient;
        private readonly IUnitWork UnitWork;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="appConfiguration"></param>
        /// <param name="mqttClient"></param>
        public MqttSubscribeApp(IUnitWork unitWork, IOptions<AppSetting> appConfiguration, MqttNetClient mqttClient)
        {
            _appConfiguration = appConfiguration;
            _mqttClient = mqttClient;
            UnitWork = unitWork;
        }

        /// <summary>
        /// 订阅数据处理
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="payload"></param>
        public void SubscribeAsyncResult(string topic, byte[] payload)
        {
            string topics = topic.Split('/').FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(topics))
            {
                switch (topics)
                {
                    case "edge_msg":
                        var payloads = Encoding.UTF8.GetString(payload);
                        JObject jo = JObject.Parse(payloads);
                        string token = jo["token"].ToString();
                        string msg_type = jo["msg_type"].ToString();
                        string edge_guids = jo["edge_guid"].ToString();
                        try
                        {
                            //获取用户信息
                            var passPortUrl = _appConfiguration.Value.PassPortUrl;
                            HttpHelper helper = new HttpHelper(passPortUrl);
                            var passTokenStr = helper.Post(new
                            {
                                clientId = _appConfiguration.Value.PassPortClientId,
                                clientSecret = _appConfiguration.Value.PassPortClientSecret,
                                scope = _appConfiguration.Value.PassPortScope
                            }, (_appConfiguration.Value.PassPortUrl) + "api/Account/GetAccessToken", "", "");
                            JObject passTokenObj = JObject.Parse(passTokenStr);
                            string passToken = passTokenObj["data"].ToString();
                            string url = $"{_appConfiguration.Value.PassPortUrl}api/Login/GetLoginUserByToken?token={token}";
                            var userInfo = helper.Get(url, passToken);
                            if (string.IsNullOrWhiteSpace(userInfo))
                            {
                                Log.Logger.Error($"边缘计算登录账号无效 edge_guids:{edge_guids},token:{token}");
                                break;
                            }
                            JObject userObj = JObject.Parse(userInfo);
                            if (userObj["resultCode"].ToString()!="200")
                            {
                                //Log.Logger.Error($"边缘计算{userObj["errorMessage"]},edge_guids={edge_guids},token={token}");
                                break;
                            }
                            string userEnterpriseId = userObj["data"]["userProfile"]["enterpriseId"]==null?"" : userObj["data"]["userProfile"]["enterpriseId"].ToString();
                            if (string.IsNullOrWhiteSpace(userEnterpriseId))
                            {
                                //Log.Logger.Error($"边缘计算登录用户没有企业id,edge_guids={edge_guids},token={token}");
                                break;
                            }
                            var enterpriseId = _appConfiguration.Value.EnterpriseIds.Split(',').ToList();
                            if (enterpriseId.Contains(userEnterpriseId))
                            {
                                string edgeKey = "EdgeGuidKeys";
                                var edgeCache = RedisHelper.Get(edgeKey);
                                if (!string.IsNullOrWhiteSpace(edgeCache))
                                {
                                    var edgeList = edgeCache.Split(',').Distinct().ToList();
                                    if (!edgeList.Any(c=>c.Contains(edge_guids)))
                                    {
                                        edgeList.Add(edge_guids);
                                    }
                                    string key = string.Join(",", edgeList);
                                    RedisHelper.SetAsync(edgeKey, key);
                                }
                                else
                                {
                                    RedisHelper.SetAsync(edgeKey, edge_guids);
                                }
                                Log.Logger.Information($"设备数据开始解析 msg_type={msg_type},topics={topics},token={token},edge_guids={edge_guids}");
                                if (msg_type == "3")
                                {
                                    var obj = JsonConvert.DeserializeObject<EdgeData>(payloads);
                                    if (obj.chl_info.edge_info != null)
                                    {
                                        var edge_guid = obj.edge_guid;
                                        edge edge = new edge();
                                        edge.edge_guid = obj.chl_info.edge_guid;
                                        edge.edg_name = obj.chl_info.edge_info.edg_name == null ? "" : obj.chl_info.edge_info.edg_name;
                                        edge.address = obj.chl_info.edge_info.address == null ? "" : obj.chl_info.edge_info.address;
                                        edge.department = obj.chl_info.edge_info.edg_name == null ? "" : obj.chl_info.edge_info.edg_name;
                                        edge.status = 1;
                                        edge.CreateTime = DateTime.Now;
                                        List<edge_host> hostList = new List<edge_host>();
                                        List<edge_mid> midList = new List<edge_mid>();
                                        List<edge_low> lowList = new List<edge_low>();
                                        List<edge_channel> channelList = new List<edge_channel>();
                                        if (obj.chl_info.data != null)
                                        {
                                            foreach (var item in obj.chl_info.data)
                                            {
                                                var host = new edge_host();
                                                host.edge_guid = edge_guid;
                                                host.srv_guid = item.srv_guid;
                                                host.bts_server_version = item.bts_server_version;
                                                host.bts_server_ip = item.bts_server_ip;
                                                host.bts_type = item.bts_type;
                                                host.status = 1;
                                                host.CreateTime = DateTime.Now;
                                                hostList.Add(host);
                                                if (item.mid_list != null)
                                                {
                                                    foreach (var mItem in item.mid_list)
                                                    {
                                                        var mid = new edge_mid();
                                                        mid.edge_guid = edge_guid;
                                                        mid.srv_guid = item.srv_guid;
                                                        mid.mid_guid = mItem.mid_guid;
                                                        mid.dev_uid = mItem.dev_uid;
                                                        mid.mid_version = mItem.mid_version;
                                                        mid.production_serial = mItem.production_serial == null ? "" : mItem.production_serial;
                                                        mid.status = 1;
                                                        mid.CreateTime = DateTime.Now;
                                                        midList.Add(mid);
                                                        if (mItem.low_list != null)
                                                        {
                                                            foreach (var lItem in mItem.low_list)
                                                            {
                                                                if (string.IsNullOrWhiteSpace(lItem.low_guid))
                                                                {
                                                                    continue;
                                                                }
                                                                var low = new edge_low();
                                                                low.edge_guid = edge_guid;
                                                                low.srv_guid = item.srv_guid;
                                                                low.mid_guid = mItem.mid_guid;
                                                                low.low_guid = lItem.low_guid;
                                                                low.low_no = lItem.low_no;
                                                                low.unit_id = lItem.unit_id;
                                                                low.range_volt = lItem.range_volt.ToString();
                                                                var low_range_curr = lItem.range_curr_array.Select(x => Math.Abs(x)).Distinct().ToList();
                                                                low.range_curr_array = string.Join(",", low_range_curr);
                                                                low.low_version = lItem.low_version;
                                                                low.status = 1;
                                                                low.CreateTime = DateTime.Now;
                                                                low.dev_uid = mItem.dev_uid;
                                                                lowList.Add(low);
                                                                if (lItem.channel_list != null)
                                                                {
                                                                    foreach (var cItem in lItem.channel_list)
                                                                    {
                                                                        var channel = new edge_channel();
                                                                        channel.edge_guid = edge_guid;
                                                                        channel.srv_guid = item.srv_guid;
                                                                        channel.mid_guid = mItem.mid_guid;
                                                                        channel.low_guid = lItem.low_guid;
                                                                        channel.bts_id = cItem.Value;
                                                                        channel.status = 1;
                                                                        channel.CreateTime = DateTime.Now;
                                                                        channel.low_no = lItem.low_no;
                                                                        channel.unit_id = lItem.unit_id;
                                                                        channel.dev_uid = mItem.dev_uid;
                                                                        channel.TestId = 0;
                                                                        channel.rt_status = -1;
                                                                        channel.bts_server_ip = item.bts_server_ip;
                                                                        channelList.Add(channel);
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        using (var dbContext = UnitWork.GetDbContext<edge>())
                                        {
                                            using (var transaction = dbContext.Database.BeginTransaction())
                                            {
                                                try
                                                {
                                                    Stopwatch st = new Stopwatch();
                                                    st.Start();
                                                    UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                                    UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                                    UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                                    UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                                    UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                                    UnitWork.Add<edge, int>(edge);
                                                    UnitWork.BatchAdd<edge_host, int>(hostList.ToArray());
                                                    UnitWork.BatchAdd<edge_mid, int>(midList.ToArray());
                                                    UnitWork.BatchAdd<edge_low, int>(lowList.ToArray());
                                                    UnitWork.BatchAdd<edge_channel, int>(channelList.ToArray());
                                                    UnitWork.Save();
                                                    transaction.Commit();
                                                    st.Stop();
                                                    Log.Logger.Information($"在线设备更新成功 耗时:{st.ElapsedMilliseconds}ms");
                                                }
                                                catch (Exception ex)
                                                {
                                                    transaction.Rollback();
                                                    Log.Logger.Error($"设备数据更新异常：msg_type={msg_type},edge_guids={edge_guids},topics={topic},token={token},message={ex.Message}");
                                                }
                                            }
                                        }
                                    }
                                }
                                else if (msg_type == "2")
                                {
                                    var edgeinfo = UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids).Any();
                                    if (edgeinfo)
                                    {
                                        UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge { status = 0 });
                                        UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge_host { status = 0 });
                                        UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge_mid { status = 0 });
                                        UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge_low { status = 0 });
                                        UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge_channel { status = 0 });
                                    }
                                }
                                else if (msg_type == "1")
                                {
                                    var edgeinfo = UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids).Any();
                                    if (edgeinfo)
                                    {
                                        UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge { status = 1 });
                                        UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge_host { status = 1 });
                                        UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge_mid { status = 1 });
                                        UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge_low { status = 1 });
                                        UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == edge_guids)
                                            .UpdateFromQuery(c => new edge_channel { status = 1 });
                                    }
                                    _ = _mqttClient.SubscribeAsync($"rt_data/subscribe_{edge_guids}");
                                    Log.Logger.Information($"边缘计算{edge_guids}上线 rt订阅成功!");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error($"设备订阅原始数据异常：msg_type={msg_type},edge_guids={edge_guids},topics={topic},token={token},message={ ex.Message}");
                        }
                        break;
                    case "rt_data":
                        EdgeCmd.VecRT vc = null;
                        MemoryStream stream = new MemoryStream(payload);
                        using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
                        {
                            using (MemoryStream ms = new MemoryStream())
                            {
                                gZipStream.CopyTo(ms);
                                vc = EdgeCmd.VecRT.Parser.ParseFrom(ms.ToArray());
                            }
                        }
                        if (vc != null)
                        {
                            var edge_guid = Encoding.UTF8.GetString(vc.EdgeGuid.ToArray());
                            try
                            {
                                if (vc.SrvRt != null && !string.IsNullOrWhiteSpace(edge_guid))
                                {
                                    int count = 1;
                                    edge edgeInfo = UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guid).FirstOrDefault();
                                    if (edgeInfo != null)
                                    {
                                        for (int i = 0; i < count; i++)
                                        {
                                            foreach (var srv_rt in vc.SrvRt)
                                            {
                                                string ip = Encoding.UTF8.GetString(srv_rt.Ip.ToArray());
                                                string srv_guid = Encoding.UTF8.GetString(srv_rt.SrvGuid.ToArray());
                                                if (srv_rt.MidRt != null)
                                                {
                                                    foreach (var mid_rt in srv_rt.MidRt)
                                                    {
                                                        string mid_guid = Encoding.UTF8.GetString(mid_rt.MidGuid.ToArray());
                                                        if (mid_rt.VecRt != null)
                                                        {
                                                            foreach (var item in mid_rt.VecRt)
                                                            {
                                                                var deviceTestLog = UnitWork.Find<DeviceTestLog>(null).Where(c => c.EdgeGuid == edge_guid && c.SrvGuid == srv_guid && c.DevUid == mid_rt.DevUid && c.UnitId == item.UnitId && c.ChlId == item.ChlId && c.TestId == item.TestId).FirstOrDefault();
                                                                if (deviceTestLog != null)
                                                                {
                                                                    deviceTestLog.Status = CheckWorkType(item.WorkType);
                                                                    deviceTestLog.ChangeStatusTime = DateTime.Now;
                                                                    deviceTestLog.StepId = (int)item.StepId;
                                                                    string v = "0x" + item.PrtCode.ToString("X8");
                                                                    deviceTestLog.CodeTxt = UnitWork.Find<DeviceTestCode>(null).Where(c => c.PrtCode == v).Select(c => c.CodeTxt).FirstOrDefault();
                                                                    deviceTestLog.PrtCode = item.PrtCode.ToString();
                                                                    UnitWork.Update(deviceTestLog);
                                                                    UnitWork.Save();
                                                                }
                                                                try
                                                                {
                                                                    UnitWork.Update<edge_channel>(c => c.edge_guid == edge_guid && c.srv_guid == srv_guid && c.mid_guid == mid_guid && c.dev_uid == mid_rt.DevUid && c.unit_id == item.UnitId && c.bts_id == item.ChlId, m => new edge_channel
                                                                    {
                                                                        TestId = item.TestId,
                                                                        rt_status = CheckWorkType(item.WorkType)
                                                                    });
                                                                    UnitWork.Save();
                                                                }
                                                                catch (Exception ex)
                                                                {
                                                                    Log.Logger.Error($"通道状态更新异常：edge_guids={edge_guid},{ex.Message}");
                                                                    continue;
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Logger.Error($"rt订阅数据解析异常：edge_guids={edge_guid},{ex.Message}");
                            }
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// 设备变更订阅
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public bool SubscribeEdgeMsg(byte[] payload)
        {
            var payloads = Encoding.UTF8.GetString(payload);
            JObject jo = JObject.Parse(payloads);
            string token = jo["token"].ToString();
            string msg_type = jo["msg_type"].ToString();
            string edge_guids = jo["edge_guid"].ToString();
            try
            {
                //获取用户信息
                var passPortUrl = _appConfiguration.Value.PassPortUrl;
                HttpHelper helper = new HttpHelper(passPortUrl);
                var passTokenStr = helper.Post(new
                {
                    clientId = _appConfiguration.Value.PassPortClientId,
                    clientSecret = _appConfiguration.Value.PassPortClientSecret,
                    scope = _appConfiguration.Value.PassPortScope
                }, (_appConfiguration.Value.PassPortUrl) + "api/Account/GetAccessToken", "", "");
                JObject passTokenObj = JObject.Parse(passTokenStr);
                string passToken = passTokenObj["data"].ToString();
                string url = $"{_appConfiguration.Value.PassPortUrl}api/Login/GetLoginUserByToken?token={token}";
                var userInfo = helper.Get(url, passToken);
                if (string.IsNullOrWhiteSpace(userInfo))
                {
                    Log.Logger.Error($"边缘计算登录账号无效 edge_guids:{edge_guids},token:{token}");
                    return false;
                }
                JObject userObj = JObject.Parse(userInfo);
                if (userObj["resultCode"].ToString() != "200")
                {
                    //Log.Logger.Error($"边缘计算{userObj["errorMessage"]},edge_guids={edge_guids},token={token}");
                    return false;
                }
                string userEnterpriseId = userObj["data"]["userProfile"]["enterpriseId"] == null ? "" : userObj["data"]["userProfile"]["enterpriseId"].ToString();
                if (string.IsNullOrWhiteSpace(userEnterpriseId))
                {
                    //Log.Logger.Error($"边缘计算登录用户没有企业id,edge_guids={edge_guids},token={token}");
                    return false;
                }
                var enterpriseId = _appConfiguration.Value.EnterpriseIds.Split(',').ToList();
                if (enterpriseId.Contains(userEnterpriseId))
                {
                    var edgeCache = RedisHelper.Get(_mqttClient.clientId);
                    if (!string.IsNullOrWhiteSpace(edgeCache))
                    {
                        var edgeList = edgeCache.Split(',').Distinct().ToList();
                        if (!edgeList.Any(c => c.Contains(edge_guids)))
                        {
                            edgeList.Add(edge_guids);
                        }
                        string key = string.Join(",", edgeList);
                        RedisHelper.SetAsync(_mqttClient.clientId, key);
                    }
                    else
                    {
                        RedisHelper.SetAsync(_mqttClient.clientId, edge_guids);
                    }
                    Log.Logger.Information($"设备数据开始解析 msg_type={msg_type},token={token},edge_guids={edge_guids}");
                    if (msg_type == "3")
                    {
                        var obj = JsonConvert.DeserializeObject<EdgeData>(payloads);
                        if (obj.chl_info.edge_info != null)
                        {
                            var edge_guid = obj.edge_guid;
                            edge edge = new edge();
                            edge.edge_guid = obj.chl_info.edge_guid;
                            edge.edg_name = obj.chl_info.edge_info.edg_name == null ? "" : obj.chl_info.edge_info.edg_name;
                            edge.address = obj.chl_info.edge_info.address == null ? "" : obj.chl_info.edge_info.address;
                            edge.department = obj.chl_info.edge_info.edg_name == null ? "" : obj.chl_info.edge_info.edg_name;
                            edge.status = 1;
                            edge.CreateTime = DateTime.Now;
                            List<edge_host> hostList = new List<edge_host>();
                            List<edge_mid> midList = new List<edge_mid>();
                            List<edge_low> lowList = new List<edge_low>();
                            List<edge_channel> channelList = new List<edge_channel>();
                            if (obj.chl_info.data != null)
                            {
                                foreach (var item in obj.chl_info.data)
                                {
                                    var host = new edge_host();
                                    host.edge_guid = edge_guid;
                                    host.srv_guid = item.srv_guid;
                                    host.bts_server_version = item.bts_server_version;
                                    host.bts_server_ip = item.bts_server_ip;
                                    host.bts_type = item.bts_type;
                                    host.status = 1;
                                    host.CreateTime = DateTime.Now;
                                    hostList.Add(host);
                                    if (item.mid_list != null)
                                    {
                                        foreach (var mItem in item.mid_list)
                                        {
                                            var mid = new edge_mid();
                                            mid.edge_guid = edge_guid;
                                            mid.srv_guid = item.srv_guid;
                                            mid.mid_guid = mItem.mid_guid;
                                            mid.dev_uid = mItem.dev_uid;
                                            mid.mid_version = mItem.mid_version;
                                            mid.production_serial = mItem.production_serial == null ? "" : mItem.production_serial;
                                            mid.status = 1;
                                            mid.CreateTime = DateTime.Now;
                                            midList.Add(mid);
                                            if (mItem.low_list != null)
                                            {
                                                foreach (var lItem in mItem.low_list)
                                                {
                                                    if (string.IsNullOrWhiteSpace(lItem.low_guid))
                                                    {
                                                        continue;
                                                    }
                                                    var low = new edge_low();
                                                    low.edge_guid = edge_guid;
                                                    low.srv_guid = item.srv_guid;
                                                    low.mid_guid = mItem.mid_guid;
                                                    low.low_guid = lItem.low_guid;
                                                    low.low_no = lItem.low_no;
                                                    low.unit_id = lItem.unit_id;
                                                    low.range_volt = lItem.range_volt.ToString();
                                                    var low_range_curr = lItem.range_curr_array.Select(x => Math.Abs(x)).Distinct().ToList();
                                                    low.range_curr_array = string.Join(",", low_range_curr);
                                                    low.low_version = lItem.low_version;
                                                    low.status = 1;
                                                    low.CreateTime = DateTime.Now;
                                                    low.dev_uid = mItem.dev_uid;
                                                    lowList.Add(low);
                                                    if (lItem.channel_list != null)
                                                    {
                                                        foreach (var cItem in lItem.channel_list)
                                                        {
                                                            var channel = new edge_channel();
                                                            channel.edge_guid = edge_guid;
                                                            channel.srv_guid = item.srv_guid;
                                                            channel.mid_guid = mItem.mid_guid;
                                                            channel.low_guid = lItem.low_guid;
                                                            channel.bts_id = cItem.Value;
                                                            channel.status = 1;
                                                            channel.CreateTime = DateTime.Now;
                                                            channel.low_no = lItem.low_no;
                                                            channel.unit_id = lItem.unit_id;
                                                            channel.dev_uid = mItem.dev_uid;
                                                            channel.TestId = 0;
                                                            channel.rt_status = -1;
                                                            channel.bts_server_ip = item.bts_server_ip;
                                                            channelList.Add(channel);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            using (var dbContext = UnitWork.GetDbContext<edge>())
                            {
                                using (var transaction = dbContext.Database.BeginTransaction())
                                {
                                    try
                                    {
                                        Stopwatch st = new Stopwatch();
                                        st.Start();
                                        UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                        UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                        UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                        UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                        UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == edge_guids).DeleteFromQuery();
                                        UnitWork.Add<edge, int>(edge);
                                        UnitWork.BatchAdd<edge_host, int>(hostList.ToArray());
                                        UnitWork.BatchAdd<edge_mid, int>(midList.ToArray());
                                        UnitWork.BatchAdd<edge_low, int>(lowList.ToArray());
                                        UnitWork.BatchAdd<edge_channel, int>(channelList.ToArray());
                                        UnitWork.Save();
                                        transaction.Commit();
                                        st.Stop();
                                        Log.Logger.Information($"在线设备更新成功 耗时:{st.ElapsedMilliseconds}ms");
                                    }
                                    catch (Exception ex)
                                    {
                                        transaction.Rollback();
                                        Log.Logger.Error($"设备数据更新异常：msg_type={msg_type},edge_guids={edge_guids},token={token},message={ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                    else if (msg_type == "2")
                    {
                        var edgeinfo = UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids).Any();
                        if (edgeinfo)
                        {
                            UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge { status = 0 });
                            UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge_host { status = 0 });
                            UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge_mid { status = 0 });
                            UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge_low { status = 0 });
                            UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge_channel { status = 0 });
                        }
                    }
                    else if (msg_type == "1")
                    {
                        var edgeinfo = UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids).Any();
                        if (edgeinfo)
                        {
                            UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge { status = 1 });
                            UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge_host { status = 1 });
                            UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge_mid { status = 1 });
                            UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge_low { status = 1 });
                            UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == edge_guids)
                                .UpdateFromQuery(c => new edge_channel { status = 1 });
                        }
                        _ = _mqttClient.SubscribeAsync($"rt_data/subscribe_{edge_guids}");
                        Log.Logger.Information($"边缘计算{edge_guids}上线 rt订阅成功!");
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"设备订阅原始数据异常：msg_type={msg_type},edge_guids={edge_guids},token={token},message={ ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// rt数据订阅
        /// </summary>
        /// <param name="payload"></param>
        /// <returns></returns>
        public bool SubscribeRtData(byte[] payload)
        {
            EdgeCmd.VecRT vc = null;
            MemoryStream stream = new MemoryStream(payload);
            using (GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress))
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    gZipStream.CopyTo(ms);
                    vc = EdgeCmd.VecRT.Parser.ParseFrom(ms.ToArray());
                }
            }
            if (vc != null)
            {
                var edge_guid = Encoding.UTF8.GetString(vc.EdgeGuid.ToArray());
                try
                {
                    if (vc.SrvRt != null && !string.IsNullOrWhiteSpace(edge_guid))
                    {
                        int count = 1;
                        edge edgeInfo = UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guid).FirstOrDefault();
                        if (edgeInfo != null)
                        {
                            for (int i = 0; i < count; i++)
                            {
                                foreach (var srv_rt in vc.SrvRt)
                                {
                                    string ip = Encoding.UTF8.GetString(srv_rt.Ip.ToArray());
                                    string srv_guid = Encoding.UTF8.GetString(srv_rt.SrvGuid.ToArray());
                                    if (srv_rt.MidRt != null)
                                    {
                                        foreach (var mid_rt in srv_rt.MidRt)
                                        {
                                            string mid_guid = Encoding.UTF8.GetString(mid_rt.MidGuid.ToArray());
                                            if (mid_rt.VecRt != null)
                                            {
                                                foreach (var item in mid_rt.VecRt)
                                                {
                                                    var deviceTestLog = UnitWork.Find<DeviceTestLog>(null).Where(c => c.EdgeGuid == edge_guid && c.SrvGuid == srv_guid && c.DevUid == mid_rt.DevUid && c.UnitId == item.UnitId && c.ChlId == item.ChlId && c.TestId == item.TestId).FirstOrDefault();
                                                    if (deviceTestLog != null)
                                                    {
                                                        deviceTestLog.Status = CheckWorkType(item.WorkType);
                                                        deviceTestLog.ChangeStatusTime = DateTime.Now;
                                                        deviceTestLog.StepId = (int)item.StepId;
                                                        string v = "0x" + item.PrtCode.ToString("X8");
                                                        deviceTestLog.CodeTxt = UnitWork.Find<DeviceTestCode>(null).Where(c => c.PrtCode == v).Select(c => c.CodeTxt).FirstOrDefault();
                                                        deviceTestLog.PrtCode = item.PrtCode.ToString();
                                                        UnitWork.Update(deviceTestLog);
                                                        UnitWork.Save();
                                                    }
                                                    try
                                                    {
                                                        UnitWork.Update<edge_channel>(c => c.edge_guid == edge_guid && c.srv_guid == srv_guid && c.mid_guid == mid_guid && c.dev_uid == mid_rt.DevUid && c.unit_id == item.UnitId && c.bts_id == item.ChlId, m => new edge_channel
                                                        {
                                                            TestId = item.TestId,
                                                            rt_status = CheckWorkType(item.WorkType)
                                                        });
                                                        UnitWork.Save();
                                                    }
                                                    catch (Exception ex)
                                                    {
                                                        Log.Logger.Error($"通道状态更新异常：edge_guids={edge_guid},{ex.Message}");
                                                        continue;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"rt订阅数据解析异常：edge_guids={edge_guid},{ex.Message}");
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// -1:测试完成  -2:测试进行中 -3:用户停止 -4:保护 1-暂停测试 2-离线,3-离线暂停 4-蜂鸣器报警 5-同步控制 6-占用下位机 7-点灯 8-抽真空 9-泄真空 10-测漏率
        /// 小于255  低位向高位数 从bit0开始
        /// bit5 bit4
        ///  0    0   测试完成
        ///  0    1   测试进行中
        ///  1    0   用户停止
        ///  1    1   保护
        /// 大于255 高位向低位右移八位 转为二进制
        /// 1  暂停测试
        /// 2  离线
        /// 3  离线暂停
        /// 4  蜂鸣器报警
        /// 5  同步控制
        /// 6  占用下位机
        /// 7  点灯
        /// 8  抽真空
        /// 9  泄真空
        /// 10 测漏率
        /// </summary>
        /// <param name="WorkType"></param>
        /// <returns></returns>
        public int CheckWorkType(uint WorkType)
        {
            string res = Convert.ToString(WorkType, 2).PadLeft(16, '0');
            char[] arr = res.ToCharArray();
            if (WorkType <= 255)
            {
                int bit4 = 0;
                int bit5 = 1;
                bit4 = Convert.ToInt32(arr.Reverse().ToArray()[4].ToString());
                bit5 = Convert.ToInt32(arr.Reverse().ToArray()[5].ToString());
                if (bit4 == 0 && bit5 == 0)
                {
                    return -1;
                }
                else if (bit4 == 1 && bit5 == 0)
                {
                    return -2;
                }
                else if (bit4 == 0 && bit5 == 1)
                {
                    return -3;
                }
                else
                {
                    return -4;
                }
            }
            else
            {
                string m = res.Substring(0, 8);
                int k = Convert.ToInt32(m, 2);
                return k;
            }
        }
    }
}
