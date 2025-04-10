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
    private IEnumerable<ISeries> _series;

    #endregion

    #region 속성

    /// <summary>
    /// LiveChartsCore의 ISeries 컬렉션으로 구성된 파이 차트 시리즈입니다.
    /// </summary>
    public IEnumerable<ISeries> Series
    {
        get => _series;
        private set
        {
            _series = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region 생성자

    /// <summary>
    /// ObservableCollection&lt;ProductInfo&gt;를 받아 각 제품의 로그 데이터를 집계한 후, 파이 차트 시리즈를 생성합니다.
    /// </summary>
    /// <param name="productInfos">제품 정보 컬렉션</param>
    public PieChartSummaryViewModel(ObservableCollection<ProductInfo> productInfos)
    {
        _productInfos = productInfos;
        // ObservableCollection의 변화(추가, 삭제 등)를 감지
        _productInfos.CollectionChanged += ProductInfos_CollectionChanged;

        // 기존 항목들의 개별 변경도 감지 (ProductInfo가 INotifyPropertyChanged를 구현하는 경우)
        foreach (var product in _productInfos)
        {
            if (product is INotifyPropertyChanged inpc)
            {
                inpc.PropertyChanged += ProductInfo_PropertyChanged;
            }
        }

        // 최초 집계 수행
        UpdateSeries();
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

        // 컬렉션 변화가 있을 때마다 시리즈를 업데이트
        UpdateSeries();
    }

    /// <summary>
    /// 개별 ProductInfo 항목의 속성이 변경되었을 때 호출됩니다.
    /// </summary>
    private void ProductInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        // 필요한 경우 특정 속성 변경에 대해서만 업데이트할 수 있습니다.
        UpdateSeries();
    }

    #endregion

    #region 메서드

    /// <summary>
    /// 로그 데이터를 집계하여 파이 차트 시리즈를 새로 생성합니다.
    /// </summary>
    private void UpdateSeries()
    {
        // 로그 레벨별 총 건수를 저장할 딕셔너리
        var aggregateCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        // 각 제품별로 DbContext를 생성하고 로그 데이터를 조회
        foreach (var product in _productInfos)
        {
            try
            {
                // 각 ProductInfo를 통해 데이터베이스 연결
                var context = DbContextFactory.GetContext(product.DatabaseName, product.ProviderType, product.ConnectionString);

                // Log 엔티티 테이블의 로그 데이터를 조회
                var logs = context.Set<Log>().ToList();

                // 로그 레벨별로 그룹핑하여 집계
                var groups = logs.GroupBy(log => log.LogLevel)
                                 .Select(g => new { LogLevel = g.Key, Count = g.Count() });

                // 집계 결과 누적
                foreach (var group in groups)
                {
                    if (aggregateCounts.ContainsKey(group.LogLevel))
                        aggregateCounts[group.LogLevel] += group.Count;
                    else
                        aggregateCounts[group.LogLevel] = group.Count;
                }
            }
            catch (Exception ex)
            {
                // 실제 애플리케이션에서는 예외 처리 로직(로깅 등)을 추가합니다.
                Console.WriteLine($"Error processing product {product.DatabaseName}: {ex.Message}");
            }
        }

        // 집계된 데이터를 기반으로 파이 차트 시리즈 생성
        var seriesList = new List<ISeries>();

        foreach (var kvp in aggregateCounts)
        {
            // 각 로그 레벨의 건수를 파이 차트 한 슬라이스로 표현
            var series = new PieSeries<double>
            {
                Values = new double[] { kvp.Value }, // 슬라이스의 값 (로그 건수)
                Name = kvp.Key,                     // 로그 레벨 명칭
                DataLabelsPosition = LiveChartsCore.Measure.PolarLabelsPosition.Outer,
                DataLabelsSize = 15,
                DataLabelsPaint = new SolidColorPaint(new SKColor(30, 30, 30)),
                DataLabelsFormatter = point => $"{kvp.Key}: {point.Coordinate.PrimaryValue}",
                ToolTipLabelFormatter = point => $"{point.StackedValue.Share:P2}"
            };

            seriesList.Add(series);
        }

        // Series 프로퍼티 업데이트 (UI 바인딩이 자동 갱신됨)
        Series = seriesList;
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