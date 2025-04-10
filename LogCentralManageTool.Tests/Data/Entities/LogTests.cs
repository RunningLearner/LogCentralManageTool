using LogCentralManageTool.Data.Entities;

namespace LogCentralManageTool.Tests.Data.Entities;

/// <summary>
/// Log 클래스에 대한 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class LogTests
{
    /// <summary>
    /// 기본 생성자 테스트: Log 객체의 모든 속성이 기본값으로 초기화되는지 확인합니다.
    /// </summary>
    [Test]
    public void DefaultConstructor_ShouldInitializeProperties()
    {
        // Arrange & Act: 기본 생성자를 통해 Log 객체 생성
        LogMySQL log = new LogMySQL();

        // Assert: 각 속성이 기본값(정수는 0, DateTime은 default값, 문자열은 null)으로 초기화되었는지 확인합니다.
        Assert.AreEqual(0, log.Id, "기본 Id 값은 0이어야 합니다.");
        Assert.AreEqual(default(DateTime), log.Timestamp, "기본 Timestamp 값은 default(DateTime)이어야 합니다.");
        Assert.IsNull(log.LogLevel, "기본 LogLevel 값은 null이어야 합니다.");
        Assert.IsNull(log.Message, "기본 Message 값은 null이어야 합니다.");
        Assert.IsNull(log.StackTrace, "기본 StackTrace 값은 null이어야 합니다.");
    }

    /// <summary>
    /// 속성 초기화 테스트: Log 객체 생성 후 각 속성에 할당한 값들이 올바르게 저장되는지 확인합니다.
    /// </summary>
    [Test]
    public void PropertyInitialization_ShouldStoreCorrectValues()
    {
        // Arrange: 예상 값 설정
        int expectedId = 1;
        DateTime expectedTimestamp = DateTime.Now;
        string expectedLogLevel = "Info";
        string expectedMessage = "테스트 메시지";
        string expectedStackTrace = "샘플 스택 트레이스";

        // Act: 속성 초기화를 통해 Log 객체 생성
        LogMySQL log = new LogMySQL
        {
            Id = expectedId,
            Timestamp = expectedTimestamp,
            LogLevel = expectedLogLevel,
            Message = expectedMessage,
            StackTrace = expectedStackTrace
        };

        // Assert: 각 속성이 예상한 값과 동일한지 확인
        Assert.AreEqual(expectedId, log.Id);
        Assert.AreEqual(expectedTimestamp, log.Timestamp);
        Assert.AreEqual(expectedLogLevel, log.LogLevel);
        Assert.AreEqual(expectedMessage, log.Message);
        Assert.AreEqual(expectedStackTrace, log.StackTrace);
    }

    /// <summary>
    /// ToString 메서드 테스트: ToString 호출 시 null 또는 빈 문자열이 아닌 결과를 반환하는지 검증합니다.
    /// </summary>
    [Test]
    public void ToString_ShouldReturnNonEmptyString()
    {
        // Arrange: 로그 데이터를 초기화하여 객체 생성
        LogMySQL log = new LogMySQL
        {
            Id = 1,
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

    /// <summary>
    /// Equals 메서드 테스트: 동일한 객체 참조에 대해서 Equals가 true를 반환하는지 확인합니다.
    /// </summary>
    [Test]
    public void Equals_ShouldReturnTrueForSameReference()
    {
        // Arrange: 하나의 Log 객체를 생성한 후 동일 참조를 다른 변수에 할당
        LogMySQL log = new LogMySQL
        {
            Id = 1,
            Timestamp = DateTime.Now,
            LogLevel = "Warning",
            Message = "경고 메시지",
            StackTrace = "스택 트레이스 정보"
        };
        LogMySQL sameReference = log;

        // Act & Assert: 동일 객체이므로 Equals가 true를 반환해야 합니다.
        Assert.IsTrue(log.Equals(sameReference), "동일한 객체 참조에 대해 Equals는 true여야 합니다.");
    }

    /// <summary>
    /// Equals 메서드 테스트: 동일한 속성 값을 가진 서로 다른 인스턴스에 대해 Equals가 false를 반환하는지 확인합니다.
    /// </summary>
    [Test]
    public void Equals_ShouldReturnFalseForDifferentInstances()
    {
        // Arrange: 동일한 값들을 설정한 두 개의 Log 인스턴스 생성 (기본 Equals는 참조 비교)
        DateTime commonTimestamp = DateTime.Now;
        LogMySQL log1 = new LogMySQL
        {
            Id = 1,
            Timestamp = commonTimestamp,
            LogLevel = "Info",
            Message = "동일 메시지",
            StackTrace = "동일 스택트레이스"
        };
        LogMySQL log2 = new LogMySQL
        {
            Id = 1,
            Timestamp = commonTimestamp,
            LogLevel = "Info",
            Message = "동일 메시지",
            StackTrace = "동일 스택트레이스"
        };

        // Act & Assert: 다른 인스턴스이므로 기본 Equals는 false를 반환합니다.
        Assert.IsFalse(log1.Equals(log2), "서로 다른 인스턴스에 대해 Equals는 false여야 합니다.");
    }

    /// <summary>
    /// GetHashCode 메서드 테스트: GetHashCode 메서드가 정수형 값을 반환하는지 검증합니다.
    /// </summary>
    [Test]
    public void GetHashCode_ShouldReturnInteger()
    {
        // Arrange: Log 객체 생성
        LogMySQL log = new LogMySQL();

        // Act: GetHashCode 메서드 호출
        int hashCode = log.GetHashCode();

        // Assert: 반환된 값이 정수형임을 확인합니다.
        Assert.IsInstanceOf(typeof(int), hashCode, "GetHashCode는 정수형 값을 반환해야 합니다.");
    }
}