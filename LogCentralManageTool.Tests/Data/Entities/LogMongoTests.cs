using LogCentralManageTool.Data.Entities;

namespace LogCentralManageTool.Tests.Data.Entities;
/// <summary>
/// LogMongo 클래스에 대한 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class LogMongoTests
{
    #region 생성자 및 기본값 테스트

    /// <summary>
    /// 기본 생성자 테스트: LogMongo 객체의 모든 속성이 기본값으로 초기화되는지 확인합니다.
    /// </summary>
    [Test]
    public void DefaultConstructor_ShouldInitializeProperties()
    {
        // Arrange & Act: 기본 생성자를 통해 LogMongo 객체 생성
        LogMongo log = new LogMongo();

        // Assert: 각 속성이 기본값으로 초기화되었는지 확인합니다.
        // Id는 string이므로 기본값은 null입니다.
        Assert.IsNull(log.Id, "기본 Id 값은 null이어야 합니다.");
        // DateTime의 기본값은 default(DateTime)입니다.
        Assert.AreEqual(default(DateTime), log.Timestamp, "기본 Timestamp 값은 default(DateTime)이어야 합니다.");
        // LogLevel, Message는 기본값 null
        Assert.IsNull(log.LogLevel, "기본 LogLevel 값은 null이어야 합니다.");
        Assert.IsNull(log.Message, "기본 Message 값은 null이어야 합니다.");
        // StackTrace는 Nullable이므로 null이어야 합니다.
        Assert.IsNull(log.StackTrace, "기본 StackTrace 값은 null이어야 합니다.");
    }

    #endregion

    #region 속성 초기화 테스트

    /// <summary>
    /// 속성 초기화 테스트: LogMongo 객체 생성 후 각 속성에 할당한 값들이 올바르게 저장되는지 확인합니다.
    /// </summary>
    [Test]
    public void PropertyInitialization_ShouldStoreCorrectValues()
    {
        // Arrange: 예상 값 설정
        string expectedId = "60a7c2f4b1d4c531d8f2a1b3"; // 예시 ObjectId 문자열
        DateTime expectedTimestamp = DateTime.Now;
        string expectedLogLevel = "Debug";
        string expectedMessage = "테스트 로그 메시지";
        string expectedStackTrace = "예제 스택 트레이스";

        // Act: 속성 초기화를 통해 LogMongo 객체 생성
        LogMongo log = new LogMongo
        {
            Id = expectedId,
            Timestamp = expectedTimestamp,
            LogLevel = expectedLogLevel,
            Message = expectedMessage,
            StackTrace = expectedStackTrace
        };

        // Assert: 각 속성이 예상한 값과 동일한지 확인합니다.
        Assert.AreEqual(expectedId, log.Id, "Id 값이 올바르게 저장되지 않았습니다.");
        Assert.AreEqual(expectedTimestamp, log.Timestamp, "Timestamp 값이 올바르게 저장되지 않았습니다.");
        Assert.AreEqual(expectedLogLevel, log.LogLevel, "LogLevel 값이 올바르게 저장되지 않았습니다.");
        Assert.AreEqual(expectedMessage, log.Message, "Message 값이 올바르게 저장되지 않았습니다.");
        Assert.AreEqual(expectedStackTrace, log.StackTrace, "StackTrace 값이 올바르게 저장되지 않았습니다.");
    }

    #endregion

    #region ToString 메서드 테스트

    /// <summary>
    /// ToString 메서드 테스트: ToString 호출 시 null 또는 빈 문자열이 아닌 결과를 반환하는지 검증합니다.
    /// </summary>
    [Test]
    public void ToString_ShouldReturnNonEmptyString()
    {
        // Arrange: 로그 데이터를 초기화하여 객체 생성
        LogMongo log = new LogMongo
        {
            Id = "60a7c2f4b1d4c531d8f2a1b3",
            Timestamp = new DateTime(2025, 4, 9),
            LogLevel = "Error",
            Message = "에러 메시지",
            StackTrace = "에러 발생 시의 스택 트레이스"
        };

        // Act: ToString 메서드 호출
        string result = log.ToString();

        // Assert: 반환된 문자열이 null 또는 빈 문자열이 아님을 확인
        Assert.IsFalse(string.IsNullOrEmpty(result), "ToString 메서드는 null 또는 빈 문자열을 반환해서는 안 됩니다.");
    }

    #endregion

    #region Equals 및 GetHashCode 테스트

    /// <summary>
    /// Equals 메서드 테스트: 동일한 객체 참조에 대해서 Equals가 true를 반환하는지 확인합니다.
    /// </summary>
    [Test]
    public void Equals_ShouldReturnTrueForSameReference()
    {
        // Arrange: 하나의 LogMongo 객체를 생성한 후 동일 참조를 다른 변수에 할당합니다.
        LogMongo log = new LogMongo
        {
            Id = "60a7c2f4b1d4c531d8f2a1b3",
            Timestamp = DateTime.Now,
            LogLevel = "Warning",
            Message = "경고 메시지",
            StackTrace = "스택 트레이스 정보"
        };

        LogMongo sameReference = log;

        // Act & Assert: 동일 객체이므로 Equals가 true를 반환해야 합니다.
        Assert.IsTrue(log.Equals(sameReference), "동일한 객체 참조에 대해 Equals는 true여야 합니다.");
    }

    /// <summary>
    /// Equals 메서드 테스트: 동일한 속성 값을 가진 서로 다른 인스턴스에 대해 Equals가 false를 반환하는지 확인합니다.
    /// </summary>
    [Test]
    public void Equals_ShouldReturnFalseForDifferentInstances()
    {
        // Arrange: 동일한 값들을 설정한 두 개의 LogMongo 인스턴스 생성 (기본 Equals는 참조 비교)
        DateTime commonTimestamp = DateTime.Now;
        LogMongo log1 = new LogMongo
        {
            Id = "60a7c2f4b1d4c531d8f2a1b3",
            Timestamp = commonTimestamp,
            LogLevel = "Info",
            Message = "동일 메시지",
            StackTrace = "동일 스택 트레이스"
        };

        LogMongo log2 = new LogMongo
        {
            Id = "60a7c2f4b1d4c531d8f2a1b3",
            Timestamp = commonTimestamp,
            LogLevel = "Info",
            Message = "동일 메시지",
            StackTrace = "동일 스택 트레이스"
        };

        // Act & Assert: 서로 다른 인스턴스이므로 기본 Equals는 false를 반환해야 합니다.
        Assert.IsFalse(log1.Equals(log2), "서로 다른 인스턴스에 대해 Equals는 false여야 합니다.");
    }

    /// <summary>
    /// GetHashCode 메서드 테스트: GetHashCode 메서드가 정수형 값을 반환하는지 검증합니다.
    /// </summary>
    [Test]
    public void GetHashCode_ShouldReturnInteger()
    {
        // Arrange: LogMongo 객체 생성
        LogMongo log = new LogMongo();

        // Act: GetHashCode 메서드 호출
        int hashCode = log.GetHashCode();

        // Assert: 반환된 값이 정수형임을 확인합니다.
        Assert.IsInstanceOf(typeof(int), hashCode, "GetHashCode는 정수형 값을 반환해야 합니다.");
    }

    #endregion
}
