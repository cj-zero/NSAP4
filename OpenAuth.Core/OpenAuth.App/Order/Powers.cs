using System;

namespace OpenAuth.App.Order
{
    public class Powers
    {
        private string _authMapBinary = string.Empty;

        public Powers(long authMap)
        {
            _authMapBinary = authMap.ToBinary();
        }
        /// <summary>
        /// 下载附件
        /// </summary>
        public bool DownloadAttachment
        {
            get { return PositionRead(14); }
        }

        /// <summary>
        /// 查看附件
        /// </summary>
        public bool ViewAttachment
        {
            get { return PositionRead(13); }
        }

        /// <summary>
        /// 查看客户资料
        /// </summary>
        public bool ViewCustom
        {
            get { return PositionRead(12); }
        }

        /// <summary>
        /// 查看销售价格
        /// </summary>
        public bool ViewSales
        {
            get { return PositionRead(11); }
        }

        /// <summary>
        /// 查看采购价格
        /// </summary>
        public bool ViewPurchase
        {
            get { return PositionRead(10); }
        }

        /// <summary>
        /// 查看毛利价格
        /// </summary>
        public bool ViewGross
        {
            get { return PositionRead(9); }
        }

        /// <summary>
        /// 查看成本价格
        /// </summary>
        public bool ViewCosts
        {
            get { return PositionRead(8); }
        }

        /// <summary>
        /// 拥有导出操作
        /// </summary>
        public bool OperateExport
        {
            get { return PositionRead(7); }
        }

        /// <summary>
        /// 拥有审核操作
        /// </summary>
        public bool OperateAudit
        {
            get { return PositionRead(6); }
        }

        /// <summary>
        /// 拥有删除操作
        /// </summary>
        public bool OperateDelete
        {
            get { return PositionRead(5); }
        }

        /// <summary>
        /// 拥有修改操作
        /// </summary>
        public bool OperateUpdate
        {
            get { return PositionRead(4); }
        }

        /// <summary>
        /// 拥有添加操作
        /// </summary>
        public bool OperateAppend
        {
            get { return PositionRead(3); }
        }

        /// <summary>
        /// 查看所有
        /// </summary>
        public bool ViewFull
        {
            get { return PositionRead(2); }
        }

        /// <summary>
        /// 查看本部门
        /// </summary>
        public bool ViewSelfDepartment
        {
            get { return PositionRead(1); }
        }

        /// <summary>
        /// 查看本人
        /// </summary>
        public bool ViewSelf
        {
            get { return PositionRead(0); }
        }

        private bool PositionRead(int position)
        {
            if (position > 31) return false;
            string _authMark = _authMapBinary.Substring(position, 1);
            return _authMark == "0" ? false : true;
        }
    }
    public static class EInt
    {
        public static string ToBinary(this long Text)
        {
            if (Text == 0) return "00000000000000000000000000000000";
            return Convert.ToString(Text, 2);
        }
    }
    public class PowerDto
    {
        public int FuncID { get; set; }
        public string PageUrl { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public long AuthMap { get; set; }
    }
    public class RolesDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class ResultOrderDto
    {
        public string docEntry { get; set; }
        public object Value { get; set; }
    }
}
