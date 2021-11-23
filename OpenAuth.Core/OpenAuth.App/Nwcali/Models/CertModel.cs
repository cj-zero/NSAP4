using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Nwcali.Models
{
    public class CertModel
    {
        public string BarCode { get; set; }
        public CalibrationCertificate CalibrationCertificate { get; set; } = new CalibrationCertificate();

        public List<MainStandardsUsed> MainStandardsUsed { get; set; } = new List<MainStandardsUsed>();
        public List<TurTable> TurTables { get; set; } = new List<TurTable>();
        public UncertaintyBudgetTable VoltageUncertaintyBudgetTables { get; set; } = new UncertaintyBudgetTable();
        public UncertaintyBudgetTable CurrentUncertaintyBudgetTables { get; set; } = new UncertaintyBudgetTable();
        public List<DataSheet> ChargingVoltage { get; set; } = new List<DataSheet>();
        public List<DataSheet> DischargingVoltage { get; set; } = new List<DataSheet>();
        public List<DataSheet> ChargingCurrent { get; set; } = new List<DataSheet>();
        public List<DataSheet> DischargingCurrent { get; set; } = new List<DataSheet>();
        public string CalibrationTechnician { get; set; }
        public string TechnicalManager { get; set; }
        public string ApprovalDirector { get; set; }
    }
    public class CalibrationCertificate
    {
        public string CertificatenNumber { get; set; }
        public string TesterMake { get; set; }

        public string CalibrationDate { get; set; }

        public string TesterModel { get; set; }

        public string CalibrationDue { get; set; }

        public string TesterSn { get; set; }

        public string DataType { get; set; }
        public string AssetNo { get; set; }
        public string SiteCode { get; set; }
        public string Temperature { get; set; }
        public string RelativeHumidity { get; set; }
        public string EntrustedUnit { get; set; }
        public string EntrustedUnitAdress { get; set; }
        public string EntrustedDate { get; set; }
    }
    public class MainStandardsUsed
    {
        public string Name { get; set; }
        public string Characterisics { get; set; }
        public string AssetNo { get; set; }
        public string CertificateNo { get; set; }
        public string DueDate { get; set; }
        public string CalibrationEntity { get; set; }
    }
    public class TurTable
    {
        public string Number { get; set; }
        public string Point { get; set; }
        public string U95Standard { get; set; }
        public string TUR { get; set; }
        public string Spec { get; set; }
    }
    public class UncertaintyBudgetTable
    {
        public string Value { get; set; }
        public List<UncertaintyBudgetTableData> Datas { get; set; } = new List<UncertaintyBudgetTableData>();
        public string TesterResolutionValue { get; set; }
        public string TesterResolutionStdUncertainty { get; set; }
        public string TesterResolutionSignificance { get; set; }
        public string RepeatabilityOfReadingValue { get; set; }
        public string RepeatabilityOfReadingStdUncertainty { get; set; }
        public string RepeatabilityOfReadingSignificance { get; set; }
        public string CombinedUncertainty { get; set; }
        public string CombinedUncertaintySignificance { get; set; }
        public string CoverageFactor { get; set; }
        public string ExpandedUncertainty { get; set; }
        public class UncertaintyBudgetTableData
        {
            public string UncertaintyContributors { get; set; }
            public string Value { get; set; }

            public string SensitivityCoefficient { get; set; }
            public string Unit { get; set; }

            public string Type { get; set; }
            public string Distribution { get; set; }
            public string CoverageFactor { get; set; }
            public string StdUncertainty { get; set; }
            public string Significance { get; set; }
        }
    }

    public class DataSheet 
    {
        public string Channel { get; set; }
        public string Range { get; set; }
        public string Indication { get; set; }
        public string MeasuredValue { get; set; }
        public string Error { get; set; }
        public string Acceptance { get; set; }
        public string Uncertainty { get; set; }
        public string Conclusion { get; set; }
    }
}