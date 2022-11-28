using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class AllowSendOrderUserResp
    {
        /// <summary>
        /// app用户Id
        /// </summary>
        public int? AppUserId { get; set; }
        /// <summary>
        /// NSAP用户Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 已经接的服务单数量
        /// </summary>
        public int? Count { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 省
        /// </summary>
        public string Province { get; set; }

        /// <summary>
        /// 市
        /// </summary>
        public string City { get; set; }

        /// <summary>
        /// 区
        /// </summary>
        public string Area { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        public string Addr { get; set; }


        /// <summary>
        /// 距离
        /// </summary>
        public double Distance { get; set; }
        /// <summary>
        /// 擅长技能
        /// </summary>
        public string SkillName { get; set; }

        /// <summary>
        /// 等级
        /// </summary>
        public string GradeName { get; set; }


    }
}
