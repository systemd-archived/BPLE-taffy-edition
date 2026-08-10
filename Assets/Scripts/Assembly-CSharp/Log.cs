using UnityEngine;

/// <summary>
/// 条件编译日志封装。
/// 在正式发布版（非 Development Build）中自动移除所有日志调用，
/// 消除 IL2CPP 构建中的字符串分配和日志开销。
/// 用法: Log.Debug("message") 替代 Debug.Log("message")
/// </summary>
public static class Log
{
	[System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void Debug(object message)
	{
		UnityEngine.Debug.Log(message);
	}

	[System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void Debug(object message, Object context)
	{
		UnityEngine.Debug.Log(message, context);
	}

	[System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void Warning(object message)
	{
		UnityEngine.Debug.LogWarning(message);
	}

	[System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void Warning(object message, Object context)
	{
		UnityEngine.Debug.LogWarning(message, context);
	}

	// Error 始终保留（生产环境需要看到错误）
	public static void Error(object message)
	{
		UnityEngine.Debug.LogError(message);
	}

	public static void Error(object message, Object context)
	{
		UnityEngine.Debug.LogError(message, context);
	}

	// 格式化版本
	[System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void DebugFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogFormat(format, args);
	}

	[System.Diagnostics.Conditional("DEVELOPMENT_BUILD"), System.Diagnostics.Conditional("UNITY_EDITOR")]
	public static void WarningFormat(string format, params object[] args)
	{
		UnityEngine.Debug.LogWarningFormat(format, args);
	}
}
