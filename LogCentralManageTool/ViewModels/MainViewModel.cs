using LogCentralManageTool.Data;
using LogCentralManageTool.Models;
using LogCentralManageTool.Utils;
using LogCentralManageTool.Views;

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace LogCentralManageTool.ViewModels
{
    /// <summary>
    /// 메인 윈도우의 전반적인 상태를 관리하는 ViewModel
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        #region 이벤트

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region 필드

        private object _currentContent;

        #endregion

        #region 속성

        /// <summary>
        /// 현재 메인 컨텐츠에 표시할 뷰 또는 컨트롤
        /// </summary>
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
        public GridLength SidebarWidth => SidebarViewModel.IsExpanded
            ? new GridLength(200)
            : new GridLength(40);

        #endregion

        #region 커맨드

        /// <summary>
        /// 홈버튼 커맨드 (메인 컨텐츠 영역에 PieChartSummaryView를 표시)
        /// </summary>
        public ICommand HomeCommand { get; }

        #endregion

        #region 생성자

        /// <summary>
        /// 생성자: SidebarViewModel의 변경 이벤트를 구독하고 초기 컨텐츠를 설정합니다.
        /// </summary>
        public MainViewModel()
        {
            // 이벤트 구독
            SidebarViewModel.PropertyChanged += SidebarViewModel_PropertyChanged;
            SidebarViewModel.ProductSelected += OnProductSelected;

            // 홈버튼 커맨드 초기화 (RelayCommand는 간단한 커맨드 구현 클래스)
            HomeCommand = new RelayCommand(o => ShowHome());

            // 시작 시 기본 컨텐츠로 홈 화면(PieChartSummaryView)를 보여줍니다.
            ShowHome();
        }

        #endregion

        #region 이벤트 핸들러

        /// <summary>
        /// 사이드바의 IsExpanded 변경 시 SidebarWidth 변경 알림
        /// </summary>
        private void SidebarViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SidebarViewModel.IsExpanded))
            {
                OnPropertyChanged(nameof(SidebarWidth));
            }
        }

        /// <summary>
        /// 제품 선택 이벤트 처리: 선택한 제품의 LogRepository를 생성하여 DashboardView를 표시
        /// </summary>
        /// <param name="product">선택된 제품 정보</param>
        private void OnProductSelected(ProductInfo product)
        {
            var logRepository = LogRepositoryFactory.GetRepository(product.DatabaseName, product.ProviderType, product.ConnectionString);

            CurrentContent = new DashBoardView
            {
                DataContext = new DashBoardViewModel(logRepository)
            };
        }

        #endregion

        #region 메서드

        /// <summary>
        /// 홈버튼 커맨드 실행 시 호출되어, PieChartSummaryView를 표시
        /// </summary>
        private void ShowHome()
        {
            CurrentContent = new PieChartSummaryView
            {
                DataContext = new PieChartSummaryViewModel(SidebarViewModel.ProductList) // 필요 시 ViewModel 할당
            };
        }

        /// <summary>
        /// 속성 변경 알림 메서드
        /// </summary>
        /// <param name="propertyName">변경된 속성명</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
