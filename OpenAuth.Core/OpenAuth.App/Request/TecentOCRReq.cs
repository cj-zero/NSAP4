using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Request
{
    public class TecentOCRReq
    {

        /// <summary>
        /// 文件Id
        /// </summary>
        public string FileId { get; set; }

        /// <summary>
        /// 图片Url
        /// </summary>
        public string ImageUrl { get; set; }
        /// <summary>
        /// 需要识别的票据类型列表，为空或不填表示识别全部类型。
        /// 0：出租车发票 1：定额发票 2：火车票 3：增值税发票 5：机票行程单 8：通用机打发票 9：汽车票 10：轮船票 11：增值税发票（卷票 ） 12：购车发票 13：过路过桥费发票
        /// </summary>
        public long?[] Types { get; set; }
    }
}
