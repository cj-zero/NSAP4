using System;
using System.Collections.Generic;
using System.Linq;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Ocr.V20181119;
using TencentCloud.Ocr.V20181119.Models;

namespace Infrastructure.TecentOCR
{
    public class TecentOCR
    {
        private readonly OcrClient _client;
        //个人秘钥
        const String secretId = "AKIDJMI0noPvp7B8tWnctuZzMzPQAi4HnWef";
        const String secretKey = "5tVrIZ6NPQCzWno9FBFQ2NUWHK9j23oM";

        public TecentOCR()
        {
            // 实例化一个认证对象，入参需要传入腾讯云账户密钥对 secretId、secretKey。
            Credential cred = new Credential
            {
                SecretId = secretId,
                SecretKey = secretKey
            };
            // 实例化一个 client 选项，
            ClientProfile clientProfile = new ClientProfile();
            clientProfile.SignMethod = ClientProfile.SIGN_TC3SHA256;
            HttpProfile httpProfile = new HttpProfile();
            httpProfile.Endpoint = ("ocr.tencentcloudapi.com");
            clientProfile.HttpProfile = httpProfile;
            // 实例化要请求产品的 client 对象
            _client = new OcrClient(cred, "ap-guangzhou", clientProfile);
        }

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
        /// 增值税发票识别
        /// </summary>
        public Result VatInvoiceOCR(VatInvoiceOCRRequest req)
        {
            var result = new Result();
            List<object> outData = new List<object>();
            try
            {
                VatInvoiceOCRResponse resp = new VatInvoiceOCRResponse();
                resp = _client.VatInvoiceOCRSync(req);
                var invoiceNo = resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "发票号码").Value.ToString()[2..resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "发票号码").Value.Length];
                var invoiceCode = resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "发票代码").Value;
                var invoiceDate = GetDateFormat(resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "开票日期").Value);
                var checkCode = resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "校验码").Value;
                var companyName = resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "购买方名称").Value;
                var companyTaxCode = resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "购买方识别号").Value;
                var amountWithTax = decimal.Parse(resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "小写金额").Value.ToString()[1..resp.VatInvoiceInfos.FirstOrDefault(s => s.Name == "小写金额").Value.Length]);
                TicketInfo ticketInfo = new TicketInfo
                {
                    InvoiceCode = invoiceCode,
                    InvoiceNo = invoiceNo,
                    InvoiceDate = invoiceDate,
                    AmountWithTax = amountWithTax,
                    CompanyName = companyName,
                    CompanyTaxCode = companyTaxCode,
                    CheckCode = checkCode,
                    Type = 3,
                    Extend = new ExtendInfo
                    {
                        ServiceName = resp.VatInvoiceInfos.FirstOrDefault(s => s.Name.Contains("服务名称")).Value
                    }
                };
                outData.Add(ticketInfo);
            }
            catch (Exception e)
            {
                result.Code = 201;
                result.Message = e.ToString();
                return result;
            }
            result.Data = outData;
            return result;
        }


        /// <summary>
        /// 混贴票据识别
        /// </summary>
        public Result MixedInvoiceOCR(MixedInvoiceOCRRequest req)
        {
            var result = new Result();
            List<object> outData = new List<object>();
            try
            {
                MixedInvoiceOCRResponse resp = _client.MixedInvoiceOCRSync(req);
                var data = resp.MixedInvoiceItems;
                if (data.Length > 0)
                {
                    foreach (var item in data)
                    {
                        ExtendInfo extend = new ExtendInfo();
                        //0：出租车发票 1：定额发票 2：火车票 3：增值税发票 5：机票行程单 8：通用机打发票 9：汽车票 10：轮船票 11：增值税发票（卷票 ）12：购车发票 13：过路过桥费发票
                        var ticketType = item.Type;
                        var SingleInvoiceInfos = item.SingleInvoiceInfos;
                        if (SingleInvoiceInfos.Length > 0)
                        {
                            string invoiceCode = string.Empty;
                            string invoiceNo = string.Empty;
                            string invoiceDate = string.Empty;
                            decimal amountWithTax = decimal.Zero;
                            string checkCode = string.Empty;
                            string companyName = string.Empty;
                            string companyTaxCode = string.Empty;
                            decimal amountWithOutTax = decimal.Zero;
                            string sellerName = string.Empty;
                            switch (ticketType)
                            {
                                case 0:
                                    invoiceCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票代码")?.Value;
                                    invoiceNo = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票号码")?.Value;
                                    amountWithTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "金额")?.Value);
                                    invoiceDate = GetDateFormat(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "日期")?.Value) + SingleInvoiceInfos.FirstOrDefault(s => s.Name == "上车")?.Value;
                                    break;
                                case 13:
                                    invoiceCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票代码")?.Value;
                                    invoiceNo = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票号码")?.Value;
                                    amountWithTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "金额")?.Value);
                                    invoiceDate = GetDateFormat(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "日期")?.Value) + SingleInvoiceInfos.FirstOrDefault(s => s.Name == "时间")?.Value;
                                    extend.ServiceName = "交通费";
                                    break;
                                case 1:
                                    invoiceCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票代码")?.Value;
                                    invoiceNo = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票号码")?.Value;
                                    amountWithTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "小写金额")?.Value);
                                    break;
                                case 2://火车票没有发票代码和发票号码 分别取火车票上的序列号和编号
                                    invoiceCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "序列号")?.Value;
                                    invoiceNo = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "编号")?.Value;
                                    amountWithTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "票价")?.Value);
                                    invoiceDate = GetDateFormat(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "出发时间")?.Value);
                                    extend.OriginationStation = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "出发站")?.Value;
                                    extend.ArrivalStation = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "到达站")?.Value;
                                    extend.ServiceName = "交通费";
                                    break;
                                case 3:
                                    invoiceNo = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票号码")?.Value.ToString()[2..SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票号码").Value.Length];
                                    invoiceCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票代码").Value;
                                    invoiceDate = GetDateFormat(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "开票日期")?.Value);
                                    checkCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "校验码")?.Value;
                                    companyName = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "购买方名称")?.Value;
                                    companyTaxCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "购买方识别号")?.Value;
                                    amountWithTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "小写金额")?.Value.ToString()[1..SingleInvoiceInfos.SingleOrDefault(s => s.Name == "小写金额").Value.Length]);
                                    amountWithOutTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "合计金额")?.Value.ToString()[1..SingleInvoiceInfos.SingleOrDefault(s => s.Name == "合计金额").Value.Length]);
                                    extend.ServiceName = SingleInvoiceInfos.FirstOrDefault(s => s.Name.Contains("服务名称"))?.Value;
                                    sellerName = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "销售方名称")?.Value;
                                    break;
                                case 5://飞机行程单没有发票代码和发票号码 分别取行程单上的印刷序号和电子客票号码
                                    invoiceCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "电子客票号码")?.Value;
                                    invoiceNo = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "印刷序号")?.Value;
                                    amountWithTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "合计金额")?.Value);
                                    invoiceDate = GetDateFormat(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "填开日期")?.Value) + SingleInvoiceInfos.FirstOrDefault(s => s.Name == "时间")?.Value;
                                    extend.ServiceName = "交通费";
                                    break;
                                case 9:
                                case 10:
                                    invoiceCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票代码")?.Value;
                                    invoiceNo = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票号码")?.Value;
                                    amountWithTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "票价")?.Value);
                                    invoiceDate = GetDateFormat(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "日期")?.Value) + SingleInvoiceInfos.FirstOrDefault(s => s.Name == "时间")?.Value;
                                    extend.OriginationStation = SingleInvoiceInfos.SingleOrDefault(s => s.Name.Contains("始发"))?.Value;
                                    extend.ArrivalStation = SingleInvoiceInfos.SingleOrDefault(s => s.Name.Contains("目的"))?.Value;
                                    extend.ServiceName = "交通费";
                                    break;
                                case 8:
                                case 11:
                                    invoiceCode = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票代码")?.Value;
                                    invoiceNo = SingleInvoiceInfos.SingleOrDefault(s => s.Name == "发票号码")?.Value;
                                    amountWithTax = decimal.Parse(SingleInvoiceInfos.SingleOrDefault(s => s.Name == "合计金额(小写)")?.Value);
                                    invoiceDate = GetDateFormat(SingleInvoiceInfos.SingleOrDefault(s => s.Name.Contains("日期"))?.Value);
                                    break;
                                case 12:
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
                                Type = (int)item.Type,
                                Extend = extend,
                                AmountWithOutTax = amountWithOutTax,
                                SellerName =sellerName
                            };
                            outData.Add(ticketInfo);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                result.Code = 201;
                result.Message = e.ToString();
                return result;
            }
            result.Data = outData;
            return result;
        }


        /// <summary>
        /// 增值税发票核验
        /// </summary>
        public Result VatInvoiceVerify(VatInvoiceVerifyRequest req)
        {
            var result = new Result();
            VatInvoiceVerifyResponse resp = new VatInvoiceVerifyResponse();
            try
            {
                resp = _client.VatInvoiceVerifySync(req);
                result.Data = resp;
            }
            catch (Exception e)
            {
                result.Code = 201;
                result.Message = "发票核验失败";
                if (e.ToString().Contains("InvalidParameterValue"))
                {
                    result.Message = "发票信息有误";
                }
                if (e.ToString().Contains("ResourceNotFound"))
                {
                    result.Message = "发票不存在";
                }
                if (e.ToString().Contains("InvoiceMismatch"))
                {
                    result.Message = "发票数据不一致";
                }
                return result;
            }
            return result;
        }


    }
}
