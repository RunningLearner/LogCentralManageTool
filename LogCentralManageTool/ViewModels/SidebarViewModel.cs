﻿using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
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

        /// <summary>
        /// 로컬 JSON 파일에서 불러온 제품 목록입니다.
        /// </summary>
        public ObservableCollection<ProductInfo> ProductList { get; set; }

        private ICommand _addProductCommand;

        /// <summary>
        /// 제품을 추가하는 명령입니다.
        /// </summary>
        public ICommand AddProductCommand => _addProductCommand ??= new RelayCommand(o => AddProduct());

        /// <summary>
        /// 생성자에서 로컬 JSON 파일의 제품 데이터를 로드합니다.
        /// </summary>
        public SidebarViewModel()
        {
            var products = ProductDataService.LoadProducts();
            ProductList = new ObservableCollection<ProductInfo>(products);
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

        private ProductInfo _selectedProduct;

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

        /// <summary>
        /// 제품 선택 이벤트 (MainViewModel에서 구독)
        /// </summary>
        public event Action<ProductInfo> ProductSelected;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
