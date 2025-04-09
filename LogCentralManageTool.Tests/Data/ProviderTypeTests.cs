using LogCentralManageTool.Data;

namespace LogCentralManageTool.Tests.Data;

/// <summary>
/// ProviderType 열거형 및 관련 동작을 검증하는 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class ProviderTypeTests
{
    /// <summary>
    /// 테스트 목적:
    /// ProviderType 열거형에 정의된 멤버의 개수가 올바른지 검증합니다.
    /// 검증 포인트:
    /// 열거형에 InMemory, MongoDB, MySQL, SQLite 총 4개의 멤버가 존재하는지 확인합니다.
    /// </summary>
    [Test]
    public void ProviderType_ShouldHaveThreeMembers()
    {
        // Act: 열거형에 정의된 이름들을 가져옵니다.
        var memberNames = Enum.GetNames(typeof(ProviderType));

        // Assert: 총 4개의 멤버가 있어야 합니다.
        Assert.AreEqual(4, memberNames.Length, "ProviderType 열거형은 총 4개의 멤버를 가져야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// ProviderType 열거형의 각 멤버가 예상한 정수 값과 동일한지 검증합니다.
    /// 검증 포인트:
    /// InMemory=0, MongoDB=1, MySQL=2, SQLite=3 인지 확인합니다.
    /// </summary>
    [Test]
    public void ProviderType_MemberValues_ShouldBeCorrect()
    {
        // Arrange & Act
        int inMemoryValue = (int)ProviderType.InMemory;
        int mongoValue = (int)ProviderType.MongoDB;
        int mysqlValue = (int)ProviderType.MySQL;
        int sqliteValue = (int)ProviderType.SQLite;

        // Assert: 각 멤버의 정수 값이 예상대로 지정되었는지 확인합니다.
        Assert.AreEqual(0, inMemoryValue, "InMemory의 값은 0이어야 합니다.");
        Assert.AreEqual(1, mongoValue, "MongoDB의 값은 1이어야 합니다.");
        Assert.AreEqual(2, mysqlValue, "MySQL의 값은 2이어야 합니다.");
        Assert.AreEqual(3, sqliteValue, "SQLite의 값은 3이어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// ProviderType 열거형의 ToString() 메서드가 올바른 문자열 값을 반환하는지 검증합니다.
    /// 검증 포인트:
    /// 각 멤버의 ToString() 결과가 해당 멤버명("MongoDB", "MySQL", "SQLite")과 동일한지 확인합니다.
    /// </summary>
    [Test]
    public void ProviderType_ToString_ShouldReturnMemberName()
    {
        // Act
        string mongoString = ProviderType.MongoDB.ToString();
        string mysqlString = ProviderType.MySQL.ToString();
        string sqliteString = ProviderType.SQLite.ToString();

        // Assert: 각 멤버의 ToString 결과가 올바른지 확인합니다.
        Assert.AreEqual("MongoDB", mongoString, "MongoDB의 ToString 결과는 'MongoDB'이어야 합니다.");
        Assert.AreEqual("MySQL", mysqlString, "MySQL의 ToString 결과는 'MySQL'이어야 합니다.");
        Assert.AreEqual("SQLite", sqliteString, "SQLite의 ToString 결과는 'SQLite'이어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// 문자열을 ProviderType 열거형으로 파싱할 때 올바른 열거형 값이 반환되는지 검증합니다.
    /// 검증 포인트:
    /// 문자열 "MySQL"을 파싱하면 ProviderType.MySQL이 반환되는지 확인합니다.
    /// </summary>
    [Test]
    public void ProviderType_Parse_ShouldReturnCorrectEnum()
    {
        // Act: 문자열 "MySQL"을 ProviderType 열거형으로 파싱
        var parsedProvider = (ProviderType)Enum.Parse(typeof(ProviderType), "MySQL");

        // Assert: 파싱 결과가 예상한 열거형 값과 동일한지 확인합니다.
        Assert.AreEqual(ProviderType.MySQL, parsedProvider, "문자열 'MySQL'의 파싱 결과는 ProviderType.MySQL이어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// ProviderType 열거형에 정의되지 않은 값을 검사할 때, IsDefined 메서드가 올바른 결과(false)를 반환하는지 검증합니다.
    /// 검증 포인트:
    /// 정수 999가 열거형에 정의되어 있지 않으므로, Enum.IsDefined가 false를 반환해야 합니다.
    /// </summary>
    [Test]
    public void ProviderType_IsDefined_ShouldReturnFalse_ForInvalidValue()
    {
        // Act: 열거형에 999가 정의되어 있는지 확인
        bool isDefined = Enum.IsDefined(typeof(ProviderType), 999);

        // Assert: 정의되지 않은 값이므로 false여야 합니다.
        Assert.IsFalse(isDefined, "999는 ProviderType 열거형에 정의되지 않았으므로 IsDefined는 false를 반환해야 합니다.");
    }
}