using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.Models;
using LogCentralManageTool.ViewModels;
using LogCentralManageTool.Views;

using Microsoft.EntityFrameworkCore;

using System.Windows;

namespace LogCentralManageTool.Tests.ViewModels;

/// <summary>
/// MainViewModel의 동작(초기 SidebarWidth, 사이드바 상태 변경, 그리고 ProductSelected 이벤트 처리)을 검증하는 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class MainViewModelTests
{
    /// <summary>
    /// 테스트 목적:
    /// MainViewModel 생성 시 기본 SidebarViewModel의 IsExpanded 상태에 따라 SidebarWidth가 올바르게 계산되는지 검증합니다.
    /// 시나리오:
    /// 1. 기본 생성 시 SidebarViewModel.IsExpanded가 false라면 SidebarWidth는 40이어야 합니다.
    /// 2. 이후 IsExpanded를 true로 변경하면 SidebarWidth가 200으로 업데이트되어야 합니다.
    /// </summary>
    [Test]
    [Apartment(ApartmentState.STA)] // UI 구성 요소를 사용하므로 STA 스레드에서 실행
    public void SidebarWidth_ShouldReflectIsExpandedState()
    {
        // Arrange
        var mainViewModel = new MainViewModel();

        // Assert 초기 상태: 기본적으로 SidebarViewModel.IsExpanded는 True
        Assert.IsTrue(mainViewModel.SidebarViewModel.IsExpanded, "기본 SidebarViewModel의 IsExpanded는 True여야 합니다.");
        Assert.AreEqual(new GridLength(200), mainViewModel.SidebarWidth, "IsExpanded가 true일 때, SidebarWidth는 200이어야 합니다.");

        // Act: IsExpanded 값을 변경
        mainViewModel.SidebarViewModel.IsExpanded = false;

        // Assert: SidebarWidth가 40 변경되어야 함
        Assert.AreEqual(new GridLength(40), mainViewModel.SidebarWidth, "IsExpanded가 false일 때, SidebarWidth는 40이어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// SidebarViewModel의 IsExpanded 속성이 변경될 때, MainViewModel에서 'SidebarWidth'에 대한 PropertyChanged 이벤트가 발생하는지 검증합니다.
    /// 시나리오:
    /// 1. MainViewModel의 PropertyChanged 이벤트를 구독합니다.
    /// 2. SidebarViewModel의 IsExpanded 값을 변경한 후 'SidebarWidth'가 포함된 이벤트가 발생하는지 확인합니다.
    /// </summary>
    [Test]
    [Apartment(ApartmentState.STA)] // UI 구성 요소를 사용하므로 STA 스레드에서 실행
    public void PropertyChanged_Event_FiresForSidebarWidth_WhenIsExpandedChanges()
    {
        // Arrange
        var mainViewModel = new MainViewModel();
        bool eventFired = false;
        mainViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "SidebarWidth")
                eventFired = true;
        };

        // Act: IsExpanded 변경
        mainViewModel.SidebarViewModel.IsExpanded = !mainViewModel.SidebarViewModel.IsExpanded;

        // Assert
        Assert.IsTrue(eventFired, "SidebarViewModel의 IsExpanded 변경 시, SidebarWidth에 대한 PropertyChanged 이벤트가 발생해야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// SidebarViewModel의 ProductSelected 이벤트가 발생하면 MainViewModel의 OnProductSelected 메서드가 호출되어,
    /// CurrentContent 속성이 DashBoardView 인스턴스로 업데이트되고, 그 DataContext가 DashBoardViewModel 타입인지 검증합니다.
    /// 
    /// 시나리오:
    /// 1. MainViewModel을 생성합니다.
    /// 2. 테스트용 ProductInfo(유효한 DatabaseName과 ConnectionString 포함)를 준비합니다.
    /// 3. SidebarViewModel의 ProductSelected 이벤트를 강제로 발생시켜 OnProductSelected 메서드를 실행시킵니다.
    /// 4. MainViewModel.CurrentContent가 DashBoardView 인스턴스로 설정되고, 그 DataContext가 DashBoardViewModel 타입임을 확인합니다.
    /// </summary>
    [Test]
    [Apartment(ApartmentState.STA)] // UI 구성 요소를 사용하므로 STA 스레드에서 실행
    public void ProductSelected_Event_UpdatesCurrentContent()
    {
        // Arrange
        var mainViewModel = new MainViewModel();

        // 테스트용 ProductInfo. 실제 테스트 환경에서는 유효한 ConnectionString 값을 전달해야 합니다.
        var product = new ProductInfo
        {
            DatabaseName = "TestDB",
            ConnectionString = "ValidConnectionString"
        };

        // Act: SidebarViewModel의 ProductSelected 이벤트를 강제로 발생시켜 OnProductSelected를 호출합니다.
        // 이벤트는 외부에서 직접 호출할 수 없으므로 Reflection 기반의 헬퍼(EventRaiser)를 사용합니다.
        EventRaiser.RaiseEvent(mainViewModel.SidebarViewModel, "ProductSelected", product);

        // Assert: CurrentContent가 DashBoardView 인스턴스로 업데이트되었는지 확인합니다.
        Assert.IsNotNull(mainViewModel.CurrentContent, "ProductSelected 이벤트 처리 후, CurrentContent는 null이 아니어야 합니다.");

        // DashBoardView 타입으로 캐스팅하여 DataContext를 확인합니다.
        var dashBoardView = mainViewModel.CurrentContent as DashBoardView;
        Assert.IsNotNull(dashBoardView, "CurrentContent는 DashBoardView 인스턴스여야 합니다.");
        Assert.IsNotNull(dashBoardView.DataContext, "DashBoardView의 DataContext는 null이 아니어야 합니다.");
        Assert.IsInstanceOf<DashBoardViewModel>(dashBoardView.DataContext, "DataContext는 DashBoardViewModel 타입이어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// HomeCommand 실행 시 MainViewModel의 ShowHome 메서드가 호출되어,
    /// CurrentContent가 PieChartSummaryView 인스턴스로 업데이트되고, 그 DataContext가 PieChartSummaryViewModel 타입임을 검증합니다.
    /// 
    /// 시나리오:
    /// 1. MainViewModel 인스턴스를 생성합니다.
    /// 2. HomeCommand를 실행합니다.
    /// 3. MainViewModel.CurrentContent가 PieChartSummaryView 인스턴스로 설정되고, 그 DataContext가 PieChartSummaryViewModel 타입임을 확인합니다.
    /// </summary>
    [Test]
    [Apartment(ApartmentState.STA)]
    public void HomeCommand_UpdatesCurrentContent_ToPieChartSummaryView()
    {
        // Arrange
        var mainViewModel = new MainViewModel();

        // Act: HomeCommand 실행
        mainViewModel.HomeCommand.Execute(null);

        // Assert
        Assert.IsNotNull(mainViewModel.CurrentContent, "HomeCommand 실행 후, CurrentContent는 null이 아니어야 합니다.");

        var homeView = mainViewModel.CurrentContent as PieChartSummaryView;
        Assert.IsNotNull(homeView, "CurrentContent는 PieChartSummaryView 인스턴스여야 합니다.");
        Assert.IsNotNull(homeView.DataContext, "PieChartSummaryView의 DataContext는 null이 아니어야 합니다.");
        Assert.IsInstanceOf<PieChartSummaryViewModel>(homeView.DataContext, "DataContext는 PieChartSummaryViewModel 타입이어야 합니다.");
    }
}
