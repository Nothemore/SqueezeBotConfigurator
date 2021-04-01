﻿using System;
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
using System.Threading;

namespace SqueezeBotConfigurator
{
    class Program
    {
        static void Main(string[] args)
        {

            var set = new BacktestSettings(TradeOpenTrigger.open);
            var bak = new Backtest(set, null);
            Console.WriteLine(bak.Configs.Count == 0);
            Console.ReadKey();

            return;
            MainInDev(null);
        }


       

        static void MainInDev(string[] args)
        {



            var readWriteCondition = new ReadWriteCondition(false);
            TikerAndTimeFrame[] files;
            BacktestSettings[] Settings;
            var configsCount = 10;
            var totalSearch = false;


            if (readWriteCondition.FromExternalFile)
            {
                files = readWriteCondition.tikerAndFrames;
                Settings = readWriteCondition.Settings;
            }


            else
            {
                files = new TikerAndTimeFrame[]
                     {

                         new TikerAndTimeFrame(Tiker.FILUSDT,TimeFrame.oneMinute),



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


            var topConfigsInMarket = new List<Config>();
            var fileToScan = new List<TikerAndTimeFrame>();
            if (totalSearch)
            {
                WebRequest myRequest = WebRequest.Create($"https://api.binance.com/api/v1/ticker/24hr");

                // Return the response.
                WebResponse myResponse = myRequest.GetResponse();
                using (Stream stream = myResponse.GetResponseStream())
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        var volumeFilter = 70 * Math.Pow(10, 6);
                        var test = reader.ReadLine();
                        Baka[] account = JsonConvert.DeserializeObject<Baka[]>(test);
                        var counter = 0;

                        account = account.Where(x => x.symbol.Contains("USDT")).ToArray();
                        var asda = account[0];
                        account = account.Where(x => x.quoteVolume > volumeFilter).ToArray();
                        asda = account[0];

                        account = account.Where(x => x.symbol.Substring(x.symbol.Length - 4) == "USDT").ToArray();
                        foreach (var item in account)
                        {
                            if (item.symbol.Contains("USDT")
                                && !item.symbol.Contains("UPUSDT")
                                && !item.symbol.Contains("DOWNUSDT")
                                && item.quoteVolume > volumeFilter)
                            {
                                Tiker result;
                                if (Enum.TryParse(item.symbol, out result))
                                {
                                    fileToScan.Add(new TikerAndTimeFrame(result, TimeFrame.oneMinute));
                                    counter++;

                                }
                            }

                        }
                        Console.WriteLine(counter);
                        Thread.Sleep(60000);
                        files = fileToScan.ToArray();
                        Console.ReadKey();

                    }
                }
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
                var backtestManager = new BacktestManager();
                var backtestReport = backtestManager.GetReport(Settings, dataSet, $"{file.Tiker} {file.TimeFrame.AsQuery()}", creationTime, true, inScopeCandeCount);
                if (backtestReport.Configs.All(x => x.takeCount == 0)) continue;
                Console.WriteLine($"Расчет завершен {currentPairIndex}/{totalPairCount}");
                currentPairIndex++;

                if (totalSearch)
                {
                    topConfigsInMarket.AddRange(backtestReport.Configs);
                    topConfigsInMarket = topConfigsInMarket.OrderBy(x => x.takeCount / x.stopCount).ThenByDescending(x => x.totalProfit).Take(100).ToList();
                }
                else
                {
                    backtestReport.Configs = backtestReport.Configs.OrderByDescending(x => x.totalProfit).ThenBy(x => x.stopCount).ThenBy(x => x.takeCount).ThenBy(x => x.tradeOpenTrigger).ToList();
                    reports.Add(backtestReport);
                }
            }

            if (totalSearch)
            {
                var backRep = new BacktestReport();
                backRep.Configs = topConfigsInMarket;
                backRep.Date = DateTime.Now.ToString();
                backRep.FileName = "AllinOneMinute";
                backRep.CandleCount = 1000;

                reports = new List<BacktestReport>()
                {
                    backRep

                };
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


    }

    

   

  

   


}
