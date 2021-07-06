using System;

namespace Infrastructure.Helpers
{
  public class DateTimeHelper
  {
		public static string FriendlyDate(DateTime? date)
		{
			if (!date.HasValue) return string.Empty;

			string strDate = date.Value.ToString("yyyy-MM-dd");
			string vDate = string.Empty;
			if(DateTime.Now.ToString("yyyy-MM-dd")==strDate)
			{
				vDate = "今天";
			}
			else if (DateTime.Now.AddDays(1).ToString("yyyy-MM-dd") == strDate)
			{
				vDate = "明天";
			}
			else if (DateTime.Now.AddDays(2).ToString("yyyy-MM-dd") == strDate)
			{
				vDate = "后天";
			}
			else if (DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") == strDate)
			{
				vDate = "昨天";
			}
			else if (DateTime.Now.AddDays(2).ToString("yyyy-MM-dd") == strDate)
			{
				vDate = "前天";
			}
			else
			{
				vDate = strDate;
			}

			return vDate;
		}


        /// <summary>  
        /// 获取时间戳,为真时获取10位(秒)时间戳(Unix),为假时获取13位(毫秒)时间戳
        /// bflag为true：秒，bflag为false：毫秒
        /// </summary>  
        /// <param name="bflag">.</param>  
        /// <returns></returns>  
        public static long GetTimeStamp(DateTime dt, bool bflag)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            TimeSpan ts = dt - startTime;
            long ret = 0;
            if (bflag)
                ret = Convert.ToInt64(ts.TotalSeconds);
            else
                ret = Convert.ToInt64(ts.TotalMilliseconds);

            return ret;
        }

        /// <summary>
        /// 将时间戳转换为DateTime时间，bSecond为true：秒，bSecond为false：毫秒
        /// </summary>
        /// <param name="timestamp"></param>
        /// <param name="bSecond"></param>
        /// <returns></returns>
        public static DateTime TimeStampToDateTime(long timestamp, bool bSecond)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区

            if (bSecond)
            {
                return startTime.AddSeconds(timestamp);
            }
            else
                return startTime.AddMilliseconds(timestamp);
        }
  }
}

