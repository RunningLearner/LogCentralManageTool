using System.Reflection;

namespace LogCentralManageTool.Tests;
public static class EventRaiser
{
    /// <summary>
    /// 대상 객체의 지정된 이벤트를 강제로 발생시킵니다.
    /// </summary>
    /// <param name="target">이벤트를 가진 객체</param>
    /// <param name="eventName">이벤트 이름 (백킹 필드 이름과 동일하게 가정)</param>
    /// <param name="args">이벤트 핸들러에 전달할 인자들</param>
    public static void RaiseEvent(object target, string eventName, params object[] args)
    {
        // 이벤트 백킹 필드를 가져옵니다.
        FieldInfo eventField = target.GetType().GetField(eventName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (eventField != null)
        {
            var eventDelegate = eventField.GetValue(target) as MulticastDelegate;
            if (eventDelegate != null)
            {
                foreach (var handler in eventDelegate.GetInvocationList())
                {
                    handler.DynamicInvoke(args);
                }
            }
        }
    }
}
