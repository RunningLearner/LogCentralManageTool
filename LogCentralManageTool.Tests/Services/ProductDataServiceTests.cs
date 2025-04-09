using LogCentralManageTool.Models;
using LogCentralManageTool.Services;

using System.Text.Json;

namespace LogCentralManageTool.Tests.Services;

/// <summary>
/// ProductDataService 클래스의 파일 입출력 동작(제품 정보 불러오기, 저장하기)을 검증하는 단위 테스트를 수행합니다.
/// 테스트 후 생성된 Config 디렉터리와 JSON 파일은 정리(clean up)됩니다.
/// </summary>
[TestFixture]
public class ProductDataServiceTests
{
    private readonly string configDirectory = "Config";
    private readonly string filePath = Path.Combine("Config", "products.json");

    /// <summary>
    /// 각 테스트 실행 전, 테스트 환경을 깨끗하게 유지하기 위해 Config 디렉터리와 JSON 파일을 삭제합니다.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        if (Directory.Exists(configDirectory))
        {
            Directory.Delete(configDirectory, true);
        }
    }

    /// <summary>
    /// 테스트 시나리오:
    /// JSON 파일이 존재하지 않을 경우 LoadProducts 메서드가 빈 리스트(List&lt;ProductInfo&gt;)를 반환하는지 검증합니다.
    /// </summary>
    [Test]
    public void LoadProducts_ReturnsEmptyList_WhenFileDoesNotExist()
    {
        // Arrange: SetUp에서 파일/디렉터리를 삭제하므로 파일은 존재하지 않습니다.

        // Act
        var products = ProductDataService.LoadProducts();

        // Assert
        Assert.IsNotNull(products, "LoadProducts는 null을 반환해서는 안 됩니다.");
        Assert.IsEmpty(products, "파일이 없을 경우 빈 목록이 반환되어야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// 유효한 JSON 파일이 존재하는 경우, LoadProducts 메서드가 파일에 기록된 제품 정보들을 올바르게 역직렬화하여 반환하는지 검증합니다.
    /// </summary>
    [Test]
    public void LoadProducts_ReturnsProducts_WhenValidJsonFileExists()
    {
        // Arrange
        Directory.CreateDirectory(configDirectory);
        var expectedProducts = new List<ProductInfo>
            {
                new ProductInfo { DatabaseName = "Product1", ConnectionString = "ConnStr1" },
                new ProductInfo { DatabaseName = "Product2", ConnectionString = "ConnStr2" }
            };
        string json = JsonSerializer.Serialize(expectedProducts, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(filePath, json);

        // Act
        var products = ProductDataService.LoadProducts();

        // Assert
        Assert.IsNotNull(products, "LoadProducts는 null을 반환해서는 안 됩니다.");
        Assert.AreEqual(expectedProducts.Count, products.Count, "파일에 기록된 제품 수와 반환된 제품 수가 일치해야 합니다.");

        // 각 제품의 속성이 올바르게 역직렬화되었는지 검증
        for (int i = 0; i < expectedProducts.Count; i++)
        {
            Assert.AreEqual(expectedProducts[i].DatabaseName, products[i].DatabaseName, $"제품 {i + 1}의 DatabaseName이 일치해야 합니다.");
            Assert.AreEqual(expectedProducts[i].ConnectionString, products[i].ConnectionString, $"제품 {i + 1}의 ConnectionString이 일치해야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 시나리오:
    /// 잘못된 형식의 JSON 파일이 존재할 경우, LoadProducts 메서드가 JSON 읽기 중 발생하는 예외를 처리하고 빈 리스트를 반환하는지 검증합니다.
    /// </summary>
    [Test]
    public void LoadProducts_ReturnsEmptyList_WhenJsonIsInvalid()
    {
        // Arrange
        Directory.CreateDirectory(configDirectory);
        // 잘못된 JSON 형식의 내용을 기록합니다.
        File.WriteAllText(filePath, "Invalid JSON Content");

        // Act
        var products = ProductDataService.LoadProducts();

        // Assert
        Assert.IsNotNull(products, "LoadProducts는 null을 반환해서는 안 됩니다.");
        Assert.IsEmpty(products, "잘못된 JSON 형식인 경우, 빈 목록이 반환되어야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// 주어진 제품 목록을 SaveProducts 메서드를 사용하여 저장한 후, JSON 파일이 실제로 생성되고 올바른 내용이 기록되는지 검증합니다.
    /// </summary>
    [Test]
    public void SaveProducts_CreatesFileWithCorrectContent()
    {
        // Arrange
        var productsToSave = new List<ProductInfo>
            {
                new ProductInfo { DatabaseName = "ProductA", ConnectionString = "ConnA" }
            };

        // Act
        ProductDataService.SaveProducts(productsToSave);

        // Assert: 파일 존재 여부 확인
        Assert.IsTrue(File.Exists(filePath), "SaveProducts 실행 후, 제품 JSON 파일이 생성되어야 합니다.");

        string json = File.ReadAllText(filePath);
        // 역직렬화하여 내용 검증
        var deserializedProducts = JsonSerializer.Deserialize<List<ProductInfo>>(json);
        Assert.IsNotNull(deserializedProducts, "저장된 JSON 파일은 올바르게 역직렬화되어야 합니다.");
        Assert.AreEqual(productsToSave.Count, deserializedProducts.Count, "저장된 제품 수가 일치해야 합니다.");
        Assert.AreEqual(productsToSave[0].DatabaseName, deserializedProducts[0].DatabaseName, "제품의 DatabaseName이 일치해야 합니다.");
        Assert.AreEqual(productsToSave[0].ConnectionString, deserializedProducts[0].ConnectionString, "제품의 ConnectionString이 일치해야 합니다.");
    }

    /// <summary>
    /// 테스트 시나리오:
    /// Config 디렉터리가 존재하지 않는 경우에도 SaveProducts 메서드가 디렉터리를 생성하고, 제품 정보를 파일에 저장하는지 검증합니다.
    /// </summary>
    [Test]
    public void SaveProducts_CreatesDirectoryAndFile_WhenDirectoryDoesNotExist()
    {
        // Arrange: SetUp에서 Config 디렉터리는 삭제되었으므로 존재하지 않습니다.
        var productsToSave = new List<ProductInfo>
            {
                new ProductInfo { DatabaseName = "ProductX", ConnectionString = "ConnX" }
            };

        // Act
        ProductDataService.SaveProducts(productsToSave);

        // Assert: 디렉터리와 파일이 생성되었는지 확인
        Assert.IsTrue(Directory.Exists(configDirectory), "Config 디렉터리가 존재하지 않을 경우, SaveProducts가 디렉터리를 생성해야 합니다.");
        Assert.IsTrue(File.Exists(filePath), "생성된 Config 디렉터리 내에 제품 JSON 파일이 존재해야 합니다.");

        // 파일 내용 검증
        string json = File.ReadAllText(filePath);
        var deserializedProducts = JsonSerializer.Deserialize<List<ProductInfo>>(json);
        Assert.IsNotNull(deserializedProducts, "저장된 JSON 파일은 올바르게 역직렬화되어야 합니다.");
        Assert.AreEqual(productsToSave.Count, deserializedProducts.Count, "저장된 제품 수가 일치해야 합니다.");
    }

    /// <summary>
    /// 테스트 실행 후 생성된 Config 디렉터리와 JSON 파일을 정리하여 파일 시스템에 영향을 주지 않도록 합니다.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(configDirectory))
        {
            Directory.Delete(configDirectory, true);
        }
    }
}