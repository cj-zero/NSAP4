using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Core;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App
{
    /// <summary>
    /// 业务层基类，UnitWork用于事务操作，Repository用于普通的数据库操作
    /// <para>如用户管理：Class UserManagerApp:BaseApp<User></para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BaseApp<T> where T : Entity
    {
        /// <summary>
        /// 用于普通的数据库操作
        /// </summary>
        /// <value>The repository.</value>
        protected IRepository<T> Repository;

        /// <summary>
        /// 用于事务操作
        /// </summary>
        /// <value>The unit work.</value>
        protected IUnitWork UnitWork;

        protected IAuth _auth;

        public BaseApp(IUnitWork unitWork, IRepository<T> repository, IAuth auth)
        {
            UnitWork = unitWork;
            Repository = repository;
            _auth = auth;
        }

        /// <summary>
        ///  获取当前登录用户的数据访问权限
        /// </summary>
        /// <param name="parameterName">linq表达式参数的名称，如u=>u.name中的"u"</param>
        /// <returns></returns>
        protected IQueryable<T> GetDataPrivilege(string parametername)
        {
            var loginUser = _auth.GetCurrentUser();
            if (loginUser.User.Account == Define.SYSTEM_USERNAME) return UnitWork.Find<T>(null);  //超级管理员特权
            
            var moduleName = typeof(T).Name;
            var rule = UnitWork.FindSingle<DataPrivilegeRule>(u => u.SourceCode == moduleName);
            if (rule == null) return UnitWork.Find<T>(null); //没有设置数据规则，那么视为该资源允许被任何主体查看
            if (rule.PrivilegeRules.Contains(Define.DATAPRIVILEGE_LOGINUSER) ||
                                             rule.PrivilegeRules.Contains(Define.DATAPRIVILEGE_LOGINROLE)||
                                             rule.PrivilegeRules.Contains(Define.DATAPRIVILEGE_LOGINORG))
            {
                
                //即把{loginUser} =='xxxxxxx'换为 loginUser.User.Id =='xxxxxxx'，从而把当前登录的用户名与当时设计规则时选定的用户id对比
                rule.PrivilegeRules = rule.PrivilegeRules.Replace(Define.DATAPRIVILEGE_LOGINUSER, loginUser.User.Id);
                
                var roles = loginUser.Roles.Select(u => u.Id).ToList();
                roles.Sort(); //按字母排序,这样可以进行like操作
                rule.PrivilegeRules = rule.PrivilegeRules.Replace(Define.DATAPRIVILEGE_LOGINROLE, 
                    string.Join(',',roles));
                
                var orgs = loginUser.Orgs.Select(u => u.Id).ToList();
                orgs.Sort(); 
                rule.PrivilegeRules = rule.PrivilegeRules.Replace(Define.DATAPRIVILEGE_LOGINORG, 
                    string.Join(',',orgs));
            }
            return UnitWork.Find<T>(null).GenerateFilter(parametername,
                JsonHelper.Instance.Deserialize<FilterGroup>(rule.PrivilegeRules));
        }
        /// <summary>
        ///  获取当前登录用户的数据访问权限
        /// </summary>
        /// <param name="parameterName">linq表达式参数的名称，如u=>u.name中的"u"</param>
        /// <returns></returns>
        protected string GetDataPrivilegeNew(string moduleName)
        {
            var loginUser = _auth.GetCurrentUser();
            if (loginUser.User.Account == Define.SYSTEM_USERNAME) return " 1=1 ";  //超级管理员特权

            //var moduleName = typeof(T).Name;
            var rules = UnitWork.Find<DataPrivilegeRule>(u => u.SourceCode == moduleName && u.Enable == true).ToList();
            if (rules == null) return " 1=1 "; //没有设置数据规则，那么视为该资源允许被任何主体查看
            string filter = "";
            foreach (var rule in rules)
            {
                if (rule.PrivilegeRules.Contains(Define.DATAPRIVILEGE_LOGINUSER) ||
                                                 rule.PrivilegeRules.Contains(Define.DATAPRIVILEGE_LOGINROLE) ||
                                                 rule.PrivilegeRules.Contains(Define.DATAPRIVILEGE_LOGINORG))
                {

                    //即把{loginUser} =='xxxxxxx'换为 loginUser.User.Id =='xxxxxxx'，从而把当前登录的用户名与当时设计规则时选定的用户id对比
                    rule.PrivilegeRules = rule.PrivilegeRules.Replace(Define.DATAPRIVILEGE_LOGINUSER, loginUser.User.Id);

                    var roles = loginUser.Roles.Select(u => u.Id).ToList();
                    roles.Sort(); //按字母排序,这样可以进行like操作
                    rule.PrivilegeRules = rule.PrivilegeRules.Replace(Define.DATAPRIVILEGE_LOGINROLE,
                        string.Join(',', roles));

                    var orgs = loginUser.Orgs.Select(u => u.Id).ToList();
                    orgs.Sort();
                    rule.PrivilegeRules = rule.PrivilegeRules.Replace(Define.DATAPRIVILEGE_LOGINORG,
                        string.Join(',', orgs));
                }
                var filters = JsonHelper.Instance.Deserialize<FilterList>(rule.PrivilegeRules);
                bool tof = false;
                foreach (var item in filters.FilterUser)
                {
                    //一个条件不满足即跳出
                    tof = DynamicLinq.ConvertCondition(item).Tof;
                    if (!tof) break;
                }
                //满足过滤用户再进行数据过滤
                if (tof)
                {
                    foreach (var item in filters.FilterData)
                    {
                        filter += DynamicLinq.ConvertCondition(item).Condition;
                    }
                }

            }
            //return UnitWork.Find<T>(null).GenerateFilter(parametername,JsonHelper.Instance.Deserialize<FilterGroup>(rule.PrivilegeRules));
            return !string.IsNullOrWhiteSpace(filter) ? filter : " 1!=1";
        }

        /// <summary>
        /// 按id批量删除
        /// </summary>
        /// <param name="ids"></param>
        public virtual void Delete(string[] ids)
        {
            Repository.Delete(u => ids.Contains(u.Id));
        }
        /// <summary>
        /// 按条件删除
        /// </summary>
        /// <param name="ids"></param>
        public virtual void Delete(Expression<Func<T, bool>> exp)
        {
            Repository.Delete(exp);
        }
        /// <summary>
        /// 按id批量删除
        /// </summary>
        /// <param name="ids"></param>
        public async Task DeleteAsync(string[] ids, CancellationToken cancellationToken = default)
        {
            await Repository.DeleteAsync(u => ids.Contains(u.Id), cancellationToken);
        }
        /// <summary>
        /// 按条件删除
        /// </summary>
        /// <param name="ids"></param>
        public async System.Threading.Tasks.Task DeleteAsync(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default)
        {
            await Repository.DeleteAsync(exp, cancellationToken);
        }

        public T Get(string id)
        {
            return Repository.FindSingle(u => u.Id == id);
        }
        public T Get(Expression<Func<T, bool>> exp)
        {
            return Repository.FindSingle(exp);
        }
        public Task<T> GetAsync(string id, CancellationToken cancellationToken = default)
        {
            return Repository.FindSingleAsync(u => u.Id == id, cancellationToken);
        }
        public Task<T> GetAsync(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default)
        {
            return Repository.FindSingleAsync(exp, cancellationToken);
        }

        /// <summary>
        /// 如果一个类有层级结构（树状），则修改该节点时，要修改该节点的所有子节点
        /// //修改对象的级联ID，生成类似XXX.XXX.X.XX
        /// </summary>
        /// <typeparam name="U">U必须是一个继承TreeEntity的结构</typeparam>
        /// <param name="entity"></param>
        public void CaculateCascade<U>(U entity) where U : TreeEntity
        {
            if (entity.ParentId == "") entity.ParentId = null;
            string cascadeId;
            int currentCascadeId = 1; //当前结点的级联节点最后一位
            var sameLevels = UnitWork.Find<U>(o => o.ParentId == entity.ParentId && o.Id != entity.Id);
            foreach (var obj in sameLevels)
            {
                int objCascadeId = int.Parse(obj.CascadeId.TrimEnd('.').Split('.').Last());
                if (currentCascadeId <= objCascadeId) currentCascadeId = objCascadeId + 1;
            }

            if (!string.IsNullOrEmpty(entity.ParentId))
            {
                var parentOrg = UnitWork.FindSingle<U>(o => o.Id == entity.ParentId);
                if (parentOrg != null)
                {
                    cascadeId = parentOrg.CascadeId + currentCascadeId + ".";
                    entity.ParentName = parentOrg.Name;
                }
                else
                {
                    throw new Exception("未能找到该组织的父节点信息");
                }
            }
            else
            {
                cascadeId = ".0." + currentCascadeId + ".";
                entity.ParentName = "根节点";
            }

            entity.CascadeId = cascadeId;
        }
        /// <summary>
        /// 如果一个类有层级结构（树状），则修改该节点时，要修改该节点的所有子节点
        /// //修改对象的级联ID，生成类似XXX.XXX.X.XX
        /// </summary>
        /// <typeparam name="U">U必须是一个继承TreeEntity的结构</typeparam>
        /// <param name="entity"></param>
        public async Task CaculateCascadeAsync<U>(U entity) where U : TreeEntity
        {
            if (entity.ParentId == "") entity.ParentId = null;
            string cascadeId;
            int currentCascadeId = 1; //当前结点的级联节点最后一位
            var sameLevels = await UnitWork.Find<U>(o => o.ParentId == entity.ParentId && o.Id != entity.Id).ToListAsync();
            foreach (var obj in sameLevels)
            {
                int objCascadeId = int.Parse(obj.CascadeId.TrimEnd('.').Split('.').Last());
                if (currentCascadeId <= objCascadeId) currentCascadeId = objCascadeId + 1;
            }

            if (!string.IsNullOrEmpty(entity.ParentId))
            {
                var parentOrg = await UnitWork.FindSingleAsync<U>(o => o.Id == entity.ParentId);
                if (parentOrg != null)
                {
                    cascadeId = parentOrg.CascadeId + currentCascadeId + ".";
                    entity.ParentName = parentOrg.Name;
                }
                else
                {
                    throw new Exception("未能找到该组织的父节点信息");
                }
            }
            else
            {
                cascadeId = ".0." + currentCascadeId + ".";
                entity.ParentName = "根节点";
            }

            entity.CascadeId = cascadeId;
        }

        public bool CheckExist(Expression<Func<T, bool>> exp)
        {
            return Repository.IsExist(exp);
        }
        public Task<bool> CheckExistAsync(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default)
        {
            return Repository.IsExistAsync(exp, cancellationToken);
        }

        public List<T> GetAll(Expression<Func<T, bool>> exp)
        {
            return Repository.Find(exp).ToList();
        }
        public Task<List<T>> GetAllAsync(Expression<Func<T, bool>> exp, CancellationToken cancellationToken = default)
        {
            return Repository.Find(exp).ToListAsync(cancellationToken);
        }
    }
}