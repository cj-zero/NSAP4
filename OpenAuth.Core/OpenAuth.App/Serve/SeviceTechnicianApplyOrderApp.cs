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
            //获取当前服务单下的所有设备类型集合
            var materialType = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId).Select(s => new { materialType = "其他设备".Equals(s.MaterialCode) ? "其他设备" : s.MaterialCode.Substring(0, s.MaterialCode.IndexOf("-")) }).ToListAsync();
            var materialTypes = materialType.Select(s => s.materialType).Distinct().ToList();
            //发送消息至聊天室
            string head = "技术员核对设备有误提交给呼叫中心的信息";
            string Content = string.Empty;
            //技术员修改设备集合 发送消息
            if (request.Devices != null && request.Devices.Count > 0)
            {
                foreach (Device item in request.Devices)
                {
                    SeviceTechnicianApplyOrder obj = new SeviceTechnicianApplyOrder();
                    var editMaterialType = "其他设备".Equals(item.newCode) ? "其他设备" : item.newCode.Substring(0, item.newCode.IndexOf("-"));
                    //判断当前编辑的设备类型是否在服务单中 若存在则直接取该设备类型的相关操作信息 不存在则默认为未派单
                    if (materialTypes.Contains(editMaterialType))
                    {
                        obj.ManufSN = item.newNumber;
                        obj.ItemCode = item.newCode;
                        obj.WarrantyEndDate = item.dlvryDate;
                        obj.ContractId = item.ContractId;
                        obj.MaterialDescription = item.ItemName;
                        obj.InternalSerialNumber = item.InternalSN;
                        //判断是否存在已经提交的申请 若已存在则更新 不存在则新增
                        var IsExist = (await UnitWork.Find<SeviceTechnicianApplyOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.OrginalWorkOrderId == item.workOrderId).ToListAsync()).Count;
                        if (IsExist > 0)
                        {
                            //获取当前设备类型服务信息
                            var currentMaterialTypeInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == request.AppUserId && s.ServiceOrderId == request.ServiceOrderId)
                            .Include(s => s.ProblemType)
                            .WhereIf("其他设备".Equals(editMaterialType), a => a.MaterialCode == "其他设备")
                            .WhereIf(!"其他设备".Equals(editMaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == editMaterialType)
                            .FirstOrDefaultAsync();
                            obj.Status = currentMaterialTypeInfo.Status;
                            obj.OrderTakeType = currentMaterialTypeInfo.OrderTakeType;
                            obj.FromTheme = currentMaterialTypeInfo.FromTheme;
                            obj.FromType = currentMaterialTypeInfo.FromType;
                            obj.ProblemTypeId = currentMaterialTypeInfo.ProblemTypeId;
                            obj.ProblemTypeName = currentMaterialTypeInfo.ProblemType.Name;
                            obj.CurrentUser = currentMaterialTypeInfo.CurrentUser;
                            obj.CurrentUserNsapId = currentMaterialTypeInfo.CurrentUserNsapId;

                            await UnitWork.UpdateAsync<SeviceTechnicianApplyOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.OrginalWorkOrderId == item.workOrderId, u => new SeviceTechnicianApplyOrder
                            {
                                ManufSN = obj.ManufSN,
                                ItemCode = obj.ItemCode,
                                WarrantyEndDate = obj.WarrantyEndDate,
                                ContractId = obj.ContractId,
                                MaterialDescription = obj.MaterialDescription,
                                InternalSerialNumber = obj.InternalSerialNumber,
                                Status = obj.Status,
                                OrderTakeType = obj.OrderTakeType,
                                FromTheme = obj.FromTheme,
                                FromType = obj.FromType,
                                ProblemTypeId = obj.ProblemTypeId,
                                ProblemTypeName = obj.ProblemTypeName,
                                CurrentUser = obj.CurrentUser,
                                CurrentUserNsapId = obj.CurrentUserNsapId,
                                CreateTime = DateTime.Now
                            });
                            await UnitWork.SaveAsync();
                            Content += $"<br>待编辑序列号: {item.manufacturerSerialNumber}<br>正确的序列号: {item.newNumber}<br>正确的物料编码: {item.newCode}<br>";
                        }
                        else
                        {
                            obj.MaterialType = request.MaterialType;
                            obj.ManufSN = item.newNumber;
                            obj.ItemCode = item.newCode;
                            obj.ServiceOrderId = request.ServiceOrderId;
                            obj.OrginalManufSN = item.manufacturerSerialNumber;
                            obj.TechnicianId = request.AppUserId;
                            obj.CreateTime = DateTime.Now;
                            obj.IsSolved = 0;
                            obj.OrginalWorkOrderId = item.workOrderId;
                            obj.WarrantyEndDate = item.dlvryDate;
                            obj.ContractId = item.ContractId;
                            obj.MaterialDescription = item.ItemName;
                            obj.InternalSerialNumber = item.InternalSN;
                            if (materialTypes.Contains(editMaterialType))
                            {
                                //获取当前设备类型服务信息
                                var currentMaterialTypeInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == request.AppUserId && s.ServiceOrderId == request.ServiceOrderId)
                                    .Include(s => s.ProblemType)
                                    .WhereIf("其他设备".Equals(editMaterialType), a => a.MaterialCode == "其他设备")
                                    .WhereIf(!"其他设备".Equals(editMaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == editMaterialType)
                                    .FirstOrDefaultAsync();
                                obj.Status = currentMaterialTypeInfo.Status;
                                obj.OrderTakeType = currentMaterialTypeInfo.OrderTakeType;
                                obj.FromTheme = currentMaterialTypeInfo.FromTheme;
                                obj.FromType = currentMaterialTypeInfo.FromType;
                                obj.ProblemTypeId = currentMaterialTypeInfo.ProblemTypeId;
                                obj.ProblemTypeName = currentMaterialTypeInfo.ProblemType.Name;
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
                    }
                    else
                    {
                        obj.Status = 0;
                        obj.OrderTakeType = 0;
                    }
                }
                await _serviceOrderApp.SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = request.ServiceOrderId, Content = head + Content, AppUserId = request.AppUserId });
            }
            //技术员新添加设备集合 发送消息
            if (request.NewDevices != null && request.NewDevices.Count > 0)
            {
                head = "请呼叫中心核对客户新设备信息";
                foreach (NewDevice item in request.NewDevices)
                {
                    var newMaterialType = "其他设备".Equals(item.ItemCode) ? "其他设备" : item.ItemCode.Substring(0, item.ItemCode.IndexOf("-"));
                    //判断当前新增的设备类型是否在服务单中 若存在则直接取该设备类型的相关操作信息 不存在则默认为未派单
                    SeviceTechnicianApplyOrder obj = new SeviceTechnicianApplyOrder();
                    obj.MaterialType = request.MaterialType;
                    obj.ManufSN = item.manufacturerSerialNumber;
                    obj.ItemCode = item.ItemCode;
                    obj.ServiceOrderId = request.ServiceOrderId;
                    obj.TechnicianId = request.AppUserId;
                    obj.CreateTime = DateTime.Now;
                    obj.IsSolved = 0;
                    obj.WarrantyEndDate = item.dlvryDate;
                    obj.ContractId = item.ContractId;
                    obj.MaterialDescription = item.ItemName;
                    obj.InternalSerialNumber = item.InternalSN;
                    if (materialTypes.Contains(newMaterialType))
                    {
                        //获取当前设备类型服务信息
                        var currentMaterialTypeInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.CurrentUserId == request.AppUserId && s.ServiceOrderId == request.ServiceOrderId)
                            .Include(s => s.ProblemType)
                            .WhereIf("其他设备".Equals(newMaterialType), a => a.MaterialCode == "其他设备")
                            .WhereIf(!"其他设备".Equals(newMaterialType), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == newMaterialType)
                            .FirstOrDefaultAsync();
                        obj.Status = currentMaterialTypeInfo.Status;
                        obj.OrderTakeType = currentMaterialTypeInfo.OrderTakeType;
                        obj.FromTheme = currentMaterialTypeInfo.FromTheme;
                        obj.FromType = currentMaterialTypeInfo.FromType;
                        obj.ProblemTypeId = currentMaterialTypeInfo.ProblemTypeId;
                        obj.ProblemTypeName = currentMaterialTypeInfo.ProblemType.Name;
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
        /// 获取技术员提交/修改的设备信息(APP)
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
                IsNew = s.OrginalWorkOrderId > 0 ? 0 : 1,
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
            //判断是否生成了工单号
            var count = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == ApplyInfo.ServiceOrderId && string.IsNullOrEmpty(s.WorkOrderNumber)).ToListAsync()).Count;
            if (count > 0)
            {
                throw new CommonException("该服务单查询到无效的工单号，请确认", 09091);
            }
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
            //添加工单时先判断当前服务单下是否已存在该设备
            var IsExist = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.ManufacturerSerialNumber == request.ManufacturerSerialNumber && s.MaterialCode == request.MaterialCode).ToListAsync()).Count;
            if (IsExist > 0)
            {
                throw new CommonException("当前设备已存在,请勿重复添加", 50001);
            }
            //在这个服务单下新建一个工单
            await _serviceOrderApp.AddWorkOrder(request);
            //判断当前设备的设备类型是否已存在服务单中
            var materialType = "其他设备".Equals(request.MaterialCode) ? "其他设备" : request.MaterialCode.Substring(0, request.MaterialCode.IndexOf("-"));
            //获取当前设备类型是否已被某个技术员接单 有则查出该技术员
            var workOrderInfo = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId)
                .WhereIf(materialType.Equals("其他设备"), a => a.ManufacturerSerialNumber == "其他设备")
                .WhereIf(!materialType.Equals("其他设备"), b => b.MaterialCode.Substring(0, b.MaterialCode.IndexOf("-")) == materialType)
                .FirstOrDefaultAsync();
            var TechnicianId = workOrderInfo.CurrentUserId;
            //如果已经存在当前设备类型并且已经派给了某个技术员了则将新建的工单派给这个设备类型的技术员
            if (MaterialTypes.Contains(materialType) && TechnicianId > 0)
            {
                //派单给该技术员
                await UnitWork.UpdateAsync<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.MaterialCode == request.MaterialCode, a => new ServiceWorkOrder { CurrentUserId = TechnicianId, Status = workOrderInfo.Status, OrderTakeType = (int)workOrderInfo.OrderTakeType, CurrentUserNsapId = workOrderInfo.CurrentUserNsapId, CurrentUser = workOrderInfo.CurrentUser, ServiceMode = workOrderInfo.ServiceMode });
                await _serviceOrderLogApp.AddAsync(new AddOrUpdateServiceOrderLogReq { Action = $"系统派单给技术员{workOrderInfo.CurrentUser}", ActionType = "系统派单工单", ServiceOrderId = request.ServiceOrderId, MaterialType = materialType });
                //获取当前新增的工单号
                var workorderNum = (await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId == request.ServiceOrderId && s.ManufacturerSerialNumber == request.ManufacturerSerialNumber && s.MaterialCode == request.MaterialCode).FirstOrDefaultAsync()).WorkOrderNumber;
                //发送消息
                await _serviceOrderApp.SendServiceOrderMessage(new SendServiceOrderMessageReq { ServiceOrderId = request.ServiceOrderId, Content = $"系统自动派给技术员{workOrderInfo.CurrentUser}派单{workorderNum}", AppUserId = 0 });
                await _serviceOrderApp.PushMessageToApp((int)TechnicianId, "派单成功提醒", "您已被派有一个新的售后服务，请尽快处理");
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

        /// <summary>
        /// 获取技术员提交/修改的设备信息
        /// </summary>
        /// <param name="sapOrderId"></param>
        /// <returns></returns>
        public async Task<TableData> GetTechnicianApplyDevices(int sapOrderId)
        {
            var result = new TableData();
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var query = from a in UnitWork.Find<SeviceTechnicianApplyOrder>(null)
                        join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id into ab
                        from b in ab.DefaultIfEmpty()
                        select new { a, b };
            query = query.Where(q => q.b.U_SAP_ID == sapOrderId);
            var data = await query.OrderByDescending(o => o.a.CreateTime).Select(s => new
            {
                s.a.OrginalWorkOrderId,
                s.a.OrginalManufSN,
                s.a.ManufSN,
                s.a.ItemCode,
                IsNew = s.a.OrginalWorkOrderId > 0 ? 0 : 1,
                Status = s.a.IsSolved == 1 ? (s.a.SolvedResult > 0 ? 1 : 2) : 0,//0未处理 1已通过 2未通过
                s.a.Id,
                s.a.FromTheme,
                s.a.FromType,
                s.a.ProblemTypeId,
                s.a.SolvedResult,
                s.a.CurrentUser,
                s.a.ProblemTypeName,
                s.a.WarrantyEndDate,
                s.a.InternalSerialNumber,
                s.a.MaterialDescription,
                s.a.ContractId
            }).ToListAsync();
            result.Data = data;
            return result;
        }

    }
}