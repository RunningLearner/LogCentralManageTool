using LogCentralManageTool.Utils;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LogCentralManageTool.ViewModels
{
    /// <summary>
    /// 사이드바의 상태와 명령을 관리하는 전용 ViewModel
    /// </summary>
    public class SidebarViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isExpanded = true;

        /// <summary>
        /// 사이드바 확장 상태 (true: 확장, false: 축소)
        /// </summary>
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ToggleIcon));
                }
            }
        }

        /// <summary>
        /// 토글 버튼에 표시할 아이콘 (확장 상태이면 "<<" 축소, 축소 상태이면 ">>" 확장)
        /// </summary>
        public string ToggleIcon => IsExpanded ? "<<" : ">>";

        private ICommand _toggleCommand;

        /// <summary>
        /// 사이드바 토글 명령
        /// </summary>
        public ICommand ToggleCommand => _toggleCommand ??= new RelayCommand(o => Toggle());

        /// <summary>
        /// 사이드바 확장 상태를 토글합니다.
        /// </summary>
        private void Toggle()
        {
            IsExpanded = !IsExpanded;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
