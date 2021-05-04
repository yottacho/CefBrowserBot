using CefSharp;
using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace CefBrowserBot.Chromium
{
    class RequestHandler : IRequestHandler
    {
        public event EventHandler OpenNewTab;

        bool IRequestHandler.OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            Debug.WriteLine("IRequestHandler.OnBeforeBrowse: " + request.Url);
            return false;
        }

        void IRequestHandler.OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("IRequestHandler.OnDocumentAvailableInMainFrame: " + browser.HasDocument);
        }

        bool IRequestHandler.OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            Debug.WriteLine("IRequestHandler.OnOpenUrlFromTab:");

            // 새 탭으로 열기 => 탭으로 처리
            if (targetDisposition == WindowOpenDisposition.NewBackgroundTab ||
                targetDisposition == WindowOpenDisposition.NewForegroundTab ||
                targetDisposition == WindowOpenDisposition.NewPopup ||
                targetDisposition == WindowOpenDisposition.NewWindow)
            {
                OpenNewTab?.Invoke(targetUrl, new EventArgs());
                return true;
            }

            // 탭으로 열 필요가 없는 경우
            return false;
        }

        IResourceRequestHandler IRequestHandler.GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            // Too many call
            //Debug.WriteLine("IRequestHandler.GetResourceRequestHandler " + request.Url);
            return null;
        }

        bool IRequestHandler.GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            Debug.WriteLine("IRequestHandler.GetAuthCredentials:");
            return false;
        }

        bool IRequestHandler.OnQuotaRequest(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, long newSize, IRequestCallback callback)
        {
            Debug.WriteLine("IRequestHandler.OnQuotaRequest:");
            callback?.Dispose();
            return false;
        }

        bool IRequestHandler.OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            Debug.WriteLine("IRequestHandler.OnCertificateError:");
            callback?.Dispose();
            return false;
        }

        bool IRequestHandler.OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            Debug.WriteLine("IRequestHandler.OnSelectClientCertificate:");
            callback?.Dispose();
            return false;
        }

        void IRequestHandler.OnPluginCrashed(IWebBrowser chromiumWebBrowser, IBrowser browser, string pluginPath)
        {
            Debug.WriteLine("IRequestHandler.OnPluginCrashed:");
        }

        void IRequestHandler.OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            Debug.WriteLine("IRequestHandler.OnRenderViewReady:");
        }

        void IRequestHandler.OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
            Debug.WriteLine("IRequestHandler.OnRenderProcessTerminated:");
        }
    }
}
