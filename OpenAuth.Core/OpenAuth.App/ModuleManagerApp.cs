using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using Infrastructure.Extensions;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App
{
    public class ModuleManagerApp : BaseTreeApp<Module>
    {
        private RevelanceManagerApp _revelanceApp;
        public void Add(Module model)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            CaculateCascade(model);
            
            Repository.Add(model);
            //当前登录用户的所有角色自动分配模块
            //loginContext.Roles.ForEach(u =>
            //{    
            //    _revelanceApp.Assign(new AssignReq
            //    {
            //        type=Define.ROLEMODULE,
            //        firstId = u.Id,
            //        secIds = new[]{model.Id}
            //    });
            //});
           
        }

        public void Update(Module obj)
        {
            UpdateTreeObj(obj);
        }

        public List<ModuleView> LoadModuleAll()
        {
            var modules = (from module in UnitWork.Find<Module>(null)
                           select new ModuleView
                           {
                               SortNo = module.SortNo,
                               Name = module.Name,
                               Id = module.Id,
                               CascadeId = module.CascadeId,
                               Code = module.Code,
                               IconName = module.IconName,
                               Url = module.Url,
                               ParentId = module.ParentId,
                               ParentName = module.ParentName,
                               IsSys = module.IsSys,
                               Status = module.Status
                           }).ToList();
            return modules;
        }

        public TableData LoadModuleForTree()
        {
            TableData res = new TableData();
            var modules = LoadModuleAll();
            var tree = modules.GenerateTree(u => u.Id, u => u.ParentId);
            List<ModuleTree> moduleTree = new List<ModuleTree>();
            lllls(tree, moduleTree);
            void lllls(IEnumerable<TreeItem<ModuleView>> items, List<ModuleTree> list2)
            {
                foreach (var item in items)
                {
                    ModuleTree moduleTree = new ModuleTree();
                    if (item.Item != null)
                    {
                        moduleTree.Code = item.Item.Code;
                        moduleTree.Name = item.Item.Name;
                        moduleTree.Children = new List<ModuleTree>();
                        list2.Add(moduleTree);
                        lllls(item.Children, moduleTree.Children);
                    }
                }
            }
            res.Data = moduleTree;
            return res;
        }
        #region 用户/角色分配模块


        /// <summary>
        /// 加载特定角色的模块
        /// </summary>
        /// <param name="roleId">The role unique identifier.</param>
        public IEnumerable<Module> LoadForRole(string roleId)
        {
            var moduleIds = UnitWork.Find<Relevance>(u => u.FirstId == roleId && u.Key == Define.ROLEMODULE)
                .Select(u => u.SecondId);
            return UnitWork.Find<Module>(u => moduleIds.Contains(u.Id)).OrderBy(u => u.SortNo);
        }

        /// <summary>
        /// 加载有菜单权限的角色
        /// </summary>
        /// <param name="moduleId"></param>
        /// <returns></returns>
        public IEnumerable<Role> LoadForRoleModule(string moduleId)
        {
            var roleIds = UnitWork.Find<Relevance>(u => u.SecondId == moduleId && u.Key == Define.ROLEMODULE)
                .Select(u => u.FirstId);
            return UnitWork.Find<Role>(u => roleIds.Contains(u.Id));
        }

        //获取角色可访问的模块字段
        public IEnumerable<string> LoadPropertiesForRole(string roleId, string moduleCode)
        {
            return _revelanceApp.Get(Define.ROLEDATAPROPERTY, roleId, moduleCode);
        }

        public IEnumerable<ModuleElement> LoadMenusForRole(string moduleId, string roleId)
        {
            var elementIds = _revelanceApp.Get(Define.ROLEELEMENT, true, roleId);
            var query = UnitWork.Find<ModuleElement>(u => elementIds.Contains(u.Id));
            if (!string.IsNullOrEmpty(moduleId))
            {
               query =  query.Where(u => u.ModuleId == moduleId);
            }

            return query;
        }

        #endregion 用户/角色分配模块


        #region 菜单操作
        /// <summary>
        /// 删除指定的菜单
        /// </summary>
        /// <param name="ids"></param>
        public void DelMenu(string[] ids)
        {
            UnitWork.Delete<ModuleElement>(u => ids.Contains(u.Id));
            UnitWork.Save();
        }

        public void AddMenu(ModuleElement model)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            UnitWork.Add(model);
            
            //当前登录用户的所有角色自动分配菜单
            //loginContext.Roles.ForEach(u =>
            //{    
            //    _revelanceApp.Assign(new AssignReq
            //    {
            //        type=Define.ROLEELEMENT,
            //        firstId = u.Id,
            //        secIds = new[]{model.Id}
            //    });
            //});
            UnitWork.Save();
        }
        #endregion

        public void UpdateMenu(ModuleElement model)
        {
            UnitWork.Update<ModuleElement>(model);
            UnitWork.Save();
        }

        #region 菜单字段
        public List<ModuleField> LoadModuleField(string moduleId, string key, string description)
        {
            var query = UnitWork.Find<ModuleField>(c => c.ModuleId == moduleId || c.ModuleCode == moduleId)
                .WhereIf(!string.IsNullOrWhiteSpace(key), c => c.Key.Contains(key) || c.Description.Contains(key))
                .ToList();
            return query;
        }

        /// <summary>
        /// 添加菜单字段
        /// </summary>
        public void AddMenuField(ModuleField model)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            UnitWork.Add(model);
            UnitWork.Save();
        }

        /// <summary>
        /// 修改菜单字段
        /// </summary>
        public void UpdateMenuField(ModuleField model)
        {
            UnitWork.Update<ModuleField>(model);
            UnitWork.Save();
        }

        /// <summary>
        /// 删除菜单字段
        /// </summary>
        /// <param name="ids"></param>
        public void DelMenuField(string id)
        {
            UnitWork.Delete<ModuleField>(u => u.Id == id);
            UnitWork.Save();
        }

        /// <summary>
        /// 获取模块下字段
        /// </summary>
        /// <param name="moduleCode"></param>
        /// <returns></returns>
        public List<KeyDescription> GetProperties(string moduleCode,string roleId)
        {
            var allprops = UnitWork.Find<ModuleField>(c => c.ModuleCode == moduleCode).Select(c => new KeyDescription { Key = c.Key, Description = c.Description, SortNo = c.SortNo }).ToList();
            var props = UnitWork.Find<Relevance>(c => c.FirstId == roleId && c.SecondId == moduleCode && c.Key == Define.ROLEDATAPROPERTY).ToList();
            allprops.ForEach(c =>
            {
                c.Permission = props.Where(p => p.ThirdId == c.Key).FirstOrDefault()?.ExtendInfo;
            });
            //var allprops = from a in UnitWork.Find<ModuleField>(null)
            //               join b in UnitWork.Find<Relevance>(null) on new { a.ModuleCode, Feild = a.Key } equals new { ModuleCode = b.SecondId, Feild = b.ThirdId } into ab
            //               from b in ab.DefaultIfEmpty()
            //               where a.ModuleCode == moduleCode && b.SecondId == moduleCode && b.FirstId == roleId
            //               select new KeyDescription
            //               {
            //                   Key = a.Key,
            //                   Description = a.Description,
            //                   SortNo = a.SortNo,
            //                   Permission = b == null ? null : b.ExtendInfo
            //               };

            return allprops;
        }

        #endregion

        public ModuleManagerApp(IUnitWork unitWork, IRepository<Module> repository
        ,RevelanceManagerApp app,IAuth auth) : base(unitWork, repository, auth)
        {
            _revelanceApp = app;
        }
    }
}