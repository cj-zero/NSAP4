using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.Request
{
    /// <summary>
    /// 编辑线索model
    /// </summary>
    public class UpdateClueReq
    {
        public int Id { get; set; }

        /// <summary>
        /// 用户名称
        /// </summary>
        public string CardName { get; set; }
        /// <summary>
        /// 客户来源 0:领英、1:国内展会、2:国外展会、3:客户介绍、4:新威官网、5:其他
        /// </summary>
        public string CustomerSource { get; set; }
        /// <summary>
        /// 所属行业
        /// 0:农、林、牧、渔业,
        /// 1,制造业(57)
        /// 2电力、热力、燃气及水生产和供应业(4)
        /// 3建筑业(6)
        /// 4批发和零售业(101)
        /// 5交通运输、仓储和邮政业(1)
        /// 6住宿和餐饮业(2)
        /// 7信息传输、软件和信息技术服务业(7)
        /// 8金融业(2)
        /// 9房地产业(4)
        /// 10租赁和商务服务业(4)
        /// 11科学研究和技术服务业(47)
        /// 12水利、环境和公共设施管理业(2)
        /// 13居民服务、修理和其他服务业(10)
        /// 14文化、体育和娱乐业(5)
        /// 15其他行业(9)
        /// </summary>
        public string IndustryInvolved { get; set; }
        /// <summary>
        /// 人员规模
        /// 0:1-20、1:20-100、2:100-500、3:500-1000、4:1000-10000、5:10000以上
        /// </summary>
        public string StaffSize { get; set; }
        /// <summary>
        /// 网址
        /// </summary>
        public string WebSite { get; set; }
        /// <summary>
        ///备注
        /// </summary>
        public string Remark { get; set; }
        /// <summary>
        /// 是否认证
        /// </summary>
        public int IsCertification { get; set; } = 0;


        /// <summary>
        ///  联系人信息列表
        /// </summary>
        public List<ContPerson> ContPerList { get; set; }
    }

    public class CluePatternReq
    {
       public string pattern { get; set; }
    }

}
