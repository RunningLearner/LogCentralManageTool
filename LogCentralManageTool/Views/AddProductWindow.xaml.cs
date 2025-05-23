﻿using LogCentralManageTool.Data;
using LogCentralManageTool.Models;
using LogCentralManageTool.Services;
using LogCentralManageTool.ViewModels;

using System.Windows;

namespace LogCentralManageTool.Views;

/// <summary>
/// 제품 추가 대화상자입니다.
/// </summary>
public partial class AddProductWindow : Window
{
    /// <summary>
    /// 입력된 제품 정보를 외부로 전달합니다.
    /// </summary>
    public ProductInfo ProductInfo { get; private set; }

    public AddProductWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 추가 버튼 클릭 시 입력한 정보를 ProductInfo에 담고 대화상자를 닫습니다.
    /// </summary>
    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is AddProductViewModel vm)
        {
            ProductInfo = new ProductInfo
            {
                DatabaseName = vm.DatabaseName,
                ConnectionString = vm.ConnectionString,
                ProviderType = vm.SelectedProviderType
            };
            
            // 기존 목록에 추가하여 저장
            var products = ProductDataService.LoadProducts();
            products.Add(ProductInfo);
            ProductDataService.SaveProducts(products);

            DialogResult = true;
        }
        Close();
    }

    /// <summary>
    /// 취소 버튼 클릭 시 대화상자를 닫습니다.
    /// </summary>
    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}