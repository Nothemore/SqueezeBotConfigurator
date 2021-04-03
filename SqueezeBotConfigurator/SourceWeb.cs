using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqueezeBotConfigurator
{
    class SourceWeb : ISource
    {
        public string reportPath { get; set; }
        public BacktestSettings[] Settings { get; set; }
        public TikerAndTimeFrame[] TikerFrame { get; set; }

        public SourceWeb(bool fromFileSettings)
        {

            var fileToScan = new List<TikerAndTimeFrame>();
            var configsCount = 2;
            WebRequest myRequest = WebRequest.Create($"https://api.binance.com/api/v1/ticker/24hr");

            // Return the response.
            WebResponse myResponse = myRequest.GetResponse();
            using (Stream stream = myResponse.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    var volumeFilterMax = 100 * Math.Pow(10, 6);
                    var volumeFilterMin = 20 * Math.Pow(10, 6);
                    var tikerData = reader.ReadLine();
                    tikerResponse[] account = JsonConvert.DeserializeObject<tikerResponse[]>(tikerData);

                    account = account
                        .Where(x => x.symbol.Substring(x.symbol.Length - 4) == "USDT")
                        .Where(x => !x.symbol.Contains("UPUSDT"))
                        .Where(x => !x.symbol.Contains("DOWNUSDT"))
                        .Where(x => x.quoteVolume > volumeFilterMin)
                         .Where(x => x.quoteVolume < volumeFilterMax)
                        .ToArray();

                    foreach (var item in account)
                    {

                        Tiker result;
                        if (Enum.TryParse(item.symbol, out result))
                            fileToScan.Add(new TikerAndTimeFrame(result, TimeFrame.oneMinute));
                    }
                    Console.WriteLine("Технический перерыв на 1 минуту");
                    Thread.Sleep(60000);
                    TikerFrame = fileToScan.ToArray();

                    if (fromFileSettings)
                    {
                        Settings = new SourceFile().Settings;


                    }
                    else
                    {
                        var settings = new BacktestSettings(TradeOpenTrigger.open) { configCount = configsCount};
                        Settings = new BacktestSettings[]
                             {
                                 settings.DeepCopy(TradeOpenTrigger.open),
                                 settings.DeepCopy(TradeOpenTrigger.close),
                                 settings.DeepCopy(TradeOpenTrigger.openClose),
                                 settings.DeepCopy(TradeOpenTrigger.high),
                                 settings.DeepCopy(TradeOpenTrigger.low),
                                 settings.DeepCopy(TradeOpenTrigger.highLow),
                             };
                    }



                    reportPath = @"C:\Users\Nocturne\Desktop\inDev\SqResult.json";



                }
            }
        }
    }
}
