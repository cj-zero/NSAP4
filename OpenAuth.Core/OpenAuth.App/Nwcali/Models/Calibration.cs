using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Models
{
    /// <summary>
    /// 接口返回
    /// </summary>
    public class CalibrationResult
    {
        /// <summary>
        /// 消息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 状态编码
        /// </summary>
        public int status { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public List<CalibrationGroups> data { get; set; }
    }

    /// <summary>
    /// 校准委托组件
    /// </summary>
    public class CalibrationGroups
    {
        /// <summary>
        /// id
        /// </summary>
        public string field_id { get; set; }

        /// <summary>
        /// 页面控制类型
        /// </summary>
        public int html_control_type { get; set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 是否请求
        /// </summary>
        public bool is_required { get; set; }

        /// <summary>
        /// 操作集
        /// </summary>
        public List<string> options { get; set; }

        /// <summary>
        /// 标识
        /// </summary>
        public string field_tag { get; set; }
        
        /// <summary>
        /// 子集
        /// </summary>
        public List<CalibrationGroups> child_list { get; set; }
    }

    public class ControlDataList
    {
        /// <summary>
        /// 提交实体
        /// </summary>
        public List<SubmitData> controls_data_list { get; set; }
    }

    public class SubmitData
    {
        /// <summary>
        /// 组件id
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 值
        /// </summary>
        public object value { get; set; }
    }

    public class SubmitDataContainChild
    {
        /// <summary>
        /// 组件id
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 值数组
        /// </summary>
        public List<SubmitDataChild> value { get; set; }
    }

    public class SubmitDataChild
    {
        public SubmitDataChild()
        {
            this.key = string.Empty;
            this.value = new List<string>();
        }

        /// <summary>
        /// 组件id
        /// </summary>
        public string key { get; set; }

        /// <summary>
        /// 值数组
        /// </summary>
        public List<string> value { get; set; }
    }

    public class ReturnResult
    {
        /// <summary>
        /// 数据
        /// </summary>
        public string data { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 状态
        /// </summary>
        public int status { get; set; }
    }
}
