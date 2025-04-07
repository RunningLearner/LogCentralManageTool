using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogCentralManageTool.Data.Entities;

/// <summary>
/// 로그 데이터를 저장하는 엔티티입니다.
/// 이 엔티티는 데이터베이스에서 "Log" 테이블로 매핑됩니다.
/// </summary>
[Table("Log")]
public class Log
{
    /// <summary>
    /// 로그의 고유 식별자입니다.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// 로그가 기록된 시간입니다.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 로그 레벨 (예: Info, Warning, Error)입니다.
    /// </summary>
    [MaxLength(50)]
    public string LogLevel { get; set; }

    /// <summary>
    /// 로그 메시지입니다.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 예외 발생 시의 스택트레이스 정보입니다.
    /// </summary>
    [Column(TypeName = "text")]
    public string StackTrace { get; set; }
    // 추가 컬럼이 필요한 경우 여기에 작성할 수 있습니다.
}
