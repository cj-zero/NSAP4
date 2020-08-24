using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using NetOffice.WordApi;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class SeviceTechnicianApplyOrdersApp : OnlyUnitWorkBaeApp
    {
        private readonly RevelanceManagerApp _revelanceApp;
        private readonly ServiceOrderApp _serviceOrderApp;
        public SeviceTechnicianApplyOrdersApp(IUnitWork unitWork, RevelanceManagerApp app, ServiceOrderApp serviceOrderApp, IAuth auth) : base(unitWork, auth)
        {
            _serviceOrderApp = serviceOrderApp;
            _revelanceApp = app;
        }

        /// <summary>
        /// 提交错误(新)设备信息
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task ApplyNewOrErrorDevices(ApplyNewOrErrorDevicesReq request)
        {
            //获取当前设备类型服务信息
            var currentMaterialTypeInfo = (await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == request.AppUserId && s.ServiceOrderId == request.ServiceOrderId).ToListAsync()).Where(s => "其他设备".Equals(request.MaterialType) ? s.MaterialCode == "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) == request.MaterialType).FirstOrDefault();
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
                    if (request.MaterialType.Equals((string.IsNullOrEmpty(item.newCode) ? "其他设备" : item.newCode.Substring(0, item.newCode.IndexOf("-"))), StringComparison.OrdinalIgnoreCase))
                    {
                        obj.Status = currentMaterialTypeInfo.Status;
                        obj.OrderTakeType = currentMaterialTypeInfo.OrderTakeType;
                        obj.FromTheme = currentMaterialTypeInfo.FromTheme;
                        obj.FromType = currentMaterialTypeInfo.FromType;
                        obj.ProblemTypeId = currentMaterialTypeInfo.ProblemTypeId;
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
                    if (request.MaterialType.Equals((string.IsNullOrEmpty(item.ItemCode) ? "其他设备" : item.ItemCode.Substring(0, item.ItemCode.IndexOf("-"))), StringComparison.OrdinalIgnoreCase))
                    {
                        obj.Status = currentMaterialTypeInfo.Status;
                        obj.OrderTakeType = currentMaterialTypeInfo.OrderTakeType;
                        obj.FromTheme = currentMaterialTypeInfo.FromTheme;
                        obj.FromType = currentMaterialTypeInfo.FromType;
                        obj.ProblemTypeId = currentMaterialTypeInfo.ProblemTypeId;
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
                s.ManufSN,
                s.ItemCode
            }).ToListAsync();
            data.Add("newData", newData);
            result.Data = data;
            return result;
        }
    }
}