using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.MQTT;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Nwcali.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class DevInfoApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        private readonly MqttNetClient _mqttNetClient;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        /// <param name="mqttNetClient"></param>
        public DevInfoApp(IUnitWork unitWork, IAuth auth, MqttNetClient mqttNetClient) : base(unitWork, auth)
        {
            _mqttNetClient = mqttNetClient;
        }

        /// <summary>
        /// 订阅消息
        /// </summary>
        /// <param name="Topic"></param>
        /// <returns></returns>
        public async Task<TableData> ReceiveMessage(string Topic)
        {
            var result = new TableData();
            result.Data = await _mqttNetClient.SubscribeAsync(Topic);
            return result;
        }


        /// <summary>
        /// 边缘计算在线未绑定/已绑定未测试设备列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> OnlineDeviceList(int page, int limit)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var departmentList = loginContext.Orgs.Select(c => c.Name).ToList();
            var onlineList = await (from a in UnitWork.Find<edge>(null)
                                    join b in UnitWork.Find<edge_host>(null) on a.edge_guid equals b.edge_guid
                                    join c in UnitWork.Find<edge_mid>(null) on new { b.edge_guid, b.srv_guid } equals new { c.edge_guid, c.srv_guid }
                                    join d in UnitWork.Find<edge_low>(null) on new { c.edge_guid, c.srv_guid, c.mid_guid } equals new { d.edge_guid, d.srv_guid, d.mid_guid }
                                    where departmentList.Contains(a.department)
                                    && a.status==1 && b.status==1 && c.status==1 && d.status==1
                                    select new { d.edge_guid, d.srv_guid, d.mid_guid, d.low_guid, b.bts_server_ip, c.dev_uid, d.unit_id, a.department, edge_status = a.status, host_status = b.status, mid_status = c.status, low_status = d.status,d.low_no }).ToListAsync();
            var lowGuidList = onlineList.Select(c => c.low_guid).ToList();
            var midGuidList = onlineList.Select(c => c.mid_guid).ToList();
            var bindGuidList = await UnitWork.Find<DeviceBindMap>(null).Where(c => lowGuidList.Contains(c.Guid) || midGuidList.Contains(c.Guid)).Select(c => new { c.EdgeGuid, c.SrvGuid, c.Guid, c.GeneratorCode, c.LowGuid, c.BindType,c.UnitId}).ToListAsync();
            var hasTestLowList = await UnitWork.Find<DeviceTestLog>(null).Where(c => lowGuidList.Contains(c.LowGuid)).ToListAsync();
            var hasTestMidList = await UnitWork.Find<DeviceTestLog>(null).Where(c => midGuidList.Contains(c.MidGuid)).ToListAsync();
            var host_list = onlineList.Select(c => new { c.edge_guid, c.srv_guid, c.bts_server_ip }).Distinct().Skip((page - 1) * limit).Take(limit).ToList();
            int total = onlineList.Select(c => new { c.edge_guid, c.srv_guid, c.bts_server_ip }).Distinct().Count();
            List<OnlineDeviceResp> list = new List<OnlineDeviceResp>();
            foreach (var item in host_list)
            {
                OnlineDeviceResp onlineDeviceResp = new OnlineDeviceResp();
                onlineDeviceResp.edge_guid = item.edge_guid;
                onlineDeviceResp.srv_guid = item.srv_guid;
                onlineDeviceResp.bts_server_ip = item.bts_server_ip;
                onlineDeviceResp.status = onlineList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid).FirstOrDefault()?.host_status;
                onlineDeviceResp.mid_Lists = new List<mid_list>();
                var mid_lists = onlineList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid).OrderBy(c=>c.dev_uid).Select(c => new { c.mid_guid, c.dev_uid }).Distinct().ToList();
                foreach (var mitem in mid_lists)
                {
                    mid_list ml = new mid_list();
                    ml.has_bind = bindGuidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.Guid == mitem.mid_guid).Any();
                    if (ml.has_bind)
                    {
                        string code = bindGuidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.Guid == mitem.mid_guid).Select(c => c.GeneratorCode).FirstOrDefault();
                        ml.has_test = hasTestMidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.MidGuid == mitem.mid_guid && c.GeneratorCode== code).Any();
                    }
                    else
                    {
                        ml.has_test = hasTestMidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.MidGuid == mitem.mid_guid).Any();
                    }
                    if (ml.has_test && ml.has_bind)
                        continue;
                    ml.dev_uid = mitem.dev_uid.Value;
                    ml.mid_guid = mitem.mid_guid;
                    ml.status = onlineList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid && c.dev_uid == mitem.dev_uid && c.mid_guid == mitem.mid_guid).FirstOrDefault()?.host_status.Value;
                    ml.GeneratorCode = bindGuidList.Where(c => c.Guid == mitem.mid_guid && c.BindType == 1).Select(c => c.GeneratorCode).Distinct().FirstOrDefault(); ;
                    ml.low_Lists = new List<low_list>();
                    var low_Lists = onlineList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid && c.dev_uid == mitem.dev_uid && c.mid_guid == mitem.mid_guid).OrderBy(c=>c.low_no).Select(c => new low_list { unit_id = c.unit_id.Value, status = c.low_status.Value, low_guid = c.low_guid,low_no=c.low_no }).Distinct().ToList();
                    foreach (var litem in low_Lists)
                    {
                        low_list low_List = new low_list();
                        low_List.low_guid = litem.low_guid;
                        low_List.has_bind = bindGuidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.Guid == mitem.mid_guid && c.LowGuid == litem.low_guid).Any();
                        if (low_List.has_bind)
                        {
                            string code = bindGuidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.Guid == mitem.mid_guid).Select(c => c.GeneratorCode).FirstOrDefault();
                            low_List.has_test = hasTestLowList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.MidGuid == mitem.mid_guid && c.LowGuid == litem.low_guid && c.GeneratorCode==code).Any();
                        }
                        else
                        {
                            low_List.has_test = hasTestLowList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.MidGuid == mitem.mid_guid && c.LowGuid == litem.low_guid).Any();
                        }
                        if (low_List.has_test && low_List.has_bind)
                            continue;
                        low_List.status = litem.status;
                        low_List.unit_id = litem.low_no.Value;
                        low_List.GeneratorCode = bindGuidList.Where(c => c.LowGuid == litem.low_guid).Select(c => c.GeneratorCode).FirstOrDefault();
                        ml.low_Lists.Add(low_List);
                    }
                    onlineDeviceResp.mid_Lists.Add(ml);
                }
                if (!onlineDeviceResp.mid_Lists.Any())
                {
                    continue;
                }
                list.Add(onlineDeviceResp);
            }
            result.Data = list;
            result.Count = total;
            return result;
        }


        /// <summary>
        /// 边缘计算在线已绑定设备列表
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> OnlineDeviceBindList(int page, int limit)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var departmentList = loginContext.Orgs.Select(c => c.Name).ToList();
            var onlineList = await (from a in UnitWork.Find<edge>(null)
                                    join b in UnitWork.Find<edge_host>(null) on a.edge_guid equals b.edge_guid
                                    join c in UnitWork.Find<edge_mid>(null) on new { b.edge_guid, b.srv_guid } equals new { c.edge_guid, c.srv_guid }
                                    join d in UnitWork.Find<edge_low>(null) on new { c.edge_guid, c.srv_guid, c.mid_guid } equals new { d.edge_guid, d.srv_guid, d.mid_guid }
                                    where departmentList.Contains(a.department)
                                    //&& a.status==1 && b.status==1 && c.status==1 && d.status==1 && e.status==1
                                    select new { d.edge_guid, d.srv_guid, d.mid_guid, d.low_guid, b.bts_server_ip, c.dev_uid, d.unit_id, a.department, edge_status = a.status, host_status = b.status, mid_status = c.status, low_status = d.status,d.low_no }).ToListAsync();
            var lowGuidList = onlineList.Select(c => c.low_guid).ToList();
            var midGuidList = onlineList.Select(c => c.mid_guid).ToList();
            var bindGuidList = await UnitWork.Find<DeviceBindMap>(null).Where(c => lowGuidList.Contains(c.Guid) || midGuidList.Contains(c.Guid)).Select(c => new { c.EdgeGuid, c.SrvGuid, c.Guid, c.GeneratorCode, c.LowGuid, c.BindType }).ToListAsync();
            var hasTestLowList = await UnitWork.Find<DeviceTestLog>(null).Where(c => lowGuidList.Contains(c.LowGuid)).ToListAsync();
            var hasTestMidList = await UnitWork.Find<DeviceTestLog>(null).Where(c => midGuidList.Contains(c.MidGuid)).ToListAsync();
            var host_list = onlineList.Select(c => new { c.edge_guid, c.srv_guid, c.bts_server_ip }).Distinct().Skip((page - 1) * limit).Take(limit).ToList();
            int total = onlineList.Select(c => new { c.edge_guid, c.srv_guid, c.bts_server_ip }).Distinct().Count();
            List<OnlineDeviceResp> list = new List<OnlineDeviceResp>();
            foreach (var item in host_list)
            {
                OnlineDeviceResp onlineDeviceResp = new OnlineDeviceResp();
                onlineDeviceResp.edge_guid = item.edge_guid;
                onlineDeviceResp.srv_guid = item.srv_guid;
                onlineDeviceResp.bts_server_ip = item.bts_server_ip;
                onlineDeviceResp.status = onlineList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid).FirstOrDefault()?.host_status;
                onlineDeviceResp.mid_Lists = new List<mid_list>();
                var mid_lists = onlineList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid).OrderBy(c=>c.dev_uid).Select(c => new { c.mid_guid, c.dev_uid }).Distinct().ToList();
                foreach (var mitem in mid_lists)
                {
                    var has_test = false;
                    var has_bind = bindGuidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.Guid == mitem.mid_guid).Any();
                    if (has_bind)
                    {
                        string code = bindGuidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.Guid == mitem.mid_guid).Select(c => c.GeneratorCode).FirstOrDefault();
                        has_test = hasTestMidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.MidGuid == mitem.mid_guid && c.GeneratorCode==code).Any();
                    }
                    else
                    {
                        has_test = hasTestMidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.MidGuid == mitem.mid_guid).Any();
                    }
                    if (has_test && has_bind)
                    {
                        mid_list ml = new mid_list();
                        ml.has_bind = has_bind;
                        ml.has_test = has_test;
                        ml.dev_uid = mitem.dev_uid.Value;
                        ml.mid_guid = mitem.mid_guid;
                        ml.status = onlineList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid && c.dev_uid == mitem.dev_uid && c.mid_guid == mitem.mid_guid).FirstOrDefault()?.host_status.Value;
                        ml.GeneratorCode = bindGuidList.Where(c => c.Guid == mitem.mid_guid).Select(c => c.GeneratorCode).FirstOrDefault();
                        ml.low_Lists = new List<low_list>();
                        var low_Lists = onlineList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid && c.dev_uid == mitem.dev_uid && c.mid_guid == mitem.mid_guid).OrderBy(c=>c.low_no).Select(c => new low_list { unit_id = c.unit_id.Value, status = c.low_status.Value, low_guid = c.low_guid,low_no=c.low_no }).Distinct().ToList();
                        foreach (var litem in low_Lists)
                        {
                            low_list low_List = new low_list();
                            low_List.has_bind = bindGuidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.Guid == mitem.mid_guid && c.LowGuid == litem.low_guid).Any();
                            if (low_List.has_bind)
                            {
                                string code = bindGuidList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.Guid == mitem.mid_guid).Select(c => c.GeneratorCode).FirstOrDefault();
                                low_List.has_test = hasTestLowList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.MidGuid == mitem.mid_guid && c.LowGuid == litem.low_guid && c.GeneratorCode==code).Any();
                            }
                            else
                            {
                                low_List.has_test = hasTestLowList.Where(c => c.EdgeGuid == item.edge_guid && c.SrvGuid == item.srv_guid && c.MidGuid == mitem.mid_guid && c.LowGuid == litem.low_guid).Any();
                            }

                            if (low_List.has_test && low_List.has_bind)
                            {
                                low_List.status = litem.status;
                                low_List.unit_id = litem.low_no.Value;
                                low_List.GeneratorCode = bindGuidList.Where(c => c.LowGuid == litem.low_guid).Select(c => c.GeneratorCode).FirstOrDefault();
                                low_List.low_guid = litem.low_guid;
                                ml.low_Lists.Add(low_List);
                            }
                        }
                        onlineDeviceResp.mid_Lists.Add(ml);
                    }
                }
                if (!onlineDeviceResp.mid_Lists.Any())
                {
                    continue;
                }
                list.Add(onlineDeviceResp);
            }
            result.Data = list;
            result.Count = total;
            return result;
        }

        /// <summary>
        /// 绑定设备
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<TableData> BindDevice(BindDeviceReq model)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var user = loginContext.User;
            var lowGuidList = model.low_Lists.Select(c => c.LowGuid).Distinct().ToList();
            var bindMap = await UnitWork.Find<DeviceBindMap>(null).Where(c =>lowGuidList.Contains(c.LowGuid)).AnyAsync();
            if (bindMap)
            {
                throw new Exception("有设备已被绑定!");
            }
            var BindType= await UnitWork.Find<DeviceBindMap>(null).Where(c => c.GeneratorCode==model.GeneratorCode).Select(c=>c.BindType).FirstOrDefaultAsync();
            if (BindType==2)
            {
                throw new Exception("有设备已被绑定!");
            }
            if (BindType==1 && model.BindType==2)
            {
                throw new Exception("当前生产码已有中位机绑定,无法单独绑定下位机!");
            }
            var department = loginContext.Orgs.OrderByDescending(c => c.CascadeId).Select(c => c.Name).FirstOrDefault();
            var lowGuids = model.low_Lists.Select(c => c.LowGuid).Distinct();
            var lowList = await UnitWork.Find<edge_low>(null).Where(c => lowGuids.Contains(c.low_guid)).Select(c => new { c.low_guid, c.range_curr_array }).ToListAsync();
            List<DeviceBindMap> list = new List<DeviceBindMap>();
            List<DeviceBindLog> logList = new List<DeviceBindLog>();
            foreach (var item in model.low_Lists)
            {
                DeviceBindMap deviceBind = new DeviceBindMap();
                deviceBind.GeneratorCode = model.GeneratorCode.ToUpper();
                deviceBind.Guid = model.Guid;
                deviceBind.EdgeGuid = model.EdgeGuid;
                deviceBind.SrvGuid = model.SrvGuid;
                deviceBind.DevUid = model.DevUid;
                deviceBind.UnitId = item.UnitId;
                deviceBind.BtsServerIp = model.BtsServerIp;
                deviceBind.CreateTime = DateTime.Now;
                deviceBind.CreateUserId = user.Id;
                deviceBind.CreateUser = user.Name;
                deviceBind.LowGuid = item.LowGuid;
                deviceBind.BindType = model.BindType;
                deviceBind.Department = department;
                deviceBind.OrderNo = Convert.ToInt64(model.GeneratorCode.Split('-')[1]);
                deviceBind.RangeCurrArray = lowList.Where(c => c.low_guid == item.LowGuid).Select(c => c.range_curr_array).FirstOrDefault();
                list.Add(deviceBind);
                DeviceBindLog deviceBindLog = new DeviceBindLog();
                deviceBindLog.GeneratorCode = model.GeneratorCode.ToUpper();
                deviceBindLog.Guid = model.Guid;
                deviceBindLog.EdgeGuid = model.EdgeGuid;
                deviceBindLog.SrvGuid = model.SrvGuid;
                deviceBindLog.DevUid = model.DevUid;
                deviceBindLog.UnitId = item.UnitId;
                deviceBindLog.BtsServerIp = model.BtsServerIp;
                deviceBindLog.CreateTime = DateTime.Now;
                deviceBindLog.CreateUserId = user.Id;
                deviceBindLog.CreateUser = user.Name;
                deviceBindLog.LowGuid = item.LowGuid;
                deviceBindLog.BindType = model.BindType;
                deviceBindLog.Department = department;
                deviceBindLog.OrderNo = Convert.ToInt64(model.GeneratorCode.Split('-')[1]);
                deviceBindLog.OperationType = 0;
                logList.Add(deviceBindLog);
            }
            await UnitWork.BatchAddAsync<DeviceBindMap, int>(list.ToArray());
            await UnitWork.BatchAddAsync<DeviceBindLog, int>(logList.ToArray());
            await UnitWork.SaveAsync();
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 解绑设备
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<TableData> UnBindDevice(UnBindDeviceReq model)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var user = loginContext.User;
            List<DeviceBindLog> logList = new List<DeviceBindLog>();
            if (model.UnBindType == 2)
            {
                var bindMap = await UnitWork.Find<DeviceBindMap>(null).Where(c => c.GeneratorCode == model.GeneratorCode && c.Guid == model.Guid && c.LowGuid == model.LowGuid).ToArrayAsync();
                if (!bindMap.Any())
                {
                    throw new Exception("当前生产码暂无绑定数据无法解绑!");
                }
                await UnitWork.BatchDeleteAsync(bindMap);
                await UnitWork.SaveAsync();
                foreach (var item in bindMap)
                {
                    DeviceBindLog deviceBindLog = new DeviceBindLog();
                    deviceBindLog.GeneratorCode = model.GeneratorCode.ToUpper();
                    deviceBindLog.Guid = model.Guid;
                    deviceBindLog.EdgeGuid = item.EdgeGuid;
                    deviceBindLog.SrvGuid = item.SrvGuid;
                    deviceBindLog.DevUid = item.DevUid;
                    deviceBindLog.UnitId = item.UnitId;
                    deviceBindLog.BtsServerIp = item.BtsServerIp;
                    deviceBindLog.CreateTime = DateTime.Now;
                    deviceBindLog.CreateUserId = user.Id;
                    deviceBindLog.CreateUser = user.Name;
                    deviceBindLog.LowGuid = item.LowGuid;
                    deviceBindLog.BindType = item.BindType;
                    deviceBindLog.Department = item.Department;
                    deviceBindLog.OrderNo = Convert.ToInt64(model.GeneratorCode.Split('-')[1]);
                    deviceBindLog.OperationType = 1;
                    logList.Add(deviceBindLog);
                }
                await UnitWork.BatchAddAsync<DeviceBindLog, int>(logList.ToArray());
                await UnitWork.SaveAsync();
            }
            else
            {
                var bindMap = await UnitWork.Find<DeviceBindMap>(null).Where(c => c.GeneratorCode == model.GeneratorCode && c.Guid == model.Guid).ToArrayAsync();
                if (!bindMap.Any())
                {
                    throw new Exception("当前生产码暂无绑定数据无法解绑!");
                }
                await UnitWork.BatchDeleteAsync(bindMap);
                await UnitWork.SaveAsync();
                foreach (var item in bindMap)
                {
                    DeviceBindLog deviceBindLog = new DeviceBindLog();
                    deviceBindLog.GeneratorCode = model.GeneratorCode.ToUpper();
                    deviceBindLog.Guid = model.Guid;
                    deviceBindLog.EdgeGuid = item.EdgeGuid;
                    deviceBindLog.SrvGuid = item.SrvGuid;
                    deviceBindLog.DevUid = item.DevUid;
                    deviceBindLog.UnitId = item.UnitId;
                    deviceBindLog.BtsServerIp = item.BtsServerIp;
                    deviceBindLog.CreateTime = DateTime.Now;
                    deviceBindLog.CreateUserId = user.Id;
                    deviceBindLog.CreateUser = user.Name;
                    deviceBindLog.LowGuid = item.LowGuid;
                    deviceBindLog.BindType = item.BindType;
                    deviceBindLog.Department = item.Department;
                    deviceBindLog.OrderNo = Convert.ToInt64(model.GeneratorCode.Split('-')[1]);
                    deviceBindLog.OperationType = 1;
                    logList.Add(deviceBindLog);
                }
                await UnitWork.BatchAddAsync<DeviceBindLog, int>(logList.ToArray());
                await UnitWork.SaveAsync();
            }
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 未绑定设备列表
        /// </summary>
        /// <param name="GeneratorCode"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> NoBindDeviceList(string GeneratorCode)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var OrderNo = Convert.ToInt64(GeneratorCode.Split("-")[1]);
            int count = Convert.ToInt32(GeneratorCode.Split("-")[2]);
            var list = await UnitWork.Find<DeviceBindMap>(null).Where(c => c.OrderNo == OrderNo && c.BindType==2).Select(c => c.GeneratorCode).Distinct().ToListAsync();
            List<string> deviceList = new List<string>();
            for (var i = 1; i <= count; i++)
            {
                string code = $"WO-{OrderNo}-{count}-{i}";
                deviceList.Add(code);
            }
            var noBindDeviceList = deviceList.Except(list);
            result.Data = noBindDeviceList;
            return result;
        }
        /// <summary>
        /// 烤机扫码
        /// </summary>
        /// <param name="GeneratorCode"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<TableData> ProductOrderList(string GeneratorCode)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var OrderNo = Convert.ToInt32(GeneratorCode.Split("-")[1]);
            var query = await UnitWork.Find<product_owor>(null).Where(c => c.DocEntry == OrderNo).FirstOrDefaultAsync();
            if (query==null)
            {
                throw new Exception($"{OrderNo}订单不存在!");
            }
            var arry = query.ItemCode.Split('-');
            string st = arry[0].Substring(0, 1).ToUpper();
            if (!st.Equal("C"))
            {
                throw new Exception($"{query.ItemCode}物料编码无法进行烤机操作!");
            }
            result.Data = new
            {
                query.ItemCode,
                GeneratorCode
            };
            return result;
        }
    }
}