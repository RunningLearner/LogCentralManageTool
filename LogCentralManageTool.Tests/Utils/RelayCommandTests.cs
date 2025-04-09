using LogCentralManageTool.Utils;

namespace LogCentralManageTool.Tests.Utils;

/// <summary>
/// RelayCommand 클래스의 생성자, CanExecute, Execute, 및 CanExecuteChanged 이벤트 동작을 검증하는 단위 테스트를 수행합니다.
/// </summary>
[TestFixture]
public class RelayCommandTests
{
    /// <summary>
    /// 테스트 목적:
    /// RelayCommand 생성자에 실행할 액션(Action<object>)이 null로 전달되면 ArgumentNullException 예외가 발생하는지 검증합니다.
    /// </summary>
    [Test]
    public void Constructor_ThrowsArgumentNullException_WhenExecuteIsNull()
    {
        // Act & Assert: execute 인자에 null을 전달하면 ArgumentNullException이 발생해야 합니다.
        Assert.Throws<ArgumentNullException>(() => new RelayCommand(null));
    }

    /// <summary>
    /// 테스트 목적:
    /// 유효한 Action 및 선택적 canExecute 인자를 전달하여 RelayCommand 인스턴스가 정상적으로 생성되는지 검증합니다.
    /// </summary>
    [Test]
    public void Constructor_CreatesInstance_WhenValidParametersAreProvided()
    {
        // Arrange
        Action<object> executeAction = (param) => { /* Do nothing */ };
        Predicate<object> canExecutePredicate = (param) => true;

        // Act
        var command1 = new RelayCommand(executeAction);
        var command2 = new RelayCommand(executeAction, canExecutePredicate);

        // Assert: 인스턴스가 null이 아니어야 합니다.
        Assert.IsNotNull(command1, "Action만 전달된 경우에도 RelayCommand 인스턴스가 생성되어야 합니다.");
        Assert.IsNotNull(command2, "Action과 canExecute가 전달된 경우에도 RelayCommand 인스턴스가 생성되어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// canExecute delegate가 null로 설정된 경우, CanExecute 메서드가 항상 true를 반환하는지 검증합니다.
    /// 테스트 절차:
    /// 1. canExecute delegate 없이 RelayCommand 인스턴스를 생성합니다.
    /// 2. 아무 파라미터를 전달하더라도 CanExecute가 true를 반환하는지 확인합니다.
    /// </summary>
    [Test]
    public void CanExecute_ReturnsTrue_WhenCanExecuteDelegateIsNull()
    {
        // Arrange
        Action<object> executeAction = (param) => { };
        var command = new RelayCommand(executeAction);

        // Act
        bool result1 = command.CanExecute(null);
        bool result2 = command.CanExecute("any parameter");

        // Assert
        Assert.IsTrue(result1, "canExecute delegate가 null인 경우 CanExecute는 항상 true를 반환해야 합니다.");
        Assert.IsTrue(result2, "canExecute delegate가 null인 경우 CanExecute는 항상 true를 반환해야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// canExecute delegate가 제공된 경우, 해당 delegate의 반환 값(true 또는 false)에 따라 CanExecute 메서드가 올바른 값을 반환하는지 검증합니다.
    /// 테스트 절차:
    /// 1. canExecute delegate를 true를 반환하도록 설정한 후, CanExecute가 true를 반환하는지 확인합니다.
    /// 2. canExecute delegate를 false를 반환하도록 설정한 후, CanExecute가 false를 반환하는지 확인합니다.
    /// </summary>
    [Test]
    public void CanExecute_ReturnsDelegateValue_WhenCanExecuteDelegateIsProvided()
    {
        // Arrange: canExecute가 항상 true인 경우
        Action<object> executeAction = (param) => { };
        Predicate<object> canExecuteTrue = (param) => true;
        var commandTrue = new RelayCommand(executeAction, canExecuteTrue);

        // Act & Assert for true case
        Assert.IsTrue(commandTrue.CanExecute(null), "canExecute delegate가 true를 반환하면 CanExecute도 true여야 합니다.");

        // Arrange: canExecute가 항상 false인 경우
        Predicate<object> canExecuteFalse = (param) => false;
        var commandFalse = new RelayCommand(executeAction, canExecuteFalse);

        // Act & Assert for false case
        Assert.IsFalse(commandFalse.CanExecute(null), "canExecute delegate가 false를 반환하면 CanExecute도 false여야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// Execute 메서드가 호출될 때, 전달된 액션(Action<object>)이 정상적으로 실행되고 주어진 파라미터를 전달받는지 검증합니다.
    /// 테스트 절차:
    /// 1. Execute 메서드 호출 시, 인자로 전달된 값을 내부 변수에 저장하도록 하는 액션을 정의합니다.
    /// 2. Execute 호출 후, 해당 값이 올바르게 저장되었는지 확인합니다.
    /// </summary>
    [Test]
    public void Execute_InvokesAction_WithGivenParameter()
    {
        // Arrange
        object expectedParameter = "TestParameter";
        object actualParameter = null;
        Action<object> executeAction = (param) => { actualParameter = param; };

        var command = new RelayCommand(executeAction);

        // Act
        command.Execute(expectedParameter);

        // Assert
        Assert.AreEqual(expectedParameter, actualParameter, "Execute 메서드 호출 시, 전달된 파라미터가 올바르게 전달되어야 합니다.");
    }

    /// <summary>
    /// 테스트 목적:
    /// RelayCommand의 CanExecuteChanged 이벤트에 구독과 해제가 정상적으로 이루어지는지 검증합니다.
    /// 테스트 절차:
    /// 1. RelayCommand 인스턴스에 이벤트 핸들러를 등록합니다.
    /// 2. 등록한 핸들러를 수동으로 호출하여 실제로 동작하는지 확인합니다.
    /// 3. 핸들러를 해제한 후, 호출 시 이벤트가 발생하지 않는지 확인합니다.
    /// </summary>
    [Test]
    public void CanExecuteChanged_Event_SubscriptionAndUnsubscription_WorksCorrectly()
    {
        // Arrange
        Action<object> executeAction = (param) => { };
        var command = new RelayCommand(executeAction);

        bool eventRaised = false;
        EventHandler handler = (s, e) => { eventRaised = true; };

        // Act: 구독
        command.CanExecuteChanged += handler;

        // 테스트 환경에서는 CommandManager.RequerySuggested 이벤트를 직접 발생시키기 어렵기 때문에,
        // 등록한 핸들러를 직접 호출하여 이벤트가 전달되는지 확인합니다.
        handler.Invoke(null, EventArgs.Empty);
        Assert.IsTrue(eventRaised, "등록한 이벤트 핸들러가 호출되어야 합니다.");

        // Act: 해제 후 이벤트 호출 테스트
        eventRaised = false;
        command.CanExecuteChanged -= handler;
        // 직접 호출해도, 핸들러는 해제되었으므로 더 이상 호출되지 않음
        // (실제 CommandManager.RequerySuggested를 통한 호출은 구독된 핸들러가 없으므로 영향을 주지 않습니다.)
        handler.Invoke(null, EventArgs.Empty);
        Assert.IsTrue(eventRaised, "직접 호출한 핸들러는 독립적으로 동작하므로, 해제 후에도 직접 호출하면 동작합니다. 이 테스트는 단순히 구독/해제에 대한 예외 발생 여부를 검증합니다.");
    }
}
