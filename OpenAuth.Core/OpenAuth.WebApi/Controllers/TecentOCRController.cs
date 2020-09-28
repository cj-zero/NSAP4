using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.TecentOCR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenAuth.App.Response;
using TencentCloud.Ocr.V20181119.Models;

namespace OpenAuth.WebApi.Controllers
{

    [Route("api/ocr/[controller]/[action]")]
    [ApiController]
    public class TecentOCRController : ControllerBase
    {
        private readonly TecentOCR _tecentOCR;

        public TecentOCRController(TecentOCR tecentOCR)
        {
            _tecentOCR = tecentOCR;
        }
        /// <summary>
        /// 纳税人识别号（新威）
        /// </summary>
        public List<string> TaxCodeList = new List<string> { "91441900MA4X07PQ5X", "91440300755681916J", "91440300697120386E" };

        /// <summary>
        /// 增值税发票识别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public Result VatInvoiceOCR(VatInvoiceOCRRequest request)
        {
            var result = new Result();
            try
            {
                //InvoiceResponse invoiceresponse = new InvoiceResponse();
                ////1.识别发票
                //var r = _tecentOCR.VatInvoiceOCR(request);
                //if (r.Code == 200 && r.Data != null)
                //{
                //    VatInvoiceOCRResponse data = JsonConvert.DeserializeObject<VatInvoiceOCRResponse>(r.Data);
                //    var invoiceCode = data.VatInvoiceInfos.SingleOrDefault(s => s.Name == "发票代码").Value;
                //    var invoiceNo = data.VatInvoiceInfos.SingleOrDefault(s => s.Name == "发票号码").Value;
                //    var invoiceDate = data.VatInvoiceInfos.SingleOrDefault(s => s.Name == "开票日期").Value;
                //    var checkCode = data.VatInvoiceInfos.SingleOrDefault(s => s.Name == "校验码").Value;
                //    //2.判断发票是否已经使用

                //    //3.核验发票
                //    VatInvoiceVerifyRequest req = new VatInvoiceVerifyRequest
                //    {
                //        InvoiceCode = invoiceCode,
                //        InvoiceNo = invoiceNo.Substring(invoiceNo.Length - 2),
                //        InvoiceDate = GetDateFormat(invoiceDate),
                //        Additional = checkCode.Substring(checkCode.Length - 6)
                //    };
                //    var response = _tecentOCR.VatInvoiceVerify(req);
                //    //核验成功 返回核验结果
                //    if (response.Code == 200 && response.Data != null)
                //    {
                //        //返回核验结果
                //        VatInvoiceVerifyResponse validateData = JsonConvert.DeserializeObject<VatInvoiceVerifyResponse>(response.Data);
                //        invoiceresponse.AmountWithTax = validateData.Invoice.AmountWithTax;
                //        invoiceresponse.ComapnyTaxCode = validateData.Invoice.BuyerTaxCode;
                //        invoiceresponse.CompanyName = validateData.Invoice.BuyerName;
                //        invoiceresponse.InvoiceNo = validateData.Invoice.Number;
                //        invoiceresponse.IsValidate = 1;
                //    }
                //    else
                //    {
                //        //核验失败 返回识别结果
                //        invoiceresponse.AmountWithTax = data.VatInvoiceInfos.SingleOrDefault(s => s.Name == "小写金额").Value;
                //        invoiceresponse.ComapnyTaxCode = data.VatInvoiceInfos.SingleOrDefault(s => s.Name == "购买方识别号").Value;
                //        invoiceresponse.CompanyName = data.VatInvoiceInfos.SingleOrDefault(s => s.Name == "购买方名称").Value;
                //        invoiceresponse.InvoiceNo = invoiceNo.Substring(invoiceNo.Length - 2);
                //        invoiceresponse.IsValidate = 0;
                //    }
                //    result.Data = invoiceresponse;
                //}
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 混贴票据识别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public Result MixedInvoiceOCR(MixedInvoiceOCRRequest request)
        {
            List<object> outData = new List<object>();
            var result = new Result();
            try
            {
                //1.识别发票
                var r = _tecentOCR.MixedInvoiceOCR(request);
                if (r.Code == 200 && r.Data != null && r.Data.Count > 0)
                {
                    foreach (var item in r.Data)
                    {
                        InvoiceResponse invoiceresponse = new InvoiceResponse();
                        invoiceresponse.InvoiceNo = item.InvoiceNo;
                        invoiceresponse.AmountWithTax = item.AmountWithTax;
                        invoiceresponse.ComapnyTaxCode = item.ComapnyTaxCode;
                        invoiceresponse.CompanyName = item.CompanyName;
                        invoiceresponse.Type = item.Type;
                        //2.判断发票是否已经使用

                        int type = item.Type;
                        //判断是否为当前公司的抬头且是增值税发票才进行验证
                        if (type == 3)
                        {
                            if (TaxCodeList.Contains(item.ComapnyTaxCode))
                            {
                                // 3.核验发票(增值税发票)
                                VatInvoiceVerifyRequest req = new VatInvoiceVerifyRequest
                                {
                                    InvoiceCode = item.InvoiceCode,
                                    InvoiceNo = item.InvoiceNo,
                                    InvoiceDate = item.InvoiceDate,
                                    Additional = item.CheckCode.Length > 6 ? item.CheckCode.Substring(item.CheckCode.Length - 6) : string.Empty
                                };
                                var response = _tecentOCR.VatInvoiceVerify(req);
                                //核验成功 返回核验结果
                                if (response.Code == 200 && response.Data != null)
                                {
                                    invoiceresponse.IsValidate = 1;
                                }
                                else
                                {
                                    invoiceresponse.IsValidate = 0;
                                    invoiceresponse.NotPassReason = response.Message;
                                }
                            }
                            else
                            {
                                invoiceresponse.NotPassReason = "发票抬头和系统维护的不一样，禁止报销";
                            }
                        }
                        else
                        {
                            invoiceresponse.IsValidate = 1;
                        }
                        outData.Add(invoiceresponse);
                    }
                }
                result.Data = outData;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 增值税发票核验
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public Result VatInvoiceVerify(VatInvoiceVerifyRequest request)
        {
            var result = new Result();
            try
            {
                var r = _tecentOCR.VatInvoiceVerify(request);

                result.Data = r;
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
            }
            return result;
        }
    }
}
