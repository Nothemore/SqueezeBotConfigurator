using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqueezeBotConfigurator
{
    public class BacktestReport
    {
        public List<Config> Configs { get; set; }
        public string FileName { get; set; }
        public int CandleCount { get; set; }
        public BacktestSettings BacktestSettings { get; set; }
        public string Date { get; set; }
    }
}
