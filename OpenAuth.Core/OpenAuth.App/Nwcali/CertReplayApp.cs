using Infrastructure;
using Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali.Response;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain.CapNwcail;
using OpenAuth.Repository.Domain.Sap;
using OpenAuth.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using DinkToPdf;
using Infrastructure.Export;
using Infrastructure.Wrod;
using Microsoft.AspNetCore.Server.IIS.Core;
using Npoi.Mapper;
using NPOI.SS.Formula.Atp;
using OpenAuth.App.Nwcali;
using OpenAuth.App.Nwcali.Models;
using OpenAuth.App.Nwcali.Request;
using Org.BouncyCastle.Ocsp;
using NSAP.Entity;
using OpenAuth.Repository;
using System.Text.RegularExpressions;
using Infrastructure.Excel;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Serilog;
using Microsoft.Extensions.Options;
using Magicodes.ExporterAndImporter.Core;
using Magicodes.ExporterAndImporter.Excel;
using OpenAuth.App.DDVoice.Common;
using Microsoft.Extensions.Logging;
using System.Net;

namespace OpenAuth.App.Nwcali
{
    public class CertReplayApp : OnlyUnitWorkBaeApp
    {
        static readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);//用信号量代替锁
        public List<CertHelp> certHelps = new List<CertHelp>();
        private readonly FileApp _fileApp;
        private readonly UserSignApp _userSignApp;
        private ILogger<CertReplayApp> _logger;
        private static readonly string BaseCertDir = Path.Combine(Directory.GetCurrentDirectory(), "certs");
        private static readonly Dictionary<int, double> PoorCoefficients = new Dictionary<int, double>()
        {
            { 2, 1.13 },
            { 3, 1.69 },
            { 4, 2.06 },
            { 5, 2.33 }
        };
        public CertReplayApp(ILogger<CertReplayApp> logger,UserSignApp userSignApp, FileApp fileApp,IUnitWork unitWork, IAuth auth) : base(unitWork, auth)
        {
            _fileApp = fileApp;
            _userSignApp = userSignApp;
            _logger = logger;
        }

