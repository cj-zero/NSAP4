namespace OpenAuth.App
{
    /// <summary>
    /// 配置项
    /// </summary>
    public class AppSetting
    {

        public AppSetting()
        {
            SSOPassport = "http://localhost:52789";
            Version = "";
            UploadPath = "";
            IdentityServerUrl = "";
            DbType = Define.DBTYPE_SQLSERVER;
        }
        /// <summary>
        /// SSO地址
        /// </summary>
        public string SSOPassport { get; set; }

        /// <summary>
        /// 版本信息
        /// 如果为demo,则屏蔽Post请求
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 数据库类型 SqlServer、MySql
        /// </summary>
        public string DbType { get; set; }

        /// <summary> 附件上传路径</summary>
        public string UploadPath { get; set; }

        //identity授权的地址
        public string IdentityServerUrl { get; set; }

        //是否是Identity授权方式
        public bool IsIdentityAuth => !string.IsNullOrEmpty(IdentityServerUrl);

        /// <summary>
        /// App API接口地址
        /// </summary>
        public string AppPushMsgUrl { get; set; }

        /// <summary>
        /// App版本号
        /// </summary>
        public string AppVersion { get; set; }

        /// <summary>
        /// App 售后服务地址
        /// </summary>
        public string AppServerUrl { get; set; }
        /// <summary>
        /// ERP3.0地址
        /// </summary>
        public string ERP3Url { get; set; }

        /// <summary>
        /// 票据识别类型 腾讯/华为
        /// </summary>
        public string OcrType { get; set; }

        /// <summary>
        /// grpc或mqtt登录者
        /// </summary>
        public static string GrpcOrMqttUserName { get; set; }
        /// <summary>
        /// grpc或mqtt密码
        /// </summary>
        public static string GrpcOrMqttPwd { get; set; }
        /// <summary>
        /// grpc的远程ip地址
        /// </summary>
        public static string GrpcIP { get; set; }
        /// <summary>
        /// grpc连接端口
        /// </summary>
        public static string GrpcPort { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public  string PassPortUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PassPortClientId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PassPortClientSecret { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string PassPortScope { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string EnterpriseIds { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string AnalyticsUrl { get; set; }
    }
}