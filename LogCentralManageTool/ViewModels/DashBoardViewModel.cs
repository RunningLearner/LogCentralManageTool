using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;

using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.Utils;

using SkiaSharp;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace LogCentralManageTool.ViewModels;

/// <summary>
/// 대시보드에서 최신 로그 데이터를 표시하기 위한 ViewModel입니다.
/// </summary>
public class DashBoardViewModel : INotifyPropertyChanged
{
    #region 이벤트
    /// <summary>
    /// 속성 변경 시 알림을 발생시키기 위한 이벤트.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;
    #endregion

    #region 필드
    // 외부에서 주입받은 LoggingDbContext 인스턴스를 저장하는 필드.
    private readonly LoggingDbContext _context;
    #endregion

    #region 속성
    /// <summary>
    /// 대시보드에 표시할 최신 로그 데이터.
    /// </summary>
    public Log SelectedLog { get; set; }

    /// <summary>
    /// 로그 데이터를 로그 레벨별로 그룹핑하여 생성한 차트의 시리즈 (Bar 형태)입니다.
    /// 각 시리즈는 해당 로그 레벨의 로그 건수를 나타냅니다.
    /// </summary>
    public ISeries[] Series { get; set; }

    /// <summary>
    /// "Info" 레벨 로그의 표시 여부 (토글 상태)입니다.
    /// </summary>
    public bool IsInfoVisible => Series?
        .FirstOrDefault(s => s.Name != null && s.Name.Equals("Info", StringComparison.OrdinalIgnoreCase))
        ?.IsVisible ?? false;

    /// <summary>
    /// "Warning" 레벨 로그의 표시 여부 (토글 상태)입니다.
    /// </summary>
    public bool IsWarningVisible => Series?
        .FirstOrDefault(s => s.Name != null && s.Name.Equals("Warning", StringComparison.OrdinalIgnoreCase))
        ?.IsVisible ?? false;

    /// <summary>
    /// "Error" 레벨 로그의 표시 여부 (토글 상태)입니다.
    /// </summary>
    public bool IsErrorVisible => Series?
        .FirstOrDefault(s => s.Name != null && s.Name.Equals("Error", StringComparison.OrdinalIgnoreCase))
        ?.IsVisible ?? false;

