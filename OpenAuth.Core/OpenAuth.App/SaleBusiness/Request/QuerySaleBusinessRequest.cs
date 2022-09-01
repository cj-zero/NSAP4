using System;
using System.Collections.Generic;
using System.Text;
using OpenAuth.App.Request;

namespace OpenAuth.App.SaleBusiness.Request
{
    /// <summary>
    /// 销售情况概览实体
    /// </summary>
    public class QuerySaleBusinessRequest
    {
        /// <summary>
        /// 模块名称
        /// </summary>
        public string ModelName { get; set; }

        /// <summary>
        /// 模块数据
        /// </summary>
        public string ModelNum { get; set; }

        /// <summary>
        /// 模块路径
        /// </summary>
        public string Url { get; set; }
    }

    /// <summary>
    /// OCRD查询实体
    /// </summary>
    public class QueryOCRD
    {
        /// <summary>
        /// 客户编码
        /// </summary>
        public string CardCode { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public System.DateTime? CreateDate { get; set; }

        /// <summary>
        /// 业务员编码
        /// </summary>
        public int? SlpCode { get; set; }
    }

    /// <summary>
    /// wfa_job查询实体
    /// </summary>
    public class QueryWfaJob
    {
        /// <summary>
        /// 流程Id
        /// </summary>
        public int? job_id { get; set; }

        /// <summary>
        /// 流程类型id
        /// </summary>
        public int job_type_id { get; set; }

        /// <summary>
        /// 用户id
        /// </summary>
        public int user_id { get; set; }

        /// <summary>
        /// 流程id
        /// </summary>
        public int step_id { get; set; }

        /// <summary>
        /// sboId
        /// </summary>
        public int sbo_id { get; set; }
    }

    /// <summary>
    /// 分页查询库存实体
    /// </summary>
    public class QueryWareHouse : PageReq
    { 
        /// <summary>
        /// 物料编码
        /// </summary>
        public string ItemCode { get; set; }
        
        /// <summary>
        /// 仓库编码
        /// </summary>
        public string WhsCode { get; set; }
        
        /// <summary>
        /// 物料描述
        /// </summary>
        public string ItemName { get; set; }

        /// <summary>
        /// 排序字段
        /// </summary>
        public string SortName { get; set; }

        /// <summary>
        /// 正序/反序
        /// </summary>
        public string SortOrder { get; set; }
    }

    /// <summary>
    /// OINV实体
    /// </summary>
    public class QueryOINV
    { 
        /// <summary>
        /// 应收款数据
        /// </summary>
        public string DeliveryNum { get; set; }

        /// <summary>
        /// 占上一年度的回款的比例
        /// </summary>
        public string CompareLaterYear { get; set; }
    }

    /// <summary>
    /// OINV实体
    /// </summary>
    public class QueryOINVRank
    {
        /// <summary>
        /// 部门排名
        /// </summary>
        public string DepartRank { get; set; }

        /// <summary>
        /// 公司排名
        /// </summary>
        public string CompanyRank { get; set; }
    }

    /// <summary>
    /// OCRN实体
    /// </summary>
    public class QueryOCRN
    {
        /// <summary>
        /// 币种编号
        /// </summary>
        public string CurrCode { get; set; }

        /// <summary>
        /// 币种名称
        /// </summary>
        public string CurrName { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_CurrCode">币种编号</param>
        /// <param name="_CurrName">币种名称</param>
        public QueryOCRN(string _CurrCode, string _CurrName)
        {
            CurrCode = _CurrCode;
            CurrName = _CurrName;
        }
    }

    /// <summary>
    /// SlpCode查询实体
    /// </summary>
    public class QuerySlpCode
    { 
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int SlpCode { get; set; }
    }

    /// <summary>
    /// 部门排名查询实体
    /// </summary>
    public class QueryRank
    { 
        /// <summary>
        /// 业务员编码
        /// </summary>
        public int? SlpCode { get; set; }

        /// <summary>
        /// 总计金额
        /// </summary>
        public decimal TotalAmount { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="_SlpCode">业务员编码</param>
        /// <param name="_TotalAmount">总计金额</param>
        public QueryRank(int? _SlpCode, decimal _TotalAmount)
        {
            SlpCode = _SlpCode;
            TotalAmount = _TotalAmount;
        }
    }

    /// <summary>
    /// 时间查询实体
    /// </summary>
    public class QueryTime
    {
        /// <summary>
        /// 开始时间
        /// </summary>
        public string startTime;

        /// <summary>
        /// 结束时间
        /// </summary>
        public string endTime;
    }

    /// <summary>
    /// 数据集
    /// </summary>
    public class QueryTableData
    {
        /// <summary>
        /// 时间查询集合
        /// </summary>
        public List<QueryTime> queryTimes { get; set; }

        /// <summary>
        /// x轴坐标集合
        /// </summary>

        public List<string> xNum;

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }
    }

}
