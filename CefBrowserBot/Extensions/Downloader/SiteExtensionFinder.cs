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
            // (ConfigManager.Default.ApplicationStoragePath)/Extensions/Downloader/**/manifest.json
            string ExtensionPath = Path.Combine(ConfigManager.Default.ApplicationStoragePath, @"Extensions", @"Downloader");

            List<SiteExtensionInfo> list = new List<SiteExtensionInfo>();

            if (Directory.Exists(ExtensionPath))
            {
                var info = GetManifestInfo(new DirectoryInfo(ExtensionPath), url);
                list.AddRange(info);
            }
            else
            {
                try
                {
                    Directory.CreateDirectory(ExtensionPath);
                }
                catch (Exception) { }
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
                    if (!urlPattern.IsMatch(url))
                        continue;

                    SiteExtensionInfo extension = new SiteExtensionInfo();
                    extension.Name = manifest.Name;
                    extension.Url = url;
                    extension.ExtensionPath = dir.FullName;
                    extension.ScriptText = "";

                    // multi script support
                    foreach (var scriptName in manifest.Script)
                    {
                        string scriptFile = Path.Combine(dir.FullName, scriptName);
                        if (!File.Exists(scriptFile))
                            continue;

                        extension.ScriptText += File.ReadAllText(scriptFile);
                        extension.ScriptText += "\r\n\r\n"; // add blank line
                    }

                    list.Add(extension);
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
            public string[] Script { get; set; }
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
