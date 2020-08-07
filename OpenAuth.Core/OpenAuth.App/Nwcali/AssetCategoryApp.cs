using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.nwcali
{

    public class AssetCategoryApp : BaseApp<AssetCategory>
    {
        public AssetCategoryApp(IUnitWork unitWork, IRepository<AssetCategory> repository, IAuth auth) : base(unitWork, repository, auth)
        {
        }
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public void Add(List<AddOrUpdateassetcategoryReq> req,string assetid)
        {
            foreach (var item in req)
            {
                var obj = item.MapTo<AssetCategory>();
                obj.AssetId = assetid;
                UnitWork.AddAsync<AssetCategory>(obj);
            }
            UnitWork.SaveAsync();
            
        }
        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        public void UpDate(List<AddOrUpdateassetcategoryReq> req)
        {
            foreach (var item in req)
            {
                var obj = item.MapTo<AssetCategory>();
                UnitWork.UpdateAsync<AssetCategory>(u => u.Id == obj.Id, u => new AssetCategory
                {
                    CategoryBHYZ = obj.CategoryBHYZ,
                    CategoryNondeterminacy = obj.CategoryNondeterminacy,
                    CategoryNumber=obj.CategoryNumber,
                    CategoryOhms=obj.CategoryOhms,
                    CategoryType=obj.CategoryType
                });
            }
        }
    }
}
