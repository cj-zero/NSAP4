using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    public class ClueListDto
    {
        public int Id { get; set; }
        /// <summary>
        ///线索编号
        /// </summary>
        public string SerialNumber { get; set; }
        /// <summary>
        /// 客户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 客户来源 0:领英、1:国内展会、2:国外展会、3:客户介绍、4:新威官网、5:其他
        /// </summary>
        public int CustomerSource { get; set; }
        /// <summary>
        ///备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 跟进时间
        /// </summary>
        public string FollowUpTime { get; set; } = "暂无跟进时间";
        /// <summary>
        /// 状态（0：销售线索，1：已转客户）
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// 未跟进天数
        /// </summary>
        public string DaysNotFollowedUp { get; set; } = "0天";
        /// <summary>
        /// 联系人名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 联系方式1
        /// </summary>
        public string Tel1 { get; set; }
        /// <summary>
        /// 详细地址国家
        /// </summary>
        public string Address1 { get; set; }
        /// <summary>
        /// 详细地址
        /// </summary>
        public string Address2 { get; set; }
        /// <summary>
        /// 标签集合
        /// </summary>
        public List<string> Tags { get; set; }
        /// <summary>
        /// 邮箱
        /// </summary>
        public string Email { get; set; }
    }
}
