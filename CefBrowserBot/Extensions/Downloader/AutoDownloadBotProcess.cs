using CefBrowserBot.Services;
using CefBrowserBot.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;

namespace CefBrowserBot.Extensions.Downloader
{
    class AutoDownloadBotProcess
    {
        public static AutoDownloadBotProcess Default
        {
            get
            {
                if (fDefault == default)
                    fDefault = new AutoDownloadBotProcess();
                return fDefault;
            }
        }
        static AutoDownloadBotProcess fDefault;

        const int MaxOpenTab = 3;

        public SynchronizedCollection<TabContentViewModel> TabList { get; protected set; }
        public SynchronizedCollection<string> UrlList { get; protected set; }

        private Thread thread;
        private string SiteKey;

        protected AutoDownloadBotProcess()
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

                        //vm.Extensions =
                        //vm.Closed += (s, e) => { };

                        //vm.WebBrowser.Address = url;
                        vm.Url = url;
                        vm.GoToPageCommand?.Execute(null);

                        vm.TabHeaderBackground = new System.Windows.Media.SolidColorBrush() { Color = System.Windows.Media.Colors.LawnGreen };
                        TabMainViewModel.FocusTab(CurrentTab);
                    });

                    UrlList.RemoveAt(0);
                }

                Thread.Sleep(3 * 1000);
            }
        }

    }
}
