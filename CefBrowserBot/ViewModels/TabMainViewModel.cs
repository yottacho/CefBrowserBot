using CefBrowserBot.Services;
using CefBrowserBot.Views;
using FontAwesome5;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;

namespace CefBrowserBot.ViewModels
{
    class TabMainViewModel : ViewModelBase
    {
        public ICommand AddNewTabCommand { get; }

        public ICommand CloseTabCommand { get; }

        public ObservableCollection<TabHeaderViewModel> TabListSource { get; }

        public int SelectedTabIndex
        {
            get { return _SelectedTabIndex; }
            set 
            {
                Debug.WriteLine($"TabMainViewModel.SelectedTabIndex: {TabListSource.Count}:{_SelectedTabIndex}->{value}.");
                _SelectedTabIndex = value;

                if (_SelectedTabIndex >= TabListSource.Count - 1)
                {
                    Debug.WriteLine("TabMainViewModel.SelectedTabIndex: Reset chagne...");

                    // 변경 대상 값이 허용값이 아니면 변경을 거부함.
                    // WPF는 프로퍼티 변경을 거부하는 기능이 없으므로, 일단 변경을 수용하고 Dispatcher 타이머로 정상 값을 재요청하는 방법을 사용한다.
                    // (이 때 Invoke를 사용하면 안되고 BeginInvoke를 사용해야 함, 우선순위는 DispatcherPriority.ContextIdle를 권장하나 Normal도 동작은 함.)
                    //SynchronizationContext.Current.Post
                    DispatcherHelperService.BeginInvoke(() => SelectedTabIndex = _SelectedTabIndex - 1, DispatcherPriority.ContextIdle);
                    return;
                }
                RaisePropertyChanged(nameof(SelectedTabIndex));
            }
        }
        private int _SelectedTabIndex;

        public TabMainViewModel()
        {
            TabListSource = new ObservableCollection<TabHeaderViewModel>();

            AddNewTabCommand = new RelayCommand<object>(x =>
            {
                OpenTab(x?.ToString());
            });

            CloseTabCommand = new RelayCommand<object>(x =>
            {
                var tab = x as TabContentViewModel;
                if (tab == null)
                {
                    Debug.WriteLine($"TabMainViewModel.CloseTabCommand: Parameter({x?.GetType().FullName}) is not TabContentViewModel.");
                    return;
                }

                TabListSource.Remove(tab);

                if (x.GetType().GetInterface("GalaSoft.MvvmLight.ICleanup") != null)
                    (x as ICleanup).Cleanup();

                if (x.GetType().GetInterface("System.IDisposable") != null)
                    (x as IDisposable).Dispose();
            });

            // Add "new default tab"
            TabListSource.Add(new TabHeaderViewModel()
            {
                TabHeaderButtonContent = GetFaIcon(EFontAwesomeIcon.Solid_Plus, 16, 16),
                TabHeaderButtonCommand = AddNewTabCommand,
                TabHeaderButtonParameter = null,
                ViewContent = null
            });

            // first tab
            AddNewTabCommand.Execute(null);
            SelectedTabIndex = 0;
        }

        private object GetFaIcon(EFontAwesomeIcon iconName, int width, int height)
        {
            return new SvgAwesome()
            {
                Width = width,
                Height = height,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center,
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                Icon = iconName
            };
        }

        public TabContentViewModel OpenTab(string url)
        {
            if (url == null)
            {
                url = ConfigManager.Default.Config.HomeUrl;
                if (string.IsNullOrEmpty(url))
                    url = @"about:blank";
            }
            Debug.WriteLine($"TabMainViewModel.OpenTab: {url}");

            TabContentViewModel newTab = SimpleIoc.Default.GetInstance<TabContentViewModel>(Guid.NewGuid().ToString());
            newTab.TabHeaderButtonContent = GetFaIcon(EFontAwesomeIcon.Solid_Times, 16, 16);
            newTab.TabHeaderButtonCommand = CloseTabCommand;
            newTab.TabHeaderButtonParameter = newTab;

            //newTab.WebBrowser.Address = url;
            //newTab.WebBrowser.Load(url);
            newTab.Url = url;
            newTab.GoToPageCommand?.Execute(null);

            newTab.ViewContent = new TabContent() { DataContext = newTab };

            TabListSource.Insert(TabListSource.Count - 1, newTab);
            SelectedTabIndex = TabListSource.Count - 2;

            return newTab;
        }

        public void FocusTab(TabHeaderViewModel vm)
        {
            for (int i = 0; i < TabListSource.Count; i++)
            {
                if (TabListSource[i] == vm)
                {
                    SelectedTabIndex = i;
                    break;
                }
            }
        }
    }
}
