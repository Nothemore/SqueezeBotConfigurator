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


            TikerAndTimeFrame[] files = null;
            BacktestSettings[] Settings = null;
            string outPutFileName = null;

            var inScopeCandeCount = 1000;
            var oneSheetTopSize = 100;
            var mergeInOneSheet = false;


            var currentPairIndex = 1;
            if (files.Length > 0) return;
            Console.WriteLine($"Найдено пар - {files.Length}");
            var reports = new List<BacktestReport>();
            var creationTime = DateTime.Now.ToString();
            foreach (var file in files)
            {
                var dataSet = new DataSet(inScopeCandeCount, file.Tiker, file.TimeFrame);
                if (!dataSet.initCorrect) continue;
                var backtestManager = new BacktestManager();
                var backtestReport = backtestManager.GetReport(Settings, dataSet, $"{file.Tiker} {file.TimeFrame.AsQuery()}", creationTime, true, inScopeCandeCount);

                currentPairIndex++;
                Console.WriteLine($"Расчет завершен {currentPairIndex}/{files.Length}");
                if (backtestReport.Configs.All(x => x.takeCount == 0)) continue;
                reports.Add(backtestReport);
            }

            if (mergeInOneSheet)
            {
                var topConfigs = reports
                    .SelectMany(x => x.Configs)
                    .OrderByDescending(x => x.totalProfit)
                    .Take(oneSheetTopSize)
                    .ToList();

                var marketReport = new BacktestReport();
                marketReport.Configs = topConfigs;
                marketReport.Date = creationTime;
                marketReport.FileName = "OneMinuteAll";
                marketReport.CandleCount = inScopeCandeCount;
                reports = new List<BacktestReport>() { marketReport };
            }

            var jsonFilePath = outPutFileName;
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
