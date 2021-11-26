using OpenAuth.App.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class QueryServiceOrderListReq : PageReq
    {
        /// <summary>
        /// 查询服务ID查询条件
        /// </summary>
        public string QryServiceOrderId { get; set; }

        public string QryServiceWorkOrderId { get; set; }
        /// <summary>
        /// 呼叫状态查询条件
        /// </summary>
        public string QryState { get; set; }
        /// <summary>
        /// 呼叫状态查询条件多选
        /// </summary>
        public List<int> QryStateList { get; set; }
        /// <summary>
        /// 客户查询条件
        /// </summary>
        public string QryCustomer { get; set; }
        /// <summary>
        /// 制造商序列号查询条件
        /// </summary>
        public string QryManufSN { get; set; }

        /// <summary>
        /// 创建日期从查询条件
        /// </summary>
        public DateTime? QryCreateTimeFrom { get; set; }

        /// <summary>
        /// 创建日期至查询条件
        /// </summary>
        public DateTime? QryCreateTimeTo { get; set; }
        /// <summary>
        /// 接单员
        /// </summary>
        public string QryRecepUser { get; set; }
        /// <summary>
        /// 工单技术员
        /// </summary>
        public string QryTechName { get; set; }
        /// <summary>
        /// 问题类型
        /// </summary>
        public string QryProblemType { get; set; }
        /// <summary>
        /// 手机号查询条件
        /// </summary>
        public string ContactTel { get; set; }
        /// <summary>
        /// 物料类别（多选)
        /// </summary>
        public List<string> QryMaterialTypes { get; set; }

        /// <summary>
        /// U_SAP_ID
        /// </summary>
        public string QryU_SAP_ID { get; set; }
        /// <summary>
        /// 呼叫类型
        /// </summary>
        public string QryFromType { get; set; }

        /// <summary>
        /// 主管名字
        /// </summary>
        public string QrySupervisor { get; set; }

        /// <summary>
        /// 状态栏 1 已派单 2 已解决 
        /// </summary>
        public int? QryStatusBar { get; set; }

        /// <summary>
        /// 呼叫主题
        /// </summary>
        public string QryFromTheme { get; set; }


        /// <summary>
        /// 物料编码
        /// </summary>
        public string QryMaterialCode { get; set; }

        /// <summary>
        /// 完工时间
        /// </summary>
        public DateTime? CompleteDate { get; set; }

        /// <summary>
        /// 完工时间
        /// </summary>
        public DateTime? EndCompleteDate { get; set; }

        /// <summary>
        /// 归属部门（1呼叫中心，2E3工程部）
        /// </summary>
        public string QryVestInOrg { get; set; }

        /// <summary>
        /// 是否服务
        /// </summary>
        public int? QryAllowOrNot { get; set; }

        /// <summary>
        /// appid
        /// </summary>
        public int? AppUserId { get; set; }

        /// <summary>
        /// 销售员查询
        /// </summary>
        public string QrySalesMan { get; set; }

    }
}
