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
        //public readonly AssetCategoryApp _categoryappapp;
        //public readonly AssetinspectApp _assetinspectapp;
        //public readonly AssetoperationApp _assetoperationapp;
        // AssetCategoryApp categoryappapp,, AssetoperationApp assetoperationapp, AssetinspectApp assetinspectapp
        public AssetApp(IUnitWork unitWork, IRepository<Asset> repository,CategoryApp categoryapp, OrgManagerApp orgmanagerapp, UserManagerApp usermanagerapp, IAuth auth) : base(unitWork, repository, auth)
        {
            _categoryapp = categoryapp;
            _orgmanagerapp = orgmanagerapp;
            _usermanagerapp = usermanagerapp;
            //_categoryappapp = categoryappapp;
            //_assetinspectapp = assetinspectapp;
            //_assetoperationapp = assetoperationapp;
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

            properties.Add(new KeyDescription { Key= "Metrological",Description="计量特性",Browsable=true,Type="String" });
            result.columnHeaders = properties;

            var CategoryObj = UnitWork.Find<AssetCategory>(null).Select(u => new
            {
                Id = u.Id,
                AssetId = u.AssetId,
                CategoryBHYZ = u.CategoryBHYZ,
                CategoryNondeterminacy = u.CategoryNondeterminacy,
                CategoryNumber = u.CategoryNumber,
                CategoryOhms = u.CategoryOhms,
                CategoryType = u.CategoryType
            });

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
                    Metrological = string.Join(',', CategoryObj.Where(u => u.AssetId == L.Id).ToList())
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
            if(req.AssetSerial.Count() < 2) 
            {
                req.AssetSerial = "0" + req.AssetSerial;
            }
            ZCNumber = "JZ-" + req.AssetSerial +"-"+ DateTime.Today.ToString("yy")+DateTime.Today.ToString("MM")+"-";
            var Listasset = UnitWork.Find<Asset>(null);
            var ZCNumberModel = Listasset.Where(u => u.AssetZCNumber.Contains(ZCNumber)).OrderByDescending(u => u.AssetCreateTime).FirstOrDefault();
            if (ZCNumberModel == null)
            {
                ZCNumber += "0001";
            }
            else 
            {
                var str= Listasset.First().AssetZCNumber;
                var Number = Convert.ToInt32(ZCNumberModel.AssetZCNumber.Substring(ZCNumberModel.AssetZCNumber.Length - 4, 4))+ 1;
                for (int i = 0; i < 4 - Number.ToString().Length; i++)
                {
                    ZCNumber += "0";
                }
                ZCNumber += Number.ToString();
            }
            obj.AssetZCNumber = ZCNumber;
            obj.AssetCreateTime = DateTime.Now;
            UnitWork.Add<Asset>(obj);
            //Repository.Add(obj);
            if (req.Listcategory != null && req.Listcategory.Count > 0) 
            {
                foreach (var item in req.Listcategory)
                {
                    var CategoryModel = item.MapTo<AssetCategory>();
                    CategoryModel.AssetId = obj.Id;
                    UnitWork.Add<AssetCategory>(CategoryModel);
                }
                //_categoryappapp.Add(req.Listcategory,obj.Id);
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
            //_assetinspectapp.Add(eassetinspectReq, ref InspectId);

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
            //_assetoperationapp.Add(eassetoperationReq);
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
            if (model.AssetAdmin != obj.AssetAdmin) ModifyModel.Append(" 管理员修改为：" + obj.AssetAdmin); 
            if (model.AssetDescribe != obj.AssetDescribe) ModifyModel.Append(" 描述修改为：" + obj.AssetDescribe); 
            if (model.AssetHolder != obj.AssetHolder) ModifyModel.Append(" 持有者修改为：" + obj.AssetHolder); 
            if (model.AssetImage != obj.AssetImage) ModifyModel.Append(" 图片修改为：" + obj.AssetImage); 
            if (model.AssetJSFile != obj.AssetJSFile) ModifyModel.Append(" 技术文件修改为：" + obj.AssetJSFile); 
            if (model.AssetRemarks != obj.AssetRemarks) ModifyModel.Append(" 备注修改为：" + obj.AssetRemarks); 
            if (model.AssetSJType != obj.AssetSJType) ModifyModel.Append(" 送检类型修改为：" + obj.AssetSJType); 
            if (model.AssetStatus != obj.AssetStatus) ModifyModel.Append(" 状态修改为：" + obj.AssetStatus); 
            if (model.OrgName != obj.OrgName) ModifyModel.Append(" 部门修改为：" + obj.OrgName);
            if (model.AssetJZCertificate != obj.AssetJZCertificate)
            {
                ModifyModel.Append(" 校准证书修改为：" + obj.AssetJZCertificate); 
                inspect = true;
            } 
            if (model.AssetJZData1 != obj.AssetJZData1) ModifyModel.Append(" 校准数据修改为：" + obj.AssetJZData1);
            if (model.AssetJZData2 != obj.AssetJZData2) ModifyModel.Append(" 校准数据修改为：" + obj.AssetJZData2);
            if (model.AssetSXDate != obj.AssetSXDate) ModifyModel.Append(" 失效日期修改为：" + obj.AssetSXDate);
            if (model.AssetJZDate != obj.AssetJZDate) ModifyModel.Append(" 校准日期修改为：" + obj.AssetJZDate);

            UnitWork.Update<Asset>(u => u.Id == obj.Id, u => new Asset
            {
                AssetAdmin = obj.AssetAdmin,
                AssetSXDate = obj.AssetSXDate,
                AssetJZDate = obj.AssetJZDate,
                AssetDescribe = obj.AssetDescribe,
                AssetHolder = obj.AssetHolder,
                AssetImage = obj.AssetImage,
                AssetJSFile = obj.AssetJSFile,
                AssetJZCertificate = obj.AssetJZCertificate,
                AssetJZData1 = obj.AssetJZData1,
                AssetJZData2 = obj.AssetJZData2,
                AssetRemarks = obj.AssetRemarks,
                AssetSJType = obj.AssetSJType,
                AssetStatus = obj.AssetStatus,
                OrgName = obj.OrgName
            });
            if (obj.Listcategory != null && obj.Listcategory.Count > 0)
            {
                foreach (var item in obj.Listcategory)
                {
                    var Categorys = item.MapTo<AssetCategory>();
                    UnitWork.Update<AssetCategory>(u => u.Id == Categorys.Id, u => new AssetCategory
                    {
                        CategoryBHYZ = Categorys.CategoryBHYZ,
                        CategoryNondeterminacy = Categorys.CategoryNondeterminacy,
                        CategoryNumber = Categorys.CategoryNumber,
                        CategoryOhms = Categorys.CategoryOhms,
                        CategoryType = Categorys.CategoryType
                    });
                }
                //_categoryappapp.UpDate(obj.Listcategory);
            }
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
                //_assetinspectapp.Add(eassetinspectReq, ref InspectId);
            }
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
                //_assetoperationapp.Add(eassetoperationReq);
                //添加一条操作记录
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

    }
}
