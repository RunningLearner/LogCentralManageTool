using LogCentralManageTool.Data;
using LogCentralManageTool.Data.Entities;

using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LogCentralManageTool.ViewModels;

/// <summary>
/// 대시보드에서 최신 로그 데이터를 표시하기 위한 ViewModel입니다.
/// </summary>
public class DashBoardViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler PropertyChanged;

    public Log SelectedLog { get; set; }
    private readonly LoggingDbContext _context;

    /// <summary>
    /// 생성자: 외부에서 주입받은 LoggingDbContext를 사용하여 최신 로그를 SelectedLog에 할당합니다.
    /// </summary>
    /// <param name="context">
    /// 제품 정보(데이터베이스명, 연결 문자열 등)에 맞게 이미 구성된 LoggingDbContext 인스턴스
    /// </param>
    public DashBoardViewModel(LoggingDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        try
        {
            SelectedLog = _context.Set<Log>()
                                  .OrderByDescending(l => l.Timestamp)
                                  .FirstOrDefault();
        }
        catch (Exception e)
        {
            // 예외 처리 로직: 예외 발생 시 SelectedLog는 null로 남도록 처리합니다.
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string prop = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}

