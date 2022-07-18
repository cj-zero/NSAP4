using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;

namespace OpenAuth.App
{
    public class OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 用于事务操作
        /// </summary>
        /// <value>The unit work.</value>
        protected IUnitWork UnitWork;

        protected IAuth _auth;

        public OnlyUnitWorkBaeApp(IUnitWork unitWork, IAuth auth)
        {
            UnitWork = unitWork;
            _auth = auth;
        }

        /// <summary>
        ///  获取当前登录用户的数据访问权限
        /// </summary>
        /// <param name=""parameterName>linq表达式参数的名称，如u=>u.name中的"u"</param>
        /// <returns></returns>
        protected IQueryable<T> GetDataPrivilege<T>(string parametername) where T : class
        {
            var loginUser = _auth.GetCurrentUser();
            if (loginUser.User.Account == Define.SYSTEM_USERNAME) return UnitWork.Find<T>(null);  //超级管理员特权

            var moduleName = typeof(T).Name;
            var rule = UnitWork.FindSingle<DataPrivilegeRule>(u => u.SourceCode == moduleName);
            if (rule == null) return UnitWork.Find<T>(null); //没有设置数据规则，那么视为该资源允许被任何主体查看
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
            var rules = UnitWork.Find<DataPrivilegeRule>(u => u.SourceCode == moduleName).ToList();
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
    }
}
