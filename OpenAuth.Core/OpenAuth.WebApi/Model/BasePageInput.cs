using System.ComponentModel.DataAnnotations;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 传入分页参数基类
    /// </summary>
    public class BasePageInput
    {
        /// <summary>
        /// 当前页
        /// </summary>
        public int page_current { get; set; }

        /// <summary>
        /// 每页最多展示数据量
        /// </summary>
        public int page_size { get; set; }

        /// <summary>
        /// 排序字段（默认是"主键id"字段）
        /// </summary>
        [MaxLength(30)]
        public string? order_filed { get; set; }

        /// <summary>
        /// 是否倒序
        /// </summary>
        public bool desc { get; set; }

        /// <summary>
        /// 待查询字符串(一般为模糊搜索)
        /// </summary>
        [MaxLength(200)]
        public string? search_text { get; set; }

        /// <summary>
        /// 纠正
        /// </summary>
        public void Rectify()
        {
            if (string.IsNullOrWhiteSpace(order_filed))
            {
                this.order_filed = "id";
            }
            if (page_size <= 0)
            {
                this.page_size = 10;
            }
            if (page_current <= 0)
            {
                this.page_current = 1;
            }
        }
    }
}
