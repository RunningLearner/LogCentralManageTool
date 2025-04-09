using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace LogCentralManageTool.Data;

/// <summary>
/// 다양한 데이터베이스 제공자를 지원하는 LoggingDbContext 생성용 팩토리 클래스입니다.
/// 기본 제공자는 MongoDB로 설정되어 있습니다.
/// </summary>
public static class DbContextFactory
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
    public static LoggingDbContext GetContext(string databaseName, ProviderType providerType = ProviderType.MongoDB, string connectionString = null)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LoggingDbContext>();

        // 기본 연결 문자열 설정 (프로젝트 환경에 맞게 수정)
        if (string.IsNullOrEmpty(connectionString))
        {
            switch (providerType)
            {
                case ProviderType.MongoDB:
                    connectionString = "mongodb://localhost:27017";
                    break;
                case ProviderType.MySQL:
                    connectionString = "server=localhost;port=3306;database=" + databaseName + ";user=root;password=yourpassword";
                    break;
                case ProviderType.SQLite:
                    connectionString = "Data Source=" + databaseName + ".sqlite";
                    break;
                case ProviderType.InMemory:
                    // In-Memory DB의 경우 연결 문자열은 사용하지 않으며, 데이터베이스 이름을 식별자로 사용
                    connectionString = databaseName;
                    break;
            }
        }

        // 제공자에 따라 옵션 설정
        switch (providerType)
        {
            case ProviderType.MongoDB:
                // 사용자로부터 입력받은 databaseName을 사용하여 MongoDB 연결 설정
                var mongoClient = new MongoClient(connectionString);
                var mongoDatabase = mongoClient.GetDatabase(databaseName);

                var mongoOptions = new DbContextOptionsBuilder<LoggingDbContext>()
                    .UseMongoDB(mongoDatabase.Client, mongoDatabase.DatabaseNamespace.DatabaseName)
                    .Options;
                return new LoggingDbContext(mongoOptions);

            case ProviderType.MySQL:
                var mysqlOptions = new DbContextOptionsBuilder<LoggingDbContext>()
                    .UseMySql(connectionString, Microsoft.EntityFrameworkCore.ServerVersion.AutoDetect(connectionString))
                    .Options;
                return new LoggingDbContext(mysqlOptions);

            case ProviderType.InMemory:
                // In-Memory 데이터베이스 사용: 테스트나 임시 데이터 저장에 유용합니다.
                var inMemoryOptions = new DbContextOptionsBuilder<LoggingDbContext>()
                    .UseInMemoryDatabase(connectionString)
                    .Options;
                return new LoggingDbContext(inMemoryOptions);

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