using CefBrowserBot.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace CefBrowserBot.ViewModels
{
    class SettingsDialogWindowViewModel : ViewModelBase 
    {
        public ICommand CloseWindowCommand { get; protected set; }

        public ICommand SaveCommand { get; protected set; }
        public ICommand CloseCommand { get; protected set; }
        public ICommand OpenExtensionsFolderCommand { get; protected set; }

        public string ApplicationStoragePath { get; protected set; }

        public string HomeUrl { get; set; }

        public string DownloadDirectory { get; set; }

        public string UpdateServer { get; set; }

        public string ExtensionsUpdateServer { get; set; }

        public string ExtensionsLocalPath { get; set; }

        public SettingsDialogWindowViewModel()
        {
            ApplicationStoragePath = ConfigManager.Default.ApplicationStoragePath;

            HomeUrl = ConfigManager.Default.Config.HomeUrl;
            DownloadDirectory = ConfigManager.Default.Config.DownloadDirectory;
            UpdateServer = ConfigManager.Default.Config.UpdateServer;
            ExtensionsUpdateServer = ConfigManager.Default.Config.ExtensionsUpdateServer;

            ExtensionsLocalPath = Path.Combine(ConfigManager.Default.ApplicationStoragePath, "Extensions");

            SaveCommand = new RelayCommand(() => Save());
            CloseCommand = new RelayCommand<Window>(window =>
            {
                if (IsPropertyChanged())
                {
                    var result = MessageBox.Show("저장하겠습니까?", "확인", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes)
                    {
                        SaveCommand.Execute(null);
                    }
                    else
                    {
                        HomeUrl = ConfigManager.Default.Config.HomeUrl;
                        DownloadDirectory = ConfigManager.Default.Config.DownloadDirectory;
                        UpdateServer = ConfigManager.Default.Config.UpdateServer;
                        ExtensionsUpdateServer = ConfigManager.Default.Config.ExtensionsUpdateServer;
                    }
                }
                // close window
                CloseWindowCommand.Execute(window);
            });
            CloseWindowCommand = new RelayCommand<Window>(window => window?.Close());

            OpenExtensionsFolderCommand = new RelayCommand(() =>
            {
                // open explorer > ExtensionsLocalPath
                ProcessStartInfo psi = new ProcessStartInfo(ExtensionsLocalPath);
                psi.UseShellExecute = true;
                Process.Start(psi);
            });
        }

        private void Save()
        {
            Debug.WriteLine("Settings saved.");

            ConfigManager.Default.Config.HomeUrl = HomeUrl;
            ConfigManager.Default.Config.DownloadDirectory = DownloadDirectory;
            ConfigManager.Default.Config.UpdateServer = UpdateServer;
            ConfigManager.Default.Config.ExtensionsUpdateServer = ExtensionsUpdateServer;

            ConfigManager.Default.Save();
        }

        private bool IsPropertyChanged()
        {
            return (HomeUrl != ConfigManager.Default.Config.HomeUrl) ||
                (DownloadDirectory != ConfigManager.Default.Config.DownloadDirectory) ||
                (UpdateServer != ConfigManager.Default.Config.UpdateServer) ||
                (ExtensionsUpdateServer != ConfigManager.Default.Config.ExtensionsUpdateServer);
        }
    }
}
