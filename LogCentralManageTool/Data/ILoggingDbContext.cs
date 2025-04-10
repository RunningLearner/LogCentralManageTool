using LogCentralManageTool.Data.Entities;

namespace LogCentralManageTool.Data;
/// <summary>
/// 로그 데이터를 처리하는 DbContext의 공통 인터페이스입니다.
/// </summary>
public interface ILoggingDbContext
{
    /// <summary>
    /// 로그 엔티티에 대한 DbSet.
    /// </summary>
    IQueryable<ILog> Logs { get; }

    int SaveChanges();
}
