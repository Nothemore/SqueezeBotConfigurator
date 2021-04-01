using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqueezeBotConfigurator
{
    public enum TradeOpenTrigger
    {
        low = 0,
        close = 1,
        open = 2,
        high = 3,
        openClose = 4,
        highLow = 5
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum Tiker
    {
        BTCUSDT,
        ETHUSDT,
        BNBUSDT,
        BCCUSDT,
        NEOUSDT,
        LTCUSDT,
        QTUMUSDT,
        ADAUSDT,
        XRPUSDT,
        EOSUSDT,
        TUSDUSDT,
        IOTAUSDT,
        XLMUSDT,
        ONTUSDT,
        TRXUSDT,
        ETCUSDT,
        ICXUSDT,
        VENUSDT,
        NULSUSDT,
        VETUSDT,
        PAXUSDT,
        BCHABCUSDT,
        BCHSVUSDT,
        USDCUSDT,
        LINKUSDT,
        WAVESUSDT,
        BTTUSDT,
        USDSUSDT,
        ONGUSDT,
        HOTUSDT,
        ZILUSDT,
        ZRXUSDT,
        FETUSDT,
        BATUSDT,
        XMRUSDT,
        ZECUSDT,
        IOSTUSDT,
        CELRUSDT,
        DASHUSDT,
        NANOUSDT,
        OMGUSDT,
        THETAUSDT,
        ENJUSDT,
        MITHUSDT,
        MATICUSDT,
        ATOMUSDT,
        TFUELUSDT,
        ONEUSDT,
        FTMUSDT,
        ALGOUSDT,
        USDSBUSDT,
        GTOUSDT,
        ERDUSDT,
        DOGEUSDT,
        DUSKUSDT,
        ANKRUSDT,
        WINUSDT,
        COSUSDT,
        NPXSUSDT,
        COCOSUSDT,
        MTLUSDT,
        TOMOUSDT,
        PERLUSDT,
        DENTUSDT,
        MFTUSDT,
        KEYUSDT,
        STORMUSDT,
        DOCKUSDT,
        WANUSDT,
        FUNUSDT,
        CVCUSDT,
        CHZUSDT,
        BANDUSDT,
        BUSDUSDT,
        BEAMUSDT,
        XTZUSDT,
        RENUSDT,
        RVNUSDT,
        HCUSDT,
        HBARUSDT,
        NKNUSDT,
        STXUSDT,
        KAVAUSDT,
        ARPAUSDT,
        IOTXUSDT,
        RLCUSDT,
        MCOUSDT,
        CTXCUSDT,
        BCHUSDT,
        TROYUSDT,
        VITEUSDT,
        FTTUSDT,
        BUSDTRY,
        USDTTRY,
        USDTRUB,
        EURUSDT,
        OGNUSDT,
        DREPUSDT,
        BULLUSDT,
        BEARUSDT,
        ETHBULLUSDT,
        ETHBEARUSDT,
        TCTUSDT,
        WRXUSDT,
        BTSUSDT,
        LSKUSDT,
        BNTUSDT,
        LTOUSDT,
        EOSBULLUSDT,
        EOSBEARUSDT,
        XRPBULLUSDT,
        XRPBEARUSDT,
        STRATUSDT,
        AIONUSDT,
        MBLUSDT,
        COTIUSDT,
        BNBBULLUSDT,
        BNBBEARUSDT,
        STPTUSDT,
        USDTZAR,
        WTCUSDT,
        DATAUSDT,
        XZCUSDT,
        SOLUSDT,
        USDTIDRT,
        CTSIUSDT,
        HIVEUSDT,
        CHRUSDT,
        BTCUPUSDT,
        BTCDOWNUSDT,
        GXSUSDT,
        ARDRUSDT,
        LENDUSDT,
        MDTUSDT,
        STMXUSDT,
        KNCUSDT,
        REPUSDT,
        LRCUSDT,
        PNTUSDT,
        USDTUAH,
        COMPUSDT,
        USDTBIDR,
        BKRWUSDT,
        SCUSDT,
        ZENUSDT,
        SNXUSDT,
        ETHUPUSDT,
        ETHDOWNUSDT,
        ADAUPUSDT,
        ADADOWNUSDT,
        LINKUPUSDT,
        LINKDOWNUSDT,
        VTHOUSDT,
        DGBUSDT,
        GBPUSDT,
        SXPUSDT,
        MKRUSDT,
        DAIUSDT,
        DCRUSDT,
        STORJUSDT,
        BNBUPUSDT,
        BNBDOWNUSDT,
        XTZUPUSDT,
        XTZDOWNUSDT,
        USDTBKRW,
        MANAUSDT,
        AUDUSDT,
        YFIUSDT,
        BALUSDT,
        BLZUSDT,
        IRISUSDT,
        KMDUSDT,
        USDTDAI,
        JSTUSDT,
        SRMUSDT,
        ANTUSDT,
        CRVUSDT,
        SANDUSDT,
        OCEANUSDT,
        NMRUSDT,
        DOTUSDT,
        LUNAUSDT,
        RSRUSDT,
        PAXGUSDT,
        WNXMUSDT,
        TRBUSDT,
        BZRXUSDT,
        SUSHIUSDT,
        YFIIUSDT,
        KSMUSDT,
        EGLDUSDT,
        DIAUSDT,
        RUNEUSDT,
        FIOUSDT,
        UMAUSDT,
        EOSUPUSDT,
        EOSDOWNUSDT,
        TRXUPUSDT,
        TRXDOWNUSDT,
        XRPUPUSDT,
        XRPDOWNUSDT,
        DOTUPUSDT,
        DOTDOWNUSDT,
        USDTNGN,
        BELUSDT,
        WINGUSDT,
        LTCUPUSDT,
        LTCDOWNUSDT,
        UNIUSDT,
        NBSUSDT,
        OXTUSDT,
        SUNUSDT,
        AVAXUSDT,
        HNTUSDT,
        FLMUSDT,
        UNIUPUSDT,
        UNIDOWNUSDT,
        ORNUSDT,
        UTKUSDT,
        XVSUSDT,
        ALPHAUSDT,
        USDTBRL,
        AAVEUSDT,
        NEARUSDT,
        SXPUPUSDT,
        SXPDOWNUSDT,
        FILUSDT,
        FILUPUSDT,
        FILDOWNUSDT,
        YFIUPUSDT,
        YFIDOWNUSDT,
        INJUSDT,
        AUDIOUSDT,
        CTKUSDT,
        BCHUPUSDT,
        BCHDOWNUSDT,
        AKROUSDT,
        AXSUSDT,
        HARDUSDT,
        DNTUSDT,
        STRAXUSDT,
        UNFIUSDT,
        ROSEUSDT,
        AVAUSDT,
        XEMUSDT,
        AAVEUPUSDT,
        AAVEDOWNUSDT,
        SKLUSDT,
        SUSDUSDT,
        SUSHIUPUSDT,
        SUSHIDOWNUSDT,
        XLMUPUSDT,
        XLMDOWNUSDT,
        GRTUSDT,
        JUVUSDT,
        PSGUSDT,
        USDTBVND,
        //1INCHUSDT,
        REEFUSDT,
        OGUSDT,
        ATMUSDT,
        ASRUSDT,
        CELOUSDT,
        RIFUSDT,
        BTCSTUSDT,
        TRUUSDT,
        CKBUSDT,
        TWTUSDT,
        FIROUSDT,
        LITUSDT,
        SFPUSDT,
        DODOUSDT,
        CAKEUSDT,
        ACMUSDT,
        BADGERUSDT,
        FISUSDT,
        OMUSDT,
        PONDUSDT,
        DEGOUSDT,
        ALICEUSDT,
        LINAUSDT,
        PERPUSDT,
        RAMPUSDT,
        SUPERUSDT,
        OneForAll
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeFrame
    {
        oneMinute,
        threeMinutes,
        fiveMinutes,
        fifteenMinutes,
        thirtyMinutes
    }


    public static class TimeFrameExtensions
    {
        public static string AsQuery(this TimeFrame timeFrame)
        {
            if (timeFrame == TimeFrame.oneMinute) return "1m";
            if (timeFrame == TimeFrame.threeMinutes) return "3m";
            if (timeFrame == TimeFrame.fiveMinutes) return "5m";
            if (timeFrame == TimeFrame.fifteenMinutes) return "15m";
            if (timeFrame == TimeFrame.thirtyMinutes) return "30m";
            return null;
        }

    }

    public class TikerAndTimeFrame
    {
        public Tiker Tiker { get; set; }
        public TimeFrame TimeFrame { get; set; }
        public TikerAndTimeFrame(Tiker tiker, TimeFrame timeFrame)
        {
            this.Tiker = tiker;
            TimeFrame = timeFrame;
        }

    }


    public class ExternalSettings
    {
        public bool CalculateStopLoss;
        public double DefauleStopLoss;
        public BacktestSettings settings;

        public TikerAndTimeFrame[] tikersAndFrames;

        [JsonProperty(ItemConverterType = typeof(StringEnumConverter))]
        public TimeFrame[] availableTimeFrames = new[]
        {
            TimeFrame.oneMinute,
            TimeFrame.threeMinutes,
            TimeFrame.fiveMinutes
        };

    }

    public class ReadWriteCondition
    {
        public bool FromExternalFile = false;
        public string ResultFileFullName;
        public TikerAndTimeFrame[] tikerAndFrames;
        public BacktestSettings[] Settings;

        public ReadWriteCondition(bool fromExternalFile)
        {
            this.FromExternalFile = fromExternalFile;
            if (FromExternalFile)
            {
                var currentPath = Assembly.GetExecutingAssembly().Location;
                var path = currentPath.Replace("SqueezeBotConfigurator.exe", "Settings.json");

                ExternalSettings externalSetting;
                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    externalSetting = (ExternalSettings)serializer.Deserialize(file, typeof(ExternalSettings));
                }
                ResultFileFullName = currentPath.Replace(@"\SqueezeBotConfigurator.exe", @"\SqResult.json");
                tikerAndFrames = externalSetting.tikersAndFrames;
                var configsCount = 10;
                var Settings = new BacktestSettings[]
         {
                new BacktestSettings(TradeOpenTrigger.open)     {configCount = configsCount,calculateStop = externalSetting.CalculateStopLoss,stopTriggerDefaul = externalSetting.DefauleStopLoss},
                new BacktestSettings(TradeOpenTrigger.close)    {configCount = configsCount,calculateStop = externalSetting.CalculateStopLoss,stopTriggerDefaul = externalSetting.DefauleStopLoss  },
                new BacktestSettings(TradeOpenTrigger.openClose){configCount = configsCount,calculateStop = externalSetting.CalculateStopLoss,stopTriggerDefaul = externalSetting.DefauleStopLoss },
                new BacktestSettings(TradeOpenTrigger.high)     {configCount = configsCount,calculateStop = externalSetting.CalculateStopLoss,stopTriggerDefaul = externalSetting.DefauleStopLoss  },
                new BacktestSettings(TradeOpenTrigger.low)      {configCount = configsCount,calculateStop = externalSetting.CalculateStopLoss,stopTriggerDefaul = externalSetting.DefauleStopLoss },
                new BacktestSettings(TradeOpenTrigger.highLow)  {configCount = configsCount,calculateStop = externalSetting.CalculateStopLoss,stopTriggerDefaul = externalSetting.DefauleStopLoss }
         };



            }
            else
            {
                ResultFileFullName = @"C:\Users\Nocturne\Desktop\inDev\SqResult.json";
            }

        }



    }


    public class tikerResponse
    {
        public string symbol;
        public double priceChange;
        public double priceChangePercent;
        public double weightedAvgPrice;
        public double prevClosePrice;
        public double lastPrice;
        public double lastQty;
        public double bidPrice;
        public double askPrice;
        public double openPrice;
        public double highPrice;
        public double lowPrice;
        public double volume;
        public double quoteVolume;
        public string openTime;
        public string closeTime;
        public int fristId;
        public int lastId28460;
        public int count;

    }


    public interface ISource
    {
        TikerAndTimeFrame[] TikerFrame { get; set; }
        BacktestSettings[] Settings { get; set; }
        string reportPath { get; set; }


    }

}
