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
    class BacktestManager
    {
        public BacktestReport GetReport(BacktestSettings[] Settings, DataSet dataSet, string reportName, string creationTime, bool useMultiThreading = true, int inScopeCandeCount = 1000)
        {
            var configsCount = Settings.Sum(x => x.configCount);
            var configs = new List<Config>(configsCount);
            if (useMultiThreading)
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
            
            return new BacktestReport()
            {
                Date = creationTime,
                FileName = reportName,
                Configs = configs,
                CandleCount = inScopeCandeCount
            };
        }
    }
}
