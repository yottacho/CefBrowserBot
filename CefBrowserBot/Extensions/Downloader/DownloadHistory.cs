using CefBrowserBot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public ObservableCollection<DownloadLog> ActionLog { get; protected set; }

        protected DownloadHistory()
        {
            lock (lockThis)
            {
                Data = new SynchronizedCollection<DownloadedInfo>();
                ActionLog = new ObservableCollection<DownloadLog>();
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
            while (Data.Count > 5000)
            {
                Data.RemoveAt(0);
            }

            lock (lockThis)
            {
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

    public class DownloadLog
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Agent { get; set; }

        public string FileName { get; set; }
        public string FullPathName { get; set; }
        public string SourceUrl { get; set; }
        public DownloadLogStatus Status { get; set; }
        public string Message { get; set; }

        public string TitleView
        {
            get
            {
                if (string.IsNullOrEmpty(Title) && Title.Length > 20)
                    return Title.Substring(0, 20);
                return Title;
            }
            private set { }
        }
    }

    public enum DownloadLogStatus
    {
        Ready,
        Download,
        Ok,
        Error
    }
}
