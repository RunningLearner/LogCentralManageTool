using LogCentralManageTool.Data;
using LogCentralManageTool.ViewModels;

namespace LogCentralManageTool.Tests.ViewModels;

/// <summary>
/// AddProductViewModel 클래스의 생성자 기본값, 프로퍼티 변경 및 PropertyChanged 이벤트 동작을 검증하는 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class AddProductViewModelTests
{
    /// <summary>
    /// 테스트 목적:
    /// 생성자 호출 시 기본 DatabaseName, ConnectionString, ProviderTypes 컬렉션 및 SelectedProviderType(MongoDB)이 올바르게 초기화되는지 검증합니다.
    /// 테스트 절차:
    /// 1. 생성자를 호출하여 AddProductViewModel 인스턴스를 생성합니다.
    /// 2. 각 기본 프로퍼티 값이 기대한 값("NewProductDb", "mongodb://localhost:27017/NewProductDb", MongoDB 등)과 일치하는지 확인합니다.
    /// 3. ProviderTypes 컬렉션에 MongoDB, MySQL, SQLite가 포함되어 있는지 검증합니다.
    /// </summary>
    [Test]
    public void Constructor_ShouldInitializeDefaultValues()
    {
        // Arrange & Act
        var viewModel = new AddProductViewModel();

        // Assert
        Assert.AreEqual("NewProductDb", viewModel.DatabaseName, "생성자 기본 DatabaseName은 'NewProductDb'여야 합니다.");
        Assert.AreEqual("mongodb://localhost:27017/NewProductDb", viewModel.ConnectionString, "생성자 기본 ConnectionString이 올바르게 초기화되어야 합니다.");
        Assert.IsNotNull(viewModel.ProviderTypes, "ProviderTypes 컬렉션은 null이 아니어야 합니다.");
        CollectionAssert.AreEquivalent(
            new ProviderType[] { ProviderType.MongoDB, ProviderType.MySQL, ProviderType.SQLite },
            viewModel.ProviderTypes,
            "ProviderTypes 컬렉션에는 MongoDB, MySQL, SQLite가 포함되어야 합니다.");
        Assert.AreEqual(ProviderType.MongoDB, viewModel.SelectedProviderType, "기본 SelectedProviderType은 MongoDB여야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// DatabaseName 프로퍼티 변경 시 PropertyChanged 이벤트가 발생하는지 검증합니다.
    /// 테스트 절차:
    /// 1. PropertyChanged 이벤트에 구독한 후 DatabaseName을 새로운 값으로 변경합니다.
    /// 2. 이벤트 콜백에서 전달된 프로퍼티 이름에 "DatabaseName"이 포함되는지 확인합니다.
    /// </summary>
    [Test]
    public void DatabaseName_PropertyChanged_EventIsRaised()
    {
        // Arrange
        var viewModel = new AddProductViewModel();
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Act
        viewModel.DatabaseName = "TestDatabase";

        // Assert
        CollectionAssert.Contains(changedProperties, "DatabaseName", "DatabaseName 변경 시 PropertyChanged 이벤트가 발생해야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// DatabaseName 변경 후, SelectedProviderType에 따른 ConnectionString 업데이트가 올바르게 동작하는지 검증합니다.
    /// 테스트 절차:
    /// 1. DatabaseName을 "TestDB"와 같이 변경합니다.
    /// 2. SelectedProviderType을 (기본값 MongoDB에서) MySQL로 변경하여 UpdateDefaultConnectionString 메서드가 호출되도록 합니다.
    /// 3. 업데이트된 ConnectionString이 새 DatabaseName("TestDB")을 반영하는지 확인합니다.
    /// </summary>
    [Test]
    public void DatabaseName_ChangeThenSelectedProvider_UpdateConnectionString()
    {
        // Arrange
        var viewModel = new AddProductViewModel();
        viewModel.DatabaseName = "TestDB";

        // Act: SelectedProviderType를 MySQL로 변경하면 UpdateDefaultConnectionString이 호출됨
        viewModel.SelectedProviderType = ProviderType.MySQL;

        // Assert: 새 DatabaseName "TestDB"가 반영된 MySQL 형식의 ConnectionString이어야 함
        string expectedConnectionString = $"server=localhost;port=3306;database=TestDB;user=root;password=yourpassword";
        Assert.AreEqual(expectedConnectionString, viewModel.ConnectionString,
            "DatabaseName 변경 후, MySQL ProviderType에 따른 ConnectionString이 올바르게 업데이트되어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// SelectedProviderType 프로퍼티 변경 시 PropertyChanged 이벤트가 발생하고, 각 ProviderType에 따라 ConnectionString이 올바르게 업데이트되는지 검증합니다.
    /// 테스트 절차:
    /// 1. PropertyChanged 이벤트에 구독한 후 SelectedProviderType를 MySQL, SQLite, MongoDB 순서로 변경합니다.
    /// 2. 각 변경 시 업데이트되는 ConnectionString이 DatabaseName을 기반으로 올바른 형식으로 설정되는지 확인합니다.
    /// 3. 또한, SelectedProviderType 변경 시 PropertyChanged 이벤트가 발생하는지 검증합니다.
    /// </summary>
    [Test]
    public void SelectedProviderType_PropertyChanged_UpdatesConnectionString()
    {
        // Arrange
        var viewModel = new AddProductViewModel();
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Act: MongoDB -> MySQL
        viewModel.SelectedProviderType = ProviderType.MySQL;

        // Assert for MySQL update
        string expectedMySQLConnectionString = $"server=localhost;port=3306;database={viewModel.DatabaseName};user=root;password=yourpassword";
        Assert.AreEqual(expectedMySQLConnectionString, viewModel.ConnectionString,
            "SelectedProviderType이 MySQL로 변경되면 ConnectionString이 MySQL 형식으로 업데이트되어야 합니다.");
        CollectionAssert.Contains(changedProperties, "SelectedProviderType", "SelectedProviderType 변경 시 PropertyChanged 이벤트가 발생해야 합니다.");

        changedProperties.Clear();

        // Act: MySQL -> SQLite
        viewModel.SelectedProviderType = ProviderType.SQLite;
        string expectedSQLiteConnectionString = $"Data Source={viewModel.DatabaseName}.sqlite";
        Assert.AreEqual(expectedSQLiteConnectionString, viewModel.ConnectionString,
            "SelectedProviderType이 SQLite로 변경되면 ConnectionString이 SQLite 형식으로 업데이트되어야 합니다.");
        CollectionAssert.Contains(changedProperties, "SelectedProviderType", "SelectedProviderType 변경 시 PropertyChanged 이벤트가 발생해야 합니다.");

        changedProperties.Clear();

        // Act: SQLite -> MongoDB
        viewModel.SelectedProviderType = ProviderType.MongoDB;
        string expectedMongoConnectionString = $"mongodb://localhost:27017/{viewModel.DatabaseName}";
        Assert.AreEqual(expectedMongoConnectionString, viewModel.ConnectionString,
            "SelectedProviderType이 MongoDB로 변경되면 ConnectionString이 MongoDB 형식으로 업데이트되어야 합니다.");
        CollectionAssert.Contains(changedProperties, "SelectedProviderType", "SelectedProviderType 변경 시 PropertyChanged 이벤트가 발생해야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// 각 프로퍼티(DatabaseName, SelectedProviderType, ConnectionString) 변경 시 PropertyChanged 이벤트가 올바른 프로퍼티 이름과 함께 발생하는지 검증합니다.
    /// 테스트 절차:
    /// 1. PropertyChanged 이벤트에 구독하여 변경된 프로퍼티 이름들을 기록합니다.
    /// 2. DatabaseName 및 SelectedProviderType을 변경하여 발생한 이벤트 목록에 기대하는 프로퍼티 이름("DatabaseName", "SelectedProviderType", "ConnectionString")이 포함되는지 확인합니다.
    /// </summary>
    [Test]
    public void PropertyChanged_Event_FiresWithCorrectPropertyNames()
    {
        // Arrange
        var viewModel = new AddProductViewModel();
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Act & Assert for DatabaseName
        viewModel.DatabaseName = "ChangedDB";
        CollectionAssert.Contains(changedProperties, "DatabaseName", "DatabaseName 변경 시 PropertyChanged 이벤트에 'DatabaseName'이 포함되어야 합니다.");

        changedProperties.Clear();

        // Act & Assert for SelectedProviderType
        viewModel.SelectedProviderType = ProviderType.MySQL;
        // SelectedProviderType 변경 시 두 개의 이벤트가 발생할 수 있음: SelectedProviderType와 ConnectionString (업데이트로 인해)
        CollectionAssert.Contains(changedProperties, "SelectedProviderType", "SelectedProviderType 변경 시 PropertyChanged 이벤트에 'SelectedProviderType'이 포함되어야 합니다.");
        CollectionAssert.Contains(changedProperties, "ConnectionString", "SelectedProviderType 변경 시 ConnectionString 업데이트로 인해 'ConnectionString' PropertyChanged 이벤트가 발생해야 합니다.");
    }
}
