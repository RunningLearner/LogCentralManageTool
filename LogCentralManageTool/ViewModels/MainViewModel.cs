using LogCentralManageTool.Data;
using LogCentralManageTool.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace LogCentralManageTool.ViewModels
{
    /// <summary>
    /// 메인 윈도우의 전반적인 상태를 관리하는 ViewModel
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private object _currentContent;
        public object CurrentContent
        {
            get => _currentContent;
            set { _currentContent = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// 사이드바 전용 ViewModel을 포함
        /// </summary>
        public SidebarViewModel SidebarViewModel { get; } = new SidebarViewModel();

        /// <summary>
        /// 사이드바 열의 너비: 확장 상태이면 200, 축소 상태이면 40
        /// </summary>
        public GridLength SidebarWidth => SidebarViewModel.IsExpanded ? new GridLength(200) : new GridLength(40);

        /// <summary>
        /// 생성자: SidebarViewModel의 변경 이벤트를 구독합니다.
        /// </summary>
        public MainViewModel()
        {
            // SidebarViewModel의 IsExpanded 값이 바뀔 때 SidebarWidth의 변경을 알림
            SidebarViewModel.PropertyChanged += SidebarViewModel_PropertyChanged;
            SidebarViewModel.ProductSelected += OnProductSelected;
        }

        private void SidebarViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SidebarViewModel.IsExpanded))
            {
                OnPropertyChanged(nameof(SidebarWidth));
            }
        }

        private void OnProductSelected(ProductInfo product)
        {
            var context = DbContextFactory.GetContext(product.DatabaseName, product.ProviderType, product.ConnectionString);
            
            CurrentContent = new Views.DashBoardView
            {
                DataContext = new DashBoardViewModel(context)
            };
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
