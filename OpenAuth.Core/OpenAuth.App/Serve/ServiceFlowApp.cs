using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    public class ServiceFlowApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;

        public ServiceFlowApp(IUnitWork unitWork,
            RevelanceManagerApp app, IAuth auth) : base(unitWork, auth)
        {
            _revelanceApp = app;
        }

        /// <summary>
        /// 添加流程
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task AddOrUpdateServerFlow(AddOrUpdateServerFlowReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            //获取当前用户nsap用户信息
            var userInfo = await UnitWork.Find<AppUserMap>(a => a.AppUserId == request.AppUserId).Include(a => a.User).FirstOrDefaultAsync();
            if (userInfo.User == null)
            {
                throw new CommonException("未绑定App账户", Define.INVALID_APPUser);
            }
            switch (request.FlowNum)
            {
                case 1://选择电话服务
                    //列表
                    await RemoteServer(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name, 1);
                    //详情
                    await RemoteServer(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name, 2);
                    break;
                case 2://选择上门服务或电话服务转上门服务
                    //列表
                    await DoorServer(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name, 1);
                    //详情
                    await DoorServer(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name, 2);
                    break;
                case 3://拨打电话之后
                case 5://跳转问题类型页面
                case 6://跳转解决方案页面
                    await UnitWork.UpdateAsync<ServiceFlow>(s => s.ServiceOrderId == request.ServiceOrderId && s.MaterialType == request.MaterialType && s.Creater == userInfo.User.Id && s.FlowNum == 2, o => new ServiceFlow { IsProceed = 1 });
                    break;
                case 4://跳转核对设备页面
                    await UnitWork.UpdateAsync<ServiceFlow>(s => s.ServiceOrderId == request.ServiceOrderId && s.MaterialType == request.MaterialType && s.Creater == userInfo.User.Id && s.FlowNum == 5, o => new ServiceFlow { IsProceed = 1 });
                    break;
                case 7://跳转服务报告单页面
                    await UnitWork.UpdateAsync<ServiceFlow>(s => s.ServiceOrderId == request.ServiceOrderId && s.MaterialType == request.MaterialType && s.Creater == userInfo.User.Id && s.FlowNum == 3, o => new ServiceFlow { IsProceed = 1 });
                    break;
                //case 8://跳转是否领料页面
                //    await UnitWork.UpdateAsync<ServiceFlow>(s => s.ServiceOrderId == request.ServiceOrderId && s.MaterialType == request.MaterialType && s.Creater == userInfo.Id && s.FlowNum == 6, o => new ServiceFlow { IsProceed = 1 });
                //    break;
                case 9://跳转领料页面
                    await GetMaterial(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name, 1);
                    await GetMaterial(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name, 2);
                    break;
                case 11://跳转退料页面
                    await UnitWork.UpdateAsync<ServiceFlow>(s => s.ServiceOrderId == request.ServiceOrderId && s.MaterialType == request.MaterialType && s.Creater == userInfo.User.Id && s.FlowNum == 8, o => new ServiceFlow { IsProceed = 1 });
                    await ReturnMaterial(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name);
                    break;
                case -1://跳转返厂页面
                    await ReturnServer(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name, 1);
                    await ReturnServer(request.ServiceOrderId, request.MaterialType, userInfo.User.Id, userInfo.User.Name, 2);
                    break;
                case -2://跳转返厂进度页面
                    await UnitWork.UpdateAsync<ServiceFlow>(s => s.ServiceOrderId == request.ServiceOrderId && s.MaterialType == request.MaterialType && s.Creater == userInfo.User.Id && s.FlowNum == 10, o => new ServiceFlow { IsProceed = 1 });
                    break;
                case 10:
                default:
                    break;
            }
            await UnitWork.SaveAsync();
        }


        /// <summary>
        /// 远程服务
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <param name="creater"></param>
        /// <param name="createname"></param>
        /// <param name="flowType"></param>
        /// <returns></returns>
        private async Task<bool> RemoteServer(int serviceOrderId, string MaterialType, string creater, string createname, int flowType)
        {
            //添加后续默认流程
            List<int> flowNums = new List<int> { 1, 2, 3 };
            List<ServiceFlow> flows = new List<ServiceFlow>();
            foreach (var item in flowNums)
            {
                var flow = new ServiceFlow { ServiceOrderId = serviceOrderId, MaterialType = MaterialType, Creater = creater, CreateTime = DateTime.Now, IsProceed = 0, CreaterName = createname, FlowNum = item, FlowName = GetFlowName(item), FlowType = flowType };
                if (item == 1)
                {
                    flow.IsProceed = 1;
                }
                flows.Add(flow);
            }
            await UnitWork.BatchAddAsync(flows.ToArray());
            return true;
        }

        /// <summary>
        /// 上门服务
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <param name="creater"></param>
        /// <param name="createname"></param>
        /// <param name="flowType"></param>
        /// <returns></returns>
        private async Task<bool> DoorServer(int serviceOrderId, string MaterialType, string creater, string createname, int flowType)
        {
            //清除之前的流程 如先进行了电话服务 后选择了转上门服务
            await UnitWork.DeleteAsync<ServiceFlow>(s => s.ServiceOrderId == serviceOrderId && s.MaterialType.Equals(MaterialType) && s.Creater.Equals(creater) && s.FlowType == flowType);
            await UnitWork.SaveAsync();
            //添加后续默认流程
            List<int> flowNums = new List<int> { 4, 5, 3 };
            List<ServiceFlow> flows = new List<ServiceFlow>();
            foreach (var item in flowNums)
            {
                var flow = new ServiceFlow { ServiceOrderId = serviceOrderId, MaterialType = MaterialType, Creater = creater, CreateTime = DateTime.Now, IsProceed = 0, CreaterName = createname, FlowNum = item, FlowName = GetFlowName(item), FlowType = flowType };
                if (item == 4)
                {
                    flow.IsProceed = 1;
                }
                flows.Add(flow);
            }
            await UnitWork.BatchAddAsync(flows.ToArray());
            return true;
        }

        /// <summary>
        /// 领料流程
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <param name="creater"></param>
        /// <param name="createname"></param>
        /// <param name="flowType"></param>
        /// <returns></returns>
        private async Task<bool> GetMaterial(int serviceOrderId, string MaterialType, string creater, string createname, int flowType)
        {
            List<ServiceFlow> flows = new List<ServiceFlow>();
            List<int> flowNums = new List<int> { };
            //详情
            if (flowType == 2)
            {
                //清除填报告单流程
                await UnitWork.DeleteAsync<ServiceFlow>(s => s.ServiceOrderId == serviceOrderId && s.MaterialType.Equals(MaterialType) && s.Creater.Equals(creater) && s.FlowNum == 3 && s.FlowType == flowType);
                await UnitWork.SaveAsync();
                flowNums = new List<int> { 7, 8, 3 };
            }
            //列表
            else if (flowType == 1)
            {
                //清除填报告单流程与是否领料
                await UnitWork.DeleteAsync<ServiceFlow>(s => s.ServiceOrderId == serviceOrderId && s.MaterialType.Equals(MaterialType) && s.Creater.Equals(creater) && s.FlowNum == 3 && s.FlowType == flowType);
                await UnitWork.SaveAsync();
                flowNums = new List<int> { 7, 3 };
            }
            foreach (var item in flowNums)
            {
                var flow = new ServiceFlow { ServiceOrderId = serviceOrderId, MaterialType = MaterialType, Creater = creater, CreateTime = DateTime.Now, IsProceed = 0, CreaterName = createname, FlowNum = item, FlowName = GetFlowName(item), FlowType = flowType };
                if (item == 7)
                {
                    flow.IsProceed = 1;
                }
                flows.Add(flow);
            }
            await UnitWork.BatchAddAsync(flows.ToArray());
            return true;
        }

        /// <summary>
        /// 返厂维修
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <param name="creater"></param>
        /// <param name="createname"></param>
        /// <param name="flowType"></param>
        /// <returns></returns>
        private async Task<bool> ReturnServer(int serviceOrderId, string MaterialType, string creater, string createname, int flowType)
        {
            List<ServiceFlow> flows = new List<ServiceFlow>();
            List<int> flowNums = new List<int> { 9, 10, 3 };
            if (flowType == 1)
            {
                //清除之前所有的流程
                await UnitWork.DeleteAsync<ServiceFlow>(s => s.ServiceOrderId == serviceOrderId && s.MaterialType.Equals(MaterialType) && s.Creater.Equals(creater) && s.FlowType == flowType);
                await UnitWork.SaveAsync();
            }
            else
            {
                //清除所有未进行的流程
                await UnitWork.DeleteAsync<ServiceFlow>(s => s.ServiceOrderId == serviceOrderId && s.MaterialType.Equals(MaterialType) && s.Creater.Equals(creater) && s.IsProceed == 0 && s.FlowType == flowType);
                await UnitWork.SaveAsync();
            }
            foreach (var item in flowNums)
            {
                var flow = new ServiceFlow { ServiceOrderId = serviceOrderId, MaterialType = MaterialType, Creater = creater, CreateTime = DateTime.Now, IsProceed = 0, CreaterName = createname, FlowNum = item, FlowName = GetFlowName(item), FlowType = flowType };
                if (item == 9)
                {
                    flow.IsProceed = 1;
                }
                flows.Add(flow);
            }
            await UnitWork.BatchAddAsync(flows.ToArray());
            return true;
        }

        /// <summary>
        /// 退料流程（列表）
        /// </summary>
        /// <param name="serviceOrderId"></param>
        /// <param name="MaterialType"></param>
        /// <param name="creater"></param>
        /// <param name="createname"></param>
        /// <returns></returns>
        private async Task<bool> ReturnMaterial(int serviceOrderId, string MaterialType, string creater, string createname)
        {
            //清除物流进度与填报告单流程
            await UnitWork.DeleteAsync<ServiceFlow>(s => s.ServiceOrderId == serviceOrderId && s.MaterialType.Equals(MaterialType) && s.Creater.Equals(creater) && (s.FlowNum == 3 || s.FlowNum == 7));
            await UnitWork.SaveAsync();
            //添加后续默认流程
            List<int> flowNums = new List<int> { 8, 3 };
            List<ServiceFlow> flows = new List<ServiceFlow>();
            foreach (var item in flowNums)
            {
                var flow = new ServiceFlow { ServiceOrderId = serviceOrderId, MaterialType = MaterialType, Creater = creater, CreateTime = DateTime.Now, IsProceed = 0, CreaterName = createname, FlowNum = item, FlowName = GetFlowName(item), FlowType = 1 };
                if (item == 8)
                {
                    flow.IsProceed = 1;
                }
                flows.Add(flow);
            }
            await UnitWork.BatchAddAsync(flows.ToArray());
            return true;
        }

        /// <summary>
        /// 获取流程名称
        /// </summary>
        /// <param name="num"></param>
        /// <returns></returns>
        private string GetFlowName(int num)
        {
            string name = string.Empty;
            switch (num)
            {
                case 1:
                    name = "拨打电话";
                    break;
                case 2:
                    name = "远程维护中";
                    break;
                case 3:
                    name = "填报告单";
                    break;
                case 4:
                    name = "预约时间";
                    break;
                case 5:
                    name = "核对设备";
                    break;
                //case 6:
                //    name = "是否领料";
                //    break;
                case 7:
                    name = "物料进度";
                    break;
                case 8:
                    name = "退料";
                    break;
                case 9:
                    name = "返厂维修";
                    break;
                case 10:
                    name = "维修进度";
                    break;
            }
            return name;
        }

    }
}