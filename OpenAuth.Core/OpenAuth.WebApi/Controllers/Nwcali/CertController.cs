using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Excel;
using Infrastructure.Export;
using Infrastructure.Wrod;
using Microsoft.AspNetCore.Mvc;
using NetOffice.Extensions.Conversion;
using OpenAuth.App;
using OpenAuth.App.Nwcali.Models;
using OpenAuth.App.Request;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 校准证书
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CertController : Controller
    {
        private readonly CertinfoApp _certinfoApp;
        private readonly CertPlcApp _certPlcApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly FlowInstanceApp _flowInstanceApp;
        private static readonly string BaseCertDir = Path.Combine(Directory.GetCurrentDirectory(), "certs");
        private static readonly Dictionary<int, double> PoorCoefficients = new Dictionary<int, double>()
        {
            { 2, 1.13 },
            { 3, 1.69 },
            { 4, 2.06 },
            { 5, 2.33 }
        };

        private static readonly Dictionary<string, string> nameDic = new Dictionary<string, string>()
            {
                { "肖淑惠","xiao.png" },
                { "覃金英","tan.png" },
                { "周定坤","zhou.png" },
                { "杨浩杰","yang.png" },
                { "陈大为","chen.png" },
            };

        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

        public CertController(CertinfoApp certinfoApp, CertPlcApp certPlcApp, ModuleFlowSchemeApp moduleFlowSchemeApp, FlowInstanceApp flowInstanceApp)
        {
            _certinfoApp = certinfoApp;
            _certPlcApp = certPlcApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _flowInstanceApp = flowInstanceApp;
        }

        [HttpPost]
        public async Task<IActionResult> Generate()
        {
            var file = Request.Form.Files[0];
            var handler = new ExcelHandler(file.OpenReadStream());
            var baseInfo = handler.GetBaseInfo();
            //if (string.IsNullOrWhiteSpace(baseInfo.Operator))
            //{
            //    return new Response<bool>()
            //    {
            //        Code = 400,
            //        Message = "Operator can not be null.",
            //        Result = false
            //    };
            //}
            var turV = handler.GetNwcaliTur("电压");
            var turA = handler.GetNwcaliTur("电流");
            try
            {
                //用信号量代替锁
                //await semaphoreSlim.WaitAsync();
                //try
                //{
                //    baseInfo.CertificateNumber = await CertificateNoGenerate("O");
                //    await _certinfoApp.AddAsync(new AddOrUpdateCertinfoReq()
                //    {
                //        CertNo = baseInfo.CertificateNumber,
                //        AssetNo = baseInfo.AssetNo,
                //        Sn = baseInfo.TesterSn,
                //        Model = baseInfo.TesterModel,
                //        CalibrationDate = DateTime.Parse(baseInfo.Time),
                //        ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time, baseInfo.TestInterval)),
                //        Operator = baseInfo.Operator
                //    });
                //}
                //finally
                //{
                //    semaphoreSlim.Release();
                //}
                handler.SetValue(baseInfo.CertificateNumber, 15, 1);
                DirUtil.CheckOrCreateDir(Path.Combine(BaseCertDir, baseInfo.CertificateNumber));
                var baseInfoTagetPath = Path.Combine(BaseCertDir, baseInfo.CertificateNumber, $"BaseInfo{baseInfo.CertificateNumber}.xls");
                handler.Save(baseInfoTagetPath);
                var plcDataDic = new Dictionary<string, List<NwcaliPLCData>>();
                var plcRepetitiveMeasurementDataDic = new Dictionary<string, List<NwcaliPLCRepetitiveMeasurementData>>();

                foreach (var plc in baseInfo.PCPLCs)
                {
                    var list = handler.GetNWCaliPLCData($"下位机{plc.No}");
                    plcDataDic.Add($"下位机{plc.No}", list);
                    var list2 = handler.GetNWCaliPLCRepetitiveMeasurementData($"下位机{plc.No}重复性测量");
                    if (list2.Count > 0)
                        plcRepetitiveMeasurementDataDic.Add($"下位机{plc.No}重复性测量", list2);
                    await _certPlcApp.AddAsync(new AddOrUpdateCertPlcReq
                    {
                        CertNo = baseInfo.CertificateNumber,
                        PlcGuid = plc.Guid,
                        CalibrationDate = DateTime.Parse(baseInfo.Time),
                        ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time, baseInfo.TestInterval))
                    });
                }
                var modelList = await BuildWordModels(baseInfo, plcDataDic, plcRepetitiveMeasurementDataDic, turV, turA);
                var datas = await ExportAllHandler.Exporterpdf(modelList, "Calibration Certificate.cshtml");

                return File(datas, "application/pdf");
                //var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Calibration Certificate(word).docx");
                //var tagetPath = Path.Combine(BaseCertDir, baseInfo.CertificateNumber, $"Cert{baseInfo.CertificateNumber}.docx");
                //var result = WordHandler.DOCTemplateConvert(templatePath, tagetPath, modelList);
                //if (result)
                //{
                //var flowInstanceId = await CreateFlow(baseInfo.CertificateNumber);
                //var c = await _certinfoApp.GetAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                //c.CertPath = tagetPath;
                //c.BaseInfoPath = baseInfoTagetPath;
                //c.FlowInstanceId = flowInstanceId;
                //var obj = c.MapTo<AddOrUpdateCertinfoReq>();
                //await _certinfoApp.UpdateAsync(obj);
                //}
                //else
                //{
                //await _certinfoApp.DeleteAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                //await _certPlcApp.DeleteAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                //}
                //return new Response<bool>()
                //{
                //    Result = true
                //};
            }
            catch (Exception ex)
            {
                //await _certinfoApp.DeleteAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                //await _certPlcApp.DeleteAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                //throw ex;
                //return new Response<bool>()
                //{
                //    Code = 500,
                //    Message = ex.Message,
                //    Result = false
                //};
                return BadRequest();
            }
        }


        [HttpGet("{certNo}")]
        public async Task<IActionResult> DownloadBaseInfo(string certNo)
        {
            var cert = await _certinfoApp.GetAsync(c => c.CertNo.Equals(certNo));
            if (cert is null)
                return new NotFoundResult();
            var fileStream = new FileStream(cert.BaseInfoPath, FileMode.Open);
            return File(fileStream, "application/vnd.ms-excel");
        }

        [HttpGet("{certNo}")]
        public async Task<IActionResult> DownloadCert(string certNo)
        {
            var cert = await _certinfoApp.GetAsync(c => c.CertNo.Equals(certNo));
            if (cert is null)
                return new NotFoundResult();
            var fileStream = new FileStream(cert.CertPath, FileMode.Open);
            return File(fileStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        }
        [HttpGet("{certNo}")]
        public async Task<IActionResult> DownloadCertPdf(string certNo)
        {
            var cert = await _certinfoApp.GetAsync(c => c.CertNo.Equals(certNo));
            if (cert is null)
                return new NotFoundResult();
            if (!string.IsNullOrWhiteSpace(cert.PdfPath) && System.IO.File.Exists(cert.PdfPath))
            {
                
                var fileStream = new FileStream(cert.PdfPath, FileMode.Open);
                return File(fileStream, "application/pdf");
            }
            var pdfPath = WordHandler.DocConvertToPdf(cert.CertPath);
            if (!pdfPath.Equals("false"))
            {
                cert.PdfPath = pdfPath;
                await _certinfoApp.UpdateAsync(cert.MapTo<AddOrUpdateCertinfoReq>());
                var fileStream = new FileStream(pdfPath, FileMode.Open);
                return File(fileStream, "application/pdf");
            }
            return new NotFoundResult();
        }
        [HttpGet("{plcGuid}")]
        public async Task<IActionResult> GetCertNoList(string plcGuid)
        {
            var certNos = (await _certPlcApp.GetAllAsync(p => p.PlcGuid.Equals(plcGuid))).OrderByDescending(c => c.CertNo).Select(cp => new { cp.CertNo, cp.CalibrationDate, cp.ExpirationDate });
            return Ok(certNos);
        }
        /// <summary>
        /// 构建证书模板参数
        /// </summary>
        /// <param name="baseInfo">基础信息</param>
        /// <param name="plcData">下位机数据</param>
        /// <param name="plcRepetitiveMeasurementData">下位机重复性测量数据</param>
        /// <param name="turV">Tur电压数据</param>
        /// <param name="turA">Tur电流数据</param>
        /// <returns></returns>
        private static async Task<CertModel> BuildWordModels(NwcaliBaseInfo baseInfo, Dictionary<string, List<NwcaliPLCData>> plcData, Dictionary<string, List<NwcaliPLCRepetitiveMeasurementData>> plcRepetitiveMeasurementData, List<NwcaliTur> turV, List<NwcaliTur> turA)
        {
            var list = new List<WordModel>();
            var model = new CertModel();
            #region 页眉
            baseInfo.CertificateNumber = "NWO20B17001";
            var barcode = await BarcodeGenerate(baseInfo.CertificateNumber);
            model.BarCode = barcode;
            #endregion
            #region Calibration Certificate
            model.CalibrationCertificate.CertificatenNumber = baseInfo.CertificateNumber;
            model.CalibrationCertificate.TesterMake = baseInfo.TesterMake;
            model.CalibrationCertificate.CalibrationDate = DateStringConverter(baseInfo.Time);
            model.CalibrationCertificate.TesterModel = baseInfo.TesterModel;
            model.CalibrationCertificate.CalibrationDue = ConvertTestInterval(baseInfo.Time, baseInfo.TestInterval);
            model.CalibrationCertificate.TesterSn = baseInfo.TesterSn;
            model.CalibrationCertificate.DataType = "";
            model.CalibrationCertificate.AssetNo = baseInfo.AssetNo == "0" ? "------" : baseInfo.AssetNo;
            model.CalibrationCertificate.SiteCode = baseInfo.SiteCode;
            model.CalibrationCertificate.Temperature = baseInfo.Temperature;
            model.CalibrationCertificate.RelativeHumidity = baseInfo.RelativeHumidity;
            #endregion
            #region Main Standards Used
            for (int i = 0; i < baseInfo.Etalons.Count; i++)
            {
                model.MainStandardsUsed.Add(new MainStandardsUsed 
                { 
                    Name = baseInfo.Etalons[i].Name, 
                    Characterisics = baseInfo.Etalons[i].Characteristics, 
                    AssetNo = baseInfo.Etalons[i].AssetNo, 
                    CertificateNo = baseInfo.Etalons[i].CertificateNo, 
                    DueDate = DateStringConverter(baseInfo.Etalons[i].DueDate) 
                });
            }
            #endregion

            #region Uncertainty Budget

            var plc = plcData.First().Value;
            var plcrmd = plcRepetitiveMeasurementData.First().Value;
            var v = plc.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals("Charge") && p.Verify_Type.Equals("Post-Calibration")).GroupBy(p=>p.Channel).First().ToList();
            var sv = v.Select(s => s.Commanded_Value).ToList();
            sv.Sort();
            var vscale = sv[(sv.Count - 1) / 2];


            var c = plc.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals("Charge") && p.Verify_Type.Equals("Post-Calibration")).OrderByDescending(a=>a.Scale).GroupBy(p => p.Scale).First().ToList();
            var cv = c.Select(c => c.Commanded_Value).ToList();
            cv.Sort();
            var cscale = cv[(cv.Count - 1) / 2];
            #region T.U.R. Table
            //电压
            var vPoint = turV.Select(v => v.TestPoint).Distinct().ToList();
            var vPointIndex = (vPoint.Count - 1) / 2;
            var vSpec = v.First().Scale * baseInfo.RatedAccuracyV * 1000;
            var u95_1 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex - 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_2 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_3 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex + 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var tur_1 = (2 * vSpec / 1000) / (2 * u95_1);
            var tur_2 = (2 * vSpec / 1000) / (2 * u95_2);
            var tur_3 = (2 * vSpec / 1000) / (2 * u95_3);
            model.TurTables.Add(new TurTable { Number = "1", Point = $"{vPoint[vPointIndex - 1]}V", Spec = $"±{vSpec}mV", U95Standard = u95_1.ToString("e3"), TUR = tur_1.ToString("f2") });
            model.TurTables.Add(new TurTable { Number = "2", Point = $"{vPoint[vPointIndex]}V", Spec = $"±{vSpec}mV", U95Standard = u95_2.ToString("e3"), TUR = tur_2.ToString("f2") });
            model.TurTables.Add(new TurTable { Number = "3", Point = $"{vPoint[vPointIndex + 1]}V", Spec = $"±{vSpec}mV", U95Standard = u95_3.ToString("e3"), TUR = tur_3.ToString("f2") });
            //电流
            var cPoint = turA.Select(v => v.TestPoint).Distinct().ToList();
            var cPointIndex = cPoint.IndexOf(cscale / 1000); //(cPoint.Count - 1) / 2;
            var cSpec = c.First().Scale * baseInfo.RatedAccuracyC;
            var u95_4 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_5 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_6 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var tur_4 = (2 * cSpec) / (2 * u95_4 * 1000);
            var tur_5 = (2 * cSpec) / (2 * u95_5 * 1000);
            var tur_6 = (2 * cSpec) / (2 * u95_6 * 1000);

            model.TurTables.Add(new TurTable { Number = "4", Point = $"{cPoint[cPointIndex - 1]}V", Spec = $"±{cSpec}mV", U95Standard = u95_4.ToString("e3"), TUR = tur_4.ToString("f2") });
            model.TurTables.Add(new TurTable { Number = "5", Point = $"{cPoint[cPointIndex]}V", Spec = $"±{cSpec}mV", U95Standard = u95_5.ToString("e3"), TUR = tur_5.ToString("f2") });
            model.TurTables.Add(new TurTable { Number = "6", Point = $"{cPoint[cPointIndex + 1]}V", Spec = $"±{cSpec}mV", U95Standard = u95_6.ToString("e3"), TUR = tur_6.ToString("f2") });

            #endregion

            #region Uncertainty Budget Table
            #region Voltage
            var vv = 2 * (double)v.FirstOrDefault().Scale / Math.Pow(2, baseInfo.VoltmeterBits);
            var vstd = 2 * (double)v.FirstOrDefault().Scale / (Math.Pow(2, baseInfo.VoltmeterBits) * Math.Sqrt(12));
            var voltageUncertaintyBudgetTable = new UncertaintyBudgetTable();
            voltageUncertaintyBudgetTable.Value = $"{vscale}V";
            voltageUncertaintyBudgetTable.TesterResolutionValue = vv.ToString("e3");
            voltageUncertaintyBudgetTable.TesterResolutionStdUncertainty = vstd.ToString("e3");
            var vmdcv = plcrmd.Where(d => d.Commanded_Value.Equals(vscale) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals("Charge") && d.Verify_Type.Equals("Post-Calibration")).GroupBy(a=>a.Channel).First().ToList();
            double vror;
            if (vmdcv.Count >= 6)//贝塞尔公式法
            {
                var vavg = vmdcv.Sum(c => c.Standard_Value) / vmdcv.Count;
                vror = Math.Sqrt(vmdcv.Select(c => Math.Pow(c.Standard_Value - vavg, 2)).Sum() / (vmdcv.Count - 1));
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingValue = vror.ToString("e3");
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = vror.ToString("e3");
            }
            else//极差法
            {
                var poorCoefficient = PoorCoefficients[vmdcv.Count];
                var vmdsv = vmdcv.Select(c => c.Standard_Value).ToList();
                var R = vmdsv.Max() - vmdsv.Min();
                var u2 = R / poorCoefficient;
                vror = u2;
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingValue = u2.ToString("e3");
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = u2.ToString("e3");
            }
            turV = turV.Where(v => v.TestPoint == vscale).ToList();
            var combinedUncertaintyV = Math.Sqrt(turV.Sum(v => Math.Pow(v.StdUncertainty, 2)) + Math.Pow(vstd, 2) + Math.Pow(vror, 2));
            voltageUncertaintyBudgetTable.CombinedUncertainty = combinedUncertaintyV.ToString("e3");
            voltageUncertaintyBudgetTable.CombinedUncertaintySignificance = "100.000%";
            voltageUncertaintyBudgetTable.CoverageFactor = baseInfo.K.ToString(); ;
            voltageUncertaintyBudgetTable.ExpandedUncertainty = (baseInfo.K * combinedUncertaintyV).ToString("e3");
            for (int i = 0; i < turV.Count; i++)
            {
                var data = new UncertaintyBudgetTable.UncertaintyBudgetTableData();
                var tv = turV[i];
                data.UncertaintyContributors = tv.UncertaintyContributors;
                data.Value = tv.Value.ToString("e3");
                data.SensitivityCoefficient = tv.SensitivityCoefficient.ToString();
                data.Unit = tv.Unit;
                data.Type = tv.Type;
                data.Distribution = tv.Distribution;
                if (tv.UncertaintyContributors.Equals("Resolution"))
                {
                    data.CoverageFactor = Math.Sqrt(12).ToString("f3");
                }
                else
                {
                    data.CoverageFactor = tv.Divisor.ToString();
                }
                data.StdUncertainty = tv.StdUncertainty.ToString("e3");
                data.Significance = (Math.Pow(tv.StdUncertainty, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3");
                voltageUncertaintyBudgetTable.Datas.Add(data);
            }
            voltageUncertaintyBudgetTable.TesterResolutionSignificance = (Math.Pow(vstd, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3");
            voltageUncertaintyBudgetTable.RepeatabilityOfReadingSignificance = (Math.Pow(vror, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3");
            model.VoltageUncertaintyBudgetTables = voltageUncertaintyBudgetTable;
            #endregion

            #region Current
            var currentUncertaintyBudgetTable = new UncertaintyBudgetTable();
            var cvv = 2 * (double)c.FirstOrDefault().Scale / 1000 / Math.Pow(2, baseInfo.AmmeterBits);
            var cstd = 2 * (double)c.FirstOrDefault().Scale / 1000 / (Math.Pow(2, baseInfo.AmmeterBits) * Math.Sqrt(12));
            list.Add(new WordModel { MarkPosition = 0, TableMark = 6, ValueType = 0, XCellMark = 1, YCellMark = 2, ValueData = $"{cscale}mA" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 4, YCellMark = 2, ValueData = cvv.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 4, YCellMark = 8, ValueData = cstd.ToString("e3") });
            currentUncertaintyBudgetTable.Value = $"{cscale}mA";
            currentUncertaintyBudgetTable.TesterResolutionValue = cvv.ToString("e3");
            currentUncertaintyBudgetTable.TesterResolutionStdUncertainty = cstd.ToString("e3");
            var cmdcv = plcrmd.Where(d => d.Commanded_Value.Equals(cscale) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals("Charge") && d.Verify_Type.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().ToList();
            double cror;
            if (cmdcv.Count >= 6)//贝塞尔公式法
            {
                var cavg = cmdcv.Sum(c => c.Standard_Value) / cmdcv.Count / 1000;
                cror = Math.Sqrt(cmdcv.Select(c => Math.Pow(c.Standard_Value / 1000 - cavg, 2)).Sum() / (cmdcv.Count - 1));
                currentUncertaintyBudgetTable.RepeatabilityOfReadingValue = cror.ToString("e3");
                currentUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = cror.ToString("e3");
            }
            else//极差法
            {
                var poorCoefficient = PoorCoefficients[cmdcv.Count];
                var cmdsv = cmdcv.Select(c => c.Standard_Value / 1000).ToList();
                var R = cmdsv.Max() - cmdsv.Min();
                var u2 = R / poorCoefficient;
                cror = u2;
                currentUncertaintyBudgetTable.RepeatabilityOfReadingValue = u2.ToString("e3");
                currentUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = u2.ToString("e3");
            }

            turA = turA.Where(a => a.TestPoint * 1000 == cscale).ToList();

            var combinedUncertaintyA = Math.Sqrt(turA.Sum(c => Math.Pow(c.StdUncertainty, 2)) + Math.Pow(cstd, 2) + Math.Pow(cror, 2));
            currentUncertaintyBudgetTable.CombinedUncertainty = combinedUncertaintyA.ToString("e3");
            currentUncertaintyBudgetTable.CombinedUncertaintySignificance = "100.000%";
            currentUncertaintyBudgetTable.CoverageFactor = baseInfo.K.ToString(); ;
            currentUncertaintyBudgetTable.ExpandedUncertainty = (baseInfo.K * combinedUncertaintyA).ToString("e3");
            for (int i = 0; i < turA.Count; i++)
            {
                var data = new UncertaintyBudgetTable.UncertaintyBudgetTableData();
                var ta = turA[i];

                data.UncertaintyContributors = ta.UncertaintyContributors;
                data.Value = ta.Value.ToString("e3");
                data.SensitivityCoefficient = ta.SensitivityCoefficient.ToString();
                data.Unit = ta.Unit;
                data.Type = ta.Type;
                data.Distribution = ta.Distribution;
                data.CoverageFactor = ta.Divisor.ToString();
                data.StdUncertainty = ta.StdUncertainty.ToString("e3");
                data.Significance = (Math.Pow(ta.StdUncertainty, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3");
                currentUncertaintyBudgetTable.Datas.Add(data);
            }
            currentUncertaintyBudgetTable.TesterResolutionSignificance = (Math.Pow(cstd, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3");
            currentUncertaintyBudgetTable.RepeatabilityOfReadingSignificance = (Math.Pow(cror, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3");
            model.CurrentUncertaintyBudgetTables = voltageUncertaintyBudgetTable;
            #endregion

            #endregion
            #endregion

            #region Data Sheet
            void CalculateVoltage(string mode, int tableIndex)
            {
                int j = 0;
                foreach (var item in plcData)
                {
                    int l = 1;
                    var data = item.Value.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals(mode) && p.Verify_Type.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.Commanded_Value).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            var cvCHH = $"{l}-{cvData.Channel}";
                            var cvRange = cvData.Scale;
                            var cvIndication = cvData.Measured_Value;
                            var cvMeasuredValue = cvData.Standard_Value;
                            var cvError = (cvIndication - cvMeasuredValue) * 1000;
                            double cvAcceptance = 0;
                            var cvAcceptanceStr = "";

                            var mdcv = plcrmd.Where(d => d.Commanded_Value.Equals(cvData.Commanded_Value) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals("Charge") && d.Verify_Type.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().ToList();
                            double ror;
                            if (baseInfo.RepetitiveMeasurementsCount >= 6)//贝塞尔公式法
                            {
                                var vavg = mdcv.Sum(c => c.Standard_Value) / mdcv.Count;
                                ror = Math.Sqrt(mdcv.Select(c => Math.Pow(c.Standard_Value - vavg, 2)).Sum() / (mdcv.Count - 1));
                            }
                            else//极差法
                            {
                                var poorCoefficient = PoorCoefficients[mdcv.Count];
                                var mdsv = mdcv.Select(c => c.Standard_Value).ToList();
                                var R = mdsv.Max() - mdsv.Min();
                                var u2 = R / poorCoefficient;
                                ror = u2;
                            }
                            //计算不确定度
                            var cvUncertaintyStr = (baseInfo.K * 1000 * Math.Sqrt(Math.Pow(cvData.Standard_total_U / 2, 2) + Math.Pow(vstd, 2) + Math.Pow(ror, 2))).ToString("G2");
                            var cvUncertainty = double.Parse(cvUncertaintyStr);
                            var T = double.Parse((cvData.Scale * baseInfo.RatedAccuracyV * 1000).ToString("G2"));
                            var cvConclustion = "";
                            //计算接受限
                            if (baseInfo.AcceptedTolerance.Equals("0"))
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyV * 1000;
                                cvAcceptance = accpetedTolerance;
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("1"))
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyV * 1000 - cvUncertainty;
                                cvAcceptance = accpetedTolerance;
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("M2%"))
                            {
                                var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale * baseInfo.RatedAccuracyV * 2 / (2 * cvUncertainty / 1000)) - 0.54);
                                if (m2 < 0)
                                {
                                    var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyV * 1000;
                                    cvAcceptance = accpetedTolerance;
                                }
                                else
                                {
                                    var accpetedTolerance = (cvData.Scale * baseInfo.RatedAccuracyV * 1000 - cvUncertainty) * m2;
                                    cvAcceptance = accpetedTolerance;
                                }
                            }
                            //约分
                            var (IndicationReduce, MeasuredValueReduce, ErrorReduce, AcceptanceReduce, UncertaintyReduce) = ReduceVoltage(cvIndication, cvMeasuredValue, cvError, cvAcceptance, cvUncertainty);
                            cvAcceptanceStr = $"±{AcceptanceReduce}";
                            //计算判定结果
                            if (baseInfo.AcceptedTolerance.Equals("0"))
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    cvConclustion = "P";
                                }
                                else
                                {
                                    cvConclustion = "F";
                                }
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("1"))
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    cvConclustion = "P";
                                }
                                else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
                                {
                                    cvConclustion = "P*";
                                }
                                else
                                {
                                    cvConclustion = "F";
                                }
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("M2%"))
                            {
                                var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale * baseInfo.RatedAccuracyV * 2 / (2 * cvUncertainty / 1000)) - 0.54);
                                if (m2 < 0)
                                {
                                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                    {
                                        cvConclustion = "P";
                                    }
                                    else
                                    {
                                        cvConclustion = "F";
                                    }
                                }
                                else
                                {
                                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                    {
                                        cvConclustion = "P";
                                    }
                                    else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
                                    {
                                        cvConclustion = "P*";
                                    }
                                    else
                                    {
                                        cvConclustion = "F";
                                    }
                                }
                            }
                            if (mode.Equals("Charge"))
                            {
                                model.ChargingVoltage.Add(new DataSheet
                                { 
                                      Channel = cvCHH,
                                      Range = cvRange.ToString(),
                                      Indication = IndicationReduce,
                                      MeasuredValue = MeasuredValueReduce,
                                      Error = ErrorReduce,
                                      Acceptance = cvAcceptanceStr,
                                      Uncertainty = UncertaintyReduce,
                                      Conclusion = cvConclustion
                                });
                            }
                            else
                            {
                                model.DischargingVoltage.Add(new DataSheet
                                {
                                    Channel = cvCHH,
                                    Range = cvRange.ToString(),
                                    Indication = IndicationReduce,
                                    MeasuredValue = MeasuredValueReduce,
                                    Error = ErrorReduce,
                                    Acceptance = cvAcceptanceStr,
                                    Uncertainty = UncertaintyReduce,
                                    Conclusion = cvConclustion
                                });
                            }
                            j++;
                        }
                    }
                    l++;
                }
            }
            void CalculateCurrent(string mode, int tableIndex)
            {
                int j = 0;
                foreach (var item in plcData)
                {
                    int l = 1;
                    var data = item.Value.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals(mode) && p.Verify_Type.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.Scale).ThenBy(dd => dd.Commanded_Value).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            var CHH = $"{l}-{cvData.Channel}";
                            var Range = cvData.Scale;
                            var Indication = cvData.Measured_Value;
                            var MeasuredValue = cvData.Standard_Value;
                            var Error = Indication - MeasuredValue;
                            double Acceptance = 0;
                            var AcceptanceStr = "";

                            var mdcv = plcrmd.Where(d => d.Commanded_Value.Equals(cvData.Commanded_Value) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals("Charge") && d.Verify_Type.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().ToList();
                            double ror;
                            if (baseInfo.RepetitiveMeasurementsCount >= 6)//贝塞尔公式法
                            {
                                var avg = mdcv.Sum(c => c.Standard_Value) / mdcv.Count / 1000;
                                ror = Math.Sqrt(mdcv.Select(c => Math.Pow(c.Standard_Value / 1000 - avg, 2)).Sum() / (mdcv.Count - 1));
                            }
                            else//极差法
                            {
                                var poorCoefficient = PoorCoefficients[mdcv.Count];
                                var mdsv = mdcv.Select(c => c.Standard_Value / 1000).ToList();
                                var R = mdsv.Max() - mdsv.Min();
                                var u2 = R / poorCoefficient;
                                ror = u2;
                            }
                            //计算不确定度
                            var UncertaintyStr = (baseInfo.K * 1000 * Math.Sqrt(Math.Pow(cvData.Standard_total_U / 2, 2) + Math.Pow(cstd, 2) + Math.Pow(ror, 2))).ToString();
                            var Uncertainty = double.Parse(UncertaintyStr);
                            var T = double.Parse((cvData.Scale * baseInfo.RatedAccuracyC).ToString("G2"));
                            var Conclustion = "";
                            //计算接受限
                            if (baseInfo.AcceptedTolerance.Equals("0"))
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC;
                                Acceptance = accpetedTolerance;
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("1"))
                            {
                                var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC - Uncertainty;
                                Acceptance = accpetedTolerance;
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("M2%"))
                            {
                                var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale * baseInfo.RatedAccuracyC * 2 / (2 * Uncertainty / 1000)) - 0.54);
                                if (m2 < 0)
                                {
                                    var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC;
                                    Acceptance = accpetedTolerance;
                                }
                                else
                                {
                                    var accpetedTolerance = (cvData.Scale * baseInfo.RatedAccuracyC - Uncertainty) * m2;
                                    Acceptance = accpetedTolerance;
                                }
                            }
                            ///约分
                            var (IndicationReduce, MeasuredValueReduce, ErrorReduce, AcceptanceReduce, UncertaintyReduce) = ReduceCurrent(Math.Abs(Indication), Math.Abs(MeasuredValue), Error, Acceptance, Uncertainty);
                            AcceptanceStr = $"±{AcceptanceReduce}";
                            //计算判定结果
                            if (baseInfo.AcceptedTolerance.Equals("0"))
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    Conclustion = "P";
                                }
                                else
                                {
                                    Conclustion = "F";
                                }
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("1"))
                            {
                                if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                {
                                    Conclustion = "P";
                                }
                                else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
                                {
                                    Conclustion = "P*";
                                }
                                else
                                {
                                    Conclustion = "F";
                                }
                            }
                            else if (baseInfo.AcceptedTolerance.Equals("M2%"))
                            {
                                var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale / 1000 * baseInfo.RatedAccuracyC * 2 / (2 * Uncertainty / 1000)) - 0.54);
                                if (m2 < 0)
                                {
                                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                    {
                                        Conclustion = "P";
                                    }
                                    else
                                    {
                                        Conclustion = "F";
                                    }
                                }
                                else
                                {
                                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
                                    {
                                        Conclustion = "P";
                                    }
                                    else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
                                    {
                                        Conclustion = "P*";
                                    }
                                    else
                                    {
                                        Conclustion = "F";
                                    }
                                }
                            }

                            if (mode.Equals("Charge"))
                            {
                                model.ChargingCurrent.Add(new DataSheet
                                {
                                    Channel = CHH,
                                    Range = ((double)Range / 1000).ToString(),
                                    Indication = IndicationReduce,
                                    MeasuredValue = MeasuredValueReduce,
                                    Error = ErrorReduce,
                                    Acceptance = AcceptanceStr,
                                    Uncertainty = UncertaintyReduce,
                                    Conclusion = Conclustion
                                });
                            }
                            else
                            {
                                model.DischargingCurrent.Add(new DataSheet
                                {
                                    Channel = CHH,
                                    Range = ((double)Range / 1000).ToString(),
                                    Indication = IndicationReduce,
                                    MeasuredValue = MeasuredValueReduce,
                                    Error = ErrorReduce,
                                    Acceptance = AcceptanceStr,
                                    Uncertainty = UncertaintyReduce,
                                    Conclusion = Conclustion
                                });
                            }
                            j++;
                        }
                    }
                    l++;
                }
            }
            #region Charging Voltage
            CalculateVoltage("Charge", 8);
            #endregion

            #region Discharging Voltage
            CalculateVoltage("DisCharge", 9);
            #endregion

            #region Charging Current
            CalculateCurrent("Charge", 10);
            #endregion

            #region Discharging Current
            CalculateCurrent("DisCharge", 11);
            #endregion

            #endregion

            #region 签名
            //var signPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "yang.png");
            //var signPath2 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "zhou.png");
            //var signPath3 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "chen.png");
            //list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 1, ValueData = signPath1 });
            //list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 3, ValueData = signPath2 });
            //list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 3, YCellMark = 1, ValueData = signPath3 });
            //var signetPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "印章.png");
            //list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 3, YCellMark = 3, ValueData = signetPath });
            //var signer = nameDic.GetValueOrDefault(baseInfo.Operator);
            //if(signer != null)
            //{
            //    var signPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", nameDic.GetValueOrDefault(baseInfo.Operator));
            //    list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 1, ValueData = signPath1 });
            //}
            #endregion
            return model;
        }

        /// <summary>
        /// 将日期转成英文格式
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string DateStringConverter(string date)
        {
            return DateTime.Parse(date).ToString("MMM-dd-yyyy", new System.Globalization.CultureInfo("en-us"));
        }

        /// <summary>
        /// 计算复校间隔时间
        /// </summary>
        /// <param name="date"></param>
        /// <param name="testInterval"></param>
        /// <returns></returns>
        private static string ConvertTestInterval(string date, string testInterval)
        {
            var y = testInterval.Substring(testInterval.Length - 1, 1);
            if (y.Equals("年"))
            {
                var years = Convert.ToInt32(testInterval[0..^1]);
                var dateTime = DateTime.Parse(date).AddYears(years).AddDays(-1).ToString();
                return DateStringConverter(dateTime);
            }
            if (y.Equals("月"))
            {
                var months = Convert.ToInt32(testInterval[0..^1]);
                var dateTime = DateTime.Parse(date).AddMonths(months).AddDays(-1).ToString();
                return DateStringConverter(dateTime);
            }
            return string.Empty;
        }

        /// <summary>
        /// 生成证书编号
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private async Task<string> CertificateNoGenerate(string type)
        {
            var now = DateTime.Now;
            var year = now.Year.ToString().Substring(3, 1);
            var month = now.Month.ToString();
            month = month == "10" ? "A" : month == "11" ? "B" : month == "12" ? "C" : month;
            var nos = await _certinfoApp.GetAllAsync(c => c.CertNo.Contains($"NW{type}{year}{month}"));
            if (nos.Count == 0)
            {
                return $"NW{type}{year}{month}0001";
            }
            else
            {
                var no = nos.Max(c => c.CertNo);
                var number = Convert.ToInt32(no.Substring(5, 4));
                number++;
                var numberStr = number.ToString().PadLeft(4, '0');
                return $"NW{type}{year}{month}{numberStr}";
            }
        }

        /// <summary>
        /// 生成证书一维码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private static async Task<string> BarcodeGenerate(string data)
        {
            System.Drawing.Font labelFont = new System.Drawing.Font("OCRB", 11f, FontStyle.Bold);
            BarcodeLib.Barcode b = new BarcodeLib.Barcode
            {
                IncludeLabel = true,
                LabelFont = labelFont
            };
            Image img = b.Encode(BarcodeLib.TYPE.CODE128, data, Color.Black, Color.White, 180, 49);

            DirUtil.CheckOrCreateDir(Path.Combine(BaseCertDir, data));
            var stream = new MemoryStream();
            
            img.Save(stream, ImageFormat.Png);
            var bytes = new byte[stream.Length];
            stream.Position = 0;
            await stream.ReadAsync(bytes, 0, bytes.Length);
            var base64str = Convert.ToBase64String(bytes);
            return base64str;
        }

        /// <summary>
        /// 电压单位约分
        /// </summary>
        /// <param name="indication"></param>
        /// <param name="measuredValue"></param>
        /// <param name="error"></param>
        /// <param name="acceptance"></param>
        /// <param name="uncertainty"></param>
        /// <returns></returns>
        private static (string, string, string, string, string) ReduceVoltage(double indication, double measuredValue, double error, double acceptance, double uncertainty)
        {
            var istr = indication.ToString("f6").Split('.')[1]; 
            var spMstr = measuredValue.ToString().Split('.');
            string mstr;
            if (spMstr.Count() == 1)
            {
                mstr = "00";
            }
            else
            {
                mstr = measuredValue.ToString().Split('.')[1];
            }
            var sp = uncertainty.ToString("G2").Split('.');
            if (sp[0] == "1" || sp[0] == "2")
                sp = (uncertainty / 1000).ToString("f4").Split('.');
            else
                sp = double.Parse((uncertainty / 1000).ToString("G2")).ToString("0.##########").Split('.');
            var ustr = sp[1];
            int j;
            if (istr.Length >= mstr.Length)
            {
                j = mstr.Length;
                if (ustr.Length < mstr.Length)
                {
                    j = ustr.Length;
                }
            }
            else
            {
                j = istr.Length;
                if (ustr.Length < istr.Length)
                {
                    j = ustr.Length;
                }
            }
            var indicationStr = indication.ToString($"f{j}");
            var measuredValueStr = measuredValue.ToString($"f{j}");
            var errorStr = (Convert.ToDouble(indicationStr) * 1000 - Convert.ToDouble(measuredValueStr) * 1000).ToString($"f{j - 3}") == "f-1" ? error.ToString("0.00") : (Convert.ToDouble(indicationStr) * 1000 - Convert.ToDouble(measuredValueStr) * 1000).ToString($"f{j - 3}");//error.ToString($"f{j - 3}");
            var acceptanceStr = acceptance.ToString($"f{j - 3}") == "f-1" ? acceptance.ToString("0.##########") : acceptance.ToString($"f{j - 3}");
            var uncertaintyStr = uncertainty.ToString($"f{j - 3}") == "f-1" ? uncertainty.ToString("0.##########") : uncertainty.ToString($"f{j - 3}"); ;
            return (indicationStr, measuredValueStr, errorStr, acceptanceStr, uncertaintyStr);
        }
        /// <summary>
        /// 电流单位约分
        /// </summary>
        /// <param name="indication"></param>
        /// <param name="measuredValue"></param>
        /// <param name="error"></param>
        /// <param name="acceptance"></param>
        /// <param name="uncertainty"></param>
        /// <returns></returns>
        private static (string, string, string, string, string) ReduceCurrent(double indication, double measuredValue, double error, double acceptance, double uncertainty)
        {
            var istr = indication.ToString("f3").Split('.')[1];
            istr = (indication / 1000).ToString($"f{istr.Length + 3}").Split('.')[1];
            var spMstr = measuredValue.ToString().Split('.');
            string mstr;
            if (spMstr.Count() == 1)
            {
                mstr = "00";
            }
            else
            {
                mstr = measuredValue.ToString().Split('.')[1];
            }
            mstr = measuredValue.ToString($"f{mstr.Length + 3}").Split('.')[1];
            var sp = ((decimal)uncertainty).ToString("G2").Split('.');
            if (sp[0] == "1" || sp[0] == "2")
                sp = (uncertainty / 1000).ToString("f4").Split('.');
            else
                sp = double.Parse((uncertainty / 1000).ToString("G2")).ToString("0.##########").Split('.');
            var ustr = sp[1];
            int j;
            if (istr.Length >= mstr.Length)
            {
                j = mstr.Length;
                if (ustr.Length < mstr.Length)
                {
                    j = ustr.Length;
                }
            }
            else
            {
                j = istr.Length;
                if (ustr.Length < istr.Length)
                {
                    j = ustr.Length;
                }
            }
            var indicationStr = (indication / 1000).ToString($"f{j}");
            var measuredValueStr = (measuredValue / 1000).ToString($"f{j}");
            var errorStr = (Convert.ToDouble(indicationStr) * 1000 - Convert.ToDouble(measuredValueStr) * 1000).ToString($"f{j - 3}") == "f-1" ? error.ToString("0.00") : (Convert.ToDouble(indicationStr) * 1000 - Convert.ToDouble(measuredValueStr) * 1000).ToString($"f{j - 3}");//error.ToString($"f{j - 3}");
            var acceptanceStr = acceptance.ToString($"f{j - 3}") == "f-1" ? acceptance.ToString("0.##########") : acceptance.ToString($"f{j - 3}");
            var uncertaintyStr = uncertainty.ToString($"f{j - 3}") == "f-1" ? uncertainty.ToString("0.##########") : uncertainty.ToString($"f{j - 3}"); ;
            return (indicationStr, measuredValueStr, errorStr, acceptanceStr, uncertaintyStr);
        }

        /// <summary>
        /// 创建流程
        /// </summary>
        /// <param name="certNo">证书编号</param>
        /// <returns></returns>
        private async Task<string> CreateFlow(string certNo)
        {
            try
            {
                var mf = await _moduleFlowSchemeApp.GetAsync(m => m.Module.Name.Equals("校准证书"));
                var req = new AddFlowInstanceReq();
                req.SchemeId = mf.FlowSchemeId;
                req.FrmType = 2;
                req.Code = DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString();
                req.CustomName = $"校准证书{certNo}审批";
                req.FrmData = $"{{\"certNo\":\"{certNo}\",\"cert\":[{{\"key\":\"{DatetimeUtil.ToUnixTimestampByMilliseconds(DateTime.Now).ToString()}\",\"url\":\"/Cert/DownloadCertPdf/{certNo}\",\"percent\":100,\"status\":\"success\",\"isImg\":false}}]}}";
                return await _flowInstanceApp.CreateInstanceAndGetIdAsync(req);
            }catch(Exception ex)
            {
                return "";
            }
        }

    }
}
