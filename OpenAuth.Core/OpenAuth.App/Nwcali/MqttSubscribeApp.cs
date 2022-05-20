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
    public class MqttSubscribeApp : OnlyUnitWorkBaeApp, IMqttSubscribe
    {
        private readonly IOptions<AppSetting> _appConfiguration;
        //private HttpHelper _helper;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="appConfiguration"></param>
        public MqttSubscribeApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration) : base(unitWork, auth)
        {
            //_helper = httpHelper;
            _appConfiguration = appConfiguration;
        }

        /// <summary>
        /// 
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
                                Log.Logger.Error($"MQTT 订阅异常 主题:{topic},报文:{payloads},token:{token}");
                                break;
                            }
                            JObject userObj = JObject.Parse(userInfo);
                            string userEnterpriseId = userObj["data"]["userProfile"]["enterpriseId"].ToString();
                            var enterpriseId = _appConfiguration.Value.EnterpriseIds.Split(',').ToList();
                            if (enterpriseId.Contains(userEnterpriseId))
                            {
                                if (msg_type == "3")
                                {
                                    var obj = JsonConvert.DeserializeObject<EdgeData>(payloads);
                                    var dbContext = UnitWork.GetDbContext<edge>();
                                    var flage = false;
                                    using (var transaction = dbContext.Database.BeginTransaction())
                                    {
                                        try
                                        {
                                            var edge_guid = obj.edge_guid;
                                            edge edge = new edge();
                                            var edgeInfo = UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guid).FirstOrDefault();
                                            if (edgeInfo != null)
                                            {
                                                edgeInfo.edg_name = obj.chl_info.edge_info.edge_name;
                                                edgeInfo.address = obj.chl_info.edge_info.address;
                                                edgeInfo.department = obj.chl_info.edge_info.edge_name;
                                                edgeInfo.status = 1;
                                            }
                                            else
                                            {
                                                if (obj.chl_info.edge_info != null)
                                                {
                                                    edge.edge_guid = obj.chl_info.edge_guid;
                                                    edge.edg_name = obj.chl_info.edge_info.edge_name == null ? "" : obj.chl_info.edge_info.edge_name;
                                                    edge.address = obj.chl_info.edge_info.address == null ? "" : obj.chl_info.edge_info.address;
                                                    edge.department = obj.chl_info.edge_info.edge_name == null ? "" : obj.chl_info.edge_info.edge_name;
                                                    edge.status = 1;
                                                    UnitWork.Add<edge, int>(edge);
                                                }
                                            }
                                            if (obj.chl_info.data!=null)
                                            {
                                                foreach (var item in obj.chl_info.data)
                                                {
                                                    var host = new edge_host();
                                                    var host_info = UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == edge_guid && c.srv_guid == item.srv_guid).FirstOrDefault();
                                                    if (host_info != null)
                                                    {
                                                        host_info.bts_server_version = item.bts_server_version;
                                                        host_info.bts_server_ip = item.bts_server_ip;
                                                        host_info.bts_type = item.bts_type;
                                                        host_info.status = 1;
                                                    }
                                                    else
                                                    {
                                                        host.edge_guid = edge_guid;
                                                        host.srv_guid = item.srv_guid;
                                                        host.bts_server_version = item.bts_server_version;
                                                        host.bts_server_ip = item.bts_server_ip;
                                                        host.bts_type = item.bts_type;
                                                        host.status = 1;
                                                        UnitWork.Add<edge_host, int>(host);
                                                    }
                                                    if (item.mid_list!=null)
                                                    {
                                                        foreach (var mItem in item.mid_list)
                                                        {
                                                            var mid = new edge_mid();
                                                            var mid_info = UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == edge_guid && c.srv_guid == item.srv_guid && c.mid_guid == mItem.mid_guid).FirstOrDefault();
                                                            if (mid_info != null)
                                                            {
                                                                mid_info.dev_uid = mItem.dev_uid;
                                                                mid_info.mid_version = mItem.mid_version;
                                                                mid_info.production_serial = mid_info.production_serial;
                                                                mid_info.status = 1;
                                                            }
                                                            else
                                                            {
                                                                mid.edge_guid = edge_guid;
                                                                mid.srv_guid = item.srv_guid;
                                                                mid.mid_guid = mItem.mid_guid;
                                                                mid.dev_uid = mItem.dev_uid;
                                                                mid.mid_version = mItem.mid_version;
                                                                mid.production_serial = mItem.production_serial;//==null?"": mItem.production_serial;
                                                                mid.status = 1;
                                                                UnitWork.Add<edge_mid, int>(mid);
                                                            }
                                                            if (mItem.low_list!=null)
                                                            {
                                                                foreach (var lItem in mItem.low_list)
                                                                {
                                                                    var low = new edge_low();
                                                                    var low_info = UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == edge_guid && c.srv_guid == item.srv_guid && c.mid_guid == mItem.mid_guid && c.low_guid == lItem.low_guid).FirstOrDefault();
                                                                    if (low_info != null)
                                                                    {
                                                                        low_info.low_no = lItem.low_no;
                                                                        low_info.unit_id = lItem.unit_id;
                                                                        low_info.range_volt = lItem.range_volt.ToString();
                                                                        low_info.range_curr_array = string.Join(",", lItem.range_curr_array);
                                                                        low_info.low_version = lItem.low_version;
                                                                        low_info.status = 1;
                                                                    }
                                                                    else
                                                                    {
                                                                        low.edge_guid = edge_guid;
                                                                        low.srv_guid = item.srv_guid;
                                                                        low.mid_guid = mItem.mid_guid;
                                                                        low.low_guid = lItem.low_guid;
                                                                        low.low_no = lItem.low_no;
                                                                        low.unit_id = lItem.unit_id;
                                                                        low.range_volt = lItem.range_volt.ToString();
                                                                        low.range_curr_array = string.Join(",", lItem.range_curr_array);
                                                                        low.low_version = lItem.low_version;
                                                                        low.status = 1;
                                                                        UnitWork.Add<edge_low, int>(low);
                                                                    }
                                                                    if (lItem.channel_list!=null)
                                                                    {
                                                                        foreach (var cItem in lItem.channel_list)
                                                                        {
                                                                            var channel = new edge_channel();
                                                                            var channel_info = UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == edge_guid && c.srv_guid == item.srv_guid && c.mid_guid == mItem.mid_guid && c.low_guid == lItem.low_guid && c.bts_id == cItem).FirstOrDefault();
                                                                            if (channel_info != null)
                                                                            {
                                                                                channel_info.status = 1;
                                                                            }
                                                                            else
                                                                            {
                                                                                channel.edge_guid = edge_guid;
                                                                                channel.srv_guid = item.srv_guid;
                                                                                channel.mid_guid = mItem.mid_guid;
                                                                                channel.low_guid = lItem.low_guid;
                                                                                channel.bts_id = cItem.Value;
                                                                                channel.status = 1;
                                                                                UnitWork.Add<edge_channel, int>(channel);
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            UnitWork.Save();
                                            transaction.Commit();
                                            flage = true;
                                        }
                                        catch (Exception ex)
                                        {
                                            Log.Logger.Error($"设备数据订阅结果解析失败 msg_type={msg_type},topics={topics},token={token},edge_guids={edge_guids}",ex);
                                            transaction.Rollback();
                                        }
                                    }
                                    if (flage)
                                    {
                                        UnitWork.Find<edge>(null).Where(c => c.edge_guid == edge_guids && c.status == 0).DeleteFromQuery();
                                        UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == edge_guids && c.status == 0).DeleteFromQuery();
                                        UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == edge_guids && c.status == 0).DeleteFromQuery();
                                        UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == edge_guids && c.status == 0).DeleteFromQuery();
                                        UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == edge_guids && c.status == 0).DeleteFromQuery();
                                    }
                                }
                                else if (msg_type == "2")
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
                                else if (msg_type == "1")
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
                            }
                        }
                        catch (Exception ex)
                        {
                            Log.Logger.Error($"设备数据订阅异常：msg_type={msg_type},edge_guids={edge_guids},topics={topic},token={token}", ex);
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
                                                                    deviceTestLog.Status = chaeckWorkType(item.WorkType);
                                                                    deviceTestLog.ChangeStatusTime = DateTime.Now;
                                                                    deviceTestLog.StepId = (int)item.StepId;
                                                                    string v = "0x" + item.PrtCode.ToString("X8");
                                                                    deviceTestLog.CodeTxt = UnitWork.Find<DeviceTestCode>(null).Where(c => c.PrtCode == v).Select(c => c.CodeTxt).FirstOrDefault();
                                                                    deviceTestLog.PrtCode = item.PrtCode.ToString();
                                                                    UnitWork.Update(deviceTestLog);
                                                                    UnitWork.Save();
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
                                Log.Logger.Error($"rt订阅数据解析异常：edge_guids={edge_guid},异常数据 vc="+JsonConvert.SerializeObject(vc) +"", ex);
                            }
                        }
                        break;
                }
            }
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
        public int chaeckWorkType(uint WorkType)
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

        public async Task<string> GetCodeText(string code)
        {
            return await UnitWork.Find<DeviceTestCode>(null).Where(c => c.PrtCode == code).Select(c => c.CodeTxt).FirstOrDefaultAsync();
        }
    }
}
