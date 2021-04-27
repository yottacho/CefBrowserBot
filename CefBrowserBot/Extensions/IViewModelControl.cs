using CefSharp;

namespace CefBrowserBot.Extensions
{
    public interface IViewModelControl
    {
        /// <summary>
        /// 현재 탭의 브라우저 인스턴스
        /// </summary>
        IWebBrowser WebBrowser { get; set; }

        /// <summary>
        /// 상태바
        /// </summary>
        string StatusMessage { get; set; }
    }
}
