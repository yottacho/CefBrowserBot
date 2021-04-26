using CefBrowserBot.Services;
using CefSharp;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace CefBrowserBot.Extensions.AutoReloader
{
    public class AutoReloaderExtension : ViewModelBase, IExBrowserExtension
    {
        const int MainReloadTimerDefault = 10 * 60; // 열린탭은 10분이 지나면 리로드
        const int DocumentLoadTimerDefault = 20;    // 문서 타이머
        bool IsDisposed;

        // 사용자가 컨트롤 불가한 강제 타이머
        Thread MainReloadTimerThread;   // 메인 리로드 타이머 쓰레드
        int MainReloadTimerRemain;      // 메인 리로드 타이머 리로드까지 남은 시간
        bool DocumentLoadTimerActivate; // 문서 로드 타이머 활성화 여부

        // 문서가 일정 시간 이내로 로드 완료되지 않으면 리로드하는 타이머
        int DocumentLoadTimerRemain;    // 문서 로드 타이머 리로드까지 남은 시간
        Thread DocumentLoadTimer;       // 문서 로드 타이머

        public bool DocumentLoadTimerEnable { get; set; }

        public AutoReloaderExtension()
        {
            IsDisposed = false;
            DocumentLoadTimerEnable = true;
        }

        #region IExBrowserExtension 구현
        /// <summary>
        /// IExBrowserExtension 버튼에 보여질 텍스트/이미지
        /// </summary>
        public object ButtonContent { get => ButtonMessage(); set { } }

        /// <summary>
        /// IExBrowserExtension 버튼 커맨드
        /// </summary>
        public ICommand ButtonCommand { get => new RelayCommand(ButtonClick); }

        /// <summary>
        /// IExBrowserExtension 뷰모델
        /// </summary>
        public IViewModelControl ViewModel { get; set; }

        /// <summary>
        /// IExBrowserExtension 로드 후 초기화 메서드
        /// </summary>
        public void Initialize()
        {
            IWebBrowser fWebBrowser = ViewModel.WebBrowser;
            fWebBrowser.FrameLoadStart += (s, e) => { FrameLoadStart(e.Frame); };
            fWebBrowser.FrameLoadEnd += (s, e) => { FrameLoadEnd(e.HttpStatusCode, e.Url, e.Frame, e.Browser); };

            // 메인 타이머 구동
            MainReloadTimerRemain = MainReloadTimerDefault;
            MainReloadTimerThread = new Thread(MainTimerRun)
            {
                IsBackground = true
            };
            MainReloadTimerThread.Start();
        }
        #endregion

        /// <summary>
        /// 상태바 버튼 클릭
        /// </summary>
        private void ButtonClick()
        {
            DocumentLoadTimerEnable = !DocumentLoadTimerEnable; // 상태 반전
            DocumentLoadTimerActivate = false;  // 문서 리로드 타이머 종료

            RaisePropertyChanged(nameof(ButtonContent));
        }

        /// <summary>
        /// 버튼 메시지 생성
        /// </summary>
        /// <returns></returns>
        private string ButtonMessage()
        {
            string message = " Reloader: OFF ";
            if (DocumentLoadTimerEnable)
            {
                if (DocumentLoadTimerActivate)
                {
                    message = $"  Loading [{DocumentLoadTimerRemain:D2}]  ";
                }
                else
                    message = "  [Loaded OK]  ";
            }
            return message;
        }

        /// <summary>
        /// 문서 로드
        /// </summary>
        /// <param name="frame"></param>
        private void FrameLoadStart(IFrame frame)
        {
            if (frame.IsMain && DocumentLoadTimerEnable)
            {
                // 이전 타이머가 있으면 종료
                if (DocumentLoadTimerActivate)
                {
                    DocumentLoadTimerActivate = false;
                    DocumentLoadTimer.Interrupt();
                    while (DocumentLoadTimer.IsAlive)
                        Thread.Sleep(50);
                }

                // 타이머 초기화
                DocumentLoadTimerRemain = DocumentLoadTimerDefault;
                RaisePropertyChanged(nameof(ButtonContent));

                DocumentLoadTimerActivate = true;
                DocumentLoadTimer = new Thread(DocumentLoadTimerRun)
                {
                    IsBackground = true
                };
                DocumentLoadTimer.Start();
            }
        }

        /// <summary>
        /// 문서 로드 완료
        /// </summary>
        private void FrameLoadEnd(int httpStatusCode, string url, IFrame frame, IBrowser browser)
        {
            if (frame.IsMain)
            {
                Debug.WriteLine($"AutoReloaderExtension.FrameLoadEnd: Reset timer. {DateTime.Now.ToString()}");
                DocumentLoadTimerActivate = false;
                MainReloadTimerRemain = MainReloadTimerDefault;
            }
        }

        /// <summary>
        /// 문서 로드 타이머 프로세스
        /// </summary>
        private void DocumentLoadTimerRun()
        {
            while (DocumentLoadTimerActivate)
            {
                if (DocumentLoadTimerRemain <= 0)
                {
                    Debug.WriteLine($"AutoReloaderExtension.timer: Timer reload. {DateTime.Now.ToString()}");

                    DocumentLoadTimerActivate = false;
                    RaisePropertyChanged(nameof(ButtonContent));

                    // action
                    DispatcherHelperService.Invoke(() =>
                    {
                        try
                        {
                            ViewModel.WebBrowser.Reload();
                        }
                        catch (Exception) { }
                    });

                    return;
                }

                try
                {
                    Thread.Sleep(1000);
                }
                catch (ThreadInterruptedException)
                {
                    DocumentLoadTimerActivate = false;
                    return;
                }

                DocumentLoadTimerRemain--;
                RaisePropertyChanged(nameof(ButtonContent));
            }

            Debug.WriteLine($"AutoReloaderExtension.timer: Timer stopped by flag. {DateTime.Now.ToString()}");
            RaisePropertyChanged(nameof(ButtonContent));
        }

        /// <summary>
        /// 메인 타이머 프로세스
        /// </summary>
        private void MainTimerRun()
        {
            while (!IsDisposed)
            {
                if (MainReloadTimerRemain <= 0)
                {
                    Debug.WriteLine($"AutoReloaderExtension.MainTimerRun: Reload. {DateTime.Now}");
                    MainReloadTimerRemain = MainReloadTimerDefault;

                    DispatcherHelperService.Invoke(() =>
                    {
                        try
                        {
                            if (!ViewModel.WebBrowser.IsDisposed)
                                ViewModel.WebBrowser.Reload();
                            else
                                return;
                        }
                        catch (Exception) { }
                    });
                }
                else
                {
                    try
                    {
                        Thread.Sleep(1000);
                    }
                    catch (ThreadInterruptedException) { }
                    MainReloadTimerRemain--;
                }
            }
            Debug.WriteLine("AutoReloaderExtension.MainTimerRun: exit. {DateTime.Now}");
        }

        public void Dispose()
        {
            IsDisposed = true;
            DocumentLoadTimerActivate = false;
        }
    }
}
