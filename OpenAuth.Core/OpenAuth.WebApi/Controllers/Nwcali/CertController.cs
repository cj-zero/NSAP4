﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure;
using Infrastructure.Excel;
using Infrastructure.Wrod;
using Microsoft.AspNetCore.Mvc;
using NetOffice.Extensions.Conversion;
using OpenAuth.App;
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

        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁

        public CertController(CertinfoApp certinfoApp, CertPlcApp certPlcApp, ModuleFlowSchemeApp moduleFlowSchemeApp, FlowInstanceApp flowInstanceApp)
        {
            _certinfoApp = certinfoApp;
            _certPlcApp = certPlcApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _flowInstanceApp = flowInstanceApp;
        }

        [HttpPost]
        public async Task<Response<bool>> Generate()
        {
            var file = Request.Form.Files[0];
            var handler = new ExcelHandler(file.OpenReadStream());
            var baseInfo = handler.GetBaseInfo();
            var turV = handler.GetNwcaliTur("电压");
            var turA = handler.GetNwcaliTur("电流");
            try
            {
                //用信号量代替锁
                await semaphoreSlim.WaitAsync();
                try
                {
                    baseInfo.CertificateNumber = await CertificateNoGenerate("O");
                    await _certinfoApp.AddAsync(new AddOrUpdateCertinfoReq()
                    {
                        CertNo = baseInfo.CertificateNumber,
                        AssetNo = baseInfo.AssetNo,
                        Sn = baseInfo.TesterSn,
                        Model = baseInfo.TesterModel,
                        CalibrationDate = DateTime.Parse(baseInfo.Time),
                        ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time, baseInfo.TestInterval)),
                        Operator = baseInfo.Operator
                    });
                }
                finally
                {
                    semaphoreSlim.Release();
                }
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
                var modelList = BuildWordModels(baseInfo, plcDataDic, plcRepetitiveMeasurementDataDic, turV, turA);
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Calibration Certificate(word).docx");
                var tagetPath = Path.Combine(BaseCertDir, baseInfo.CertificateNumber, $"Cert{baseInfo.CertificateNumber}.docx");
                var result = WordHandler.DOCTemplateConvert(templatePath, tagetPath, modelList);
                if (result)
                {
                    var flowInstanceId = await CreateFlow(baseInfo.CertificateNumber);
                    var c = await _certinfoApp.GetAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                    c.CertPath = tagetPath;
                    c.BaseInfoPath = baseInfoTagetPath;
                    c.FlowInstanceId = flowInstanceId;
                    var obj = c.MapTo<AddOrUpdateCertinfoReq>();
                    await _certinfoApp.UpdateAsync(obj);
                }
                else
                {
                    await _certinfoApp.DeleteAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                    await _certPlcApp.DeleteAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                }
                return new Response<bool>()
                {
                    Result = true
                };
            }catch(Exception ex)
            {
                await _certinfoApp.DeleteAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                await _certPlcApp.DeleteAsync(s => s.CertNo.Equals(baseInfo.CertificateNumber));
                //throw ex;
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
        /// <returns></returns>
        private static List<WordModel> BuildWordModels(NwcaliBaseInfo baseInfo, Dictionary<string, List<NwcaliPLCData>> plcData, Dictionary<string, List<NwcaliPLCRepetitiveMeasurementData>> plcRepetitiveMeasurementData, List<NwcaliTur> turV, List<NwcaliTur> turA)
        {
            var list = new List<WordModel>();
            var barcode = BarcodeGenerate(baseInfo.CertificateNumber);
            #region 页眉
            list.Add(new WordModel { MarkPosition = 1, TableMark = 1, ValueType = 1, XCellMark = 1, YCellMark = 2, ValueData = barcode });
            #endregion
            #region Calibration Certificate
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 1, YCellMark = 4, ValueData = baseInfo.CertificateNumber });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 2, YCellMark = 2, ValueData = baseInfo.TesterMake });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 2, YCellMark = 4, ValueData = DateStringConverter(baseInfo.Time) });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 3, YCellMark = 2, ValueData = baseInfo.TesterModel });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 3, YCellMark = 4, ValueData = ConvertTestInterval(baseInfo.Time, baseInfo.TestInterval) });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 4, YCellMark = 2, ValueData = baseInfo.TesterSn });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 4, YCellMark = 4, ValueData = "------" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 5, YCellMark = 2, ValueData = baseInfo.AssetNo == "0" ? "------" : baseInfo.AssetNo });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 5, YCellMark = 4, ValueData = baseInfo.SiteCode });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 6, YCellMark = 2, ValueData = baseInfo.Temperature });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 1, ValueType = 0, XCellMark = 6, YCellMark = 4, ValueData = baseInfo.RelativeHumidity });
            #endregion
            #region Main Standards Used
            for (int i = 0; i < baseInfo.Etalons.Count; i++)
            {
                list.Add(new WordModel { MarkPosition = 0, TableMark = 2, ValueType = 0, XCellMark = i + 2, YCellMark = 1, ValueData = baseInfo.Etalons[i].Name });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 2, ValueType = 0, XCellMark = i + 2, YCellMark = 2, ValueData = baseInfo.Etalons[i].Characteristics });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 2, ValueType = 0, XCellMark = i + 2, YCellMark = 3, ValueData = baseInfo.Etalons[i].AssetNo });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 2, ValueType = 0, XCellMark = i + 2, YCellMark = 4, ValueData = baseInfo.Etalons[i].CertificateNo });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 2, ValueType = 0, XCellMark = i + 2, YCellMark = 5, ValueData = DateStringConverter(baseInfo.Etalons[i].DueDate) });
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
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 2, YCellMark = 1, ValueData = $"1" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 3, YCellMark = 1, ValueData = $"2" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 4, YCellMark = 1, ValueData = $"3" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 2, YCellMark = 2, ValueData = $"{vPoint[vPointIndex - 1]}V" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 3, YCellMark = 2, ValueData = $"{vPoint[vPointIndex]}V" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 4, YCellMark = 2, ValueData = $"{vPoint[vPointIndex + 1]}V" });
            var vSpec = v.First().Scale * baseInfo.RatedAccuracyV * 1000;
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 2, YCellMark = 5, ValueData = $"±{vSpec}mV" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 3, YCellMark = 5, ValueData = $"±{vSpec}mV" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 4, YCellMark = 5, ValueData = $"±{vSpec}mV" });
            var u95_1 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex - 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_2 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_3 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex + 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 2, YCellMark = 3, ValueData = u95_1.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 3, YCellMark = 3, ValueData = u95_2.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 4, YCellMark = 3, ValueData = u95_3.ToString("e3") });
            var tur_1 = (2 * vSpec / 1000) / (2 * u95_1);
            var tur_2 = (2 * vSpec / 1000) / (2 * u95_2);
            var tur_3 = (2 * vSpec / 1000) / (2 * u95_3);
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 2, YCellMark = 4, ValueData = tur_1.ToString("f2") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 3, YCellMark = 4, ValueData = tur_2.ToString("f2") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 4, YCellMark = 4, ValueData = tur_3.ToString("f2") });

            //电流
            var cPoint = turA.Select(v => v.TestPoint).Distinct().ToList();
            var cPointIndex = cPoint.IndexOf(cscale / 1000); //(cPoint.Count - 1) / 2;
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 5, YCellMark = 1, ValueData = $"4" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 6, YCellMark = 1, ValueData = $"5" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 7, YCellMark = 1, ValueData = $"6" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 5, YCellMark = 2, ValueData = $"{cPoint[cPointIndex - 1]}A" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 6, YCellMark = 2, ValueData = $"{cPoint[cPointIndex]}A" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 7, YCellMark = 2, ValueData = $"{cPoint[cPointIndex + 1]}A" });
            var cSpec = c.First().Scale * baseInfo.RatedAccuracyC;
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 5, YCellMark = 5, ValueData = $"±{cSpec}mA" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 6, YCellMark = 5, ValueData = $"±{cSpec}mA" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 7, YCellMark = 5, ValueData = $"±{cSpec}mA" });
            var u95_4 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_5 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            var u95_6 = 2 * Math.Sqrt(turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 5, YCellMark = 3, ValueData = u95_4.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 6, YCellMark = 3, ValueData = u95_5.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 7, YCellMark = 3, ValueData = u95_6.ToString("e3") });
            var tur_4 = (2 * cSpec) / (2 * u95_4 * 1000);
            var tur_5 = (2 * cSpec) / (2 * u95_5 * 1000);
            var tur_6 = (2 * cSpec) / (2 * u95_6 * 1000);
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 5, YCellMark = 4, ValueData = tur_4.ToString("f2") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 6, YCellMark = 4, ValueData = tur_5.ToString("f2") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 3, ValueType = 0, XCellMark = 7, YCellMark = 4, ValueData = tur_6.ToString("f2") });


            #endregion

            #region Uncertainty Budget Table
            #region Voltage
            var vv = 2 * (double)v.FirstOrDefault().Scale / Math.Pow(2, baseInfo.VoltmeterBits);
            var vstd = 2 * (double)v.FirstOrDefault().Scale / (Math.Pow(2, baseInfo.VoltmeterBits) * Math.Sqrt(12));
            list.Add(new WordModel { MarkPosition = 0, TableMark = 4, ValueType = 0, XCellMark = 1, YCellMark = 2, ValueData = $"{vscale}V" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 6, YCellMark = 2, ValueData = vv.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 6, YCellMark = 8, ValueData = vstd.ToString("e3") });

            var vmdcv = plcrmd.Where(d => d.Commanded_Value.Equals(vscale) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals("Charge") && d.Verify_Type.Equals("Post-Calibration")).GroupBy(a=>a.Channel).First().ToList();
            double vror;
            if (vmdcv.Count >= 6)//贝塞尔公式法
            {
                var vavg = vmdcv.Sum(c => c.Standard_Value) / vmdcv.Count;
                vror = Math.Sqrt(vmdcv.Select(c => Math.Pow(c.Standard_Value - vavg, 2)).Sum() / (vmdcv.Count - 1));
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 7, YCellMark = 2, ValueData = vror.ToString("e3") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 7, YCellMark = 8, ValueData = vror.ToString("e3") });
            }
            else//极差法
            {
                var poorCoefficient = PoorCoefficients[vmdcv.Count];
                var vmdsv = vmdcv.Select(c => c.Standard_Value).ToList();
                var R = vmdsv.Max() - vmdsv.Min();
                var u2 = R / poorCoefficient;
                vror = u2;
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 7, YCellMark = 2, ValueData = u2.ToString("e3") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 7, YCellMark = 8, ValueData = u2.ToString("e3") });
            }
            turV = turV.Where(v => v.TestPoint == vscale).ToList();
            var combinedUncertaintyV = Math.Sqrt(turV.Sum(v => Math.Pow(v.StdUncertainty, 2)) + Math.Pow(vstd, 2) + Math.Pow(vror, 2));
            list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 8, YCellMark = 6, ValueData = combinedUncertaintyV.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 8, YCellMark = 7, ValueData = "100.000%" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 9, YCellMark = 7, ValueData = baseInfo.K });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 10, YCellMark = 6, ValueData = (baseInfo.K * combinedUncertaintyV).ToString("e3") });
            for (int i = 0; i < turV.Count; i++)
            {
                var tv = turV[i];
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 1, ValueData = tv.UncertaintyContributors });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 2, ValueData = tv.Value.ToString("e3") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 3, ValueData = tv.SensitivityCoefficient });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 4, ValueData = tv.Unit });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 5, ValueData = tv.Type });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 6, ValueData = tv.Distribution });
                if (tv.UncertaintyContributors.Equals("Resolution"))
                {
                    list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 7, ValueData = Math.Sqrt(12).ToString("f3") });
                }
                else
                {
                    list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 7, ValueData = tv.Divisor });
                }
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 8, ValueData = tv.StdUncertainty.ToString("e3") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 2 + i, YCellMark = 9, ValueData = (Math.Pow(tv.StdUncertainty, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3") });
            }
            list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 6, YCellMark = 9, ValueData = (Math.Pow(vstd, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 5, ValueType = 0, XCellMark = 7, YCellMark = 9, ValueData = (Math.Pow(vror, 2) / Math.Pow(combinedUncertaintyV, 2)).ToString("P3") });

            #endregion

            #region Current
            var cvv = 2 * (double)c.FirstOrDefault().Scale / 1000 / Math.Pow(2, baseInfo.AmmeterBits);
            var cstd = 2 * (double)c.FirstOrDefault().Scale / 1000 / (Math.Pow(2, baseInfo.AmmeterBits) * Math.Sqrt(12));
            list.Add(new WordModel { MarkPosition = 0, TableMark = 6, ValueType = 0, XCellMark = 1, YCellMark = 2, ValueData = $"{cscale}mA" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 4, YCellMark = 2, ValueData = cvv.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 4, YCellMark = 8, ValueData = cstd.ToString("e3") });
            var cmdcv = plcrmd.Where(d => d.Commanded_Value.Equals(cscale) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals("Charge") && d.Verify_Type.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().ToList();
            double cror;
            if (cmdcv.Count >= 6)//贝塞尔公式法
            {
                var cavg = cmdcv.Sum(c => c.Standard_Value) / cmdcv.Count / 1000;
                cror = Math.Sqrt(cmdcv.Select(c => Math.Pow(c.Standard_Value / 1000 - cavg, 2)).Sum() / (cmdcv.Count - 1));
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 5, YCellMark = 2, ValueData = cror.ToString("e3") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 5, YCellMark = 8, ValueData = cror.ToString("e3") });
            }
            else//极差法
            {
                var poorCoefficient = PoorCoefficients[cmdcv.Count];
                var cmdsv = cmdcv.Select(c => c.Standard_Value / 1000).ToList();
                var R = cmdsv.Max() - cmdsv.Min();
                var u2 = R / poorCoefficient;
                cror = u2;
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 5, YCellMark = 2, ValueData = u2.ToString("e3") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 5, YCellMark = 8, ValueData = u2.ToString("e3") });
            }

            turA = turA.Where(a => a.TestPoint * 1000 == cscale).ToList();

            var combinedUncertaintyA = Math.Sqrt(turA.Sum(c => Math.Pow(c.StdUncertainty, 2)) + Math.Pow(cstd, 2) + Math.Pow(cror, 2));
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 6, YCellMark = 6, ValueData = combinedUncertaintyA.ToString("e3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 6, YCellMark = 7, ValueData = "100.000%" });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 7, YCellMark = 7, ValueData = baseInfo.K });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 8, YCellMark = 6, ValueData = (baseInfo.K * combinedUncertaintyA).ToString("e3") });
            for (int i = 0; i < turA.Count; i++)
            {
                var ta = turA[i];
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 1, ValueData = ta.UncertaintyContributors });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 2, ValueData = ta.Value.ToString("e3") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 3, ValueData = ta.SensitivityCoefficient.ToString("f5") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 4, ValueData = ta.Unit });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 5, ValueData = ta.Type });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 6, ValueData = ta.Distribution });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 7, ValueData = ta.Divisor });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 8, ValueData = ta.StdUncertainty.ToString("e3") });
                list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 2 + i, YCellMark = 9, ValueData = (Math.Pow(ta.StdUncertainty, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3") });
            }
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 4, YCellMark = 9, ValueData = (Math.Pow(cstd, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3") });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 7, ValueType = 0, XCellMark = 5, YCellMark = 9, ValueData = (Math.Pow(cror, 2) / Math.Pow(combinedUncertaintyA, 2)).ToString("P3") });
            #endregion

            #endregion
            #endregion

            #region Data Sheet
            void CalculateVoltage(string mode, int tableIndex)
            {
                int j = 0;
                foreach (var item in plcData)
                {
                    var data = item.Value.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals(mode) && p.Verify_Type.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.Commanded_Value).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            var cvCHH = $"{item.Key.Substring(item.Key.Length - 1, 1)}-{cvData.Channel}";
                            var cvRange = cvData.Scale;
                            var cvIndication = cvData.Measured_Value;
                            var cvMeasuredValue = cvData.Standard_Value;
                            var cvError = (cvIndication - cvMeasuredValue) * 1000;
                            double cvAcceptance = 0;
                            var cvAcceptanceStr = "";

                            var mdcv = plcrmd.Where(d => d.Commanded_Value.Equals(cvData.Commanded_Value) && d.VoltsorAmps.Equals("Volts") && d.Channel == item2.Key && d.Mode.Equals("Charge") && d.Verify_Type.Equals("Post-Calibration")).ToList();
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

                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 1, ValueData = cvCHH });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 2, ValueData = cvRange });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 3, ValueData = IndicationReduce });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 4, ValueData = MeasuredValueReduce });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 5, ValueData = ErrorReduce });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 6, ValueData = cvAcceptanceStr });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 7, ValueData = UncertaintyReduce });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 8, ValueData = $"        {cvConclustion}" });
                            j++;
                        }
                    }
                }
            }
            void CalculateCurrent(string mode, int tableIndex)
            {
                int j = 0;
                foreach (var item in plcData)
                {
                    var data = item.Value.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals(mode) && p.Verify_Type.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                    foreach (var item2 in data)
                    {
                        var cvDataList = item2.OrderBy(dd => dd.Scale).ThenBy(dd => dd.Commanded_Value).ToList();
                        foreach (var cvData in cvDataList)
                        {
                            var CHH = $"{item.Key.Substring(item.Key.Length - 1, 1)}-{cvData.Channel}";
                            var Range = cvData.Scale;
                            var Indication = cvData.Measured_Value;
                            var MeasuredValue = cvData.Standard_Value;
                            var Error = Indication - MeasuredValue;
                            double Acceptance = 0;
                            var AcceptanceStr = "";

                            var mdcv = plcrmd.Where(d => d.Commanded_Value.Equals(cvData.Commanded_Value) && d.VoltsorAmps.Equals("Amps") && d.Channel == item2.Key && d.Mode.Equals("Charge") && d.Verify_Type.Equals("Post-Calibration")).ToList();
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

                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 1, ValueData = CHH });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 2, ValueData = (double)Range / 1000 });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 3, ValueData = IndicationReduce });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 4, ValueData = MeasuredValueReduce });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 5, ValueData = ErrorReduce });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 6, ValueData = AcceptanceStr });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 7, ValueData = UncertaintyReduce });
                            list.Add(new WordModel { MarkPosition = 0, TableMark = tableIndex, ValueType = 0, XCellMark = j + 2, YCellMark = 8, ValueData = $"       {Conclustion}" });
                            j++;
                        }
                    }
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
            var signPath1 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "yang.png");
            var signPath2 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "zhou.png");
            var signPath3 = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "chen.png");
            list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 1, ValueData = signPath1 });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 1, YCellMark = 3, ValueData = signPath2 });
            list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 3, YCellMark = 1, ValueData = signPath3 });
            var signetPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "印章.png");
            list.Add(new WordModel { MarkPosition = 0, TableMark = 12, ValueType = 1, XCellMark = 3, YCellMark = 3, ValueData = signetPath });
            #endregion
            return list;
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
        private static string BarcodeGenerate(string data)
        {
            System.Drawing.Font labelFont = new System.Drawing.Font("OCRB", 11f, FontStyle.Bold);
            BarcodeLib.Barcode b = new BarcodeLib.Barcode
            {
                IncludeLabel = true,
                LabelFont = labelFont
            };
            Image img = b.Encode(BarcodeLib.TYPE.CODE128, data, Color.Black, Color.White, 180, 49);

            DirUtil.CheckOrCreateDir(Path.Combine(BaseCertDir, data));

            var path = Path.Combine(BaseCertDir, data, $"{data}.png");
            img.Save(path, ImageFormat.Png);

            //return img;
            return path;
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
            var istr = indication.ToString().Split('.')[1];
            var mstr = measuredValue.ToString().Split('.')[1];
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
            var errorStr = (Convert.ToDouble(indicationStr) * 1000 - Convert.ToDouble(measuredValueStr) * 1000).ToString($"f{j - 3}");//error.ToString($"f{j - 3}");
            var acceptanceStr = acceptance.ToString($"f{j - 3}");
            var uncertaintyStr = uncertainty.ToString($"f{j - 3}");
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
            var istr = indication.ToString().Split('.')[1];
            istr = (indication / 1000).ToString($"f{istr.Length + 3}").Split('.')[1];
            var mstr = measuredValue.ToString().Split('.')[1];
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
            var errorStr = (Convert.ToDouble(indicationStr) * 1000 - Convert.ToDouble(measuredValueStr) * 1000).ToString($"f{j - 3}");// error.ToString($"f{j - 3}");
            var acceptanceStr = acceptance.ToString($"f{j - 3}");
            var uncertaintyStr = uncertainty.ToString($"f{j - 3}");
            return (indicationStr, measuredValueStr, errorStr, acceptanceStr, uncertaintyStr);
        }

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
