using System;
using System.Linq;
using Infrastructure;
using OpenAuth.App.Interface;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;


namespace OpenAuth.App
{
    public class ReturnnoteApp : OnlyUnitWorkBaeApp
    {

        /// <summary>
        /// 加载列表
        /// </summary>
        public TableData Load(QueryReturnnoteListReq request)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
            throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }
            
            var properties = loginContext.GetProperties("Returnnote");
            
            if (properties == null || properties.Count == 0)
            {
            throw new Exception("当前登录用户没有访问该模块字段的权限，请联系管理员配置");
            }
                        
            var result = new TableData();
            return result;
        }

        public void Add(AddOrUpdateReturnnoteReq obj)
        {
            //程序类型取入口应用的名称，可以根据自己需要调整
            var addObj = obj.MapTo<ReturnnoteApp>();
        }
        
        public void Update(AddOrUpdateReturnnoteReq obj)
        {

        }

        public ReturnnoteApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
        }
    }
}