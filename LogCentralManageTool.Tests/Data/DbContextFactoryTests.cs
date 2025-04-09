using LogCentralManageTool.Data;

using Microsoft.EntityFrameworkCore;

namespace LogCentralManageTool.Tests.Data;

/// <summary>
/// DbContextFactory 클래스의 동작을 검증하는 단위 테스트를 수행합니다.
/// 이 테스트는 운영 환경에서 실제 연결 문자열이 고정되어 있지 않은 점을 고려하여,
/// 각 공급자별로 유효한 LoggingDbContext 인스턴스가 반환되는지 검증합니다.
/// </summary>
[TestFixture]
public class DbContextFactoryTests
{
    /// <summary>
    /// 테스트 시나리오:
    /// MongoDB 제공자를 사용하여 지정한 유효한 연결 문자열로 DbContext를 생성할 경우,
    /// 올바른 LoggingDbContext 인스턴스가 반환되는지 검증합니다.
    /// </summary>
    [Test]
    public void GetContext_ShouldReturnMongoDbContext_WithValidConnectionString()
    {
        // Arrange
        string databaseName = "TestDB";
        string connectionString = "mongodb://localhost:27017";

        // Act
        var context = DbContextFactory.GetContext(databaseName, ProviderType.MongoDB, connectionString);

        // Assert
        Assert.IsNotNull(context, "MongoDB 제공자를 사용한 경우 반환된 LoggingDbContext 인스턴스는 null이 아니어야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// InMemory 제공자를 사용하여 DbContext를 생성할 경우,
    /// 전달된 connectionString 매개변수가 데이터베이스 이름으로 사용되어 InMemory 데이터베이스가 올바르게 생성되는지 확인합니다.
    /// InMemory 공급자의 경우, DbContext.Database.GetDbConnection()은 사용할 수 없으므로 대신 ProviderName을 검증합니다.
    /// </summary>
    [Test]
    public void GetContext_ShouldReturnInMemoryContext_WithExplicitDatabaseName()
    {
        // Arrange
        string databaseName = "InMemoryTestDB";
        // InMemory 공급자에서는 connectionString 매개변수가 데이터베이스 이름으로 사용됩니다.
        string connectionString = databaseName;

        // Act
        var context = DbContextFactory.GetContext(databaseName, ProviderType.InMemory, connectionString);

        // Assert
        Assert.IsNotNull(context, "InMemory 제공자를 사용한 경우 반환된 LoggingDbContext 인스턴스는 null이 아니어야 합니다.");
        // InMemory 공급자의 경우 DbConnection을 사용할 수 없으므로, 대신 ProviderName을 확인합니다.
        Assert.AreEqual("Microsoft.EntityFrameworkCore.InMemory", context.Database.ProviderName,
            "InMemory 공급자를 사용해야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// 지원되지 않는 ProviderType을 전달할 경우, NotSupportedException이 발생하는지 검증합니다.
    /// </summary>
    [Test]
    public void GetContext_ShouldThrowNotSupportedException_ForInvalidProviderType()
    {
        // Arrange
        string databaseName = "TestDB";
        ProviderType invalidProviderType = (ProviderType)999;

        // Act & Assert
        Assert.Throws<NotSupportedException>(
            () => DbContextFactory.GetContext(databaseName, invalidProviderType),
            "지원되지 않는 ProviderType을 전달할 경우 NotSupportedException이 발생해야 합니다.");
    }
}