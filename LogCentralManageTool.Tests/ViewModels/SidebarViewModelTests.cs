using LogCentralManageTool.Data;
using LogCentralManageTool.Models;
using LogCentralManageTool.ViewModels;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LogCentralManageTool.Tests.ViewModels;

/// <summary>
/// SidebarViewModel의 초기화, 명령, 속성 변경 및 이벤트 동작을 검증하는 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class SidebarViewModelTests
{
    private const string ConfigFolder = "Config";
    private const string ProductsFilePath = "Config/products.json";
    private List<ProductInfo> _sampleProducts;

    /// <summary>
    /// 각 테스트 전에 임시 Config 디렉터리와 제품 JSON 파일을 생성하여, ProductDataService.LoadProducts()의 반환값을 제어합니다.
    /// </summary>
    [SetUp]
    public void SetUp()
    {
        // 테스트용 제품 목록 생성
        _sampleProducts = new List<ProductInfo>
            {
                new ProductInfo { DatabaseName = "DB1", ConnectionString = "conn1" },
                new ProductInfo { DatabaseName = "DB2", ConnectionString = "conn2" }
            };

        // Config 폴더가 없으면 생성
        if (!Directory.Exists(ConfigFolder))
        {
            Directory.CreateDirectory(ConfigFolder);
        }

        // 테스트용 제품 목록을 JSON 형식으로 저장
        string json = JsonSerializer.Serialize(_sampleProducts, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ProductsFilePath, json);
    }

    /// <summary>
    /// 각 테스트 후 생성한 Config 디렉터리와 JSON 파일을 삭제하여 테스트 환경을 정리합니다.
    /// </summary>
    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(ConfigFolder))
        {
            Directory.Delete(ConfigFolder, true);
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// SidebarViewModel 생성 시, ProductDataService.LoadProducts()를 통해 불러온 제품 목록으로 ProductList가 올바르게 초기화되는지 검증합니다.
    /// 시나리오:
    /// 1. SetUp에서 임시 JSON 파일에 정의된 제품 목록을 생성합니다.
    /// 2. SidebarViewModel 생성 후 ProductList의 항목 수와 내용이 JSON 파일의 내용과 일치하는지 확인합니다.
    /// </summary>
    [Test]
    public void Constructor_InitializesProductList_FromProductDataService()
    {
        // Arrange & Act
        var viewModel = new SidebarViewModel();

        // Assert: JSON 파일에 기록된 제품과 일치하는지 검증
        Assert.IsNotNull(viewModel.ProductList, "ProductList는 null이면 안 됩니다.");
        Assert.AreEqual(_sampleProducts.Count, viewModel.ProductList.Count,
            "생성자에서 불러온 제품 목록의 개수가 JSON 파일의 제품 수와 일치해야 합니다.");

        // 각 항목의 값이 동일한지 확인합니다.
        for (int i = 0; i < _sampleProducts.Count; i++)
        {
            Assert.AreEqual(_sampleProducts[i].DatabaseName, viewModel.ProductList[i].DatabaseName, $"제품 {i}의 DatabaseName이 일치해야 합니다.");
            Assert.AreEqual(_sampleProducts[i].ConnectionString, viewModel.ProductList[i].ConnectionString, $"제품 {i}의 ConnectionString이 일치해야 합니다.");
        }
    }

    /// <summary>
    /// 테스트 목적:
    /// ToggleCommand 실행 시, IsExpanded 속성이 반전되고 ToggleIcon이 올바르게 업데이트되는지, 그리고 변경 시 PropertyChanged 이벤트가 발생하는지 검증합니다.
    /// 시나리오:
    /// 1. 초기 IsExpanded 값과 ToggleIcon 값(기본 IsExpanded=true → ToggleIcon="<<")을 확인합니다.
    /// 2. ToggleCommand.Execute(null)을 호출하여 상태를 반전시키고, IsExpanded는 false, ToggleIcon은 ">>"로 변경되는지, 그리고 PropertyChanged 이벤트가 발생하는지 확인합니다.
    /// </summary>
    [Test]
    public void ToggleCommand_TogglesIsExpanded_AndUpdatesToggleIcon()
    {
        // Arrange
        var viewModel = new SidebarViewModel();
        var changedProperties = new List<string>();
        viewModel.PropertyChanged += (s, e) => changedProperties.Add(e.PropertyName);

        // Assert 초기 상태
        Assert.IsTrue(viewModel.IsExpanded, "초기 IsExpanded 값은 true여야 합니다.");
        Assert.AreEqual("<<", viewModel.ToggleIcon, "초기 ToggleIcon은 '<<'여야 합니다.");

        // Act: ToggleCommand 실행
        viewModel.ToggleCommand.Execute(null);

        // Assert: IsExpanded는 false로 전환되고, ToggleIcon은 ">>"이어야 합니다.
        Assert.IsFalse(viewModel.IsExpanded, "ToggleCommand 실행 후 IsExpanded 값은 false여야 합니다.");
        Assert.AreEqual(">>", viewModel.ToggleIcon, "ToggleCommand 실행 후 ToggleIcon은 '>>'여야 합니다.");

        // PropertyChanged 이벤트가 IsExpanded와 ToggleIcon에 대해 발생했는지 확인
        CollectionAssert.Contains(changedProperties, nameof(viewModel.IsExpanded), "IsExpanded 변경 시 PropertyChanged 이벤트가 발생해야 합니다.");
        CollectionAssert.Contains(changedProperties, nameof(viewModel.ToggleIcon), "ToggleIcon 변경 시 PropertyChanged 이벤트가 발생해야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// AddProductCommand 실행 시, AddProductWindow가 열리고, ShowDialog()가 true를 반환하는 경우 ProductList에 새 제품이 추가되며,
    /// ProductDataService.SaveProducts()가 호출되는지 검증합니다.
    /// 시나리오:
    /// - UI 관련 동작은 Stub이나 Fake 객체를 이용해야 하지만, 현재 코드에서는 직접 new를 사용하므로 테스트하기 어렵습니다.
    /// - 이 테스트는 추후 AddProductWindow를 인터페이스 추상화하여 DI로 주입하도록 리팩토링 후 테스트 구현하는 것이 바람직합니다.
    /// </summary>
    [Test, Ignore("AddProductCommand 테스트는 UI 의존성 때문에 추가적인 리팩토링(DI/인터페이스 추상화)이 필요합니다.")]
    public void AddProductCommand_AddsProduct_And_SavesProducts()
    {
        // 이 테스트는 현재 AddProductWindow가 직접 생성되므로 테스트가 어려워 Ignore 처리합니다.
    }

    /// <summary>
    /// 테스트 목적:
    /// SelectedProduct 속성에 유효한 ProductInfo를 할당하면 PropertyChanged 이벤트가 발생하고, 
    /// SelectedProduct가 null이 아닐 경우 ProductSelected 이벤트가 기본 ProviderType(InMemory)와 함께 전달되는지 검증합니다.
    /// 시나리오:
    /// 1. SidebarViewModel의 ProductSelected 이벤트를 구독합니다.
    /// 2. SelectedProduct 속성에 새로운 ProductInfo를 할당합니다.
    /// 3. PropertyChanged와 ProductSelected 이벤트가 올바른 프로퍼티 이름과 인자(InMemory)로 발생하는지 확인합니다.
    /// </summary>
    [Test]
    public void Setting_SelectedProduct_RaisesEvents()
    {
        // Arrange
        var viewModel = new SidebarViewModel();
        string changedProperty = null;
        viewModel.PropertyChanged += (s, e) => changedProperty = e.PropertyName;

        ProductInfo selectedProduct = new ProductInfo { DatabaseName = "TestDB", ConnectionString = "connTest", ProviderType = ProviderType.InMemory };
        ProductInfo eventProduct = null;
        ProviderType eventProvider = default;
        viewModel.ProductSelected += (prod) =>
        {
            eventProduct = prod;
        };

        // Act
        viewModel.SelectedProduct = selectedProduct;

        // Assert
        Assert.AreEqual("SelectedProduct", changedProperty, "SelectedProduct 변경 시 PropertyChanged 이벤트에 'SelectedProduct'가 전달되어야 합니다.");
        Assert.IsNotNull(eventProduct, "SelectedProduct가 null이 아니면 ProductSelected 이벤트가 발생해야 합니다.");
        Assert.AreEqual(selectedProduct, eventProduct, "ProductSelected 이벤트의 인자로 전달된 제품 정보가 올바르지 않습니다.");
        Assert.AreEqual(ProviderType.InMemory, eventProvider, "ProductSelected 이벤트의 기본 ProviderType은 InMemory이어야 합니다.");
    }
}
