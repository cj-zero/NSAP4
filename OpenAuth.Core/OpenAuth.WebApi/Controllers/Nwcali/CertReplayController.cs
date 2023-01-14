using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DinkToPdf;
using Infrastructure;
using Infrastructure.Excel;
using Infrastructure.Export;
using OpenAuth.Repository.Interface;
using Microsoft.AspNetCore.Mvc;
using OpenAuth.App;
using OpenAuth.App.Interface;
using OpenAuth.App.Nwcali;
using OpenAuth.App.Nwcali.Models;
using OpenAuth.App.Nwcali.Request;
using OpenAuth.App.Request;
using OpenAuth.App.Response;
using OpenAuth.Repository.Domain.CapNwcail;
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
    public class CertReplayController : Controller
    {
        private readonly IAuth _authUtil;
        private readonly CertReplayApp _certReplayApp;
        private readonly CertinfoApp _certinfoApp;
        private readonly CertPlcApp _certPlcApp;
        private readonly ModuleFlowSchemeApp _moduleFlowSchemeApp;
        private readonly FlowInstanceApp _flowInstanceApp;
        private readonly UserSignApp _userSignApp;
        private readonly FileApp _fileApp;
        private readonly DevInfoApp _devInfoApp;
        private readonly MachineInfoApp _machineInfoApp;
        private static readonly string BaseCertDir = Path.Combine(Directory.GetCurrentDirectory(), "certs");
        public IUnitWork _unitWork;
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
        public CertReplayController(CertReplayApp certReplayApp, CertinfoApp certinfoApp, CertPlcApp certPlcApp, ModuleFlowSchemeApp moduleFlowSchemeApp, FlowInstanceApp flowInstanceApp, UserSignApp userSignApp, FileApp fileApp, DevInfoApp devInfoApp, MachineInfoApp machineInfoApp, IUnitWork unitWork, IAuth auth)
        {
            _certReplayApp = certReplayApp;
            _certinfoApp = certinfoApp;
            _certPlcApp = certPlcApp;
            _moduleFlowSchemeApp = moduleFlowSchemeApp;
            _flowInstanceApp = flowInstanceApp;
            _userSignApp = userSignApp;
            _fileApp = fileApp;
            _authUtil = auth;
            _devInfoApp = devInfoApp;
            _machineInfoApp = machineInfoApp;
            _unitWork = unitWork;
        }

        [HttpPost]
        public async Task<Response<bool>> Generate(string loginfo = "")
        {
            var loginContext = _authUtil.GetCurrentUser();
            var loginOrg = loginContext.Orgs.OrderByDescending(c => c.CascadeId).FirstOrDefault();
            Log.Logger.Information($"校准证书上传，阶段：{loginOrg.Name}，参数loginfo：{loginfo}。");

            var file = Request.Form.Files[0];
            string fileName = file.FileName.Split(".")[0];
            var handler = new ExcelHandler(file.OpenReadStream());
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
                return new Response<bool>()
                {
                    Code = 400,
                    Message = "Operator can not be null.",
                    Result = false
                };
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
                await _certReplayApp.AddAsync(baseInfo, fileName);
                //保存文件
                //var fileExtension = Path.GetExtension(file.FileName);
                //var fileResp = await _fileApp.UploadFileToHuaweiOBS($"nwcail/{baseInfo.CertificateNumber}/{baseInfo.CertificateNumber}{fileExtension}", file);
                //var savePath = $"{fileResp.FileName},{fileResp.FilePath}";
                //await _certReplayApp.UpdateFilePath(baseInfo.CertificateNumber, savePath);
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

        [HttpGet]
        public async Task CreateNwcailFile(string certNo)
        {
            await _certReplayApp.CreateNwcailFile(certNo);
        }

        /// <summary>
        /// 重新生成当前校准证书
        /// </summary>
        /// <param name="certNo">证书编号</param>
        /// <returns>返回校准结果</returns>
        [HttpGet]
        public async Task<TableData> UpdateCertInfo(string certNo)
        {
            var result = new TableData();
            try
            {
                return await _certReplayApp.UpdateCertInfo(certNo);
            }
            catch (Exception ex)
            {
                result.Code = 500;
                result.Message = ex.Message;
                Log.Logger.Error($"地址：{Request.Path}，校准参数：{certNo.ToJson()}, 错误：{result.Message}");
            }

            return result;
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
        /// 将日期转成英文格式
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private static string DateStringConverter(string date)
        {
            return DateTime.Parse(date).ToString("MMM-dd-yyyy", new System.Globalization.CultureInfo("en-us"));
        }
        

        [HttpGet]
        public async Task UpdatePath()
        {
            await _certReplayApp.UpdatePath();
        }
    }
}
