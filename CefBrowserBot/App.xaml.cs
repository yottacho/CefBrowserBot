using CefBrowserBot.Services;
using CefSharp;
using CefSharp.Wpf;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace CefBrowserBot
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            // default application path
            var localAppPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefBrowserBot");
            ConfigManager.Default.ApplicationStoragePath = localAppPath;
            ConfigManager.Default.Load();

            InitializeCefSharp();

#if DEBUG
            //System.Diagnostics.PresentationTraceSources.DataBindingSource.Switch.Level = System.Diagnostics.SourceLevels.All;
#endif

            // create default extension folder
            var extensionsPath = Path.Combine(localAppPath, @"Extensions");
            if (!Directory.Exists(extensionsPath))
                Directory.CreateDirectory(extensionsPath);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void InitializeCefSharp()
        {
            var userDataPath = Path.Combine(ConfigManager.Default.ApplicationStoragePath, "User Data");
            var cachePath = Path.Combine(ConfigManager.Default.ApplicationStoragePath, "cache");

            if (!Directory.Exists(userDataPath))
                Directory.CreateDirectory(userDataPath);

            if (!Directory.Exists(cachePath))
                Directory.CreateDirectory(cachePath);

            CefSharpSettings.ConcurrentTaskExecution = true;    // javascript async/await
            Cef.EnableHighDPISupport();

            var settings = new CefSettings();
            //settings.RemoteDebuggingPort = 8088;
            settings.Locale = "ko";
            // 인증서 오류를 무시
            //settings.IgnoreCertificateErrors = true;

            settings.UserDataPath = userDataPath;
            settings.CachePath = cachePath;
            settings.LogFile = Path.Combine(ConfigManager.Default.ApplicationStoragePath, "cefdebug.log");

            //Enables WebRTC
            settings.CefCommandLineArgs.Add("enable-media-stream");
            settings.CefCommandLineArgs.Add("disable-web-security");    // off CORS

            // Make sure you set performDependencyCheck false
            Cef.Initialize(settings, performDependencyCheck: false, browserProcessHandler: null);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Cef.Shutdown();
            base.OnExit(e);
        }
    }
}