    /// <summary>
    /// X축에 날짜를 표시하기 위한 Axis 배열입니다.
    /// 이 예제에서는 X축 값을 ticks(정수)로 해석한 후, 날짜 문자열("yyyy-MM-dd")로 포맷팅합니다.
    /// 실제 날짜 데이터를 사용하는 경우 데이터를 해당 값에 맞게 매핑해야 합니다.
    /// </summary>
    public Axis[] XAxes { get; set; } = new Axis[]
    {
        new DateTimeAxis(TimeSpan.FromDays(1), date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))
    };
    #endregion

    #region 커맨드
    /// <summary>
    /// 로그 레벨(문자열)을 매개변수로 받아 해당 시리즈의 표시 여부를 토글하는 커맨드입니다.
    /// </summary>
    public ICommand ToggleSeriesCommand { get; }
    #endregion

    #region 생성자
    /// <summary>
    /// 생성자: 외부에서 주입받은 LoggingDbContext를 사용하여 최신 로그를 SelectedLog에 할당합니다.
    /// </summary>
    /// <param name="context">
    /// 제품 정보(데이터베이스명, 연결 문자열 등)에 맞게 이미 구성된 LoggingDbContext 인스턴스
    /// </param>
    public DashBoardViewModel(LoggingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        try
        {
            // 최신 로그 데이터를 SelectedLog에 할당
            SelectedLog = _context.Set<Log>()
                                  .OrderByDescending(l => l.Timestamp)
                                  .FirstOrDefault();

            // 전체 로그 데이터를 가져옵니다.
            var logs = _context.Set<Log>().ToList();

            // 모든 날짜(하루 단위) 목록과 로그 레벨 목록을 추출합니다.
            var distinctDates = logs.Select(l => l.Timestamp.Date)
                                    .Distinct()
                                    .OrderBy(d => d)
                                    .ToList();

            var distinctLevels = logs.Select(l => l.LogLevel)
                                     .Distinct()
                                     .OrderBy(l => l)
                                     .ToList();

            var seriesList = new List<ISeries>();

            // 로그 레벨별로 각 시리즈 생성 시 오프셋 값을 조정 (하루 내에서 30분씩 차이)
            foreach (var level in distinctLevels)
            {
                var points = new List<DateTimePoint>();

                // 각 로그 레벨에 따라 오프셋(시간 단위)을 아주 작게 지정합니다.
                double offsetHours = 0;
                if (level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                    offsetHours = -3;    
                else if (level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                    offsetHours = 3;     

                foreach (var date in distinctDates)
                {
                    // 해당 날짜와 로그 레벨에 해당하는 건수를 구합니다.
                    int count = logs.Count(l =>
                        l.Timestamp.Date == date &&
                        string.Equals(l.LogLevel, level, StringComparison.OrdinalIgnoreCase));

                    // 날짜에 오프셋(0.5시간)을 적용하여 DateTimePoint 생성.
                    // X값은 DateTime.Ticks 값이어야 하므로, date.AddHours(offsetHours).Ticks를 사용합니다.
                    var pointDate = date.AddHours(offsetHours);
                    points.Add(new DateTimePoint(pointDate, count));
                }

                // 각 로그 레벨에 따른 시리즈 색상 지정
                var fillColor = SKColors.Gray; // 기본값
                if (level.Equals("Info", StringComparison.OrdinalIgnoreCase))
                    fillColor = new SKColor(135, 206, 235); // SkyBlue
                else if (level.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                    fillColor = new SKColor(255, 165, 0); // Orange
                else if (level.Equals("Error", StringComparison.OrdinalIgnoreCase))
                    fillColor = new SKColor(255, 0, 0); // Red

                seriesList.Add(new ColumnSeries<DateTimePoint>
                {
                    Name = level,
                    Values = new ObservableCollection<DateTimePoint>(points),
                    Fill = new SolidColorPaint(fillColor),
                    // 여기서 DataPadding, MaxColumnWidth 등 추가 옵션을 조정해도 좋습니다.
                });
            }

            Series = seriesList.ToArray();
        }
        catch (Exception e)
        {
            // 예외 처리: 예외 발생 시 SelectedLog는 null, Series는 빈 배열 등으로 처리
            SelectedLog = null;
            Series = Array.Empty<ISeries>();
            // 실제 애플리케이션에서는 로깅 등 예외 처리를 추가합니다.
        }

        // ToggleSeriesCommand를 초기화 (매개변수로 전달된 로그 레벨에 따라 토글 수행)
        ToggleSeriesCommand = new RelayCommand(
            param => ToggleSeries(param),
            param => param is string
        );
    }
    #endregion

    #region 메서드
    /// <summary>
    /// 로그 레벨(문자열)을 매개변수로 받아, 해당 로그 레벨의 시리즈 표시 여부를 토글합니다.
    /// </summary>
    /// <param name="parameter">로그 레벨 문자열</param>
    private void ToggleSeries(object parameter)
    {
        string logLevel = parameter as string;
        if (string.IsNullOrEmpty(logLevel))
            return;

        var seriesToToggle = Series?.FirstOrDefault(s =>
            s.Name != null && s.Name.Equals(logLevel, StringComparison.OrdinalIgnoreCase));
        if (seriesToToggle != null)
        {
            seriesToToggle.IsVisible = !seriesToToggle.IsVisible;
            OnPropertyChanged(nameof(Series));

            // 로그 레벨별 계산 속성도 갱신하여 UI에 알림.
            if (logLevel.Equals("Info", StringComparison.OrdinalIgnoreCase))
                OnPropertyChanged(nameof(IsInfoVisible));
            else if (logLevel.Equals("Warning", StringComparison.OrdinalIgnoreCase))
                OnPropertyChanged(nameof(IsWarningVisible));
            else if (logLevel.Equals("Error", StringComparison.OrdinalIgnoreCase))
                OnPropertyChanged(nameof(IsErrorVisible));
        }
    }

    /// <summary>
    /// 속성 변경 시 PropertyChanged 이벤트를 발생시킵니다.
    /// </summary>
    /// <param name="prop">변경된 속성명 (CallerMemberName을 사용하여 자동 설정)</param>
    protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
    #endregion
}

