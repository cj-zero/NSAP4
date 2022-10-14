using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Request
{
    public class AddOrUpdatePortfolioAssetsReq
    {
        public AddOrUpdatePortfolioAssetsReq()
        {
            this.FirmwareInfo = new List<AssetsInfo>();
            this.TemporaryInfo = new List<AssetsInfo>();
        }
        public int? Id { get; set; }
        /// <summary>
        /// 资产id
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public int Category { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        public string OrgName { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string Remark { get; set; }

        /// <summary>
        /// 固定
        /// </summary>
        public List<AssetsInfo> FirmwareInfo { get; set; }
        /// <summary>
        /// 临时
        /// </summary>
        public List<AssetsInfo> TemporaryInfo { get; set; }
    }

    public class AssetsInfo 
    {
        /// <summary>
        /// 资产类型 1=实验室资产  2=普通资产
        /// </summary>
        public int SourceType { get; set; }


        /// <summary>
        ///  配件类型 1=固定配件   2=临时配件
        /// </summary>
        public int PartsType { get; set; }

        /// <summary>
        /// 序号
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 资产id
        /// </summary>
        public int AssetId { get; set; }

    }
}
