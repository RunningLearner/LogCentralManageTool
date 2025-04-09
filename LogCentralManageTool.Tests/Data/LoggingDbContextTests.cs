using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Tests.Data;

/// <summary>
/// LoggingDbContext의 In-Memory 데이터베이스를 활용한 기본 동작(삽입, 조회, 수정, 삭제)을 검증하는 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class LoggingDbContextTests
{
    /// <summary>
    /// 테스트 목적:
    /// In-Memory 데이터베이스를 사용하여 로그 항목을 삽입한 후, 해당 항목이 데이터베이스에 정상적으로 저장되었는지 확인합니다.
    /// 테스트 절차:
    /// 1. In-Memory 데이터베이스 옵션으로 LoggingDbContext 인스턴스를 생성합니다.
    /// 2. 로그 항목을 추가하고 SaveChanges를 호출합니다.
    /// 3. 새로운 컨텍스트를 통해 추가된 로그 항목을 조회하여 검증합니다.
    /// </summary>
    [Test]
    public void InsertLog_ShouldAddLogToDatabase()
    {
        // Arrange: In-Memory 데이터베이스 옵션 생성 (데이터베이스 이름을 고유하게 지정)
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
                      .UseInMemoryDatabase(databaseName: "InsertLogTestDB")
                      .Options;

        var newLog = new Log
        {
            Id = 1,
            Timestamp = DateTime.Now,
            LogLevel = "Info",
            Message = "Test Insert",
            StackTrace = null
        };

        // Act: 로그 항목 삽입 후 저장
        using (var context = new LoggingDbContext(options))
        {
            context.Logs.Add(newLog);
            context.SaveChanges();
        }

        // Assert: 새로운 컨텍스트를 사용하여 삽입된 로그 항목을 조회 및 검증
        using (var context = new LoggingDbContext(options))
        {
            var log = context.Logs.Find(1);
            Assert.IsNotNull(log, "삽입된 로그 항목을 데이터베이스에서 찾아야 합니다.");
            Assert.AreEqual("Test Insert", log.Message, "로그 메시지가 올바르게 저장되어야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// In-Memory 데이터베이스에 저장된 로그 항목을 특정 조건으로 조회할 때, 올바른 데이터를 반환하는지 검증합니다.
    /// 테스트 절차:
    /// 1. In-Memory 데이터베이스에 미리 로그 항목을 삽입합니다.
    /// 2. 조건에 맞는 로그 항목을 LINQ 쿼리로 조회합니다.
    /// 3. 조회 결과가 기대한 값과 일치하는지 확인합니다.
    /// </summary>
    [Test]
    public void QueryLog_ShouldRetrieveCorrectLogEntry()
    {
        // Arrange: In-Memory 데이터베이스 옵션 생성
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
                      .UseInMemoryDatabase(databaseName: "QueryLogTestDB")
                      .Options;

        // 미리 로그 항목 삽입
        using (var context = new LoggingDbContext(options))
        {
            context.Logs.Add(new Log
            {
                Id = 1,
                Timestamp = DateTime.Now,
                LogLevel = "Error",
                Message = "Test Query",
                StackTrace = "StackTrace"
            });
            context.SaveChanges();
        }

        // Act & Assert: LINQ 쿼리를 통해 로그 항목 조회 후 결과 검증
        using (var context = new LoggingDbContext(options))
        {
            var logEntry = context.Logs.FirstOrDefault(l => l.LogLevel == "Error");
            Assert.IsNotNull(logEntry, "쿼리 결과, 로그 항목을 찾아야 합니다.");
            Assert.AreEqual("Test Query", logEntry.Message, "조회된 로그 메시지가 기대한 값과 일치해야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// In-Memory 데이터베이스에 삽입된 로그 항목의 데이터를 업데이트한 후, 변경사항이 데이터베이스에 정상적으로 반영되었는지 검증합니다.
    /// 테스트 절차:
    /// 1. In-Memory 데이터베이스에 로그 항목을 삽입합니다.
    /// 2. 해당 로그 항목을 조회하여 메시지 값을 수정한 후 SaveChanges를 호출합니다.
    /// 3. 새로운 컨텍스트를 사용하여 수정된 값이 저장되었는지 확인합니다.
    /// </summary>
    [Test]
    public void UpdateLog_ShouldModifyLogEntry()
    {
        // Arrange: In-Memory 데이터베이스 옵션 생성
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
                      .UseInMemoryDatabase(databaseName: "UpdateLogTestDB")
                      .Options;

        // 초기 로그 항목 삽입
        using (var context = new LoggingDbContext(options))
        {
            context.Logs.Add(new Log
            {
                Id = 1,
                Timestamp = DateTime.Now,
                LogLevel = "Warning",
                Message = "Old Message",
                StackTrace = null
            });
            context.SaveChanges();
        }

        // Act: 로그 항목 수정 및 저장
        using (var context = new LoggingDbContext(options))
        {
            var logEntry = context.Logs.Find(1);
            Assert.IsNotNull(logEntry, "업데이트할 로그 항목이 존재해야 합니다.");
            logEntry.Message = "Updated Message";
            context.SaveChanges();
        }

        // Assert: 수정된 로그 항목 조회 및 검증
        using (var context = new LoggingDbContext(options))
        {
            var logEntry = context.Logs.Find(1);
            Assert.AreEqual("Updated Message", logEntry.Message, "업데이트된 로그 메시지가 데이터베이스에 반영되어야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// In-Memory 데이터베이스에 삽입된 로그 항목을 삭제한 후, 해당 항목이 데이터베이스에서 제거되었는지 확인합니다.
    /// 테스트 절차:
    /// 1. In-Memory 데이터베이스에 로그 항목을 삽입합니다.
    /// 2. 해당 로그 항목을 삭제한 후 SaveChanges를 호출합니다.
    /// 3. 새로운 컨텍스트를 통해 삭제된 항목이 조회되지 않는지 검증합니다.
    /// </summary>
    [Test]
    public void DeleteLog_ShouldRemoveLogEntry()
    {
        // Arrange: In-Memory 데이터베이스 옵션 생성
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
                      .UseInMemoryDatabase(databaseName: "DeleteLogTestDB")
                      .Options;

        // 테스트용 로그 항목 삽입
        using (var context = new LoggingDbContext(options))
        {
            context.Logs.Add(new Log
            {
                Id = 1,
                Timestamp = DateTime.Now,
                LogLevel = "Info",
                Message = "Test Delete",
                StackTrace = null
            });
            context.SaveChanges();
        }

        // Act: 로그 항목 삭제 처리
        using (var context = new LoggingDbContext(options))
        {
            var logEntry = context.Logs.Find(1);
            context.Logs.Remove(logEntry);
            context.SaveChanges();
        }

        // Assert: 삭제된 로그 항목이 데이터베이스에 존재하지 않는지 확인
        using (var context = new LoggingDbContext(options))
        {
            var logEntry = context.Logs.Find(1);
            Assert.IsNull(logEntry, "삭제된 로그 항목은 데이터베이스에 존재해서는 안 됩니다.");
        }
    }
}