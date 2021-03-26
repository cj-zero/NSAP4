using OpenAuth.App.Serve.Request;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    public class DailyExpendResp
    {
        /// <summary>
        /// 填写了日费的日期集合
        /// </summary>
        public List<string> DailyDates { get; set; }

        /// <summary>
        /// 差旅费
        /// </summary>
        public TravelExpense TravelExpense { get; set; }

        /// <summary>
        /// 交通费集合
        /// </summary>
        public List<TransportExpense> TransportExpenses { get; set; }

        /// <summary>
        /// 住宿费集合
        /// </summary>
        public List<HotelExpense> HotelExpenses { get; set; }

        /// <summary>
        /// 其他费用集合
        /// </summary>
        public List<OtherExpense> OtherExpenses { get; set; }
    }
}
