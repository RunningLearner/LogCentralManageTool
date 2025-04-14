using LiveChartsCore.SkiaSharpView.WPF;

using System.Diagnostics;
using System.Windows;
using System.Windows.Threading;

namespace LogCentralManageTool.Behaviors;
/// <summary>
/// 이 Behavior는 차트에서 마우스 휠 이벤트가 발생하면 타이머를 재시작하여,
/// 사용자가 휠링을 끝냈을 때 차트의 X축 최솟값과 최댓값(날짜로 변환한 값)을 출력합니다.
/// </summary>
public static class MouseWheelAxisRangeBehavior
{
    #region Property
    public static readonly DependencyProperty EnableAxisRangePrintProperty =
        DependencyProperty.RegisterAttached(
            "EnableAxisRangePrint",
            typeof(bool),
            typeof(MouseWheelAxisRangeBehavior),
            new PropertyMetadata(false, OnEnableAxisRangePrintChanged));

    public static void SetEnableAxisRangePrint(DependencyObject element, bool value)
    {
        element.SetValue(EnableAxisRangePrintProperty, value);
    }

    public static bool GetEnableAxisRangePrint(DependencyObject element)
    {
        return (bool)element.GetValue(EnableAxisRangePrintProperty);
    }

    private static readonly DependencyProperty MouseWheelTimerProperty =
        DependencyProperty.RegisterAttached(
            "MouseWheelTimer",
            typeof(DispatcherTimer),
            typeof(MouseWheelAxisRangeBehavior),
            new PropertyMetadata(null));

    private static void SetMouseWheelTimer(DependencyObject element, DispatcherTimer value)
    {
        element.SetValue(MouseWheelTimerProperty, value);
    }

    private static DispatcherTimer GetMouseWheelTimer(DependencyObject element)
    {
        return (DispatcherTimer)element.GetValue(MouseWheelTimerProperty);
    }

    public static readonly DependencyProperty ComputedAxisMinProperty =
            DependencyProperty.RegisterAttached(
                "ComputedAxisMin",
                typeof(DateTime?),
                typeof(MouseWheelAxisRangeBehavior),
                new PropertyMetadata(null));

    public static void SetComputedAxisMin(DependencyObject element, DateTime? value)
    {
        element.SetValue(ComputedAxisMinProperty, value);
    }

    public static DateTime? GetComputedAxisMin(DependencyObject element)
    {
        return (DateTime?)element.GetValue(ComputedAxisMinProperty);
    }

    public static readonly DependencyProperty ComputedAxisMaxProperty =
            DependencyProperty.RegisterAttached(
                "ComputedAxisMax",
                typeof(DateTime?),
                typeof(MouseWheelAxisRangeBehavior),
                new PropertyMetadata(null));

    public static void SetComputedAxisMax(DependencyObject element, DateTime? value)
    {
        element.SetValue(ComputedAxisMaxProperty, value);
    }

    public static DateTime? GetComputedAxisMax(DependencyObject element)
    {
        return (DateTime?)element.GetValue(ComputedAxisMaxProperty);
    }

    #endregion

    private static void OnEnableAxisRangePrintChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is UIElement element)
        {
            bool isEnabled = (bool)e.NewValue;
            if (isEnabled)
            {
                element.MouseWheel += Element_MouseWheel;
                // 타이머 초기화
                var timer = GetMouseWheelTimer(element);
                if (timer == null)
                {
                    timer = new DispatcherTimer
                    {
                        Interval = TimeSpan.FromMilliseconds(300) // 300ms 후 실행 (원하는 시간으로 조정)
                    };
                    timer.Tick += (s, args) =>
                    {
                        // 타이머가 Tick되면, 차트의 X축 범위 계산 후 타이머 중지
                        if (element is CartesianChart chart)
                        {
                            var xAxis = chart.XAxes.FirstOrDefault();
                            if (xAxis != null && xAxis.MinLimit.HasValue && xAxis.MaxLimit.HasValue)
                            {
                                double minValue = xAxis.MinLimit.Value;
                                double maxValue = xAxis.MaxLimit.Value;

                                // DateTimeAxis의 X값이 Ticks 형태로 설정된 경우 변환합니다.
                                if (IsValidTicks(minValue) && IsValidTicks(maxValue))
                                {
                                    DateTime dtMin = new DateTime((long)minValue);
                                    DateTime dtMax = new DateTime((long)maxValue);

                                    // Attached Property 업데이트
                                    SetComputedAxisMin(chart, dtMin);
                                    SetComputedAxisMax(chart, dtMax);

                                    Debug.WriteLine($"[Delayed] X축 최솟값: {dtMin:yyyy-MM-dd HH:mm:ss}, 최댓값: {dtMax:yyyy-MM-dd HH:mm:ss}");
                                }
                                else
                                {
                                    Debug.WriteLine("X축 값이 유효한 DateTime.Ticks 범위를 벗어났습니다.");
                                }
                            }
                        }
                        timer.Stop();
                    };
                    SetMouseWheelTimer(element, timer);
                }
            }
            else
            {
                element.MouseWheel -= Element_MouseWheel;
                var timer = GetMouseWheelTimer(element);
                if (timer != null)
                {
                    timer.Stop();
                    SetMouseWheelTimer(element, null);
                }
            }
        }
    }

    private static void Element_MouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {
        // 마우스 휠 이벤트가 발생하면 타이머를 재시작합니다.
        if (sender is CartesianChart chart)
        {
            var timer = GetMouseWheelTimer(chart);
            if (timer != null)
            {
                timer.Stop();
                timer.Start();
            }
        }
    }

    /// <summary>
    /// 전달된 double 값이 DateTime.Ticks 범위 내에 있는지 확인합니다.
    /// </summary>
    /// <param name="value">확인할 값</param>
    /// <returns>유효하면 true, 아니면 false</returns>
    private static bool IsValidTicks(double value)
    {
        return value >= DateTime.MinValue.Ticks && value <= DateTime.MaxValue.Ticks;
    }
}