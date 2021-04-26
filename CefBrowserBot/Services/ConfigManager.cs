using Newtonsoft.Json;
using Syroot.Windows.IO;
using System;
using System.IO;
using System.Text;

namespace CefBrowserBot.Services
{
    class ConfigManager
    {
        public static ConfigManager Default
        {
            get
            {
                if (fDefault == default)
                    fDefault = new ConfigManager();
                return fDefault;
            }
        }
        private static ConfigManager fDefault;

        protected ConfigManager() 
        {
            Config = new Config();
        }

        public string ApplicationStoragePath { get; set; }

        public string ConfigFilePathName
        {
            get
            {
                if (string.IsNullOrEmpty(fConfigFilePathName))
                {
                    if (!string.IsNullOrEmpty(ApplicationStoragePath))
                        fConfigFilePathName = Path.Combine(ApplicationStoragePath, "config.json");
                }
                return fConfigFilePathName;
            }
        }
        string fConfigFilePathName;

        public Config Config { get; protected set; }

        public void Load()
        {
            if (string.IsNullOrEmpty(ApplicationStoragePath))
                return;

            if (File.Exists(ConfigFilePathName))
            {
                try
                {
                    var text = File.ReadAllText(ConfigFilePathName);
                    var obj = JsonConvert.DeserializeObject<Config>(text);

                    if (obj != null)
                        Config = obj;
                }
                catch (Exception) { }
            }
        }

        public void Save()
        {
            if (string.IsNullOrEmpty(ApplicationStoragePath))
                return;

            var json = JsonConvert.SerializeObject(Config);

            File.WriteAllText(ConfigFilePathName, json, Encoding.UTF8);
        }
    }

    public class Config
    {
        public string HomeUrl { get; set; }

        public string DownloadDirectory
        {
            get
            {
                if (string.IsNullOrEmpty(fDownloadDirectory))
                    fDownloadDirectory = Path.Combine(new KnownFolder(KnownFolderType.Downloads).Path, @"Downloaded comics");
                return fDownloadDirectory;
            }
            set
            {
                fDownloadDirectory = value;
            }
        }
        private string fDownloadDirectory;

        public string UpdateServer { get; set; }

        public string ExtensionsUpdateServer { get; set; }
    }
}
