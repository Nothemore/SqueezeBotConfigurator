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
            #region "Тестирование"
            //var initTime = DateTime.UtcNow;
            //var intScope = 5400;
            //var stack = new Stack<Tuple<DateTime, int>>();
            //while (intScope > 1000)
            //{
            //    initTime = initTime.AddMinutes(-1000);
            //    intScope -= 1000;
            //    var tupleTime = initTime;
            //    stack.Push(Tuple.Create(tupleTime, 1000));
            //}
            //initTime = initTime.AddMinutes(-intScope);
            //stack.Push(Tuple.Create(initTime, intScope));

            //foreach (var item in stack)
            //{
            //    Console.WriteLine($"{item.Item1} + {item.Item2}");

            //}
            //Console.ReadKey();
            //return;
            #endregion

            MainInDev(null);
        }




        static void MainInDev(string[] args)
        {
            //Отдельная еденица для работы с консолью ?
            Console.WriteLine("Выберите источник данных");
            Console.WriteLine($"\t-введите 1 для чтения данных из кода");
            Console.WriteLine($"\t-введите 2 для чтения данных из файла");
            Console.WriteLine($"\t-введите 3 для чтения из интернета");
            string input = string.Empty;
            var sourceNotSelected = true;
            while (sourceNotSelected)
            {
                input = Console.ReadLine();
                if (input == "1" || input == "2" || input == "3")
                    sourceNotSelected = false;
                else Console.WriteLine("Введено недопустимое значение");
            }

            ISource source = null;
            switch (input)
            {
                case "1":
                    source = new SourceCode();
                    break;
                case "2":
                    source = new SourceFile();
                    break;
                case "3":
                    source = new SourceWeb(false);
                    break;
            }

            TikerAndTimeFrame[] charts = source.TikerFrame;
            BacktestSettings[] settings = source.Settings;
            string outputFileFullName = source.ReportPath;
            var inScopeCandeCount = 1000;
            var topResultCount = 100;
            var currentPairIndex = 1;
            if (charts.Length == 0) return;
            Console.WriteLine($"Найдено пар - {charts.Length}");
            var reports = new List<BacktestReport>();
            var creationTime = DateTime.Now.ToString();
            foreach (var chart in charts)
            {
                var dataSet = new DataSet(inScopeCandeCount, chart.Tiker, chart.TimeFrame);
                if (!dataSet.initCorrect) continue;
                var backtestManager = new BacktestManager();
                var backtestReport = backtestManager.GetReport(settings,
                                                                dataSet,
                                                                $"{chart.Tiker} {chart.TimeFrame.AsQuery()}",
                                                                creationTime,
                                                                true,
                                                                inScopeCandeCount);
                Console.WriteLine($"Расчет завершен {currentPairIndex}/{charts.Length}");
                currentPairIndex++;
                if (backtestReport.Configs.All(x => x.takeCount == 0)) continue;
                reports.Add(backtestReport);
            }

            if (source is SourceWeb)
            {
                var topConfigs = reports
                    .SelectMany(x => x.Configs)
                    .Where(x => x.takeCount > 0)
                    .OrderByDescending(x => x.totalProfit)
                    .ThenByDescending(x => x.stopCount)
                    .Take(topResultCount)
                    .ToList();

                var marketReport = new BacktestReport();
                marketReport.Configs = topConfigs;
                marketReport.Date = creationTime;
                marketReport.FileName = "OneMinuteAll>70";
                marketReport.CandleCount = inScopeCandeCount;
                reports = new List<BacktestReport>() { marketReport };
            }

            //Отдельная еденица для работы с отчетом ? 
            var jsonFilePath = outputFileFullName;
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
