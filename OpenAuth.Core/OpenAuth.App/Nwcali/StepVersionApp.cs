using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Infrastructure;
using Infrastructure.Extensions;
using Infrastructure.Helpers;
using Infrastructure.MQTT;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali;
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
        /// <param name="appConfiguration"></param>
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
                .WhereIf(!string.IsNullOrWhiteSpace(request.StepVersionName), c => c.StepVersionName.Contains(request.StepVersionName.ToUpper()))
                .WhereIf(!string.IsNullOrWhiteSpace(request.StepName), c => c.StepName.Contains(request.StepName))
                .WhereIf(request.Current != 0, c => c.Current == request.Current);
            result.Data = await objs.OrderByDescending(u => u.Id)
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
            result.Data = await UnitWork.Find<StepVersion>(null).Where(c => c.Id == id).FirstOrDefaultAsync();
            return result;
        }
        /// <summary>
        /// 新增模板
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateStepVersionReq req)
        {
            string str = string.Empty;
            if (req.Voltage != 0)
            {
                str = $"{req.Voltage}V";
            }
            if (req.Current != 0)
            {
                str += $"{req.Current}A";
            }
            req.StepVersionName = str;
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
        public async Task Update(AddOrUpdateStepVersionReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            string str = string.Empty;
            if (obj.Voltage != 0)
            {
                str = $"{obj.Voltage}V";
            }
            if (obj.Current != 0)
            {
                str += $"{obj.Current}A";
            }
            obj.StepVersionName = str;
            await UnitWork.UpdateAsync<StepVersion>(u => u.Id == obj.Id, u => new StepVersion
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
                FilePath2 = obj.FilePath2,
                FileName2 = obj.FileName2,
                Remark2 = obj.Remark2,
                FirstStart = obj.FirstStart,
                UpdateTime = DateTime.Now,
                Voltage = obj.Voltage,
                Current = obj.Current
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
        /// <param name="Current"></param>
        /// <param name="Voltage"></param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> DingTalkStepList(string SeriesName, decimal Current, decimal Voltage)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            result.Data = await UnitWork.Find<StepVersion>(null)
                .Where(c => c.SeriesName == SeriesName && c.Status == true)
                .WhereIf(SeriesName == "6" || SeriesName == "7", c => c.Voltage == Voltage || c.Current == Current)
                .WhereIf(SeriesName != "6" && SeriesName != "7", c => c.Current == Current)
                .OrderByDescending(c => c.Sorts).Select(c => new { c.Id, c.FileName, c.FilePath, c.Remark, c.FilePath2, c.FileName2, c.Remark2, c.FirstStart }).ToListAsync();
            return result;
        }

        /// <summary>
        /// 保存测试结果
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
                    deviceTest.FileIds = item.FileIds;
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
                    deviceTest.ChangeStatusTime = DateTime.Now;
                    deviceTestLogList.Add(deviceTest);

                    DeviceCheckTask checkTask = new DeviceCheckTask();
                    checkTask.EdgeGuid = item.EdgeGuid;
                    checkTask.SrvGuid = item.SrvGuid;
                    checkTask.DevUid = citem.dev_uid;
                    checkTask.UnitId = citem.unit_id;
                    checkTask.ChlId = citem.chl_id;
                    checkTask.TestId = citem.test_id;
                    checkTask.LowGuid = item.LowGuid;
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
                var hasTestList = await UnitWork.Find<DeviceTestLog>(null).Where(c => c.Department == department && deviceList.Contains(c.GeneratorCode) && c.TestId != 0).GroupBy(c => new { c.EdgeGuid, c.SrvGuid, c.DevUid, c.UnitId, c.ChlId })
                    .Select(c => new { c.Key.EdgeGuid, c.Key.SrvGuid, c.Key.DevUid, c.Key.UnitId, c.Key.ChlId, Id = c.Max(c => c.Id) }).Select(c => c.Id)
                    .ToListAsync();//当前订单最新测试数据对应的id
                var hasBindList = await UnitWork.Find<DeviceBindMap>(null).Where(c => c.OrderNo == OrderNo).Select(c => c.GeneratorCode).Distinct().ToListAsync();
                var lastTestList = await UnitWork.Find<DeviceTestLog>(null).Where(c => hasTestList.Contains(c.Id)).ToListAsync();
                foreach (var item in itemList)
                {
                    //指令执行状态（-1:测试完成  -2:测试进行中 -3:用户停止 -4:保护 0:测试已启动(控制中) -5:未入栈 1:暂停测试 2:离线,
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
                            progress = Math.Round(currentStepCount / (decimal)totalStep * 100);
                        }
                        else if (statusList.Any(c => c.Status == -3))
                        {
                            var dockStatus = new List<int> { -1, -3 };
                            var flage = statusList.Select(c => c.Status).Distinct().Except(dockStatus).ToList().Any();
                            if (!flage)
                            {
                                status = -2;
                                int totalStep = statusList.Sum(c => c.StepCount);
                                int currentStepCount = statusList.Sum(c => c.StepId);
                                progress = Math.Round(currentStepCount / (decimal)totalStep * 100);
                            }
                        }
                        else if (statusList.Any(c => (c.Status == -4 || c.Status == 4)))
                        {
                            status = -4;
                            warningList = statusList.Where(c => c.Status == -4 || c.Status == 4).Select(c => new { deviceInfo = c.DevUid + "-" + c.UnitId + "-" + c.ChlId, c.CodeTxt }).ToList<object>();
                        }
                        else
                        {
                            var dockStatus = new List<int> { -1, -2 };
                            var flage = statusList.Select(c => c.Status).Distinct().Except(dockStatus).ToList().Any();
                            if (!flage)
                            {
                                status = -2;
                                int totalStep = statusList.Sum(c => c.StepCount);
                                int currentStepCount = statusList.Sum(c => c.StepId);
                                progress = Math.Round(currentStepCount / (decimal)totalStep * 100);
                            }
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
                            progress = Math.Round(currentStepCount / (decimal)totalStep * 100);
                        }
                        else if (statusList.Any(c => c.Status == -4 || c.Status == 4))
                        {
                            status = -4;
                            warningList = statusList.Where(c => c.Status == -4 || c.Status == 4).Select(c => new { deviceInfo = c.DevUid + "-" + c.UnitId + "-" + c.ChlId, c.CodeTxt }).ToList<object>();
                        }
                        else
                        {
                            var dockStatus = new List<int> { -1, -2 };
                            var flage = statusList.Select(c => c.Status).Distinct().Except(dockStatus).ToList().Any();
                            if (!flage)
                            {
                                status = -2;
                                int totalStep = statusList.Sum(c => c.StepCount);
                                int currentStepCount = statusList.Sum(c => c.StepId);
                                progress = Math.Round(currentStepCount / (decimal)totalStep * 100);
                            }
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
            var list = await UnitWork.Find<DeviceCheckTask>(null).Where(c => string.IsNullOrWhiteSpace(c.TaskId) && c.ErrCount <= 3).OrderBy(c => c.Id).ToListAsync();
            string url = $"{_appConfiguration.Value.AnalyticsUrl}api/DataCheck/Task";
            Infrastructure.HttpHelper helper = new Infrastructure.HttpHelper(url);
            foreach (var item in list)
            {
                try
                {
                    List<object> CheckItemsList = new List<object>();
                    CheckItemsList.Add(new { CheckType = 1, CheckArgs = new { full_scale = 1000, tolerance = 0.002 } });
                    //CheckItemsList.Add(new { CheckType = 2, CheckArgs = new { std_thr = 0.0007 } });
                    CheckItemsList.Add(new { CheckType = 5, CheckArgs = new { std_thr = 0.0007 } });
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
                    }
                    else
                    {
                        item.TaskId = taskObj["data"] == null ? "" : taskObj["data"]["TaskId"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    Log.Logger.Error($"烤机结果校验失败 EdgeGuid={item.EdgeGuid},topics={item.SrvGuid},DevUid={item.DevUid},UnitId={item.UnitId},ChlId={item.ChlId},TestId={item.TestId},message={ex.Message}");
                    continue;
                }
            }
            if (list.Any())
            {
                await UnitWork.BatchUpdateAsync(list.ToArray());
                await UnitWork.SaveAsync();
            }
            return result;
        }

        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<TableData<List<string>>> EdgeGuidList()
        {
            var result = new TableData<List<string>>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                result.Code = Define.INVALID_TOKEN;
                result.Message = "登录已过期";
                return result;
            }
            var department = loginContext.Orgs.OrderByDescending(c => c.CascadeId).Select(c => c.Name).FirstOrDefault();
            result.Data = await UnitWork.Find<edge>(null).Where(c => c.department == department).Select(c => c.edge_guid).ToListAsync();
            return result;
        }

        /// <summary>
        /// wms token
        /// </summary>
        /// <returns></returns>
        public string WmsAccessToken()
        {
            try
            {
                var rk = "xy.service.api";
                if (RedisHelper.Exists(rk))
                {
                    var token = RedisHelper.Get<string>(rk);
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        return token;
                    }
                }
                var key = "devbindxy.service.apixinwei123";
                byte[] bytes = Encoding.UTF8.GetBytes(key);
                byte[] hash = SHA256.Create().ComputeHash(bytes);
                var signature = Convert.ToBase64String(hash);
                var urls = "http://identity.neware.cloud/v1/token";
                Infrastructure.HttpHelper helper = new Infrastructure.HttpHelper(urls);
                var passTokenStr = helper.Post(new
                {
                    clientCode = "devbind",
                    scope = new List<string> { "xy.service.api" },
                    signature = signature
                }, urls, "", "");
                JObject passTokenObj = JObject.Parse(passTokenStr);
                if (passTokenObj == null || passTokenObj["data"] == null || passTokenObj["data"]["accessToken"] == null)
                {
                    return "";
                }
                if (!string.IsNullOrWhiteSpace(passTokenObj["data"]["accessToken"].ToString()))
                {
                    RedisHelper.Set(rk, passTokenObj["data"]["accessToken"].ToString(), new TimeSpan(0, 0, Convert.ToInt32(passTokenObj["data"]["expireIn"])));
                }
                return passTokenObj["data"]["accessToken"].ToString();
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"WMS Token 获取失败! message={ex.Message}");
                return "";
            }
        }

        /// <summary>
        /// 同步设备数据
        /// </summary>
        /// <param name="list"></param>
        /// <param name="EdgeGuid"></param>
        /// <returns></returns>
        public async Task<TableData> SyncDeviceList(List<BtsDeviceResp> list, string EdgeGuid)
        {
            var result = new TableData();
            var edgeInfo = await UnitWork.Find<edge>(null).Where(c => c.edge_guid == EdgeGuid).FirstOrDefaultAsync();
            if (edgeInfo == null)
            {
                throw new Exception($"{EdgeGuid}不存在!");
            }
            //guid 获取sn TO DO
            //var wmsAccessToken = WmsAccessToken();
            //if (string.IsNullOrWhiteSpace(wmsAccessToken))
            //{
            //    throw new Exception($"WMS token 获取失败!");
            //}
            //string urls = "http://service.neware.cloud/common/DevSnByGuid";
            //Infrastructure.HttpHelper helper = new Infrastructure.HttpHelper(urls);
            //var guids = list.Select(c => c.low_guid).Distinct().ToArray();
            //var hasBindGuids = await UnitWork.Find<DeviceBindMap>(null).Where(c => guids.Contains(c.LowGuid)).Select(c => c.LowGuid).ToListAsync();
            //var unBindGuids = guids.Where(c => !hasBindGuids.Contains(c)).ToArray();
            //var datastr = helper.PostAuthentication(unBindGuids, urls, wmsAccessToken);
            //JObject dataObj = JObject.Parse(datastr);
            //List<WmsLowGuidResp> wmsLowGuids = new List<WmsLowGuidResp>();
            //try
            //{
            //    wmsLowGuids = JsonConvert.DeserializeObject<List<WmsLowGuidResp>>(JsonConvert.SerializeObject(dataObj["data"]["devBindInfo"]));
            //}
            //catch (Exception ex)
            //{
            //    wmsLowGuids = new List<WmsLowGuidResp>();
            //    Log.Logger.Error($"WMS guid 获取失败! message={ex.Message}");
            //}
            //sn获取生产码  TO DO



            DateTime dt = DateTime.Now;
            edgeInfo.status = 1;
            edgeInfo.CreateTime = dt;
            List<edge_host> hostList = new List<edge_host>();
            List<edge_mid> midList = new List<edge_mid>();
            List<edge_low> lowList = new List<edge_low>();
            List<edge_channel> channelList = new List<edge_channel>();
            foreach (var item in list)
            {
                var host = hostList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid).Any();
                if (!host)
                {
                    edge_host hostModel = new edge_host();
                    hostModel.edge_guid = EdgeGuid;
                    hostModel.srv_guid = item.srv_guid;
                    hostModel.bts_server_version = item.bts_server_version;
                    hostModel.bts_server_ip = item.bts_server_ip;
                    hostModel.bts_type = item.bts_type;
                    hostModel.status = 1;
                    hostModel.CreateTime = dt;
                    hostList.Add(hostModel);
                }
                var mid = midList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid && c.dev_uid == item.dev_uid && c.mid_guid == item.mid_guid).Any();
                if (!mid)
                {
                    edge_mid midModel = new edge_mid();
                    midModel.edge_guid = item.edge_guid;
                    midModel.srv_guid = item.srv_guid;
                    midModel.mid_guid = item.mid_guid;
                    midModel.dev_uid = item.dev_uid;
                    midModel.mid_version = item.mid_version;
                    midModel.production_serial = item.production_serial;
                    midModel.status = 1;
                    midModel.CreateTime = dt;
                    midList.Add(midModel);
                }
                if (!string.IsNullOrWhiteSpace(item.low_guid))
                {
                    var low = lowList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid && c.mid_guid == item.mid_guid && c.low_guid == item.low_guid).Any();
                    if (!low)
                    {
                        edge_low lowModel = new edge_low();
                        lowModel.edge_guid = item.edge_guid;
                        lowModel.srv_guid = item.srv_guid;
                        lowModel.mid_guid = item.mid_guid;
                        lowModel.low_guid = item.low_guid;
                        lowModel.low_no = item.low_no;
                        lowModel.unit_id = item.unit_id;
                        lowModel.range_volt = item.range_volt;
                        lowModel.range_curr_array = item.range_curr_array;
                        lowModel.low_version = item.low_version;
                        lowModel.status = 1;
                        lowModel.CreateTime = dt;
                        lowModel.dev_uid = item.dev_uid;
                        lowList.Add(lowModel);
                    }
                }
                foreach (var citem in item.bts_ids)
                {
                    if (!string.IsNullOrWhiteSpace(citem))
                    {
                        var bts_id = Convert.ToInt32(citem);
                        var channel = channelList.Where(c => c.edge_guid == item.edge_guid && c.srv_guid == item.srv_guid && c.mid_guid == item.mid_guid && c.low_guid == item.low_guid && c.bts_id == bts_id).Any();
                        if (!channel)
                        {
                            edge_channel channelModel = new edge_channel();
                            channelModel.edge_guid = item.edge_guid;
                            channelModel.srv_guid = item.srv_guid;
                            channelModel.mid_guid = item.mid_guid;
                            channelModel.low_guid = item.low_guid;
                            channelModel.bts_id = bts_id;
                            channelModel.status = 1;
                            channelModel.CreateTime = dt;
                            channelModel.low_no = item.low_no;
                            channelModel.unit_id = item.unit_id;
                            channelModel.dev_uid = item.dev_uid;
                            channelModel.TestId = 0;
                            channelModel.rt_status = -1;
                            channelModel.bts_server_ip = item.bts_server_ip;
                            channelList.Add(channelModel);
                        }
                    }
                }
            }
            var dbContext = UnitWork.GetDbContext<edge>();
            var flag = false;
            using (var transaction = dbContext.Database.BeginTransaction())
            {
                try
                {
                    await UnitWork.Find<edge>(null).Where(c => c.edge_guid == EdgeGuid).DeleteFromQueryAsync();
                    await UnitWork.Find<edge_host>(null).Where(c => c.edge_guid == EdgeGuid).DeleteFromQueryAsync();
                    await UnitWork.Find<edge_mid>(null).Where(c => c.edge_guid == EdgeGuid).DeleteFromQueryAsync();
                    await UnitWork.Find<edge_low>(null).Where(c => c.edge_guid == EdgeGuid).DeleteFromQueryAsync();
                    await UnitWork.Find<edge_channel>(null).Where(c => c.edge_guid == EdgeGuid).DeleteFromQueryAsync();
                    await UnitWork.AddAsync<edge, int>(edgeInfo);
                    await UnitWork.BatchAddAsync<edge_host, int>(hostList.ToArray());
                    await UnitWork.BatchAddAsync<edge_mid, int>(midList.ToArray());
                    await UnitWork.BatchAddAsync<edge_low, int>(lowList.ToArray());
                    await UnitWork.BatchAddAsync<edge_channel, int>(channelList.ToArray());
                    await UnitWork.SaveAsync();
                    transaction.Commit();
                    flag = true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Log.Logger.Error($"设备数据手动更新异常：edge_guid={EdgeGuid},ex={ex.Message}");
                }
            }
            if (!flag)
            {
                throw new Exception($"设备数据手动同步失败!");
            }
            return result;
        }

        /// <summary>
        /// 停止测试通道列表
        /// </summary>
        /// <returns></returns>
        public TableData<List<StopReq>> CanStopTestList(List<StartDeviceListResp> list)
        {
            var result = new TableData<List<StopReq>>();
            var hostList = list.GroupBy(c => new { c.EdgeGuid, c.SrvGuid, c.BtsServerIp }).Select(c => new { c.Key.EdgeGuid, c.Key.SrvGuid, c.Key.BtsServerIp }).ToList();
            List<StopReq> stopReqs = new List<StopReq>();
            foreach (var item in hostList)
            {
                StopReq model = new StopReq();
                model.edge_guid = item.EdgeGuid;
                model.control = new StopControl();
                model.control.cmd_type = "stop_test";
                StopArg stopArg = new StopArg();
                stopArg.ip = item.BtsServerIp;
                stopArg.chl = new List<ChlInfo>();
                var lowList = list.Where(c => c.EdgeGuid == item.EdgeGuid && c.SrvGuid == item.SrvGuid && c.BtsServerIp == item.BtsServerIp).ToList();
                foreach (var litem in lowList)
                {
                    var channelList = list.Where(c => c.EdgeGuid == litem.EdgeGuid && c.SrvGuid == litem.SrvGuid && c.BtsServerIp == litem.BtsServerIp && c.LowGuid == litem.LowGuid).ToList();
                    foreach (var citem in channelList)
                    {
                        ChlInfo chlInfo = new ChlInfo();
                        chlInfo.dev_uid = citem.dev_uid;
                        chlInfo.unit_id = citem.unit_id;
                        chlInfo.chl_id = citem.bts_id;
                        stopArg.chl.Add(chlInfo);
                    }
                }
                model.control.arg = JsonConvert.SerializeObject(stopArg);
                stopReqs.Add(model);
            }
            result.Data = stopReqs;
            return result;
        }

        #region 启动相关
        /// <summary>
        /// 获取启动列表
        /// </summary>
        /// <param name="GeneratorCode"></param>
        /// <param name="FilterType">过滤类型 1:生产码  2:下位机</param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> CanStartTestList(string GeneratorCode, int FilterType)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var departmen = loginContext.Orgs.Select(c => c.Name).FirstOrDefault();
            long.TryParse(GeneratorCode.Split('-')[1], out long OrderNo);
            var onlineDevList = await (from a in UnitWork.Find<edge>(null)
                                       join b in UnitWork.Find<edge_low>(null) on a.edge_guid equals b.edge_guid
                                       where a.department == departmen && a.status == 1
                                       select new { b.edge_guid, b.srv_guid, b.dev_uid, b.mid_guid, b.unit_id, b.low_guid, b.low_no }).ToListAsync();
            var onlineLowGuid = onlineDevList.Select(c => c.low_guid).Distinct().ToList();
            var bindDevList = await UnitWork.Find<DeviceBindMap>(null).Where(c => onlineLowGuid.Contains(c.LowGuid)).ToListAsync();
            if (!bindDevList.Any() || !onlineDevList.Any())
            {
                result.Data = new List<string> { };
                return result;
            }
            if (FilterType == 1)
            {
                //var hasTestCode= await UnitWork.Find<DeviceTestLog>(null).Where(c => c.OrderNo == OrderNo).Select(c=>c.GeneratorCode).Distinct().ToListAsync();
                result.Data = (from a in bindDevList.AsEnumerable()
                               join b in onlineDevList.AsEnumerable() on a.LowGuid  equals b.low_guid 
                               //where !hasTestCode.Contains(a.GeneratorCode)
                               select new { a.GeneratorCode })
                               .OrderBy(c => c.GeneratorCode)
                               .Distinct()
                               .ToList();
            }
            else
            {
                //var hasTestLow = await UnitWork.Find<DeviceTestLog>(null).Where(c => c.OrderNo == OrderNo).Select(c => c.LowGuid).Distinct().ToListAsync();
                result.Data = (from a in bindDevList.AsEnumerable()
                               join b in onlineDevList.AsEnumerable() on a.LowGuid  equals b.low_guid
                               //where !hasTestLow.Contains(a.LowGuid)
                               select new { a.GeneratorCode, a.DevUid, b.low_no, a.EdgeGuid, a.SrvGuid, a.BtsServerIp, a.Guid, a.LowGuid, a.UnitId })
                               .OrderBy(c => c.GeneratorCode)
                               .ThenBy(c => c.DevUid)
                               .ThenBy(c => c.low_no)
                               .Distinct()
                               .ToList();

            }
            return result;
        }

        /// <summary>
        /// 获取重启动列表
        /// </summary>
        /// <param name="GeneratorCode"></param>
        /// <param name="FilterType">过滤类型 1:生产码  2:下位机</param>
        /// <returns></returns>
        /// <exception cref="CommonException"></exception>
        public async Task<TableData> CanResStartTestList(string GeneratorCode, int FilterType)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var departmen = loginContext.Orgs.Select(c => c.Name).FirstOrDefault();
            long.TryParse(GeneratorCode.Split('-')[1], out long OrderNo);
            var onlineDevList = await (from a in UnitWork.Find<edge>(null)
                                       join b in UnitWork.Find<edge_low>(null) on a.edge_guid equals b.edge_guid
                                       where a.department == departmen && a.status == 1
                                       select new { b.edge_guid, b.srv_guid, b.dev_uid, b.mid_guid, b.unit_id, b.low_guid, b.low_no }).ToListAsync();
            var onlineLowGuid = onlineDevList.Select(c => c.low_guid).Distinct().ToList();
            var bindDevList = await UnitWork.Find<DeviceBindMap>(null).Where(c => onlineLowGuid.Contains(c.LowGuid)).ToListAsync();
            if (!bindDevList.Any() || !onlineDevList.Any())
            {
                result.Data = new List<string> { };
                return result;
            }
            if (FilterType == 1)
            {
                result.Data = (from a in bindDevList.AsEnumerable()
                               join b in onlineDevList.AsEnumerable() on a.LowGuid equals b.low_guid 
                               select new { a.GeneratorCode })
                               .OrderBy(c => c.GeneratorCode)
                               .Distinct()
                               .ToList();
            }
            else
            {
                result.Data = (from a in bindDevList.AsEnumerable()
                               join b in onlineDevList.AsEnumerable() on a.LowGuid equals b.low_guid
                               select new { a.GeneratorCode, DevUid=b.dev_uid, b.low_no, EdgeGuid= b.edge_guid, SrvGuid=b.srv_guid, a.BtsServerIp, Guid=b.mid_guid, LowGuid= b.low_guid, UnitId=b.unit_id })
                               .OrderBy(c => c.GeneratorCode)
                               .ThenBy(c => c.DevUid)
                               .ThenBy(c => c.low_no)
                               .Distinct()
                               .ToList();

            }
            return result;
        }


        /// <summary>
        /// 获取生产码启动数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<List<StartDeviceListResp>> DeviceListByCode(List<string> list)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var departmen = loginContext.Orgs.Select(c => c.Name).FirstOrDefault();
            return await (from a in UnitWork.Find<DeviceBindMap>(null)
                          join c in UnitWork.Find<edge_low>(null) on a.LowGuid  equals c.low_guid 
                          join b in UnitWork.Find<edge_channel>(null) on new { c.edge_guid, c.srv_guid, c.dev_uid, c.low_guid } equals new { b.edge_guid, b.srv_guid, b.dev_uid, b.low_guid }
                          join d in UnitWork.Find<edge>(null) on c.edge_guid equals d.edge_guid
                          where list.Contains(a.GeneratorCode) && d.department==departmen && d.status==1
                          select new StartDeviceListResp { EdgeGuid = b.edge_guid, GeneratorCode = a.GeneratorCode, BtsServerIp = a.BtsServerIp, SrvGuid = a.SrvGuid, MidGuid = a.Guid, RangeCurrArray = c.range_curr_array, dev_uid = b.dev_uid, unit_id = b.unit_id, bts_id = b.bts_id, LowGuid = b.low_guid }).ToListAsync();
        }

        /// <summary>
        /// 获取下位机启动数据
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public async Task<List<StartDeviceListResp>> DeviceListByLow(List<LowDeviceList> list)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var departmen = loginContext.Orgs.Select(c => c.Name).FirstOrDefault();
            var edgeList = list.Select(c => c.EdgeGuid).ToList();
            var srvGuidList = list.Select(c => c.SrvGuid).ToList();
            var devuidList = list.Select(c => c.DevUid).ToList();
            var lowList = list.Select(c => c.LowGuid).ToList();
            return await (from a in UnitWork.Find<DeviceBindMap>(null)
                          join c in UnitWork.Find<edge_low>(null) on a.LowGuid  equals c.low_guid 
                          join b in UnitWork.Find<edge_channel>(null) on new { c.edge_guid,c.srv_guid, c.dev_uid, c.low_guid } equals new { b.edge_guid, b.srv_guid,  b.dev_uid, b.low_guid }
                          join d in UnitWork.Find<edge>(null) on c.edge_guid equals d.edge_guid
                          where edgeList.Contains(b.edge_guid) && srvGuidList.Contains(b.srv_guid) && devuidList.Contains(b.dev_uid) && lowList.Contains(b.low_guid)
                          && d.department == departmen && d.status == 1
                          select new StartDeviceListResp { EdgeGuid = b.edge_guid, GeneratorCode = a.GeneratorCode, BtsServerIp = a.BtsServerIp, SrvGuid = a.SrvGuid, MidGuid = a.Guid, RangeCurrArray = c.range_curr_array, dev_uid = b.dev_uid, unit_id = b.unit_id, bts_id = b.bts_id, LowGuid = b.low_guid }).ToListAsync();
        }

        /// <summary>
        /// 工步文件解析
        /// </summary>
        /// <param name="FilePath"></param>
        /// <returns></returns>
        public TableData<StepContentResp> StepContent(string FilePath)
        {
            var result = new TableData<StepContentResp>();
            StepContentResp model = new StepContentResp();
            var xmlCpntent = XMLHelper.GetXDocument(FilePath).ToString();
            StringReader Reader = new StringReader(xmlCpntent);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(Reader);
            string work_path = $"{AppDomain.CurrentDomain.BaseDirectory}step\\";
            Directory.CreateDirectory(work_path);
            string filename = DateTime.Now.ToString("yyyyMMddHHmmssffff");
            string dir = $"{work_path}{filename}.xml";
            xmlDoc.Save(dir);
            if (!File.Exists(dir))
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
            File.Delete(dir);
            model.stepCount = step.ListStep.Count();
            model.stepData = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlCpntent.ToString()));
            result.Data = model;
            return result;
        }


        /// <summary>
        /// 6,7系列设备启动
        /// </summary>
        /// <param name="startlist"></param>
        /// <param name="stepCount"></param>
        /// <param name="step_data"></param>
        /// <param name="stepCount2"></param>
        /// <param name="step_data2"></param>
        /// <param name="FirstStart"></param>
        /// <param name="TestType"></param>
        /// <returns></returns>
        public TableData<List<DeviceTestResponse>> ShortCircuitStart(List<StartDeviceListResp> startlist, int stepCount, string step_data, int stepCount2, string step_data2, int FirstStart, int? TestType)
        {
            var result = new TableData<List<DeviceTestResponse>>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                result.Code = Define.INVALID_TOKEN;
                result.Message = $"登录已过期!";
                return result;
            }
            var department = loginContext.Orgs.Select(c => c.Name).FirstOrDefault();
            List<DeviceTestResponse> list = new List<DeviceTestResponse>();
            var lowList = startlist.Select(c => new { c.GeneratorCode, c.EdgeGuid, c.SrvGuid, c.BtsServerIp, c.MidGuid, c.dev_uid, c.LowGuid, c.RangeCurrArray, c.unit_id }).Distinct().ToList();
            foreach (var item in lowList)
            {
                int maxRange = MaxCurrent(item.RangeCurrArray);
                int scale = GetCurFactor(maxRange);
                var chlList = startlist.Where(c => c.EdgeGuid == item.EdgeGuid && c.dev_uid == item.dev_uid && c.LowGuid == item.LowGuid).OrderBy(c => c.bts_id).ToList();
                if (chlList.Count == 1)
                {
                    DeviceTestResponse deviceTest = new DeviceTestResponse();
                    deviceTest.GeneratorCode = item.GeneratorCode;
                    deviceTest.EdgeGuid = item.EdgeGuid;
                    deviceTest.SrvGuid = item.SrvGuid;
                    deviceTest.BtsServerIp = item.BtsServerIp;
                    deviceTest.MidGuid = item.MidGuid;
                    deviceTest.LowGuid = item.LowGuid;
                    deviceTest.canTestDeviceResp = new CanTestDeviceResp();
                    deviceTest.canTestDeviceResp.edge_guid = item.EdgeGuid;
                    deviceTest.canTestDeviceResp.control = new control();
                    deviceTest.canTestDeviceResp.control.arg = "";
                    deviceTest.canTestDeviceResp.control.cmd_type = "start_test";
                    deviceTest.Department = department;
                    deviceTest.MaxRange = maxRange;
                    arg arg = new arg();
                    arg.srv_guid = item.SrvGuid;
                    arg.ip = item.BtsServerIp;
                    arg.dev_uid = item.dev_uid;
                    arg.batch_no = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                    arg.test_name = "";
                    arg.creator = loginContext.User.Name;
                    arg.step_file_name = "";
                    arg.start_step = 1;
                    arg.scale = scale;
                    arg.battery_mass = 0;
                    arg.desc = "";
                    arg.chl = new List<chl>();
                    if (FirstStart == 1)
                    {
                        deviceTest.stepCount = stepCount2;
                        arg.step_data = step_data2;
                    }
                    else
                    {
                        deviceTest.stepCount = stepCount;
                        arg.step_data = step_data;
                    }
                    foreach (var citem in chlList)
                    {
                        chl chl = new chl();
                        chl.chl_id = citem.bts_id;
                        chl.dev_uid = item.dev_uid;
                        chl.unit_id = item.unit_id;
                        chl.barcode = "";
                        chl.battery_mass = 0;
                        chl.desc = "";
                        arg.chl.Add(chl);
                    }
                    deviceTest.canTestDeviceResp.control.arg = JsonConvert.SerializeObject(arg);
                    list.Add(deviceTest);
                }
                else
                {
                    DeviceTestResponse deviceTest1 = new DeviceTestResponse();
                    deviceTest1.GeneratorCode = item.GeneratorCode;
                    deviceTest1.EdgeGuid = item.EdgeGuid;
                    deviceTest1.SrvGuid = item.SrvGuid;
                    deviceTest1.BtsServerIp = item.BtsServerIp;
                    deviceTest1.MidGuid = item.MidGuid;
                    deviceTest1.LowGuid = item.LowGuid;
                    deviceTest1.canTestDeviceResp = new CanTestDeviceResp();
                    deviceTest1.canTestDeviceResp.edge_guid = item.EdgeGuid;
                    deviceTest1.canTestDeviceResp.control = new control();
                    deviceTest1.canTestDeviceResp.control.arg = "";
                    deviceTest1.canTestDeviceResp.control.cmd_type = "start_test";
                    deviceTest1.Department = department;
                    deviceTest1.MaxRange = maxRange;
                    arg arg1 = new arg();
                    arg1.srv_guid = item.SrvGuid;
                    arg1.ip = item.BtsServerIp;
                    arg1.dev_uid = item.dev_uid;
                    arg1.batch_no = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                    arg1.test_name = "";
                    arg1.creator = loginContext.User.Name;
                    arg1.step_file_name = "";
                    arg1.start_step = 1;
                    arg1.scale = scale;
                    arg1.battery_mass = 0;
                    arg1.desc = "";
                    arg1.chl = new List<chl>();

                    DeviceTestResponse deviceTest2 = new DeviceTestResponse();
                    deviceTest2.GeneratorCode = item.GeneratorCode;
                    deviceTest2.EdgeGuid = item.EdgeGuid;
                    deviceTest2.SrvGuid = item.SrvGuid;
                    deviceTest2.BtsServerIp = item.BtsServerIp;
                    deviceTest2.MidGuid = item.MidGuid;
                    deviceTest2.LowGuid = item.LowGuid;
                    deviceTest2.canTestDeviceResp = new CanTestDeviceResp();
                    deviceTest2.canTestDeviceResp.edge_guid = item.EdgeGuid;
                    deviceTest2.canTestDeviceResp.control = new control();
                    deviceTest2.canTestDeviceResp.control.arg = "";
                    deviceTest2.canTestDeviceResp.control.cmd_type = "start_test";
                    deviceTest2.Department = department;
                    deviceTest2.MaxRange = maxRange;
                    arg arg2 = new arg();
                    arg2.srv_guid = item.SrvGuid;
                    arg2.ip = item.BtsServerIp;
                    arg2.dev_uid = item.dev_uid;
                    arg2.batch_no = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                    arg2.test_name = "";
                    arg2.creator = loginContext.User.Name;
                    arg2.step_file_name = "";
                    arg2.start_step = 1;
                    arg2.scale = scale;
                    arg2.battery_mass = 0;
                    arg2.desc = "";
                    arg2.chl = new List<chl>();

                    if (FirstStart == 1)
                    {
                        deviceTest1.stepCount = stepCount;
                        arg1.step_data = step_data;
                        deviceTest2.stepCount = stepCount2;
                        arg2.step_data = step_data2;
                    }
                    else
                    {
                        deviceTest1.stepCount = stepCount2;
                        arg1.step_data = step_data2;
                        deviceTest2.stepCount = stepCount;
                        arg2.step_data = step_data;
                    }
                    if (TestType == 2)
                    {
                        var channelLimit = chlList.Count / 2;
                        foreach (var row in chlList)
                        {
                            chl chl = new chl();
                            chl.chl_id = row.bts_id;
                            chl.dev_uid = item.dev_uid;
                            chl.unit_id = item.unit_id;
                            chl.barcode = "";
                            chl.battery_mass = 0;
                            chl.desc = "";
                            if (row.bts_id <= channelLimit)
                            {
                                arg1.chl.Add(chl);
                            }
                            else
                            {
                                arg2.chl.Add(chl);
                            }
                        }
                    }
                    else
                    {
                        foreach (var row in chlList)
                        {
                            chl chl = new chl();
                            chl.chl_id = row.bts_id;
                            chl.dev_uid = item.dev_uid;
                            chl.unit_id = item.unit_id;
                            chl.barcode = "";
                            chl.battery_mass = 0;
                            chl.desc = "";
                            if (row.bts_id % 2 > 0)
                            {
                                arg1.chl.Add(chl);
                            }
                            else
                            {
                                arg2.chl.Add(chl);
                            }
                        }
                    }
                    deviceTest1.canTestDeviceResp.control.arg = JsonConvert.SerializeObject(arg1);
                    list.Add(deviceTest1);
                    deviceTest2.canTestDeviceResp.control.arg = JsonConvert.SerializeObject(arg2);
                    list.Add(deviceTest2);
                }
            }
            result.Data = list;
            return result;
        }


        /// <summary>
        /// 烤机启动测试
        /// </summary>
        /// <param name="startlist"></param>
        /// <param name="stepCount"></param>
        /// <param name="step_data"></param>
        /// <returns></returns>
        public TableData<List<DeviceTestResponse>> NormalStart(List<StartDeviceListResp> startlist, int stepCount, string step_data)
        {
            var result = new TableData<List<DeviceTestResponse>>();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                result.Code = Define.INVALID_TOKEN;
                result.Message = $"登录已过期!";
                return result;
            }
            var department = loginContext.Orgs.Select(c => c.Name).FirstOrDefault();
            List<DeviceTestResponse> list = new List<DeviceTestResponse>();
            var lowList = startlist.Select(c => new { c.GeneratorCode, c.EdgeGuid, c.SrvGuid, c.BtsServerIp, c.MidGuid, c.dev_uid, c.LowGuid, c.RangeCurrArray, c.unit_id }).Distinct().ToList();
            foreach (var item in lowList)
            {
                DeviceTestResponse deviceTest = new DeviceTestResponse();
                deviceTest.GeneratorCode = item.GeneratorCode;
                deviceTest.EdgeGuid = item.EdgeGuid;
                deviceTest.SrvGuid = item.SrvGuid;
                deviceTest.BtsServerIp = item.BtsServerIp;
                deviceTest.MidGuid = item.MidGuid;
                deviceTest.LowGuid = item.LowGuid;
                deviceTest.canTestDeviceResp = new CanTestDeviceResp();
                deviceTest.canTestDeviceResp.edge_guid = item.EdgeGuid;
                deviceTest.canTestDeviceResp.control = new control();
                deviceTest.canTestDeviceResp.control.arg = "";
                deviceTest.canTestDeviceResp.control.cmd_type = "start_test";
                deviceTest.Department = department;
                deviceTest.stepCount = stepCount;
                int maxRange = MaxCurrent(item.RangeCurrArray);
                deviceTest.MaxRange = maxRange;
                arg arg = new arg();
                arg.srv_guid = item.SrvGuid;
                arg.ip = item.BtsServerIp;
                arg.dev_uid = item.dev_uid;
                arg.batch_no = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                arg.test_name = "";
                arg.creator = loginContext.User.Name;
                arg.step_file_name = "";
                arg.start_step = 1;
                arg.scale = GetCurFactor(maxRange);
                arg.battery_mass = 0;
                arg.desc = "";
                arg.step_data = step_data;
                var chlList = startlist.Where(c => c.EdgeGuid == item.EdgeGuid && c.dev_uid == item.dev_uid && c.LowGuid == item.LowGuid).OrderBy(c => c.bts_id).ToList();
                arg.chl = new List<chl>();
                foreach (var citem in chlList)
                {
                    chl chl = new chl();
                    chl.chl_id = citem.bts_id;
                    chl.dev_uid = item.dev_uid;
                    chl.unit_id = item.unit_id;
                    chl.barcode = "";
                    chl.battery_mass = 0;
                    chl.desc = "";
                    arg.chl.Add(chl);
                }
                deviceTest.canTestDeviceResp.control.arg = JsonConvert.SerializeObject(arg);
                list.Add(deviceTest);
            }
            result.Data = list;
            return result;
        }

        /// <summary>
        /// 量程系数计算
        /// </summary>
        /// <param name="rng_cur"></param>
        /// <returns></returns>
        public int GetCurFactor(int rng_cur)
        {
            //电流量程范围mA
            long CUR_SCALE_10 = 10;
            long CUR_SCALE_100 = 100;
            long CUR_SCALE_1000 = 1000;

            //	电流单位
            double CUR_SCALE_FACTOR_10 = 10000.0;
            double CUR_SCALE_FACTOR_100 = 1000.0;
            double CUR_SCALE_FACTOR_1000 = 100.0;
            double CUR_SCALE_FACTOR_MAX = 10.0;
            double factor = CUR_SCALE_FACTOR_MAX;
            if (rng_cur >= 0)
            {
                if (rng_cur < CUR_SCALE_10)
                {
                    factor = CUR_SCALE_FACTOR_10;
                }
                else if (rng_cur < CUR_SCALE_100)
                {
                    factor = CUR_SCALE_FACTOR_100;
                }
                else if (rng_cur < CUR_SCALE_1000)
                {
                    factor = CUR_SCALE_FACTOR_1000;
                }
                else
                {
                    factor = CUR_SCALE_FACTOR_MAX;
                }

                return Convert.ToInt32(factor);
            }
            else
            {
                double d_rng_cur = 0;//单位:mA
                rng_cur = Math.Abs(rng_cur);
                if (rng_cur > 0 && rng_cur < 999)
                {//为负数,并且是值范围在1-999时单位为mA
                    d_rng_cur = rng_cur;
                }
                else if (rng_cur >= 1000 && rng_cur <= 999999)
                {//为负数,并且是值范围在1000-999999时单位为mA
                    d_rng_cur = rng_cur;
                }
                else if (rng_cur >= 1000000 && rng_cur <= 999999999)
                {//为负数,并且是值范围在1000000-999999999时为1到999.999999uA
                    d_rng_cur = rng_cur / 1000000000.0;
                }
                if (d_rng_cur < 0.01)
                {//10uA
                    factor = 100000000.0;
                }
                else if (d_rng_cur < 0.1)
                {//100uA
                    factor = 10000000.0;
                }
                else if (d_rng_cur < 1)
                {//1000uA
                    factor = 1000000.0;
                }
                else if (d_rng_cur < 10)
                {//10mA
                    factor = 100000.0;
                }
                else if (d_rng_cur < 100)
                {//100mA
                    factor = 10000.0;
                }
                else if (d_rng_cur < 1000)
                {//1000mA
                    factor = 1000.0;
                }
                else
                {
                    factor = 100.0;
                }
                return Convert.ToInt32(factor);
            }
        }

        /// <summary>
        /// 提取最大量程
        /// </summary>
        /// <param name="rangeCurrArray"></param>
        /// <returns></returns>
        public int MaxCurrent(string rangeCurrArray)
        {
            var list = rangeCurrArray.Split(',');
            Dictionary<int, int> dic = new Dictionary<int, int>();
            foreach (var item in list)
            {
                if (!string.IsNullOrWhiteSpace(item))
                {
                    var m = Convert.ToInt32(item);
                    var k = Math.Abs(m);
                    if (m <= 0)
                    {
                        if (1 <= k && k <= 999)
                        {
                            dic.Add(m, k);
                        }
                        else if (1000 <= k && k <= 999999)
                        {
                            var current = k ;
                            dic.Add(m, current);
                        }
                        else if (1000000 <= k && k <= 999999999)
                        {
                            var current = k / 1000000000;
                            dic.Add(m, current);
                        }
                    }
                    else
                    {
                        dic.Add(m, k);
                    }
                }
            }
            return dic.OrderByDescending(c => c.Value).Select(c => c.Key).FirstOrDefault();
        }
        #endregion
    }
}