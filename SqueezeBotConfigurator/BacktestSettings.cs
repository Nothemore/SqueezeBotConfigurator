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
        public double buyTriggerMax = 4;//8
        public double buyTriggerStep = 0.01;

        public double sellTriggerMin = 0.55;//0.55
        public double sellTriggerMax = 7;
        public double sellTriggerStep = 0.01;

        public double stopTriggerMin = 1;
        public double stopTriggerMax =3;
        public double stopTriggerStep = 0.1;
        public double stopTriggerDefaul = 5;
        public bool useStopLoss = true;
        public bool calculateStop = true;

        public double buySellRatio = 0.4;
        public int configCount = 10;
        public TradeOpenTrigger tradeOpenTrigger;

        [NonSerialized]
        public Func<List<Config>, List<Config>> ConfigFilter;
        [NonSerialized]
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


        public BacktestSettings DeepCopy(TradeOpenTrigger tradeTrigger)
        {
            var copySettings = new BacktestSettings(tradeTrigger);

            copySettings.buyTriggerMin = buyTriggerMin;
            copySettings.buyTriggerMax = buyTriggerMax;
            copySettings.buyTriggerStep = buyTriggerStep;

            copySettings.sellTriggerMin = sellTriggerMin;
            copySettings.sellTriggerMax = sellTriggerMax;
            copySettings.sellTriggerStep = sellTriggerStep;

            copySettings.stopTriggerMin = stopTriggerMin;
            copySettings.stopTriggerMax = stopTriggerMax;
            copySettings.stopTriggerStep = stopTriggerStep;

            copySettings.stopTriggerDefaul = stopTriggerDefaul;
            copySettings.useStopLoss = useStopLoss;
            copySettings.calculateStop = calculateStop;
            copySettings.buySellRatio =buySellRatio;
            copySettings.configCount = configCount;
            return copySettings;
        }
    }
}
