using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DinkToPdf;
using Infrastructure;
using Infrastructure.Excel;
using Infrastructure.Export;
using Infrastructure.Wrod;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali;
using OpenAuth.App.Nwcali.Models;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain;
using OpenAuth.WebApi.Model;
using Serilog;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenAuth.WebApi.Controllers
{
    /// <summary>
    /// 校准证书
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    [ApiExplorerSettings(GroupName = "Nwcali")]
    public class CertController : Controller
    {
        private readonly IAuth _authUtil;
        private readonly CertinfoApp _certinfoApp;
        private readonly CertPlcApp _certPlcApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly NwcaliCertApp _nwcaliCertApp;
        private readonly UserSignApp _userSignApp;
        private readonly FileApp _fileApp;
        private readonly DevInfoApp _devInfoApp;
        private readonly MachineInfoApp _machineInfoApp;
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

        public CertController(CertinfoApp certinfoApp, CertPlcApp certPlcApp, ModuleFlowSchemeApp moduleFlowSchemeApp, FlowInstanceApp flowInstanceApp, NwcaliCertApp nwcaliCertApp, UserSignApp userSignApp, FileApp fileApp, DevInfoApp devInfoApp, MachineInfoApp machineInfoApp, IAuth authUtil)
        {
            _certinfoApp = certinfoApp;
            _certPlcApp = certPlcApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _flowInstanceApp = flowInstanceApp;
            _nwcaliCertApp = nwcaliCertApp;
            _userSignApp = userSignApp;
            _fileApp = fileApp;
            _authUtil = authUtil;
            _devInfoApp = devInfoApp;
            _machineInfoApp = machineInfoApp;
        }

        [HttpPost]
        public async Task<Response<bool>> Generate(string loginfo = "")
        {


            var loginContext = _authUtil.GetCurrentUser();
            var loginOrg = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault();
            Log.Logger.Information($"校准证书上传，阶段：{loginOrg.Name}，参数loginfo：{loginfo}。");

            if (loginOrg.Name == "T1")//校准部门 上传校准数据 走校准流程生成证书
            {
                var file = Request.Form.Files[0];
                var handler = new ExcelHandler(file.OpenReadStream());
                var baseInfo = handler.GetBaseInfo<NwcaliBaseInfo>(sheet =>
                {
                    var baseInfo = new NwcaliBaseInfo();
                    var timeRow = sheet.GetRow(1);
                    var time1 = timeRow.GetCell(1).StringCellValue;
                    var timeRow2 = sheet.GetRow(2);
                    var time2 = timeRow2.GetCell(1).StringCellValue;
                    var timecomb = DateTime.Parse($"{time1} {time2}");
                    //var timeRow = sheet.GetRow(1);
                    //baseInfo.Time = DateTime.Parse(timeRow.GetCell(1).StringCellValue);
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
                    return new Response<bool>()
                    {
                        Code = 400,
                        Message = "Operator can not be null.",
                        Result = false
                    };
                }
                var turV = handler.GetNwcaliTur("电压");
                var turA = handler.GetNwcaliTur("电流");
                var tv = turV.Select(v => new Repository.Domain.NwcaliTur { DataType = 1, Range = v.Range, TestPoint = v.TestPoint, Tur = v.Tur, UncertaintyContributors = v.UncertaintyContributors, SensitivityCoefficient = v.SensitivityCoefficient, Value = v.Value, Unit = v.Unit, Type = v.Type, Distribution = v.Distribution, Divisor = v.Divisor, StdUncertainty = v.StdUncertainty, DegreesOfFreedom = v.DegreesOfFreedom, SignificanceCheck = v.SignificanceCheck }).ToList();
                var ta = turA.Select(v => new Repository.Domain.NwcaliTur { DataType = 2, Range = v.Range, TestPoint = v.TestPoint, Tur = v.Tur, UncertaintyContributors = v.UncertaintyContributors, SensitivityCoefficient = v.SensitivityCoefficient, Value = v.Value, Unit = v.Unit, Type = v.Type, Distribution = v.Distribution, Divisor = v.Divisor, StdUncertainty = v.StdUncertainty, DegreesOfFreedom = v.DegreesOfFreedom, SignificanceCheck = v.SignificanceCheck }).ToList();
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
                    baseInfo.FlowInstanceId = await CreateFlow(baseInfo.CertificateNumber);
                    await _nwcaliCertApp.AddAsync(baseInfo);
                    //保存文件
                    //var folderYear = DateTime.Now.ToString("yyyy");
                    var fileExtension = Path.GetExtension(file.FileName);
                    //var basePath = Path.Combine("D:\\nsap4file", "nwcail", folderYear, baseInfo.CertificateNumber);
                    //var savePath = Path.Combine(basePath, $"{baseInfo.CertificateNumber}{fileExtension}");
                    //DirUtil.CheckOrCreateDir(basePath);
                    //using (var fs = new FileStream(savePath, FileMode.Create))
                    //{
                    //    file.CopyTo(fs);
                    //    fs.Flush();
                    //}
                    var fileResp = await _fileApp.UploadFileToHuaweiOBS($"nwcail/{baseInfo.CertificateNumber}/{baseInfo.CertificateNumber}{fileExtension}", file);
                    var savePath = $"{fileResp.FileName},{fileResp.FilePath}";
                    await _nwcaliCertApp.UpdateFilePath(baseInfo.CertificateNumber, savePath);
                    return new Response<bool>()
                    {
                        Result = true
                    };
                }
                catch (Exception ex)
                {
                    await _flowInstanceApp.DeleteAsync(f => f.Id.Equals(baseInfo.FlowInstanceId));
                    return new Response<bool>()
                    {
                        Code = 500,
                        Message = ex.Message,
                        Result = false
                    };
                }
            }
            else //生产阶段的校准数据
            {
                try
                {
                    var files = Request.Form.Files;
                    var file = files[0];
                    //保存文件
                    var fileResp = await _fileApp.UploadFileToHuaweiOBS($"nwcail/machine/{file.FileName}", file);
                    //var fileResp = await _fileApp.Add(files, "machine");
                    //读取文件
                    var handler = new ExcelHandler(file.OpenReadStream());

                    var baseInfo = handler.GetBaseInfo<ProduceNwcaliBaseInfo>(sheet =>
                    {
                        var baseInfo = new ProduceNwcaliBaseInfo();
                        var timeRow = sheet.GetRow(1);
                        var time1 = timeRow.GetCell(1).StringCellValue;
                        var timeRow2 = sheet.GetRow(2);
                        var time2 = timeRow2.GetCell(1).StringCellValue;
                        var timecomb = DateTime.Parse($"{time1} {time2}");
                        //var timeRow = sheet.GetRow(1);
                        //baseInfo.Time = DateTime.Parse(timeRow.GetCell(1).StringCellValue);
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
                                baseInfo.ProduceEtalon.Add(new ProduceEtalon
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
                        //var pclCommentRow = sheet.GetRow(31);
                        //var pclNoRow = sheet.GetRow(32);
                        //var pclGuidRow = sheet.GetRow(33);
                        //for (int i = 1; i < pclNoRow.LastCellNum; i++)
                        //{
                        //    if (string.IsNullOrWhiteSpace(pclGuidRow.GetCell(i)?.StringCellValue))
                        //        continue;
                        //    try
                        //    {
                        //        baseInfo.PcPlcs.Add(new PcPlc
                        //        {
                        //            Comment = pclCommentRow.GetCell(i).StringCellValue,
                        //            No = Convert.ToInt32(pclNoRow.GetCell(i).StringCellValue),
                        //            Guid = pclGuidRow.GetCell(i).StringCellValue,
                        //            CalibrationDate = baseInfo.Time,
                        //            ExpirationDate = DateTime.Parse(ConvertTestInterval(baseInfo.Time.Value.ToString(), baseInfo.TestInterval))
                        //        });
                        //    }
                        //    catch
                        //    {
                        //        break;
                        //    }
                        //}
                        #endregion
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

                        return baseInfo;
                    });
                    if (baseInfo.StartTime != null)
                    {
                        var ts = baseInfo.Time.Value.Subtract(baseInfo.StartTime.Value);
                        baseInfo.TotalSeconds = ts.TotalSeconds.ToString();
                    }
                    await _nwcaliCertApp.AddProduceNwcaliBaseInfo(baseInfo);

                    if (string.IsNullOrWhiteSpace(baseInfo.CalibrationStatus) || baseInfo.CalibrationStatus == "OK")
                    {
                        var sheet = handler.GetSheet();
                        var timeRow = sheet.GetRow(1);
                        var time1 = timeRow.GetCell(1).StringCellValue;
                        var timeRow2 = sheet.GetRow(2);
                        var time2 = timeRow2.GetCell(1).StringCellValue;
                        var timecomb = DateTime.Parse($"{time1} {time2}");
                        var testerSnRow = sheet.GetRow(6);
                        var orderno = testerSnRow.GetCell(1).StringCellValue;
                        var pclNoRow = sheet.GetRow(32);
                        var pclGuidRow = sheet.GetRow(33);
                        List<MachineInfo> machineInfo = new List<MachineInfo>();
                        for (int i = 1; i < pclNoRow.LastCellNum; i++)
                        {
                            if (string.IsNullOrWhiteSpace(pclGuidRow.GetCell(i)?.StringCellValue))
                                continue;

                            machineInfo.Add(new MachineInfo
                            {
                                Guid = pclGuidRow.GetCell(i).StringCellValue,
                                OrderNo = orderno,
                                Status = 1,
                                CreateTime = DateTime.Now,
                                FileId = fileResp.FilePath,
                                CalibrationTime = timecomb,
                                CreateUser = loginContext.User.Name,
                                CreateUserId = loginContext.User.Id
                            });
                        }
                        //var guid = machineInfo.Select(c => c.Guid).ToList();
                        //var orderNo = await _devInfoApp.GetDevInfoByGuid(guid);
                        //machineInfo.ForEach(c =>
                        //{
                        //    var no = orderNo.Where(o => o.low_guid == c.Guid).FirstOrDefault();
                        //    c.OrderNo = no?.order_no;
                        //});
                        await _machineInfoApp.BatchAddAsycn(machineInfo);
                    }
                    return new Response<bool>()
                    {
                        Result = true
                    };
                }
                catch (Exception ex)
                {
                    return new Response<bool>()
                    {
                        Code = 500,
                        Message = ex.Message,
                        Result = false
                    };
                }
            }
        }

        [HttpGet]
        public async Task UpdateTesterModel()
        {
            await _nwcaliCertApp.UpdateTesterModel();
        }

        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<Response<NwcaliBaseInfo>> GetBaseInfo(string serialNumber, string sign, string timespan)
        {
            var result = new Response<NwcaliBaseInfo>();
            var info = await _nwcaliCertApp.GetInfo(serialNumber);
            result.Result = info;
            return result;
        }

        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> DownloadBaseInfo(string serialNumber, string sign, string timespan)
        {
            var cert = await _certinfoApp.GetAsync(c => c.CertNo.Equals(serialNumber));
            if (cert is null)
                return new NotFoundResult();
            var fileStream = new FileStream(cert.BaseInfoPath, FileMode.Open);
            return File(fileStream, "application/vnd.ms-excel");
        }

        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> DownloadCert(string serialNumber, string sign, string timespan)
        {
            var cert = await _certinfoApp.GetAsync(c => c.CertNo.Equals(serialNumber));
            if (cert is null)
                return new NotFoundResult();
            var fileStream = new FileStream(cert.CertPath, FileMode.Open);
            return File(fileStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document");
        }

        /// <summary>
        /// 获取签名
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetSign(string serialNumber, string timespan)
        {
            var result = new TableData();
            try
            {
                Dictionary<string, string> keyValues = new Dictionary<string, string>();
                keyValues.Add("SerialNumber", serialNumber);
                keyValues.Add("TimeSpan", timespan);
                var loginInfo = _authUtil.GetLoginInfo();
                string appKey = loginInfo.Token;
                keyValues.Add("Token", appKey);
                result.Data = SignHelper.Sign(keyValues);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{serialNumber},{timespan}, 错误：{ex.Message}");
            }
            //获取签名进行校验
            return result;
        }

        /// <summary>
        /// 批量下载证书--老版
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> BatchDownloadCertPdf2(string serialNumber, string sign, string timespan)
        {
            //System.IO.Compression.ZipFile.
            //if (System.IO.File.Exists(Directory.GetCurrentDirectory() + "/wwwroot/ziliao.zip"))
            //{
            //    System.IO.File.Delete(Directory.GetCurrentDirectory() + "/wwwroot/ziliao.zip");
            //}
            //准备用来存放下载的多个文件流目录
            var time = DateTime.Now.ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo);
            string pathZip = Directory.GetCurrentDirectory() + "/wwwroot/downfile/downfile-" + time + "/";

            var num = serialNumber.Split(',').ToList();
            //var bases = await _nwcaliCertApp.GetInfoList(num);
            var bases = await _nwcaliCertApp.GetNwcaliList(num);
            foreach (var baseInfo in bases)
            {
                //var baseInfo = await _nwcaliCertApp.GetInfo(item);
                if (baseInfo != null)
                {
                    if (!string.IsNullOrWhiteSpace(baseInfo.PdfPath))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(pathZip);
                        string filename = Path.GetFileName(baseInfo.PdfPath);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        System.IO.File.Copy(baseInfo.PdfPath, directoryInfo.FullName + @"\" + filename, true);
                        continue;
                    }

                    var model = await _certinfoApp.BuildModel(baseInfo);
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
                    Bytes2File(datas, pathZip, baseInfo.CertificateNumber + ".pdf");
                    //Stream ms = new MemoryStream(datas);
                    //var fils = File(ms, "application/pdf", baseInfo.CertificateNumber + ".pdf");
                }
            }
            var certs = await _certinfoApp.GetAllAsync(c => num.Contains(c.CertNo));
            if (certs.Count > 0)
            {
                foreach (var cert in certs)
                {
                    if (!string.IsNullOrWhiteSpace(cert.PdfPath) && System.IO.File.Exists(cert.PdfPath))
                    {
                        DirectoryInfo directoryInfo = new DirectoryInfo(pathZip);
                        string filename = Path.GetFileName(cert.PdfPath);
                        if (!directoryInfo.Exists)
                        {
                            directoryInfo.Create();
                        }
                        System.IO.File.Copy(cert.PdfPath, directoryInfo.FullName + @"\" + filename, true);
                    }
                    var pdfPath = WordHandler.DocConvertToPdf(cert.CertPath);
                    if (!pdfPath.Equals("false"))
                    {
                        cert.PdfPath = pdfPath;
                        await _certinfoApp.UpdateAsync(cert.MapTo<AddOrUpdateCertinfoReq>());

                        DirectoryInfo directory = new DirectoryInfo(pathZip);
                        string filename = Path.GetFileName(pdfPath);
                        if (!directory.Exists)
                        {
                            directory.Create();
                        }
                        System.IO.File.Copy(pdfPath, directory.FullName + @"\" + filename, true);
                    }
                }
            }
            var outname = $"/wwwroot/downfile/ziliao{time}.zip";
            System.IO.Compression.ZipFile.CreateFromDirectory(pathZip, Directory.GetCurrentDirectory() + outname);
            //存在即删除
            if (Directory.Exists(pathZip))
            {
                Directory.Delete(pathZip, true);
            }
            var stream = new FileStream(Directory.GetCurrentDirectory() + outname, FileMode.Open);
            //stream.Dispose();
            //System.IO.File.Delete(Directory.GetCurrentDirectory() + outname);
            return File(stream, "application/octet-stream", "ziliao.zip");
            //Stream stream = new MemoryStream(datas);
            //stream.Flush();
            //stream.Position = 0;
            ////return File(stream, "application/pdf");
            ////return File(stream, "application/pdf");
            //ZipHelper zipHelper = new ZipHelper();
            //return File(stream, "application/octet-stream ; Charset=UTF8", System.Web.HttpUtility.UrlEncode("123.pdf", System.Text.Encoding.UTF8));
        }

        /// <summary>
        /// 批量下载证书
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> BatchDownloadCertPdf(string serialNumber, string sign, string timespan)
        {
            var num = serialNumber.Split(',').ToList();
            var bases = await _nwcaliCertApp.GetNwcaliList(num);
            //远程下载多个文件的地址
            List<string> filePaths = bases.Select(a => a.PdfPath).ToList();
            //准备用来存放下载的多个文件流目录
            string pathZip = Directory.GetCurrentDirectory() + "/wwwroot/downfile" + timespan + "/";

            if (!Directory.Exists(pathZip))
            {
                Directory.CreateDirectory(pathZip);
            }
            foreach (var item in bases)
            {
                if (string.IsNullOrEmpty(item.PdfPath))
                {
                    continue;
                }
                string name = item.TesterSn + "-" + item.PdfPath.Split('/').Last();
                string path = item.PdfPath;
                HttpClient client = new HttpClient();
                client.BaseAddress = new Uri(path);
                //根据文件信息中的文件地址获取远程服务器，返回文件流
                var stream = await client.GetStreamAsync(path);

                var fils = File(stream, "application/vnd.android.package-archive", Path.GetFileName(path));
                //创建文件流(文件路径，文件操作.创建)
                using (FileStream fs = new FileStream(pathZip + "/" + name, FileMode.Create))
                {
                    //复制文件流
                    fils.FileStream.CopyTo(fs);
                }
            }
            //for (int i = 0; i < filePaths.Count; i++)
            //{
            //    if (string.IsNullOrEmpty(filePaths[i]))
            //    {
            //        continue;
            //    }
            //    string name = filePaths[i].Split('/').Last() ;
            //    string path = filePaths[i];
            //    HttpClient client = new HttpClient();
            //    client.BaseAddress = new Uri(path);
            //    //根据文件信息中的文件地址获取远程服务器，返回文件流
            //    var stream = await client.GetStreamAsync(path);

            //    var fils = File(stream, "application/vnd.android.package-archive", Path.GetFileName(path));
            //    //创建文件流(文件路径，文件操作.创建)
            //    using (FileStream fs = new FileStream(pathZip + "/" + name, FileMode.Create))
            //    {
            //        //复制文件流
            //        fils.FileStream.CopyTo(fs);
            //    }
            //}
            //对多个文件流所在的目录进行压缩
            string pathRes = Directory.GetCurrentDirectory() + "/wwwroot/" + "CertifiCate" + timespan + ".zip";
            ZipFile.CreateFromDirectory(pathZip, pathRes);
            //删除目录以及目录下的子文件
            //存在即删除
            if (Directory.Exists(pathZip))
            {
                Directory.Delete(pathZip, true);
            }
            var file = new FileStream(pathRes, FileMode.Open);
            return File(file, "application/octet-stream", "CertifiCate" + timespan + ".zip");
        }
        /// <summary>
        /// 将byte数组转换为文件并保存到指定地址
        /// </summary>
        /// <param name="buff">byte数组</param>
        /// <param name="savepath">保存地址</param>
        public static void Bytes2File(byte[] buff, string savepath, string name)
        {
            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }

            FileStream fs = new FileStream(savepath + name, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            bw.Write(buff, 0, buff.Length);
            bw.Close();
            fs.Close();
        }

        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> DownloadCertPdf(string serialNumber, string sign, string timespan)
        {
            var baseInfo = await _nwcaliCertApp.GetInfo(serialNumber);
            if (baseInfo != null)
            {
                if (!string.IsNullOrWhiteSpace(baseInfo.PdfPath))
                {
                    System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(baseInfo.PdfPath);
                    System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    //var filestream = new FileStream(baseInfo.CNASPdfPath, FileMode.Open);
                    return File(responseStream, "application/pdf", baseInfo.TesterSn + "-" + baseInfo.PdfPath.Split('/').Last());
                }
                var model = await _certinfoApp.BuildModel(baseInfo);
                foreach (var item in model.MainStandardsUsed)
                {
                    if (item.Name.Contains(","))
                    {
                        var split = item.Name.Split(",");
                        item.Name = split[1];
                    }
                }
                var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Header.html");
                var text = System.IO.File.ReadAllText(url);
                text = text.Replace("@Model.Data.BarCode", model.BarCode);
                var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                System.IO.File.WriteAllText(tempUrl, text);
                var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Footer.html");
                var datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate.cshtml", pdf =>
                {
                    //pdf.Name =  baseInfo.TesterSn + "-" + baseInfo.
                    pdf.IsWriteHtml = true;
                    pdf.PaperKind = PaperKind.A4;
                    pdf.Orientation = Orientation.Portrait;
                    pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                    pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 2.812, HtmUrl = footerUrl };
                });
                System.IO.File.Delete(tempUrl);
                return File(datas, "application/pdf");
            }
            else
            {
                var cert = await _certinfoApp.GetAsync(c => c.CertNo.Equals(serialNumber));
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
            }
            return new NotFoundResult();
        }

        /// <summary>
        /// CNAS证书下载
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <param name="sign"></param>
        /// <param name="timespan"></param>
        /// <returns></returns>
        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> DownloadCNASCertPdf(string serialNumber, string sign, string timespan)
        {
            var baseInfo = await _nwcaliCertApp.GetInfo(serialNumber);
            if (baseInfo != null)
            {
                await _nwcaliCertApp.SetIssuser(baseInfo);
                if (!string.IsNullOrWhiteSpace(baseInfo.CNASPdfPath))
                {
                    System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(baseInfo.CNASPdfPath);
                    System.Net.HttpWebResponse response = (System.Net.HttpWebResponse)request.GetResponse();
                    Stream responseStream = response.GetResponseStream();
                    //var filestream = new FileStream(baseInfo.CNASPdfPath, FileMode.Open);
                    return File(responseStream, "application/pdf");
                }
                var model = await _certinfoApp.BuildModel(baseInfo, "cnas");
                //获取委托单
                var entrustment = await _certinfoApp.GetEntrustment(model.CalibrationCertificate.TesterSn);
                model.CalibrationCertificate.EntrustedUnit = entrustment?.CertUnit;
                model.CalibrationCertificate.EntrustedUnitAdress = entrustment?.CertCountry + entrustment?.CertProvince + entrustment?.CertCity + entrustment?.CertAddress;
                //委托日期需小于校准日期
                if (entrustment != null && !string.IsNullOrWhiteSpace(entrustment.EntrustedDate.ToString()) && entrustment?.EntrustedDate > DateTime.Parse(model.CalibrationCertificate.CalibrationDate))
                    entrustment.EntrustedDate = entrustment.EntrustedDate.Value.AddDays(-2);

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

                var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Header.html");
                var text = System.IO.File.ReadAllText(url);
                text = text.Replace("@Model.Data.BarCode", model.BarCode);
                var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                System.IO.File.WriteAllText(tempUrl, text);
                var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Footer.html");
                var datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate CNAS.cshtml", pdf =>
                {
                    pdf.IsWriteHtml = true;
                    pdf.PaperKind = PaperKind.A4;
                    pdf.Orientation = Orientation.Portrait;
                    pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };//2.812
                    pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 0, HtmUrl = footerUrl };
                });
                System.IO.File.Delete(tempUrl);
                return File(datas, "application/pdf");
            }
            return new NotFoundResult();
        }

        [HttpGet]
        public async Task TestDownload(string serialNumber1, string serialNumber2)
        {
            int a = Convert.ToInt32(serialNumber1);
            int b = Convert.ToInt32(serialNumber2);
            for (int i = a; i <= b; i++)
            {
                var temp = $"T2112-{i}";

                var baseInfo = await _nwcaliCertApp.GetInfoBySn(temp);
                if (baseInfo != null)
                {
                    var model = await BuildModel(baseInfo);
                    //获取委托单
                    var entrustment = await _certinfoApp.GetEntrustment(model.CalibrationCertificate.TesterSn);
                    model.CalibrationCertificate.EntrustedUnit = entrustment?.CertUnit;
                    model.CalibrationCertificate.EntrustedUnitAdress = entrustment?.CertCountry + entrustment?.CertProvince + entrustment?.CertCity + entrustment?.CertAddress;
                    model.CalibrationCertificate.EntrustedDate = !string.IsNullOrWhiteSpace(entrustment?.EntrustedDate.ToString()) ? entrustment?.EntrustedDate.Value.ToString("yyyy年MM月dd日") : "";
                    model.CalibrationCertificate.CalibrationDate = DateTime.Parse(model.CalibrationCertificate.CalibrationDate).ToString("yyyy年MM月dd日");
                    foreach (var item in model.MainStandardsUsed)
                    {
                        if (!string.IsNullOrWhiteSpace(item.DueDate))
                            item.DueDate = DateTime.Parse(item.DueDate).ToString("yyyy-MM-dd");
                        if (item.Name.Contains(","))
                        {
                            var split = item.Name.Split(",");
                            item.EnName = split[0];
                            item.Name = split[1];
                        }
                        item.Characterisics = item.Characterisics.Replace("Urel", "<i>U</i><sub>rel</sub>").Replace("k=", "<i>k</i>=");
                    }

                    var url = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Header.html");
                    var text = System.IO.File.ReadAllText(url);
                    text = text.Replace("@Model.Data.BarCode", model.BarCode);
                    var tempUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", $"Header{Guid.NewGuid()}.html");
                    System.IO.File.WriteAllText(tempUrl, text);
                    var footerUrl = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "CNAS Footer.html");
                    var datas = await ExportAllHandler.Exporterpdf(model, "Calibration Certificate CNAS.cshtml", pdf =>
                    {
                        pdf.IsWriteHtml = true;
                        pdf.PaperKind = PaperKind.A4;
                        pdf.Orientation = Orientation.Portrait;
                        pdf.HeaderSettings = new HeaderSettings() { HtmUrl = tempUrl };
                        pdf.FooterSettings = new FooterSettings() { FontSize = 6, Right = "Page [page] of [toPage] ", Line = false, Spacing = 2.812, HtmUrl = footerUrl };
                    });
                    System.IO.File.Delete(tempUrl);
                    Bytes2File(datas, $"D:\\钉钉下载\\校准证书\\T2112-{serialNumber1}-{serialNumber2}\\", baseInfo.CertificateNumber + ".pdf");
                }
            }
        }

        [ServiceFilter(typeof(CertAuthFilter))]
        [HttpGet]
        public async Task<IActionResult> GetCertNoList(string serialNumber, string sign, string timespan)
        {
            var certNos = (await _certPlcApp.GetAllAsync(p => p.PlcGuid.Equals(serialNumber))).OrderByDescending(c => c.CertNo).Select(cp => new { cp.CertNo, cp.CalibrationDate, cp.ExpirationDate });
            if (certNos is null || certNos.Count() == 0)
            {
                var data = await _nwcaliCertApp.GetPcPlcs(serialNumber);
                return Ok(data);
            }
            return Ok(certNos);
        }

        /// <summary>
        /// APP批量获取证书
        /// </summary>
        /// <param name="serialNumber"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetCertNoByMultiNum(List<string> serialNumber)
        {
            TableData res = new TableData();
            res = await _certinfoApp.GetCertNoByMultiNum(serialNumber);
            return res;
        }
        /// <summary>
        /// 构建证书模板参数
        /// </summary>
        /// <param name="baseInfo">基础信息</param>
        /// <param name="turV">Tur电压数据</param>
        /// <param name="turA">Tur电流数据</param>
        /// <returns></returns>
        private async Task<CertModel> BuildModel(NwcaliBaseInfo baseInfo, string type = "")
        {
            var list = new List<WordModel>();
            var model = new CertModel();
            var plcData = baseInfo.NwcaliPlcDatas.Where(d => d.DataType == 1).ToList();
            var plcRepetitiveMeasurementData = baseInfo.NwcaliPlcDatas.Where(d => d.DataType == 2).ToList();
            var turV = baseInfo.NwcaliTurs.Where(d => d.DataType == 1).ToList();
            var turA = baseInfo.NwcaliTurs.Where(d => d.DataType == 2).ToList();
            #region 页眉
            var barcode = await _certinfoApp.BarcodeGenerate(baseInfo.CertificateNumber);
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
                    CertificateNo = baseInfo.Etalons[i].CertificateNo,
                    DueDate = DateStringConverter(baseInfo.Etalons[i].DueDate),
                    CalibrationEntity = baseInfo.Etalons[i].CalibrationEntity
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
                            var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
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
                                    Range = baseInfo.TesterModel.Contains("mA") ? Range.ToString() : ((double)Range / 1000).ToString(),
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
                                    Range = baseInfo.TesterModel.Contains("mA") ? Range.ToString() : ((double)Range / 1000).ToString(),
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

            var CategoryObj = await _certinfoApp.GetCategory(baseInfo.TesterModel);

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
            #endregion



            #endregion

            #region 签名
            var us = await _userSignApp.GetUserSignList(new QueryUserSignListReq { });
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
            if (approvalDirector != null)
            {
                model.ApprovalDirector = await GetSignBase64(approvalDirector.PictureId);
            }
            #endregion
            return model;
        }

        /// <summary>
        /// 获取设备类型
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetMaterialCode(QueryMaterialCodeReq req)
        {
            var result = new TableData();
            try
            {
                return await _certinfoApp.GetMaterialCode(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 获取证书信息
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<TableData> GetCertificate(QueryMaterialCodeReq req)
        {
            var result = new TableData();
            try
            {
                return await _certinfoApp.GetCertificate(req);
            }
            catch (Exception e)
            {
                result.Code = 500;
                result.Message = e.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
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

        ///// <summary>
        ///// 生成证书一维码
        ///// </summary>
        ///// <param name="data"></param>
        ///// <returns></returns>
        //private static async Task<string> BarcodeGenerate(string data)
        //{
        //    System.Drawing.Font labelFont = new System.Drawing.Font("OCRB", 11f, FontStyle.Bold, FontStyle.Bold);//
        //    BarcodeLib.Barcode b = new BarcodeLib.Barcode
        //    {
        //        IncludeLabel = true,
        //        LabelFont = labelFont
        //    };
        //    Image img = b.Encode(BarcodeLib.TYPE.CODE128, data, Color.Black, Color.White, 131, 50);

        //    DirUtil.CheckOrCreateDir(Path.Combine(BaseCertDir, data));
        //    using (var stream = new MemoryStream())
        //    {
        //        img.Save(stream, ImageFormat.Png);
        //        var bytes = new byte[stream.Length];
        //        stream.Position = 0;
        //        await stream.ReadAsync(bytes, 0, bytes.Length);
        //        var base64str = Convert.ToBase64String(bytes);
        //        return base64str;
        //    }
        //    return "";
        //}

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

        #region
        /// <summary>
        /// 电流mA单位约分
        /// </summary>
        /// <param name="indication"></param>
        /// <param name="measuredValue"></param>
        /// <param name="error"></param>
        /// <param name="acceptance"></param>
        /// <param name="uncertainty"></param>
        /// <returns></returns>
        //private static (string, string, string, string, string) ReduceCurrentmA(double indication, double measuredValue, double error, double acceptance, double uncertainty)
        //{
        //    var istr = indication.ToString("f3").Split('.')[1];
        //    istr = (indication / 1000).ToString($"f{istr.Length + 3}").Split('.')[1];
        //    var spMstr = measuredValue.ToString().Split('.');
        //    string mstr;
        //    if (spMstr.Count() == 1)
        //    {
        //        mstr = "00";
        //    }
        //    else
        //    {
        //        mstr = measuredValue.ToString().Split('.')[1];
        //    }
        //    mstr = measuredValue.ToString($"f{mstr.Length + 3}").Split('.')[1];
        //    var sp = ((decimal)uncertainty).ToString("G2").Split('.');
        //    if (sp[0] == "1" || sp[0] == "2")
        //        sp = (uncertainty / 1000).ToString("f4").Split('.');
        //    else
        //        sp = double.Parse((uncertainty / 1000).ToString("G2")).ToString("0.##########").Split('.');
        //    var ustr = sp[1];
        //    int j;
        //    if (istr.Length >= mstr.Length)
        //    {
        //        j = mstr.Length;
        //        if (ustr.Length < mstr.Length)
        //        {
        //            j = ustr.Length;
        //        }
        //    }
        //    else
        //    {
        //        j = istr.Length;
        //        if (ustr.Length < istr.Length)
        //        {
        //            j = ustr.Length;
        //        }
        //    }
        //    var indicationStr = indication.ToString($"f{j}");
        //    var measuredValueStr = measuredValue.ToString($"f{j}");
        //    var errorStr = (Convert.ToDouble(indicationStr) * 1000 - Convert.ToDouble(measuredValueStr) * 1000).ToString($"f{j - 3}") == "f-1" ? error.ToString("0.00") : (Convert.ToDouble(indicationStr) * 1000 - Convert.ToDouble(measuredValueStr) * 1000).ToString($"f{j - 3}");//error.ToString($"f{j - 3}");
        //    var acceptanceStr = (acceptance * 1000).ToString($"f{j - 3}") == "f-1" ? (acceptance * 1000).ToString("0.##########") : (acceptance * 1000).ToString($"f{j - 3}");
        //    var uncertaintyStr = (uncertainty * 1000).ToString($"f{j - 3}") == "f-1" ? (uncertainty * 1000).ToString("0.##########") : (uncertainty * 1000).ToString($"f{j - 3}"); ;
        //    return (indicationStr, measuredValueStr, errorStr, acceptanceStr, uncertaintyStr);
        //}
        #region
        //void CalculateCurrentmA(string mode)
        //{
        //    int j = 0;
        //    foreach (var item in plcGroupData)
        //    {
        //        int l = 1;
        //        var data = item.Where(p => p.VoltsorAmps.Equals("Amps") && p.Mode.Equals(mode) && p.VerifyType.Equals("Post-Calibration")).GroupBy(d => d.Channel);
        //        foreach (var item2 in data)
        //        {
        //            var cvDataList = item2.OrderBy(dd => dd.Scale).ThenBy(dd => dd.CommandedValue).ToList();
        //            foreach (var cvData in cvDataList)
        //            {
        //                var CHH = $"{l}-{cvData.Channel}";
        //                var Range = cvData.Scale;
        //                var Indication = cvData.MeasuredValue;
        //                var MeasuredValue = cvData.StandardValue;
        //                var Error = Indication - MeasuredValue;
        //                double Acceptance = 0;
        //                var AcceptanceStr = "";

        //                var plcrmd = plcRepetitiveMeasurementGroupData.First(a => a.Key.Equals(cvData.PclNo));
        //                var mdcv = plcrmd.Where(d => d.CommandedValue.Equals(cvData.CommandedValue) && d.VoltsorAmps.Equals("Amps") && d.Mode.Equals(mode) && d.VerifyType.Equals("Post-Calibration")).GroupBy(a => a.Channel).First().GroupBy(a => a.Point).First().ToList();
        //                double ror;
        //                if (baseInfo.RepetitiveMeasurementsCount >= 6)//贝塞尔公式法
        //                {
        //                    var avg = mdcv.Sum(c => c.StandardValue) / mdcv.Count / 1000;
        //                    ror = Math.Sqrt(mdcv.Select(c => Math.Pow(c.StandardValue / 1000 - avg, 2)).Sum() / (mdcv.Count - 1));
        //                }
        //                else//极差法
        //                {
        //                    var poorCoefficient = PoorCoefficients[mdcv.Count];
        //                    var mdsv = mdcv.Select(c => c.StandardValue / 1000).ToList();
        //                    var R = mdsv.Max() - mdsv.Min();
        //                    var u2 = R / poorCoefficient;
        //                    ror = u2;
        //                }
        //                //计算不确定度
        //                var UncertaintyStr = (baseInfo.K * 1000 * Math.Sqrt(Math.Pow(cvData.StandardTotalU / 2, 2) + Math.Pow(cstd, 2) + Math.Pow(ror, 2))).ToString();
        //                var Uncertainty = double.Parse(UncertaintyStr);
        //                var T = double.Parse((cvData.Scale * baseInfo.RatedAccuracyC).ToString("G2"));
        //                var Conclustion = "";
        //                //计算接受限
        //                if (baseInfo.AcceptedTolerance.Equals("0"))
        //                {
        //                    var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC;
        //                    Acceptance = accpetedTolerance;
        //                }
        //                else if (baseInfo.AcceptedTolerance.Equals("1"))
        //                {
        //                    var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC - Uncertainty;
        //                    Acceptance = accpetedTolerance;
        //                }
        //                else if (baseInfo.AcceptedTolerance.Equals("M2%"))
        //                {
        //                    var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale * baseInfo.RatedAccuracyC * 2 / (2 * Uncertainty / 1000)) - 0.54);
        //                    if (m2 < 0)
        //                    {
        //                        var accpetedTolerance = cvData.Scale * baseInfo.RatedAccuracyC;
        //                        Acceptance = accpetedTolerance;
        //                    }
        //                    else
        //                    {
        //                        var accpetedTolerance = (cvData.Scale * baseInfo.RatedAccuracyC - Uncertainty) * m2;
        //                        Acceptance = accpetedTolerance;
        //                    }
        //                }
        //                //约分
        //                var (IndicationReduce, MeasuredValueReduce, ErrorReduce, AcceptanceReduce, UncertaintyReduce) = ReduceCurrent(Math.Abs(Indication), Math.Abs(MeasuredValue), Error, Acceptance, Uncertainty);
        //                AcceptanceStr = $"±{AcceptanceReduce}";
        //                //计算判定结果
        //                if (baseInfo.AcceptedTolerance.Equals("0"))
        //                {
        //                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
        //                    {
        //                        Conclustion = "P";
        //                    }
        //                    else
        //                    {
        //                        Conclustion = "F";
        //                    }
        //                }
        //                else if (baseInfo.AcceptedTolerance.Equals("1"))
        //                {
        //                    if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
        //                    {
        //                        Conclustion = "P";
        //                    }
        //                    else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
        //                    {
        //                        Conclustion = "P*";
        //                    }
        //                    else
        //                    {
        //                        Conclustion = "F";
        //                    }
        //                }
        //                else if (baseInfo.AcceptedTolerance.Equals("M2%"))
        //                {
        //                    var m2 = 1.04 - Math.Pow(Math.E, 0.38 * Math.Log(cvData.Scale / 1000 * baseInfo.RatedAccuracyC * 2 / (2 * Uncertainty / 1000)) - 0.54);
        //                    if (m2 < 0)
        //                    {
        //                        if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
        //                        {
        //                            Conclustion = "P";
        //                        }
        //                        else
        //                        {
        //                            Conclustion = "F";
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(double.Parse(AcceptanceReduce)))
        //                        {
        //                            Conclustion = "P";
        //                        }
        //                        else if (Math.Abs(double.Parse(ErrorReduce)) >= Math.Abs(double.Parse(AcceptanceReduce)) && Math.Abs(double.Parse(ErrorReduce)) <= Math.Abs(T))
        //                        {
        //                            Conclustion = "P*";
        //                        }
        //                        else
        //                        {
        //                            Conclustion = "F";
        //                        }
        //                    }
        //                }

        //                if (mode.Equals("Charge"))
        //                {
        //                    model.ChargingCurrent.Add(new DataSheet
        //                    {
        //                        Channel = CHH,
        //                        Range = ((double)Range / 1000).ToString(),
        //                        Indication = IndicationReduce,
        //                        MeasuredValue = MeasuredValueReduce,
        //                        Error = ErrorReduce,
        //                        Acceptance = AcceptanceStr,
        //                        Uncertainty = UncertaintyReduce,
        //                        Conclusion = Conclustion
        //                    });
        //                }
        //                else
        //                {
        //                    model.DischargingCurrent.Add(new DataSheet
        //                    {
        //                        Channel = CHH,
        //                        Range = ((double)Range / 1000).ToString(),
        //                        Indication = IndicationReduce,
        //                        MeasuredValue = MeasuredValueReduce,
        //                        Error = ErrorReduce,
        //                        Acceptance = AcceptanceStr,
        //                        Uncertainty = UncertaintyReduce,
        //                        Conclusion = Conclustion
        //                    });
        //                }
        //                j++;
        //            }
        //        }
        //        l++;
        //    }
        //}
        #endregion

        #endregion

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
            }
            catch (Exception ex)
            {
                Log.Logger.Error($"地址：{Request.Path}，参数：{certNo}, 错误：{ex.Message}");
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
        /// 校准报表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCalibrateReport([FromQuery] QueryCertReportReq req)
        {
            var result = new TableData();
            try

            {
                return await _nwcaliCertApp.GetCalibrateReport(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
        /// <summary>
        /// 校准报表详情
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCalibrateDetailReport([FromQuery] QueryCertReportReq req)
        {
            var result = new TableData();
            try

            {
                return await _nwcaliCertApp.GetCalibrateDetailReport(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }


        /// <summary>
        /// 校准报表2.0
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCalibrateReportNew([FromQuery] QueryCertReportReq req)
        {
            var result = new TableData();
            try

            {
                return await _nwcaliCertApp.GetCalibrateReportNew(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }

        /// <summary>
        /// 校准报表详情2.0
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<TableData> GetCalibrateDetailReportNew([FromQuery] QueryCertReportReq req)
        {
            var result = new TableData();
            try

            {
                return await _nwcaliCertApp.GetCalibrateDetailReportNew(req);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，参数：{req.ToJson()}, 错误：{result.Message}");
            }
            return result;
        }
    }
}
