# LogCentralManageTool
WPF를 사용한 로그 모니터링 어플리케이션
efcore를 통해 mysql과 mongodb를 지원합니다.

# 실행
- Repo를 클론한 뒤에 빌드를 해주세요.
- LogCentralManageTool파일을 실행합니다.

# 메인화면
- 추가된 제품들의 대략적인 에러 비율을 파이차트로 확인할 수 있습니다.
- 사이드바에서는 제품을 추가할 수 있습니다.
  - 데이터베이스 이름과 연결문자열로 제품을 구분합니다.
  - 테이블의 이름은 "Log"여야합니다.
  - Log 테이블에는 다음과 같은 속성이 있어야합니다.

```csharp
/// <summary>
/// 로그 엔티티의 공통 인터페이스입니다.
/// </summary>
public interface ILog
{
    DateTime Timestamp { get; set; }
    string LogLevel { get; set; }
    string Message { get; set; }
    string? StackTrace { get; set; }
}

```

# 각 제품의 상세페이지
- 시계열 막대차트를 확인할 수 있습니다.
- 상단의 토글 버튼을 통해 원하는 로그레벨 막대만 확인할 수 있습니다.
