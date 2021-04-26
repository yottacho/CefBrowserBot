using CefSharp;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace CefBrowserBot.Extensions.Downloader
{
    public class DownloaderExtension : ViewModelBase, IExBrowserExtension
    {
        bool IsButtonActivated = false; // 버튼 활성화여부
        bool IsBrowserLoading = false;  // 문서 로딩중 여부
        Window popupWindow;             // 팝업윈도우

        /// <summary>
        /// 브라우저에 주입할 모듈 이름
        /// </summary>
        public string ScriptObjectName { get; private set; }

        /// <summary>
        /// 브라우저에 주입한 모듈
        /// </summary>
        public BrowserJavascriptInterface ScriptObject { get => fScriptObject; }
        BrowserJavascriptInterface fScriptObject;

        /// <summary>
        /// 내부에서 사용하는 스크립트
        /// </summary>
        public string ExtensionScript { get; protected set; }

        #region View Property
        /// <summary>
        /// 모듈 활성화 여부
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// 인식한 사이트 종류
        /// </summary>
        public string SiteType { get; private set; }

        /// <summary>
        /// 자동 다운로드 여부
        /// </summary>
        public bool AutoDownload { get; set; }

        /// <summary>
        /// 다음화 이동 여부
        /// </summary>
        public bool MoveNext { get; set; }

        public ObservableCollection<DownloadLog> ActionLog { get; set; }
        #endregion

        /// <summary>
        /// 생성자
        /// </summary>
        public DownloaderExtension()
        {
            ActionLog = new ObservableCollection<DownloadLog>();

            //int rnd = new Random((int)(DateTime.Now.Ticks % int.MaxValue)).Next();
            //ScriptObjectName = $"Js_{rnd}_O";
            ScriptObjectName = $"Js__O";
            fScriptObject = new BrowserJavascriptInterface(this);

            // 기본값 셋팅 (나중에 개선)
            Enabled = true;
            AutoDownload = true;
            MoveNext = true;
        }

        #region IExBrowserExtension 구현
        /// <summary>
        /// IExBrowserExtension 버튼에 보여질 텍스트/이미지
        /// </summary>
        public object ButtonContent { get => $" 다운로더 "; }

        /// <summary>
        /// IExBrowserExtension 버튼 커맨드
        /// </summary>
        public ICommand ButtonCommand { get => new RelayCommand(ButtonClick, () => IsButtonActivated); }

        public IViewModelControl ViewModel { get; set; }

        public void Initialize()
        {
            IWebBrowser fWebBrowser = ViewModel.WebBrowser;

            // script callable object
            fWebBrowser.JavascriptObjectRepository.Register(ScriptObjectName, fScriptObject, options: BindingOptions.DefaultBinder);
            Debug.WriteLine($"DownloaderExtension: Javascript object registered [{ScriptObjectName}]");

            // event listener
            fWebBrowser.LoadingStateChanged += (s, e) => { IsBrowserLoading = e.IsLoading; IsButtonActivated = !e.IsLoading; RaisePropertyChanged(nameof(ButtonCommand)); };

            fWebBrowser.FrameLoadStart += (s, e) => { FrameLoadStart(e.Url, e.Frame, e.TransitionType); };
            fWebBrowser.FrameLoadEnd += (s, e) => { FrameLoadEnd(e.HttpStatusCode, e.Url, e.Frame, e.Browser); };
        }
        #endregion

        /// <summary>
        /// 상태바 버튼 클릭
        /// </summary>
        private void ButtonClick()
        {
            if (popupWindow == null)
            {
                DownloaderExtensionView view = new DownloaderExtensionView();
                view.DataContext = this;

                popupWindow = new Window();
                popupWindow.Owner = Application.Current.MainWindow;  // 메인 윈도우가 닫히면 함께 종료

                popupWindow.WindowStyle = WindowStyle.ToolWindow;
                popupWindow.Title = "다운로드";

                popupWindow.Width = 300;
                popupWindow.Height = 250;

                popupWindow.Top = Application.Current.MainWindow.Top + Application.Current.MainWindow.Height - popupWindow.Height - 90;
                popupWindow.Left = Application.Current.MainWindow.Left + Application.Current.MainWindow.Width - popupWindow.Width - 30;
                popupWindow.Topmost = true;

                popupWindow.Content = view;
                popupWindow.Show();
            }
            else
            {
                popupWindow.Close();
                popupWindow = null;
            }
        }

        /// <summary>
        /// 문서 로드
        /// </summary>
        /// <param name="url"></param>
        /// <param name="frame"></param>
        /// <param name="transitionType"></param>
        private void FrameLoadStart(string url, IFrame frame, TransitionType transitionType)
        {
            Debug.WriteLine($"DownloaderExtension.FrameLoadStart: {url}, {transitionType}");

            // 메인프레임인 경우 사이트 종류 초기화
            if (frame.IsMain)
            {
                SiteType = "";
                RaisePropertyChanged(nameof(SiteType));
            }
        }

        /// <summary>
        /// 문서 로드 완료
        /// </summary>
        private void FrameLoadEnd(int httpStatusCode, string url, IFrame frame, IBrowser browser)
        {
            Debug.WriteLine($"DownloaderExtension.FrameLoadEnd: {httpStatusCode} {url}");
            ExtensionScript = "";

            if (httpStatusCode == 200 && Enabled)
            {
                var extensions = SiteExtensionFinder.GetSiteExtension(url);
                if (extensions.Length > 0)
                {
                    StringBuilder script = new StringBuilder();
                    foreach (var ext in extensions)
                    {
                        if (SiteType.Length > 0)
                            SiteType += "\r\n";

                        SiteType += ext.Name;
                        script.Append($"/* Name: {ext.Name} */;\r\n");
                        script.Append(ext.ScriptText.Replace("$Downloader$", ScriptObjectName));
                        script.Append("\r\n");
                    }
                    ExtensionScript = script.ToString();

                    // register object
                    StringBuilder objectRegistScript = new StringBuilder();
                    objectRegistScript.Append("CefSharp.BindObjectAsync(\"" + ScriptObjectName + "\").then(async function () {\r\n");
                    objectRegistScript.Append("var script = await " + ScriptObjectName + ".getExtensionScript();\r\n");
                    objectRegistScript.Append("setTimeout(script, 1);");
                    objectRegistScript.Append("\r\n});\r\n");

                    frame.ExecuteJavaScriptAsync(objectRegistScript.ToString());
                }
            }

            RaisePropertyChanged(nameof(SiteType));
        }

        public void Dispose()
        {
        }
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
    }

    public enum DownloadLogStatus
    {
        Ready,
        Download,
        Ok,
        Error
    }
}
