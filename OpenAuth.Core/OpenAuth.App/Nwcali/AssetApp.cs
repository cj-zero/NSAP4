using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using OpenAuth.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Npoi.Mapper;
using OpenAuth.App.Nwcali.Response;

namespace OpenAuth.App.nwcali
{
    /// <summary>
    /// 资产逻辑操作 by zlg 2020.7.31
    /// </summary>
    public class AssetApp : OnlyUnitWorkBaeApp
    {
        public readonly CategoryApp _categoryapp;
        public readonly OrgManagerApp _orgmanagerapp;
        public readonly UserManagerApp _usermanagerapp;
        public AssetApp(IUnitWork unitWork, CategoryApp categoryapp, OrgManagerApp orgmanagerapp, UserManagerApp usermanagerapp, IAuth auth) : base(unitWork, auth)
        {
            _categoryapp = categoryapp;
            _orgmanagerapp = orgmanagerapp;
            _usermanagerapp = usermanagerapp;
        }
        
        /// <summary>
        /// 加载资产列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> Load(QueryassetListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var objs = UnitWork.Find<Asset>(null);
            var Assets =  objs.WhereIf(!string.IsNullOrWhiteSpace(request.Id.ToString()), u => u.Id==request.Id).
                WhereIf(!string.IsNullOrWhiteSpace(request.AssetCategory), u => u.AssetCategory.Contains(request.AssetCategory)).
                WhereIf(!string.IsNullOrWhiteSpace(request.AssetStockNumber), u => u.AssetStockNumber.Contains(request.AssetStockNumber)).
                WhereIf(!string.IsNullOrWhiteSpace(request.AssetInspectType), u => u.AssetInspectType.Contains(request.AssetInspectType)).
                WhereIf(!string.IsNullOrWhiteSpace(request.AssetStatus), u => u.AssetStatus.Contains(request.AssetStatus)).
                WhereIf(!string.IsNullOrWhiteSpace(request.AssetType), u => u.AssetType.Contains(request.AssetType)).
                WhereIf(!string.IsNullOrWhiteSpace(request.AssetNumber), u => u.AssetNumber.Contains(request.AssetNumber)).
                WhereIf(!string.IsNullOrWhiteSpace(request.OrgName), u => u.OrgName.Contains(request.OrgName)).
                WhereIf(request.AssetStartDate != null && request.AssetEndDate != null, u => u.AssetStartDate >= request.AssetStartDate && u.AssetEndDate < Convert.ToDateTime(request.AssetEndDate).AddMinutes(1440));

            
            result.Data = await Assets.OrderByDescending(u => u.AssetCreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(L => new
                {
                    Id = L.Id,
                    AssetStatus = L.AssetStatus,
                    AssetCategory = L.AssetCategory,
                    OrgName = L.OrgName,
                    AssetType = L.AssetType,
                    AssetHolder = L.AssetHolder,
                    AssetStockNumber = L.AssetStockNumber,
                    AssetAdmin = L.AssetAdmin,
                    AssetNumber = L.AssetNumber,
                    AssetFactory = L.AssetFactory,
                    AssetInspectType = L.AssetInspectType,
                    AssetInspectWay = L.AssetInspectWay,
                    AssetStartDate =Convert.ToDateTime(L.AssetStartDate).ToString("yyyy-MM-dd"),
                    AssetCalibrationCertificate = L.AssetCalibrationCertificate,
                    AssetEndDate = Convert.ToDateTime(L.AssetEndDate).ToString("yyyy-MM-dd"),
                    AssetInspectDataOne = L.AssetInspectDataOne,
                    AssetInspectDataTwo = L.AssetInspectDataTwo,
                    AssetTCF = L.AssetTCF,
                    AssetDescribe = L.AssetDescribe,
                    AssetRemarks = L.AssetRemarks,
                    AssetImage = L.AssetImage,
                    AssetCreateTime = L.AssetCreateTime,
                    AssetCategorys =L.AssetCategorys!=null && L.AssetCategorys.Count>0? CalculateMetrological(L.AssetCategorys, L.AssetCategory):""
                }).ToListAsync();

            result.Count = Assets.Count();
            return result;
        }

        /// <summary>
        /// 获取单个资产
        /// </summary>
        /// <param name="assetid"></param>
        /// <returns></returns>
        public async Task<TableData> GetAsset(int assetid)
        {
            var result = new TableData();
            var AssetModel =await UnitWork.Find<Asset>(u => u.Id == assetid)
                .Include(u=>u.AssetCategorys)
                .Include(u=>u.AssetOperations)
                .Include(u=>u.AssetInspects).FirstOrDefaultAsync();
            result.Data = AssetModel;
            return result;
        }

