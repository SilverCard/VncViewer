using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;

namespace VncViewer.App.Core
{
    public static class ConfigManager
    {
        public const String LocalConfigFileName = "Config.json";

        public static Config ReadFromFile(String fileName)
        {
            return JsonConvert.DeserializeObject<Config>(File.ReadAllText(fileName));
        }

        public static Config ReadLocalConfig()
        {
            String configFile = GetLocalStringPath();
            if (File.Exists(configFile))
            {
                return ReadFromFile(configFile);
            }

            return null;
        }

        public static void SaveToLocalConfig(Config config)
        {
            SaveConfig(config, GetLocalStringPath());
        }

        public static String GetLocalStringPath() => Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), LocalConfigFileName);


        public static void SaveConfig(Config config, String fileName)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(config, Formatting.Indented));
        }
    }
}
