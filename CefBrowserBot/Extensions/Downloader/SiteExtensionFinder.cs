using CefBrowserBot.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CefBrowserBot.Extensions.Downloader
{
    static class SiteExtensionFinder
    {
        public static SiteExtensionInfo[] GetSiteExtension(string url)
        {
            string[] ExtensionPaths =
            {
                // ConfigManager.Default.ApplicationStoragePath/Extensions/Downloader/**/manifest.json
                Path.Combine(ConfigManager.Default.ApplicationStoragePath, @"Extensions", @"Downloader")
            };

            List<SiteExtensionInfo> list = new List<SiteExtensionInfo>();

            foreach (string searchPath in ExtensionPaths)
            {
                if (Directory.Exists(searchPath))
                {
                    var info = GetManifestInfo(new DirectoryInfo(searchPath), url);
                    list.AddRange(info);
                }
                else
                {
                    try
                    {
                        Directory.CreateDirectory(searchPath);
                    }
                    catch (Exception) { }
                }
            }

            return list.ToArray();
        }

        private static SiteExtensionInfo[] GetManifestInfo(DirectoryInfo baseDir, string url)
        {
            List<SiteExtensionInfo> list = new List<SiteExtensionInfo>();

            var subdir = baseDir.GetDirectories();
            foreach (var dir in subdir)
            {
                var manifestPathName = Path.Combine(dir.FullName, "manifest.json");
                if (!File.Exists(manifestPathName))
                    continue;

                // manifiest proc
                try
                {
                    string manifestText = File.ReadAllText(manifestPathName);
                    DownloaderManifest manifest = JsonConvert.DeserializeObject<DownloaderManifest>(manifestText);

                    Regex urlPattern = new Regex(manifest.Site);
                    bool matched = urlPattern.IsMatch(url);

                    if (matched)
                    {
                        string scriptFile = Path.Combine(dir.FullName, manifest.Script);
                        if (!File.Exists(scriptFile))
                            continue;

                        SiteExtensionInfo extension = new SiteExtensionInfo();
                        extension.Name = manifest.Name;
                        extension.Url = url;
                        extension.ExtensionPath = dir.FullName;
                        extension.ScriptText = File.ReadAllText(scriptFile);
                        list.Add(extension);
                    }

                }
                catch (Exception)
                {
                    continue;
                }
            }

            return list.ToArray();
        }

        class DownloaderManifest
        {
            public string Name { get; set; }
            public string Site { get; set; }
            public string Script { get; set; }
        }
    }

    public class SiteExtensionInfo
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public string ExtensionPath { get; set; }
        public string ScriptText { get; set; }
    }
}
