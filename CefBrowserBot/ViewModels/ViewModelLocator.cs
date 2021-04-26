using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;

namespace CefBrowserBot.ViewModels
{
    class ViewModelLocator
    {
        static ViewModelLocator()
        {
            //ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            if (ViewModelBase.IsInDesignModeStatic)
            {
                //SimpleIoc.Default.Register<IDataService, Design.DesignDataService>();
            }
            else
            {
                //SimpleIoc.Default.Register<IDataService, DataService>();
            }

            SimpleIoc.Default.Register<MainWindowViewModel>();
            SimpleIoc.Default.Register<SettingsDialogWindowViewModel>();
            SimpleIoc.Default.Register<TabMainViewModel>();
            SimpleIoc.Default.Register<TabContentViewModel>();
        }

        public MainWindowViewModel MainWindowViewModel { get => SimpleIoc.Default.GetInstance<MainWindowViewModel>(); }

        public SettingsDialogWindowViewModel SettingsDialogWindowViewModel { get => SimpleIoc.Default.GetInstance<SettingsDialogWindowViewModel>(); }

        public TabMainViewModel TabMainViewModel { get => SimpleIoc.Default.GetInstance<TabMainViewModel>(); }

        public TabContentViewModel TabContentViewModel { get => SimpleIoc.Default.GetInstance<TabContentViewModel>(); }

    }
}
