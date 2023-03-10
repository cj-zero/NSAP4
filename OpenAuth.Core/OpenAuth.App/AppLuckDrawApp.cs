using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Interface;
using OpenAuth.App.Response;
using OpenAuth.App.Serve.Response;
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
        /// 同一服务Id参与一次，不同服务id相同序列号和呼叫主题也只参与一次,相同序列编号呼叫主题被其他的呼叫主题全包含也只参与一次
        /// </summary>
        /// <returns></returns>
        //public async Task<TableData> LuckyDrawForRepair()
        //{
        //    try
        //    {
        //        var result = new TableData();
        //        var date = DateTime.Now;
        //        var m = (date.DayOfWeek == DayOfWeek.Sunday ? (DayOfWeek)7 : date.DayOfWeek) - DayOfWeek.Monday;
        //        var s = (date.DayOfWeek == DayOfWeek.Sunday ? (DayOfWeek)7 : date.DayOfWeek) - (DayOfWeek)7;
        //        var Mon = date.AddDays((-7 - m)).Date;
        //        var Sun = date.AddDays((-7 - s)).AddDays(1).Date;
        //        object obj = new object();
        //        List<string> serverIdList = new List<string>();
        //        var allRepaorList = await (from a in UnitWork.Find<ServiceWorkOrder>(null)
        //                                   join b in UnitWork.Find<ServiceOrder>(null) on a.ServiceOrderId equals b.Id
        //                                   where b.FromId == 6 && b.Status == 2
        //                                   && a.CreateTime >= Mon && a.CreateTime < Sun && a.ManufacturerSerialNumber != "无序列号" && b.VestInOrg == 1 && b.FromAppUserId != null && b.FromAppUserId != 0 && b.CustomerId != "C00550"
        //                                   select new { a.ManufacturerSerialNumber, a.FromTheme, a.ServiceOrderId, b.U_SAP_ID, b.FromAppUserId }).ToListAsync();
        //        var U_SAP_ID_List = allRepaorList.GroupBy(c => c.U_SAP_ID).Select(c => c.Key).ToList();
        //        List<int> r_u_sap_id = new List<int>();
        //        List<LuckyServiceOrder> luckyServiceOrdersList = new List<LuckyServiceOrder>();
        //        foreach (var item in U_SAP_ID_List)
        //        {
        //            var list = allRepaorList.Where(c => c.U_SAP_ID == item).ToList();
        //            LuckyServiceOrder luckyServiceOrder = new LuckyServiceOrder();
        //            luckyServiceOrder.U_SAP_ID = item.Value;
        //            luckyServiceOrder.ManufacturerSerialNumberList = list.Select(c => c.ManufacturerSerialNumber).ToList();
        //            List<LuckyFromTheme> luckyFromThemesList = new List<LuckyFromTheme>();
        //            luckyServiceOrder.CodeList = luckyFromThemesList;
        //            foreach (var row in list)
        //            {
        //                try
        //                {
        //                    LuckyFromTheme model = new LuckyFromTheme();
        //                    List<string> code_list = new List<string>();
        //                    var LuckyFromThemeModelList = JsonConvert.DeserializeObject<List<LuckyFromThemeModel>>(row.FromTheme);
        //                    foreach (var citem in LuckyFromThemeModelList)
        //                    {
        //                        code_list.Add(citem.code);
        //                    }
        //                    model.code_list = code_list;
        //                    model.ManufacturerSerialNumber = row.ManufacturerSerialNumber;
        //                    luckyServiceOrder.CodeList.Add(model);
        //                }
        //                catch (Exception ex)
        //                {
        //                    continue;
        //                }
        //            }
        //            luckyServiceOrdersList.Add(luckyServiceOrder);
        //        }
        //        foreach (var item in luckyServiceOrdersList)
        //        {
        //            var data = luckyServiceOrdersList.Where(c => c.U_SAP_ID == item.U_SAP_ID).FirstOrDefault();
        //            var itemList = luckyServiceOrdersList.Where(c => c.U_SAP_ID != item.U_SAP_ID).ToList();
        //            foreach (var row in itemList)
        //            {
        //                var flag = IsContainsAll(data.ManufacturerSerialNumberList, row.ManufacturerSerialNumberList);
        //                if (flag == true)
        //                {
        //                    var count = 0;
        //                    foreach (var sn in data.CodeList)
        //                    {
        //                        var ListA = sn.code_list;
        //                        var ListB = row.CodeList.Where(c => c.ManufacturerSerialNumber == sn.ManufacturerSerialNumber).Select(c => c.code_list).FirstOrDefault();
        //                        var res = IsContainsAll(ListA, ListB);
        //                        if (res == false)
        //                            count++;
        //                    }
        //                    if (count == 0)
        //                    {
        //                        if (!r_u_sap_id.Contains(row.U_SAP_ID))
        //                        {
        //                            U_SAP_ID_List.Remove(item.U_SAP_ID);
        //                            r_u_sap_id.Add(item.U_SAP_ID);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        var all = allRepaorList.Where(c => U_SAP_ID_List.Contains(c.U_SAP_ID)).Select(c => new { c.U_SAP_ID, c.FromAppUserId }).Distinct().ToList();
        //        if (all.Count <= 80)
        //        {
        //            obj = new { allRepaorList = all };
        //        }
        //        else
        //        {
        //            List<int> WinningPrize = new List<int>();
        //            List<int> idList = new List<int>();
        //            var indexList = RndomStr(U_SAP_ID_List.Count, idList, 80);
        //            foreach (var index in indexList)
        //            {
        //                WinningPrize.Add(U_SAP_ID_List[index].Value);
        //            }
        //            var all_data = allRepaorList.Where(c => WinningPrize.Contains(c.U_SAP_ID.Value)).Select(c => new { c.U_SAP_ID, c.FromAppUserId }).Distinct().ToList();
        //            obj = new { allRepaorList = all_data };
        //        }
        //        result.Data = obj;
        //        return result;
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception(ex.Message);
        //    }
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="codeLength"></param>
        /// <param name="idList"></param>
        /// <param name="counts"></param>
        /// <returns></returns>
        public List<int> RndomStr(int codeLength, List<int> idList, int counts)
        {
            int[] array = new int[codeLength];
            for (int i = 0; i < codeLength; i++)
            {
                array[i] = i;
            }
            var randList = array.ToList().Except(idList);
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
                    return RndomStr(count, idList, counts);
                }
                temp = data;
                idList.Add(data);
                if (idList.Count >= counts)
                {
                    break;
                }
            }
            return idList;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ListA"></param>
        /// <param name="ListB"></param>
        /// <returns></returns>
        public bool IsContainsAll(List<string> ListA, List<string> ListB)
        {
            int count = ListA.Except(ListB).ToList().Count;
            if (count <= 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<TableData> LuckyDrawForRepair()
        {
            try
            {
                DateTime dt = DateTime.Now;
                var startTime = dt.AddDays(1 - dt.Day).AddMonths(-1).Date;
                var endTime = dt.AddDays(1 - dt.Day).Date;
                var result = new TableData();
                object obj = new object();
                List<string> serverIdList = new List<string>();
                var allRepaorList = await UnitWork.Find<ServiceOrder>(null)
                    .Where(b => b.CreateTime >= startTime && b.CreateTime < endTime && b.VestInOrg == 1 && b.FromId == 6 && b.Status == 2 && b.FromAppUserId != null && b.FromAppUserId != 0 && b.CustomerId != "C00550")
                    .Select(c => new { c.Id, c.CustomerId, c.NewestContactTel, c.FromAppUserId, c.U_SAP_ID }).ToListAsync();
                List<int> serverOrderIds = new List<int>();
                var customerList = allRepaorList.Select(b => b.CustomerId).Distinct().ToList();
                foreach (var item in customerList)
                {
                    var ids = allRepaorList.Where(c => c.CustomerId.Equals(item)).Select(c => c.Id).ToList();
                    var indexList = RndomStr(ids.Count, ids, 1);
                    if (!indexList.Any())
                    {
                        continue;
                    }
                    serverOrderIds.Add(indexList[0]);
                }
                var repaorIds = allRepaorList.Where(c => serverOrderIds.Contains(c.Id)).Select(c => new {c.Id,c.NewestContactTel }).Distinct().Select(c=>c.Id).ToList();
                var repaorList = allRepaorList.Where(c => repaorIds.Contains(c.Id)).Select(c => new {c.U_SAP_ID,c.FromAppUserId}).ToList();
                if (repaorList.Count <= 80)
                {
                    obj = new { allRepaorList = repaorList };
                }
                else
                {
                    List<int> WinningPrize = new List<int>();
                    List<int> idList = new List<int>();
                    var indexList = RndomStr(repaorIds.Count, idList,80);
                    foreach (var index in indexList)
                    {
                        WinningPrize.Add(repaorIds[index]);
                    }
                    var all_data = allRepaorList.Where(c => WinningPrize.Contains(c.Id)).Select(c => new { c.U_SAP_ID, c.FromAppUserId }).ToList();
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
    }
}
