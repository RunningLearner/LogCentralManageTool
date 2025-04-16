using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.Data;
using LogCentralManageTool.ViewModels;
using Microsoft.EntityFrameworkCore;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LogCentralManageTool.Models;
using System.ComponentModel;
using Moq;

namespace LogCentralManageTool.Tests.ViewModels;

/// <summary>
/// PieChartSummaryViewModel의 동작(생성 시 집계, 컬렉션 변화 및 개별 제품 변경 시 재집계)을 검증하는 단위 테스트를 수행합니다.
/// 본 테스트는 InMemory 공급자를 사용하여 각 ProductInfo에 대해 미리 지정한 InMemory 데이터베이스에 시드 데이터를 추가하는 방식으로 진행합니다.
/// </summary>
[TestFixture]
public class PieChartSummaryViewModelTests
{
    private ObservableCollection<ProductInfo> _productInfos;

    /// <summary>
    /// 테스트 실행 전에 각 제품에 대한 InMemory 데이터베이스를 미리 시드합니다.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // 테스트용 제품 목록 생성
        _productInfos = new ObservableCollection<ProductInfo>
            {
                // InMemory 공급자의 경우, connectionString 매개변수로 데이터베이스 이름을 그대로 사용합니다.
                new ProductInfo { DatabaseName = "Product1DB", ConnectionString = "Product1DB", ProviderType = ProviderType.InMemory },
                new ProductInfo { DatabaseName = "Product2DB", ConnectionString = "Product2DB", ProviderType = ProviderType.InMemory }
            };

