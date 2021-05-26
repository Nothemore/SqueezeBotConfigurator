using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SqueezeBotConfigurator
{
    class SourceFile : ISource
    {
        public string ReportPath { get; set; }
        public BacktestSettings[] Settings { get; set; }
        public TikerAndTimeFrame[] TikerFrame { get; set; }

        public SourceFile()
        {
            var currentPath = Assembly.GetExecutingAssembly().Location;
            var path = currentPath.Replace("SqueezeBotConfigurator.exe", "Settings.json");

            ExternalSettings externalSetting;
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                externalSetting = (ExternalSettings)serializer.Deserialize(file, typeof(ExternalSettings));
            }

            ReportPath = currentPath.Replace(@"\SqueezeBotConfigurator.exe", @"\SqResult.json");
            TikerFrame = externalSetting.tikersAndFrames;
            
            Settings = new BacktestSettings[]
            {
                externalSetting.settings.Clone(TradeOpenTrigger.high),
                externalSetting.settings.Clone(TradeOpenTrigger.low),
                externalSetting.settings.Clone(TradeOpenTrigger.highLow),
                externalSetting.settings.Clone(TradeOpenTrigger.open),
                externalSetting.settings.Clone(TradeOpenTrigger.close),
                externalSetting.settings.Clone(TradeOpenTrigger.openClose),
            };
        }
    }
}
