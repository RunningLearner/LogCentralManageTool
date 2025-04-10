using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.Data.Repositories;
using LogCentralManageTool.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Tests.ViewModels;

/// <summary>
/// DashBoardViewModel 생성자와 공개 인터페이스(토글 커맨드)의 동작을 검증하는 단위 테스트 클래스입니다.
/// </summary>
[TestFixture]
public class DashBoardViewModelTests
{
    /// <summary>
    /// 유효한 MySQLLoggingDbContext 인스턴스를 통한 ILogRepository를 전달할 경우,
    /// 인메모리 데이터베이스에 저장된 로그 중 Timestamp가 가장 큰 로그가 SelectedLog에 올바르게 할당되는지 검증합니다.
    /// </summary>
    [Test]
    public void Constructor_AssignsLatestLog_WhenLogsExist()
    {
        // Arrange: 고유한 인메모리 데이터베이스 이름 사용
        var options = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new MySQLLoggingDbContext(options))
        {
            // 두 개 이상의 로그 엔티티 추가 (log2가 더 최신)
            var log1 = new LogMySQL
            {
                Id = 1,
                Timestamp = new DateTime(2025, 1, 1),
                Message = "Old Log",
                LogLevel = "Info",
                StackTrace = "No StackTrace"
            };
            var log2 = new LogMySQL
            {
                Id = 2,
                Timestamp = new DateTime(2025, 2, 1),
                Message = "Latest Log",
                LogLevel = "Info",
                StackTrace = "No StackTrace"
            };

            context.Set<LogMySQL>().AddRange(log1, log2);
            context.SaveChanges();

            // ILogRepository 생성 (구현체는 MySQLLogRepository)
            ILogRepository repository = new MySQLLogRepository(context);

            // Act: DashBoardViewModel 생성
            var viewModel = new DashBoardViewModel(repository);

            // Assert: 가장 최신 로그(log2)가 SelectedLog에 할당되어야 함
            Assert.IsNotNull(viewModel.SelectedLog, "로그 데이터가 존재하면 SelectedLog는 null이 아니어야 합니다.");
            // ILog 인터페이스를 구현한 객체이므로 LogMySQL로 캐스팅합니다.
            var selected = viewModel.SelectedLog as LogMySQL;
            Assert.IsNotNull(selected, "SelectedLog는 LogMySQL 타입이어야 합니다.");
            Assert.AreEqual(2, selected.Id, "SelectedLog에는 가장 최신의 로그(log2)가 할당되어야 합니다.");
        }
    }

    /// <summary>
    /// MySQLLoggingDbContext에 로그 데이터가 없는 경우, SelectedLog가 null로 설정되는지 검증합니다.
    /// </summary>
    [Test]
    public void Constructor_SetsSelectedLogToNull_WhenNoLogsExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
            .UseInMemoryDatabase(databaseName: "EmptyDB_" + Guid.NewGuid().ToString())
            .Options;
        using (var context = new MySQLLoggingDbContext(options))
        {
            context.SaveChanges();
            ILogRepository repository = new MySQLLogRepository(context);

            // Act
            var viewModel = new DashBoardViewModel(repository);

            // Assert
            Assert.IsNull(viewModel.SelectedLog, "로그 엔티티가 없으면 SelectedLog는 null이어야 합니다.");
        }
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
    /// ToggleSeriesCommand를 실행하면, 해당 로그 레벨의 시리즈 표시 여부가 토글되는지 검증합니다.
    /// 테스트에서는 공개된 ToggleSeriesCommand를 통해 동작을 확인합니다.
    /// </summary>
    [Test]
    public void ToggleSeriesCommand_TogglesVisibility_ForGivenLevel()
    {
        // Arrange: 인메모리 데이터베이스에 여러 로그 데이터를 추가 (로그 레벨: Info, Warning, Error)
        var options = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
            .UseInMemoryDatabase(databaseName: "ToggleDB_" + Guid.NewGuid().ToString())
            .Options;
        using (var context = new MySQLLoggingDbContext(options))
        {
            var logInfo = new LogMySQL
            {
                Id = 1,
                Timestamp = new DateTime(2025, 1, 1),
                Message = "Info Log",
                LogLevel = "Info",
                StackTrace = string.Empty
            };
            var logWarning = new LogMySQL
            {
                Id = 2,
                Timestamp = new DateTime(2025, 1, 1),
                Message = "Warning Log",
                LogLevel = "Warning",
                StackTrace = string.Empty
            };
            var logError = new LogMySQL
            {
                Id = 3,
                Timestamp = new DateTime(2025, 1, 1),
                Message = "Error Log",
                LogLevel = "Error",
                StackTrace = string.Empty
            };
            context.Set<LogMySQL>().AddRange(logInfo, logWarning, logError);
            context.SaveChanges();

            ILogRepository repository = new MySQLLogRepository(context);
            var viewModel = new DashBoardViewModel(repository);

            // 기본적으로 모든 시리즈는 표시(true)되어 있다고 가정합니다.
            Assert.IsTrue(viewModel.IsInfoVisible, "초기 Info 시리즈는 표시되어야 합니다.");
            Assert.IsTrue(viewModel.IsWarningVisible, "초기 Warning 시리즈는 표시되어야 합니다.");
            Assert.IsTrue(viewModel.IsErrorVisible, "초기 Error 시리즈는 표시되어야 합니다.");

            // Act: ToggleSeriesCommand를 실행하여 "Warning" 로그의 표시를 토글 (표시 여부 false로 전환)
            viewModel.ToggleSeriesCommand.Execute("Warning");

            // Assert: "Warning" 로그 시리즈의 표시 여부가 false로 변경되어야 함
            Assert.IsFalse(viewModel.IsWarningVisible, "ToggleSeriesCommand 실행 후 Warning 시리즈의 IsVisible은 false여야 합니다.");

            // Act: 다시 ToggleSeriesCommand 실행하여 "Warning" 로그의 표시를 true로 전환
            viewModel.ToggleSeriesCommand.Execute("Warning");

            // Assert: "Warning" 로그 시리즈의 표시 여부가 true로 변경되어야 함
            Assert.IsTrue(viewModel.IsWarningVisible, "ToggleSeriesCommand를 다시 실행하면 Warning 시리즈의 IsVisible은 true여야 합니다.");
        }
    }
}