using System;
using System.Windows.Input;

namespace CefBrowserBot.Extensions
{
    public interface IExBrowserExtension : IDisposable
    {
        /// <summary>
        /// 버튼 내용 (이미지 또는 텍스트)
        /// </summary>IExBrowserExtension
        object ButtonContent { get; }
        
        /// <summary>
        /// 버튼이 눌렸을 경우 실행할 커맨드
        /// </summary>
        ICommand ButtonCommand { get; }

        /// <summary>
        /// 관련 뷰모델
        /// TODO ViewModelBase -> Status
        /// </summary>
        IViewModelControl ViewModel { get; set; }

        /// <summary>
        /// 클래스 생성 후 호출
        /// </summary>
        public void Initialize();
    }
}
