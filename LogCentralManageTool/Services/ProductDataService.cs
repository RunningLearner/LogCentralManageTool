using LogCentralManageTool.Models;

using System.IO;
using System.Text.Json;

namespace LogCentralManageTool.Services;

/// <summary>
/// 로컬 JSON 파일에서 제품 정보를 불러오고 저장하는 서비스입니다.
/// </summary>
public static class ProductDataService
{
    private static readonly string FilePath = "products.json";

    /// <summary>
    /// 로컬 JSON 파일에서 제품 정보를 읽어옵니다.
    /// 파일이 없으면 빈 목록을 반환합니다.
    /// </summary>
    /// <returns>제품 정보 목록</returns>
    public static List<ProductInfo> LoadProducts()
    {
        if (!File.Exists(FilePath))
        {
            return new List<ProductInfo>();
        }

        try
        {
            string json = File.ReadAllText(FilePath);
            var products = JsonSerializer.Deserialize<List<ProductInfo>>(json);
            return products ?? new List<ProductInfo>();
        }
        catch (Exception ex)
        {
            // 오류 발생 시 로그 기록 또는 예외 처리
            return new List<ProductInfo>();
        }
    }

    /// <summary>
    /// 제품 정보를 로컬 JSON 파일에 저장합니다.
    /// </summary>
    /// <param name="products">저장할 제품 정보 목록</param>
    public static void SaveProducts(List<ProductInfo> products)
    {
        try
        {
            string json = JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(FilePath, json);
        }
        catch (Exception ex)
        {
            // 오류 발생 시 처리 로직
        }
    }
}