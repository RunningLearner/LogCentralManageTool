using LogCentralManageTool.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Data;

/// <summary>
/// EF Core를 위한 MySQL 전용 로그 DbContext 클래스입니다.
/// </summary>
public class MySQLLoggingDbContext : DbContext, ILoggingDbContext
{
    public DbSet<LogMySQL> MySQLLogs { get; set; }

    // ILoggingDbContext 인터페이스 구현:
    // 여기서 변환 작업 등을 통해 ILog 타입(또는 공통 Log 타입)으로 노출할 수 있습니다.
    // 예시로 단순히 DbSet을 IQueryable<Log>로 캐스팅한다면:
    public IQueryable<ILog> Logs => MySQLLogs;

    /// <summary>
    /// DbContextOptions를 받는 생성자입니다.
    /// </summary>
    /// <param name="options">DbContext 옵션</param>
    public MySQLLoggingDbContext(DbContextOptions<MySQLLoggingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // MySQL 환경에 필요한 추가 Fluent API 매핑이 있다면 여기서 지정
    }
}