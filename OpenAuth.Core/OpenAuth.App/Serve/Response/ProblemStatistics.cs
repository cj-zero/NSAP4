using System;
using System.Collections.Generic;
using System.Text;

namespace OpenAuth.App.Serve.Response
{
    public class ProblemStatisticsMin
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Num { get; set; }
    }
    public class ProblemStatisticsMax: ProblemStatisticsMin
    {
        public string Id { get; set; }
        public List<ProblemStatisticsMin> Children { get; set; }
    }
    public class DailyReport
    {
        public string Id { get; set; }
        public string description { get; set; }
        public string code { get; set; }

    }
}
