using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqueezeBotConfigurator
{
    class SourceCode : ISource
    {
        public string reportPath { get; set; }
        public BacktestSettings[] Settings { get; set; }
        public TikerAndTimeFrame[] TikerFrame { get; set; }

        public SourceCode()
        {
            int configsCount = 10;
            TikerFrame = new TikerAndTimeFrame[]
                   {

                         new TikerAndTimeFrame(Tiker.FILUSDT,TimeFrame.oneMinute),
                         new TikerAndTimeFrame(Tiker.EOSUSDT,TimeFrame.oneMinute),
                         new TikerAndTimeFrame(Tiker.TRXUSDT,TimeFrame.oneMinute),
                          new TikerAndTimeFrame(Tiker.DOTUSDT,TimeFrame.oneMinute),
                           new TikerAndTimeFrame(Tiker.THETAUSDT,TimeFrame.oneMinute),
                           new TikerAndTimeFrame(Tiker.TFUELUSDT,TimeFrame.oneMinute),
                           new TikerAndTimeFrame(Tiker.LUNAUSDT,TimeFrame.oneMinute),
                            new TikerAndTimeFrame(Tiker.ONTUSDT,TimeFrame.oneMinute),
                             new TikerAndTimeFrame(Tiker.ONEUSDT,TimeFrame.oneMinute),
                                 new TikerAndTimeFrame(Tiker.ALICEUSDT,TimeFrame.oneMinute),
                                     new TikerAndTimeFrame(Tiker.LINAUSDT,TimeFrame.oneMinute),
                                         new TikerAndTimeFrame(Tiker.AUDIOUSDT,TimeFrame.oneMinute),

                   };
            Settings = new BacktestSettings[]
                {
                         new BacktestSettings(TradeOpenTrigger.open)     {configCount = configsCount},
                         new BacktestSettings(TradeOpenTrigger.close)    {configCount = configsCount  },
                         new BacktestSettings(TradeOpenTrigger.openClose){configCount = configsCount},
                         new BacktestSettings(TradeOpenTrigger.high)     {configCount = configsCount},
                         new BacktestSettings(TradeOpenTrigger.low)      {configCount = configsCount },
                         new BacktestSettings(TradeOpenTrigger.highLow)  {configCount = configsCount }
                 };
            reportPath = @"C:\Users\Nocturne\Desktop\inDev\SqResult.json";



        }




    }
}
