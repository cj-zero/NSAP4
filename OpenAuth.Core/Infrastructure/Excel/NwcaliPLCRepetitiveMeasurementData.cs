using Npoi.Mapper.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.Excel
{
    public class NwcaliPLCRepetitiveMeasurementData
    {
        [Column(0)]
        public string Verify_Type { get; set; }

        [Column(1)]
        public string VoltsorAmps { get; set; }

        [Column(2)]
        public int MeasurementTimes { get; set; }

        [Column(3)]
        public int Channel { get; set; }

        [Column(4)]
        public string Mode { get; set; }

        [Column(5)]
        public int Range { get; set; }

        [Column(6)]
        public int Point { get; set; }

        [Column(7)]
        public double Commanded_Value { get; set; }

        [Column(8)]
        public double Measured_Value { get; set; }

        [Column(9)]
        public int Scale { get; set; }

        [Column(10)]
        public double Standard_Value { get; set; }

        [Column(11)]
        public double Standard_total_U { get; set; }
    }
}
