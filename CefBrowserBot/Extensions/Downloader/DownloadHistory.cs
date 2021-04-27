using CefBrowserBot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CefBrowserBot.Extensions.Downloader
{
    class DownloadHistory
    {
        static readonly object lockThis = new();

        public static DownloadHistory Default
        {
            get
            {
                if (fDefault == default)
                    fDefault = new DownloadHistory();
                return fDefault;
            } 
        }
        static DownloadHistory fDefault;

        public string JsonFilePathName 
        { 
            get 
            {
                if (string.IsNullOrEmpty(fJsonFilePathName))
                    fJsonFilePathName = Path.Combine(ConfigManager.Default.ApplicationStoragePath, "downloaded.json");
                return fJsonFilePathName;
            }
        }
        string fJsonFilePathName;

        public SynchronizedCollection<DownloadedInfo> Data { get; protected set; }

        protected DownloadHistory()
        {
            lock (lockThis)
            {
                Data = new SynchronizedCollection<DownloadedInfo>();
                Load();
            }
        }

        public void Load()
        {
            if (File.Exists(JsonFilePathName))
            {
                try
                {
                    Data = JsonConvert.DeserializeObject<SynchronizedCollection<DownloadedInfo>>(File.ReadAllText(JsonFilePathName));
                }
                catch (Exception)
                {
                    Data = new SynchronizedCollection<DownloadedInfo>();
                }

                if (Data == null)
                    Data = new SynchronizedCollection<DownloadedInfo>();
            }
        }

        public void Save()
        {
            lock (lockThis)
            {
                while (Data.Count > 5000)
                {
                    Data.RemoveAt(0);
                }

                string json = JsonConvert.SerializeObject(Data);

                File.WriteAllText(JsonFilePathName, json, Encoding.UTF8);
            }
        }
    }

    public class DownloadedInfo
    {
        public string Url { get; set; }
        public string Path { get; set; }
        public string Site { get; set; }
        public string Title { get; set; }
        public string DateTime { get; set; }
    }
}
