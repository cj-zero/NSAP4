using System;

namespace OpenAuth.App
{
    public static class Define
    {
        public static string USERROLE = "UserRole";       //用户角色关联KEY
        public const string ROLERESOURCE= "RoleResource";  //角色资源关联KEY
        public const string USERORG = "UserOrg";  //用户机构关联KEY
        public const string ROLEELEMENT = "RoleElement"; //角色菜单关联KEY
        public const string ROLEMODULE = "RoleModule";   //角色模块关联KEY
        public const string ROLEDATAPROPERTY = "RoleDataProperty";   //角色数据字段权限
        public const string USERAPP = "App"; //App登录账号

        public const string DBTYPE_SQLSERVER = "SqlServer";    //sql server
        public const string DBTYPE_MYSQL = "MySql";    //sql server


        public const int INVALID_TOKEN = 50014;     //token无效
        public const int INVALID_InvoiceNumber = 50015;   //发票号码不唯一
        public const int INVALID_ReimburseAgain = 50016;   //重复提交报销单
        public const int INVALID_APPUser = 50017;     //未绑定App账户
        public const int ExpressNum_IsNull = 50018;     //快递单号为空
        public const int IS_OverTime = 50019;//时间超时
        public const int IS_Return_Finish = 50020;//退料完成
        public const int Express_NotFound = 50021;//快递已不存在
        public const string TOKEN_NAME = "X-Token";


        public const string SYSTEM_USERNAME = "ErpAdmin";
        public const string SYSTEM_USERPWD = "newareerp";

        public const string DATAPRIVILEGE_LOGINUSER = "{loginUser}";  //数据权限配置中，当前登录用户的key
        public const string DATAPRIVILEGE_LOGINROLE = "{loginRole}";  //数据权限配置中，当前登录用户角色的key
        public const string DATAPRIVILEGE_LOGINORG = "{loginOrg}";  //数据权限配置中，当前登录用户部门的key

        public const string JOBMAPKEY = "OpenJob";

        public const int SBO_ID = 1;//ERP3.0当前账套

    }
}
