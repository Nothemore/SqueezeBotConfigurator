using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Diagnostics;


namespace SqueezeBotConfigurator
{
    class Program
    {
        static void Main(string[] args)
        {

            var directoryPath = @"C:\Users\Nocturne\Desktop\Новая папка (5)";
            var files = Directory.GetFiles(directoryPath, "*.csv");

            var inScopeCandeCount = 1440;
            var configsCount = 10;

            var Settings = new BacktestSettings[]
            {
                new BacktestSettings(TradeOpenTrigger.open)     {configCount = configsCount },
                new BacktestSettings(TradeOpenTrigger.close)    {configCount = configsCount },
                new BacktestSettings(TradeOpenTrigger.openClose){configCount = configsCount },
                new BacktestSettings(TradeOpenTrigger.high)     {configCount = configsCount },
                new BacktestSettings(TradeOpenTrigger.low)      {configCount = configsCount },
                new BacktestSettings(TradeOpenTrigger.highLow)  {configCount = configsCount }
            };

            var reports = new List<BacktestReport>(files.Count() * Settings.Length);
            var date = DateTime.Now.ToString();

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var dataSet = new DataSet(inScopeCandeCount, fileInfo.FullName);
                var configs = new List<Config>(Settings.Length * configsCount);

                //Многопоточный вызов
                var tasks = new Task[Settings.Length];
                var backtests = new BacktestProvider[Settings.Length];

                for (int i = 0; i < tasks.Length; i++)
                {
                    var backtest = new BacktestProvider(Settings[i], dataSet);
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

                ////Однопоточный вызов
                //for (int i = 0; i < Settings.Length; i++)
                //{
                //    var currentTest = new BacktestProvider(Settings[i], dataSet);
                //    if (Settings[i].calculateStop)
                //        currentTest.RunTestCalculatedStop();
                //    else
                //        currentTest.RunTestDefaltStop();

                //    configs.AddRange(currentTest.Configs);
                //}



                //configs.ForEach(x => x.WriteStatistic());

                //Console.ReadKey();
                //return;








                var backtestReport = new BacktestReport()
                {
                    Date = date,
                    FileName = fileInfo.Name,
                    Configs = configs,
                    CandleCount = inScopeCandeCount
                };
                reports.Add(backtestReport);
            }


            var jsonFilePath = directoryPath + @"\SqResult.json";
            using (StreamWriter file = File.CreateText(jsonFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, reports);
            }





            //var result = stricts.Test(dataSet,100);
            //result.ForEach(x => x.WriteStatistic());
            //Console.ReadKey();
        }
    }


    public class BacktestProvider
    {
        public BacktestSettings Settings;
        public DataSet Data;
        public List<Config> Configs { get; private set; }
        public BacktestProvider(BacktestSettings settings, DataSet data)
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
            Configs = bestConfigs.OrderByDescending(x => x.takeCount).ThenBy(x => x.stopCount).ThenBy(x => x.tradeOpenTrigger).ToList();

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
        private string Path { get; set; }
        public double[][] tradeOpenTriggerValues = new double[6][];

        public DataSet(int candleCount, string path)
        {
            Path = path;
            this.inScopeCandeCount = candleCount;

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

            FillDataSet();
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
        public double stopTriggerMax = 8;
        public double stopTriggerStep = 0.1;
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
}
