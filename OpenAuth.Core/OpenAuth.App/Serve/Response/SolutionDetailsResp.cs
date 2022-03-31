using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [AutoMapTo(typeof(Solution))]
    public class SolutionDetailsResp
    {
        /// <summary>
        ///id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 编号
        /// </summary>
        public int SltCode { get; set; }
        /// <summary>
        /// 解决方案
        /// </summary>
        public string Subject { get; set; }
        /// <summary>
        /// 原因
        /// </summary>
        public string Cause { get; set; }
        /// <summary>
        /// 症状
        /// </summary>
        public string Symptom { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string Descriptio { get; set; }

        /// <summary>
        /// 解决方案编码
        /// </summary>
        public string Code { get; set; }
    }

    public class SolutionInfo
    {
        public string Code { get; set; }
        public string Desc { get; set; }
        public int Count { get; set; }
    }
}
