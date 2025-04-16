using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.Data.Repositories;
using LogCentralManageTool.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.Defaults;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView;

namespace LogCentralManageTool.Tests.ViewModels;

/// <summary>
/// DashBoardViewModel의 생성자, 초기 상태, 시리즈 생성, 토글 커맨드, 범위 필터링 동작을 검증하는 단위 테스트 클래스입니다.
/// </summary>
[TestFixture]
public class DashBoardViewModelTests
{
    private DbContextOptions<MySQLLoggingDbContext> _options;
    private MySQLLoggingDbContext _context;
    private ILogRepository _repository;

    // 각 테스트 실행 전에 인메모리 데이터베이스 설정
    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB_" + Guid.NewGuid().ToString()) // 각 테스트마다 고유 DB 사용
            .Options;
        _context = new MySQLLoggingDbContext(_options);
        _repository = new MySQLLogRepository(_context);
    }

    // 각 테스트 실행 후에 컨텍스트 정리
    [TearDown]
    public void TearDown()
    {
        _context?.Dispose();
    }

    /// <summary>
    /// DashBoardViewModel 생성자에 null을 전달할 경우, ArgumentNullException이 발생하는지 검증합니다.
    /// </summary>
    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenRepositoryIsNull()
    {
        // Act & Assert: null 전달 시 ArgumentNullException 발생
        Assert.Throws<ArgumentNullException>(() => new DashBoardViewModel(null));
    }

    /// <summary>
    /// 로그 데이터가 없을 때 생성자가 호출되면 빈 데이터를 가진 세 가지 시리즈가 생성되고 SelectedLogs가 비어 있는지 검증합니다.
    /// </summary>
    [Test]
    public void Constructor_InitializesEmpty_WhenNoLogsExist()
    {
        // Arrange (Setup에서 빈 DB 생성됨)

        // Act
        var viewModel = new DashBoardViewModel(_repository);

        // Assert
        Assert.IsNotNull(viewModel.Series, "Series는 null이 아니어야 합니다.");
        Assert.AreEqual(3, viewModel.Series.Length, "로그가 없어도 Info, Warning, Error 3개의 시리즈가 생성되어야 합니다.");

        // 각 시리즈가 존재하고 데이터 포인트가 비어있거나 0인지 확인
        var logLevels = new[] { "Info", "Warning", "Error" };
        foreach (var level in logLevels)
        {
            var series = viewModel.Series.FirstOrDefault(s => s.Name == level);
            Assert.IsNotNull(series, $"{level} 시리즈가 존재해야 합니다.");
            var values = series.Values as IEnumerable<DateTimePoint>;
            Assert.IsNotNull(values, $"{level} 시리즈 값은 DateTimePoint 컬렉션이어야 합니다.");
            Assert.IsTrue(values.All(p => p.Value == 0), $"{level} 시리즈의 모든 포인트는 값이 0이어야 합니다.");
        }

        // SelectedLogs 검증
        Assert.IsNotNull(viewModel.SelectedLogs, "SelectedLogs는 null이 아니어야 합니다.");
        Assert.IsEmpty(viewModel.SelectedLogs, "초기 SelectedLogs는 비어 있어야 합니다.");

        // AxisMin/Max의 기본값 확인
        Assert.AreEqual(default(DateTime), viewModel.AxisMin, "초기 AxisMin은 DateTime 기본값이어야 합니다.");
        Assert.AreEqual(default(DateTime), viewModel.AxisMax, "초기 AxisMax는 DateTime 기본값이어야 합니다.");
    }

    /// <summary>
    /// 로그 데이터가 존재할 때 생성자가 호출되면 Series가 올바르게 생성되고 SelectedLogs는 초기에 비어 있는지 검증합니다.
    /// (UpdateSelectedLogsBasedOnRange는 기본 Axis 범위로 인해 초기에는 비어있을 가능성이 높음)
    /// </summary>
    [Test]
    public void Constructor_InitializesSeriesAndEmptyLogs_WhenLogsExist()
    {
        // Arrange: 여러 로그 추가
        SeedDatabaseWithSampleLogs();

        // Act
        var viewModel = new DashBoardViewModel(_repository);

        // Assert
        Assert.IsNotNull(viewModel.Series, "Series는 null이 아니어야 합니다.");
        Assert.IsNotEmpty(viewModel.Series, "로그가 있으면 Series는 비어 있지 않아야 합니다.");
        Assert.AreEqual(3, viewModel.Series.Length, "Info, Warning, Error 3개의 시리즈가 생성되어야 합니다."); // 예시 로그 기준
        Assert.IsNotNull(viewModel.SelectedLogs, "SelectedLogs는 null이 아니어야 합니다.");
        // 기본 AxisMin/Max (DateTime.MinValue/MinValue) 범위에는 로그가 없으므로 비어 있어야 합니다.
        Assert.IsEmpty(viewModel.SelectedLogs, "초기 SelectedLogs는 비어 있어야 합니다 (기본 Axis 범위).");
    }

    /// <summary>
    /// 생성 시 로그 데이터를 기반으로 Series가 올바르게 생성되는지 상세하게 검증합니다.
    /// (이름, 데이터 포인트, 색상 등)
    /// </summary>
    [Test]
    public void Constructor_CreatesSeriesCorrectly_WithLogData()
    {
        // Arrange: 특정 날짜와 레벨의 로그 추가
        var testDate = new DateTime(2024, 5, 20);
        _context.Set<LogMySQL>().AddRange(
            new LogMySQL { Id = "1", Timestamp = testDate, LogLevel = "Info", Message = "Test" },
            new LogMySQL { Id = "2", Timestamp = testDate, LogLevel = "Info", Message = "Test" }, // Info 2건
            new LogMySQL { Id = "3", Timestamp = testDate.AddDays(1), LogLevel = "Warning", Message = "Test" } // Warning 1건 (다른 날짜)
        );
        _context.SaveChanges();

        // Act
        var viewModel = new DashBoardViewModel(_repository);

        // Assert: Series 검증
        Assert.AreEqual(3, viewModel.Series.Length, "Info, Warning, Error 3개의 시리즈가 생성되어야 합니다.");

        // Info 시리즈 검증
        var infoSeries = viewModel.Series.FirstOrDefault(s => s.Name == "Info");
        Assert.IsNotNull(infoSeries, "Info 시리즈가 존재해야 합니다.");
        Assert.IsTrue(infoSeries.IsVisible, "Info 시리즈는 기본적으로 보여야 합니다.");
        var infoValues = infoSeries.Values as IEnumerable<DateTimePoint>;
        Assert.IsNotNull(infoValues, "Info 시리즈 값은 DateTimePoint 컬렉션이어야 합니다.");
        var infoPoint = infoValues.FirstOrDefault(p => p.DateTime.Date == testDate.Date);
        Assert.IsNotNull(infoPoint, $"{testDate.Date}에 해당하는 Info 데이터 포인트가 있어야 합니다.");
        Assert.AreEqual(2, infoPoint.Value, $"{testDate.Date}의 Info 로그 건수는 2여야 합니다.");
        var infoColumnSeries = infoSeries as ColumnSeries<DateTimePoint>;
        Assert.IsNotNull(infoColumnSeries, "Info 시리즈는 ColumnSeries<DateTimePoint> 타입이어야 합니다.");
        var infoPaint = infoColumnSeries.Fill as SolidColorPaint;
        Assert.IsNotNull(infoPaint, "Info 시리즈 채우기는 SolidColorPaint여야 합니다.");
        Assert.AreEqual(new SKColor(135, 206, 235), infoPaint.Color, "Info 시리즈 색상이 올바른지 확인합니다.");

        // Warning 시리즈 검증
        var warningSeries = viewModel.Series.FirstOrDefault(s => s.Name == "Warning");
        Assert.IsNotNull(warningSeries, "Warning 시리즈가 존재해야 합니다.");
        Assert.IsTrue(warningSeries.IsVisible, "Warning 시리즈는 기본적으로 보여야 합니다.");
        var warningValues = warningSeries.Values as IEnumerable<DateTimePoint>;
        Assert.IsNotNull(warningValues, "Warning 시리즈 값은 DateTimePoint 컬렉션이어야 합니다.");

        // 정확한 날짜와 값을 가진 포인트가 있는지 확인
        var targetWarningPoint = warningValues.FirstOrDefault(p => 
            Math.Abs((p.DateTime - testDate.AddDays(1).AddHours(-3)).TotalMinutes) < 1 && 
            p.Value == 1);
        Assert.IsNotNull(targetWarningPoint, 
            "Warning 시리즈에는 기대하는 날짜(-3시간 오프셋 적용)와 값(1)을 가진 포인트가 있어야 합니다.");

        var warningColumnSeries = warningSeries as ColumnSeries<DateTimePoint>;
        Assert.IsNotNull(warningColumnSeries, "Warning 시리즈는 ColumnSeries<DateTimePoint> 타입이어야 합니다.");
        var warningPaint = warningColumnSeries.Fill as SolidColorPaint;
        Assert.IsNotNull(warningPaint, "Warning 시리즈 채우기는 SolidColorPaint여야 합니다.");
        Assert.AreEqual(new SKColor(255, 165, 0), warningPaint.Color, "Warning 시리즈 색상이 올바른지 확인합니다.");

        // Error 시리즈 검증 (데이터는 없지만 시리즈는 존재해야 함)
        var errorSeries = viewModel.Series.FirstOrDefault(s => s.Name == "Error");
        Assert.IsNotNull(errorSeries, "Error 시리즈가 존재해야 합니다.");
        Assert.IsTrue(errorSeries.IsVisible, "Error 시리즈는 기본적으로 보여야 합니다.");
        var errorValues = errorSeries.Values as IEnumerable<DateTimePoint>;
        Assert.IsNotNull(errorValues, "Error 시리즈 값은 DateTimePoint 컬렉션이어야 합니다.");
        // Error 로그가 없으므로 모든 포인트의 값이 0이어야 함
        Assert.IsTrue(errorValues.All(p => p.Value == 0), "Error 시리즈의 모든 포인트는 값이 0이어야 합니다.");
    }


    /// <summary>
    /// ToggleSeriesCommand를 실행하면, 해당 로그 레벨의 시리즈 표시 여부와 관련 계산 속성이 토글되는지 검증합니다.
    /// </summary>
    [Test]
    public void ToggleSeriesCommand_TogglesVisibilityAndProperties_ForGivenLevel()
    {
        // Arrange: 여러 로그 레벨 추가
        SeedDatabaseWithSampleLogs();
        var viewModel = new DashBoardViewModel(_repository);

        // 초기 상태 확인
        var warningSeries = viewModel.Series.First(s => s.Name == "Warning");
        Assert.IsTrue(warningSeries.IsVisible, "초기 Warning 시리즈는 표시되어야 합니다.");
        Assert.IsTrue(viewModel.IsWarningVisible, "초기 IsWarningVisible 속성은 true여야 합니다.");

        // Act: ToggleSeriesCommand 실행 (Warning 토글)
            viewModel.ToggleSeriesCommand.Execute("Warning");

        // Assert: 토글 후 상태 확인
        Assert.IsFalse(warningSeries.IsVisible, "Toggle 후 Warning 시리즈 IsVisible은 false여야 합니다.");
        Assert.IsFalse(viewModel.IsWarningVisible, "Toggle 후 IsWarningVisible 속성은 false여야 합니다.");

        // Act: 다시 ToggleSeriesCommand 실행 (Warning 다시 토글)
            viewModel.ToggleSeriesCommand.Execute("Warning");

        // Assert: 다시 토글 후 상태 확인
        Assert.IsTrue(warningSeries.IsVisible, "다시 Toggle 후 Warning 시리즈 IsVisible은 true여야 합니다.");
        Assert.IsTrue(viewModel.IsWarningVisible, "다시 Toggle 후 IsWarningVisible 속성은 true여야 합니다.");
    }

    /// <summary>
    /// AxisMin 또는 AxisMax 속성 변경 시 SelectedLogs가 올바르게 필터링되고 PropertyChanged 이벤트가 발생하는지 검증합니다.
    /// </summary>
    [Test]
    public void AxisRangeProperties_FiltersSelectedLogs_AndRaisesPropertyChanged()
    {
        // Arrange: 여러 날짜에 걸친 로그 추가
        var date1 = new DateTime(2024, 5, 20);
        var date2 = new DateTime(2024, 5, 21);
        var date3 = new DateTime(2024, 5, 22);
        _context.Set<LogMySQL>().AddRange(
            new LogMySQL { Id = "1", Timestamp = date1.AddHours(10), LogLevel = "Info", Message = "Log 1" }, // 5/20
            new LogMySQL { Id = "2", Timestamp = date2.AddHours(11), LogLevel = "Warning", Message = "Log 2" },// 5/21
            new LogMySQL { Id = "3", Timestamp = date2.AddHours(15), LogLevel = "Info", Message = "Log 3" },   // 5/21
            new LogMySQL { Id = "4", Timestamp = date3.AddHours(9), LogLevel = "Error", Message = "Log 4" }    // 5/22
        );
        _context.SaveChanges();

        var viewModel = new DashBoardViewModel(_repository);
        List<string> receivedEvents = new List<string>();
        viewModel.PropertyChanged += (sender, args) => receivedEvents.Add(args.PropertyName);

        // Act: AxisMin, AxisMax 설정 (5월 21일 범위)
        viewModel.AxisMin = date2; // 2024-05-21 00:00:00
        viewModel.AxisMax = date2.AddDays(1).AddTicks(-1); // 2024-05-21 23:59:59...

        // Assert: PropertyChanged 이벤트 확인
        Assert.Contains(nameof(viewModel.AxisMin), receivedEvents, "AxisMin 변경 시 PropertyChanged 발생해야 합니다.");
        Assert.Contains(nameof(viewModel.AxisMax), receivedEvents, "AxisMax 변경 시 PropertyChanged 발생해야 합니다.");
        Assert.Contains(nameof(viewModel.SelectedLogs), receivedEvents, "범위 변경으로 SelectedLogs 변경 시 PropertyChanged 발생해야 합니다.");

        // Assert: SelectedLogs 필터링 확인
        Assert.AreEqual(2, viewModel.SelectedLogs.Count, "SelectedLogs에는 5월 21일 로그 2개만 포함되어야 합니다.");
        Assert.IsTrue(viewModel.SelectedLogs.All(log => log.Timestamp >= date2 && log.Timestamp < date2.AddDays(1)),
                      "SelectedLogs의 모든 로그는 설정된 범위 내에 있어야 합니다.");
        Assert.IsTrue(viewModel.SelectedLogs.Any(log => log.Id == "2"), "ID '2' 로그가 포함되어야 합니다.");
        Assert.IsTrue(viewModel.SelectedLogs.Any(log => log.Id == "3"), "ID '3' 로그가 포함되어야 합니다.");

        // Act: 범위 변경 (5월 20일)
        receivedEvents.Clear();
        viewModel.AxisMin = date1;
        viewModel.AxisMax = date1.AddDays(1).AddTicks(-1);

         // Assert: PropertyChanged 이벤트 확인 (SelectedLogs 포함)
        Assert.Contains(nameof(viewModel.AxisMin), receivedEvents);
        Assert.Contains(nameof(viewModel.AxisMax), receivedEvents);
        Assert.Contains(nameof(viewModel.SelectedLogs), receivedEvents);

        // Assert: SelectedLogs 필터링 확인
        Assert.AreEqual(1, viewModel.SelectedLogs.Count, "SelectedLogs에는 5월 20일 로그 1개만 포함되어야 합니다.");
        Assert.IsTrue(viewModel.SelectedLogs.All(log => log.Timestamp >= date1 && log.Timestamp < date1.AddDays(1)),
                      "SelectedLogs의 모든 로그는 설정된 범위 내에 있어야 합니다.");
        Assert.AreEqual("1", viewModel.SelectedLogs.First().Id, "ID '1' 로그가 포함되어야 합니다.");
    }


    // 테스트 데이터 생성을 위한 도우미 메서드
    private void SeedDatabaseWithSampleLogs()
    {
        var date = new DateTime(2024, 5, 20);
        _context.Set<LogMySQL>().AddRange(
            new LogMySQL { Id = "i1", Timestamp = date.AddHours(1), LogLevel = "Info", Message = "Info 1" },
            new LogMySQL { Id = "i2", Timestamp = date.AddDays(1).AddHours(2), LogLevel = "Info", Message = "Info 2" },
            new LogMySQL { Id = "w1", Timestamp = date.AddHours(3), LogLevel = "Warning", Message = "Warning 1" },
            new LogMySQL { Id = "e1", Timestamp = date.AddDays(1).AddHours(4), LogLevel = "Error", Message = "Error 1" },
            new LogMySQL { Id = "e2", Timestamp = date.AddDays(2).AddHours(5), LogLevel = "Error", Message = "Error 2" }
        );
        _context.SaveChanges();
    }
}