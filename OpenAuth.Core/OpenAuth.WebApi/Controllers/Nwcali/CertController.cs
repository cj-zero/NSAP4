using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DinkToPdf;
using Infrastructure;
using Infrastructure.Excel;
using Infrastructure.Export;
using Infrastructure.Wrod;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Nwcali;
using OpenAuth.App.Nwcali.Models;
using OpenAuth.App.Request;
using OpenAuth.Repository.Domain;

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
        private readonly NwcaliCertApp _nwcaliCertApp;
        private readonly UserSignApp _userSignApp;
        private readonly FileApp _fileApp;
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

        public CertController(CertinfoApp certinfoApp, CertPlcApp certPlcApp, ModuleFlowSchemeApp moduleFlowSchemeApp, FlowInstanceApp flowInstanceApp, NwcaliCertApp nwcaliCertApp, UserSignApp userSignApp, FileApp fileApp)
        {
            _certinfoApp = certinfoApp;
            _certPlcApp = certPlcApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _flowInstanceApp = flowInstanceApp;
            _nwcaliCertApp = nwcaliCertApp;
            _userSignApp = userSignApp;
            _fileApp = fileApp;
        }

        [HttpPost]
        public async Task<Response<bool>> Generate()
        {
            var file = Request.Form.Files[0];
            var handler = new ExcelHandler(file.OpenReadStream());
            var baseInfo = handler.GetBaseInfo<NwcaliBaseInfo>(sheet => {
                var baseInfo = new NwcaliBaseInfo();
                var timeRow = sheet.GetRow(1);
                baseInfo.Time = timeRow.GetCell(1).StringCellValue;
                var fileVersionRow = sheet.GetRow(3);
                baseInfo.FileVersion = fileVersionRow.GetCell(1).StringCellValue;
                var testerMakeRow = sheet.GetRow(4);
                baseInfo.TesterMake = testerMakeRow.GetCell(1).StringCellValue;
                var testerModelRow = sheet.GetRow(5);
                baseInfo.TesterModel = testerModelRow.GetCell(1).StringCellValue;
                var testerSnRow = sheet.GetRow(6);
                baseInfo.TesterSn = testerSnRow.GetCell(1).StringCellValue;
                var assetNoRow = sheet.GetRow(7);
                baseInfo.AssetNo = assetNoRow.GetCell(1).StringCellValue;
                var siteCodeRow = sheet.GetRow(8);
                baseInfo.SiteCode = "Electrical Lab";//siteCodeRow.GetCell(1).StringCellValue;
                var temperatureRow = sheet.GetRow(9);
                baseInfo.Temperature = temperatureRow.GetCell(1).StringCellValue;
                var relativeHumidityRow = sheet.GetRow(10);
                baseInfo.RelativeHumidity = relativeHumidityRow.GetCell(1).StringCellValue;
                var ratedAccuracyCRow = sheet.GetRow(11);
                baseInfo.RatedAccuracyC = ratedAccuracyCRow.GetCell(1).NumericCellValue / 1000;
                var ratedAccuracyVRow = sheet.GetRow(12);
                baseInfo.RatedAccuracyV = ratedAccuracyVRow.GetCell(1).NumericCellValue / 1000;
                var ammeterBitsRow = sheet.GetRow(13);
                baseInfo.AmmeterBits = Convert.ToInt32(ammeterBitsRow.GetCell(1).NumericCellValue);
                var VoltmeterBitsRow = sheet.GetRow(14);
                baseInfo.VoltmeterBits = Convert.ToInt32(VoltmeterBitsRow.GetCell(1).NumericCellValue);
                var certificateNumberRow = sheet.GetRow(15);
                baseInfo.CertificateNumber = certificateNumberRow.GetCell(1).StringCellValue;
                var calibrationEntityRow = sheet.GetRow(16);
                baseInfo.CallbrationEntity = calibrationEntityRow.GetCell(1).StringCellValue;
                var operatorRow = sheet.GetRow(17);
                baseInfo.Operator = operatorRow.GetCell(1).StringCellValue;
                #region 标准器设备信息
                var etalonsNameRow = sheet.GetRow(18);
                var etalonsCharacteristicsRow = sheet.GetRow(19);
                var etalonsAssetNoRow = sheet.GetRow(20);
                var etalonsCertificateNoRow = sheet.GetRow(22);
                var etalonsDueDateRow = sheet.GetRow(23);
                for (int i = 1; i < etalonsNameRow.LastCellNum; i++)
                {
                    if (string.IsNullOrWhiteSpace(etalonsNameRow.GetCell(i).StringCellValue))
                        break;
                    try
                    {
                        baseInfo.Etalons.Add(new Etalon
                        {
                            Name = etalonsNameRow.GetCell(i).StringCellValue,
                            Characteristics = etalonsCharacteristicsRow.GetCell(i).StringCellValue,
                            AssetNo = etalonsAssetNoRow.GetCell(i).StringCellValue,
                            CertificateNo = etalonsCertificateNoRow.GetCell(i).StringCellValue,
                            DueDate = etalonsDueDateRow.GetCell(i).StringCellValue
                        });
                    }
                    catch
                    {
                        break;
                    }
                }
                #endregion
                var commentRow = sheet.GetRow(24);
                baseInfo.Comment = commentRow.GetCell(1).StringCellValue;
                var calibrationTypeRow = sheet.GetRow(25);
                baseInfo.CalibrationType = calibrationTypeRow.GetCell(1).StringCellValue;
                var repetitiveMeasurementsCountRow = sheet.GetRow(26);
                baseInfo.RepetitiveMeasurementsCount = Convert.ToInt32(repetitiveMeasurementsCountRow.GetCell(1).NumericCellValue);
                var turRow = sheet.GetRow(27);
                baseInfo.TUR = turRow.GetCell(1).StringCellValue;
                var acceptedToleranceRow = sheet.GetRow(28);
                baseInfo.AcceptedTolerance = acceptedToleranceRow.GetCell(1).StringCellValue;
                var kRow = sheet.GetRow(29);
                baseInfo.K = kRow.GetCell(1).NumericCellValue;
                var testIntervalRow = sheet.GetRow(30);
                baseInfo.TestInterval = testIntervalRow.GetCell(1).StringCellValue;
                #region 下位机
                var pclCommentRow = sheet.GetRow(31);
                var pclNoRow = sheet.GetRow(32);
                var pclGuidRow = sheet.GetRow(33);
                for (int i = 1; i < pclNoRow.LastCellNum; i++)
                {
                    if (string.IsNullOrWhiteSpace(pclGuidRow.GetCell(i)?.StringCellValue))
                        continue;
                    try
                    {
                        baseInfo.PcPlcs.Add(new PcPlc
                        {
                            Comment = pclCommentRow.GetCell(i).StringCellValue,
                            No = Convert.ToInt32(pclNoRow.GetCell(i).StringCellValue),
                            Guid = pclGuidRow.GetCell(i).StringCellValue,
                            CalibrationDate = DateTime.Parse(baseInfo.Time),
                            ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time, baseInfo.TestInterval))
                        });
                    }
                    catch
                    {
                        break;
                    }
                }
                #endregion
                return baseInfo;
                });
            if (string.IsNullOrWhiteSpace(baseInfo.Operator))
            {
                return new Response<bool>()
                {
                    Code = 400,
                    Message = "Operator can not be null.",
                    Result = false
                };
            }
            var turV = handler.GetNwcaliTur("电压");
            var turA = handler.GetNwcaliTur("电流");
            var tv = turV.Select(v => new Repository.Domain.NwcaliTur { DataType = 1, Range = v.Range, TestPoint = v.TestPoint, Tur = v.Tur, UncertaintyContributors = v.UncertaintyContributors, SensitivityCoefficient = v.SensitivityCoefficient, Value = v.Value, Unit = v.Unit, Type = v.Type, Distribution = v.Distribution, Divisor = v.Divisor, StdUncertainty = v.StdUncertainty,DegreesOfFreedom = v.DegreesOfFreedom, SignificanceCheck = v.SignificanceCheck }).ToList();
            var ta = turA.Select(v => new Repository.Domain.NwcaliTur { DataType = 2, Range = v.Range, TestPoint = v.TestPoint, Tur = v.Tur, UncertaintyContributors = v.UncertaintyContributors, SensitivityCoefficient = v.SensitivityCoefficient, Value = v.Value, Unit = v.Unit, Type = v.Type, Distribution = v.Distribution, Divisor = v.Divisor, StdUncertainty = v.StdUncertainty, DegreesOfFreedom = v.DegreesOfFreedom, SignificanceCheck = v.SignificanceCheck }).ToList();
            baseInfo.NwcaliTurs.AddRange(tv);
            baseInfo.NwcaliTurs.AddRange(ta);
            baseInfo.ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time, baseInfo.TestInterval));
            try
            {
                foreach (var plc in baseInfo.PcPlcs)
                {
                    var list = handler.GetNWCaliPLCData($"下位机{plc.No}");
                    baseInfo.NwcaliPlcDatas.AddRange(list.Select(l=> new NwcaliPlcData {
                            PclNo = plc.No,
                            DataType = 1,
                            VerifyType = l.Verify_Type,
                            VoltsorAmps = l.VoltsorAmps,
                            Channel = l.Channel,
                            Mode = l.Mode,
                            Range = l.Range,
                            Point = l.Point,
                            CommandedValue = l.Commanded_Value,
                            MeasuredValue = l.Measured_Value,
                            Scale = l.Scale,
                            StandardTotalU = l.Standard_total_U,
                            StandardValue = l.Standard_Value
                        }));
                    var list2 = handler.GetNWCaliPLCRepetitiveMeasurementData($"下位机{plc.No}重复性测量");
                    if (list2.Count > 0)
                        baseInfo.NwcaliPlcDatas.AddRange(list2.Select(l => new NwcaliPlcData
                        {
                            PclNo = plc.No,
                            DataType = 2,
                            VerifyType = l.Verify_Type,
                            VoltsorAmps = l.VoltsorAmps,
                            Channel = l.Channel,
                            Mode = l.Mode,
                            Range = l.Range,
                            Point = l.Point,
                            CommandedValue = l.Commanded_Value,
                            MeasuredValue = l.Measured_Value,
                            Scale = l.Scale,
                            StandardTotalU = l.Standard_total_U,
                            StandardValue = l.Standard_Value
                        }));
                }
                baseInfo.FlowInstanceId = await CreateFlow(baseInfo.CertificateNumber);
                await _nwcaliCertApp.AddAsync(baseInfo);
                return new Response<bool>()
                {
                    Result = true
                };
            }
            catch (Exception ex)
            {
                await _flowInstanceApp.DeleteAsync(f=>f.Id.Equals(baseInfo.FlowInstanceId));
                return new Response<bool>()
                {
                    Code = 500,
                    Message = ex.Message,
                    Result = false
                };
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
            var baseInfo = await _nwcaliCertApp.GetInfo(certNo);
            if(baseInfo != null)
            {
                var model = await BuildModel(baseInfo);
                var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Header.html");
                var text = System.IO.File.ReadAllText(url);
                text = text.Replace("@Model.Data.BarCode", model.BarCode);
                var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                System.IO.File.WriteAllText(tempUrl, text);
                var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Footer.html");
                var datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate.cshtml", pdf=> {
                    pdf.IsWriteHtml = true;
                    pdf.PaperKind = PaperKind.A4;
                    pdf.Orientation = Orientation.Portrait;
                    pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                    pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 2.812, HtmUrl = footerUrl };
                });
                System.IO.File.Delete(tempUrl);
                return File(datas, "application/pdf");
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
        /// <param name="turV">Tur电压数据</param>
        /// <param name="turA">Tur电流数据</param>
        /// <returns></returns>
        private async Task<CertModel> BuildModel(NwcaliBaseInfo baseInfo)
        {
            var list = new List<WordModel>();
            var model = new CertModel();
            var plcData = baseInfo.NwcaliPlcDatas.Where(d => d.DataType == 1).ToList();
            var plcRepetitiveMeasurementData = baseInfo.NwcaliPlcDatas.Where(d => d.DataType == 2).ToList();
            var turV = baseInfo.NwcaliTurs.Where(d => d.DataType == 1).ToList();
            var turA = baseInfo.NwcaliTurs.Where(d => d.DataType == 2).ToList();
            #region 页眉
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
            var plcGroupData = plcData.GroupBy(d => d.PclNo);
            var plcRepetitiveMeasurementGroupData = plcRepetitiveMeasurementData.GroupBy(d => d.PclNo);
            var plc = plcGroupData.First();
            var plcrmd = plcRepetitiveMeasurementGroupData.First();
            var v = plc.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals("Charge") && p.VerifyType.Equals("Post-Calibration")).GroupBy(p=>p.Channel).First().ToList();
            var sv = v.Select(s => s.CommandedValue).OrderBy(s => s).ToList();
            sv.Sort();
            var vscale = sv[(sv.Count - 1) / 2];


            var c = plc.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals("Charge") && p.VerifyType.Equals("Post-Calibration")).OrderByDescending(a=>a.Scale).GroupBy(p => p.Scale).First().ToList();
            var cv = c.Select(c => c.CommandedValue).OrderBy(s=>s).ToList();
            cv.Sort();
            var cscale = cv[(cv.Count - 1) / 2];
            #region T.U.R. Table
            //电压
            var vPoint = turV.Select(v => v.TestPoint).Distinct().OrderBy(v => v).ToList();
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
            var cPoint = turA.Select(v => v.TestPoint).Distinct().OrderBy(v=>v).ToList();
            var cPointIndex = cPoint.IndexOf(cscale / 1000); //(cPoint.Count - 1) / 2;
            var cSpec = c.First().Scale * baseInfo.RatedAccuracyC;
            var u95_4 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_5 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_6 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var tur_4 = (2 * cSpec) / (2 * u95_4 * 1000);
            var tur_5 = (2 * cSpec) / (2 * u95_5 * 1000);
            var tur_6 = (2 * cSpec) / (2 * u95_6 * 1000);

            model.TurTables.Add(new TurTable { Number = "4", Point = $"{cPoint[cPointIndex - 1]}A", Spec = $"±{cSpec}mA", U95Standard = u95_4.ToString("e3"), TUR = tur_4.ToString("f2") });
            model.TurTables.Add(new TurTable { Number = "5", Point = $"{cPoint[cPointIndex]}A", Spec = $"±{cSpec}mA", U95Standard = u95_5.ToString("e3"), TUR = tur_5.ToString("f2") });
            model.TurTables.Add(new TurTable { Number = "6", Point = $"{cPoint[cPointIndex + 1]}A", Spec = $"±{cSpec}mA", U95Standard = u95_6.ToString("e3"), TUR = tur_6.ToString("f2") });

            #endregion

            #region Uncertainty Budget Table
            #region Voltage
            var vv = 2 * (double)v.FirstOrDefault().Scale / Math.Pow(2, baseInfo.VoltmeterBits);
            var vstd = 2 * (double)v.FirstOrDefault().Scale / (Math.Pow(2, baseInfo.VoltmeterBits) * Math.Sqrt(12));
            var voltageUncertaintyBudgetTable = new UncertaintyBudgetTable();
            voltageUncertaintyBudgetTable.Value = $"{vscale}V";
            voltageUncertaintyBudgetTable.TesterResolutionValue = vv.ToString("e3");
            voltageUncertaintyBudgetTable.TesterResolutionStdUncertainty = vstd.ToString("e3");
            var vmdcv = plcrmd.Where(d => d.CommandedValue.Equals(vscale) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals("Charge") && d.VerifyType.Equals("Post-Calibration")).GroupBy(a=>a.Channel).First().GroupBy(a => a.Point).First().ToList();
            double vror;
            if (vmdcv.Count >= 6)//贝塞尔公式法
            {
                var vavg = vmdcv.Sum(c => c.StandardValue) / vmdcv.Count;
                vror = Math.Sqrt(vmdcv.Select(c => Math.Pow(c.StandardValue - vavg, 2)).Sum() / (vmdcv.Count - 1));
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingValue = vror.ToString("e3");
                voltageUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = vror.ToString("e3");
            }
            else//极差法
            {
                var poorCoefficient = PoorCoefficients[vmdcv.Count];
                var vmdsv = vmdcv.Select(c => c.StandardValue).ToList();
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
            currentUncertaintyBudgetTable.Value = $"{cscale}mA";
            currentUncertaintyBudgetTable.TesterResolutionValue = cvv.ToString("e3");
            currentUncertaintyBudgetTable.TesterResolutionStdUncertainty = cstd.ToString("e3");
            var cmdcv = plcrmd.Where(d => d.CommandedValue.Equals(cscale) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals("Charge") && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a=>a.Point).First().ToList();
            double cror;
            if (cmdcv.Count >= 6)//贝塞尔公式法
            {
                var cavg = cmdcv.Sum(c => c.StandardValue) / cmdcv.Count / 1000;
                cror = Math.Sqrt(cmdcv.Select(c => Math.Pow(c.StandardValue / 1000 - cavg, 2)).Sum() / (cmdcv.Count - 1));
                currentUncertaintyBudgetTable.RepeatabilityOfReadingValue = cror.ToString("e3");
                currentUncertaintyBudgetTable.RepeatabilityOfReadingStdUncertainty = cror.ToString("e3");
            }
            else//极差法
            {
                var poorCoefficient = PoorCoefficients[cmdcv.Count];
                var cmdsv = cmdcv.Select(c => c.StandardValue / 1000).ToList();
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
            model.CurrentUncertaintyBudgetTables = currentUncertaintyBudgetTable;
            #endregion

            #endregion
            #endregion

            #region Data Sheet
            void CalculateVoltage(string mode, int tableIndex)
            {
                int j = 0;
                foreach (var item in plcGroupData)
                {
                    int l = 1;
                    var data = item.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals(mode) && p.VerifyType.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.CommandedValue).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            var cvCHH = $"{l}-{cvData.Channel}";
                            var cvRange = cvData.Scale;
                            var cvIndication = cvData.MeasuredValue;
                            var cvMeasuredValue = cvData.StandardValue;
                            var cvError = (cvIndication - cvMeasuredValue) * 1000;
                            double cvAcceptance = 0;
                            var cvAcceptanceStr = "";
                            var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
                            var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
                            double ror;
                            if (baseInfo.RepetitiveMeasurementsCount >= 6)//贝塞尔公式法
                            {
                                var vavg = mdcv.Sum(c => c.StandardValue) / mdcv.Count;
                                ror = Math.Sqrt(mdcv.Select(c => Math.Pow(c.StandardValue - vavg, 2)).Sum() / (mdcv.Count - 1));
                            }
                            else//极差法
                            {
                                var poorCoefficient = PoorCoefficients[mdcv.Count];
                                var mdsv = mdcv.Select(c => c.StandardValue).ToList();
                                var R = mdsv.Max() - mdsv.Min();
                                var u2 = R / poorCoefficient;
                                ror = u2;
                            }
                            //计算不确定度
                            var cvUncertaintyStr = (baseInfo.K * 1000 * Math.Sqrt(Math.Pow(cvData.StandardTotalU / 2, 2) + Math.Pow(vstd, 2) + Math.Pow(ror, 2))).ToString("G2");
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
                foreach (var item in plcGroupData)
                {
                    int l = 1;
                    var data = item.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals(mode) && p.VerifyType.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.Scale).ThenBy(dd => dd.CommandedValue).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            var CHH = $"{l}-{cvData.Channel}";
                            var Range = cvData.Scale;
                            var Indication = cvData.MeasuredValue;
                            var MeasuredValue = cvData.StandardValue;
                            var Error = Indication - MeasuredValue;
                            double Acceptance = 0;
                            var AcceptanceStr = "";

                            var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
                            var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a=>a.Point).First().ToList();
                            double ror;
                            if (baseInfo.RepetitiveMeasurementsCount >= 6)//贝塞尔公式法
                            {
                                var avg = mdcv.Sum(c => c.StandardValue) / mdcv.Count / 1000;
                                ror = Math.Sqrt(mdcv.Select(c => Math.Pow(c.StandardValue / 1000 - avg, 2)).Sum() / (mdcv.Count - 1));
                            }
                            else//极差法
                            {
                                var poorCoefficient = PoorCoefficients[mdcv.Count];
                                var mdsv = mdcv.Select(c => c.StandardValue / 1000).ToList();
                                var R = mdsv.Max() - mdsv.Min();
                                var u2 = R / poorCoefficient;
                                ror = u2;
                            }
                            //计算不确定度
                            var UncertaintyStr = (baseInfo.K * 1000 * Math.Sqrt(Math.Pow(cvData.StandardTotalU / 2, 2) + Math.Pow(cstd, 2) + Math.Pow(ror, 2))).ToString();
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
            model.ChargingVoltage = model.ChargingVoltage.OrderBy(s => s.Channel).ToList();
            #endregion

            #region Discharging Voltage
            CalculateVoltage("DisCharge", 9);
            model.DischargingVoltage = model.DischargingVoltage.OrderBy(s => s.Channel).ToList();
            #endregion

            #region Charging Current
            CalculateCurrent("Charge", 10);
            model.ChargingCurrent = model.ChargingCurrent.OrderBy(s => s.Channel).ToList();
            #endregion

            #region Discharging Current
            CalculateCurrent("DisCharge", 11);
            model.DischargingCurrent = model.DischargingCurrent.OrderBy(s => s.Channel).ToList();
            #endregion



            #endregion

            #region 签名
            var us = await _userSignApp.Load(new QueryUserSignListReq { });
            var calibrationTechnician = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.Operator));
            if (calibrationTechnician != null)
            {
                model.CalibrationTechnician = await GetSignBase64(calibrationTechnician.PictureId);
            }
            var technicalManager = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.TechnicalManager));
            if (technicalManager != null)
            {
                model.TechnicalManager = await GetSignBase64(technicalManager.PictureId);
            }
            var approvalDirector = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.ApprovalDirector));
            if (technicalManager != null)
            {
                model.ApprovalDirector = await GetSignBase64(approvalDirector.PictureId);
            }
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
            Image img = b.Encode(BarcodeLib.TYPE.CODE128, data, Color.Black, Color.White, 131, 50);

            DirUtil.CheckOrCreateDir(Path.Combine(BaseCertDir, data));
            using (var stream = new MemoryStream()) 
            {
                img.Save(stream, ImageFormat.Png);
                var bytes = new byte[stream.Length];
                stream.Position = 0;
                await stream.ReadAsync(bytes, 0, bytes.Length);
                var base64str = Convert.ToBase64String(bytes);
                return base64str;
            } 
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

        /// <summary>
        /// 获取签名base64字符串
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>
        private async Task<string> GetSignBase64(string fileId)
        {
            var file = await _fileApp.GetFileAsync(fileId);
            if(file!=null)
            {
                using (var fs = await _fileApp.GetFileStreamAsync(file.BucketName, file.FilePath))
                {
                    var bytes = new byte[fs.Length];
                    fs.Position = 0;
                    await fs.ReadAsync(bytes, 0, bytes.Length);
                    var base64str = Convert.ToBase64String(bytes);
                    return base64str;
                }
            }
            throw new Exception($"用户未配置签名。");
        }
    }
}
