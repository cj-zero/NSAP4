using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Material.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 报价单操作
    /// </summary>
    public class QuotationApp : OnlyUnitWorkBaeApp
    {
        private readonly FlowInstanceApp _flowInstanceApp;

        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;

        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).WhereIf(!string.IsNullOrWhiteSpace(request.CardCode), q => q.CustomerId.Contains(request.CardCode) || q.CustomerName.Contains(request.CardCode)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).ToListAsync();
            var ServiceOrderids = ServiceOrders.Select(s => s.Id).ToList();
            var Quotations = UnitWork.Find<Quotation>(null).WhereIf(request.QuotationId.ToString() != null, q => q.Id.ToString().Contains(request.QuotationId.ToString()))
                                .WhereIf(request.ServiceOrderSapId != null, q => q.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.CreateUser.Contains(request.CreateUser))
                                .WhereIf(request.StartCreateTime != null, q => q.CreateTime > request.StartCreateTime)
                                .WhereIf(request.EndCreateTime != null, q => q.CreateTime < request.EndCreateTime)
                                .Where(q => ServiceOrderids.Contains(q.ServiceOrderId));


            #region 分页条件
            switch (request.StartType)
            {
                case 1://草稿箱
                    Quotations = Quotations.Where(q => q.IsDraft == true);
                    break;

                case 2://审批中
                    Quotations = Quotations.Where(q => q.QuotationStatus > 3);
                    break;

                case 3://已领料
                    Quotations = Quotations.Where(q => q.QuotationStatus == 8);
                    break;

                case 4://已驳回
                    Quotations = Quotations.Where(q => q.QuotationStatus == 1);
                    break;
                default:
                    break;
            }
            #endregion

            var QuotationDate = await Quotations.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();

            var query = from a in QuotationDate
                        join b in ServiceOrders on a.ServiceOrderId equals b.Id
                        select new { a, b };

            result.Data = query.Select(q => new
            {
                q.a.Id,
                q.a.ServiceOrderSapId,
                q.b.TerminalCustomer,
                q.b.TerminalCustomerId,
                q.a.TotalMoney,
                q.a.CreateUser,
                q.a.Reamrk,
                CreateTime = q.a.CreateTime.ToString("yyyy-MM-dd"),
                q.a.QuotationStatus
            }).ToList();
            result.Count = await Quotations.CountAsync();
            return result;
        }

        /// <summary>
        /// 加载服务单列表
        /// </summary>
        public async Task<TableData> GetServiceOrderList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }
            var result = new TableData();

            var ServiceOrders = from a in UnitWork.Find<ServiceOrder>(null)
                                join b in UnitWork.Find<ServiceWorkOrder>(null) on a.Id equals b.ServiceOrderId
                                select new { a, b };

            var ServiceOrderList = (await ServiceOrders.Where(s => s.b.CurrentUserNsapId.Equals(loginUser.Id)).ToListAsync()).GroupBy(s => s.a.Id).Select(s => s.First());
            result.Data = ServiceOrderList.Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(q => new
                {
                    q.a.Id,
                    q.a.U_SAP_ID,
                    q.a.TerminalCustomer,
                    q.a.TerminalCustomerId,
                    q.b.FromTheme,
                    q.a.SalesMan
                });
            result.Count = ServiceOrderList.Count();

            return result;
        }

        /// <summary>
        /// 获取序列号和设备
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetSerialNumberList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(request.AppId));
            }
            var result = new TableData();

            var ServiceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(request.ServiceOrderId) && s.CurrentUserNsapId.Equals(loginUser.Id) && !s.MaterialCode.Equals("其他设备")).WhereIf(!string.IsNullOrWhiteSpace(request.MaterialType),s=> s.MaterialCode.Substring(0, 2) == request.MaterialType).Select(s => new { s.ManufacturerSerialNumber, s.MaterialCode }).ToListAsync();
           
            result.Data = ServiceWorkOrderList.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();
            result.Count = ServiceWorkOrderList.Count();
            return result;
        }

        /// <summary>
        /// 获取物料列表
        /// </summary>
        public async Task<TableData> GetMaterialCodeList(QueryQuotationListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();

            #region 暂时废弃
            //var ServiceWorkOrderList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(request.ServiceOrderId)).Select(s => new { s.ManufacturerSerialNumber, s.MaterialCode }).ToListAsync();
            //string ManufacturerSerialNumbers = "";
            //ServiceWorkOrderList.ForEach(m => ManufacturerSerialNumbers += "'" + m.ManufacturerSerialNumber + "',");
            //ManufacturerSerialNumbers = ManufacturerSerialNumbers.Substring(0, ManufacturerSerialNumbers.Length - 1);

            //var Datas = DataList.GroupBy(d => d.a.MnfSerial).ToList();
            //List<MaterialCodeListReap> MaterialCodeListReaps = new List<MaterialCodeListReap>();
            //foreach (var item in Datas)
            //{
            //    MaterialCodeListReap mcr = new MaterialCodeListReap();
            //    mcr.MnfSerial = item.Key;
            //    mcr.MaterialDetailList = item.Select(i => new MaterialDetails { ItemCode = i.c.ItemCode, OnHand = i.c.OnHand, WhsCode = i.c.WhsCode, Quantity = i.b.BaseQty }).ToList();
            //    MaterialCodeListReaps.Add(mcr);
            //}
            //var ManufacturerSerialNumberList = Datas.Where(d => !ManufacturerSerialNumbers.Contains(d.Key)).Select(d => d.Key).ToList();
            //if (ManufacturerSerialNumberList != null && ManufacturerSerialNumberList.Count() > 0)
            //{
            //    var MaterialCodes = ServiceWorkOrderList.Where(s => ManufacturerSerialNumberList.Contains(s.ManufacturerSerialNumber)).Select(s => s.MaterialCode).ToList();
            //    var itt1list = await UnitWork.Find<ITT1>(o => MaterialCodes.Contains(o.Father)).Select(o => new { o.Father, o.Code, o.Quantity }).ToListAsync();
            //    var codes = itt1list.Select(i => i.Code).ToList();
            //    oitwlist = await UnitWork.Find<OITW>(o => codes.Contains(o.ItemCode) && o.WhsCode == "37").Select(o => new { o.ItemCode, o.OnHand, o.WhsCode }).ToListAsync();

            //    var OrderList = from a in ServiceWorkOrderList
            //                    join b in itt1list on a.MaterialCode equals b.Father
            //                    join c in oitwlist on b.Code equals c.ItemCode
            //                    select new { a, b, c };
            //    var ListData = OrderList.GroupBy(d => d.a.ManufacturerSerialNumber).ToList();
            //    foreach (var item in ListData)
            //    {
            //        MaterialCodeListReap mcr = new MaterialCodeListReap();
            //        mcr.MnfSerial = item.Key;
            //        mcr.MaterialDetailList = item.Select(i => new MaterialDetails { ItemCode = i.c.ItemCode, WhsCode = i.c.WhsCode, OnHand = i.c.OnHand, Quantity = i.b.Quantity }).ToList();
            //        MaterialCodeListReaps.Add(mcr);
            //    }
            //}
            #endregion

            var BaseEntrys = await UnitWork.Query<SysOIT1Column>(@$"SELECT a.BaseEntry,c.MnfSerial
            FROM nsap_bone.store_oitl a left join nsap_bone.store_itl1 b
            on a.LogEntry = b.LogEntry and a.ItemCode = b.ItemCode and a.sbo_id = b.sbo_id
            left join nsap_bone.store_osrn c on b.sbo_id = c.sbo_id and b.ItemCode = c.ItemCode and b.SysNumber = c.SysNumber
            where a.DocType in (15, 59) and c.MnfSerial ='{request.ManufacturerSerialNumbers}'  and a.BaseType=202").Select(s => new { s.MnfSerial, s.BaseEntry }).ToListAsync();
            var BaseEntrysIds = BaseEntrys.Select(s => s.BaseEntry).ToList();
            var wor1list = await UnitWork.Find<WOR1>(w => BaseEntrysIds.Contains(w.DocEntry)).Select(w => new { w.DocEntry, w.ItemCode, w.BaseQty }).ToListAsync();
            var ItemCodeList = wor1list.Select(w => w.ItemCode).ToList();
            var oitwlist = await UnitWork.Find<OITW>(o => ItemCodeList.Contains(o.ItemCode) && o.WhsCode == "37").WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), o => o.ItemCode.Contains(request.PartCode)).Select(o => new { o.ItemCode, o.OnHand, o.WhsCode }).ToListAsync();

            if (oitwlist == null && oitwlist.Count() <= 0)
            {
                var itt1list = await UnitWork.Find<ITT1>(o => o.Father.Equals(request.MaterialCode)).Select(o => new { o.Father, o.Code, o.Quantity, o.U_Desc }).ToListAsync();
                var codes = itt1list.Select(i => i.Code).ToList();
                oitwlist = await UnitWork.Find<OITW>(o => codes.Contains(o.ItemCode) && o.WhsCode == "37").WhereIf(!string.IsNullOrWhiteSpace(request.PartCode), o => o.ItemCode.Contains(request.PartCode)).Select(o => new { o.ItemCode, o.OnHand, o.WhsCode }).ToListAsync();

                result.Data = oitwlist.Select(o => new
                {
                    o.ItemCode,
                    o.OnHand,
                    o.WhsCode,
                    Describe= itt1list.FirstOrDefault(i=>i.Code.Equals(o.ItemCode))?.U_Desc
                }).ToList();
            }

            result.Data = oitwlist.Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToList();

            result.Count = oitwlist.Count();
            return result;
        }

        /// <summary>
        /// 获取报价单详情
        /// </summary>
        public async Task<TableData> GetDetails(int QuotationId)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var result = new TableData();
            var Quotations = UnitWork.Find<Quotation>(q => q.Id.Equals(QuotationId)).Include(q => q.QuotationOperationHistory)
                            .Include(q => q.QuotationProducts).Include(q => q.QuotationMaterials).FirstOrDefault();
            var ServiceOrders = await UnitWork.Find<ServiceOrder>(null).WhereIf(!string.IsNullOrWhiteSpace(Quotations.ServiceOrderId.ToString()), s => s.Id.Equals(Quotations.ServiceOrderId)).Select(s => new { s.Id, s.TerminalCustomer, s.TerminalCustomerId }).FirstOrDefaultAsync();
            result.Data = new
            {
                Quotations,
                ServiceOrders
            };

            return result;
        }

        /// <summary>
        /// 新增报价单
        /// </summary>
        /// <param name="obj"></param>
        public async Task Add(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            var QuotationObj = obj.MapTo<Quotation>();
            QuotationObj.QuotationProducts.ForEach(q => q.QuotationMaterials.ForEach(m => m.Id = Guid.NewGuid().ToString()));
            QuotationObj.CreateTime = DateTime.Now;
            QuotationObj.CreateUser = loginContext.User.Name;
            QuotationObj.CreateUserId = loginContext.User.Id;
            QuotationObj.Status = 1;
            QuotationObj.QuotationStatus = 3;
            QuotationObj = await UnitWork.AddAsync<Quotation, int>(QuotationObj);
            await UnitWork.SaveAsync();

            if (!obj.IsDraft)
            {
                if ((bool)QuotationObj.IsProtected)
                {
                    QuotationObj.QuotationStatus = 4;
                }
                else
                {
                    QuotationObj.QuotationStatus = 5;
                }
                QuotationObj.CreateTime = DateTime.Now;
                QuotationObj.CreateUser = loginContext.User.Name;
                QuotationObj.CreateUserId = loginContext.User.Id;
                QuotationObj.Status = 1;
                //创建物料报价单审批流程
                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单"));
                var afir = new AddFlowInstanceReq();
                afir.SchemeId = mf.FlowSchemeId;
                afir.FrmType = 2;
                afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                afir.CustomName = $"物料报价单" + DateTime.Now;
                afir.OrgId = "";
                //保内保外
                if ((bool)QuotationObj.IsProtected)
                {
                    //保外申请报价单
                    afir.FrmData = $"{{\"QuotationId\":\"{QuotationObj.Id}\",\"IsProtected\":\" 0\"}}";
                }
                else
                {
                    //保内申请报价单
                    afir.FrmData = $"{{\"QuotationId\":\"{QuotationObj.Id}\",\"IsProtected\":\" 1\"}}";
                }
                var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);
                QuotationObj.FlowInstanceId = FlowInstanceId;
                await UnitWork.UpdateAsync<Quotation>(QuotationObj);
                await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                {
                    Action = "提交报价单",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    QuotationId = QuotationObj.Id
                });
                await UnitWork.SaveAsync();
            }

        }

        /// <summary>
        /// 修改报价单
        /// </summary>
        /// <param name="obj"></param>
        public async Task Update(AddOrUpdateQuotationReq obj)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            var loginUser = loginContext.User;
            if (loginUser.Account == Define.USERAPP)
            {
                loginUser = await GetUserId(Convert.ToInt32(obj.AppId));
            }
            var QuotationObj = obj.MapTo<Quotation>();
            if (obj.IsDraft)
            {
                QuotationObj.CreateTime = DateTime.Now;
                QuotationObj.CreateUser = loginContext.User.Name;
                QuotationObj.CreateUserId = loginContext.User.Id;
                QuotationObj.Status = 1;
                QuotationObj.QuotationStatus = 3;
                await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                {
                    QuotationStatus = QuotationObj.QuotationStatus,
                    QuotationMaterials = QuotationObj.QuotationMaterials,
                    CollectionAddress = QuotationObj.CollectionAddress,
                    ShippingAddress = QuotationObj.ShippingAddress,
                    DeliveryMethod = QuotationObj.DeliveryMethod,
                    InvoiceCompany = QuotationObj.InvoiceCompany,
                    TotalMoney = QuotationObj.TotalMoney,
                    Reamrk = QuotationObj.Reamrk,
                    IsDraft = QuotationObj.IsDraft,
                    IsProtected = QuotationObj.IsProtected,
                    Status = QuotationObj.Status
                    //todo:要修改的字段赋值
                });
                await UnitWork.SaveAsync();
            }
            else
            {
                if ((bool)QuotationObj.IsProtected)
                {
                    QuotationObj.QuotationStatus = 4;
                }
                else
                {
                    QuotationObj.QuotationStatus = 5;
                }
                QuotationObj.CreateTime = DateTime.Now;
                QuotationObj.CreateUser = loginContext.User.Name;
                QuotationObj.CreateUserId = loginContext.User.Id;
                QuotationObj.Status = 1;

                //创建物料报价单审批流程
                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("物料报价单审批"));
                var afir = new AddFlowInstanceReq();
                afir.SchemeId = mf.FlowSchemeId;
                afir.FrmType = 2;
                afir.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                afir.CustomName = $"物料报价单" + DateTime.Now;
                afir.OrgId = "";
                //保内保外
                if ((bool)QuotationObj.IsProtected)
                {
                    //保外申请报价单
                    afir.FrmData = $"{{\"QuotationId\":\"{QuotationObj.Id}\",\"IsProtected\":\" 1\"}}";
                }
                else
                {
                    //保内申请报价单
                    afir.FrmData = $"{{\"QuotationId\":\"{QuotationObj.Id}\",\"IsProtected\":\" 0\"}}";
                }
                var FlowInstanceId = await _flowInstanceApp.CreateInstanceAndGetIdAsync(afir);

                await UnitWork.UpdateAsync<Quotation>(u => u.Id == QuotationObj.Id, u => new Quotation
                {
                    //todo:要修改的字段赋值
                });
                await UnitWork.AddAsync<QuotationOperationHistory>(new QuotationOperationHistory
                {
                    Action = "提交报价单",
                    CreateUser = loginUser.Name,
                    CreateUserId = loginUser.Id,
                    CreateTime = DateTime.Now,
                    QuotationId = QuotationObj.Id
                });
                await UnitWork.SaveAsync();
            }


        }

        /// <summary>
        /// 审批
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task<TableData> Accraditation(AddOrUpdateQuotationReq obj)
        {
            var result = new TableData();
            return result;
        }

        /// <summary>
        /// 获取用户
        /// </summary>
        /// <param name="AppId"></param>
        /// <returns></returns>
        private async Task<User> GetUserId(int AppId)
        {
            var userid = UnitWork.Find<AppUserMap>(u => u.AppUserId.Equals(AppId)).Select(u => u.UserID).FirstOrDefault();

            return await UnitWork.Find<User>(u => u.Id.Equals(userid)).FirstOrDefaultAsync();
        }

        public QuotationApp(IUnitWork unitWork, FlowInstanceApp flowInstanceApp, ModuleFlowSchemeApp moduleFlowSchemeApp, IAuth auth) : base(unitWork, auth)
        {
            _flowInstanceApp = flowInstanceApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
        }

    }
}
