using LogCentralManageTool.Converters;

using System.Globalization;
using System.Windows.Media;

namespace LogCentralManageTool.Tests.Converters;
/// <summary>
/// BoolToColorConverter의 단위 테스트를 위한 클래스입니다.
/// </summary>
[TestFixture]
public class BoolToColorConverterTests
{
    private BoolToColorConverter _converter;

    /// <summary>
    /// 각 테스트 실행 전에 BoolToColorConverter 인스턴스를 초기화합니다.
    /// </summary>
    [SetUp]
    public void Setup()
    {
        _converter = new BoolToColorConverter
        {
            TrueBrush = Brushes.LightGreen,
            FalseBrush = Brushes.LightGray
        };
    }

    /// <summary>
    /// 입력값이 true인 경우 Convert 메서드가 TrueBrush (LightGreen)를 반환하는지 테스트합니다.
    /// </summary>
    [Test]
    public void Convert_WithTrueValue_ReturnsTrueBrush()
    {
        // Arrange
        bool input = true;
        // Act
        var result = _converter.Convert(input, typeof(Brush), null, CultureInfo.InvariantCulture);
        // Assert
        Assert.IsInstanceOf<Brush>(result);
        Assert.AreEqual(Brushes.LightGreen.ToString(), result.ToString());
    }

    /// <summary>
    /// 입력값이 false인 경우 Convert 메서드가 FalseBrush (LightGray)를 반환하는지 테스트합니다.
    /// </summary>
    [Test]
    public void Convert_WithFalseValue_ReturnsFalseBrush()
    {
        // Arrange
        bool input = false;
        // Act
        var result = _converter.Convert(input, typeof(Brush), null, CultureInfo.InvariantCulture);
        // Assert
        Assert.IsInstanceOf<Brush>(result);
        Assert.AreEqual(Brushes.LightGray.ToString(), result.ToString());
    }

    /// <summary>
    /// boolean이 아닌 입력값의 경우 Convert 메서드가 FalseBrush (LightGray)를 반환하는지 테스트합니다.
    /// </summary>
    [Test]
    public void Convert_WithNonBooleanValue_ReturnsFalseBrush()
    {
        // Arrange
        var input = "non-boolean";
        // Act
        var result = _converter.Convert(input, typeof(Brush), null, CultureInfo.InvariantCulture);
        // Assert
        Assert.IsInstanceOf<Brush>(result);
        Assert.AreEqual(Brushes.LightGray.ToString(), result.ToString());
    }

    /// <summary>
    /// ConvertBack 메서드가 호출되면 NotImplementedException이 발생하는지 테스트합니다.
    /// </summary>
    [Test]
    public void ConvertBack_ThrowsNotImplementedException()
    {
        // Act & Assert
        Assert.Throws<NotImplementedException>(() =>
            _converter.ConvertBack(Brushes.LightGreen, typeof(bool), null, CultureInfo.InvariantCulture)
        );
    }
}
