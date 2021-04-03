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



           // var tiime = DateTime.UtcNow;
           // Console.WriteLine((Int32)tiime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);
           // //var asdfa = new TimeSpan(0, 0, 1000, 0);
           //tiime =  tiime.AddMinutes(-1000);

           // Console.WriteLine((Int32)tiime.Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

          
           // //Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
           // //DateTime.UtcNow.AddMinutes(-10);


           // //Console.WriteLine(unixTimestamp);
           // Console.ReadKey();

            //var sette = new ExternalSettings();
            //sette.settings = new BacktestSettings(TradeOpenTrigger.open);

            //sette.tikersAndFrames = new[] { new TikerAndTimeFrame(Tiker.ADAUSDT, TimeFrame.oneMinute) };

            //var jsonFilePath = @" C:\Users\Nocturne\Desktop\inDev\Settings.json";
            //using (StreamWriter file = File.CreateText(jsonFilePath))
            //{
            //    JsonSerializer serializer = new JsonSerializer();
            //    serializer.Formatting = Formatting.Indented;
            //    serializer.Serialize(file, sette);
            //}




            //return;


            MainInDev(null);
        }




        static void MainInDev(string[] args)
        {

            Console.WriteLine("Выберите источник данных");
            Console.WriteLine($"\t-введите 1 для чтения данных из кода");
            Console.WriteLine($"\t-введите 2 для чтения данных из файла");
            Console.WriteLine($"\t-введите 3 для чтения из интернета");
            string input = string.Empty;
            var stateCoplite = true;
            while (stateCoplite)
            {
                input = Console.ReadLine();
                if (input == "1" || input == "2" || input == "3")
                    stateCoplite = false;
                else Console.WriteLine("Введено недопустимое значение");
            }

            ISource source=null;
            switch (input)
            {
                case "1": source = new SourceCode();
                    break;

                case "2":
                    source = new SourceFile();
                    break;
                case "3":
                    source = new SourceWeb(false);
                    break;
            }

            TikerAndTimeFrame[] files = source.TikerFrame;
            BacktestSettings[] Settings = source.Settings;
            string outPutFileName = source.reportPath;

            var inScopeCandeCount = 1000;
            var oneSheetTopSize = 100;


            var currentPairIndex = 1;
            if (files.Length == 0) return;
            Console.WriteLine($"Найдено пар - {files.Length}");
            var reports = new List<BacktestReport>();
            var creationTime = DateTime.Now.ToString();
            foreach (var file in files)
            {
                var dataSet = new DataSet(inScopeCandeCount, file.Tiker, file.TimeFrame);
                if (!dataSet.initCorrect) continue;
                var backtestManager = new BacktestManager();
                var backtestReport = backtestManager.GetReport(Settings, dataSet, $"{file.Tiker} {file.TimeFrame.AsQuery()}", creationTime, true, inScopeCandeCount);


                Console.WriteLine($"Расчет завершен {currentPairIndex}/{files.Length}");
                currentPairIndex++;
                if (backtestReport.Configs.All(x => x.takeCount == 0)) continue;
                reports.Add(backtestReport);
                
            }

            if (source is SourceWeb)
            {
                var topConfigs = reports
                    .SelectMany(x => x.Configs)
                    .Where(x=>x.takeCount>0)
                    .OrderByDescending(x => x.totalProfit)
                    .ThenByDescending(x=>x.stopCount)
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
