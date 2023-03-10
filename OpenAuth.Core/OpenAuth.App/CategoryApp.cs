using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class CategoryApp : BaseApp<Category>
    {
        /// <summary>
        /// 加载列表
        /// </summary>
        public async Task<TableData> Load(QueryCategoryListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var properties = loginContext.GetProperties("Category");

            //if (properties == null || properties.Count == 0)
            //{
            //    throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            //}
            
            var result = new TableData();
            var objs = UnitWork.Find<Category>(null);
            if (!string.IsNullOrEmpty(request.TypeId))
            {
                objs = objs.Where(u => u.TypeId == request.TypeId);
            }
            
            if (!string.IsNullOrEmpty(request.key))
            {
                objs = objs.Where(u => u.Id.Contains(request.key) || u.Name.Contains(request.key));
            }

            var propertyStr = string.Join(',', properties.Select(u =>u.Key));
            result.columnHeaders = properties;
            result.Data = await objs.OrderBy(u => u.SortNo)
                .Skip((request.page - 1) * request.limit)
                .Take(request.limit).ToListAsync();//.Select($"new ({propertyStr})");
            result.Count = await objs.CountAsync();
            return result;
        }

        public void Add(AddOrUpdateCategoryReq req)
        {
            var obj = req.MapTo<Category>();
            obj.CreateTime = DateTime.Now;
            var user = _auth.GetCurrentUser().User;
            obj.CreateUserId = user.Id;
            obj.CreateUserName = user.Name;
            Repository.Add(obj);
        }
        
        public void Update(AddOrUpdateCategoryReq obj)
        {
            var user = _auth.GetCurrentUser().User;
            UnitWork.Update<Category>(u => u.Id == obj.Id, u => new Category
            {
                Enable = obj.Enable,
                DtValue = obj.DtValue,
                DtCode = obj.DtCode,
                TypeId = obj.TypeId,
                Name=obj.Name,
                SortNo=obj.SortNo,
                Description=obj.Description,
                UpdateTime = DateTime.Now,
                UpdateUserId = user.Id,
                UpdateUserName = user.Name
               //todo:要修改的字段赋值
            });

        }

        /// <summary>
        /// 按字典ID查询字典类型 by zlg 2020.7.31（暂用）
        /// </summary>
        public async Task<TableData> GetListCategoryName(string ids)
        {
            var result = new TableData();
            var objs = UnitWork.Find<Category>(null);
            objs = objs.Where(u => ids.Contains(u.TypeId)).OrderBy(u => u.CreateTime);
            result.Data = await objs.Select(u => new { u.Name, u.TypeId,u.DtValue,u.Description}).ToListAsync();
            return result;
        }

        /// <summary>
        /// 按字典ID查询字典类型 by zlg 2020.11.13（最新）
        /// </summary>
        public async Task<TableData> GetCategoryNameList(List<string> ids)
        {
            var result = new TableData();
            var objs = UnitWork.Find<Category>(null);
            objs = objs.Where(u => ids.Contains(u.TypeId)).OrderBy(u => u.CreateTime);
            result.Data = await objs.Select(u => new { u.Name, u.TypeId, u.DtValue, u.Description }).ToListAsync();
            return result;
        }

        public List<CategoryType> AllTypes()
        {
            return UnitWork.Find<CategoryType>(null).ToList();
        }

        /// <summary>
        /// 加载一个分类类型里面的所有值，即字典的所有值
        /// </summary>
        /// <param name="typeId"></param>
        /// <returns></returns>
        public List<Category> LoadByTypeId(string typeId)
        {
            return Repository.Find(u => u.TypeId == typeId).ToList();
        }

        public CategoryApp(IUnitWork unitWork, IRepository<Category> repository,IAuth auth) : base(unitWork, repository, auth)
        {
        }
    }
}