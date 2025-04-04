using System.Windows.Input;

namespace LogCentralManageTool.Utils
{
    /// <summary>
    /// MVVM에서 ICommand 구현을 위한 RelayCommand 클래스
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// RelayCommand 생성자
        /// </summary>
        /// <param name="execute">실행할 액션</param>
        /// <param name="canExecute">실행 가능 여부를 판단하는 함수 (옵션)</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// ICommand.CanExecuteChanged 이벤트
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// 명령 실행 가능 여부 확인
        /// </summary>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// 명령 실행
        /// </summary>
        public void Execute(object parameter) => _execute(parameter);
    }
}
