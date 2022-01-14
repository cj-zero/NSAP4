namespace OpenAuth.App.Request
{
    public class QueryBeforeSaleDemandOperationHistoryListReq : PageReq
    {
        /// <summary>
        /// 关联售前需求Id
        /// </summary>
        public int BeforeSaleDemandId { get; set; }
    }
}