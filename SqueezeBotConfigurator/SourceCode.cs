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

                         //new TikerAndTimeFrame(Tiker.WRXUSDT,TimeFrame.oneMinute),
                         //new TikerAndTimeFrame(Tiker.WRXUSDT,TimeFrame.fiveMinutes),
                         //  new TikerAndTimeFrame(Tiker.ADAUSDT,TimeFrame.oneMinute),
                         //new TikerAndTimeFrame(Tiker.ADAUSDT,TimeFrame.fiveMinutes),
                         //    new TikerAndTimeFrame(Tiker.DENTUSDT,TimeFrame.oneMinute),
                         //new TikerAndTimeFrame(Tiker.DENTUSDT,TimeFrame.fiveMinutes),
                         //     new TikerAndTimeFrame(Tiker.THETAUSDT,TimeFrame.oneMinute),
                         //new TikerAndTimeFrame(Tiker.THETAUSDT,TimeFrame.fiveMinutes),
                         //         new TikerAndTimeFrame(Tiker.GRTUSDT,TimeFrame.oneMinute),
                         //new TikerAndTimeFrame(Tiker.GRTUSDT,TimeFrame.fiveMinutes),
                         //       new TikerAndTimeFrame(Tiker.VETUSDT,TimeFrame.oneMinute),
                         //new TikerAndTimeFrame(Tiker.VETUSDT,TimeFrame.fiveMinutes),
                         //         new TikerAndTimeFrame(Tiker.SXPUSDT,TimeFrame.oneMinute),
                         //new TikerAndTimeFrame(Tiker.SXPUSDT,TimeFrame.fiveMinutes),

                            new TikerAndTimeFrame(Tiker.FILUSDT,TimeFrame.oneMinute),
                            new TikerAndTimeFrame(Tiker.EOSUSDT,TimeFrame.oneMinute),
                            new TikerAndTimeFrame(Tiker.ADAUSDT,TimeFrame.oneMinute),

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
