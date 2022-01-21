using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    public class ClassificationDto
    {
        public int Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 父级Id
        /// </summary>
        public int ParentId { get; set; }
        /// <summary>
        /// 层级
        /// </summary>
        public int Level { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string CreateUser { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime CreateTime { get; set; }
        /// <summary>
        ///更新人
        /// </summary>
        public string UpdateUser { get; set; }
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        /// <summary>
        /// 子级
        /// </summary>
        public List<ClassificationDto> Children { get; set; }
    }

}
