using System;
using System.Collections.Generic;

namespace OpenAuth.App.Request
{
    public class QueryEleSealReq
    {

        /// <summary>
        /// 合同申请单Id
        /// </summary>
        public string ContractApplyId { get; set; }

        /// <summary>
        /// 合同文件Id
        /// </summary>
        public string ContractFileId { get; set; }

        /// <summary>
        /// 图片Id
        /// </summary>
        public string ImageId { get; set; }

        /// <summary>
        /// 图片x坐标
        /// </summary>
        public float ImagePositionX { get; set; }

        /// <summary>
        /// 图片y坐标
        /// </summary>
        public float ImagePositionY { get; set; }

        /// <summary>
        /// 图片宽度
        /// </summary>
        public float ImageWidth { get; set; }

        /// <summary>
        /// 图片高度
        /// </summary>
        public float ImageHeight { get; set; }

        /// <summary>
        /// 页码
        /// </summary>
        public int Page { get; set; }
    }
}