        // 각 제품에 대해 InMemory 데이터베이스를 생성하고 시드합니다.
        SeedInMemoryDatabase("Product1DB", "Info", 5);  // Product1DB에는 "Info" 로그 5건
        SeedInMemoryDatabase("Product2DB", "Error", 3); // Product2DB에는 "Error" 로그 3건
    }

    /// <summary>
    /// 테스트 목적:
    /// 생성자 호출 시, 전달된 ObservableCollection<ProductInfo> 기반으로 제품별 파이 차트 정보(Charts)가 생성되는지 검증합니다.
    ///
    /// 시나리오:
    /// 1. SetUp에서 두 제품(Product1DB, Product2DB)에 대해 각각 시드 데이터를 추가합니다.
    /// 2. PieChartSummaryViewModel을 생성하면 내부 UpdateCharts 메서드가 호출되어 각 제품의 집계 결과에 따라 파이 차트 정보가 생성됩니다.
    /// 3. 결과적으로 두 개의 ProductPieChartViewModel 객체가 Charts 컬렉션에 생성되어야 합니다.
    /// </summary>
    [Test]
    public void Constructor_InitializesCharts_BasedOnProductInfos()
    {
        // Act
        var viewModel = new PieChartSummaryViewModel(_productInfos);

        // Assert
        Assert.IsNotNull(viewModel.Charts, "생성자 호출 후 Charts는 null이 아니어야 합니다.");
        Assert.AreEqual(2, viewModel.Charts.Count, "두 개의 제품 정보가 주어졌으므로 Charts 컬렉션에는 2개의 항목이 있어야 합니다.");

        // 각 제품 차트의 시리즈 내용 검증 (더 상세하게)
        var product1Chart = viewModel.Charts.FirstOrDefault(c => c.DatabaseName == "Product1DB");
        Assert.IsNotNull(product1Chart, "Product1DB에 대한 차트 정보가 있어야 합니다.");
        Assert.AreEqual(1, product1Chart.Series.Count(), "Product1DB는 'Info' 로그만 있으므로 시리즈는 1개여야 합니다.");
        Assert.AreEqual("Info", product1Chart.Series.First().Name, "Product1DB 시리즈의 이름은 'Info'여야 합니다.");

        var product2Chart = viewModel.Charts.FirstOrDefault(c => c.DatabaseName == "Product2DB");
        Assert.IsNotNull(product2Chart, "Product2DB에 대한 차트 정보가 있어야 합니다.");
        Assert.AreEqual(1, product2Chart.Series.Count(), "Product2DB는 'Error' 로그만 있으므로 시리즈는 1개여야 합니다.");
        Assert.AreEqual("Error", product2Chart.Series.First().Name, "Product2DB 시리즈의 이름은 'Error'여야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// ObservableCollection의 변화가 발생하면 UpdateCharts가 호출되어 Charts가 업데이트되는지 검증합니다.
    ///
    /// 시나리오:
    /// 1. 초기 제품 목록을 사용하여 PieChartSummaryViewModel을 생성합니다.
    /// 2. 새로운 ProductInfo를 추가한 후, CollectionChanged 이벤트가 발생하여 Charts 컬렉션이 업데이트되는지 확인합니다.
    /// </summary>
    [Test]
    public void CollectionChanged_UpdatesCharts()
    {
        // Arrange
        var viewModel = new PieChartSummaryViewModel(_productInfos);
        int initialChartCount = viewModel.Charts.Count; // 초기 차트 개수 (제품 개수)

        // Act: 새로운 제품 추가
        string newDbName = "Product3DB";
        // Product3DB에 대해 "Warning" 로그 4건 시드
        SeedInMemoryDatabase(newDbName, "Warning", 4);
        var newProduct = new ProductInfo { DatabaseName = newDbName, ConnectionString = newDbName, ProviderType = ProviderType.InMemory };
        _productInfos.Add(newProduct);

        // Assert: 제품 추가 후 Charts 컬렉션이 업데이트되어야 합니다.
        Assert.AreEqual(initialChartCount + 1, viewModel.Charts.Count, "제품 추가 후 Charts 컬렉션의 개수가 증가해야 합니다.");
        var newChart = viewModel.Charts.FirstOrDefault(c => c.DatabaseName == newDbName);
        Assert.IsNotNull(newChart, "새로 추가된 제품에 대한 차트 정보가 Charts 컬렉션에 있어야 합니다.");
        Assert.AreEqual(1, newChart.Series.Count(), "새 제품은 'Warning' 로그만 있으므로 시리즈는 1개여야 합니다.");
        Assert.AreEqual("Warning", newChart.Series.First().Name, "새 제품 시리즈의 이름은 'Warning'이어야 합니다.");
    }

    /// <summary>
    /// ObservableProductInfo를 사용하여, 제품의 DatabaseName과 ConnectionString이 변경될 때
    /// PieChartSummaryViewModel의 Charts 속성이 올바르게 업데이트되고, "Charts"에 대한 PropertyChanged 이벤트가 발생하는지 검증합니다.
    /// 시나리오:
    /// 1. ObservableProductInfo 인스턴스를 생성(초기값 "Product1DB")하고, InMemory 데이터베이스에 시드 데이터를 추가합니다.
    /// 2. 이 제품을 ObservableCollection에 담아 PieChartSummaryViewModel을 생성합니다.
    /// 3. 제품의 DatabaseName과 ConnectionString을 "Product1DB_New"로 변경하고, 새 데이터베이스에 대해 시드 데이터를 추가합니다.
    /// 4. 이후 ViewModel의 Charts 컬렉션이 업데이트되어, 새 데이터베이스 이름으로 차트가 생성되는지 검증합니다.
    /// </summary>
    [Test]
    public void ProductInfo_PropertyChanged_RaisesChartsPropertyChanged_Simple()
    {
        // Arrange: ObservableProductInfo 인스턴스를 생성 및 초기값 설정 (생성자 사용)
        var product = new ObservableProductInfo("Product1DB", "Product1DB", ProviderType.InMemory);

        // 초기 데이터베이스 시드: "Product1DB"에 "Info" 로그 5건 시드
        SeedInMemoryDatabase("Product1DB", "Info", 5);

        // ObservableCollection에 제품 추가 후 ViewModel 생성
        var products = new ObservableCollection<ProductInfo> { product };
        var viewModel = new PieChartSummaryViewModel(products);

        bool chartsChanged = false;
        viewModel.PropertyChanged += (sender, args) =>
        {
            if (args.PropertyName == nameof(PieChartSummaryViewModel.Charts))
            {
                chartsChanged = true;
            }
        };

        // Assert 초기 상태: Charts 컬렉션에 "Product1DB"의 차트가 생성되어 있어야 함
        Assert.AreEqual(1, viewModel.Charts.Count, "초기 Charts 컬렉션에는 1개의 항목이 있어야 합니다.");

        // Act: 제품의 DatabaseName과 ConnectionString을 변경하고, 새 데이터베이스("Product1DB_New")에 "Error" 로그 7건 시드
        string newDbName = "Product1DB_New";
        product.DatabaseName = newDbName;
        product.ConnectionString = newDbName;
        SeedInMemoryDatabase(newDbName, "Error", 7);

        // 비동기 업데이트를 고려하여 약간의 지연 처리
        Thread.Sleep(100);

        // Assert: 제품 정보 변경 후 Charts 컬렉션이 업데이트되어야 하며, 새 데이터베이스 이름에 해당하는 차트가 존재해야 합니다.
        Assert.IsTrue(chartsChanged, "제품 정보 변경 시 Charts 속성이 변경되었음을 알리는 이벤트가 발생해야 합니다.");
        Assert.AreEqual(1, viewModel.Charts.Count, "Charts 컬렉션에는 여전히 1개의 항목이 있어야 합니다.");
        var updatedChart = viewModel.Charts.FirstOrDefault(c => c.DatabaseName == newDbName);
        Assert.IsNotNull(updatedChart, "새 데이터베이스 이름으로 업데이트된 차트가 있어야 합니다.");
        Assert.AreEqual(1, updatedChart.Series.Count(), "새 제품은 'Error' 로그만 있으므로 시리즈는 1개여야 합니다.");
        Assert.AreEqual("Error", updatedChart.Series.First().Name, "시리즈의 이름은 'Error'여야 합니다.");
    }

    /// <summary>
    /// 지정된 InMemory 데이터베이스 이름에 대해 기존 데이터를 삭제한 후,
    /// 주어진 로그 레벨의 로그를 지정한 건수만큼 시드하는 헬퍼 메서드입니다.
    /// </summary>
    /// <param name="dbName">InMemory 데이터베이스 이름</param>
    /// <param name="logLevel">로그 레벨(예: "Info", "Error", "Warning")</param>
    /// <param name="count">시드할 로그 건수</param>
    private void SeedInMemoryDatabase(string dbName, string logLevel, int count)
    {
        var options = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        // 기존 데이터 삭제
        using (var context = new MySQLLoggingDbContext(options))
        {
            context.Database.EnsureDeleted();
        }

        // 새 컨텍스트 생성하여 시드 데이터 추가
        using (var context = new MySQLLoggingDbContext(options))
        {
            for (int i = 0; i < count; i++)
            {
                context.Set<LogMySQL>().Add(new LogMySQL
                {
                    Id = (i + 1).ToString(),
                    Timestamp = DateTime.Now.AddMinutes(-i),
                    LogLevel = logLevel,
                    Message = $"{logLevel} log {i + 1}",
                    StackTrace = string.Empty
                });
            }
            context.SaveChanges();
        }
    }
}

/// <summary>
/// 테스트용으로 INotifyPropertyChanged를 구현한 ProductInfo 파생 클래스입니다.
/// 이 클래스는 제품의 속성이 변경될 때 PropertyChanged 이벤트를 발생시켜,
/// ViewModel이 해당 변경을 감지할 수 있도록 합니다.
/// </summary>
public class ObservableProductInfo : ProductInfo, INotifyPropertyChanged
{
    // 기본값을 빈 문자열로 할당하여 null 문제를 방지합니다.
    private string _connectionString = string.Empty;
    private string _databaseName = string.Empty;

    /// <summary>
    /// 기본 생성자: 기본값은 빈 문자열로 설정됩니다.
    /// </summary>
    public ObservableProductInfo() { }

    /// <summary>
    /// 초기 DatabaseName, ConnectionString, ProviderType을 설정하는 생성자입니다.
    /// </summary>
    /// <param name="databaseName">초기 데이터베이스 이름</param>
    /// <param name="connectionString">초기 커넥션 문자열</param>
    /// <param name="providerType">데이터 공급자 타입</param>
    public ObservableProductInfo(string databaseName, string connectionString, ProviderType providerType)
    {
        _databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        ProviderType = providerType;
    }

    /// <summary>
    /// ConnectionString 속성. 값 변경 시 PropertyChanged 이벤트가 발생합니다.
    /// </summary>
    public new string ConnectionString
    {
        get => _connectionString;
        set
        {
            if (_connectionString != value)
            {
                _connectionString = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ConnectionString)));
            }
        }
    }

    /// <summary>
    /// DatabaseName 속성. 값 변경 시 PropertyChanged 이벤트가 발생합니다.
    /// </summary>
    public new string DatabaseName
    {
        get => _databaseName;
        set
        {
            if (_databaseName != value)
            {
                _databaseName = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DatabaseName)));
            }
        }
    }

    /// <summary>
    /// INotifyPropertyChanged 이벤트.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
}
