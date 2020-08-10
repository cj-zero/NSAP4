namespace OpenAuth.App.Request
{
    public class QueryassetListReq : PageReq
    {
        /// <summary>
        /// 资产ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string AssetCategory { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string AssetStatus { get; set; }
        /// <summary>
        /// 送检类型
        /// </summary>
        public string AssetSJType { get; set; }
        /// <summary>
        /// 型号
        /// </summary>
        public string AssetType { get; set; }
        /// <summary>
        /// 出厂编号S/N
        /// </summary>
        public string AssetCCNumber { get; set; }
        /// <summary>
        /// 资产编号
        /// </summary>
        public string AssetZCNumber { get; set; }
        /// <summary>
        /// 校准日期
        /// </summary>
        public System.DateTime? AssetJZDate { get; set; }
        /// <summary>
        /// 失效日期
        /// </summary>
        public System.DateTime? AssetSXDate { get; set; }
        //todo:添加自己的请求字段
    }
}