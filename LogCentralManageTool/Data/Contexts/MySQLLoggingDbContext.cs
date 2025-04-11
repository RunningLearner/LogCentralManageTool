using LogCentralManageTool.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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
        // 문자열을 정수로 변환하는 ValueConverter 생성
        var idConverter = new ValueConverter<string, int>(
            v => ConvertStringToInt(v),
            v => v.ToString());

        modelBuilder.Entity<LogMySQL>(entity =>
        {
            // Id 속성에 ValueConverter 적용
            entity.Property(e => e.Id)
                .HasConversion(idConverter)
                .ValueGeneratedOnAdd() // 자동 증가 키인 경우
                .HasColumnType("int"); // DB 컬럼은 int 타입으로 설정
        });

        base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// 문자열을 정수로 변환하는 메서드입니다.
    /// 변환에 실패하면 기본값인 0을 반환합니다.
    /// </summary>
    /// <param name="v">변환할 문자열</param>
    /// <returns>변환된 정수 값</returns>
    private static int ConvertStringToInt(string v)
    {
        return int.TryParse(v, out int intValue) ? intValue : 0;
    }
}