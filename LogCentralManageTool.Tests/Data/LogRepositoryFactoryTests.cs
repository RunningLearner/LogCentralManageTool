using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Repositories;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Tests.Data;

/// <summary>
/// LogRepositoryFactory 클래스의 동작을 검증하는 단위 테스트입니다.
/// 각 공급자별로 유효한 ILogRepository 인스턴스가 반환되는지 확인합니다.
/// </summary>
[TestFixture]
public class LogRepositoryFactoryTests
{
    /// <summary>
    /// 테스트 시나리오:
    /// MongoDB 공급자를 사용하여 지정한 유효한 연결 문자열로 ILogRepository를 생성할 경우,
    /// MongoLogRepository 인스턴스가 반환되는지 검증합니다.
    /// </summary>
    [Test]
    public void GetRepository_ShouldReturnMongoLogRepository_WithValidConnectionString()
    {
        // Arrange
        string databaseName = "TestDB";
        string connectionString = "mongodb://localhost:27017";

        // Act
        ILogRepository repository = LogRepositoryFactory.GetRepository(databaseName, ProviderType.MongoDB, connectionString);

        // Assert
        Assert.IsNotNull(repository, "MongoDB 공급자를 사용한 경우 반환된 ILogRepository 인스턴스는 null이 아니어야 합니다.");
        Assert.IsInstanceOf<MongoLogRepository>(repository, "반환된 repository는 MongoLogRepository 타입이어야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// InMemory 공급자를 사용하여 ILogRepository를 생성할 경우,
    /// MySQLLogRepository 인스턴스가 반환되는지 (InMemory 공급자로 처리함) 검증합니다.
    /// </summary>
    [Test]
    public void GetRepository_ShouldReturnInMemoryRepository_WithExplicitDatabaseName()
    {
        // Arrange
        string databaseName = "InMemoryTestDB";
        // InMemory 공급자의 경우 connectionString 매개변수가 데이터베이스 이름으로 사용됩니다.
        string connectionString = databaseName;

        // Act
        ILogRepository repository = LogRepositoryFactory.GetRepository(databaseName, ProviderType.InMemory, connectionString);

        // Assert
        Assert.IsNotNull(repository, "InMemory 공급자를 사용한 경우 반환된 ILogRepository 인스턴스는 null이 아니어야 합니다.");
        Assert.IsInstanceOf<MySQLLogRepository>(repository, "InMemory 공급자는 MySQLLogRepository로 반환되어야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// 지원되지 않는 ProviderType을 전달할 경우, NotSupportedException이 발생하는지 검증합니다.
    /// </summary>
    [Test]
    public void GetRepository_ShouldThrowNotSupportedException_ForInvalidProviderType()
    {
        // Arrange
        string databaseName = "TestDB";
        ProviderType invalidProviderType = (ProviderType)999;

        // Act & Assert
        Assert.Throws<NotSupportedException>(
            () => LogRepositoryFactory.GetRepository(databaseName, invalidProviderType),
            "지원되지 않는 ProviderType을 전달할 경우 NotSupportedException이 발생해야 합니다.");
    }
}