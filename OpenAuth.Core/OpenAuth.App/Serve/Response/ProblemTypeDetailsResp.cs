using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    [AutoMapTo(typeof(ProblemType))]
    public class ProblemTypeDetailsResp
    {
        /// <summary>
        /// 问题名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 问题描述
        /// </summary>
        public string Description { get; set; }
    }
}
