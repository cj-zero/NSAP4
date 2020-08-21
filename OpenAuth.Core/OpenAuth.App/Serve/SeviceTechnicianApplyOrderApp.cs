using System;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
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
        public async Task ApplyNewOrErrorDevices(ApplyNewOrErrorDevicesReq request)
        {
            //获取当前设备类型服务信息
            var currentMaterialTypeInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == request.AppUserId && s.ServiceOrderId == request.ServiceOrderId && (string.IsNullOrEmpty(s.MaterialCode) ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-"))) == request.MaterialType).FirstOrDefaultAsync();
            //发送消息至聊天室
            string head = "技术员核对设备有误提交给呼叫中心的信息";
            string Content = string.Empty;
            if (request.Devices != null && request.Devices.Count > 0)
            {
                foreach (Device item in request.Devices)
                {
                    SeviceTechnicianApplyOrder obj = new SeviceTechnicianApplyOrder();
                    obj.MaterialType = item.newNumber.Substring(0, item.newNumber.IndexOf("-"));
                    obj.ManufSN = item.newNumber;
                    obj.ItemCode = item.newCode;
                    obj.ServiceOrderId = request.ServiceOrderId;
                    obj.OrginalManufSN = item.manufacturerSerialNumber;
                    obj.TechnicianId = request.AppUserId;
                    obj.CreateTime = DateTime.Now;
                    if (request.MaterialType.Equals(obj.MaterialType, StringComparison.OrdinalIgnoreCase))
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
                    obj.MaterialType = item.manufacturerSerialNumber.Substring(0, item.manufacturerSerialNumber.IndexOf("-"));
                    obj.ManufSN = item.manufacturerSerialNumber;
                    obj.ItemCode = item.ItemCode;
                    obj.ServiceOrderId = request.ServiceOrderId;
                    obj.TechnicianId = request.AppUserId;
                    obj.CreateTime = DateTime.Now;
                    if (request.MaterialType.Equals(obj.MaterialType, StringComparison.OrdinalIgnoreCase))
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
    }
}