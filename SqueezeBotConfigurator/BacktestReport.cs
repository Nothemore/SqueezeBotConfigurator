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
        public string FileName;
        public int CandleCount;
        public BacktestSettings BacktestSettings;
        public string Date;
    }
}
