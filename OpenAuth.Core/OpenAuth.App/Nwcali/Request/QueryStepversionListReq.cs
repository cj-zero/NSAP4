namespace OpenAuth.App.Request
{
    public class QueryStepversionListReq : PageReq
    {
        /// <summary>
        /// 工步名称
        /// </summary>
        public string StepName { get; set; }
        /// <summary>
        /// 系列名称
        /// </summary>
        public string SeriesName { get; set; }
        /// <summary>
        /// 工步型号名称
        /// </summary>
        public string StepVersionName { get; set; }
        /// <summary>
        /// 电流
        /// </summary>
        public int Current { get; set; }
    }
}