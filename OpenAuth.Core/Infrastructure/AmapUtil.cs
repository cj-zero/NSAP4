using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Infrastructure
{
    public class AmapUtil
    {
        private readonly static string key = "o3Shlf9DguFsv6mH9wQ9RSVGjKjq0THU";
        private readonly static string gdkey = "53aa0193d72ff1fb9482847d145f49dc";//高德key
        private static HttpClient client = new HttpClient();

        /// <summary>
        /// 获取经纬度
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static double[] GetXY(string address)
        {
            string url = String.Format("http://api.map.baidu.com/geocoding/v3/?address={0}&output=json&ak={1}", address, key);
            //结果
            string result = client.GetStringAsync(url).Result;

            var locationResult = (JObject)JsonConvert.DeserializeObject(result);

            if (locationResult["status"].ToString() != "0")
            {
                return null;
            }
            string lngStr = locationResult["result"]["location"]["lng"].ToString();

            string latStr = locationResult["result"]["location"]["lat"].ToString();

            double lng = double.Parse(lngStr);

            double lat = double.Parse(latStr);

            return new double[] { lng, lat };
        }

        /// <summary>
        /// 根据经纬度获取详细地址信息
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static JObject GetLocation(string address)
        {
            var xy = GetXY(address);
            if (xy == null)
            {
                return null;
            }
            string url = String.Format("http://api.map.baidu.com/reverse_geocoding/v3/?location={0},{1}&output=json&ak={2}", xy[1], xy[0], key);
            string result = client.GetStringAsync(url).Result;
            var locationResult = (JObject)JsonConvert.DeserializeObject(result);
            return locationResult;
        }
    }
}
