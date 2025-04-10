using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LogCentralManageTool.Data.Entities;

/// <summary>
/// MySQL용 로그 엔티티입니다.
/// </summary>
[Table("Log")]
public class LogMySQL : ILog
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
    public string? StackTrace { get; set; }
}
