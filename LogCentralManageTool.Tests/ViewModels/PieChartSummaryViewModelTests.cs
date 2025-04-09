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
    /// 지정된 InMemory 데이터베이스 이름에 대해, 기존 데이터는 삭제한 후 주어진 로그 레벨의 로그를 지정한 건수만큼 시드합니다.
    /// </summary>
    /// <param name="dbName">InMemory 데이터베이스 이름</param>
    /// <param name="logLevel">로그 레벨</param>
    /// <param name="count">추가할 로그 건수</param>
    private void SeedInMemoryDatabase(string dbName, string logLevel, int count)
    {
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        // 기존 데이터 삭제 처리: EnsureDeleted 호출
        using (var context = new LoggingDbContext(options))
        {
            context.Database.EnsureDeleted();
        }

        // 새 컨텍스트 생성하여 시드 데이터 추가
        using (var context = new LoggingDbContext(options))
        {
            for (int i = 0; i < count; i++)
            {
                context.Set<Log>().Add(new Log
                {
                    Id = i + 1,
                    Timestamp = DateTime.Now.AddMinutes(-i),
                    Message = $"{logLevel} log {i + 1}",
                    LogLevel = logLevel,
                    StackTrace = ""
                });
            }
            context.SaveChanges();
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// 생성자 호출 시, 전달된 ObservableCollection&lt;ProductInfo&gt; 기반으로 파이 차트 시리즈(Series)가 생성되는지 검증합니다.
    /// 
    /// 시나리오:
    /// 1. SetUp에서 두 제품(Product1DB, Product2DB)에 대해 각각 시드 데이터를 추가합니다.
    /// 2. PieChartSummaryViewModel을 생성하면 내부 UpdateSeries 메서드가 호출되어 집계 결과에 따라 파이 시리즈가 생성됩니다.
    /// 3. 집계 결과로 두 개의 로그 레벨("Info", "Error")에 해당하는 시리즈가 생성되어야 합니다.
    /// </summary>
    [Test]
    public void Constructor_InitializesSeries_BasedOnProductInfos()
    {
        // Act
        var viewModel = new PieChartSummaryViewModel(_productInfos);

        // Assert
        Assert.IsNotNull(viewModel.Series, "생성자 호출 후 Series는 null이 아니어야 합니다.");

        // 위 SetUp에 따르면, Product1DB의 "Info" 로그와 Product2DB의 "Error" 로그가 있으므로,
        // 집계 결과로 2개의 파이 슬라이스가 생성되어야 합니다.
        int seriesCount = viewModel.Series.Count();
        Assert.AreEqual(2, seriesCount, "집계 결과, 두 개의 로그 레벨('Info'와 'Error')에 해당하는 시리즈가 생성되어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// ObservableCollection의 변화가 발생하면 UpdateSeries가 호출되어 Series가 업데이트되는지 검증합니다.
    /// 
    /// 시나리오:
    /// 1. 초기 제품 목록을 사용하여 PieChartSummaryViewModel을 생성합니다.
    /// 2. 새로운 ProductInfo를 추가한 후, CollectionChanged 이벤트가 발생하여 시리즈가 다시 업데이트되는지 확인합니다.
    /// </summary>
    [Test]
    public void CollectionChanged_UpdatesSeries()
    {
        // Arrange
        var viewModel = new PieChartSummaryViewModel(_productInfos);
        int initialSeriesCount = viewModel.Series.Count();

        // Act: 새로운 제품 추가
        string newDbName = "Product3DB";
        // Product3DB에 대해 "Warning" 로그 4건 시드
        SeedInMemoryDatabase(newDbName, "Warning", 4);
        var newProduct = new ProductInfo { DatabaseName = newDbName, ConnectionString = newDbName, ProviderType = ProviderType.InMemory };
        _productInfos.Add(newProduct);

        // Assert: 제품 추가 후 Series 집계가 업데이트되어야 합니다.
        int updatedSeriesCount = viewModel.Series.Count();
        // 기존 두 제품에서 "Info"와 "Error" 시리즈가 있었고, 새 제품은 "Warning" 시리즈를 추가하므로 총 3개가 되어야 함
        Assert.AreEqual(3, updatedSeriesCount, "제품 추가 후, 세 개의 로그 레벨이 집계되어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// 개별 ProductInfo 항목의 속성 변경이 발생하면, 해당 항목의 PropertyChanged 이벤트를 통해 UpdateSeries가 호출되어 
    /// Series가 재계산되는지 검증합니다.
    /// 
    /// 시나리오:
    /// 1. 초기 제품 목록(ObservableProductInfo)을 사용하여 PieChartSummaryViewModel을 생성합니다.
    /// 2. 기존 제품 중 하나의 DatabaseName과 ConnectionString을 동시에 변경하여, 새로운 InMemory 데이터베이스를 사용하도록 합니다.
    /// 3. 새 데이터베이스에 대해 시드 데이터를 추가하고, 변경 후 PropertyChanged 이벤트가 발생하여 UpdateSeries가 호출되는지 확인합니다.
    /// 4. Series 속성에 변경(즉, PropertyChanged 이벤트 "Series"가 발생)이 되었음을 검증합니다.
    /// </summary>
    [Test]
    public void ProductInfo_PropertyChanged_RaisesSeriesPropertyChanged()
    {
        // Arrange
        var observableProduct = new ObservableProductInfo
        {
            DatabaseName = "Product1DB",
            ConnectionString = "Product1DB",
            ProviderType = ProviderType.InMemory
        };
        var productList = new ObservableCollection<ProductInfo> { observableProduct };

        // 초기 InMemory 데이터베이스 시드
        SeedInMemoryDatabase("Product1DB", "Info", 5);

        var viewModel = new PieChartSummaryViewModel(productList);
        bool seriesChanged = false;
        viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == "Series")
                seriesChanged = true;
        };

        // Act: 제품의 DatabaseName과 ConnectionString을 동시에 변경하여 새로운 InMemory DB 사용
        observableProduct.DatabaseName = "Product1DB_New";
        observableProduct.ConnectionString = "Product1DB_New";
        // 새 데이터베이스에 대해 시드 데이터를 추가 (예: "Info" 로그 7건)
        SeedInMemoryDatabase("Product1DB_New", "Info", 7);

        // Assert: PropertyChanged 이벤트가 "Series"에 대해 발생해야 함
        Assert.IsTrue(seriesChanged, "제품 정보 변경 시, Series 속성의 PropertyChanged 이벤트가 발생해야 합니다.");
    }
}

/// <summary>
/// 테스트용으로 INotifyPropertyChanged를 구현한 ProductInfo 파생 클래스입니다.
/// </summary>
public class ObservableProductInfo : ProductInfo, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;
    private string _connectionString;

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
}