        /// <summary>
        /// 添加资产
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Add(AddOrUpdateassetReq req)
        {
            var obj = req.MapTo<Asset>();
            var user = _auth.GetCurrentUser().User;
            var ZCNumber = "JZ" + Convert.ToInt32(req.AssetSerial).ToString("00") + DateTime.Today.ToString("yy") + DateTime.Today.ToString("MM");
            var Listasset = UnitWork.Find<Asset>(u => u.AssetNumber.Contains(ZCNumber)).OrderByDescending(u => u.AssetNumber).FirstOrDefault();
            if (Listasset == null)
            {
                ZCNumber += "0001";
            }
            else
            {
                var Number = Convert.ToInt32(Listasset.AssetNumber.Substring(Listasset.AssetNumber.Length - 4, 4)) + 1;
                ZCNumber += Number.ToString("0000");
            }
            obj.AssetNumber = ZCNumber;
            obj.AssetCreateTime = DateTime.Now;
            obj.AssetCreateUser = user.Name;
            int num = 0;
            if (obj.AssetCategorys != null)
            {
                obj.AssetCategorys.ForEach(a => a.CategoryAort = ++num);
            }
            obj=await UnitWork.AddAsync<Asset,int>(obj);
            await UnitWork.SaveAsync();
            //if (req.Listcategory != null && req.Listcategory.Count > 0)
            //{
            //    var CategoryModel = req.Listcategory.MapToList<AssetCategory>();
            //    int num = 0;
            //    foreach (var item in CategoryModel)
            //    {
            //        item.AssetId = obj.Id;
            //        item.CategoryAort = ++num;
            //    }
            //    UnitWork.BatchAdd<AssetCategory>(CategoryModel.ToArray());
            //}

            //保存第一次校准记录
            var eassetinspectReq = new AddOrUpdateassetinspectReq();
            eassetinspectReq.AssetId = obj.Id;
            eassetinspectReq.InspectStartDate = obj.AssetStartDate;
            eassetinspectReq.InspectEndDate = obj.AssetEndDate;
            eassetinspectReq.InspectDataOne = obj.AssetInspectDataOne;
            eassetinspectReq.InspectDataTwo = obj.AssetInspectDataTwo;
            eassetinspectReq.InspectCertificate = obj.AssetCalibrationCertificate;
            var InspectModel = eassetinspectReq.MapTo<AssetInspect>();
            InspectModel.InspectCreatTime = DateTime.Now;
            await UnitWork.AddAsync<AssetInspect>(InspectModel);
            //保存第一次操作记录
            var eassetoperationReq = new AddOrUpdateassetoperationReq();
            eassetoperationReq.AssetId = obj.Id;
            eassetoperationReq.InspectId = InspectModel.Id;
            eassetoperationReq.OperationContent = "创建资产成功";
            var OperationModel = eassetoperationReq.MapTo<AssetOperation>();
            OperationModel.OperationCreateTime = DateTime.Now;
            OperationModel.OperationUser = user.Name;
            await UnitWork.AddAsync<AssetOperation>(OperationModel);
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 修改资产
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task Update(AddOrUpdateassetReq obj)
        {
            Asset model = await UnitWork.Find<Asset>(u => u.Id == obj.Id).FirstOrDefaultAsync();
            StringBuilder ModifyModel = new StringBuilder();
            string InspectId = null;
            bool inspect = false;
            //判断修改的数据，放到字符串中
            if (model.AssetAdmin != obj.AssetAdmin) ModifyModel.Append("管理员修改为：" + obj.AssetAdmin + @"\r\n");
            if (model.AssetDescribe != obj.AssetDescribe) ModifyModel.Append("描述修改为：" + obj.AssetDescribe + @"\r\n");
            if (model.AssetHolder != obj.AssetHolder) ModifyModel.Append("持有者修改为：" + obj.AssetHolder + @"\r\n");
            if (model.AssetRemarks != obj.AssetRemarks) ModifyModel.Append("备注修改为：" + obj.AssetRemarks + @"\r\n");
            if (model.AssetInspectType != obj.AssetInspectType) ModifyModel.Append("送检类型修改为：" + obj.AssetInspectType + @"\r\n");
            if (model.AssetStatus != obj.AssetStatus) ModifyModel.Append("状态修改为：" + obj.AssetStatus + @"\r\n");
            if (model.OrgName != obj.OrgName) ModifyModel.Append("部门修改为：" + obj.OrgName + @"\r\n");
            if (model.AssetInspectDataTwo != obj.AssetInspectDataTwo) ModifyModel.Append("修改校准数据" +@"\r\n");
            if (model.AssetStartDate != obj.AssetStartDate) ModifyModel.Append("校准日期修改为：" + obj.AssetStartDate + @"\r\n");
            if (model.AssetEndDate != obj.AssetEndDate) ModifyModel.Append("失效日期修改为：" + obj.AssetEndDate + @"\r\n");
            if (model.AssetInspectWay != obj.AssetInspectWay) ModifyModel.Append("送检方式修改为：" + obj.AssetInspectWay + @"\r\n");
            if (model.AssetImage != obj.AssetImage) ModifyModel.Append("修改图片" + @"\r\n");
            if (model.AssetCalibrationCertificate != obj.AssetCalibrationCertificate)
            {
                ModifyModel.Append("修改校准证书" + @"\r\n");
                inspect = true;
            }
            await UnitWork.UpdateAsync<Asset>(u => u.Id == obj.Id, u => new Asset
            {
                AssetAdmin = obj.AssetAdmin,
                AssetEndDate = obj.AssetEndDate,
                AssetStartDate = obj.AssetStartDate,
                AssetDescribe = obj.AssetDescribe,
                AssetHolder = obj.AssetHolder,
                AssetCalibrationCertificate = obj.AssetCalibrationCertificate,
                AssetInspectDataTwo = obj.AssetInspectDataTwo,
                AssetRemarks = obj.AssetRemarks,
                AssetInspectType = obj.AssetInspectType,
                AssetStatus = obj.AssetStatus,
                OrgName = obj.OrgName,
                AssetInspectWay = obj.AssetInspectWay,
                AssetImage =obj.AssetImage
            });
            if (obj.AssetCategorys != null && obj.AssetCategorys.Count > 0)
            {
                await UnitWork.BatchUpdateAsync<AssetCategory>(obj.AssetCategorys.MapToList<AssetCategory>().ToArray());
            }
            //添加一条送检记录
            if (inspect)
            {
                var eassetinspectReq = new AddOrUpdateassetinspectReq();
                eassetinspectReq.AssetId = obj.Id;
                eassetinspectReq.InspectStartDate = obj.AssetStartDate;
                eassetinspectReq.InspectEndDate = obj.AssetEndDate;
                eassetinspectReq.InspectDataOne = obj.AssetInspectDataOne;
                eassetinspectReq.InspectDataTwo = obj.AssetInspectDataTwo;
                eassetinspectReq.InspectCertificate = obj.AssetCalibrationCertificate;
                var InspectModel = eassetinspectReq.MapTo<AssetInspect>();
                InspectModel.InspectCreatTime = DateTime.Now;
                InspectModel=await UnitWork.AddAsync<AssetInspect>(InspectModel);
                InspectId = InspectModel.Id;
            }
            //添加一条操作记录
            if (!string.IsNullOrEmpty(ModifyModel.ToString()))
            {
                var eassetoperationReq = new AddOrUpdateassetoperationReq();
                eassetoperationReq.AssetId = obj.Id;
                eassetoperationReq.InspectId = InspectId;
                eassetoperationReq.OperationContent = ModifyModel.ToString();
                var OperationModel = eassetoperationReq.MapTo<AssetOperation>();
                OperationModel.OperationCreateTime = DateTime.Now;
                var user = _auth.GetCurrentUser().User;
                OperationModel.OperationUser = user.Name;
                await UnitWork.AddAsync<AssetOperation>(OperationModel);
            }
            await UnitWork.SaveAsync();
        }

        /// <summary>
        /// 查询字典
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public async Task<TableData> GetListCategoryName(string ids)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("Category");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }

