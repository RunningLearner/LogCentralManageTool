namespace LogCentralManageTool.Data;

/// <summary>
/// 사용 가능한 데이터베이스 제공자 종류를 나타내는 열거형입니다.
/// </summary>
public enum ProviderType
{
    /// <summary>
    /// MongoDB 제공자
    /// </summary>
    MongoDB,

    /// <summary>
    /// MySQL 제공자
    /// </summary>
    MySQL,

    /// <summary>
    /// SQLite 제공자
    /// </summary>
    SQLite
}
