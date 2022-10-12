using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.Nwcali;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App.Nwcali
{
    public class PortfolioAssetsApp : OnlyUnitWorkBaeApp
    {
        public readonly CategoryApp _categoryapp;
        public readonly OrgManagerApp _orgmanagerapp;
        public readonly UserManagerApp _usermanagerapp;
        public PortfolioAssetsApp(IUnitWork unitWork, CategoryApp categoryapp, OrgManagerApp orgmanagerapp, UserManagerApp usermanagerapp, IAuth auth) : base(unitWork, auth)
        {
            _categoryapp = categoryapp;
            _orgmanagerapp = orgmanagerapp;
            _usermanagerapp = usermanagerapp;
        }

        /// <summary>
        /// 加载组合资产列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> Load(QueryPortfolioAssetListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var objs = UnitWork.Find<PortfolioAssets>(null);
            var Assets = objs.WhereIf(!string.IsNullOrWhiteSpace(request.AssetsId), u => u.Guid == request.AssetsId).
                WhereIf(!string.IsNullOrWhiteSpace(request.Name), u => u.Name.Contains(request.Name)).
                WhereIf(request.Category > 0, u => u.Category == request.Category).
                WhereIf(request.FirmwareParts > 0, u => u.FirmwareParts == request.FirmwareParts).
                WhereIf(request.TemporaryParts > 0, u => u.TemporaryParts == request.TemporaryParts).
                WhereIf(!string.IsNullOrWhiteSpace(request.CreateUser), u => u.CreateUser == request.CreateUser).
                WhereIf(!string.IsNullOrWhiteSpace(request.OrgName), u => u.CreateUser == request.OrgName).
                WhereIf(request.CreateStartTime != null, u => u.CreateTime >= request.CreateStartTime).
                WhereIf(request.CreateEndTime != null, u => u.CreateTime < request.CreateEndTime).
                WhereIf(request.UpdateStartTime != null, u => u.UpdateTime >= request.UpdateStartTime).
                WhereIf(request.UpdateEndTime != null, u => u.UpdateTime < request.UpdateEndTime);

            result.Data = await Assets.OrderByDescending(u => u.CreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();
            result.Count = Assets.Count();
            return result;
        }

        /// <summary>
        /// 加载组合资产详情
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> GetDetails(int  Id)
        {
            var result = new TableData();
            var info = UnitWork.Find<PortfolioAssets>(a => a.Id == Id).First();
            var mapList = UnitWork.Find<PortfolioAssetsMap>(a => a.PortfolioId == Id).ToList();
            result.Data = new
            {

                info = info,
                FirmwareInfo = mapList.Where(a => a.PartsType ==1).ToList(),
                TemporaryInfo = mapList.Where(a => a.PartsType ==2).ToList(),
            };
            return result;
        }
        /// <summary>
        /// 添加/修改组合资产
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task AddOrUpdate(AddOrUpdatePortfolioAssetsReq req)
        {
            var user = _auth.GetCurrentUser().User;

            int id= 0;
            if (req.Id > 0)
            {
                id = (int)req.Id;
                var info = UnitWork.Find<PortfolioAssets>(a=> a.Id == req.Id).First();
                info.Name = req.Name;
                info.Category = req.Category;
                info.FirmwareParts = req.FirmwareInfo.Count();
                info.TemporaryParts = req.TemporaryInfo.Count();
                info.OrgName = req.OrgName;
                info.Remark = req.Remark;
                info.UpdateTime = DateTime.Now;
                await UnitWork.UpdateAsync(info);

                var mapList = UnitWork.Find<PortfolioAssetsMap>(a => a.PortfolioId == req.Id).ToList();
                await UnitWork.BatchDeleteAsync(mapList.ToArray());
            }
            else
            {
                PortfolioAssets info = new PortfolioAssets();
                info.Guid = req.Guid;
                info.Name = req.Name;
                info.Category = req.Category;
                info.FirmwareParts = req.FirmwareInfo.Count();
                info.TemporaryParts = req.TemporaryInfo.Count();
                info.OrgName = req.OrgName;
                info.Remark = req.Remark;
                info.CreateTime = DateTime.Now;
                info.UpdateTime = DateTime.Now;
                info.CreateUser = user.Name;
                info.CreateUserId = user.Id;
                var obj = await UnitWork.AddAsync<PortfolioAssets, int>(info);
                await UnitWork.SaveAsync();
                id = obj.Id;
            }

            foreach (var item in req.FirmwareInfo)
            {
                PortfolioAssetsMap map = new PortfolioAssetsMap();
                map.PortfolioId = id;
                map.AssetId = item.AssetId;
                map.SourceType = item.SourceType;
                map.PartsType = item.PartsType;
                map.Sort = item.Sort;
                await UnitWork.AddAsync<PortfolioAssetsMap, int>(map);
            }
            await UnitWork.SaveAsync();
            foreach (var item in req.TemporaryInfo)
            {
                PortfolioAssetsMap map = new PortfolioAssetsMap();
                map.PortfolioId = id;
                map.AssetId = item.AssetId;
                map.SourceType = item.SourceType;
                map.PartsType = item.PartsType;
                map.Sort = item.Sort;
                await UnitWork.AddAsync<PortfolioAssetsMap, int>(map);
            }
            await UnitWork.SaveAsync();


        }


        /// <summary>
        /// 删除组合资产
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public async Task Delete(int Id)
        {
            var info = UnitWork.Find<PortfolioAssets>(a => a.Id == Id).First();
            var mapList = UnitWork.Find<PortfolioAssetsMap>(a => a.PortfolioId == Id).ToList();
            await UnitWork.DeleteAsync(info);
            if (mapList.Count() >0)
            {
                await UnitWork.BatchDeleteAsync(mapList.ToArray());
            }
            await UnitWork.SaveAsync();
        }


        // <summary>
        /// 加载实验室资产列表
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<TableData> LoadLaboratory(QueryassetListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            var list = UnitWork.Find<PortfolioAssetsMap>(a => a.SourceType == 1).Select(a => a.AssetId).ToList();
            var objs = UnitWork.Find<Asset>(a => !list.Contains(a.Id) );
            var Assets = objs.WhereIf(!string.IsNullOrWhiteSpace(request.Id.ToString()), u => u.Id == request.Id).
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
                    AssetStartDate = Convert.ToDateTime(L.AssetStartDate).ToString("yyyy-MM-dd"),
                    AssetCalibrationCertificate = L.AssetCalibrationCertificate,
                    AssetEndDate = Convert.ToDateTime(L.AssetEndDate).ToString("yyyy-MM-dd"),
                    AssetInspectDataOne = L.AssetInspectDataOne,
                    AssetInspectDataTwo = L.AssetInspectDataTwo,
                    AssetTCF = L.AssetTCF,
                    AssetDescribe = L.AssetDescribe,
                    AssetRemarks = L.AssetRemarks,
                    AssetImage = L.AssetImage,
                    AssetCreateTime = L.AssetCreateTime,
                    AssetCategorys = L.AssetCategorys != null && L.AssetCategorys.Count > 0 ? CalculateMetrological(L.AssetCategorys, L.AssetCategory) : ""
                }).ToListAsync();

            result.Count = Assets.Count();
            return result;
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

    }
}
