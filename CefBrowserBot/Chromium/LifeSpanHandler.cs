using CefSharp;
using System;
using System.Diagnostics;

namespace CefBrowserBot.Chromium
{
    /// <summary>
    /// Chromium popup control
    /// </summary>
    class LifeSpanHandler : ILifeSpanHandler
    {
        // EventHandler
        public event EventHandler OpenPopupWindow;
        public event Action OnCloseAction;

        bool ILifeSpanHandler.OnBeforePopup(IWebBrowser browserControl, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName,
            WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo,
            IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
        {
            Debug.WriteLine($"ILifeSpanHandler.OnBeforePopup: {targetUrl}");

            // Popup event handler
            OpenPopupWindow?.Invoke(targetUrl, new EventArgs());

            // stop open popup window
            newBrowser = null;
            return true;
        }

        bool ILifeSpanHandler.DoClose(IWebBrowser browserControl, IBrowser browser)
        {
            Debug.WriteLine($"ILifeSpanHandler.DoClose: popup?{browser.IsPopup}, url:{browser.MainFrame.Url}");
            return false; // don't change
        }

        void ILifeSpanHandler.OnBeforeClose(IWebBrowser browserControl, IBrowser browser)
        {
            Debug.WriteLine($"ILifeSpanHandler.OnBeforeClose: popup?{browser.IsPopup}, url:{browser.MainFrame.Url}");
            if (!browser.IsPopup)
            {
                OnCloseAction?.Invoke();
            }
        }

        void ILifeSpanHandler.OnAfterCreated(IWebBrowser browserControl, IBrowser browser)
        {
            Debug.WriteLine($"ILifeSpanHandler.OnAfterCreated:");
        }
    }
}
