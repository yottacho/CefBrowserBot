using GalaSoft.MvvmLight;

namespace CefBrowserBot.ViewModels
{
    class TabHeaderViewModel : ViewModelBase
    {
        public string TabHeaderTitle { get; set; }

        public string Title { get; set; }

        public object TabHeaderButtonContent { get; set; }

        public object TabHeaderButtonCommand { get; set; }

        public object TabHeaderButtonParameter { get; set; }

        public System.Windows.Media.Brush TabHeaderBackground { get => fTabHeaderBackground; set { fTabHeaderBackground = value; RaisePropertyChanged(nameof(TabHeaderBackground)); } }
        private System.Windows.Media.Brush fTabHeaderBackground;

        public object ViewContent { get; set; }
    }
}
