using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net;
using System.IO;

namespace SqueezeBotConfigurator
{
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
        public Tiker sourceTiker;

        public DataSet(int candleCount, string path)
        {
            Path = path;
            this.inScopeCandeCount = candleCount;
            InitArrays(candleCount);
            FillDataSet();
        }



        /// <summary>
        /// Поддержка candleCount не реализована
        /// </summary>
        /// <param name="candleCount"></param>
        /// <param name="tiker"></param>
        /// <param name="timeFrame"></param>
        public DataSet(int candleCount, Tiker tiker, TimeFrame timeFrame)
        {
            this.inScopeCandeCount = candleCount;
            this.sourceTiker = tiker;
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
            //Не берет последнюю свечу 
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
}
