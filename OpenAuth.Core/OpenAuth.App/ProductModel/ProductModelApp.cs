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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
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
            return productModelSelections.Select(zw => zw.Voltage).Distinct().OrderBy(zw => zw).ToList();
        }
        /// <summary>
        /// 获取电压等级
        /// </summary>
        /// <returns></returns>
        public List<string> GetCurrentList()
        {
            var productModelSelections = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete);
            return productModelSelections.Select(zw => zw.Current).Distinct().OrderBy(zw => zw).ToList();
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
            return productModelSelections.Select(zw => zw.TotalPower).Distinct().OrderBy(zw => zw).ToList();
        }
        /// <summary>
        /// 查询
        /// </summary>
        /// <returns></returns>
        public List<ProductModelInfo> GetProductModelGrid(ProductModelReq queryModel, out int rowcount)
        {
            Expression<Func<ProductModelSelection, bool>> exps = t => true;
            exps = exps.And(t => !t.IsDelete);
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
            var productModelSelectionList = UnitWork.Find(queryModel.page, queryModel.limit, "", exps);
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
            var productModelCategory = UnitWork.Find<ProductModelType>(u => !u.IsDelete && u.Id == ProductModelTypeId)?.FirstOrDefault();
            if (productModelCategory != null && !string.IsNullOrWhiteSpace(productModelCategory.ImageBanner))
            {
                foreach (var item in productModelCategory.ImageBanner.Replace("\r", "").Replace("\n", "").TrimEnd(',').Split(','))
                {
                    imgs.Add(host + item);
                }
                //imgs = JsonConvert.DeserializeObject<List<string>>(productModelCategory.ImageBanner);
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
        public string ExportProductSpecsDoc(int Id, string host, string Language)
        {
            var productModelSelection = UnitWork.Find<ProductModelSelection>(u => !u.IsDelete && u.Id == Id).FirstOrDefault();
            string pdfName = DateTime.Now.ToString("yyyyMMddHHmmssffffff")+ ".doc";

            if (productModelSelection != null)
            {
                var productModelCategory = UnitWork.Find<ProductModelCategory>(u => !u.IsDelete && u.Id == productModelSelection.ProductModelCategoryId).FirstOrDefault();
                var productModelSelectionInfo = UnitWork.Find<ProductModelSelectionInfo>(u => !u.IsDelete && u.ProductModelSelectionId == productModelSelection.Id).FirstOrDefault();
                var productModelDetails = GetSpecifications(Id);
                object templatePath = "";
                List<WordMarkModel> wordModels = new List<WordMarkModel>() {
                new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.DeviceCoding),
                 MarkType=0,
                 MarkValue=productModelDetails.DeviceCoding
                },
                   new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.ChannelNumber),
                 MarkType=0,
                 MarkValue=productModelDetails.ChannelNumber
                },
                          new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.InputPowerType),
                 MarkType=0,
                 MarkValue=productModelDetails.InputPowerType
                },       new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.InputActivePower),
                 MarkType=0,
                 MarkValue=productModelDetails.InputActivePower
                },       new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.InputCurrent),
                 MarkType=0,
                 MarkValue=productModelDetails.InputCurrent
                },       new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.Efficiency),
                 MarkType=0,
                 MarkValue=productModelDetails.Efficiency
                },       new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.Noise),
                 MarkType=0,
                 MarkValue=productModelDetails.Noise
                },       new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.DeviceType),
                 MarkType=0,
                 MarkValue=productModelDetails.DeviceType
                },       new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.PowerControlModuleType),
                 MarkType=0,
                 MarkValue=productModelDetails.PowerControlModuleType
                },       new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.PowerConnection),
                 MarkType=0,
                 MarkValue=productModelDetails.PowerConnection
                },
                           new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.ChargeVoltageRange),
                 MarkType=0,
                 MarkValue=productModelDetails.ChargeVoltageRange
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.DischargeVoltageRange),
                 MarkType=0,
                 MarkValue=productModelDetails.DischargeVoltageRange
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.MinimumDischargeVoltage),
                 MarkType=0,
                 MarkValue=productModelDetails.MinimumDischargeVoltage
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.CurrentRange),
                 MarkType=0,
                 MarkValue=productModelDetails.CurrentRange
                },
                            new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.CurrentAccurack),
                 MarkType=0,
                 MarkValue=productModelDetails.CurrentAccurack
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.CutOffCurrent),
                 MarkType=0,
                 MarkValue=productModelDetails.CutOffCurrent
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.SinglePower),
                 MarkType=0,
                 MarkValue=productModelDetails.SinglePower
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.CurrentResponseTime),
                 MarkType=0,
                 MarkValue=productModelDetails.CurrentResponseTime
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.CurrentConversionTime),
                 MarkType=0,
                 MarkValue=productModelDetails.CurrentConversionTime
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.RecordFreq),
                 MarkType=0,
                 MarkValue=productModelDetails.RecordFreq
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.MinimumVoltageInterval),
                 MarkType=0,
                 MarkValue=productModelDetails.MinimumVoltageInterval
                },
                             new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.MinimumCurrentInterval),
                 MarkType=0,
                 MarkValue=productModelDetails.MinimumCurrentInterval
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.TotalPower),
                 MarkType=0,
                 MarkValue=productModelDetails.TotalPower
                }, new WordMarkModel(){
                 MarkName=nameof(ProductModelDetails.Size),
                 MarkType=0,
                 MarkValue=productModelDetails.Size
                },
                };
                if (Language == "CN")
                {
                    templatePath = host + productModelCategory.SpecsDocTemplatePath_CH;
                }
                if (Language == "EN")
                {
                    templatePath = host + productModelCategory.SpecsDocTemplatePath_EN;

                }
                string filePath = "/Templates/";
                filePath = "";
                //object[] oBookMark = new object[20];
                //oBookMark[0] = "DeviceCoding";
                object[] oBookMark = wordModels.Select(zw => (object)zw.MarkName).Distinct().ToArray();
                FileHelper.DOCTemplateConvert(templatePath, filePath + pdfName, wordModels, oBookMark);
            }
            return host + "\\Templates\\" + pdfName;

        }
        /// <summary>
        /// 产品规格
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public ProductModelDetails GetSpecifications(int Id)

        {
            var result = new ProductModelDetails();
            var productmodelselection = UnitWork.FindSingle<ProductModelSelection>(q => q.Id == Id);
            var productmodeltype = UnitWork.FindSingle<ProductModelType>(q => q.Id == productmodelselection.ProductModelCategoryId);
            var productmodelselectioninfo = UnitWork.FindSingle<ProductModelSelectionInfo>(q => q.ProductModelSelectionId == productmodelselection.Id);
            result.DeviceCoding = productmodelselection.DeviceCoding;
            result.ChannelNumber = productmodelselection.ChannelNumber;
            result.InputPowerType = productmodelselectioninfo.InputPowerType;
            result.InputActivePower = productmodelselectioninfo.InputActivePower;
            result.InputCurrent = productmodelselectioninfo.InputCurrent;
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
            result.ChargeVoltageRange = "充电：0" + "V~" + productmodelselection.Voltage + "V";
            result.DischargeVoltageRange = "放电：" + productmodelselectioninfo.MinimumDischargeVoltage + "~" + productmodelselection.Voltage + "V";
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
            result.SinglePower = Temp.ToString() + "KW";
            result.RecordFreq = productmodelselectioninfo.Fre;
            if (productmodelselectioninfo.Fre == "100HZ")
            {
                result.RecordFreq = productmodelselectioninfo.Fre + "(接入辅助通道为10HZ)";

            }
            result.MinimumVoltageInterval = "最小电压间隔: " + Temp + "V";
            result.MinimumCurrentInterval = "最小电流间隔: " + Temp + "A";//Minimum current interval: 0.1A
            result.TotalPower = productmodelselection.TotalPower;
            result.Size = productmodelselection.Size;
            return result;
        }

    }
}
