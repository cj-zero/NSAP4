using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Material.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.App.Material
{
    /// <summary>
    /// 报价单操作
    /// </summary>
    public class QuotationApp : OnlyUnitWorkBaeApp
    {

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
            var Quotations = from a in UnitWork.Find<Quotation>(null)
                             join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id
                             select new { a, b };

            var QuotationList = Quotations.WhereIf(request.QuotationId.ToString() != null, q => q.a.Id.ToString().Contains(request.QuotationId.ToString()))
                                .WhereIf(request.ServiceOrderSapId != null, q => q.a.ServiceOrderSapId.ToString().Contains(request.ServiceOrderSapId.ToString()))
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), q => q.a.CreateUser.Contains(request.CreateUser))
                                .WhereIf(request.StartCreateTime != null, q => q.a.CreateTime > request.StartCreateTime)
                                .WhereIf(request.EndCreateTime != null, q => q.a.CreateTime < request.EndCreateTime)
                                .WhereIf(!string.IsNullOrWhiteSpace(request.CardCode), q => q.b.CustomerId.Contains(request.CardCode) || q.b.CustomerName.Contains(request.CardCode));
            #region 分页条件
            switch (request.StartType)
            {
                case 1://草稿箱
                    QuotationList = QuotationList.Where(q => q.a.IsDraft == true);
                    break;

                case 2://审批中
                    QuotationList = QuotationList.Where(q => q.a.QuotationStatus > 3);
                    break;

                case 3://已领料
                    QuotationList = QuotationList.Where(q => q.a.QuotationStatus == 8);
                    break;

                case 4://已驳回
                    QuotationList = QuotationList.Where(q => q.a.QuotationStatus == 2);
                    break;
                default:
                    break;
            }
            #endregion

            result.Data = await QuotationList.Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(q => new
                {
                    q.a.Id,
                    q.a.ServiceOrderSapId,
                    q.b.CustomerId,
                    q.b.CustomerName,
                    q.a.TotalMoney,
                    q.a.CreateUser,
                    q.a.Reamrk,
                    CreateTime = q.a.CreateTime.ToString("yyyy-MM-dd"),
                    q.a.QuotationStatus
                }).ToListAsync();
            result.Count = await QuotationList.CountAsync();
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
            var result = new TableData();

            var ServiceOrders = from a in UnitWork.Find<ServiceOrder>(null)
                                join b in UnitWork.Find<ServiceWorkOrder>(null) on a.Id equals b.ServiceOrderId
                                select new { a, b };

            var ServiceOrderList = (await ServiceOrders.Where(s => s.b.CurrentUserNsapId.Equals(loginContext.User.Id)).ToListAsync()).GroupBy(s => s.a.Id).Select(s => s.First());
            result.Data = ServiceOrderList.Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(q => new
                {
                    q.a.Id,
                    q.a.U_SAP_ID,
                    q.a.CustomerId,
                    q.a.CustomerName,
                    q.b.FromTheme,
                    q.a.SalesMan
                });
            result.Count = ServiceOrderList.Count();
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

            var MaterialCodeList = await UnitWork.Find<ServiceWorkOrder>(s => s.ServiceOrderId.Equals(request.ServiceOrderId)).Select(s => new { s.ManufacturerSerialNumber, s.MaterialCode }).ToListAsync();
            //var Components = from a in UnitWork.Find<OINS>(null)
            //                 join b in UnitWork.Find<DLN1>(null) on a.deliveryNo equals b.BaseEntry
            //                 join c in UnitWork.Find<OWOR>(null) on b.DocEntry equals c.OriginAbs;
                             //join d in UnitWork.Find<WOR1>(null) on c.DocEntry equals d
            result.Data = "";
            result.Count = 0;
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

            if (obj.IsDraft)
            {
                var QuotationObj = obj.MapTo<Quotation>();
                QuotationObj.CreateTime = DateTime.Now;
                QuotationObj.CreateUser = loginContext.User.Name;
                QuotationObj.CreateUserId = loginContext.User.Id;
                QuotationObj.Status = 1;
                QuotationObj.QuotationStatus = 3;
                await UnitWork.AddAsync<Quotation, int>(QuotationObj);
            }
            else
            {
                var QuotationObj = obj.MapTo<Quotation>();
                QuotationObj.CreateTime = DateTime.Now;
                QuotationObj.CreateUser = loginContext.User.Name;
                QuotationObj.CreateUserId = loginContext.User.Id;
                QuotationObj.Status = 1;
                QuotationObj.QuotationStatus = 4;
                await UnitWork.AddAsync<Quotation, int>(QuotationObj);
            }
        }

        public void Update(AddOrUpdateQuotationReq obj)
        {
            UnitWork.Update<Repository.Domain.Returnnote>(u => u.Id == obj.Id, u => new Repository.Domain.Returnnote
            {
                //todo:要修改的字段赋值
            });

        }

        public QuotationApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }

    }
}
