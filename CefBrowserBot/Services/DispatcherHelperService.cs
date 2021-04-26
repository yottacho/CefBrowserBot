using System;
using System.Windows;
using System.Windows.Threading;

namespace CefBrowserBot.Services
{
    static class DispatcherHelperService
    {
        // DispatcherHelperService.Invoke(delegate { [code] });
        public static void Invoke(Action action)
        {
            Dispatcher dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;

            if (dispatcher == null || dispatcher.CheckAccess())
                action();
            else
                dispatcher.Invoke(action);
        }

        public static void BeginInvoke(Action action, DispatcherPriority priority = DispatcherPriority.Normal, params object[] param)
        {
            Dispatcher dispatcher = Application.Current != null ? Application.Current.Dispatcher : null;
            dispatcher.BeginInvoke(action, priority, param);
        }

    }
}
