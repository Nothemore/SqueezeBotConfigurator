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
        public string reportPath { get; set; }
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

            reportPath = currentPath.Replace(@"\SqueezeBotConfigurator.exe", @"\SqResult.json");
            TikerFrame = externalSetting.tikersAndFrames;
            
            Settings = new BacktestSettings[]
            {
                externalSetting.settings.DeepCopy(TradeOpenTrigger.high),
                externalSetting.settings.DeepCopy(TradeOpenTrigger.low),
                externalSetting.settings.DeepCopy(TradeOpenTrigger.highLow),
                externalSetting.settings.DeepCopy(TradeOpenTrigger.open),
                externalSetting.settings.DeepCopy(TradeOpenTrigger.close),
                externalSetting.settings.DeepCopy(TradeOpenTrigger.openClose),
            };
        }
    }
}
