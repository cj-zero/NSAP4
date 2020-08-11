using Infrastructure;
using NetOffice.WordApi;
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
using Org.BouncyCastle.Ocsp;
using NPOI.SS.Formula.Functions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Localization.Internal;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace OpenAuth.App.nwcali
{
    /// <summary>
    /// 资产逻辑操作 by zlg 2020.7.31
    /// </summary>
    public class AssetApp : BaseApp<Asset>
    {
        public readonly CategoryApp _categoryapp;
        public readonly OrgManagerApp _orgmanagerapp;
        public readonly UserManagerApp _usermanagerapp;
        public AssetApp(IUnitWork unitWork, IRepository<Asset> repository,CategoryApp categoryapp, OrgManagerApp orgmanagerapp, UserManagerApp usermanagerapp, IAuth auth) : base(unitWork, repository, auth)
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
        public TableData Load(QueryassetListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("Asset");

            if (properties == null || properties.Count == 0)
            {
                throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }

            var result = new TableData();
            var objs = UnitWork.Find<Asset>(null);
            objs = objs.WhereIf(!string.IsNullOrEmpty(request.Id), u => u.Id.Contains(request.Id)).
                WhereIf(!string.IsNullOrEmpty(request.AssetCategory), u => u.AssetCategory.Contains(request.AssetCategory)).
                WhereIf(!string.IsNullOrEmpty(request.AssetCCNumber), u => u.AssetCCNumber.Contains(request.AssetCCNumber)).
                WhereIf(!string.IsNullOrEmpty(request.AssetSJType), u => u.AssetSJType.Contains(request.AssetSJType)).
                WhereIf(!string.IsNullOrEmpty(request.AssetStatus), u => u.AssetStatus.Contains(request.AssetStatus)).
                WhereIf(!string.IsNullOrEmpty(request.AssetType), u => u.AssetType.Contains(request.AssetType)).
                WhereIf(!string.IsNullOrEmpty(request.AssetZCNumber), u => u.AssetZCNumber.Contains(request.AssetZCNumber)).
                WhereIf(!string.IsNullOrEmpty(request.OrgName), u => u.OrgName.Contains(request.OrgName)).
                WhereIf(request.AssetJZDate != null && request.AssetSXDate != null, u => u.AssetJZDate >= request.AssetJZDate && u.AssetSXDate <= request.AssetSXDate);

            result.columnHeaders = properties;
            result.Data = objs.OrderByDescending(u => u.AssetCreateTime)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).Select(L => new
                {
                    Id = L.Id,
                    AssetStatus = L.AssetStatus,
                    AssetCategory = L.AssetCategory,
                    OrgName = L.OrgName,
                    AssetType = L.AssetType,
                    AssetHolder = L.AssetHolder,
                    AssetCCNumber = L.AssetCCNumber,
                    AssetAdmin = L.AssetAdmin,
                    AssetZCNumber = L.AssetZCNumber,
                    AssetFactory = L.AssetFactory,
                    AssetSJType = L.AssetSJType,
                    AssetSJWay = L.AssetSJWay,
                    AssetJZDate = L.AssetJZDate,
                    AssetJZCertificate = L.AssetJZCertificate,
                    AssetSXDate = L.AssetSXDate,
                    AssetJZData1 = L.AssetJZData1,
                    AssetJZData2 = L.AssetJZData2,
                    AssetJSFile = L.AssetJSFile,
                    AssetDescribe = L.AssetDescribe,
                    AssetRemarks = L.AssetRemarks,
                    AssetImage = L.AssetImage,
                    AssetCreateTime = L.AssetCreateTime,
                    AssetCategorys = CalculateMetrological(L.AssetCategorys)
        });

            result.Count = objs.Count();
            return result;
        }

        
        /// <summary>
        /// 添加资产
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public void Add(AddOrUpdateassetReq req) 
        {
            var obj = req.MapTo<Asset>();
            string ZCNumber="";
            ZCNumber = "JZ" + Convert.ToInt32(req.AssetSerial).ToString("00")+ DateTime.Today.ToString("yy")+DateTime.Today.ToString("MM");
            var Listasset = UnitWork.Find<Asset>(u => u.AssetZCNumber.Contains(ZCNumber)).OrderByDescending(u => u.AssetCreateTime).FirstOrDefault();
            if (Listasset == null)
            {
                ZCNumber += "0001";
            }
            else 
            {
                var Number = Convert.ToInt32(Listasset.AssetZCNumber.Substring(Listasset.AssetZCNumber.Length - 4, 4)) + 1;
                ZCNumber += Number.ToString("0000");
            }
            obj.AssetZCNumber = ZCNumber;
            obj.AssetCreateTime = DateTime.Now;
            UnitWork.Add<Asset>(obj);

            if (req.Listcategory != null && req.Listcategory.Count > 0) 
            {
                var CategoryModel = req.Listcategory.MapToList<AssetCategory>();
                CategoryModel.ForEach(u => u.AssetId = obj.Id);
                UnitWork.BatchAdd<AssetCategory>(CategoryModel.ToArray());
            }

            //保存第一次校准记录
            var eassetinspectReq = new AddOrUpdateassetinspectReq();
            eassetinspectReq.AssetId = obj.Id;
            eassetinspectReq.AssetJZDate = obj.AssetJZDate;
            eassetinspectReq.AssetSXDate = obj.AssetSXDate;
            eassetinspectReq.AssetJZData1 = obj.AssetJZData1;
            eassetinspectReq.AssetJZData2 = obj.AssetJZData2;
            eassetinspectReq.AssetJZCertificate = obj.AssetJZCertificate;
            var InspectModel = eassetinspectReq.MapTo<AssetInspect>();
            UnitWork.Add<AssetInspect>(InspectModel);

            //保存第一次操作记录
            var eassetoperationReq = new AddOrUpdateassetoperationReq();
            eassetoperationReq.AssetId = obj.Id;
            eassetoperationReq.InspectId = InspectModel.Id;
            eassetoperationReq.OperationCZContent = "创建资产成功";          
            var OperationModel = eassetoperationReq.MapTo<AssetOperation>();
            OperationModel.OperationCZDate = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            OperationModel.OperationCZName = user.Name;
            UnitWork.Add<AssetOperation>(OperationModel);
            try
            {
                UnitWork.Save();
            }
            catch (Exception)
            {
                throw new Exception("添加失败，请检查后重试");
            }
            
        }
        /// <summary>
        /// 修改资产
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public void Update(AddOrUpdateassetReq obj)
        {
            Asset model=UnitWork.Find<Asset>(u => u.Id == obj.Id).First();
            StringBuilder ModifyModel = new StringBuilder();
            string InspectId = null;
            bool inspect = false;
            //判断修改的数据，放到字符串中
            if (model.AssetAdmin != obj.AssetAdmin) ModifyModel.Append("管理员修改为：" + obj.AssetAdmin+ @"\r\n"); 
            if (model.AssetDescribe != obj.AssetDescribe) ModifyModel.Append("描述修改为：" + obj.AssetDescribe + @"\r\n"); 
            if (model.AssetHolder != obj.AssetHolder) ModifyModel.Append("持有者修改为：" + obj.AssetHolder + @"\r\n"); 
            if (model.AssetRemarks != obj.AssetRemarks) ModifyModel.Append("备注修改为：" + obj.AssetRemarks + @"\r\n"); 
            if (model.AssetSJType != obj.AssetSJType) ModifyModel.Append("送检类型修改为：" + obj.AssetSJType + @"\r\n"); 
            if (model.AssetStatus != obj.AssetStatus) ModifyModel.Append("状态修改为：" + obj.AssetStatus + @"\r\n"); 
            if (model.OrgName != obj.OrgName) ModifyModel.Append("部门修改为：" + obj.OrgName + @"\r\n");
            if (model.AssetJZData2 != obj.AssetJZData2) ModifyModel.Append("校准数据修改为：" + obj.AssetJZData2 + @"\r\n");
            if (model.AssetSXDate != obj.AssetSXDate) ModifyModel.Append("失效日期修改为：" + obj.AssetSXDate + @"\r\n");
            if (model.AssetJZDate != obj.AssetJZDate) ModifyModel.Append("校准日期修改为：" + obj.AssetJZDate + @"\r\n");
            if (model.AssetJZCertificate != obj.AssetJZCertificate)
            {
                ModifyModel.Append("校准证书修改为：" + obj.AssetJZCertificate + @"\r\n");
                inspect = true;
            }
            UnitWork.UpdateAsync<Asset>(u => u.Id == obj.Id, u => new Asset
            {
                AssetAdmin = obj.AssetAdmin,
                AssetSXDate = obj.AssetSXDate,
                AssetJZDate = obj.AssetJZDate,
                AssetDescribe = obj.AssetDescribe,
                AssetHolder = obj.AssetHolder,
                AssetJZCertificate = obj.AssetJZCertificate,
                AssetJZData2 = obj.AssetJZData2,
                AssetRemarks = obj.AssetRemarks,
                AssetSJType = obj.AssetSJType,
                AssetStatus = obj.AssetStatus,
                OrgName = obj.OrgName
            });
            if (obj.Listcategory != null && obj.Listcategory.Count > 0)
            {
                UnitWork.BatchUpdate<AssetCategory>(obj.Listcategory.MapToList<AssetCategory>().ToArray());
            }
            //添加一条送检记录
            if (inspect)
            {
                var eassetinspectReq = new AddOrUpdateassetinspectReq();
                eassetinspectReq.AssetId = obj.Id;
                eassetinspectReq.AssetJZDate = obj.AssetJZDate;
                eassetinspectReq.AssetSXDate = obj.AssetSXDate;
                eassetinspectReq.AssetJZData1 = obj.AssetJZData1;
                eassetinspectReq.AssetJZData2 = obj.AssetJZData2;
                eassetinspectReq.AssetJZCertificate = obj.AssetJZCertificate;
                var InspectModel = eassetinspectReq.MapTo<AssetInspect>();
                UnitWork.Add<AssetInspect>(InspectModel);
                InspectId = InspectModel.Id;
            }
            //添加一条操作记录
            if (!string.IsNullOrEmpty(ModifyModel.ToString())) 
            {
                var eassetoperationReq = new AddOrUpdateassetoperationReq();
                eassetoperationReq.AssetId = obj.Id;
                eassetoperationReq.InspectId = InspectId;
                eassetoperationReq.OperationCZContent = ModifyModel.ToString();
                var OperationModel = eassetoperationReq.MapTo<AssetOperation>();
                OperationModel.OperationCZDate = DateTime.Now;
                var user = _auth.GetCurrentUser().User;
                OperationModel.OperationCZName = user.Name;
                UnitWork.Add<AssetOperation>(OperationModel);
            }
            try
            {
                UnitWork.Save();
            }
            catch (Exception)
            {
                throw new Exception("修改失败，请检查后重试");
            }
        }
        /// <summary>
        /// 查询字典
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public TableData GetListCategoryName(string ids)
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

            return _categoryapp.GetListCategoryName(ids);
        }

        /// <summary>
        /// 按名称模糊查询部门
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TableData GetListOrg(string name)
        {
            return _orgmanagerapp.GetListOrg(name);
        }

        /// <summary>
        /// 按名称模糊查询人员
        /// </summary>
        /// <param name="name"></param>
        /// <param name="Orgid"></param>
        /// <returns></returns>
        public TableData GetListUser(string name, string Orgid)
        {
            return _usermanagerapp.GetListUser(name, Orgid);
        }

        /// <summary>
        /// 计算计量特性
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string CalculateMetrological(List<AssetCategory> obj)
        {
            string Metrological = "";
            foreach (var item in obj.OrderBy(u=>u.CategoryNumber))
            {
                Metrological += item.CategoryNumber;
                if (item.CategoryType.Contains("绝对不确定度"))
                {
                    string CategoryNondeterminacy = Convert.ToDecimal(item.CategoryNondeterminacy).ToString("G0");
                    int r = 0;
                    for (int i = 0; i < CategoryNondeterminacy.Length; i++)
                    {
                        if (CategoryNondeterminacy.Substring(i, 1) != "." && Convert.ToInt32(CategoryNondeterminacy.Substring(i, 1)) > 0)
                        {
                            if (CategoryNondeterminacy.Contains("."))
                            {
                                int s = Convert.ToInt32(CategoryNondeterminacy.Substring(0, CategoryNondeterminacy.IndexOf(".")));
                                if (s > 0) i = i + 1;
                            }
                            r = i + 1;
                            break;
                        }
                    }
                    Metrological+="(" + Convert.ToDecimal(item.CategoryOhms).ToString("G0") + "±" + Convert.ToDecimal(CategoryNondeterminacy).ToString("E" + (CategoryNondeterminacy.ToString().Length - r)) + ")Ω (k="+ Convert.ToDecimal(item.CategoryBHYZ).ToString("G0") + @")\r\n";
                }
                else if (item.CategoryType.Contains("相对不确定度"))
                {
                    Metrological += Convert.ToDecimal(item.CategoryOhms).ToString("G0") + "m Ω,"+ Convert.ToDecimal(item.CategoryNondeterminacy).ToString("G0") + "ppm (k=" + Convert.ToDecimal(item.CategoryBHYZ).ToString("G0") + @")\r\n";
                }
            }

            return Metrological;
        }

        /// <summary>
        /// 根据出厂编号或资产编号查询数据(备用)
        /// </summary>
        /// <param name="AssetCCNumber"></param>
        /// <param name="AssetZCNumber"></param>
        /// <returns></returns>

        public AddOrUpdateAppReq GetAsset(string AssetCCNumber,string AssetZCNumber) 
        {
            var obj = UnitWork.Find<Asset>(u=>u.AssetCCNumber==AssetCCNumber).FirstOrDefault();
            if (obj == null) obj=UnitWork.Find<Asset>(u => u.AssetZCNumber == AssetZCNumber).FirstOrDefault();
            return obj.MapTo<AddOrUpdateAppReq>();
        }

    }
}
