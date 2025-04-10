﻿using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;
using LogCentralManageTool.ViewModels;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Tests.ViewModels;

/// <summary>
/// DashBoardViewModel 생성자 및 공개 인터페이스(토글 커맨드)의 동작을 검증하는 단위 테스트 클래스입니다.
/// </summary>
[TestFixture]
public class DashBoardViewModelTests
{
    /// <summary>
    /// 유효한 LoggingDbContext 인스턴스를 전달할 경우, 인메모리 데이터베이스에 저장된 로그 중 Timestamp가 가장 큰 로그가
    /// SelectedLog에 올바르게 할당되는지 검증합니다.
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
            // 두 개 이상의 로그 엔티티 추가 (log2가 더 최신)
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
    /// LoggingDbContext에 로그 데이터가 없는 경우, SelectedLog가 null로 설정되는지 검증합니다.
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
            context.SaveChanges();

            // Act
            var viewModel = new DashBoardViewModel(context);

            // Assert
            Assert.IsNull(viewModel.SelectedLog, "로그 엔티티가 없으면 SelectedLog는 null이어야 합니다.");
        }
    }

    /// <summary>
    /// DashBoardViewModel 생성자에 null을 전달할 경우, ArgumentNullException이 발생하는지 검증합니다.
    /// </summary>
    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenContextIsNull()
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
        var options = new DbContextOptionsBuilder<LoggingDbContext>()
            .UseInMemoryDatabase(databaseName: "ToggleDB_" + Guid.NewGuid().ToString())
            .Options;
        using (var context = new LoggingDbContext(options))
        {
            var logInfo = new Log { Id = 1, Timestamp = new DateTime(2025, 1, 1), Message = "Info Log", LogLevel = "Info", StackTrace = "" };
            var logWarning = new Log { Id = 2, Timestamp = new DateTime(2025, 1, 1), Message = "Warning Log", LogLevel = "Warning", StackTrace = "" };
            var logError = new Log { Id = 3, Timestamp = new DateTime(2025, 1, 1), Message = "Error Log", LogLevel = "Error", StackTrace = "" };
            context.Set<Log>().AddRange(logInfo, logWarning, logError);
            context.SaveChanges();

            var viewModel = new DashBoardViewModel(context);

            // 기본적으로 모든 시리즈는 표시(true)되어 있다고 가정합니다.
            Assert.IsTrue(viewModel.IsInfoVisible);
            Assert.IsTrue(viewModel.IsWarningVisible);
            Assert.IsTrue(viewModel.IsErrorVisible);

            // Act: ToggleSeriesCommand를 실행하여 "Warning" 로그의 표시를 토글 (표시 여부 false로 전환)
            viewModel.ToggleSeriesCommand.Execute("Warning");

            // Assert: "Warning" 로그 시리즈의 표시 여부가 false로 변경되어야 함
            Assert.IsFalse(viewModel.IsWarningVisible, "ToggleSeriesCommand 실행 후 Warning 시리즈의 IsVisible이 false여야 합니다.");

            // Act: 다시 ToggleSeriesCommand 실행하여 "Warning" 로그의 표시를 다시 true로 전환
            viewModel.ToggleSeriesCommand.Execute("Warning");

            // Assert: "Warning" 로그 시리즈의 표시 여부가 true로 변경되어야 함
            Assert.IsTrue(viewModel.IsWarningVisible, "ToggleSeriesCommand를 다시 실행하면 Warning 시리즈의 IsVisible이 true여야 합니다.");
        }
    }
}