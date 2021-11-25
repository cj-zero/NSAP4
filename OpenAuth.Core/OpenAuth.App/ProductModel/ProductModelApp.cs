using Infrastructure;
using Infrastructure.Helpers;
using Infrastructure.Wrod;
using Newtonsoft.Json;
using OpenAuth.App.Interface;
using OpenAuth.App.Meeting.ModelDto;
using OpenAuth.App.ProductModel;
using OpenAuth.App.ProductModel.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.Repository.Domain.ProductModel;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;


namespace OpenAuth.App
{
    /// <summary>
    /// 商品选项服务
    /// </summary>
    public class ProductModelApp : OnlyUnitWorkBaeApp
    {
        private RevelanceManagerApp _revelanceApp;
        ServiceBaseApp _serviceBaseApp;
        public ProductModelApp(ServiceBaseApp serviceBaseApp, IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _serviceBaseApp = serviceBaseApp;

        }
        /// <summary>
        /// 获取设备编码
        /// </summary>
        /// <returns></returns>
        public List<string> GetDeviceCodingList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete && !u.DeviceCoding.Contains("CTE"));
            return productModelSelections.Select(zw => zw.DeviceCoding).OrderBy(zw => zw).Distinct().ToList();
        }
        /// <summary>
        /// 获取产品系列
        /// </summary>
        /// <returns></returns>
        public List<TextVaule> GetProductTypeList()
        {
            var list = new List<TextVaule>();

            var productModelSelections = UnitWork.Find<ProductModelType>(u => !u.IsDelete);
            var data = productModelSelections.OrderBy(zw => zw).Distinct().ToList();
            list = data.Select(m => new TextVaule
            {
                Text = m.Name,
                Value = m.Id
            }).ToList();
            return list;
        }
        /// <summary>
        /// 获取电流等级
        /// </summary>
        /// <returns></returns>
        public List<string> GetVoltageList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.Voltage).Distinct().OrderBy(int.Parse).ToList();
        }
        /// <summary>
        /// 获取电压等级
        /// </summary>
        /// <returns></returns>
        public List<string> GetCurrentList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.Current).Distinct().OrderBy(int.Parse).ToList();
        }
        /// <summary>
        /// 获取通道数量
        /// </summary>
        /// <returns></returns>
        public List<int> GetChannelList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.ChannelNumber).Distinct().OrderBy(zw => zw).ToList();
        }
        /// <summary>
        /// 获取功率
        /// </summary>
        /// <returns></returns>
        public List<string> GetTotalPowerList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.TotalPower).Distinct().OrderBy(float.Parse).ToList();
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public List<ProductModelInfo> GetProductModelGrid(ProductModelReq queryModel, out int rowcount)
        {
            Expression<Func<ProductModelSelection, bool>> exps = t => true;
            exps = exps.And(t => !t.IsDelete);
            exps = exps.And(t =>  !t.DeviceCoding.Contains("CTE"));
            if (!string.IsNullOrWhiteSpace(queryModel.ProductType))
            {
                exps = exps.And(t => t.ProductType == queryModel.ProductType);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.DeviceCoding))
            {
                exps = exps.And(t => t.DeviceCoding.Contains(queryModel.DeviceCoding));
            }
            if (!string.IsNullOrWhiteSpace(queryModel.Voltage))
            {
                exps = exps.And(t => t.Voltage == queryModel.Voltage);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.Current))
            {
                exps = exps.And(t => t.Current == queryModel.Current);
            }
            if (!string.IsNullOrWhiteSpace(queryModel.TotalPower))
            {
                exps = exps.And(t => t.TotalPower == queryModel.TotalPower);
            }
            if (queryModel.ChannelNumber > 0)
            {
                exps = exps.And(t => t.ChannelNumber == queryModel.ChannelNumber);
            }
            if (queryModel.ProductModelCategoryId != -1)
            {
                exps = exps.And(t => t.ProductModelCategoryId == queryModel.ProductModelCategoryId);

            }
            var productModelSelectionList = UnitWork.Find(queryModel.page, queryModel.limit, "Id", exps);
            rowcount = UnitWork.GetCount(exps);
            return productModelSelectionList.MapToList<ProductModelInfo>();

        }
        /// <summary>
        /// 获取产品手册
        /// </summary>
        /// <param name="ProductModelCategoryId"></param>
        /// <returns></returns>
        public List<string> GetProductImg(int ProductModelTypeId, string host)
        {
            List<string> imgs = new List<string>();
            if (ProductModelTypeId != 0)
            {
                var productModelCategory = UnitWork.Find<ProductModelType>(u => !u.IsDelete && u.Id == ProductModelTypeId)?.FirstOrDefault();
                if (productModelCategory != null && !string.IsNullOrWhiteSpace(productModelCategory.ImageBanner))
                {
                    foreach (var item in productModelCategory.ImageBanner.Replace("\r", "").Replace("\n", "").TrimEnd(',').Split(','))
                    {
                        imgs.Add(host + item);
                    }

                    //imgs = JsonConvert.DeserializeObject<List<string>>(productModelCategory.ImageBanner);
                }

            }
            else
            {
                imgs.Add(host + "/Templates/files/images/产品手册1.jpg");
                imgs.Add(host + "/Templates/files/images/产品手册2.jpg");
                var productModelCategory = UnitWork.Find<ProductModelType>(u => !u.IsDelete)?.ToList();
                foreach (var item in productModelCategory)
                {
                    foreach (var scon in item.ImageBanner.Replace("\r", "").Replace("\n", "").TrimEnd(',').Split(','))
                    {
                        imgs.Add(host + scon);
                    }

                }
               
            }

            return imgs;
        }



        /// <summary>
        /// 获取应用案例
        /// </summary>
        /// <param name="ProductModelCategoryId"></param>
        /// <returns></returns>
        public List<string> GetCaseImage(int ProductModelCategoryId, string host)
        {
            List<string> imgs = new List<string>();
            var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == ProductModelCategoryId)?.FirstOrDefault();
            if (productModelCategory != null && !string.IsNullOrWhiteSpace(productModelCategory.CaseImage))
            {
                foreach (var item in productModelCategory.CaseImage.Replace("\r", "").Replace("\n", "").TrimEnd(',').Split(','))
                {
                    imgs.Add(host + item);
                }
                //imgs = JsonConvert.DeserializeObject<List<string>>(productModelCategory.CaseImage);
            }
            return imgs;
        }
        /// <summary>
        /// 导出规格说明书
        /// </summary>
        //public string ExportProductSpecsDoc(int Id, string host, string Language)
        //{
        //    var productModelSelection = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete && u.Id == Id).FirstOrDefault();
        //    var type = UnitWork.FindSingle<ProductModelType>(q => q.Id == productModelSelection.ProductModelTypeId);

        //    if (productModelSelection != null)
        //    {
        //        var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == productModelSelection.ProductModelCategoryId).FirstOrDefault();
        //        var productModelSelectionInfo = UnitWork.Find<ProductModelSelectionInfo>(u => !u.IsDelete && u.ProductModelSelectionId == productModelSelection.Id).FirstOrDefault();
        //        var productModelDetails = GetSpecifications(Id, null, Language);
        //        string templatePath = "";
        //        //List<WordMarkModel> wordModels = new List<WordMarkModel>() {

        //        //   new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.ChannelNumber),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.ChannelNumber
        //        //},
        //        //          new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.InputPowerType),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.InputPowerType
        //        //},
        //        //new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.InputActivePower),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.InputActivePower
        //        //},
        //        //new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.InputCurrent),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.InputCurrent
        //        //},       new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.Efficiency),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.Efficiency
        //        //},       new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.Noise),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.Noise
        //        //},       new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.DeviceType),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.DeviceType
        //        //},       new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.PowerControlModuleType),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.PowerControlModuleType
        //        //},       new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.PowerConnection),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.PowerConnection
        //        //},
        //        //           new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.ChargeVoltageRange),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.ChargeVoltageRange
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.DischargeVoltageRange),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.DischargeVoltageRange
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.MinimumDischargeVoltage),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.MinimumDischargeVoltage
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.CurrentRange),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.CurrentRange
        //        //},
        //        //            new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.CurrentAccurack),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.CurrentAccurack
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.CutOffCurrent),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.CutOffCurrent
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.SinglePower),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.SinglePower
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.CurrentResponseTime),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.CurrentResponseTime
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.CurrentConversionTime),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.CurrentConversionTime
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.RecordFreq),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.RecordFreq
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.MinimumVoltageInterval),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.MinimumVoltageInterval
        //        //},
        //        //             new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.MinimumCurrentInterval),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.MinimumCurrentInterval
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.TotalPower),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.TotalPower
        //        //}, new WordMarkModel(){
        //        // MarkName=nameof(ProductModelDetails.Size),
        //        // MarkType=0,
        //        // MarkValue=productModelDetails.Size
        //        //},
        //        //};
        //        if (Language == "CN")
        //        {
        //            templatePath = Path.Combine(Directory.GetCurrentDirectory() + productModelCategory.SpecsDocTemplatePath_CH);
        //        }
        //        if (Language == "EN")
        //        {
        //            templatePath = Path.Combine(Directory.GetCurrentDirectory() + productModelCategory.SpecsDocTemplatePath_EN);

        //        }
        //        //var ParamTemplate = new
        //        //{
        //        //    DeviceCoding = productModelSelection.DeviceCoding,
        //        //    ChannelNumber = productModelSelection.ChannelNumber,
        //        //    InputPowerType = productModelDetails.InputPowerType,
        //        //    InputActivePower = productModelDetails.InputActivePower,
        //        //    InputCurrent = productModelDetails.InputCurrent,
        //        //    Efficiency = productModelDetails.Efficiency,
        //        //    Noise = productModelDetails.Noise,
        //        //    DeviceType = productModelDetails.DeviceType,
        //        //    PowerControlModuleType = productModelDetails.PowerControlModuleType,
        //        //    PowerConnection = productModelDetails.PowerConnection,
        //        //    ChargeVoltageRange = productModelDetails.ChargeVoltageRange,
        //        //    DischargeVoltageRange = productModelDetails.DischargeVoltageRange,
        //        //    MinimumDischargeVoltage = productModelDetails.MinimumDischargeVoltage,
        //        //    CurrentRange = productModelDetails.CurrentRange,
        //        //    CurrentAccurack = productModelDetails.CurrentAccurack,
        //        //    CutOffCurrent = productModelDetails.CutOffCurrent,
        //        //    SinglePower = productModelDetails.SinglePower,
        //        //    CurrentResponseTime = productModelDetails.CurrentResponseTime,
        //        //    CurrentConversionTime = productModelDetails.CurrentConversionTime,
        //        //    RecordFreq = productModelDetails.RecordFreq,
        //        //    MinimumVoltageInterval = productModelDetails.MinimumVoltageInterval,
        //        //    MinimumCurrentInterval = productModelDetails.MinimumCurrentInterval,
        //        //    TotalPower = productModelDetails.TotalPower,
        //        //    Size = productModelDetails.Size
        //        //};

        //        string filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Templates\\files\\" + DateTime.Now.ToString("yyyyMMdd") + "\\");
        //        //object[] oBookMark = wordModels.Select(zw => (object)zw.MarkName).Distinct().ToArray();

        //        //WordTemplateHelper.WordTemplateReplace(templatePath,
        //        //    filePath + pdfName,
        //        //    new Dictionary<string, string>()
        //        //    {
        //        //        ["DeviceCoding"] = productModelDetails.DeviceCoding,
        //        //        ["InputActivePower"] = productModelDetails.InputActivePower,
        //        //        ["InputPowerType"] = productModelDetails.InputPowerType.ToString(),
        //        //        ["InputCurrent"] = productModelDetails.InputCurrent,
        //        //        ["Efficiency"] = productModelDetails.Efficiency,
        //        //        ["Noise"] = productModelDetails.Noise,
        //        //        ["DeviceType"] = productModelDetails.DeviceType,
        //        //        ["PowerControlModuleType"] = productModelDetails.PowerControlModuleType,
        //        //        ["PowerConnection"] = productModelDetails.PowerConnection,
        //        //        ["ChargeVoltageRange"] = productModelDetails.ChargeVoltageRange,
        //        //        ["DischargeVoltageRange"] = productModelDetails.DischargeVoltageRange,
        //        //        ["MinimumDischargeVoltage"] = productModelDetails.MinimumDischargeVoltage,
        //        //        ["CurrentRange"] = productModelDetails.CurrentRange,
        //        //        ["CurrentAccurack"] = productModelDetails.CurrentAccurack,
        //        //        ["CutOffCurrent"] = productModelDetails.CutOffCurrent,
        //        //        ["SinglePower"] = productModelDetails.SinglePower,
        //        //        ["CurrentResponseTime"] = productModelDetails.CurrentResponseTime,
        //        //        ["CurrentConversionTime"] = productModelDetails.CurrentConversionTime,
        //        //        ["RecordFreq"] = productModelDetails.RecordFreq,
        //        //        ["MinimumVoltageInterval"] = productModelDetails.MinimumVoltageInterval,
        //        //        ["MinimumCurrentInterval"] = productModelDetails.MinimumCurrentInterval,
        //        //        ["TotalPower"] = productModelDetails.TotalPower,
        //        //        ["Size"] = productModelDetails.Size
        //        //    });
        //        //FileHelper.DOCTemplateConvert(templatePath, filePath + pdfName, wordModels, oBookMark);
        //        //WordTemplateHelper.WriteToPublicationOfResult(templatePath, filePath + pdfName, WordTemplateHelper.getProperties(ParamTemplate));
        //        ProductParamTemplate productParamTemplate = new ProductParamTemplate()
        //        {
        //            Title = productModelSelection.DeviceCoding,
        //            DeviceCoding = productModelSelection.DeviceCoding,
        //            ChannelNumber = productModelSelection.ChannelNumber.ToString(),
        //            InputPowerType = productModelDetails.InputPowerType,
        //            InputActivePower = productModelDetails.InputActivePower,
        //            InputCurrent = productModelDetails.InputCurrent,
        //            Efficiency = productModelDetails.Efficiency,
        //            Noise = productModelDetails.Noise,
        //            DeviceType = productModelDetails.DeviceType,
        //            PowerControlModuleType = productModelDetails.PowerControlModuleType,
        //            PowerConnection = productModelDetails.PowerConnection,
        //            ChargeVoltageRange = productModelDetails.ChargeVoltageRange,
        //            DischargeVoltageRange = productModelDetails.DischargeVoltageRange,
        //            MinimumDischargeVoltage = productModelDetails.MinimumDischargeVoltage,
        //            CurrentRange = productModelDetails.CurrentRange,
        //            CurrentAccurack = productModelDetails.CurrentAccurack,
        //            CutOffCurrent = productModelDetails.CutOffCurrent,
        //            SinglePower = productModelDetails.SinglePower,
        //            CurrentResponseTime = productModelDetails.CurrentResponseTime,
        //            CurrentConversionTime = productModelDetails.CurrentConversionTime,
        //            RecordFreq = productModelDetails.RecordFreq,
        //            MinimumVoltageInterval = productModelDetails.MinimumVoltageInterval,
        //            MinimumCurrentInterval = productModelDetails.MinimumCurrentInterval,
        //            TotalPower = productModelDetails.TotalPower,
        //            Size = productModelDetails.Size,
        //            Image = host + type.Image,
        //            Weight = productModelSelection.Weight.ToString()

        //        };
        //        SpireDocWord.GetDocument(templatePath);
        //        SpireDocWord.ReplaseTemplateWord(productParamTemplate);
        //        SpireDocWord.CreateNewWord(filePath + productModelSelection.DeviceCoding + "-技术规格书" + ".docx");
        //    }
        //    return host + "/Templates/files/" + DateTime.Now.ToString("yyyyMMdd") + "/" + productModelSelection.DeviceCoding + "-技术规格书.docx";

        //}

        //public string TechnicalDoc(int Id, string host, string Language)
        //{
        //    var productModelSelection = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete && u.Id == Id).FirstOrDefault();
        //    var type = UnitWork.FindSingle<ProductModelType>(q => q.Id == productModelSelection.ProductModelTypeId);

        //    if (productModelSelection != null)
        //    {
        //        var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == productModelSelection.ProductModelCategoryId).FirstOrDefault();
        //        var productModelSelectionInfo = UnitWork.Find<ProductModelSelectionInfo>(u => !u.IsDelete && u.ProductModelSelectionId == productModelSelection.Id).FirstOrDefault();
        //        var productModelDetails = GetSpecifications(Id, null, Language);
        //        string templatePath = "";

        //        if (Language == "CN")
        //        {

        //            templatePath = Path.Combine(Directory.GetCurrentDirectory() + type.TAgreementDocTemplatePath_CH);


        //        }
        //        if (Language == "EN")
        //        {
        //            templatePath = Path.Combine(Directory.GetCurrentDirectory() + type.TAgreementDocTemplatePath_EN);

        //        }


        //        string filePath = Path.Combine(Directory.GetCurrentDirectory() + "\\Templates\\files\\" + DateTime.Now.ToString("yyyyMMdd") + "\\");

        //        ProductParamTemplate productParamTemplate = new ProductParamTemplate()
        //        {
        //            Title = productModelSelection.DeviceCoding,
        //            DeviceCoding = productModelSelection.DeviceCoding,
        //            ChannelNumber = productModelSelection.ChannelNumber.ToString(),
        //            InputPowerType = productModelDetails.InputPowerType,
        //            InputActivePower = productModelDetails.InputActivePower,
        //            InputCurrent = productModelDetails.InputCurrent,
        //            Efficiency = productModelDetails.Efficiency,
        //            Noise = productModelDetails.Noise,
        //            DeviceType = productModelDetails.DeviceType,
        //            PowerControlModuleType = productModelDetails.PowerControlModuleType,
        //            PowerConnection = productModelDetails.PowerConnection,
        //            ChargeVoltageRange = productModelDetails.ChargeVoltageRange,
        //            DischargeVoltageRange = productModelDetails.DischargeVoltageRange,
        //            MinimumDischargeVoltage = productModelDetails.MinimumDischargeVoltage,
        //            CurrentRange = productModelDetails.CurrentRange,
        //            CurrentAccurack = productModelDetails.CurrentAccurack,
        //            CutOffCurrent = productModelDetails.CutOffCurrent,
        //            SinglePower = productModelDetails.SinglePower,
        //            CurrentResponseTime = productModelDetails.CurrentResponseTime,
        //            CurrentConversionTime = productModelDetails.CurrentConversionTime,
        //            RecordFreq = productModelDetails.RecordFreq,
        //            MinimumVoltageInterval = productModelDetails.MinimumVoltageInterval,
        //            MinimumCurrentInterval = productModelDetails.MinimumCurrentInterval,
        //            TotalPower = productModelDetails.TotalPower,
        //            Size = productModelDetails.Size != null ? productModelDetails.Size : "0.0",
        //            Weight=productModelSelection.Weight.ToString(),
        //            Image = host + type.Image
        //        };
        //        SpireDocWord.GetDocument(templatePath);
        //        SpireDocWord.ReplaseTemplateWord(productParamTemplate);
        //        SpireDocWord.CreateNewWord(filePath + productModelSelection.DeviceCoding + "-技术规格协议书" + ".docx");
        //    }
        //    return host + "/Templates/files/" + DateTime.Now.ToString("yyyyMMdd") + "/" + productModelSelection.DeviceCoding + "-技术规格协议书.docx";
        //}

        public string GetCalculation(string host)
        {
            return host + @"/Templates/CE-6000n系列AC交流输入电源线计算&下单说明.pdf";
        }

        public string GetCodingRules(string host)
        {
            return host + @"/Templates/files/images/rulecode.jpg";
        }

        /// <summary>
        /// 产品规格
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ProductModelDetails GetSpecifications(int Id, string? host, string Language)

        {
            var result = new ProductModelDetails();
            var productmodelselection = UnitWork.FindSingle<ProductModelSelection>(q => q.Id == Id);
            var productmodeltype = UnitWork.FindSingle<ProductModelType>(q => q.Id == productmodelselection.ProductModelTypeId);
            var productmodelselectioninfo = UnitWork.FindSingle<ProductModelSelectionInfo>(q => q.ProductModelSelectionId == productmodelselection.Id);
            result.DeviceCoding = productmodelselection.DeviceCoding;
            result.ChannelNumber = productmodelselection.ChannelNumber;
            result.InputPowerType = productmodelselectioninfo.InputPowerType;
            result.InputActivePower = productmodelselectioninfo.InputActivePower + "KW";
            result.InputCurrent = productmodelselectioninfo.InputCurrent + "A/每相";
            result.Weight = "约" + productmodelselection.Weight.ToString() + "KG";
            if (Language == "CN")
            {
                if (productmodeltype.Name == "模块机")
                {
                    result.Efficiency = "90%";
                    result.Noise = "≤65dB";
                    result.DeviceType = "四线制连接(充放电异口)";
                    result.PowerControlModuleType = "MOSFET";
                    if (productmodelselectioninfo.InputPowerType.Contains("AC220V"))
                    {
                        result.PowerConnection = "单相三线";
                    }
                    else
                    {
                        result.PowerConnection = "三相五线";
                    }
                    result.CurrentResponseTime = "≤3ms";
                    result.CurrentConversionTime = "≤6ms";

                }
                if (productmodeltype.Name == "塔式机")
                {
                    result.Efficiency = "94%";
                    result.Noise = "≤75dB";
                    result.DeviceType = "四线制连接(充放电同口)";
                    result.PowerControlModuleType = "IGBT";
                    result.PowerConnection = "三相四线";
                    result.CurrentResponseTime = "≤5ms";
                    result.CurrentConversionTime = "≤10ms";

                }
                result.VoltageAccuracy = productmodelselectioninfo.VoltAccurack;
                result.ChargeVoltageRange = "充电：0" + "V~" + productmodelselection.Voltage + "V";
                result.DischargeVoltageRange = "放电：" + productmodelselectioninfo.MinimumDischargeVoltage + "V~" + productmodelselection.Voltage + "V";
                result.MinimumDischargeVoltage = productmodelselectioninfo.MinimumDischargeVoltage+"V";
                result.CurrentRange = (float.Parse(productmodelselection.Current) * 0.005).ToString() + "A~" + productmodelselection.Current + "A";
                result.CurrentAccurack = productmodelselection.CurrentAccurack;
                float Temp = (float.Parse(productmodelselection.Current) * 1000);
                if (Temp >= 30000)
                {
                    Temp = (float)(Temp * 0.001);
                }
                else
                {
                    Temp = 30;//

                }
                result.CutOffCurrent = Temp.ToString() + "mA";
                Temp = (float)(float.Parse(productmodelselection.Voltage)* float.Parse(productmodelselection.Current) * 0.001);

                result.SinglePower = Temp.ToString() + "KW";
                result.RecordFreq = productmodelselectioninfo.Fre;
                if (productmodelselectioninfo.Fre == "100HZ")
                {
                    result.RecordFreq = productmodelselectioninfo.Fre + "(接入辅助通道为10HZ)";

                }
                Temp = (float)(float.Parse(productmodelselection.Voltage) * 0.002);
                result.MinimumVoltageInterval = "最小电压间隔: " + Temp + "V";
                result.MinimumCurrentInterval = "最小电流间隔: " + Temp + "A";//Minimum current interval: 0.1A
                result.TotalPower = productmodelselection.TotalPower+"KW";
                result.Size = productmodelselection.Size;
                result.Pic = host + productmodeltype.Image;
            }
            else if (Language == "EN")
            {
                if (productmodeltype.Name == "模块机")
                {
                    result.Efficiency = "90%";
                    result.Noise = "≤65dB";
                    result.DeviceType = "Four-wire connection(different port for charging and discharging)";
                    result.PowerControlModuleType = "MOSFET";
                    if (productmodelselectioninfo.InputPowerType.Contains("AC220V"))
                    {
                        result.PowerConnection = "Single-phase-four wire system";
                    }
                    else
                    {
                        result.PowerConnection = "Three-phase-five wire system";
                    }
                    result.CurrentResponseTime = "≤3ms";
                    result.CurrentConversionTime = "≤6ms";


                }
                if (productmodeltype.Name == "塔式机")
                {
                    result.Efficiency = "94%";
                    result.Noise = "≤75dB";
                    result.DeviceType = "Four-wire connection(same port for charging and discharging)";
                    result.PowerControlModuleType = "IGBT";
                    result.PowerConnection = "Three-phase-four wire system";
                    result.CurrentResponseTime = "≤5ms";
                    result.CurrentConversionTime = "≤10ms";



                }
                result.VoltageAccuracy = productmodelselectioninfo.VoltAccurack;
                result.ChargeVoltageRange = "Charge：0" + "V~" + productmodelselection.Voltage + "V";
                result.DischargeVoltageRange = "Discharge：" + productmodelselectioninfo.MinimumDischargeVoltage + "V~" + productmodelselection.Voltage + "V";
                result.MinimumDischargeVoltage = productmodelselectioninfo.MinimumDischargeVoltage;
                result.CurrentRange = (float.Parse(productmodelselection.Current) * 0.005).ToString() + "A~" + productmodelselection.Current + "A";
                result.CurrentAccurack = productmodelselection.CurrentAccurack;
                float Temp = (float.Parse(productmodelselection.Current) * 1000);
                if (Temp >= 30000)
                {
                    Temp = (float)(Temp * 0.001);
                }
                else
                {
                    Temp = 30;//

                }
                result.CutOffCurrent = Temp.ToString();
                Temp = (float)(float.Parse(productmodelselection.Voltage) * float.Parse(productmodelselection.Current) * 0.001);
                result.SinglePower = Temp.ToString() + "KW";
                result.RecordFreq = productmodelselectioninfo.Fre;
                if (productmodelselectioninfo.Fre == "100HZ")
                {
                    result.RecordFreq = productmodelselectioninfo.Fre + "(connected with AUX channel:10Hz)";

                }
                Temp = (float)(float.Parse(productmodelselection.Voltage) * 0.002);
                result.MinimumVoltageInterval = "Minimum voltage interval: " + Temp + "V";
                result.MinimumCurrentInterval = "Minimum current interval: " + Temp + "A";//Minimum current interval: 0.1A
                result.TotalPower = productmodelselection.TotalPower+"KW";
                result.Size = productmodelselection.Size;
                result.Pic = host + productmodeltype.Image;
            }

            return result;
        }
        /// <summary>
        /// 产品选型视频介绍
        /// </summary>
        /// <returns></returns>
        public List<TextVauleString> GetVideo( )
        {
            var data = new List<TextVauleString>();
            data.Add(new TextVauleString { Id=1,Name= "CE-6000 模块机整机解析视频.mp4", Value= "https://file.neware.com.cn/CE-6000%20%E6%A8%A1%E5%9D%97%E6%9C%BA%E6%95%B4%E6%9C%BA%E8%A7%A3%E6%9E%90%E8%A7%86%E9%A2%912020.12.10~1_converted.mp4.zip"});//CE-6000 模块机整机解析视频
            data.Add(new TextVauleString { Id=2,Name= "CE-6000 模块机组装3D.mp4", Value= "https://file.neware.com.cn/CE-6000%20%E6%A8%A1%E5%9D%97%E6%9C%BA%E7%BB%84%E8%A3%853D%20Video%202020.8.24.mp4.zip"});//CE-6000 模块机组装3D
            data.Add(new TextVauleString { Id=3,Name= "CE-6000n 模块机系列整机生产测试指导.pdf", Value= "https://file.neware.com.cn/CE-6000n%20%E6%A8%A1%E5%9D%97%E6%9C%BA%E7%B3%BB%E5%88%97%E6%95%B4%E6%9C%BA%E7%94%9F%E4%BA%A7%E6%B5%8B%E8%AF%95%E6%8C%87%E5%AF%BC.pdf" });//CE-6000n 模块机系列整机生产测试指导.pdf
            return data;
        }
    }
}
