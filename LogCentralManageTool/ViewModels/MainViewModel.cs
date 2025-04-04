using LogCentralManageTool.Utils;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace LogCentralManageTool.ViewModels
{
    /// <summary>
    /// 메인 윈도우의 ViewModel: 사이드바 토글 기능 포함
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSidebarVisible = true;

        /// <summary>
        /// 사이드바의 표시 여부 (true: 보임, false: 숨김)
        /// </summary>
        public bool IsSidebarVisible
        {
            get => _isSidebarVisible;
            set
            {
                if (_isSidebarVisible != value)
                {
                    _isSidebarVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SidebarWidth));
                }
            }
        }

        /// <summary>
        /// 사이드바 열의 너비: 보이면 200, 숨기면 0
        /// </summary>
        public GridLength SidebarWidth => IsSidebarVisible ? new GridLength(200) : new GridLength(0);

        private ICommand _toggleSidebarCommand;

        /// <summary>
        /// 사이드바를 토글하는 명령
        /// </summary>
        public ICommand ToggleSidebarCommand => _toggleSidebarCommand ??= new RelayCommand(o => ToggleSidebar());

        /// <summary>
        /// 사이드바 표시 상태를 토글합니다.
        /// </summary>
        private void ToggleSidebar()
        {
            IsSidebarVisible = !IsSidebarVisible;
        }

        /// <summary>
        /// 속성 변경 알림 메서드
        /// </summary>
        /// <param name="propertyName">변경된 속성명</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
