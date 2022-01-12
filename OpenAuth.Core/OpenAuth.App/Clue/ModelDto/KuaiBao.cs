using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Clue.ModelDto
{
    public class KuaiBaosHelper    {
        public class KuaiBao
        {
            public int code { get; set; }
            public string msg { get; set; }
            public List<Datum> data { get; set; }
            public string uid { get; set; }
        }
        public class Datum
        {
            public string original { get; set; }
            public string phone { get; set; }
            public string mobile { get; set; }
            public string name { get; set; }
            public string note { get; set; }
            public string province_id { get; set; }
            public string province_name { get; set; }
            public string province_shortname { get; set; }
            public string province_code { get; set; }
            public string city_id { get; set; }
            public string city_name { get; set; }
            public string city_shortname { get; set; }
            public string city_code { get; set; }
            public string county_id { get; set; }
            public string county_name { get; set; }
            public string county_shortname { get; set; }
            public string county_code { get; set; }
            public string detail { get; set; }
        }
        /// <summary>
        /// 返回
        /// </summary>
        public class KuaiBaoResponse
        {
            /// <summary>
            /// 收货人号码
            /// </summary>
            public string Phone { get; set; }
            /// <summary>
            /// 收货人姓名
            /// </summary>
            public string Name { get; set; }
            /// <summary>
            /// 省份
            /// </summary>
            public string ProvinceName { get; set; }
            /// <summary>
            /// 城市
            /// </summary>
            public string CityName { get; set; }
            /// <summary>
            /// 区县名称
            /// </summary>
            public string CountyName { get; set; }
            /// <summary>
            /// 详细地址
            /// </summary>
            public string Detail { get; set; }
        }
        /// <summary>
        /// 快宝请求对象
        /// </summary>
        public class KuaiRequest
        {
            public bool multimode { get; set; }
            public string text { get; set; }
            public bool resolveTown { get; set; }
        }
    }
}