            return await _categoryapp.GetListCategoryName(ids);
        }

        /// <summary>
        /// 按名称模糊查询部门
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public async Task<TableData> GetListOrg(string name)
        {
            return await _orgmanagerapp.GetListOrg(name);
        }

        /// <summary>
        /// 按名称模糊查询人员
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Orgid"></param>
        /// <returns></returns>
        public async Task<TableData> GetListUser(string name, string Orgid)
        {
            return await _usermanagerapp.GetListUser(name, Orgid);
        }

        /// <summary>
        /// 计算计量特性
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="Category"></param>
        /// <returns></returns>
        public static string CalculateMetrological(List<AssetCategory> obj, string Category)
        {
            StringBuilder Metrological = new StringBuilder();
            foreach (var item in obj.OrderBy(u => u.CategoryAort))
            {
                Metrological.Append(item.CategoryNumber + "：");
                string symbol = "";
                if (Category.Contains("万用表"))
                {

                    if (item.CategoryNumber.Contains("DCV") || item.CategoryNumber.Contains("ACV"))
                    {
                        symbol = "V";
                    }
                    else if (item.CategoryNumber.Contains("DCI") || item.CategoryNumber.Contains("ACI"))
                    {
                        symbol = "A";
                    }
                    else if (item.CategoryNumber.Contains("OHM"))
                    {
                        symbol = "Ω";
                    }
                    if (item.CategoryType.Contains("绝对不确定度"))
                    {
                        if (!string.IsNullOrWhiteSpace(item.CategoryOhms.ToString()) && item.CategoryOhms != 0)
                        {
                            Metrological.Append("(" + Convert.ToDecimal(item.CategoryOhms).ToString("G0") + "±" + String.Format("{0:#.##########E+0}", item.CategoryNondeterminacy) + ")" + symbol + " (k=" + Convert.ToDecimal(item.CategoryBHYZ).ToString("G0") + @")\r\n");
                        }
                        else
                        {
                            Metrological.Append("±" + String.Format("{0:#.##########E+0}", item.CategoryNondeterminacy) + "" + symbol + " (k=" + Convert.ToDecimal(item.CategoryBHYZ).ToString("G0") + @")\r\n");
                        }
                    }
                    else if (item.CategoryType.Contains("相对不确定度"))
                    {
                        if (!string.IsNullOrWhiteSpace(item.CategoryOhms.ToString()) && item.CategoryOhms != 0)
                        {
                            Metrological.Append(Convert.ToDecimal(item.CategoryOhms).ToString("G0") + symbol + "，Urel=" + Convert.ToDecimal(item.CategoryNondeterminacy).ToString("G0") + "% (k=" + Convert.ToDecimal(item.CategoryBHYZ).ToString("G0") + @")\r\n");
                        }
                        else
                        {
                            Metrological.Append("Urel=" + Convert.ToDecimal(item.CategoryNondeterminacy).ToString("G0") + "% (k=" + Convert.ToDecimal(item.CategoryBHYZ).ToString("G0") + @")\r\n");
                        }
                    }
                }
                else if (Category.Contains("分流器") || Category.Contains("工装"))
                {
                    if (item.CategoryType.Contains("绝对不确定度"))
                    {
                        Metrological.Append("(" + Convert.ToDecimal(item.CategoryOhms).ToString("G0") + "±" + String.Format("{0:#.##########E+0}", item.CategoryNondeterminacy) + ")Ω (k=" + Convert.ToDecimal(item.CategoryBHYZ).ToString("G0") + @")\r\n");
                    }
                    else if (item.CategoryType.Contains("相对不确定度"))
                    {
                        Metrological.Append(Convert.ToDecimal(item.CategoryOhms).ToString("G0") + "Ω，Urel=" + Convert.ToDecimal(item.CategoryNondeterminacy).ToString("G0") + "ppm (k=" + Convert.ToDecimal(item.CategoryBHYZ).ToString("G0") + @")\r\n");
                    }
                }

            }

            return Metrological.ToString();
        }

