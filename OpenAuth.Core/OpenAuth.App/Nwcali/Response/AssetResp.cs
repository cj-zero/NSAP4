using Infrastructure.AutoMapper;
using OpenAuth.Repository.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Response
{
    [AutoMapTo(typeof(Asset))]
    public class AssetResp
    {
        /// <summary>
        /// Id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string AssetStatus { get; set; }
        /// <summary>
        /// 类别
        /// </summary>
        public string AssetCategory { get; set; }
        /// <summary>
        /// 部门
        /// </summary>
        public string OrgName { get; set; }
        /// <summary>
        /// 型号
        /// </summary>
        public string AssetType { get; set; }
        /// <summary>
        /// 持有者
        /// </summary>
        public string AssetHolder { get; set; }
        /// <summary>
        /// 出厂编号S/N
        /// </summary>
        public string AssetStockNumber { get; set; }
        /// <summary>
        /// 管理员
        /// </summary>
        public string AssetAdmin { get; set; }
        /// <summary>
        /// 资产编号
        /// </summary>
        public string AssetNumber { get; set; }
        /// <summary>
        /// 制造厂
        /// </summary>
        public string AssetFactory { get; set; }
        /// <summary>
        /// 送检类型
        /// </summary>
        public string AssetInspectType { get; set; }
        /// <summary>
        /// 送检方式
        /// </summary>
        public string AssetInspectWay { get; set; }
        /// <summary>
        /// 校准日期
        /// </summary>
        public System.DateTime? AssetStartDate { get; set; }
        /// <summary>
        /// 校准证书
        /// </summary>
        public string AssetCalibrationCertificate { get; set; }
        /// <summary>
        /// 失效日期
        /// </summary>
        public System.DateTime? AssetEndDate { get; set; }
        /// <summary>
        /// 校准数据1
        /// </summary>
        public string AssetInspectDataOne { get; set; }
        /// <summary>
        /// 校准数据2
        /// </summary>
        public string AssetInspectDataTwo { get; set; }
        /// <summary>
        /// 技术文件
        /// </summary>
        public string AssetTCF { get; set; }
        /// <summary>
        /// 描述
        /// </summary>
        public string AssetDescribe { get; set; }
        /// <summary>
        /// 备注
        /// </summary>
        public string AssetRemarks { get; set; }
        /// <summary>
        /// 图片
        /// </summary>
        public string AssetImage { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public System.DateTime? AssetCreateTime { get; set; }
        /// <summary>
        /// 创建人
        /// </summary>
        public string AssetCreateUser { get; set; }

        /// <summary>
        /// 类型详情表
        /// </summary>
        public List<AssetCategory> AssetCategorys { get; set; }

        /// <summary>
        /// 类型详情表
        /// </summary>
        public List<AssetInspect> AssetInspects { get; set; }

        /// <summary>
        /// 类型详情表
        /// </summary>
        public List<AssetOperation> AssetOperations { get; set; }
    }
}
