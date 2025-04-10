using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Tests.Data;

/// <summary>
/// LogRepository의 핵심 기능(로그 조회)을 검증하는 단위 테스트를 수행합니다.
/// In-Memory 데이터베이스를 활용하여 테스트가 서로 독립적으로 수행되도록 구성합니다.
/// </summary>
[TestFixture]
public class LogRepositoryTests
{
    /// <summary>
    /// 테스트 목적:
    /// 데이터베이스에 로그 항목이 없는 경우, GetLatestLog 메서드가 null을 반환하는지 검증합니다.
    /// 테스트 절차:
    /// 1. In-Memory 데이터베이스를 사용하여 빈 LoggingDbContext 인스턴스를 생성합니다.
    /// 2. LogRepository를 초기화한 후, GetLatestLog 메서드를 호출합니다.
    /// 3. 반환 결과가 null인지 확인합니다.
    /// </summary>
    [Test]
    public void GetLatestLog_ReturnsNull_WhenNoLogsExist()
    {
        // Arrange: 고유한 In-Memory 데이터베이스 이름 생성
        var options = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
                          .UseInMemoryDatabase(databaseName: "NoLogsDB_" + Guid.NewGuid().ToString())
                          .Options;

        using (var context = new MySQLLoggingDbContext(options))
        {
            var repository = new LogRepository(context);

            // Act
            var result = repository.GetLatestLog();

            // Assert
            Assert.IsNull(result, "데이터베이스에 로그 항목이 없으면 GetLatestLog는 null을 반환해야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// 하나의 로그 항목이 존재할 때, GetLatestLog 메서드가 해당 로그 항목을 올바르게 반환하는지 검증합니다.
    /// 테스트 절차:
    /// 1. In-Memory 데이터베이스에 단일 로그 항목을 추가합니다.
    /// 2. LoggingDbContext와 LogRepository 인스턴스를 생성합니다.
    /// 3. GetLatestLog 메서드를 호출하고, 반환된 로그 항목이 추가한 항목과 동일한지 확인합니다.
    /// </summary>
    [Test]
    public void GetLatestLog_ReturnsLog_WhenSingleLogExists()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
                          .UseInMemoryDatabase(databaseName: "SingleLogDB_" + Guid.NewGuid().ToString())
                          .Options;

        using (var context = new MySQLLoggingDbContext(options))
        {
            var log = new LogMySQL
            {
                Id = 1,
                Timestamp = DateTime.Now,
                LogLevel = "Info",
                Message = "유일한 로그 항목"
            };

            context.Logs.Add(log);
            context.SaveChanges();

            var repository = new LogRepository(context);

            // Act
            var result = repository.GetLatestLog();

            // Assert
            Assert.IsNotNull(result, "하나의 로그 항목이 있을 때, GetLatestLog는 해당 항목을 반환해야 합니다.");
            Assert.AreEqual(log.Id, result.Id, "반환된 로그 항목의 ID가 예상한 값과 동일해야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// 여러 로그 항목이 존재할 때, GetLatestLog 메서드가 Timestamp가 가장 최신인 로그 항목을 반환하는지 검증합니다.
    /// 테스트 절차:
    /// 1. In-Memory 데이터베이스에 서로 다른 Timestamp 값을 가진 여러 로그 항목을 추가합니다.
    /// 2. LoggingDbContext와 LogRepository 인스턴스를 생성합니다.
    /// 3. GetLatestLog 메서드를 호출하고, 가장 최신(Timestamp 최대)의 로그 항목이 반환되는지 확인합니다.
    /// </summary>
    [Test]
    public void GetLatestLog_ReturnsLatestLog_WhenMultipleLogsExist()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
                          .UseInMemoryDatabase(databaseName: "MultipleLogsDB_" + Guid.NewGuid().ToString())
                          .Options;

        using (var context = new MySQLLoggingDbContext(options))
        {
            var log1 = new LogMySQL { Id = 1, Timestamp = new DateTime(2025, 1, 1), LogLevel = "Info", Message = "로그 1" };
            var log2 = new LogMySQL { Id = 2, Timestamp = new DateTime(2025, 2, 1), LogLevel = "Warning", Message = "로그 2" };
            var log3 = new LogMySQL { Id = 3, Timestamp = new DateTime(2025, 3, 1), LogLevel = "Error", Message = "로그 3" };

            context.Logs.AddRange(log1, log2, log3);
            context.SaveChanges();

            var repository = new LogRepository(context);

            // Act
            var result = repository.GetLatestLog();

            // Assert
            Assert.IsNotNull(result, "로그 항목이 존재하는 경우 GetLatestLog는 null이 아니어야 합니다.");
            Assert.AreEqual(log3.Id, result.Id, "가장 최신의 로그 항목(로그 3)이 반환되어야 합니다.");
        }
    }
}
