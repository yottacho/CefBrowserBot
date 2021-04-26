using CefBrowserBot.Views;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace CefBrowserBot.ViewModels
{
    class MainWindowViewModel : ViewModelBase
    {
        public string WindowTitle { get; protected set; }

        public ICommand SettingsDialogCommand { get; }
        public ICommand ExitCommand { get; }

        public MainWindowViewModel()
        {
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
            // version: FileMajorPart + FileMinorPart + FileBuildPart + FilePrivatePart
            WindowTitle = $"CefBrowserBot v{version.FileMajorPart}.{version.FileMinorPart}";

            SettingsDialogCommand = new RelayCommand(() =>
            {
                // TODO change MVVM pattern
                SettingsDialogWindow dialog = new SettingsDialogWindow();
                dialog.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                dialog.ShowDialog();
            });

            ExitCommand = new RelayCommand(() =>
            {
                Application.Current.Shutdown(0);
            });
        }
    }
}
