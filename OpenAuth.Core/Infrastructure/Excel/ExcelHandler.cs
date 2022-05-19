using Npoi.Mapper;
using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Infrastructure.Excel
{

    public class ExcelHandler
    {
        private readonly Mapper mapper;

        public ExcelHandler(Stream excleStream)
        {
            this.mapper = new Mapper(excleStream);
        }
        public ExcelHandler(string filePath)
        {
            this.mapper = new Mapper(filePath);
        }
        public ExcelHandler(IWorkbook workbook)
        {
            this.mapper = new Mapper(workbook);
        }

        public List<T> GetData<T>(string sheetName, Func<Mapper, Mapper> func) where T : class
        {
            var data = func.Invoke(mapper)
                .Take<T>(sheetName);
            return data.Select(d => d.Value).SkipWhile(v => v is null).ToList();
        }

        public List<NwcaliPLCData> GetNWCaliPLCData(string sheetName)
        {
            var data = mapper
                .Map<NwcaliPLCData>(0, o => o.Verify_Type)
                .Map<NwcaliPLCData>(1, o => o.VoltsorAmps)
                .Map<NwcaliPLCData>(2, o => o.Channel)
                .Map<NwcaliPLCData>(3, o => o.Mode)
                .Map<NwcaliPLCData>(4, o => o.Range)
                .Map<NwcaliPLCData>(5, o => o.Point)
                .Map<NwcaliPLCData>(6, o => o.Commanded_Value)
                .Map<NwcaliPLCData>(7, o => o.Measured_Value)
                .Map<NwcaliPLCData>(8, o => o.Scale)
                .Map<NwcaliPLCData>(9, o => o.Standard_Value)
                .Map<NwcaliPLCData>(10, o => o.Standard_total_U)
                .Take<NwcaliPLCData>(sheetName);
            return data.Select(d => d.Value).SkipWhile(v => v is null).ToList();
        }
        public List<NwcaliPLCRepetitiveMeasurementData> GetNWCaliPLCRepetitiveMeasurementData(string sheetName)
        {
            var data = mapper
                .Map<NwcaliPLCRepetitiveMeasurementData>(0, o => o.Verify_Type)
                .Map<NwcaliPLCRepetitiveMeasurementData>(1, o => o.VoltsorAmps)
                .Map<NwcaliPLCRepetitiveMeasurementData>(2, o => o.Channel)
                .Map<NwcaliPLCRepetitiveMeasurementData>(3, o => o.Mode)
                .Map<NwcaliPLCRepetitiveMeasurementData>(4, o => o.Range)
                .Map<NwcaliPLCRepetitiveMeasurementData>(5, o => o.Point)
                .Map<NwcaliPLCRepetitiveMeasurementData>(6, o => o.Commanded_Value)
                .Map<NwcaliPLCRepetitiveMeasurementData>(7, o => o.Measured_Value)
                .Map<NwcaliPLCRepetitiveMeasurementData>(8, o => o.Scale)
                .Map<NwcaliPLCRepetitiveMeasurementData>(9, o => o.Standard_Value)
                .Map<NwcaliPLCRepetitiveMeasurementData>(10, o => o.Standard_total_U)
                .Take<NwcaliPLCRepetitiveMeasurementData>(sheetName);
            return data.Select(d => d.Value).SkipWhile(v => v is null).ToList();
        }
        public List<T> GetListData<T>(Func<Mapper, List<T>> func)
        {
            return func(mapper);
        }


        public T GetBaseInfo<T>(Func<ISheet, T> func)
        {
            var sheet = mapper.Workbook.GetSheetAt(0);
            return func(sheet);
            //var timeRow = sheet.GetRow(1);
            //baseInfo.Time = timeRow.GetCell(1).StringCellValue;
            //var fileVersionRow = sheet.GetRow(3);
            //baseInfo.FileVersion = fileVersionRow.GetCell(1).StringCellValue;
            //var testerMakeRow = sheet.GetRow(4);
            //baseInfo.TesterMake = testerMakeRow.GetCell(1).StringCellValue;
            //var testerModelRow = sheet.GetRow(5);
            //baseInfo.TesterModel = testerModelRow.GetCell(1).StringCellValue;
            //var testerSnRow = sheet.GetRow(6);
            //baseInfo.TesterSn = testerSnRow.GetCell(1).StringCellValue;
            //var assetNoRow = sheet.GetRow(7);
            //baseInfo.AssetNo = assetNoRow.GetCell(1).StringCellValue;
            //var siteCodeRow = sheet.GetRow(8);
            //baseInfo.SiteCode = "Electrical Lab";//siteCodeRow.GetCell(1).StringCellValue;
            //var temperatureRow = sheet.GetRow(9);
            //baseInfo.Temperature = temperatureRow.GetCell(1).StringCellValue;
            //var relativeHumidityRow = sheet.GetRow(10);
            //baseInfo.RelativeHumidity = relativeHumidityRow.GetCell(1).StringCellValue;
            //var ratedAccuracyCRow = sheet.GetRow(11);
            //baseInfo.RatedAccuracyC = ratedAccuracyCRow.GetCell(1).NumericCellValue / 1000;
            //var ratedAccuracyVRow = sheet.GetRow(12);
            //baseInfo.RatedAccuracyV = ratedAccuracyVRow.GetCell(1).NumericCellValue / 1000;
            //var ammeterBitsRow = sheet.GetRow(13);
            //baseInfo.AmmeterBits = Convert.ToInt32(ammeterBitsRow.GetCell(1).NumericCellValue);
            //var VoltmeterBitsRow = sheet.GetRow(14);
            //baseInfo.VoltmeterBits = Convert.ToInt32(VoltmeterBitsRow.GetCell(1).NumericCellValue);
            //var certificateNumberRow = sheet.GetRow(15);
            //baseInfo.CertificateNumber = certificateNumberRow.GetCell(1).StringCellValue;
            //var calibrationEntityRow = sheet.GetRow(16);
            //baseInfo.CallbrationEntity = calibrationEntityRow.GetCell(1).StringCellValue;
            //var operatorRow = sheet.GetRow(17);
            //baseInfo.Operator = operatorRow.GetCell(1).StringCellValue;
            //#region 标准器设备信息
            //var etalonsNameRow = sheet.GetRow(18);
            //var etalonsCharacteristicsRow = sheet.GetRow(19);
            //var etalonsAssetNoRow = sheet.GetRow(20);
            //var etalonsCertificateNoRow = sheet.GetRow(22);
            //var etalonsDueDateRow = sheet.GetRow(23);
            //for (int i = 1; i < etalonsNameRow.LastCellNum; i++)
            //{
            //    if (string.IsNullOrWhiteSpace(etalonsNameRow.GetCell(i).StringCellValue))
            //        break;
            //    try
            //    {
            //        baseInfo.Etalons.Add(new Etalon
            //        {
            //            Name = etalonsNameRow.GetCell(i).StringCellValue,
            //            Characteristics = etalonsCharacteristicsRow.GetCell(i).StringCellValue,
            //            AssetNo = etalonsAssetNoRow.GetCell(i).StringCellValue,
            //            CertificateNo = etalonsCertificateNoRow.GetCell(i).StringCellValue,
            //            DueDate = etalonsDueDateRow.GetCell(i).StringCellValue
            //        });
            //    }
            //    catch
            //    {
            //        break;
            //    }
            //}
            //#endregion
            //var commentRow = sheet.GetRow(24);
            //baseInfo.Comment = commentRow.GetCell(1).StringCellValue;
            //var calibrationTypeRow = sheet.GetRow(25);
            //baseInfo.CalibrationType = calibrationTypeRow.GetCell(1).StringCellValue;
            //var repetitiveMeasurementsCountRow = sheet.GetRow(26);
            //baseInfo.RepetitiveMeasurementsCount = Convert.ToInt32(repetitiveMeasurementsCountRow.GetCell(1).NumericCellValue);
            //var turRow = sheet.GetRow(27);
            //baseInfo.TUR = turRow.GetCell(1).StringCellValue;
            //var acceptedToleranceRow = sheet.GetRow(28);
            //baseInfo.AcceptedTolerance = acceptedToleranceRow.GetCell(1).StringCellValue;
            //var kRow = sheet.GetRow(29);
            //baseInfo.K = kRow.GetCell(1).NumericCellValue;
            //var testIntervalRow = sheet.GetRow(30);
            //baseInfo.TestInterval = testIntervalRow.GetCell(1).StringCellValue;
            //#region 下位机
            //var pclCommentRow = sheet.GetRow(31);
            //var pclNoRow = sheet.GetRow(32);
            //var pclGuidRow = sheet.GetRow(33);
            //for (int i = 1; i < pclNoRow.LastCellNum; i++)
            //{
            //    if (string.IsNullOrWhiteSpace(pclGuidRow.GetCell(i)?.StringCellValue))
            //        continue;
            //    try
            //    {
            //        baseInfo.PCPLCs.Add(new PCPLC
            //        {
            //            Comment = pclCommentRow.GetCell(i).StringCellValue,
            //            No = Convert.ToInt32(pclNoRow.GetCell(i).StringCellValue),
            //            Guid = pclGuidRow.GetCell(i).StringCellValue
            //        });
            //    }
            //    catch
            //    {
            //        break;
            //    }
            //}
            //#endregion
        }

        public ISheet GetSheet()
        {
            var sheet = mapper.Workbook.GetSheetAt(0);
            return sheet;
        }

        public List<NwcaliTur> GetNwcaliTur(string sheetName)
        {
            var data = mapper
                .Map<NwcaliTur>(0, o => o.Range)
                .Map<NwcaliTur>(1, o => o.TestPoint)
                .Map<NwcaliTur>(2, o => o.Tur)
                .Map<NwcaliTur>(3, o => o.UncertaintyContributors)
                .Map<NwcaliTur>(4, o => o.SensitivityCoefficient)
                .Map<NwcaliTur>(5, o => o.Value)
                .Map<NwcaliTur>(6, o => o.Unit)
                .Map<NwcaliTur>(7, o => o.Type)
                .Map<NwcaliTur>(8, o => o.Distribution)
                .Map<NwcaliTur>(9, o => o.Divisor)
                .Map<NwcaliTur>(10, o => o.StdUncertainty)
                //.Map<NwcaliTur>(11, o => o.DegreesOfFreedom)
                //.Map<NwcaliTur>(10, o => o.SignificanceCheck)
                .Take<NwcaliTur>(sheetName);
            return data.Select(d => d.Value).SkipWhile(v => v is null).Where(v => v.Tur != 0).ToList();
        }

        public void SetValue(string value, int row, int col)
        {
            var sheet = mapper.Workbook.GetSheetAt(0);
            sheet.GetRow(row).GetCell(col).SetCellValue(value);
        }
        public void SetValue(DateTime value, int row, int col)
        {
            var sheet = mapper.Workbook.GetSheetAt(0);
            sheet.GetRow(row).GetCell(col).SetCellValue(value);
        }
        public void SetValue(bool value, int row, int col)
        {
            var sheet = mapper.Workbook.GetSheetAt(0);
            sheet.GetRow(row).GetCell(col).SetCellValue(value);
        }
        public void SetValue(double value, int row, int col)
        {
            var sheet = mapper.Workbook.GetSheetAt(0);
            sheet.GetRow(row).GetCell(col).SetCellValue(value);
        }
        public void Save(string filePath)
        {
            mapper.Save(filePath);
        }

       
    }
}
