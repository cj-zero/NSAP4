using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Nwcali.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Serilog;

namespace OpenAuth.App
{
    /// <summary>
    /// 工步模板
    /// </summary>
    public class StepVersionApp : OnlyUnitWorkBaeApp
    {
        private readonly IOptions<AppSetting> _appConfiguration;

        /// <summary>
        /// 工步模板
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public StepVersionApp(IUnitWork unitWork, IAuth auth, IOptions<AppSetting> appConfiguration) : base(unitWork, auth)
        {
            _appConfiguration = appConfiguration;
        }

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryStepversionListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var objs = UnitWork.Find<StepVersion>(null)
                .WhereIf(!string.IsNullOrWhiteSpace(request.SeriesName), c => c.SeriesName.Contains(request.SeriesName))
                .WhereIf(!string.IsNullOrWhiteSpace(request.StepVersionName), c => c.StepVersionName.Contains(request.StepVersionName))
                .WhereIf(!string.IsNullOrWhiteSpace(request.StepName), c => c.StepName.Contains(request.StepName));
            result.Data = await objs.OrderByDescending(u => u.Sorts)
              .Skip((request.page - 1) * request.limit)
              .Take(request.limit).ToListAsync();
            result.Count = objs.Count();
            return result;
        }
        /// <summary>
        /// 模板详情
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(int id)
        {
            var result = new TableData();
            var query = UnitWork.Find<StepVersion>(null).Where(c => c.Id == id);

            result.Data = await query.ToListAsync();
            result.Count = await query.CountAsync();
            return result;
        }
        /// <summary>
        /// 新增模板
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateStepVersionReq req)
        {
            var obj = req.MapTo<StepVersion>();
            //todo:补充或调整自己需要的字段
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUser = user.Name;
            obj = await UnitWork.AddAsync<StepVersion, int>(obj);
            await UnitWork.SaveAsync();
        }
        /// <summary>
        /// 更新模板
        /// </summary>
        /// <param name="obj"></param>
        public void Update(AddOrUpdateStepVersionReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<StepVersion>(u => u.Id == obj.Id, u => new StepVersion
            {
                StepName = obj.StepName,
                SeriesName = obj.SeriesName,
                StepVersionName = obj.StepVersionName,
                FilePath = obj.FilePath,
                FileName = obj.FileName,
                Remark = obj.Remark,
                CreateUserId = obj.CreateUserId,
                CreateUser = obj.CreateUser,
                CreateTime = obj.CreateTime,
                Sorts = obj.Sorts,
                Status = obj.Status,
                UpdateTime = DateTime.Now
            });
        }
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task Delete(int[] ids)
        {
            await UnitWork.DeleteAsync<StepVersion>(u => ids.Contains(u.Id));
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 工步模板列表
        /// </summary>
        /// <param name="SeriesName"></param>
        /// <param name="StepVersionName"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> DingTalkStepList(string SeriesName, string StepVersionName)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            result.Data = await UnitWork.Find<StepVersion>(null).Where(c => c.SeriesName == SeriesName && c.StepVersionName == StepVersionName && c.Status == true).OrderByDescending(c => c.Sorts).Select(c => new { c.FileName, c.Id, c.Remark, c.FilePath }).ToListAsync();
            return result;
        }
        /// <summary>
        /// 烤机启动测试
        /// </summary>
        /// <param name="model"></param>
        /// <param name="stepCount"></param>
        /// <param name="step_data"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        /// <exception cref="Exception"></exception>
        public async Task<TableData<List<DeviceTestResponse>>> ChannelControlAsync(ChannelControlReq model, int stepCount, string step_data)
        {
            var result = new TableData<List<DeviceTestResponse>>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var departmentList = loginContext.Orgs.Select(c => c.Name).ToList();
            List<DeviceTestResponse> list = new List<DeviceTestResponse>();
            string offLineGeneratorCode = "";
            if (model.type == 1)
            {

                var allBindList = await UnitWork.Find<DeviceBindMap>(null).Where(c => departmentList.Contains(c.Department)).ToListAsync();//已绑定
                var hasTestLow = (from a in allBindList.AsQueryable()
                                  join b in UnitWork.Find<DeviceTestLog>(null) on new { a.GeneratorCode, a.LowGuid } equals new { b.GeneratorCode, b.LowGuid }
                                  select new { a.GeneratorCode, a.LowGuid }).ToList();
                var hasTestLowGuid = hasTestLow.Select(c => c.LowGuid).ToList();
                var hasTestCode = hasTestLow.Select(c => c.GeneratorCode).ToList();
                var canTestList = allBindList.Where(c => !hasTestLowGuid.Contains(c.LowGuid) && !hasTestCode.Contains(c.GeneratorCode)).ToList();

                var allOnlineLowList = await (from a in UnitWork.Find<edge>(null)
                                              join b in UnitWork.Find<edge_host>(null) on a.edge_guid equals b.edge_guid
                                              join d in UnitWork.Find<edge_mid>(null) on new { b.edge_guid, b.srv_guid } equals new { d.edge_guid, d.srv_guid }
                                              join c in UnitWork.Find<edge_low>(null) on new { d.edge_guid, d.srv_guid, d.mid_guid } equals new { c.edge_guid, c.srv_guid, c.mid_guid }
                                              where departmentList.Contains(a.department) && a.status == 1 && b.status == 1 && c.status == 1 && d.status == 1
                                              select new { c.low_guid, c.range_curr_array }).ToListAsync();
                var allOnlineLowListGuid = allOnlineLowList.Select(c => c.low_guid).Distinct().ToList();
                var hasBindList = await UnitWork.Find<DeviceBindMap>(null).Where(c => departmentList.Contains(c.Department)).ToListAsync();
                List<string> allBindlowList = hasBindList.Select(c => c.GeneratorCode).ToList();
                if (hasBindList.Count <= 0)
                {
                    throw new Exception("暂未绑定数据无法启动测试");
                }
                var hasTestLowList = await UnitWork.Find<DeviceTestLog>(null).Where(c => allBindlowList.Contains(c.GeneratorCode)).Select(c => c.GeneratorCode).Distinct().ToListAsync();
                var needTestList = hasBindList.Where(c => !hasTestLowList.Contains(c.GeneratorCode)).ToList();
                var offLineList = needTestList.Where(c => !allOnlineLowListGuid.Contains(c.LowGuid)).Select(c => new { c.GeneratorCode, c.LowGuid }).Distinct().ToList();
                offLineGeneratorCode = offLineList.Select(c => c.GeneratorCode).Distinct().FirstOrDefault();
                //var canTestList = needTestList.Where(c => allOnlineLowListGuid.Contains(c.LowGuid)).Distinct().ToList();
                var canTestLowGuid = canTestList.Select(c => c.LowGuid).ToList();
                var channelList = await UnitWork.Find<edge_channel>(null).Where(c => canTestLowGuid.Contains(c.low_guid)).ToListAsync();
                foreach (var item in canTestList)
                {
                    DeviceTestResponse deviceTest = new DeviceTestResponse();
                    deviceTest.GeneratorCode = item.GeneratorCode;
                    deviceTest.EdgeGuid = item.EdgeGuid;
                    deviceTest.SrvGuid = item.SrvGuid;
                    deviceTest.BtsServerIp = item.BtsServerIp;
                    deviceTest.MidGuid = item.Guid;
                    deviceTest.LowGuid = item.LowGuid;
                    deviceTest.canTestDeviceResp = new CanTestDeviceResp();
                    deviceTest.canTestDeviceResp.edge_guid = item.EdgeGuid;
                    deviceTest.canTestDeviceResp.control = new control();
                    deviceTest.canTestDeviceResp.control.arg = "";
                    deviceTest.canTestDeviceResp.control.cmd_type = "start_test";
                    deviceTest.Department = item.Department;
                    deviceTest.stepCount = stepCount;
                    int maxRange = Convert.ToInt32(item.RangeCurrArray.Split(',').Max());
                    deviceTest.MaxRange = maxRange;
                    arg arg = new arg();
                    arg.srv_guid = item.SrvGuid;
                    arg.ip = item.BtsServerIp;
                    arg.dev_uid = item.DevUid;
                    arg.batch_no = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                    arg.test_name = "";
                    arg.creator = loginContext.User.Name;
                    arg.step_file_name = "";
                    arg.start_step = 1;
                    arg.scale = 1000;//待处理
                    arg.battery_mass = 0;
                    arg.desc = "";
                    arg.step_data = step_data;
                    var chlList = channelList.Where(c => c.edge_guid == item.EdgeGuid && c.srv_guid == item.SrvGuid && c.mid_guid == item.Guid && c.low_guid == item.LowGuid).ToList();
                    arg.chl = new List<chl>();
                    foreach (var citem in chlList)
                    {
                        chl chl = new chl();
                        chl.chl_id = citem.bts_id;
                        chl.dev_uid = item.DevUid;
                        chl.unit_id = item.UnitId;
                        chl.barcode = "";
                        chl.battery_mass = 0;
                        chl.desc = "";
                        arg.chl.Add(chl);
                    }
                    deviceTest.canTestDeviceResp.control.arg = JsonConvert.SerializeObject(arg);
                    list.Add(deviceTest);
                }
            }
            else
            {
                var allOnlineLowList = await (from a in UnitWork.Find<edge>(null)
                                              join b in UnitWork.Find<edge_host>(null) on a.edge_guid equals b.edge_guid
                                              join d in UnitWork.Find<edge_mid>(null) on new { b.edge_guid, b.srv_guid } equals new { d.edge_guid, d.srv_guid }
                                              join c in UnitWork.Find<edge_low>(null) on new { d.edge_guid, d.srv_guid, d.mid_guid } equals new { c.edge_guid, c.srv_guid, c.mid_guid }
                                              where departmentList.Contains(a.department) && a.status == 1 && b.status == 1 && c.status == 1 && d.status == 1
                                              select new { c.low_guid, c.range_curr_array }).ToListAsync();
                var allOnlineLowListGuid = allOnlineLowList.Select(c => c.low_guid).Distinct().ToList();
                var hasBindList = await UnitWork.Find<DeviceBindMap>(null).Where(c => departmentList.Contains(c.Department)).ToListAsync();
                //List<string> allBindlowList = hasBindList.Select(c => c.LowGuid).ToList();
                if (hasBindList.Count <= 0)
                {
                    throw new Exception("暂未绑定数据无法启动测试");
                }
                var canTestList = hasBindList.Where(c => allOnlineLowListGuid.Contains(c.LowGuid)).Distinct().ToList();
                var canTestLowGuid = canTestList.Select(c => c.LowGuid).ToList();
                var channelList = await UnitWork.Find<edge_channel>(null).Where(c => canTestLowGuid.Contains(c.low_guid)).ToListAsync();
                foreach (var item in canTestList)
                {
                    DeviceTestResponse deviceTest = new DeviceTestResponse();
                    deviceTest.GeneratorCode = item.GeneratorCode;
                    deviceTest.EdgeGuid = item.EdgeGuid;
                    deviceTest.SrvGuid = item.SrvGuid;
                    deviceTest.BtsServerIp = item.BtsServerIp;
                    deviceTest.MidGuid = item.Guid;
                    deviceTest.LowGuid = item.LowGuid;
                    deviceTest.canTestDeviceResp = new CanTestDeviceResp();
                    deviceTest.canTestDeviceResp.edge_guid = item.EdgeGuid;
                    deviceTest.canTestDeviceResp.control = new control();
                    deviceTest.canTestDeviceResp.control.arg = "";
                    deviceTest.canTestDeviceResp.control.cmd_type = "start_test";
                    deviceTest.Department = item.Department;
                    int maxRange = Math.Abs(Convert.ToInt32(item.RangeCurrArray.Split(',').Max()));
                    deviceTest.MaxRange = maxRange;
                    deviceTest.stepCount = stepCount;
                    arg arg = new arg();
                    arg.srv_guid = item.SrvGuid;
                    arg.ip = item.BtsServerIp;
                    arg.dev_uid = item.DevUid;
                    arg.batch_no = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                    arg.test_name = "";
                    arg.creator = loginContext.User.Name;
                    arg.step_file_name = "";
                    arg.start_step = 1;
                    arg.scale = 10;
                    if (maxRange < 10)
                        arg.scale = 10000;
                    else if (maxRange < 100)
                        arg.scale = 1000;
                    else if (maxRange < 1000)
                        arg.scale = 100;
                    arg.battery_mass = 0;
                    arg.desc = "";
                    var xmlCpntent = XMLHelper.GetXDocument(model.xmlpath);
                    arg.step_data = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlCpntent.ToString()));
                    var chlList = channelList.Where(c => c.edge_guid == item.EdgeGuid && c.srv_guid == item.SrvGuid && c.mid_guid == item.Guid && c.low_guid == item.LowGuid).ToList();
                    arg.chl = new List<chl>();
                    foreach (var citem in chlList)
                    {
                        chl chl = new chl();
                        chl.chl_id = citem.bts_id;
                        chl.dev_uid = item.DevUid;
                        chl.unit_id = item.UnitId;
                        chl.barcode = "";
                        chl.battery_mass = 0;
                        chl.desc = "";
                        arg.chl.Add(chl);
                    }
                    deviceTest.canTestDeviceResp.control.arg = JsonConvert.SerializeObject(arg);
                    list.Add(deviceTest);
                }
            }
            if (!string.IsNullOrWhiteSpace(offLineGeneratorCode))
            {
                result.Message = $"{offLineGeneratorCode}已离线,请检查设备连接状况!";
            }
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> SaveTestResult(List<StartTestResp> list)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var user = loginContext.User;
            List<DeviceTestLog> deviceTestLogList = new List<DeviceTestLog>();
            List<DeviceCheckTask> deviceCheckTasks = new List<DeviceCheckTask>();
            foreach (var item in list)
            {
                foreach (var citem in item.chl_info)
                {
                    DeviceTestLog deviceTest = new DeviceTestLog();
                    deviceTest.GeneratorCode = item.GeneratorCode;
                    deviceTest.EdgeGuid = item.EdgeGuid;
                    deviceTest.EdgeGuid = item.EdgeGuid;
                    deviceTest.BtsServerIp = item.BtsServerIp;
                    deviceTest.SrvGuid = item.SrvGuid;
                    deviceTest.MidGuid = item.MidGuid;
                    deviceTest.LowGuid = item.LowGuid;
                    deviceTest.DevUid = citem.dev_uid;
                    deviceTest.UnitId = citem.unit_id;
                    deviceTest.CreateTime = DateTime.Now;
                    deviceTest.CreateUserId = user.Id;
                    deviceTest.CreateUser = user.Name;
                    deviceTest.ChlId = citem.chl_id;
                    deviceTest.OrderNo = Convert.ToInt64(item.GeneratorCode.Split('-')[1]);
                    deviceTest.Department = item.Department;
                    deviceTest.StepId = 1;
                    deviceTest.StepCount = item.stepCount;
                    deviceTest.MaxRange = item.MaxRange;
                    if (citem.success == true)
                    {
                        deviceTest.Status = 0;
                    }
                    else
                    {
                        deviceTest.Status = 11;
                        deviceTest.CodeTxt = citem.error;
                    }
                    deviceTest.TestId = citem.test_id;
                    deviceTestLogList.Add(deviceTest);

                    DeviceCheckTask checkTask = new DeviceCheckTask();
                    checkTask.EdgeGuid = item.EdgeGuid;
                    checkTask.SrvGuid = item.SrvGuid;
                    checkTask.DevUid = citem.dev_uid;
                    checkTask.UnitId = citem.unit_id;
                    checkTask.ChlId = citem.chl_id;
                    checkTask.TestId = citem.test_id;
                    checkTask.CreateTime = DateTime.Now;
                    deviceCheckTasks.Add(checkTask);
                }
            }
            await UnitWork.BatchAddAsync<DeviceTestLog, int>(deviceTestLogList.ToArray());
            await UnitWork.BatchAddAsync<DeviceCheckTask, int>(deviceCheckTasks.ToArray());
            await UnitWork.SaveAsync();
            result.Data = true;
            return result;
        }

        /// <summary>
        /// 烤机清单
        /// </summary>
        /// <param name="page">分页索引</param>
        /// <param name="limit">分页大小</param>
        /// <param name="GeneratorCode">生产码</param>
        /// <param name="type">1-当前订单 2-所有订单</param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> BakeMachineList(int page, int limit, string GeneratorCode, int type, string key)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var user = loginContext.User;
            int total = 0;
            var department = loginContext.Orgs.OrderByDescending(c => c.CascadeId).Select(c => c.Name).FirstOrDefault();
            List<object> list = new List<object>();
            if (type == 1)
            {
                var OrderNo = Convert.ToInt64(GeneratorCode.Split("-")[1]);
                int count = Convert.ToInt32(GeneratorCode.Split("-")[2]);
                List<string> deviceList = new List<string>();
                for (var i = 1; i <= count; i++)
                {
                    string code = $"WO-{OrderNo}-{count}-{i}";
                    deviceList.Add(code);
                }
                total = deviceList.Count;
                var itemList = deviceList.Skip((page - 1) * limit).Take(limit).ToList();
                var hasTestList = await UnitWork.Find<DeviceTestLog>(null).Where(c => c.Department == department && deviceList.Contains(c.GeneratorCode)).GroupBy(c => new { c.LowGuid, c.ChlId })
                    .Select(c => c.Max(c => c.Id))
                    .ToListAsync();//当前订单最新测试数据对应的id
                var hasBindList = await UnitWork.Find<DeviceBindMap>(null).Where(c => c.OrderNo == OrderNo).Select(c => c.GeneratorCode).ToListAsync();
                var lastTestList = await UnitWork.Find<DeviceTestLog>(null).Where(c => hasTestList.Contains(c.Id)).ToListAsync();
                foreach (var item in itemList)
                {
                    //指令执行状态（-1:测试完成  -2:测试进行中 -3:用户停止 -4:保护 0:测试已启动(控制中) 1:暂停测试 2:离线,
                    ////3:离线暂停 4:蜂鸣器报警 5:同步控制 6:占用下位机 7:点灯 8:抽真空 9:泄真空 10:测漏率）
                    List<object> warningList = new List<object>();
                    string code = item;
                    int status = 0;
                    decimal progress = 0;
                    var statusList = lastTestList.Where(c => c.GeneratorCode == item).Select(c => new { c.Id, c.Status, c.StepCount, c.StepId, c.DevUid, c.UnitId, c.PrtCode, c.CodeTxt, c.ChlId }).ToList();
                    if (!hasBindList.Any(c => c == item))
                    {
                        status = -5;
                    }
                    else if (statusList.Any())
                    {
                        if (statusList.All(c => c.Status == 0))
                        {
                            status = 0;
                        }
                        else if (statusList.All(c => c.Status == -1))
                        {
                            status = -1;
                        }
                        else if (statusList.All(c => c.Status == -2))
                        {
                            status = -2;
                            int totalStep = statusList.Sum(c => c.StepCount);
                            int currentStepCount = statusList.Sum(c => c.StepId);
                            progress = Math.Round((decimal)currentStepCount / (decimal)totalStep * 100);
                        }
                        else if (statusList.Any(c => (c.Status == -4 || c.Status == 4)))
                        {
                            status = -4;
                            warningList = statusList.Where(c => c.Status == -4 || c.Status == 4).Select(c => new { deviceInfo = c.DevUid + "-" + c.UnitId + "-" + c.ChlId, c.CodeTxt }).ToList<object>();
                        }
                    }
                    else
                    {
                        status = -5;
                    }
                    list.Add(new { code, status, warningList, progress });
                }
            }
            else if (type == 2)
            {
                var allOrderList = await UnitWork.Find<DeviceTestLog>(null).Where(c => c.Department == department).GroupBy(c => new { c.OrderNo, c.GeneratorCode }).Select(c => new { c.Key.GeneratorCode, c.Key.OrderNo }).ToListAsync();//已测试的所有订单
                var allBindOrder = await UnitWork.Find<DeviceBindMap>(null).Where(c => c.Department == department).GroupBy(c => new { c.OrderNo, c.GeneratorCode }).Select(c => new { c.Key.GeneratorCode, c.Key.OrderNo }).ToListAsync();
                allOrderList.AddRange(allBindOrder);
                Dictionary<long, string> dic = new Dictionary<long, string>();
                foreach (var item in allOrderList.Distinct())
                {
                    if (dic.ContainsKey(item.OrderNo))
                    {
                        continue;
                    }
                    dic.Add(item.OrderNo, item.GeneratorCode);
                }
                List<string> deviceList = new List<string>();
                foreach (var item in dic)
                {
                    int count = Convert.ToInt32(item.Value.Split("-")[2]);
                    for (var i = 1; i <= count; i++)
                    {
                        string code = $"WO-{item.Key}-{count}-{i}";
                        deviceList.Add(code);
                    }
                }
                List<string> itemList = new List<string>();
                if (!string.IsNullOrWhiteSpace(key))
                {
                    var searchaList = deviceList.Where(c => c.Contains(key)).ToList();
                    total = searchaList.Count;
                    itemList = searchaList.Skip((page - 1) * limit).Take(limit).ToList();
                }
                else
                {
                    total = deviceList.Count;
                    itemList = deviceList.Skip((page - 1) * limit).Take(limit).ToList();
                }
                var hasTestList = await UnitWork.Find<DeviceTestLog>(null).Where(c => c.Department == department && deviceList.Contains(c.GeneratorCode)).GroupBy(c => new { c.LowGuid, c.ChlId })
                                    .Select(c => c.Max(c => c.Id))
                                    .ToListAsync();
                var hasBindList = await UnitWork.Find<DeviceBindMap>(null).Where(c => c.Department == department).Select(c => c.GeneratorCode).ToListAsync();
                var lastTestList = await UnitWork.Find<DeviceTestLog>(null).Where(c => hasTestList.Contains(c.Id)).ToListAsync();
                foreach (var item in itemList)
                {
                    //指令执行状态（-1:测试完成  -2:测试进行中 -3:用户停止 -4:保护,-5:未入栈 0:测试已启动(控制中) 1:暂停测试 2:离线,
                    ////3:离线暂停 4:蜂鸣器报警 5:同步控制 6:占用下位机 7:点灯 8:抽真空 9:泄真空 10:测漏率）
                    List<object> warningList = new List<object>();
                    string code = item;
                    int status = 0;
                    decimal progress = 0;
                    var statusList = lastTestList.Where(c => c.GeneratorCode == item).Select(c => new { c.Id, c.Status, c.StepCount, c.StepId, c.DevUid, c.UnitId, c.PrtCode, c.CodeTxt, c.ChlId }).ToList();
                    if (!hasBindList.Any(c => c == item))
                    {
                        status = -5;
                    }
                    else if (statusList.Any())
                    {
                        if (statusList.All(c => c.Status == 0))
                        {
                            status = 0;
                        }
                        else if (statusList.All(c => c.Status == -1))
                        {
                            status = -1;
                        }
                        else if (statusList.All(c => c.Status == -2))
                        {
                            status = -2;
                            int totalStep = statusList.Sum(c => c.StepCount);
                            int currentStepCount = statusList.Sum(c => c.StepId);
                            progress = Math.Round((decimal)currentStepCount / (decimal)totalStep * 100);
                        }
                        else if (statusList.Any(c => c.Status == -4 || c.Status == 4))
                        {
                            status = -4;
                            warningList = statusList.Where(c => c.Status == -4 || c.Status == 4).Select(c => new { deviceInfo = c.DevUid + "-" + c.UnitId + "-" + c.ChlId, c.CodeTxt }).ToList<object>();
                        }
                    }
                    else
                    {
                        status = -5;
                    }
                    list.Add(new { code, status, warningList, progress });
                }
            }
            result.Data = list;
            result.Count = total;
            return result;
        }

        #region 烤机结果校验

        /// <summary>
        /// 烤机结果校验
        /// </summary>
        public async Task<TableData> DeviceTestCheckResult()
        {
            var result = new TableData();
            var list = await UnitWork.Find<DeviceCheckTask>(null).Where(c => string.IsNullOrWhiteSpace(c.TaskId) && c.ErrCount<=3).OrderBy(c => c.Id).ToListAsync();
            var taskList = await UnitWork.Find<DeviceCheckTask>(null).Where(c => !string.IsNullOrWhiteSpace(c.TaskId) && c.TaskStatus!=2).OrderBy(c => c.Id).ToListAsync();
            string url = $"{_appConfiguration.Value.AnalyticsUrl}api/DataCheck/Task";
            Infrastructure.HttpHelper helper = new Infrastructure.HttpHelper(url);
            foreach (var item in list)
            {
                try
                {
                    List<object> CheckItemsList = new List<object>();
                    CheckItemsList.Add(new { CheckType = 1, CheckArgs = new { full_scale = 1000, tolerance = 0.002 } });
                    CheckItemsList.Add(new { CheckType = 2, CheckArgs = new { std_thr = 0.02 } });
                    CheckItemsList.Add(new { CheckType = 5, CheckArgs = new { std_thr = 0.02 } });
                    var taskData = helper.Post(new
                    {
                        EdgeGuid = item.EdgeGuid,
                        SrvGuid = item.SrvGuid,
                        DevUid = item.DevUid,
                        UnitId = item.UnitId,
                        ChlId = item.ChlId,
                        TestId = item.TestId,
                        CheckItems = CheckItemsList
                    }, url, "", "");
                    JObject taskObj = JObject.Parse(taskData);
                    if (taskObj["status"] == null || taskObj["status"].ToString() != "200")
                    {
                        item.ErrCount++;
                        item.TaskContent = $"烤机检测任务创建失败{taskObj["message"]}";
                        continue;
                    }
                    item.TaskId = taskObj["data"]["TaskId"].ToString();
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"烤机结果校验失败 EdgeGuid={item.EdgeGuid},topics={item.SrvGuid},DevUid={item.DevUid},UnitId={item.UnitId},ChlId={item.ChlId},TestId={item.TestId}",ex);
                    continue;
                }
            }           
            await UnitWork.BatchUpdateAsync(list.ToArray());
            await UnitWork.SaveAsync();

            foreach (var item in taskList)
            {
                try
                {
                    var channelTest = await UnitWork.Find<DeviceTestLog>(null).Where(c => c.EdgeGuid == item.EdgeGuid && c.SrvGuid == item.SrvGuid && c.DevUid == item.DevUid && c.UnitId == item.UnitId && c.TestId == item.TestId && c.ChlId == item.ChlId).FirstOrDefaultAsync();
                    string taskurl = $"{_appConfiguration.Value.AnalyticsUrl}api/DataCheck/TaskResult?id={item.TaskId}";
                    Dictionary<string, string> dic = null;
                    var taskResult = helper.Get(dic, taskurl);
                    JObject res = JObject.Parse(taskResult);
                    if (res["status"] == null || res["status"].ToString() != "200")
                    {
                        continue;
                    }
                    if (res["data"] != null)
                    {
                        sbyte.TryParse(res["data"]["Status"].ToString(), out sbyte TaskStatus);
                        item.TaskStatus = TaskStatus;
                        int.TryParse(res["data"]["ErrCount"].ToString(), out int ErrCount);
                        item.ErrCount = ErrCount;

                        channelTest.TaskErrCount = ErrCount;
                        channelTest.TaskId = item.TaskId;
                        channelTest.TaskStatus = TaskStatus;
                        channelTest.TaskContent = "";
                        if (res["data"]["CheckItems"] != null)
                        {
                            List<DeviceTaskCheckResp> deviceTaskCheckList = new List<DeviceTaskCheckResp>();
                            foreach (var citem in res["data"]["CheckItems"])
                            {
                                DeviceTaskCheckResp deviceTaskCheck = new DeviceTaskCheckResp();
                                long.TryParse(citem["CheckId"].ToString(), out long CheckId);
                                int.TryParse(citem["ErrCount"].ToString(), out int errcount);
                                int.TryParse(citem["CheckType"].ToString(), out int CheckType);
                                deviceTaskCheck.CheckId = CheckId;
                                deviceTaskCheck.CheckType = CheckType;
                                switch (CheckType)
                                {
                                    case 0:
                                        deviceTaskCheck.CheckName = "测试是否按流程完成";
                                        break;
                                    case 1:
                                        deviceTaskCheck.CheckName = "搁置工步是否漏电";
                                        break;
                                    case 2:
                                        deviceTaskCheck.CheckName = "恒压工步电压是否稳定";
                                        break;
                                    case 3:
                                        deviceTaskCheck.CheckName = "放电工步电压是否下降";
                                        break;
                                    case 4:
                                        deviceTaskCheck.CheckName = "充电工步电压是否上升";
                                        break;
                                    case 5:
                                        deviceTaskCheck.CheckName = "恒流工步电流稳定";
                                        break;
                                }
                                deviceTaskCheck.ErrList = new List<string>();
                                if (errcount>0)
                                {
                                    if (citem["Records"] != null)
                                    {
                                        foreach (var ritem in citem["Records"])
                                        {
                                            if (!string.IsNullOrWhiteSpace(ritem["Err"].ToString()))
                                            {
                                                deviceTaskCheck.ErrList.Add(ritem["Err"].ToString());
                                            }
                                        }
                                    }
                                }
                            }
                            if (deviceTaskCheckList.Any())
                            {
                                channelTest.TaskContent = JsonConvert.SerializeObject(deviceTaskCheckList);
                                item.TaskContent = JsonConvert.SerializeObject(deviceTaskCheckList);
                            }
                        }
                    }
                    await UnitWork.UpdateAsync(channelTest);
                    await UnitWork.SaveAsync();
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"烤机任务数据获取失败 TaskId={item.TaskId}", ex);
                    continue;
                }
            }
            var hasCompleteList = taskList.Where(c => c.TaskStatus == 2).ToList();
            var noCompleteList = taskList.Where(c => c.TaskStatus != 2).ToList();
            await UnitWork.BatchUpdateAsync(noCompleteList.ToArray());
            await UnitWork.BatchDeleteAsync(hasCompleteList.ToArray());
            await UnitWork.SaveAsync();
            return result;
        }

        #endregion
    }
}