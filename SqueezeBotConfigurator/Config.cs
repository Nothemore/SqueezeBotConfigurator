using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;

namespace SqueezeBotConfigurator
{
    public class Config
    {
        public double buyTrigger;
        public double sellTrigger;
        public double stopTrigger;
        public double totalProfit = 100;
        public int takeCount;
        public int stopCount = -1;
        public Tiker tiker;
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
        public int closeDealCandleIndex;

        private double commission = 0.2;

        public void RunTest(DataSet data, TradeOpenTrigger tradeOpenTrigger)
        {
            this.tradeOpenTrigger = tradeOpenTrigger;
            this.tiker = data.sourceTiker;
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
                    totalProfit *= (1 + (sellTrigger - commission) / 100);
                    closeDealCandleIndex = currentCandleIndex;
                    takeCount++;
                }

                //Закроем по стопу ?
                if (isDealOpen
                    && useStop
                    && currentCandleIndex > openDealCandleIndex
                    && stopPrice > data.low[currentCandleIndex])
                {
                    isDealOpen = false;
                    totalProfit *= (1 - (stopTrigger + commission) / 100);
                    closeDealCandleIndex = currentCandleIndex;
                    if (stopCount == -1) stopCount = 1;
                    else stopCount++;
                }

            }
        }

        public void WriteStatistic()
        {
            var builder = new StringBuilder();
            builder.Append($"\nТриггер покупки {buyTrigger}, триггер продажи {sellTrigger}, Стоп триггер {stopTrigger}");
            builder.Append($"\nКоличество положительных сделок {takeCount}, количество отрицательных сделок {stopCount}");
            builder.Append($"\nПрофитность {totalProfit}, config {tradeOpenTrigger}");
            Console.WriteLine(builder.ToString());
        }


    }
}
