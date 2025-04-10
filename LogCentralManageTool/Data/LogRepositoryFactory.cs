using LogCentralManageTool.Data.Repositories;

using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace LogCentralManageTool.Data;

/// <summary>
/// 다양한 데이터베이스 제공자를 지원하는 LoggingDbContext 생성용 팩토리 클래스입니다.
/// 기본 제공자는 MongoDB로 설정되어 있습니다.
/// </summary>
public static class LogRepositoryFactory
{
    /// <summary>
    /// 지정한 ProviderType에 따라 LoggingDbContext를 생성합니다.
    /// 기본값은 MongoDB입니다.
    /// </summary>
    /// <param name="providerType">사용할 데이터베이스 제공자 (기본값: MongoDB)</param>
    /// <param name="connectionString">
    /// 연결 문자열. null 또는 빈 문자열일 경우 기본 연결 문자열이 사용됩니다.
    /// </param>
    /// <returns>구성된 LoggingDbContext 인스턴스</returns>
    public static ILogRepository GetRepository(string databaseName, ProviderType providerType, string connectionString = null)
    {
        // 제공자에 따라 옵션 설정
        switch (providerType)
        {
            case ProviderType.MongoDB:
                // 사용자로부터 입력받은 databaseName을 사용하여 MongoDB 연결 설정
                var mongoClient = new MongoClient(connectionString);
                var mongoDatabase = mongoClient.GetDatabase(databaseName);

                var mongoOptions = new DbContextOptionsBuilder<MongoLoggingDbContext>()
                    .UseMongoDB(mongoClient, databaseName)
                    .Options;
                return new MongoLogRepository(new MongoLoggingDbContext(mongoOptions));

            case ProviderType.MySQL:
                var mysqlOptions = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
                    .UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connectionString))
                    .Options;
                return new MySQLLogRepository(new MySQLLoggingDbContext(mysqlOptions));

            case ProviderType.InMemory:
                // In-Memory 데이터베이스 사용: 테스트나 임시 데이터 저장에 유용합니다.
                var inMemoryOptions = new DbContextOptionsBuilder<MySQLLoggingDbContext>()
                    .UseInMemoryDatabase(connectionString)
                    .Options;
                return new MySQLLogRepository(new MySQLLoggingDbContext(inMemoryOptions));

            // 필요시 추가
            //case ProviderType.SQLite:
            //    var sqliteOptions = new DbContextOptionsBuilder<LoggingDbContext>()
            //        .UseSqlite(connectionString)
            //        .Options;
            //    return new LoggingDbContext(sqliteOptions);

            default:
                throw new NotSupportedException("지정된 ProviderType은 지원되지 않습니다.");
        }
    }
}