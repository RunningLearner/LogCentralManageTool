using LogCentralManageTool.Models;

using System.ComponentModel.DataAnnotations;

namespace LogCentralManageTool.Tests.Models;

/// <summary>
/// ProductInfo 클래스의 속성 초기화, 데이터 유효성 검사 및 변환 로직을 검증하는 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class ProductConnectionInfoTests
{
    /// <summary>
    /// 테스트 시나리오:
    /// ProductInfo 인스턴스의 DatabaseName과 ConnectionString을 초기화한 후,
    /// 설정한 값이 올바르게 저장되고 반환되는지 검증합니다.
    /// 검증 포인트:
    /// - 속성에 할당한 값과 실제 프로퍼티 값이 동일한지 확인합니다.
    /// </summary>
    [Test]
    public void ProductInfo_PropertyInitialization_ShouldStoreAssignedValues()
    {
        // Arrange
        string expectedDatabaseName = "ProductDB";
        string expectedConnectionString = "Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;";

        // Act
        ProductInfo product = new ProductInfo
        {
            DatabaseName = expectedDatabaseName,
            ConnectionString = expectedConnectionString
        };

        // Assert
        Assert.AreEqual(expectedDatabaseName, product.DatabaseName, "DatabaseName이 올바르게 초기화되어야 합니다.");
        Assert.AreEqual(expectedConnectionString, product.ConnectionString, "ConnectionString이 올바르게 초기화되어야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// ProductInfo 인스턴스에서 [Required] 속성이 적용된 DatabaseName이 null인 경우
    /// 유효성 검사에서 실패하는지 검증합니다.
    /// 검증 포인트:
    /// - DatabaseName이 null이면 Validator.TryValidateObject가 false를 반환하고,
    ///   DatabaseName 관련 오류 메시지가 포함되어야 합니다.
    /// </summary>
    [Test]
    public void ProductInfo_Validation_ShouldFail_WhenDatabaseNameIsMissing()
    {
        // Arrange
        ProductInfo product = new ProductInfo
        {
            DatabaseName = null, // 필수 속성 누락
            ConnectionString = "SomeConnectionString"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(product, new ValidationContext(product), validationResults, validateAllProperties: true);

        // Assert
        Assert.IsFalse(isValid, "DatabaseName이 null이면 유효성 검증이 실패해야 합니다.");
        Assert.IsTrue(validationResults.Any(r => r.MemberNames.Contains("DatabaseName")), "DatabaseName 관련 유효성 오류가 있어야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// ProductInfo 인스턴스에 모든 필수 속성(특히 DatabaseName)이 올바르게 설정된 경우
    /// 유효성 검사에서 오류가 발생하지 않는지 검증합니다.
    /// 검증 포인트:
    /// - 올바른 값이 할당된 경우 Validator.TryValidateObject가 true를 반환해야 합니다.
    /// </summary>
    [Test]
    public void ProductInfo_Validation_ShouldSucceed_WithValidData()
    {
        // Arrange
        ProductInfo product = new ProductInfo
        {
            DatabaseName = "ValidDB",
            ConnectionString = "Server=localhost;Database=ValidDB;Trusted_Connection=True;"
        };

        // Act
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(product, new ValidationContext(product), validationResults, validateAllProperties: true);

        // Assert
        Assert.IsTrue(isValid, "모든 필수 속성이 올바르게 설정되면 유효성 검증이 통과해야 합니다.");
        Assert.AreEqual(0, validationResults.Count, "유효성 검증 오류가 없어야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// ProductInfo 인스턴스의 변환 로직(예: ToString 호출 시 의미 있는 정보 반환)을 검증합니다.
    /// 검증 포인트:
    /// - 별도의 변환 로직이 구현되어 있지 않더라도, 기본 ToString() 호출 결과가
    ///   null 또는 빈 문자열이 아닌지 확인합니다.
    ///   (필요시 커스터마이징 된 변환 로직 검증으로 확장할 수 있습니다.)
    /// </summary>
    [Test]
    public void ProductInfo_ToString_ShouldReturnNonEmptyString()
    {
        // Arrange
        ProductInfo product = new ProductInfo
        {
            DatabaseName = "TestDB",
            ConnectionString = "ConnectionString"
        };

        // Act
        string result = product.ToString();

        // Assert
        Assert.IsFalse(string.IsNullOrWhiteSpace(result), "ToString()은 비어있는 문자열을 반환해서는 안 됩니다.");
    }
}
