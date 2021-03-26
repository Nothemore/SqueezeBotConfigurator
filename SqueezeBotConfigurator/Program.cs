using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace SqueezeBotConfigurator
{
    class Program
    {
        static void Main(string[] args)
        {

            var directoryPath = @"C:\Users\Nocturne\Desktop\Новая папка (5)";
            var files = Directory.GetFiles(directoryPath, "*.csv");
            var inScopeCandeCount = 1440;

            //var path = @"C:\Users\Nocturne\Desktop\Новая папка (5)\BINANCE_FILUSDT, 1.txt";
            //var dataSet = new DataSet(inScopeCandeCount, path);

            var stricts = new Stricts() { sellTrigerStep = 0.01, buyTriggerStep = 0.01 };
            var reportCandleCount = 100;

            var results = new List<TestResult>(files.Count());

            var date = DateTime.Now.ToString();
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var localDataSet = new DataSet(inScopeCandeCount, fileInfo.FullName);
                var testResult = new TestResult()
                {
                    FileName = fileInfo.Name,
                    Configs = stricts.Test(localDataSet, reportCandleCount),
                    CandleCount = inScopeCandeCount,
                    Strict = stricts,
                    Date = date
                };
                results.Add(testResult);

            }


            var jsonFilePath = directoryPath + @"\SqResult.json";
            using (StreamWriter file = File.CreateText(jsonFilePath))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Formatting = Formatting.Indented;
                serializer.Serialize(file, results);
            }





            //var result = stricts.Test(dataSet,100);
            //result.ForEach(x => x.WriteStatistic());
            //Console.ReadKey();
        }
    }

    public class Config
    {
        public double stopTrigger;
        public double sellTrigger;
        public double buyTrigger;
        public double totalProfit = 100;
        public int takeCount;
        public int stopCount;
        [JsonConverter(typeof(StringEnumConverter))]
        public TestCase config;

        [NonSerialized]
        public bool useStop = true;

        [NonSerialized]
        public bool isDealOpen = false;

        [NonSerialized]
        public double buyPrice;

        [NonSerialized]
        public double sellPrice;

        [NonSerialized]
        public int openDealCandleIndex;

        [NonSerialized]
        public int closeCandleIndex;



        public void Test(DataSet data, int testCase)
        {
            double[] currentTestCase = data.buyTriggerFlags[testCase];
            for (int currentCandleIndex = 1; currentCandleIndex < data.inScopeCandeCount - 1; currentCandleIndex++)
            {
                //Откроем сделку?
                if (!isDealOpen)
                {
                    var delta = (currentTestCase[currentCandleIndex - 1] / data.low[currentCandleIndex] - 1);
                    if (delta >= buyTrigger / 100)
                    {
                        isDealOpen = true;
                        var triggerPrice = currentTestCase[currentCandleIndex - 1];
                        buyPrice = currentTestCase[currentCandleIndex - 1] * (1 - buyTrigger / 100);
                        sellPrice = buyPrice * (1 + sellTrigger / 100);
                        openDealCandleIndex = currentCandleIndex;
                    }
                }
                //Закроем сделку?
                if (isDealOpen)
                {
                    if (data.close[currentCandleIndex] > sellPrice)
                    {
                        isDealOpen = false;
                        totalProfit *= (1 + sellTrigger / 100);
                        closeCandleIndex = currentCandleIndex;
                        takeCount++;
                    }
                    if (isDealOpen && useStop && currentCandleIndex > openDealCandleIndex)
                    {
                        var stopPrice = buyPrice * (1 - stopTrigger / 100);
                        var currentlow = data.low[currentCandleIndex];
                        if (stopPrice > data.low[currentCandleIndex])
                        {
                            totalProfit *= (1 - stopTrigger / 100);
                            isDealOpen = false;
                            closeCandleIndex = currentCandleIndex;
                            stopCount++;
                        }
                    }
                }
            }








        }

        public void WriteStatistic()
        {
            var builder = new StringBuilder();
            builder.Append($"\nТриггер покупки {buyTrigger}, триггер продажи {sellTrigger}, Стоп триггер {stopTrigger}");
            builder.Append($"\nКоличество положительных сделок {takeCount}, количество отрицательных сделок {stopCount}");
            builder.Append($"\nПрофитность {totalProfit}, config {(TestCase)config}");
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
        public double[][] buyTriggerFlags = new double[6][];


        public DataSet(int candleCount, string path)
        {
            Path = path;
            this.inScopeCandeCount = candleCount;
            low = new double[inScopeCandeCount];
            high = new double[inScopeCandeCount];
            close = new double[inScopeCandeCount];
            open = new double[inScopeCandeCount];
            openCloseAverage = new double[inScopeCandeCount];
            highLowAverage = new double[inScopeCandeCount];
            FillDataSet();
            buyTriggerFlags[0] = low;
            buyTriggerFlags[1] = close;
            buyTriggerFlags[2] = open;
            buyTriggerFlags[3] = high;
            buyTriggerFlags[4] = openCloseAverage;
            buyTriggerFlags[5] = highLowAverage;
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

    public class Stricts
    {
        public double maxBuyTrigger = 5;
        public double minBuyTrigger = 1.2;
        public double buyTriggerStep = 0.1;
        public double maxSellTrigger = 5;
        public double minSellTrigger = 0.55;
        public double sellTrigerStep = 0.1;
        public double minStopTrigger = 2;
        public double maxStopTrigger = 8;
        public double tempStopLoss = 5;
        public double stopTriggerStep = 0.1;
        public bool useStopLoss = true;
        private double[][] buyTriggerFlags = new double[6][];
        public double multiplier = 0.33;

        public List<Config> Test(DataSet dataSet, int configCount = 10)
        {
            buyTriggerFlags[0] = dataSet.low;
            buyTriggerFlags[1] = dataSet.close;
            buyTriggerFlags[2] = dataSet.open;
            buyTriggerFlags[3] = dataSet.high;
            buyTriggerFlags[4] = dataSet.openCloseAverage;
            buyTriggerFlags[5] = dataSet.highLowAverage;
            var bestConfigs = new List<Config>(configCount + 1);
            Config currentConfig;

            for (int i = 0; i < buyTriggerFlags.Length; i++)
            {

                for (double buyTrigger = minBuyTrigger; buyTrigger <= maxBuyTrigger; buyTrigger += buyTriggerStep)
                {
                    maxSellTrigger = buyTrigger * multiplier;
                    for (double sellTrigger = minSellTrigger; sellTrigger <= maxSellTrigger; sellTrigger += sellTrigerStep)
                    {
                        //for (double stopTrigger = minStopTrigger; stopTrigger <= maxStopTrigger; stopTrigger += stopTriggerStep)
                        //{
                        currentConfig = new Config();
                        currentConfig.isDealOpen = false;
                        currentConfig.useStop = useStopLoss;
                        currentConfig.buyTrigger = buyTrigger;
                        currentConfig.sellTrigger = sellTrigger;
                        currentConfig.stopTrigger = tempStopLoss;
                        currentConfig.totalProfit = 100;
                        currentConfig.config = (TestCase)i;
                        currentConfig.Test(dataSet, i);
                        bestConfigs.Add(currentConfig);
                        bestConfigs = bestConfigs.OrderByDescending(x => x.totalProfit).Take(configCount).ToList();
                        //}
                    }
                }
            }
            return bestConfigs = bestConfigs.OrderByDescending(x => x.takeCount).ThenBy(x => x.stopCount).ThenBy(x => x.config).ToList();


        }
    }

    public enum TestCase
    {
        low = 0,
        close = 1,
        open = 2,
        high = 3,
        openClose = 4,
        highLow = 5
    }

    public class TestResult
    {
        public List<Config> Configs { get; set; }
        public string FileName;
        public int CandleCount;
        public Stricts Strict;
        public string Date;
    }
}
