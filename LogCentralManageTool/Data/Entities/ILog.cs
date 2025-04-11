namespace LogCentralManageTool.Data.Entities;
/// <summary>
/// 로그 엔티티의 공통 인터페이스입니다.
/// </summary>
public interface ILog
{
    string Id { get; set; }
    DateTime Timestamp { get; set; }
    string LogLevel { get; set; }
    string Message { get; set; }
    string? StackTrace { get; set; }
}
