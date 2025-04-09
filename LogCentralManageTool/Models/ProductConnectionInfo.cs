using System.ComponentModel.DataAnnotations;
using LogCentralManageTool.Data;

namespace LogCentralManageTool.Models;

/// <summary>
/// 각 제품의 정보를 저장하는 엔티티입니다.
/// DB 이름과 연결 문자열을 포함합니다.
/// </summary>
public class ProductInfo
{
    /// <summary>
    /// 제품의 이름이자 해당 제품의 데이터베이스 이름입니다.
    /// </summary>
    [Required]
    public string DatabaseName { get; set; }

    /// <summary>
    /// 해당 제품의 연결 문자열입니다.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// 데이터베이스 제공자 종류
    /// </summary>
    public ProviderType ProviderType { get; set; }
}
