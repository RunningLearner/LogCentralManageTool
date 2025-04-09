using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Tests.ViewModels;

/// <summary>
/// DashBoardViewModel 생성자 동작(정상 처리, 빈 로그, 예외 처리, 생성자 인자 null) 을 검증하는 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class DashBoardViewModelTests
{
    /// <summary>
    /// 테스트 목적:
    /// 유효한 LoggingDbContext 인스턴스를 전달할 경우, 인메모리 데이터베이스에 저장된 로그 중 Timestamp가 가장 큰 로그가
    /// SelectedLog에 올바르게 할당되는지 검증합니다.
    /// 테스트 절차:
    /// 1. In‑Memory 데이터베이스 옵션을 사용하여 LoggingDbContext를 생성합니다.
    /// 2. 두 개 이상의 로그 엔티티를 추가한 후 저장합니다.
    /// 3. DashBoardViewModel 생성 시, 가장 최신 로그(가장 큰 Timestamp를 가진 로그)가 SelectedLog에 할당되는지 확인합니다.
    /// </summary>
    [Test]
    public void Constructor_AssignsLatestLog_WhenLogsExist()
    {
        // Arrange: 고유한 인메모리 데이터베이스 이름 사용
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDB_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new LoggingDbContext(options))
        {
            // 로그 엔티티 추가: log1과 log2 (log2가 더 최신)
            var log1 = new Log
            {
                Id = 1,
                Timestamp = new DateTime(2025, 1, 1),
                Message = "Old Log",
                LogLevel = "Info",
                StackTrace = "No StackTrace"
            };
            var log2 = new Log
            {
                Id = 2,
                Timestamp = new DateTime(2025, 2, 1),
                Message = "Latest Log",
                LogLevel = "Info",
                StackTrace = "No StackTrace"
            };
            context.Set<Log>().AddRange(log1, log2);
            context.SaveChanges();

            // Act: DashBoardViewModel 생성
            var viewModel = new DashBoardViewModel(context);

            // Assert: 가장 최신 로그(log2)가 SelectedLog에 할당되어야 함
            Assert.IsNotNull(viewModel.SelectedLog, "로그 데이터가 존재하면 SelectedLog는 null이 아니어야 합니다.");
            Assert.AreEqual(2, viewModel.SelectedLog.Id, "SelectedLog에는 가장 최신의 로그(로그2)가 할당되어야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// LoggingDbContext의 로그 데이터가 빈 경우, SelectedLog 속성이 null로 설정되는지 검증합니다.
    /// 테스트 절차:
    /// 1. In‑Memory 데이터베이스 옵션을 사용하여 LoggingDbContext를 생성하되, 로그 엔티티를 추가하지 않습니다.
    /// 2. DashBoardViewModel 생성 시, SelectedLog가 null인지 확인합니다.
    /// </summary>
    [Test]
    public void Constructor_SetsSelectedLogToNull_WhenNoLogsExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseInMemoryDatabase(databaseName: "EmptyDB_" + Guid.NewGuid().ToString())
            .Options;

        using (var context = new LoggingDbContext(options))
        {
            // 로그 엔티티 없이 컨텍스트 생성 및 저장 (저장할 데이터 없음)
            context.SaveChanges();

            // Act
            var viewModel = new DashBoardViewModel(context);

            // Assert: 로그가 없으므로 SelectedLog는 null이어야 합니다.
            Assert.IsNull(viewModel.SelectedLog, "로그 엔티티가 없으면 SelectedLog는 null이어야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// LoggingDbContext 사용 중 예외가 발생할 경우, DashBoardViewModel 생성자에서 예외를 내부적으로 처리하여
    /// SelectedLog가 null로 남는지, 그리고 생성자가 예외를 전파하지 않는지 검증합니다.
    /// 테스트 절차:
    /// 1. In‑Memory 데이터베이스 옵션을 사용하여 LoggingDbContext를 생성한 후, 명시적으로 Dispose하여 컨텍스트를 종료시킵니다.
    /// 2. 종료된(Dispose된) 컨텍스트를 DashBoardViewModel 생성자에 전달하여 예외 상황을 재현합니다.
    /// 3. 생성자 호출 시 예외가 전파되지 않고, SelectedLog가 null인지 확인합니다.
    /// </summary>
    [Test]
    public void Constructor_CatchesExceptions_AndLeavesSelectedLogNull()
    {
        // Arrange: 인메모리 데이터베이스 옵션 사용, 컨텍스트를 생성한 후 Dispose
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseInMemoryDatabase(databaseName: "ExceptionDB_" + Guid.NewGuid().ToString())
            .Options;

        var context = new LoggingDbContext(options);
        // SaveChanges 호출 없이 바로 Dispose하여, 이후 컨텍스트 사용 시 ObjectDisposedException 발생 유도
        context.Dispose();

        // Act & Assert: Dispose된 컨텍스트를 전달하여 생성자 호출 시 예외가 전파되지 않고, SelectedLog가 null이어야 함
        DashBoardViewModel viewModel = null;
        Assert.DoesNotThrow(() =>
        {
            viewModel = new DashBoardViewModel(context);
        }, "생성자 내부에서 발생한 예외가 전파되어서는 안 됩니다.");

        Assert.IsNull(viewModel.SelectedLog, "예외 발생 시 SelectedLog는 null로 남아야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// DashBoardViewModel 생성자에 null을 전달할 경우 ArgumentNullException이 발생하는지 검증합니다.
    /// 테스트 절차:
    /// 1. 생성자 호출 시 null을 전달합니다.
    /// 2. ArgumentNullException 예외가 발생하는지 Assert.Throws를 통해 확인합니다.
    /// </summary>
    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
    {
        // Act & Assert: null 전달 시 ArgumentNullException이 발생해야 합니다.
        Assert.Throws<ArgumentNullException>(() => new DashBoardViewModel(null));
    }
}