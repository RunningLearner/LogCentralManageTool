using LogCentralManageTool.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Data;

/// <summary>
/// EF Core를 위한 로그 전용 DbContext 클래스입니다.
/// LogEntry 엔티티에 대한 DbSet을 포함합니다.
/// </summary>
public class LoggingDbContext : DbContext
{
    /// <summary>
    /// 로그 엔티티에 대한 DbSet입니다.
    /// </summary>
    public DbSet<Log> Logs { get; set; }

    /// <summary>
    /// DbContextOptions를 받는 생성자입니다.
    /// </summary>
    /// <param name="options">DbContext 옵션</param>
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options)
        : base(options)
    {
    }
}