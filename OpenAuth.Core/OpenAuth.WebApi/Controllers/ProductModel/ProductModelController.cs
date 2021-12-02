using Infrastructure;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting.ModelDto;
using OpenAuth.App.ProductModel;
using OpenAuth.App.ProductModel.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.ProductModel;
using OpenAuth.Repository.Interface;
using OpenAuth.WebApi.Comm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OpenAuth.WebApi.Controllers.ProductModel
{
    [Route("api/ProductModel/[controller]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "ProductModel")]
    public class ProductModelController : Controller
    {
        private readonly ProductModelApp _productModelApp;
        IAuth _auth;
        IUnitWork UnitWork;
        ServiceBaseApp _serviceBaseApp;
        public ProductModelController(IUnitWork UnitWork, ServiceBaseApp _serviceBaseApp, IAuth _auth, ProductModelApp productModelApp)
        {
            this.UnitWork = UnitWork;
            this._serviceBaseApp = _serviceBaseApp;
            this._auth = _auth;
            _productModelApp = productModelApp;
        }
        /// <summary>
        /// 选型列表
        /// </summary>
        /// <param name="QueryModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("GetProductModelGrid")]
        public async Task<TableData> GetProductModelGrid(ProductModelReq QueryModel)
        {
            int rowcount = 0;
            var result = new TableData();
            try
            {
                result.Data = _productModelApp.GetProductModelGrid(QueryModel, out rowcount);
                result.Count = rowcount;
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }


            return result;
        }
        /// <summary>
        /// 获取设备编码
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetDeviceCodingList")]
        public async Task<Response<List<string>>> GetDeviceCodingList()
        {
            var result = new Response<List<string>>();
            try
            {
                result.Result = _productModelApp.GetDeviceCodingList();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取产品系列
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetProductTypeList")]
        public async Task<Response<List<TextVaule>>> GetProductTypeList()
        {
            var result = new Response<List<TextVaule>>();
            try
            {
                result.Result = _productModelApp.GetProductTypeList();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取电压等级
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetCurrentList")]
        public async Task<Response<List<string>>> GetCurrentList()
        {
            var result = new Response<List<string>>();
            try
            {
                result.Result = _productModelApp.GetCurrentList();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取电流等级
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetVoltageList")]
        public async Task<Response<List<string>>> GetVoltageList()
        {
            var result = new Response<List<string>>();
            try
            {
                result.Result = _productModelApp.GetCurrentList();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取通道数
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetChannelList")]
        public async Task<Response<List<int>>> GetChannelList()
        {
            var result = new Response<List<int>>();
            try
            {
                result.Result = _productModelApp.GetChannelList();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取功耗等级
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetTotalPowerList")]
        public async Task<Response<List<string>>> GetTotalPowerList()
        {
            var result = new Response<List<string>>();
            try
            {
                result.Result = _productModelApp.GetTotalPowerList();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取参数规格
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetSpecifications")]
        public async Task<Response<ProductModelDetails>> GetSpecifications(int Id, string Language)
        {
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;

            var result = new Response<ProductModelDetails>();
            try
            {
                result.Result = _productModelApp.GetSpecifications(Id, host, Language);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取产品手册banner
        /// </summary>
        /// <param name="ProductModelCategoryId"></param>
        [HttpGet]
        [Route("GetProductImg")]
        public async Task<Response<List<string>>> GetProductImg(int ProductModelTypeId)
        {
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            var result = new Response<List<string>>();
            try
            {
                result.Result = _productModelApp.GetProductImg(ProductModelTypeId, host);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        /// 获取应用案例banner
        /// </summary>
        /// <param name="ProductModelCategoryId"></param>
        [HttpGet]
        [Route("GetCaseImage")]
        public async Task<Response<List<string>>> GetCaseImage(int ProductModelCategoryId)
        {
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            var result = new Response<List<string>>();
            try
            {
                result.Result = _productModelApp.GetCaseImage(ProductModelCategoryId, host);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 下载规格书
        /// </summary>
        /// <param name="Id"></param>
        [HttpGet]
        [Route("ExportProductSpecsDoc")]
        public async Task<Response<string>> ExportProductSpecsDoc(int Id, string Language)
        {
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            var result = new Response<string>();
            try
            {

                var productModelSelection = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete && u.Id == Id).FirstOrDefault();
                var type = UnitWork.FindSingle<ProductModelType>(q => q.Id == productModelSelection.ProductModelTypeId);

                if (productModelSelection != null)
                {
                    var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == productModelSelection.ProductModelCategoryId).FirstOrDefault();
                    var productModelSelectionInfo = UnitWork.Find<ProductModelSelectionInfo>(u => !u.IsDelete && u.ProductModelSelectionId == productModelSelection.Id).FirstOrDefault();
                    var productModelDetails = _productModelApp.GetSpecifications(Id, null, Language);
                    string templatePath = "";

                    if (Language == "CN")
                    {
                        templatePath = Path.Combine(Directory.GetCurrentDirectory() + productModelCategory.SpecsDocTemplatePath_CH);
                    }
                    if (Language == "EN")
                    {
                        templatePath = Path.Combine(Directory.GetCurrentDirectory() + productModelCategory.SpecsDocTemplatePath_EN);

                    }
                    string filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Templates\\files\\" + DateTime.Now.ToString("yyyyMMdd") + "\\");
                    ProductParamTemplate productParamTemplate = new ProductParamTemplate()
                    {
                        Title = productModelSelection.DeviceCoding,
                        DeviceCoding = productModelSelection.DeviceCoding,
                        ChannelNumber = productModelSelection.ChannelNumber.ToString(),
                        InputPowerType = productModelDetails.InputPowerType,
                        InputActivePower = productModelDetails.InputActivePower,
                        InputCurrent = productModelDetails.InputCurrent,
                        Efficiency = productModelDetails.Efficiency,
                        Noise = productModelDetails.Noise,
                        DeviceType = productModelDetails.DeviceType,
                        PowerControlModuleType = productModelDetails.PowerControlModuleType,
                        PowerConnection = productModelDetails.PowerConnection,
                        ChargeVoltageRange = productModelDetails.ChargeVoltageRange,
                        DischargeVoltageRange = productModelDetails.DischargeVoltageRange,
                        MinimumDischargeVoltage = productModelDetails.MinimumDischargeVoltage,
                        CurrentRange = productModelDetails.CurrentRange,
                        CurrentAccurack = productModelDetails.CurrentAccurack,
                        CutOffCurrent = productModelDetails.CutOffCurrent,
                        SinglePower = productModelDetails.SinglePower,
                        CurrentResponseTime = productModelDetails.CurrentResponseTime,
                        CurrentConversionTime = productModelDetails.CurrentConversionTime,
                        RecordFreq = productModelDetails.RecordFreq,
                        MinimumVoltageInterval = productModelDetails.MinimumVoltageInterval,
                        MinimumCurrentInterval = productModelDetails.MinimumCurrentInterval,
                        TotalPower = productModelDetails.TotalPower,
                        Size = productModelDetails.Size,
                        Weights = productModelDetails.Weight,
                        VoltageAccuracy = productModelSelectionInfo.VoltAccurack


                    };
                    SpireDocWord.GetDocument(templatePath);
                    SpireDocWord.ReplaseTemplateWord(productParamTemplate);
                    SpireDocWord.AddImage(Path.Combine(Directory.GetCurrentDirectory() + type.Image));
                    SpireDocWord.CreateNewWord(filePath + productModelSelection.DeviceCoding + "-技术规格书" + ".docx");
                }
                result.Result = host + "/Templates/files/" + DateTime.Now.ToString("yyyyMMdd") + "/" + productModelSelection.DeviceCoding + "-技术规格书.docx";
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 获取编码规则
        /// </summary>
        [HttpGet]
        [Route("GetCodingRules")]
        public async Task<Response<string>> GetCodingRules()
        {
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            var result = new Response<string>();
            try
            {
                result.Result = _productModelApp.GetCodingRules(host);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
        /// <summary>
        ///线材计算及下载
        /// </summary>
        [HttpGet]
        [Route("GetCalculation")]
        public async Task<Response<string>> GetCalculation()
        {
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            var result = new Response<string>();
            try
            {
                result.Result = _productModelApp.GetCalculation(host);
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }

        /// <summary>
        /// 技术协议书
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Language"></param>
        /// <returns></returns>
        [HttpGet]
        [Route("TechnicalDoc")]
        public async Task<Response<string>> TechnicalDoc(int Id, string Language)
        {
            string host = HttpContext.Request.Scheme + "://" + HttpContext.Request.Host;
            var result = new Response<string>();
            try
            {
                var productModelSelection = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete && u.Id == Id).FirstOrDefault();
                var type = UnitWork.FindSingle<ProductModelType>(q => q.Id == productModelSelection.ProductModelTypeId);

                if (productModelSelection != null)
                {
                    var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == productModelSelection.ProductModelCategoryId).FirstOrDefault();
                    var productModelSelectionInfo = UnitWork.Find<ProductModelSelectionInfo>(u => !u.IsDelete && u.ProductModelSelectionId == productModelSelection.Id).FirstOrDefault();
                    var productModelDetails = _productModelApp.GetSpecifications(Id, null, Language);
                    string templatePath = "";

                    if (Language == "CN")
                    {

                        templatePath = Path.Combine(Directory.GetCurrentDirectory() + type.TAgreementDocTemplatePath_CH);


                    }
                    if (Language == "EN")
                    {
                        templatePath = Path.Combine(Directory.GetCurrentDirectory() + type.TAgreementDocTemplatePath_EN);

                    }


                    string filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Templates\\files\\" + DateTime.Now.ToString("yyyyMMdd") + "\\");

                    ProductParamTemplate productParamTemplate = new ProductParamTemplate()
                    {
                        Title = productModelSelection.DeviceCoding,
                        DeviceCoding = productModelSelection.DeviceCoding,
                        ChannelNumber = productModelSelection.ChannelNumber.ToString(),
                        InputPowerType = productModelDetails.InputPowerType,
                        InputActivePower = productModelDetails.InputActivePower,
                        InputCurrent = productModelDetails.InputCurrent,
                        Efficiency = productModelDetails.Efficiency,
                        Noise = productModelDetails.Noise,
                        DeviceType = productModelDetails.DeviceType,
                        PowerControlModuleType = productModelDetails.PowerControlModuleType,
                        PowerConnection = productModelDetails.PowerConnection,
                        ChargeVoltageRange = productModelDetails.ChargeVoltageRange,
                        DischargeVoltageRange = productModelDetails.DischargeVoltageRange,
                        MinimumDischargeVoltage = productModelDetails.MinimumDischargeVoltage,
                        CurrentRange = productModelDetails.CurrentRange,
                        CurrentAccurack = productModelDetails.CurrentAccurack,
                        CutOffCurrent = productModelDetails.CutOffCurrent,
                        SinglePower = productModelDetails.SinglePower,
                        CurrentResponseTime = productModelDetails.CurrentResponseTime,
                        CurrentConversionTime = productModelDetails.CurrentConversionTime,
                        RecordFreq = productModelDetails.RecordFreq,
                        MinimumVoltageInterval = productModelDetails.MinimumVoltageInterval,
                        MinimumCurrentInterval = productModelDetails.MinimumCurrentInterval,
                        TotalPower = productModelDetails.TotalPower,
                        Size = productModelDetails.Size != null ? productModelDetails.Size : "0.0",
                        Weights = productModelSelection.Weight.ToString(),
                        VoltageAccuracy = productModelSelectionInfo.VoltAccurack

                    };
                    SpireDocWord.GetDocument(templatePath);
                    SpireDocWord.ReplaseTemplateWord(productParamTemplate);
                    SpireDocWord.AddImageTechnical(Path.Combine(Directory.GetCurrentDirectory() + type.Image));
                    SpireDocWord.CreateNewWord(filePath + productModelSelection.DeviceCoding + "-技术规格协议书" + ".docx");
                }
                result.Result = host + "/Templates/files/" + DateTime.Now.ToString("yyyyMMdd") + "/" + productModelSelection.DeviceCoding + "-技术规格协议书.docx";

            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }


        /// <summary>
        ///CE-6000宣传视频下载
        /// </summary>
        [HttpGet]
        [Route("GetVideo")]
        public async Task<Response<List<TextVauleString>>> GetVideo()
        {

            var result = new Response<List<TextVauleString>>();
            try
            {
                result.Result = _productModelApp.GetVideo();
            }
            catch (Exception ex)
            {

                result.Message = ex.Message;
            }
            return result;
        }
    }
}
