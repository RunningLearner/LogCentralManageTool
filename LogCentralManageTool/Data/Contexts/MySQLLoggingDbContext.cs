using LogCentralManageTool.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Data;

/// <summary>
/// EF Core를 위한 MySQL 전용 로그 DbContext 클래스입니다.
/// </summary>
public class MySQLLoggingDbContext : DbContext
{
    public DbSet<LogMySQL> Logs { get; set; }

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