using LogCentralManageTool.Models;
using LogCentralManageTool.Services;
using LogCentralManageTool.Utils;
using LogCentralManageTool.Views;

using System.Collections.ObjectModel;
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
        #region 이벤트

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region 필드

        private bool _isExpanded = true;
        private ProductInfo _selectedProduct;
        private ICommand _toggleCommand;
        private ICommand _addProductCommand;
        private ICommand _deleteProductCommand;

        #endregion

        #region 속성

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

        /// <summary>
        /// 로컬 JSON 파일에서 불러온 제품 목록입니다.
        /// </summary>
        public ObservableCollection<ProductInfo> ProductList { get; set; }

        /// <summary>
        /// 현재 선택된 제품. 변경 시 ProductSelected 이벤트 발생.
        /// </summary>
        public ProductInfo SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (_selectedProduct != value)
                {
                    _selectedProduct = value;
                    OnPropertyChanged();

                    // 선택된 제품이 null이 아닐 경우 이벤트 발생
                    if (_selectedProduct != null)
                    {
                        ProductSelected?.Invoke(_selectedProduct);
                    }
                }
            }
        }

        #endregion

        #region 커맨드

        /// <summary>
        /// 사이드바 토글 명령
        /// </summary>
        public ICommand ToggleCommand => _toggleCommand ??= new RelayCommand(o => Toggle());

        /// <summary>
        /// 제품 추가 명령
        /// </summary>
        public ICommand AddProductCommand => _addProductCommand ??= new RelayCommand(o => AddProduct());

        /// <summary>
        /// 제품 삭제 명령
        /// </summary>
        public ICommand DeleteProductCommand => _deleteProductCommand ??= new RelayCommand(o => DeleteProduct(), o => SelectedProduct != null);

        #endregion

        #region 생성자

        /// <summary>
        /// 생성자에서 로컬 JSON 파일의 제품 데이터를 로드합니다.
        /// </summary>
        public SidebarViewModel()
        {
            var products = ProductDataService.LoadProducts();
            ProductList = new ObservableCollection<ProductInfo>(products);
        }

        #endregion

        #region 메서드

        /// <summary>
        /// 사이드바 확장 상태를 토글합니다.
        /// </summary>
        private void Toggle()
        {
            IsExpanded = !IsExpanded;
        }

        /// <summary>
        /// 새 제품을 추가하는 메서드입니다.
        /// 실제 구현에서는 사용자 입력 폼을 통해 정보를 받는 것이 좋습니다.
        /// </summary>
        private void AddProduct()
        {
            var addProductWindow = new AddProductWindow();
            if (addProductWindow.ShowDialog() == true)
            {
                var newProduct = addProductWindow.ProductInfo;
                ProductList.Add(newProduct);
                ProductDataService.SaveProducts(new List<ProductInfo>(ProductList));
            }
        }

        /// <summary>
        /// 선택된 제품을 삭제하는 메서드입니다.
        /// </summary>
        private void DeleteProduct()
        {
            if (SelectedProduct == null)
                return;

            // 제품 목록에서 삭제
            ProductList.Remove(SelectedProduct);
            // JSON 파일에 변경 사항 저장
            ProductDataService.SaveProducts(new List<ProductInfo>(ProductList));

            // 선택 항목 초기화 (또는 다른 로직 처리)
            SelectedProduct = null;
        }

        /// <summary>
        /// 제품 선택 이벤트 (MainViewModel에서 구독)
        /// </summary>
        public event Action<ProductInfo> ProductSelected;

        /// <summary>
        /// 속성 변경 알림 메서드
        /// </summary>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
