using CefBrowserBot.Chromium;
using CefBrowserBot.Extensions;
using CefSharp;
using CefSharp.Wpf;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace CefBrowserBot.ViewModels
{
    class TabContentViewModel : TabHeaderViewModel, IViewModelControl
    {
        #region TabHeader
        public new string TabHeaderTitle
        {
            get
            {
                if (!string.IsNullOrEmpty(Title))
                    return Title.Length > 15 ? Title.Substring(0, 15) : Title;
                else
                    return Url == null ? @"about:blank" : (Url.Length > 15 ? Url.Substring(0, 15) : Url);
            }

            private set { }
        }

        public new string Title { get => fChromiumWebBrowser.Title; }
        #endregion

        #region IViewModelControl
        public string StatusMessage
        {
            get { return fStatusMessage; }
            set { fStatusMessage = value; RaisePropertyChanged(nameof(StatusMessage)); }
        }
        private string fStatusMessage;

        public IWebBrowser WebBrowser { get; set; }
        #endregion

        // on closed event (sender object => viewmodel)
        public event EventHandler Closed;

        public string Url
        {
            get { return fUrl; }
            set { fUrl = value; RaisePropertyChanged(nameof(Url)); }
        }
        private string fUrl;

        public ICommand BackCommand { get; }
        public ICommand ForwardCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand GoToPageCommand { get; }
        public ICommand OpenDevToolsCommand { get; }

        public ObservableCollection<IExBrowserExtension> Extensions { get; set; }

        private ChromiumWebBrowser fChromiumWebBrowser;

        public TabContentViewModel()
        {
            // initialize browser
            fChromiumWebBrowser = WebBrowserFactory.CreateChromiumWebBrowser(this);
            WebBrowser = fChromiumWebBrowser;

            // browser event handler
            fChromiumWebBrowser.AddressChanged += (s, e) => { Url = e.NewValue.ToString(); };
            fChromiumWebBrowser.TitleChanged += (s, e) => { RaisePropertyChanged(nameof(Title)); RaisePropertyChanged(nameof(TabHeaderTitle)); };

            //fChromiumWebBrowser.StatusMessage += (s, e) => { StatusMessage = e.Value; };
            fChromiumWebBrowser.FrameLoadStart += (s, e) => { if (e.Frame.IsMain) { StatusMessage = "Loading..."; } };
            fChromiumWebBrowser.FrameLoadEnd += (s, e) => { if (e.Frame.IsMain) { StatusMessage = (e.HttpStatusCode == 200 ? "완료" : $"오류(HTTP:{e.HttpStatusCode})"); } };

            // button handler
            BackCommand = fChromiumWebBrowser.BackCommand;
            ForwardCommand = fChromiumWebBrowser.ForwardCommand;
            RefreshCommand = fChromiumWebBrowser.ReloadCommand;
            StopCommand = fChromiumWebBrowser.StopCommand;
            GoToPageCommand = new RelayCommand(() => fChromiumWebBrowser.Address = Url);
            OpenDevToolsCommand = new RelayCommand(() => fChromiumWebBrowser.ShowDevTools());

            // initialize extension
            Extensions = ExBrowserExtensionFactory.GetExtensions(WebBrowser, this, new Assembly[] { Assembly.GetExecutingAssembly() });

            Debug.WriteLine("TabContentViewModel: Init tab");
        }

        public void Close()
        {
            Closed?.Invoke(this, new EventArgs());

            var Locator = Application.Current.TryFindResource("Locator") as ViewModelLocator;
            Locator.TabMainViewModel.CloseTabCommand.Execute(this);
        }

        public override void Cleanup()
        {
            Debug.WriteLine("TabContentViewModel.CleanUp:");

            base.Cleanup();
            WebBrowser.Dispose();

            foreach (var item in Extensions)
            {
                item.Dispose();
            }
        }
    }
}