        public async Task AddAsync(NwcaliBaseInfo baseInfo, string fileName)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var user = loginContext.User;
            string userId = await UnitWork.Find<OpenAuth.Repository.Domain.User>(r => r.Name == baseInfo.Operator).Select(r => r.Id).FirstOrDefaultAsync();
            baseInfo.OperatorId = userId;
            await semaphoreSlim.WaitAsync();
            try
            {
                
                CertHelp certHelp = certHelps.Where(r => r.CertPath.Contains(fileName)).FirstOrDefault();
                if (certHelp != null)
                {
                    baseInfo.CertificateNumber = certHelp.CertificateNumber;
                    baseInfo.CreateTime = certHelp.CreateTime;
                    baseInfo.CreateUser = certHelp.CreateUser;
                    baseInfo.CreateUserId = certHelp.CreateUserId;
                    baseInfo.Issuer = certHelp.Issuer;
                    baseInfo.IssuerId = certHelp.IssuerId;
                    baseInfo.Operator = certHelp.Operator;
                    baseInfo.OperatorId = certHelp.OperatorId;
                    baseInfo.TechnicalManager = certHelp.TechnicalManager;
                    baseInfo.TechnicalManagerId = certHelp.TechnicalManagerId;
                    baseInfo.ApprovalDirector = certHelp.ApprovalDirector;
                    baseInfo.ApprovalDirectorId = certHelp.ApprovalDirectorId;
                    baseInfo.Time = certHelp.Time;
                    baseInfo.CertPath = certHelp.CertPath;
                    baseInfo.FileVersion = certHelp.FileVersion;
                    baseInfo.TesterMake = certHelp.TesterMake;
                    baseInfo.TesterModel = certHelp.TesterModel;
                    baseInfo.TesterSn = certHelp.TesterSn;
                    baseInfo.UpdateTime = certHelp.UpdateTime;

                    var testerModel = await UnitWork.Find<OINS>(o => o.manufSN.Equals(baseInfo.TesterSn)).Select(o => o.itemCode).ToListAsync();
                    if (testerModel != null && testerModel.Count == 1 && !testerModel.Contains("ZWJ"))
                    {
                        if (testerModel.FirstOrDefault().Contains(baseInfo.TesterModel))
                            baseInfo.TesterModel = testerModel.FirstOrDefault();
                    }

                    var baseInfos = await UnitWork.Find<NwcaliBaseInfo>(r => r.CertificateNumber == baseInfo.CertificateNumber).ToListAsync();
                    if (!(baseInfos != null && baseInfos.Count() > 0))
                    {
                        await UnitWork.AddAsync(baseInfo);
                        await UnitWork.SaveAsync();
                    }                 
                }
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// 第一签名人更改为庞远球/张平
        /// </summary>
        /// <param name="baseInfo">校准报表基础信息</param>
        /// <returns></returns>
        public async Task SetIssuser1(NwcaliBaseInfo baseInfo)
        {
            var category = await UnitWork.Find<OpenAuth.Repository.Domain.Category>(c => c.TypeId == "SYS_FirstSignAcount").Select(c => c.Name).ToListAsync();
            if (!category.Contains(baseInfo.Issuer))
            {
                string userName = category.FirstOrDefault();
                var uinfo = await UnitWork.Find<OpenAuth.Repository.Domain.UserSign>(c => c.UserName == userName).FirstOrDefaultAsync();
                if (uinfo == null)
                {
                    throw new CommonException("当前出证人暂无签名图片，请确认后上传。", 400100);
                }

                baseInfo.Issuer = uinfo.UserName;
                baseInfo.IssuerId = uinfo.UserId;
            }
        }

        public List<CertHelp> GetProblemData()
        {
            var data = from a in  UnitWork.Find<OpenAuth.Repository.Domain.NwcaliBaseInfo>(r => r.Time >= Convert.ToDateTime("2022-12-01") && r.Time <= Convert.ToDateTime("2022-12-30")).ToList()
                       join b in  UnitWork.Find<OpenAuth.Repository.Domain.EntrustmentDetail>(null).Select(r => new { r.SerialNumber, r.EntrustmentId }).ToList()
                       on a.TesterSn equals b.SerialNumber
                       join c in  UnitWork.Find<OpenAuth.Repository.Domain.Entrustment>(null).Select(r => new { r.EntrustedDate, r.Id }).ToList()
                       on b.EntrustmentId equals c.Id
                       where a.Time <= c.EntrustedDate
                       select new CertHelp
                       {
                           Id = a.Id,
                           Time = a.Time,
                           CertPath = a.CertPath,
                           PdfPath = a.PdfPath,
                           CNASPdfPath = a.CNASPdfPath,
                           FileVersion = a.FileVersion,
                           TesterMake = a.TesterMake,
                           TesterModel = a.TesterModel,
                           TesterSn = a.TesterSn,
                           AssetNo = a.AssetNo,
                           SiteCode = a.SiteCode,
                           Temperature = a.Temperature,
                           RelativeHumidity = a.RelativeHumidity,
                           RatedAccuracyC = a.RatedAccuracyC,
                           RatedAccuracyV = a.RatedAccuracyV,
                           AmmeterBits = a.AmmeterBits,
                           VoltmeterBits = a.VoltmeterBits,
                           CertificateNumber = a.CertificateNumber,
                           CallbrationEntity = a.CallbrationEntity,
                           Operator = a.Operator,
                           OperatorId = a.OperatorId,
                           TechnicalManager = a.TechnicalManager,
                           TechnicalManagerId = a.TechnicalManagerId,
                           ApprovalDirector = a.ApprovalDirector,
                           ApprovalDirectorId = a.ApprovalDirectorId,
                           Comment = a.Comment,
                           CalibrationType = a.CalibrationType,
                           RepetitiveMeasurementsCount = a.RepetitiveMeasurementsCount,
                           TUR = a.TUR,
                           AcceptedTolerance = a.AcceptedTolerance,
                           K = a.K,
                           TestInterval = a.TestInterval,
                           CreateTime = a.CreateTime,
                           ExpirationDate = a.ExpirationDate,
                           CreateUserId = a.CreateUserId,
                           CreateUser = a.CreateUser,
                           UpdateTime = a.UpdateTime,
                           UpdateUserId =a.UpdateUserId,
                           UpdateUser = a.UpdateUser,
                           FlowInstanceId = a.FlowInstanceId,
                           Issuer= a.Issuer,
                           IssuerId = a.IssuerId,
                           StartTime = a.StartTime,
                           CalibrationStatus = a.CalibrationStatus,
                           CalibrationMode = a.CalibrationMode,
                           ToolAssetCode = a.ToolAssetCode,
                           TotalSeconds = a.TotalSeconds,
                           EntrustedDate = c.EntrustedDate
                       };

            return data.ToList();
        }

        /// <summary>
        /// 保存证书pdf
        /// </summary>
        /// <param name="certNo"></param>
        /// <returns></returns>
        public async Task CreateNwcailFile(string certNo)
        {
            var baseInfo = await GetInfo(certNo);
            if (baseInfo != null)
            {
                try
                {
                    var folderYear = DateTime.Now.ToString("yyyy");
                    var basePath = Path.Combine("D:\\nsap4file", "nwcail", folderYear, baseInfo.CertificateNumber);
                    var model = await BuildModel(baseInfo);
                    #region 生成英文版
                    var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Header.html");
                    var text = System.IO.File.ReadAllText(url);
                    text = text.Replace("@Model.Data.BarCode", model.BarCode);
                    var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                    System.IO.File.WriteAllText(tempUrl, text);
                    var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Footer.html");
                    var datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate.cshtml", pdf =>
                    {
                        pdf.IsWriteHtml = true;
                        pdf.PaperKind = PaperKind.A4;
                        pdf.Orientation = Orientation.Portrait;
                        pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                        pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 2.812, HtmUrl = footerUrl };
                    });
                    System.IO.File.Delete(tempUrl);
                    Stream stream1 = new MemoryStream(datas);
                    #endregion

                    #region 生成中文版
                    //获取委托单
                    await SetIssuser1(baseInfo);
                    var calibrationTechnician = await UnitWork.Find<OpenAuth.Repository.Domain.UserSign>(r => r.UserName.Equals(baseInfo.Issuer)).FirstOrDefaultAsync();
                    if (calibrationTechnician != null)
                    {
                        model.CalibrationTechnician = await GetSignBase64(calibrationTechnician.PictureId);
                    }

                    var entrustment = await GetEntrustment(model.CalibrationCertificate.TesterSn);
                    model.CalibrationCertificate.EntrustedUnit = entrustment?.CertUnit;
                    model.CalibrationCertificate.EntrustedUnitAdress = entrustment?.CertCountry + entrustment?.CertProvince + entrustment?.CertCity + entrustment?.CertAddress;
                    //委托日期需小于校准日期
                    if (entrustment != null && !string.IsNullOrWhiteSpace(entrustment.EntrustedDate.ToString()) && entrustment?.EntrustedDate > DateTime.Parse(model.CalibrationCertificate.CalibrationDate))
                        entrustment.EntrustedDate = (DateTime.Parse(model.CalibrationCertificate.CalibrationDate)).AddDays(-2);

                    model.CalibrationCertificate.EntrustedDate = !string.IsNullOrWhiteSpace(entrustment?.EntrustedDate.ToString()) ? entrustment?.EntrustedDate.Value.ToString("yyyy年MM月dd日") : "";
                    model.CalibrationCertificate.CalibrationDate = DateTime.Parse(model.CalibrationCertificate.CalibrationDate).ToString("yyyy年MM月dd日");
                    var temp = Math.Round(Convert.ToDecimal(model.CalibrationCertificate.Temperature), 1);
                    model.CalibrationCertificate.Temperature = temp.ToString("0.0");
                    foreach (var item in model.MainStandardsUsed)
                    {
                        if (!string.IsNullOrWhiteSpace(item.DueDate))
                            item.DueDate = DateTime.Parse(item.DueDate).ToString("yyyy-MM-dd");
                        if (item.Name.Contains(","))
                        {
                            var split = item.Name.Split(",");
                            //item.EnName = split[0];
                            item.Name = split[0];
                        }
                        item.Characterisics = item.Characterisics.Replace("Urel", "<i>U</i><sub>rel</sub>").Replace("k=", "<i>k</i>=");
                    }

                    url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Header.html");
                    text = System.IO.File.ReadAllText(url);
                    text = text.Replace("@Model.Data.BarCode", model.BarCode);
                    tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                    System.IO.File.WriteAllText(tempUrl, text);
                    footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Footer.html");
                    datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate CNAS.cshtml", pdf =>
                    {
                        pdf.IsWriteHtml = true;
                        pdf.PaperKind = PaperKind.A4;
                        pdf.Orientation = Orientation.Portrait;
                        pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };//2.812
                        pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 0, HtmUrl = footerUrl };
                    });
                    System.IO.File.Delete(tempUrl);
                    var fullPathCnas = Path.Combine(basePath, $"{certNo}_CNAS" + ".pdf");
                    Stream stream2 = new MemoryStream(datas);
                    #endregion

                    await semaphoreSlim.WaitAsync();
                    //上传华为云
                    var fileResp = await _fileApp.UploadFileToHuaweiOBS($"nwcail/{baseInfo.CertificateNumber}/{baseInfo.CertificateNumber}_EN.pdf", null, stream1);
                    var fileRespCn = await _fileApp.UploadFileToHuaweiOBS($"nwcail/{baseInfo.CertificateNumber}/{baseInfo.CertificateNumber}_CNAS.pdf", null, stream2);

                    await UnitWork.UpdateAsync<NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new NwcaliBaseInfo { PdfPath = fileResp.FilePath, CNASPdfPath = fileRespCn.FilePath });

                    //生成证书文件后删除校准数据
                    await UnitWork.DeleteAsync<Repository.Domain.CapNwcail.Etalon>(x => x.NwcaliBaseInfoId == baseInfo.Id);
                    await UnitWork.DeleteAsync<Repository.Domain.CapNwcail.NwcaliPlcData>(x => x.NwcaliBaseInfoId == baseInfo.Id);
                    await UnitWork.DeleteAsync<Repository.Domain.CapNwcail.NwcaliTur>(x => x.NwcaliBaseInfoId == baseInfo.Id);

                    await UnitWork.SaveAsync();
                    semaphoreSlim.Release();
                }
                catch (Exception e)
                {

                    throw e;
                }
            }
        }

        public async Task<OpenAuth.Repository.Domain.Entrustment> GetEntrustment(string serialNumber)
        {
            return await UnitWork.Find<OpenAuth.Repository.Domain.Entrustment>(c => c.EntrustmentDetails.Any(e => e.SerialNumber == serialNumber)).FirstOrDefaultAsync();
        }

        public async Task<NwcaliBaseInfo> GetInfo(string certNo)
        {
            var info = await UnitWork.Find<NwcaliBaseInfo>(null).FirstOrDefaultAsync(b => b.CertificateNumber.Equals(certNo));
            if (info != null)
            {
                info.NwcaliTurs = await UnitWork.Find<OpenAuth.Repository.Domain.CapNwcail.NwcaliTur>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.NwcaliPlcDatas = await UnitWork.Find<NwcaliPlcData>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.PcPlcs = await UnitWork.Find<PcPlc>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
                info.Etalons = await UnitWork.Find<Etalon>(c => c.NwcaliBaseInfoId == info.Id).ToListAsync();
            }
            return info;
        }

        public async Task<CertModel> BuildModel(NwcaliBaseInfo baseInfo, string type = "")
        {
            var list = new List<WordModel>();
            var model = new CertModel();
            var plcData = baseInfo.NwcaliPlcDatas.Where(d => d.DataType == 1).ToList();
            var plcRepetitiveMeasurementData = baseInfo.NwcaliPlcDatas.Where(d => d.DataType == 2).ToList();
            var turV = baseInfo.NwcaliTurs.Where(d => d.DataType == 1).ToList();
            var turA = baseInfo.NwcaliTurs.Where(d => d.DataType == 2).ToList();
            try
            {
                #region 页眉
                var barcode = await BarcodeGenerate(baseInfo.CertificateNumber);
                model.BarCode = barcode;
                #endregion

                #region Calibration Certificate
                model.CalibrationCertificate.CertificatenNumber = baseInfo.CertificateNumber;
                model.CalibrationCertificate.TesterMake = baseInfo.TesterMake;
                model.CalibrationCertificate.CalibrationDate = DateStringConverter(baseInfo.Time.Value.ToString());
                model.CalibrationCertificate.TesterModel = baseInfo.TesterModel;
                model.CalibrationCertificate.CalibrationDue = ConvertTestInterval(baseInfo.Time.Value.ToString(), baseInfo.TestInterval);
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
                        DueDate = DateStringConverter(baseInfo.Etalons[i].DueDate),
                        CalibrationEntity = baseInfo.Etalons[i].CalibrationEntity,
                        //CertificateNo = ((baseInfo.Etalons[i].CalibrationEntity).Contains(baseInfo.Etalons[i].CertificateNo) ? "" : baseInfo.Etalons[i].CertificateNo).TrimEnd('\\')
                        CertificateNo = baseInfo.Etalons[i].CertificateNo.TrimEnd('\\')
                    });
                }
                #endregion

                #region Uncertainty Budget
                var plcGroupData = plcData.GroupBy(d => d.PclNo);
                var plcRepetitiveMeasurementGroupData = plcRepetitiveMeasurementData.GroupBy(d => d.PclNo);
                var plc = plcGroupData.First();
                var plcrmd = plcRepetitiveMeasurementGroupData.First();
                var v = plc.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals("Charge") && p.VerifyType.Equals("Post-Calibration")).GroupBy(p => p.Channel).First().ToList();
                var sv = v.Select(s => s.CommandedValue).OrderBy(s => s).ToList();
                sv.Sort();
                var vscale = sv[(sv.Count - 1) / 2];


                var c = plc.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals("Charge") && p.VerifyType.Equals("Post-Calibration")).OrderByDescending(a => a.Scale).GroupBy(p => p.Scale).First().ToList();
                var cv = c.Select(c => c.CommandedValue).OrderBy(s => s).ToList();
                cv.Sort();
                var cscale = cv[(cv.Count - 1) / 2];
                if (type != "cnas")
                {
                    #region T.U.R. Table
                    //电压
                    var vPoint = turV.Select(v => v.TestPoint).Distinct().OrderBy(v => v).ToList();
                    var vPointIndex = (vPoint.Count - 1) / 2;
                    var vSpec = v.First().Scale * baseInfo.RatedAccuracyV * 1000;
                    var u95_1 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex - 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
                    var a = turV.Where(v => v.TestPoint == vPoint[vPointIndex]).ToList();
                    var u95_2 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
                    var u95_3 = 2 * Math.Sqrt(turV.Where(v => v.TestPoint == vPoint[vPointIndex + 1]).Sum(v => Math.Pow(v.StdUncertainty, 2)));
                    var tur_1 = (2 * vSpec / 1000) / (2 * u95_1);
                    var tur_2 = (2 * vSpec / 1000) / (2 * u95_2);
                    var tur_3 = (2 * vSpec / 1000) / (2 * u95_3);
                    model.TurTables.Add(new TurTable { Number = "1", Point = $"{vPoint[vPointIndex - 1]}V", Spec = $"±{vSpec}mV", U95Standard = u95_1.ToString("e3") + "V", TUR = tur_1.ToString("f2") });
                    model.TurTables.Add(new TurTable { Number = "2", Point = $"{vPoint[vPointIndex]}V", Spec = $"±{vSpec}mV", U95Standard = u95_2.ToString("e3") + "V", TUR = tur_2.ToString("f2") });
                    model.TurTables.Add(new TurTable { Number = "3", Point = $"{vPoint[vPointIndex + 1]}V", Spec = $"±{vSpec}mV", U95Standard = u95_3.ToString("e3") + "V", TUR = tur_3.ToString("f2") });
                    //电流
                    var cPoint = turA.Select(v => v.TestPoint).Distinct().OrderBy(v => v).ToList();
                    var cPointIndex = cPoint.IndexOf(cscale / 1000); //(cPoint.Count - 1) / 2;
                    var cSpec = c.First().Scale * baseInfo.RatedAccuracyC;
                    var U95_4turA = turA;
                    if (turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1] && v.Tur != 0).ToList().Count > 2)
                    {
                        U95_4turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1] && v.Tur != 0).GroupBy(t => t.UncertaintyContributors).Select(t => t.First()).ToList();
                        if (U95_4turA.Count > 2)
                        {
                            U95_4turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1] && v.Tur != 0).OrderBy(t => t.Range).Take(2).ToList();
                        }
                    }
                    else
                    {
                        U95_4turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex - 1] && v.Tur != 0).ToList();
                    }
                    var u95_4 = 2 * Math.Sqrt(U95_4turA.Sum(v => Math.Pow(v.StdUncertainty, 2)));
                    var U95_5turA = turA;
                    if (turA.Where(v => v.TestPoint == cPoint[cPointIndex] && v.Tur != 0).ToList().Count > 2)
                    {
                        U95_5turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex] && v.Tur != 0).GroupBy(t => t.UncertaintyContributors).Select(t => t.First()).ToList();
                        if (U95_5turA.Count > 2)
                        {
                            U95_5turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex] && v.Tur != 0).OrderBy(t => t.Range).Take(2).ToList();
                        }
                    }
                    else
                    {
                        U95_5turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex] && v.Tur != 0).ToList();
                    }
                    var u95_5 = 2 * Math.Sqrt(U95_5turA.Sum(v => Math.Pow(v.StdUncertainty, 2)));
                    var U95_6turA = turA;
                    if (turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1] && v.Tur != 0).ToList().Count > 2)
                    {
                        U95_6turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1] && v.Tur != 0).GroupBy(t => t.UncertaintyContributors).Select(t => t.First()).ToList();
                        if (U95_6turA.Count > 2)
                        {
                            U95_6turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1] && v.Tur != 0).OrderBy(t => t.Range).Take(2).ToList();
                        }
                    }
                    else
                    {
                        U95_6turA = turA.Where(v => v.TestPoint == cPoint[cPointIndex + 1] && v.Tur != 0).ToList();
                    }
                    var u95_6 = 2 * Math.Sqrt(U95_6turA.Sum(v => Math.Pow(v.StdUncertainty, 2)));
                    var tur_4 = (2 * cSpec) / (2 * u95_4 * 1000);
                    var tur_5 = (2 * cSpec) / (2 * u95_5 * 1000);
                    var tur_6 = (2 * cSpec) / (2 * u95_6 * 1000);

                    model.TurTables.Add(new TurTable { Number = "4", Point = $"{cPoint[cPointIndex - 1]}A", Spec = $"±{cSpec}mA", U95Standard = u95_4.ToString("e3") + "A", TUR = tur_4.ToString("f2") });
                    model.TurTables.Add(new TurTable { Number = "5", Point = $"{cPoint[cPointIndex]}A", Spec = $"±{cSpec}mA", U95Standard = u95_5.ToString("e3") + "A", TUR = tur_5.ToString("f2") });
                    model.TurTables.Add(new TurTable { Number = "6", Point = $"{cPoint[cPointIndex + 1]}A", Spec = $"±{cSpec}mA", U95Standard = u95_6.ToString("e3") + "A", TUR = tur_6.ToString("f2") });

                    #endregion
                }

                #region Uncertainty Budget Table
                #region Voltage
                var vv = 2 * (double)v.FirstOrDefault().Scale / Math.Pow(2, baseInfo.VoltmeterBits);
                var vstd = 2 * (double)v.FirstOrDefault().Scale / (Math.Pow(2, baseInfo.VoltmeterBits) * Math.Sqrt(12));
                var voltageUncertaintyBudgetTable = new UncertaintyBudgetTable();
                voltageUncertaintyBudgetTable.Value = $"{vscale}V";
                voltageUncertaintyBudgetTable.TesterResolutionValue = vv.ToString("e3");
                voltageUncertaintyBudgetTable.TesterResolutionStdUncertainty = vstd.ToString("e3");
                var vmdcv = plcrmd.Where(d => d.CommandedValue.Equals(vscale) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals("Charge") && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
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
                var cmdcv = plcrmd.Where(d => d.CommandedValue.Equals(cscale) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals("Charge") && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
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
                if (turA.Count > 2)
                {
                    var turAOne = turA.GroupBy(t => t.UncertaintyContributors).Select(t => t.First()).ToList();
                    if (turAOne.Count > 2)
                    {
                        turA = turA.OrderBy(t => t.Range).Take(2).ToList();
                    }
                    else
                    {
                        turA = turAOne;
                    }
                }
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
                void CalculateVoltage(string mode, int tableIndex, int DecimalPlace)
                {
                    int j = 0;
                    int l = 1;
                    foreach (var item in plcGroupData)
                    {
                        var data = item.Where(p => p.VoltsorAmps.Equals("Volts") && p.Mode.Equals(mode) && p.VerifyType.Equals("Post-Calibration")).GroupBy(d => d.Channel).ToList();
                        foreach (var item2 in data)
                        {
                            var cvDataList = item2.OrderBy(dd => dd.CommandedValue).ToList();
                            foreach (var cvData in cvDataList)
                            {
                                //var cvCHH = $"{l}-{cvData.Channel}";
                                var cvCHH = $"{item.Key}-{cvData.Channel}";
                                var cvRange = cvData.Scale;
                                var cvIndication = cvData.MeasuredValue;
                                var cvMeasuredValue = cvData.StandardValue;
                                var cvError = (cvIndication - cvMeasuredValue) * 1000;
                                double cvAcceptance = 0;
                                var cvAcceptanceStr = "";
                                var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
                                var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Volts") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration") && d.Scale.Equals(cvData.Scale)).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
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
                                else//默认为0的处理方法
                                {

                                    var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyV * 1000;
                                    cvAcceptance = accpetedTolerance;
                                }
                                //约分
                                //var (IndicationReduce, MeasuredValueReduce, ErrorReduce, AcceptanceReduce, UncertaintyReduce) = ReduceVoltage(cvIndication, cvMeasuredValue, cvError, cvAcceptance, cvUncertainty);

                                var IndicationReduce = cvIndication.ToString($"F{DecimalPlace + 3}");
                                var MeasuredValueReduce = cvMeasuredValue.ToString($"F{DecimalPlace + 3}");
                                var ErrorReduce = (Convert.ToDouble(IndicationReduce) * 1000 - Convert.ToDouble(MeasuredValueReduce) * 1000).ToString($"F{DecimalPlace}");
                                var AcceptanceReduce = cvAcceptance.ToString($"F{DecimalPlace}");
                                var UncertaintyReduce = cvUncertainty.ToString($"F{DecimalPlace}");

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
                                else//默认为0的处理方法
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

                                if (mode.Equals("Charge"))
                                {
                                    model.ChargingVoltage.Add(new DataSheet
                                    {
                                        Sort1 = item.Key,
                                        Sort2 = cvData.Channel,
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
                                        Sort1 = item.Key,
                                        Sort2 = cvData.Channel,
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
                void CalculateCurrent(string mode, int tableIndex, int DecimalPlace, int Cunit)
                {
                    int j = 0;
                    int l = 1;
                    foreach (var item in plcGroupData)
                    {
                        var data = item.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals(mode) && p.VerifyType.Equals("Post-Calibration")).GroupBy(d => d.Channel);
                        foreach (var item2 in data)
                        {
                            var cvDataList = item2.OrderBy(dd => dd.Scale).ThenBy(dd => dd.CommandedValue).ToList();
                            foreach (var cvData in cvDataList)
                            {
                                //var CHH = $"{l}-{cvData.Channel}";
                                var CHH = $"{item.Key}-{cvData.Channel}";
                                var Range = cvData.Scale;
                                var Indication = cvData.MeasuredValue;
                                var MeasuredValue = cvData.StandardValue;
                                var Error = Indication - MeasuredValue;
                                double Acceptance = 0;
                                var AcceptanceStr = "";

                                var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
                                var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration") && d.Scale.Equals(cvData.Scale)).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
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
                                else//默认为0的处理方法
                                {
                                    var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC;
                                    Acceptance = accpetedTolerance;
                                }
                                //约分
                                //var (IndicationReduce, MeasuredValueReduce, ErrorReduce, AcceptanceReduce, UncertaintyReduce) = ReduceCurrent(Math.Abs(Indication), Math.Abs(MeasuredValue), Error, Acceptance, Uncertainty);
                                string IndicationReduce = "", MeasuredValueReduce = "", ErrorReduce = "", AcceptanceReduce = "", UncertaintyReduce = "";

                                IndicationReduce = (Math.Abs(Indication) / Math.Pow(1000, Cunit)).ToString($"F{DecimalPlace + 3}");
                                MeasuredValueReduce = (Math.Abs(MeasuredValue) / Math.Pow(1000, Cunit)).ToString($"F{DecimalPlace + 3}");
                                ErrorReduce = (Convert.ToDouble(IndicationReduce) * 1000 - Convert.ToDouble(MeasuredValueReduce) * 1000).ToString($"F{DecimalPlace}");
                                AcceptanceReduce = (Acceptance / Math.Pow(1000, Cunit - 1)).ToString($"F{DecimalPlace}");
                                UncertaintyReduce = (Uncertainty / Math.Pow(1000, Cunit - 1)).ToString($"F{DecimalPlace}");

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
                                else//默认为0的处理方法
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

                                if (mode.Equals("Charge"))
                                {
                                    model.ChargingCurrent.Add(new DataSheet
                                    {
                                        Sort1 = item.Key,
                                        Sort2 = cvData.Channel,
                                        Channel = CHH,
                                        Range = baseInfo.TesterModel.Contains("mA") ? Range.ToString() : ((decimal)Range / 1000).ToString(),
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
                                        Sort1 = item.Key,
                                        Sort2 = cvData.Channel,
                                        Channel = CHH,
                                        Range = baseInfo.TesterModel.Contains("mA") ? Range.ToString() : ((decimal)Range / 1000).ToString(),
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

                var CategoryObj = await GetCategory(baseInfo.TesterModel);

                #region Charging Voltage
                CalculateVoltage("Charge", 8, int.Parse(CategoryObj.DtValue));
                model.ChargingVoltage = model.ChargingVoltage.OrderBy(s => s.Sort1).ThenBy(s => s.Sort2).ToList();
                #endregion

                #region Discharging Voltage
                CalculateVoltage("DisCharge", 9, int.Parse(CategoryObj.DtValue));
                model.DischargingVoltage = model.DischargingVoltage.OrderBy(s => s.Sort1).ThenBy(s => s.Sort2).ToList();
                #endregion

                #region Charging Current
                CalculateCurrent("Charge", 10, int.Parse(CategoryObj.Description), int.Parse(CategoryObj.DtCode));
                model.ChargingCurrent = model.ChargingCurrent.OrderBy(s => s.Sort1).ThenBy(s => s.Sort2).ToList();
                #endregion

                #region Discharging Current
                CalculateCurrent("DisCharge", 10, int.Parse(CategoryObj.Description), int.Parse(CategoryObj.DtCode));
                model.DischargingCurrent = model.DischargingCurrent.OrderBy(s => s.Sort1).ThenBy(s => s.Sort2).ToList();
                //放电不确定度改为充电不确定度
                for (int i = 0; i < model.DischargingCurrent.Count; i++)
                {
                    model.DischargingCurrent[i].Uncertainty = model.ChargingCurrent[i].Uncertainty;
                }
                #endregion



                #endregion

                #region 签名
                var us = await _userSignApp.GetUserSignList(new QueryUserSignListReq { });
                //if (baseInfo.Operator == "肖淑惠" || baseInfo.Operator == "阙勤勤")
                //{
                //    var name = await UnitWork.Find<Category>(c => c.TypeId == "SYS_CalibrationCertificateSign").Select(c => c.Name).FirstOrDefaultAsync();
                //    baseInfo.Operator = name;
                //}
                var calibrationTechnician = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.Issuer));
                if (calibrationTechnician != null)
                {
                    bool isBool = GetUserSign(baseInfo.Issuer);
                    if (!isBool)
                    {
                        throw new Exception($"当前用户没有签名权限,请联系管理人员");
                    }

                    model.CalibrationTechnician = await GetSignBase64(calibrationTechnician.PictureId);
                }
                var technicalManager = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.TechnicalManager));
                if (technicalManager != null)
                {
                    bool tecBool = GetUserSign(baseInfo.TechnicalManager);
                    if (!tecBool)
                    {
                        throw new Exception($"当前用户没有签名权限,请联系管理人员");
                    }

                    model.TechnicalManager = await GetSignBase64(technicalManager.PictureId);
                }
                var approvalDirector = us.Data.FirstOrDefault(u => u.UserName.Equals(baseInfo.ApprovalDirector));
                if (approvalDirector != null)
                {
                    bool appBool = GetUserSign(baseInfo.ApprovalDirector);
                    if (!appBool)
                    {
                        throw new Exception($"当前用户没有签名权限,请联系管理人员");
                    }

                    model.ApprovalDirector = await GetSignBase64(approvalDirector.PictureId);
                }
                #endregion
            }
            catch (Exception ex)
            {
                _logger.LogError("校准公式：" + ex.Message.ToString());
            }

            return model;
        }

        private async Task<string> GetSignBase64(string fileId)
        {
            var file = await _fileApp.GetFileAsync(fileId);
            if (file != null)
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

        /// <summary>
        /// 判定当前是否存在
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public bool GetUserSign(string userName)
        {
            List<OpenAuth.Repository.Domain.UserSign> userSigns = UnitWork.Find<OpenAuth.Repository.Domain.UserSign>(r => r.UserName == userName).ToList();
            if (userSigns != null && userSigns.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 查询字典
        /// </summary>
        /// <param name="Model"></param>
        /// <returns></returns>
        public async Task<OpenAuth.Repository.Domain.Category> GetCategory(string Model)
        {
            var objs = await UnitWork.Find<OpenAuth.Repository.Domain.Category>(c => Model.Contains(c.Name) && c.TypeId.Equals("SYS_CalibrationCertificateType")).FirstOrDefaultAsync();
            return objs;
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
        /// 生成证书一维码
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public async Task<string> BarcodeGenerate(string data)
        {
            System.Drawing.Font labelFont = new System.Drawing.Font("OCRB", 11f, FontStyle.Bold);//
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
        /// 替换CNAS路径帮助方法
        /// </summary>
        /// <returns></returns>
        public async Task UpdatePath()
        {
            List<string> nums = new List<string>() {"E2212-363132","E2212-363135","E2212-363136",
"E2212-363137","E2212-363139","E2212-363146","E2212-364139","E2212-364140","E2212-364141",
"E2212-364143",
"E2212-364144",
"E2212-364145",
"E2212-364150",
"E2212-364151",
"E2212-364152",
"E2212-364153",
"E2212-364154",
"E2212-364155",
"E2212-364157",
"E2212-364159",
"E2212-364160",
"E2212-364165",
"E2212-364166",
"E2212-364170",
"E2212-364174",
"E2212-364176",
"E2212-364178",
"E2212-364180",
"E2212-364182",
"E2212-364183",
"E2212-364185",
"E2212-364189",
"E2212-364193",
"E2212-364408",
"E2212-364415",
"E2212-364416",
"E2212-364423",
"E2212-364426",
"E2212-364428",
"E2212-364429",
"E2212-364430",
"E2212-364431",
"E2212-364434",
"E2212-364437",
"E2212-364438",
"E2212-364439",
"E2212-364441",
"E2212-364443",
"E2212-364444",
"E2212-364445",
"E2212-364446",
"E2212-364447",
"E2212-364448",
"E2212-364449",
"E2212-364450",
"E2212-364451",
"E2212-364452",
"E2212-364453",
"E2212-364454",
"E2212-364455",
"E2212-364456",
"E2212-364457",
"E2212-364458",
"E2212-364459",
"E2212-364467",
"E2212-364468",
"E2212-364469",
"E2212-364470",
"E2212-364471",
"E2212-364472",
"E2212-364473",
"E2212-364474",
"E2212-365042" };
            foreach (string item in nums)
            {
               List<OpenAuth.Repository.Domain.NwcaliBaseInfo> nwcaliBaseInfos = await UnitWork.Find<OpenAuth.Repository.Domain.NwcaliBaseInfo>(r => r.TesterSn == item && r.CNASPdfPath != null).ToListAsync();
                foreach (OpenAuth.Repository.Domain.NwcaliBaseInfo itemReq in nwcaliBaseInfos)
                {
                    await UnitWork.UpdateAsync<OpenAuth.Repository.Domain.NwcaliBaseInfo>(q => q.Id == itemReq.Id, q => new OpenAuth.Repository.Domain.NwcaliBaseInfo
                    {
                        CNASPdfPath = "https://erp4.obs.cn-south-1.myhuaweicloud.com/nwcail/" + item + ".pdf"
                    }) ;

                    await UnitWork.SaveAsync();
                }
            }

        }

        /// <summary>
        /// 重新生成当前校准证书
        /// </summary>
        /// <param name="certNo">证书编号</param>
        /// <returns>校准证书生成结果</returns>
        public async Task<TableData> UpdateCertInfo(string certNo)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var result = new TableData();
            if (loginContext.Roles.Any(r => r.Name.Equals("校准软件-授权签字人")))
            {
                if (!string.IsNullOrEmpty(certNo))
                {
                    //获取校准证书主表信息
                    OpenAuth.Repository.Domain.NwcaliBaseInfo nwcaliBaseInfo = await UnitWork.Find<OpenAuth.Repository.Domain.NwcaliBaseInfo>(r => r.CertificateNumber == certNo).FirstOrDefaultAsync();
                    if (nwcaliBaseInfo != null)
                    {
                        #region 将文件下载url地址转换为文件流
                        string url = nwcaliBaseInfo.CertPath.Split(",")[1];
                        Stream memoryStream = new MemoryStream();
                        using (var client = new WebClient())
                        {
                            string tempFile = Path.GetTempFileName();
                            client.DownloadFile(url, tempFile);//下载临时文件
                            memoryStream = FileToStream(tempFile, true);
                        }
                        #endregion

                        #region 上传文件流并保存到erp4_settlement数据库
                        var handler = new ExcelHandler(memoryStream);
                        var baseInfo = handler.GetBaseInfo<NwcaliBaseInfo>(sheet =>
                        {
                            var baseInfo = new NwcaliBaseInfo();
                            var timeRow = sheet.GetRow(1);
                            var time1 = timeRow.GetCell(1).StringCellValue;
                            var timeRow2 = sheet.GetRow(2);
                            var time2 = timeRow2.GetCell(1).StringCellValue;
                            var timecomb = DateTime.Parse($"{time1} {time2}");
                            baseInfo.Time = timecomb;
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
                            baseInfo.SiteCode = siteCodeRow.GetCell(1).StringCellValue; //"Electrical Lab";
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
                            var etalonsCalibrationEntity = sheet.GetRow(21);
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
                                        DueDate = etalonsDueDateRow.GetCell(i).StringCellValue,
                                        CalibrationEntity = etalonsCalibrationEntity.GetCell(i).StringCellValue
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
                                        CalibrationDate = baseInfo.Time,
                                        ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time.Value.ToString(), baseInfo.TestInterval))
                                    });
                                }
                                catch
                                {
                                    break;
                                }
                            }
                            //新增字段 
                            var startTime = sheet.GetRow(34);
                            if (startTime != null)
                            {
                                var startTimeValue = startTime.GetCell(1)?.StringCellValue;
                                if (!string.IsNullOrWhiteSpace(startTimeValue))
                                    baseInfo.StartTime = DateTime.Parse(startTimeValue);
                            }
                            var calibrationStatus = sheet.GetRow(35);
                            if (calibrationStatus != null) baseInfo.CalibrationStatus = calibrationStatus.GetCell(1).StringCellValue;
                            var calibrationMode = sheet.GetRow(36);
                            if (calibrationMode != null) baseInfo.CalibrationMode = calibrationMode.GetCell(1).StringCellValue;
                            var toolAssetCode = sheet.GetRow(37);
                            if (toolAssetCode != null) baseInfo.ToolAssetCode = toolAssetCode.GetCell(1).StringCellValue;

                            #endregion
                            return baseInfo;
                        });
                        if (string.IsNullOrWhiteSpace(baseInfo.Operator))
                        {
                            result.Code = 500;
                            result.Message = "Operator can not be null.";
                            return result;
                        }
                        var turV = handler.GetNwcaliTur("电压");
                        var turA = handler.GetNwcaliTur("电流");
                        var tv = turV.Select(v => new Repository.Domain.CapNwcail.NwcaliTur { DataType = 1, Range = v.Range, TestPoint = v.TestPoint, Tur = v.Tur, UncertaintyContributors = v.UncertaintyContributors, SensitivityCoefficient = v.SensitivityCoefficient, Value = v.Value, Unit = v.Unit, Type = v.Type, Distribution = v.Distribution, Divisor = v.Divisor, StdUncertainty = v.StdUncertainty, DegreesOfFreedom = v.DegreesOfFreedom, SignificanceCheck = v.SignificanceCheck }).ToList();
                        var ta = turA.Select(v => new Repository.Domain.CapNwcail.NwcaliTur { DataType = 2, Range = v.Range, TestPoint = v.TestPoint, Tur = v.Tur, UncertaintyContributors = v.UncertaintyContributors, SensitivityCoefficient = v.SensitivityCoefficient, Value = v.Value, Unit = v.Unit, Type = v.Type, Distribution = v.Distribution, Divisor = v.Divisor, StdUncertainty = v.StdUncertainty, DegreesOfFreedom = v.DegreesOfFreedom, SignificanceCheck = v.SignificanceCheck }).ToList();
                        baseInfo.NwcaliTurs.AddRange(tv);
                        baseInfo.NwcaliTurs.AddRange(ta);
                        baseInfo.ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time.ToString(), baseInfo.TestInterval));
                        try
                        {
                            foreach (var plc in baseInfo.PcPlcs)
                            {
                                var list = handler.GetNWCaliPLCData($"下位机{plc.No}");
                                baseInfo.NwcaliPlcDatas.AddRange(list.Select(l => new NwcaliPlcData
                                {
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
                            if (baseInfo.StartTime != null)
                            {
                                var ts = baseInfo.Time.Value.Subtract(baseInfo.StartTime.Value);
                                baseInfo.TotalSeconds = ts.TotalSeconds.ToString();
                            }
                            await AddUpdate(baseInfo, nwcaliBaseInfo);

                        }
                        catch (Exception ex)
                        {
                            result.Code = 500;
                            result.Message = "该校准证书" + certNo + "重新生成失败";
                            return result;
                        }
                        #endregion
                    }
                    else
                    {
                        result.Code = 500;
                        result.Message = "该校准证书" + certNo + "重新生成失败";
                    }
                }
                else
                {
                    result.Code = 500;
                    result.Message = "校准证书编号为空";
                }
            }
            else
            {
                result.Code = 500;
                result.Message = "您不属于授权签字人角色，没有权限";
            }

            return result;
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="baseInfo"></param>
        /// <param name="nwcaliBaseInfo"></param>
        /// <returns></returns>
        public async Task AddUpdate(NwcaliBaseInfo baseInfo, OpenAuth.Repository.Domain.NwcaliBaseInfo nwcaliBaseInfo)
        {
            var loginContext = _auth.GetCurrentUser();
            if (loginContext == null)
            {
                throw new CommonException("登录已过期", Define.INVALID_TOKEN);
            }

            var user = loginContext.User;
            string userId = await UnitWork.Find<OpenAuth.Repository.Domain.User>(r => r.Name == baseInfo.Operator).Select(r => r.Id).FirstOrDefaultAsync();
            baseInfo.OperatorId = userId;
            await semaphoreSlim.WaitAsync();
            try
            {
                baseInfo.CertificateNumber = nwcaliBaseInfo.CertificateNumber;
                baseInfo.CreateTime = nwcaliBaseInfo.CreateTime;
                baseInfo.CreateUser = nwcaliBaseInfo.CreateUser;
                baseInfo.CreateUserId = nwcaliBaseInfo.CreateUserId;
                baseInfo.Issuer = nwcaliBaseInfo.Issuer;
                baseInfo.IssuerId = nwcaliBaseInfo.IssuerId;
                baseInfo.Operator = nwcaliBaseInfo.Operator;
                baseInfo.OperatorId = nwcaliBaseInfo.OperatorId;
                baseInfo.TechnicalManager = nwcaliBaseInfo.TechnicalManager;
                baseInfo.TechnicalManagerId = nwcaliBaseInfo.TechnicalManagerId;
                baseInfo.ApprovalDirector = nwcaliBaseInfo.ApprovalDirector;
                baseInfo.ApprovalDirectorId = nwcaliBaseInfo.ApprovalDirectorId;
                baseInfo.Time = nwcaliBaseInfo.Time;
                baseInfo.CertPath = nwcaliBaseInfo.CertPath;
                baseInfo.FileVersion = nwcaliBaseInfo.FileVersion;
                baseInfo.TesterMake = nwcaliBaseInfo.TesterMake;
                baseInfo.TesterModel = nwcaliBaseInfo.TesterModel;
                baseInfo.TesterSn = nwcaliBaseInfo.TesterSn;
                baseInfo.UpdateTime = nwcaliBaseInfo.UpdateTime;
                var testerModel = await UnitWork.Find<OINS>(o => o.manufSN.Equals(baseInfo.TesterSn)).Select(o => o.itemCode).ToListAsync();
                if (testerModel != null && testerModel.Count == 1 && !testerModel.Contains("ZWJ"))
                {
                    if (testerModel.FirstOrDefault().Contains(baseInfo.TesterModel))
                        baseInfo.TesterModel = testerModel.FirstOrDefault();
                }

                var baseInfos = await UnitWork.Find<NwcaliBaseInfo>(r => r.CertificateNumber == baseInfo.CertificateNumber).ToListAsync();
                if (!(baseInfos != null && baseInfos.Count() > 0))
                {
                    await UnitWork.AddAsync(baseInfo);
                    await UnitWork.SaveAsync();
                }
            }
            finally
            {
                semaphoreSlim.Release();
                await UpdateNwcailFile(baseInfo.CertificateNumber);
            }
        }

        /// <summary>
        /// 修改校准证书
        /// </summary>
        /// <param name="certNo">证书编号</param>
        /// <returns></returns>
        public async Task UpdateNwcailFile(string certNo)
        {
            var loginContext = _auth.GetCurrentUser();
            var user = loginContext.User;
            var baseInfo = await GetInfo(certNo);
            if (baseInfo != null)
            {
                try
                {
                    var folderYear = DateTime.Now.ToString("yyyy");
                    var basePath = Path.Combine("D:\\nsap4file", "nwcail", folderYear, baseInfo.CertificateNumber);
                    var model = await BuildModel(baseInfo);
                    #region 生成英文版
                    var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Header.html");
                    var text = System.IO.File.ReadAllText(url);
                    text = text.Replace("@Model.Data.BarCode", model.BarCode);
                    var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                    System.IO.File.WriteAllText(tempUrl, text);
                    var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Footer.html");
                    var datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate.cshtml", pdf =>
                    {
                        pdf.IsWriteHtml = true;
                        pdf.PaperKind = PaperKind.A4;
                        pdf.Orientation = Orientation.Portrait;
                        pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                        pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 2.812, HtmUrl = footerUrl };
                    });
                    System.IO.File.Delete(tempUrl);
                    Stream stream1 = new MemoryStream(datas);
                    #endregion

                    #region 生成中文版
                    //获取委托单
                    await SetIssuser1(baseInfo);
                    var calibrationTechnician = await UnitWork.Find<OpenAuth.Repository.Domain.UserSign>(r => r.UserName.Equals(baseInfo.Issuer)).FirstOrDefaultAsync();
                    if (calibrationTechnician != null)
                    {
                        model.CalibrationTechnician = await GetSignBase64(calibrationTechnician.PictureId);
                    }

                    var entrustment = await GetEntrustment(model.CalibrationCertificate.TesterSn);
                    model.CalibrationCertificate.EntrustedUnit = entrustment?.CertUnit;
                    model.CalibrationCertificate.EntrustedUnitAdress = entrustment?.CertCountry + entrustment?.CertProvince + entrustment?.CertCity + entrustment?.CertAddress;
                    //委托日期需小于校准日期
                    if (entrustment != null && !string.IsNullOrWhiteSpace(entrustment.EntrustedDate.ToString()) && entrustment?.EntrustedDate > DateTime.Parse(model.CalibrationCertificate.CalibrationDate))
                        entrustment.EntrustedDate = (DateTime.Parse(model.CalibrationCertificate.CalibrationDate)).AddDays(-2);

                    model.CalibrationCertificate.EntrustedDate = !string.IsNullOrWhiteSpace(entrustment?.EntrustedDate.ToString()) ? entrustment?.EntrustedDate.Value.ToString("yyyy年MM月dd日") : "";
                    model.CalibrationCertificate.CalibrationDate = DateTime.Parse(model.CalibrationCertificate.CalibrationDate).ToString("yyyy年MM月dd日");
                    var temp = Math.Round(Convert.ToDecimal(model.CalibrationCertificate.Temperature), 1);
                    model.CalibrationCertificate.Temperature = temp.ToString("0.0");
                    foreach (var item in model.MainStandardsUsed)
                    {
                        if (!string.IsNullOrWhiteSpace(item.DueDate))
                            item.DueDate = DateTime.Parse(item.DueDate).ToString("yyyy-MM-dd");
                        if (item.Name.Contains(","))
                        {
                            var split = item.Name.Split(",");
                            //item.EnName = split[0];
                            item.Name = split[0];
                        }
                        item.Characterisics = item.Characterisics.Replace("Urel", "<i>U</i><sub>rel</sub>").Replace("k=", "<i>k</i>=");
                    }

                    url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Header.html");
                    text = System.IO.File.ReadAllText(url);
                    text = text.Replace("@Model.Data.BarCode", model.BarCode);
                    tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                    System.IO.File.WriteAllText(tempUrl, text);
                    footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Footer.html");
                    datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate CNAS.cshtml", pdf =>
                    {
                        pdf.IsWriteHtml = true;
                        pdf.PaperKind = PaperKind.A4;
                        pdf.Orientation = Orientation.Portrait;
                        pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };//2.812
                        pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 0, HtmUrl = footerUrl };
                    });
                    System.IO.File.Delete(tempUrl);
                    var fullPathCnas = Path.Combine(basePath, $"{certNo}_CNAS" + ".pdf");
                    Stream stream2 = new MemoryStream(datas);
                    #endregion

                    await semaphoreSlim.WaitAsync();

                    //上传华为云
                    var fileResp = await _fileApp.UploadFileToHuaweiOBS($"nwcail/{baseInfo.CertificateNumber}/{baseInfo.CertificateNumber}_EN.pdf", null, stream1);
                    var fileRespCn = await _fileApp.UploadFileToHuaweiOBS($"nwcail/{baseInfo.CertificateNumber}/{baseInfo.CertificateNumber}_CNAS.pdf", null, stream2);
                    await UnitWork.UpdateAsync<Repository.Domain.NwcaliBaseInfo>(b => b.CertificateNumber == certNo, o => new Repository.Domain.NwcaliBaseInfo { PdfPath = fileResp.FilePath, CNASPdfPath = fileRespCn.FilePath, UpdateTime = DateTime.Now, UpdateUser = user.Name, UpdateUserId = user.Id });
                    await UnitWork.SaveAsync();
                    semaphoreSlim.Release();
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// 文件转文件流
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="isDelete"></param>
        /// <returns></returns>
        public Stream FileToStream(string fileName, bool isDelete = false)
        {
            //打开文件
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

            // 读取文件的 byte[]
            byte[] bytes = new byte[fileStream.Length];
            fileStream.Read(bytes, 0, bytes.Length);
            fileStream.Close();

            // 把 byte[] 转换成 Stream
            Stream stream = new MemoryStream(bytes);
            if (isDelete)
            {
                File.Delete(fileName);//删除临时文件
            }

            return stream;
        }
    }

    public class CertHelp:NwcaliBaseInfo
    {
        public DateTime? EntrustedDate { get; set; }
    }
}
