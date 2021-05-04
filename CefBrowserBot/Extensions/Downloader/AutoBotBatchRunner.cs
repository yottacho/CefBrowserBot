using CefBrowserBot.Services;
using CefBrowserBot.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace CefBrowserBot.Extensions.Downloader
{
    class AutoBotBatchRunner
    {
        public static AutoBotBatchRunner Default
        {
            get
            {
                if (fDefault == default)
                    fDefault = new AutoBotBatchRunner();
                return fDefault;
            }
        }
        static AutoBotBatchRunner fDefault;

        const int MaxOpenTab = 3;

        public SynchronizedCollection<TabContentViewModel> TabList { get; protected set; }
        public SynchronizedCollection<string> UrlList { get; protected set; }

        private Thread thread;
        private string SiteKey;

        protected AutoBotBatchRunner()
        {
            TabList = new SynchronizedCollection<TabContentViewModel>();
            UrlList = new SynchronizedCollection<string>();
        }

        public void StartProcess(string siteKey)
        {
            if (thread != null && thread.IsAlive)
                return;

            SiteKey = siteKey;
            thread = new Thread(Run);
            thread.IsBackground = true;
            thread.Start();
        }

        public void BotFinished(string url)
        {
            var Locator = Application.Current.TryFindResource("Locator") as ViewModelLocator;

            for (int i = 0; i < TabList.Count; i++)
            {
                var viewModel = TabList[i];

                DispatcherHelperService.Invoke(delegate 
                {
                    if (viewModel.WebBrowser.Address == url)
                    {
                        viewModel.Close();
                        TabList.Remove(viewModel);
                    }
                });
            }
        }

        private void Run()
        {
            var Locator = Application.Current.TryFindResource("Locator") as ViewModelLocator;
            var TabMainViewModel = Locator.TabMainViewModel;
            var CurrentTab = TabMainViewModel.TabListSource[TabMainViewModel.SelectedTabIndex];

            while (UrlList.Count > 0)
            {
                // 부하조정: 탭이 한꺼번에 열리지 않도록
                Thread.Sleep(2 * 1000);

                if (TabList.Count < MaxOpenTab)
                {
                    string url = UrlList[0];

                    // 이미 다운로드 완료한 url은 생략
                    Uri uri = new Uri(url);
                    var data = DownloadHistory.Default.Data.Where(x => x.Site == SiteKey && x.Path == uri.AbsolutePath).FirstOrDefault();
                    if (data != default)
                    {
                        UrlList.RemoveAt(0);
                        continue;
                    }

                    DispatcherHelperService.Invoke(delegate
                    {
                        var vm = TabMainViewModel.OpenTab(@"about:blank");
                        TabList.Add(vm);

                        // Extension 활성화
                        foreach (var item in vm.Extensions)
                        {
                            var ext = item as DownloaderExtension;
                            if (ext != null)
                            {
                                ext.Enabled = true;
                                ext.AutoDownload = true;
                                ext.MoveNext = true;
                            }
                        }

                        //vm.Closed += (s, e) => { };

                        vm.Url = url;
                        vm.GoToPageCommand?.Execute(null);

                        vm.TabHeaderBackground = new System.Windows.Media.SolidColorBrush() { Color = System.Windows.Media.Colors.LawnGreen };
                        TabMainViewModel.FocusTab(CurrentTab);
                    });

                    UrlList.RemoveAt(0);
                }
            }
        }

    }
}
