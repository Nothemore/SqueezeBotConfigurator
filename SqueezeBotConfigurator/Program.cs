using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace SqueezeBotConfigurator
{
    class Program
    {
        static void Main(string[] args)
        {
            MainInDev(null);
        }













        static void MainProd(string[] args)
        {
            var currentPath = Assembly.GetExecutingAssembly().Location;
            var path = currentPath.Replace("SqueezeBotConfigurator.exe", "Settings.json");

            ExternalSettings externalSetting;
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                externalSetting = (ExternalSettings)serializer.Deserialize(file, typeof(ExternalSettings));
            }


            // var files = new[]
            //{
            //          new TikerAndTimeFrame(   Tiker.STORJUSDT, TimeFrame.oneMinute ),
            // };

            var files = externalSetting.tikersAndFrames;

            var totalPairCount = files.Length;
            var currentPairIndex = 0;
            if (totalPairCount > 0) currentPairIndex = 1;
            Console.WriteLine($"Найдено пар - {totalPairCount}");

            //var directoryPath = @"C:\Users\Nocturne\Desktop\Новая папка (5)";
            var directoryPath = currentPath.Replace(@"\SqueezeBotConfigurator.exe", string.Empty);

            var inScopeCandeCount = 1000;
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

            var reports = new List<BacktestReport>(files.Count() * Settings.Length);
            var creationTime = DateTime.Now.ToString();
            foreach (var file in files)
            {
                var dataSet = new DataSet(inScopeCandeCount, file.Tiker, file.TimeFrame);
                if (!dataSet.initCorrect) continue;
                var configs = new List<Config>(Settings.Length * configsCount);
                var backtestReport = CreatReport(Settings, dataSet, $"{file.Tiker} {file.TimeFrame.AsQuery()}", creationTime);
                reports.Add(backtestReport);
                Console.WriteLine($"Расчет завершен {currentPairIndex}/{totalPairCount}");
                currentPairIndex++;
            }

            var jsonFilePath = directoryPath + @"\SqResult.json";
            using (StreamWriter file = File.CreateText(jsonFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, reports);
            }
            Console.WriteLine("Работа программы завершена. Нажмите любую клавишу");
            Console.ReadKey();

        }

        static void MainInDev(string[] args)
        {
            var readWriteCondition = new ReadWriteCondition(false);
            TikerAndTimeFrame[] files;
            BacktestSettings[] Settings;
            var configsCount = 10;

            if (readWriteCondition.FromExternalFile)
            {
                files = readWriteCondition.tikerAndFrames;
                Settings = readWriteCondition.Settings;
            }


            else
            {
                files = new TikerAndTimeFrame[]
                     {
                         new TikerAndTimeFrame(Tiker.MTLUSDT,TimeFrame.oneMinute),
                         new TikerAndTimeFrame(Tiker.STORJUSDT,TimeFrame.oneMinute)
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


            }

            var totalPairCount = files.Length;
            var currentPairIndex = 0;
            if (totalPairCount > 0) currentPairIndex = 1;
            Console.WriteLine($"Найдено пар - {totalPairCount}");
            var inScopeCandeCount = 1000;

            var reports = new List<BacktestReport>(files.Count() * Settings.Length);
            var creationTime = DateTime.Now.ToString();
            foreach (var file in files)
            {
                var dataSet = new DataSet(inScopeCandeCount, file.Tiker, file.TimeFrame);
                if (!dataSet.initCorrect) continue;
                var configs = new List<Config>(Settings.Length * configsCount);
                var backtestReport = CreatReport(Settings, dataSet, $"{file.Tiker} {file.TimeFrame.AsQuery()}", creationTime);
                backtestReport.Configs = backtestReport.Configs.OrderByDescending(x => x.totalProfit).ThenBy(x => x.stopCount).ThenBy(x => x.takeCount).ThenBy(x => x.tradeOpenTrigger).ToList();
                reports.Add(backtestReport);
                Console.WriteLine($"Расчет завершен {currentPairIndex}/{totalPairCount}");
                currentPairIndex++;
            }

            var jsonFilePath = readWriteCondition.ResultFileFullName;
            using (StreamWriter file = File.CreateText(jsonFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, reports);
            }
            Console.WriteLine("Работа программы завершена. Нажмите любую клавишу");
            Console.ReadKey();

        }

        static BacktestReport CreatReport(BacktestSettings[] Settings, DataSet dataSet, string reportName, string creationTime, bool useMultiThreading = true, int inScopeCandeCount = 1000)
        {

            var configsCount = Settings.Sum(x => x.configCount);
            var configs = new List<Config>(configsCount);
            if (useMultiThreading)//
            {
                //Многопоточный вызов
                var tasks = new Task[Settings.Length];
                var backtests = new Backtest[Settings.Length];

                for (int i = 0; i < tasks.Length; i++)
                {
                    var backtest = new Backtest(Settings[i], dataSet);
                    backtests[i] = backtest;
                    Action currentTest;
                    if (Settings[i].calculateStop)
                        currentTest = () => { backtest.RunTestCalculatedStop(); };
                    else
                        currentTest = () => { backtest.RunTestDefaltStop(); };
                    tasks[i] = new Task(currentTest);
                    tasks[i].Start();
                }
                Task.WaitAll(tasks);

                for (int i = 0; i < tasks.Length; i++)
                {
                    configs.AddRange(backtests[i].Configs);
                }
            }
            else
            {
                //Однопоточный вызов
                for (int i = 0; i < Settings.Length; i++)
                {
                    var currentTest = new Backtest(Settings[i], dataSet);
                    if (Settings[i].calculateStop)
                        currentTest.RunTestCalculatedStop();
                    else
                        currentTest.RunTestDefaltStop();

                    configs.AddRange(currentTest.Configs);
                }

            }


            return new BacktestReport()//
            {
                Date = creationTime,
                FileName = reportName,
                Configs = configs,
                CandleCount = inScopeCandeCount
            };
        }

    }







    public class Backtest
    {
        public BacktestSettings Settings;
        public DataSet Data;
        public List<Config> Configs { get; private set; }
        public Backtest(BacktestSettings settings, DataSet data)
        {
            Settings = settings;
            Data = data;
        }

        public void RunTestDefaltStop()
        {
            var bestConfigs = new List<Config>(Settings.configCount + 1);
            for (double buyTrigger = Settings.buyTriggerMin; buyTrigger <= Settings.buyTriggerMax; buyTrigger += Settings.buyTriggerStep)
            {
                Settings.sellTriggerMax = buyTrigger * Settings.buySellRatio;
                for (double sellTrigger = Settings.sellTriggerMin; sellTrigger <= Settings.sellTriggerMax; sellTrigger += Settings.sellTriggerStep)
                {

                    var currentConfig = new Config();
                    currentConfig.useStop = Settings.useStopLoss;
                    currentConfig.stopTrigger = Settings.stopTriggerDefaul;
                    currentConfig.tradeOpenTrigger = Settings.tradeOpenTrigger;
                    currentConfig.buyTrigger = buyTrigger;
                    currentConfig.sellTrigger = sellTrigger;
                    currentConfig.RunTest(Data, Settings.tradeOpenTrigger);


                    bestConfigs.Add(currentConfig);
                    //Вопрос о сортировки об отборе элементов передать в сеттенгс
                    //bestConfigs = bestConfigs.OrderByDescending(x => x.totalProfit).Take(Settings.configCount).ToList();
                    bestConfigs = Settings.ConfigFilter(bestConfigs);



                }
            }
            //Configs = bestConfigs.OrderByDescending(x => x.takeCount).ThenBy(x => x.stopCount).ThenBy(x => x.tradeOpenTrigger).ToList();
            Configs = bestConfigs.OrderByDescending(x => x.totalProfit).ThenBy(x => x.stopCount).ThenBy(x => x.takeCount).ThenBy(x => x.tradeOpenTrigger).ToList();
        }

        public void RunTestCalculatedStop()
        {
            for (double stopTrigger = Settings.stopTriggerMax; stopTrigger >= Settings.stopTriggerMin; stopTrigger -= Settings.stopTriggerStep)
            {
                Settings.stopTriggerDefaul = stopTrigger;
                RunTestDefaltStop();
            }
        }






    }

    public class Config
    {
        public double buyTrigger;
        public double sellTrigger;
        public double stopTrigger;
        public double totalProfit = 100;
        public int takeCount;
        public int stopCount;
        [JsonConverter(typeof(StringEnumConverter))]
        public TradeOpenTrigger tradeOpenTrigger;

        [NonSerialized]
        public bool useStop;

        [NonSerialized]
        public bool isDealOpen = false;

        [NonSerialized]
        public double buyPrice;

        [NonSerialized]
        public double sellPrice;

        [NonSerialized]
        public double stopPrice;

        [NonSerialized]
        public int openDealCandleIndex;

        [NonSerialized]
        public int closeCandleIndex;

        public void RunTest(DataSet data, TradeOpenTrigger tradeOpenTrigger)
        {
            this.tradeOpenTrigger = tradeOpenTrigger;
            double[] triggerPrice = data.tradeOpenTriggerValues[(int)tradeOpenTrigger];
            for (int currentCandleIndex = 1; currentCandleIndex < data.inScopeCandeCount - 1; currentCandleIndex++)
            {
                //Откроем сделку?
                if (!isDealOpen)
                {
                    //var delta = (triggerPrice[currentCandleIndex - 1] / data.low[currentCandleIndex] - 1);
                    var delta = 1 - data.low[currentCandleIndex] / triggerPrice[currentCandleIndex - 1];
                    if (delta >= buyTrigger / 100)
                    {
                        isDealOpen = true;
                        buyPrice = triggerPrice[currentCandleIndex - 1] * (1 - buyTrigger / 100);
                        sellPrice = buyPrice * (1 + sellTrigger / 100);
                        stopPrice = buyPrice * (1 - stopTrigger / 100);
                        openDealCandleIndex = currentCandleIndex;
                    }
                }
                //Закроем по тейку ?
                if (isDealOpen && data.close[currentCandleIndex] > sellPrice)
                {
                    isDealOpen = false;
                    totalProfit *= (1 + sellTrigger / 100);
                    closeCandleIndex = currentCandleIndex;
                    takeCount++;
                }

                //Закроем по стопу ?
                if (isDealOpen
                    && useStop
                    && currentCandleIndex > openDealCandleIndex
                    && stopPrice > data.low[currentCandleIndex])
                {
                    isDealOpen = false;
                    totalProfit *= (1 - stopTrigger / 100);
                    closeCandleIndex = currentCandleIndex;
                    stopCount++;
                }

            }
        }

        public void WriteStatistic()
        {
            var builder = new StringBuilder();
            builder.Append($"\nТриггер покупки {buyTrigger}, триггер продажи {sellTrigger}, Стоп триггер {stopTrigger}");
            builder.Append($"\nКоличество положительных сделок {takeCount}, количество отрицательных сделок {stopCount}");
            builder.Append($"\nПрофитность {totalProfit}, config {tradeOpenTrigger}");
            //builder.Append($"\nПрофитность {totalProfit}, config {(Config)config} + {openDealCandleIndex} +{closeCandleIndex}  + {sellPrice } + { buyPrice}");
            //builder.AppendLine();
            Console.WriteLine(builder.ToString());
        }


    }

    public class DataSet
    {
        public double[] low;
        public double[] high;
        public double[] close;
        public double[] open;
        public double[] openCloseAverage;
        public double[] highLowAverage;
        public int inScopeCandeCount;
        public bool initCorrect = true;
        private string Path { get; set; }//передавать напрямую в метод 
        public double[][] tradeOpenTriggerValues = new double[6][];

        public DataSet(int candleCount, string path)
        {
            Path = path;
            this.inScopeCandeCount = candleCount;
            InitArrays(candleCount);
            FillDataSet();
        }

        public DataSet(int candleCount, Tiker tiker, TimeFrame timeFrame)
        {
            this.inScopeCandeCount = candleCount;
            var requestCandle = 1000;
            InitArrays(requestCandle);

            WebRequest myRequest = WebRequest.Create($"https://api.binance.com/api/v3/klines?symbol={tiker.ToString()}&interval={timeFrame.AsQuery()}&limit={requestCandle}");
            WebResponse myResponse;

            try
            {
                myResponse = myRequest.GetResponse();
            }
            catch (Exception)
            {
                Console.Write($"Ошибка в запросе данных {tiker}");
                initCorrect = false;
                return;
            }

            using (Stream stream = myResponse.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var content = reader.ReadLine();
                    var extractedDate = JsonConvert.DeserializeObject<double[][]>(content);
                    for (int i = 0; i < extractedDate.Length; i++)
                    {
                        open[i] = extractedDate[i][1];
                        high[i] = extractedDate[i][2];
                        low[i] = extractedDate[i][3];
                        close[i] = extractedDate[i][4];
                        openCloseAverage[i] = (open[i] + close[i]) / 2;
                        highLowAverage[i] = (high[i] + low[i]) / 2;
                    }
                }
            }
        }

        public void InitArrays(int inScopeCandeCount)
        {
            low = new double[inScopeCandeCount];
            close = new double[inScopeCandeCount];
            open = new double[inScopeCandeCount];
            high = new double[inScopeCandeCount];
            openCloseAverage = new double[inScopeCandeCount];
            highLowAverage = new double[inScopeCandeCount];

            tradeOpenTriggerValues[0] = low;
            tradeOpenTriggerValues[1] = close;
            tradeOpenTriggerValues[2] = open;
            tradeOpenTriggerValues[3] = high;
            tradeOpenTriggerValues[4] = openCloseAverage;
            tradeOpenTriggerValues[5] = highLowAverage;
        }

        private void FillDataSet()
        {
            //Не берет последнюю свечу хз почему надо думать
            var lastCandles = new Queue<string>(inScopeCandeCount);
            using (var streamReader = new StreamReader(Path))
            {
                streamReader.ReadLine();
                var counter = 0;
                while (streamReader.Peek() >= 0)
                {
                    if (counter < inScopeCandeCount)
                    {
                        lastCandles.Enqueue(streamReader.ReadLine());
                        counter++;
                    }
                    else
                    {
                        lastCandles.Dequeue();
                        lastCandles.Enqueue(streamReader.ReadLine());

                    }
                }
            }

            var lastCandlesArray = lastCandles.ToArray();
            for (int i = 0; i < inScopeCandeCount - 1; i++)
            {
                var lineAsArray = lastCandlesArray[i].Split(',');
                open[i] = double.Parse(lineAsArray[1].Replace(".", ","));
                high[i] = double.Parse(lineAsArray[2].Replace(".", ","));
                low[i] = double.Parse(lineAsArray[3].Replace(".", ","));
                close[i] = double.Parse(lineAsArray[4].Replace(".", ","));
                openCloseAverage[i] = (open[i] + close[i]) / 2;
                highLowAverage[i] = (high[i] + low[i]) / 2;
            }
        }
    }

    public class BacktestSettings
    {
        public double buyTriggerMin = 1.2;
        public double buyTriggerMax = 5;
        public double buyTriggerStep = 0.01;

        public double sellTriggerMin = 0.55;
        public double sellTriggerMax = 5;
        public double sellTriggerStep = 0.01;

        public double stopTriggerMin = 2;
        public double stopTriggerMax = 5;
        public double stopTriggerStep = 0.05;
        public double stopTriggerDefaul = 5;
        public bool useStopLoss = true;
        public bool calculateStop = true;

        public double buySellRatio = 0.4;
        public int configCount = 10;
        public TradeOpenTrigger tradeOpenTrigger;
        public Func<List<Config>, List<Config>> ConfigFilter;

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
        }
    }

    public enum TradeOpenTrigger
    {
        low = 0,
        close = 1,
        open = 2,
        high = 3,
        openClose = 4,
        highLow = 5
    }

    public class BacktestReport
    {
        public List<Config> Configs { get; set; }
        public string FileName;
        public int CandleCount;
        public BacktestSettings BacktestSettings;
        public string Date;
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
        SUPERUSDT
    }
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TimeFrame
    {
        oneMinute,
        threeMinutes,
        fiveMinutes
    }

    public static class TimeFrameExtensions
    {
        public static string AsQuery(this TimeFrame timeFrame)
        {
            if (timeFrame == TimeFrame.oneMinute) return "1m";
            if (timeFrame == TimeFrame.threeMinutes) return "3m";
            if (timeFrame == TimeFrame.fiveMinutes) return "5m";
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


}
