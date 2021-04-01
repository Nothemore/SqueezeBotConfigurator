using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqueezeBotConfigurator
{
    public class BacktestSettings
    {
        public double buyTriggerMin = 1.2;//1.2
        public double buyTriggerMax = 8;//8
        public double buyTriggerStep = 0.01;

        public double sellTriggerMin = 0.55;//0.55
        public double sellTriggerMax = 7;
        public double sellTriggerStep = 0.01;

        public double stopTriggerMin = 1;
        public double stopTriggerMax = 5;
        public double stopTriggerStep = 0.1;
        public double stopTriggerDefaul = 5;
        public bool useStopLoss = true;
        public bool calculateStop = true;

        public double buySellRatio = 0.4;
        public int configCount = 10;
        public TradeOpenTrigger tradeOpenTrigger;
        public Func<List<Config>, List<Config>> ConfigFilter;
        public Func<Config, bool> ConfigPreFilter;


        public BacktestSettings(TradeOpenTrigger tradeOpenTrigger)
        {
            this.tradeOpenTrigger = tradeOpenTrigger;
            ConfigFilter = (bestConfigs) =>
            {
                return bestConfigs
                .OrderByDescending(x => x.totalProfit)
                .Take(configCount)
                .ToList();
            };

            ConfigPreFilter = (thisConfig) =>
            {
                return true;
            };

        }
    }
}