        #region 废弃
        /// <summary>
        /// 加载送检记录
        /// </summary>
        /// <param name="AssetId"></param>
        /// <returns></returns>
        //public TableData AssetInspectsLoad(string AssetId)
        //{
        //    var result = new TableData();
        //    var objs = UnitWork.Find<AssetInspect>(null);
        //    objs = objs.Where(u => u.AssetId == AssetId);
        //    var loginContext = _auth.GetCurrentUser();
        //    var properties = loginContext.GetProperties("AssetInspect");
        //    var propertyStr = string.Join(',', properties.Select(u => u.Key));
        //    result.columnHeaders = properties;
        //    result.Data = objs.OrderBy(u => u.Id).Select($"new ({propertyStr})");
        //    result.Count = objs.Count();
        //    return result;
        //}

        /// <summary>
        /// 加载操作记录
        /// </summary>
        /// <param name="AssetId"></param>
        /// <returns></returns>
        //public async Task<TableData> AssetOperationsLoad(string AssetId)
        //{
        //    var loginContext = _auth.GetCurrentUser();
        //    var properties = loginContext.GetProperties("assetoperation");
        //    var result = new TableData();
        //    var objs = UnitWork.Find<AssetOperation>(null);
        //    objs = objs.Where(u => u.AssetId.Contains(AssetId));
        //    result.Data = await objs.OrderBy(u => u.Id).ToListAsync();
        //    result.Count = objs.Count();
        //    return result;
        //}
        #endregion

        /// <summary>
        /// 根据出厂编号或资产编号查询数据(备用)
        /// </summary>
        /// <param name="AssetStockNumber"></param>
        /// <param name="AssetNumber"></param>
        /// <returns></returns>

        public AddOrUpdateAppReq GetAsset(string AssetStockNumber, string AssetNumber)
        {
            var obj = UnitWork.Find<Asset>(u => u.AssetStockNumber == AssetStockNumber).FirstOrDefault();
            if (obj == null) obj = UnitWork.Find<Asset>(u => u.AssetNumber == AssetNumber).FirstOrDefault();
            return obj.MapTo<AddOrUpdateAppReq>();
        }


    }
}
