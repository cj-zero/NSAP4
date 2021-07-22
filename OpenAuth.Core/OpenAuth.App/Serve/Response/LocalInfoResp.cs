using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class LocalInfoResp
    {
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// AppUserId
        /// </summary>
        public int? AppUserId { get; set; }
        /// <summary>
        /// 电话
        /// </summary>
        public string Mobile { get; set; }
        /// <summary>
        /// 地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        public string Status { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 省份
        /// </summary>
        public string Province { get; set; }
        /// <summary>
        /// 城市
        /// </summary>
        public string City { get; set; }
        /// <summary>
        /// 区域
        /// </summary>
        public string Area { get; set; }
        /// <summary>
        /// 历史轨迹
        /// </summary>
        //public List<Trajectory> HistoryPositions { get; set; }
        /// <summary>
        /// 当天轨迹
        /// </summary>
        //public List<Position> CurrPositions { get; set; }
        /// <summary>
        /// 在线间隔
        /// </summary>
        public string Interval { get; set; }
        /// <summary>
        /// 间隔总时长
        /// </summary>
        public double? TotalHour { get; set; }

    }

    public class HistoryPositions
    {
        /// <summary>
        /// 服务ID
        /// </summary>
        public string ServiceOrderId { get; set; }
        /// <summary>
        /// 签到时间
        /// </summary>
        public DateTime? SignInDate { get; set; }
        /// <summary>
        /// 签退时间
        /// </summary>
        public DateTime? SignOutDate { get; set; }

        public List<Trajectory> Trajectory { get; set; }
        //public string Date { get; set; }
        //public List<Position> Pos { get; set; }
    }


    public class Position
    {
        /// <summary>
        /// 经度
        /// </summary>
        public decimal? Longitude { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public decimal? Latitude { get; set; }
        /// <summary>
        /// 定位地址
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// 定位时间
        /// </summary>
        public DateTime? PosDate { get; set; }

    }

    public class Trajectory
    {
        public string Date { get; set; }
        public List<Position> Pos { get; set; }
    }

    public class CountInfo 
    {
        public int HasIdUser { get; set; }
        public int HasIdTech { get; set; }
        public int NoIdTech { get; set; }
        public int HasIdOnline { get; set; }
        public int HasIdOffline { get; set; }
    }
    public class DataInfo
    {
        public List<LocalInfoResp> LocalInfoResp { get; set; }
        public CountInfo CountInfo { get; set; }
    }
}
