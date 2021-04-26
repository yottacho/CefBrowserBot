using CefSharp;
using System;
using System.Collections.ObjectModel;
using System.Reflection;

namespace CefBrowserBot.Extensions
{
    static class ExBrowserExtensionFactory
    {
        public static ObservableCollection<IExBrowserExtension> GetExtensions(IWebBrowser webBrowser, IViewModelControl viewModel, Assembly[] assemblies)
        {
            //var assembly = Assembly.GetExecutingAssembly();
            ObservableCollection<IExBrowserExtension> extensions = new ObservableCollection<IExBrowserExtension>();

            foreach (Assembly assembly in assemblies)
            {
                Type[] types = assembly.GetExportedTypes();

                foreach (Type type in types)
                {
                    if (!type.IsClass || type.GetInterface("CefBrowserBot.Extensions.IExBrowserExtension") == null)
                        continue;

                    // 객체 생성
                    var constructor = type.GetConstructor(new Type[] { });
                    if (constructor == null)
                        continue;
                    var obj = constructor.Invoke(null);

                    // 프로퍼티 셋
                    var viewModelProp = type.GetProperty("ViewModel");
                    if (viewModelProp != null)
                        viewModelProp.SetValue(obj, viewModel);

                    // 초기화 메서드 호출
                    var initMethod = type.GetMethod("Initialize", new Type[] { });
                    if (initMethod != null)
                        initMethod.Invoke(obj, null);

                    // init end
                    extensions.Add(obj as IExBrowserExtension);
                }
            }

            return extensions;
        }
    }
}
