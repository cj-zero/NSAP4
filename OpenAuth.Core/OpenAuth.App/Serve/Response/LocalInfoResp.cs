using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Response
{
    public class LocalInfoResp
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
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

    }

    public class HistoryPositions
    {
        public string Date { get; set; }
        public List<Position> Pos { get; set; }
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

    }

    public class Trajectory
    {
        public string Date { get; set; }
        public List<Position> Pos { get; set; }
    }

}
