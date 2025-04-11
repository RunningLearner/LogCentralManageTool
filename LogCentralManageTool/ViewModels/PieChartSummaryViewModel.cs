using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.Models;

using SkiaSharp;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Linq;

namespace LogCentralManageTool.ViewModels;

/// <summary>
/// 각 제품의 로그 데이터를 집계하여 로그 레벨별 파이 차트를 보여주는 ViewModel입니다.
/// </summary>
public class PieChartSummaryViewModel : INotifyPropertyChanged
{
    #region 이벤트

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

    #region 필드

    private ObservableCollection<ProductInfo> _productInfos;
    private ObservableCollection<ProductPieChartViewModel> _charts;

    #endregion

    #region 속성

    /// <summary>
    /// 각 제품별 파이 차트 정보를 담고 있는 ObservableCollection입니다.
    /// </summary>
    public ObservableCollection<ProductPieChartViewModel> Charts
    {
        get => _charts;
        private set
        {
            _charts = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region 생성자

    /// <summary>
    /// ObservableCollection&lt;ProductInfo&gt;를 받아 각 제품의 로그 데이터를 집계한 후, 제품별 파이 차트 시리즈를 생성합니다.
    /// </summary>
    /// <param name="productInfos">제품 정보 컬렉션</param>
    public PieChartSummaryViewModel(ObservableCollection<ProductInfo> productInfos)
    {
        _productInfos = productInfos;
        _productInfos.CollectionChanged += ProductInfos_CollectionChanged;

        // 기존 항목들의 개별 변경 감지 (ProductInfo가 INotifyPropertyChanged를 구현한 경우)
        foreach (var product in _productInfos)
        {
            if (product is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += ProductInfo_PropertyChanged;
            }
        }

        // 최초 집계 수행
        UpdateCharts();
    }

    #endregion

    #region 이벤트 핸들러

    /// <summary>
    /// 제품 컬렉션에 변화가 생겼을 때 호출됩니다.
    /// </summary>
    private void ProductInfos_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // 새로 추가된 항목들의 PropertyChanged 이벤트 구독
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged += ProductInfo_PropertyChanged;
                }
            }
        }

        // 제거된 항목들의 PropertyChanged 이벤트 구독 해제
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                if (item is INotifyPropertyChanged inpc)
                {
                    inpc.PropertyChanged -= ProductInfo_PropertyChanged;
                }
            }
        }

        // 컬렉션 변화가 있을 때마다 차트를 업데이트합니다.
        UpdateCharts();
    }

    /// <summary>
    /// 개별 ProductInfo 항목의 속성이 변경되었을 때 호출됩니다.
    /// </summary>
    private void ProductInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // 필요한 경우 특정 속성 변경에 대해서만 업데이트할 수 있습니다.
        UpdateCharts();
    }

    #endregion

    #region 메서드

    /// <summary>
    /// 각 제품의 로그 데이터를 집계하여 제품별 파이 차트 시리즈를 새로 생성합니다.
    /// </summary>
    private void UpdateCharts()
    {
        var charts = new List<ProductPieChartViewModel>();
        
        // 범주별로 미리 지정한 색상 매핑 (SKColor 사용)
        var categoryColors = new Dictionary<string, SKColor>
        {
            { "Info", new SKColor(30, 144, 255) },    // DodgerBlue
            { "Warning", new SKColor(255, 165, 0) },    // Orange
            { "Error", new SKColor(220, 20, 60) },      // Crimson
            { "Debug", new SKColor(34, 139, 34) }       // ForestGreen
        };

        // 각 제품별로 로그 데이터를 조회하고 집계
        foreach (var product in _productInfos)
        {
            try
            {
                // 각 ProductInfo를 통해 데이터베이스에 연결하여 해당 제품의 로그 데이터를 가져옵니다.
                var logRepository = LogRepositoryFactory.GetRepository(product.DatabaseName, product.ProviderType, product.ConnectionString);
                var logs = logRepository.GetAllLogs();

                // 로그 레벨별로 그룹핑하여 집계합니다.
                var groups = logs.GroupBy(log => log.LogLevel)
                                 .Select(g => new { LogLevel = g.Key, Count = g.Count() });

                // 그룹별 집계 결과로 파이 차트 시리즈 생성
                var seriesList = new List<ISeries>();
                foreach (var group in groups)
                {
                    // 색상 매핑 딕셔너리에서 해당 범주의 색상을 가져오거나, 기본 색상을 지정
                    SKColor fillColor = categoryColors.ContainsKey(group.LogLevel) ? categoryColors[group.LogLevel] : new SKColor(128, 128, 128);

                    var series = new PieSeries<double>
                    {
                        Values = new double[] { group.Count },
                        Name = group.LogLevel,
                        Fill = new SolidColorPaint(fillColor),
                        DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                        DataLabelsSize = 15,
                        DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30)),
                        DataLabelsFormatter = point => $"{group.LogLevel}: {point.Coordinate.PrimaryValue}",
                        ToolTipLabelFormatter = point => $"{point.StackedValue.Share:P2}"
                    };

                    seriesList.Add(series);
                }

                charts.Add(new ProductPieChartViewModel
                {
                    DatabaseName = product.DatabaseName,
                    Series = seriesList
                });
            }
            catch (Exception ex)
            {
                // 실제 애플리케이션에서는 예외 로깅 등의 처리를 추가합니다.
                Console.WriteLine($"Error processing product {product.DatabaseName}: {ex.Message}");
            }
        }

        // 제품별 파이 차트 컬렉션 업데이트 (UI 바인딩 자동 갱신)
        Charts = new ObservableCollection<ProductPieChartViewModel>(charts);
    }

    /// <summary>
    /// 속성 변경 알림 메서드
    /// </summary>
    /// <param name="propertyName">변경된 속성명</param>
    protected void OnPropertyChanged([CallerMemberName]string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    #endregion
}

/// <summary>
/// 각 제품의 데이터베이스 이름과 해당 파이 차트 시리즈 정보를 담는 모델입니다.
/// </summary>
public class ProductPieChartViewModel
{
    /// <summary>
    /// 제품의 데이터베이스 이름입니다.
    /// </summary>
    public string DatabaseName { get; set; }

    /// <summary>
    /// 제품의 로그 레벨별 집계 결과를 나타내는 파이 차트 시리즈 컬렉션입니다.
    /// </summary>
    public IEnumerable<ISeries> Series { get; set; }
}