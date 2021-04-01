using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqueezeBotConfigurator
{
    public class Backtest
    {
        public BacktestSettings Settings;
        public DataSet Data;
        public List<Config> Configs { get; private set; }
        public Backtest(BacktestSettings settings, DataSet data)
        {
            Settings = settings;
            Data = data;
            Configs = new List<Config>(Settings.configCount + 1);
        }

        public void RunTestDefaltStop()
        {
            for (double buyTrigger = Settings.buyTriggerMin; buyTrigger <= Settings.buyTriggerMax; buyTrigger += Settings.buyTriggerStep)
            {
                Settings.sellTriggerMax = buyTrigger * Settings.buySellRatio;
                for (double sellTrigger = Settings.sellTriggerMin; sellTrigger <= Settings.sellTriggerMax; sellTrigger += Settings.sellTriggerStep)
                {
                    var currentConfig = new Config();
                    currentConfig.useStop = Settings.useStopLoss;
                    currentConfig.stopTrigger = Settings.stopTriggerDefaul;
                    currentConfig.buyTrigger = buyTrigger;
                    currentConfig.sellTrigger = sellTrigger;

                    currentConfig.RunTest(Data, Settings.tradeOpenTrigger);

                    if (Settings.ConfigPreFilter(currentConfig))
                    {
                        Configs.Add(currentConfig);
                        Configs = Settings.ConfigFilter(Configs);
                    }
                }
            }
        }

        public void RunTestCalculatedStop()
        {
            for (double stopTrigger = Settings.stopTriggerMin; stopTrigger <= Settings.stopTriggerMax; stopTrigger += Settings.stopTriggerStep)
            {
                Settings.stopTriggerDefaul = stopTrigger;
                RunTestDefaltStop();
            }
        }






    }
}
