using System;
using System.Net;
using System.IO;
using Infrastructure.TecentOCR;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Infrastructure.HuaweiOCR
{
    public class HuaweiOCR
    {

        /// <summary>
        /// 日期格式转换
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private string GetDateFormat(string date)
        {
            return date.Replace("年", "-").Replace("月", "-").Replace("日", " ");
        }

        /// <summary>
        /// 智能识别
        /// </summary>
        public TecentOCR.Result CommonInvoiceOCR(HuaweiOCRRequest request)
        {
            var result = new TecentOCR.Result();
            List<object> outData = new List<object>();
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            Signer signer = new Signer();
            //Set the AK/SK to sign and authenticate the request.
            signer.Key = "59CWHEA0UPGWUUFWQJBD";
            signer.Secret = "nUO5BKxdkag4ID76Cc7a2Rx3oKKhj0WvZEETT7cz";
            HttpRequest r = new HttpRequest("POST",
                new Uri("https://ocr.cn-north-4.myhuaweicloud.com/v2/0a18ab14120025c52f2cc0093aee0aa5/ocr/auto-classification"));
            r.body = JsonConvert.SerializeObject(request);
            r.headers.Add("Content-Type", "application/json");

            HttpWebRequest req = signer.Sign(r);
            try
            {
                var writer = new StreamWriter(req.GetRequestStream());
                writer.Write(r.body);
                writer.Flush();
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();
                var reader = new StreamReader(resp.GetResponseStream());
                var a = reader.ReadToEnd();
                HuaweiOCRResponse response = JsonConvert.DeserializeObject<HuaweiOCRResponse>(a);
                var data = response.result;
                if (data.Length > 0)
                {
                    foreach (var item in data)
                    {
                        ExtendInfo extend = new ExtendInfo();
                        var status = item.status;
                        if ("AIS.0000".Equals(status.error_code))
                        {
                            string invoiceCode = string.Empty;
                            string invoiceNo = string.Empty;
                            string invoiceDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            decimal amountWithTax = decimal.Zero;
                            string checkCode = string.Empty;
                            string companyName = string.Empty;
                            string companyTaxCode = string.Empty;
                            decimal amountWithOutTax = decimal.Zero;
                            string sellerName = string.Empty;
                            string type = item.Type;
                            int ticketType = -1;
                            var content = item.content;
                            switch (type)
                            {
                                case "vat_invoice"://增值税发票
                                    invoiceCode = content?.code;
                                    invoiceNo = content?.number;
                                    invoiceDate = GetDateFormat(content?.issue_date);
                                    amountWithTax = decimal.Parse(content?.total.Replace("￥", string.Empty));
                                    companyName = content?.buyer_name;
                                    companyTaxCode = content?.buyer_id;
                                    checkCode = content?.check_code;
                                    extend.ServiceName = item.content?.item_list[0]?.name;
                                    amountWithOutTax = decimal.Parse(content?.subtotal_amount.Replace("￥", string.Empty));
                                    sellerName = content.seller_name;
                                    ticketType = 3;
                                    break;
                                case "train_ticket"://火车票
                                    invoiceCode = content?.log_id;
                                    invoiceNo = content?.ticket_id;
                                    invoiceDate = GetDateFormat(content?.departure_time);
                                    amountWithTax = decimal.Parse(content?.ticket_price.Replace("￥", string.Empty));
                                    extend.OriginationStation = content?.destination_station;
                                    extend.ArrivalStation = content?.departure_station;
                                    extend.ServiceName = "交通费";
                                    ticketType = 2;
                                    break;
                                case "flight_itinerary"://飞机行程单
                                    invoiceCode = content?.e_ticket_number;
                                    invoiceNo = content?.serial_number.Replace(" ", string.Empty);
                                    invoiceDate = GetDateFormat(content?.issue_date);
                                    amountWithTax = decimal.Parse(content?.total.Replace("CNY", string.Empty).Trim());
                                    extend.OriginationStation = content?.itinerary_list[0].departure_station;
                                    extend.ArrivalStation = content?.itinerary_list[0].destination_station;
                                    extend.ServiceName = "交通费";
                                    ticketType = 5;
                                    break;
                                case "quota_invoice"://定额发票
                                    invoiceCode = content?.code;
                                    invoiceNo = content?.number;
                                    amountWithTax = decimal.Parse(content?.amount.Replace("￥", string.Empty));
                                    ticketType = 1;
                                    break;
                                case "taxi_invoice"://出租车发票
                                    invoiceCode = content?.code;
                                    invoiceNo = content?.number;
                                    invoiceDate = GetDateFormat(content?.date + " " + content?.boarding_time);
                                    amountWithTax = decimal.Parse(content?.total.Replace("￥", string.Empty));
                                    extend.ServiceName = "交通费";
                                    ticketType = 0;
                                    break;
                                case "toll_invoice"://车辆通行费发票
                                    invoiceCode = content?.code;
                                    invoiceNo = content?.number;
                                    invoiceDate = GetDateFormat(content?.date + " " + content?.time);
                                    amountWithTax = decimal.Parse(content?.amount.Replace("￥", string.Empty).Replace("元", string.Empty));
                                    extend.ServiceName = "交通费";
                                    extend.OriginationStation = content?.entry;
                                    extend.ArrivalStation = content?.exit;
                                    ticketType = 13;
                                    break;
                                default:
                                    break;
                            }
                            TicketInfo ticketInfo = new TicketInfo
                            {
                                InvoiceCode = invoiceCode,
                                InvoiceNo = invoiceNo,
                                InvoiceDate = invoiceDate,
                                AmountWithTax = amountWithTax,
                                CompanyName = companyName,
                                CompanyTaxCode = companyTaxCode,
                                CheckCode = checkCode,
                                Type = ticketType,
                                Extend = extend,
                                AmountWithOutTax = amountWithOutTax,
                                SellerName = sellerName
                            };
                            outData.Add(ticketInfo);
                        }
                    }
                }
            }
            catch (WebException e)
            {
                HttpWebResponse resp = (HttpWebResponse)e.Response;
                if (resp != null)
                {
                    //Console.WriteLine((int)resp.StatusCode + " " + resp.StatusDescription);
                    var reader = new StreamReader(resp.GetResponseStream());
                    result.Code = 201;
                    var errResult = reader.ReadToEnd();
                    Status status = JsonConvert.DeserializeObject<Status>(errResult);
                    result.Message = status?.error_msg;
                    return result;
                }
                else
                {
                    result.Code = 201;
                    result.Message = e.Message;
                    return result;
                }
            }
            result.Data = outData;
            return result;
        }
    }
}
