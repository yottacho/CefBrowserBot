using CefBrowserBot.Services;
using CefBrowserBot.ViewModels;
using CefSharp.Wpf;
using CefSharp.Wpf.Experimental;
using System;
using System.Diagnostics;
using System.Windows;

namespace CefBrowserBot.Chromium
{
    static class WebBrowserFactory
    {
        public static ChromiumWebBrowser CreateChromiumWebBrowser(TabContentViewModel viewModel)
        {
            ChromiumWebBrowser webBrowser = new ChromiumWebBrowser();

            // use IME setting
            webBrowser.WpfKeyboardHandler = new WpfImeKeyboardHandler(webBrowser);

            // register handlers
            var lifeSpanHandler = new LifeSpanHandler();
            lifeSpanHandler.OpenPopupWindow += NewTabOpen;
            lifeSpanHandler.OnCloseAction += () => { DispatcherHelperService.Invoke(delegate { viewModel.Close(); }); };
            webBrowser.LifeSpanHandler = lifeSpanHandler;

            var requestHandler = new RequestHandler();
            requestHandler.OpenNewTab += NewTabOpen;
            webBrowser.RequestHandler = requestHandler;

            // init browser
            webBrowser.CreateBrowser(null, new Size(Application.Current.MainWindow.Width, Application.Current.MainWindow.Height));
            return webBrowser;
        }

        private static void NewTabOpen(object sender, EventArgs e)
        {
            string url = sender as string;
            if (url == null)
            {
                Debug.WriteLine("WebBrowserFactory.NewTabOpen: Call error!");
                return;
            }

            var Locator = Application.Current.TryFindResource("Locator") as ViewModelLocator;
            DispatcherHelperService.Invoke(delegate { Locator.TabMainViewModel.OpenTab(url); });
        }
    }
}
