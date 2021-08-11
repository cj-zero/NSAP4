using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenAuth.App
{
    /// <summary>
    /// App抽奖
    /// </summary>
    public class AppLuckDrawApp : OnlyUnitWorkBaeApp
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unitWork"></param>
        /// <param name="auth"></param>
        public AppLuckDrawApp(IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {

        }
        /// <summary>
        /// 计算中奖名单
        /// </summary>
        /// <returns></returns>
        public async Task<TableData> LuckyDrawForRepair()
        {
            try
            {
                var result = new TableData();
                var date = DateTime.Now;
                var m = (date.DayOfWeek == DayOfWeek.Sunday ? (DayOfWeek)7 : date.DayOfWeek) - DayOfWeek.Monday;
                var s = (date.DayOfWeek == DayOfWeek.Sunday ? (DayOfWeek)7 : date.DayOfWeek) - (DayOfWeek)7;
                var Mon = date.AddDays((-7 - m)).Date;
                var Sun = Convert.ToDateTime(date.AddDays((-7 - s)).Date.ToString("yyyy-MM-dd 23:59:59"));
                object obj = new object();
                List<string> serverIdList = new List<string>();
                var allRepaorList = await (from a in UnitWork.Find<ServiceWorkOrder>(null)
                                           join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id
                                           where b.FromId == 6 && b.Status == 2 && a.Status >= 2
                                           && a.CreateTime >= Mon && a.CreateTime <= Sun && a.ManufacturerSerialNumber != "无序列号" && b.VestInOrg == 1 && b.FromAppUserId != null
                                           select new { a.ManufacturerSerialNumber, a.ServiceOrderId, b.U_SAP_ID, b.FromAppUserId }).ToListAsync();
                var ListSn = allRepaorList.GroupBy(c => c.ManufacturerSerialNumber).Select(c => c.Key).ToList();
                for (int i = 0; i < ListSn.Count; i++)
                {
                    Random rd = new Random();
                    var list = allRepaorList.Where(c => c.ManufacturerSerialNumber == ListSn[i]).Select(c=>c.ManufacturerSerialNumber).ToList();
                    if (list.Count > 1)
                    {
                        serverIdList.Add(list[rd.Next(list.Count)]);
                    }
                    else
                    {
                        serverIdList.Add(list.FirstOrDefault());
                    }
                }
                if (serverIdList.Count <= 20)
                {
                    obj = new { allRepaorList };
                }
                else
                {
                    List<string> WinningPrize = new List<string>();
                    List<int> idList = new List<int>();
                    var indexList = RndomStr(serverIdList.Count, idList);
                    foreach (var index in indexList)
                    {
                        WinningPrize.Add(serverIdList[index]);
                    }
                    var all_data=allRepaorList.Where(c => WinningPrize.Contains(c.ManufacturerSerialNumber)).ToList();
                    obj = new { allRepaorList = all_data };
                }
                result.Data = obj;
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="codeLength">24</param>
        /// <param name="idList">0</param>
        /// <returns></returns>
        public List<int> RndomStr(int codeLength,List<int> idList)
        {
            int[] array=new int[codeLength];
            for(int i=0;i< codeLength;i++)
            {
                array[i]=i;
            }
            var randList=array.ToList().Except(idList);
            int length = randList.Count();
            int temp = -1;
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                if (temp != -1)
                {
                    rand = new Random(i * temp * unchecked((int)DateTime.Now.Ticks));
                }
                int t = rand.Next(randList.Count());
                var data = randList.ToArray()[t];
                if (idList.Contains(data))
                {
                    int count = codeLength - idList.Count;
                    return RndomStr(count, idList);
                }
                temp = data;
                idList.Add(data);
                if (idList.Count >= 20)
                {
                    break;
                }
            }
            return idList;
        }
    }
}
