using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// WaitForSeconds 缓存池，避免在协程中反复 new WaitForSeconds(delay) 产生 GC 分配。
/// 用法: yield return WaitForSecondsCache.Get(0.5f)
///      yield return WaitForSecondsCache.HalfSecond;
/// </summary>
public static class WaitForSecondsCache
{
	private static Dictionary<float, WaitForSeconds> s_cache = new Dictionary<float, WaitForSeconds>(32);

	// 常用间隔直接暴露为静态属性
	public static readonly WaitForSeconds Point1 = Get(0.1f);
	public static readonly WaitForSeconds Point2 = Get(0.2f);
	public static readonly WaitForSeconds Point3 = Get(0.3f);
	public static readonly WaitForSeconds Point4 = Get(0.4f);
	public static readonly WaitForSeconds Half = Get(0.5f);
	public static readonly WaitForSeconds Point6 = Get(0.6f);
	public static readonly WaitForSeconds Point7 = Get(0.7f);
	public static readonly WaitForSeconds One = Get(1f);
	public static readonly WaitForSeconds OneAndHalf = Get(1.5f);
	public static readonly WaitForSeconds Two = Get(2f);
	public static readonly WaitForSeconds Three = Get(3f);
	public static readonly WaitForSeconds Five = Get(5f);

	/// <summary>
	/// 获取指定秒数的 WaitForSeconds 实例（缓存复用）。
	/// </summary>
	public static WaitForSeconds Get(float seconds)
	{
		if (!s_cache.TryGetValue(seconds, out var ws))
		{
			ws = new WaitForSeconds(seconds);
			s_cache[seconds] = ws;
		}
		return ws;
	}

	/// <summary>
	/// 清理缓存（极少使用）。
	/// </summary>
	public static void Clear()
	{
		s_cache.Clear();
	}
}
