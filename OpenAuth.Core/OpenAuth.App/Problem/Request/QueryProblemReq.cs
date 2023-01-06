using System;
using System.Collections.Generic;
using OpenAuth.App.Request;

namespace OpenAuth.App.Problem.Request
{
    public class QueryProblemReq : PageReq
    {
        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortName { get; set; }

        /// <summary>
        /// desc 降序，asc 升序
        /// </summary>
        public string SortOrder { get; set; }

        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 菜单模块
        /// </summary>
        public string MenuModel { get; set; }

        /// <summary>
        /// 更新人
        /// </summary>
        public string UpdateUserName { get; set; }

        /// <summary>
        /// 意见建议
        /// </summary>
        public string ProblemSugg { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUserName { get; set; }

        /// <summary>
        /// 处理状态
        /// </summary>
        public string ProcessStatus { get; set; }

        /// <summary>
        /// 开始日期
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// 结束日期
        /// </summary>
        public DateTime? EndDate { get; set; }
    }






}