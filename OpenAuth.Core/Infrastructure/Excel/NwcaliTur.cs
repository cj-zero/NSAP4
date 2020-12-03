using Npoi.Mapper.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Excel
{
    public class NwcaliTur
    {
        [Column(0)]
        public double Range { get; set; }
        [Column(1)]
        public double TestPoint { get; set; }
        [Column(2)]
        public double Tur { get; set; }
        [Column(3)]
        public string UncertaintyContributors { get; set; }
        [Column(4)]
        public double SensitivityCoefficient { get; set; }
        [Column(5)]
        public double Value { get; set; }
        [Column(6)]
        public string Unit { get; set; }
        [Column(7)]
        public string Type { get; set; }
        [Column(8)]
        public string Distribution { get; set; }
        [Column(9)]
        public double Divisor { get; set; }
        [Column(10)]
        public double StdUncertainty { get; set; }
        [Column(11)]
        public string DegreesOfFreedom { get; set; }
        [Column(12)]
        public double SignificanceCheck { get; set; }
    }
}
