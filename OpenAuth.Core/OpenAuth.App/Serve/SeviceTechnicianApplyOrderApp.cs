using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using Z.EntityFramework.Plus;

namespace OpenAuth.App
{
    public class SeviceTechnicianApplyOrdersApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly ServiceOrderApp _serviceOrderApp;
        private readonly ServiceOrderLogApp _serviceOrderLogApp;
        public SeviceTechnicianApplyOrdersApp(IUnitWork unitWork, RevelanceManagerApp app, ServiceOrderApp serviceOrderApp, ServiceOrderLogApp serviceOrderLogApp, IAuth auth) : base(unitWork, auth)
        {
            _serviceOrderApp = serviceOrderApp;
            _revelanceApp = app;
            _serviceOrderLogApp = serviceOrderLogApp;
        }

        /// <summary>
        /// 提交错误(新)设备信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task ApplyNewOrErrorDevices(ApplyNewOrErrorDevicesReq request)
        {
            //获取当前设备类型服务信息
            var currentMaterialTypeInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == request.AppUserId && s.ServiceOrderId == request.ServiceOrderId)
                .WhereIf("其他设备".Equals(request.MaterialType), a => a.MaterialCode == "其他设备")
                .WhereIf(!"其他设备".Equals(request.MaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == request.MaterialType)
                .FirstOrDefaultAsync();
            //发送消息至聊天室
            string head = "技术员核对设备有误提交给呼叫中心的信息";
            string Content = string.Empty;
            if (request.Devices != null && request.Devices.Count > 0)
            {
                foreach (Device item in request.Devices)
                {
                    SeviceTechnicianApplyOrder obj = new SeviceTechnicianApplyOrder();
                    obj.MaterialType = request.MaterialType;
                    obj.ManufSN = item.newNumber;
                    obj.ItemCode = item.newCode;
                    obj.ServiceOrderId = request.ServiceOrderId;
                    obj.OrginalManufSN = item.manufacturerSerialNumber;
                    obj.TechnicianId = request.AppUserId;
                    obj.CreateTime = DateTime.Now;
                    obj.IsSolved = 0;
                    obj.OrginalWorkOrderId = item.workOrderId;
                    if (request.MaterialType.Equals((("其他设备".Equals(item.newCode)) ? "其他设备" : item.newCode.Substring(0, item.newCode.IndexOf("-"))), StringComparison.OrdinalIgnoreCase))
                    {
                        obj.Status = currentMaterialTypeInfo.Status;
                        obj.OrderTakeType = currentMaterialTypeInfo.OrderTakeType;
                        obj.FromTheme = currentMaterialTypeInfo.FromTheme;
                        obj.FromType = currentMaterialTypeInfo.FromType;
                        obj.ProblemTypeId = currentMaterialTypeInfo.ProblemTypeId;
                        obj.CurrentUser = currentMaterialTypeInfo.CurrentUser;
                        obj.CurrentUserNsapId = currentMaterialTypeInfo.CurrentUserNsapId;
                    }
                    else
                    {
                        obj.Status = 0;
                        obj.OrderTakeType = 0;
                    }
                    await UnitWork.AddAsync(obj);
                    await UnitWork.SaveAsync();
                    Content += $"<br>待编辑序列号: {item.manufacturerSerialNumber}<br>正确的序列号: {item.newNumber}<br>正确的物料编码: {item.newCode}<br>";
                }
                await _serviceOrderApp.SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = request.ServiceOrderId, Content = head + Content, AppUserId = request.AppUserId });
            }
            //技术员新添加设备集合 发送消息
            if (request.NewDevices != null && request.NewDevices.Count > 0)
            {
                head = "请呼叫中心核对客户新设备信息";
                foreach (NewDevice item in request.NewDevices)
                {
                    SeviceTechnicianApplyOrder obj = new SeviceTechnicianApplyOrder();
                    obj.MaterialType = request.MaterialType;
                    obj.ManufSN = item.manufacturerSerialNumber;
                    obj.ItemCode = item.ItemCode;
                    obj.ServiceOrderId = request.ServiceOrderId;
                    obj.TechnicianId = request.AppUserId;
                    obj.CreateTime = DateTime.Now;
                    obj.IsSolved = 0;
                    if (request.MaterialType.Equals(("其他设备".Equals(item.ItemCode) ? "其他设备" : item.ItemCode.Substring(0, item.ItemCode.IndexOf("-"))), StringComparison.OrdinalIgnoreCase))
                    {
                        obj.Status = currentMaterialTypeInfo.Status;
                        obj.OrderTakeType = currentMaterialTypeInfo.OrderTakeType;
                        obj.FromTheme = currentMaterialTypeInfo.FromTheme;
                        obj.FromType = currentMaterialTypeInfo.FromType;
                        obj.ProblemTypeId = currentMaterialTypeInfo.ProblemTypeId;
                        obj.CurrentUser = currentMaterialTypeInfo.CurrentUser;
                        obj.CurrentUserNsapId = currentMaterialTypeInfo.CurrentUserNsapId;
                    }
                    else
                    {
                        obj.Status = 0;
                        obj.OrderTakeType = 0;
                    }
                    var o = await UnitWork.AddAsync<SeviceTechnicianApplyOrder, int>(obj);
                    await UnitWork.SaveAsync();
                    Content += $"<br>序列号: {item.manufacturerSerialNumber}<br>物料编码: {item.ItemCode}<br>";
                }
                await _serviceOrderApp.SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = request.ServiceOrderId, Content = head + Content, AppUserId = request.AppUserId });
            }
        }

        /// <summary>
        /// 获取技术员提交/修改的设备信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> GetApplyDevices(GetApplyDevicesReq req)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var queryOrder = UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == req.TechnicianId && s.ServiceOrderId == req.ServiceOrderId).Select(o => new
            {
                o.MaterialCode,
                o.ManufacturerSerialNumber,
                MaterialType = "其他设备".Equals(req.MaterialType) ? "其他设备" : o.MaterialCode.Substring(0, o.MaterialCode.IndexOf("-")),
                o.Status,
                o.Id,
                o.IsCheck,
                o.OrderTakeType
            });
            var checkData = await queryOrder.Where(s => req.MaterialType == "其他设备" ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == req.MaterialType).OrderByDescending(o => o.Id).ToListAsync();
            data.Add("checkData", checkData);
            var query = UnitWork.Find<SeviceTechnicianApplyOrder>(s => s.TechnicianId == req.TechnicianId && s.ServiceOrderId == req.ServiceOrderId && s.MaterialType == req.MaterialType && s.IsSolved == 0);
            var newData = await query.OrderByDescending(o => o.CreateTime).Select(s => new
            {
                s.OrginalWorkOrderId,
                s.OrginalManufSN,
                s.ManufSN,
                s.ItemCode,
                IsNew = s.OrginalWorkOrderId > 0 ? 1 : 0,
                s.IsSolved,
                s.Id
            }).ToListAsync();
            data.Add("newData", newData);
            result.Data = data;
            return result;
        }

        /// <summary>
        /// 处理技术员提交的错误/新设备
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task SolveTechApplyDevices(SolveTechApplyDevicesReq req)
        {
            //获取要处理的设备信息（技术员提交）
            var ApplyInfo = await UnitWork.Find<SeviceTechnicianApplyOrder>(s => s.Id == req.ApplyId).FirstOrDefaultAsync();
            //获取当前的服务单设备类型集合
            var MaterialTypes = (await UnitWork.Find<ServiceWorkOrder>(a => a.ServiceOrderId == ApplyInfo.ServiceOrderId).ToListAsync()).GroupBy(g => "其他设备".Equals(g.MaterialCode) ? "其他设备" : g.MaterialCode.Substring(0, g.MaterialCode.IndexOf("-"))).Select(s => s.Key).ToArray();
            switch (req.SolveType)
            {
                //修改
                case 1:
                    await EditDevice(ApplyInfo, MaterialTypes);
                    break;
                //新增
                case 2:
                    await AddDeviceAsync(req.addServiceWorkOrder, MaterialTypes, ApplyInfo);
                    break;
                //不处理
                default:
                    break;
            }
            //更新处理结果
            await UnitWork.UpdateAsync<SeviceTechnicianApplyOrder>(s => s.Id == req.ApplyId, u => new SeviceTechnicianApplyOrder { SolvedResult = req.SolveType, IsSolved = 1, SolvedTime = DateTime.Now });
        }

        //新增工单
        private async Task AddDeviceAsync(AddServiceWorkOrderReq request, string[] MaterialTypes, SeviceTechnicianApplyOrder ApplyInfo)
        {
            //在这个服务单下新建一个工单
            await _serviceOrderApp.AddWorkOrder(request);
            //判断当前设备的设备类型是否已存在服务单中
            var materialType = "其他设备".Equals(request.MaterialCode) ? "其他设备" : request.MaterialCode.Substring(0, request.MaterialCode.IndexOf("-"));
            //如果已经存在则将新建的工单派给这个设备类型的技术员
            if (MaterialTypes.Contains(materialType))
            {
                //派单给该技术员
                await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.MaterialCode == request.MaterialCode, a => new ServiceWorkOrder { CurrentUserId = ApplyInfo.TechnicianId, Status = ApplyInfo.Status, OrderTakeType = (int)ApplyInfo.OrderTakeType, CurrentUserNsapId = ApplyInfo.CurrentUserNsapId, CurrentUser = ApplyInfo.CurrentUser });
                await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"系统派单给技术员{ApplyInfo.CurrentUser}派单", ActionType = "系统派单工单" });
                await _serviceOrderApp.PushMessageToApp((int)ApplyInfo.TechnicianId, "派单成功提醒", "您已被派有一个新的售后服务，请尽快处理");
            }
        }

        /// <summary>
        /// 修改工单（先删后增）
        /// </summary>
        private async Task EditDevice(SeviceTechnicianApplyOrder ApplyInfo, string[] MaterialTypes)
        {
            //获取旧工单数据
            var OrginalWorkOrderInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.Id == ApplyInfo.OrginalWorkOrderId).FirstOrDefaultAsync();
            AddServiceWorkOrderReq request = new AddServiceWorkOrderReq
            {
                Priority = OrginalWorkOrderInfo.Priority,
                FeeType = OrginalWorkOrderInfo.FeeType,
                FromTheme = OrginalWorkOrderInfo.FromTheme,
                FromType = OrginalWorkOrderInfo.FromType,
                ProblemTypeId = OrginalWorkOrderInfo.ProblemTypeId,
                MaterialCode = ApplyInfo.ItemCode,
                ManufacturerSerialNumber = ApplyInfo.ManufSN,
                ContractId = OrginalWorkOrderInfo.ContractId,
                BookingDate = OrginalWorkOrderInfo.BookingDate,
                ServiceOrderId = OrginalWorkOrderInfo.ServiceOrderId
            };
            //新增工单
            await AddDeviceAsync(request, MaterialTypes, ApplyInfo);
            //删除旧工单
            await _serviceOrderApp.DeleteWorkOrder((int)ApplyInfo.OrginalWorkOrderId);
        }
    }
}