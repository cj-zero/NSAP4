using System.Data;

namespace OpenAuth.WebApi.Model
{
    /// <summary>
    /// 保存导入数据输入
    /// </summary>
    public class ImportTemplateDataSaveInput
    {
        /// <summary>
        /// 导入数据
        /// </summary>
        public dynamic data { get; set; }

 

        /// <summary>
        /// 模板id
        /// </summary>
        public long template_id { get; set; }
        /// <summary>
        /// 设备类型id
        /// </summary>
        public long equip_type_id { get; set; }
    }
}
