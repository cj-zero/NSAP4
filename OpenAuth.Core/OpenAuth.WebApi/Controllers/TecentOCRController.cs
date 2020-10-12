using Infrastructure.TecentOCR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using OpenAuth.App;
using OpenAuth.App.Request;
using SharpDX.Direct3D11;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TencentCloud.Ocr.V20181119.Models;

namespace OpenAuth.WebApi.Controllers
{

    /// <summary>
    /// 票据识别
    /// </summary>
    [Route("api/ocr/[controller]/[action]")]
    [ApiController]
    public class TecentOCRController : ControllerBase
    {
        private readonly TecentOCR _tecentOCR;
        private readonly FileApp _fileapp;
        private ReimburseInfoApp _reimburseInfoApp;

        public TecentOCRController(TecentOCR tecentOCR, FileApp fileApp, ReimburseInfoApp reimburseInfoApp)
        {
            _tecentOCR = tecentOCR;
            _fileapp = fileApp;
            _reimburseInfoApp = reimburseInfoApp;
        }
        /// <summary>
        /// 纳税人识别号（新威）
        /// </summary>
        public List<string> TaxCodeList = new List<string> { "91441900MA4X07PQ5X", "91440300755681916J", "91440300697120386E" };

        /// <summary>
        /// 文件后缀名集合
        /// </summary>
        public List<string> FileExtensions = new List<string> { ".JPG", ".PNG", ".JPEG", ".PDF" };

        /// <summary>
        /// 票据识别
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Result> TecentInvoiceOCR(TecentOCRReq request)
        {
            List<object> outData = new List<object>();
            var result = new Result();
            try
            {
                //获取文件详情
                var file = await _fileapp.GetFileAsync(request.FileId);
                if (file is null)
                {
                    result.Code = (int)ResultCode.FileNotFound;
                    result.Message = $"fileId:{request.FileId} is not exists";
                    return result;
                }
                //支持的图片格式：PNG、JPG、JPEG，暂不支持 GIF 格式。
                if (!FileExtensions.Contains(file.Extension.ToUpper()))
                {
                    result.Code = 500;
                    result.Message = "不支持的图片格式，支持的图片格式：PNG、JPG、JPEG，文件格式只支持PDF。";
                    return result;
                }
                //获取文件格式转为base64
                var f = new FileExtensionContentTypeProvider();
                f.TryGetContentType(file.FileName, out string contentType);
                var fileStream = await _fileapp.GetFileStreamAsync(file.BucketName, file.FilePath);
                byte[] bt = new byte[fileStream.Length];
                //调用read读取方法
                fileStream.Read(bt, 0, bt.Length);
                var base64Str = Convert.ToBase64String(bt);
                //判断图片大小
                double size = base64Str.Length;// 获取文本所占字节大小
                //所下载图片经Base64编码后不超过 7M。图片下载时间不超过 3 秒。
                if (size / 1048576 > 7)
                {
                    result.Code = 500;
                    result.Message = $"fileId:{request.FileId} is more than 7M";
                    return result;
                }
                //1.识别发票
                var r = new Result();
                //1.1判断是否为PDF文件 若为PDF文件则使用VatInvoiceOCR方法进行识别
                if (".PDF".Equals(file.Extension, StringComparison.OrdinalIgnoreCase))
                {
                    var vatInvoiceOCRRequest = new VatInvoiceOCRRequest
                    {
                        ImageUrl = string.Empty,
                        ImageBase64 = base64Str,
                        IsPdf = true,
                        PdfPageNumber = 1
                    };
                    r = _tecentOCR.VatInvoiceOCR(vatInvoiceOCRRequest);
                }
                else//图片格式文件识别使用混贴MixedInvoiceOCR进行识别
                {
                    var invoiceRequest = new MixedInvoiceOCRRequest
                    {
                        Types = request.Types,
                        ImageUrl = string.Empty,
                        ImageBase64 = base64Str
                    };
                    r = _tecentOCR.MixedInvoiceOCR(invoiceRequest);
                }
                if (r.Code == 200 && r.Data != null && r.Data.Count > 0)
                {
                    //识别成功返回识别信息集合
                    foreach (var item in r.Data)
                    {
                        InvoiceResponse invoiceresponse = new InvoiceResponse();
                        invoiceresponse.InvoiceNo = item.InvoiceNo;
                        invoiceresponse.AmountWithTax = item.AmountWithTax;
                        invoiceresponse.CompanyTaxCode = item.CompanyTaxCode;
                        invoiceresponse.CompanyName = item.CompanyName;
                        invoiceresponse.Type = item.Type;
                        //2.判断发票是否已经使用 已使用不走验证
                        List<string> InvoiceNo = new List<string>();
                        InvoiceNo.Add(item.InvoiceNo);
                        if (!await _reimburseInfoApp.IsSole(InvoiceNo))
                        {
                            invoiceresponse.IsUsed = 1;
                            invoiceresponse.NotPassReason = "发票已被使用";
                        }
                        else
                        {
                            invoiceresponse.IsUsed = 0;
                            int type = item.Type;
                            //判断是否为当前公司的抬头且是增值税发票才进行验证
                            if (type == 3)
                            {
                                if (TaxCodeList.Contains(item.CompanyTaxCode))
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
                                        invoiceresponse.NotPassReason = "核验失败：" + response.Message;
                                    }
                                }
                                else
                                {
                                    invoiceresponse.IsValidate = 0;
                                    invoiceresponse.NotPassReason = "发票抬头和系统维护的不一样，禁止报销";
                                }
                            }
                            else
                            {
                                invoiceresponse.IsValidate = 1;
                            }
                        }
                        invoiceresponse.IsCanExpense = invoiceresponse.IsUsed == 0 && invoiceresponse.IsValidate == 1 ? 1 : 0;
                        outData.Add(invoiceresponse);
                    }
                }
                else
                {
                    result.Code = 500;
                    result.Message = "识别失败";
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
    }
}
